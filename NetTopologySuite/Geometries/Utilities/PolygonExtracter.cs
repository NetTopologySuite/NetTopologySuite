using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the <see cref="IPolygon"/> elements from a <see cref="IGeometry"/>.
    /// </summary>
    /// <see cref="GeometryExtracter"/>
    public class PolygonExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts the <see cref="IPolygon"/> elements from a single <see cref="IGeometry"/> and adds them to the provided <see cref="IList{IPolygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static IList<IGeometry> GetPolygons(IGeometry geom, IList<IGeometry> list)
        {
            if (geom is IPolygon)
            {
                list.Add(geom);
            }
            else if (geom is IGeometryCollection)
            {
                geom.Apply(new PolygonExtracter(list));
            }
            // skip non-Polygonal elemental geometries

            return list;
        }

        /// <summary>
        /// Extracts the <see cref="IPolygon"/> elements from a single <see cref="IGeometry"/> and returns them in a <see cref="IList{IPolygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        public static IList<IGeometry> GetPolygons(IGeometry geom)
        {
            return GetPolygons(geom, new List<IGeometry>());
        }

        private readonly IList<IGeometry> _comps;

        /// <summary>
        /// Constructs a PolygonExtracterFilter with a list in which to store Polygons found.
        /// </summary>
        /// <param name="comps"></param>
        public PolygonExtracter(IList<IGeometry> comps)
        {
            _comps = comps;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(IGeometry geom)
        {
            if (geom is IPolygon)
                _comps.Add(geom);
        }
    }
}
