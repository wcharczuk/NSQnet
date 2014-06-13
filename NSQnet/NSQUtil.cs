using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet
{
    public sealed class NSQUtil
    {
        public static async Task<string> PublishAsync(string nsqd_http_address, string topic, IEnumerable<string> messages)
        {
            if (messages.Any() == false)
            {
                throw new ArgumentException("messages is empty", "messages");
            }

            var uri = new Uri(nsqd_http_address);
            var baseUriString = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var req = new NSQUtil(baseUriString);
            var res = await req.mput(topic, messages);
            return res;
        }

        public static async Task<string> PublishAsync(string nsqd_http_address, string topic, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("a message cannot be null or blank", "message");
            }

            var uri = new Uri(nsqd_http_address);
            var baseUriString = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var req = new NSQUtil(baseUriString);
            var res = await req.put(topic, message);
            return res;
        }

        private string baseUriString;

        #region .ctor
        private NSQUtil(string baseUriString)
        {
            this.baseUriString = baseUriString;
        }
        #endregion

        private async Task<string> put(string topic, string message)
        {
            var path = string.Format("{0}/put?topic={1}", baseUriString, topic);
            var data = message;

            using (var http = new WebClient())
            {
                var resp = await http.UploadStringTaskAsync(path, "POST", data);
                return resp;
            }
        }

        private async Task<string> mput(string topic, IEnumerable<string> messages)
        {
            var path = string.Format("{0}/mput?topic={1}", baseUriString, topic);
            var data = string.Join("\n", messages);

            using (var http = new WebClient())
            {
                var resp = await http.UploadStringTaskAsync(path, "POST", data);
                return resp;
            }
        }
    }
}
