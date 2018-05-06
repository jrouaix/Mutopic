using Mutopic.Middleware;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    public class PubSubBuilder
    {
        readonly Stack<IPublishMiddleware> _middlewares = new Stack<IPublishMiddleware>();

        public PubSubBuilder WithPublishMiddleware(IPublishMiddleware middleware)
        {
            _middlewares.Push(middleware);
            return this;
        }

        public PubSubBuilder WithPublishMiddleware(Func<(bool shouldPublish, object message, string[] topicNames), (bool shouldPublish, object message, string[] topicNames)> setupContext)
        {
            _middlewares.Push(new GenericPublishMiddleware(setupContext));
            return this;
        }

        public IPubSub Build()
        {
            return new PubSub(_middlewares.ToArray());
        }
    }
}
