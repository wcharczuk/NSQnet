using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public abstract class NSQClient : IDisposable
    {
        public NSQClient()
        {
            _protocol = new NSQProtocol();
            _protocol.NSQProtocolDisconnected += new NSQProtocolDisconnectedHandler(NSQProtocolDisconnected_Handler);
            this.ShortIdentifier = System.Net.Dns.GetHostName();
            this.LongIdentifier = System.Guid.NewGuid().ToString("N"); 
        }

        public NSQClient(String hostname, Int32 port) : this()
        {
            _protocol.Hostname = hostname;
            _protocol.Port = port;
        }

        public NSQClient(String shortIdentifier, String longIdentifier, String hostname, Int32 port)
            : this(hostname, port)
        {
            this.ShortIdentifier = shortIdentifier;
            this.LongIdentifier = longIdentifier;
        }

        protected NSQProtocol _protocol = null;

        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }

        public String ShortIdentifier { get; set; }
        public String LongIdentifier { get; set; }

        public Int32 HeartbeatMilliseconds { get; set; }

        public void NSQProtocolDisconnected_Handler(object sender, EventArgs e)
        {
            OnNSQClientDisconnected(e);
        }

        public event NSQProtocolDisconnectedHandler NSQClientDisconnected;
        public void OnNSQClientDisconnected(EventArgs e)
        {
            var handler = this.NSQClientDisconnected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Boolean IsConnected
        {
            get
            {
                return _protocol.IsConnected;
            }
        }

        public virtual void Initialize()
        {
            _protocol.Initialize();
            _protocol.Identify(this.ShortIdentifier, this.LongIdentifier, this.HeartbeatMilliseconds, false); 
        }

        public virtual void Stop()
        {
            try
            {
                _protocol.DestroyConnection();
            }
            catch { }
            _protocol = null;
        }

        public void Dispose()
        {
            if (_protocol != null)
            {
                _protocol.Dispose();
                _protocol = null;
            }
        }
    }
}
