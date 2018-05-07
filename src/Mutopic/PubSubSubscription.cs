using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    internal sealed class PubSubSubscription : IPubSubSubscription
    {
        readonly PubSub _pubSub;
        readonly string _topicName;

        public Action<object> Handler { get; }

        public PubSubSubscription(PubSub pubSub, string topicName, Action<object> handler)
        {
            _pubSub = pubSub;
            _topicName = topicName;
            Handler = handler;
        }

        public void Dispose() => _pubSub.Unsubscribe(_topicName, this);
    }
}
