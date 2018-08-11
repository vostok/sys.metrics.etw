using System;

namespace Vostok.Sys.Metrics.ETW.Helpers
{
    internal class EventObserverCollection<T> : IObserverCollection<T>
    {
        private delegate void OnNextDelegate(T value);

        private delegate void OnErrorDelegate(Exception ex);

        private delegate void OnCompletedDelegate();

        private OnNextDelegate onNext;
        private OnErrorDelegate onError;
        private OnCompletedDelegate onCompleted;

        private readonly object syncObject = new object();

        public void OnNext(T value)
        {
            OnNextDelegate curOnNext;
            lock (syncObject)
            {
                curOnNext = onNext;
            }
            
            curOnNext?.Invoke(value);
        }

        public void OnError(Exception error)
        {
            OnErrorDelegate curOnError;
            lock (syncObject)
            {
                curOnError = onError;
            }

            curOnError?.Invoke(error);
        }

        public void OnCompleted()
        {
            OnCompletedDelegate curOnCompleted;
            lock (syncObject)
            {
                curOnCompleted = onCompleted;
            }

            curOnCompleted?.Invoke();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (syncObject)
            {
                onNext += observer.OnNext;
                onError += observer.OnError;
                onCompleted += observer.OnCompleted;
            }
            return new EventObserverCollectionHandle(this, observer);
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            lock (syncObject)
            {
                // ReSharper disable DelegateSubtraction
                onNext -= observer.OnNext;
                onError -= observer.OnError;
                onCompleted -= observer.OnCompleted;
                // ReSharper restore DelegateSubtraction
            }
        }

        public void UnsubscribeAll(bool sendOnCompleted)
        {
            var curCompleted = onCompleted;
            lock (syncObject)
            {
                onNext = null;
                onError = null;
                onCompleted = null;
            }

            if (sendOnCompleted)
            {
                curCompleted?.Invoke();
            }
        }

        private class EventObserverCollectionHandle : IDisposable
        {
            private readonly EventObserverCollection<T> collection;
            private readonly IObserver<T> observer;

            public EventObserverCollectionHandle(
                EventObserverCollection<T> collection,
                IObserver<T> observer)
            {
                this.collection = collection;
                this.observer = observer;
            }

            public void Dispose()
            {
                collection.Unsubscribe(observer);
            }
        }
    }
}