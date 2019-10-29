////#define CheckIntersectionUsingDoubleDouble
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes a point in the interior of an areal geometry.
    /// The point will lie in the geometry interior
    /// in all except certain pathological cases.
    /// </summary>
    /// <remarks>
    /// <h2>Algorithm:</h2>
    /// For each input polygon:
    /// <list type="bullet">
    /// <item><description>
    /// Determine a horizontal scan line on which the interior
    /// point will be located.
    /// To increase the chance of the scan line
    /// having non-zero-width intersection with the polygon
    /// the scan line Y ordinate is chosen to be near the centre of the polygon's
    /// Y extent but distinct from all of vertex Y ordinates.
    /// </description></item>
    /// <item><description>
    /// Compute the sections of the scan line
    /// which lie in the interior of the polygon.
    /// </description></item>
    /// <item><description>
    /// Choose the widest interior section
    /// and take its midpoint as the interior point.
    /// </description></item>
    /// </list>
    /// The final interior point is chosen as
    /// the one occurring in the widest interior section.
    /// <para>
    /// This algorithm is a tradeoff between performance
    /// and point quality (where points further from the geometry
    /// boundary are considered to be higher quality)
    /// Priority is given to performance.
    /// This means that the computed interior point
    /// may not be suitable for some uses
    /// (such as label positioning).
    /// </para>
    /// <para>
    /// The algorithm handles some kinds of invalid/degenerate geometry,
    /// including zero-area and self-intersecting polygons.
    /// </para>
    /// <para>
    /// Empty geometry is handled by returning a <see langword="null"/> point.
    /// </para>
    /// <h3>KNOWN BUGS</h3>
    /// <list type="bullet">
    /// <item><description>
    /// If a fixed precision model is used,
    /// in some cases this method may return a point
    /// which does not lie in the interior.
    /// </description></item>
    /// <item><description>
    /// If the input polygon is <i>extremely</i> narrow the computed point
    /// may not lie in the interior of the polygon.
    /// </description></item>
    /// </list>
    /// </remarks>
    public class InteriorPointArea
    {
        /// <summary>
        /// Computes an interior point for the
        /// polygonal components of a Geometry.
        /// </summary>
        /// <param name="geom">The geometry to compute.</param>
        /// <returns>
        /// The computed interior point,
        /// or <see langword="null"/> if the geometry has no polygonal components.
        /// </returns>
        public static Coordinate GetInteriorPoint(Geometry geom)
        {
            var intPt = new InteriorPointArea(geom);
            return intPt.InteriorPoint;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double Avg(double a, double b)
        {
            return (a + b)/2.0;
        }

        private Coordinate _interiorPoint;
        private double _maxWidth = -1;

        /// <summary>
        /// Creates a new interior point finder
        /// for an areal geometry.
        /// </summary>
        /// <param name="g">An areal geometry</param>
        public InteriorPointArea(Geometry g)
        {
            Process(g);
        }

        /// <summary>
        /// Gets the computed interior point
        /// or <see langword="null"/> if the input geometry is empty.
        /// </summary>
        public Coordinate InteriorPoint => _interiorPoint;

        /// <summary>
        /// Processes a geometry to determine
        /// the best interior point for
        /// all component polygons.
        /// </summary>
        /// <param name="geom">The geometry to process.</param>
        private void Process(Geometry geom)
        {
            if (geom.IsEmpty)
            {
                return;
            }

            if (geom is Polygon polygon)
            {
                ProcessPolygon(polygon);
            }
            else if (geom is GeometryCollection gc)
            {
                foreach (var geometry in gc.Geometries)
                {
                    Process(geometry);
                }
            }
        }

        /// <summary>
        /// Computes an interior point of a component Polygon
        /// and updates current best interior point
        /// if appropriate.
        /// </summary>
        /// <param name="polygon">The polygon to process.</param>
        private void ProcessPolygon(Polygon polygon)
        {
            var intPtPoly = new InteriorPointPolygon(polygon);
            intPtPoly.Process();
            double width = intPtPoly.Width;
            if (width > _maxWidth)
            {
                _maxWidth = width;
                _interiorPoint = intPtPoly.InteriorPoint;
            }
        }

        /// <summary>
        /// Computes an interior point in a single <see cref="Polygon"/>,
        /// as well as the width of the scan-line section it occurs in
        /// to allow choosing the widest section occurrence.
        /// </summary>
        private class InteriorPointPolygon
        {
            private readonly Polygon _polygon;
            private readonly double _interiorPointY;
            //private readonly List<double> _crossings = new List<double>();

            private double _interiorSectionWidth;
            private Coordinate _interiorPoint;

            /// <summary>
            /// Initializes a new instance of the <see cref="InteriorPointPolygon"/> class.
            /// </summary>
            /// <param name="polygon">The polygon to test.</param>
            public InteriorPointPolygon(Polygon polygon)
            {
                _polygon = polygon;
                _interiorPointY = ScanLineYOrdinateFinder.GetScanLineY(polygon);
            }

            /// <summary>
            /// Gets the computed interior point,
            /// or <see langword="null"/> if the input geometry is empty.
            /// </summary>
            public Coordinate InteriorPoint => _interiorPoint;

            /// <summary>
            /// Gets the width of the scanline section containing the interior point.
            /// Used to determine the best point to use.
            /// </summary>
            public double Width => _interiorSectionWidth;

            /// <summary>
            /// Compute the interior point.
            /// </summary>
            public void Process()
            {
                // This results in returning a null Coordinate
                if (_polygon.IsEmpty)
                {
                    return;
                }

                // set default interior point in case polygon has zero area
                _interiorPoint = new Coordinate(_polygon.Coordinate);
                var crossings = new List<double>();
                ScanRing((LinearRing)_polygon.ExteriorRing, crossings);
                for (int i = 0; i < _polygon.NumInteriorRings; i++)
                {
                    ScanRing((LinearRing)_polygon.GetInteriorRingN(i), crossings);
                }

                FindBestMidpoint(crossings);
            }

            private void ScanRing(LinearRing ring, List<double> crossings)
            {
                // skip rings which don't cross scan line
                if (!IntersectsHorizontalLine(ring.EnvelopeInternal, _interiorPointY))
                {
                    return;
                }

                var seq = ring.CoordinateSequence;
                for (int i = 1, cnt = seq.Count; i < cnt; i++)
                {
                    var ptPrev = seq.GetCoordinate(i - 1);
                    var pt = seq.GetCoordinate(i);
                    AddEdgeCrossing(ptPrev, pt, _interiorPointY, crossings);
                }
            }

            private void AddEdgeCrossing(Coordinate p0, Coordinate p1, double scanY, List<double> crossings)
            {
                // skip non-crossing segments
                if (!IntersectsHorizontalLine(p0, p1, scanY))
                {
                    return;
                }

                if (!IsEdgeCrossingCounted(p0, p1, scanY))
                {
                    return;
                }

                // edge intersects scan line, so add a crossing
                double xInt = Intersection(p0, p1, scanY);
                crossings.Add(xInt);

#if CheckIntersectionUsingDoubleDouble
                CheckIntersectionDD(p0, p1, scanY, xInt);
#endif
            }

            /// <summary>
            /// Finds the midpoint of the widest interior section.
            /// Sets the <see cref="_interiorPoint"/> location and the
            /// <see cref="_interiorSectionWidth"/>
            /// </summary>
            /// <param name="crossings">The list of scan-line X ordinates</param>
            private void FindBestMidpoint(List<double> crossings)
            {
                // zero-area polygons will have no crossings
                if (crossings.Count == 0)
                {
                    return;
                }

                // TODO: is there a better way to verify the crossings are correct?
                Assert.IsTrue(0 == crossings.Count % 2, "Interior Point robustness failure: odd number of scanline crossings");

                crossings.Sort();

                // Entries in crossings list are expected to occur in pairs representing a
                // section of the scan line interior to the polygon (which may be zero-length)
                for (int i = 0; i < crossings.Count; i += 2)
                {
                    double x1 = crossings[i];

                    // crossings count must be even so this should be safe
                    double x2 = crossings[i + 1];

                    double width = x2 - x1;
                    if (width > _interiorSectionWidth)
                    {
                        _interiorSectionWidth = width;
                        double interiorPointX = Avg(x1, x2);
                        _interiorPoint = new Coordinate(interiorPointX, _interiorPointY);
                    }
                }
            }

            /// <summary>
            /// Tests if an edge intersection contributes to the crossing count.
            /// Some crossing situations are not counted,
            /// to ensure that the list of crossings
            /// captures strict inside/outside topology.
            /// </summary>
            /// <param name="p0">An endpoint of the segment.</param>
            /// <param name="p1">An endpoint of the segment.</param>
            /// <param name="scanY">The Y-ordinate of the horizontal line.</param>
            /// <returns><see langword="true"/> if the edge crossing is counted.</returns>
            private bool IsEdgeCrossingCounted(Coordinate p0, Coordinate p1, double scanY)
            {
                double y0 = p0.Y;
                double y1 = p1.Y;
                // skip horizontal lines
                if (y0 == y1)
                    return false;
                
                // handle cases where vertices lie on scan-line
                // downward segment does not include start point
                if (y0 == scanY && y1 < scanY)
                    return false;

                // upward segment does not include endpoint
                if (y1 == scanY && y0 < scanY)
                    return false;
                
                return true;
            }

            /// <summary>
            /// Computes the intersection of a segment with a horizontal line.
            /// The segment is expected to cross the horizontal line
            /// - this condition is not checked.
            /// Computation uses regular double-precision arithmetic.
            /// Test seems to indicate this is as good as using DD arithmetic.
            /// </summary>
            /// <param name="p0">An endpoint of the segment.</param>
            /// <param name="p1">An endpoint of the segment.</param>
            /// <param name="y">The Y-ordinate of the horizontal line</param>
            /// <returns></returns>
            private static double Intersection(Coordinate p0, Coordinate p1, double y)
            {
                double x0 = p0.X;
                double x1 = p1.X;

                if (x0 == x1)
                {
                    return x0;
                }

                // Assert: segDX is non-zero, due to previous equality test
                double segDX = x1 - x0;
                double segDY = p1.Y - p0.Y;
                double m = segDY / segDX;
                double x = x0 + ((y - p0.Y) / m);
                return x;
            }
        }

        /// <summary>
        /// Tests if an envelope intersects a horizontal line.
        /// </summary>
        /// <param name="env">The envelope to test.</param>
        /// <param name="y">The Y-ordinate of the horizontal line.</param>
        /// <returns><see langword="true"/> if the envelope and line intersect.</returns>
        private static bool IntersectsHorizontalLine(Envelope env, double y)
        {
            if (y < env.MinY)
            {
                return false;
            }

            if (y > env.MaxY)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tests if a line segment intersects a horizontal line.
        /// </summary>
        /// <param name="p0">A segment endpoint.</param>
        /// <param name="p1">A segment endpoint.</param>
        /// <param name="y">The Y-ordinate of the horizontal line.</param>
        /// <returns><see langword="true"/> if the segment and line intersect.</returns>
        private static bool IntersectsHorizontalLine(Coordinate p0, Coordinate p1, double y)
        {
            // both ends above?
            if (p0.Y > y && p1.Y > y)
            {
                return false;
            }

            // both ends below?
            if (p0.Y < y && p1.Y < y)
            {
                return false;
            }

            // segment must intersect line
            return true;
        }

        /// <summary>
        /// Finds a safe scan line Y ordinate by projecting
        /// the polygon segments
        /// to the Y axis and finding the
        /// Y-axis interval which contains the centre of the Y extent.
        /// The centre of
        /// this interval is returned as the scan line Y-ordinate.
        /// <para>
        /// Note that in the case of (degenerate, invalid)
        /// zero-area polygons the computed Y value
        /// may be equal to a vertex Y-ordinate.
        /// </para>
        /// </summary>
        /// <author>Martin Davis</author>
        private class ScanLineYOrdinateFinder
        {
            public static double GetScanLineY(Polygon poly)
            {
                var finder = new ScanLineYOrdinateFinder(poly);
                return finder.GetScanLineY();
            }

            private readonly Polygon _poly;

            private readonly double _centreY;
            private double _hiY;// = double.MaxValue;
            private double _loY;// = -double.MaxValue;

            private ScanLineYOrdinateFinder(Polygon poly)
            {
                _poly = poly;

                // initialize using extremal values
                var env = poly.EnvelopeInternal;
                _hiY = env.MaxY;
                _loY = env.MinY;
                _centreY = Avg(_loY, _hiY);
            }

            private double GetScanLineY()
            {
                Process(_poly.ExteriorRing);
                for (int i = 0; i < _poly.NumInteriorRings; i++)
                {
                    Process(_poly.GetInteriorRingN(i));
                }
                double scanLineY = Avg(_hiY, _loY);
                return scanLineY;
            }

            private void Process(LineString line)
            {
                var seq = line.CoordinateSequence;
                for (int i = 0; i < seq.Count; i++)
                {
                    double y = seq.GetY(i);
                    UpdateInterval(y);
                }
            }

            private void UpdateInterval(double y)
            {
                if (y <= _centreY)
                {
                    if (y > _loY)
                        _loY = y;
                }
                else if (y > _centreY)
                {
                    if (y < _hiY)
                    {
                        _hiY = y;
                    }
                }

            }
        }

#if CheckIntersectionUsingDoubleDouble
        // for testing only
        private static void CheckIntersectionDD(Coordinate p0, Coordinate p1, double scanY, double xInt)
        {
          double xIntDD = IntersectionDD(p0, p1, scanY);
          System.Console.WriteLine(
              ((xInt != xIntDD) ? ">>" : "")
              + "IntPt x - DP: " + xInt + ", DD: " + xIntDD 
              + "   y: " + scanY + "   " + IO.WKTWriter.ToLineString(p0, p1) );
        }

        private static double IntersectionDD(Coordinate p0, Coordinate p1, double y)
        {
            double x0 = p0.X;
            double x1 = p1.X;
            if (x0 == x1)
            {
                return x0;
            }

            var segDX = Mathematics.DD.ValueOf(x1) - x0;

            // Assert: segDX is non-zero, due to previous equality test
            var segDY = Mathematics.DD.ValueOf(p1.Y) - p0.Y;
            var m = segDY / segDX;
            var dy = Mathematics.DD.ValueOf(y) - p0.Y;
            var dx = dy / m;
            var xInt = Mathematics.DD.ValueOf(x0) + dx;
            return xInt.ToDoubleValue();
        }
#endif
    }
}
