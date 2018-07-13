using NetTopologySuite.Utilities;

using BitConverter = System.BitConverter;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Determines the maximum number of common most-significant
    /// bits in the mantissa of one or numbers.
    /// Can be used to compute the double-precision number which
    /// is represented by the common bits.
    /// If there are no common bits, the number computed is 0.0.
    /// </summary>
    public class CommonBits
    {
        /// <summary>
        /// Computes the bit pattern for the sign and exponent of a
        /// double-precision number.
        /// </summary>
        /// <param name="num"></param>
        /// <returns>The bit pattern for the sign and exponent.</returns>
        public static long SignExpBits(long num)
        {
            return num >> 52;
        }

        /// <summary>
        /// This computes the number of common most-significant bits in the mantissas
        /// of two double-precision numbers.
        /// It does not count the hidden bit, which is always 1.
        /// It does not determine whether the numbers have the same exponent - if they do
        /// not, the value computed by this function is meaningless.
        /// </summary>
        /// <param name="num1"></param>
        /// /// <param name="num2"></param>
        /// <returns>The number of common most-significant mantissa bits.</returns>
        public static int NumCommonMostSigMantissaBits(long num1, long num2)
        {
            int count = 0;
            for (int i = 52; i >= 0; i--)
            {
                if (GetBit(num1, i) != GetBit(num2, i))
                    return count;
                count++;
            }
            return 52;
        }

        /// <summary>
        /// Zeroes the lower n bits of a bitstring.
        /// </summary>
        /// <param name="bits">The bitstring to alter.</param>
        /// <param name="nBits">the number of bits to zero.</param>
        /// <returns>The zeroed bitstring.</returns>
        public static long ZeroLowerBits(long bits, int nBits)
        {
            long invMask = (1L << nBits) - 1L;
            long mask = ~invMask;
            long zeroed = bits & mask;
            return zeroed;
        }

        /// <summary>
        /// Extracts the i'th bit of a bitstring.
        /// </summary>
        /// <param name="bits">The bitstring to extract from.</param>
        /// <param name="i">The bit to extract.</param>
        /// <returns>The value of the extracted bit.</returns>
        public static int GetBit(long bits, int i)
        {
            long mask = (1L << i);
            return (bits & mask) != 0 ? 1 : 0;
        }

        private bool _isFirst = true;
        private int _commonMantissaBitsCount = 53;
        private long _commonBits;
        private long _commonSignExp;

        /*
        /// <summary>
        ///
        /// </summary>
        public CommonBits() { }
        */

        /// <summary>
        ///
        /// </summary>
        /// <param name="num"></param>
        public void Add(double num)
        {
            long numBits = BitConverter.DoubleToInt64Bits(num);
            if (_isFirst)
            {
                _commonBits = numBits;
                _commonSignExp = SignExpBits(_commonBits);
                _isFirst = false;
                return;
            }

            long numSignExp = SignExpBits(numBits);
            if (numSignExp != _commonSignExp)
            {
                _commonBits = 0;
                return;
            }
            _commonMantissaBitsCount = NumCommonMostSigMantissaBits(_commonBits, numBits);
            _commonBits = ZeroLowerBits(_commonBits, 64 - (12 + _commonMantissaBitsCount));
        }

        /// <summary>
        ///
        /// </summary>
        public double Common => BitConverter.Int64BitsToDouble(_commonBits);

        /// <summary>
        /// A representation of the Double bits formatted for easy readability
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public string ToString(long bits)
        {
            double x = BitConverter.Int64BitsToDouble(bits);
            string numStr = HexConverter.ConvertAny2Any(bits.ToString(), 10, 2);
            string padStr = "0000000000000000000000000000000000000000000000000000000000000000" + numStr;
            string bitStr = padStr.Substring(padStr.Length - 64);
            string str = bitStr.Substring(0, 1) + "  " + bitStr.Substring(1, 12) + "(exp) "
                         + bitStr.Substring(12) + " [ " + x + " ]";
            return str;
        }
    }
}