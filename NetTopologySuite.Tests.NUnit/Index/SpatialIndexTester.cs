using System;
using System.Collections.Generic;
using NetTopologySuite.Index;
using GeoAPI.Geometries;

namespace NetTopologySuite.Tests.NUnit.Index
{
    /**
     * @version 1.7
     */

    public class SpatialIndexTester
    {
        private static readonly bool Verbose = true;

        private List<Envelope> _sourceData;

        public SpatialIndexTester()
        {
            IsSuccess = true;
        }

        public bool IsSuccess { get; private set; }

        public ISpatialIndex<object> SpatialIndex { get; set; }

        public void Init()
        {
            if (Verbose)
            {
                Console.WriteLine("===============================");
                Console.WriteLine("Grid Extent: " + (CellExtent*CellsPerGridSide));
                Console.WriteLine("Cell Extent: " + CellExtent);
                Console.WriteLine("Feature Extent: " + FeatureExtent);
                Console.WriteLine("Cells Per Grid Side: " + CellsPerGridSide);
                Console.WriteLine("Offset For 2nd Set Of Features: " + OFFSET);
            }
            _sourceData = new List<Envelope>();
            AddSourceData(0, _sourceData);
            AddSourceData(OFFSET, _sourceData);
            if (Verbose)
                Console.WriteLine("Feature Count: " + _sourceData.Count);
            Insert(_sourceData, SpatialIndex);
        }

        public void Run()
        {
            DoTest(SpatialIndex, QueryEnvelopeExtent1, _sourceData);
            DoTest(SpatialIndex, QueryEnvelopeExtent2, _sourceData);
        }

        private static void Insert(IEnumerable<Envelope> sourceData, ISpatialIndex<object> index)
        {
            foreach (var envelope in sourceData)
            {
                index.Insert(envelope, envelope);
            }
        }

        private const double CellExtent = 20.31;
        private const int CellsPerGridSide = 10;
        private const double FeatureExtent = 10.1;
        private const double OFFSET = 5.03;
        private const double QueryEnvelopeExtent1 = 1.009;
        private const double QueryEnvelopeExtent2 = 11.7;

        private static void AddSourceData(double offset, List<Envelope> sourceData)
        {
            for (int i = 0; i < CellsPerGridSide; i++)
            {
                double minx = (i*CellExtent) + offset;
                double maxx = minx + FeatureExtent;
                for (int j = 0; j < CellsPerGridSide; j++)
                {
                    var miny = (j*CellExtent) + offset;
                    var maxy = miny + FeatureExtent;
                    var e = new Envelope(minx, maxx, miny, maxy);
                    sourceData.Add(e);
                }
            }
        }

        private void DoTest(ISpatialIndex<object> index, double queryEnvelopeExtent, List<Envelope> sourceData)
        {
            if (Verbose)
            {
                Console.WriteLine("---------------");
                Console.WriteLine("Envelope Extent: " + queryEnvelopeExtent);
            }
            int extraMatchCount = 0;
            int expectedMatchCount = 0;
            int actualMatchCount = 0;
            int queryCount = 0;
            for (var x = 0d; x < CellExtent*CellsPerGridSide; x += queryEnvelopeExtent)
            {
                for (var y = 0d; y < CellExtent*CellsPerGridSide; y += queryEnvelopeExtent)
                {
                    var queryEnvelope = new Envelope(x, x + queryEnvelopeExtent, y, y + queryEnvelopeExtent);
                    var expectedMatches = IntersectingEnvelopes(queryEnvelope, sourceData);
                    var actualMatches = index.Query(queryEnvelope);
                    // since index returns candidates only, it may return more than the expected value
                    if (expectedMatches.Count > actualMatches.Count)
                    {
                        IsSuccess = false;
                    }
                    extraMatchCount += (actualMatches.Count - expectedMatches.Count);
                    expectedMatchCount += expectedMatches.Count;
                    actualMatchCount += actualMatches.Count;
                    Compare(expectedMatches, actualMatches);
                    queryCount++;
                }
            }

            if (Verbose)
            {
                Console.WriteLine("Expected Matches: " + expectedMatchCount);
                Console.WriteLine("Actual Matches: " + actualMatchCount);
                Console.WriteLine("Extra Matches: " + extraMatchCount);
                Console.WriteLine("Query Count: " + queryCount);
                Console.WriteLine("Average Expected Matches: " + (expectedMatchCount/(double) queryCount));
                Console.WriteLine("Average Actual Matches: " + (actualMatchCount/(double) queryCount));
                Console.WriteLine("Average Extra Matches: " + (extraMatchCount/(double) queryCount));
            }
        }

        private void Compare(IEnumerable<Envelope> expectedEnvelopes, IList<object> actualEnvelopes)
        {
            //Don't use #containsAll because we want to check using
            //==, not #equals. [Jon Aquino]
            foreach (var expected in expectedEnvelopes)
            {
                var found = false;
                foreach (var actual in actualEnvelopes)
                {
                    /*if (actual == expected)
                    {
                        found = true;
                        break;
                    }*/
                    if (actual.Equals(expected))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    IsSuccess = false;
            }
        }

        private static List<Envelope> IntersectingEnvelopes(Envelope queryEnvelope, IEnumerable<Envelope> envelopes)
        {
            var intersectingEnvelopes = new List<Envelope>();
            foreach (var candidate in envelopes)
            {
                if (candidate.Intersects(queryEnvelope))
                {
                    intersectingEnvelopes.Add(candidate);
                }
            }
            return intersectingEnvelopes;
        }
    }
}