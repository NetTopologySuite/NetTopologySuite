using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Samples.Lab.Clean
{
    /// <summary>
    /// Removes holes which are invalid due to not being wholly covered by the parent shell.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Does not remove holes which are invalid due to touching other rings at more than one point.</description></item>
    /// <item><description>Does not remove holes which are nested inside another hole.</description></item>
    /// </list>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class InvalidHoleRemover
    {
        /// <summary>
        /// Removes invalid holes from the polygons in a geometry.
        /// </summary>
        /// <param name="geom">The geometry to clean</param>
        /// <returns>The geometry with invalid holes removed</returns>
        public static Geometry Clean(Geometry geom)
        {
            var pihr = new InvalidHoleRemover(geom);
            return pihr.GetResult();
        }

        private readonly Geometry _geom;

        /// <summary>
        /// Creates a new invalid hole remover instance.
        /// </summary>
        /// <param name="geom">The geometry to process</param>
        public InvalidHoleRemover(Geometry geom)
        {
            _geom = geom;
        }

        /// <summary>
        /// Gets the cleaned geometry.
        /// </summary>
        /// <returns>The geometry with invalid holes removed.</returns>
        public Geometry GetResult()
        {
            return GeometryMapper.Map(_geom, new InvalidHoleRemoverMapOp());
        }

        private class InvalidHoleRemoverMapOp : GeometryMapper.IMapOp
        {
            public Geometry Map(Geometry geom)
            {
                if (geom is Polygon)
                {
                    var poly = (Polygon)geom;
                    return PolygonInvalidHoleRemover.Clean(poly);
                }
                return geom;
            }
        }

        private class PolygonInvalidHoleRemover
        {
            public static Polygon Clean(Polygon poly)
            {
                var pihr = new PolygonInvalidHoleRemover(poly);
                return pihr.GetResult();
            }

            private readonly Polygon _poly;

            private PolygonInvalidHoleRemover(Polygon poly)
            {
                _poly = poly;
            }

            private Polygon GetResult()
            {
                var gf = _poly.Factory;
                var shell = (LinearRing)_poly.ExteriorRing;
                var shellPrep = PreparedGeometryFactory.Prepare(gf.CreatePolygon(shell));

                IList<Geometry> holes = new List<Geometry>();
                for (int i = 0; i < _poly.NumInteriorRings; i++)
                {
                    Geometry hole = _poly.GetInteriorRingN(i);
                    if (shellPrep.Covers(hole))
                        holes.Add(hole);
                }
                // all holes valid, so return original
                if (holes.Count == _poly.NumInteriorRings)
                    return _poly;

                // return new polygon with covered holes only
                var arr = GeometryFactory.ToLinearRingArray(holes);
                var result = gf.CreatePolygon(shell, arr);
                return result;
            }
        }
    }
}
