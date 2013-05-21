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
    public class NSQClient
    {
        public NSQClient() : base() 
        {
            _protocol = new NSQProtocol();
        }

        public NSQClient(String hostname, Int32 port) : this()
        {
            _protocol.Hostname = hostname;
            _protocol.Port = port;
        }

        public NSQClient(String hostname, Int32 port, Stream output) : this()
        {
            _protocol.Hostname = hostname;
            _protocol.Port = port;
            _protocol.OutputStream = output;
        }

        private NSQProtocol _protocol = null;

        public void Initialize()
        {
            _protocol.Initialize();
        }

        public Boolean Publish(String topic_name, Object data)
        {
            try
            {
                return _protocol.Publish(topic_name, data).Equals(Response.OK);
            }
            catch
            {
                return false;
            }
        }
    }
}

