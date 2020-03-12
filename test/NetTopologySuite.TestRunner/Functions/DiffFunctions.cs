using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public class DiffFunctions
    {

        public static GeometryCollection DiffVerticesBoth(Geometry a, Geometry b)
        {
            var diffAB = DiffVertices(a, b);
            var diffBA = DiffVertices(b, a);

            return a.Factory.CreateGeometryCollection(
                new Geometry[] {diffAB, diffBA});
        }

        /// <summary>
        /// Diff the vertices in A against B to
        /// find vertices in A which are not in B.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>The vertices in A which are not in B</returns>
        private static MultiPoint DiffVertices(Geometry a, Geometry b)
        {

            var ptsB = b.Coordinates;
            var pts = new HashSet<Coordinate>();
            for (int i = 0; i < ptsB.Length; i++)
            {
                pts.Add(ptsB[i]);
            }

            var diffPts = new CoordinateList();
            var ptsA = b.Coordinates;
            for (int j = 0; j < ptsB.Length; j++)
            {
                var pA = ptsA[j];
                if (!pts.Contains(pA))
                {
                    diffPts.Add(pA);
                }
            }

            return a.Factory.CreateMultiPointFromCoords(diffPts.ToCoordinateArray());
        }

        public static GeometryCollection DiffSegments(Geometry a, Geometry b)
        {
            var segsA = ExtractSegments(a);
            var segsB = ExtractSegments(b);

            var diffAB = DiffSegments(segsA, segsB, a.Factory);

            return diffAB;
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
            var segs = new HashSet<LineSegment>(segsB);
            var segsDiffA = new List<LineSegment>();
            foreach (var seg in segsA)
            {
                if (!segs.Contains(seg))
                {
                    segsDiffA.Add(seg);
                }
            }

            var diffLines = ToLineStrings(segsDiffA, factory);
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
