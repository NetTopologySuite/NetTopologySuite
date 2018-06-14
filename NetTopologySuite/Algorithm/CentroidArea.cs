using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the centroid of an area point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// <para>
    /// Based on the usual algorithm for calculating
    /// the centroid as a weighted sum of the centroids
    /// of a decomposition of the area into (possibly overlapping) triangles.</para>
    /// <para>
    /// The algorithm has been extended to handle holes and multi-polygons.
    /// See <see href="http://www.faqs.org/faqs/graphics/algorithms-faq"/>
    /// for further details of the basic approach.
    /// </para>
    /// <para>
    /// The code has also be extended to handle degenerate (zero-area) polygons.
    /// In this case, the centroid of the line segments in the polygon
    /// will be returned.
    /// </para>
    ///</remarks>
    [Obsolete("Use Centroid instead")]
    public class CentroidArea
    {
        private Coordinate _basePt;                            // the point all triangles are based at
        private Coordinate _triangleCent3 = new Coordinate();  // temporary variable to hold centroid of triangle
        private double _areasum2;                               // Partial area sum
        private readonly Coordinate _cg3 = new Coordinate();   // partial centroid sum

        // data for linear centroid computation, if needed
        private readonly Coordinate _centSum = new Coordinate();
        private double _totalLength;

        /// <summary>
        /// Adds the area defined by a Geometry to the centroid total.
        /// If the point has no area it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(IGeometry geom)
        {
            if (geom is IPolygon)
            {
                var poly = (IPolygon) geom;
                BasePoint = poly.ExteriorRing.GetCoordinateN(0);
                Add(poly);
            }
            else if (geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                for (int i = 0; i < gc.NumGeometries; i++)
                {
                    Add(gc.GetGeometryN(i));
                }
            }
        }

        /// <summary>
        /// Adds the area defined by an array of
        /// coordinates.  The array must be a ring;
        /// i.e. end with the same coordinate as it starts with.
        /// </summary>
        /// <param name="ring">An array of Coordinates.</param>
        public void Add(Coordinate[] ring)
        {
            BasePoint = ring[0];
            AddShell(ring);
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate Centroid
        {
            get
            {
                var cent = new Coordinate();
                if (Math.Abs(_areasum2) > 0.0)
                {
                    cent.X = _cg3.X / 3 / _areasum2;
                    cent.Y = _cg3.Y / 3 / _areasum2;
                }
                else
                {
                    // if polygon was degenerate, compute linear centroid instead
                    cent.X = _centSum.X / _totalLength;
                    cent.Y = _centSum.Y / _totalLength;
                }
                return cent;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private Coordinate BasePoint
        {
            /*get
            {
                return _basePt;
            }*/
            set
            {
                if (_basePt == null)
                    _basePt = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="poly"></param>
        private void Add(IPolygon poly)
        {
            AddShell(poly.ExteriorRing.Coordinates);
            foreach (var ls in poly.InteriorRings)
                AddHole(ls.Coordinates);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        private void AddShell(Coordinate[] pts)
        {
            bool isPositiveArea = !Orientation.IsCCW(pts);
            for (int i = 0; i < pts.Length - 1; i++)
                AddTriangle(_basePt, pts[i], pts[i + 1], isPositiveArea);
            AddLinearSegments(pts);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        private void AddHole(Coordinate[] pts)
        {
            bool isPositiveArea = Orientation.IsCCW(pts);
            for (int i = 0; i < pts.Length - 1; i++)
                AddTriangle(_basePt, pts[i], pts[i + 1], isPositiveArea);
            AddLinearSegments(pts);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="isPositiveArea"></param>
        private void AddTriangle(Coordinate p0, Coordinate p1, Coordinate p2, bool isPositiveArea)
        {
            double sign = (isPositiveArea) ? 1.0 : -1.0;
            Centroid3(p0, p1, p2, ref _triangleCent3);
            double area2 = Area2(p0, p1, p2);
            _cg3.X += sign * area2 * _triangleCent3.X;
            _cg3.Y += sign * area2 * _triangleCent3.Y;
            _areasum2 += sign * area2;
        }

        /// <summary>
        /// Returns three times the centroid of the triangle p1-p2-p3.
        /// The factor of 3 is
        /// left in to permit division to be avoided until later.
        /// </summary>
        private static void Centroid3(Coordinate p1, Coordinate p2, Coordinate p3, ref Coordinate c)
        {
            c.X = p1.X + p2.X + p3.X;
            c.Y = p1.Y + p2.Y + p3.Y;
        }

        /// <summary>
        /// Returns twice the signed area of the triangle p1-p2-p3,
        /// positive if a,b,c are oriented ccw, and negative if cw.
        /// </summary>
        private static double Area2(Coordinate p1, Coordinate p2, Coordinate p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
        }

        ///<summary>
        /// Adds the linear segments defined by an array of coordinates
        /// to the linear centroid accumulators.
        /// This is done in case the polygon(s) have zero-area,
        /// in which case the linear centroid is computed instead.
        ///</summary>
        /// <param name="pts">an array of <see cref="Coordinate"/>s</param>
        private void AddLinearSegments(Coordinate[] pts)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                double segmentLen = pts[i].Distance(pts[i + 1]);
                _totalLength += segmentLen;

                double midx = (pts[i].X + pts[i + 1].X) / 2;
                _centSum.X += segmentLen * midx;
                double midy = (pts[i].Y + pts[i + 1].Y) / 2;
                _centSum.Y += segmentLen * midy;
            }
        }

    }
}
