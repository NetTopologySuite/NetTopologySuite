using System;
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
    public class NormalizeTest : BaseSamples
    {
        private IPolygon polygon = null;
        private ILinearRing shell = null;
        private ILinearRing hole = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NormalizeTest"/> class.
        /// </summary>
        public NormalizeTest() : base() { }

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Init()
        {
            shell = Factory.CreateLinearRing(new ICoordinate[] {    new Coordinate(100,100),
                                                                    new Coordinate(200,100),
                                                                    new Coordinate(200,200),                
                                                                    new Coordinate(100,200),
                                                                    new Coordinate(100,100), });
            // NOTE: Hole is created with not correct order for holes
            hole = Factory.CreateLinearRing(new ICoordinate[] {      new Coordinate(120,120),
                                                                    new Coordinate(180,120),
                                                                    new Coordinate(180,180),                                                                                
                                                                    new Coordinate(120,180),                                                                
                                                                    new Coordinate(120,120), });
            polygon = Factory.CreatePolygon(shell, new ILinearRing[] { hole, });
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NotNormalizedGDBOperation()
        {                        
	        byte[] bytes = new GDBWriter().Write(polygon);
            IGeometry test = new GDBReader().Read(bytes);

            Assert.IsNull(test);    
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]        
        public void NormalizedGDBOperation()
        {
            polygon.Normalize();

            byte[] bytes = new GDBWriter().Write(polygon);
            IGeometry test = new GDBReader().Read(bytes);

            Assert.IsNotNull(test);            
        }
    }
}
