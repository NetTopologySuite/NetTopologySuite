using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Coverage
{
    internal sealed class CoveragePolygon
    {
        private readonly Polygon _polygon;
        private readonly Envelope _polyEnv;
        IndexedPointInAreaLocator _locator;

        public CoveragePolygon(Polygon polygon)
        {
            _polygon = polygon;
            _polyEnv = polygon.EnvelopeInternal;
        }

        public bool IntersectsEnv(Envelope env)
        {
            //-- test intersection explicitly to avoid expensive null check
            //return polyEnv.intersects(env);
            return !(env.MinX > _polyEnv.MaxX
                  || env.MaxX < _polyEnv.MinX
                  || env.MinY > _polyEnv.MaxY
                  || env.MaxY < _polyEnv.MinY);
        }

        private bool IntersectsEnv(Coordinate p)
        {
            return !(p.X > _polyEnv.MaxX
                  || p.X < _polyEnv.MinX
                  || p.Y > _polyEnv.MaxY
                  || p.Y < _polyEnv.MinY);
        }

        public bool Contains(Coordinate p)
        {
            //-- test intersection explicitly to avoid expensive null check
            if (!IntersectsEnv(p))
                return false;
            return Location.Interior == Locator.Locate(p);
        }

        private IPointOnGeometryLocator Locator
        {
            get
            {
                if (_locator == null)
                {
                    _locator = new IndexedPointInAreaLocator(_polygon);
                }
                return _locator;
            }
        }
    }
}
