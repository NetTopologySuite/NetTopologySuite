using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes whether a point
    /// lies in the interior of an area <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// The algorithm used is only guaranteed to return correct results
    /// for points which are not on the boundary of the Geometry.
    /// </remarks>
    public static class SimplePointInAreaLocator
    {
        /// <summary> 
        /// Locate is the main location function.  It handles both single-element
        /// and multi-element <see cref="IGeometry{TCoordinate}"/> instances.
        /// The algorithm for multi-element geometries is more complex, 
        /// since it has to take into account the boundary determination rule.
        /// </summary>
        public static Locations Locate<TCoordinate>(TCoordinate p, IGeometry<TCoordinate> geom)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
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

        public static Boolean ContainsPointInPolygon<TCoordinate>(TCoordinate p, IPolygon<TCoordinate> poly)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
        {
            if (poly.IsEmpty)
            {
                return false;
            }

            ILinearRing<TCoordinate> shell = (ILinearRing<TCoordinate>) poly.ExteriorRing;

            if (!CGAlgorithms<TCoordinate>.IsPointInRing(p, shell.Coordinates))
            {
                return false;
            }

            // now test if the point lies in or on the holes
            foreach (ILinearRing<TCoordinate> hole in poly.InteriorRings)
            {
                if (CGAlgorithms<TCoordinate>.IsPointInRing(p, hole.Coordinates))
                {
                    return false;
                }
            }

            return true;
        }

        public static Boolean ContainsPoint<TCoordinate>(TCoordinate p, IGeometry<TCoordinate> geom)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
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
    }
}