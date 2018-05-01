using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Utopic.Tests
{
    public class PubSubTests
    {
        [Fact]
        public void Should_publish_messages_of_any_type()
        {
            var sut = new PubSub();

            var received = new List<object>();
            using (var subscription = sut.Subscribe("topic", (object message) => { received.Add(message); }))
            {
                sut.Publish(42, "topic");
                sut.Publish("message", "topic");
                sut.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
            }

            received.ShouldBe(new object[] { 42, "message" });
        }

        [Fact]
        public void Should_not_receive_after_subscription_disposal()
        {
            var sut = new PubSub();

            var received = 0;
            var subscription = sut.Subscribe("topic", (object message) => received++);
            subscription.Dispose();

            sut.Publish(42, "topic");
            sut.Publish("message", "topic");

            received.ShouldBe(0); ;
        }

        [Fact]
        public void Should_not_throw_exception_from_handler()
        {
            var sut = new PubSub();

            using (var subscription = sut.Subscribe("topic", (object message) => throw new IndexOutOfRangeException()))
            {
                sut.Publish("haha", "topic");
            };
        }
    }
}
