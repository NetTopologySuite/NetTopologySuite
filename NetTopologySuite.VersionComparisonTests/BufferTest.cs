using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetTopologySuite.VersionComparisonTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class BufferTest
    {
        public BufferTest()
        {
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

        [TestMethod]
        public void BasicBuffer()
        {
            ILineString ls = new WKTReader().Read("LINESTRING(0 0, 100 100)") as ILineString;
            Assert.IsNotNull(ls);
            BufferBuilder bb = new BufferBuilder();
            IGeometry res = bb.Buffer(ls, 10);
            Assert.IsNotNull(res);
        }



        [TestMethod]
        public void BasicBufferPoint()
        {
            IPoint ls = new WKTReader().Read("POINT(0 0)") as IPoint;
            Assert.IsNotNull(ls);
            BufferBuilder bb = new BufferBuilder();
            bb.EndCapStyle = BufferStyle.CapSquare;
            bb.QuadrantSegments = 2;
            IGeometry actual = bb.Buffer(ls, 10);
            IGeometry expected = new WKTReader().Read("POLYGON((10 10, -10 10, -10 -10, 10 -10, 10 10))");
            Assert.AreEqual(actual, expected);
        }
    }
}
