using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class WKTTest : BaseSamples
    {
        private static readonly byte[] test00_Geom0_WkbByteArray = new byte[] { 1, 3, 0, 0, 0, 2, 0, 0, 0, 13, 0, 0, 0, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 42, 111, 71, 56, 45, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 42, 111, 71, 56, 45, 242, 86, 192, 211, 254, 135, 112, 223, 195, 68, 64, 129, 200, 50, 52, 45, 242, 86, 192, 127, 54, 124, 110, 223, 195, 68, 64, 2, 15, 125, 36, 45, 242, 86, 192, 37, 181, 4, 97, 223, 195, 68, 64, 129, 35, 212, 22, 45, 242, 86, 192, 249, 75, 52, 79, 223, 195, 68, 64, 122, 104, 190, 11, 45, 242, 86, 192, 11, 60, 186, 57, 223, 195, 68, 64, 85, 235, 168, 3, 45, 242, 86, 192, 250, 206, 105, 33, 223, 195, 68, 64, 110, 165, 204, 0, 45, 242, 86, 192, 142, 4, 179, 17, 223, 195, 68, 64, 210, 113, 152, 244, 44, 242, 86, 192, 207, 65, 172, 183, 223, 195, 68, 64, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 0, 0, 0, 8, 248, 122, 33, 45, 242, 86, 192, 75, 147, 142, 89, 222, 195, 68, 64, 152, 198, 14, 30, 45, 242, 86, 192, 76, 48, 182, 82, 222, 195, 68, 64, 154, 224, 62, 13, 45, 242, 86, 192, 186, 99, 22, 49, 222, 195, 68, 64, 59, 117, 2, 10, 45, 242, 86, 192, 45, 25, 43, 146, 222, 195, 68, 64, 61, 36, 145, 9, 45, 242, 86, 192, 160, 207, 117, 154, 222, 195, 68, 64, 105, 176, 12, 8, 45, 242, 86, 192, 199, 198, 24, 175, 222, 195, 68, 64, 8, 248, 122, 33, 45, 242, 86, 192, 75, 147, 142, 89, 222, 195, 68, 64, 0, 0, 0, 0 };
        private static readonly byte[] test00_Geom1_WkbByteArray = new byte[] { 1, 3, 0, 0, 0, 1, 0, 0, 0, 10, 0, 0, 0, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 7, 153, 100, 228, 44, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 82, 111, 236, 17, 45, 242, 86, 192, 41, 60, 104, 118, 221, 195, 68, 64, 141, 15, 113, 18, 45, 242, 86, 192, 154, 252, 23, 138, 221, 195, 68, 64, 67, 238, 101, 18, 45, 242, 86, 192, 141, 150, 130, 150, 221, 195, 68, 64, 59, 117, 2, 10, 45, 242, 86, 192, 45, 25, 43, 146, 222, 195, 68, 64, 61, 36, 145, 9, 45, 242, 86, 192, 160, 207, 117, 154, 222, 195, 68, 64, 210, 113, 152, 244, 44, 242, 86, 192, 207, 65, 172, 183, 223, 195, 68, 64, 56, 203, 50, 243, 44, 242, 86, 192, 34, 23, 156, 193, 223, 195, 68, 64, 0, 0, 0, 0 };

        private readonly WKTWriter writer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WKTTest"/> class.
        /// </summary>
        public WKTTest() : base()
        {            
            writer = new WKTWriter();
        }

         /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WriteZeroBasedCoordinateWithDifferentFactories()
        {            
            TestFormatting(new Coordinate(0.00000000001, 0.00000000002));
            TestFormatting(new Coordinate(0.00001, 0.00002));
            TestFormatting(new Coordinate(0.01, 0.02));
            TestFormatting(new Coordinate(0.1, 0.2));
            TestFormatting(new Coordinate(0, 0));
        }

        private void TestFormatting(ICoordinate c)
        {            
            IGeometry point = GeometryFactory.Floating.CreatePoint(c);
            String result = writer.Write(point);
            Debug.WriteLine(result);
            IGeometry geom = new WKTReader(GeometryFactory.Floating).Read(result);
            string tos = geom.ToString();
            Assert.IsTrue(String.Equals(tos, result));

            point = GeometryFactory.FloatingSingle.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result);
            geom = new WKTReader(GeometryFactory.Floating).Read(result);
            tos = geom.ToString();
            Assert.IsTrue(String.Equals(tos, result));

            point = GeometryFactory.Fixed.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result);
            geom = new WKTReader(GeometryFactory.Floating).Read(result);
            tos = geom.ToString();
            Assert.IsTrue(String.Equals(tos, result));
        }

		/// <summary>
		/// Issue 12
		/// http://code.google.com/p/nettopologysuite/issues/detail?id=12
		/// </summary>
		[Test]
		public void MultiPoint_WKT_reader_should_skip_extra_parenthesis_around_coordinates()
		{
			WKTReader reader = new WKTReader();
			IGeometry mp1 = reader.Read("MULTIPOINT (10 10, 20 20)");
			IGeometry mp2 = reader.Read("MULTIPOINT ((10 10), (20 20))");
			Assert.AreEqual(mp1, mp2);
		}

        [Test]
        public void TestMaximimPrecisionDigitsFormatting()
        {
            IGeometryFactory factory = GeometryFactory.Default;

            WKBReader wkbreader = new WKBReader(factory);
            IGeometry wkb1 = wkbreader.Read(test00_Geom0_WkbByteArray);            
            Assert.IsNotNull(wkb1);
            Assert.IsTrue(wkb1.IsValid);

            IGeometry wkb2 = wkbreader.Read(test00_Geom1_WkbByteArray);
            Assert.IsNotNull(wkb2);
            Assert.IsTrue(wkb2.IsValid);

            Exception ex = TryOverlay(wkb1, wkb2);
            Assert.IsNotNull(ex);
            Assert.IsTrue(ex.GetType() == typeof(TopologyException));

            string tos1 = writer.Write(wkb1);
            Assert.IsNotNull(tos1);            
            string tos2 = writer.Write(wkb2);
            Assert.IsNotNull(tos2);            

            WKTReader reader = new WKTReader(factory);
            IGeometry wkt1 = reader.Read(tos1);
            Assert.IsNotNull(wkt1);
            Assert.IsTrue(wkt1.IsValid);

            IGeometry wkt2 = reader.Read(tos2);
            Assert.IsNotNull(wkt2);
            Assert.IsTrue(wkt2.IsValid);

            Assert.IsTrue(wkb1.EqualsExact(wkt1), "First geometry pair must be equal!");
            Assert.IsTrue(wkb2.EqualsExact(wkt2), "Second geometry pair must be equal!");

            ex = TryOverlay(wkt1, wkt2);
            Assert.IsNotNull(ex, "Operation must fail!");
            Assert.IsTrue(ex.GetType() == typeof(TopologyException));          
        }

        private Exception TryOverlay(IGeometry g1, IGeometry g2)
        {
            Exception ex = null;
            try
            {
                OverlayOp.Overlay(g1, g2, SpatialFunction.Intersection);
            }
            catch (Exception e)
            {
                ex = e;
            }
            return ex;
        }
    }
}
