using Mutopic.Middleware;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    /// <summary>
    /// Only way to build an IPubSub instance.
    /// Allow to stack publish middlewares used in the IPubSub to be created.
    /// Middlewares will be executed in the same order they are registered in the builder.
    /// </summary>
    public class PubSubBuilder
    {
        readonly List<IPublishMiddleware> _middlewares = new List<IPublishMiddleware>();

        /// <summary>
        /// Stack a publish middleware
        /// </summary>
        /// <param name="middleware">Publish middleware</param>
        /// <returns>returns the same instance</returns>
        public PubSubBuilder WithPublishMiddleware(IPublishMiddleware middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// Stack a publish middleware
        /// </summary>
        /// <param name="setupContext">Setup context method implementation</param>
        /// <returns>returns the same instance</returns>
        public PubSubBuilder WithPublishMiddleware(Func<(bool shouldPublish, object message, string[] topicNames), (bool shouldPublish, object message, string[] topicNames)> setupContext)
        {
            _middlewares.Add(new GenericPublishMiddleware(setupContext));
            return this;
        }

        /// <summary>
        /// Build the PubSub instance
        /// </summary>
        /// <returns>Returns an instance of IPubsub that will use all declared publish middlewares</returns>
        public IPubSub Build()
        {
            return new PubSub(_middlewares.ToArray());
        }
    }
}
