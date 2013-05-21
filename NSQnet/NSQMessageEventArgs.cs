using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQMessageEventArgs : EventArgs
    {
        public NSQMessageEventArgs() : base() { }
        public NSQMessageEventArgs(NSQMessage message)
            : base()
        {
            this.Message = message;
        }

        public NSQMessage Message { get; set; }
    }
}
