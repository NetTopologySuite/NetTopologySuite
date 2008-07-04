using System.Collections;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 0-dimensional (<c>Point</c>) components from a <c>Geometry</c>.    
    /// </summary>
    public class PointExtracter : IGeometryFilter
    {
        /// <summary> 
        /// Returns the Point components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <c>PointExtracterFilter</c> instance
        /// and pass it to multiple geometries.
        /// </summary>
        /// <param name="geom"></param>
        public static IList GetPoints(IGeometry geom)
        {
            IList pts = new ArrayList();
            geom.Apply(new PointExtracter(pts));
            return pts;
        }

        private IList pts;

        /// <summary> 
        /// Constructs a PointExtracterFilter with a list in which to store Points found.
        /// </summary>
        /// <param name="pts"></param>
        public PointExtracter(IList pts)
        {
            this.pts = pts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(IGeometry geom)
        {
            if (geom is IPoint)
                pts.Add(geom);
        }
    }
}
