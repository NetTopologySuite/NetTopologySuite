using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// This class offers utility methods to extract single component
    /// geometries of requested type from ordinary geometries.
    /// </summary>
    /// <seealso cref="PolygonExtracter"/>
    /// <seealso cref="LineStringExtracter"/>
    /// <seealso cref="PointExtracter"/>
    /// <seealso cref="GeometryExtracter"/>
    public static class Extracter
    {
        /// <summary>
        /// Extracts the <see cref="Point"/> elements from a single <see cref="Geometry"/>
        /// and adds them to the provided <see cref="IList{Point}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static IList<Point> GetPoints(Geometry geom, IList<Point> list)
            => Extracter<Point>.GetItems(geom, list);

        /// <summary>
        /// Extracts the <see cref="Point"/> elements from a single <see cref="Geometry"/>
        /// and returns them in a <see cref="IList{Point}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        public static IList<Point> GetPoints(Geometry geom)
            => Extracter<Point>.GetItems(geom);

        /// <summary>
        /// Extracts the <see cref="Polygon"/> elements from a single <see cref="Geometry"/>
        /// and adds them to the provided <see cref="IList{Polygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static IList<LineString> GetLines(Geometry geom, IList<LineString> list)
            => Extracter<LineString>.GetItems(geom, list);

        /// <summary>
        /// Extracts the <see cref="LineString"/> elements from a single <see cref="Geometry"/>
        /// and returns them in a <see cref="IList{LineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        public static IList<LineString> GetLines(Geometry geom)
            => Extracter<LineString>.GetItems(geom);

        /// <summary>
        /// Extracts the <see cref="Polygon"/> elements from a single <see cref="Geometry"/>
        /// and adds them to the provided <see cref="IList{Polygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        /// <returns></returns>
        public static IList<Polygon> GetPolygons(Geometry geom, IList<Polygon> list)
            => Extracter<Polygon>.GetItems(geom, list);


        /// <summary>
        /// Extracts the <see cref="Polygon"/> elements from a single
        /// <see cref="Geometry"/> and returns them in a <see cref="IList{Polygon}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        public static IList<Polygon> GetPolygons(Geometry geom)
            => Extracter<Polygon>.GetItems(geom);
    }

    /// <summary>
    /// Class to extract single instance geometries of <see cref="T"/>
    /// </summary>
    /// <typeparam name="T">The type of the geometries to extract</typeparam>
    public sealed class Extracter<T> : IGeometryFilter where T : Geometry
    {
        /// <summary>
        /// Extracts the <typeparamref name="T"/>-geometry elements from a single
        /// <see cref="Geometry"/> and adds them to the provided <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="comps">The list to add the extracted elements to</param>
        /// <returns><paramref name="comps"/>, extended by <c>T</c>-geometries of <paramref name="geom"/></returns>
        public static IList<T> GetItems(Geometry geom, IList<T> comps)
        {
            if (geom is T tg)
                comps.Add(tg);
            else if (geom is GeometryCollection)
                geom.Apply(new Extracter<T>(comps));
            // skip non-T elemental geometries

            return comps;
        }

        /// <summary>
        /// Extracts the <typeparamref name="T"/>-geometry elements from a single
        /// <see cref="Geometry"/> and adds them to the provided <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <returns>A list of <c>T</c>-geometry elements in <paramref name="geom"/></returns>
        public static IList<T> GetItems(Geometry geom)
            => GetItems(geom, new List<T>(geom.NumGeometries));


        private readonly IList<T> _comps;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="comps">The list to add <typeparamref name="T"/>-geometry elements to</param>
        /// <exception cref="InvalidOperationException"></exception>
        private Extracter(IList<T> comps)
        {
            if (typeof(T).IsInstanceOfType(typeof(GeometryCollection)))
                throw new InvalidOperationException("Invalid geometry type");

            _comps = comps;
        }

        /// <summary>
        /// Adds <paramref name="geom"/> to the list when it is a <typeparamref name="T"/>-geometry.
        /// </summary>
        public void Filter(Geometry geom)
        {
            if (geom is T tg)
                _comps.Add(tg);
        }
    }
}
