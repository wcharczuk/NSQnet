using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace NSQnet.Apps.RandomService
{
    public class ProgramOptions
    {
        public static bool GetProgramOptions(string[] args, out ProgramOptions options)
        {
            options = new ProgramOptions();
            return CommandLine.Parser.Default.ParseArguments(args, options);
        }

        #region .ctor
        public ProgramOptions()
        {
            
        }
        #endregion

        [Option("n", DefaultValue = 0, HelpText = "total messages to produce", Required = false)]
        public int Count { get; set; }

        [Option("interval", DefaultValue = 3000, HelpText = "milliseconds between each message", Required = false)]
        public int Interval { get; set; }

        [Option("nsqd-http-address", HelpText = "nsqd HTTP address (required)", Required = true)]
        public string NsqdHttpAddress { get; set; }

        [Option("topic", DefaultValue = "", HelpText = "nsq topic (required)", Required = true)]
        public string Topic { get; set; }

        [VerbOption("version", DefaultValue = false, HelpText = "print version string")]
        public bool Version { get; set; }

        [HelpOption()]
        public String GetUsage()
        {
            const string Usage = @"Publishes JSON-encoded random numbers to the specified topic

Usage of nsq_rand:
  -nsqd-http-address=: nsqd HTTP address
  -n=0: total messages to produce
  -interval=3000: milliseconds between each message
  -topic="": nsq topic
  -version=false: print version string";

            return Usage;
        }

        public bool Continuous
        {
            get { return Count <= 0; }
        }
    }
}
