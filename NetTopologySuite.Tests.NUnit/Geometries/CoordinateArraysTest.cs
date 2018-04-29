using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class CoordinateArraysTest
    {
        [Test]
        public void TestPtNotInList1()
        {
            Coordinate list = CoordinateArrays.PointNotInList(
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new[] { new Coordinate(1, 1), new Coordinate(1, 2), new Coordinate(1, 3) }
                );
            Assert.IsTrue(list.Equals2D(new Coordinate(2, 2)));
        }
        [Test]
        public void TestPtNotInList2()
        {
            Coordinate list = CoordinateArrays.PointNotInList(
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) }
                );
            Assert.IsTrue(list == null);
        }
        private static readonly Coordinate[] Coords1 = { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) };
        private static readonly Coordinate[] Empty = new Coordinate[0];
        [Test]
        public void TestEnvelope1()
        {
            Assert.AreEqual(CoordinateArrays.Envelope(Coords1), new Envelope(1, 3, 1, 3));
        }
        [Test]
        public void TestEnvelopeEmpty()
        {
            Assert.AreEqual(CoordinateArrays.Envelope(Empty), new Envelope());
        }
        [Test]
        public void TestIntersection_envelope1()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Coords1, new Envelope(1, 2, 1, 2)),
                new[] { new Coordinate(1, 1), new Coordinate(2, 2) }
                ));
        }
        [Test]
        public void TestIntersection_envelopeDisjoint()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Coords1, new Envelope(10, 20, 10, 20)), Empty)
                );
        }
        [Test]
        public void TestIntersection_empty_envelope()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Empty, new Envelope(1, 2, 1, 2)), Empty)
                );
        }
        public void TestIntersection_coords_emptyEnvelope()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Coords1, new Envelope()), Empty)
                );
        }
    }
}
