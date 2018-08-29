﻿using Mutopic.Reactive;
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
    }
}
