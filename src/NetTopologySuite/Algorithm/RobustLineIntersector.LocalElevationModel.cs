using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using System;

namespace NetTopologySuite.Algorithm
{
    public partial class RobustLineIntersector
    {
        /// <summary>
        /// A local elevation model.
        /// This mimics the functionality that was previously hard coded into <see cref="RobustLineIntersector"/>.
        /// </summary>
        private class LocalElevationModel : ElevationModel
        {
            private readonly Coordinate _p1, _p2, _q1, _q2;
            private readonly OrientationIndex _Pq1, _Pq2, _Qp1, _Qp2;
            private readonly bool _collinear;
            private readonly bool _hasZ;

            public LocalElevationModel(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2,
                OrientationIndex Pq1, OrientationIndex Pq2, OrientationIndex Qp1, OrientationIndex Qp2,
                bool collinear)
            {
                _p1 = p1; _p2 = p2;
                _q1 = q1; _q2 = q2;
                _Pq1 = Pq1; _Pq2 = Pq2;
                _Qp1 = Qp1; _Qp2 = Qp2;
                _collinear = collinear;

                // Flag indicating if any of the relevant coordinates has a z-ordinage value
                _hasZ = !double.IsNaN(_p1.Z) || !double.IsNaN(_p2.Z) ||
                        !double.IsNaN(_q1.Z) || !double.IsNaN(_q2.Z);
            }

            /* 
             * For the computation of collinear z-ordinates then values of
             * _Pq1, _Pq2, _Qp1 and _Qp2 are used to store the boolean values 
             * for _q1InP, q2inP, _p1inQ and _p2inQ
             */
            private bool _q1inP => _Pq1 == OrientationIndex.Clockwise;

            private bool _q2inP => _Pq2 == OrientationIndex.Clockwise;

            private bool _p1inQ => _Qp1 == OrientationIndex.Clockwise;

            private bool _p2inQ => _Qp2 == OrientationIndex.Clockwise;

            /// <inheritdoc/>
            public override double GetZ(Coordinate c)
            {
                if (!_hasZ) return Coordinate.NullOrdinate;

                if (_collinear)
                {
                    if (_q1inP && _q2inP)
                        return GetZOrInterpolate(c, _p1, _p2);
                    if (_p1inQ && _p2inQ)
                        return GetZOrInterpolate(c, _q1, _q2);
                    if ((_q1inP && _p1inQ) || (_q1inP && _p2inQ))
                        return c.Equals2D(_q1)
                            ? GetZOrInterpolate(c, _p1, _p2)
                            : GetZOrInterpolate(c, _q1, _q2);
                    if ((_q2inP && _p1inQ) || (_q2inP && _p2inQ))
                        return c.Equals2D(_q2)
                            ? GetZOrInterpolate(c, _p1, _p2)
                            : GetZOrInterpolate(c, _q1, _q2);
                }
                else
                {
                    if (_Pq1 == 0 || _Pq2 == 0 || _Qp1 == 0 || _Qp2 == 0)
                    {
                        if (_p1.Equals2D(_q1))
                            return GetZFromEither(_p1, _q1);
                        if (_p1.Equals2D(_q2))
                            return GetZFromEither(_p1, _q2);
                        if (_p2.Equals2D(_q1))
                            return GetZFromEither(_p2, _q1);
                        if (_p2.Equals2D(_q2))
                            return GetZFromEither(_p2, _q2);
                        if (_Pq1 == 0)
                            return GetZOrInterpolate(_q1, _p1, _p2);
                        if (_Pq2 == 0)
                            return GetZOrInterpolate(_q2, _p1, _p2);
                        if (_Qp1 == 0)
                            return GetZOrInterpolate(_p1, _q1, _q2);
                        if (_Qp2 == 0)
                            return GetZOrInterpolate(_p2, _q1, _q2);
                    }
                    else
                    {
                        return InterpolateZ(c, _p1, _p2, _q1, _q2);
                    }
                }

                // This should not happen whatsoever
                Assert.ShouldNeverReachHere();

                return Coordinate.NullOrdinate;
            }

            /// <summary>
            /// Not supported
            /// </summary>
            public override double GetZ(double x, double y) => throw new NotSupportedException();

            /// <summary>
            /// Not supported
            /// </summary>
            public override void GetZ(ReadOnlySpan<double> xy, Span<double> z) => throw new NotSupportedException();

            /// <summary>
            /// Gets the Z value of the first argument if present,
            /// otherwise the value of the second argument.
            /// </summary>
            /// <param name="p">A coordinate, possibly with Z</param>
            /// <param name="q">A coordinate, possibly with Z</param>
            /// <returns>The Z value if present</returns>
            private static double GetZFromEither(Coordinate p, Coordinate q)
            {
                double z = p.Z;
                if (double.IsNaN(z))
                {
                    z = q.Z; // may be NaN
                }
                return z;
            }

            /// <summary>
            /// Gets the Z value of a coordinate if present, or
            /// interpolates it from the segment it lies on.
            /// If the segment Z values are not fully populate
            /// NaN is returned.
            /// </summary>
            /// <param name="p">A coordinate, possibly with Z</param>
            /// <param name="p1">A segment endpoint, possibly with Z</param>
            /// <param name="p2">A segment endpoint, possibly with Z</param>
            /// <returns>The extracted or interpolated Z value (may be NaN)</returns>
            private static double GetZOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
            {
                double z = p.Z;
                if (!double.IsNaN(z))
                    return z;
                return InterpolateZ(p, p1, p2); // may be NaN
            }

            /// <summary>
            /// Interpolates a Z value for a point along
            /// a line segment between two points.
            /// The Z value of the interpolation point (if any) is ignored.
            /// If either segment point is missing Z,
            /// returns NaN.
            /// </summary>
            /// <param name="p">A coordinate, possibly with Z</param>
            /// <param name="p1">A segment endpoint, possibly with Z</param>
            /// <param name="p2">A segment endpoint, possibly with Z</param>
            /// <returns>The extracted or interpolated Z value (may be NaN)</returns>
            private static double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2)
            {
                double p1z = p1.Z;
                double p2z = p2.Z;
                if (double.IsNaN(p1z))
                {
                    return p2z; // may be NaN
                }
                if (double.IsNaN(p2z))
                {
                    return p1z; // may be NaN
                }
                if (p.Equals2D(p1))
                {
                    return p1z; // not NaN
                }
                if (p.Equals2D(p2))
                {
                    return p2z; // not NaN
                }
                double dz = p2z - p1z;
                if (dz == 0.0)
                {
                    return p1z;
                }
                // interpolate Z from distance of p along p1-p2
                double dx = (p2.X - p1.X);
                double dy = (p2.Y - p1.Y);
                // seg has non-zero length since p1 < p < p2 
                double seglen = (dx * dx + dy * dy);
                double xoff = (p.X - p1.X);
                double yoff = (p.Y - p1.Y);
                double plen = (xoff * xoff + yoff * yoff);
                double frac = Math.Sqrt(plen / seglen);
                double zoff = dz * frac;
                double zInterpolated = p1z + zoff;
                return zInterpolated;
            }

            /// <summary>
            /// Interpolates a Z value for a point along
            /// two line segments and computes their average.
            /// The Z value of the interpolation point (if any) is ignored.
            /// If one segment point is missing Z that segment is ignored
            /// if both segments are missing Z, returns NaN.
            /// </summary>
            /// <param name="p">A coordinate</param>
            /// <param name="p1">A segment endpoint, possibly with Z</param>
            /// <param name="p2">A segment endpoint, possibly with Z</param>
            /// <param name="q1">A segment endpoint, possibly with Z</param>
            /// <param name="q2">A segment endpoint, possibly with Z</param>
            /// <returns>The averaged interpolated Z value (may be NaN)</returns>    
            private static double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
            {
                double zp = InterpolateZ(p, p1, p2);
                double zq = InterpolateZ(p, q1, q2);
                if (double.IsNaN(zp))
                {
                    return zq; // may be NaN
                }
                if (double.IsNaN(zq))
                {
                    return zp; // may be NaN
                }
                // both Zs have values, so average them
                return (zp + zq) / 2.0;
            }
        }
    }
}
