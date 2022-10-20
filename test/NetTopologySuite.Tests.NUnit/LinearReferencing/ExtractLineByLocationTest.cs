using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    internal class ExtractLineByLocationTest : GeometryTestCase
    {
        [Test]
        public void TestSimple()
        {
            CheckExtract("LINESTRING(0 0, 10 0, 10 5, 10 10, 0 10, 0 0)", 2, 3, "LINESTRING(10 5, 10 10)");
        }

        public void CheckExtract(string geomWkt, int startIdx, int endIdx, string expectedWkt)
        {
            var geom = Read(geomWkt);
            var ptStart = geom.Factory.CreatePoint(geom.Coordinates[startIdx]);
            var ptEnd = geom.Factory.CreatePoint(geom.Coordinates[endIdx]);

            CheckExtract(geom, ptStart, ptEnd, expectedWkt);
        }

        private void CheckExtract(Geometry geom, Point ptStart, Point ptEnd, string expectedWkt)
        {
            var lip = new NetTopologySuite.LinearReferencing.LengthIndexOfPoint(geom);
            var llm = new NetTopologySuite.LinearReferencing.LengthLocationMap(geom);
            var llStart = llm.GetLocation(lip.IndexOf(ptStart.Coordinate));
            var llEnd = llm.GetLocation(lip.IndexOf(ptEnd.Coordinate));

            var expected = Read(expectedWkt);
            var result = NetTopologySuite.LinearReferencing.ExtractLineByLocation.Extract(geom, llStart, llEnd);

            Assert.That(result.EqualsExact(expected));
        }
    }
}
