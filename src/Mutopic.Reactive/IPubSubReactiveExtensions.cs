using Mutopic.Reactive;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Mutopic
{
    public static class IPubSubReactiveExtensions
    {
        public static IObservableSubscription<T> SubscribeObservable<T>(this IPubSub pubSub, string topicName)
        {
            var subject = new Subject<T>();

            var subcription = pubSub.Subscribe<T>(topicName, message => subject.OnNext(message));

            var observable = new ObservableSubscription<T>(subcription, subject);

            return observable;
        }
    }
}
