using Mutopic.Reactive;
using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Xunit;

namespace Mutopic.Tests.Reactive
{
    public class ReactiveExtensionsTests
    {
        const string TOPIC = "test_topic";

        [Fact]
        public void SubscribeObservable()
        {
            var sut = new PubSub();

            var received = new List<object>();
            using (var observable = sut.SubscribeObservable<int>(TOPIC))
            {
                using (observable
                    .Where(i => i == 42)
                    .Select(i => $"the answer to life the universe and everything is {i}.")
                    .Subscribe(s => received.Add(s))
                    )
                {
                    sut.Publish(42, TOPIC);
                    sut.Publish("not subscribed type", TOPIC);
                    sut.Publish(42, "not the correct topic name");
                    sut.Publish(41, TOPIC); // nopek
                    sut.Publish(42, TOPIC);
                }

                sut.Publish(42, TOPIC); // no effect here cause observable subscription is disposed
            }

            received.ShouldBe(new[]
            {
                "the answer to life the universe and everything is 42.",
                "the answer to life the universe and everything is 42."
            });
        }


        [Fact]
        public void Super_Hello_World()
        {
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
        }
    }
}
