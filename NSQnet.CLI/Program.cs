using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSQnet.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread pub = new Thread(new ThreadStart(DoPublisher));
            pub.Start();
            Thread sub = new Thread(new ThreadStart(DoSubscriber));
            sub.Start();

            pub.Join();
            sub.Join();
        }

        public static void DoSubscriber()
        {
            var sub = new NSQSubscriber("192.168.1.17", 4150);
            sub.Initialize();

            Console.WriteLine("Subscriber Connected.");

            Action<Object, NSQMessageEventArgs> messageHandler = (sender, e) =>
            {
                Console.WriteLine("Processed Message");
                sub.Finish(e.Message.MessageId);
                sub.UpdateReadyCount();
            };

            sub.MaxReadyCount = 1;
            sub.NSQMessageRecieved += new NSQMessageRecievedHandler(messageHandler);
            sub.Subscribe("activities", "activities");
            sub.UpdateReadyCount();

            Console.WriteLine("Subscribed To \"activities\"");

            while (sub.IsConnected)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Subscriber Disconnected.");
        }

        public static void DoPublisher()
        {
            var pub = new NSQPublisher("192.168.1.17", 4150);
            pub.Initialize();

            Console.WriteLine("Publisher Connected.");
            var data = new List<Object>()
            {   
                GetData(),
                GetData(),
                GetData(),
                GetData(),
                GetData(),
                GetData(),
                GetData()
            };

            pub.Publish("activities", data);
        }

        public static object GetData()
        {
            dynamic obj = new AgileObject();
            obj.Id = System.Guid.NewGuid().ToString("N");
            obj.ActivityTypeId = 201;
            obj.ActivityDateTime = DateTime.Now;
            obj.IpAddress = "68.199.76.175";
            obj.ServerName = "Web1";
            obj.CustomerId = 18;
            obj.SiteAreaId = 2;
            obj.AlphaBetaTestId = 13;
            obj.AlphaBetaTestStateId = 2;
            obj.ProductGroupId = 46;
            obj.TrackingId = "ccr81YL1ErF6eHikLIlFA3LDEr";
            obj.SessionId = "z2fra5tcnyxdmhmdo3lr525c";
            obj.UserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36";
            obj.uiCulture = "en-US";
            return obj;
        }
    }
}
