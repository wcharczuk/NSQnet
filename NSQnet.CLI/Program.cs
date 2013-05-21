using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NSQPublisher("192.168.1.147", 4150, Console.OpenStandardOutput());
            client.Identify("test_client", "test_client", 5000, false);
            Console.ReadKey();
            client.DestroyConnection();
        }
    }
}
