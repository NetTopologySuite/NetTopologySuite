using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Locate
{
    ///<summary>
    /// Computes the location of points relative to an areal <see cref="IGeometry"/>, using a simple O(n) algorithm.
    /// This algorithm is suitable for use in cases where only one or a few points will be tested against a given area.
    ///</summary>
    /// <remarks>The algorithm used is only guaranteed to return correct results for points which are <b>not</b> on the boundary of the Geometry.</remarks>
    public class SimplePointInAreaLocator : IPointOnGeometryLocator
    {
        /// <summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="IGeometry"/>.
        /// Computes <see cref="Location.Boundary"/> if the point lies exactly on a geometry line segment.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="geom">The areal geometry to test</param>
        /// <returns>The Location of the point in the geometry  </returns>
        public static Location Locate(Coordinate p, IGeometry geom)
        {
            if (geom.IsEmpty) return Location.Exterior;

            return LocateInGeometry(p, geom);
        }

        private static Location LocateInGeometry(Coordinate p, IGeometry geom)
        {
            if (geom is IPolygon)
                return LocatePointInPolygon(p, (IPolygon)geom);

            if (geom is IGeometryCollection)
            {
                var geomi = new GeometryCollectionEnumerator((IGeometryCollection)geom);
                while (geomi.MoveNext())
                {
                    var g2 = geomi.Current;
                    if (g2 != geom)
                    {
                        var loc = LocateInGeometry(p, g2);
                        if (loc != Location.Exterior) return loc;
                    }
                }
            }
            return Location.Exterior;
        }

        /// <summary>
        /// Determines the <see cref="Location"/> of a point in a <see cref="IPolygon"/>.
        /// Computes <see cref="Location.Boundary"/> if the point lies exactly
        /// on the polygon boundary.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="poly">The areal geometry to test</param>
        /// <returns>The Location of the point in the polygon</returns>
        public static Location LocatePointInPolygon(Coordinate p, IPolygon poly)
        {
            if (poly.IsEmpty) return Location.Exterior;
            var shell = (ILinearRing)poly.ExteriorRing;
            var shellLoc = LocatePointInRing(p, shell);
            if (shellLoc != Location.Interior) return shellLoc;
            // now test if the point lies in or on the holes
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = (ILinearRing)poly.GetInteriorRingN(i);
                var holeLoc = LocatePointInRing(p, hole);
                if (holeLoc == Location.Boundary) return Location.Boundary;
                if (holeLoc == Location.Interior) return Location.Exterior;
                // if in EXTERIOR of this hole keep checking the other ones
            }

            // If not in any hole must be inside polygon
            return Location.Interior;
        }

        /// <summary>
        /// Determines whether a point lies in a <see cref="IPolygon"/>.
        /// If the point lies on the polygon boundary it is
        /// considered to be inside.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="poly">The areal geometry to test</param>
        /// <returns><c>true</c> if the point lies in the polygon</returns>
        public static bool ContainsPointInPolygon(Coordinate p, IPolygon poly)
        {
            return Location.Exterior != LocatePointInPolygon(p, poly);
        }

        ///<summary>
        /// Determines whether a point lies in a LinearRing, using the ring envelope to short-circuit if possible.
        ///</summary>
        /// <param name="p">The point to test</param>
        /// <param name="ring">A linear ring</param>
        /// <returns><c>true</c> if the point lies inside the ring</returns>
        private static Location LocatePointInRing(Coordinate p, ILinearRing ring)
        {
            // short-circuit if point is not in ring envelope
            if (!ring.EnvelopeInternal.Intersects(p))
                return Location.Exterior;
            return PointLocation.LocateInRing(p, ring.CoordinateSequence);
        }

        private readonly IGeometry _geom;

        public SimplePointInAreaLocator(IGeometry geom)
        {
            _geom = geom;
        }

        public Location Locate(Coordinate p)
        {
            return Locate(p, _geom);
        }

    }
}