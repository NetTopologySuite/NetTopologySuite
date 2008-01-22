using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

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
        private WKTWriter writer = null;

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

            point = GeometryFactory.FloatingSingle.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result);

            point = GeometryFactory.Fixed.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result);
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
    }
}
