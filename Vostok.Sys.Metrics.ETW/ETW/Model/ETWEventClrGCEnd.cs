using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Vostok.Sys.Metrics.ETW.ETW.Model
{
    internal class ETWEventClrGCEnd : ETWEventClrGC
    {
        public ETWEventClrGCEnd()
        {
        }

        public ETWEventClrGCEnd(GCEndTraceData gcEndTraceData) : base(gcEndTraceData)
        {
        }
    }
}