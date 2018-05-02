using Mutopic.Middleware;
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
        private readonly IPublishMiddleware[] _publishMiddlewares;

        public PubSub(params IPublishMiddleware[] publishMiddlewares)
        {
            _publishMiddlewares = publishMiddlewares;
        }

        #region Publish

        public void Publish(object message, params string[] topicNames)
        {
            if (message == null) return;

            var context = (shouldPublish: true, message, topicNames);
            foreach (var middleware in _publishMiddlewares)
                context = middleware.SetupContext(context);

            if (!context.shouldPublish) return;

            foreach (var topic in context.topicNames)
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
            void protectTypeHandler(object o) { if (o is T t) handler(t); }

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