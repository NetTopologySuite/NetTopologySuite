using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    public static class GeometryExtracter
    {

        static bool IsOfClass<T>(Object o /*, Type clz*/) where T:class, IGeometry
        {
            return o is T;
        }

        ///<summary>
        /// Extracts the <see cref="T"/> elements from a single <see cref="IGeometry"/> and adds them to the provided <see cref="List{T}"/>.
        ///</summary>
        /// <param name="geom">the geometry from which to extract</param>
        /// <param name="list">the list to add the extracted elements to</param>
        /// <typeparam name="T">The geometry type to extract</typeparam>
        public static IList<T> Extract<T>(IGeometry geom, IList<T> list) where T : class, IGeometry
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
        /// Extracts the <see cref="T"/> elements from a single <see cref="IGeometry"/> and returns them in a <see cref="List{T}"/>.
        ///</summary>
        ///<param name="geom">the geometry from which to extract</param>
        public static IList<T> Extract<T>(IGeometry geom) where T : class, IGeometry
        {
            return Extract(geom, new List<T>());
        }

    }

    ///<summary>
    /// Extracts all the <see cref="T"/> elements from a <see cref="IGeometry"/>.
    ///</summary>
    public class GeometryExtracter<T> : IGeometryFilter
        where T: IGeometry
    {

        //private readonly Type _clz;
        private readonly IList<T> _comps;
        /**
         * 
         */
        ///<summary>
        /// Constructs a filter with a list in which to store the elements found.
        ///</summary>
        public GeometryExtracter(/*Type clz, */IList<T> comps)
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