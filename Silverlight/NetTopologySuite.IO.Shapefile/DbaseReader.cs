// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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
using System.IO;
using System.Text;

namespace NetTopologySuite.Shapefile
{
    internal partial class DbaseFile
    {
        private class DbaseReader : IDisposable
        {
            #region Instance fields
            private readonly DbaseFile _dbaseFile;
            private readonly UInt32 _rowCount;
            private BinaryReader _dbaseReader;
            private Boolean _isDisposed;
            private UInt32 _currentRowId;
            private readonly Object[] _currentRowValues;
            private Boolean _isCurrentRowDeleted;

            #endregion

            #region Object Construction/Destruction

            /// <summary>
            /// Creates a new instance of a <see cref="DbaseReader"/> for the
            /// <paramref name="file" />.
            /// </summary>
            /// <param name="file">The controlling DbaseFile instance.</param>
            public DbaseReader(DbaseFile file)
            {
                _dbaseFile = file;
                _dbaseReader = new BinaryReader(file.DataStream, file.Encoding);
                _rowCount = file.Header.RecordCount;
                _currentRowValues = new Object[_dbaseFile.Header.Columns.Count];
            }

            #region Dispose Pattern

            ~DbaseReader()
            {
                dispose(false);
            }

            /// <summary>
            /// Gets a value which indicates if this Object is disposed: 
            /// <see langword="true"/> if it is, <see langword="false"/> otherwise
            /// </summary>
            /// <seealso cref="Dispose"/>
            internal Boolean IsDisposed
            {
                get { return _isDisposed; }
                private set { _isDisposed = value; }
            }

            #region IDisposable members

            /// <summary>
            /// Closes all files and disposes of all resources.
            /// </summary>
            /// <seealso cref="Close"/>
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
            #endregion

            private void dispose(Boolean disposing)
            {
                if (IsDisposed)
                {
                    return;
                }

                if (disposing) // Do deterministic finalization of managed resources
                {
                    // Closing the reader closes the file stream
                    if (_dbaseReader != null) _dbaseReader.Close();
                    _dbaseReader = null;
                }
            }

            #endregion

            #endregion

            internal Boolean IsRowDeleted(UInt32 row)
            {
                ensureCurrentValues(row);
                return _isCurrentRowDeleted;
            }

            /// <summary>
            /// Gets a value for the given <paramref name="row">row index</paramref> and 
            /// <paramref name="column">column index</paramref>.
            /// </summary>
            /// <param name="row">
            /// Index of the row to retrieve value from. Zero-based.
            /// </param>
            /// <param name="column">
            /// Index of the column to retrieve value from. Zero-based.
            /// </param>
            /// <returns>
            /// The value at the given (row, column).
            /// </returns>
            /// <exception cref="ObjectDisposedException">
            /// Thrown when the method is called and 
            /// Object has been disposed.
            /// </exception>
            /// <exception cref="InvalidDbaseFileOperationException">
            /// Thrown if this reader is 
            /// closed (check <see cref="IsOpen"/> before calling), or if the column is an 
            /// unsupported type.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Thrown if <paramref name="row"/> is 
            /// less than 0 or greater than <see cref="RecordCount"/> - 1.
            /// </exception>
            internal Object GetValue(UInt32 row, DbaseField column)
            {
                ensureCurrentValues(row);

                if (_currentRowValues[column.Ordinal] is Exception)
                {
                    throw (Exception)_currentRowValues[column.Ordinal];
                }

                return _currentRowValues[column.Ordinal];
            }

            internal Object[] GetValues(UInt32 row)
            {
                checkState();

                checkRowIndex(row);

                ensureCurrentValues(row);
                Int32 length = _currentRowValues.Length;
                Object[] values = new Object[length];
                Array.Copy(_currentRowValues, values, length);
                return values;
            }

            #region Private helper methods

            private void ensureCurrentValues(UInt32 row)
            {
                if (row != _currentRowId)
                {
                    moveToRow(row);
                    readDbfValues();
                    _currentRowId = row;
                }
            }

            private void moveToRow(UInt32 row)
            {
                DbaseHeader header = _dbaseFile.Header;

                // Compute the position in the file stream for the requested row
                Int64 offset = header.HeaderLength + ((row - 1) * header.RecordLength);

                // Seek to the computed offset
                _dbaseFile.DataStream.Seek(offset, SeekOrigin.Begin);
            }

            private void checkRowIndex(UInt32 row)
            {
                if (row <= 0 || row > _rowCount)
                {
                    throw new ArgumentOutOfRangeException("Invalid row requested " +
                                                          "at index " + row);
                }
            }

            private void checkState()
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("Attempt to access a disposed " +
                                                      "DbaseReader Object");
                }

                if (!_dbaseFile.IsOpen)
                {
                    throw new InvalidDbaseFileOperationException("An attempt was made to read " +
                                                                 "from a closed DBF file");
                }
            }

            private void readDbfValues()
            {
                _isCurrentRowDeleted = _dbaseReader.ReadChar() == DbaseConstants.DeletedIndicator;

                foreach (DbaseField dbf in _dbaseFile.Header.Columns)
                {
                    Object value;

                    switch (Type.GetTypeCode(dbf.DataType))
                    {
                        case TypeCode.Boolean:
                            Char tempChar = (Char)_dbaseReader.ReadByte();
                            value = ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'));
                            break;

                        case TypeCode.DateTime:
                            DateTime date;
                            // Mono has not yet implemented DateTime.TryParseExact
#if !MONO
                            value = DateTime.TryParseExact(Encoding.UTF8.GetString(_dbaseReader.ReadBytes(8), 0, 8),
                                                          "yyyyMMdd", DbaseConstants.StorageNumberFormat,
                                                          DateTimeStyles.None, out date)
                                       ? (Object)date
                                       : DBNull.Value;
#else
					    try 
					    {
						    date = DateTime.ParseExact(Encoding.UTF8.GetString((_dbaseReader.ReadBytes(8))), 	
						        "yyyyMMdd", DbaseConstants.StorageNumberFormat, DateTimeStyles.None);

					        return date;
					    }
					    catch (FormatException)
					    {
						    return DBNull.Value;
					    }
#endif
                            break;

                        case TypeCode.Double:
                            String temp =
                                Encoding.UTF8.GetString(_dbaseReader.ReadBytes(dbf.Length), 0, dbf.Length).Replace("\0", "").Trim();
                            Double dbl;

                            value = Double.TryParse(temp, NumberStyles.Float, DbaseConstants.StorageNumberFormat, out dbl)
                                       ? (Object)dbl
                                       : DBNull.Value;
                            break;

                        case TypeCode.Int16:
                            String temp16 =
                                Encoding.UTF8.GetString(_dbaseReader.ReadBytes(dbf.Length), 0, dbf.Length).Replace("\0", "").Trim();
                            Int16 i16;

                            value = Int16.TryParse(temp16, NumberStyles.Float, DbaseConstants.StorageNumberFormat,
                                                  out i16)
                                       ? (Object)i16
                                       : DBNull.Value;
                            break;

                        case TypeCode.Int32:
                            String temp32 =
                                Encoding.UTF8.GetString(_dbaseReader.ReadBytes(dbf.Length), 0, dbf.Length).Replace("\0", "").Trim();
                            Int32 i32;

                            value = Int32.TryParse(temp32, NumberStyles.Float, DbaseConstants.StorageNumberFormat,
                                                  out i32)
                                       ? (Object)i32
                                       : DBNull.Value;
                            break;

                        case TypeCode.Int64:
                            String temp64 =
                                Encoding.UTF8.GetString(_dbaseReader.ReadBytes(dbf.Length), 0, dbf.Length).Replace("\0", "").Trim();
                            Int64 i64 = 0;

                            value = Int64.TryParse(temp64, NumberStyles.Float, DbaseConstants.StorageNumberFormat,
                                                  out i64)
                                       ? (Object)i64
                                       : DBNull.Value;
                            break;

                        case TypeCode.Single:
                            String temp4 = Encoding.UTF8.GetString(_dbaseReader.ReadBytes(dbf.Length), 0, dbf.Length);
                            Single f = 0;

                            value = Single.TryParse(temp4, NumberStyles.Float, DbaseConstants.StorageNumberFormat, out f)
                                       ? (Object)f
                                       : DBNull.Value;
                            break;

                        case TypeCode.String:
                            {
                                Byte[] chars = _dbaseReader.ReadBytes(dbf.Length);
                                String s = _dbaseFile.Encoding.GetString(chars, 0, chars.Length);
                                value = s.Replace("\0", "").Trim();
                            }
                            break;

                        case TypeCode.Char:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Byte:
                        case TypeCode.DBNull:
                        case TypeCode.Object:
                        case TypeCode.SByte:
                        case TypeCode.Empty:
                        case TypeCode.Decimal:
                        default:
                            value = new NotSupportedException("Cannot parse DBase field '" +
                                                              dbf.ColumnName + "' of type '" +
                                                              dbf.DataType + "'");
                            break;
                    }

                    _currentRowValues[dbf.Ordinal] = value;
                }
            }
            #endregion
        }
    }
}