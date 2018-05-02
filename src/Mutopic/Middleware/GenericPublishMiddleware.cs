using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic.Middleware
{
    internal class GenericPublishMiddleware : IPublishMiddleware
    {
        private readonly Func<(bool shouldPublish, object message, string[] topicNames), (bool shouldPublish, object message, string[] topicNames)> _setupContext;

        public GenericPublishMiddleware(
            Func<(bool shouldPublish, object message, string[] topicNames), (bool shouldPublish, object message, string[] topicNames)> setupContext
            )
        {
            _setupContext = setupContext;
        }

        public (bool shouldPublish, object message, string[] topicNames) SetupContext((bool shouldPublish, object message, string[] topicNames) context)
        {
            return _setupContext(context);
        }
    }
}
