using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Locate
{
    /// <summary>
    /// Computes the location of points
    /// relative to an areal <see cref="Geometry"/>,
    /// using a simple <c>O(n)</c> algorithm.
    /// <para>
    /// The algorithm used reports
    /// if a point lies in the interior, exterior,
    /// or exactly on the boundary of the Geometry.
    /// </para>
    /// <para>
    /// Instance methods are provided to implement
    /// the interface <see cref="IPointOnGeometryLocator"/>.
    /// However, they provide no performance
    /// advantage over the class methods.
    /// </para>
    /// <para>
    /// This algorithm is suitable for use in cases where
    /// only a few points will be tested.
    /// If many points will be tested,
    /// <see cref="IndexedPointInAreaLocator"/> may provide better performance.
    /// </para>
    /// </summary>
    /// <remarks>The algorithm used is only guaranteed to return correct results for points which are <b>not</b> on the boundary of the Geometry.</remarks>
    public class SimplePointInAreaLocator : IPointOnGeometryLocator
    {
        /// <summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="Geometry"/>.
        /// The return value is one of:
        /// <list type="bullet">
        /// <item><term><see cref="Location.Interior"/></term><description>if the point is in the geometry interior</description></item>
        /// <item><term><see cref="Location.Boundary"/></term><description>if the point lies exactly on the boundary</description></item>
        /// <item><term><see cref="Location.Exterior"/></term><description>if the point is outside the geometry</description></item>
        /// </list>
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="geom">The areal geometry to test</param>
        /// <returns>The Location of the point in the geometry  </returns>
        public static Location Locate(Coordinate p, Geometry geom)
        {
            if (geom.IsEmpty) return Location.Exterior;

            // Do a fast check against the geometry envelope first
            if (!geom.EnvelopeInternal.Intersects(p))
            {
                return Location.Exterior;
            }

            return LocateInGeometry(p, geom);
        }

        /// <summary>
        /// Determines whether a point is contained in a <see cref="Geometry"/>,
        /// or lies on its boundary.
        /// This is a convenience method for
        /// <code>
        /// Location.Exterior != Locate(p, geom)
        /// </code>
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <param name="geom">The geometry to test.</param>
        /// <returns><see langword="true"/> if the point lies in or on the geometry.</returns>
        public static bool IsContained(Coordinate p, Geometry geom)
        {
            return Location.Exterior != Locate(p, geom);
        }

        private static Location LocateInGeometry(Coordinate p, Geometry geom)
        {
            if (geom is Polygon)
                return LocatePointInPolygon(p, (Polygon)geom);

            if (geom is GeometryCollection)
            {
                var geomi = new GeometryCollectionEnumerator((GeometryCollection)geom);
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

        private static Location LocatePointInPolygon(Coordinate p, Polygon poly)
        {
            if (poly.IsEmpty) return Location.Exterior;
            var shell = (LinearRing)poly.ExteriorRing;
            var shellLoc = LocatePointInRing(p, shell);
            if (shellLoc != Location.Interior) return shellLoc;
            // now test if the point lies in or on the holes
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = (LinearRing)poly.GetInteriorRingN(i);
                var holeLoc = LocatePointInRing(p, hole);
                if (holeLoc == Location.Boundary) return Location.Boundary;
                if (holeLoc == Location.Interior) return Location.Exterior;
                // if in EXTERIOR of this hole keep checking the other ones
            }

            // If not in any hole must be inside polygon
            return Location.Interior;
        }

        /// <summary>
        /// Determines whether a point lies in a <see cref="Polygon"/>.
        /// If the point lies on the polygon boundary it is
        /// considered to be inside.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="poly">The areal geometry to test</param>
        /// <returns><c>true</c> if the point lies in the polygon</returns>
        public static bool ContainsPointInPolygon(Coordinate p, Polygon poly)
        {
            return Location.Exterior != LocatePointInPolygon(p, poly);
        }

        /// <summary>
        /// Determines whether a point lies in a LinearRing, using the ring envelope to short-circuit if possible.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="ring">A linear ring</param>
        /// <returns><c>true</c> if the point lies inside the ring</returns>
        private static Location LocatePointInRing(Coordinate p, LinearRing ring)
        {
            // short-circuit if point is not in ring envelope
            if (!ring.EnvelopeInternal.Intersects(p))
                return Location.Exterior;
            return PointLocation.LocateInRing(p, ring.CoordinateSequence);
        }

        private readonly Geometry _geom;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePointInAreaLocator"/> class,
        /// using the provided areal geometry.
        /// </summary>
        /// <param name="geom">The areal geometry to locate in.</param>
        public SimplePointInAreaLocator(Geometry geom)
        {
            _geom = geom;
        }

        /// <summary>
        /// Determines the <see cref="Location"/> of a point in an areal <see cref="Geometry"/>.
        /// The return value is one of:
        /// <list type="bullet">
        /// <item><term><see cref="Location.Interior"/></term><description>if the point is in the geometry interior</description></item>
        /// <item><term><see cref="Location.Boundary"/></term><description>if the point lies exactly on the boundary</description></item>
        /// <item><term><see cref="Location.Exterior"/></term><description>if the point is outside the geometry</description></item>
        /// </list>
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <returns>The Location of the point in the geometry.</returns>
        public Location Locate(Coordinate p)
        {
            return Locate(p, _geom);
        }

    }
}
