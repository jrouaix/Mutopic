using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Utopic
{
    public class PubSub : IPubSub
    {
        readonly ConcurrentDictionary<string, List<Action<object>>> _subscribers = new ConcurrentDictionary<string, List<Action<object>>>();
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
            if (_subscribers.TryGetValue(topicName, out List<Action<object>> handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler(message);
                    }
                    catch (Exception)
                    {
                        // Disable any published message to handler
                        // to fuck the publisher
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

            lock (_syncLock)
            {
                if (_subscribers.TryGetValue(topicName, out var previousHandlers))
                {
                    var newHandlers = new List<Action<object>>(previousHandlers) { protectTypeHandler };
                    _subscribers.TryUpdate(topicName, newHandlers, previousHandlers);
                }
                else
                {
                    var newHandlers = new List<Action<object>>(new[] { protectTypeHandler });
                    _subscribers.TryAdd(topicName, newHandlers);
                }
            }
                
            return new PubSubSubscription(this, topicName, protectTypeHandler);
        }

        internal void Unsubscribe(string topicName, Action<object> handler)
        {
            lock (_syncLock)
            {
                if (!_subscribers.TryGetValue(topicName, out var previousHandlers)) return;

                var newHandlers = new List<Action<object>>(previousHandlers);
                newHandlers.Remove(handler);

                _subscribers.TryUpdate(topicName, newHandlers, previousHandlers);
            }
        }

        internal class PubSubSubscription : IPubSubSubscription
        {
            readonly PubSub _pubsub;
            readonly string _topicName;
            readonly Action<object> _handler;

            public PubSubSubscription(PubSub pubsub, string topicName, Action<object> handler)
            {
                _pubsub = pubsub;
                _topicName = topicName;
                _handler = handler;
            }

            public void Dispose() => _pubsub.Unsubscribe(_topicName, _handler);
        }

        #endregion
    }
}