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

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A container for a prioritized node that sites in an
    /// <see cref="AlternativePriorityQueue{TPriority, TData}"/>.
    /// </summary>
    /// <typeparam name="TPriority">
    /// The type to use for the priority of the node in the queue.
    /// </typeparam>
    /// <typeparam name="TData">
    /// The type to use for the data stored by the node in the queue.
    /// </typeparam>
    public sealed class PriorityQueueNode<TPriority, TData>
    {
        private readonly TData data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueueNode{TPriority, TData}"/> class.
        /// </summary>
        /// <param name="data">
        /// The <typeparamref name="TData"/> to store in this node.
        /// </param>
        public PriorityQueueNode(TData data)
        {
            this.data = data;
        }

        internal PriorityQueueNode(PriorityQueueNode<TPriority, TData> copyFrom)
        {
            this.data = copyFrom.data;
            this.Priority = copyFrom.Priority;
            this.QueueIndex = copyFrom.QueueIndex;
        }

        /// <summary>
        /// Gets the <typeparamref name="TData"/> that is stored in this node.
        /// </summary>
        public TData Data => this.data;

        /// <summary>
        /// Gets the <typeparamref name="TPriority"/> of this node in the queue.
        /// </summary>
        /// <remarks>
        /// The queue may update this priority while the node is still in the queue.
        /// </remarks>
        public TPriority Priority { get; internal set; }

        /// <summary>
        /// Gets or sets the index of this node in the queue.
        /// </summary>
        /// <remarks>
        /// This should only be read and written by the queue itself.
        /// It has no "real" meaning to anyone else.
        /// </remarks>
        internal int QueueIndex { get; set; }
    }
}
