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
using System.IO;
using System.Text;
using NetTopologySuite.Encodings;

namespace NetTopologySuite.Shapefile
{
    internal partial class DbaseFile
    {
        #region Nested type: DbaseWriter

        private class DbaseWriter : IDisposable
        {
            private const String NumberFormatTemplate = "{0,:F}";
            private readonly BinaryReader _binaryReader;
            private readonly BinaryWriter _binaryWriter;
            private readonly DbaseFile _dbaseFile;
            private readonly StringBuilder _format = new StringBuilder(NumberFormatTemplate, 32);

            #region Object Construction/Destruction

            public DbaseWriter(DbaseFile file)
            {
                _dbaseFile = file;
                _binaryWriter = new BinaryWriter(file.DataStream, file.Encoding);
                _binaryReader = new BinaryReader(file.DataStream, file.Encoding);
            }

            ~DbaseWriter()
            {
                dispose(false);
            }

            #region Dispose Pattern

            internal Boolean IsDisposed { get; private set; }

            public void Dispose()
            {
                if (IsDisposed)
                {
                    return;
                }

                dispose(true);
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }

            private void dispose(Boolean disposing)
            {
                if (disposing) // Deterministically dispose managed resources
                {
                    if (_binaryWriter != null)
                    {
                        _binaryWriter.Close();
                    }

                    if (_binaryReader != null)
                    {
                        _binaryReader.Close();
                    }
                }
            }

            #endregion

            #endregion

            internal void BeginWrite()
            {
            }

            internal void EndWrite()
            {
                _binaryWriter.Flush();
                _binaryWriter.Seek(0, SeekOrigin.End);
                _binaryWriter.Write(DbaseConstants.FileTerminator);
            }

            internal void UpdateHeader(DbaseHeader header)
            {
                _binaryWriter.Seek(1, SeekOrigin.Begin);
                Byte[] dateBytes = new Byte[3]
                                       {
                                           (Byte) (header.LastUpdate.Year - DbaseConstants.DbaseEpoch),
                                           (Byte) header.LastUpdate.Month,
                                           (Byte) header.LastUpdate.Day
                                       };
                _binaryWriter.Write(dateBytes);
                _binaryWriter.Write(header.RecordCount);
            }

            internal void WriteFullHeader(DbaseHeader header)
            {
                _binaryWriter.Seek(0, SeekOrigin.Begin);
                _binaryWriter.Write(DbaseConstants.DbfVersionCode);
                UpdateHeader(header);
                _binaryWriter.Write(header.HeaderLength);
                _binaryWriter.Write(header.RecordLength);
                _binaryWriter.Write(new Byte[DbaseConstants.EncodingOffset - (Int32)_binaryWriter.BaseStream.Position]);
                _binaryWriter.Write(header.LanguageDriver);
                _binaryWriter.Write(new Byte[2]);

                foreach (DbaseField field in header.Columns)
                {
                    String colName = field.ColumnName + new String('\0', DbaseConstants.FieldNameLength);
                    Byte[] colNameBytes = EncodingEx.GetASCII().GetBytes(colName.Substring(0, DbaseConstants.FieldNameLength));
                    _binaryWriter.Write(colNameBytes);
                    Char fieldTypeCode = DbaseSchema.GetFieldTypeCode(field.DataType);
                    _binaryWriter.Write(fieldTypeCode);
                    _binaryWriter.Write(0); // Address field isn't supported

                    if (fieldTypeCode == 'N' || fieldTypeCode == 'F')
                    {
                        _binaryWriter.Write((Byte)field.Length);
                        _binaryWriter.Write(field.Decimals);
                    }
                    else
                    {
                        _binaryWriter.Write((Byte)field.Length);
                        _binaryWriter.Write((Byte)(field.Length >> 8));
                    }

                    _binaryWriter.Write(new Byte[14]);
                }

                _binaryWriter.Write(DbaseConstants.HeaderTerminator);
            }

            //internal void WriteRow(DataRow row)
            //{
            //    if (row == null)
            //    {
            //        throw new ArgumentNullException("row");
            //    }

            //    if (row.Table == null)
            //    {
            //        throw new ArgumentException("Row must be associated to a table.");
            //    }

            //    _binaryWriter.Write(DbaseConstants.NotDeletedIndicator);

            //    DbaseHeader header = _dbaseFile.Header;

            //    foreach (DbaseField column in header.Columns)
            //    {
            //        if (!row.Table.Columns.Contains(column.ColumnName)
            //            || String.Compare(column.ColumnName, DbaseSchema.OidColumnName,
            //                              StringComparison.CurrentCultureIgnoreCase) == 0)
            //        {
            //            continue;
            //        }

            //        // TODO: reconsider type checking
            //        //if ((header.Columns[rowColumnIndex].DataType != row.Table.Columns[rowColumnIndex].DataType) || (header.Columns[rowColumnIndex].Length != row.Table.Columns[rowColumnIndex].MaxLength))
            //        //    throw new SchemaMismatchException(String.Format("Row doesn't match this DbaseWriter schema at column {0}", i));

            //        switch (Type.GetTypeCode(column.DataType))
            //        {
            //            case TypeCode.Boolean:
            //                if (row[column.ColumnName] == null || row[column.ColumnName] == DBNull.Value)
            //                {
            //                    _binaryWriter.Write(DbaseConstants.BooleanNullChar);
            //                }
            //                else
            //                {
            //                    writeBoolean((Boolean)row[column.ColumnName]);
            //                }
            //                break;
            //            case TypeCode.DateTime:
            //                if (row[column.ColumnName] == null || row[column.ColumnName] == DBNull.Value)
            //                {
            //                    writeNullDateTime();
            //                }
            //                else
            //                {
            //                    writeDateTime((DateTime)row[column.ColumnName]);
            //                }
            //                break;
            //            case TypeCode.Single:
            //            case TypeCode.Double:
            //                if (row[column.ColumnName] == null || row[column.ColumnName] == DBNull.Value)
            //                {
            //                    writeNullNumber(column.Length);
            //                }
            //                else
            //                {
            //                    writeNumber(Convert.ToDouble(row[column.ColumnName]),
            //                                column.Length, column.Decimals);
            //                }
            //                break;
            //            case TypeCode.Int16:
            //            case TypeCode.Int32:
            //            case TypeCode.Int64:
            //                if (row[column.ColumnName] == null || row[column.ColumnName] == DBNull.Value)
            //                {
            //                    writeNullNumber(column.Length);
            //                }
            //                else
            //                {
            //                    writeNumber(Convert.ToInt64(row[column.ColumnName]), column.Length);
            //                }
            //                break;
            //            case TypeCode.String:
            //                if (row[column.ColumnName] == null || row[column.ColumnName] == DBNull.Value)
            //                {
            //                    writeNullString(column.Length);
            //                }
            //                else
            //                {
            //                    object value = row[column.ColumnName]; //jd: can be a Guid 
            //                    if (value is string && String.IsNullOrEmpty((string)value))
            //                        writeNullString(column.Length);
            //                    else
            //                        writeString(value is string ? (string)value : value.ToString(), column.Length);
            //                }
            //                break;
            //            case TypeCode.Char:
            //            case TypeCode.SByte:
            //            case TypeCode.Byte:
            //            case TypeCode.Decimal:
            //            case TypeCode.UInt16:
            //            case TypeCode.UInt32:
            //            case TypeCode.UInt64:
            //            case TypeCode.DBNull:
            //            case TypeCode.Empty:
            //            case TypeCode.Object:
            //            default:
            //                throw new NotSupportedException(String.Format(
            //                                                    "Type not supported: {0}", column.DataType));
            //        }
            //    }
            //}

            #region Private helper methods

            private void writeNullDateTime()
            {
                _binaryWriter.Write(DbaseConstants.NullDateValue);
            }

            private void writeNullString(Int32 length)
            {
                Byte[] bytes = EncodingEx.GetASCII().GetBytes(new String('\0', length));
                _binaryWriter.Write(bytes);
            }

            private void writeNullNumber(Int32 length)
            {
                _binaryWriter.Write(new String(DbaseConstants.NumericNullIndicator, length));
            }

            private void writeNumber(Double value, Int16 length, Byte decimalPlaces)
            {
                // Create number format String
                _format.Length = 0;
                _format.Append(NumberFormatTemplate);
                //_format.Insert(5, decimalPlaces).Insert(3, length);
                throw new NotImplementedException("fix line above");
                String number = String.Format(DbaseConstants.StorageNumberFormat, _format.ToString(), value);
                Byte[] bytes = EncodingEx.GetASCII().GetBytes(number);
                _binaryWriter.Write(bytes);
            }

            private void writeNumber(Int64 value, Int16 length)
            {
                // Create number format String
                writeNumber(value, length, 0);
            }

            private void writeDateTime(DateTime dateTime)
            {
                Byte[] bytes = EncodingEx.GetASCII().GetBytes(dateTime.ToString("yyyyMMdd"));
                _binaryWriter.Write(bytes);
            }

            private void writeString(String value, Int32 length)
            {
                value = (value ?? String.Empty) + new String((Char)0x0, length);
                Byte[] chars = _dbaseFile.Encoding.GetBytes(value.Substring(0, length));
                _binaryWriter.Write(chars);
            }

            private void writeBoolean(Boolean value)
            {
                Byte[] bytes = value ? EncodingEx.GetASCII().GetBytes("T") : EncodingEx.GetASCII().GetBytes("F");
                _binaryWriter.Write(bytes);
            }

            #endregion
        }

        #endregion
    }
}