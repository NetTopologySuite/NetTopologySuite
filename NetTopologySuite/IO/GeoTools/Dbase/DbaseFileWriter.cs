using System;
using System.Collections;
using System.IO;
using System.Text;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// This class aids in the writing of Dbase IV files. 
    /// </summary>
    /// <remarks>
    /// Attribute information of an ESRI Shapefile is written using Dbase IV files.
    /// </remarks>
    public class DbaseFileWriter
    {
        readonly BinaryWriter _writer;
        private bool headerWritten;
        private bool recordsWritten;
        private DbaseFileHeader _header;

        /// <summary>
        /// Initializes a new instance of the DbaseFileWriter class.
        /// </summary>
        /// <param name="filename"></param>
        public DbaseFileWriter(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            FileStream filestream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
            _writer = new BinaryWriter(filestream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        public void Write(DbaseFileHeader header)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            if (recordsWritten)
                throw new InvalidOperationException("Records have already been written. Header file needs to be written first.");
            headerWritten = true;
            header.WriteHeader(_writer);
            _header = header;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnValues"></param>
        public void Write(IList columnValues)
        {
            if (columnValues == null)
                throw new ArgumentNullException("columnValues");
            if (!headerWritten)
                throw new InvalidOperationException("Header records need to be written first.");
            int i = 0;
            _writer.Write((byte)0x20); // the deleted flag
            foreach (object columnValue in columnValues)
            {
                DbaseFieldDescriptor headerField = _header.Fields[i];

                if (columnValue == null)
                    // Don't corrupt the file by not writing if the value is null.
                    // Instead, treat it like an empty string.
                    Write(string.Empty, headerField.Length);
                else if (headerField.Type == typeof(string))
                    // If the column is a character column, the values in that
                    // column should be treated as text, even if the column value
                    // is not a string.
                    Write(columnValue.ToString(), headerField.Length);
                else if (IsRealType(columnValue.GetType()))
                    Write(Convert.ToDecimal(columnValue), headerField.Length, headerField.DecimalCount);
                else if (IsIntegerType(columnValue.GetType()))
                    Write(Convert.ToDecimal(columnValue), headerField.Length, headerField.DecimalCount);
                else if (columnValue is Decimal)
                    Write((decimal)columnValue, headerField.Length, headerField.DecimalCount);
                else if (columnValue is Boolean)
                    Write((bool)columnValue);
                else if (columnValue is string)
                    Write((string)columnValue, headerField.Length);
                else if (columnValue is DateTime)
                    Write((DateTime)columnValue);
                else if (columnValue is Char)
                    Write((Char)columnValue, headerField.Length);

                i++;
            }
        }

        /// <summary>
        /// Determine if the type provided is a "real" or "float" type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static private bool IsRealType(Type type)
        {
            return ((type == typeof(Double)) || (type == typeof(Single)));
        }

        /// <summary>
        /// Determine if the type provided is a "whole" number type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static private bool IsIntegerType(Type type)
        {
            return ((type == typeof(Int16)) || (type == typeof(Int32)) || (type == typeof(Int64)) ||
                    (type == typeof(UInt16)) || (type == typeof(UInt32)) || (type == typeof(UInt64)));
        }

        /// <summary>
        /// Write a decimal value to the file.
        /// </summary>
        /// <param name="number">The value to write.</param>
        /// <param name="length">The overall width of the column being written to.</param>
        /// <param name="decimalCount">The number of decimal places in the column.</param>
        public void Write(decimal number, int length, int decimalCount)
        {
            string outString = string.Empty;

            int wholeLength = length;
            if (decimalCount > 0)
                wholeLength -= (decimalCount + 1);

            // Force to use point as decimal separator
            string strNum = Convert.ToString(number, Global.GetNfi());
            int decimalIndex = strNum.IndexOf('.');
            if (decimalIndex < 0)
                 decimalIndex = strNum.Length;

            if (decimalIndex > wholeLength)
            {
                // Too many digits to the left of the decimal. Use the left
                // most "wholeLength" number of digits. All values to the right
                // of the decimal will be '0'.
                StringBuilder sb = new StringBuilder();
                sb.Append(strNum.Substring(0, wholeLength));
                if (decimalCount > 0)
                {
                    sb.Append('.');
                    for (int i = 0; i < decimalCount; ++i)
                        sb.Append('0');
                }
                outString = sb.ToString();
            }
            else
            {
                // Chop extra digits to the right of the decimal.
                StringBuilder sb = new StringBuilder();
                sb.Append("{0:0");
                if (decimalCount > 0)
                {
                    sb.Append('.');
                    for (int i = 0; i < decimalCount; ++i)
                        sb.Append('0');
                }
                sb.Append('}');
                // Force to use point as decimal separator
                outString = String.Format(Global.GetNfi(), sb.ToString(), number);                
            }

            for (int i = 0; i < length - outString.Length; i++)
                _writer.Write((byte)0x20);
            foreach (char c in outString)
                _writer.Write(c);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="length"></param>
        /// <param name="decimalCount"></param>
        public void Write(double number, int length, int decimalCount)
        {
            Write(Convert.ToDecimal(number), length, decimalCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="length"></param>
        /// <param name="decimalCount"></param>
        public void Write(float number, int length, int decimalCount)
        {
            Write(Convert.ToDecimal(number), length, decimalCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        public void Write(string text, int length)
        {
            // ensure string is not too big
            text = text.PadRight(length, ' ');
            string dbaseString = text.Substring(0, length);

            // will extra chars get written??
            foreach (char c in dbaseString)
                _writer.Write(c);

            int extraPadding = length - dbaseString.Length;
            for (int i = 0; i < extraPadding; i++)
                _writer.Write((byte)0x20);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        public void Write(DateTime date)
        {
            foreach (char c in date.Year.ToString())
                _writer.Write(c);

            if (date.Month < 10)
                _writer.Write('0');
            foreach (char c in date.Month.ToString())
                _writer.Write(c);

            if (date.Day < 10)
                _writer.Write('0');
            foreach (char c in date.Day.ToString())
                _writer.Write(c);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag"></param>
        public void Write(bool flag)
        {
            if (flag)
                _writer.Write('T');
            else
                _writer.Write('F');
        }

        /// <summary>
        /// Write a character to the file.
        /// </summary>
        /// <param name="c">The character to write.</param>
        /// <param name="length">The length of the column to write in. Writes
        /// left justified, filling with spaces.</param>
        public void Write(char c, int length)
        {
            string str = string.Empty;
            str += c;
            Write(str, length);
        }

        /// <summary>
        /// Write a byte to the file.
        /// </summary>
        /// <param name="number">The byte.</param>
        public void Write(byte number)
        {
            _writer.Write(number);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            _writer.Close();
        }
    }
}
