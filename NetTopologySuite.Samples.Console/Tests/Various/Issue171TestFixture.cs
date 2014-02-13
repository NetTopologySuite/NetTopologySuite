using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using GeoAPI.Geometries;
using MiscUtil.Conversion;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    // see https://code.google.com/p/nettopologysuite/issues/detail?id=171
    [TestFixture]
    public class Issue171TestFixture
    {
        [Test, Category("Issue171")]
        public void large_integers_are_formatted_properly()
        {
            const string expected = "123456789012345680";
            const long l = 123456789012345680;

            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            NumberFormatInfo formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);
            string actual = l.ToString(format, formatter);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Category("Issue171")]
        public void large_decimals_are_formatted_properly()
        {
            const string expected = "123456789012345680";
            const decimal m = 123456789012345680;

            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            NumberFormatInfo formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);
            string actual = m.ToString(format, formatter);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Category("Issue171")]
        public void large_doubles_arent_formatted_properly()
        {
            /*
             * http://stackoverflow.com/questions/2105096/why-is-tostring-rounding-my-double-value
             * 
             * By default, the return value only contains 15 digits of precision although a maximum of 17 digits is maintained internally. 
             */
            const string expected = "123456789012345680";
            const double d = 123456789012345680;

            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            NumberFormatInfo formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);
            string actual = d.ToString(format, formatter);
            Assert.That(actual, Is.Not.EqualTo(expected));
        }

        [Test, Category("Issue171")]
        public void large_doubles_are_formatted_properly_using_doubleconverter()
        {
            // see http://www.yoda.arachsys.com/csharp/DoubleConverter.cs
            const string expected = "123456789012345680";
            const double d = 123456789012345680;
            string actual = DoubleConverter.ToExactString(d);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Category("Issue171")]
        public void very_large_doubles_arent_formatted_properly_using_doubleconverter()
        {
            // some problems still remains :(
            const string expected = "123456789012345690000000000000000";
            const double d = 123456789012345678000000E9d;
            string actual = DoubleConverter.ToExactString(d);
            Assert.That(actual, Is.Not.EqualTo(expected));
        }

        [Test, Category("Issue171")]
        public void small_doubles_are_formatted_properly_using_doubleconverter()
        {
            const string expected = "123456";
            const double d = 123456;
            string actual = DoubleConverter.ToExactString(d);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Category("Issue171"), Category("Stress"), Explicit]
        public void performances_are_valid_using_doubleconverter()
        {
            DoPerformancesTest(1000);
            DoPerformancesTest(10000);
            DoPerformancesTest(100000);
            DoPerformancesTest(1000000);
            DoPerformancesTest(10000000);

        }

        private static void DoPerformancesTest(int times)
        {
            const double d = 123456789;
            Stopwatch w = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                string s = DoubleConverter.ToExactString(d + i);
                Assert.IsNotNull(s);
            }
            w.Stop();
            TimeSpan usingDc = w.Elapsed;

            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            NumberFormatInfo formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);
            w = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                string s = d.ToString(format, formatter);
                Assert.IsNotNull(s);
            }
            w.Stop();
            TimeSpan usingDef = w.Elapsed;

            if (usingDc <= usingDef)
                return;
            TimeSpan diff = usingDc - usingDef;
            Console.WriteLine("slower for {0}: {1} seconds", times, diff.TotalSeconds);
        }

        // same code used in WKTWriter
        private static NumberFormatInfo CreateFormatter(IPrecisionModel precisionModel)
        {
            int digits = precisionModel.MaximumSignificantDigits;
            int decimalPlaces = Math.Max(0, digits);
            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = decimalPlaces,
                NumberGroupSeparator = String.Empty,
                NumberGroupSizes = new int[] { }
            };
            return nfi;
        }

        // same code used in WKTWriter
        private static string StringOfChar(char ch, int count)
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < count; i++)
                buf.Append(ch);
            return buf.ToString();
        }
    }
}
