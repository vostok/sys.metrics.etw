using System;
using Microsoft.Diagnostics.Tracing.Session;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal class ETWSessionManager : IETWSessionManager
    {
        public static readonly ETWSessionManager Default = new ETWSessionManager();
        public const string ETWSessionName = "KonturWinSysMetrics_v1";

        public ETWSession GetSession()
        {
            EnsureElevatedProcess();
            var session = new TraceEventSession(
                ETWSessionName,
                TraceEventSessionOptions.NoRestartOnCreate);
            // The session is reused and should remain after application exit
            session.StopOnDispose = false;
            return new ETWSession(session);
        }

        public void KillSession(Action<Exception> onError = null)
        {
            try
            {
                var handle = new TraceEventSession(ETWSessionName, TraceEventSessionOptions.Attach);
                handle.Dispose();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
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