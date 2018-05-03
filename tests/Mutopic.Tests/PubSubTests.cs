using Mutopic.Middleware;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Mutopic.Tests
{
    public class PubSubTests
    {
        const string TOPIC = "test_topic";

        [Fact]
        public void Should_publish_messages_of_any_type()
        {
            var sut = new PubSub();

            var received = new List<object>();
            using (var subscription = sut.Subscribe(TOPIC, (object message) => { received.Add(message); }))
            {
                sut.Publish(42, TOPIC);
                sut.Publish("message", TOPIC);
                sut.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
            }

            received.ShouldBe(new object[] { 42, "message" });
        }

        [Fact]
        public void Should_not_receive_after_subscription_disposal()
        {
            var sut = new PubSub();

            var received = 0;
            var subscription = sut.Subscribe(TOPIC, (object message) => received++);
            subscription.Dispose();

            sut.Publish(42, TOPIC);
            sut.Publish("message", TOPIC);

            received.ShouldBe(0); ;
        }

        [Fact]
        public void Should_not_throw_exception_from_handler()
        {
            var sut = new PubSub();
            var received = new List<object>();
            sut.OnSubscriptionException += (sub, mess, ex) => { received.Add(mess); };

            using (var subscription = sut.Subscribe(TOPIC, (object message) => throw new IndexOutOfRangeException()))
            {
                sut.Publish(42, TOPIC);
                sut.Publish("message", TOPIC);
                sut.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
            }

            received.ShouldBe(new object[] { 42, "message" });
        }

        [Fact]
        public void Should_not_throw_exception_from_OnSubscriptionException_handler()
        {
            var sut = new PubSub();
            sut.OnSubscriptionException += (sub, mess, ex) => throw new IndexOutOfRangeException("AGAIN !");

            using (var subscription = sut.Subscribe(TOPIC, (object message) => throw new IndexOutOfRangeException("primary exception")))
            {
                sut.Publish(42, TOPIC);
            }
        }

        [Fact]
        public void Should_publish_nothing_if_context_setup_says_so()
        {
            var sut = new PubSub(
                new GenericPublishMiddleware(ctx =>
                {
                    if (ctx.message is string) return ctx;
                    else return (false, null, null);
                }));

            var received = new List<object>();
            using (var subscription = sut.Subscribe(TOPIC, (object message) => { received.Add(message); }))
            {
                sut.Publish(42, TOPIC);
                sut.Publish("message is a string", TOPIC);
                sut.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
            }

            received.ShouldBe(new object[] { "message is a string" });
        }

        [Fact]
        public void Should_not_publish_same_message_twice_in_the_same_topic()
        {
            var sut = new PubSub();

            var receivedCount = 0;
            using (var subscription = sut.Subscribe(TOPIC, (object message) => receivedCount++))
            {
                sut.Publish(42, TOPIC, TOPIC, TOPIC, TOPIC);
            }

            receivedCount.ShouldBe(1);
        }
    }
}
