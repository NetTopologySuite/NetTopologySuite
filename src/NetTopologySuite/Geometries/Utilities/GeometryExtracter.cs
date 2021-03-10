using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts the components of a given type from a <see cref="Geometry"/>.
    /// </summary>
    public static class GeometryExtracter
    {


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
        /// Extracts the <c>T</c> elements from a single <see cref="Geometry"/> and returns them in a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="geom">the geometry from which to extract</param>
        public static IList<Geometry> Extract<T>(Geometry geom) where T : Geometry
        {
            return Extract<T>(geom, new List<Geometry>());
        }

        /// <summary>
        /// Extracts the components of <tt>geometryType</tt> from a <see cref="Geometry"/>
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="geometryType">Geometry type to extract (null or all white-space means all types)</param>
        public static IList<Geometry> Extract(Geometry geom, string geometryType)
        {
            return Extract(geom, geometryType, new List<Geometry>());
        }

        /// <summary>
        /// Extracts the components of <tt>geometryType</tt> from a <see cref="Geometry"/>
        /// and adds them to the provided <see cref="IList{T}"/>
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="geometryType">Geometry type to extract (null or all white-space means all types)</param>
        /// <param name="list">The list to add the extracted elements to</param>
        public static IList<Geometry> Extract(Geometry geom, string geometryType, IList<Geometry> list)
        {
            if (geom.GeometryType == geometryType)
            {
                list.Add(geom);
            }
            else if (geom is GeometryCollection) {
                geom.Apply(new GeometryExtracterByTypeName(geometryType, list));
            }
            // skip non-LineString elemental geometries

            return list;
        }

    }

    /// <summary>
    /// Extracts the components of type <c>T</c> from a <see cref="Geometry"/>.
    /// </summary>
    public class GeometryExtracterByTypeName : IGeometryFilter
    {

        private readonly string _geometryType;
        private readonly IList<Geometry> _comps;

        /// <summary>
        /// Constructs a filter with a list in which to store the elements found.
        /// </summary>
        /// <param name="geometryType">Geometry type to extract (null means all types)</param>
        /// <param name="comps">The list to extract into</param>
        public GeometryExtracterByTypeName(string geometryType, IList<Geometry> comps)
        {
            _geometryType = geometryType;
            _comps = comps;
        }

        /// <summary>
        /// Y
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="geometryType"></param>
        /// <returns></returns>
        protected static bool IsOfType(Geometry geom, string geometryType)
        {
            if (geom.GeometryType == geometryType) return true;
            if (geometryType == Geometry.TypeNameLineString
                && geom.GeometryType == Geometry.TypeNameLinearRing) return true;
            return false;
        }

        /// <inheritdoc cref="IGeometryFilter.Filter"/>
        public void Filter(Geometry geom)
        {
            if (string.IsNullOrWhiteSpace(_geometryType) || IsOfType(geom, _geometryType))
                _comps.Add(geom);
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
