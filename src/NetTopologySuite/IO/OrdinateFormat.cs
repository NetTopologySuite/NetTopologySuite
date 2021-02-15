using System;
using System.Globalization;
using System.Text;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Formats numeric values for ordinates
    /// in a consistent, accurate way.
    /// <para/>
    /// The format has the following characteristics:
    /// <list type="Bullet">
    /// <item>It is consistent in all locales (in particular, the decimal separator is always a period)</item>
    /// <item><description>Scientific notation is never output, even for very large numbers. This means that it is possible that output can contain a large number of digits. </description></item>
    /// <item><description>The maximum number of decimal places reflects the available precision</description></item>
    /// <item><description>NaN values are represented as "NaN"</description></item>
    /// <item><description>Inf values are represented as "Inf" or "-Inf"</description></item>
    /// </list>
    /// </summary>
    /// <author>mdavis</author>
    public class OrdinateFormat
    {
        ///<summary>
        /// The output representation of <see cref="double.PositiveInfinity"/>
        /// </summary>
        internal const string REP_POS_INF = "Inf";

        ///<summary>
        /// The output representation of <see cref="double.NegativeInfinity"/>
        /// </summary>
        internal const string REP_NEG_INF = "-Inf";

        ///<summary>
        /// The output representation of <see cref="double.NaN"/>
        /// </summary>
        internal const string REP_NAN = "NaN";

        
        /// <summary>
        /// The maximum number of fraction digits to support output of reasonable ordinate values.
        /// <para/>
        /// The default is chosen to allow representing the smallest possible IEEE-754 double-precision value,
        /// although this is not expected to occur (and is not supported by other areas of the JTS/NTS code).
        /// </summary>
        const int MAX_FRACTION_DIGITS = 325;
        
        /// <summary>
        /// The default formatter using the maximum number of digits in the fraction portion of a number.
        /// </summary>
        public static OrdinateFormat Default = new OrdinateFormat();

        /*
        /// <summary>
        /// Creates a new formatter with the given maximum number of digits in the fraction portion of a number.
        /// </summary>
        /// <param name="maximumFractionDigits">the maximum number of fraction digits to output</param>
        /// <returns>A formatter</returns>
        public static OrdinateFormat Create(int maximumFractionDigits)
        {
            return new OrdinateFormat(maximumFractionDigits);
        }
         */

        private readonly string _format;

        /// <summary>
        /// Creates an OrdinateFormat using the default maximum number of fraction digits.
        /// </summary>
        public OrdinateFormat() : this(MAX_FRACTION_DIGITS)
        {
        }

        /// <summary>
        /// Creates an OrdinateFormat using the given maximum number of fraction digits.
        /// </summary>
        /// <param name="maximumFractionDigits">The maximum number of fraction digits to output</param>
        public OrdinateFormat(int maximumFractionDigits)
        {
            _format = maximumFractionDigits < 16
                ? $"0.{new string('#', maximumFractionDigits)}"
                : "R";
        }

        internal static NumberFormatInfo CreateFormat(int maximumFractionDigits)
        {
            // specify decimal separator explicitly to work in all locales
            var nfi = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
            nfi.NaNSymbol = REP_NAN;
            nfi.PositiveInfinitySymbol = REP_POS_INF;
            nfi.NegativeInfinitySymbol = REP_NEG_INF;
            nfi.NumberGroupSizes = Array.Empty<int>();
            nfi.NumberNegativePattern = 1;
            nfi.NumberDecimalDigits = Math.Min(99, maximumFractionDigits);
            
            return nfi;
        }

        /// <summary>
        /// Returns a string representation of the given ordinate numeric value.
        /// </summary>
        /// <param name="ord">The ordinate value</param>
        /// <returns>The formatted number string</returns>
        public string Format(double ord)
        {
            // finite values only pass through this one outer branch
            if ((BitConverter.DoubleToInt64Bits(ord) & 0x7FFFFFFFFFFFFFFF) >= 0x7FF0000000000000)
            {
                if (double.IsNaN(ord))
                {
                    return REP_NAN;
                }

                if (double.IsPositiveInfinity(ord))
                {
                    return REP_POS_INF;
                }

                if (double.IsNegativeInfinity(ord))
                {
                    return REP_NEG_INF;
                }

                Assert.ShouldNeverReachHere("All values are either finite, NaN, +Inf, or -Inf.");
            }

            string res;
            if (_format == "R")
            {
                res = ord.ToString("R", CultureInfo.InvariantCulture);

                // use the built-in parsing to check for exponents; some popular platforms have a
                // bug that we ought to work around, so we want to parse it anyway.
                if (double.TryParse(res, NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double parsed))
                {
                    // work around a bug in .NET Framework and .NET Core pre-3.0.  Those versions
                    // will still occasionally see "too many" significant digits regardless of what
                    // we do in this block (the fix was quite comprehensive!), but the value will at
                    // least be something that won't round differently (airbreather 2020-03-25).
                    // see: https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/
                    if (ord != parsed)
                    {
                        // the "G15" step is *probably* redundant, but it's here for safety...
                        // callers who are interested in getting the best possible performance will
                        // be on the latest version of .NET Core, which should never get here anyway
                        res = ord.ToString("G15", CultureInfo.InvariantCulture);
                        if (double.TryParse(res, NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out parsed) && ord != parsed)
                        {
                            res = ord.ToString("G16", CultureInfo.InvariantCulture);
                            if (double.TryParse(res, NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out parsed) && ord != parsed)
                            {
                                res = ord.ToString("G17", CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    return res;
                }

                // else it has an exponent in it, which we need to account for.
            }
            else
            {
                return ord.ToString(_format, CultureInfo.InvariantCulture);
            }

            int posE = res.IndexOf('E');

            int exp = int.Parse(res.Substring(posE + 1));
            int posD = res.IndexOf('.');
            if (posD < 0) posD = posE;
            int numberOffset = ord < 0 ? 1 : 0;
            string fraction = posE > posD ? res.Substring(posD + 1, posE - posD - 1) : string.Empty;
            
            var sb = new StringBuilder();
            if (ord < 0) sb.Append("-");
            if (exp >= 0)
            {
                sb.Append(res, numberOffset, posD - numberOffset);
                if (fraction.Length <= exp)
                    sb.Append(fraction);
                else
                {
                    sb.Append(fraction, 0, exp);
                    sb.Append(".");
                    sb.Append(fraction, exp, fraction.Length-exp);
                }

                if (exp - fraction.Length > 0)
                    sb.Append('0', exp - fraction.Length);
            }
            else
            {
                exp = Math.Abs(exp);
                sb.Append("0.");
                sb.Append('0', exp - 1);
                sb.Append(res, numberOffset, posD - numberOffset);
                sb.Append(fraction);
            }

            return sb.ToString();
        }
    }
}
