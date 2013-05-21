using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQProtocol
    {
        public NSQProtocol()
        {
            this.HeartbeatInterval = 30 * 1000; //the default, 30 seconds.
        }

        public NSQProtocol(String hostname, Int32 port) : this()
        {
            this.Hostname = hostname;
            this.Port = port;
            this.Initialize();
        }

        public NSQProtocol(String hostname, Int32 port, Stream output) : this()
        {
            this.Hostname = hostname;
            this.Port = port;
            this.OutputStream = output;
        }

        public String Hostname { get; set; }
        public Int32 Port { get; set; }

        public Int32 HeartbeatInterval { get; set; }

        public Stream OutputStream { get; set; }
        private StreamWriter _outputWriter { get; set; }

        private static readonly Byte[] Version = new Byte[4] { 0x20, 0x20, 0x56, 0x32 };
        private static readonly Int16 MAX_NAME_LENGTH = 32;
        private static readonly String VALID_NAME_EXPR = "[.a-zA-Z0-9_-]";

        public static Boolean CheckName(String name)
        {
            return name.Length > 1 && name.Length < MAX_NAME_LENGTH && System.Text.RegularExpressions.Regex.IsMatch(name, VALID_NAME_EXPR);
        }

        private Boolean _continue = true;

        private System.Net.Sockets.TcpClient _client = null;
        private System.Net.Sockets.NetworkStream _networkStream = null;
        private System.IO.BinaryReader _networkReader = null;

        public void Initialize()
        {
            if (String.IsNullOrWhiteSpace(this.Hostname))
                throw new Exception("Hostname must be set.");

            if (this.Port == default(Int16))
                throw new Exception("Port must be set.");

            _client = new System.Net.Sockets.TcpClient();
            _client.Connect(hostname: this.Hostname, port: this.Port);
            _networkStream = _client.GetStream();
            _networkReader = new System.IO.BinaryReader(_networkStream);
            _networkStream.Write(Version, 0, Version.Length);

            if (OutputStream != null)
                _outputWriter = new StreamWriter(OutputStream);

            RecieveLoop();
        }

        private async void RecieveLoop()
        {
            byte[] sizebuffer = await ReadPreambleAsync();

            if (sizebuffer != null && sizebuffer.Length == 4)
            {
                Array.Reverse(sizebuffer);
                var size = BitConverter.ToInt32(sizebuffer, 0);

                byte[] buffer = new Byte[size];
                await _networkStream.ReadAsync(buffer, 0, (int)size);

                Dispatch(UnpackResponse(size, buffer));
            }

            if (_continue)
                RecieveLoop();
        }

        private static Result UnpackResponse(Int32 size, Byte[] buffer)
        {
            byte[] frameTypeBuffer = new Byte[4];
            Array.Copy(buffer, frameTypeBuffer, 4);
            Array.Reverse(frameTypeBuffer);
            FrameType frameType = (FrameType)BitConverter.ToInt32(frameTypeBuffer, 0);

            byte[] bodyBuffer = new Byte[size - 4];
            Array.Copy(buffer, 4, bodyBuffer, 0, (int)size - 4);

            byte[] converted = System.Text.Encoding.Convert(System.Text.Encoding.ASCII, System.Text.Encoding.Default, bodyBuffer);

            String result = System.Text.Encoding.Default.GetString(converted);

            return new Result { Size = size, FrameType = frameType, Body = result };
        }

        private Task<Byte[]> ReadPreambleAsync()
        {
            return Task.Run<Byte[]>(() => _networkReader.ReadBytes(4));
        }

        public void DestroyConnection()
        {
            _continue = false;
            Close();
            if (_networkStream != null)
            {
                _networkStream.Dispose();
                _networkStream = null;
            }
            if (_client != null)
            {
                _client = null;
            }
        }

        public void Dispatch(Result result)
        {
            switch (result.Body)
            {
                case "_heartbeat_":
                    NOP();
                    break;
            }
            LogOutput(result.Body);
        }

        private void LogOutput(String output)
        {
            if (_outputWriter != null)
            {
                _outputWriter.Write(output);
                _outputWriter.Flush();
            }
            else
            {
                Debug.Write(output);
            }
        }

        public void Identify(String short_id, String long_id, Int32 heartbeat_interval, Boolean feature_negotiation)
        {
            dynamic json = new AgileObject();
            json.short_id = short_id;
            json.long_id = long_id;
            json.heartbeat_interval = heartbeat_interval;
            json.feature_negotiation = feature_negotiation;

            var jsonText = JsonSerializer.Current.SerializeObject(json);
            WriteAscii("IDENTIFY\n");
            WriteBinary(PackMessage(jsonText));
        }

        public void Subscribe(String topic_name, String channel_name)
        {
            WriteAscii(String.Format("SUB {0} {1}\n", topic_name, channel_name));
        }

        public String Publish(String topic_name, object data)
        {
            if (!CheckName(topic_name))
                throw new Exception("Bad topic_name");

            WriteAscii(String.Format("PUB {0}\n"));
            var json = JsonSerializer.Current.SerializeObject(data);
            WriteBinary(PackMessage(json));


        }

        public String MultiPublish(String topic_name)
        {
            if (!CheckName(topic_name))
                throw new Exception("Bad topic_name");

            WriteAscii(String.Format("MPUB {0}\n", topic_name));

        }

        public String Ready(int count)
        {

        }

        public String Finish(String message_id)
        {
            WriteAscii(String.Format("FIN {0}\n", message_id));
        }

        public String Requeue()
        {

        }

        public String Touch(String message_id)
        {
            WriteAscii(String.Format("TOUCH {0}\n", message_id));
        }

        /// <summary>
        /// Cleanly closes the connection.
        /// </summary>
        public String Close()
        {
            WriteAscii("CLS\n");
        }

        /// <summary>
        /// No-Op
        /// </summary>
        /// <remarks>No response.</remarks>
        public void NOP()
        {
            WriteAscii("NOP\n");
        }

        private void WriteBinary(Byte[] binary)
        {
            _networkStream.Write(binary, 0, binary.Length);
        }

        private void WriteAscii(String unicode)
        {
            LogOutput(unicode);

            var asciiBytes = ConvertToAscii(unicode);
            _networkStream.Write(asciiBytes, 0, asciiBytes.Length);
        }

        private static Byte[] ConvertToAscii(String unicode)
        {
            var bytes = System.Text.Encoding.Default.GetBytes(unicode);
            return System.Text.Encoding.Convert(System.Text.Encoding.Default, System.Text.Encoding.ASCII, bytes);
        }

        private static Byte[] PackMessage(String text)
        {
            byte[] textBytes = ConvertToAscii(text);
            var size = textBytes.Length;

            byte[] preBuffer = BitConverter.GetBytes(size);

            byte[] output = new byte[4 + size];
            Array.Reverse(preBuffer); //Endian Swap ...
            Array.Copy(preBuffer, output, 4);
            Array.Copy(textBytes, 0, output, 4, size);

            return output;
        }

        private static Byte[] PackMessage(FrameType type, String text)
        {
            byte[] textBytes = ConvertToAscii(text);
            var size = textBytes.Length;

            byte[] sizeBuffer = BitConverter.GetBytes(size);
            Array.Reverse(sizeBuffer); //Endian Swap ...

            byte[] frameTypeBuffer = BitConverter.GetBytes((int)type);
            Array.Reverse(frameTypeBuffer);

            byte[] output = new byte[8 + size];
            Array.Copy(sizeBuffer, output, 4);
            Array.Copy(frameTypeBuffer, 0, output, 4, 4);
            Array.Copy(textBytes, 0, output, 8, size);

            return output;
        }
    }
}
