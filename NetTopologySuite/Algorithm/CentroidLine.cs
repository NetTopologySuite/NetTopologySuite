using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of a linear point.
    /// Algorithm:
    /// Compute the average of the midpoints
    /// of all line segments weighted by the segment length.
    /// </summary>
    public class CentroidLine
    {
        private ICoordinate centSum = new Coordinate();
        private double totalLength = 0.0;

        /// <summary>
        /// 
        /// </summary>
        public CentroidLine() { }

        /// <summary> 
        /// Adds the linestring(s) defined by a Geometry to the centroid total.
        /// If the point is not linear it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(IGeometry geom)
        {
            if (geom is ILineString)             
                Add(geom.Coordinates);            

            else if (geom is IGeometryCollection) 
            {
                IGeometryCollection gc = (IGeometryCollection) geom;
                foreach (IGeometry geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate Centroid
        {
            get
            {
                ICoordinate cent = new Coordinate();
                cent.X = centSum.X / totalLength;
                cent.Y = centSum.Y / totalLength;
                return cent;
            }
        }

        /// <summary> 
        /// Adds the length defined by an array of coordinates.
        /// </summary>
        /// <param name="pts">An array of <c>Coordinate</c>s.</param>
        public void Add(ICoordinate[] pts)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                double segmentLen = pts[i].Distance(pts[i + 1]);
                totalLength += segmentLen;

                double midx = (pts[i].X + pts[i + 1].X) / 2;
                centSum.X += segmentLen * midx;
                double midy = (pts[i].Y + pts[i + 1].Y) / 2;
                centSum.Y += segmentLen * midy;
            }
        }
    }
}
