using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Tests methods involving only <see cref="LinearLocation" />s
    /// </summary>
    /// <author>Martin Davis</author>
    [TestFixture]
    public class LinearLocationTest
    {
        private readonly WKTReader reader = new WKTReader();

        [Test]
        public void TestZeroLengthLineString()
        {
            var line = reader.Read("LINESTRING (10 0, 10 0)");
            var indexedLine = new LocationIndexedLine(line);
            var loc0 = indexedLine.IndexOf(new Coordinate(11, 0));
            Assert.IsTrue(loc0.CompareTo(new LinearLocation(0, double.NaN)) == 0);
        }

        /// <summary>
        /// Tests that two locations that compare equal are also <see cref="LinearLocation.Equals"/>.
        /// </summary>
        /// <remarks>
        /// Ported from JTS commit
        /// <see href="https://github.com/locationtech/jts/commit/d923a011c75c31d83428a7f63446e955788a9ad2"/>
        /// (JTS locationtech/jts#1201, JTS #1184)
        /// </remarks>
        [Test]
        public void TestEqualsValuesConsistentWithCompareTo_JTS1184()
        {
            var loc = new LinearLocation(1, 2, 0.5);
            var locSame = new LinearLocation(1, 2, 0.5);
            Assert.That(loc.CompareTo(locSame), Is.EqualTo(0));
            Assert.That(loc, Is.EqualTo(locSame));
        }

        /// <summary>
        /// Tests that equal locations report equal hash codes, and dedup in hash-based collections.
        /// </summary>
        /// <remarks>
        /// Ported from JTS commit
        /// <see href="https://github.com/locationtech/jts/commit/d923a011c75c31d83428a7f63446e955788a9ad2"/>
        /// (JTS locationtech/jts#1201, JTS #1184)
        /// </remarks>
        [Test]
        public void TestEqualsHashCodeContract_JTS1184()
        {
            var loc = new LinearLocation(1, 2, 0.5);
            var locSame = new LinearLocation(1, 2, 0.5);
            Assert.That(loc.GetHashCode(), Is.EqualTo(locSame.GetHashCode()));
            var set = new System.Collections.Generic.HashSet<LinearLocation>();
            set.Add(loc);
            set.Add(locSame);
            Assert.That(set.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Regression test documenting a NaN-related bug found while reviewing JTS #1184
        /// (NTS PR #872): because IEEE-754 comparisons involving <c>NaN</c> are always
        /// <c>false</c>, <see cref="LinearLocation.CompareTo(LinearLocation)"/> falls through to
        /// <c>return 0</c> whenever a <c>NaN</c> segment fraction is compared to <b>any</b> other
        /// fraction (this NaN-tolerant branch is pre-existing NTS behavior exercised by
        /// <see cref="TestZeroLengthLineString"/>, not introduced by PR #872). Since PR #872
        /// defines <see cref="LinearLocation.Equals"/> as <c>CompareTo(other) == 0</c>, this
        /// makes <c>Equals</c> non-transitive: this test does not assert that any particular pair
        /// of locations <b>should</b> be equal, only that <c>Equals</c> must satisfy the
        /// transitivity axiom of <see cref="object.Equals(object)"/> for whatever it does report.
        /// </summary>
        /// <remarks>
        /// This test currently FAILS against PR #872 as submitted, for a <c>NaN</c>-fraction
        /// location that today compares equal to both <c>0.0</c> and <c>0.5</c> fractions (which
        /// are not equal to each other). It should pass once the NaN handling in
        /// <c>CompareTo</c>/<c>Equals</c>/<c>GetHashCode</c> is made mutually consistent (e.g. by
        /// giving <c>NaN</c> fractions an explicit, well-defined ordering), regardless of what
        /// specific equality that fix settles on.
        /// </remarks>
        [Test]
        public void TestNaNSegmentFractionBreaksEqualsTransitivity()
        {
            var zero = new LinearLocation(0, 0, 0.0);
            var nan = new LinearLocation(0, 0, double.NaN);
            var half = new LinearLocation(0, 0, 0.5);

            bool zeroEqualsNan = zero.Equals(nan);
            bool nanEqualsHalf = nan.Equals(half);
            bool zeroEqualsHalf = zero.Equals(half);

            // Transitivity: if zero == nan and nan == half, then zero == half must also hold.
            // (If a future fix makes zero != nan or nan != half, this premise no longer applies
            // and the test passes vacuously -- it only fails while the current bug's specific
            // symptom, NaN comparing equal to unrelated fractions, is present.)
            if (zeroEqualsNan && nanEqualsHalf)
            {
                Assert.That(zeroEqualsHalf, Is.True,
                    "Equals violates transitivity: zero == nan and nan == half, but zero != half");
            }
        }

        /// <summary>
        /// Regression test documenting that the NaN-fraction bug described in
        /// <see cref="TestNaNSegmentFractionBreaksEqualsTransitivity"/> also breaks the
        /// <c>Equals</c>/<c>GetHashCode</c> contract directly: whenever
        /// <see cref="LinearLocation.Equals"/> reports two locations as equal, their
        /// <see cref="LinearLocation.GetHashCode"/> must also match.
        /// </summary>
        /// <remarks>
        /// This test currently FAILS against PR #872 as submitted, for a <c>NaN</c>-fraction
        /// location that today compares equal to an unrelated <c>0.7</c>-fraction location but
        /// hashes differently. It should pass once the NaN handling is made mutually consistent.
        /// </remarks>
        [Test]
        public void TestNaNSegmentFractionBreaksHashCodeContract()
        {
            var nan = new LinearLocation(0, 0, double.NaN);
            var other = new LinearLocation(0, 0, 0.7);

            // Equals/GetHashCode contract: equal objects must report equal hash codes.
            if (nan.Equals(other))
            {
                Assert.That(nan.GetHashCode(), Is.EqualTo(other.GetHashCode()),
                    "Equals/GetHashCode contract violated: nan.Equals(other) is true, but hash codes differ");
            }
        }

        [Test]
        public void TestRepeatedCoordsLineString()
        {
            var line = reader.Read("LINESTRING (10 0, 10 0, 20 0)");
            var indexedLine = new LocationIndexedLine(line);
            var loc0 = indexedLine.IndexOf(new Coordinate(11, 0));
            Assert.IsTrue(loc0.CompareTo(new LinearLocation(1, 0.1)) == 0);
        }

        [Test]
        public void TestEndLocation()
        {
            var line = reader.Read("LINESTRING (10 0, 20 0)");
            var loc0 = LinearLocation.GetEndLocation(line);
            Assert.That(loc0.SegmentFraction, Is.EqualTo(0d));
            Assert.That(loc0.SegmentIndex, Is.EqualTo(1));

            var indexedLine = new LocationIndexedLine(line);
            var endLoc = indexedLine.EndIndex;
            var normLoc = new LinearLocation(endLoc.ComponentIndex, endLoc.SegmentIndex, endLoc.SegmentFraction);
            Assert.That(normLoc.ComponentIndex, Is.EqualTo(endLoc.ComponentIndex));
            Assert.That(normLoc.SegmentIndex, Is.EqualTo(endLoc.SegmentIndex));
            Assert.That(normLoc.SegmentFraction, Is.EqualTo(endLoc.SegmentFraction));
        }

        [Test]
        public void TestIsEndPoint()
        {
            var line = reader.Read("LINESTRING (10 0, 20 0)");

            Assert.True(!(new LinearLocation(0, 0)).IsEndpoint(line));
            Assert.True(!(new LinearLocation(0, 0.5)).IsEndpoint(line));
            Assert.True(!(new LinearLocation(0, 0.9999)).IsEndpoint(line));

            Assert.True((new LinearLocation(0, 1.0)).IsEndpoint(line));

            Assert.True((new LinearLocation(1, 0.0)).IsEndpoint(line));
            Assert.True((new LinearLocation(1, 0.5)).IsEndpoint(line));
            Assert.True((new LinearLocation(1, 1.0)).IsEndpoint(line));
            Assert.True((new LinearLocation(1, 1.5)).IsEndpoint(line));

            Assert.True((new LinearLocation(2, 0.5)).IsEndpoint(line));

            var loc = new LinearLocation(0, 0.0);
            loc.SetToEnd(line);
            Assert.True(loc.IsEndpoint(line));

            var locLow = loc.ToLowest(line);
            Assert.True(locLow.IsEndpoint(line));
        }

        [Test]
        public void TestEndPointLowest()
        {
            var line = reader.Read("LINESTRING (10 0, 20 0, 30 10)");

            Assert.True((new LinearLocation(1, 1.0)).IsEndpoint(line));
            Assert.True((new LinearLocation(2, 0.0)).IsEndpoint(line));
            Assert.True((new LinearLocation(2, 0.5)).IsEndpoint(line));

            var loc = new LinearLocation(0, 0.0);
            loc.SetToEnd(line);
            Assert.True(loc.IsEndpoint(line));
            Assert.AreEqual(2, loc.SegmentIndex);
            Assert.AreEqual(0.0, loc.SegmentFraction);

            var locLow = loc.ToLowest(line);
            Assert.True(locLow.IsEndpoint(line));
            Assert.AreEqual(1, locLow.SegmentIndex);
            Assert.AreEqual(1.0, locLow.SegmentFraction);
        }

        [Test]
        public void TestSameSegmentLineString()
        {
            var line = reader.Read("LINESTRING (0 0, 10 0, 20 0, 30 0)");
            var indexedLine = new LocationIndexedLine(line);

            var loc0 = indexedLine.IndexOf(new Coordinate(0, 0));
            var loc0_5 = indexedLine.IndexOf(new Coordinate(5, 0));
            var loc1 = indexedLine.IndexOf(new Coordinate(10, 0));
            var loc2 = indexedLine.IndexOf(new Coordinate(20, 0));
            var loc2_5 = indexedLine.IndexOf(new Coordinate(25, 0));
            var loc3 = indexedLine.IndexOf(new Coordinate(30, 0));

            Assert.IsTrue(loc0.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0.IsOnSameSegment(loc0_5));
            Assert.IsTrue(loc0.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2_5));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc3));

            Assert.IsTrue(loc0_5.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0_5.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc2.IsOnSameSegment(loc0));
            Assert.IsTrue(loc2.IsOnSameSegment(loc1));
            Assert.IsTrue(loc2.IsOnSameSegment(loc2));
            Assert.IsTrue(loc2.IsOnSameSegment(loc3));

            Assert.IsTrue(loc2_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc3.IsOnSameSegment(loc0));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2_5));
            Assert.IsTrue(loc3.IsOnSameSegment(loc3));
        }

        [Test]
        public void TestSameSegmentMultiLineString()
        {
            var line = reader.Read("MULTILINESTRING ((0 0, 10 0, 20 0), (20 0, 30 0))");
            var indexedLine = new LocationIndexedLine(line);

            var loc0 = indexedLine.IndexOf(new Coordinate(0, 0));
            var loc0_5 = indexedLine.IndexOf(new Coordinate(5, 0));
            var loc1 = indexedLine.IndexOf(new Coordinate(10, 0));
            var loc2 = indexedLine.IndexOf(new Coordinate(20, 0));
            var loc2B = new LinearLocation(1, 0, 0.0);

            var loc2_5 = indexedLine.IndexOf(new Coordinate(25, 0));
            var loc3 = indexedLine.IndexOf(new Coordinate(30, 0));

            Assert.IsTrue(loc0.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0.IsOnSameSegment(loc0_5));
            Assert.IsTrue(loc0.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2_5));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc3));

            Assert.IsTrue(loc0_5.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0_5.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc2.IsOnSameSegment(loc0));
            Assert.IsTrue(loc2.IsOnSameSegment(loc1));
            Assert.IsTrue(loc2.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc2.IsOnSameSegment(loc3));
            Assert.IsTrue(loc2B.IsOnSameSegment(loc3));

            Assert.IsTrue(loc2_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc3.IsOnSameSegment(loc0));
            Assert.IsTrue(!loc3.IsOnSameSegment(loc2));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2B));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2_5));
            Assert.IsTrue(loc3.IsOnSameSegment(loc3));
        }

        [Test]
        public void TestGetSegmentMultiLineString()
        {
            var line = reader.Read("MULTILINESTRING ((0 0, 10 0, 20 0), (20 0, 30 0))");
            var indexedLine = new LocationIndexedLine(line);

            var loc0 = indexedLine.IndexOf(new Coordinate(0, 0));
            var loc0_5 = indexedLine.IndexOf(new Coordinate(5, 0));
            var loc1 = indexedLine.IndexOf(new Coordinate(10, 0));
            var loc2 = indexedLine.IndexOf(new Coordinate(20, 0));
            var loc2B = new LinearLocation(1, 0, 0.0);

            var loc2_5 = indexedLine.IndexOf(new Coordinate(25, 0));
            var loc3 = indexedLine.IndexOf(new Coordinate(30, 0));

            var seg0 = new LineSegment(new Coordinate(0, 0), new Coordinate(10, 0));
            var seg1 = new LineSegment(new Coordinate(10, 0), new Coordinate(20, 0));
            var seg2 = new LineSegment(new Coordinate(20, 0), new Coordinate(30, 0));

            Assert.IsTrue(loc0.GetSegment(line).Equals(seg0));
            Assert.IsTrue(loc0_5.GetSegment(line).Equals(seg0));

            Assert.IsTrue(loc1.GetSegment(line).Equals(seg1));
            Assert.IsTrue(loc2.GetSegment(line).Equals(seg1));

            Assert.IsTrue(loc2_5.GetSegment(line).Equals(seg2));
            Assert.IsTrue(loc3.GetSegment(line).Equals(seg2));
        }
    }
}
