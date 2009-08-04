//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

using System;
using System.Collections.Generic;

namespace Goletas.Collections
{
    /// <summary>
    /// Represents a general-purpose collection.
    /// </summary>
    /// <remarks>
    /// The <see cref="ICollection&lt;T&gt;"/> is the most general
    /// interface in the <see cref="Goletas.Collections"/> hierarchy.
    /// Implementations can vary in whether they allow the items in the
    /// collection to be a <c>null</c> reference.
    /// </remarks>
    /// <typeparam name="T">
    /// The element type of the <see cref="ICollection&lt;T&gt;"/>.
    /// </typeparam>
    public interface ICollection<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection&lt;T&gt;"/>
        /// is read-only.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <see cref="ICollection&lt;T&gt;"/> is read-only;
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// A collection that is read-only does not allow the addition, removal or
        /// modification of elements after the collection is created.
        /// </remarks>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection&lt;T&gt;"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="ICollection&lt;T&gt;"/>.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Adds the <paramref name="item"/> to the <see cref="ICollection&lt;T&gt;"/>.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="ICollection&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="item"/> was successfully added to the
        /// <see cref="ICollection&lt;T&gt;"/>; otherwise, <c>false</c>.
        /// This method also returns <c>false</c> if a particular
        /// <see cref="ICollection&lt;T&gt;"/> implementation supports only unique
        /// elements and the collection already contains the specified <paramref name="item"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection&lt;T&gt;"/> is read-only.
        /// </exception>
        bool Add(T item);

        /// <summary>
        /// Determines whether the <see cref="ICollection&lt;T&gt;"/> contains
        /// a specific value.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="ICollection&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="item"/> is found in the
        /// <see cref="ICollection&lt;T&gt;"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Implementations can vary in how they determine equality of objects.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is a <c>null</c> reference.
        /// </exception>
        bool Contains(T item);

        /// <summary>
        /// Copies the <see cref="ICollection&lt;T&gt;"/> elements to an existing
        /// one-dimensional <see cref="Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the
        /// elements copied from this <see cref="ICollection&lt;T&gt;"/>.
        /// The <paramref name="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in the <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside of the <paramref name="array"/> bounds.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ICollection&lt;T&gt;"/>
        /// is greater than the available space from the <paramref name="index"/> to the end
        /// of the destination <paramref name="array"/>.
        /// </exception>
        void CopyTo(T[] array, int index);

        /// <summary>
        /// Removes the <paramref name="item"/> from the <see cref="ICollection&lt;T&gt;"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ICollection&lt;T&gt;"/>.
        /// </param>
        /// <returns>
        /// <para>
        /// <c>true</c> if the <paramref name="item"/> was successfully removed from the
        /// <see cref="ICollection&lt;T&gt;"/>; otherwise, <c>false</c>. This method
        /// also returns <c>false</c> if the <paramref name="item"/> is not found
        /// in the original <see cref="ICollection&lt;T&gt;"/>.
        /// </para>
        /// <para>
        /// Implementations can vary in how they deal with single or multiple occurences
        /// of an element in the <see cref="ICollection&lt;T&gt;"/> and in which order
        /// the elements are removed.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is a <c>null</c> reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection&lt;T&gt;"/> is read-only.
        /// </exception>
        bool Remove(T item);

        /// <summary>
        /// Removes all items from the <see cref="ICollection&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Count"/> must be set to zero, and references to other objects
        /// from elements of the collection must be released.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection&lt;T&gt;"/> is read-only.
        /// </exception>
        void Clear();
    }
}