using System;
using System.Collections;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Iterates over all <c>Geometry</c>'s in a <c>GeometryCollection</c>. 
    /// Implements a pre-order depth-first traversal of the <c>GeometryCollection</c>
    /// (which may be nested). The original <c>GeometryCollection</c> is
    /// returned as well (as the first object), as are all sub-collections. It is
    /// simple to ignore the <c>GeometryCollection</c> objects if they are not
    /// needed.
    /// </summary>    
    public class GeometryCollectionEnumerator : IEnumerator
    {
        /// <summary>
        /// The <c>GeometryCollection</c> being iterated over.
        /// </summary>
        private IGeometryCollection parent;

        /// <summary>
        /// Indicates whether or not the first element (the <c>GeometryCollection</c>)
        /// has been returned.
        /// </summary>
        private bool atStart;

        /// <summary>
        /// The number of <c>Geometry</c>s in the the <c>GeometryCollection</c>.
        /// </summary>
        private int max;

        /// <summary>
        /// The index of the <c>Geometry</c> that will be returned when <c>next</c>
        /// is called.
        /// </summary>
        private int index;

        /// <summary>
        /// The iterator over a nested <c>GeometryCollection</c>, or <c>null</c>
        /// if this <c>GeometryCollectionIterator</c> is not currently iterating
        /// over a nested <c>GeometryCollection</c>.
        /// </summary>
        private GeometryCollectionEnumerator subcollectionEnumerator;

        /// <summary>
        /// Constructs an iterator over the given <c>GeometryCollection</c>.
        /// </summary>
        /// <param name="parent">
        /// The collection over which to iterate; also, the first
        /// element returned by the iterator.
        /// </param>
        public GeometryCollectionEnumerator(IGeometryCollection parent) 
        {
            this.parent = parent;
            atStart = true;
            index = 0;
            max = parent.NumGeometries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext() 
        {
            if (atStart) 
                return true;
            if (subcollectionEnumerator != null) 
            {
                if (subcollectionEnumerator.MoveNext())  
                    return true;
                subcollectionEnumerator = null;
            }
            if (index >= max) 
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks> The parent GeometryCollection is the first object returned!</remarks>
        public object Current
        {
            get
            {
                // the parent GeometryCollection is the first object returned
                if (atStart) 
                {
                    atStart = false;
                    return parent;
                }
                if (subcollectionEnumerator != null) 
                {
                    if (subcollectionEnumerator.MoveNext()) 
                        return subcollectionEnumerator.Current;
                    else subcollectionEnumerator = null;
                }
                if (index >= max) 
                    throw new ArgumentOutOfRangeException(); 
                
                IGeometry obj = parent.GetGeometryN(index++);
                if (obj is IGeometryCollection) 
                {
                    subcollectionEnumerator = new GeometryCollectionEnumerator((IGeometryCollection) obj);
                    // there will always be at least one element in the sub-collection
                    return subcollectionEnumerator.Current;
                }
                return obj;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            atStart = true;
            index = 0;            
        }
    }    
}
