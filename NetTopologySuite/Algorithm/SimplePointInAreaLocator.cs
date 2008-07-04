using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes whether a point
    /// lies in the interior of an area <c>Geometry</c>.
    /// The algorithm used is only guaranteed to return correct results
    /// for points which are not on the boundary of the Geometry.
    /// </summary>
    public class SimplePointInAreaLocator
    {
        /// <summary>
        /// 
        /// </summary>
        private SimplePointInAreaLocator() { }

        /// <summary> 
        /// Locate is the main location function.  It handles both single-element
        /// and multi-element Geometries.  The algorithm for multi-element Geometries
        /// is more complex, since it has to take into account the boundaryDetermination rule.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="p"></param>
        public static Locations Locate(ICoordinate p, IGeometry geom)
        {
            if (geom.IsEmpty)
                return Locations.Exterior;

            if (ContainsPoint(p, geom))
                return Locations.Interior;
            return Locations.Exterior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="geom"></param>
        /// <returns></returns>
        private static bool ContainsPoint(ICoordinate p, IGeometry geom)
        {
            if (geom is IPolygon) 
                return ContainsPointInPolygon(p, (IPolygon) geom);            
            else if(geom is IGeometryCollection) 
            {
                IEnumerator geomi = new GeometryCollectionEnumerator((IGeometryCollection) geom);
                while (geomi.MoveNext()) 
                {
                    IGeometry g2 = (IGeometry) geomi.Current;
                    // if(g2 != geom)  --- Diego Guidi say's: Java code tests reference equality: in C# with operator overloads we tests the object.equals()... more slower!                    
                    if (!ReferenceEquals(g2, geom)) 
                        if (ContainsPoint(p, g2))
                            return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static bool ContainsPointInPolygon(ICoordinate p, IPolygon poly)
        {
            if (poly.IsEmpty) 
                return false;
            ILinearRing shell = (ILinearRing) poly.ExteriorRing;
            if (!CGAlgorithms.IsPointInRing(p, shell.Coordinates)) 
                return false;
            // now test if the point lies in or on the holes
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                ILinearRing hole = (ILinearRing) poly.GetInteriorRingN(i);
                if (CGAlgorithms.IsPointInRing(p, hole.Coordinates)) 
                    return false;
            }
            return true;
        }
    }
}
