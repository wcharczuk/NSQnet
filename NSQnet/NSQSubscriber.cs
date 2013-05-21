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
        public NSQSubscriber() : base() 
        {
            this.MaxReadyCount = 2500;
        }

        public NSQSubscriber(String hostname, Int32 port) : base(hostname, port) 
        {
            this.MaxReadyCount = 2500; 
        }

        public NSQSubscriber(String hostname, Int32 port, Stream output)
            : base(hostname, port, output) 
        {
            this.MaxReadyCount = 2500; 
        }

        public override void Initialize()
        {
            base.Initialize();

            _protocol.NSQMessageRecieved += new NSQMessageRecievedHandler(NSQProtocolMessageRecieved);
            _protocol.NSQAnyMessageRecieved += new NSQMessageRecievedHandler(NSQProtocolAnyMessageRecieved);

            this.ReadyCount = MaxReadyCount;
        }

        public Int32 MaxReadyCount { get; set; }
        public Int32 ReadyCount { get; set; }

        public Int16 MaxAttemptCount { get; set; }

        public Int64 MessagesInFlight { get; private set; }
        public Int64 MessagesRecieved { get; private set; }
        public Int64 MessagesFinished { get; private set; }
        public Int64 MessagesRequeued { get; private set; }

        public Int32 BackoffCounter { get; private set; }

        public void Subscribe(String topic_name, String channel_name)
        {
            _protocol.Subscribe(topic_name, channel_name);
        }

        public void UpdateReadyCount()
        {
            _protocol.Ready(ReadyCount);
        }

        private void NSQProtocolMessageRecieved(object sender, NSQMessageEventArgs e)
        {
            this.OnNSQMessageRecieved(e);
        }

        private void NSQProtocolAnyMessageRecieved(object sender, NSQMessageEventArgs e)
        {
            this.OnNSQAnyMessageRecieved(e);
        }

        public event NSQMessageRecievedHandler NSQMessageRecieved;

        private void OnNSQMessageRecieved(NSQMessageEventArgs e)
        {
            if (NSQMessageRecieved != null)
                NSQMessageRecieved(this, e);
        }

        public event NSQMessageRecievedHandler NSQAnyMessageRecieved;

        private void OnNSQAnyMessageRecieved(NSQMessageEventArgs e)
        {
            if (NSQAnyMessageRecieved != null)
                NSQAnyMessageRecieved(this, e);
        }
    }
}
