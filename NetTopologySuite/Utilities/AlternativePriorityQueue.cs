using System;
using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
    public sealed class AlternativePriorityQueue<TPriority, TData> : IEnumerable<PriorityQueueNode<TPriority, TData>>
    {
        private readonly PriorityQueueNode<TPriority, TData>[] nodes;

        private readonly IComparer<TPriority> priorityComparer;

        private int version;

        public AlternativePriorityQueue(int capacity)
            : this(capacity, null)
        {
        }

        public AlternativePriorityQueue(int capacity, IComparer<TPriority> priorityComparer)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than zero.");
            }

            this.nodes = new PriorityQueueNode<TPriority, TData>[capacity + 1];
            this.Count = 0;
            this.priorityComparer = priorityComparer ?? Comparer<TPriority>.Default;
        }

        public int Count { get; private set; }

        public int Capacity { get { return this.nodes.Length - 1; } }

        public PriorityQueueNode<TPriority, TData> Head { get { return this.nodes[1]; } }

#if DEBUG
        public bool IsValid
        {
            get
            {
                for (int i = 1; i < this.nodes.Length; i++)
                {
                    if (this.nodes[i] == null)
                    {
                        continue;
                    }

                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < this.nodes.Length &&
                        this.nodes[childLeftIndex] != null &&
                        this.HasHigherPriority(this.nodes[childLeftIndex], this.nodes[i]))
                    {
                        return false;
                    }

                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < this.nodes.Length &&
                        this.nodes[childRightIndex] != null &&
                        this.HasHigherPriority(this.nodes[childRightIndex], this.nodes[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
#endif

        public void Clear()
        {
            Array.Clear(this.nodes, 0, this.nodes.Length);
            this.Count = 0;
            ++this.version;
        }

        public bool Contains(PriorityQueueNode<TPriority, TData> node)
        {
            return this.nodes[node.QueueIndex] == node;
        }

        public void Enqueue(PriorityQueueNode<TPriority, TData> node, TPriority priority)
        {
            node.Priority = priority;
            node.QueueIndex = ++this.Count;

            this.nodes[node.QueueIndex] = node;
            this.HeapifyUp(this.nodes[this.Count]);
            ++this.version;
        }

        public PriorityQueueNode<TPriority, TData> Dequeue()
        {
            var result = this.Head;
            this.Remove(result);

            // Remove() updates our version for us.
            ////++this.version;
            return result;
        }

        public void UpdatePriority(PriorityQueueNode<TPriority, TData> node, TPriority priority)
        {
            node.Priority = priority;
            this.OnNodeUpdated(node);
            ++this.version;
        }

        public void Remove(PriorityQueueNode<TPriority, TData> node)
        {
            if (!this.Contains(node))
            {
                return;
            }

            if (this.Count <= 1)
            {
                this.nodes[1] = null;
                this.Count = 0;
                ++this.version;
                return;
            }

            bool wasSwapped = false;
            var formerLastNode = this.nodes[this.Count];
            if (node.QueueIndex != this.Count)
            {
                this.Swap(node, formerLastNode);
                wasSwapped = true;
            }

            --this.Count;
            this.nodes[node.QueueIndex] = null;

            if (wasSwapped)
            {
                this.OnNodeUpdated(formerLastNode);
            }

            ++this.version;
        }

        public IEnumerator<PriorityQueueNode<TPriority, TData>> GetEnumerator()
        {
            int originalVersion = this.version;
            for (int i = 0; i < this.Count; i++)
            {
                if (originalVersion != this.version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                yield return this.nodes[i + 1];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void HeapifyUp(PriorityQueueNode<TPriority, TData> node)
        {
            int parent = node.QueueIndex / 2;
            while (parent >= 1)
            {
                var parentNode = this.nodes[parent];
                if (this.HasHigherPriority(parentNode, node))
                {
                    break;
                }

                this.Swap(node, parentNode);

                parent = node.QueueIndex / 2;
            }
        }

        private void HeapifyDown(PriorityQueueNode<TPriority, TData> node)
        {
            int finalQueueIndex = node.QueueIndex;
            while (true)
            {
                var newParent = node;
                int childLeftIndex = 2 * finalQueueIndex;

                if (childLeftIndex > this.Count)
                {
                    node.QueueIndex = finalQueueIndex;
                    this.nodes[finalQueueIndex] = node;
                    break;
                }

                var childLeft = this.nodes[childLeftIndex];
                if (this.HasHigherPriority(childLeft, newParent))
                {
                    newParent = childLeft;
                }

                int childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= this.Count)
                {
                    var childRight = this.nodes[childRightIndex];
                    if (this.HasHigherPriority(childRight, newParent))
                    {
                        newParent = childRight;
                    }
                }

                if (newParent != node)
                {
                    this.nodes[finalQueueIndex] = newParent;

                    int temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                }
                else
                {
                    node.QueueIndex = finalQueueIndex;
                    this.nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }

        private void OnNodeUpdated(PriorityQueueNode<TPriority, TData> node)
        {
            int parentIndex = node.QueueIndex / 2;
            var parentNode = this.nodes[parentIndex];

            if (parentIndex > 0 && this.HasHigherPriority(node, parentNode))
            {
                this.HeapifyUp(node);
            }
            else
            {
                this.HeapifyDown(node);
            }
        }

        private void Swap(PriorityQueueNode<TPriority, TData> node1, PriorityQueueNode<TPriority, TData> node2)
        {
            this.nodes[node1.QueueIndex] = node2;
            this.nodes[node2.QueueIndex] = node1;

            int temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }

        private bool HasHigherPriority(PriorityQueueNode<TPriority, TData> higher, PriorityQueueNode<TPriority, TData> lower)
        {
            return this.priorityComparer.Compare(higher.Priority, lower.Priority) <= 0;
        }
    }
}
