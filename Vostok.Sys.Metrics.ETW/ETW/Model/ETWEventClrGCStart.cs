using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Vostok.Sys.Metrics.ETW.ETW.Model
{
    internal class ETWEventClrGCStart : ETWEventClrGC
    {
        public GCReason Reason { get; set; }
        public GCType Type { get; set; }

        public ETWEventClrGCStart()
        {
        }

        public ETWEventClrGCStart(GCStartTraceData gcStartTraceData) : base(gcStartTraceData)
        {
            Reason = gcStartTraceData.Reason;
            Type = gcStartTraceData.Type;
        }
    }
}