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
            try
            {
                var sub = new NSQSubscriber("192.168.1.17", 4150);
                sub.Initialize();
                sub.NSQAnyMessageRecieved += new NSQMessageRecievedHandler(sub_NSQMessageRecieved);
                sub.Subscribe("activities", "activities");

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void DoPublisher()
        {
            var pub = new NSQPublisher("192.168.1.17", 4150);
            pub.Initialize();

            Task pubTask = new Task(() =>
            {
                try
                {
                    for (int x = 0; x < 1000; x++)
                    {
                        var result = pub.Publish("activities", GetData());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });

            pubTask.Start();
        }

        public static void sub_NSQMessageRecieved(object sender, NSQMessageEventArgs e)
        { 
            Console.WriteLine(e.Message.Body);
            Thread.Sleep(1000);
        }

        public static object GetData()
        {
            dynamic obj = new AgileObject();
            obj.ActivityTypeId = 201;
            obj.ActivityDateTime = "2013-05-21 14:04:52.097";
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
