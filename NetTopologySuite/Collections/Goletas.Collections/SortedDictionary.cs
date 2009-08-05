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
    /// Represents a collection of key/value pairs that are sorted on the key.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each element in the <see cref="SortedDictionary&lt;K,V&gt;"/> is a
    /// key/value pair stored in the <see cref="KeyValuePair&lt;K,V&gt;"/> object.
    /// </para>
    /// <para>
    /// A key cannnot be a <c>null</c> reference. Each key must be unique within
    /// <see cref="SortedDictionary&lt;K,V&gt;"/>. Keys must be immutable for the
    /// <see cref="IComparable&lt;T&gt;"/> interface as long as they are used as
    /// keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
    /// </para>
    /// <para>
    /// This <see cref="SortedDictionary&lt;K,V&gt;"/> uses the same algorithms as the
    /// <see cref="SortedSet&lt;T&gt;"/> to store and manage its elements.
    /// </para>
    /// </remarks>
    /// <typeparam name="K">
    /// The type of keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of values in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
    /// </typeparam>
    public sealed class SortedDictionary<K, V> : IDictionary<K, V> where K : IComparable<K>
    {
        /// <summary>
        /// The number of elements contained in this <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        private int _Count;

        /// <summary>
        /// The <see cref="KeyCollection"/> containing the
        /// keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// </summary>
        private KeyCollection _Keys;

        /// <summary>
        /// The root node of this <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        private Node _Root;

        /// <summary>
        /// The <see cref="ValueCollection"/> containing the
        /// values in the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// </summary>
        private ValueCollection _Values;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedDictionary&lt;K,V&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is an O(1) operation.
        /// </remarks>
        public SortedDictionary()
        {
            _Keys = new KeyCollection(this);
            _Values = new ValueCollection(this);
        }

        /// <summary>
        /// Gets a collection containing the keys in the the <see cref="SortedDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <value>
        /// The <see cref="KeyCollection"/> containing the
        /// keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        public KeyCollection Keys
        {
            get { return _Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values in the <see cref="SortedDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <value>
        /// The <see cref="ValueCollection"/> containing the
        /// values in the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        public ValueCollection Values
        {
            get { return _Values; }
        }

        #region IDictionary<K,V> Members

        /// <summary>
        /// Adds an element with the provided <paramref name="key"/> and
        /// <paramref name="value"/> to the <see cref="SortedDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="SortedDictionary&lt;K,V&gt;"/> did not
        /// already contain the specified <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        public bool Add(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            if (p == null)
            {
                _Root = new Node(new KeyValuePair<K, V>(key, value));
            }
            else
            {
                while (true)
                {
                    int c = key.CompareTo(p.Item._Key);

                    if (c < 0)
                    {
                        if (p.Left != null)
                        {
                            p = p.Left;
                        }
                        else
                        {
                            p.Left = new Node(new KeyValuePair<K, V>(key, value), p);
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
                            p.Right = new Node(new KeyValuePair<K, V>(key, value), p);
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
        /// Retrieves the value associated with the specified <paramref name="key"/>. 
        /// </summary>
        /// <param name="key">
        /// The key whose value to retrieve.
        /// </param>
        /// <param name="value">
        /// If the key is found, the value associated with the specified <paramref name="key"/>;
        /// otherwise, the default value for the type of the <paramref name="value"/> parameter.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="SortedDictionary&lt;K,V&gt;"/> contains an element with the
        /// specified <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        public bool TryGetValue(K key, out V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            while (p != null)
            {
                int c = key.CompareTo(p.Item._Key);

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
                    value = p.Item._Value;
                    return true;
                }
            }

            value = default(V);
            return false;
        }

        /// <summary>
        /// Removes the element with the specified <paramref name="key"/>
        /// from the the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if this <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// contained the specified element; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        public bool Remove(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            while (p != null)
            {
                int c = key.CompareTo(p.Item._Key);

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

        /// <summary>
        /// Gets or sets the element with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        /// <returns>
        /// The value of the element with the specified <paramref name="key"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The <see cref="this"/> property can be used to add new elements by setting
        /// the value of a key that does not exist in <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// If the specified <paramref name="key"/> already exists in the dictionary,
        /// setting the <see cref="this"/> property overwrites the old value.
        /// </para>
        /// <para>
        /// Accessing this property is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// The property is retrieved and the <paramref name="key"/> is not found.
        /// </exception>
        public V this[K key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException();
                }

                Node p = _Root;

                while (p != null)
                {
                    int c = key.CompareTo(p.Item._Key);

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
                        return p.Item._Value;
                    }
                }

                throw new KeyNotFoundException();
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException();
                }

                Node p = _Root;

                if (p == null)
                {
                    _Root = new Node(new KeyValuePair<K, V>(key, value));
                }
                else
                {
                    while (true)
                    {
                        int c = key.CompareTo(p.Item._Key);

                        if (c < 0)
                        {
                            if (p.Left != null)
                            {
                                p = p.Left;
                            }
                            else
                            {
                                p.Left = new Node(new KeyValuePair<K, V>(key, value), p);
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
                                p.Right = new Node(new KeyValuePair<K, V>(key, value), p);
                                p.Balance++;

                                break;
                            }
                        }
                        else
                        {
                            p.Item._Value = value;
                            return;
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
            }
        }

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <value>
        /// The <see cref="ICollection&lt;T&gt;"/> containing the
        /// keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        ICollection<K> IDictionary<K, V>.Keys
        {
            get { return _Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values in the <see cref="SortedDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <value>
        /// The <see cref="ICollection&lt;T&gt;"/> containing the
        /// values in the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        ICollection<V> IDictionary<K, V>.Values
        {
            get { return _Values; }
        }

        /// <summary>
        /// Removes all elements from this <see cref="SortedDictionary&lt;K,V&gt;"/>. 
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
        /// Gets the number of key/value pairs contained in this <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <value>
        /// The number of key/value pairs contained in this <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </value>
        /// <remarks>
        /// Retrieving the value of this property is an O(1) operation.
        /// </remarks>
        public int Count
        {
            get { return _Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// is read-only.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <see cref="SortedDictionary&lt;K,V&gt;"/> is read-only;
        /// otherwise, <c>false</c>. This property always returns <c>false</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// A collection that is read-only does not allow the addition, removal or
        /// modification of elements after the collection is created.
        /// </para>
        /// <para>
        /// Retrieving the value of this property is an O(1) operation.
        /// </para>
        /// </remarks>
        bool IDictionary<K, V>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Copies the <see cref="SortedDictionary&lt;K,V&gt;"/> elements to an existing
        /// one-dimensional <see cref="Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements
        /// copied from this <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// The <paramref name="array"/> must have zero-based indexing.
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
        /// <paramref name="index"/> is outside of <paramref name="array"/> bounds.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="SortedDictionary&lt;K,V&gt;"/>
        /// is greater than the available space from the <paramref name="index"/> to the end
        /// of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(KeyValuePair<K, V>[] array, int index)
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
        /// Returns an enumerator that iterates through the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Overwrites the value for the specified <paramref name="key"/> in the
        /// <see cref="SortedDictionary&lt;K,V&gt;"/> with the new <paramref name="value"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value to replace.
        /// </param>
        /// <param name="value">
        /// The value to associate with the specified <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an element with the speficied <paramref name="key"/>
        /// was found and updated with the new <paramref name="value"/>; otherwise,
        /// <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        public bool Replace(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException();
            }

            Node p = _Root;

            while (p != null)
            {
                int c = key.CompareTo(p.Item._Key);

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
                    p.Item._Value = value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="SortedDictionary&lt;K,V&gt;"/>.
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
        /// Enumerates the key/value pairs of the <see cref="SortedDictionary&lt;K,V&gt;"/>.
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
        public struct AscendingOrderEnumerator : IEnumerator<KeyValuePair<K, V>>
        {
            /// <summary>
            /// The element at the current position of the enumerator.
            /// </summary>
            private KeyValuePair<K, V> _Current;

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
            /// <see cref="SortedDictionary&lt;K,V&gt;"/> elements.
            /// </param>
            internal AscendingOrderEnumerator(Node node)
            {
                _Next = node;
                _Current = new KeyValuePair<K, V>();
            }

            #region IEnumerator<KeyValuePair<K,V>> Members

            /// <summary>
            /// Gets the element in the <see cref="SortedDictionary&lt;K,V&gt;"/> at
            /// the current position of the enumerator. 
            /// </summary>
            /// <value>
            /// The element in the <see cref="SortedDictionary&lt;K,V&gt;"/> at the current
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
            public KeyValuePair<K, V> Current
            {
                get { return _Current; }
            }

            /// <summary>
            /// Gets the element in the <see cref="SortedDictionary&lt;K,V&gt;"/> at
            /// the current position of the enumerator. 
            /// </summary>
            /// <value>
            /// The element in the <see cref="SortedDictionary&lt;K,V&gt;"/> at the current
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
            /// Advances the enumerator to the next element of the <see cref="SortedDictionary&lt;K,V&gt;"/>.
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
            /// which is before the first element in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
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

        #region Nested type: KeyCollection

        /// <summary>
        /// Represents a collection of keys in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="KeyCollection"/> is not a static copy; instead, the
        /// <see cref="KeyCollection"/> refers back to the keys in the original
        /// <see cref="SortedDictionary&lt;K,V&gt;"/>. Therefore, changes to the
        /// <see cref="SortedDictionary&lt;K,V&gt;"/> continue to be reflected in
        /// the <see cref="KeyCollection"/>.
        /// </remarks>
        public sealed class KeyCollection : ICollection<K>
        {
            /// <summary>
            /// The dictionary for which this <see cref="KeyCollection"/> was created.
            /// </summary>
            private SortedDictionary<K, V> _Dictionary;

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyCollection"/>
            /// class with the specified <paramref name="dictionary"/>.
            /// </summary>
            /// <remarks>
            /// This constructor is an O(1) operation.
            /// </remarks>
            internal KeyCollection(SortedDictionary<K, V> dictionary)
            {
                _Dictionary = dictionary;
            }

            #region ICollection<K> Members

            /// <summary>
            /// Gets the number of elements contained in this <see cref="KeyCollection"/>.
            /// </summary>
            /// <value>
            /// The number of elements contained in this <see cref="KeyCollection"/>.
            /// </value>
            /// <remarks>
            /// Retrieving the value of this property is an O(1) operation.
            /// </remarks>
            public int Count
            {
                get { return _Dictionary._Count; }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="KeyCollection"/>
            /// is read-only.
            /// </summary>
            /// <value>
            /// <c>true</c> if the <see cref="KeyCollection"/> is read-only;
            /// otherwise, <c>false</c>. This property always returns <c>true</c>.
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
            bool ICollection<K>.IsReadOnly
            {
                get { return true; }
            }

            /// <summary>
            /// Determines whether this <see cref="KeyCollection"/> contains a specific key.
            /// </summary>
            /// <param name="item">
            /// The key to locate in this <see cref="KeyCollection"/>.
            /// </param>
            /// <returns>
            /// <c>true</c>, if the <paramref name="item"/> is found in this
            /// <see cref="KeyCollection"/>; otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="item"/> is a <c>null</c> reference.
            /// </exception>
            public bool Contains(K item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException();
                }

                Node p = _Dictionary._Root;

                while (p != null)
                {
                    int c = item.CompareTo(p.Item._Key);

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
            /// Copies the <see cref="KeyCollection"/> elements to an existing
            /// one-dimensional <see cref="Array"/>, starting at the specified array index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional <see cref="Array"/> that is the destination of the elements
            /// copied from this <see cref="KeyCollection"/>.
            /// The <paramref name="array"/> must have zero-based indexing.
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
            /// The number of elements in the source <see cref="KeyCollection"/> is greater
            /// than the available space from the <paramref name="index"/> to the end of the
            /// destination <paramref name="array"/>.
            /// </exception>
            public void CopyTo(K[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if ((index < 0) || (index >= array.Length))
                {
                    throw new ArgumentOutOfRangeException();
                }

                if ((array.Length - index) < _Dictionary._Count)
                {
                    throw new ArgumentException();
                }

                Node p = _Dictionary._Root;

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
                    array[index] = p.Item._Key;

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
            /// Returns an enumerator that iterates through the <see cref="KeyCollection"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="KeyCollection"/>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            IEnumerator<K> IEnumerable<K>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="KeyCollection"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="KeyCollection"/>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Adds an item to the <see cref="KeyCollection"/>.
            /// This method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="item">
            /// The object to add to the <see cref="KeyCollection"/>.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// Always thrown; the <see cref="KeyCollection"/> is read-only.
            /// </exception>
            bool ICollection<K>.Add(K item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes the first occurrence of the <paramref name="item"/> from
            /// the <see cref="KeyCollection"/>. This method always throws
            /// a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="item">
            /// The object to remove from the <see cref="KeyCollection"/>.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// Always thrown; the <see cref="KeyCollection"/> is read-only.
            /// </exception>
            bool ICollection<K>.Remove(K item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="KeyCollection"/>.
            /// This method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <exception cref="NotSupportedException">
            /// Always thrown; the <see cref="KeyCollection"/> is read-only.
            /// </exception>
            void ICollection<K>.Clear()
            {
                throw new NotSupportedException();
            }

            #endregion

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="KeyCollection"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="KeyCollection"/>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            public AscendingOrderEnumerator GetEnumerator()
            {
                Node p = _Dictionary._Root;

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
            /// Enumerates the elements of the <see cref="KeyCollection"/>.
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
            public struct AscendingOrderEnumerator : IEnumerator<K>
            {
                /// <summary>
                /// The element at the current position of the enumerator.
                /// </summary>
                private K _Current;

                /// <summary>
                /// The <see cref="Node"/> at the current position of the enumerator.
                /// </summary>
                private Node _Next;

                /// <summary>
                /// Initializes a new instance of the <see cref="AscendingOrderEnumerator"/>
                /// structure with the specified <paramref name="node"/>.
                /// </summary>
                /// <param name="node">
                /// The node from which to start enumerating the <see cref="KeyCollection"/>
                /// elements.
                /// </param>
                internal AscendingOrderEnumerator(Node node)
                {
                    _Next = node;
                    _Current = default(K);
                }

                #region IEnumerator<K> Members

                /// <summary>
                /// Gets the element in the <see cref="KeyCollection"/> at
                /// the current position of the enumerator. 
                /// </summary>
                /// <value>
                /// The element in the <see cref="KeyCollection"/> at the current position
                /// of the enumerator.
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
                public K Current
                {
                    get { return _Current; }
                }

                /// <summary>
                /// Gets the element in the <see cref="KeyCollection"/> at
                /// the current position of the enumerator. 
                /// </summary>
                /// <value>
                /// The element in the <see cref="KeyCollection"/> at the current position
                /// of the enumerator.
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
                /// Advances the enumerator to the next element of the <see cref="KeyCollection"/>.
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

                    _Current = _Next.Item._Key;

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
                /// which is before the first element in the <see cref="KeyCollection"/>.
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
        }

        #endregion

        #region Nested type: Node

        /// <summary>
        /// Represents a node in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Node"/> contains a key/value pair, a reference to the parent
        /// node, a reference to the left child node, a reference to the right child node
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
            /// The key/value pair contained in this node.
            /// </summary>
            public KeyValuePair<K, V> Item;

            /// <summary>
            /// The reference to the left child node of this <see cref="Node"/>
            /// or <c>null</c> if this <see cref="Node"/> has no left child node.
            /// </summary>
            public Node Left;

            /// <summary>
            /// The reference to the parent node of this <see cref="Node"/> or <c>null</c> if
            /// this <see cref="Node"/> is the root node in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
            /// </summary>
            public Node Parent;

            /// <summary>
            /// The reference to the right child node of this <see cref="Node"/>
            /// or <c>null</c> if this <see cref="Node"/> has no right child node.
            /// </summary>
            public Node Right;

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class containing the
            /// specified key/value pair.
            /// </summary>
            /// <param name="item">
            /// The key/value pair to contain in the <see cref="Node"/>.
            /// </param>
            /// <remarks>
            /// The <see cref="Parent"/>, <see cref="Left"/>, and <see cref="Right"/> fields are
            /// initialized to <c>null</c>. The <see cref="Balance"/> factor field is initialized
            /// to zero.
            /// </remarks>
            public Node(KeyValuePair<K, V> item)
            {
                Item = item;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class containing the
            /// specified key/value pair and the reference to the parent node of this
            /// <see cref="Node"/>.
            /// </summary>
            /// <param name="item">
            /// The key/value pair to contain in the <see cref="Node"/>.
            /// </param>
            /// <param name="parent">
            /// The reference to the parent node of this <see cref="Node"/>.
            /// </param>
            /// <remarks>
            /// The <see cref="Left"/> and <see cref="Right"/> fields are initialized to
            /// <c>null</c>. The <see cref="Balance"/> factor field is initialized to zero.
            /// </remarks>
            public Node(KeyValuePair<K, V> item, Node parent)
            {
                Item = item;
                Parent = parent;
            }
        }

        #endregion

        #region Nested type: ValueCollection

        /// <summary>
        /// Represents a collection of values in the <see cref="SortedDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ValueCollection"/> is not a static copy; instead, the
        /// <see cref="ValueCollection"/> refers back to the keys in the original
        /// <see cref="SortedDictionary&lt;K,V&gt;"/>. Therefore, changes to the
        /// <see cref="SortedDictionary&lt;K,V&gt;"/> continue to be reflected in
        /// the <see cref="ValueCollection"/>.
        /// </remarks>
        public sealed class ValueCollection : ICollection<V>
        {
            /// <summary>
            /// The dictionary for which this <see cref="ValueCollection"/> was created.
            /// </summary>
            private SortedDictionary<K, V> _Dictionary;

            /// <summary>
            /// Initializes a new instance of the <see cref="ValueCollection"/>
            /// class with the specified <paramref name="dictionary"/>.
            /// </summary>
            /// <remarks>
            /// This constructor is an O(1) operation.
            /// </remarks>
            internal ValueCollection(SortedDictionary<K, V> dictionary)
            {
                _Dictionary = dictionary;
            }

            #region ICollection<V> Members

            /// <summary>
            /// Gets the number of elements contained in this <see cref="ValueCollection"/>.
            /// </summary>
            /// <value>
            /// The number of elements contained in this <see cref="ValueCollection"/>.
            /// </value>
            /// <remarks>
            /// Retrieving the value of this property is an O(1) operation.
            /// </remarks>
            public int Count
            {
                get { return _Dictionary._Count; }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="ValueCollection"/>
            /// is read-only.
            /// </summary>
            /// <value>
            /// <c>true</c> if the <see cref="ValueCollection"/> is read-only;
            /// otherwise, <c>false</c>. This property always returns <c>true</c>.
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
            bool ICollection<V>.IsReadOnly
            {
                get { return true; }
            }

            /// <summary>
            /// Determines whether this <see cref="ValueCollection"/> contains a specific value.
            /// </summary>
            /// <param name="item">
            /// The value to locate in this <see cref="ValueCollection"/>.
            /// </param>
            /// <returns>
            /// <c>true</c>, if the <paramref name="item"/> is found in this
            /// <see cref="ValueCollection"/>; otherwise, <c>false</c>.
            /// </returns>
            /// <remarks>
            /// This method is an O(n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            public bool Contains(V item)
            {
                Node p = _Dictionary._Root;

                if (p == null)
                {
                    return false;
                }

                while (p.Left != null)
                {
                    p = p.Left;
                }

                if (item != null)
                {
                    while (true)
                    {
                        if (item.Equals(p.Item._Value))
                        {
                            return true;
                        }

                        if (p.Right == null)
                        {
                            while (true)
                            {
                                if (p.Parent == null)
                                {
                                    return false;
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
                    }
                }
                else
                {
                    while (true)
                    {
                        if (p.Item._Value == null)
                        {
                            return true;
                        }

                        if (p.Right == null)
                        {
                            while (true)
                            {
                                if (p.Parent == null)
                                {
                                    return false;
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
                    }
                }
            }

            /// <summary>
            /// Copies the <see cref="ValueCollection"/> elements to an existing
            /// one-dimensional <see cref="Array"/>, starting at the specified array index.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional <see cref="Array"/> that is the destination of the elements
            /// copied from this <see cref="ValueCollection"/>.
            /// The <paramref name="array"/> must have zero-based indexing.
            /// </param>
            /// <param name="index">
            /// The zero-based index in the <paramref name="array"/> at which copying begins.
            /// </param>
            /// <remarks>
            /// <para>
            /// The elements are copied to <paramref name="array"/> in ascending order.
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
            /// The number of elements in the source <see cref="ValueCollection"/> is greater
            /// than the available space from the <paramref name="index"/> to the end of the
            /// destination <paramref name="array"/>.
            /// </exception>
            public void CopyTo(V[] array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException();
                }

                if ((index < 0) || (index >= array.Length))
                {
                    throw new ArgumentOutOfRangeException();
                }

                if ((array.Length - index) < _Dictionary._Count)
                {
                    throw new ArgumentException();
                }

                Node p = _Dictionary._Root;

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
                    array[index] = p.Item._Value;

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
            /// Returns an enumerator that iterates through the <see cref="ValueCollection"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="ValueCollection"/>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            IEnumerator<V> IEnumerable<V>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="ValueCollection"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="ValueCollection"/>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Adds an item to the <see cref="ValueCollection"/>.
            /// This method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="item">
            /// The object to add to the <see cref="ValueCollection"/>.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// Always thrown; the <see cref="ValueCollection"/> is read-only.
            /// </exception>
            bool ICollection<V>.Add(V item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes the first occurrence of the <paramref name="Item"/> from
            /// the <see cref="ValueCollection"/>. This method always throws
            /// a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="item">
            /// The object to remove from the <see cref="ValueCollection"/>.
            /// </param>
            /// <exception cref="NotSupportedException">
            /// Always thrown; the <see cref="ValueCollection"/> is read-only.
            /// </exception>
            bool ICollection<V>.Remove(V item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="ValueCollection"/>.
            /// This method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <exception cref="NotSupportedException">
            /// Always thrown; the <see cref="ValueCollection"/> is read-only.
            /// </exception>
            void ICollection<V>.Clear()
            {
                throw new NotSupportedException();
            }

            #endregion

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="ValueCollection"/>.
            /// </summary>
            /// <returns>
            /// An <see cref="AscendingOrderEnumerator"/> for the <see cref="ValueCollection"/>.
            /// </returns>
            /// <remarks>
            /// This method is an O(log2 n) operation, where n is <see cref="Count"/>.
            /// </remarks>
            public AscendingOrderEnumerator GetEnumerator()
            {
                Node p = _Dictionary._Root;

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
            /// Enumerates the elements of the <see cref="ValueCollection"/>.
            /// </summary>
            /// <remarks>
            /// <para>
            /// The elements are enumerated in ascending order for the key.
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
            public struct AscendingOrderEnumerator : IEnumerator<V>
            {
                /// <summary>
                /// The element at the current position of the enumerator.
                /// </summary>
                private V _Current;

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
                /// <see cref="ValueCollection"/> elements.
                /// </param>
                internal AscendingOrderEnumerator(Node node)
                {
                    _Next = node;
                    _Current = default(V);
                }

                #region IEnumerator<V> Members

                /// <summary>
                /// Gets the element in the <see cref="ValueCollection"/> at
                /// the current position of the enumerator. 
                /// </summary>
                /// <value>
                /// The element in the <see cref="ValueCollection"/> at the current
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
                public V Current
                {
                    get { return _Current; }
                }

                /// <summary>
                /// Gets the element in the <see cref="ValueCollection"/> at
                /// the current position of the enumerator. 
                /// </summary>
                /// <value>
                /// The element in the <see cref="ValueCollection"/> at the current
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
                /// Advances the enumerator to the next element of the <see cref="ValueCollection"/>.
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

                    _Current = _Next.Item._Value;

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
                /// which is before the first element in the <see cref="ValueCollection"/>.
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
        }

        #endregion
    }
}