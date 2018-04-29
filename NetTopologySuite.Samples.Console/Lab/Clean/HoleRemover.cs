using System.Collections.Generic;
using GeoAPI.Geometries;
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
            bool Value(IGeometry geom);
        }
        private readonly IGeometry _geom;
        private readonly Predicate _predicate;
        /// <summary>
        /// Creates a new hole remover instance.
        /// </summary>
        public HoleRemover(IGeometry geom, Predicate predicate)
        {
            _geom = geom;
            _predicate = predicate;
        }
        /// <summary>
        /// Gets the cleaned geometry.
        /// </summary>
        public IGeometry GetResult()
        {
            HoleRemoverMapOp op = new HoleRemoverMapOp(_predicate);
            return GeometryMapper.Map(_geom, op);
        }
        private class HoleRemoverMapOp : GeometryMapper.IMapOp
        {
            private readonly Predicate _predicate;
            public HoleRemoverMapOp(Predicate predicate)
            {
                _predicate = predicate;
            }
            public IGeometry Map(IGeometry geom)
            {
                if (geom is IPolygon)
                    return PolygonHoleRemover.Clean((IPolygon)geom, _predicate);
                return geom;
            }
        }
        private class PolygonHoleRemover
        {
            public static IPolygon Clean(IPolygon poly, Predicate isRemoved)
            {
                PolygonHoleRemover pihr = new PolygonHoleRemover(poly, isRemoved);
                return pihr.GetResult();
            }
            private readonly IPolygon _poly;
            private readonly Predicate _predicate;
            public PolygonHoleRemover(IPolygon poly, Predicate predicate)
            {
                _poly = poly;
                _predicate = predicate;
            }
            public IPolygon GetResult()
            {
                IGeometryFactory gf = _poly.Factory;
                IList<IGeometry> holes = new List<IGeometry>();
                for (int i = 0; i < _poly.NumInteriorRings; i++)
                {
                    ILinearRing hole = (ILinearRing)_poly.GetInteriorRingN(i);
                    if (!_predicate.Value(hole))
                        holes.Add(hole);
                }
                // all holes valid, so return original
                if (holes.Count == _poly.NumInteriorRings)
                    return _poly;
                // return new polygon with covered holes only
                ILinearRing shell = (ILinearRing)_poly.ExteriorRing;
                ILinearRing[] rings = GeometryFactory.ToLinearRingArray(holes);
                IPolygon result = gf.CreatePolygon(shell, rings);
                return result;
            }
        }
    }
}
