using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Iterates over all <c>Geometry</c>'s in a <c>GeometryCollection</c>.
    /// Implements a pre-order depth-first traversal of the <c>GeometryCollection</c>
    /// (which may be nested). The original <c>GeometryCollection</c> is
    /// returned as well (as the first object), as are all sub-collections. It is
    /// simple to ignore the <c>GeometryCollection</c> objects if they are not
    /// needed.
    /// </summary>
    public class GeometryCollectionEnumerator : IEnumerator<Geometry>, IEnumerable<Geometry>
    {
        /// <summary>
        /// The <c>GeometryCollection</c> being iterated over.
        /// </summary>
        private readonly Geometry _parent;

        /// <summary>
        /// Indicates whether or not the first element (the <c>GeometryCollection</c>)
        /// has been returned.
        /// </summary>
        private bool _atStart;

        /// <summary>
        /// The number of <c>Geometry</c>s in the the <c>GeometryCollection</c>.
        /// </summary>
        private readonly int _max;

        /// <summary>
        /// The index of the <c>Geometry</c> that will be returned when <c>next</c>
        /// is called.
        /// </summary>
        private int _index;

        /// <summary>
        /// The iterator over a nested <c>GeometryCollection</c>, or <c>null</c>
        /// if this <c>GeometryCollectionIterator</c> is not currently iterating
        /// over a nested <c>GeometryCollection</c>.
        /// </summary>
        private GeometryCollectionEnumerator _subcollectionEnumerator;

        private Geometry _current = null;

        /// <summary>
        /// Constructs an iterator over the given <c>GeometryCollection</c>.
        /// </summary>
        /// <param name="parent">
        /// The collection over which to iterate; also, the first
        /// element returned by the iterator.
        /// </param>
        public GeometryCollectionEnumerator(Geometry parent)
        {
            _parent = parent;
            _atStart = true;
            _index = 0;
            _max = parent.NumGeometries;
        }

        private bool HasNext()
        {
            if (_atStart)
                return true;
            if (_subcollectionEnumerator != null)
            {
                if (_subcollectionEnumerator.HasNext())
                    return true;
                _subcollectionEnumerator = null;
            }
            if (_index >= _max)
                return false;
            return true;
        }

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>>
        public bool MoveNext()
        {
            if (!HasNext())
            {
                _current = null;
                return false;
            }

            // the parent GeometryCollection is the first object returned
            if (_atStart)
            {
                _atStart = false;
                if (IsAtomic(_parent))
                    _index++;
                _current = _parent;
                return true;
            }
            if (_subcollectionEnumerator != null)
            {
                if (_subcollectionEnumerator.MoveNext())
                {
                    _current = _subcollectionEnumerator.Current;
                    return true;
                }
                _subcollectionEnumerator = null;
            }
            if (_index >= _max)
                throw new ArgumentOutOfRangeException();

            var obj = _parent.GetGeometryN(_index++);
            if (obj is GeometryCollection gc)
            {
                _subcollectionEnumerator = new GeometryCollectionEnumerator(gc);
                // there will always be at least one element in the sub-collection
                _subcollectionEnumerator.MoveNext();
                _current = _subcollectionEnumerator.Current;
            }
            else
                _current = obj;

            return true;

        }

        /// <inheritdoc cref="System.Collections.IEnumerator.Current"/>>
        /// <remarks> The parent GeometryCollection is the first object returned!</remarks>
        object System.Collections.IEnumerator.Current => Current;

        /// <inheritdoc cref="System.Collections.IEnumerator.Reset"/>
        public void Reset()
        {
            _atStart = true;
            _index = 0;
            _subcollectionEnumerator = null;
            _current = null;
        }

        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public Geometry Current => _current;

        private static bool IsAtomic(Geometry geom)
        {
            return !(geom is GeometryCollection);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (_subcollectionEnumerator != null)
                _subcollectionEnumerator.Dispose();
        }

        #region Implementation of IEnumerable

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public IEnumerator<Geometry> GetEnumerator()
        {
            return this;
        }

        /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
