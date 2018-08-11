using System.Collections.Generic;
using Vostok.Sys.Metrics.ETW.ETW.Model;

namespace Vostok.Sys.Metrics.ETW.Meters.DotNet.GCMonitoring
{
    internal class GCEventsMerger : IGCEventsMerger
    {
        private readonly int maxUnmergedEventsCount;
        private readonly LinkedList<ETWEventClrGCStart> startEvents;

        // reuse removed node to generate less memory traffic
        private LinkedListNode<ETWEventClrGCStart> cachedNode;

        public GCEventsMerger(int maxUnmergedEventsCount = 2048)
        {
            this.maxUnmergedEventsCount = maxUnmergedEventsCount;
            startEvents = new LinkedList<ETWEventClrGCStart>();
            cachedNode = null;
        }

        public void AddStart(ETWEventClrGCStart start)
        {
            if (startEvents.Count >= maxUnmergedEventsCount)
            {
                Remove(startEvents.First);
            }

            Add(start);
        }

        public GCInfo CompleteOrNull(ETWEventClrGCEnd end)
        {
            var curNode = startEvents.Last;
            while (curNode != null)
            {
                if (Corresponds(curNode.Value, end))
                {
                    var merged = new GCInfo(curNode.Value, end);
                    Remove(curNode);
                    return merged;
                }
                curNode = curNode.Previous;
            }

            return null;
        }

        private void Add(ETWEventClrGCStart startEvent)
        {
            if (cachedNode == null)
            {
                startEvents.AddLast(startEvent);
            }
            else
            {
                cachedNode.Value = startEvent;
                startEvents.AddLast(cachedNode);
                cachedNode = null;
            }
        }

        private void Remove(LinkedListNode<ETWEventClrGCStart> node)
        {
            startEvents.Remove(node);
            node.Value = null;
            cachedNode = node;
        }

        private static bool Corresponds(ETWEventClrGCStart start, ETWEventClrGCEnd end)
        {
            return start.Count == end.Count && start.ProcessId == end.ProcessId && start.Depth == end.Depth;
        }
    }
}