using System;
using System.Collections;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 0-dimensional (<c>Point</c>) components from a <see cref="Geometry{TCoordinate}"/>.    
    /// </summary>
    public class PointExtracter<TCoordinate> : IGeometryFilter<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                             IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Returns the Point components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <c>PointExtracterFilter</c> instance
        /// and pass it to multiple geometries.
        /// </summary>
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
        public PointExtracter(IList pts)
        {
            this.pts = pts;
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is IPoint)
            {
                pts.Add(geom);
            }
        }
    }
}