using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Maps the members of a <see cref="IGeometryCollection"/>
    /// into another <tt>GeometryCollection</tt> via a defined
    /// mapping function.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryCollectionMapper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        public delegate IGeometry MapGeometryDelegate(IGeometry geometry);

        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static IGeometryCollection Map(IGeometryCollection gc, MapGeometryDelegate op)
        {
            var mapper = new GeometryCollectionMapper(op);
            return mapper.Map(gc);
        }

        private readonly MapGeometryDelegate _mapOp;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="mapOp"></param>
        public GeometryCollectionMapper(MapGeometryDelegate mapOp)
        {
            _mapOp = mapOp;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        public IGeometryCollection Map(IGeometryCollection gc)
        {
            IList<IGeometry> mapped = new List<IGeometry>();
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                IGeometry g = _mapOp(gc.GetGeometryN(i));
                if (!g.IsEmpty)
                    mapped.Add(g);
            }
            return gc.Factory.CreateGeometryCollection(
                GeometryFactory.ToGeometryArray(mapped));
        }
    }
}