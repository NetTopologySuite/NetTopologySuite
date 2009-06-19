using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetTopologySuite.VersionComparisonTests
{
    /// <summary>
    /// Summary description for StrTreeTests
    /// </summary>
    [TestClass]
    public class StrTreeTests
    {
        public StrTreeTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        readonly Random _rnd = new Random(DateTime.Now.Millisecond);
        private readonly IGeometryFactory _geometryFactory = new GeometryFactory();

        public IEnumerable<IGeometry> CreateTestGeometries(int count, double minx, double miny, double maxx, double maxy)
        {
            double xrange = Math.Abs(maxx - minx);
            double yrange = Math.Abs(maxy - miny);
            for (int i = 0; i < count; i++)
            {
                double x1 = _rnd.NextDouble() * xrange + minx;
                double x2 = _rnd.NextDouble() * xrange + minx;
                double y1 = _rnd.NextDouble() * yrange + miny;
                double y2 = _rnd.NextDouble() * yrange + miny;

                double lrx = Math.Min(x1, x2);
                double lry = Math.Min(y1, y2);
                double trx = Math.Max(x1, x2);
                double @try = Math.Max(y1, y2);

                ICoordinate[] coords = new ICoordinate[]
                                            {
                                                new Coordinate(lrx, lry),
                                                new Coordinate(lrx, @try),
                                                new Coordinate(trx, @try),
                                                new Coordinate(trx, lry),
                                                new Coordinate(lrx,lry)
                                            };

                yield return _geometryFactory.CreatePolygon(
                    _geometryFactory.CreateLinearRing(
                    _geometryFactory.CoordinateSequenceFactory.Create(coords)
                    ), new ILinearRing[] { });
            }


        }

        [TestMethod]
        public void TestStrIndex()
        {
            STRtree ndx = new STRtree();

            foreach (var v in CreateTestGeometries(1000, 0.0, 0.0, 3000.0, 3000.0))
                ndx.Insert(v.EnvelopeInternal, v);

            ndx.Build();
            IEnvelope queryExtents = new Envelope(100.0, 120.0, 100.0, 120.0);
            IList matches = ndx.Query(queryExtents);
            foreach (IGeometry list in matches)
            {
                Assert.IsTrue(list.EnvelopeInternal.Intersects(queryExtents), "a result from the index does not intersect the query bounds");
            }

        }
    }
}
