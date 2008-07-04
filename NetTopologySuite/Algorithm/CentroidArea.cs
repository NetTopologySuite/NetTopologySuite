using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of an area point.
    /// Algorithm:
    /// Based on the usual algorithm for calculating
    /// the centroid as a weighted sum of the centroids
    /// of a decomposition of the area into (possibly overlapping) triangles.
    /// The algorithm has been extended to handle holes and multi-polygons.
    /// See <see href="http://www.faqs.org/faqs/graphics/algorithms-faq"/>
    /// for further details of the basic approach.
    /// </summary>
    public class CentroidArea
    {
        private ICoordinate basePt = null;                       // the point all triangles are based at
        private ICoordinate triangleCent3 = new Coordinate();    // temporary variable to hold centroid of triangle
        private double areasum2 = 0;                            // Partial area sum
        private ICoordinate cg3 = new Coordinate();              // partial centroid sum

        /// <summary>
        /// 
        /// </summary>
        public CentroidArea() { }

        /// <summary> 
        /// Adds the area defined by a Geometry to the centroid total.
        /// If the point has no area it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(IGeometry geom)
        {
            if (geom is IPolygon) 
            {
                IPolygon poly = (IPolygon) geom;
                BasePoint = poly.ExteriorRing.GetCoordinateN(0);
                Add(poly);
            }
            else if (geom is IGeometryCollection) 
            {
                IGeometryCollection gc = (IGeometryCollection) geom;
                foreach (IGeometry geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary> 
        /// Adds the area defined by an array of
        /// coordinates.  The array must be a ring;
        /// i.e. end with the same coordinate as it starts with.
        /// </summary>
        /// <param name="ring">An array of Coordinates.</param>
        public void Add(ICoordinate[] ring)
        {
            BasePoint = ring[0];
            AddShell(ring);
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate Centroid
        {
            get
            {
                ICoordinate cent = new Coordinate();
                cent.X = cg3.X / 3 / areasum2;
                cent.Y = cg3.Y / 3 / areasum2;
                return cent;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private ICoordinate BasePoint
        {
            get
            {
                return this.basePt;
            }
            set
            {
                if (this.basePt == null)
                    this.basePt = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poly"></param>
        private void Add(IPolygon poly)
        {
            AddShell(poly.ExteriorRing.Coordinates);
            foreach (ILineString ls in poly.InteriorRings)
                AddHole(ls.Coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        private void AddShell(ICoordinate[] pts)
        {
            bool isPositiveArea = !CGAlgorithms.IsCCW(pts);
            for (int i = 0; i < pts.Length - 1; i++)
                AddTriangle(basePt, pts[i], pts[i + 1], isPositiveArea);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        private void AddHole(ICoordinate[] pts)
        {
            bool isPositiveArea = CGAlgorithms.IsCCW(pts);
            for (int i = 0; i < pts.Length - 1; i++)
                AddTriangle(basePt, pts[i], pts[i + 1], isPositiveArea);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="isPositiveArea"></param>
        private void AddTriangle(ICoordinate p0, ICoordinate p1, ICoordinate p2, bool isPositiveArea)
        {
            double sign = (isPositiveArea) ? 1.0 : -1.0;
            Centroid3(p0, p1, p2, ref triangleCent3);
            double area2 = Area2(p0, p1, p2);
            cg3.X += sign * area2 * triangleCent3.X;
            cg3.Y += sign * area2 * triangleCent3.Y;
            areasum2 += sign * area2;
        }

        /// <summary> 
        /// Returns three times the centroid of the triangle p1-p2-p3.
        /// The factor of 3 is
        /// left in to permit division to be avoided until later.
        /// </summary>
        private static void Centroid3(ICoordinate p1, ICoordinate p2, ICoordinate p3, ref ICoordinate c)
        {
            c.X = p1.X + p2.X + p3.X;
            c.Y = p1.Y + p2.Y + p3.Y;
            return;
        }

        /// <summary>
        /// Returns twice the signed area of the triangle p1-p2-p3,
        /// positive if a,b,c are oriented ccw, and negative if cw.
        /// </summary>
        private static double Area2(ICoordinate p1, ICoordinate p2, ICoordinate p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
        }
    }
}
