using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public class NSQMessage
    {
        public NSQMessage() { }
        public Int32 Size { get; set; }
        public FrameType FrameType { get; set; }
        public String Body { get; set; }

        public Int16? Attempts { get; set; }
        public DateTime? TimeStamp { get; set; }
        public String? MessageId { get; set; }
    }
}
