using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class CoordinateListTest
    {
        [Test]
        public void TestForward()
        {
            CheckValue(CoordList(0, 0, 1, 1, 2, 2).ToCoordinateArray(true),
                0, 0, 1, 1, 2, 2);
        }

        [Test]
        public void TestReverse()
        {
            CheckValue(CoordList(0, 0, 1, 1, 2, 2).ToCoordinateArray(false),
                2, 2, 1, 1, 0, 0);
        }

        [Test]
        public void TestReverseEmpty()
        {
            CheckValue(CoordList().ToCoordinateArray(false));
        }

        private static void CheckValue(Coordinate[] coordArray, params double[] ords)
        {

            Assert.That(coordArray.Length * 2, Is.EqualTo(ords.Length));

            for (int i = 0; i < coordArray.Length; i += 2)
            {
                var pt = coordArray[i];
                Assert.That(pt.X, Is.EqualTo(ords[2 * i]));
                Assert.That(pt.Y, Is.EqualTo(ords[2 * i + 1]));
            }
        }

        private static CoordinateList CoordList(params double[] ords)
        {
            var cl = new CoordinateList();
            for (int i = 0; i < ords.Length; i += 2)
            {
                cl.Add(new Coordinate(ords[i], ords[i + 1]), false);
            }
            return cl;
        }
    }
}
