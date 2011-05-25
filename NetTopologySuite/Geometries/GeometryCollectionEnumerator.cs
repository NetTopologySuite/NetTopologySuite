using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

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
    public class GeometryCollectionEnumerator : IEnumerator<IGeometry>
    {
        /// <summary>
        /// The <c>GeometryCollection</c> being iterated over.
        /// </summary>
        private readonly IGeometryCollection _parent;

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

        /// <summary>
        /// Constructs an iterator over the given <c>GeometryCollection</c>.
        /// </summary>
        /// <param name="parent">
        /// The collection over which to iterate; also, the first
        /// element returned by the iterator.
        /// </param>
        public GeometryCollectionEnumerator(IGeometryCollection parent) 
        {
            _parent = parent;
            _atStart = true;
            _index = 0;
            _max = parent.NumGeometries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext() 
        {
            if (_atStart) 
                return true;
            if (_subcollectionEnumerator != null) 
            {
                if (_subcollectionEnumerator.MoveNext())  
                    return true;
                _subcollectionEnumerator = null;
            }
            if (_index >= _max) 
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks> The parent GeometryCollection is the first object returned!</remarks>
        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _atStart = true;
            _index = 0;            
        }

        public IGeometry Current
        {
            get
            {
                // the parent GeometryCollection is the first object returned
                if (_atStart)
                {
                    _atStart = false;
                    return _parent;
                }
                if (_subcollectionEnumerator != null)
                {
                    if (_subcollectionEnumerator.MoveNext())
                        return _subcollectionEnumerator.Current;
                    _subcollectionEnumerator = null;
                }
                if (_index >= _max)
                    throw new ArgumentOutOfRangeException();

                IGeometry obj = _parent.GetGeometryN(_index++);
                if (obj is IGeometryCollection)
                {
                    _subcollectionEnumerator = new GeometryCollectionEnumerator((IGeometryCollection)obj);
                    // there will always be at least one element in the sub-collection
                    return _subcollectionEnumerator.Current;
                }
                return obj;
            }
        }

        public void Dispose()
        {
            if (_subcollectionEnumerator != null)
                _subcollectionEnumerator.Dispose();
        }
    }    
}
