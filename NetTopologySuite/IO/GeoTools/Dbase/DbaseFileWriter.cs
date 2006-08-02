using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

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
		string _filename;
		BinaryWriter _writer;
		bool headerWritten = false;
		bool recordsWritten = false;
		DbaseFileHeader _header;

		/// <summary>
		/// Initializes a new instance of the DbaseFileWriter class.
		/// </summary>
        /// <param name="filename"></param>
		public DbaseFileWriter(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");
			_filename = filename;
			FileStream filestream  = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
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
			foreach(object columnValue in columnValues)
			{				
				if (columnValue is double)
					Write((double)columnValue, _header.Fields[i].Length, _header.Fields[i].DecimalCount);
				else if (columnValue is float)
					Write((float)columnValue,  _header.Fields[i].Length, _header.Fields[i].DecimalCount);
				else if (columnValue is bool)
					Write((bool)columnValue);
				else if (columnValue is string)
				{
					int length = _header.Fields[i].Length;
					Write((string)columnValue, length);
				}
				else if (columnValue is DateTime)
					Write((DateTime)columnValue);
				i++;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="length"></param>
        /// <param name="decimalCount"></param>
		public void Write(double number, int length, int decimalCount)
		{
			// write with 19 chars.
			string format="{0:";
			for(int i = 0; i < decimalCount;i++)
			{
				if (i==0)
					format = format + "0.";
				format = format + "0";
			}
			format=format + "}";
			string str = String.Format(format,number);
			for (int i=0; i< length-str.Length; i++)
				_writer.Write((byte)0x20);		
			foreach(char c in str)
				_writer.Write(c);			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="length"></param>
        /// <param name="decimalCount"></param>
		public void Write(float number, int length, int decimalCount)
		{
			_writer.Write(String.Format("{0:000000000.000000000}",number));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
		public void Write(string text, int length)
		{
			// ensure string is not too big
			text = text.PadRight(length,' ');
			string dbaseString = text.Substring(0,length);

			// will extra chars get written??
			foreach(char c in dbaseString)
				_writer.Write(c);

			int extraPadding = length - dbaseString.Length;
			for(int i=0; i < extraPadding; i++)
				_writer.Write((byte)0x20);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
		public void Write(DateTime date)
		{
			_writer.Write(date.Year-1900);
			_writer.Write(date.Month);
			_writer.Write(date.Day);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag"></param>
		public void Write(bool flag)
		{
			if (flag)
				 _writer.Write("T");
			else _writer.Write("F");
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
