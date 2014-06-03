using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Samples.Lab.Clean
{
    /// <summary>
    /// Removes holes which are invalid due to not being wholly covered by the parent shell.
    /// </summary>
    /// <remarks>
    /// Does not remove holes which are invalid due to touching other rings at more than one point
    /// </remarks>
    /// <author>Martin Davis</author>
    public class InvalidHoleRemover
    {
        /// <summary>
        /// Removes invalid holes from the polygons in a geometry.
        /// </summary>
        /// <param name="geom">The geometry to clean</param>
        /// <returns>The geometry with invalid holes removed</returns>
        public static IGeometry Clean(IGeometry geom)
        {
            InvalidHoleRemover pihr = new InvalidHoleRemover(geom);
            return pihr.GetResult();
        }

        private readonly IGeometry _geom;

        /// <summary>
        /// Creates a new invalid hole remover instance.
        /// </summary>
        /// <param name="geom">The geometry to process</param>
        public InvalidHoleRemover(IGeometry geom)
        {
            _geom = geom;
        }

        /// <summary>
        /// Gets the cleaned geometry.
        /// </summary>
        /// <returns>The geometry with invalid holes removed.</returns>
        public IGeometry GetResult()
        {
            return GeometryMapper.Map(_geom, new InvalidHoleRemoverMapOp());
        }

        private class InvalidHoleRemoverMapOp : GeometryMapper.IMapOp
        {
            public IGeometry Map(IGeometry geom)
            {
                if (geom is IPolygon)
                {
                    IPolygon poly = (IPolygon)geom;
                    return PolygonInvalidHoleRemover.Clean(poly);
                }
                return geom;
            }
        }

        private class PolygonInvalidHoleRemover
        {
            public static IPolygon Clean(IPolygon poly)
            {
                PolygonInvalidHoleRemover pihr = new PolygonInvalidHoleRemover(poly);
                return pihr.GetResult();
            }

            private readonly IPolygon _poly;

            private PolygonInvalidHoleRemover(IPolygon poly)
            {
                _poly = poly;
            }

            private IPolygon GetResult()
            {
                IGeometryFactory gf = _poly.Factory;
                ILinearRing shell = (ILinearRing)_poly.ExteriorRing;
                IPreparedGeometry shellPrep = PreparedGeometryFactory.Prepare(gf.CreatePolygon(shell));

                IList<IGeometry> holes = new List<IGeometry>();
                for (int i = 0; i < _poly.NumInteriorRings; i++)
                {
                    IGeometry hole = _poly.GetInteriorRingN(i);
                    if (shellPrep.Covers(hole))
                        holes.Add(hole);
                }
                // all holes valid, so return original
                if (holes.Count == _poly.NumInteriorRings)
                    return _poly;

                // return new polygon with covered holes only
                ILinearRing[] arr = GeometryFactory.ToLinearRingArray(holes);
                IPolygon result = gf.CreatePolygon(shell, arr);
                return result;
            }
        }
    }
}