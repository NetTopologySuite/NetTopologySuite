using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.VersionComparisonTests
{
    /// <summary>
    /// Summary description for StrIndexTest
    /// </summary>
    [TestClass]
    public class StrIndexTest
    {
        public StrIndexTest()
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
        private readonly IGeometryFactory<BufferedCoordinate> _geometryFactory = new GeometryFactory<BufferedCoordinate>(
            new BufferedCoordinateSequenceFactory(
                new BufferedCoordinateFactory(PrecisionModelType.DoubleFloating)));


        public IEnumerable<IGeometry<BufferedCoordinate>> CreateTestGeometries(int count, double minx, double miny, double maxx, double maxy)
        {
            double xrange = Math.Abs(maxx - minx);
            double yrange = Math.Abs(maxy - miny);

            for (int i = 0; i < count; i++)
            {
                double x1 = _rnd.NextDouble() * xrange + minx;
                double x2 = _rnd.NextDouble() * xrange + minx;
                double y1 = _rnd.NextDouble() * yrange + miny;
                double y2 = _rnd.NextDouble() * yrange + miny;

                yield return (IGeometry<BufferedCoordinate>)_geometryFactory.CreateExtents2D(Math.Min(x1, x2), Math.Min(y1, y2), Math.Max(x1, x2),
                                                        Math.Max(y1, y2)).ToGeometry();
            }
        }

        [TestMethod]
        public void TestStrIndex()
        {
            StrTree<BufferedCoordinate, IGeometry<BufferedCoordinate>>
                index = new StrTree<BufferedCoordinate, IGeometry<BufferedCoordinate>>(_geometryFactory);

            index.BulkLoad(
                CreateTestGeometries(1000, 0.0, 0.0, 3000.0, 3000.0));
            index.Build();

            IExtents<BufferedCoordinate> queryExtents =
                (IExtents<BufferedCoordinate>)_geometryFactory.CreateExtents2D(100.0, 100.0, 120.0, 120.0);

            IList<IGeometry<BufferedCoordinate>> matches = new List<IGeometry<BufferedCoordinate>>(
                index.Query(queryExtents));


            foreach (IGeometry<BufferedCoordinate> list in matches)
            {
                Assert.IsTrue(list.Bounds.Intersects(queryExtents), "a result from the index does not intersect the query bounds");
            }

        }
    }
}
