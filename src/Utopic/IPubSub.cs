using System;
using System.Threading.Tasks;

namespace Utopic
{
    public delegate void SubscriptionExceptionHandler(IPubSubSubscription subscription, object message, Exception exception);

    public interface IPubSub
    {
        Func<Type, string> TypeToTopicnameStrategy { get; }

        Func<Type, string[]> TypeToInheritedTopicsStrategy { get; }

        /// <summary>
        /// Publish the message in all topic names and all sub classes/interfaces topics of the message.
        /// </summary>
        /// <param name="message">Message published</param>
        /// <param name="topicNames">Named topic names</param>
        void Publish(object message, params string[] topicNames);

        /// <summary>
        /// Publish the message in all topic names and all sub classes/interfaces topics of the message.
        /// </summary>
        /// <param name="topicNamesOnly">If true, the message will be published only in the topic names passed in parameters</param>
        /// <param name="message">Message published</param>
        /// <param name="topicNames">Named topic names</param>
        void Publish(object message, bool topicNamesOnly, params string[] topicNames);

        IPubSubSubscription Subscribe<T>(string topicName, Action<T> handler);

        event SubscriptionExceptionHandler OnSubscriptionException;
    }
}