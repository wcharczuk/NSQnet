#NSQ.net#
This project is a .net implementation of the [NSQ protocol](https://github.com/bitly/nsq/blob/master/docs/protocol.md).

##Installation##
* Nuget: ```install-package nsqnet```
* Binary: Download from the releases page, make sure to fill requirements.

###Requrements###
* [.net 4.5](http://www.microsoft.com/visualstudio/eng/downloads#d-net-45) or [Mono 3.0+](http://www.mono-project.com/Release_Notes_Mono_3.0#New_in_Mono_3.0.10)
* [newtonsoft.Json](http://json.codeplex.com/)

###Examples###

**Lookup Client**

This is the main subscriber system you should use. This class automatically picks up new producers from the lookup server, and subscribes to them. This class can optionally be limited to select topics. 

```C#
var nsq = new NSQ("127.0.0.1");

nsq.MessageHandler = (sender, e) =>
{
    var sub = sender as NSQSubscriber;
    var main_subscription = sub.Subscriptions.FirstOrDefault();
    
    try
    {
        Console.Write(String.Format("{0}::{2}.{1} MSG "
            , sub.Hostname
            , main_subscription.Channel
            , main_subscription.Topic
            )
        );
        Console.WriteLine(e.Message.Body);
        sub.Finish(e.Message.MessageId);
    }
    catch
    {
        sub.Requeue(e.Message.MessageId, 0);
    }
};

//comment these out to receive messages from all topics.
nsq.Topics.Add("activity");
nsq.Topics.Add("informational");

nsq.Listen();
```

**Publisher**
```C#
var pub = new NSQPublisher("192.168.1.17", 4150);
pub.Initialize();

//GetData() here returns an object

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
