using System;
using GisSharpBlog.NetTopologySuite.Utilities;
using BitConverter = System.BitConverter;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// DoubleBits manipulates Double numbers
    /// by using bit manipulation and bit-field extraction.
    /// For some operations (such as determining the exponent)
    /// this is more accurate than using mathematical operations
    /// (which suffer from round-off error).
    /// The algorithms and constants in this class
    /// apply only to IEEE-754 Double-precision floating point format.
    /// </summary>
    public struct DoubleBits
    {
        public const Int32 ExponentBias = 1023;

        public static Double PowerOf2(Int32 exp)
        {
            if (exp > 1023 || exp < -1022)
            {
                throw new ArgumentException("Exponent out of bounds");
            }

            Int64 expBias = exp + ExponentBias;
            Int64 bits = expBias << 52;
            return BitConverter.Int64BitsToDouble(bits);
        }

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

            Int32 maxCommon = db1.CommonSignificandBitsCount(db2);
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
        /// Determines the exponent for the number.
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
        /// Determines the exponent for the number.
        /// </summary>
        public Int32 Exponent
        {
            get { return BiasedExponent - ExponentBias; }
        }

        public DoubleBits ZeroLowerBits(Int32 nBits)
        {
            Int64 invMask = (1L << nBits) - 1L;
            Int64 mask = ~invMask;
            return new DoubleBits(_x, _xBits & mask);
        }

        public Int32 GetBit(Int32 i)
        {
            Int64 mask = (1L << i);
            return (_xBits & mask) != 0 ? 1 : 0;
        }

        /// <summary> 
        /// This computes the number of common most-significant bits in the significand.
        /// It does not count the hidden bit, which is always 1.
        /// It does not determine whether the numbers have the same exponent - if they do
        /// not, the value computed by this function is meaningless.
        /// </summary>
        /// <returns> The number of common most-significant significand bits.</returns>
        public Int32 CommonSignificandBitsCount(DoubleBits db)
        {
            for (Int32 i = 0; i < 52; i++)
            {
                if (GetBit(i) != db.GetBit(i))
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
            String zero64 = "0000000000000000000000000000000000000000000000000000000000000000";
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