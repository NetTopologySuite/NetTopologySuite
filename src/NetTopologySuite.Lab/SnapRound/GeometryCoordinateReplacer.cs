using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.SnapRound
{
    internal class GeometryCoordinateReplacer : GeometryEditorEx.CoordinateSequenceOperation
    {
        private readonly IDictionary<Geometry, Coordinate[]> _geometryLinesMap;

        public GeometryCoordinateReplacer(IDictionary<Geometry, Coordinate[]> linesMap)
        {
            this._geometryLinesMap = linesMap;
        }

        public override CoordinateSequence Edit(CoordinateSequence coordSeq,
            Geometry geometry, GeometryFactory targetFactory)
        {
            if (_geometryLinesMap.ContainsKey(geometry))
            {
                var pts = _geometryLinesMap[geometry];
                // Assert: pts should always have length > 0
                bool isValidPts = IsValidSize(pts, geometry);
                if (!isValidPts) return null;
                return targetFactory.CoordinateSequenceFactory.Create(pts);
            }
            //TODO: should this return null if no matching snapped line is found
            // probably should never reach here?
            return coordSeq;
        }

        private static bool IsValidSize(Coordinate[] pts, Geometry geom)
        {
            if (pts.Length == 0)
                return true;
            int minSize = MinimumNonEmptyCoordinatesSize(geom);
            if (pts.Length < minSize)
                return false;
            return true;
        }

        private static int MinimumNonEmptyCoordinatesSize(Geometry geom)
        {
            if (geom is LinearRing)
                return 4;
            if (geom is LineString)
                return 2;
            return 0;
        }
    }
}