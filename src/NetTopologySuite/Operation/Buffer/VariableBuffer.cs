using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates a buffer polygon with a varying buffer distance
    /// at each vertex along a line.
    /// <para/>
    /// Only single lines are supported as input, since buffer widths
    /// generally need to be specified individually for each line.
    /// </summary>
    /// <author>Martin Davis</author>
    public class VariableBuffer
    {
        /// <summary>
        /// Creates a buffer polygon along a line with the buffer distance interpolated
        /// between a start distance and an end distance.
        /// </summary>
        /// <param name="line">The line to buffer</param>
        /// <param name="startDistance">The buffer width at the start of the line</param>
        /// <param name="endDistance">The buffer width at the end of the line</param>
        /// <returns>The variable-distance buffer polygon</returns>
        public static Geometry Buffer(Geometry line, double startDistance,
            double endDistance)
        {
            double[] distance = Interpolate((LineString)line,
                startDistance, endDistance);
            var vb = new VariableBuffer(line, distance);
            return vb.GetResult();
        }

        /// <summary>
        /// Creates a buffer polygon along a line with the buffer distance interpolated
        /// between a start distance, a middle distance and an end distance.
        /// The middle distance is attained at
        /// the vertex at or just past the half-length of the line.
        /// For smooth buffering of a <see cref="LinearRing"/> (or the rings of a <see cref="Polygon"/>)
        /// the start distance and end distance should be equal.
        /// </summary>
        /// <param name="line">The line to buffer</param>
        /// <param name="startDistance">The buffer width at the start of the line</param>
        /// <param name="midDistance">The buffer width at the middle vertex of the line</param>
        /// <param name="endDistance">The buffer width at the end of the line</param>
        /// <returns>The variable-distance buffer polygon</returns>
        public static Geometry Buffer(Geometry line, double startDistance,
            double midDistance,
            double endDistance)
        {
            double[] distance = Interpolate((LineString)line,
                startDistance, midDistance, endDistance);
            var vb = new VariableBuffer(line, distance);
            return vb.GetResult();
        }

        /// <summary>
        /// Creates a buffer polygon along a line with the distance specified
        /// at each vertex.
        /// </summary>
        /// <param name="line">The line to buffer</param>
        /// <param name="distance">The buffer distance for each vertex of the line</param>
        /// <returns>The variable-width buffer polygon</returns>
        public static Geometry Buffer(Geometry line, double[] distance)
        {
            var vb = new VariableBuffer(line, distance);
            return vb.GetResult();
        }

        /// <summary>
        /// Computes a list of values for the points along a line by
        /// interpolating between values for the start and end point.
        /// The interpolation is
        /// based on the distance of each point along the line
        /// relative to the total line length.
        /// </summary>
        /// <param name="line">The line to interpolate along</param>
        /// <param name="startValue">The start value</param>
        /// <param name="endValue">The end value</param>
        /// <returns>The array of interpolated values</returns>
        private static double[] Interpolate(LineString line,
            double startValue,
            double endValue)
        {
            startValue = Math.Abs(startValue);
            endValue = Math.Abs(endValue);
            double[] values = new double[line.NumPoints];
            values[0] = startValue;
            values[values.Length - 1] = endValue;

            double totalLen = line.Length;
            var pts = line.Coordinates;
            double currLen = 0;
            for (int i = 1; i < values.Length; i++)
            {
                double segLen = pts[i].Distance(pts[i - 1]);
                currLen += segLen;
                double lenFrac = currLen / totalLen;
                double delta = lenFrac * (endValue - startValue);
                values[i] = startValue + delta;
            }
            return values;
        }

        /// <summary>
        /// Computes a list of values for the points along a line by
        /// interpolating between values for the start, middle and end points.
        /// The interpolation is
        /// based on the distance of each point along the line
        /// relative to the total line length.
        /// The middle distance is attained at
        /// the vertex at or just past the half-length of the line.
        /// </summary>
        /// <param name="line">The line to interpolate along</param>
        /// <param name="startValue">The start value</param>
        /// <param name="midValue">The mid value</param>
        /// <param name="endValue">The end value</param>
        /// <returns>The array of interpolated values</returns>
        private static double[] Interpolate(LineString line,
            double startValue,
            double midValue,
            double endValue)
        {
            startValue = Math.Abs(startValue);
            midValue = Math.Abs(midValue);
            endValue = Math.Abs(endValue);

            double[] values = new double[line.NumPoints];
            values[0] = startValue;
            values[values.Length - 1] = endValue;

            var pts = line.Coordinates;
            double lineLen = line.Length;
            int midIndex = IndexAtLength(pts, lineLen / 2);

            double delMidStart = midValue - startValue;
            double delEndMid = endValue - midValue;

            double lenSM = Length(pts, 0, midIndex);
            double currLen = 0;
            for (int i = 1; i <= midIndex; i++)
            {
                double segLen = pts[i].Distance(pts[i - 1]);
                currLen += segLen;
                double lenFrac = currLen / lenSM;
                double val = startValue + lenFrac * delMidStart;
                values[i] = val;
            }

            double lenME = Length(pts, midIndex, pts.Length - 1);
            currLen = 0;
            for (int i = midIndex + 1; i < values.Length - 1; i++)
            {
                double segLen = pts[i].Distance(pts[i - 1]);
                currLen += segLen;
                double lenFrac = currLen / lenME;
                double val = midValue + lenFrac * delEndMid;
                values[i] = val;
            }
            return values;
        }

        private static int IndexAtLength(Coordinate[] pts, double targetLen)
        {
            double len = 0;
            for (int i = 1; i < pts.Length; i++)
            {
                len += pts[i].Distance(pts[i - 1]);
                if (len > targetLen)
                    return i;
            }
            return pts.Length - 1;
        }

        private static double Length(Coordinate[] pts, int i1, int i2)
        {
            double len = 0;
            for (int i = i1 + 1; i <= i2; i++)
            {
                len += pts[i].Distance(pts[i - 1]);
            }
            return len;
        }

        private readonly LineString _line;
        private readonly double[] _distance;
        private readonly GeometryFactory _geomFactory;
        private readonly int _quadrantSegs = BufferParameters.DefaultQuadrantSegments;

        /// <summary>
        /// Creates a generator for a variable-distance line buffer.
        /// </summary>
        /// <param name="line">The linestring to buffer</param>
        /// <param name="distance">The buffer distance for each vertex of the line</param>
        public VariableBuffer(Geometry line, double[] distance)
        {
            _line = (LineString)line;
            _distance = distance;
            _geomFactory = line.Factory;

            if (distance.Length != _line.NumPoints)
            {
                throw new ArgumentException("Number of distances is not equal to number of vertices", nameof(distance));
            }
        }

        /// <summary>
        /// Computes the buffer polygon.
        /// </summary>
        /// <returns>A buffer polygon</returns>
        public Geometry GetResult()
        {
            var parts = new List<Geometry>();

            var pts = _line.Coordinates;
            // construct segment buffers
            for (int i = 1; i < pts.Length; i++)
            {
                double dist0 = _distance[i - 1];
                double dist1 = _distance[i];
                if (dist0 > 0 || dist1 > 0)
                {
                    var poly = SegmentBuffer(pts[i - 1], pts[i], dist0, dist1);
                    if (poly != null)
                        parts.Add(poly);
                }
            }

            var partsGeom = _geomFactory.CreateGeometryCollection(parts.ToArray());
            var buffer = partsGeom.Union();

            // ensure an empty polygon is returned if needed
            if (buffer.IsEmpty)
            {
                return _geomFactory.CreatePolygon();
            }
            return buffer;
        }

        /// <summary>
        /// Computes a variable buffer polygon for a single segment,
        /// with the given endpoints and buffer distances.
        /// The individual segment buffers are unioned
        /// to form the final buffer.
        /// </summary>
        /// <param name="p0">The segment start point</param>
        /// <param name="p1">The segment end point</param>
        /// <param name="dist0">The buffer distance at the start point</param>
        /// <param name="dist1">The buffer distance at the end point</param>
        /// <returns>The segment buffer</returns>
        private Polygon SegmentBuffer(Coordinate p0, Coordinate p1,
            double dist0, double dist1)
        {
            /*
             * Compute for increasing distance only, so flip if needed
             */
            if (dist0 > dist1)
            {
                return SegmentBuffer(p1, p0, dist1, dist0);
            }

            // forward tangent line
            var tangent = OuterTangent(p0, dist0, p1, dist1);

            // if tangent is null then compute a buffer for largest circle
            if (tangent == null)
            {
                var center = p0;
                double dist = dist0;
                if (dist1 > dist0)
                {
                    center = p1;
                    dist = dist1;
                }
                return Circle(center, dist);
            }

            var t0 = tangent.GetCoordinate(0);
            var t1 = tangent.GetCoordinate(1);

            // reverse tangent line on other side of segment
            var seg = new LineSegment(p0, p1);
            var tr0 = seg.Reflect(t0);
            var tr1 = seg.Reflect(t1);

            var coords = new CoordinateList();
            coords.Add(t0);
            coords.Add(t1);

            // end cap
            AddCap(p1, dist1, t1, tr1, coords);

            coords.Add(tr1);
            coords.Add(tr0);

            // start cap
            AddCap(p0, dist0, tr0, t0, coords);

            // close
            coords.Add(t0);

            var pts = coords.ToCoordinateArray();
            var polygon = _geomFactory.CreatePolygon(pts);
            return polygon;
        }

        /// <summary>
        /// Returns a circular polygon.
        /// </summary>
        /// <param name="center">The circle center point</param>
        /// <param name="radius">The radius</param>
        /// <returns>A polygon, or null if the radius is 0</returns>
        private Polygon Circle(Coordinate center, double radius)
        {
            if (radius <= 0)
                return null;
            int nPts = 4 * _quadrantSegs;
            var pts = new Coordinate[nPts + 1];
            double angInc = Math.PI / 2 / _quadrantSegs;
            for (int i = 0; i < nPts; i++)
            {
                pts[i] = ProjectPolar(center, radius, i * angInc);
            }
            pts[pts.Length - 1] = pts[0].Copy();
            return _geomFactory.CreatePolygon(pts);
        }

        /// <summary>
        /// Adds a semi-circular cap CCW around the point <paramref name="p"/>.
        /// </summary>
        /// <param name="p">The centre point of the cap</param>
        /// <param name="r">The cap radius</param>
        /// <param name="t1">the starting point of the cap</param>
        /// <param name="t2">The ending point of the cap</param>
        /// <param name="coords">The coordinate list to add to</param>
        private void AddCap(Coordinate p, double r, Coordinate t1, Coordinate t2, CoordinateList coords)
        {

            double angStart = AngleUtility.Angle(p, t1);
            double angEnd = AngleUtility.Angle(p, t2);
            if (angStart < angEnd)
                angStart += 2 * Math.PI;

            int indexStart = CapAngleIndex(angStart);
            int indexEnd = CapAngleIndex(angEnd);

            for (int i = indexStart; i > indexEnd; i--)
            {
                // use negative increment to create points CW
                double ang = CapAngle(i);
                coords.Add(ProjectPolar(p, r, ang));
            }
        }

        /// <summary>
        /// Computes the angle for the given cap point index.
        /// </summary>
        /// <param name="index">The fillet angle index</param>
        private double CapAngle(int index)
        {
            double capSegAng = Math.PI / 2 / _quadrantSegs;
            return index * capSegAng;
        }

        /// <summary>
        /// Computes the canonical cap point index for a given angle.
        /// The angle is rounded down to the next lower
        /// index.
        /// <para/>
        /// In order to reduce the number of points created by overlapping end caps,
        /// cap points are generated at the same locations around a circle.
        /// The index is the index of the points around the circle, 
        /// with 0 being the point at (1,0).
        /// The total number of points around the circle is 
        /// <c>4 * <see cref="_quadrantSegs"/></c>.
        /// </summary>
        /// <param name="ang">The angle</param>
        /// <returns>The index for the angle.</returns>
        private int CapAngleIndex(double ang)
        {
            double capSegAng = Math.PI / 2 / _quadrantSegs;
            int index = (int)(ang / capSegAng);
            return index;
        }

        /// <summary>
        /// Computes the two circumference points defining the outer tangent line
        /// between two circles.
        /// <para/>
        /// For the algorithm see <a href='https://en.wikipedia.org/wiki/Tangent_lines_to_circles#Outer_tangent'>Wikipedia</a>.
        /// </summary>
        /// <param name="c1">The centre of circle 1</param>
        /// <param name="r1">The radius of circle 1</param>
        /// <param name="c2">The centre of circle 2</param>
        /// <param name="r2">The radius of circle 2</param>
        /// <returns>The outer tangent line segment, or <c>null</c> if none exists</returns>
        private static LineSegment OuterTangent(Coordinate c1, double r1, Coordinate c2, double r2)
        {
            /*
             * If distances are inverted then flip to compute and flip result back.
             */
            if (r1 > r2)
            {
                var seg = OuterTangent(c2, r2, c1, r1);
                return new LineSegment(seg.P1, seg.P0);
            }
            double x1 = c1.X;
            double y1 = c1.Y;
            double x2 = c2.X;
            double y2 = c2.Y;
            // TODO: handle r1 == r2?
            double a3 = -Math.Atan2(y2 - y1, x2 - x1);

            double dr = r2 - r1;
            double d = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

            double a2 = Math.Asin(dr / d);
            // check if no tangent exists
            if (double.IsNaN(a2))
                return null;

            double a1 = a3 - a2;

            double aa = Math.PI / 2 - a1;
            double x3 = x1 + r1 * Math.Cos(aa);
            double y3 = y1 + r1 * Math.Sin(aa);
            double x4 = x2 + r2 * Math.Cos(aa);
            double y4 = y2 + r2 * Math.Sin(aa);

            return new LineSegment(x3, y3, x4, y4);
        }

        private static Coordinate ProjectPolar(Coordinate p, double r, double ang)
        {
            double x = p.X + r * SnapTrig(Math.Cos(ang));
            double y = p.Y + r * SnapTrig(Math.Sin(ang));
            return new Coordinate(x, y);
        }

        private const double SNAP_TRIG_TOL = 1e-6;

        /// <summary>
        /// Snap trig values to integer values for better consistency.
        /// </summary>
        /// <param name="x">The result of a trigonometric function</param>
        /// <returns><paramref name="x"/> snapped to the integer interval</returns>
        private static double SnapTrig(double x)
        {
            if (x > (1 - SNAP_TRIG_TOL)) return 1;
            if (x < (-1 + SNAP_TRIG_TOL)) return -1;
            if (Math.Abs(x) < SNAP_TRIG_TOL) return 0;
            return x;
        }
    }

}
