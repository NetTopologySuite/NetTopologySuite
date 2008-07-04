using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various 
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class GeomBinOpTest_Issue14_Simplified
    {
        private static readonly byte[] test00_Geom0_WkbByteArray = new byte[] { 1, 3, 0, 0, 0, 2, 0, 0, 0, 13, 0, 0, 0, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 42, 111, 71, 56, 45, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 42, 111, 71, 56, 45, 242, 86, 192, 211, 254, 135, 112, 223, 195, 68, 64, 129, 200, 50, 52, 45, 242, 86, 192, 127, 54, 124, 110, 223, 195, 68, 64, 2, 15, 125, 36, 45, 242, 86, 192, 37, 181, 4, 97, 223, 195, 68, 64, 129, 35, 212, 22, 45, 242, 86, 192, 249, 75, 52, 79, 223, 195, 68, 64, 122, 104, 190, 11, 45, 242, 86, 192, 11, 60, 186, 57, 223, 195, 68, 64, 85, 235, 168, 3, 45, 242, 86, 192, 250, 206, 105, 33, 223, 195, 68, 64, 110, 165, 204, 0, 45, 242, 86, 192, 142, 4, 179, 17, 223, 195, 68, 64, 210, 113, 152, 244, 44, 242, 86, 192, 207, 65, 172, 183, 223, 195, 68, 64, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 0, 0, 0, 8, 248, 122, 33, 45, 242, 86, 192, 75, 147, 142, 89, 222, 195, 68, 64, 152, 198, 14, 30, 45, 242, 86, 192, 76, 48, 182, 82, 222, 195, 68, 64, 154, 224, 62, 13, 45, 242, 86, 192, 186, 99, 22, 49, 222, 195, 68, 64, 59, 117, 2, 10, 45, 242, 86, 192, 45, 25, 43, 146, 222, 195, 68, 64, 61, 36, 145, 9, 45, 242, 86, 192, 160, 207, 117, 154, 222, 195, 68, 64, 105, 176, 12, 8, 45, 242, 86, 192, 199, 198, 24, 175, 222, 195, 68, 64, 8, 248, 122, 33, 45, 242, 86, 192, 75, 147, 142, 89, 222, 195, 68, 64, 0, 0, 0, 0 };
        private static readonly byte[] test00_Geom1_WkbByteArray = new byte[] { 1, 3, 0, 0, 0, 1, 0, 0, 0, 10, 0, 0, 0, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 82, 111, 236, 17, 45, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 141, 15, 113, 18, 45, 242, 86, 192, 154, 252, 23, 138, 221, 195, 68, 64, 67, 238, 101, 18, 45, 242, 86, 192, 141, 150, 130, 150, 221, 195, 68, 64, 59, 117, 2, 10, 45, 242, 86, 192, 45, 25, 43, 146, 222, 195, 68, 64, 61, 36, 145, 9, 45, 242, 86, 192, 160, 207, 117, 154, 222, 195, 68, 64, 210, 113, 152, 244, 44, 242, 86, 192, 207, 65, 172, 183, 223, 195, 68, 64, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 0, 0, 0, 0 };
        private static readonly IGeometryFactory factory = GeometryFactory.Default;

        private WKBReader wkbreader = null;
        private IGeometry geometry0 = null;
        private IGeometry geometry1 = null;

        /// <summary>
        /// 
        /// </summary>
        [TestFixtureSetUp]
        public void Setup()
        {
            wkbreader = new WKBReader(factory);

            geometry0 = wkbreader.Read(test00_Geom0_WkbByteArray);
            Debug.WriteLine(geometry0.ToString());
            geometry1 = wkbreader.Read(test00_Geom1_WkbByteArray);
            Debug.WriteLine(geometry1.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GeomBinOpTest_Issue14_00_Simplified_Union()
        {
            Assert.IsTrue(geometry0.IsValid);
            Assert.IsTrue(geometry1.IsValid);
            try
            {
                IGeometry result = geometry0.Union(geometry1);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
            catch (Exception ex)
            {
                Assert.Fail("GeomBinOpTest" + 0 + " failed with exception: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GeomBinOpTest_Issue14_00_Simplified_Difference()
        {
            Assert.IsTrue(geometry0.IsValid);
            Assert.IsTrue(geometry1.IsValid);
            try
            {
                IGeometry result = geometry0.Difference(geometry1);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
            catch (Exception ex)
            {
                Assert.Fail("GeomBinOpTest" + 0 + " failed with exception: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GeomBinOpTest_Issue14_00_Simplified_SymmetricDifference()
        {
            Assert.IsTrue(geometry0.IsValid);
            Assert.IsTrue(geometry1.IsValid);
            try
            {
                IGeometry result = geometry0.SymmetricDifference(geometry1);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
            catch (Exception ex)
            {
                Assert.Fail("GeomBinOpTest" + 0 + " failed with exception: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GeomBinOpTest_Issue14_00_Simplified_Intersection()
        {
            Assert.IsTrue(geometry0.IsValid);
            Assert.IsTrue(geometry1.IsValid);
            try
            {
                IGeometry result = geometry0.Intersection(geometry1);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsValid);
            }
            catch (Exception ex)
            {
                Assert.Fail("GeomBinOpTest" + 0 + " failed with exception: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GeomBinOpTest_Issue14_00_Simplified_WkbWkt()
        {
            string wktStr = geometry0.AsText();
            WKTReader wktReader = new WKTReader();
            IGeometry geometry0_bis = wktReader.Read(wktStr);
            byte[] test00_Geom0_WkbByteArray_bis = geometry0_bis.AsBinary();
            Assert.AreEqual(test00_Geom0_WkbByteArray.Length, test00_Geom0_WkbByteArray_bis.Length, "Different wkb array length.");
            for (int i = 0; i < test00_Geom0_WkbByteArray_bis.Length; i++)
                Assert.AreEqual(test00_Geom0_WkbByteArray[i], test00_Geom0_WkbByteArray_bis[i], "Different wkb array element at index " + i + ".");            
        }
    }
}
