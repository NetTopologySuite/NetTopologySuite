
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates a buffer polygon with variable width along a line.
    /// <para/>
    /// Only single lines are supported as input, since buffer widths
    /// generally need to be specified specifically for each line.
    /// </summary>
    /// <author>Marting Davis</author>
    public class VariableWidthBuffer
    {
        /// <summary>
        /// </summary>
        /// <param name="line">the line to buffer</param>
        /// <param name="startWidth">the buffer width at the start of the line</param>
        /// <param name="endWidth">the buffer width at the end of the line</param>
        /// <returns>The variable-width buffer polygon</returns>
        public static IGeometry Buffer(ILineString line, double startWidth,
            double endWidth)
        {
            var width = Interpolate(line, startWidth, endWidth);
            var vb = new VariableWidthBuffer(line, width);
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
        /// <param name="start">The start value</param>
        /// <param name="end">The end value</param>
        /// <returns>The array of interpolated values</returns>
        public static double[] Interpolate(ILineString line, double start,
            double end)
        {
            start = Math.Abs(start);
            end = Math.Abs(end);
            var widths = new double[line.NumPoints];
            widths[0] = start;
            widths[widths.Length - 1] = end;

            var totalLen = line.Length;
            var pts = line.Coordinates;
            var currLen = 0.0;
            for (int i = 1; i < widths.Length; i++)
            {
                var segLen = pts[i].Distance(pts[i - 1]);
                currLen += segLen;
                var lenFrac = currLen / totalLen;
                var delta = lenFrac * (end - start);
                widths[i] = start + delta;
            }
            return widths;
        }

        private static double[] Abs(double[] v)
        {
            var a = new double[v.Length];
            for (var i = 0; i < v.Length; i++)
            {
                a[i] = Math.Abs(v[i]);
            }
            return a;
        }

        private readonly ILineString _line;
        private readonly double[] _width;
        private readonly IGeometryFactory _geomFactory;

        /// <summary>
        /// Creates a generator for a variable-width line buffer.
        /// </summary>
        /// <param name="line">The line to buffer</param>
        /// <param name="width">An array of witdth values</param>
        public VariableWidthBuffer(ILineString line, double[] width)
        {
            _line = line;
            _width = Abs(width);
            _geomFactory = line.Factory;
        }

        /// <summary>
        /// Gets the computed variable-width line buffer.
        /// </summary>
        /// <returns>A polygon</returns>
        public IGeometry GetResult()
        {
            Utilities.Assert.IsTrue(_line.NumPoints == _width.Length);

            var parts = new List<IGeometry>();

            var pts = _line.Coordinates;
            for (var i = 0; i < _line.NumPoints; i++)
            {
                var dist = _width[i] / 2;
                var ptBuf = _line.GetPointN(i).Buffer(dist);
                parts.Add(ptBuf);

                if (i >= 1)
                {
                    var curvePts = GenerateSegmentCurve(pts[i - 1], pts[i],
                        _width[i - 1], _width[i]);
                    var segBuf = _geomFactory.CreatePolygon(curvePts);
                    parts.Add(segBuf);
                }
            }

            var partsGeom = _geomFactory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(parts));
            var buffer = partsGeom.Union();
            return buffer;
        }

        private static Coordinate[] GenerateSegmentCurve(Coordinate p0, Coordinate p1,
            double width0, double width1)
        {
            var seg = new LineSegment(p0, p1);

            var dist0 = width0 / 2;
            var dist1 = width1 / 2;
            var s0 = seg.PointAlongOffset(0, dist0);
            var s1 = seg.PointAlongOffset(1, dist1);
            var s2 = seg.PointAlongOffset(1, -dist1);
            var s3 = seg.PointAlongOffset(0, -dist0);

            Coordinate[] pts = { s0, s1, s2, s3, s0 };

            return pts;
        }

    }
}