using System;

namespace Vostok.Sys.Metrics.ETW.TestProcess
{
    internal class EatPrivateMemoryTask
    {
        private readonly long sizeBytes;
        private byte[] eaten;

        public EatPrivateMemoryTask(long sizeBytes)
        {
            this.sizeBytes = sizeBytes;
        }

        public void Start()
        {
            GC.Collect(2, GCCollectionMode.Forced, true, true);

            eaten = new byte[sizeBytes];
        }

        public void Stop()
        {
            eaten = null;
        }
    }
}