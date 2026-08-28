using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.GeometriesGraph
{
    /// <summary>
    /// Tests <see cref="EdgeIntersection.Equals"/> and <see cref="EdgeIntersection.GetHashCode"/>
    /// for consistency with <see cref="EdgeIntersection.CompareTo"/> (JTS #1184).
    /// </summary>
    /// <remarks>
    /// Ported from JTS commit
    /// <see href="https://github.com/locationtech/jts/commit/d923a011c75c31d83428a7f63446e955788a9ad2"/>
    /// (JTS locationtech/jts#1201)
    /// </remarks>
    [TestFixture]
    public class EdgeIntersectionTest
    {
        [Test]
        public void TestEqualsValuesConsistentWithCompareTo()
        {
            var ei = new EdgeIntersection(new Coordinate(1, 2), 3, 0.5);
            var eiSame = new EdgeIntersection(new Coordinate(1, 2), 3, 0.5);
            Assert.That(ei.CompareTo(eiSame), Is.EqualTo(0));
            Assert.That(ei, Is.EqualTo(eiSame));
        }

        [Test]
        public void TestEqualsHashCodeContract()
        {
            var ei = new EdgeIntersection(new Coordinate(1, 2), 3, 0.5);
            var eiSame = new EdgeIntersection(new Coordinate(1, 2), 3, 0.5);
            Assert.That(ei.GetHashCode(), Is.EqualTo(eiSame.GetHashCode()));
            var set = new HashSet<EdgeIntersection>();
            set.Add(ei);
            set.Add(eiSame);
            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
