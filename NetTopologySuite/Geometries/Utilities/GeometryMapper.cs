using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Methods to map various collections
    /// of <see cref="IGeometry"/>s
    /// via defined mapping functions.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryMapper
    {
        /// <summary>
        /// Maps the members of a <see cref="IGeometry"/>
        /// (which may be atomic or composite)
        /// into another <tt>Geometry</tt> of most specific type.
        /// <tt>null</tt> results are skipped.
        /// In the case of hierarchical <see cref="IGeometryCollection"/>s,
        /// only the first level of members are mapped.
        /// </summary>
        /// <param name="geom">The input atomic or composite geometry</param>
        /// <param name="op">The mapping operation delegate</param>
        /// <returns>A result collection or geometry of most specific type</returns>
        public static IGeometry Map(IGeometry geom, Func<IGeometry, IGeometry> op)
        {
            var mapped = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = op(geom);
                if (g != null)
                    mapped.Add(g);
            }
            return geom.Factory.BuildGeometry(mapped);
        }

        /// <summary>
        /// Maps the members of a <see cref="IGeometry"/>
        /// (which may be atomic or composite)
        /// into another <tt>Geometry</tt> of most specific type.
        /// <tt>null</tt> results are skipped.
        /// In the case of hierarchical <see cref="IGeometryCollection"/>s,
        /// only the first level of members are mapped.
        /// </summary>
        /// <param name="geom">The input atomic or composite geometry</param>
        /// <param name="op">The mapping operation</param>
        /// <returns>A result collection or geometry of most specific type</returns>
        public static IGeometry Map(IGeometry geom, IMapOp op)
        {
            var mapped = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = op.Map(geom.GetGeometryN(i));
                if (g != null)
                    mapped.Add(g);
            }
            return geom.Factory.BuildGeometry(mapped);
        }

        public static ICollection<IGeometry> Map(ICollection<IGeometry> geoms, IMapOp op)
        {
            var mapped = new List<IGeometry>();
            foreach (var g in geoms)
            {
                var gr = op.Map(g);
                if (gr != null)
                    mapped.Add(gr);
            }
            return mapped;
        }

        /// <summary>
        /// An interface for geometry functions used for mapping.
        /// </summary>
        /// <author>Martin Davis</author>
        public interface IMapOp
        {
            /// <summary>
            /// Computes a new geometry value.
            /// </summary>
            /// <param name="g">The input geometry</param>
            /// <returns>A result geometry</returns>
            IGeometry Map(IGeometry g);
        }
    }
}