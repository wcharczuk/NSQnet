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

        public override void Initialize()
        {
            base.Initialize();
        }

        public void Publish(String topic_name, Object data)
        {
            _protocol.Publish(topic_name, data);
        }

        public void Publish(String topic_name, List<Object> data)
        {
            _protocol.MultiPublish(topic_name, data);
        }
    }
}