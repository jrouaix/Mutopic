using Mutopic.Reactive;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Mutopic
{
    /// <summary>
    /// Adds some helpful extensions methods to enjoy subscribing with reactive extensions
    /// </summary>
    public static class IPubSubReactiveExtensions
    {
        /// <summary>
        /// Returns an observable subscription.
        /// </summary>
        /// <typeparam name="T">The type of T prevent some other typed messages to be published on this subscription</typeparam>
        /// <param name="pubSub">IPubSub instance</param>
        /// <param name="topicName">Topic name</param>
        /// <returns>An observable subscription</returns>
        public static IObservableSubscription<T> SubscribeObservable<T>(this IPubSub pubSub, string topicName)
        {
            var subject = new Subject<T>();

            var subcription = pubSub.Subscribe<T>(topicName, message => subject.OnNext(message));

            var observable = new ObservableSubscription<T>(subcription, subject);

            return observable;
        }
    }
}
