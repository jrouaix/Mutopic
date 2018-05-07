using Mutopic.Middleware;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Mutopic
{
    /// <summary>
    /// Middleware extensions
    /// </summary>
    public static class PubSubBuilderMessageInheritancePublishMiddlewareExtensions
    {
        /// <summary>
        /// Add a middleware that will publish every message in the full base classes / interfaces inherited topic names.
        /// </summary>
        /// <param name="builder">Instance of PubSubBuilder</param>
        /// <returns>Returns the same PubSubBuilder instance.</returns>
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
                topicsFromTypeInheritance = messageType.GetAllInheritedTypes(true).Select(t => t.GetTopicName()).ToArray();
                _topicsFromTypeInheritance.TryAdd(messageType, topicsFromTypeInheritance);
            }

            var allTopics = topicNames.Concat(topicsFromTypeInheritance).ToArray();
            return (shouldPublish, message, allTopics);
        }
    }
}
