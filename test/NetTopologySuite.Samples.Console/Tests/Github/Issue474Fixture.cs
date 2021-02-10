using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    [Category("GitHub Issue")]
    [Category("GitHub Issue #474")]
    public class Issue474Fixture
    {
        const string Wkt1 = "POLYGON ((180468.153 436502.89, 181385.164 436064.011, 181864.723 435504.13, 183905.374 434543.219, 183947.748 433885.974, 181879.615 434884.596, 180975.23 435703.384, 179967.81 436141.722, 180468.153 436502.89))";
        const string Wkt2 = "POLYGON ((180468.15 436502.89, 181385.16 436064.01, 181864.72 435504.13, 183905.37 434543.22, 183947.75 433885.97, 181879.61 434884.6, 180975.23 435703.38, 179967.81 436141.72, 180468.15 436502.89))";

        [Test]
        public void TestOriginal()
        {
            var services = new NtsGeometryServices(CoordinateArraySequenceFactory.Instance, new PrecisionModel(1000d), 28992);

            // Geometry from WKT
            var reader = new IO.WKTReader(services);
            var geom1 = reader.Read(Wkt1);
            var geom2 = reader.Read(Wkt2);

            // Validate geometry
            Assert.IsTrue(geom1.IsValid);
            Assert.IsTrue(geom2.IsValid);

            try
            {
                var intersection = geom1.Intersection(geom2);
            }
            catch (TopologyException e)
            {
                Assert.Fail($"Intersection failed with:\n{e}");
            }
        }

        [Test]
        public void TestUsingOverlayNG()
        {
            var services = new NtsGeometryServices(CoordinateArraySequenceFactory.Instance, new PrecisionModel(1000d), 28992, GeometryOverlay.NG);

            // Geometry from WKT
            var reader = new IO.WKTReader(services);
            var geom1 = reader.Read(Wkt1);
            var geom2 = reader.Read(Wkt2);

            // Validate geometry
            Assert.IsTrue(geom1.IsValid);
            Assert.IsTrue(geom2.IsValid);

            try
            {
                var intersection = geom1.Intersection(geom2);
            }
            catch (TopologyException e)
            {
                Assert.Fail($"Intersection failed with:\n{e}");
            }
        }
    }
}
