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
        static object _consoleLock = new object();

        static int processed = 0;
        static int last_processed = 0;
        static DateTime last_timestamp;

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
            var line = new StringBuilder();
            for (int x = 0; x < Console.WindowWidth; x++)
            {
                line.Append("=");
            }
            Console.WriteLine(line.ToString());

            var nsq = new NSQ(options.Hostname, options.Port);

            nsq.MessageHandler = (sender, e) =>
            {
                var sub = sender as NSQSubscriber;
                var main_subscription = sub.Subscriptions.FirstOrDefault();

                if (e.Message.Body.Length != 84)
                {
                    throw new Exception("Bad Message!");
                }

                System.Threading.Interlocked.Increment(ref processed);

                sub.Finish(e.Message.MessageId);
                sub.ResetReadyCount();
            };

            nsq.DisconnectedHandler = (sender, e) =>
            {
                var sub = (sender as NSQSubscriber);
                var main_subscription = sub.Subscriptions.FirstOrDefault();
                Console.WriteLine(String.Format("{0}::{2}.{1} Disconnected", sub.Hostname, main_subscription.Channel, main_subscription.Topic));
            };
            
            nsq.Topics.Add("load_test");

            new Task(() =>
            {
                while (true)
                {
                    var delta = processed - last_processed;
                    var time_delta = DateTime.Now - last_timestamp;
                    var rate = (float)delta / (float)(time_delta.TotalMilliseconds / 1000);

                    if (delta != 0)
                    {
                        Console.WriteLine(String.Format("Processed {0} Messages at a rate of {1} m/sec", processed, rate));
                        last_timestamp = DateTime.Now;
                        last_processed = processed;
                    }

                    Thread.Sleep(500);
                }
            }).Start();

            last_timestamp = DateTime.Now;
            nsq.Listen();
        }
    }
}
