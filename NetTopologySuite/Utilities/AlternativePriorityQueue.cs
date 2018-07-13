// Derived from BlueRaja's "High Speed Priority Queue for C#",
// which has the following license text:
/*
The MIT License (MIT)

Copyright (c) 2013 Daniel "BlueRaja" Pflughoeft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// An alternative implementation of the priority queue abstract data type.
    /// This allows us to do more than <see cref="PriorityQueue{T}"/>, which we
    /// got from JTS.  Ultimately, this queue enables scenarios that have more
    /// favorable execution speed characteristics at the cost of less favorable
    /// memory and usability characteristics.
    /// </summary>
    /// <typeparam name="TPriority">
    /// The type of the priority for each queue node.
    /// </typeparam>
    /// <typeparam name="TData">
    /// The type of data stored in the queue.
    /// </typeparam>
    /// <remarks>
    /// When enumerating over the queue, note that the elements will not be in
    /// sorted order.  To get at the elements in sorted order, use the copy
    /// constructor and repeatedly <see cref="Dequeue"/> elements from it.
    /// </remarks>
    public sealed class AlternativePriorityQueue<TPriority, TData> : IEnumerable<PriorityQueueNode<TPriority, TData>>
    {
        private const int DefaultCapacity = 4;

        private readonly List<PriorityQueueNode<TPriority, TData>> nodes;

        private readonly IComparer<TPriority> priorityComparer;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AlternativePriorityQueue{TPriority, TData}"/> class.
        /// </summary>
        public AlternativePriorityQueue()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AlternativePriorityQueue{TPriority, TData}"/> class.
        /// </summary>
        /// <param name="capacity">
        /// The initial queue capacity.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is less than 1.
        /// </exception>
        public AlternativePriorityQueue(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AlternativePriorityQueue{TPriority, TData}"/> class.
        /// </summary>
        /// <param name="priorityComparer">
        /// The <see cref="IComparer{T}"/> to use to compare priority values,
        /// or <see langword="null"/> to use the default comparer for the type.
        /// </param>
        public AlternativePriorityQueue(IComparer<TPriority> priorityComparer)
            : this(DefaultCapacity, priorityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AlternativePriorityQueue{TPriority, TData}"/> class.
        /// </summary>
        /// <param name="capacity">
        /// The initial queue capacity.
        /// </param>
        /// <param name="priorityComparer">
        /// The <see cref="IComparer{T}"/> to use to compare priority values,
        /// or <see langword="null"/> to use the default comparer for the type.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is less than 1.
        /// </exception>
        public AlternativePriorityQueue(int capacity, IComparer<TPriority> priorityComparer)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than zero.");
            }

            this.nodes = new List<PriorityQueueNode<TPriority, TData>>(capacity + 1);
            for (int i = 0; i <= capacity; i++)
            {
                this.nodes.Add(null);
            }

            this.Count = 0;
            this.priorityComparer = priorityComparer ?? Comparer<TPriority>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AlternativePriorityQueue{TPriority, TData}"/> class.
        /// </summary>
        /// <param name="copyFrom">
        /// The <see cref="AlternativePriorityQueue{TPriority, TData}"/> to
        /// copy from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="copyFrom"/> is <see langword="null"/>.
        /// </exception>
        public AlternativePriorityQueue(AlternativePriorityQueue<TPriority, TData> copyFrom)
        {
            if (copyFrom == null)
            {
                throw new ArgumentNullException("copyFrom");
            }

            this.nodes = new List<PriorityQueueNode<TPriority, TData>>(copyFrom.nodes.Count);
            this.priorityComparer = copyFrom.priorityComparer;

            // We need to copy the nodes, because they store queue state that
            // will change in one queue but not in the other.
            for (int i = 0; i < copyFrom.nodes.Count; i++)
            {
                var nodeToCopy = copyFrom.nodes[i];
                var copiedNode = nodeToCopy == null
                    ? null
                    : new PriorityQueueNode<TPriority, TData>(nodeToCopy);
                this.nodes.Add(copiedNode);
            }
        }

        /// <summary>
        /// Gets the number of nodes currently stored in this queue.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the node at the head of the queue.
        /// This is the node whose <typeparamref name="TPriority"/> compares
        /// less than or equal to the priority of all other nodes in the queue.
        /// </summary>
        public PriorityQueueNode<TPriority, TData> Head => this.nodes[1];

        /// <summary>
        /// Removes all nodes from this queue.
        /// </summary>
        public void Clear()
        {
            this.nodes.Clear();

            // There must always be a slot for the sentinel at the top, plus a
            // slot for the head (even if the head is null).
            this.nodes.Add(null);
            this.nodes.Add(null);

            this.Count = 0;
        }

        /// <summary>
        /// Determines whether the given node is contained within this queue.
        /// </summary>
        /// <param name="node">
        /// The node to locate in the queue.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="node"/> is found in the
        /// queue, otherwise <see langword="false"/>.
        /// </returns>
        public bool Contains(PriorityQueueNode<TPriority, TData> node)
        {
            return node != null &&
                   node.QueueIndex < this.nodes.Count &&
                   this.nodes[node.QueueIndex] == node;
        }

        /// <summary>
        /// Adds a given node to the queue with the given priority.
        /// </summary>
        /// <param name="node">
        /// The node to add to the queue.
        /// </param>
        /// <param name="priority">
        /// The priority for the node.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="node"/> is <see langword="null"/>.
        /// </exception>
        public void Enqueue(PriorityQueueNode<TPriority, TData> node, TPriority priority)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            node.Priority = priority;
            node.QueueIndex = ++this.Count;

            if (this.nodes.Count <= this.Count)
            {
                this.nodes.Add(null);
            }

            this.nodes[this.Count] = node;
            this.HeapifyUp(this.nodes[this.Count]);
        }

        /// <summary>
        /// Removes and returns the head of the queue.
        /// </summary>
        /// <returns>
        /// The removed element.
        /// </returns>
        public PriorityQueueNode<TPriority, TData> Dequeue()
        {
            var result = this.Head;
            this.Remove(result);
            return result;
        }

        /// <summary>
        /// Changes the priority of the given node.
        /// </summary>
        /// <param name="node">
        /// The node whose priority to change.
        /// </param>
        /// <param name="priority">
        /// The new priority for the node.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="node"/> is <see langword="null"/>.
        /// </exception>
        public void ChangePriority(PriorityQueueNode<TPriority, TData> node, TPriority priority)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            node.Priority = priority;
            this.OnNodeUpdated(node);
        }

        /// <summary>
        /// Removes the given node from this queue if it is present.
        /// </summary>
        /// <param name="node">
        /// The node to remove if present.
        /// </param>
        /// <returns>
        /// A value indicating whether the node was removed.
        /// </returns>
        public bool Remove(PriorityQueueNode<TPriority, TData> node)
        {
            if (!this.Contains(node))
            {
                return false;
            }

            if (this.Count <= 1)
            {
                this.nodes[1] = null;
                this.Count = 0;
                return true;
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

            return true;
        }

        /// <inheritdoc />
        public IEnumerator<PriorityQueueNode<TPriority, TData>> GetEnumerator()
        {
            return this.nodes
                       .Skip(1)
                       .Take(this.Count)
                       .GetEnumerator();
        }

        /// <inheritdoc />
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
            // The "higher-priority" item is actually the item whose priority
            // compares *lower*, because this is a min-heap.
            return this.priorityComparer.Compare(higher.Priority, lower.Priority) < 0;
        }
    }
}
