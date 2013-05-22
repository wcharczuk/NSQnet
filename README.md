#NSQ.net#
This project is a .net implementation of the [NSQ protocol](https://github.com/bitly/nsq/blob/master/docs/protocol.md).

It leverages the async / await pattern and ~~is~~ will be used in production at [Clothes Horse](https://www.clotheshor.se)

###Examples###

**Subscriber**
```C#
var sub = new NSQSubscriber("127.0.0.1", 4150);
sub.Initialize();
sub.MaxReadyCount = 1;

Action<Object, NSQMessageEventArgs> messageHandler = (sender, e) =>
{
    Console.WriteLine("Processed Message");
    sub.Finish(e.Message.MessageId);
    sub.UpdateReadyCount();
};

sub.NSQMessageRecieved += new NSQMessageRecievedHandler(messageHandler);
sub.Subscribe("activities", "activities");
sub.UpdateReadyCount();

//handle a bunch of messages.
```

**Publisher**
```C#
var pub = new NSQPublisher("192.168.1.17", 4150);
pub.Initialize();
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

//MPUB
pub.Publish("activities", data);

//PUB
pub.Publish("activities", GetData());
```

###Thanks###
To the team behind [NSQ](https://github.com/bitly/nsq)
