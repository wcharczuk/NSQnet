using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQPublisher : NSQClient
    {
        public NSQPublisher() : base() { }

        public NSQPublisher(String hostname, Int32 port) : base(hostname, port) { }

        public NSQPublisher(String hostname, Int32 port, Stream output) : base(hostname, port, output) { }

        public override void Initialize()
        {
            base.Initialize();
        }

        public NSQMessage Publish(String topic_name, Object data)
        {
            return _protocol.Publish(topic_name, data);
        }
    }
}