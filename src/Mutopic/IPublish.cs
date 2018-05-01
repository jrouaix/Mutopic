using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    public interface IPublish
    {
        /// <summary>
        /// Publish the message in all topic names and all sub classes/interfaces topics of the message.
        /// </summary>
        /// <param name="message">Message published</param>
        /// <param name="topicNames">Named topic names</param>
        void Publish(object message, params string[] topicNames);
    }
}
