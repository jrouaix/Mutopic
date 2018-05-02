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
        [Theory]
        [MemberData(nameof(GetTestData))]
        public void SetupContext(
            bool shouldPublish, object message, string[] topicNames,
            bool expectedShouldPublish, object expectedMessage, string[] expectedTopicNames
            )
        {
            var sut = new PublishToMessageInheritanceMiddleware();

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
                true, message, new string[0],
                true, message, new [] {nameof(A), nameof(Object), nameof(IA)}
                };
            }
            {
                var message = new B();
                yield return new object[]
                {
                true, message, new string[0],
                true, message, new [] { nameof(B), nameof(A), nameof(Object), nameof(IA), nameof(IB)}
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



        interface IA { }
        interface IB { }
        interface IC { }

        class A : IA { }
        class B : A, IB { }
        class C : IA, IC { }
    }
}
