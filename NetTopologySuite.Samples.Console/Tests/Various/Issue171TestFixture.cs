using System;
using System.Globalization;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    // see https://code.google.com/p/nettopologysuite/issues/detail?id=171
    [TestFixture]
    public class Issue171TestFixture
    {
        [Test]
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

        [Test]
        public void large_decimals_are_formatted_properly()
        {
            const string expected = "123456789012345680";
            const decimal l = 123456789012345680;

            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            NumberFormatInfo formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);
            string actual = l.ToString(format, formatter);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void large_doubles_should_be_formatted_properly()
        {
            const string expected = "123456789012345680";
            const double d = 123456789012345680;

            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            NumberFormatInfo formatter = CreateFormatter(precisionModel);
            string format = "0." + StringOfChar('#', formatter.NumberDecimalDigits);            
            string actual = d.ToString(format, formatter);
            Assert.That(actual, Is.EqualTo(expected));
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
