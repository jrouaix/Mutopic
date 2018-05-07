using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    /// <summary>
    /// Allow publishing a message in some topics
    /// </summary>
    public interface IPublish
    {
        /// <summary>
        /// Publish the message in all topic names.
        /// </summary>
        /// <param name="message">Message published</param>
        /// <param name="topicNames">Named topics</param>
        void Publish(object message, params string[] topicNames);
    }
}
