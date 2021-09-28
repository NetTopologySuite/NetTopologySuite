using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Distance3D
{
    /// <summary>
    /// Models a polygon lying in a plane in 3-dimensional Cartesian space.
    /// The polygon representation is supplied
    /// by a <see cref="Polygon"/>,
    /// containing coordinates with XYZ ordinates.
    /// 3D polygons are assumed to lie in a single plane.
    /// The plane best fitting the polygon coordinates is
    /// computed and is represented by a <see cref="Plane3D"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PlanarPolygon3D
    {

        private readonly Plane3D _plane;
        private readonly Polygon _poly;
        private readonly Plane _facingPlane;

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="Polygon"/>.
        /// </summary>
        /// <param name="poly">The polygon</param>
        public PlanarPolygon3D(Polygon poly)
        {
            _poly = poly;
            _plane = FindBestFitPlane(poly);
            _facingPlane = _plane.ClosestAxisPlane();
        }

        /// <summary>
        /// Finds a best-fit plane for the polygon,
        /// by sampling a few points from the exterior ring.
        /// <para/>
        /// The algorithm used is Newell's algorithm:
        /// <list type="bullet">
        /// <item><description>a base point for the plane is determined from the average of all vertices</description></item>
        /// <item><description>the normal vector is determined by computing the area of the projections on each of the axis planes</description></item>
        /// </list>
        /// </summary>
        /// <param name="poly">The polygon to determine the plane for</param>
        /// <returns>The best-fit plane</returns>
        private static Plane3D FindBestFitPlane(Polygon poly)
        {
            var seq = poly.ExteriorRing.CoordinateSequence;
            var basePt = AveragePoint(seq);
            var normal = AverageNormal(seq);
            return new Plane3D(normal, basePt);
        }

        /// <summary>
        /// Computes an average normal vector from a list of polygon coordinates.
        /// Uses Newell's method, which is based
        /// on the fact that the vector with components
        /// equal to the areas of the projection of the polygon onto
        ///  the Cartesian axis planes is normal.
        /// </summary>
        /// <param name="seq">The sequence of coordinates for the polygon</param>
        /// <returns>A normal vector</returns>
        private static Vector3D AverageNormal(CoordinateSequence seq)
        {
            int n = seq.Count;
            var sum = new CoordinateZ(0, 0, 0);
            var p1 = new CoordinateZ(0, 0, 0);
            var p2 = new CoordinateZ(0, 0, 0);
            for (int i = 0; i < n - 1; i++)
            {
                seq.GetCoordinate(i, p1);
                seq.GetCoordinate(i + 1, p2);
                sum.X += (p1.Y - p2.Y) * (p1.Z + p2.Z);
                sum.Y += (p1.Z - p2.Z) * (p1.X + p2.X);
                sum.Z += (p1.X - p2.X) * (p1.Y + p2.Y);
            }
            sum.X /= n;
            sum.Y /= n;
            sum.Z /= n;
            var norm = Vector3D.Create(sum).Normalize();
            return norm;
        }

        /// <summary>
        /// Computes a point which is the average of all coordinates
        /// in a sequence.<br/>
        /// If the sequence lies in a single plane,
        /// the computed point also lies in the plane.
        /// </summary>
        /// <param name="seq">A coordinate sequence</param>
        /// <returns>A Coordinate with averaged ordinates </returns>
        private static Coordinate AveragePoint(CoordinateSequence seq)
        {
            var a = new CoordinateZ(0, 0, 0);
            int n = seq.Count;
            for (int i = 0; i < n; i++)
            {
                a.X += seq.GetOrdinate(i, 0);
                a.Y += seq.GetOrdinate(i, 1);
                a.Z += seq.GetOrdinate(i, 2);
            }
            a.X /= n;
            a.Y /= n;
            a.Z /= n;
            return a;
        }

        /// <summary>
        /// Gets a value indicating the plane
        /// </summary>
        public Plane3D Plane => _plane;

        /// <summary>
        /// Gets a value indicating the polygon
        /// </summary>
        public Polygon Polygon => _poly;

        /// <summary>
        /// Checks if <paramref name="intPt"/> intersects with this <see cref="PlanarPolygon3D"/>.
        /// </summary>
        /// <param name="intPt">The point to check</param>
        /// <returns><c>true</c> if <c>intPt</c> intersects this <c>PlanarPolygon3d</c>.</returns>
        public bool Intersects(Coordinate intPt)
        {
            if (Location.Exterior == Locate(intPt, _poly.ExteriorRing))
                return false;

            for (int i = 0; i < _poly.NumInteriorRings; i++)
            {
                if (Location.Interior == Locate(intPt, _poly.GetInteriorRingN(i)))
                    return false;
            }
            return true;
        }

        private Location Locate(Coordinate pt, LineString ring)
        {
            var seq = ring.CoordinateSequence;
            var seqProj = Project(seq, _facingPlane);
            var ptProj = Project(pt, _facingPlane);
            return RayCrossingCounter.LocatePointInRing(ptProj, seqProj);
        }

        /// <summary>
        /// Checks if the point <paramref name="pt"/> intersects with <paramref name="ring"/> when projected to this instance's facing plane
        /// </summary>
        /// <param name="pt">A point</param>
        /// <param name="ring">A ring</param>
        /// <returns><c>true</c> if point and linestring intersect</returns>
        public bool Intersects(Coordinate pt, LineString ring)
        {
            if (!ring.IsClosed)
                throw new ArgumentException("Not forming a ring.", nameof(ring));

            var seq = ring.CoordinateSequence;
            var seqProj = Project(seq, _facingPlane);
            var ptProj = Project(pt, _facingPlane);
            return Location.Exterior != RayCrossingCounter.LocatePointInRing(ptProj, seqProj);
        }

        private static CoordinateSequence Project(CoordinateSequence seq, Plane facingPlane)
        {
            switch (facingPlane)
            {
                case Mathematics.Plane.XY: return AxisPlaneCoordinateSequence.ProjectToXY(seq);
                case Mathematics.Plane.XZ: return AxisPlaneCoordinateSequence.ProjectToXZ(seq);
                default: return AxisPlaneCoordinateSequence.ProjectToYZ(seq);
            }
        }

        private static Coordinate Project(Coordinate p, Plane facingPlane)
        {
            switch (facingPlane)
            {
                case Mathematics.Plane.XY: return new Coordinate(p.X, p.Y);
                case Mathematics.Plane.XZ: return new Coordinate(p.X, p.Z);
                // Plane3D.YZ
                default: return new Coordinate(p.Y, p.Z);
            }
        }

    }
}
