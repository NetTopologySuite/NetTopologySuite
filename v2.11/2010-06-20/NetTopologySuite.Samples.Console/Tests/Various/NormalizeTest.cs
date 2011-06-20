using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class NormalizeTest : BaseSamples
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            shell = GeoFactory.CreateLinearRing(new ICoordinate[]
                                                    {
                                                        CoordFactory.Create(100, 100),
                                                        CoordFactory.Create(200, 100),
                                                        CoordFactory.Create(200, 200),
                                                        CoordFactory.Create(100, 200),
                                                        CoordFactory.Create(100, 100),
                                                    });
            // NOTE: Hole is created with not correct order for holes
            hole = GeoFactory.CreateLinearRing(new ICoordinate[]
                                                   {
                                                       CoordFactory.Create(120, 120),
                                                       CoordFactory.Create(180, 120),
                                                       CoordFactory.Create(180, 180),
                                                       CoordFactory.Create(120, 180),
                                                       CoordFactory.Create(120, 120),
                                                   });
            polygon = GeoFactory.CreatePolygon(shell, new[] {hole,});
        }

        #endregion

        private IPolygon polygon;
        private ILinearRing shell;
        private ILinearRing hole;

        [Test]
        [Ignore("GDBWriter and GDBReader not implemented")]
        public void NormalizedGdbOperation()
        {
            polygon.Normalize();

            //Byte[] bytes = new GDBWriter().Write(polygon);
            //IGeometry test = new GDBReader().Read(bytes);

            //Assert.IsNotNull(test);            
        }

        [Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        [Ignore("GDBWriter and GDBReader not implemented")]
        public void NotNormalizedGdbOperation()
        {
            //Byte[] bytes = new GDBWriter().Write(polygon);
            //IGeometry test = new GDBReader().Read(bytes);

            //Assert.IsNull(test);    
        }
    }
}