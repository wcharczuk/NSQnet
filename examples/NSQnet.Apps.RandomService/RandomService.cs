using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace NSQnet.Apps.RandomService
{
    public sealed class RandomService
    {
        private readonly ILog log;
        private readonly ProgramOptions options;

        public RandomService(ProgramOptions options)
        {
            this.log = LogManager.GetLogger(typeof(RandomService));
            this.options = options;
        }

        public async Task Start()
        {
            const int ReportIntervalSeconds = 10;

            var c = options.Count;
            var r = new Random();
            var generated = 0L;
            var last_generated = 0L;
            var notBefore = DateTime.Now.AddSeconds(ReportIntervalSeconds);

            while (options.Continuous || c > 0)
            {
                var when = DateTime.Now.ToString("o");

                var body = string.Format(@"{{""value"": {0}, ""timestamp"": ""{1}""}}", r.Next(), when);

                await NSQUtil.PublishAsync(options.NsqdHttpAddress, options.Topic, body);

                generated++;

                if (DateTime.Now > notBefore)
                {
                    log.InfoFormat("{0} random numbers generated (+{1})", generated, generated - last_generated);

                    notBefore = DateTime.Now.AddSeconds(ReportIntervalSeconds);
                    last_generated = generated;
                }

                Thread.Sleep(options.Interval);

                if (c > 0)
                {
                    c--;
                }
            }
        }
    }
}
