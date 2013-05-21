using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQProtocolException : Exception
    {
        public NSQProtocolException() : base() { }
        public NSQProtocolException(String message) : base(message) { }
    }
}
