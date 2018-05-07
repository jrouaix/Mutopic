using System;
using System.Threading.Tasks;

namespace Mutopic
{
    /// <summary>
    /// Call some exception handler.
    /// </summary>
    /// <param name="subscription">Responsible subscription for the exception</param>
    /// <param name="message">Message that was sent</param>
    /// <param name="exception">Exception raised</param>
    public delegate void SubscriptionExceptionHandler(IPubSubSubscription subscription, object message, Exception exception);

    /// <summary>
    /// Can subscribe a handler on a topic name
    /// </summary>
    public interface IPubSub : IPublish
    {
        /// <summary>
        /// Subscribe a handler on a topic name
        /// </summary>
        /// <typeparam name="T">The type of T prevent some other typed messages to be published on this subscription</typeparam>
        /// <param name="topicName">Topic name</param>
        /// <param name="handler">Action invoked on message published</param>
        /// <returns>Returns a subscription in order to be able to dispose/unsubscribe it.</returns>
        IPubSubSubscription Subscribe<T>(string topicName, Action<T> handler);

        /// <summary>
        /// This event will be called when a subscription handler raised an exception.
        /// </summary>
        event SubscriptionExceptionHandler OnSubscriptionException;
    }
}