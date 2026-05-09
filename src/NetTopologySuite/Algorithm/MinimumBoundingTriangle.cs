/*
 * Copyright (c) 2025 Michael Carleton
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License 2.0
 * and Eclipse Distribution License v. 1.0 which accompanies this distribution.
 * The Eclipse Public License is available at http://www.eclipse.org/legal/epl-v20.html
 * and the Eclipse Distribution License is available at
 *
 * http://www.eclipse.org/org/documents/edl-v10.php.
 */

using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the Minimum Bounding Triangle (MBT) for the points in a <see cref="Geometry"/>.
    /// The MBT is the smallest triangle which covers all the input points
    /// (also known as the Smallest Enclosing Triangle).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm for finding minimum-area enclosing triangles is based on the
    /// geometric characterisation of Klee &amp; Laskowski. For each edge of the
    /// convex polygon, side <i>C</i> of the enclosing triangle is set flush with
    /// that edge. O'Rourke et al. show that for each fixed flush side a local
    /// minimum enclosing triangle exists, and that:
    /// <list type="bullet">
    /// <item><description>The midpoints of the enclosing triangle's sides must touch the polygon.</description></item>
    /// <item><description>There exists a local minimum enclosing triangle with at least two sides flush with edges of the polygon. The third side is either flush with an edge or tangent to the polygon.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Overall complexity is <i>O(n log n)</i> due to the convex hull computation
    /// (the rotating-calipers stage itself is <i>O(n)</i>).
    /// </para>
    /// </remarks>
    /// <author>
    /// Python implementation by Charlie Marsh; Java port and enhancements by Michael Carleton.
    /// </author>
    public class MinimumBoundingTriangle
    {
        // Equivalent to java.lang.Math.ulp(1.0). Computed at static init because
        // System.Math.Ulp / Math.BitIncrement are not available on netstandard2.0.
        private static readonly double DoubleEps = ComputeUlpOfOne();

        private readonly Geometry _hull;
        private readonly int _n;
        private readonly Coordinate[] _points;
        private readonly GeometryFactory _gf;
        private readonly double _tol;

        /// <summary>
        /// Creates a solver for the minimum-area enclosing triangle of the input geometry.
        /// </summary>
        /// <param name="shape">
        /// Any geometry; only the vertices of its convex hull are used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="shape"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the convex hull of <paramref name="shape"/> has dimension
        /// less than <see cref="Dimension.Surface"/> (i.e. fewer than three
        /// non-collinear points).
        /// </exception>
        public MinimumBoundingTriangle(Geometry shape)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));

            _hull = shape.ConvexHull();
            _gf = _hull.Factory;

            if (_hull.Dimension < Dimension.Surface)
            {
                throw new ArgumentException(
                    "MinimumBoundingTriangle requires at least 3 non-collinear points.",
                    nameof(shape));
            }

            _points = ExtractOpenRing(_hull.Coordinates);
            _n = _points.Length;
            _tol = ComputeAdaptiveTolerance(_points);
        }

        /// <summary>
        /// Computes a minimum-area triangle enclosing the convex hull of the input.
        /// </summary>
        /// <returns>
        /// <para>
        /// A triangular <see cref="Polygon"/> of minimum area enclosing the hull.
        /// If the hull is itself a triangle (or degenerate to one), the hull
        /// geometry is returned.
        /// </para>
        /// <para>
        /// May return <c>null</c> in rare numerical-degeneracy configurations
        /// where the rotating-calipers search cannot construct a valid triangle
        /// for any flush base (e.g. parallel intermediate side intersections,
        /// or optimality predicates that can't be satisfied within the adaptive
        /// tolerance). The constructor pre-validates that the hull has
        /// dimension &gt;= 2; this null path is purely numerical, not structural.
        /// </para>
        /// </returns>
        public Geometry GetTriangle()
        {
            // A triangle has 3 unique vertices + a closing duplicate = 4 coordinates.
            if (_hull.NumPoints <= 4)
            {
                return _hull;
            }

            // Rotating-calipers: one pass over hull edges, carrying a/b state.
            int a = 1;
            int b = 2;

            double minArea = double.MaxValue;
            Polygon optimal = null;

            for (int i = 0; i < _n; i++)
            {
                var t = new TriangleForIndex(this, i, a, b);
                a = t.AOut;
                b = t.BOut;
                var triangle = t.Triangle;
                if (triangle != null)
                {
                    double area = triangle.Area;
                    if (optimal == null || area < minArea)
                    {
                        optimal = triangle;
                        minArea = area;
                    }
                }
            }

            return optimal;
        }

        /// <summary>
        /// Strips the trailing duplicate from a closed coordinate ring and returns
        /// a deep copy of the resulting open sequence.
        /// </summary>
        private static Coordinate[] ExtractOpenRing(Coordinate[] closedRing)
        {
            int len = closedRing.Length - 1;
            var open = new Coordinate[len];
            CoordinateArrays.CopyDeep(closedRing, 0, open, 0, len);
            return open;
        }

        /// <summary>
        /// Returns a numeric tolerance scaled to the magnitude of the input
        /// coordinates, with a unit-coordinate baseline so that small inputs
        /// don't end up with a degenerate sub-epsilon tolerance.
        /// </summary>
        private static double ComputeAdaptiveTolerance(Coordinate[] points)
        {
            double coordMag = 1.0; // baseline so we never go below 10*ulp(1.0)
            for (int i = 0; i < points.Length; i++)
            {
                var c = points[i];
                double m = Math.Max(Math.Abs(c.X), Math.Abs(c.Y));
                if (m > coordMag) coordMag = m;
            }
            return 10.0 * DoubleEps * coordMag;
        }

        private static double ComputeUlpOfOne()
        {
            long bits = BitConverter.DoubleToInt64Bits(1.0);
            return BitConverter.Int64BitsToDouble(bits + 1) - 1.0;
        }

        // ---------------------------------------------------------------------
        // Side: a directed edge / line abstraction. Direct port of the JTS
        // nested class of the same name. Slope/intercept form with an explicit
        // vertical flag; cross-product distance; intersection delegated to NTS.
        // ---------------------------------------------------------------------
        private sealed class Side
        {
            internal readonly Coordinate P1, P2;
            internal readonly double Slope, Intercept;
            internal readonly bool Vertical;

            internal Side(Coordinate p1, Coordinate p2)
            {
                P1 = p1;
                P2 = p2;
                Slope = (p2.Y - p1.Y) / (p2.X - p1.X);
                Intercept = p1.Y - Slope * p1.X;
                Vertical = p1.X == p2.X;
            }

            internal double SqrDistance(Coordinate p)
            {
                double numerator = (P2.X - P1.X) * (P1.Y - p.Y) - (P1.X - p.X) * (P2.Y - P1.Y);
                numerator *= numerator;
                double denominator = (P2.X - P1.X) * (P2.X - P1.X) + (P2.Y - P1.Y) * (P2.Y - P1.Y);
                return numerator / denominator;
            }

            /// <summary>
            /// Returns a point on this side at the given x-coordinate. For
            /// vertical sides returns <see cref="P1"/> (matches JTS: imprecise
            /// but never <c>null</c>; callers gate on <see cref="Vertical"/>).
            /// </summary>
            internal Coordinate AtX(double x)
            {
                if (Vertical)
                {
                    return P1;
                }
                return new Coordinate(x, Slope * x + Intercept);
            }

            internal double Distance(Coordinate p)
            {
                return Math.Sqrt(SqrDistance(p));
            }

            internal Coordinate Intersection(Side that)
            {
                var c = IntersectionComputer.Intersection(P1, P2, that.P1, that.P2);
                if (c == null || !IsFinite(c.X) || !IsFinite(c.Y))
                {
                    return null;
                }
                return c;
            }

            internal Coordinate Midpoint()
            {
                return new Coordinate((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
            }

            // Polyfill for double.IsFinite (added in netstandard2.1; NTS targets
            // both 2.0 and 2.1). JTS uses java.lang.Double.isFinite directly.
            private static bool IsFinite(double v)
            {
                return !double.IsNaN(v) && !double.IsInfinity(v);
            }
        }

        // ---------------------------------------------------------------------
        // TriangleForIndex: the per-iteration state of the rotating-calipers
        // search. Direct port of the JTS nested class of the same name.
        //
        // Step 3 lands the class skeleton + all geometric helpers, with a stub
        // constructor body. Step 4 fills in the caliper-advance state machine.
        // ---------------------------------------------------------------------
        private sealed class TriangleForIndex
        {
            private readonly Coordinate[] _points;
            private readonly int _n;
            private readonly double _tol;
            private readonly GeometryFactory _gf;

            internal readonly int AOut;
            internal readonly int BOut;
            internal readonly Polygon Triangle;

            private readonly Side _sideC;
            private Side _sideA;
            private Side _sideB;

            internal TriangleForIndex(MinimumBoundingTriangle outer, int c, int a, int b)
            {
                _points = outer._points;
                _n = outer._n;
                _tol = outer._tol;
                _gf = outer._gf;

                a = FloorMod(Math.Max(a, c + 1), _n);
                b = FloorMod(Math.Max(b, c + 2), _n);
                _sideC = SideAt(c);

                // Move b onto the right chain.
                int iter = 0;
                while (OnLeftChain(b))
                {
                    Debug.Assert(iter++ < _n + 2, "MBT advancement: 'right chain' loop exceeded n steps");
                    b = FloorMod(b + 1, _n);
                }

                // Advance a/b until a and b are high/critical.
                iter = 0;
                while (Dist(b, _sideC) > Dist(a, _sideC) + _tol)
                {
                    Debug.Assert(iter++ < _n + 2, "MBT advancement: 'high/critical' loop exceeded n steps");
                    var ab = IncrementLowHigh(a, b);
                    a = ab[0];
                    b = ab[1];
                }

                // Advance b until tangency.
                iter = 0;
                while (Tangency(a, b))
                {
                    Debug.Assert(iter++ < _n + 2, "MBT advancement: 'tangency' loop exceeded n steps");
                    b = FloorMod(b + 1, _n);
                }

                // Compute gamma for B.
                var gammaB = Gamma(_points[b], SideAt(a), _sideC);
                if (gammaB == null)
                {
                    Triangle = null;
                    AOut = a;
                    BOut = b;
                    return;
                }

                // Decide construction based on low/high and relative distances.
                if (Low(b, gammaB) || Dist(b, _sideC) < Dist(FloorMod(a - 1, _n), _sideC) - _tol)
                {
                    var tempSideB = SideAt(b);
                    var tempSideA = SideAt(a);

                    var iCB = _sideC.Intersection(tempSideB);
                    var iAB = tempSideA.Intersection(tempSideB);
                    if (iCB == null || iAB == null)
                    {
                        Triangle = null;
                        AOut = a;
                        BOut = b;
                        return;
                    }
                    _sideB = new Side(iCB, iAB);
                    _sideA = tempSideA;

                    if (Dist(_sideB.Midpoint(), _sideC) < Dist(FloorMod(a - 1, _n), _sideC) - _tol)
                    {
                        var gammaA = Gamma(_points[FloorMod(a - 1, _n)], _sideB, _sideC);
                        if (gammaA == null)
                        {
                            Triangle = null;
                            AOut = a;
                            BOut = b;
                            return;
                        }
                        _sideA = new Side(gammaA, _points[FloorMod(a - 1, _n)]);
                    }
                }
                else
                {
                    _sideB = new Side(gammaB, _points[b]);
                    _sideA = new Side(gammaB, _points[FloorMod(a - 1, _n)]);
                }

                // Final pairwise intersections.
                var vertexA = _sideC.Intersection(_sideB);
                var vertexB = _sideC.Intersection(_sideA);
                var vertexC = _sideA.Intersection(_sideB);

                if (!IsValidTriangle(vertexA, vertexB, vertexC, a, b, c))
                {
                    Triangle = null;
                }
                else
                {
                    var coords = new[] { vertexA, vertexB, vertexC, vertexA };
                    // JTS may output triangles with either orientation depending on
                    // the start edge. NTS consumers expect CCW shells for predictable
                    // topological predicates, so enforce CCW here (deliberate
                    // deviation from raw JTS output).
                    if (!Orientation.IsCCW(coords))
                    {
                        CoordinateArrays.Reverse(coords);
                    }
                    Triangle = _gf.CreatePolygon(coords);
                }

                AOut = a;
                BOut = b;
            }

            // ---- helpers (verbatim port of JTS TriangleForIndex methods) ----

            /// <summary>JTS: <c>private double dist(int point, Side side)</c>.</summary>
            private double Dist(int point, Side side)
            {
                return side.Distance(_points[FloorMod(point, _points.Length)]);
            }

            /// <summary>JTS: <c>private double dist(Coordinate point, Side side)</c>.</summary>
            private double Dist(Coordinate point, Side side)
            {
                return side.Distance(point);
            }

            /// <summary>
            /// JTS: <c>private Coordinate gamma(Coordinate point, Side on, Side base)</c>.
            /// (Parameter <c>base</c> renamed to <c>baseSide</c> — C# reserved word.)
            /// </summary>
            private Coordinate Gamma(Coordinate point, Side on, Side baseSide)
            {
                var I = on.Intersection(baseSide);
                if (I == null) return null;

                double dxOn = on.P2.X - on.P1.X;
                double dyOn = on.P2.Y - on.P1.Y;

                double bx = baseSide.P2.X - baseSide.P1.X;
                double by = baseSide.P2.Y - baseSide.P1.Y;
                double nx = -by;
                double ny = bx;
                double nLen = Hypot(nx, ny);
                if (nLen == 0) return null;

                // Signed distance of point from base.
                double signedP = ((point.X - baseSide.P1.X) * nx + (point.Y - baseSide.P1.Y) * ny) / nLen;

                // Change in signed distance per unit t along 'on'.
                double denom = (dxOn * nx + dyOn * ny) / nLen;

                // Use analytic solution if well-conditioned.
                if (Math.Abs(denom) > _tol)
                {
                    double t = (2.0 * signedP) / denom;
                    return new Coordinate(I.X + t * dxOn, I.Y + t * dyOn);
                }

                // Fallback: finite-difference step.
                double target = 2.0 * Math.Abs(signedP);

                if (on.Vertical)
                {
                    // Move 1 unit along 'on' (vertical).
                    double dd = baseSide.Distance(new Coordinate(I.X, I.Y + 1));
                    if (dd <= _tol) return null;
                    double s = target / dd;
                    var guess = new Coordinate(I.X, I.Y + s);
                    if (Ccw(baseSide.P1, baseSide.P2, guess) != Ccw(baseSide.P1, baseSide.P2, point))
                    {
                        guess = new Coordinate(I.X, I.Y - s);
                    }
                    return guess;
                }
                else
                {
                    // Move 1 unit in +x along 'on'.
                    var p = on.AtX(I.X + 1);
                    double dd = baseSide.Distance(p);
                    if (dd <= _tol) return null;
                    double s = target / dd;
                    var guess = on.AtX(I.X + s);
                    if (Ccw(baseSide.P1, baseSide.P2, guess) != Ccw(baseSide.P1, baseSide.P2, point))
                    {
                        guess = on.AtX(I.X - s);
                    }
                    return guess;
                }
            }

            /// <summary>
            /// JTS: <c>private boolean onLeftChain(int b)</c>. Returns <c>true</c>
            /// while the next vertex is at least as far from the flush base as
            /// the current — i.e. we have not yet crested the polygon's profile
            /// onto the right (descending) chain. JTS name retained for
            /// cross-referencing the source paper.
            /// </summary>
            private bool OnLeftChain(int b)
            {
                double dNext = Dist(FloorMod(b + 1, _n), _sideC);
                double dCurr = Dist(b, _sideC);
                return dNext >= dCurr - _tol;
            }

            /// <summary>JTS: <c>private int[] incrementLowHigh(int a, int b)</c>.</summary>
            private int[] IncrementLowHigh(int a, int b)
            {
                var gammaA = Gamma(_points[a], SideAt(a), _sideC);
                if (High(b, gammaA))
                {
                    b = FloorMod(b + 1, _n);
                }
                else
                {
                    a = FloorMod(a + 1, _n);
                }
                return new[] { a, b };
            }

            /// <summary>JTS: <c>private boolean tangency(int a, int b)</c>.</summary>
            private bool Tangency(int a, int b)
            {
                var gammaB = Gamma(_points[b], SideAt(a), _sideC);
                if (gammaB == null) return false;
                return Dist(b, _sideC) > Dist(FloorMod(a - 1, _n), _sideC) && High(b, gammaB);
            }

            /// <summary>JTS: <c>private boolean ccw(...)</c>.</summary>
            private static bool Ccw(Coordinate a, Coordinate b, Coordinate c)
            {
                return Orientation.Index(a, b, c) == OrientationIndex.CounterClockwise;
            }

            // High and Low are deliberately asymmetric: in High the `t1 == t2`
            // branch returns the distance comparison and the else branch returns
            // false; in Low the branches are swapped. This is the Klee/O'Rourke
            // characterisation of "is gamma above (resp. below) the support
            // vertex on the same side of the chord as the polygon", not a
            // copy-paste error. See JTS MinimumBoundingTriangle for the source.

            /// <summary>JTS: <c>private boolean high(int b, Coordinate gammaB)</c>.</summary>
            private bool High(int b, Coordinate gammaB)
            {
                if (gammaB == null) return false;

                int bm1 = FloorMod(b - 1, _n);
                int bp1 = FloorMod(b + 1, _n);

                bool s1 = Ccw(gammaB, _points[b], _points[bm1]);
                bool s2 = Ccw(gammaB, _points[b], _points[bp1]);
                if (s1 == s2) return false;

                bool t1 = Ccw(_points[bm1], _points[bp1], gammaB);
                bool t2 = Ccw(_points[bm1], _points[bp1], _points[b]);

                if (t1 == t2)
                {
                    return Dist(gammaB, _sideC) > Dist(b, _sideC);
                }
                return false;
            }

            /// <summary>JTS: <c>private boolean low(int b, Coordinate gammaB)</c>.</summary>
            private bool Low(int b, Coordinate gammaB)
            {
                if (gammaB == null) return false;

                int bm1 = FloorMod(b - 1, _n);
                int bp1 = FloorMod(b + 1, _n);

                bool s1 = Ccw(gammaB, _points[b], _points[bm1]);
                bool s2 = Ccw(gammaB, _points[b], _points[bp1]);
                if (s1 == s2) return false;

                bool t1 = Ccw(_points[bm1], _points[bp1], gammaB);
                bool t2 = Ccw(_points[bm1], _points[bp1], _points[b]);

                if (t1 == t2)
                {
                    return false;
                }
                return Dist(gammaB, _sideC) > Dist(b, _sideC);
            }

            /// <summary>JTS: <c>private boolean isValidTriangle(...)</c>.</summary>
            private bool IsValidTriangle(Coordinate vertexA, Coordinate vertexB, Coordinate vertexC, int a, int b, int c)
            {
                if (vertexA == null || vertexB == null || vertexC == null) return false;
                var midpointA = Midpoint(vertexC, vertexB);
                var midpointB = Midpoint(vertexA, vertexC);
                var midpointC = Midpoint(vertexA, vertexB);
                return ValidateMidpoint(midpointA, a)
                    && ValidateMidpoint(midpointB, b)
                    && ValidateMidpoint(midpointC, c);
            }

            /// <summary>JTS: <c>private boolean validateMidpoint(...)</c>.</summary>
            private bool ValidateMidpoint(Coordinate midpoint, int index)
            {
                var s = SideAt(index);
                double d = DistanceComputer.PointToSegment(midpoint, s.P1, s.P2);
                return d <= _tol;
            }

            /// <summary>
            /// JTS: <c>private Side side(final int i)</c>. Renamed to <c>SideAt</c>
            /// so it doesn't visually shadow the sibling type <c>Side</c>.
            /// </summary>
            private Side SideAt(int i)
            {
                return new Side(_points[FloorMod(i - 1, _n)], _points[i]);
            }

            /// <summary>JTS: <c>private Coordinate midpoint(Coordinate a, Coordinate b)</c>.</summary>
            private static Coordinate Midpoint(Coordinate a, Coordinate b)
            {
                return new Coordinate((a.X + b.X) / 2, (a.Y + b.Y) / 2);
            }

            /// <summary>
            /// Polyfill for <c>java.lang.Math.floorMod</c>. Always non-negative
            /// for positive divisor.
            /// </summary>
            private static int FloorMod(int a, int n)
            {
                int r = a % n;
                if (r < 0) r += n;
                return r;
            }

            /// <summary>
            /// Polyfill for <c>org.locationtech.jts.math.MathUtil.hypot</c>.
            /// NTS' <see cref="NetTopologySuite.Mathematics.MathUtil"/> doesn't
            /// expose Hypot, so this scoped helper avoids the overflow risk of
            /// the naive <c>sqrt(x*x + y*y)</c> formulation for extreme inputs.
            /// </summary>
            private static double Hypot(double x, double y)
            {
                x = Math.Abs(x);
                y = Math.Abs(y);
                double max = Math.Max(x, y);
                if (max == 0) return 0;
                double min = Math.Min(x, y);
                double r = min / max;
                return max * Math.Sqrt(1 + r * r);
            }
        }
    }
}
