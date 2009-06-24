using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpMap.Data.Providers.PostGis.Tests
{

    [TestClass]
    public class UnrelatedTests
    {

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
        }


        public static void MyClassCleanup()
        {
        }



        [TestMethod]
        public void Test_T97Buffer()
        {
            ILineString ls = new WKTReader().Read("LINESTRING(0 0, 100 100)") as ILineString;
            Assert.IsNotNull(ls);

            BufferBuilder bb =
                new BufferBuilder();
            IGeometry res = bb.Buffer(ls, 10);
            Assert.IsNotNull(res);
        }
    }
}