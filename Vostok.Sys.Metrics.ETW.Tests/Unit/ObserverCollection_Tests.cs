using System;
using NSubstitute;
using NUnit.Framework;
using Vostok.Sys.Metrics.ETW.Helpers;

namespace Vostok.Sys.Metrics.ETW.Tests.Unit
{
    [TestFixture]
    internal abstract class ObserverCollection_Tests
    {
        protected abstract IObserverCollection<int> CreateObserverCollection();

        [Test]
        public void Observers_should_receive_OnNext()
        {
            var collection = CreateObserverCollection();
            var (observer1, handle1) = SetupObserver(collection);
            var (observer2, handle2) = SetupObserver(collection);
            
            collection.OnNext(1);

            observer1.Received(1).OnNext(1);
            observer2.Received(1).OnNext(1);
        }

        [Test]
        public void Observers_should_receive_OnError()
        {
            var collection = CreateObserverCollection();
            var (observer1, handle1) = SetupObserver(collection);
            var (observer2, handle2) = SetupObserver(collection);
            var exception = new Exception();

            collection.OnError(exception);

            observer1.Received(1).OnError(exception);
            observer2.Received(1).OnError(exception);
        }

        [Test]
        public void Observers_should_receive_OnCompleted()
        {
            var collection = CreateObserverCollection();
            var (observer1, handle1) = SetupObserver(collection);
            var (observer2, handle2) = SetupObserver(collection);

            collection.OnCompleted();

            observer1.Received(1).OnCompleted();
            observer2.Received(1).OnCompleted();
        }

        [Test]
        public void Observer_should_not_get_notifications_after_handle_is_disposed()
        {
            var collection = CreateObserverCollection();
            var (observer, handle) = SetupObserver(collection);

            handle.Dispose();
            collection.OnNext(1);

            observer.DidNotReceive().OnNext(Arg.Any<int>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UnsubscribeAll_should_respect_sendOnCompleted_flag(bool sendOnCompleted)
        {
            var collection = CreateObserverCollection();
            var (observer, handle) = SetupObserver(collection);

            collection.UnsubscribeAll(sendOnCompleted);

            if (sendOnCompleted)
                observer.Received(1).OnCompleted();
            else
                observer.DidNotReceive().OnCompleted();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void After_UnsubscribeAll_observers_do_not_receive_notifications(bool sendOnCompleted)
        {
            var collection = CreateObserverCollection();
            var (observer, handle) = SetupObserver(collection);

            collection.UnsubscribeAll(sendOnCompleted);
            collection.OnNext(10);
            
            observer.DidNotReceive().OnNext(Arg.Any<int>());
        }

        private (IObserver<int>, IDisposable) SetupObserver(IObserverCollection<int> collection)
        {
            var observer = Substitute.For<IObserver<int>>();
            var handle = collection.Subscribe(observer);
            return (observer, handle);
        }
    }
}