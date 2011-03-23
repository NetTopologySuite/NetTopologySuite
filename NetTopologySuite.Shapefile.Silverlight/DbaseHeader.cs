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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Data;

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    /// <summary>
    /// Represents the header of an xBase file.
    /// </summary>
    public class DbaseHeader
    {
        private DateTime _lastUpdate = DateTime.Now;
        private UInt32 _numberOfRecords;
        private Int16 _headerLength;
        private Int16 _recordLength;
        private readonly Byte _languageDriver;
        private readonly Dictionary<String, DbaseField> _dbaseColumns
            = new Dictionary<String, DbaseField>();
        private List<DbaseField> _columnList;

        internal DbaseHeader(Byte languageDriverCode, DateTime lastUpdate, UInt32 numberOfRecords)
        {
            _languageDriver = languageDriverCode;
            _lastUpdate = lastUpdate;
            _numberOfRecords = numberOfRecords;
        }

        /// <summary>
        /// Gets a value which indicates which code page text data is 
        /// stored in.
        /// </summary>
        public Byte LanguageDriver
        {
            get { return _languageDriver; }
        }

        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set { _lastUpdate = value; }
        }

        public UInt32 RecordCount
        {
            get { return _numberOfRecords; }
            set { _numberOfRecords = value; }
        }

        public IList<DbaseField> Columns
        {
            get
            {
                if (_columnList == null)
                {
                    _columnList = new List<DbaseField>(_dbaseColumns.Values);
                }

                return _columnList;
            }
            set
            {
                _columnList = null;
                _dbaseColumns.Clear();

                RecordLength = 1; // Deleted flag length

                foreach (DbaseField field in value)
                {
                    _dbaseColumns.Add(field.ColumnName, field);
                    RecordLength += field.Length;
                }

                HeaderLength = (Int16)((DbaseConstants.ColumnDescriptionLength * _dbaseColumns.Count) +
                    DbaseConstants.ColumnDescriptionOffset + 1 /* For terminator */);
            }
        }

        public Int16 HeaderLength
        {
            get { return _headerLength; }
            private set { _headerLength = value; }
        }

        public Int16 RecordLength
        {
            get { return _recordLength; }
            private set { _recordLength = value; }
        }

        public Encoding FileEncoding
        {
            get { return DbaseLocaleRegistry.GetEncoding(LanguageDriver); }
        }

        public override String ToString()
        {
            return String.Format("[DbaseHeader] Records: {0}; Columns: {1}; Last Update: {2}; " +
                "Record Length: {3}; Header Length: {4}", RecordCount, Columns.Count,
                LastUpdate, RecordLength, HeaderLength);
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the DBase file.
        /// </summary>
        /// <returns>A DataTable that describes the column metadata.</returns>
        internal ISchema GetSchemaTable(ISchemaFactory schemaFactory)
        {
            List<IPropertyInfo> propertyInfos = new List<IPropertyInfo>(_dbaseColumns.Count);
            propertyInfos.Add(schemaFactory.PropertyFactory.Create<uint>("OID"));
            propertyInfos.Add(schemaFactory.PropertyFactory.Create<IGeometry>("Geom"));
            foreach (KeyValuePair<String, DbaseField> entry in _dbaseColumns)
            {
                DbaseField field = entry.Value;
                IPropertyInfo propertyInfo =
                    schemaFactory.PropertyFactory.Create(field.DataType, field.ColumnName);
                propertyInfos.Add(propertyInfo);
                if (field.DataType == typeof(string))
                    ((IStringPropertyInfo)propertyInfo).MaxLength = field.Length;
                if (field.DataType == typeof(decimal))
                    ((IDecimalPropertyInfo)propertyInfo).Precision = field.Decimals;

            }

            return schemaFactory.Create(propertyInfos, propertyInfos.First(a => a.Name == "OID"));
        }

        internal static DbaseHeader ParseDbfHeader(Stream dataStream)
        {
            DbaseHeader header;

            using (BinaryReader reader = new BinaryReader(dataStream))
            {
                if (reader.ReadByte() != DbaseConstants.DbfVersionCode)
                {
                    throw new NotSupportedException("Unsupported DBF Type");
                }

                DateTime lastUpdate = new DateTime(reader.ReadByte() + DbaseConstants.DbaseEpoch,
                    reader.ReadByte(), reader.ReadByte()); //Read the last update date
                UInt32 recordCount = reader.ReadUInt32(); // read number of records.
                Int16 storedHeaderLength = reader.ReadInt16(); // read length of header structure.
                Int16 storedRecordLength = reader.ReadInt16(); // read length of a record
                reader.BaseStream.Seek(DbaseConstants.EncodingOffset, SeekOrigin.Begin); //Seek to encoding flag
                Byte languageDriver = reader.ReadByte(); //Read and parse Language driver
                reader.BaseStream.Seek(DbaseConstants.ColumnDescriptionOffset, SeekOrigin.Begin); //Move past the reserved bytes
                Int32 numberOfColumns = (storedHeaderLength - DbaseConstants.ColumnDescriptionOffset) /
                    DbaseConstants.ColumnDescriptionLength;  // calculate the number of DataColumns in the header

                header = new DbaseHeader(languageDriver, lastUpdate, recordCount);

                DbaseField[] columns = new DbaseField[numberOfColumns];
                Int32 offset = 1;

                for (Int32 i = 0; i < columns.Length; i++)
                {
                    String colName = header.FileEncoding.GetString(
                        reader.ReadBytes(11), 0, 11).Replace("\0", "").Trim();

                    Char fieldtype = reader.ReadChar();

                    // Unused address data...
                    reader.ReadInt32();

                    Int16 fieldLength = reader.ReadByte();
                    Byte decimals = 0;

                    if (fieldtype == 'N' || fieldtype == 'F')
                    {
                        decimals = reader.ReadByte();
                    }
                    else
                    {
                        fieldLength += (Int16)(reader.ReadByte() << 8);
                    }

                    Type dataType = mapFieldTypeToClrType(fieldtype, decimals, fieldLength);

                    columns[i] = new DbaseField(header, colName, dataType, fieldLength, decimals, i, offset);

                    offset += fieldLength;

                    // Move stream to next field record
                    reader.BaseStream.Seek(DbaseConstants.BytesFromEndOfDecimalInFieldRecord, SeekOrigin.Current);
                }

                header.Columns = columns;

                if (storedHeaderLength != header.HeaderLength)
                {
                    throw new InvalidDbaseFileException(
                        "Recorded header length doesn't equal computed header length.");
                }

                if (storedRecordLength != header.RecordLength)
                {
                    throw new InvalidDbaseFileException(
                        "Recorded record length doesn't equal computed record length.");
                }
            }

            return header;
        }

        private static Type mapFieldTypeToClrType(Char fieldtype, Byte decimals, Int16 length)
        {
            Type dataType;

            switch (fieldtype)
            {
                case 'L':
                    return typeof(Boolean);
                    break;
                case 'C':
                    return typeof(String);
                    break;
                case 'D':
                    return typeof(DateTime);
                    break;
                case 'N':
                    // If the number doesn't have any decimals, 
                    // make the type an integer, if possible
                    if (decimals == 0)
                    {
                        if (length <= 4) return typeof(Int16);
                        else if (length <= 9) return typeof(Int32);
                        else if (length <= 18) return typeof(Int64);
                        else return typeof(Double);
                    }
                    else
                    {
                        return typeof(Double);
                    }
                case 'F':
                    return typeof(Single);
                case 'B':
                    return typeof(Byte[]);
                default:
                    return null;
            }
        }
    }
}
