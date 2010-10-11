// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

namespace NetTopologySuite.IO.WellKnownBinary
{
	/// <summary>
	/// Specifies the specific binary encoding 
	/// (NDR or XDR, Little Endian or Big Endian) 
	/// used for a geometry byte stream
	/// </summary>
	public enum WkbByteOrder : byte
	{
		/// <summary>
		/// XDR (Big Endian) encoding of numeric types.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The XDR representation of an Unsigned Integer is 
        /// Big Endian (most significant byte first).
		/// </para>
		/// <para>
		/// The XDR representation of a Double is 
        /// Big Endian (sign bit is first byte).
		/// </para>
		/// </remarks>
		Xdr = 0,

        /// <summary>
        /// Big Endian (XDR) encoding of numeric types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For an Unsigned Integer, the most significant byte comes first.
        /// </para>
        /// <para>
        /// For a Double, the sign bit comes first.
        /// </para>
        /// </remarks>
        BigEndian = Xdr,

		/// <summary>
		/// NDR (Little Endian) encoding of numeric types.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The NDR representation of an Unsigned Integer is 
		/// Little Endian (least significant byte first).
		/// </para>
		/// <para>
		/// The NDR representation of a Double is 
		/// Little Endian (sign bit is in the last byte).
		/// </para>
		/// </remarks>
		Ndr = 1,

        /// <summary>
        /// Little Endian (NDR) encoding of numeric types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For an Unsigned Integer, the least significant byte comes first.
        /// </para>
        /// <para>
        /// For a Double, the sign bit is in the last byte.
        /// </para>
        /// </remarks>
        LittleEndian = Ndr
	}
}
