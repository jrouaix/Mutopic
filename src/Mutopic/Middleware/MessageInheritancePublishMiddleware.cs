using Mutopic.Middleware;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Mutopic
{
    public static class PubSubBuilderMessageInheritancePublishMiddlewareExtensions
    {
        public static PubSubBuilder WithMessageInheritancePublishing(this PubSubBuilder builder)
        {
            builder.WithPublishMiddleware(new MessageInheritancePublishMiddleware());
            return builder;
        }
    }
}

namespace Mutopic.Middleware
{
    internal class MessageInheritancePublishMiddleware : IPublishMiddleware
    {
        /// <summary>
        /// This is just a cache
        /// </summary>
        readonly ConcurrentDictionary<Type, string[]> _topicsFromTypeInheritance = new ConcurrentDictionary<Type, string[]>();

        public (bool shouldPublish, object message, string[] topicNames) SetupContext((bool shouldPublish, object message, string[] topicNames) context)
        {
            (bool shouldPublish, object message, string[] topicNames) = context;
            if (message == null) return context; // early return

            var messageType = message.GetType();
            if (!_topicsFromTypeInheritance.TryGetValue(messageType, out var topicsFromTypeInheritance))
            {
                topicsFromTypeInheritance = messageType.GetAllInheritedTypes(true).Select(t => t.Name).ToArray();
                _topicsFromTypeInheritance.TryAdd(messageType, topicsFromTypeInheritance);
            }

            var allTopics = topicNames.Concat(topicsFromTypeInheritance).ToArray();
            return (shouldPublish, message, allTopics);
        }
    }
}
