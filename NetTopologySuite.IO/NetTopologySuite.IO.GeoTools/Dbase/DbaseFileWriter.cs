using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using NetTopologySuite.IO.Streams;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    ///     This class aids in the writing of Dbase IV files.
    /// </summary>
    /// <remarks>
    ///     Attribute information of an ESRI Shapefile is written using Dbase IV files.
    /// </remarks>
    public class DbaseFileWriter : IDisposable
    {
        private readonly Encoding _encoding;
        private readonly BinaryWriter _writer;
        //private bool _recordsWritten;
        private DbaseFileHeader _header;
        private bool _headerWritten;

        /// <summary>
        /// Initializes a new instance of the DbaseFileWriter class with standard windows encoding (CP1252, LATIN1)
        /// </summary>
        /// <param name="filename">The path to the dbase file</param>
        public DbaseFileWriter(string filename) : this(filename,
            Encoding.GetEncoding(1252)
            )
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileWriter class with the provided encoding.
        /// </summary>
        /// <param name="filename">The path to the dbase file</param>
        /// <param name="encoding">The encoding to use</param>
        public DbaseFileWriter(string filename, Encoding encoding)
            : this(new ShapefileStreamProviderRegistry(filename), encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileWriter class using the provided <paramref name="streamProviderRegistry"/> and the default encoding
        /// </summary>
        /// <param name="streamProviderRegistry">The stream provider registry</param>
        public DbaseFileWriter(IStreamProviderRegistry streamProviderRegistry)
            : this(streamProviderRegistry, Encoding.GetEncoding(1252))
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileWriter class using the provided <paramref name="streamProviderRegistry"/> and the given <paramref name="encoding"/>.
        /// </summary>
        /// <param name="streamProviderRegistry">The stream provider registry</param>
        /// <param name="encoding">The encoding</param>
        public DbaseFileWriter(IStreamProviderRegistry streamProviderRegistry, Encoding encoding)
        {
            if (streamProviderRegistry == null)
                throw new ArgumentNullException("streamProviderRegistry");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _encoding = encoding;
            _writer = new BinaryWriter(streamProviderRegistry[StreamTypes.Data].OpenWrite(false), _encoding);
        }

        /// <summary>
        /// Method to write <paramref name="header"/> to the dbase stream
        /// </summary>
        /// <param name="header">The header to write</param>
        public void Write(DbaseFileHeader header)
        {
            //if (header == null)
            //    throw new ArgumentNullException("header");
            ////if (_recordsWritten)
            ////    throw new InvalidOperationException("Records have already been written. Header file needs to be written first.");
            //_headerWritten = true;

            //if (header.Encoding.WindowsCodePage != _encoding.WindowsCodePage)
            //{
            //    header.Encoding = _encoding;
            //}

            //header.WriteHeader(_writer);
            //_header = header;

            if (header == null)
                throw new ArgumentNullException("header");
            //if (_recordsWritten)
            //    throw new InvalidOperationException("Records have already been written. Header file needs to be written first.");

            _headerWritten = true;

            if (header.Encoding.WindowsCodePage != _encoding.WindowsCodePage)
            {
                header.Encoding = _encoding;
            }

            // Get the current position
            var currentPosition = (int)_writer.BaseStream.Position;

            //Header should always be written first in the file
            if (_writer.BaseStream.Position != 0)
                _writer.Seek(0, SeekOrigin.Begin);

            // actually write the header
            header.WriteHeader(_writer);

            // reposition the stream
            if (currentPosition != 0)
                _writer.Seek(currentPosition, SeekOrigin.Begin);

            _header = header;

        }

        /// <summary>
        /// Gets a value indicating if the header has been written or not
        /// </summary>
        public bool HeaderWritten { get { return _headerWritten; } }

        /// <summary>
        /// Method to write the column values for a dbase record
        /// </summary>
        /// <param name="columnValues">The column values</param>
        public void Write(IList columnValues)
        {
            if (columnValues == null)
                throw new ArgumentNullException("columnValues");
            if (!_headerWritten)
                throw new InvalidOperationException("Header records need to be written first.");
            var i = 0;

            // Check integrety of data
            if (columnValues.Count != _header.NumFields)
                throw new ArgumentException("The number of provided values does not match the number of fields defined", "columnValues");

            // Get the current position
            var initialPosition = _writer.BaseStream.Position;

            // the deleted flag
            _writer.Write((byte)0x20); 
            foreach (var columnValue in columnValues)
            {
                var headerField = _header.Fields[i];

                if (columnValue == null || columnValue == DBNull.Value)
                {
                    // Don't corrupt the file by not writing if the value is null.
                    // Instead, treat it like an empty string.
                    Write(string.Empty, headerField.Length);
                }
                else if (headerField.Type == typeof(string))
                {
                    // If the column is a character column, the values in that
                    // column should be treated as text, even if the column value
                    // is not a string.
                    Write(columnValue.ToString(), headerField.Length);
                }
                else if (IsRealType(columnValue.GetType()))
                {
                    Write(Convert.ToDecimal(columnValue), headerField.Length, headerField.DecimalCount);
                }
                else if (IsIntegerType(columnValue.GetType()))
                {
                    Write(Convert.ToDecimal(columnValue), headerField.Length, headerField.DecimalCount);
                }
                else if (columnValue is decimal)
                {
                    Write((decimal)columnValue, headerField.Length, headerField.DecimalCount);
                }
                else if (columnValue is bool)
                {
                    Write((bool)columnValue);
                }
                else if (columnValue is string)
                {
                    Write((string)columnValue, headerField.Length);
                }
                else if (columnValue is DateTime)
                {
                    Write((DateTime)columnValue);
                }
                else if (columnValue is char)
                {
                    Write((char)columnValue, headerField.Length);
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("Invalid argument for column '{0}': {1}", 
                                      headerField.Name, columnValue),
                        "columnValues");
                }
                i++;
            }

            // Get the number of bytes written
            var bytesWritten = _writer.BaseStream.Position - initialPosition;

            // Get the record length (at least one byte for the deleted marker)
            var recordLength = Math.Max(1, _header.RecordLength);

            // Check if the correct amount of bytes was written
            if (bytesWritten != recordLength)
                throw new ShapefileException("Error writing Dbase record");
        }

        /// <summary>
        /// Function to determine if the <paramref name="type"/> is a <see cref="System.Single"/> or <see cref="System.Double"/> type.
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <returns><value>true</value> if it is either a <see cref="System.Single"/> or <see cref="System.Double"/> type, otherwise <value>false</value></returns>
        private static bool IsRealType(Type type)
        {
            return ((type == typeof(double)) || (type == typeof(float)));
        }

        /// <summary>
        /// Function to determine if <paramref name="type"/> is a "whole" number type.
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <returns><value>true</value> if <paramref name="type"/>is one of <list type="bullet">
        /// <item><see cref="System.Int16"/>, <see cref="System.UInt16"/></item>
        /// <item><see cref="System.Int32"/>, <see cref="System.UInt32"/></item>
        /// <item><see cref="System.Int64"/>, <see cref="System.UInt64"/></item>
        /// </list>, otherwise <value>false</value>
        /// </returns>
        private static bool IsIntegerType(Type type)
        {
            return ((type == typeof(short)) || (type == typeof(int)) || (type == typeof(long)) ||
                    (type == typeof(ushort)) || (type == typeof(uint)) || (type == typeof(ulong)));
        }

        /// <summary>
        /// Write a decimal value to the file.
        /// </summary>
        /// <param name="number">The value to write.</param>
        /// <param name="length">The overall width of the column being written to.</param>
        /// <param name="decimalCount">The number of decimal places in the column.</param>
        private void Write(decimal number, int length, int decimalCount)
        {
            string outString;

            var wholeLength = length;
            if (decimalCount > 0)
                wholeLength -= (decimalCount + 1);

            // Force to use point as decimal separator
            var strNum = Convert.ToString(number, Global.GetNfi());
            var decimalIndex = strNum.IndexOf('.');
            if (decimalIndex < 0)
                decimalIndex = strNum.Length;

            if (decimalIndex > wholeLength)
            {
                // Too many digits to the left of the decimal. Use the left
                // most "wholeLength" number of digits. All values to the right
                // of the decimal will be '0'.
                var sb = new StringBuilder();
                sb.Append(strNum.Substring(0, wholeLength));
                if (decimalCount > 0)
                {
                    sb.Append('.');
                    for (var i = 0; i < decimalCount; ++i)
                        sb.Append('0');
                }
                outString = sb.ToString();
            }
            else
            {
                // Chop extra digits to the right of the decimal.
                var sb = new StringBuilder();
                sb.Append("{0:0");
                if (decimalCount > 0)
                {
                    sb.Append('.');
                    for (var i = 0; i < decimalCount; ++i)
                        sb.Append('0');
                }
                sb.Append('}');
                // Force to use point as decimal separator
                outString = string.Format(Global.GetNfi(), sb.ToString(), number);
            }

            for (var i = 0; i < length - outString.Length; i++)
                _writer.Write((byte)0x20);
            foreach (var c in outString)
                _writer.Write(c);
        }

        ///// <summary>
        ///// </summary>
        ///// <param name="number"></param>
        ///// <param name="length"></param>
        ///// <param name="decimalCount"></param>
        //private void Write(double number, int length, int decimalCount)
        //{
        //    Write(Convert.ToDecimal(number), length, decimalCount);
        //}

        ///// <summary>
        ///// </summary>
        ///// <param name="number"></param>
        ///// <param name="length"></param>
        ///// <param name="decimalCount"></param>
        //private void Write(float number, int length, int decimalCount)
        //{
        //    Write(Convert.ToDecimal(number), length, decimalCount);
        //}

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        private void Write(string text, int length)
        {
            // ensure string is not too big, multibyte encodings can cause more bytes to be written
            var bytes = _encoding.GetBytes(text);
            var counter = 0;
            foreach (var c in bytes)
            {
                _writer.Write(c);
                counter++;
                if (counter >= length)
                    break;
            }

            // pad the text after exact byte count is known
            var padding = length - counter;
            for (var i = 0; i < padding; i++)
                _writer.Write((byte)0x20);
        }

        /// <summary>
        /// </summary>
        /// <param name="date"></param>
        private void Write(DateTime date)
        {
            foreach (var c in date.Year.ToString(NumberFormatInfo.InvariantInfo))
                _writer.Write(c);

            if (date.Month < 10)
                _writer.Write('0');
            foreach (var c in date.Month.ToString(NumberFormatInfo.InvariantInfo))
                _writer.Write(c);

            if (date.Day < 10)
                _writer.Write('0');
            foreach (var c in date.Day.ToString(NumberFormatInfo.InvariantInfo))
                _writer.Write(c);
        }

        /// <summary>
        /// </summary>
        /// <param name="flag"></param>
        private void Write(bool flag)
        {
            _writer.Write(flag ? 'T' : 'F');
        }

        /// <summary>
        ///     Write a character to the file.
        /// </summary>
        /// <param name="c">The character to write.</param>
        /// <param name="length">
        ///     The length of the column to write in. Writes
        ///     left justified, filling with spaces.
        /// </param>
        private void Write(char c, int length)
        {
            var str = string.Empty;
            str += c;
            Write(str, length);
        }

        /// <summary>
        ///     Write a byte to the file.
        /// </summary>
        /// <param name="number">The byte.</param>
        private void Write(byte number)
        {
            _writer.Write(number);
        }

        /// <summary>
        /// Method to close this dbase file writer
        /// </summary>
        public void Close()
        {
            _writer.Close();
        }

        /// <summary>
        /// Method to dispose this writers instance
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~DbaseFileWriter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating that this dbase file writer has been disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Method to write the end of dbase file marker (<value>0x1A</value>).
        /// </summary>
        public void WriteEndOfDbf()
        {
            Write((byte)0x1A);
        }
    }
}