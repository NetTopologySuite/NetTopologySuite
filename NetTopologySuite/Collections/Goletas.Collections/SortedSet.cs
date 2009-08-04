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
    /// Represents a set backed up by a balanced binary tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binary search tree is kept balanced using the AVL algorithm invented
    /// by G.M. Adelson-Velsky and E.M. Landis.
    /// </para>
    /// <para>
    /// <see cref="SortedSet&lt;T&gt;"/> provides guaranteed O(log2 n) time cost for
    /// the <see cref="Add"/>, <see cref="Contains"/> and <see cref="Remove"/> operations.
    /// </para>
    /// <para>
    /// <see cref="SortedSet&lt;T&gt;"/> elements must be immutable for the
    /// <see cref="IComparable&lt;T&gt;"/> interface as long as they are used in the
    /// <see cref="SortedSet&lt;T&gt;"/>. Every element in the <see cref="SortedSet&lt;T&gt;"/>
    /// must be unique. An element cannot be a <c>null</c> reference.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The element type of the <see cref="SortedSet&lt;T&gt;"/>.
    /// </typeparam>
    public sealed class SortedSet<T> : ICollection<T> where T : IComparable<T>
    {
        /// <summary>
        /// The number of elements contained in this <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        private int _Count;

        /// <summary>
        /// The root node of this <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        private Node _Root;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedSet&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is an O(1) operation.
        /// </remarks>
        public SortedSet()
        {
        }

        #region ICollection<T> Members

        /// <summary>
        /// Gets the number of elements contained in this <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in this <see cref="SortedSet&lt;T&gt;"/>.
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        public int Count
        {
            get { return _Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SortedSet&lt;T&gt;"/>
        /// is read-only.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <see cref="SortedSet&lt;T&gt;"/> is read-only;
        /// otherwise, <c>false</c>. This property always returns <c>false</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// A collection that is read-only does not allow the addition, removal, or
        /// modification of elements after the collection is created.
        /// </para>
        /// <para>
        /// Retrieving the value of this property is an O(1) operation.
        /// </para>
        /// </remarks>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Adds the specified <paramref name="item"/> to this
        /// <see cref="SortedSet&lt;T&gt;"/> if it is not already present.
        /// </summary>
        /// <param name="item">
        /// The item to add to this <see cref="SortedSet&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="SortedSet&lt;T&gt;"/> did not
        /// already contain the specified <paramref name="item"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is a <c>null</c> reference.
        /// </exception>
        public bool Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            if (p == null)
            {
                _Root = new Node(item);
            }
            else
            {
                while (true)
                {
                    int c = item.CompareTo(p.Item);

                    if (c < 0)
                    {
                        if (p.Left != null)
                        {
                            p = p.Left;
                        }
                        else
                        {
                            p.Left = new Node(item, p);
                            p.Balance--;

                            break;
                        }
                    }
                    else if (c > 0)
                    {
                        if (p.Right != null)
                        {
                            p = p.Right;
                        }
                        else
                        {
                            p.Right = new Node(item, p);
                            p.Balance++;

                            break;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                while ((p.Balance != 0) && (p.Parent != null))
                {
                    if (p.Parent.Left == p)
                    {
                        p.Parent.Balance--;
                    }
                    else
                    {
                        p.Parent.Balance++;
                    }

                    p = p.Parent;

                    if (p.Balance == -2)
                    {
                        Node x = p.Left;

                        if (x.Balance == -1)
                        {
                            x.Parent = p.Parent;

                            if (p.Parent == null)
                            {
                                _Root = x;
                            }
                            else
                            {
                                if (p.Parent.Left == p)
                                {
                                    p.Parent.Left = x;
                                }
                                else
                                {
                                    p.Parent.Right = x;
                                }
                            }

                            p.Left = x.Right;

                            if (p.Left != null)
                            {
                                p.Left.Parent = p;
                            }

                            x.Right = p;
                            p.Parent = x;

                            x.Balance = 0;
                            p.Balance = 0;
                        }
                        else
                        {
                            Node w = x.Right;

                            w.Parent = p.Parent;

                            if (p.Parent == null)
                            {
                                _Root = w;
                            }
                            else
                            {
                                if (p.Parent.Left == p)
                                {
                                    p.Parent.Left = w;
                                }
                                else
                                {
                                    p.Parent.Right = w;
                                }
                            }

                            x.Right = w.Left;

                            if (x.Right != null)
                            {
                                x.Right.Parent = x;
                            }

                            p.Left = w.Right;

                            if (p.Left != null)
                            {
                                p.Left.Parent = p;
                            }

                            w.Left = x;
                            w.Right = p;

                            x.Parent = w;
                            p.Parent = w;

                            if (w.Balance == -1)
                            {
                                x.Balance = 0;
                                p.Balance = 1;
                            }
                            else if (w.Balance == 0)
                            {
                                x.Balance = 0;
                                p.Balance = 0;
                            }
                            else // w.Balance == 1
                            {
                                x.Balance = -1;
                                p.Balance = 0;
                            }

                            w.Balance = 0;
                        }

                        break;
                    }
                    else if (p.Balance == 2)
                    {
                        Node x = p.Right;

                        if (x.Balance == 1)
                        {
                            x.Parent = p.Parent;

                            if (p.Parent == null)
                            {
                                _Root = x;
                            }
                            else
                            {
                                if (p.Parent.Left == p)
                                {
                                    p.Parent.Left = x;
                                }
                                else
                                {
                                    p.Parent.Right = x;
                                }
                            }

                            p.Right = x.Left;

                            if (p.Right != null)
                            {
                                p.Right.Parent = p;
                            }

                            x.Left = p;
                            p.Parent = x;

                            x.Balance = 0;
                            p.Balance = 0;
                        }
                        else
                        {
                            Node w = x.Left;

                            w.Parent = p.Parent;

                            if (p.Parent == null)
                            {
                                _Root = w;
                            }
                            else
                            {
                                if (p.Parent.Left == p)
                                {
                                    p.Parent.Left = w;
                                }
                                else
                                {
                                    p.Parent.Right = w;
                                }
                            }

                            x.Left = w.Right;

                            if (x.Left != null)
                            {
                                x.Left.Parent = x;
                            }

                            p.Right = w.Left;

                            if (p.Right != null)
                            {
                                p.Right.Parent = p;
                            }

                            w.Right = x;
                            w.Left = p;

                            x.Parent = w;
                            p.Parent = w;

                            if (w.Balance == 1)
                            {
                                x.Balance = 0;
                                p.Balance = -1;
                            }
                            else if (w.Balance == 0)
                            {
                                x.Balance = 0;
                                p.Balance = 0;
                            }
                            else // w.Balance == -1
                            {
                                x.Balance = 1;
                                p.Balance = 0;
                            }

                            w.Balance = 0;
                        }

                        break;
                    }
                }
            }

            _Count++;
            return true;
        }

        /// <summary>
        /// Removes all elements from this <see cref="SortedSet&lt;T&gt;"/>. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is an O(1) operation.
        /// </para>
        /// <para>
        /// The <see cref="Count"/> property is set to zero, and references to other
        /// objects from elements of the collection are also released.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            _Root = null;
            _Count = 0;
        }

        /// <summary>
        /// Determines whether this <see cref="SortedSet&lt;T&gt;"/>
        /// contains a specific item.
        /// </summary>
        /// <param name="item">
        /// The item to locate in this <see cref="SortedSet&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <c>true</c>, if the <paramref name="item"/> is found in this
        /// <see cref="SortedSet&lt;T&gt;"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is a <c>null</c> reference.
        /// </exception>
        public bool Contains(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            while (p != null)
            {
                int c = item.CompareTo(p.Item);

                if (c < 0)
                {
                    p = p.Left;
                }
                else if (c > 0)
                {
                    p = p.Right;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Copies the <see cref="SortedSet&lt;T&gt;"/> elements to
        /// an existing one-dimensional <see cref="Array"/>, starting at the
        /// specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements
        /// copied from this <see cref="SortedSet&lt;T&gt;"/>.
        /// <paramref name="array"/> must have zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in the <paramref name="array"/> at which copying begins.
        /// </param>
        /// <remarks>
        /// <para>
        /// The elements are copied to the <paramref name="array"/> in ascending order.
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
        /// The number of elements in the source <see cref="SortedSet&lt;T&gt;"/>
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

            Node p = _Root;

            if (p == null)
            {
                return;
            }

            while (p.Left != null)
            {
                p = p.Left;
            }

            while (true)
            {
                array[index] = p.Item;

                if (p.Right == null)
                {
                    while (true)
                    {
                        if (p.Parent == null)
                        {
                            return;
                        }

                        if (p != p.Parent.Right)
                        {
                            break;
                        }

                        p = p.Parent;
                    }

                    p = p.Parent;
                }
                else
                {
                    p = p.Right;

                    while (p.Left != null)
                    {
                        p = p.Left;
                    }
                }

                index++;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="SortedSet&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="SortedSet&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Removes the specified <paramref name="item"/> from this
        /// <see cref="SortedSet&lt;T&gt;"/> if it is present. 
        /// </summary>
        /// <param name="item">
        /// The item to remove from this <see cref="SortedSet&lt;T&gt;"/>,
        /// if present.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="SortedSet&lt;T&gt;"/>
        /// contained the specified <paramref name="item"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is a <c>null</c> reference.
        /// </exception>
        public bool Remove(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            while (p != null)
            {
                int c = item.CompareTo(p.Item);

                if (c < 0)
                {
                    p = p.Left;
                }
                else if (c > 0)
                {
                    p = p.Right;
                }
                else
                {
                    Node y; // node from which rebalancing begins

                    if (p.Right == null) // Case 1: p has no right child
                    {
                        if (p.Left != null)
                        {
                            p.Left.Parent = p.Parent;
                        }

                        if (p.Parent == null)
                        {
                            _Root = p.Left;

                            goto Done;
                        }

                        if (p == p.Parent.Left)
                        {
                            p.Parent.Left = p.Left;

                            y = p.Parent;
                            // goto LeftDelete;
                        }
                        else
                        {
                            p.Parent.Right = p.Left;

                            y = p.Parent;
                            goto RightDelete;
                        }
                    }
                    else if (p.Right.Left == null) // Case 2: p's right child has no left child
                    {
                        if (p.Left != null)
                        {
                            p.Left.Parent = p.Right;
                            p.Right.Left = p.Left;
                        }

                        p.Right.Balance = p.Balance;
                        p.Right.Parent = p.Parent;

                        if (p.Parent == null)
                        {
                            _Root = p.Right;
                        }
                        else
                        {
                            if (p == p.Parent.Left)
                            {
                                p.Parent.Left = p.Right;
                            }
                            else
                            {
                                p.Parent.Right = p.Right;
                            }
                        }

                        y = p.Right;

                        goto RightDelete;
                    }
                    else // Case 3: p's right child has a left child
                    {
                        Node s = p.Right.Left;

                        while (s.Left != null)
                        {
                            s = s.Left;
                        }

                        if (p.Left != null)
                        {
                            p.Left.Parent = s;
                            s.Left = p.Left;
                        }

                        s.Parent.Left = s.Right;

                        if (s.Right != null)
                        {
                            s.Right.Parent = s.Parent;
                        }

                        p.Right.Parent = s;
                        s.Right = p.Right;

                        y = s.Parent; // for rebalacing, must be set before we change s.Parent

                        s.Balance = p.Balance;
                        s.Parent = p.Parent;

                        if (p.Parent == null)
                        {
                            _Root = s;
                        }
                        else
                        {
                            if (p == p.Parent.Left)
                            {
                                p.Parent.Left = s;
                            }
                            else
                            {
                                p.Parent.Right = s;
                            }
                        }

                        // goto LeftDelete;
                    }

                    // rebalancing begins

                    LeftDelete:

                    y.Balance++;

                    if (y.Balance == 1)
                    {
                        goto Done;
                    }
                    else if (y.Balance == 2)
                    {
                        Node x = y.Right;

                        if (x.Balance == -1)
                        {
                            Node w = x.Left;

                            w.Parent = y.Parent;

                            if (y.Parent == null)
                            {
                                _Root = w;
                            }
                            else
                            {
                                if (y.Parent.Left == y)
                                {
                                    y.Parent.Left = w;
                                }
                                else
                                {
                                    y.Parent.Right = w;
                                }
                            }

                            x.Left = w.Right;

                            if (x.Left != null)
                            {
                                x.Left.Parent = x;
                            }

                            y.Right = w.Left;

                            if (y.Right != null)
                            {
                                y.Right.Parent = y;
                            }

                            w.Right = x;
                            w.Left = y;

                            x.Parent = w;
                            y.Parent = w;

                            if (w.Balance == 1)
                            {
                                x.Balance = 0;
                                y.Balance = -1;
                            }
                            else if (w.Balance == 0)
                            {
                                x.Balance = 0;
                                y.Balance = 0;
                            }
                            else // w.Balance == -1
                            {
                                x.Balance = 1;
                                y.Balance = 0;
                            }

                            w.Balance = 0;

                            y = w; // for next iteration
                        }
                        else
                        {
                            x.Parent = y.Parent;

                            if (y.Parent != null)
                            {
                                if (y.Parent.Left == y)
                                {
                                    y.Parent.Left = x;
                                }
                                else
                                {
                                    y.Parent.Right = x;
                                }
                            }
                            else
                            {
                                _Root = x;
                            }

                            y.Right = x.Left;

                            if (y.Right != null)
                            {
                                y.Right.Parent = y;
                            }

                            x.Left = y;
                            y.Parent = x;

                            if (x.Balance == 0)
                            {
                                x.Balance = -1;
                                y.Balance = 1;

                                goto Done;
                            }
                            else
                            {
                                x.Balance = 0;
                                y.Balance = 0;

                                y = x; // for next iteration
                            }
                        }
                    }

                    goto LoopTest;


                    RightDelete:

                    y.Balance--;

                    if (y.Balance == -1)
                    {
                        goto Done;
                    }
                    else if (y.Balance == -2)
                    {
                        Node x = y.Left;

                        if (x.Balance == 1)
                        {
                            Node w = x.Right;

                            w.Parent = y.Parent;

                            if (y.Parent == null)
                            {
                                _Root = w;
                            }
                            else
                            {
                                if (y.Parent.Left == y)
                                {
                                    y.Parent.Left = w;
                                }
                                else
                                {
                                    y.Parent.Right = w;
                                }
                            }

                            x.Right = w.Left;

                            if (x.Right != null)
                            {
                                x.Right.Parent = x;
                            }

                            y.Left = w.Right;

                            if (y.Left != null)
                            {
                                y.Left.Parent = y;
                            }

                            w.Left = x;
                            w.Right = y;

                            x.Parent = w;
                            y.Parent = w;

                            if (w.Balance == -1)
                            {
                                x.Balance = 0;
                                y.Balance = 1;
                            }
                            else if (w.Balance == 0)
                            {
                                x.Balance = 0;
                                y.Balance = 0;
                            }
                            else // w.Balance == 1
                            {
                                x.Balance = -1;
                                y.Balance = 0;
                            }

                            w.Balance = 0;

                            y = w; // for next iteration
                        }
                        else
                        {
                            x.Parent = y.Parent;

                            if (y.Parent != null)
                            {
                                if (y.Parent.Left == y)
                                {
                                    y.Parent.Left = x;
                                }
                                else
                                {
                                    y.Parent.Right = x;
                                }
                            }
                            else
                            {
                                _Root = x;
                            }

                            y.Left = x.Right;

                            if (y.Left != null)
                            {
                                y.Left.Parent = y;
                            }

                            x.Right = y;
                            y.Parent = x;

                            if (x.Balance == 0)
                            {
                                x.Balance = 1;
                                y.Balance = -1;

                                goto Done;
                            }
                            else
                            {
                                x.Balance = 0;
                                y.Balance = 0;

                                y = x; // for next iteration
                            }
                        }
                    }

                    LoopTest:

                    if (y.Parent != null)
                    {
                        if (y == y.Parent.Left)
                        {
                            y = y.Parent;
                            goto LeftDelete;
                        }

                        y = y.Parent;
                        goto RightDelete;
                    }

                    Done:

                    _Count--;
                    return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="SortedSet&lt;T&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        public AscendingOrderEnumerator GetEnumerator()
        {
            Node p = _Root;

            if (p != null)
            {
                while (p.Left != null)
                {
                    p = p.Left;
                }
            }

            return new AscendingOrderEnumerator(p);
        }

        #region Nested type: AscendingOrderEnumerator

        /// <summary>
        /// Enumerates the elements of the <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The elements are enumerated in ascending order.
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
        /// The <see cref="AscendingOrderEnumerator"/> is not designed to provide
        /// any fast-fail safety mechanisms against concurrent modifications.
        /// </para>
        /// </remarks>
        public struct AscendingOrderEnumerator : IEnumerator<T>
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
            /// Initializes a new instance of the <see cref="AscendingOrderEnumerator"/>
            /// structure with the specified <paramref name="node"/>.
            /// </summary>
            /// <param name="node">
            /// The node from which to start enumerating the
            /// <see cref="SortedSet&lt;T&gt;"/> elements.
            /// </param>
            internal AscendingOrderEnumerator(Node node)
            {
                _Next = node;
                _Current = default(T);
            }

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the element in the <see cref="SortedSet&lt;T&gt;"/> at
            /// the current position of the enumerator. 
            /// </summary>
            /// <value>
            /// The element in the <see cref="SortedSet&lt;T&gt;"/> at the current
            /// position of the enumerator.
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
            /// Gets the element in the <see cref="SortedSet&lt;T&gt;"/> at
            /// the current position of the enumerator. 
            /// </summary>
            /// <value>
            /// The element in the <see cref="SortedSet&lt;T&gt;"/> at the current
            /// position of the enumerator.
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
            /// Advances the enumerator to the next element of the <see cref="SortedSet&lt;T&gt;"/>.
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

                if (_Next.Right == null)
                {
                    while ((_Next.Parent != null) && (_Next == _Next.Parent.Right))
                    {
                        _Next = _Next.Parent;
                    }

                    _Next = _Next.Parent;
                }
                else
                {
                    _Next = _Next.Right;

                    while (_Next.Left != null)
                    {
                        _Next = _Next.Left;
                    }
                }

                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position,
            /// which is before the first element in the <see cref="SortedSet&lt;T&gt;"/>.
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
            /// Releases all resources allocated by the <see cref="AscendingOrderEnumerator"/>.
            /// </summary>
            void IDisposable.Dispose()
            {
            }

            #endregion
        }

        #endregion

        #region Nested type: Node

        /// <summary>
        /// Represents a node in the <see cref="SortedSet&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Node"/> contains a value, a reference to the parent node,
        /// a reference to the left child node, a reference to the right child node
        /// and a balance factor for this node.
        /// </remarks>
        internal sealed class Node
        {
            /// <summary>
            /// The balance factor of this node.
            /// </summary>
            /// <remarks>
            /// The balance factor of a node is the height of its right subtree minus the height
            /// of its left subtree. A node with balance factor 1, 0, or -1 is considered balanced.
            /// A node with balance factor -2 or 2 is considered unbalanced and requires rebalancing
            /// the tree.
            /// </remarks>
            public sbyte Balance;

            /// <summary>
            /// The object contained in this node.
            /// </summary>
            public T Item;

            /// <summary>
            /// The reference to the left child node of this <see cref="Node"/>
            /// or <c>null</c> if this <see cref="Node"/> has no left child node.
            /// </summary>
            public Node Left;

            /// <summary>
            /// The reference to the parent node of this <see cref="Node"/> or <c>null</c>
            /// if this <see cref="Node"/> is the root node in the balanced binary tree.
            /// </summary>
            public Node Parent;

            /// <summary>
            /// The reference to the right child node of this <see cref="Node"/>
            /// or <c>null</c> if this <see cref="Node"/> has no right child node.
            /// </summary>
            public Node Right;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class containing the
            /// specified value.
            /// </summary>
            /// <param name="item">
            /// The object to contain in the <see cref="Node"/>.
            /// </param>
            /// <remarks>
            /// The <see cref="Parent"/>, <see cref="Left"/>, and <see cref="Right"/> fields are
            /// initialized to <c>null</c>. The <see cref="Balance"/> factor field is initialized
            /// to zero.
            /// </remarks>
            public Node(T item)
            {
                Item = item;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class containing the
            /// specified value and the reference to the parent node of this <see cref="Node"/>.
            /// </summary>
            /// <param name="item">
            /// The object to contain in the <see cref="Node"/>.
            /// </param>
            /// <param name="parent">
            /// The reference to the parent node of this <see cref="Node"/>.
            /// </param>
            /// <remarks>
            /// The <see cref="Left"/> and <see cref="Right"/> fields are initialized to
            /// <c>null</c>. The <see cref="Balance"/> factor field is initialized to zero.
            /// </remarks>
            public Node(T item, Node parent)
            {
                Item = item;
                Parent = parent;
            }
        }

        #endregion
    }
}