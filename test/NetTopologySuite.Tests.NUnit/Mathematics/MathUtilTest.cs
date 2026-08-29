using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    public class MathUtilTest
    {
        private const double Tolerance = 1e-12;

        /// <summary>
        /// Showcases the bug in Max(double, double, double):
        ///   if (v2 > v3) max = v3;   // assigns the SMALLER value
        /// With v1=1, v2=3, v3=2 the correct answer is 3, but the buggy
        /// implementation sets max=v3 (2) because v2 > v3, returning 2.
        /// </summary>
        [Test]
        public void TestMax3_BugShowcase()
        {
            // Arrange: v2 is the largest, and v2 > v3, which triggers the bug.
            double v1 = 1, v2 = 3, v3 = 2;

            // Act
            double result = MathUtil.Max(v1, v2, v3);

            // Assert: expected 3, but the buggy code returns 2.
            Assert.That(result, Is.EqualTo(3).Within(Tolerance),
                $"Max({v1},{v2},{v3}) returned {result} instead of 3 — " +
                "the second condition 'if (v2 > v3) max = v3' wrongly replaces " +
                "the running maximum with the smaller value v3.");
        }

        [Test]
        public void TestMax3_AllPermutations()
        {
            // Exhaustively check every permutation so the largest is always returned.
            double a = 1, b = 2, c = 3;
            Assert.That(MathUtil.Max(a, b, c), Is.EqualTo(3).Within(Tolerance));
            Assert.That(MathUtil.Max(a, c, b), Is.EqualTo(3).Within(Tolerance));
            Assert.That(MathUtil.Max(b, a, c), Is.EqualTo(3).Within(Tolerance));
            Assert.That(MathUtil.Max(b, c, a), Is.EqualTo(3).Within(Tolerance));
            Assert.That(MathUtil.Max(c, a, b), Is.EqualTo(3).Within(Tolerance));
            Assert.That(MathUtil.Max(c, b, a), Is.EqualTo(3).Within(Tolerance));
        }
    }
}
