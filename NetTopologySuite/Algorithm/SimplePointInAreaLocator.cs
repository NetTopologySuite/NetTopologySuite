using System;
using System.Collections;
using System.Text;
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
        public static Locations Locate(Coordinate p, Geometry geom)
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
        private static bool ContainsPoint(Coordinate p, Geometry geom)
        {
            if(geom is Polygon) 
                return ContainsPointInPolygon(p, (Polygon)geom);            
            else if(geom is GeometryCollection) 
            {
                IEnumerator geomi = new GeometryCollectionEnumerator((GeometryCollection)geom);
                while(geomi.MoveNext()) 
                {
                    Geometry g2 = (Geometry)geomi.Current;
                    // if(g2 != geom)  --- Diego Guidi say's: Java code tests reference equality: in C# with operator overloads we tests the object.equals()... more slower!                    
                    if (!Object.ReferenceEquals(g2, geom)) 
                        if(ContainsPoint(p, g2))
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
        public static bool ContainsPointInPolygon(Coordinate p, Polygon poly)
        {
            if(poly.IsEmpty) 
                return false;
            LinearRing shell = (LinearRing)poly.ExteriorRing;
            if (!CGAlgorithms.IsPointInRing(p, shell.Coordinates)) 
                return false;
            // now test if the point lies in or on the holes
            for(int i = 0; i < poly.NumInteriorRings; i++)
            {
                LinearRing hole = (LinearRing)poly.GetInteriorRingN(i);
                if(CGAlgorithms.IsPointInRing(p, hole.Coordinates)) 
                    return false;
            }
            return true;
        }
    }
}
