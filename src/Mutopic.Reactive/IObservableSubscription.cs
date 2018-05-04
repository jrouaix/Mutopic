using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic.Reactive
{
    public interface IObservableSubscription<out T> : IObservable<T>, IPubSubSubscription
    {
    }
}
