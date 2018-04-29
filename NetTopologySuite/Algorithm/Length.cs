﻿using System;
using GeoAPI.Geometries;
namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions for computing length.
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    public class Length
    {
        /// <summary>
        /// Computes the length of a <c>LineString</c> specified by a sequence of points.
        /// </summary>
        /// <param name="pts">The points specifying the <c>LineString</c></param>
        /// <returns>The length of the <c>LineString</c></returns>
        public static double OfLine(ICoordinateSequence pts)
        {
            // optimized for processing CoordinateSequences
            var n = pts.Count;
            if (n <= 1)
                return 0.0;
            var len = 0.0;
            var p = new Coordinate();
            pts.GetCoordinate(0, p);
            var x0 = p.X;
            var y0 = p.Y;
            for (var i = 1; i < n; i++)
            {
                pts.GetCoordinate(i, p);
                var x1 = p.X;
                var y1 = p.Y;
                var dx = x1 - x0;
                var dy = y1 - y0;
                len += Math.Sqrt(dx * dx + dy * dy);
                x0 = x1;
                y0 = y1;
            }
            return len;
        }
    }
}
