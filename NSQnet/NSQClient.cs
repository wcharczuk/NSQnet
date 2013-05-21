using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public abstract class NSQClient
    {
        public NSQClient()
        {
            _protocol = new NSQProtocol();
        }

        public NSQClient(String hostname, Int32 port) : this()
        {
            _protocol.Hostname = hostname;
            _protocol.Port = port;
        }

        public NSQClient(String hostname, Int32 port, Stream output)
            : this()
        {
            _protocol.Hostname = hostname;
            _protocol.Port = port;
            _protocol.OutputStream = output;
        }

        protected NSQProtocol _protocol = null;

        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }

        public String ShortIdentifier { get; set; }
        public String LongIdentifier { get; set; }

        public Int32 HeartbeatMilliseconds { get; private set; }

        public void Initialize()
        {
            _protocol.Initialize();
            _protocol.Identify(this.ShortIdentifier, this.LongIdentifier, this.HeartbeatMilliseconds, false); 
            //todo: in future, feature negotiation. test if response is json or not.

        }
    }
}
