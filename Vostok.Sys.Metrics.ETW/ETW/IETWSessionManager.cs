namespace Vostok.Sys.Metrics.ETW.ETW
{
    internal interface IETWSessionManager
    {
        /// <summary>
        /// Gets a real-time TraceEventSession. 
        /// There is one system ETW session for WinSysMetrics.ETW lib
        /// </summary>
        /// <returns>A wrapper around TraceEventSession with some helpful methods</returns>
        ETWSession GetSession();
    }
}