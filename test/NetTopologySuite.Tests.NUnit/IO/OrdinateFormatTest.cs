using System;
using System.Diagnostics;
using System.Globalization;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    public class OrdinateFormatTest
    {
        [Test]
        public void TestLargeNumber()
        {
            // ensure scientific notation is not used
            CheckFormat(1234567890.0, "1234567890");
        }

        [Test]
        public void TestDoubleMinMaxEpsilon()
        {
            CheckFormat(double.MaxValue, "179769313486231570000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            CheckFormat(double.MinValue, "-179769313486231570000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");

            // note: .NET Framework and .NET Core pre-3.0 fail this check.
            // see https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/
            CheckFormat(double.Epsilon, "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005");
        }

        [Test]
        public void TestVeryLargeNumber()
        {
            // ensure scientific notation is not used
            // note output is rounded since it exceeds double precision accuracy
            CheckFormat(12345678901234567890.0, "12345678901234567000");
        }

        [Test]
        public void TestDecimalPoint()
        {
            CheckFormat(1.123, "1.123");
        }

        [Test]
        public void TestNegative()
        {
            CheckFormat(-1.123, "-1.123");
        }

        [Test]
        public void TestFractionDigits()
        {
            CheckFormat(1.123456789012345, "1.123456789012345");
            CheckFormat(0.0123456789012345, "0.0123456789012345");
            CheckFormat(1.123456789012345E4, "11234.56789012345");
        }

        [Test]
        public void TestLimitedFractionDigits2()
        {
            CheckFormat(1.123456789012345, 2, "1.12");
            CheckFormat(1.123456789012345, 3, "1.123");
            CheckFormat(1.123456789012345, 4, "1.1235");
            CheckFormat(1.123456789012345, 5, "1.12346");
            CheckFormat(1.123456789012345, 6, "1.123457");
        }

        [Test]
        public void TestMaximumFractionDigits()
        {
            CheckFormat(0.0000000000123456789012345, "0.0000000000123456789012345");
        }

        [TestCase(0.84551240822557006, "0.8455124082255701")]
        public void TestValue(double value, string expected)
        {
            // note: .NET Framework and .NET Core pre-3.0 fail this check.
            // see https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/
            CheckFormat(value, expected);
        }

        [Test]
        public void TestPi()
        {
            // note: .NET Framework and .NET Core pre-3.0 fail this check.
            // see https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/
            CheckFormat(Math.PI, "3.141592653589793");
        }

        /*
        [Test, Ignore("Just testing")]
        public void TestFormat()
        {
            TestContext.WriteLine(string.Format(NumberFormatInfo.InvariantInfo,
                $"{{0:0.{new string('#', 325)}}}", double.MaxValue));
            TestContext.WriteLine(string.Format(NumberFormatInfo.InvariantInfo,
                "{0:G17}", double.MaxValue));

            double parsed1 = double.Parse(string.Format(NumberFormatInfo.InvariantInfo,
                $"{{0:0.{new string('#', 325)}}}", double.MaxValue), NumberFormatInfo.InvariantInfo);

            double parsed2 = double.Parse(string.Format(NumberFormatInfo.InvariantInfo,
                "{0:G17}", double.MaxValue), NumberFormatInfo.InvariantInfo);
            Assert.That(parsed1, Is.EqualTo(double.MaxValue));

            TestContext.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, $"{{0:0.{new string('#', 325)}}}", Math.PI));
            TestContext.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, $"{{0:R}}", Math.PI));
            TestContext.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, $"{{0:G17}}", Math.PI));
        }
        */

        [Test]
        public void TestNaN()
        {
            CheckFormat(double.NaN, "NaN");
        }

        [Test]
        public void TestInf()
        {
            CheckFormat(double.PositiveInfinity, "Inf");
            CheckFormat(double.NegativeInfinity, "-Inf");
        }

        private void CheckFormat(double d, string expected)
        {
            string actual = OrdinateFormat.Default.Format(d);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private void CheckFormat(double d, int maxFractionDigits, string expected)
        {
            var format = new OrdinateFormat(maxFractionDigits);
            string actual = format.Format(d);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
