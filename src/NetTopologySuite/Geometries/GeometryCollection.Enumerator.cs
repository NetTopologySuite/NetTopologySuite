#define Legacy
using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    public partial class GeometryCollection
    {
        /// <summary>
        /// Returns a <c>GeometryCollectionEnumerator</c>:
        /// this IEnumerator returns the parent geometry as first element.
        /// In most cases is more useful the code
        /// <c>geometryCollectionInstance.Geometries.GetEnumerator()</c>:
        /// this returns an IEnumerator over geometries composing GeometryCollection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Geometry> GetEnumerator()
        {
#if Legacy
            return new GeometryCollectionEnumerator(this);
#else
            return new SimpleGeometryArrayEnumerator(_geometries);
#endif
        }

#if !Legacy
        private class SimpleGeometryArrayEnumerator : IEnumerator<Geometry>
        {
            private readonly Geometry[] _geometries;
            private int _index;

            public SimpleGeometryArrayEnumerator(Geometry[] geometries)
            {
                _geometries = geometries;
                _index = -1;
            }

            public bool MoveNext()
            {
                // Increment index
                _index++;

                // Are there more members
                return _index < _geometries.Length;
            }

            public void Reset()
            {
                _index = -1;
            }

            public Geometry Current
            {
                get
                {
                    if (-1 < _index && _index < _geometries.Length)
                        return _geometries[_index];
                    return null;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
#endif
    }
}
