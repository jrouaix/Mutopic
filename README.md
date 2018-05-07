
# <img alt="icon" src="icon.png" height="30"> Mutopic
Micro, yet turbocharged, C# pubsub library.

[![Build status](https://ci.appveyor.com/api/projects/status/1mpqa3gly0xkg8wy/branch/master?svg=true)](https://ci.appveyor.com/project/JromeRx/mutopic/branch/master)
![License](https://img.shields.io/badge/License-Apache_2.0-44cc11.svg)
![Nuget](https://img.shields.io/nuget/v/Mutopic.svg)
![Nuget](https://img.shields.io/nuget/v/Mutopic.Reactive.svg)

## Installing

Using NuGet Package Manager Console:
`PM> Install-Package Mutopic`

## Design principles
- **Topic based** - You publish in a topic name, you subscibe on the topic name too. Any "typed" implementation is just using the GetType().Name as topic name.
- **Fire & Forget** - Publish should never throw any exception
- **Synchronous** - Publish is synchronous and will call the subscribed handler as quick as possible. Then you can achieve asynchronous handling by pluging any task queuing system you need. (example : Reactive Extensions)

## Getting started
The best place to start will be examples (when added). For a moment, best examples will be in the unit tests.


### Subscribe multiple handlers

```csharp
using Mutopic
```

Build a PubSub
```csharp
const string TOPIC = "topicName";
var sut = new PubSubBuilder().Build();

var receivedA = new List<string>();
var receivedB = new List<int>();
var receivedC = new List<object>();
```

Subscribe & Publish
```csharp 
using (var subscriptionA = sut.Subscribe<string>(TOPIC, receivedA.Add))
using (var subscriptionB = sut.Subscribe<int>(TOPIC, receivedB.Add))
using (var subscriptionC = sut.Subscribe<object>(TOPIC, receivedC.Add))
{
    sut.Publish(1, TOPIC);
    sut.Publish("topic to rule them all", TOPIC);
    sut.Publish("dead letter", "no_one_listen_this_topic"); // no handler subscribed on this topic
}
```

Assertions
```csharp
receivedA.ShouldBe(new string[] { "topic to rule them all" });
receivedB.ShouldBe(new int[] { 1 });
receivedC.ShouldBe(new object[] { 1, "topic to rule them all" });
```

### Achieve asynchronous handling with Reactive Extensions

```csharp
using Mutopic;
using Mutopic.Reactive;
```

```csharp
const string TOPIC = "topicName";
var randy = new Random();
var sut = new PubSubBuilder().Build();

var received = new ConcurrentBag<object>();
var count = 0;
using (var observable = sut.SubscribeObservable<int>(TOPIC))
{
    using (observable
        // without this next "ObserveOn" line, the publishing would be blocked by the long running one
        .ObserveOn(TaskPoolScheduler.Default)                                       // after this, all message processing will be asynchronous
        .Do(i => Thread.Sleep(randy.Next(20, 50)))                                  // some long running in the pipeline
        .Do(i => Interlocked.Increment(ref count))
        .Where(i => i == 42)                                                        // some filtering provided by reactive extension
        .Select(i => $"the answer to life the universe and everything is {i}.")     // some transformation ..
        .Subscribe(s => received.Add(s))                                            // this is reactive extensions subscription
        )
    {
        var sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < 100; i++)
        {
            sut.Publish(i, TOPIC);
        }
        sw.Stop();
        sw.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(0.05)); // This was quick !

        Thread.Sleep(50 * 100); // Give it time to process all
        count.ShouldBe(100);
        received.ShouldBe(new[] { "the answer to life the universe and everything is 42." });
    }
}
```



interface IA { }
interface IB { }
interface IC { }

class A : IA { }
class B : A, IB { }
class C : IA, IC { }
```

Build a pubsub with middleware
```csharp
const string TOPIC = "topic";

var sut = new PubSubBuilder()
    // this is a middleware
    // it will propage messages in all their inheritance topic names
    .WithMessageInheritancePublishing()
    .Build();
```

Publish a message
```csharp
var b = new B();

var topicReceived = new List<object>();
var AReceived = new List<object>();
var IBReceived = new List<object>();
using (sut.Subscribe<object>(TOPIC, topicReceived.Add))
using (sut.Subscribe<A>(typeof(A).Name, AReceived.Add))
using (sut.Subscribe<IB>(typeof(IB).Name, IBReceived.Add))
{
    sut.Publish(b, TOPIC);
}
```

Assertions
```csharp
// Message have been propagaged in multiple topic names
topicReceived.ShouldBe(new object[] { b });
AReceived.ShouldBe(new object[] { b });
IBReceived.ShouldBe(new object[] { b });

```    

### Exception handling

Since no exception will buddle from a handler, a OnSubscriptionException event is exposed to collect eventual exception.
Use it to log errors !

```csharp
var sut = new PubSubBuilder().Build();
var received = new List<object>();
sut.OnSubscriptionException += (sub, mess, ex) => { received.Add(mess); };

using (var subscription = sut.Subscribe(TOPIC, (object message) => throw new IndexOutOfRangeException()))
{
    sut.Publish(42, TOPIC);
    sut.Publish("message", TOPIC);
    sut.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
}

received.ShouldBe(new object[] { 42, "message" });
```

# <img alt="Mutopic" src="title.png" height="100">