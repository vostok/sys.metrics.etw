using NUnit.Framework;
using Vostok.Sys.Metrics.ETW.Helpers;

namespace Vostok.Sys.Metrics.ETW.Tests.Unit
{
    [TestFixture]
    internal class EventObserverCollection_Tests : ObserverCollection_Tests
    {
        protected override IObserverCollection<int> CreateObserverCollection()
        {
            return new EventObserverCollection<int>();
        }
    }
}