using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
    /// Extends the <see cref="BinaryWriter" /> class to allow the writing of integers 
    /// and double values in the Big Endian format.
	/// </summary>
	public class BigEndianBinaryWriter : BinaryWriter
	{		
		/// <summary>
		/// Initializes a new instance of the BigEndianBinaryWriter class.
		/// </summary>
		public BigEndianBinaryWriter() : base() { }

		/// <summary>
		/// Initializes a new instance of the BigEndianBinaryWriter class 
        /// based on the supplied stream and using UTF-8 as the encoding for strings.
		/// </summary>
		/// <param name="output">The supplied stream.</param>
		public BigEndianBinaryWriter(Stream output) : base(output) { }

		/// <summary>
		/// Initializes a new instance of the BigEndianBinaryWriter class 
        /// based on the supplied stream and a specific character encoding.
		/// </summary>
		/// <param name="output">The supplied stream.</param>
		/// <param name="encoding">The character encoding.</param>
		public BigEndianBinaryWriter(Stream output, Encoding encoding): base(output,encoding) { }

		/// <summary>
		/// Reads a 4-byte signed integer using the big-endian layout from the current stream 
        /// and advances the current position of the stream by two bytes.
		/// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
		public void WriteIntBE(int value)
		{
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);

            Array.Reverse(bytes, 0, 4);
            Write(bytes);				
		}

        /// <summary>
        /// Reads a 8-byte signed integer using the big-endian layout from the current stream 
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        public void WriteDoubleBE(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 8);

            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }
	}
}
