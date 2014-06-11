using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSQnet
{
    /// <summary>
    /// This class listens to the NSQ Lookup server and dispatches messages to handlers. 
    /// </summary>
    public class NSQ
    {
        public NSQ() 
        {
            this.PollEveryMilliseconds = 500;
            this.MaxReadyCount = 5; 
            this.Topics = new HashSet<String>();
        }

        public NSQ(String hostname) 
            : this()
        {
            _lookupClient = new NSQLookup(hostname);
        }
        public NSQ(String hostname, Int32 port)
            : this()
        {
            _lookupClient = new NSQLookup(hostname, port);
        }

        /// <summary>
        /// The action to take when a message is recieved.
        /// Note: this method is required.
        /// Note #2: You are responsible for marking a message "Finished" and resetting the ready count.
        /// </summary>
        public NSQMessageRecievedHandler MessageHandler { get; set; }

        /// <summary>
        /// Use this if you want a custom step to run when a disconnection happens.
        /// </summary>
        public NSQProtocolDisconnectedHandler DisconnectedHandler { get; set; }

        public String Hostname { get { return _lookupClient.Hostname; } }
        public Int32 Port { get { return _lookupClient.Port; } }
        public Int32 MaxReadyCount { get; set; }

        public Int32 PollEveryMilliseconds { get; set; }

        public HashSet<String> Topics { get; set; }
        public IEnumerable<String> AvailableTopics { get { return _lookupClient.Topics(); } }

        private NSQLookup _lookupClient { get; set; }
        private ConcurrentDictionary<String, NSQSubscriber> _mappedSubscribers = new ConcurrentDictionary<String, NSQSubscriber>();

        /// <summary>
        /// Start listening to the Lookup server to see if there are new producers for topics. Will block the current thread.
        /// </summary>
        public void Listen()
        {
            if (_lookupClient == null)
            {
                throw new InvalidOperationException("_lookup client hasn't been initialized; can't start listening.");
            }

            var up = _lookupClient.Ping();
            if (up)
            {
                while (true)
                {
                    _checkForNewProducers();
                    Thread.Sleep(PollEveryMilliseconds);
                }
            }
            else
            {
                throw new Exception("NSQ Lookup server is down.");
            }
        }

        private void _checkForNewProducers()
        {
            var topics = _lookupClient.Topics();

            foreach (var topic in topics)
            {
                if (this.Topics.Any() && this.Topics.Contains(topic))
                {
                    foreach (var producer in _lookupClient.ProducersForTopic(topic))
                    {
                        var addr = (producer.Broadcast_Address ?? producer.Hostname).ToLower();

                        if (!_mappedSubscribers.ContainsKey(addr))
                        {
                            var sub = _getSubscriber(addr, addr, addr, (int) producer.TCP_Port, topic);
                            _mappedSubscribers.AddOrUpdate(sub.LongIdentifier, sub, (long_id, oldSub) => sub);
                        }
                        else if (!_mappedSubscribers[addr].IsSubscribed(topic, topic))
                        {
                            _mappedSubscribers[addr].Subscribe(topic, topic);
                        }
                    }
                }
            }
        }

        private NSQSubscriber _getSubscriber(String shortId, String longId, String host, Int32 port, String topicName, String channelName = null)
        {
            var sub = new NSQSubscriber(shortId, longId, host, port);

            channelName = channelName ?? topicName;

            sub.Initialize();

            sub.MaxReadyCount = this.MaxReadyCount;

            if (this.MessageHandler != null)
            {
                sub.NSQMessageRecieved += new NSQMessageRecievedHandler(this.MessageHandler);
            }

            if (this.DisconnectedHandler != null)
            {
                sub.NSQClientDisconnected += new NSQProtocolDisconnectedHandler(this.DisconnectedHandler);
            }

            Action<Object, EventArgs> defaultDisconnectedHandler = (sender, e) =>
            {
                var disconnected_sub = sender as NSQSubscriber;

                NSQSubscriber cached_sub = null;
                _mappedSubscribers.TryRemove(disconnected_sub.LongIdentifier.ToLower(), out cached_sub);
                disconnected_sub.Dispose();
                cached_sub.Dispose();
                disconnected_sub = null;
                cached_sub = null;
            };

            sub.NSQClientDisconnected += new NSQProtocolDisconnectedHandler(defaultDisconnectedHandler);

            sub.Subscribe(topicName, channelName);
            sub.ResetReadyCount();

            return sub;
        }
    }
}
