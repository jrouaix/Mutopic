using System;
using System.Collections.Generic;
using System.Text;

namespace Mutopic.Reactive
{
    /// <summary>
    /// IPubSubSubscription that is also a reactive observable !
    /// </summary>
    /// <typeparam name="T">The object that provides notification information.</typeparam>
    public interface IObservableSubscription<out T> : IObservable<T>, IPubSubSubscription
    {
    }
}
