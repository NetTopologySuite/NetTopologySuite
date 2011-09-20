using GeoAPI.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    public class LineTopology
    {
        private readonly IGeometryFactory _geomFact;
        private readonly NodedSegmentString _segStr;

        public LineTopology(Coordinate[] pts, IGeometryFactory geomFact)
        {
            _segStr = new NodedSegmentString(pts, this);
            _geomFact = geomFact;
        }

        public void AddIntersection(Coordinate intPt, int segmentIndex)
        {
            _segStr.AddIntersection(intPt, segmentIndex);
        }

        public IGeometry Result
        {
            get
            {
                var resultPts = new Coordinate[0];
                return _geomFact.CreateLineString(resultPts);
            }
        }

    }
}