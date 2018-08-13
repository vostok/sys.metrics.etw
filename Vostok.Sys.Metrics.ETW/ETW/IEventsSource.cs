using System;

namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal interface IEventsSource : IDisposable
    {
        void Start();
    }
}