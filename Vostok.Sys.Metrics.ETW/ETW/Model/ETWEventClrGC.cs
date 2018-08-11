using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Vostok.Sys.Metrics.ETW.ETW.Model
{
    internal abstract class ETWEventClrGC : ETWEvent
    {
        public int Count { get; set; }
        public int Depth { get; set; }

        protected ETWEventClrGC()
        {
        }

        protected ETWEventClrGC(GCStartTraceData gcStartData) : base(gcStartData)
        {
            Count = gcStartData.Count;
            Depth = gcStartData.Depth;
        }

        protected ETWEventClrGC(GCEndTraceData gcEndData) : base(gcEndData)
        {
            Count = gcEndData.Count;
            Depth = gcEndData.Depth;
        }
    }
}