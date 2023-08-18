using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Computes the distance between a point and a geometry
    /// (which may be a collection containing any type of geometry).
    /// Also computes the pair of points containing the input
    /// point and the nearest point on the geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class IndexedDistanceToPoint
    {

        private readonly Geometry _targetGeometry;
        private IndexedFacetDistance _facetDistance;
        private IndexedPointInPolygonsLocator _ptLocater;

        public IndexedDistanceToPoint(Geometry geom)
        {
            _targetGeometry = geom;
        }

        private void Init()
        {
            if (_facetDistance != null)
                return;
            _facetDistance = new IndexedFacetDistance(_targetGeometry);
            _ptLocater = new IndexedPointInPolygonsLocator(_targetGeometry);
        }

        /// <summary>
        /// Computes the distance from a point to the geometry.
        /// </summary>
        /// <param name="pt">The input point</param>
        /// <returns>The distance to the geometry</returns>
        public double Distance(Point pt)
        {
            Init();
            //-- distance is 0 if point is inside a target polygon
            if (IsInArea(pt))
            {
                return 0;
            }
            return _facetDistance.Distance(pt);
        }

        private bool IsInArea(Point pt)
        {
            return Location.Exterior != _ptLocater.Locate(pt.Coordinate);
        }

        /// <summary>
        /// Gets the nearest locations between the geometry and a point.
        /// The first location lies on the geometry,
        /// and the second location is the provided point.</summary>
        /// <param name="pt">The point to compute the nearest location for</param>
        /// <returns>A pair of locations</returns>
        public Coordinate[] NearestPoints(Point pt)
        {
            Init();
            if (IsInArea(pt))
            {
                var p = pt.Coordinate;
                return new Coordinate[] { p.Copy(), p.Copy() };
            }
            return _facetDistance.NearestPoints(pt);
        }
    }
}
