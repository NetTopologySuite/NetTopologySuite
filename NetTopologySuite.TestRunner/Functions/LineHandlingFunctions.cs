using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;

namespace Open.Topology.TestRunner.Functions
{
    public static class LineHandlingFunctions
    {
        public static IGeometry MergeLines(IGeometry g)
        {
            var merger = new LineMerger();
            merger.Add(g);
            var lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry SequenceLines(IGeometry g)
        {
            var ls = new LineSequencer();
            ls.Add(g);
            return ls.GetSequencedLineStrings();
        }

        public static IGeometry ExtractLines(IGeometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry ExtractSegments(IGeometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var segments = new List<IGeometry>();
            foreach (ILineString line in lines)
            {
                for (var i = 1; i < line.NumPoints; i++)
                {
                    var seg = g.Factory.CreateLineString(
                        new[] { line.GetCoordinateN(i - 1), line.GetCoordinateN(i) }
                        );
                    segments.Add(seg);
                }
            }
            return g.Factory.BuildGeometry(segments);
        }
        public static IGeometry ExtractChains(IGeometry g, int maxChainSize)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var chains = new List<IGeometry>();
            foreach (ILineString line in lines)
            {
                for (var i = 0; i < line.NumPoints - 1; i += maxChainSize)
                {
                    var chain = ExtractChain(line, i, maxChainSize);
                    chains.Add(chain);
                }
            }
            return g.Factory.BuildGeometry(chains);
        }

        private static ILineString ExtractChain(ILineString line, int index, int maxChainSize)
        {
            var size = maxChainSize + 1;
            if (index + size > line.NumPoints)
                size = line.NumPoints - index;
            var pts = new Coordinate[size];
            for (var i = 0; i < size; i++)
            {
                pts[i] = line.GetCoordinateN(index + i);
            }
            return line.Factory.CreateLineString(pts);
        }
    }
}