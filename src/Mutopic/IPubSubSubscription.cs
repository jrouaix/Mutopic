using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic
{
    public interface IPubSubSubscription : IDisposable
    {
        Action<object> Handler { get; }
    }
}
