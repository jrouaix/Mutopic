using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutopic
{
    /// <summary>
    /// Some extensions methods to an IPubSub instance.
    /// </summary>
    public static class IPubSubExtensions
    {
        /// <summary>
        /// Enable to subscribe an async handler to a topic. 
        /// Basically just a wrapper arount Task.Wait().
        /// </summary>
        /// <typeparam name="T">The type of T prevent some other typed messages to be published on this subscription</typeparam>
        /// <param name="pubSub">IPubSub instance</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="asyncHandler">Asynchronous action invoked on message published</param>
        /// <returns>Returns a subscription in order to be able to dispose/unsubscribe it.</returns>
        public static IPubSubSubscription Subscribe<T>(this IPubSub pubSub, string topicName, Func<T, Task> asyncHandler) => pubSub.Subscribe<T>(topicName, t => { asyncHandler(t).Wait(); });

        /// <summary>
        /// Subscribe a handler to a topic name with a filter predicate
        /// </summary>
        /// <typeparam name="T">The type of T prevent some other typed messages to be published on this subscription</typeparam>
        /// <param name="pubSub">IPubSub instance</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="where">Filter predicate</param>
        /// <param name="handler">Action invoked on message published</param>
        /// <returns>Returns a subscription in order to be able to dispose/unsubscribe it.</returns>
        public static IPubSubSubscription SubscribeWhere<T>(this IPubSub pubSub, string topicName, Func<T, bool> where, Action<T> handler)
            => pubSub.Subscribe(
                topicName,
                (T m) => { if (where(m)) handler(m); }
                );

        /// <summary>
        /// Subscribe an async handler to a topic name with a filter predicate
        /// </summary>
        /// <typeparam name="T">The type of T prevent some other typed messages to be published on this subscription</typeparam>
        /// <param name="pubsub">IPubSub instance</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="where">Filter predicate</param>
        /// <param name="handler">Action invoked on message published</param>
        /// <returns>Returns a subscription in order to be able to dispose/unsubscribe it.</returns>
        public static IPubSubSubscription SubscribeWhere<T>(this IPubSub pubsub, string topicName, Func<T, bool> where, Func<T, Task> handler)
            => pubsub.Subscribe(
                topicName,
                (T m) => (where(m)) ? handler(m) : Task.CompletedTask
                );
    }
}
