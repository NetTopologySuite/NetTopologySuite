using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Overlay
{
    [TestFixture]
    public class FixedPrecisionSnappingTest
    {
        private readonly WKTReader _rdr;

        public FixedPrecisionSnappingTest()
        {
            _rdr = new WKTReader(new NtsGeometryServices(PrecisionModel.Fixed.Value, 0));
        }

        [Test]
        public void TestTriangles()
        {
            var a = _rdr.Read("POLYGON ((545 317, 617 379, 581 321, 545 317))");
            var b = _rdr.Read("POLYGON ((484 290, 558 359, 543 309, 484 290))");
            a.Intersection(b);
        }
    }
}
