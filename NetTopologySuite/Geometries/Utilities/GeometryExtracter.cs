using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts the components of a given type from a <see cref="IGeometry"/>.
    /// </summary>
    public static class GeometryExtracter
    {
        //static bool IsOfClass<T>(Object o /*, Type clz*/) where T:class, IGeometry
        //{
        //    return o is T;
        //}

        ///<summary>
        /// Extracts the <c>T</c> components from an <see cref="IGeometry"/> and adds them to the provided <see cref="List{T}"/>.
        ///</summary>
        /// <param name="geom">the geometry from which to extract</param>
        /// <param name="list">the list to add the extracted elements to</param>
        /// <typeparam name="T">The geometry type to extract</typeparam>
        public static IList<IGeometry> Extract<T>(IGeometry geom, IList<IGeometry> list) where T : class, IGeometry
        {
            if (geom is T)
            {
                list.Add(geom as T);
            }
            else if (geom is IGeometryCollection)
            {
                geom.Apply(new GeometryExtracter<T>(list));
            }
            // skip non-T elemental geometries
            return list;
        }

        ///<summary>
        /// Extracts the <code>T</code> elements from a single <see cref="IGeometry"/> and returns them in a <see cref="List{T}"/>.
        ///</summary>
        ///<param name="geom">the geometry from which to extract</param>
        public static IList<IGeometry> Extract<T>(IGeometry geom) where T : class, IGeometry
        {
            return Extract<T>(geom, new List<IGeometry>());
        }

    }

    ///<summary>
    /// Extracts the components of type <c>T</c> from a <see cref="IGeometry"/>.
    ///</summary>
    public class GeometryExtracter<T> : IGeometryFilter
        where T: IGeometry
    {

        //private readonly Type _clz;
        private readonly IList<IGeometry> _comps;

        ///<summary>
        /// Constructs a filter with a list in which to store the elements found.
        ///</summary>
        /// <param name="comps">The list to extract into</param>
        public GeometryExtracter(/*Type clz, */IList<IGeometry> comps)
        {
            //_clz = clz;
            _comps = comps;
        }

        public void Filter(IGeometry geom)
        {
            if (geom is T) _comps.Add((T)geom);
        }

    }
}
