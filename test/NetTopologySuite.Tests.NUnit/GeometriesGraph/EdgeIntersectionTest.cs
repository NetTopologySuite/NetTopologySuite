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

        /// <summary>
        /// Regression test documenting the same NaN-related bug found in
        /// <see cref="NetTopologySuite.LinearReferencing.LinearLocation"/> (see review of PR #872):
        /// because IEEE-754 comparisons involving <c>NaN</c> are always <c>false</c>,
        /// <see cref="EdgeIntersection.Compare"/> (used by <see cref="EdgeIntersection.CompareTo"/>)
        /// falls through to <c>return 0</c> whenever a <c>NaN</c> distance is compared to
        /// <b>any</b> other distance on the same segment index. Since PR #872 defines
        /// <see cref="EdgeIntersection.Equals"/> as <c>CompareTo(other) == 0</c>, this makes
        /// <c>Equals</c> non-transitive: this test does not assert that any particular pair of
        /// intersections <b>should</b> be equal, only that <c>Equals</c> must satisfy the
        /// transitivity axiom of <see cref="object.Equals(object)"/> for whatever it does report.
        /// </summary>
        /// <remarks>
        /// This test currently FAILS against PR #872 as submitted, for a <c>NaN</c>-distance
        /// intersection that today compares equal to intersections at distance <c>0.2</c> and
        /// <c>0.9</c> (which are not equal to each other). It should pass once the NaN handling
        /// in <c>Compare</c>/<c>Equals</c>/<c>GetHashCode</c> is made mutually consistent (e.g. by
        /// giving <c>NaN</c> distances an explicit, well-defined ordering), regardless of what
        /// specific equality that fix settles on.
        /// </remarks>
        [Test]
        public void TestNaNDistanceBreaksEqualsTransitivity()
        {
            var nan = new EdgeIntersection(new Coordinate(1, 2), 3, double.NaN);
            var a = new EdgeIntersection(new Coordinate(1, 2), 3, 0.2);
            var b = new EdgeIntersection(new Coordinate(1, 2), 3, 0.9);

            bool aEqualsNan = a.Equals(nan);
            bool nanEqualsB = nan.Equals(b);
            bool aEqualsB = a.Equals(b);

            // Transitivity: if a == nan and nan == b, then a == b must also hold.
            // (If a future fix makes a != nan or nan != b, this premise no longer applies and the
            // test passes vacuously -- it only fails while the current bug's specific symptom,
            // NaN comparing equal to unrelated distances, is present.)
            if (aEqualsNan && nanEqualsB)
            {
                Assert.That(aEqualsB, Is.True,
                    "Equals violates transitivity: a == nan and nan == b, but a != b");
            }
        }

        /// <summary>
        /// Regression test documenting that the NaN-distance bug described in
        /// <see cref="TestNaNDistanceBreaksEqualsTransitivity"/> also breaks the
        /// <c>Equals</c>/<c>GetHashCode</c> contract directly: whenever
        /// <see cref="EdgeIntersection.Equals"/> reports two intersections as equal, their
        /// <see cref="EdgeIntersection.GetHashCode"/> must also match.
        /// </summary>
        /// <remarks>
        /// This test currently FAILS against PR #872 as submitted, for a <c>NaN</c>-distance
        /// intersection that today compares equal to an unrelated <c>0.7</c>-distance
        /// intersection but hashes differently. It should pass once the NaN handling is made
        /// mutually consistent.
        /// </remarks>
        [Test]
        public void TestNaNDistanceBreaksHashCodeContract()
        {
            var nan = new EdgeIntersection(new Coordinate(1, 2), 3, double.NaN);
            var other = new EdgeIntersection(new Coordinate(1, 2), 3, 0.7);

            // Equals/GetHashCode contract: equal objects must report equal hash codes.
            if (nan.Equals(other))
            {
                Assert.That(nan.GetHashCode(), Is.EqualTo(other.GetHashCode()),
                    "Equals/GetHashCode contract violated: nan.Equals(other) is true, but hash codes differ");
            }
        }
    }
}
