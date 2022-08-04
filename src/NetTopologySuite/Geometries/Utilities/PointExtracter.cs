using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the 0-dimensional (<c>Point</c>) components from a <c>Geometry</c>.
    /// </summary>
    /// <seealso cref="GeometryExtracter"/>
    /// <seealso cref="Extracter"/>
    /// <seealso cref="Extracter{T}"/>
    public class PointExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts the <see cref="Point"/> elements from a single <see cref="Geometry"/> and adds them to the provided <see cref="ICollection{Point}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static TCollection GetPoints<TCollection>(Geometry geom, TCollection list)
            where TCollection : ICollection<Geometry>
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
        public static ReadOnlyCollection<Geometry> GetPoints(Geometry geom)
        {
            var points = GetPoints(geom, new List<Geometry>());
            return points.AsReadOnly();
        }

        private readonly ICollection<Geometry> _pts;

        /// <summary>
        /// Constructs a PointExtracterFilter with a list in which to store Points found.
        /// </summary>
        /// <param name="pts"></param>
        public PointExtracter(ICollection<Geometry> pts)
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
