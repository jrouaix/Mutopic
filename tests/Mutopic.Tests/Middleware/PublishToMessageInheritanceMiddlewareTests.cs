using Mutopic.Middleware;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Mutopic.Tests.Middleware
{
    public class PublishToMessageInheritanceMiddlewareTests
    {
        interface IA { }
        interface IB { }
        interface IC { }

        class A : IA { }
        class B : A, IB { }
        class C : IA, IC { }

        [Fact]
        public void Example()
        {
            const string TOPIC = "topic";

            var sut = new PubSubBuilder()
                // this is middleware
                // it will propage messages in all their inheritance topic names
                .WithMessageInheritancePublishing()
                .Build();

            var b = new B();

            var topicReceived = new List<object>();
            var AReceived = new List<object>();
            var IBReceived = new List<object>();
            using (sut.Subscribe<object>(TOPIC, topicReceived.Add))
            using (sut.Subscribe<A>(typeof(A).GetTopicName(), AReceived.Add))
            using (sut.Subscribe<IB>(typeof(IB).GetTopicName(), IBReceived.Add))
            {
                sut.Publish(b, TOPIC);
            }

            // Message have been propagaged in multiple topic names
            topicReceived.ShouldBe(new object[] { b });
            AReceived.ShouldBe(new object[] { b });
            IBReceived.ShouldBe(new object[] { b });
        }


        [Theory]
        [MemberData(nameof(GetTestData))]
        public void SetupContext(
            bool shouldPublish, object message, string[] topicNames,
            bool expectedShouldPublish, object expectedMessage, string[] expectedTopicNames
            )
        {
            var sut = new MessageInheritancePublishMiddleware();

            var result = sut.SetupContext((shouldPublish, message, topicNames));

            result.shouldPublish.ShouldBe(expectedShouldPublish);
            result.message.ShouldBe(expectedMessage);
            result.topicNames.ShouldBe(expectedTopicNames);
        }

        public static IEnumerable<object[]> GetTestData()
        {
            {
                var message = new A();
                yield return new object[]
                {
                true, message, new string[0],
                true, message, new [] {nameof(A), nameof(Object), nameof(IA)}
                };
            }
            {
                var message = (IA)new A();
                yield return new object[]
                {
                true, message, new string[]{"test" },
                true, message, new [] { "test", nameof(A), nameof(Object), nameof(IA)}
                };
            }
            {
                var message = new B();
                yield return new object[]
                {
                true, message, new string[]{"test1", "test2" },
                true, message, new [] { "test1", "test2", nameof(B), nameof(A), nameof(Object), nameof(IA), nameof(IB)}
                };
            }
            {
                var message = new C();
                yield return new object[]
                {
                false, message, new string[0],
                false, message, new [] { nameof(C), nameof(Object), nameof(IA), nameof(IC)}
                };
            }
            {
                var message = (object)null;
                yield return new object[]
                {
                true, message, new string[0],
                true, message, new string[0]
                };
            }
            {
                var message = (object)null;
                yield return new object[]
                {
                false, message, new string[0],
                false, message, new string[0]
                };
            }
        }




    }
}
