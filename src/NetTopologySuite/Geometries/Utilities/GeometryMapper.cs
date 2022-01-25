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
        /// Maps the atomic elements of a <see cref="Geometry"/>
        /// (which may be atomic or composite)
        /// using a <see cref="IMapOp"/> mapping operation
        /// into an atomic <tt>Geometry</tt> or a flat collection
        /// of the most specific type.
        /// <tt>null</tt> and empty values returned from the mapping operation
        /// are discarded.
        /// </summary>
        /// <param name="geom">The geometry to map</param>
        /// <param name="emptyDim">The dimension of empy geometry to create</param>
        /// <param name="op">The mapping operation</param>
        /// <returns>The mapped result</returns>
        public static Geometry FlatMap(Geometry geom, Dimension emptyDim, IMapOp op)
        {
            var mapped = new List<Geometry>();
            FlatMap(geom, op, mapped);

            if (mapped.Count == 0)
            {
                return geom.Factory.CreateEmpty(emptyDim);
            }
            if (mapped.Count == 1)
                return mapped[0];
            return geom.Factory.BuildGeometry(mapped);
        }

        private static void FlatMap(Geometry geom, IMapOp op, List<Geometry> mapped)
        {
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = geom.GetGeometryN(i);
                if (g is GeometryCollection)
                {
                    FlatMap(g, op, mapped);
                }
                else
                {
                    var res = op.Map(g);
                    if (res != null && !res.IsEmpty)
                    {
                        AddFlat(res, mapped);
                    }
                }
            }
        }

        private static void AddFlat(Geometry geom, List<Geometry> geomList)
        {
            if (geom.IsEmpty) return;
            if (geom is GeometryCollection)
            {
                for (int i = 0; i < geom.NumGeometries; i++)
                {
                    AddFlat(geom.GetGeometryN(i), geomList);
                }
            }
            else
            {
                geomList.Add(geom);
            }
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

        /// <summary>
        /// Standard implementation of a geometry mapping 
        /// </summary>
        public sealed class MapOp : IMapOp
        {
            private readonly Func<Geometry, Geometry> _mapOp;

            /// <summary>
            /// Creates an instance of this class using the provided mapping operation function
            /// </summary>
            /// <param name="mapOp">A mapping operation function</param>
            public MapOp(Func<Geometry, Geometry> mapOp)
            {
                _mapOp = mapOp;
            }

            /// <summary>
            /// Computes a new geometry value.
            /// </summary>
            /// <param name="g">The input geometry</param>
            /// <returns>A result geometry</returns>
            public Geometry Map(Geometry geom) => _mapOp(geom);
        }
    }
}
