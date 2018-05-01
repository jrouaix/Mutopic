using System;
using System.Collections.Generic;
using System.Text;

namespace Utopic
{
    internal class PubSubSubscription : IPubSubSubscription
    {
        readonly PubSub _pubsub;
        readonly string _topicName;

        public Action<object> Handler { get; }

        public PubSubSubscription(PubSub pubsub, string topicName, Action<object> handler)
        {
            _pubsub = pubsub;
            _topicName = topicName;
            Handler = handler;
        }

        public void Dispose() => _pubsub.Unsubscribe(_topicName, this);
    }
}
