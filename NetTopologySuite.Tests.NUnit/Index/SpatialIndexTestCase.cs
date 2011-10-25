using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    [TestFixture]
    public abstract class SpatialIndexTestCase
    {
        protected abstract ISpatialIndex<object> CreateSpatialIndex();

        [Test]
        public void TestSpatialIndex()
        {
            Console.WriteLine("===============================");
            Console.WriteLine("Spatial-Index Test: " + this.GetType().FullName);
            Console.WriteLine("Grid Extent: " + (CELL_EXTENT * CELLS_PER_GRID_SIDE));
            Console.WriteLine("Cell Extent: " + CELL_EXTENT);
            Console.WriteLine("Feature Extent: " + FEATURE_EXTENT);
            Console.WriteLine("Cells Per Grid Side: " + CELLS_PER_GRID_SIDE);
            Console.WriteLine("Offset For 2nd Set Of Features: " + OFFSET);
            var index = CreateSpatialIndex();
            var sourceData = new List<Envelope>();
            AddSourceData(0, sourceData);
            AddSourceData(OFFSET, sourceData);
            Console.WriteLine("Feature Count: " + sourceData.Count);
            Insert(sourceData, index);
            DoTest(index, QUERY_ENVELOPE_EXTENT_1, sourceData);
            DoTest(index, QUERY_ENVELOPE_EXTENT_2, sourceData);
        }

        private void Insert(IList<Envelope> sourceData, ISpatialIndex<object> index)
        {
            foreach (var envelope in sourceData)
            {
                index.Insert(envelope, envelope);
            }
        }

        private static double CELL_EXTENT = 20.31;
        private static int CELLS_PER_GRID_SIDE = 10;
        private static double FEATURE_EXTENT = 10.1;
        private static double OFFSET = 5.03;
        private static double QUERY_ENVELOPE_EXTENT_1 = 1.009;
        private static double QUERY_ENVELOPE_EXTENT_2 = 11.7;

        private void AddSourceData(double offset, IList<Envelope> sourceData)
        {
            for (int i = 0; i < CELLS_PER_GRID_SIDE; i++)
            {
                double minx = (i * CELL_EXTENT) + offset;
                double maxx = minx + FEATURE_EXTENT;
                for (int j = 0; j < CELLS_PER_GRID_SIDE; j++)
                {
                    double miny = (j * CELL_EXTENT) + offset;
                    double maxy = miny + FEATURE_EXTENT;
                    Envelope e = new Envelope(minx, maxx, miny, maxy);
                    sourceData.Add(e);
                }
            }
        }

        private void DoTest(ISpatialIndex<object> index, double queryEnvelopeExtent, IList<Envelope> sourceData)
        {
            Console.WriteLine("---------------");
            Console.WriteLine("Envelope Extent: " + queryEnvelopeExtent);
            int extraMatchCount = 0;
            int expectedMatchCount = 0;
            int actualMatchCount = 0;
            int queryCount = 0;
            for (double x = 0; x < CELL_EXTENT * CELLS_PER_GRID_SIDE; x += queryEnvelopeExtent)
            {
                for (double y = 0; y < CELL_EXTENT * CELLS_PER_GRID_SIDE; y += queryEnvelopeExtent)
                {
                    Envelope queryEnvelope = new Envelope(x, x + queryEnvelopeExtent, y, y + queryEnvelopeExtent);
                    var expectedMatches = IntersectingEnvelopes(queryEnvelope, sourceData);
                    var actualMatches = index.Query(queryEnvelope);
                    Assert.IsTrue(expectedMatches.Count <= actualMatches.Count);
                    extraMatchCount += (actualMatches.Count - expectedMatches.Count);
                    expectedMatchCount += expectedMatches.Count;
                    actualMatchCount += actualMatches.Count;
                    Compare(expectedMatches, actualMatches);
                    queryCount++;
                }
            }
            Console.WriteLine("Expected Matches: " + expectedMatchCount);
            Console.WriteLine("Actual Matches: " + actualMatchCount);
            Console.WriteLine("Extra Matches: " + extraMatchCount);
            Console.WriteLine("Query Count: " + queryCount);
            Console.WriteLine("Average Expected Matches: " + (expectedMatchCount/(double)queryCount));
            Console.WriteLine("Average Actual Matches: " + (actualMatchCount/(double)queryCount));
            Console.WriteLine("Average Extra Matches: " + (extraMatchCount/(double)queryCount));
        }

        private void Compare(IList<object> expectedEnvelopes, IList<object> actualEnvelopes)
        {
            //Don't use #containsAll because we want to check using
            //==, not #equals. [Jon Aquino]
            foreach (var expected in expectedEnvelopes)
            {
                bool found = false;
                foreach (var actual in actualEnvelopes)
                {
                    if (actual == expected)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found);
            }
        }

        private IList<object> IntersectingEnvelopes(Envelope queryEnvelope, IList<Envelope> envelopes)
        {
            var intersectingEnvelopes = new List<object>();
            foreach (var candidate in envelopes)
            {
                if (candidate.Intersects(queryEnvelope)) { intersectingEnvelopes.Add(candidate); }
            }
            return intersectingEnvelopes;
        }
    }
}
