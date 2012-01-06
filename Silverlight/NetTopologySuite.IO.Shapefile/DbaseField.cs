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
	/// Represents a field in a DBase file.
	/// </summary>
    public class DbaseField : IEquatable<DbaseField>
	{
		private readonly String _columnName;
		private readonly Type _dataType;
		private readonly Int16 _length;
		private readonly Byte _decimals;
		private readonly DbaseHeader _header;
		private readonly Int32 _ordinal;
		private readonly Int32 _offset;

		internal DbaseField(DbaseHeader header, String name, Type type, Int16 length, Byte decimals, Int32 ordinal, Int32 offset)
		{
			_header = header;
			_columnName = name;
			_dataType = type;
			_length = length;
			_decimals = decimals;
			_ordinal = ordinal;
			_offset = offset;
		}

		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public String ColumnName
		{
			get { return _columnName; }
		}

		/// <summary>
		/// Gets the CLR equivalent type of the field.
		/// </summary>
		public Type DataType
		{
			get { return _dataType; }
		}

		/// <summary>
		/// Gets the length in bytes of the field in a record.
		/// </summary>
		public Int16 Length
		{
			get { return _length; }
		}

		/// <summary>
		/// Gets the number of decimals in a numeric field.
		/// </summary>
		public Byte Decimals
		{
			get { return _decimals; }
		}

		/// <summary>
		/// Gets the position of the column relative to all columns
		/// in the dBase schema.
		/// </summary>
		public Int32 Ordinal
		{
			get { return _ordinal; }
		}

		/// <summary>
		/// Gets the number of bytes from the start of the record
		/// where the field data starts.
		/// </summary>
		public Int32 Offset
		{
			get { return _offset; }
		}

		/// <summary>
		/// Converts the field instance into a String representation.
		/// </summary>
		/// <returns>A String which describes the field.</returns>
		public override String ToString()
		{
			return String.Format("[DbaseField] Name: {0}; Type: {1}; Length: {2}; " +
								 "Decimals: {3}", ColumnName, DataType, Length, Decimals);
		}

		public override Int32 GetHashCode()
		{
			return (ColumnName ?? "").ToLower().GetHashCode() ^
				   DataType.GetHashCode() ^
				   Length.GetHashCode() ^
				   Decimals.GetHashCode();
		}

		/// <summary>
		/// Compares two DbaseField instances to determine if they describe the
		/// same field.
		/// </summary>
		/// <param name="obj">The field to compare to.</param>
		/// <returns>True if the fields are equal, false otherwise.</returns>
		public override Boolean Equals(Object obj)
		{
			DbaseField other = obj as DbaseField;
			return Equals(other);
		}

		#region IEquatable<DbaseField> Members

		/// <summary>
		/// Compares two DbaseField instances to determine if they describe the
		/// same field.
		/// </summary>
        /// <param name="other">The field to compare to.</param>
		/// <returns>True if the fields are equal, false otherwise.</returns>
		public Boolean Equals(DbaseField other)
		{
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			CompareInfo compare = DbaseLocaleRegistry.GetCulture(_header.LanguageDriver).CompareInfo;

			return compare.Compare(other.ColumnName, ColumnName, CompareOptions.IgnoreCase) == 0 &&
				   other.DataType == DataType &&
				   other.Length == Length &&
				   other.Decimals == Decimals;
		}

		#endregion
	}
}