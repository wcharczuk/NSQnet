#NSQ.net#
This project is a .net implementation of the [NSQ protocol](https://github.com/bitly/nsq/blob/master/docs/protocol.md).

##Installation##
* Nuget: ```install-package nsqnet```
* Binary: Download from the releases page, make sure to fill requirements.

###Requrements###
* [.net 4.5](http://www.microsoft.com/visualstudio/eng/downloads#d-net-45) or [Mono 3.0+](http://www.mono-project.com/Release_Notes_Mono_3.0#New_in_Mono_3.0.10)
* [newtonsoft.Json](http://json.codeplex.com/)

###Examples###

**Subscriber**
```C#
var sub = new NSQSubscriber("127.0.0.1", 4150);

sub.Initialize(); //connects to nsqd and identifies.
sub.MaxReadyCount = 5; //or 2500, or whatever you want.

Action<Object, NSQMessageEventArgs> messageHandler = (sender, e) =>
{
    //do your message processing here, can be as complex or long winded
    //as you want because this action will not block the main thread.
    Console.WriteLine("Processed Message");
    sub.Finish(e.Message.MessageId);
};

//this event hook will fire in a separate task/threadpool context
sub.NSQMessageRecieved += new NSQMessageRecievedHandler(messageHandler);
sub.Subscribe("activities", "activities");
sub.ResetReadyCount(); //here we go.
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
