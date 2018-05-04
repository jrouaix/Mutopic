using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace Mutopic.Reactive
{
    internal class ObservableSubscription<T> : IObservableSubscription<T>
    {
        private readonly IPubSubSubscription _subcription;
        private readonly Subject<T> _subject;

        public ObservableSubscription(IPubSubSubscription subcription, Subject<T> subject)
        {
            _subcription = subcription;
            _subject = subject;
        }
        public Action<object> Handler => _subcription.Handler;

        public void Dispose()
        {
            _subcription.Dispose();
            _subject.Dispose();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
