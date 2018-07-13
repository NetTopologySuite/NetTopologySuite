using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the 0-dimensional (<c>Point</c>) components from a <c>Geometry</c>.
    /// </summary>
    /// <see cref="GeometryExtracter"/>
    public class PointExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts the <see cref="IPoint"/> elements from a single <see cref="IGeometry"/> and adds them to the provided <see cref="IList{IPoint}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static ICollection<IGeometry> GetPoints(IGeometry geom, List<IGeometry> list)
        {
            if (geom is IPoint)
            {
                list.Add(geom);
            }
            else if (geom is IGeometryCollection)
            {
                geom.Apply(new PointExtracter(list));
            }
            // skip non-Polygonal elemental geometries

            return list;
        }

        /// <summary>
        /// Extracts the <see cref="IPoint"/> elements from a single <see cref="IGeometry"/> and returns them in a <see cref="IList{IPoint}"/>.
        /// </summary>
        /// <param name="geom">the geometry from which to extract</param>
        public static ICollection<IGeometry> GetPoints(IGeometry geom)
        {
            return GetPoints(geom, new List<IGeometry>());
        }

        private readonly List<IGeometry> _pts;

        /// <summary>
        /// Constructs a PointExtracterFilter with a list in which to store Points found.
        /// </summary>
        /// <param name="pts"></param>
        public PointExtracter(List<IGeometry> pts)
        {
            _pts = pts;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(IGeometry geom)
        {
            if (geom is IPoint)
                _pts.Add(geom);
        }
    }
}
