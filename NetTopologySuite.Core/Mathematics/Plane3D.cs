﻿using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Mathematics
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Enumeration for the 3 coordinate planes
    /// </summary>
    public enum Plane
    {
        Undefined = -1,
        XY = 1,
        YZ = 2,
        XZ = 3
    }
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Models a plane in 3-dimensional Cartesian space.
    /// </summary>
    /// <author>Martin Davis</author>
    public class Plane3D
    {
        private readonly Vector3D _normal;
        private readonly Coordinate _basePt;

        public Plane3D(Vector3D normal, Coordinate basePt)
        {
            _normal = normal;
            _basePt = basePt;
        }

        /// <summary>
        /// Computes the oriented distance from a point to the plane.<br/>
        /// The distance is:
        /// <list type="Bullet">
        /// <item><b>positive</b> if the point lies above the plane (relative to the plane normal)</item>
        /// <item><b>zero</b> if the point is on the plane</item>
        /// <item><b>negative</b> if the point lies below the plane (relative to the plane normal)</item>
        /// </list> 
        /// </summary>
        /// <param name="p">The point to compute the distance for</param>
        /// <returns>The oriented distance to the plane</returns>
        public double OrientedDistance(Coordinate p)
        {
            var pb = new Vector3D(p, _basePt);
            var pbdDotNormal = pb.Dot(_normal);
            if (Double.IsNaN(pbdDotNormal))
                throw new ArgumentException("3D Coordinate has NaN ordinate");
            var d = pbdDotNormal/_normal.Length();
            return d;
        }

        /// <summary>
        /// Computes the axis plane that this plane lies closest to.
        /// <para/>
        /// Geometries lying in this plane undergo least distortion
        /// (and have maximum area)
        /// when projected to the closest axis plane.
        /// This provides optimal conditioning for
        /// computing a Point-in-Polygon test.
        /// </summary>
        /// <returns>The index of the closest axis plane</returns>
        public Plane ClosestAxisPlane()
        {
            var xmag = Math.Abs(_normal.X);
            var ymag = Math.Abs(_normal.Y);
            var zmag = Math.Abs(_normal.Z);
            if (xmag > ymag)
            {
                if (xmag > zmag)
                    return Plane.YZ;
                return Plane.XY;
            }
            // y >= x
            if (zmag > ymag)
            {
                return Plane.XY;
            }
            // y >= z
            return Plane.XZ;
        }

    }
}