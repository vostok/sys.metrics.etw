using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Vostok.Sys.Metrics.ETW.ETW.Model;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal class ETWGCEventsSource : IGCEventsSource
    {
        private readonly Func<TraceEvent, bool> shouldProcess;
        private readonly IETWSessionManager manager;
        private ETWSession session;

        public ETWGCEventsSource(
            IETWSessionManager manager,
            Func<TraceEvent, bool> shouldProcess = null)
        {
            this.shouldProcess = shouldProcess;
            this.manager = manager;
        }

        private void SetupEvents()
        {
            var clr = session.GetSession().Source.Clr;
            clr.GCStart += OnGCStart;
            clr.GCStop += OnGCEnd;
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

        public void Dispose()
        {
            session?.Dispose();
        }

        public event Action<ETWEventClrGCStart> GCStart;
        public event Action<ETWEventClrGCEnd> GCStop;
        public void Start()
        {
            if (session != null)
            {
                return;
            }

            session = manager.GetSession();
            try
            {
                session.GetSession().EnableProvider(
                    ClrTraceEventParser.ProviderGuid,
                    TraceEventLevel.Verbose,
                    (ulong) ClrTraceEventParser.Keywords.GC);
            }
            catch
            {
                // this means that we attached to session, not created it.
                // TODO For some reason enabling providers on attached sessions is not allowed in c# wrapper
                // TODO Should ask at PerfView repo why is it implemented this way
            }

            // add filtering hook
            if (shouldProcess != null)
            {
                session.GetSession().Source.AddDispatchHook((ev, next) =>
                {
                    if (!shouldProcess(ev))
                    {
                        return;
                    }

                    next(ev);
                });
            }

            SetupEvents();
            session.StartProcessing();
        }
    }
}