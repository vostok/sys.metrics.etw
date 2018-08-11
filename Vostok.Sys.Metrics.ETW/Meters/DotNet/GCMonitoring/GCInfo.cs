using System;
using Vostok.Sys.Metrics.ETW.ETW.Model;

namespace Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring
{
    public class GCInfo
    {
        public DateTime StartTimestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public int Depth { get; set; }
        public int Count { get; set; }
        public GCType Type { get; set; }
        public GCReason Reason { get; set; }
        public int ProcessId { get; set; }

        public GCInfo()
        {
        }

        internal GCInfo(ETWEventClrGCStart start, ETWEventClrGCEnd end)
        {
            StartTimestamp = start.Timestamp;
            Duration = end.Timestamp - start.Timestamp;
            Depth = start.Depth;
            Count = start.Count;
            // we use custom enums here because otherwise
            // users must reference TraceEvents directly
            Type = (GCType) start.Type;
            Reason = (GCReason) start.Reason;
            ProcessId = start.ProcessId;
        }

        public override string ToString()
        {
            return $"{nameof(Depth)}: {Depth}, {nameof(Duration)}: {Duration}, {nameof(Reason)}: {Reason}, {nameof(StartTimestamp)}: {StartTimestamp:O}, {nameof(Count)}: {Count}, {nameof(Type)}: {Type}, {nameof(ProcessId)}: {ProcessId}";
        }
    }
}