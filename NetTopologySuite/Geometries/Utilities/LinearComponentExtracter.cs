using System.Collections;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 1-dimensional (<c>LineString</c>) components from a <c>Geometry</c>.
    /// </summary>
    public class LinearComponentExtracter : IGeometryComponentFilter
    {
        /// <summary> 
        /// Extracts the linear components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <c>LineExtracterFilter</c> instance
        /// and pass it to multiple geometries.
        /// </summary>
        /// <param name="geom">The point from which to extract linear components.</param>
        /// <returns>The list of linear components.</returns>
        public static IList GetLines(IGeometry geom)
        {
            IList lines = new ArrayList();
            geom.Apply(new LinearComponentExtracter(lines));
            return lines;
        }

        private IList lines;

        /// <summary> 
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        /// <param name="lines"></param>
        public LinearComponentExtracter(IList lines)
        {
            this.lines = lines;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(IGeometry geom)
        {
            if (geom is ILineString) 
                lines.Add(geom);
        }
    }
}
