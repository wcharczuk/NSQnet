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

            sub.MaxReadyCount = 5;
            sub.NSQMessageRecieved += new NSQMessageRecievedHandler(sub_NSQMessageRecieved);
            sub.Subscribe("activities", "activities");
            sub.UpdateReadyCount();

            Console.WriteLine("Subscribed To \"activities\"");

            while (sub.IsConnected)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Subscriber Disconnected.");
        }

        public static void sub_NSQMessageRecieved(object sender, NSQMessageEventArgs e)
        {
            var sub = sender as NSQSubscriber;
            sub.Finish(e.Message.MessageId);
            Console.WriteLine("Processed Message");
            sub.UpdateReadyCount();
        }

        public static void DoPublisher()
        {
            var pub = new NSQPublisher("192.168.1.17", 4150);
            pub.Initialize();

            Console.WriteLine("Publisher Connected.");
            for (int x = 0; x < 1000; x++)
            {
                pub.Publish("activities", GetData());
                Console.WriteLine("Published Message");
            }
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
