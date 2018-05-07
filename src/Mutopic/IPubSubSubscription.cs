using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    /// <summary>
    /// Summarize a subscription on a topic.
    /// You can unsubsribe this subscription by calling it's Dispose() method.
    /// </summary>
    public interface IPubSubSubscription : IDisposable
    {
        /// <summary>
        /// Reference to the subscription handler
        /// </summary>
        Action<object> Handler { get; }
    }
}
