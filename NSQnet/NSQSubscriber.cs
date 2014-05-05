using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQSubscriber : NSQClient
    {
        public class Subscription
        {
            public String Topic { get; set; }
            public String Channel { get; set; }

            public String HashId { get { return String.Format("{0}.{1}", Topic, Channel); } }

            public override int GetHashCode()
            {
                return HashId.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var that = obj as Subscription;
                if(that == null)
                    return false;

                return this.HashId.Equals(that.HashId);
            }
        }

        public NSQSubscriber() : base() 
        {
            this.MaxReadyCount = 2500;
        }

        public NSQSubscriber(String hostname, Int32 port) : base(hostname, port) 
        {
            this.MaxReadyCount = 2500;
        }

        public NSQSubscriber(String shortIdentifier, String longIdentifier, String hostname, Int32 port) :
            base(shortIdentifier, longIdentifier, hostname, port)
        {
            this.MaxReadyCount = 2500;
        }

        public override void Initialize()
        {
            base.Initialize();
            _readyCount = MaxReadyCount;
            _protocol.NSQMessageReceived += new NSQMessageRecievedHandler(NSQProtocolMessageRecieved);
        }

        public Int32 MaxReadyCount { get; set; }

        public ConcurrentBag<Subscription> _subscriptions = new ConcurrentBag<Subscription>();

        public Boolean IsSubscribed(String topic, String channel)
        {
            return _subscriptions.Contains(new Subscription() { Topic = topic, Channel = channel });
        }

        private Int32 _readyCount = default(int);
        public Int32 ReadyCount { get { return _readyCount; } }

        private Int64 _processingCount = default(Int32);
        public Int64 ProccessingCount { get { return _processingCount; } }

        /// <summary>
        /// SUB - subscribe to a specified topic/channel
        /// </summary>
        /// <param name="topic_name"></param>
        /// <param name="channel_name"></param>
        /// <returns></returns>
        public void Subscribe(String topic_name, String channel_name)
        {
            _protocol.Subscribe(topic_name, channel_name);
            _subscriptions.Add(new Subscription() { Topic = topic_name, Channel = channel_name });
        }

        /// <summary>
        /// REQ - re-queue a message (indicate failure to procees)
        /// </summary>
        /// <param name="message_id"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public void Requeue(String message_id, Int32 timeout)
        {
            _protocol.Requeue(message_id, timeout);
        }

        /// <summary>
        /// FIN - finish a message (indicate successful processing)
        /// </summary>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public void Finish(String message_id)
        {
            _protocol.Finish(message_id);
        }

        /// <summary>
        /// TOUCH - reset the timeout for an in-flight message.
        /// </summary>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public void Touch(String message_id)
        {
            _protocol.Touch(message_id);
        }

        /// <summary>
        /// Send the RDY Command to the NSQD with the current ready count.
        /// </summary>
        public void UpdateReadyCount()
        {
            _protocol.Ready(_readyCount);
        }

        /// <summary>
        /// RDY - update RDY state (indicate you are ready to receive messages) to MaxReadyCount;
        /// </summary>
        public void ResetReadyCount()
        {
            _readyCount = MaxReadyCount;
            _protocol.Ready(_readyCount);
        }

        #region Events

        private void NSQProtocolMessageRecieved(object sender, NSQMessageEventArgs e)
        {
            this.OnNSQMessageRecieved(e);
        }

        /// <summary>
        /// Fire this delegate when a message is recieved. 
        /// </summary>
        /// <remarks>
        /// Will be off the main thread!
        /// </remarks>
        public event NSQMessageRecievedHandler NSQMessageRecieved;

        private void OnNSQMessageRecieved(NSQMessageEventArgs e)
        {
            System.Threading.Interlocked.Decrement(ref _readyCount);

            if (_readyCount == 0)
            {
                //signal failure??
                return;
            }
            
            System.Threading.Interlocked.Increment(ref _processingCount);

            if (NSQMessageRecieved != null)
                NSQMessageRecieved(this, e);

            System.Threading.Interlocked.Decrement(ref _processingCount);
            System.Threading.Interlocked.Increment(ref _readyCount);
            UpdateReadyCount();
        }
        #endregion
    }
}
