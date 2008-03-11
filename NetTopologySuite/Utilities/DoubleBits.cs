using System;
using System.Diagnostics;
using GisSharpBlog.NetTopologySuite.Utilities;
#if NETCF
using BitConverter = GisSharpBlog.NetTopologySuite.Utilities;
#endif

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// <see cref="DoubleBits"/> manipulates <see cref="Double"/> numbers
    /// by using bit manipulation and bit-field extraction.
    /// For some operations (such as determining the exponent)
    /// this is more accurate than using mathematical operations
    /// (which suffer from round-off error).
    /// The algorithms and constants in this class
    /// apply only to IEEE-754 double-precision floating point format.
    /// </summary>
    public struct DoubleBits
    {
        /// <summary>
        /// Value to add to the exponent in order to make it positive. This
        /// avoids using using two's-complement in the exponent, which inverts the 
        /// values of the double value's exponents bits for negative exponents.
        /// </summary>
        public const Int32 ExponentBias = 1023;

        public static Double PowerOf2(Int32 exp)
        {
            if (exp > 1023 || exp < -1022)
            {
                throw new ArgumentException("Exponent out of bounds");
            }

            Int64 expBias = exp + ExponentBias;

            // expBias is now raised (biased) by 1023, meaning it will be between
            // 1 and 2046.
            Debug.Assert(expBias >= 1 && expBias <= 2046);

            // Move the biased value to the exponent bits of the IEEE double 
            // (bits 52 - 62)
            Int64 bits = expBias << 52;
            return BitConverter.Int64BitsToDouble(bits);
        }

        /// <summary>
        /// Computes the exponent of a double floating point 
        /// value.
        /// </summary>
        public static Int32 GetExponent(Double d)
        {
            DoubleBits db = new DoubleBits(d);
            return db.Exponent;
        }

        public static Double TruncateToPowerOfTwo(Double d)
        {
            DoubleBits db = new DoubleBits(d);
            db.ZeroLowerBits(52);
            return db.Double;
        }

        public static String ToBinaryString(Double d)
        {
            DoubleBits db = new DoubleBits(d);
            return db.ToString();
        }

        public static Double MaximumCommonSignificand(Double d1, Double d2)
        {
            if (d1 == 0.0 || d2 == 0.0)
            {
                return 0.0;
            }

            DoubleBits db1 = new DoubleBits(d1);
            DoubleBits db2 = new DoubleBits(d2);

            if (db1.Exponent != db2.Exponent)
            {
                return 0.0;
            }

            Int32 maxCommon = db1.GetCommonSignificandBitsCount(db2);
            db1.ZeroLowerBits(64 - (12 + maxCommon));
            return db1.Double;
        }

        private readonly Double _x;
        private readonly Int64 _xBits;

        public DoubleBits(Double x)
        {
            _x = x;
            _xBits = BitConverter.DoubleToInt64Bits(x);
        }

        private DoubleBits(Double x, Int64 xBits)
        {
            _x = x;
            _xBits = xBits;
        }

        public Double Double
        {
            get { return BitConverter.Int64BitsToDouble(_xBits); }
        }

        /// <summary>
        /// Gets the raw exponent value for the double-floating point value. 
        /// The raw exponent value is the exponent biased by 
        /// <see cref="ExponentBias"/> in order to remain positive.
        /// </summary>
        public Int32 BiasedExponent
        {
            get
            {
                Int32 signExp = (Int32)(_xBits >> 52);
                Int32 exp = signExp & 0x07ff;
                return exp;
            }
        }

        /// <summary>
        /// Gets the exponent for the double value.
        /// </summary>
        public Int32 Exponent
        {
            get { return BiasedExponent - ExponentBias; }
        }

        /// <summary>
        /// Creates a new <see cref="DoubleBits"/> value
        /// with the lower <paramref name="bitCount"/> bits
        /// set to 0.
        /// </summary>
        /// <param name="bitCount">The number of bits to set to 0.</param>
        /// <returns>
        /// A <see cref="DoubleBits"/> structure with the <paramref name="bitCount"/>
        /// lower bits set to 0.
        /// </returns>
        public DoubleBits ZeroLowerBits(Int32 bitCount)
        {
            Int64 invMask = (1L << bitCount) - 1L;
            Int64 mask = ~invMask;
            return new DoubleBits(_x, _xBits & mask);
        }

        /// <summary>
        /// Gets the bit at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the bit to retrieve.</param>
        /// <returns>The value of the bit at <paramref name="index"/>.</returns>
        public Int32 this[Int32 index]
        {
            get
            {
                Int64 mask = (1L << index);
                return (_xBits & mask) != 0 ? 1 : 0;
            }
        }

        /// <summary> 
        /// This computes the number of common most-significant bits in the significand.
        /// It does not count the hidden bit, which is always 1.
        /// It does not determine whether the numbers have the same exponent - if they do
        /// not, the value computed by this function is meaningless.
        /// </summary>
        /// <returns> The number of common most-significant significand bits.</returns>
        public Int32 GetCommonSignificandBitsCount(DoubleBits db)
        {
            for (Int32 i = 0; i < 52; i++)
            {
                if (this[i] != db[i])
                {
                    return i;
                }
            }

            return 52;
        }

        /// <summary>
        /// A representation of the Double bits formatted for easy readability.
        /// </summary>
        public override String ToString()
        {
            String numStr = HexConverter.ConvertAnyToAny(_xBits.ToString(), 10, 2);

            // 64 zeroes!
            String zero64 = new String('0', 64);
            String padStr = zero64 + numStr;
            String bitStr = padStr.Substring(padStr.Length - 64);
            String str = bitStr.Substring(0, 1) + "  "
                         + bitStr.Substring(1, 12) + "(" + Exponent + ") "
                         + bitStr.Substring(12)
                         + " [ " + _x + " ]";
            return str;
        }
    }
}