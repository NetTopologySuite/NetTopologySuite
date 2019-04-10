using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the 0-dimensional (<c>Point</c>) components from a <c>Geometry</c>.
    /// </summary>
    /// <see cref="GeometryExtracter"/>
    public class PointExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts the <see cref="Point"/> elements from a single <see cref="Geometry"/> and adds them to the provided <see cref="IList{Point}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static ICollection<Geometry> GetPoints(Geometry geom, List<Geometry> list)
        {
            if (geom is Point)
            {
                list.Add(geom);
            }
            else if (geom is GeometryCollection)
            {
                geom.Apply(new PointExtracter(list));
            }
            // skip non-Polygonal elemental geometries

            return list;
        }

        /// <summary>
        /// Extracts the <see cref="Point"/> elements from a single <see cref="Geometry"/> and returns them in a <see cref="IList{Point}"/>.
        /// </summary>
        /// <param name="geom">the geometry from which to extract</param>
        public static ICollection<Geometry> GetPoints(Geometry geom)
        {
            return GetPoints(geom, new List<Geometry>());
        }

        private readonly List<Geometry> _pts;

        /// <summary>
        /// Constructs a PointExtracterFilter with a list in which to store Points found.
        /// </summary>
        /// <param name="pts"></param>
        public PointExtracter(List<Geometry> pts)
        {
            _pts = pts;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(Geometry geom)
        {
            if (geom is Point)
                _pts.Add(geom);
        }
    }
}
