using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Mutopic.Tests.Examples
{
    public class SuperHelloWorld
    {
        [Fact]
        public void Test()
        {
            const string TOPIC = "topicName";
            var randy = new Random();
            var sut = new PubSubBuilder().Build();

            var start = DateTime.Now;

            var received = new ConcurrentBag<(string value, TimeSpan elapsed)>();
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
                    .Subscribe(s => received.Add((s, DateTime.Now - start)))                    // this is reactive extensions subscription
                    )
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    for (int i = 0; i < 100; i++)
                    {
                        sut.Publish(i, TOPIC);
                    }
                    sw.Stop();
                    sw.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(0.1)); // This was quick !

                    Thread.Sleep(50 * 100); // Give it time to process all
                    count.ShouldBe(100);
                    received.Count.ShouldBe(1);
                    var result = received.Single();
                    result.value.ShouldBe("the answer to life the universe and everything is 42.");
                    result.elapsed.ShouldBeGreaterThan(sw.Elapsed);
                }
            }
        }
    }
}
