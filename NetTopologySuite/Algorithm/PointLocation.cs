﻿using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
     /// <summary>
     /// Functions for locating points within basic geometric
     /// structures such as lines and rings.
     /// </summary>
     /// <author>Martin Davis</author>
    public static class PointLocation
    {
        /// <summary>
        /// Tests whether a point lies on the line defined by a list of
        /// coordinates.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="line">The line coordinates</param>
        /// <returns>
        /// <c>true</c> if the point is a vertex of the line or lies in the interior
        /// of a line segment in the line
        /// </returns>
        public static bool IsOnLine(Coordinate p, Coordinate[] line)
        {
            var lineIntersector = new RobustLineIntersector();
            for (var i = 1; i < line.Length; i++)
            {
                var p0 = line[i - 1];
                var p1 = line[i];
                lineIntersector.ComputeIntersection(p, p0, p1);
                if (lineIntersector.HasIntersection)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether a point lies on the line defined by a list of
        /// coordinates.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="line">The line coordinates</param>
        /// <returns>
        /// <c>true</c> if the point is a vertex of the line or lies in the interior
        /// of a line segment in the line
        /// </returns>
        public static bool IsOnLine(Coordinate p, ICoordinateSequence line)
        {
            var lineIntersector = new RobustLineIntersector();
            var p0 = new Coordinate();
            var p1 = new Coordinate();
            int n = line.Count;
            for (int i = 1; i < n; i++)
            {
                line.GetCoordinate(i - 1, p0);
                line.GetCoordinate(i, p1);
                lineIntersector.ComputeIntersection(p, p0, p1);
                if (lineIntersector.HasIntersection)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether a point lies inside or on a ring. The ring may be oriented in
        /// either direction. A point lying exactly on the ring boundary is considered
        /// to be inside the ring.
        /// <para/>
        /// This method does <i>not</i> first check the point against the envelope of
        /// the ring.
        /// </summary>
        /// <param name="p">The point to check for ring inclusion</param>
        /// <param name="ring">An array of coordinates representing the ring (which must have
        /// first point identical to last point)</param>
        /// <returns><c>true</c> if p is inside ring</returns>
        public static bool IsInRing(Coordinate p, Coordinate[] ring) => LocateInRing(p, ring) != Location.Exterior;

        /// <summary>
        /// Tests whether a point lies inside or on a ring. The ring may be oriented in
        /// either direction. A point lying exactly on the ring boundary is considered
        /// to be inside the ring.
        /// <para/>
        /// This method does <i>not</i> first check the point against the envelope of
        /// the ring.
        /// </summary>
        /// <param name="p">The point to check for ring inclusion</param>
        /// <param name="ring">A <c>CoordinateSequence</c> representing the ring (which must have
        /// first point identical to last point)</param>
        /// <returns><c>true</c> if p is inside ring</returns>
        public static bool IsInRing(Coordinate p, ICoordinateSequence ring)
            => LocateInRing(p, ring) != Location.Exterior;

        /// <summary>
        /// Determines whether a point lies in the interior, on the boundary, or in the
        /// exterior of a ring.The ring may be oriented in either direction.
        /// <para/>
        /// This method does<i> not</i> first check the point against the envelope of
        /// the ring.
        /// </summary>
        /// <param name="p">The point to check for ring inclusion</param>
        /// <param name="ring">A <c>CoordinateSequence</c> representing the ring (which must have
        /// first point identical to last point)</param>
        /// <returns>the <see cref="Location"/> of p relative to the ring</returns>
        public static Location LocateInRing(Coordinate p, Coordinate[] ring)
            => RayCrossingCounter.LocatePointInRing(p, ring);

        /// <summary>
        /// Determines whether a point lies in the interior, on the boundary, or in the
        /// exterior of a ring.The ring may be oriented in either direction.
        /// <para/>
        /// This method does<i> not</i> first check the point against the envelope of
        /// the ring.
        /// </summary>
        /// <param name="p">The point to check for ring inclusion</param>
        /// <param name="ring">A <c>CoordinateSequence</c> representing the ring (which must have
        /// first point identical to last point)</param>
        public static Location LocateInRing(Coordinate p, ICoordinateSequence ring)
            => RayCrossingCounter.LocatePointInRing(p, ring);
    }
}
