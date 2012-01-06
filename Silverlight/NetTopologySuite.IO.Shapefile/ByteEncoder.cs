// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;

namespace GeoAPI.DataStructures
{
    public static class ByteEncoder
    {
        #region Endian conversion helper routines
        /// <summary>
        /// Returns the value encoded in Big Endian (PPC, XDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Big-endian encoded value.</returns>
        public static Int32 GetBigEndian(Int32 value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return swapByteOrder(value);
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Returns the value encoded in Big Endian (PPC, XDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Big-endian encoded value.</returns>
        [CLSCompliant(false)]
        public static UInt16 GetBigEndian(UInt16 value)
        {
            return BitConverter.IsLittleEndian
                       ? swapByteOrder(value)
                       : value;
        }

        /// <summary>
        /// Returns the value encoded in Big Endian (PPC, XDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Big-endian encoded value.</returns>
        [CLSCompliant(false)]
        public static UInt32 GetBigEndian(UInt32 value)
        {
            return BitConverter.IsLittleEndian
                       ? swapByteOrder(value)
                       : value;
        }

        /// <summary>
        /// Returns the value encoded in Big Endian (PPC, XDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Big-endian encoded value.</returns>
        public static Double GetBigEndian(Double value)
        {
            return BitConverter.IsLittleEndian
                       ? swapByteOrder(value)
                       : value;
        }

        /// <summary>
        /// Returns the value encoded in Little Endian (x86, NDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Little-endian encoded value.</returns>
        public static Int32 GetLittleEndian(Int32 value)
        {
            return BitConverter.IsLittleEndian
                       ? value
                       : swapByteOrder(value);
        }

        /// <summary>
        /// Returns the value encoded in Little Endian (x86, NDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Little-endian encoded value.</returns>
        [CLSCompliant(false)]
        public static UInt32 GetLittleEndian(UInt32 value)
        {
            return BitConverter.IsLittleEndian
                       ? value
                       : swapByteOrder(value);
        }

        /// <summary>
        /// Returns the value encoded in Little Endian (x86, NDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Little-endian encoded value.</returns>
        [CLSCompliant(false)]
        public static UInt16 GetLittleEndian(UInt16 value)
        {
            return BitConverter.IsLittleEndian
                       ? value
                       : swapByteOrder(value);
        }

        /// <summary>
        /// Returns the value encoded in Little Endian (x86, NDR) format.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>Little-endian encoded value.</returns>
        public static Double GetLittleEndian(Double value)
        {
            return BitConverter.IsLittleEndian
                       ? value
                       : swapByteOrder(value);
        }

        /// <summary>
        /// Swaps the Byte order of an <see cref="Int32"/>.
        /// </summary>
        /// <param name="value"><see cref="Int32"/> to swap the bytes of.</param>
        /// <returns>Byte order swapped <see cref="Int32"/>.</returns>
        private static Int32 swapByteOrder(Int32 value)
        {
            Int32 swapped = (Int32)((0x000000FF) & (value >> 24)
                                     | (0x0000FF00) & (value >> 8)
                                     | (0x00FF0000) & (value << 8)
                                     | (0xFF000000) & (value << 24));
            return swapped;
        }

        /// <summary>
        /// Swaps the byte order of a <see cref="UInt16"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt16"/> to swap the bytes of.</param>
        /// <returns>Byte order swapped <see cref="UInt16"/>.</returns>
        private static UInt16 swapByteOrder(UInt16 value)
        {
            return (UInt16)((0x00FF & (value >> 8))
                             | (0xFF00 & (value << 8)));
        }

        /// <summary>
        /// Swaps the byte order of a <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt32"/> to swap the bytes of.</param>
        /// <returns>Byte order swapped <see cref="UInt32"/>.</returns>
        private static UInt32 swapByteOrder(UInt32 value)
        {
            UInt32 swapped = ((0x000000FF) & (value >> 24)
                             | (0x0000FF00) & (value >> 8)
                             | (0x00FF0000) & (value << 8)
                             | (0xFF000000) & (value << 24));
            return swapped;
        }

        /// <summary>
        /// Swaps the byte order of a <see cref="Int64"/>.
        /// </summary>
        /// <param name="value"><see cref="Int64"/> to swap the bytes of.</param>
        /// <returns>Byte order swapped <see cref="Int64"/>.</returns>
        private static Int64 swapByteOrder(Int64 value)
        {
            UInt64 uvalue = (UInt64)value;
            UInt64 swapped = ((0x00000000000000FF) & (uvalue >> 56)
                             | (0x000000000000FF00) & (uvalue >> 40)
                             | (0x0000000000FF0000) & (uvalue >> 24)
                             | (0x00000000FF000000) & (uvalue >> 8)
                             | (0x000000FF00000000) & (uvalue << 8)
                             | (0x0000FF0000000000) & (uvalue << 24)
                             | (0x00FF000000000000) & (uvalue << 40)
                             | (0xFF00000000000000) & (uvalue << 56));
            return (Int64)swapped;
        }

        /// <summary>
        /// Swaps the byte order of a <see cref="Double"/> (double precision IEEE 754)
        /// </summary>
        /// <param name="value"><see cref="Double"/> to swap.</param>
        /// <returns>Byte order swapped <see cref="Double"/> value.</returns>
        private static Double swapByteOrder(Double value)
        {
            Int64 bits = BitConverter.ToInt64(BitConverter.GetBytes(value), 0);

            //Int64 bits = BitConverter.DoubleToInt64Bits(value);
            bits = swapByteOrder(bits);
            //return BitConverter.Int64BitsToDouble(bits);
            return BitConverter.ToDouble(BitConverter.GetBytes(bits), 0);

        }
        #endregion
    }
}
