using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    /// <summary>
    /// Tests <see cref="OrientedCoordinateArray.Equals"/> and <see cref="OrientedCoordinateArray.GetHashCode"/>
    /// for consistency with <see cref="OrientedCoordinateArray.CompareTo"/> (JTS #1184), including
    /// the orientation-independence the class exists to provide.
    /// </summary>
    /// <remarks>
    /// Ported from JTS commit
    /// <see href="https://github.com/locationtech/jts/commit/d923a011c75c31d83428a7f63446e955788a9ad2"/>
    /// (JTS locationtech/jts#1201)
    /// </remarks>
    [TestFixture]
    public class OrientedCoordinateArrayTest
    {
        private static Coordinate[] Coords(params double[] xy)
        {
            var pts = new Coordinate[xy.Length / 2];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new Coordinate(xy[2 * i], xy[2 * i + 1]);
            return pts;
        }

        private static Coordinate[] Reverse(Coordinate[] pts)
        {
            var r = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                r[i] = pts[pts.Length - 1 - i];
            return r;
        }

        [Test]
        public void TestEqualsValuesConsistentWithCompareTo()
        {
            var a = new OrientedCoordinateArray(Coords(0, 0, 1, 1, 2, 0));
            var b = new OrientedCoordinateArray(Coords(0, 0, 1, 1, 2, 0));
            Assert.That(a.CompareTo(b), Is.EqualTo(0));
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void TestEqualsHashCodeContract()
        {
            var a = new OrientedCoordinateArray(Coords(0, 0, 1, 1, 2, 0));
            var b = new OrientedCoordinateArray(Coords(0, 0, 1, 1, 2, 0));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            var set = new HashSet<OrientedCoordinateArray>();
            set.Add(a);
            set.Add(b);
            Assert.That(set.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestEqualsHashCodeOrientationIndependent()
        {
            // an asymmetric array, so a stored-order hash would differ from its reverse
            var pts = Coords(0, 0, 5, 1, 2, 7, 9, 3);
            var fwd = new OrientedCoordinateArray(pts);
            var rev = new OrientedCoordinateArray(Reverse(pts));
            // the class compares orientation-independently, so these are equal ...
            Assert.That(fwd.CompareTo(rev), Is.EqualTo(0));
            Assert.That(fwd, Is.EqualTo(rev));
            // ... therefore their hash codes must agree, and they must dedup
            Assert.That(fwd.GetHashCode(), Is.EqualTo(rev.GetHashCode()));
            var set = new HashSet<OrientedCoordinateArray>();
            set.Add(fwd);
            set.Add(rev);
            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
