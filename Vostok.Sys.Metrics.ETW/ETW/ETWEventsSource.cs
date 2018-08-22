using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal abstract class ETWEventsSource : IEventsSource
    {
        private readonly IETWSessionManager manager;
        private ETWSession session;

        protected ETWEventsSource(
            IETWSessionManager manager)
        {
            this.manager = manager;
        }

        protected abstract void SetupEvents(ETWTraceEventSource traceEventSource);
        protected abstract void EnableProviders(TraceEventSession traceEventSession);

        public void Dispose()
        {
            session?.Dispose();
        }

        public void Start()
        {
            if (session != null)
            {
                return;
            }

            session = manager.GetSession();
            try
            {
                EnableProviders(session.GetSession());
            }
            catch
            {
                // this means that we attached to session, not created it.
                // TODO For some reason enabling providers on attached sessions is not allowed in c# wrapper
                // TODO Should ask at PerfView repo why is it implemented this way
            }

            SetupEvents(session.GetSession().Source);
            session.StartProcessing();
        }
    }
}