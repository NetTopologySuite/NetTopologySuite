using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.IO
{

    /// <summary>
    /// Extends the <see cref="BinaryWriter" /> class to allow writing values in the BigEndian format.    
    /// </summary>
    /// <remarks>
    /// While <see cref="BigEndianBinaryWriter" /> extends <see cref="BinaryWriter" /> 
    /// adding methods for writing integer values (<see cref="BigEndianBinaryWriter.WriteIntBE" />)
    /// and double values (<see cref="BigEndianBinaryWriter.WriteDoubleBE" />) in the BigEndian format,
    /// this implementation overrides methods, such <see cref="BinaryWriter.Write(int)" /> 
    /// and <see cref="BinaryWriter.Write(double)" /> and more, 
    /// for writing <see cref="T:ByteOrder.BigEndian" /> values in the BigEndian format.
    /// </remarks>
    public class BEBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryWriter"/> class.
        /// </summary>
        public BEBinaryWriter() : base() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The supplied stream.</param>
        /// <exception cref="T:System.ArgumentNullException">output is null. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed. </exception>
        public BEBinaryWriter(Stream output) : base(output) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The supplied stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <exception cref="T:System.ArgumentNullException">output or encoding is null. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed. </exception>
        public BEBinaryWriter(Stream output, Encoding encoding) : base(output, encoding) { }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream using BigEndian encoding
        /// and advances the stream position by two bytes.
        /// </summary>
        /// <param name="value">The two-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 2);

            Array.Reverse(bytes, 0, 2);
            Write(bytes);
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream  using BigEndian encoding
        /// and advances the stream position by two bytes.
        /// </summary>
        /// <param name="value">The two-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(ushort value)
        {            
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 2);

            Array.Reverse(bytes, 0, 2);
            Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte signed integer to the current stream using BigEndian encoding
        /// and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);

            Array.Reverse(bytes, 0, 4);
            Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream using BigEndian encoding
        /// and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);

            Array.Reverse(bytes, 0, 4);
            Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream using BigEndian encoding
        /// and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 8);

            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream using BigEndian encoding
        /// and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 8);

            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream using BigEndian encoding
        /// and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte floating-point value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);

            Array.Reverse(bytes, 0, 4);
            Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream using BigEndian encoding
        /// and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte floating-point value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 8);

            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }

        /// <summary>
        /// Writes a length-prefixed string to this stream in the current encoding 
        /// of the <see cref="T:System.IO.BinaryWriter"></see>, 
        /// and advances the current position of the stream in accordance 
        /// with the encoding used and the specific characters being written to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ArgumentNullException">value is null. </exception>
        [Obsolete("Not implemented")]
        public override void Write(string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a decimal value to the current stream and advances the stream position by sixteen bytes.
        /// </summary>
        /// <param name="value">The decimal value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        [Obsolete("Not implemented")]
        public override void Write(decimal value)
        {
            throw new NotImplementedException();
        }
    }
}
