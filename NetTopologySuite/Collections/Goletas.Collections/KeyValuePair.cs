//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Collections
{
    /// <summary>
    /// Represents a key/value pair.
    /// </summary>
    /// <typeparam name="K">
    /// The type of the <see cref="Key"/> contained
    /// in the <see cref="KeyValuePair&lt;K,V&gt;"/>.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of the <see cref="Value"/> contained
    /// in the <see cref="KeyValuePair&lt;K,V&gt;"/>.
    /// </typeparam>
    public struct KeyValuePair<K, V>
    {
        /// <summary>
        /// The key contained in this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </summary>
        internal K _Key;

        /// <summary>
        /// The value contained in this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </summary>
        internal V _Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePair&lt;K,V&gt;"/>
        /// structure with the specified key and value.
        /// </summary>
        /// <param name="key">
        /// The key identifying each <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </param>
        /// <param name="value">
        /// The definition associated with the <paramref name="key"/>
        /// of this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </param>
        public KeyValuePair(K key, V value)
        {
            _Key = key;
            _Value = value;
        }

        /// <summary>
        /// Gets the key contained in this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </summary>
        /// <value>
        /// The key contained in this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </value>
        public K Key
        {
            get { return _Key; }
        }

        /// <summary>
        /// Gets the value contained in this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </summary>
        /// <value>
        /// The value contained in this <see cref="KeyValuePair&lt;K,V&gt;"/>.
        /// </value>
        public V Value
        {
            get { return _Value; }
        }
    }
}