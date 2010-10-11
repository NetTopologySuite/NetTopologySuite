using System;
//using NetTopologySuite.Utilities;
using GeoAPI.DataStructures;
using BitConverter = System.BitConverter;

namespace NetTopologySuite.Precision
{
    /// <summary> 
    /// Determines the maximum number of common most-significant
    /// bits in the significand of one or numbers.
    /// Can be used to compute the Double-precision number which
    /// is represented by the common bits.
    /// If there are no common bits, the number computed is 0.0.
    /// </summary>
    public class CommonBits
    {
        /// <summary>
        /// Computes the bit pattern for the sign and exponent of a
        /// double-precision number.
        /// </summary>
        /// <returns>The bit pattern for the sign and exponent.</returns>
        public static Int64 SignExpBits(Int64 num)
        {
            return num >> 52;
        }

        /// <summary>
        /// This computes the number of common most-significant bits in the significand
        /// of two double-precision numbers.
        /// It does not count the hidden bit, which is always 1.
        /// It does not determine whether the numbers have the same exponent - if they do
        /// not, the value computed by this function is meaningless.
        /// </summary>
        /// <returns>The number of common most-significant significand bits.</returns>
        public static Int32 CommonMostSignificantSignificandBitsCount(Int64 num1, Int64 num2)
        {
            Int32 count = 0;

            for (Int32 i = 52; i >= 0; i--)
            {
                if (GetBit(num1, i) != GetBit(num2, i))
                {
                    return count;
                }

                count++;
            }

            return 52;
        }

        /// <summary>
        /// Zeroes the lower n bits of a bitstring.
        /// </summary>
        /// <param name="bits">The bitstring to alter.</param>
        /// <param name="bitCount">the number of bits to zero.</param>
        /// <returns>The zeroed bitstring.</returns>
        public static Int64 ZeroLowerBits(Int64 bits, Int32 bitCount)
        {
            Int64 mask = ~((1L << bitCount) - 1L);
            return bits & mask;
        }

        /// <summary>
        /// Extracts the i'th bit of a bitstring.
        /// </summary>
        /// <param name="bits">The bitstring to extract from.</param>
        /// <param name="i">The bit to extract.</param>
        /// <returns>The value of the extracted bit.</returns>
        public static Int32 GetBit(Int64 bits, Int32 i)
        {
            Int64 mask = (1L << i);
            return (bits & mask) != 0 ? 1 : 0;
        }

        private Boolean _isFirst = true;
        private Int32 _commonSignificandBitsCount = 53;
        private Int64 _commonBits;
        private Int64 _commonSignExp;

        ///<summary>
        /// Adds a <see langword="Double"/>.
        ///</summary>
        ///<param name="num">number to add</param>
        public void Add(Double num)
        {
            Int64 numBits = BitConverter.DoubleToInt64Bits(num);

            if (_isFirst)
            {
                _commonBits = numBits;
                _commonSignExp = SignExpBits(_commonBits);
                _isFirst = false;
                return;
            }

            Int64 numSignExp = SignExpBits(numBits);

            if (numSignExp != _commonSignExp)
            {
                _commonBits = 0;
                return;
            }

            _commonSignificandBitsCount = CommonMostSignificantSignificandBitsCount(_commonBits, numBits);
            _commonBits = ZeroLowerBits(_commonBits, 64 - (12 + _commonSignificandBitsCount));
        }

        /// <summary>
        /// Common as <see langword="Double"/>.
        /// </summary>
        public Double Common
        {
            get { return BitConverter.Int64BitsToDouble(_commonBits); }
        }

        /// <summary>
        /// A representation of the Double bits formatted for easy readability.
        /// </summary>
        public string ToString(Int64 bits)
        {
            Double x = BitConverter.Int64BitsToDouble(bits);
            string numStr = NumberBaseConverter.ConvertAnyToAny(bits.ToString(), 10, 2);
            string padStr = "0000000000000000000000000000000000000000000000000000000000000000" + numStr;
            string bitStr = padStr.Substring(padStr.Length - 64);
            string str = bitStr.Substring(0, 1) + "  " + bitStr.Substring(1, 12) + "(exp) "
                         + bitStr.Substring(12) + " [ " + x + " ]";
            return str;
        }
    }
}
