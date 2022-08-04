using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the <see cref="Polygon"/> elements from a <see cref="Geometry"/>.
    /// </summary>
    /// <seealso cref="GeometryExtracter"/>
    /// <seealso cref="Extracter"/>
    /// <seealso cref="Extracter{T}"/>
    public class PolygonExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts the <see cref="Polygon"/> elements from a single <see cref="Geometry"/> and adds them to the provided <see cref="IList{Polygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static IList<Geometry> GetPolygons(Geometry geom, IList<Geometry> list)
        {
            if (geom is Polygon)
            {
                list.Add(geom);
            }
            else if (geom is GeometryCollection)
            {
                geom.Apply(new PolygonExtracter(list));
            }
            // skip non-Polygonal elemental geometries

            return list;
        }

        /// <summary>
        /// Extracts the <see cref="Polygon"/> elements from a single <see cref="Geometry"/> and returns them in a <see cref="IList{Polygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        public static IList<Geometry> GetPolygons(Geometry geom)
        {
            return GetPolygons(geom, new List<Geometry>());
        }

        private readonly IList<Geometry> _comps;

        /// <summary>
        /// Constructs a PolygonExtracterFilter with a list in which to store Polygons found.
        /// </summary>
        /// <param name="comps"></param>
        public PolygonExtracter(IList<Geometry> comps)
        {
            _comps = comps;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(Geometry geom)
        {
            if (geom is Polygon)
                _comps.Add(geom);
        }
    }
}
