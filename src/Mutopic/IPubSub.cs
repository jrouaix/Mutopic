using System;
using System.Threading.Tasks;

namespace Mutopic
{
    public delegate void SubscriptionExceptionHandler(IPubSubSubscription subscription, object message, Exception exception);

    public interface IPubSub : IPublish
    {
        IPubSubSubscription Subscribe<T>(string topicName, Action<T> handler);

        event SubscriptionExceptionHandler OnSubscriptionException;
    }
}