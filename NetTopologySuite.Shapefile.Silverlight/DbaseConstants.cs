// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Globalization;

namespace NetTopologySuite.Shapefile
{
	/// <summary>
	/// Constants used to parse dBase files.
	/// </summary>
	internal class DbaseConstants
	{
		/// <summary>
		/// Version of the dBase file specification Shapefiles use: 3.
		/// </summary>
		internal static readonly Byte DbfVersionCode = 0x03;

		/// <summary>
		/// Offset of the language driver code in the dBase header.
		/// </summary>
		internal static readonly Int32 EncodingOffset = 29;

		/// <summary>
		/// Offset of the start of the column description section.
		/// </summary>
		internal static readonly Int32 ColumnDescriptionOffset = 32;

		/// <summary>
		/// Length of a column description entry.
		/// </summary>
		internal static readonly Int32 ColumnDescriptionLength = 32;

		/// <summary>
		/// Number of bytes to the end of a column entry from the end of the decimal Byte.
		/// </summary>
		internal static readonly Int32 BytesFromEndOfDecimalInFieldRecord = 14;

		/// <summary>
		/// Character used to specify a null value for numbers.
		/// </summary>
		internal static readonly Char NumericNullIndicator = '*';

		/// <summary>
		/// Character used to specify a deleted row.
		/// </summary>
		internal static readonly Char DeletedIndicator = '*';

		/// <summary>
		/// Character used to specify a non-deleted row.
		/// </summary>
		internal static readonly Char NotDeletedIndicator = ' ';

		/// <summary>
		/// Character used to specify a null value for boolean values.
		/// </summary>
		internal static readonly Char BooleanNullChar = '?';

		/// <summary>
		/// Character used to specify a null value for date-time values.
		/// </summary>
		internal static readonly String NullDateValue = new String('0', 8);

		/// <summary>
		/// Character used to indicate the end of the dBase header in the file.
		/// </summary>
		internal static readonly Byte HeaderTerminator = 0x0d;

		/// <summary>
		/// Character used to indicate the end of the dBase file.
		/// </summary>
		internal static readonly Byte FileTerminator = 0x1a;

		/// <summary>
		/// The last updated value in the header is measured from this year.
		/// </summary>
		internal static readonly Int32 DbaseEpoch = 1900;

		/// <summary>
		/// Size of the column name field in a column description entry.
		/// </summary>
		internal static readonly Int32 FieldNameLength = 11;

		/// <summary>
		/// The format used to store numbers in a dBase file.
		/// </summary>
		internal static readonly NumberFormatInfo StorageNumberFormat
			= NumberFormatInfo.InvariantInfo;
	}
}