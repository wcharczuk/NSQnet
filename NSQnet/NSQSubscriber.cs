using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQSubscriber : NSQClient
    {
        public NSQSubscriber() : base() {}

        public NSQSubscriber(String hostname, Int32 port) : base(hostname, port) {}

        public NSQSubscriber(String hostname, Int32 port, Stream output)
            : base(hostname, port, output) {}

        public override void Initialize()
        {
            _protocol.NSQMessageRecieved += new NSQMessageRecievedHandler(NSQProtocolMessageRecieved);
            base.Initialize();
        }

        private NSQProtocol _protocol = null;

        public Int64 MaxReadyCount { get; set; }
        public Int64 ReadyCount { get; set; }

        public Int16 MaxAttemptCount { get; set; }

        public Int64 MessagesInFlight { get; private set; }
        public Int64 MessagesRecieved { get; private set; }
        public Int64 MessagesFinished { get; private set; }
        public Int64 MessagesRequeued { get; private set; }

        public Int32 BackoffCounter { get; private set; }

        public Boolean Subscribe(String topic_name, String channel_name)
        {
            try
            {
                var subscribed = _protocol.Subscribe(topic_name, channel_name).Equals(NSQResponseString.OK);
                _protocol.Ready(ReadyCount);
                return subscribed;
            }
            catch
            {
                return false;
            }
        }

        private void NSQProtocolMessageRecieved(object sender, NSQMessageEventArgs e)
        {
            this.OnNSQMessageRecieved(e);
        }

        public event NSQMessageRecievedHandler NSQMessageRecieved;

        public Task<NSQMessage> ReceiveMessageAsync()
        {
            return _protocol.ReceiveMessageAsync();
        }

        private void OnNSQMessageRecieved(NSQMessageEventArgs e)
        {
            if (NSQMessageRecieved != null)
                NSQMessageRecieved(this, e);
        }
    }
}
