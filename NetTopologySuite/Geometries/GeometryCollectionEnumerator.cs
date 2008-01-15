using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Iterates over all <see cref="Geometry{TCoordinate}"/>'s in a <see cref="GeometryCollection{TCoordinate}" />. 
    /// Implements a pre-order depth-first traversal of the <see cref="GeometryCollection{TCoordinate}" />
    /// (which may be nested). The original <see cref="GeometryCollection{TCoordinate}" /> is
    /// returned as well (as the first object), as are all sub-collections. It is
    /// simple to ignore the <see cref="GeometryCollection{TCoordinate}" /> objects if they are not
    /// needed.
    /// </summary>    
    public class GeometryCollectionEnumerator<TCoordinate> : IEnumerator<IGeometry<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private Boolean _isDisposed = false;

        // The <see cref="GeometryCollection{TCoordinate}" /> being iterated over.
        private readonly IGeometryCollection<TCoordinate> _parent;

        // Indicates whether or not the first element 
        // (the <see cref="GeometryCollection{TCoordinate}" />)
        // has been returned.
        private Boolean _atStart;

        // The number of <see cref="Geometry{TCoordinate}"/>s in the
        // <see cref="GeometryCollection{TCoordinate}" />.
        private readonly Int32 _max;

        // The index of the <see cref="Geometry{TCoordinate}"/> that 
        // will be returned when <see cref="MoveNext"/> is called.
        private Int32 _index;

        // The iterator over a nested <see cref="GeometryCollection{TCoordinate}" />, or <see langword="null" />
        // if this <c>GeometryCollectionIterator</c> is not currently iterating
        // over a nested <see cref="GeometryCollection{TCoordinate}" />.
        private GeometryCollectionEnumerator<TCoordinate> _subcollectionEnumerator;

        /// <summary>
        /// Constructs an iterator over the given <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>
        /// <param name="parent">
        /// The collection over which to iterate; also, the first
        /// element returned by the iterator.
        /// </param>
        public GeometryCollectionEnumerator(IGeometryCollection<TCoordinate> parent)
        {
            _parent = parent;
            _atStart = true;
            _index = 0;
            _max = parent.Count;
        }

        public Boolean MoveNext()
        {
            checkDisposed();

            if (_atStart)
            {
                return true;
            }

            if (_subcollectionEnumerator != null)
            {
                if (_subcollectionEnumerator.MoveNext())
                {
                    return true;
                }

                _subcollectionEnumerator = null;
            }

            if (_index >= _max)
            {
                return false;
            }

            return true;
        }

        /// <remarks> 
        /// The parent <see cref="GeometryCollection{TCoordinatE}"/> is the 
        /// first object returned!
        /// </remarks>
        public IGeometry<TCoordinate> Current
        {
            get
            {
                checkDisposed();

                // the parent GeometryCollection is the first object returned
                if (_atStart)
                {
                    _atStart = false;
                    return _parent;
                }

                if (_subcollectionEnumerator != null)
                {
                    if (_subcollectionEnumerator.MoveNext())
                    {
                        return _subcollectionEnumerator.Current;
                    }
                    else
                    {
                        _subcollectionEnumerator = null;
                    }
                }

                if (_index >= _max)
                {
                    throw new ArgumentOutOfRangeException();
                }

                IGeometry<TCoordinate> obj = _parent[_index++];

                if (obj is IGeometryCollection<TCoordinate>)
                {
                    _subcollectionEnumerator = new GeometryCollectionEnumerator<TCoordinate>(obj as IGeometryCollection<TCoordinate>);
                    // there will always be at least one element in the sub-collection
                    return _subcollectionEnumerator.Current;
                }

                return obj;
            }
        }

        public void Reset()
        {
            checkDisposed();

            _atStart = true;
            _index = 0;
        }

        public Boolean IsDisposed
        {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        #endregion

        protected virtual void Dispose(Boolean disposing) { }

        private void checkDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }
        }
    }
}