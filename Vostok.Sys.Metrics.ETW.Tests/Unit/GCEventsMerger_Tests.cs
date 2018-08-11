using FluentAssertions;
using NUnit.Framework;
using Vostok.Sys.Metrics.ETW.ETW.Model;
using Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring;

namespace Vostok.Sys.Metrics.ETW.Tests.Unit
{
    [TestFixture]
    public class GCEventsMerger_Tests
    {
        [Test]
        public void Merge_events_by_count_depth_and_pid()
        {
            var merger = CreateMerger();

            merger.AddStart(CreateStart(10, 1, 100));
            var result = merger.CompleteOrNull(CreateEnd(10, 1, 100));

            result.Should().NotBeNull();
            result.Depth.Should().Be(1);
            result.Count.Should().Be(10);
            result.ProcessId.Should().Be(100);
        }

        [Test]
        public void Returns_null_if_has_no_corresponding_start_event()
        {
            var merger = CreateMerger();

            var result = merger.CompleteOrNull(new ETWEventClrGCEnd());

            result.Should().BeNull();
        }

        [TestCase(0, 0, 0, 1, 0, 0)]
        [TestCase(0, 0, 0, 0, 1, 0)]
        [TestCase(0, 0, 0, 0, 0, 1)]
        [TestCase(1, 0, 0, 0, 0, 0)]
        [TestCase(0, 1, 0, 0, 0, 0)]
        [TestCase(0, 0, 1, 0, 0, 0)]
        public void Does_not_merge_events_with_different_pid_count_or_depth(
            int startCount, int startDepth, int startPid,
            int endCount, int endDepth, int endPid)
        {
            var merger = CreateMerger();
            var startEvent = CreateStart(startCount, startDepth, startPid);
            var endEvent = CreateEnd(endCount, endDepth, endPid);

            merger.AddStart(startEvent);
            var result = merger.CompleteOrNull(endEvent);
            
            result.Should().BeNull();
        }

        [Test]
        public void Can_merge_several_events()
        {
            var merger = CreateMerger();
            var start1 = CreateStart(1, 1, 1);
            var end1 = CreateEnd(1, 1, 1);
            var start2 = CreateStart(2, 2, 2);
            var end2 = CreateEnd(2, 2, 2);
            var start3 = CreateStart(3, 3, 3);
            var end3 = CreateEnd(3, 3, 3);

            merger.AddStart(start1);
            merger.AddStart(start2);
            var result1 = merger.CompleteOrNull(end1);
            merger.AddStart(start3);
            var result3 = merger.CompleteOrNull(end3);
            var result2 = merger.CompleteOrNull(end2);

            result1.ProcessId.Should().Be(1);
            result2.ProcessId.Should().Be(2);
            result3.ProcessId.Should().Be(3);
        }

        private ETWEventClrGCStart CreateStart(int count, int depth, int pid)
        {
            return new ETWEventClrGCStart {Count = count, Depth = depth, ProcessId = pid};
        }

        private ETWEventClrGCEnd CreateEnd(int count, int depth, int pid)
        {
            return new ETWEventClrGCEnd { Count = count, Depth = depth, ProcessId = pid };
        }

        private GCEventsMerger CreateMerger()
        {
            return new GCEventsMerger();
        }
    }
}