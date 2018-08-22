using System;
using System.Threading;
using Microsoft.Diagnostics.Tracing.Session;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal class ETWSessionManager : IETWSessionManager
    {
        public static readonly ETWSessionManager Default = new ETWSessionManager();
        public const string ETWSessionName = "KonturWinSysMetrics_v1";

        private Lazy<ETWSession> cachedSession = new Lazy<ETWSession>(() => ObtainSession(), LazyThreadSafetyMode.PublicationOnly);

        public ETWSession GetSession() => cachedSession.Value;

        public void KillSession(Action<Exception> onError = null)
        {
            try
            {
                cachedSession = new Lazy<ETWSession>(() => ObtainSession(), LazyThreadSafetyMode.PublicationOnly);
                var handle = new TraceEventSession(ETWSessionName, TraceEventSessionOptions.Attach);
                handle.Dispose();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        private static ETWSession ObtainSession()
        {
            EnsureElevatedProcess();
            Console.WriteLine("Obtain");
            var session = new TraceEventSession(
                ETWSessionName,
                TraceEventSessionOptions.NoRestartOnCreate) {StopOnDispose = false};
            // The session is reused and should remain after application exit
            return new ETWSession(session);
        }

        private static void EnsureElevatedProcess()
        {
            var isElevated = TraceEventSession.IsElevated() ?? false;
            if (!isElevated)
            {
                throw new UnauthorizedAccessException("You should run as Admin to create ETW session");
            }
        }
    }
}