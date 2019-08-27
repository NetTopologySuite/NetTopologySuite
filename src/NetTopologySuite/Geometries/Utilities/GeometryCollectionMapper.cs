using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Maps the members of a <see cref="GeometryCollection"/>
    /// into another <tt>GeometryCollection</tt> via a defined
    /// mapping function.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryCollectionMapper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static GeometryCollection Map(GeometryCollection gc, Func<Geometry, Geometry> op)
        {
            var mapper = new GeometryCollectionMapper(op);
            return mapper.Map(gc);
        }

        private readonly Func<Geometry, Geometry> _mapOp;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="mapOp"></param>
        public GeometryCollectionMapper(Func<Geometry, Geometry> mapOp)
        {
            _mapOp = mapOp;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        public GeometryCollection Map(GeometryCollection gc)
        {
            var mapped = new List<Geometry>();
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = _mapOp(gc.GetGeometryN(i));
                if (!g.IsEmpty)
                    mapped.Add(g);
            }
            return gc.Factory.CreateGeometryCollection(
                GeometryFactory.ToGeometryArray(mapped));
        }
    }
}