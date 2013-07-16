using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace NSQnet.CLI
{
    public class Options
    {
        public Options()
        {
            this.Port = 4161;
        }

        [Option('h', "hostname", Required=true, HelpText="The host name / ip address of lookupd to connect to.")]
        public String Hostname { get; set; }

        [Option('p', "port", HelpText = "The port to connect to. Defaults to 4161")]
        public Int32 Port { get; set; }

        [HelpOption()]
        public String GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("NSQnet CLI 1.0");
            usage.AppendLine("--hostname\t\t: The nsqlookupd hostname to connect to (required).");
            usage.AppendLine("--port\t\t: The port on the host to connect to.");
            return usage.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var valid = CommandLine.Parser.Default.ParseArguments(args, options);

            if (!valid)
            {
                System.Environment.Exit(1);
                return;
            }
            Console.WriteLine("NSQnet CLI 1.0");
            for (int x = 0; x < Console.WindowWidth; x++)
            {
                Console.Write("=");
            }
            Console.WriteLine();

            NSQLookup lookupClient = new NSQLookup(options.Hostname, options.Port);
            Console.WriteLine("NSQLookupd Server is " + (lookupClient.Ping() ? "UP" : "NOT OK"));

            var topics = lookupClient.Topics();
            var nodes = lookupClient.Nodes();

            foreach (var topic in topics)
            {
                foreach(var producer in lookupClient.ProducersForTopic(topic))
                {
                    //producer.Hostname
                    GetSubscriber("192.168.1.24", (int)producer.TCP_Port, topic, "main");
                }
            }

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        public static void GetSubscriber(String host, Int32 port, String topicName, String channelName)
        {
            var sub = new NSQSubscriber(host, port);
            sub.Initialize();

            Action<Object, NSQMessageEventArgs> messageHandler = (sender, e) =>
            {
                Console.Write(String.Format("{0}::{2}.{1} MSG ", host, channelName, topicName));
                Console.WriteLine(e.Message.Body);
                sub.Finish(e.Message.MessageId);
                sub.ResetReadyCount();
            };

            sub.MaxReadyCount = 5;
            sub.NSQMessageRecieved += new NSQMessageRecievedHandler(messageHandler);
            sub.Subscribe(topicName, channelName);
            sub.ResetReadyCount();

            Console.WriteLine(String.Format("{0}::{2}.{1} Subscribed", host, channelName, topicName));

            while (sub.IsConnected)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine(String.Format("{0}::{2}.{1} Disconnected", host, channelName, topicName));
        }
    }
}
