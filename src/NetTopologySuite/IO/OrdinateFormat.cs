using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

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
        const string REP_POS_INF = "Inf";

        ///<summary>
        /// The output representation of <see cref="double.NegativeInfinity"/>
        /// </summary>
        const string REP_NEG_INF = "-Inf";

        ///<summary>
        /// The output representation of <see cref="double.NaN"/>
        /// </summary>
        const string REP_NAN = "NaN";

        
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

        private readonly NumberFormatInfo _format;
        private readonly string _formatText;

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
            _format = CreateFormat(maximumFractionDigits);
            _formatText = maximumFractionDigits < 16
                ? "{" + $"0:0.{new string('#', maximumFractionDigits)}" + "}"
                : "{0:R}";
                //: "{0:G17}";
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
            string res = string.Format(_format, _formatText, ord);
            int posE = res.IndexOf('E');
            if (posE < 0) return res;

            int exp = int.Parse(res.Substring(posE + 1));
            int posD = res.IndexOf('.');
            if (posD < 0) posD = posE;
            int numberOffset = ord < 0 ? 1 : 0;
            string whole = res.Substring(numberOffset, posD-numberOffset);
            string fraction = posE > posD ? res.Substring(posD + 1, posE - posD - 1) : string.Empty;
            
            var sb = new StringBuilder();
            if (ord < 0) sb.Append("-");
            if (exp >= 0)
            {
                sb.Append(whole);
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
                sb.Append(whole);
                sb.Append(fraction);
            }

            return sb.ToString();
        }
    }
}
