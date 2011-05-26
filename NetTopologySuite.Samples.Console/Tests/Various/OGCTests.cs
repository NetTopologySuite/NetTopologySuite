using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    [Ignore("The expected result geometries are wrong!")]
    public class OgcTests : BaseSamples
    {
        private const String BlueLakeWkt =
            "POLYGON((52 18,66 23,73 9,48 6,52 18),(59 18,67 18,67 13,59 13,59 18))";

        private const String AshtonWkt = "POLYGON(( 62 48, 84 48, 84 30, 56 30, 56 34, 62 48))";

        private readonly IGeometry _blueLake;
        private readonly IGeometry _ashton;


        public OgcTests()
            : base(GeometryServices.GetGeometryFactory(PrecisionModelType.Fixed))
        {
            _blueLake = Reader.Read(BlueLakeWkt);
            _ashton = Reader.Read(AshtonWkt);
        }

        [Test]
        public void OgcSymDifferenceTest()
        {
            Assert.IsNotNull(_blueLake);
            Assert.IsNotNull(_ashton);

            IGeometry expected = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18))");
            IGeometry result = _blueLake.SymmetricDifference(_ashton);

            Debug.WriteLine(result);
            //Assert.IsTrue(result.EqualsExact(expected));
            Assert.IsTrue(result.Equals(expected));
        }

        [Test]
        public void OgcUnionTest()
        {
            Assert.IsNotNull(_blueLake);
            Assert.IsNotNull(_ashton);

            IGeometry expected = Reader.Read("POLYGON((52 18,66 23,73 9,48 6,52 18))");
            IGeometry result = _blueLake.Union(_ashton);

            Debug.WriteLine(result);
            //Assert.IsTrue(result.EqualsExact(expected));
            Assert.IsTrue(result.Equals(expected));
        }
    }
}