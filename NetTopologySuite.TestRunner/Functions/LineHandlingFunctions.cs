using System.Collections.Generic;
using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;

namespace Open.Topology.TestRunner.Functions
{
    public static class LineHandlingFunctions
    {
        public static Geometry MergeLines(Geometry g)
        {
            var merger = new LineMerger();
            merger.Add(g);
            var lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static Geometry SequenceLines(Geometry g)
        {
            var ls = new LineSequencer();
            ls.Add(g);
            return ls.GetSequencedLineStrings();
        }

        public static Geometry ExtractLines(Geometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(lines);
        }

        public static Geometry ExtractSegments(Geometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var segments = new List<Geometry>();
            foreach (LineString line in lines)
            {
                for (int i = 1; i < line.NumPoints; i++)
                {
                    var seg = g.Factory.CreateLineString(
                        new[] { line.GetCoordinateN(i - 1), line.GetCoordinateN(i) }
                        );
                    segments.Add(seg);
                }
            }
            return g.Factory.BuildGeometry(segments);
        }
        public static Geometry ExtractChains(Geometry g, int maxChainSize)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var chains = new List<Geometry>();
            foreach (LineString line in lines)
            {
                for (int i = 0; i < line.NumPoints - 1; i += maxChainSize)
                {
                    var chain = ExtractChain(line, i, maxChainSize);
                    chains.Add(chain);
                }
            }
            return g.Factory.BuildGeometry(chains);
        }

        private static LineString ExtractChain(LineString line, int index, int maxChainSize)
        {
            int size = maxChainSize + 1;
            if (index + size > line.NumPoints)
                size = line.NumPoints - index;
            var pts = new Coordinate[size];
            for (int i = 0; i < size; i++)
            {
                pts[i] = line.GetCoordinateN(index + i);
            }
            return line.Factory.CreateLineString(pts);
        }

        public static Geometry Dissolve(Geometry geom)
        {
            return LineDissolver.Dissolve(geom);
        }
    }
}