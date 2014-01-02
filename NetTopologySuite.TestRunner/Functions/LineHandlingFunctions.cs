using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;

namespace Open.Topology.TestRunner.Functions
{
    public static class LineHandlingFunctions
    {
        public static IGeometry MergeLines(IGeometry g)
        {
            LineMerger merger = new LineMerger();
            merger.Add(g);
            IList<IGeometry> lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry SequenceLines(IGeometry g)
        {
            LineSequencer ls = new LineSequencer();
            ls.Add(g);
            return ls.GetSequencedLineStrings();
        }

        public static IGeometry ExtractLines(IGeometry g)
        {
            ICollection<IGeometry> lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry ExtractSegments(IGeometry g)
        {
            ICollection<IGeometry> lines = LinearComponentExtracter.GetLines(g);
            List<IGeometry> segments = new List<IGeometry>();
            foreach (ILineString line in lines)
            {
                for (int i = 1; i < line.NumPoints; i++)
                {
                    ILineString seg = g.Factory.CreateLineString(
                        new[] { line.GetCoordinateN(i - 1), line.GetCoordinateN(i) }
                        );
                    segments.Add(seg);
                }
            }
            return g.Factory.BuildGeometry(segments);
        }
        public static IGeometry ExtractChains(IGeometry g, int maxChainSize)
        {
            ICollection<IGeometry> lines = LinearComponentExtracter.GetLines(g);
            List<IGeometry> chains = new List<IGeometry>();
            foreach (ILineString line in lines)
            {
                for (int i = 0; i < line.NumPoints - 1; i += maxChainSize)
                {
                    ILineString chain = ExtractChain(line, i, maxChainSize);
                    chains.Add(chain);
                }
            }
            return g.Factory.BuildGeometry(chains);
        }

        private static ILineString ExtractChain(ILineString line, int index, int maxChainSize)
        {
            int size = maxChainSize + 1;
            if (index + size > line.NumPoints)
                size = line.NumPoints - index;
            Coordinate[] pts = new Coordinate[size];
            for (int i = 0; i < size; i++)
            {
                pts[i] = line.GetCoordinateN(index + i);
            }
            return line.Factory.CreateLineString(pts);
        }

        public static IGeometry Dissolve(IGeometry geom)
        {
            return LineDissolver.Dissolve(geom);
        }
    }
}