using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mutopic
{
    internal class PubSub : IPubSub
    {
        readonly ConcurrentDictionary<string, List<IPubSubSubscription>> _subscribers = new ConcurrentDictionary<string, List<IPubSubSubscription>>();
        readonly ConcurrentDictionary<Type, string[]> _topicsFromTypeInheritance = new ConcurrentDictionary<Type, string[]>();

        public Func<Type, string> TypeToTopicnameStrategy { get; }
        public Func<Type, string[]> TypeToInheritedTopicsStrategy { get; }

        public PubSub(Func<Type, string> typeToTopicnameStrategy = null, Func<Type, string[]> typeToInheritedTopicsStrategy = null)
        {
            TypeToTopicnameStrategy = typeToTopicnameStrategy ?? ((t) => t.Name);
            TypeToInheritedTopicsStrategy = typeToInheritedTopicsStrategy ?? (t => t.GetAllInheritedTypes(true).Select(x => TypeToTopicnameStrategy(x)).ToArray());
        }

        #region Publish

        public void Publish(object message, params string[] topicNames) => Publish(message, false, topicNames);

        public void Publish(object message, bool topicNamesOnly, params string[] topicNames)
        {
            if (message == null) return;

            if (!topicNamesOnly)
            {
                var messageType = message.GetType();
                if (!_topicsFromTypeInheritance.TryGetValue(messageType, out var topicsFromTypeInheritance))
                {
                    topicsFromTypeInheritance = TypeToInheritedTopicsStrategy(messageType);
                    _topicsFromTypeInheritance.TryAdd(messageType, topicsFromTypeInheritance);
                }

                foreach (var topic in topicsFromTypeInheritance)
                {
                    PublishInternal(topic, message);
                }
            }

            foreach (var topic in topicNames)
            {
                PublishInternal(topic, message);
            }

            // THIS WAS UNTESTED / UNUSED / CAN BE DONE EASY BY A PIPELINE ?
            //var mes = message as IMessage;
            //if (mes != null)
            //{
            //    await PublishInternal(mes.CorrelationId.ToString(), message).CAF();
            //}
        }


        void PublishInternal(string topicName, object message)
        {
            if (_subscribers.TryGetValue(topicName, out var subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        subscription.Handler(message);
                    }
                    catch (Exception ex)
                    {
                        RaiseOnSubscriptionException(subscription, message, ex);
                    }
                }
            }
        }

        #endregion

        #region Subscribe / Unsubscribe

        private readonly object _syncLock = new object();

        public IPubSubSubscription Subscribe<T>(string topicName, Action<T> handler)
        {
            Action<object> protectTypeHandler = o => { if (o is T t) handler(t); };

            var subscription = new PubSubSubscription(this, topicName, protectTypeHandler);

            lock (_syncLock)
            {
                if (_subscribers.TryGetValue(topicName, out var previousHandlers))
                {
                    var newSubscriptions = new List<IPubSubSubscription>(previousHandlers) { subscription };
                    _subscribers.TryUpdate(topicName, newSubscriptions, previousHandlers);
                }
                else
                {
                    var newSubscriptions = new List<IPubSubSubscription>(new[] { subscription });
                    _subscribers.TryAdd(topicName, newSubscriptions);
                }
            }

            return subscription;
        }

        internal void Unsubscribe(string topicName, IPubSubSubscription subscription)
        {
            lock (_syncLock)
            {
                if (!_subscribers.TryGetValue(topicName, out var previous)) return;

                var newSubscriptions = new List<IPubSubSubscription>(previous);
                newSubscriptions.Remove(subscription);

                _subscribers.TryUpdate(topicName, newSubscriptions, previous);
            }
        }



        #endregion

        #region Events
        private void RaiseOnSubscriptionException(IPubSubSubscription subscription, object message, Exception exception)
        {
            try
            {
                OnSubscriptionException?.Invoke(subscription, message, exception);
            }
            catch (Exception)
            {
                // lost for ever :'(
            }
        }

        public event SubscriptionExceptionHandler OnSubscriptionException;
        #endregion
    }
}