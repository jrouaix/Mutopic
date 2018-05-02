using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Mutopic.Tests
{
    public class PubSubBuilderTests
    {
        const string TOPIC = "test_topic";

        [Fact]
        public void Should_apply_middleware_in_reverse_order()
        {
            var sut = new PubSubBuilder()
                .WithPublishMiddleware(ctx => (true, ctx.message.ToString() + ", applied A", ctx.topicNames))
                .WithPublishMiddleware(ctx => (true, ctx.message.ToString() + ", applied B", ctx.topicNames))
                ;

            var pubsub = sut.Build();

            var received = new List<object>();
            using (var subscription = pubsub.Subscribe(TOPIC, (object message) => { received.Add(message); }))
            {
                pubsub.Publish(42, TOPIC);
                pubsub.Publish("message", TOPIC);
                pubsub.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
            }

            received.ShouldBe(new object[] { "42, applied B, applied A", "message, applied B, applied A" });
        }

        [Fact]
        public void Should_apply_middleware_topic_changes()
        {
            var ctxLogger = new List<(bool shouldPublish, object message, string[] topicNames)>();
            var sut = new PubSubBuilder()
                .WithPublishMiddleware(ctx => { ctxLogger.Add(ctx); return ctx; })
                .WithPublishMiddleware(ctx => (
                    true,
                    ctx.message,
                    ctx.topicNames.SelectMany(t => t == "spread_topic" ? new[] { "spread1", "spread2" } : new[] { t }).ToArray()
                    ))
                .WithPublishMiddleware(ctx => (
                    true,
                    ctx.message,
                    ctx.topicNames.Select(t => t == "reroute_topic" ? "rerouted" : t).ToArray()
                    ))
                ;

            var pubsub = sut.Build();

            var received = new List<object>();
            using (pubsub.Subscribe(TOPIC, (object message) => { received.Add(message); }))
            using (pubsub.Subscribe("spread1", (object message) => { received.Add(message); }))
            using (pubsub.Subscribe("spread2", (object message) => { received.Add(message); }))
            using (pubsub.Subscribe("rerouted", (object message) => { received.Add(message); }))
            {
                pubsub.Publish(42, TOPIC);
                pubsub.Publish("route_me", TOPIC, "reroute_topic");
                pubsub.Publish("spread_me", "spread_topic", TOPIC);
                pubsub.Publish("dead letter", "no_one_listen_topic"); // no handler subscribed on this topic
            }

            received.ShouldBe(new object[] {
                42,
                "route_me", "route_me",
                "spread_me", "spread_me", "spread_me"
            });

            ctxLogger.SelectMany(ctx => ctx.topicNames).ShouldBe(new string[] {
                TOPIC,
                TOPIC, "rerouted",
                "spread1", "spread2", TOPIC,
                "no_one_listen_topic"
            });
        }
    }
}
