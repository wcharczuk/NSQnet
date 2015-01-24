using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace NSQnet.Test
{
    public class NSQProtocol_Tests
    {
        [Fact]
        public void UnpackMessage_Test()
        {
            // TODO: requires nsqlookupd/nsqd to be running. consider refactoring for testability.
            // I'd like to test PackMessage and UnpackMessage in NSQProtocol in isolation.

            // Arrange

            const string expectedName = "ʕ◔ϖ◔ʔ";
            DateTime expectedHireDate = new DateTime(2006, 1, 2, 15, 4, 5);

            NSQMessageEventArgs nsqMessageEventArgs = null;

            var pub = new NSQPublisher("127.0.0.1", 4150);
            pub.Initialize();

            var sub = new NSQ("127.0.0.1", 4161);
            sub.Topics.Add("unit_test_topic");
            sub.MessageHandler = (s, e) =>
                                 {
                                     ((NSQSubscriber)s).Finish(e.Message.MessageId);
                                     nsqMessageEventArgs = e;
                                 };

            Thread t = new Thread(sub.Listen);
            t.IsBackground = true;
            t.Start();

            // Act

            var sendTime = DateTime.UtcNow;
            pub.Publish("unit_test_topic", new { Name = expectedName, HireDate = expectedHireDate });

            while (nsqMessageEventArgs == null)
            {
                Task.Delay(50).Wait();

                if (DateTime.UtcNow > sendTime + TimeSpan.FromSeconds(2))
                {
                    Assert.True(false, "Timeout waiting for message");
                }
            }

            t.Abort();

            // Assert

            Assert.NotNull(nsqMessageEventArgs);
            Assert.NotNull(nsqMessageEventArgs.Message);
            Assert.NotNull(nsqMessageEventArgs.Message.Body);
            Assert.NotNull(nsqMessageEventArgs.Message.TimeStamp);

            dynamic body = JsonConvert.DeserializeObject(nsqMessageEventArgs.Message.Body);

            Assert.Equal(expectedName, (string)body.Name);
            Assert.Equal(expectedHireDate, (DateTime)body.HireDate);
            Assert.InRange(nsqMessageEventArgs.Message.TimeStamp.Value, sendTime, sendTime + TimeSpan.FromSeconds(2));
        }
    }
}
