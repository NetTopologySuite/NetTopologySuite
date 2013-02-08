using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class NormalizeTest : BaseSamples
    {
        private IPolygon _polygon = null;
        private ILinearRing _shell = null;
        private ILinearRing _hole = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeTest"/> class.
        /// </summary>
        public NormalizeTest() : base() { }

        /// <summary>
        /// Method called prior to every test in this fixture
        /// </summary>
        [SetUp]
        public void Init()
        {
            _shell = Factory.CreateLinearRing(new Coordinate[] {    new Coordinate(100,100),
                                                                    new Coordinate(200,100),
                                                                    new Coordinate(200,200),                
                                                                    new Coordinate(100,200),
                                                                    new Coordinate(100,100), });
            // NOTE: Hole is created with not correct order for holes
            _hole = Factory.CreateLinearRing(new Coordinate[] {      new Coordinate(120,120),
                                                                    new Coordinate(180,120),
                                                                    new Coordinate(180,180),                                                                                
                                                                    new Coordinate(120,180),                                                                
                                                                    new Coordinate(120,120), });
            _polygon = Factory.CreatePolygon(_shell, new ILinearRing[] { _hole, });
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void NotNormalizedGDBOperation()
        {                        
	        byte[] bytes = new GDBWriter().Write(_polygon);
            IGeometry test = new GDBReader().Read(bytes);

            //This is no longer true
            //Assert.IsNull(test);    
            Assert.IsTrue(test.IsEmpty);
            Assert.IsTrue(test is IPolygonal);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]        
        public void NormalizedGDBOperation()
        {
            _polygon.Normalize();

            byte[] bytes = new GDBWriter().Write(_polygon);
            IGeometry test = new GDBReader().Read(bytes);

            Assert.IsNotNull(test);
            Assert.IsTrue(_polygon.EqualsExact(test));
        }
    }
}
