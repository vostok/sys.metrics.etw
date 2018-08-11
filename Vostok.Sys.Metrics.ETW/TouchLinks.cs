using System.Collections.Immutable;
using System.Reflection;

namespace Vostok.Sys.Metrics.ETW
{
    internal static class TouchLinks
    {
        static TouchLinks()
        {
            // @ezsilmar 
            // It's just an awful hack for TraceEvent lib
            // It has an indirect dependency on System.Collections.Immutable v. 1.2.2.0
            // which does not come with nuget package.
            // This breaks apps in runtime with missing dll error, because msbuild can't find dll during build
            // Reported this: https://github.com/Microsoft/perfview/issues/628
            ImmutableArray.Create(1);
            AssemblyFlags.ContentTypeMask.ToString();
        }
    }
}