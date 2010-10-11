using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Overlay
{
    [TestFixture]
    public class FixedPrecisionSnappingTest
    {
        [Test]
	public void TestTriangles()
	{

            IGeometryFactory<Coordinate> fac = GeometryUtils.GetScaledFactory(1.0);
            IWktGeometryReader<Coordinate> rdr = fac.WktReader;

            IGeometry<Coordinate> a = rdr.Read("POLYGON ((545 317, 617 379, 581 321, 545 317))");
		    IGeometry<Coordinate> b = rdr.Read("POLYGON ((484 290, 558 359, 543 309, 484 290))");
		    IGeometry<Coordinate> res = a.Intersection(b);
            Assert.AreEqual(res.ToString(), "POINT (545 317)");
	}
    }
}