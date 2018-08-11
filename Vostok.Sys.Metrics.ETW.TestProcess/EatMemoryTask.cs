using System;
using System.Collections.Generic;

namespace Vostok.Sys.Metrics.ETW.TestProcess
{
    internal class EatMemoryTask
    {
        private readonly long sizeBytes;
        private readonly long bucketSizeBytes = 2 * 1024;
        private List<byte[]> eaten;

        public EatMemoryTask(long sizeBytes)
        {
            this.sizeBytes = sizeBytes;
        }

        public void Start()
        {
            GC.Collect(2, GCCollectionMode.Forced, true, true);

            eaten = new List<byte[]>();
            var iterations = (int) (sizeBytes / bucketSizeBytes) + 1;
            for (var i = 0; i < iterations; i++)
            {
                eaten.Add(new byte[bucketSizeBytes]);
            }
        }

        public void Stop()
        {
            eaten = null;
        }
    }
}