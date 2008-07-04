using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
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
        private IGeometry blueLake = null;
        private IGeometry ashton = null;
 
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
        // [Test]
        public void OGCUnionTest()
        {                        
            Assert.IsNotNull(blueLake);            
            Assert.IsNotNull(ashton);

            IGeometry expected = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18))");
            IGeometry result = blueLake.Union(ashton);

            Debug.WriteLine(result);
            Assert.IsTrue(result.EqualsExact(expected));
        }

        /// <summary>
        /// 
        /// </summary>
        // [Test]
        public void OGCSymDifferenceTest()
        {
            Assert.IsNotNull(blueLake);
            Assert.IsNotNull(ashton);

            IGeometry expected = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18))");
            IGeometry result = blueLake.SymmetricDifference(ashton);

            Debug.WriteLine(result);
            Assert.IsTrue(result.EqualsExact(expected));
        }       
    }
}
