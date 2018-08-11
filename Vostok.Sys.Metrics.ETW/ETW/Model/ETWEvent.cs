using System;
using Microsoft.Diagnostics.Tracing;

namespace Vostok.Sys.Metrics.ETW.ETW.Model
{
    internal class ETWEvent
    {
        public ETWEvent(TraceEvent traceEvent)
        {
            ProcessId = traceEvent.ProcessID;
            Timestamp = traceEvent.TimeStamp;
        }

        public ETWEvent()
        {
        }

        public int ProcessId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}