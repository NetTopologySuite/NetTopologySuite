using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
namespace NetTopologySuite.SnapRound
{
    internal class GeometryCoordinateReplacer : GeometryEditorEx.CoordinateSequenceOperation
    {
        private readonly IDictionary<IGeometry, Coordinate[]> _geometryLinesMap;
        public GeometryCoordinateReplacer(IDictionary<IGeometry, Coordinate[]> linesMap)
        {
            this._geometryLinesMap = linesMap;
        }
        public override ICoordinateSequence Edit(ICoordinateSequence coordSeq,
            IGeometry geometry, IGeometryFactory targetFactory)
        {
            if (_geometryLinesMap.ContainsKey(geometry))
            {
                Coordinate[] pts = _geometryLinesMap[geometry];
                // Assert: pts should always have length > 0
                bool isValidPts = IsValidSize(pts, geometry);
                if (!isValidPts) return null;
                return targetFactory.CoordinateSequenceFactory.Create(pts);
            }
            //TODO: should this return null if no matching snapped line is found
            // probably should never reach here?
            return coordSeq;
        }
        private static bool IsValidSize(Coordinate[] pts, IGeometry geom)
        {
            if (pts.Length == 0)
                return true;
            int minSize = MinimumNonEmptyCoordinatesSize(geom);
            if (pts.Length < minSize)
                return false;
            return true;
        }
        private static int MinimumNonEmptyCoordinatesSize(IGeometry geom)
        {
            if (geom is LinearRing)
                return 4;
            if (geom is LineString)
                return 2;
            return 0;
        }
    }
}
