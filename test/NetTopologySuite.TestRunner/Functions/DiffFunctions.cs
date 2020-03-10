using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public class DiffFunctions
    {

        public static GeometryCollection DiffVerticeBoths(Geometry a, Geometry b)
        {
            var diffAB = DiffVertices(a, b);
            var diffBA = DiffVertices(b, a);

            return a.Factory.CreateGeometryCollection(
                new Geometry[] {diffAB, diffBA});
        }

        private static MultiPoint DiffVertices(Geometry a, Geometry b)
        {

            var ptsA = a.Coordinates;
            var pts = new HashSet<Coordinate>();
            for (int i = 0; i < ptsA.Length; i++)
            {
                pts.Add(ptsA[i]);
            }

            var diffPts = new CoordinateList();
            var ptsB = b.Coordinates;
            for (int j = 0; j < ptsB.Length; j++)
            {
                var p = ptsB[j];
                if (!pts.Contains(p))
                {
                    diffPts.Add(p);
                }
            }

            return a.Factory.CreateMultiPointFromCoords(diffPts.ToCoordinateArray());
        }

        public static GeometryCollection DiffSegmentsBoth(Geometry a, Geometry b)
        {
            var segsA = ExtractSegments(a);
            var segsB = ExtractSegments(b);

            var diffAB = DiffSegments(segsA, segsB, a.Factory);
            var diffBA = DiffSegments(segsB, segsA, a.Factory);


            return a.Factory.CreateGeometryCollection(
                new Geometry[] {diffAB, diffBA});
        }

        private static MultiLineString DiffSegments(IEnumerable<LineSegment> segsA, IEnumerable<LineSegment> segsB,
            GeometryFactory factory)
        {
            var segs = new HashSet<LineSegment>(segsA);
            var segsDiff = new List<LineSegment>();
            foreach (var seg in segsB)
            {
                if (!segs.Contains(seg))
                {
                    segsDiff.Add(seg);
                }
            }

            var diffLines = ToLineStrings(segsDiff, factory);
            return factory.CreateMultiLineString(diffLines);
        }

        private static LineString[] ToLineStrings(ICollection<LineSegment> segs, GeometryFactory factory)
        {
            var lines = new LineString[segs.Count];
            int i = 0;
            foreach (var seg in segs)
            {
                lines[i++] = seg.ToGeometry(factory);
            }

            return lines;
        }

        private static List<LineSegment> ExtractSegments(Geometry geom)
        {
            var segs = new List<LineSegment>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (var line in lines)
            {
                var pts = line.Coordinates;
                for (int i = 0; i < pts.Length - 1; i++)
                {
                    var seg = new LineSegment(pts[i], pts[i + 1]);
                    seg.Normalize();
                    segs.Add(seg);
                }

            }

            return segs;
        }
    }
}
