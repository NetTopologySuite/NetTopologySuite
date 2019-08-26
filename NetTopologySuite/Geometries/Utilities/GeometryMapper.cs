using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Methods to map various collections
    /// of <see cref="Geometry"/>s
    /// via defined mapping functions.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryMapper
    {
        /// <summary>
        /// Maps the members of a <see cref="Geometry"/>
        /// (which may be atomic or composite)
        /// into another <tt>Geometry</tt> of most specific type.
        /// <tt>null</tt> results are skipped.
        /// In the case of hierarchical <see cref="GeometryCollection"/>s,
        /// only the first level of members are mapped.
        /// </summary>
        /// <param name="geom">The input atomic or composite geometry</param>
        /// <param name="op">The mapping operation delegate</param>
        /// <returns>A result collection or geometry of most specific type</returns>
        public static Geometry Map(Geometry geom, Func<Geometry, Geometry> op)
        {
            var mapped = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = op(geom);
                if (g != null)
                    mapped.Add(g);
            }
            return geom.Factory.BuildGeometry(mapped);
        }

        /// <summary>
        /// Maps the members of a <see cref="Geometry"/>
        /// (which may be atomic or composite)
        /// into another <tt>Geometry</tt> of most specific type.
        /// <tt>null</tt> results are skipped.
        /// In the case of hierarchical <see cref="GeometryCollection"/>s,
        /// only the first level of members are mapped.
        /// </summary>
        /// <param name="geom">The input atomic or composite geometry</param>
        /// <param name="op">The mapping operation</param>
        /// <returns>A result collection or geometry of most specific type</returns>
        public static Geometry Map(Geometry geom, IMapOp op)
        {
            var mapped = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = op.Map(geom.GetGeometryN(i));
                if (g != null)
                    mapped.Add(g);
            }
            return geom.Factory.BuildGeometry(mapped);
        }

        public static ReadOnlyCollection<Geometry> Map(IEnumerable<Geometry> geoms, IMapOp op)
        {
            var mapped = new List<Geometry>();
            foreach (var g in geoms)
            {
                var gr = op.Map(g);
                if (gr != null)
                    mapped.Add(gr);
            }
            return mapped.AsReadOnly();
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
            Geometry Map(Geometry g);
        }
    }
}
