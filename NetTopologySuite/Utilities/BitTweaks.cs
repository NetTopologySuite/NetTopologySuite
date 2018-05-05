using System;
using System.Diagnostics;

namespace NetTopologySuite.Utilities
{
    internal static class BitTweaks
    {
        internal static short ReverseByteOrder(short value)
        {
            unchecked
            {
                return (short)ReverseByteOrder((ushort)value);
            }
        }

        internal static int ReverseByteOrder(int value)
        {
            unchecked
            {
                return (int)ReverseByteOrder((uint)value);
            }
        }

        internal static long ReverseByteOrder(long value)
        {
            unchecked
            {
                return (long)ReverseByteOrder((ulong)value);
            }
        }

        internal static float ReverseByteOrder(float value)
        {
            // TODO: BitConverter.SingleToInt32Bits will exist eventually
            // see https://github.com/dotnet/coreclr/pull/833
            byte[] bytes = System.BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);

            Array.Reverse(bytes, 0, 4);
            return System.BitConverter.ToSingle(bytes, 0);
        }

        internal static double ReverseByteOrder(double value)
        {
            return System.BitConverter.Int64BitsToDouble(ReverseByteOrder(System.BitConverter.DoubleToInt64Bits(value)));
        }

        internal static ushort ReverseByteOrder(ushort value)
        {
            unchecked
            {
                return (ushort)((value & 0x00FF) << 8 |
                                (value & 0xFF00) >> 8);
            }
        }

        internal static uint ReverseByteOrder(uint value)
        {
            return (value & 0x000000FF) << 24 |
                   (value & 0x0000FF00) << 8 |
                   (value & 0x00FF0000) >> 8 |
                   (value & 0xFF000000) >> 24;
        }

        internal static ulong ReverseByteOrder(ulong value)
        {
            return (value & 0x00000000000000FF) << 56 |
                   (value & 0x000000000000FF00) << 40 |
                   (value & 0x0000000000FF0000) << 24 |
                   (value & 0x00000000FF000000) << 8 |
                   (value & 0x000000FF00000000) >> 8 |
                   (value & 0x0000FF0000000000) >> 24 |
                   (value & 0x00FF000000000000) >> 40 |
                   (value & 0xFF00000000000000) >> 56;
        }
    }
}
