using System;
using System.Collections.Concurrent;
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
            
            var up = lookupClient.Ping();
            
            Console.WriteLine("NSQLookupd Server is " + ( up ? "UP" : "NOT OK"));

            if(up)
            {
                while (true)
                {
                    PollForNewSubscribers(lookupClient);
                    Thread.Sleep(500); //poll every half second for new 
                }
            }
        }

        public static ConcurrentDictionary<String, NSQSubscriber> _subscribers = new ConcurrentDictionary<string,NSQSubscriber>();

        public static void PollForNewSubscribers(NSQLookup lookupClient)
        {
            var topics = lookupClient.Topics();

            foreach (var topic in topics)
            {
	            foreach(var producer in lookupClient.ProducersForTopic(topic))
	            {
	                if (!_subscribers.ContainsKey(producer.Hostname.ToLower()))
	                {
						var sub = GetSubscriber(producer.Hostname.ToLower(), producer.Hostname.ToLower(), producer.Hostname, (int)producer.TCP_Port, topic);
	                    _subscribers.AddOrUpdate(sub.LongIdentifier, sub, (long_id, oldSub) => sub);
	                }
                    else if (!_subscribers[producer.Hostname.ToLower()].IsSubscribed(topic, topic))
                    {
                        _subscribers[producer.Hostname.ToLower()].Subscribe(topic, topic);
                    }
	            }
            }
        }

        public static NSQSubscriber GetSubscriber(String shortId, String longId, String host, Int32 port, String topicName, String channelName = null)
        {
            var sub = new NSQSubscriber(shortId, longId, host, port);

            channelName = channelName ?? topicName;

            sub.Initialize();

            //closures are AWESOME and you should use them.
            Action<Object, NSQMessageEventArgs> messageHandler = (sender, e) =>
            {
                Console.Write(String.Format("{0}::{2}.{1} MSG ", host, channelName, topicName));
                Console.WriteLine(e.Message.Body);
                sub.Finish(e.Message.MessageId);
                sub.ResetReadyCount();
            };

            Action<Object, EventArgs> disconnectedHandler = (sender, e) =>
            {
                var disconnected_sub = (sender as NSQSubscriber);

                Console.WriteLine(String.Format("{0}::{2}.{1} Disconnected", host, channelName, topicName));
                
                NSQSubscriber cached_sub = null;
                _subscribers.TryRemove(disconnected_sub.LongIdentifier.ToLower(), out cached_sub);

                disconnected_sub.Dispose();
                cached_sub.Dispose();

                disconnected_sub = null;
                cached_sub = null;
            };

            sub.MaxReadyCount = 5;
            
            sub.NSQMessageRecieved += new NSQMessageRecievedHandler(messageHandler);
            sub.NSQClientDisconnected += new NSQProtocolDisconnectedHandler(disconnectedHandler);

            sub.Subscribe(topicName, channelName);
            sub.ResetReadyCount();

            Console.WriteLine(String.Format("{0}::{2}.{1} Subscribed", host, channelName, topicName));

            return sub;
        }
    }
}
