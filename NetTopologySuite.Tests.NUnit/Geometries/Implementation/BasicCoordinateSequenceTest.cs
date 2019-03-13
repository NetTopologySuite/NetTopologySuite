using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    [TestFixtureAttribute]
    public class BasicCoordinateSequenceTest
    {
        [TestAttribute]
        public void TestClone()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(
                new[] { new Coordinate(1, 2), new Coordinate(3, 4) });
            var s2 = (ICoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        [TestAttribute]
        public void TestCloneDimension2()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(2, 2);
            s1.SetOrdinate(0, Ordinate.X, 1);
            s1.SetOrdinate(0, Ordinate.Y, 2);
            s1.SetOrdinate(1, Ordinate.X, 3);
            s1.SetOrdinate(1, Ordinate.Y, 4);

            var s2 = (ICoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.Dimension == s2.Dimension);
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        [TestAttribute]
        public void TestCloneDimension3()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(2, 3);
            s1.SetOrdinate(0, Ordinate.X, 1);
            s1.SetOrdinate(0, Ordinate.Y, 2);
            s1.SetOrdinate(0, Ordinate.Z, 10);
            s1.SetOrdinate(1, Ordinate.X, 3);
            s1.SetOrdinate(1, Ordinate.Y, 4);
            s1.SetOrdinate(1, Ordinate.Z, 20);

            var s2 = (ICoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.Dimension == s2.Dimension);
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        [TestAttribute]
        public void TestCloneDimension4()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(2, 4, 1);
            s1.SetOrdinate(0, Ordinate.X, 1);
            s1.SetOrdinate(0, Ordinate.Y, 2);
            s1.SetOrdinate(0, Ordinate.Z, 10);
            s1.SetOrdinate(0, Ordinate.M, 100);
            s1.SetOrdinate(1, Ordinate.X, 3);
            s1.SetOrdinate(1, Ordinate.Y, 4);
            s1.SetOrdinate(1, Ordinate.Z, 20);
            s1.SetOrdinate(1, Ordinate.M, 200);

            var s2 = (ICoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.Dimension == s2.Dimension);
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        /// <summary>
        /// A simple test that using CoordinateM works
        /// for creation and running a basic function.
        /// </summary>
        [Test]
        public void TestLengthWithXYM()
        {
            CoordinateM[] coords =
            {
                new CoordinateM(1, 1, 1),
                new CoordinateM(2, 1, 2),
            };

            var factory = new GeometryFactory();
            var line = factory.CreateLineString(coords);

            double len = line.Length;
            Assert.That(len, Is.EqualTo(1));
        }
    }
}
