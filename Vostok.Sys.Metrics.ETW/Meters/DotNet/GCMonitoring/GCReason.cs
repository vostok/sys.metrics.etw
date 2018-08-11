namespace Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring
{
    public enum GCReason
    {
        AllocSmall,
        Induced,
        LowMemory,
        Empty,
        AllocLarge,
        OutOfSpaceSOH,
        OutOfSpaceLOH,
        InducedNotForced,
        Internal,
        InducedLowMemory,
        InducedCompacting,
        LowMemoryHost,
        PMFullGC,
    }
}