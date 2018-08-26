using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Sys.Metrics.ETW.ETW;
using Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring;
using Vostok.Sys.Metrics.ETW.TestProcess;

namespace Vostok.Sys.Metrics.ETW.Tests.Integration
{
    [TestFixture]
    //[Explicit]
    public class GCMonitor_Tests
    {
        [SetUp]
        public void SetUp()
        {
            Console.WriteLine("SetUp. Killing a session...");
            ETWSessionManager.Default.KillSession(Console.WriteLine);
            Console.WriteLine("SetUp. Session killed.");
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("TearDown. Killing a session...");
            ETWSessionManager.Default.KillSession(Console.WriteLine);
            Console.WriteLine("TearDown. Session killed.");
        }

        [Test]
        public void When_monitor_which_created_etw_session_disposes_other_should_continue_to_get_events()
        {
            using (var testProcess = new TestProcessHandle())
            {
                using (var firstM = GCMonitor.StartForProcess(testProcess.Process.Id))
                {
                    var firstO = Substitute.For<IObserver<GCInfo>>();
                    firstM.Subscribe(firstO);

                    using (var secondM = GCMonitor.StartForProcess(testProcess.Process.Id))
                    {
                        var secondO = Substitute.For<IObserver<GCInfo>>();
                        secondM.Subscribe(secondO);

                        // application which created the first monitor exits
                        Console.WriteLine("Disposing first monitor");
                        firstM.Dispose();
                        Console.WriteLine("Disposed first monitor");
                        // let some time pass...
                        Thread.Sleep(1000);

                        testProcess.MakeGC(2);
                        try
                        {
                            TestHelpers.ShouldPassIn(() =>
                            {
                                secondO.Received(1).OnNext(Arg.Is<GCInfo>(i => IsInducedGc(i, 2)));
                            }, 5.Seconds(), 25.Milliseconds());
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Waiting on second monitor failed");
                            throw;
                        }
                    }
                }
            }
        }

        [Test]
        public void Another_monitor_can_safely_attach_and_deattach_to_existsing_session()
        {
            using (var testProcess = new TestProcessHandle())
            {
                using (var monitor = GCMonitor.StartForProcess(testProcess.Process.Id))
                {
                    var observer = Substitute.For<IObserver<GCInfo>>();
                    
                    observer.When(o => o.OnError(Arg.Any<Exception>())).Do(ci => Console.WriteLine(ci.Arg<Exception>()));
                    monitor.Subscribe(observer);

                    using (var monitor2 = GCMonitor.StartForProcess(testProcess.Process.Id))
                    {
                        var observer2 = Substitute.For<IObserver<GCInfo>>();
                        
                        observer2.When(o => o.OnError(Arg.Any<Exception>())).Do(ci => Console.WriteLine(ci.Arg<Exception>()));
                        monitor2.Subscribe(observer2);
                        
                        testProcess.MakeGC(2);
                        TestHelpers.ShouldPassIn(() =>
                        {
                            observer.Received(1).OnNext(Arg.Is<GCInfo>(i => IsInducedGc(i, 2)));
                        }, 5.Seconds(), 25.Milliseconds());
                    }

                    testProcess.MakeGC(2);
                    TestHelpers.ShouldPassIn(() =>
                    {
                        // the first monitor should continue to receive GCs
                        observer.Received(2).OnNext(Arg.Is<GCInfo>(i => IsInducedGc(i, 2)));
                    }, 5.Seconds(), 25.Milliseconds());
                }
            }
        }

        [Test]
        public void Can_monitor_itself()
        {
            using (var monitor = GCMonitor.StartForCurrentProcess())
            {
                var observer = Substitute.For<IObserver<GCInfo>>();
                
                observer.When(o => o.OnError(Arg.Any<Exception>())).Do(ci => Console.WriteLine(ci.Arg<Exception>()));
                monitor.Subscribe(observer);

                GC.Collect(2, GCCollectionMode.Forced, true);
                TestHelpers.ShouldPassIn(() =>
                {
                    observer.Received(1).OnNext(Arg.Is<GCInfo>(i => IsInducedGc(i, 2)));
                }, 5.Seconds(), 25.Milliseconds());
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(8)] // Max ETW sessions per producer
        [TestCase(64)] // Max ETW sessions per host
        //This cases sometimes fail
        //        [TestCase(256)] 
        //        [TestCase(512)]
        public void Can_create_many_gc_monitors(int count)
        {
            for (var y = 0; y < 100; ++y)
            using (var testProcess = new TestProcessHandle())
            {
                var monitors = new GCMonitor[count];
                var observers = new IObserver<GCInfo>[count];
                var received = 0;
                for (var i = 0; i < count; i++)
                {
                    observers[i] = Substitute.For<IObserver<GCInfo>>();
                    observers[i].When(o => o.OnNext(Arg.Any<GCInfo>())).Do(
                        ci =>
                        {
                            var gc = ci.Arg<GCInfo>();
                            Console.WriteLine(gc.Reason);
                            if (!IsInducedGc(gc, 2))
                                return;
                            Interlocked.Increment(ref received);
                        });
                    observers[i].When(o => o.OnError(Arg.Any<Exception>())).Do(ci => Console.WriteLine(ci.Arg<Exception>()));
                    monitors[i] = GCMonitor.StartForProcess(testProcess.Process.Id);
                    monitors[i].Subscribe(observers[i]);
                }

                testProcess.MakeGC(2);
                try
                {
                    var expected = observers.Length;
                    TestHelpers.ShouldPassIn(() => received.Should().Be(expected), 10.Seconds(), 100.Milliseconds());
                }
                finally
                {
                    Console.WriteLine($"Received: {received}.");
                }
            }
        }

        [Test]
        public void Catches_gc_events()
        {
            using (var testProcess = new TestProcessHandle())
            using (var monitor = GCMonitor.StartForProcess(testProcess.Process.Id))
            {
                var observer = Substitute.For<IObserver<GCInfo>>();
                observer.When(o => o.OnNext(Arg.Any<GCInfo>())).Do(ci => DumpGCInfo(ci.Arg<GCInfo>()));
                observer.When(o => o.OnError(Arg.Any<Exception>())).Do(ci => Console.WriteLine(ci.Arg<Exception>()));
                monitor.Subscribe(observer);
                var depth = 2;

                testProcess.MakeGC(depth);

                TestHelpers.ShouldPassIn(() =>
                {
                    observer
                        .Received(1)
                        .OnNext(Arg.Is<GCInfo>(i => IsInducedGc(i, depth)));
                }, 3.Seconds(), 25.Milliseconds());
            }
        }

        private bool IsInducedGc(GCInfo info, int depth)
        {
            return (info.Reason == GCReason.InducedCompacting
                    || info.Reason == GCReason.InducedLowMemory
                    || info.Reason == GCReason.InducedNotForced
                    || info.Reason == GCReason.Induced)
                   && info.Depth == depth;
        }

        private void DumpGCInfo(GCInfo arg)
        {
            Console.WriteLine(arg);
        }
    }
}
