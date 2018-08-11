using System;

namespace Vostok.Sys.Metrics.ETW.Helpers
{
    internal interface IObserverCollection<T> : IObserver<T>, IObservable<T>
    {
        void UnsubscribeAll(bool sendOnCompleted);
    }
}