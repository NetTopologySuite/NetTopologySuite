using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

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
    public class OGCTests : BaseSamples
    {
        private Geometry blueLake = null;
        private Geometry ashton = null;
 
        /// <summary>
        /// 
        /// </summary>
        public OGCTests() : base(GeometryFactory.Fixed) 
        {
            blueLake = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18),(59 18,67 18,67 13,59 13,59 18))");
            ashton = Reader.Read("POLYGON(( 62 48, 84 48, 84 30, 56 30, 56 34, 62 48))");
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void OGCUnionTest()
        {                        
            Assert.IsNotNull(blueLake);            
            Assert.IsNotNull(ashton);

            Geometry expected = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18))");
            Geometry result = blueLake.Union(ashton);

            Debug.WriteLine(result);
            Assert.IsTrue(result.EqualsExact(expected));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void OGCSymDifferenceTest()
        {
            Assert.IsNotNull(blueLake);
            Assert.IsNotNull(ashton);

            Geometry expected = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18))");
            Geometry result = blueLake.SymmetricDifference(ashton);

            Debug.WriteLine(result);
            Assert.IsTrue(result.EqualsExact(expected));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ExtractToShapefile()
        {                         
            string path = Path.GetTempPath() + "\\test";
            if(File.Exists(path + ".shp"))
                File.Delete(path);
            Assert.IsFalse(File.Exists(path));

            ShapefileWriter writer = new ShapefileWriter(); 
            writer.Write(path, new GeometryCollection(new Geometry[] { blueLake, ashton, }));
            Assert.IsTrue(File.Exists(path + ".shp"));
        }

    }
}
