namespace NetTopologySuite.Utilities
{
    public sealed class PriorityQueueNode<TPriority, TData>
    {
        private readonly TData data;

        public PriorityQueueNode(TData data)
        {
            this.data = data;
        }

        public TData Data { get { return this.data; } }

        // These should only be updated by the queue itself.
        public TPriority Priority { get; internal set; }
        public int QueueIndex { get; internal set; }
    }
}
