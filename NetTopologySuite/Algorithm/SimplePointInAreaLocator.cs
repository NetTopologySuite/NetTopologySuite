using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes whether a point
    /// lies in the interior of an area <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// The algorithm used is only guaranteed to return correct results
    /// for points which are not on the boundary of the Geometry.
    /// </remarks>
    public class SimplePointInAreaLocator<TCoordinate> : IPointInAreaLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Locate is the main location function.  It handles both single-element
        /// and multi-element <see cref="IGeometry{TCoordinate}"/> instances.
        /// The algorithm for multi-element geometries is more complex, 
        /// since it has to take into account the boundary determination rule.
        /// </summary>
        public static Locations Locate(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            if (geom.IsEmpty)
            {
                return Locations.Exterior;
            }

            if (ContainsPoint(p, geom))
            {
                return Locations.Interior;
            }

            return Locations.Exterior;
        }

        private static Boolean IsPointInRing(TCoordinate p, ILinearRing<TCoordinate> ring)
        {
            if ( !ring.Extents.Intersects(p) )
                return false;

            return CGAlgorithms<TCoordinate>.IsPointInRing(p, ring.Coordinates);
        }

        public static Boolean ContainsPointInPolygon(TCoordinate p, IPolygon<TCoordinate> poly)
        {
            if (poly.IsEmpty)
            {
                return false;
            }

            ILinearRing<TCoordinate> shell = (ILinearRing<TCoordinate>) poly.ExteriorRing;
            if (!IsPointInRing( p, shell ))
                return false;

            // now test if the point lies in or on the holes
            foreach (ILinearRing<TCoordinate> hole in poly.InteriorRings)
            {
                if (IsPointInRing(p, hole))
                    return false;
            }

            return true;
        }

        public static Boolean ContainsPoint(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            if (geom is IPolygon<TCoordinate>)
            {
                return ContainsPointInPolygon(p, (IPolygon<TCoordinate>) geom);
            }

            if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> collection = geom as IGeometryCollection<TCoordinate>;
                Debug.Assert(collection != null);

                IEnumerator<IGeometry<TCoordinate>> geometryEnumerator
                    = new GeometryCollectionEnumerator<TCoordinate>(collection);

                while (geometryEnumerator.MoveNext())
                {
                    IGeometry<TCoordinate> g2 = geometryEnumerator.Current;
                    // if(g2 != geom)  
                    // ---  Diego Guidi says: Java's "!=" operator tests reference equality: 
                    //      in C# with operator overloads it tests using Object.Equals()... slower!
                    if (!ReferenceEquals(g2, geom) && ContainsPoint(p, g2))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private IGeometry<TCoordinate> _geom;

        public SimplePointInAreaLocator(IGeometry<TCoordinate> geom)
        {
            _geom = geom;
        }

        public Locations Locate(TCoordinate p)
        {
            return Locate(p, _geom);
        }

    }
}