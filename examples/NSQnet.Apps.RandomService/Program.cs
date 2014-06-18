using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)] 

namespace NSQnet.Apps.RandomService
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger(typeof(Program));

            ProgramOptions options;
            if (!ProgramOptions.GetProgramOptions(args, out options))
            {
                Environment.Exit(1);
                return;
            }

            if (options.Version)
            {
                PrintVersion(log);
                return;
            }

            log.InfoFormat("NSQD HTTP Address: {0}", options.NsqdHttpAddress);
            log.InfoFormat("Topic: {0}", options.Topic);
            log.InfoFormat("Interval (s): {0}", options.Interval);
            log.InfoFormat("Count: {0}", options.Continuous ? "continuous" : options.Count.ToString());

            var svc = new RandomService(options);
            svc.Start().Wait();
        }

        static void PrintVersion(ILog log)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attr = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            var version = attr == null ? "FIXME" : attr.Version;
            log.InfoFormat("nsq_rand {0}", version);
        }
    }
}
