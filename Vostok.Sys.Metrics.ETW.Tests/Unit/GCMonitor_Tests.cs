using System;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Sys.Metrics.ETW.ETW;
using Vostok.Sys.Metrics.ETW.ETW.Model;
using Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring;

namespace Vostok.Sys.Metrics.ETW.Tests.Unit
{
    [TestFixture]
    public class GCMonitor_Tests
    {
        [Test]
        public void Should_call_start_on_first_subscribe()
        {
            var (monitor, source, merger) = CreateMonitor();
            source.Received(0).Start();

            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);

            source.Received(1).Start();
        }

        [Test]
        public void Should_not_call_start_on_second_subscribe()
        {
            var (monitor, source, merger) = CreateMonitor();

            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);
            source.ClearReceivedCalls();

            var observer2 = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer2);

            source.DidNotReceive().Start();
        }

        [Test]
        public void Should_call_onError_if_source_start_failed()
        {
            var (monitor, source, merger) = CreateMonitor();
            var ex = new Exception();
            source.When(s => s.Start()).Throw(ex);

            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);

            TestHelpers.ShouldPassIn(() =>
            {
                observer.Received(1).OnError(Arg.Is(ex));
            }, 2.Seconds(), 20.Milliseconds());
        }

        [Test]
        public void Should_call_onError_on_second_observer_if_source_start_failed()
        {
            var (monitor, source, merger) = CreateMonitor();
            var ex = new Exception();
            source.When(s => s.Start()).Throw(ex);

            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);
            var observer2 = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer2);
            
            TestHelpers.ShouldPassIn(() =>
            {
                observer2.Received(1).OnError(Arg.Is(ex));
            }, 2.Seconds(), 20.Milliseconds());
        }

        [Test]
        public void Should_put_gc_start_events_to_merger()
        {
            var (monitor, source, merger) = CreateMonitor();
            
            source.GCStart += Raise.Event<Action<ETWEventClrGCStart>>(null as ETWEventClrGCStart);

            merger.Received(1).AddStart(Arg.Is(null as ETWEventClrGCStart));
        }

        [Test]
        public void Should_put_gc_stop_events_to_merger()
        {
            var (monitor, source, merger) = CreateMonitor();

            source.GCStop += Raise.Event<Action<ETWEventClrGCEnd>>(null as ETWEventClrGCEnd);

            merger.Received(1).CompleteOrNull(Arg.Is(null as ETWEventClrGCEnd));
        }

        [Test]
        public void Should_notify_observers_if_merger_returned_GCInfo_on_GCStop()
        {
            var (monitor, source, merger) = CreateMonitor();
            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);
            var gcInfo = new GCInfo();
            merger.CompleteOrNull(Arg.Any<ETWEventClrGCEnd>()).Returns(gcInfo);

            source.GCStop += Raise.Event<Action<ETWEventClrGCEnd>>(null as ETWEventClrGCEnd);

            observer.Received(1).OnNext(gcInfo);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_dispose_gcEventSource_with_respect_to_ctor_parameter(
            bool shouldDispose)
        {
            var (monitor, source, merger) = CreateMonitor(shouldDispose);

            monitor.Dispose();

            if (shouldDispose)
                source.Received(1).Dispose();
            else
                source.DidNotReceive().Dispose();
        }

        [Test]
        public void Should_not_notify_observers_if_merger_returned_null_on_gcStop()
        {
            var (monitor, source, merger) = CreateMonitor();
            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);
            merger.CompleteOrNull(Arg.Any<ETWEventClrGCEnd>()).Returns(null as GCInfo);

            source.GCStop += Raise.Event<Action<ETWEventClrGCEnd>>(null as ETWEventClrGCEnd);

            observer.DidNotReceive().OnNext(Arg.Any<GCInfo>());
        }

        [Test]
        public void Should_call_OnCompleted_on_observers_when_disposing()
        {
            var (monitor, source, merger) = CreateMonitor();
            var observer = Substitute.For<IObserver<GCInfo>>();
            monitor.Subscribe(observer);

            monitor.Dispose();

            observer.Received(1).OnCompleted();
        }

        private (GCMonitor monitor, IGCEventsSource source, IGCEventsMerger merger) CreateMonitor(
            bool disposeSource = true)
        {
            var source = Substitute.For<IGCEventsSource>();
            var merger = Substitute.For<IGCEventsMerger>();
            var monitor = new GCMonitor(source, merger, disposeSource);
            return (monitor, source, merger);
        }
    }
}