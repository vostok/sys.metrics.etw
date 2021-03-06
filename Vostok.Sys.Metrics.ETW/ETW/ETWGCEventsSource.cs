using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using Vostok.Sys.Metrics.ETW.ETW.Model;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal class ETWGCEventsSource : ETWEventsSource, IGCEventsSource
    {
        public ETWGCEventsSource(
            IETWSessionManager manager,
            Func<TraceEvent, bool> shouldProcess = null)
        : base(manager, shouldProcess)
        {
        }

        protected override void SetupEvents(ETWTraceEventSource traceEventSource)
        {
            var clr = traceEventSource.Clr;
            clr.GCStart += OnGCStart;
            clr.GCStop += OnGCEnd;
        }

        protected override void EnableProviders(TraceEventSession traceEventSession)
        {
            traceEventSession.EnableProvider(
                ClrTraceEventParser.ProviderGuid,
                TraceEventLevel.Verbose,
                (ulong) ClrTraceEventParser.Keywords.GC);
        }

        private void OnGCStart(GCStartTraceData obj)
        {
            try
            {
                GCStart?.Invoke(new ETWEventClrGCStart(obj));
            }
            catch { }
            // suppress errors in callbacks.
            // If exception happens here, all session.Process() will stop
        }

        private void OnGCEnd(GCEndTraceData obj)
        {
            try
            {
                GCStop?.Invoke(new ETWEventClrGCEnd(obj));
            }
            catch { }
            // suppress errors in callbacks.
            // If exception happens here, all session.Process() will stop
        }

        public event Action<ETWEventClrGCStart> GCStart;
        public event Action<ETWEventClrGCEnd> GCStop;
    }
}