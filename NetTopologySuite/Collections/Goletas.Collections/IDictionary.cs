//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

using System;
using System.Collections.Generic;

namespace Goletas.Collections
{
    /// <summary>
    /// Represents a collection of key/value pairs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each element in the <see cref="IDictionary&lt;K,V&gt;"/> interface is a
    /// key/value pair stored in the <see cref="KeyValuePair&lt;K,V&gt;"/> structure.
    /// </para>
    /// <para>
    /// Each pair must have a unique <see cref="KeyValuePair&lt;K,V&gt;.Key"/>.
    /// Implementations can vary in whether they allow the key to be a <c>null</c>
    /// reference. The <see cref="KeyValuePair&lt;K,V&gt;.Value"/> can be a <c>null</c>
    /// reference and does not have to be unique. The <see cref="IDictionary&lt;K,V&gt;"/>
    /// interface allows the contained keys and values to be enumerated, but it does not
    /// imply any particular sort order.
    /// </para>
    /// </remarks>
    /// <typeparam name="K">
    /// The type of keys in the <see cref="IDictionary&lt;K,V&gt;"/>.
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of values in the <see cref="IDictionary&lt;K,V&gt;"/>.
    /// </typeparam>
    public interface IDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IDictionary&lt;K,V&gt;"/>
        /// is read-only.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <see cref="IDictionary&lt;K,V&gt;"/> is read-only;
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A dictionary that is read-only does not allow the addition, removal or
        /// modification of elements after the dictionary is created.
        /// </remarks>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="IDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <value>
        /// The number of key/value pairs contained in the <see cref="IDictionary&lt;K,V&gt;"/>.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets or sets the element with the specified <paramref name="key"/>.
        /// </summary>
        /// <value>
        /// The element with the specified <paramref name="key"/>.
        /// </value>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        /// <returns>
        /// The value of the element with the specified <paramref name="key"/>.
        /// </returns>
        /// <remarks>
        /// You can use the <see cref="this"/> property to add new elements by setting
        /// the value of a key that does not exist in the <see cref="IDictionary&lt;K,V&gt;"/>.
        /// If the specified <paramref name="key"/> already exists in the dictionary,
        /// setting the <see cref="this"/> property overwrites the old value.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// The property is retrieved and the <paramref name="key"/> is not found.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="IDictionary&lt;K,V&gt;"/> is read-only.
        /// </exception>
        V this[K key] { get; set; }

        /// <summary>
        /// Gets an <see cref="ICollection&lt;T&gt;"/> containing the keys of the
        /// <see cref="IDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <value>
        /// An <see cref="ICollection&lt;T&gt;"/> containing the keys of the
        /// object that implements <see cref="IDictionary&lt;K,V&gt;"/>. 
        /// </value>
        ICollection<K> Keys { get; }

        /// <summary>
        /// Gets an <see cref="ICollection&lt;T&gt;"/> containing the values of the
        /// <see cref="IDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <value>
        /// An <see cref="ICollection&lt;T&gt;"/> containing the values of the
        /// object that implements <see cref="IDictionary&lt;K,V&gt;"/>. 
        /// </value>
        ICollection<V> Values { get; }

        /// <summary>
        /// Adds an element with the provided <paramref name="key"/> and
        /// <paramref name="value"/> to the <see cref="IDictionary&lt;K,V&gt;"/>. 
        /// </summary>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element was successfully added to the
        /// <see cref="IDictionary&lt;K,V&gt;"/>; otherwise, <c>false</c>.
        /// This method also returns <c>false</c> if a particular
        /// <see cref="IDictionary&lt;K,V&gt;"/> implementation supports only unique
        /// keys and the dictionary already contains the specified <paramref name="key"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IDictionary&lt;K,V&gt;"/> is read-only.
        /// </exception>
        bool Add(K key, V value);

        /// <summary>
        /// Copies the <see cref="IDictionary&lt;K,V&gt;"/> key/value pairs to an existing
        /// one-dimensional <see cref="Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the
        /// key/value pairs copied from this <see cref="IDictionary&lt;K,V&gt;"/>.
        /// The <paramref name="array"/> must have zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in the <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside of <paramref name="array"/> bounds.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of key/value pairs in the source <see cref="IDictionary&lt;K,V&gt;"/>
        /// is greater than the available space from the <paramref name="index"/> to the end
        /// of the destination <paramref name="array"/>.
        /// </exception>
        void CopyTo(KeyValuePair<K, V>[] array, int index);

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
        /// <c>true</c> if <see cref="IDictionary&lt;K,V&gt;"/> contains an element with the
        /// specified <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        bool TryGetValue(K key, out V value);

        /// <summary>
        /// Removes the element with the specified <paramref name="key"/>
        /// from the <see cref="IDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element is successfully removed; otherwise, <c>false</c>.
        /// This method also returns false if the <paramref name="key"/> was not found in
        /// the original <see cref="IDictionary&lt;K,V&gt;"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IDictionary&lt;K,V&gt;"/> is read-only.
        /// </exception>
        bool Remove(K key);

        /// <summary>
        /// Removes all key/value pairs from the <see cref="IDictionary&lt;K,V&gt;"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Count"/> must be set to zero, and references to other objects
        /// from key/value pairs of the dictionary must be released.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IDictionary&lt;K,V&gt;"/> is read-only.
        /// </exception>
        void Clear();
    }
}