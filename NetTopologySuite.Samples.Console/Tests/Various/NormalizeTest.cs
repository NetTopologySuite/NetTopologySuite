using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
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
    public class NormalizeTest : BaseSamples
    {
        private IPolygon polygon = null;
        private ILinearRing shell = null;
        private ILinearRing hole = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NormalizeTest"/> class.
        /// </summary>
        public NormalizeTest() : base() { }

        [SetUp]
        public void Init()
        {
            shell = GeoFactory.CreateLinearRing(new ICoordinate[] { CoordFactory.Create(100,100),
                                                                    CoordFactory.Create(200,100),
                                                                    CoordFactory.Create(200,200),                
                                                                    CoordFactory.Create(100,200),
                                                                    CoordFactory.Create(100,100), });
            // NOTE: Hole is created with not correct order for holes
            hole = GeoFactory.CreateLinearRing(new ICoordinate[] {      CoordFactory.Create(120,120),
                                                                    CoordFactory.Create(180,120),
                                                                    CoordFactory.Create(180,180),                                                                                
                                                                    CoordFactory.Create(120,180),                                                                
                                                                    CoordFactory.Create(120,120), });
            polygon = GeoFactory.CreatePolygon(shell, new ILinearRing[] { hole, });
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Ignore("GDBWriter and GDBReader not implemented")]
        public void NotNormalizedGDBOperation()
        {                        
            //byte[] bytes = new GDBWriter().Write(polygon);
            //IGeometry test = new GDBReader().Read(bytes);

            //Assert.IsNull(test);    
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [Ignore("GDBWriter and GDBReader not implemented")]
        public void NormalizedGDBOperation()
        {
            polygon.Normalize();

            //byte[] bytes = new GDBWriter().Write(polygon);
            //IGeometry test = new GDBReader().Read(bytes);

            //Assert.IsNotNull(test);            
        }
    }
}
