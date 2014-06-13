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
            const int ReportIntervalSeconds = 60;

            var c = options.Count;
            var r = new Random();
            var generated = 0L;
            var notBefore = DateTime.Now.AddSeconds(ReportIntervalSeconds);

            while (options.Continuous || c > 0)
            {
                var body = string.Format(@"{{""value"": {0}}}", r.Next());

                await NSQUtil.PublishAsync(options.NsqdHttpAddress, options.Topic, body);

                generated++;

                if (DateTime.Now > notBefore)
                {
                    log.InfoFormat("{0} random numbers generated", generated);

                    notBefore = DateTime.Now.AddSeconds(ReportIntervalSeconds);
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
