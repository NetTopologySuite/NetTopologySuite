using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the location of points
    /// relative to a <see cref="IPolygonal"/> <see cref="IGeometry"/>,
    /// using a simple O(n) algorithm.
    /// This algorithm is suitable for use in cases where
    /// only one or a few points will be tested against a given area.
    /// </summary>
    /// <remarks>
    /// The algorithm used is only guaranteed to return correct results
    /// for points which are not on the boundary of the Geometry.
    /// </remarks>
    public class SimplePointInAreaLocator : IPointInAreaLocator
    {
        /// <summary> 
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="IGeometry"/>.
        /// Currently this will never return a value of <see cref ="Location.Boundary"/>.  
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="geom">the areal geometry to test</param>
        /// <returns>The Location of the point in the geometry</returns>
        public static Location Locate(ICoordinate p, IGeometry geom)
        {
            if (geom.IsEmpty)
                return Location.Exterior;

            if (ContainsPoint(p, geom))
                return Location.Interior;
            return Location.Exterior;
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
            
            if(geom is IGeometryCollection) 
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
            if (!IsPointInRing(p, shell)) return false;
            // now test if the point lies in or on the holes
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                ILinearRing hole = (ILinearRing) poly.GetInteriorRingN(i);
                if (IsPointInRing(p, hole)) return false;
            }
            return true;
        }

        ///<summary>
        /// Determines whether a point lies in a LinearRing, using the ring envelope to short-circuit if possible.
        ///</summary>
        /// <param name="p">The point to test</param>
        /// <param name="ring">A linear ring</param>
        /// <returns>true if the point lies inside the ring</returns>
        private static bool IsPointInRing(ICoordinate p, ILinearRing ring)
        {
            // short-circuit if point is not in ring envelope
            if (!ring.EnvelopeInternal.Intersects(p))
                return false;
            return CGAlgorithms.IsPointInRing(p, ring.Coordinates);
        }

        private readonly Geometry _geom;

        public SimplePointInAreaLocator(Geometry geom)
        {
            _geom = geom;
        }

        public Location Locate(ICoordinate p)
        {
            return Locate(p, _geom);
        }

    }
}
