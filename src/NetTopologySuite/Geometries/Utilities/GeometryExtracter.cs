using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts the components of a given type from a <see cref="Geometry"/>.
    /// </summary>
    public static class GeometryExtracter
    {
        //static bool IsOfClass<T>(Object o /*, Type clz*/) where T:class, Geometry
        //{
        //    return o is T;
        //}

        /// <summary>
        /// Extracts the <c>T</c> components from an <see cref="Geometry"/> and adds them to the provided <see cref="List{T}"/>.
        /// </summary>
        /// <param name="geom">the geometry from which to extract</param>
        /// <param name="list">the list to add the extracted elements to</param>
        /// <typeparam name="T">The geometry type to extract</typeparam>
        public static IList<Geometry> Extract<T>(Geometry geom, IList<Geometry> list) where T : Geometry
        {
            if (geom is T)
            {
                list.Add(geom as T);
            }
            else if (geom is GeometryCollection)
            {
                geom.Apply(new GeometryExtracter<T>(list));
            }
            // skip non-T elemental geometries
            return list;
        }

        /// <summary>
        /// Extracts the <code>T</code> elements from a single <see cref="Geometry"/> and returns them in a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="geom">the geometry from which to extract</param>
        public static IList<Geometry> Extract<T>(Geometry geom) where T : Geometry
        {
            return Extract<T>(geom, new List<Geometry>());
        }

    }

    /// <summary>
    /// Extracts the components of type <c>T</c> from a <see cref="Geometry"/>.
    /// </summary>
    public class GeometryExtracter<T> : IGeometryFilter
        where T: Geometry
    {

        //private readonly Type _clz;
        private readonly IList<Geometry> _comps;

        /// <summary>
        /// Constructs a filter with a list in which to store the elements found.
        /// </summary>
        /// <param name="comps">The list to extract into</param>
        public GeometryExtracter(/*Type clz, */IList<Geometry> comps)
        {
            //_clz = clz;
            _comps = comps;
        }

        public void Filter(Geometry geom)
        {
            if (geom is T) _comps.Add((T)geom);
        }

    }
}
