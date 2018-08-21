using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Diagnostics.Tracing;
using Vostok.Sys.Metrics.ETW.ETW;
using Vostok.Sys.Metrics.ETW.ETW.Model;
using Vostok.Sys.Metrics.ETW.Helpers;

namespace Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring
{
    /// <summary>
    /// Provides IObservable interface for .net Garbage Collection events.
    /// <para>
    /// Actual start of ETW session occurs at first Subscribe.
    /// In case ETW session can't be created Subscribe returns and IObserver.OnError method is called on thread-pool thread.
    /// </para>
    /// </summary>
    public class GCMonitor : IObservable<GCInfo>, IDisposable
    {
        private readonly bool disposeSource;
        private readonly IGCEventsSource gcEventsSource;
        private readonly IGCEventsMerger gcEventsMerger;
        private readonly IObserverCollection<GCInfo> observers;

        private bool triedToStartSource;
        private Exception sourceStartError;
        private readonly object syncObject;

        internal GCMonitor(
            IGCEventsSource gcEventsSource,
            IGCEventsMerger gcEventsMerger,
            bool disposeSource)
        {
            this.gcEventsSource = gcEventsSource;
            this.gcEventsMerger = gcEventsMerger;
            this.disposeSource = disposeSource;
            observers = new EventObserverCollection<GCInfo>();

            gcEventsSource.GCStart += OnGcStart;
            gcEventsSource.GCStop += OnGcStop;

            triedToStartSource = false;
            sourceStartError = null;
            syncObject = new object();
        }

        private void OnGcStart(ETWEventClrGCStart obj)
        {
            gcEventsMerger.AddStart(obj);
        }

        private void OnGcStop(ETWEventClrGCEnd obj)
        {
            var result = gcEventsMerger.CompleteOrNull(obj);
            if (result != null)
            {
                observers.OnNext(result);
            }
        }

        public IDisposable Subscribe(IObserver<GCInfo> observer)
        {
            lock (syncObject)
            {
                if (!triedToStartSource)
                {
                    try
                    {
                        gcEventsSource.Start();
                    }
                    catch (Exception ex)
                    {
                        sourceStartError = ex;
                    }
                    triedToStartSource = true;
                }
            }

            if (sourceStartError != null)
            {
                ThreadPool.QueueUserWorkItem(
                    state => observer.OnError((Exception) state),
                    sourceStartError);
            }

            return observers.Subscribe(observer);
        }

        public void Dispose()
        {
            gcEventsSource.GCStart -= OnGcStart;
            gcEventsSource.GCStop -= OnGcStop;

            if (disposeSource)
            {
                gcEventsSource.Dispose();
            }
            observers.UnsubscribeAll(true);
        }

        public static GCMonitor StartForCurrentProcess()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return Create(process.Id);
            }
        }

        public static GCMonitor StartForProcess(int pid)
        {
            return Create(pid);
        }

        public static GCMonitor StartForAllProcesses()
        {
            return Create(null);
        }

        private static GCMonitor Create(int? pid = null, IETWSessionManager etwSessionManager = null)
        {
            Func<TraceEvent, bool> filter = null;
            if (pid != null)
            {
                filter = te => te.ProcessID == pid;
            }
            var source = new ETWGCEventsSource(
                etwSessionManager ?? ETWSessionManager.Default,
                filter);
            return new GCMonitor(source, new GCEventsMerger(), false);
        }
    }
}