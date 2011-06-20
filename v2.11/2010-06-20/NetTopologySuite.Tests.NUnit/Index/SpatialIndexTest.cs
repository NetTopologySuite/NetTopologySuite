using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    [TestFixture]
    public abstract class SpatialIndexTest
    {
        public abstract ISpatialIndex<IExtents<Coordinate>, IGeometry<Coordinate>> CreateSpatialIndex();

        [Test]
        public void TestSpatialIndex()
        {
            Console.WriteLine("===============================");
            Console.WriteLine("Spatial-Index Test: " + GetType().Name);
            Console.WriteLine("Grid Extent: " + (CellExtent * CellsPerGridSide));
            Console.WriteLine("Cell Extent: " + CellExtent);
            Console.WriteLine("Feature Extent: " + FeatureExtent);
            Console.WriteLine("Cells Per Grid Side: " + CellsPerGridSide);
            Console.WriteLine("Offset For 2nd Set Of Features: " + Offset);
            ISpatialIndex<IExtents<Coordinate>, IGeometry<Coordinate>> index = CreateSpatialIndex();
            IList<IGeometry<Coordinate>> sourceData = new List<IGeometry<Coordinate>>();
            AddSourceData(0, sourceData);
            AddSourceData(Offset, sourceData);
            Console.WriteLine("Feature Count: " + sourceData.Count);
            Insert(sourceData, index);
            DoTest(index, QueryEnvelopeExtent1, sourceData);
            DoTest(index, QueryEnvelopeExtent2, sourceData);
        }

        protected IExtents<Coordinate> CreateExtents(double a, double b, double c, double d)
        {
            Coordinate min = GeometryUtils.CoordFac.Create(a, b);
            Coordinate max = GeometryUtils.CoordFac.Create(c, d);
            return GeometryUtils.GeometryFactory.CreateExtents(min, max);
        }

        private void Insert(IEnumerable<IGeometry<Coordinate>> sourceData, ISpatialIndex<IExtents<Coordinate>, IGeometry<Coordinate>> index)
        {
            //index.InsertRange(sourceData);
            StrTree<Coordinate, IGeometry<Coordinate>> str = index as StrTree<Coordinate, IGeometry<Coordinate>>;
            if (str != null)
            {
                str.BulkLoad(sourceData);
                return;
            }
            /*
            SirTree<IGeometry<Coordinate>> sir = index as SirTree<IGeometry<Coordinate>>;
            if (sir != null)
            {
                sir.BulkLoad(sourceData);
                return;
            }
             */
            index.InsertRange(sourceData);
        }

        const double CellExtent = 20.31;
        const int CellsPerGridSide = 10;
        const double FeatureExtent = 10.1;
        const double Offset = 5.03;
        const double QueryEnvelopeExtent1 = 1.009;
        const double QueryEnvelopeExtent2 = 11.7;

        private static void AddSourceData(double offset, IList<IGeometry<Coordinate>> sourceData)
        {
            for (int i = 0; i < CellsPerGridSide; i++)
            {
                double minx = (i * CellExtent) + offset;
                double maxx = minx + FeatureExtent;
                for (int j = 0; j < CellsPerGridSide; j++)
                {
                    double miny = (j * CellExtent) + offset;
                    double maxy = miny + FeatureExtent;
                    IGeometry<Coordinate> e = GeometryUtils.GeometryFactory.CreateExtents(
                        GeometryUtils.CoordFac.Create(minx, miny),
                        GeometryUtils.CoordFac.Create(maxx, maxy)
                        ).ToGeometry();
                    sourceData.Insert(i * CellsPerGridSide + j, e);
                }
            }
        }

        private void DoTest(ISpatialIndex<IExtents<Coordinate>, IGeometry<Coordinate>> index, Double queryEnvelopeExtent, IList<IGeometry<Coordinate>> sourceData)
        {
            Console.WriteLine("---------------");
            Console.WriteLine("Envelope Extent: " + queryEnvelopeExtent);
            int extraMatchCount = 0;
            int expectedMatchCount = 0;
            int actualMatchCount = 0;
            int queryCount = 0;
            for (int x = 0; x < CellExtent * CellsPerGridSide; x += (int)queryEnvelopeExtent)
            {
                for (int y = 0; y < CellExtent * CellsPerGridSide; y += (int)queryEnvelopeExtent)
                {
                    IExtents<Coordinate> queryEnvelope = GeometryUtils.GeometryFactory.CreateExtents(
                        GeometryUtils.CoordFac.Create(x, y),
                        GeometryUtils.CoordFac.Create(x + queryEnvelopeExtent, y, y + queryEnvelopeExtent));
                    List<IGeometry<Coordinate>> expectedMatches = IntersectingEnvelopes(queryEnvelope, sourceData);
                    List<IGeometry<Coordinate>> actualMatches = new List<IGeometry<Coordinate>>(index.Query(queryEnvelope));
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
            Console.WriteLine("Average Expected Matches: " + (expectedMatchCount / (double)queryCount));
            Console.WriteLine("Average Actual Matches: " + (actualMatchCount / (double)queryCount));
            Console.WriteLine("Average Extra Matches: " + (extraMatchCount / (double)queryCount));
        }

        private void Compare(List<IGeometry<Coordinate>> expectedgeGeometries, IList<IGeometry<Coordinate>> actualGeometries)
        {
            //Don't use #containsAll because we want to check using
            //==, not #equals. [Jon Aquino]
            //for (Iterator i = expectedEnvelopes.iterator(); i.hasNext(); ) {
            foreach (IGeometry<Coordinate> i in expectedgeGeometries)
            {
                IExtents<Coordinate> expected = i.Extents;
                Boolean found = false;

                //for (Iterator j = actualEnvelopes.iterator(); j.hasNext(); ) {
                foreach (IGeometry<Coordinate> j in actualGeometries)
                {
                    IExtents<Coordinate> actual = j.Extents;
                    if (actual == expected)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found);
            }
        }

        private static List<IGeometry<Coordinate>> IntersectingEnvelopes(IExtents<Coordinate> queryEnvelope, IEnumerable<IGeometry<Coordinate>> envelopes)
        {
            List<IGeometry<Coordinate>> intersectingEnvelopes = new List<IGeometry<Coordinate>>();
            //for (Iterator i = envelopes.iterator(); i.hasNext(); )
            foreach (IGeometry<Coordinate> i in envelopes)
            {
                if (i.Extents.Intersects(queryEnvelope))
                    intersectingEnvelopes.Add(i);
            }
            {
                return intersectingEnvelopes;
            }
        }
    }
}