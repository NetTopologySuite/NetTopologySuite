using System;
using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{

    /// <summary>
    /// Computes the centroid of a <see cref="Geometry"/> of any dimension.
    /// For collections the centroid is computed for the collection of
    /// non-empty elements of highest dimension.
    /// The centroid of an empty geometry is <c>null</c>
    /// </summary>
    /// <remarks>
    /// <h3>Algorithm</h3>
    /// <list type="bullet">
    /// <item><description><b>Dimension 2</b> - the centroid ic computed
    /// as a weighted sum of the centroids
    /// of a decomposition of the area into (possibly overlapping) triangles.
    /// Holes and multipolygons are handled correctly.
    /// See <c>http://www.faqs.org/faqs/graphics/algorithms-faq/</c>
    /// for further details of the basic approach.</description></item>
    /// <item><description><b>Dimension 1</b> - Computes the average of the midpoints
    /// of all line segments weighted by the segment length.
    /// Zero-length lines are treated as points.
    /// </description></item>
    /// <item><description><b>Dimension 0</b> - Compute the average coordinate over all points.
    /// Repeated points are all included in the average
    /// </description></item>
    /// </list>
    /// If the input geometries are empty, a <c>null</c> Coordinate is returned.
    /// </remarks>
    /// <see cref="InteriorPoint"/>
    /// <see cref="MaximumInscribedCircle"/>
    /// <see cref="LargestEmptyCircle"/>
    /// <version>1.7</version>
    public class Centroid
    {
        /// <summary>
        /// Computes the centroid point of a geometry.
        /// </summary>
        /// <param name="geom">The geometry to use</param>
        /// <returns>
        /// The centroid point, or null if the geometry is empty
        /// </returns>
        public static Coordinate GetCentroid(Geometry geom)
        {
            var cent = new Centroid(geom);
            return cent.GetCentroid();
        }

        /// <summary>
        /// the point all triangles are based at
        /// </summary>
        private Coordinate _areaBasePt;

        /// <summary>
        /// temporary variable to hold centroid of triangle
        /// </summary>
        private readonly Coordinate _triangleCent3 = new Coordinate();

        /// <summary>
        /// Partial area sum
        /// </summary>
        private double _areasum2;
        /// <summary>
        /// partial centroid sum
        /// </summary>
        private readonly Coordinate _cg3 = new Coordinate();

        // data for linear centroid computation, if needed
        private readonly Coordinate _lineCentSum = new Coordinate();
        private double _totalLength;

        private int _ptCount;
        private readonly Coordinate _ptCentSum = new Coordinate();

        /// <summary>
        /// Creates a new instance for computing the centroid of a geometry
        /// </summary>
        public Centroid(Geometry geom)
        {
            _areaBasePt = null;
            Add(geom);
        }

        /// <summary>
        /// Adds a <see cref="Geometry"/> to the centroid total.
        /// </summary>
        /// <param name="geom">>The <see cref="Geometry"/> to add.</param>
        private void Add(Geometry geom)
        {
            if (geom.IsEmpty)
                return;
            if (geom is Point)
            {
                AddPoint(geom.Coordinate);
            }
            else if (geom is LineString)
            {
                AddLineSegments(geom.Coordinates);
            }
            else if (geom is Polygon)
            {
                var poly = (Polygon)geom;
                Add(poly);
            }
            else if (geom is GeometryCollection)
            {
                var gc = (GeometryCollection)geom;
                for (int i = 0; i < gc.NumGeometries; i++)
                {
                    Add(gc.GetGeometryN(i));
                }
            }
        }

        /// <summary>
        /// Gets the computed centroid.
        /// </summary>
        /// <returns>The computed centroid, or null if the input is empty</returns>
        public Coordinate GetCentroid()
        {
            /*
             * The centroid is computed from the highest dimension components present in the input.
             * I.e. areas dominate lineal geometry, which dominates points.
             * Degenerate geometry are computed using their effective dimension
             * (e.g. areas may degenerate to lines or points)
             */
            var cent = new Coordinate();
            if (Math.Abs(_areasum2) > 0.0)
            {
                // Input contains areal geometry
                cent.X = _cg3.X / 3 / _areasum2;
                cent.Y = _cg3.Y / 3 / _areasum2;
            }
            else if (_totalLength > 0.0)
            {
                // Input contains lineal geometry
                cent.X = _lineCentSum.X / _totalLength;
                cent.Y = _lineCentSum.Y / _totalLength;
            }
            else if (_ptCount > 0)
            {
                // Input contains puntal geometry only
                cent.X = _ptCentSum.X / _ptCount;
                cent.Y = _ptCentSum.Y / _ptCount;
            }
            else
            {
                return null;
            }
            return cent;
        }

        private void SetAreaBasePoint(Coordinate basePt)
        {
            _areaBasePt = basePt;
        }

        private void Add(Polygon poly)
        {
            AddShell(poly.ExteriorRing.Coordinates);
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                AddHole(poly.GetInteriorRingN(i).Coordinates);
            }
        }

        private void AddShell(Coordinate[] pts)
        {
            if (pts.Length > 0)
                SetAreaBasePoint(pts[0]);
            bool isPositiveArea = !Orientation.IsCCW(pts);
            for (int i = 0; i < pts.Length - 1; i++)
            {
                AddTriangle(_areaBasePt, pts[i], pts[i + 1], isPositiveArea);
            }
            AddLineSegments(pts);
        }

        private void AddHole(Coordinate[] pts)
        {
            bool isPositiveArea = Orientation.IsCCW(pts);
            for (int i = 0; i < pts.Length - 1; i++)
            {
                AddTriangle(_areaBasePt, pts[i], pts[i + 1], isPositiveArea);
            }
            AddLineSegments(pts);
        }

        private void AddTriangle(Coordinate p0, Coordinate p1, Coordinate p2, bool isPositiveArea)
        {
            double sign = (isPositiveArea) ? 1.0 : -1.0;
            Centroid3(p0, p1, p2, _triangleCent3);
            double area2 = Area2(p0, p1, p2);
            _cg3.X += sign * area2 * _triangleCent3.X;
            _cg3.Y += sign * area2 * _triangleCent3.Y;
            _areasum2 += sign * area2;
        }

        /// <summary>
        /// Computes three times the centroid of the triangle p1-p2-p3.
        /// The factor of 3 is
        /// left in to permit division to be avoided until later.
        /// </summary>
        private static void Centroid3(Coordinate p1, Coordinate p2, Coordinate p3, Coordinate c)
        {
            c.X = p1.X + p2.X + p3.X;
            c.Y = p1.Y + p2.Y + p3.Y;
        }

        /// <summary>
        /// Returns twice the signed area of the triangle p1-p2-p3.
        /// The area is positive if the triangle is oriented CCW, and negative if CW.
        /// </summary>
        private static double Area2(Coordinate p1, Coordinate p2, Coordinate p3)
        {
            return
                (p2.X - p1.X) * (p3.Y - p1.Y) -
                (p3.X - p1.X) * (p2.Y - p1.Y);
        }

        /// <summary>
        /// Adds the line segments defined by an array of coordinates
        /// to the linear centroid accumulators.
        /// </summary>
        /// <param name="pts">An array of <see cref="Coordinate"/>s</param>
        private void AddLineSegments(Coordinate[] pts)
        {
            double lineLen = 0.0d;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                double segmentLen = pts[i].Distance(pts[i + 1]);
                if (segmentLen == 0.0)
                    continue;

                lineLen += segmentLen;

                double midx = (pts[i].X + pts[i + 1].X) / 2;
                _lineCentSum.X += segmentLen * midx;
                double midy = (pts[i].Y + pts[i + 1].Y) / 2;
                _lineCentSum.Y += segmentLen * midy;
            }
            _totalLength += lineLen;
            if (lineLen == 0.0 && pts.Length > 0)
            {
                AddPoint(pts[0]);
            }
        }

        /// <summary>
        /// Adds a point to the point centroid accumulator.
        /// </summary>
        /// <param name="pt">A <see cref="Coordinate"/></param>
        private void AddPoint(Coordinate pt)
        {
            _ptCount += 1;
            _ptCentSum.X += pt.X;
            _ptCentSum.Y += pt.Y;
        }
    }
}
