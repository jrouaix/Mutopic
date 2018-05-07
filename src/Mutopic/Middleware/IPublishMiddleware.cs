using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic.Middleware
{
    /// <summary>
    /// In order to the pubsub to be thread safe, all middlewares have to be thread safe.
    /// </summary>
    public interface IPublishMiddleware
    {
        /// <summary>
        /// Setup the publish context by changing it and returning it.
        /// </summary>
        /// <param name="context">Publishing context.</param>
        /// <returns>an other instance of the context Tuple, changed if needed.</returns>
        (bool shouldPublish, object message, string[] topicNames) SetupContext((bool shouldPublish, object message, string[] topicNames) context);
    }
}
