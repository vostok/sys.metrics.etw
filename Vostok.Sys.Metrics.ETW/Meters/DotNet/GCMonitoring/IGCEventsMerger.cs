using Vostok.Sys.Metrics.ETW.ETW.Model;

namespace Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring
{
    internal interface IGCEventsMerger
    {
        void AddStart(ETWEventClrGCStart start);
        GCInfo CompleteOrNull(ETWEventClrGCEnd end);
    }
}