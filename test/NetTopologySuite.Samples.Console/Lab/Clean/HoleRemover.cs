#nullable disable
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Samples.Lab.Clean
{
    /// <summary>
    /// Removes holes which match a given predicate.
    /// </summary>
    public class HoleRemover
    {
        public interface Predicate
        {
            bool Value(Geometry geom);
        }

        private readonly Geometry _geom;
        private readonly Predicate _predicate;

        /// <summary>
        /// Creates a new hole remover instance.
        /// </summary>
        public HoleRemover(Geometry geom, Predicate predicate)
        {
            _geom = geom;
            _predicate = predicate;
        }

        /// <summary>
        /// Gets the cleaned geometry.
        /// </summary>
        public Geometry GetResult()
        {
            var op = new HoleRemoverMapOp(_predicate);
            return GeometryMapper.Map(_geom, op);
        }

        private class HoleRemoverMapOp : GeometryMapper.IMapOp
        {
            private readonly Predicate _predicate;

            public HoleRemoverMapOp(Predicate predicate)
            {
                _predicate = predicate;
            }

            public Geometry Map(Geometry geom)
            {
                if (geom is Polygon)
                    return PolygonHoleRemover.Clean((Polygon)geom, _predicate);
                return geom;
            }
        }

        private class PolygonHoleRemover
        {
            public static Polygon Clean(Polygon poly, Predicate isRemoved)
            {
                var pihr = new PolygonHoleRemover(poly, isRemoved);
                return pihr.GetResult();
            }

            private readonly Polygon _poly;
            private readonly Predicate _predicate;

            public PolygonHoleRemover(Polygon poly, Predicate predicate)
            {
                _poly = poly;
                _predicate = predicate;
            }

            public Polygon GetResult()
            {
                var gf = _poly.Factory;

                IList<Geometry> holes = new List<Geometry>();
                for (int i = 0; i < _poly.NumInteriorRings; i++)
                {
                    var hole = (LinearRing)_poly.GetInteriorRingN(i);
                    if (!_predicate.Value(hole))
                        holes.Add(hole);
                }
                // all holes valid, so return original
                if (holes.Count == _poly.NumInteriorRings)
                    return _poly;

                // return new polygon with covered holes only
                var shell = (LinearRing)_poly.ExteriorRing;
                var rings = GeometryFactory.ToLinearRingArray(holes);
                var result = gf.CreatePolygon(shell, rings);
                return result;
            }
        }
    }
}
