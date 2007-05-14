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
            ICoordinate c = new Coordinate(0.0001, 0.0002);
            Geometry point = GeometryFactory.Floating.CreatePoint(c);
            String result = writer.Write(point); // TODO: writer needs to accept a IGeometry parameter...
            Debug.WriteLine(result);

            point = GeometryFactory.FloatingSingle.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result); 
            
            point = GeometryFactory.Fixed.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result); 
        }
    }
}
