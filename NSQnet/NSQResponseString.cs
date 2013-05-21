using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public static class NSQResponseString
    {
        public const String HEARTBEAT = "_heartbeat_";
        public const String OK = "OK";
        public const String E_INVALID = "E_INVALID";
        public const String E_TOUCH_FAILED = "E_TOUCH_FAILED";
        public const String E_REQ_FAILED = "E_REQ_FAILED";
        public const String E_FIN_FAILED = "E_FIN_FAILED";
        public const String E_BAD_TOPIC = "E_BAD_TOPIC";
        public const String E_BAD_BODY = "E_BAD_BODY";
        public const String E_BAD_MESSAGE = "E_BAD_MESSAGE";
        public const String E_BAD_CHANNEL = "E_BAD_CHANNEL";
        public const String E_MPUB_FAILED = "E_MPUB_FAILED";
        public const String E_PUB_FAILED = "E_PUB_FAILED";
        public const String CLOSE_WAIT = "CLOSE_WAIT";
    }
}
