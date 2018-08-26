using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Session;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal class ETWSession : IDisposable
    {
        private readonly TraceEventSession session;
        private readonly CancellationTokenSource backgroundProcessesCts;
        private readonly object syncObject;
        private ETWSessionState state;
        private Task processingTask;

        public ETWSession(TraceEventSession session)
        {
            this.session = session;
            this.backgroundProcessesCts = new CancellationTokenSource();
            this.syncObject = new object();
            state = ETWSessionState.NotStarted;
            processingTask = null;
        }

        public TraceEventSession GetSession()
        {
            return session;
        }

        public void Dispose()
        {
            lock (syncObject)
            {
                if (state == ETWSessionState.Started)
                {
                    backgroundProcessesCts.Cancel();
                }
                session.Dispose();
                try
                {
                    processingTask.Wait(1000);
                }
                catch { }
                state = ETWSessionState.Disposed;
            }
        }

        public void StartProcessing()
        {
            lock (syncObject)
            {
                if (state == ETWSessionState.NotStarted)
                {
#pragma warning disable 4014
                    StartTimeSynchronizer(backgroundProcessesCts.Token);
#pragma warning restore 4014
                    processingTask = Task.Factory.StartNew(StartProcessingInternal, TaskCreationOptions.LongRunning);
                    state = ETWSessionState.Started;
                }
            }
        }

        private void StartProcessingInternal()
        {
            try
            {
                session.Source.Process();
            }
            catch (COMException e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.ErrorCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task StartTimeSynchronizer(CancellationToken token)
        {
            var syncPeriod = TimeSpan.FromHours(1);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    session.Source.SynchronizeClock();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(syncPeriod, token).ConfigureAwait(false);
            }
        }

        private enum ETWSessionState
        {
            NotStarted = 0,
            Started = 1,
            Disposed = 2
        }
    }
}