using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A utility class to get <see cref="GeometryFactory"/>s, <see cref="Envelope"/>s
    /// off of <see cref="Geometry"/>s or to build aggregate geometries.
    /// </summary>
    public class FunctionsUtil
    {

        /// <summary>
        /// Gets a default envelope
        /// </summary>
        public static readonly Envelope DefaultEnvelope = new Envelope(0, 100, 0, 100);

        /// <summary>
        /// Gets the envelope of a geometry.
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <returns>The envelope of <paramref name="g"/> or <see cref="DefaultEnvelope"/> if <c>g == null</c>.</returns>
        public static Envelope GetEnvelopeOrDefault(Geometry g)
        {
            return g == null ? DefaultEnvelope : g.EnvelopeInternal;
        }

        private static readonly GeometryFactory Factory = new GeometryFactory();

        /// <summary>
        /// Function to get the geometry factory of a geometry. If
        /// <paramref name="g"/> is <c>null</c>, a default geometry
        /// factory is returned.
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <returns>A geometry factory</returns>
        public static GeometryFactory GetFactoryOrDefault(Geometry g)
        {
            return g == null ? Factory : g.Factory;
        }

        /// <summary>
        /// Function to get the geometry factory of the first
        /// geometry in a series of geometries.<para/>
        /// If no geometry is provided in <paramref name="gs"/>,
        /// a default geometry factory is returned.
        /// </summary>
        /// <param name="gs">An enumeration of geometries</param>
        /// <returns>A geometry factory</returns>
        public static GeometryFactory GetFactoryOrDefault(IEnumerable<Geometry> gs)
        {
            if (gs == null)
                return Factory;
            foreach (var g in gs)
            {
                if (g != null)
                    return g.Factory ?? Factory;
            }
            return Factory;
        }

        /// <summary>
        /// Builds a geometry from a list of geometries.
        /// <para/>
        /// The function returns
        /// <list type="bullet">
        /// <item><term><c>null</c></term><description>if the list is <c>null</c> or empty</description></item>
        /// <item><term><c><paramref name="geoms"/>[0]</c></term><description>if the list contains one single item.</description></item>
        /// <item><term>a <see cref="GeometryCollection"/></term>if <paramref name="parentGeom"/> is a <tt>GeometryCollection</tt>.</item>
        /// <item><term>a <tt>Multi</tt>-geometry</term> in all other cases.</item>
        /// </list>
        /// </summary>
        /// <param name="geoms">A list of geometries.</param>
        /// <param name="parentGeom">A parent geometry</param>
        /// <returns>A geometry.</returns>
        [Obsolete]
        public static Geometry BuildGeometry(List<Geometry> geoms, Geometry parentGeom)
        {
            return BuildGeometry((IList<Geometry>) geoms, parentGeom);
        }

        /// <summary>
        /// Builds a geometry from a list of geometries.
        /// <para/>
        /// The function returns
        /// <list type="bullet">
        /// <item><term><c>null</c></term><description>if the list is <c>null</c> or empty</description></item>
        /// <item><term><c><paramref name="geoms"/>[0]</c></term><description>if the list contains one single item.</description></item>
        /// <item><term>a <see cref="GeometryCollection"/></term>if <paramref name="parentGeom"/> is a <tt>GeometryCollection</tt>.</item>
        /// <item><term>a <tt>Multi</tt>-geometry</term> in all other cases.</item>
        /// </list>
        /// </summary>
        /// <param name="geoms">A list of geometries.</param>
        /// <param name="parentGeom">A parent geometry</param>
        /// <returns>A geometry.</returns>
        public static Geometry BuildGeometry(IList<Geometry> geoms, Geometry parentGeom)
        {
            if (geoms.Count <= 0)
                return null;
            if (geoms.Count == 1)
                return geoms[0];

            // if parent was a GC, ensure returning a GC
            if (parentGeom != null && parentGeom.OgcGeometryType == OgcGeometryType.GeometryCollection)
                return parentGeom.Factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));

            // otherwise return MultiGeom
            return GetFactoryOrDefault(geoms).BuildGeometry(geoms);
        }

        /// <summary>
        /// Method to build a geometry. 
        /// </summary>
        /// <param name="geoms">An array of geometries</param>
        /// <returns>A <c>GEOMETRYCOLLECTION</c> containing <paramref name="geoms"/>.</returns>
        public static Geometry BuildGeometry(params Geometry[] geoms)
        {
            var gf = GetFactoryOrDefault(geoms);
            return gf.CreateGeometryCollection(geoms);
        }

        /// <summary>
        /// Method to build a geometry.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>A <c>GEOMETRYCOLLECTION</c> containing <paramref name="a"/> and <paramref name="b"/>.</returns>

        public static Geometry BuildGeometry(Geometry a, Geometry b)
        {
            int size = 0;
            if (a != null) size++;
            if (b != null) size++;
            var geoms = new Geometry[size];
            size = 0;
            if (a != null) geoms[size++] = a;
            if (b != null) geoms[size] = b;
            return GetFactoryOrDefault(geoms).CreateGeometryCollection(geoms);
        }
    }
}
