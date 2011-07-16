using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.VersionComparisonTests
{
    /// <summary>
    /// Summary description for BufferTest
    /// </summary>
    [TestClass]
    public class BufferTest
    {
        public BufferTest()
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

        private readonly IGeometryFactory<BufferedCoordinate> _geometryFactory =
            new GeometryFactory<BufferedCoordinate>(
                new BufferedCoordinateSequenceFactory(
                    new BufferedCoordinateFactory(PrecisionModelType.DoubleFloating))
                );

        [TestMethod]
        public void Test_T97Buffer()
        {
            ILineString ls = _geometryFactory.WktReader.Read("LINESTRING(0 0, 100 100)") as ILineString;
            Assert.IsNotNull(ls);

            BufferBuilder<BufferedCoordinate> bb =
                new BufferBuilder<BufferedCoordinate>(_geometryFactory);
            IGeometry<BufferedCoordinate> res = bb.Buffer((IGeometry<BufferedCoordinate>)ls, 10);
            Assert.IsNotNull(res);
        }
    }
}
