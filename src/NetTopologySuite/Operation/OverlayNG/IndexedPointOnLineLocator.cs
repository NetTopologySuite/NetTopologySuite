using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    internal class IndexedPointOnLineLocator : IPointOnGeometryLocator
    {

        private readonly Geometry _inputGeom;

        public IndexedPointOnLineLocator(Geometry geomLinear)
        {
            _inputGeom = geomLinear;
        }

        public Location Locate(Coordinate p)
        {
            // TODO: optimize this with a segment index
            var locator = new PointLocator();
            return locator.Locate(p, _inputGeom);
        }

    }
}
