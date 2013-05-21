using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQClient
    {
        public NSQClient()
        {

        }

        public NSQClient(String hostname, Int32 port)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.InitializeConnection();
        }

        public String Hostname { get; set; }
        public Int32 Port { get; set; }

        private static readonly Byte[] Version = new Byte[4] { 0x20, 0x20, 0x56, 0x32 };
        private static readonly Int16 MAX_NAME_LENGTH = 32;
        private static readonly String VALID_NAME_EXPR = "[.a-zA-Z0-9_-]";

        private System.Net.Sockets.TcpClient _client = null;
        private System.Net.Sockets.NetworkStream _stream = null;
        private System.IO.StreamReader _streamReader = null;
        private System.IO.StreamWriter _streamWriter = null;
        private ReaderWriterLockSlim _streamLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void InitializeConnection()
        {
            if (String.IsNullOrWhiteSpace(this.Hostname))
                throw new Exception("Hostname must be set.");

            if (this.Port == default(Int16))
                throw new Exception("Port must be set.");

            _client = new System.Net.Sockets.TcpClient();
            _client.Connect(hostname: this.Hostname, port: this.Port);
            _stream = _client.GetStream();
            _streamReader = new System.IO.StreamReader(_stream);
            _streamWriter = new System.IO.StreamWriter(_stream);
            _stream.Write(Version, 0, Version.Length);
        }

        private async Task IOLoop()
        {
            Task<String> 
        }

        public void DestroyConnection()
        {
            Close();
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            if (_client != null)
            {
                _client = null;
            }
        }

        public void Identify()
        {

        }

        public void Subscribe()
        {

        }

        public void Publist()
        {

        }

        public void MultiPublish()
        {

        }

        public void Ready(int count)
        {

        }

        public void Finnish()
        {

        }

        public void Requeue()
        {

        }

        public void Touch()
        {

        }

        /// <summary>
        /// Cleanly closes the connection.
        /// </summary>
        public void Close()
        {
            WriteAscii("CLS\n");
            var resp = ReadResponse();
        }

        /// <summary>
        /// No-Op
        /// </summary>
        /// <remarks>No response.</remarks>
        public void NOP()
        {
            WriteAscii("NOP\n");
        }

        private void WriteAscii(String unicode)
        {
            _streamLock.EnterWriteLock();
            try
            {
                var bytes = System.Text.Encoding.Default.GetBytes(unicode);
                var asciiBytes = System.Text.Encoding.Convert(System.Text.Encoding.Default, System.Text.Encoding.ASCII, bytes);
                _stream.Write(asciiBytes, 0, asciiBytes.Length);
            }
            finally
            {
                _streamLock.ExitWriteLock();
            }
        }

        private Boolean CheckName(String name)
        {
            return name.Length > 1 && name.Length < MAX_NAME_LENGTH && System.Text.RegularExpressions.Regex.IsMatch(name, VALID_NAME_EXPR);
        }

        private String ReadResponse()
        {
            _streamLock.EnterReadLock();
            try
            {
                return _streamReader.ReadLine();
            }
            finally
            {
                _streamLock.ExitReadLock();
            }
        }
    }
}

