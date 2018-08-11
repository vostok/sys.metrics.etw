using System;
using Vostok.Sys.Metrics.ETW.ETW.Model;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal interface IGCEventsSource : IDisposable
    {
        event Action<ETWEventClrGCStart> GCStart;
        event Action<ETWEventClrGCEnd> GCStop;
        void Start();
    }
}