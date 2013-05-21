using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NSQnet.Test
{
    public class NSQClient_Tests
    {
        [Fact]
        public void Constructor_Test()
        {
            var client = new NSQClient("192.168.1.147", 4150);

            Assert.NotNull(client);
        }
    }
}
