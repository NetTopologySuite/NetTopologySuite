using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class PrecisionUtilityTest : GeometryTestCase
    {
        [Test] public void TestInts()
        {
            CheckRobustScale("POINT(1 1)", "POINT(10 10)",
                1, 1e12, 1);
        }

        [Test] public void TestBNull()
        {
            CheckRobustScale("POINT(1 1)", null,
                1, 1e13, 1);
        }

        [Test] public void TestPower10()
        {
            CheckRobustScale("POINT(100 100)", "POINT(1000 1000)",
                1, 1e11, 1);
        }

        [Test] public void TestDecimalsDifferent()
        {
            CheckRobustScale("POINT( 1.123 1.12 )", "POINT( 10.123 10.12345 )",
                1e5, 1e12, 1e5);
        }

        [Test] public void TestDecimalsShort()
        {
            CheckRobustScale("POINT(1 1.12345)", "POINT(10 10)",
                1e5, 1e12, 1e5);
        }

        [Test] public void TestDecimalsMany()
        {
            CheckRobustScale("POINT(1 1.123451234512345)", "POINT(10 10)",
                1e12, 1e12, 1e15);
        }

        [Test] public void TestDecimalsAllLong()
        {
            CheckRobustScale("POINT( 1.123451234512345 1.123451234512345 )", "POINT( 10.123451234512345 10.123451234512345 )",
                1e12, 1e12, 1e15);
        }

        [Test] public void TestSafeScaleChosen()
        {
            CheckRobustScale("POINT( 123123.123451234512345 1 )", "POINT( 10 10 )",
                1e8, 1e8, 1e11);
        }

        [Test] public void TestSafeScaleChosenLargeMagnitude()
        {
            CheckRobustScale("POINT( 123123123.123451234512345 1 )", "POINT( 10 10 )",
                1e5, 1e5, 1e8);
        }

        [Test] public void TestInherentWithLargeMagnitude()
        {
            CheckRobustScale("POINT( 123123123.12 1 )", "POINT( 10 10 )",
                1e2, 1e5, 1e2);
        }

        [Test] public void TestMixedMagnitude()
        {
            CheckRobustScale("POINT( 1.123451234512345 1 )", "POINT( 100000.12345 10 )",
                1e8, 1e8, 1e15);
        }

        [Test] public void TestInherentBelowSafe()
        {
            CheckRobustScale("POINT( 100000.1234512 1 )", "POINT( 100000.12345 10 )",
                1e7, 1e8, 1e7);
        }

        private void CheckRobustScale(string wktA, string wktB, double scaleExpected,
            double safeScaleExpected, double inherentScaleExpected)
        {
            var a = Read(wktA);
            Geometry b = null;
            if (wktB != null)
            {
                b = Read(wktB);
            }
            double robustScale = PrecisionUtility.RobustScale(a, b);
            Assert.That(robustScale, Is.EqualTo(scaleExpected), "Auto scale: ");
            Assert.That(PrecisionUtility.InherentScale(a, b), Is.EqualTo(inherentScaleExpected), "Inherent scale: ");
            Assert.That(PrecisionUtility.SafeScale(a, b), Is.EqualTo(safeScaleExpected), "Safe scale: ");
        }

    }
}
