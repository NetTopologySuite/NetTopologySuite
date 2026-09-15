using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    /// <summary>
    /// Tests <see cref="NetTopologySuite.Operation.RelateNG.NodeSection.Equals"/> and
    /// <see cref="NetTopologySuite.Operation.RelateNG.NodeSection.GetHashCode"/>
    /// for consistency with <see cref="NetTopologySuite.Operation.RelateNG.NodeSection.CompareTo"/> (JTS #1184).
    /// </summary>
    /// <remarks>
    /// Ported from JTS commit
    /// <see href="https://github.com/locationtech/jts/commit/d923a011c75c31d83428a7f63446e955788a9ad2"/>
    /// (JTS locationtech/jts#1201)
    /// </remarks>
    [TestFixture]
    public class NodeSectionTest
    {
        private static NetTopologySuite.Operation.RelateNG.NodeSection Section(int id, Coordinate v0, Coordinate v1)
        {
            var nodePt = new Coordinate(5, 5);
            return new NetTopologySuite.Operation.RelateNG.NodeSection(true, Dimension.A, id, 0, null, false, v0, nodePt, v1);
        }

        [Test]
        public void TestEqualsValuesConsistentWithCompareTo()
        {
            var ns = Section(1, new Coordinate(0, 0), new Coordinate(10, 10));
            var nsSame = Section(1, new Coordinate(0, 0), new Coordinate(10, 10));
            Assert.That(ns.CompareTo(nsSame), Is.EqualTo(0));
            Assert.That(ns, Is.EqualTo(nsSame));
        }

        [Test]
        public void TestEqualsHashCodeContract()
        {
            var ns = Section(1, new Coordinate(0, 0), new Coordinate(10, 10));
            var nsSame = Section(1, new Coordinate(0, 0), new Coordinate(10, 10));
            Assert.That(ns.GetHashCode(), Is.EqualTo(nsSame.GetHashCode()));
            var set = new HashSet<NetTopologySuite.Operation.RelateNG.NodeSection>();
            set.Add(ns);
            set.Add(nsSame);
            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
