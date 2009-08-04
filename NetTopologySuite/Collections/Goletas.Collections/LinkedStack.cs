//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace Goletas.Collections
{
    /// <summary>
    /// Represents a variable size last-in-first-out (LIFO) collection
    /// of objects of the same arbitrary type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="LinkedStack&lt;T&gt;"/> is implemented as a singly linked list.
    /// </para>
    /// <para>
    /// <see cref="LinkedStack&lt;T&gt;"/> provides guaranteed O(1) time cost
    /// for the <see cref="Push"/> and <see cref="Pop"/> operations.
    /// </para>
    /// <para>
    /// <see cref="LinkedStack&lt;T&gt;"/> accepts <c>null</c> as a valid value
    /// for reference types and allows duplicate elements.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The element type of the <see cref="LinkedStack&lt;T&gt;"/>.
    /// </typeparam>
    public sealed class LinkedStack<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of elements contained in the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        private int _Count;

        /// <summary>
        /// The top node of the <see cref="LinkedStack&lt;T&gt;"/>.
        /// If this field is <c>null</c>, then the stack is empty.
        /// </summary>
        private Node _Top;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedStack&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is an O(1) operation.
        /// </remarks>
        public LinkedStack()
        {
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        public int Count
        {
            get { return _Count; }
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/>
        /// for the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/>
        /// for the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds an object on top of the <see cref="LinkedStack&lt;T&gt;"/>. 
        /// </summary>
        /// <param name="item">
        /// The object to push onto the <see cref="LinkedStack&lt;T&gt;"/>.
        /// The <paramref name="item"/> can be <c>null</c> for reference types.
        /// </param>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        public void Push(T item)
        {
            _Top = new Node(item, _Top);
            _Count++;
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="LinkedStack&lt;T&gt;"/>.
        /// The value can be <c>null</c> for reference types.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="item"/> is found in the
        /// <see cref="LinkedStack&lt;T&gt;"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method determines equality by calling <see cref="Object.Equals(object)"/>.
        /// To enhance performance, it is recommended that in addition to implementing
        /// <see cref="Object.Equals(object)"/>, any class/struct also implement
        /// <see cref="IEquatable&lt;T&gt;"/> interface for their own type.
        /// </para>
        /// <para>
        /// This method is an O(n) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        public bool Contains(T item)
        {
            Node p = _Top;

            if (item != null)
            {
                while (p != null)
                {
                    if (item.Equals(p.Item))
                    {
                        return true;
                    }

                    p = p.Next;
                }
            }
            else
            {
                while (p != null)
                {
                    if (p.Item == null)
                    {
                        return true;
                    }

                    p = p.Next;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the object at the top of the <see cref="LinkedStack&lt;T&gt;"/>
        /// without removing it.
        /// </summary>
        /// <returns>
        /// The object at the top of the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is similar to the <see cref="Pop()"/> method, but <see cref="Peek()"/>
        /// does not modify the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </para>
        /// <para>
        /// This method is an O(1) operation.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="LinkedStack&lt;T&gt;"/> is empty.
        /// </exception>
        public T Peek()
        {
            if (_Top == null)
            {
                throw new InvalidOperationException();
            }

            return _Top.Item;
        }

        /// <summary>
        /// Removes and returns the object at the top of the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// The object removed from the top of the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is similar to the <see cref="Peek()"/> method, but <see cref="Peek()"/>
        /// does not modify the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </para>
        /// <para>
        /// This method is an O(1) operation.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="LinkedStack&lt;T&gt;"/> is empty.
        /// </exception>
        public T Pop()
        {
            if (_Top == null)
            {
                throw new InvalidOperationException();
            }

            T item = _Top.Item;

            _Top = _Top.Next;
            _Count--;

            return item;
        }

        /// <summary>
        /// Removes all objects from the <see cref="LinkedStack&lt;T&gt;"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Count"/> is set to zero and references to other objects
        /// from elements of the collection are also released.
        /// </para>
        /// <para>
        /// This method is an O(1) operation.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            _Top = null;
            _Count = 0;
        }

        /// <summary>
        /// Copies the <see cref="LinkedStack&lt;T&gt;"/> elements to an existing
        /// one-dimensional <see cref="Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements
        /// copied from this <see cref="LinkedStack&lt;T&gt;"/>. The <paramref name="array"/>
        /// must have zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in the <paramref name="array"/> at which copying begins.
        /// </param>
        /// <remarks>
        /// <para>
        /// The elements are copied to the <paramref name="array"/> in last-in-first-out (LIFO)
        /// order, similar to the order of the elements returned by a succession of calls to
        /// the <see cref="Pop()"/> method.
        /// </para>
        /// <para>
        /// This method is an O(n) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside of the <paramref name="array"/> bounds.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="LinkedStack&lt;T&gt;"/>
        /// is greater than the available space from the <paramref name="index"/> to the end
        /// of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((array.Length - index) < _Count)
            {
                throw new ArgumentException();
            }

            Node p = _Top;

            while (p != null)
            {
                array[index] = p.Item;

                p = p.Next;
                index++;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Enumerator"/>
        /// for the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_Top);
        }

        #region Nested type: Enumerator

        /// <summary>
        /// Enumerates the elements of the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The elements are enumerated in last-in-first-out (LIFO) order, similar to the
        /// order of the elements returned by a succession of calls to the <see cref="Pop()"/>
        /// method.
        /// </para>
        /// <para>
        /// Initially, the enumerator is positioned before the first element in the collection.
        /// At this position, <see cref="Current"/> is undefined. Therefore, <see cref="MoveNext"/>
        /// must be called to advance the enumerator to the first element of the collection before
        /// reading the value of <see cref="Current"/>.
        /// </para>
        /// <para>
        /// <see cref="Current"/> returns the same object until <see cref="MoveNext"/> is
        /// called. <see cref="MoveNext"/> sets <see cref="Current"/> to the next element.
        /// </para>
        /// <para>
        /// If <see cref="MoveNext"/> passes the end of the collection, the enumerator is
        /// positioned after the last element in the collection and <see cref="MoveNext"/>
        /// returns <c>false</c>. When the enumerator is at this position, subsequent calls to
        /// <see cref="MoveNext"/> also return <c>false</c>. If the last call to
        /// <see cref="MoveNext"/> returned <c>false</c>, <see cref="Current"/> is undefined.
        /// <see cref="Current"/> cannot be set to the first element of the collection again.
        /// A new enumerator instance must be created instead.
        /// </para>
        /// <para>
        /// An enumerator remains valid as long as the collection remains unchanged. If changes
        /// are made to the collection, such as adding, modifying, or deleting elements, the
        /// enumerator behavior is undefined.
        /// </para>
        /// <para>
        /// The enumerator does not have exclusive access to the collection; therefore,
        /// enumerating through a collection is intrinsically not a thread-safe procedure.
        /// To guarantee thread safety during enumeration, the collection can be locked
        /// during the entire enumeration. To allow the collection to be accessed by
        /// multiple threads for reading and writing, a custom synchronization must be
        /// implemented.
        /// </para>
        /// <para>
        /// The <see cref="Enumerator"/> is not designed to provide
        /// any fast-fail safety mechanisms against concurrent modifications.
        /// </para>
        /// </remarks>
        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The element at the current position of the enumerator.
            /// </summary>
            private T _Current;

            /// <summary>
            /// The <see cref="Node"/> at the current position of the enumerator.
            /// </summary>
            private Node _Next;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/>
            /// structure with the specified <paramref name="node"/>.
            /// </summary>
            /// <param name="node">
            /// The node from which to start enumerating the
            /// <see cref="LinkedStack&lt;T&gt;"/> elements.
            /// </param>
            internal Enumerator(Node node)
            {
                _Next = node;
                _Current = default(T);
            }

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the element in the <see cref="LinkedStack&lt;T&gt;"/> at
            /// the current position of the enumerator. 
            /// </summary>
            /// <value>
            /// The element in the <see cref="LinkedStack&lt;T&gt;"/>
            /// at the current position of the enumerator.
            /// </value>
            /// <remarks>
            /// <para>
            /// <see cref="Current"/> is undefined under any of the following conditions:
            /// 1) The enumerator is positioned before the first element in the collection,
            /// immediately after the enumerator is created. <see cref="MoveNext"/> must be
            /// called to advance the enumerator to the first element of the collection
            /// before reading the value of <see cref="Current"/>; 2) The last call to
            /// <see cref="MoveNext"/> returned <c>false</c>, which indicates the end of
            /// the collection; 3) The collection was modified after the enumerator was created.
            /// </para>
            /// <para>
            /// <see cref="Current"/> returns the same object until <see cref="MoveNext"/>
            /// is called. <see cref="MoveNext"/> sets <see cref="Current"/> to the next element.
            /// </para>
            /// </remarks>
            public T Current
            {
                get { return _Current; }
            }

            /// <summary>
            /// Gets the element in the <see cref="LinkedStack&lt;T&gt;"/> at
            /// the current position of the enumerator. 
            /// </summary>
            /// <value>
            /// The element in the <see cref="LinkedStack&lt;T&gt;"/>
            /// at the current position of the enumerator.
            /// </value>
            /// <remarks>
            /// <para>
            /// <see cref="Current"/> is undefined under any of the following conditions:
            /// 1) The enumerator is positioned before the first element in the collection,
            /// immediately after the enumerator is created. <see cref="MoveNext"/> must be
            /// called to advance the enumerator to the first element of the collection
            /// before reading the value of <see cref="Current"/>; 2) The last call to
            /// <see cref="MoveNext"/> returned <c>false</c>, which indicates the end of
            /// the collection; 3) The collection was modified after the enumerator was created.
            /// </para>
            /// <para>
            /// <see cref="Current"/> returns the same object until <see cref="MoveNext"/>
            /// is called. <see cref="MoveNext"/> sets <see cref="Current"/> to the next element.
            /// </para>
            /// </remarks>
            object IEnumerator.Current
            {
                get { return _Current; }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="LinkedStack&lt;T&gt;"/>.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element;
            /// <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <remarks>
            /// <para>
            /// After an enumerator is created, the enumerator is positioned before the first element
            /// in the collection, and the first call to <see cref="MoveNext"/> advances the
            /// enumerator to the first element of the collection.
            /// </para>
            /// <para>
            /// If <see cref="MoveNext"/> passes the end of the collection, the enumerator is positioned
            /// after the last element in the collection and <see cref="MoveNext"/> returns <c>false</c>.
            /// When the enumerator is at this position, subsequent calls to <see cref="MoveNext"/>
            /// also return <c>false</c>.
            /// </para> 
            /// </remarks>
            public bool MoveNext()
            {
                if (_Next == null)
                {
                    return false;
                }

                _Current = _Next.Item;
                _Next = _Next.Next;

                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position,
            /// which is before the first element in the <see cref="LinkedStack&lt;T&gt;"/>.
            /// This method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <exception cref="NotSupportedException">
            /// Always thrown since this operation is not supported.
            /// </exception>
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Releases all resources allocated by the <see cref="Enumerator"/>.
            /// </summary>
            void IDisposable.Dispose()
            {
            }

            #endregion
        }

        #endregion

        #region Nested type: Node

        /// <summary>
        /// Represents a node in the <see cref="LinkedStack&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Node"/> contains a value and a reference to the next node.
        /// </remarks>
        internal sealed class Node
        {
            /// <summary>
            /// The object contained in this node.
            /// </summary>
            public T Item;

            /// <summary>
            /// The reference to the next node in the <see cref="LinkedStack&lt;T&gt;"/> or <c>null</c>
            /// if this <see cref="Node"/> is the last node in the <see cref="LinkedStack&lt;T&gt;"/>.
            /// </summary>
            public Node Next;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class containing the specified
            /// object and the reference to the next node in the <see cref="LinkedStack&lt;T&gt;"/>.
            /// </summary>
            /// <param name="item">
            /// The object to contain in the <see cref="Node"/>.
            /// </param>
            /// <param name="next">
            /// The reference to the next node in the <see cref="LinkedStack&lt;T&gt;"/>.
            /// </param>
            public Node(T item, Node next)
            {
                Item = item;
                Next = next;
            }
        }

        #endregion
    }
}