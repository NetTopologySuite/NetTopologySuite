using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Locates points on a linear geometry,
    /// using a spatial index to provide good performance.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class IndexedPointOnLineLocator : IPointOnGeometryLocator
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
