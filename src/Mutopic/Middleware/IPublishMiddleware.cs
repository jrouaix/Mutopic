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
        (bool shouldPublish, object message, string[] topicNames) SetupContext((bool shouldPublish, object message, string[] topicNames) context);
    }
}
