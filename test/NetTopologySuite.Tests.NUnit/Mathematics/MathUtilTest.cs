using System;
using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    public class MathUtilTest
    {
        private const double Tolerance = 1e-12;

        [Test]
        public void TestHypot_ZeroAndZero()
        {
            Assert.That(MathUtil.Hypot(0, 0), Is.EqualTo(0).Within(Tolerance));
        }

        [Test]
        public void TestHypot_AlongX()
        {
            Assert.That(MathUtil.Hypot(3, 0), Is.EqualTo(3).Within(Tolerance));
            Assert.That(MathUtil.Hypot(-3, 0), Is.EqualTo(3).Within(Tolerance));
        }

        [Test]
        public void TestHypot_AlongY()
        {
            Assert.That(MathUtil.Hypot(0, 4), Is.EqualTo(4).Within(Tolerance));
            Assert.That(MathUtil.Hypot(0, -4), Is.EqualTo(4).Within(Tolerance));
        }

        [Test]
        public void TestHypot_ThreeFourFive()
        {
            Assert.That(MathUtil.Hypot(3, 4), Is.EqualTo(5).Within(Tolerance));
            Assert.That(MathUtil.Hypot(-3, -4), Is.EqualTo(5).Within(Tolerance));
        }

        [Test]
        public void TestHypot_MatchesNaiveFormula()
        {
            // JTS Hypot is deliberately the naive formula (not the overflow-safe
            // scaled variant). Document and pin this with explicit equivalence.
            double x = 1.5, y = 2.5;
            Assert.That(MathUtil.Hypot(x, y), Is.EqualTo(Math.Sqrt(x * x + y * y)).Within(Tolerance));
        }
    }
}
