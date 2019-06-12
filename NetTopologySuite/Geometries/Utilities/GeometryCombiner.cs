using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Combines <see cref="Geometry"/>s to produce a <see cref="GeometryCollection"/> of the most appropriate type.
    /// </summary>
    /// <remarks>
    /// <para>Input geometries which are already collections will have their elements extracted first.</para>
    /// <para>No validation of the result geometry is performed.
    /// (The only case where invalidity is possible is where <see cref="IPolygonal"/> geometries are combined and result in a self-intersection).</para>
    /// </remarks>
    /// <author>mbdavis</author>
    /// <seealso cref="GeometryFactory.BuildGeometry"/>
    public class GeometryCombiner
    {
        /// <summary>Combines a collection of geometries.</summary>
        /// <param name="geoms">The geometries to combine</param>
        /// <returns>The combined geometry</returns>
        public static Geometry Combine(IEnumerable<Geometry> geoms)
        {
            var combiner = new GeometryCombiner(geoms);
            return combiner.Combine();
        }

        /// <summary>
        ///Combines two geometries.
        /// </summary>
        /// <param name="g0">A geometry to combine</param>
        /// <param name="g1">A geometry to combine</param>
        /// <returns>The combined geometry</returns>
        public static Geometry Combine(Geometry g0, Geometry g1)
        {
            var combiner = new GeometryCombiner(CreateList(g0, g1));
            return combiner.Combine();
        }

        /// <summary>
        ///Combines three geometries.
        /// </summary>
        /// <param name="g0">A geometry to combine</param>
        /// <param name="g1">A geometry to combine</param>
        /// <param name="g2">A geometry to combine</param>
        /// <returns>The combined geometry</returns>
        public static Geometry Combine(Geometry g0, Geometry g1, Geometry g2)
        {
            var combiner = new GeometryCombiner(CreateList(g0, g1, g2));
            return combiner.Combine();
        }

       /// <summary>
        /// Creates a list from two items
        /// </summary>
        /// <param name="obj0"></param>
        /// <param name="obj1"></param>
        /// <returns>A list from two geometries</returns>
        private static List<Geometry> CreateList(Geometry obj0, Geometry obj1)
        {
            return new List<Geometry> {obj0, obj1};
        }

        /// <summary>
        /// Creates a list from three items
        /// </summary>
        /// <param name="obj0"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>A list from three geometries</returns>
        private static List<Geometry> CreateList(Geometry obj0, Geometry obj1, Geometry obj2)
        {
            return new List<Geometry> {obj0, obj1, obj2};
        }

        /// <summary>
        /// Value indicating whether empty geometries should be skipped
        /// </summary>
        public static bool SkipEmpty { get; set; }

        private readonly IEnumerable<Geometry> _inputGeoms;

        /// <summary>
        /// Creates a new combiner for a collection of geometries
        /// </summary>
        /// <param name="geoms">The geometries to combine</param>
        public GeometryCombiner(IEnumerable<Geometry> geoms)
        {
            // DEVIATION: JTS uses ExtractFactory here, but if we wait to do so
            // until we actually loop over it, then this can be IEnumerable<T>
            // without having to iterate over it multiple times.
            _inputGeoms = geoms;
        }

        /// <summary>
        /// Extracts the GeometryFactory used by the geometries in a collection
        /// </summary>
        /// <param name="geoms"></param>
        /// <returns>a GeometryFactory</returns>
        public static GeometryFactory ExtractFactory(IEnumerable<Geometry> geoms)
        {
            foreach (var geom in geoms)
            {
                return geom.Factory;
            }

            return null;
        }

        /// <summary>
        /// Computes the combination of the input geometries to produce the most appropriate <see cref="Geometry"/> or <see cref="GeometryCollection"/>
        /// </summary>
        /// <returns>A Geometry which is the combination of the inputs</returns>
        /// <returns></returns>
        public Geometry Combine()
        {
            var elems = new List<Geometry>();

            bool first = true;
            GeometryFactory geomFactory = null;
            foreach (var geom in _inputGeoms)
            {
                if (first)
                {
                    geomFactory = geom.Factory;
                    first = false;
                }

                ExtractElements(geom, elems);
            }

            if (elems.Count == 0)
                return geomFactory?.CreateGeometryCollection();
            return geomFactory.BuildGeometry(elems);
        }

        private static void ExtractElements(Geometry geom, List<Geometry> elems)
        {
            if (geom == null)
                return;

            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var elemGeom = geom.GetGeometryN(i);
                if (SkipEmpty && elemGeom.IsEmpty)
                    continue;
                elems.Add(elemGeom);
            }
        }

    }
}
