using System;
using System.IO;
using System.Text;

using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{

    /// <summary>
    /// Extends the <see cref="BinaryWriter" /> class to allow writing values in the BigEndian format.
    /// </summary>
    /// <remarks>
    /// While <see cref="BEBinaryWriter" /> extends <see cref="BinaryWriter" />
    /// adding methods for writing integer values (<see cref="Write(int)" />)
    /// and double values (<see cref="Write(double)" />) in the BigEndian format,
    /// this implementation overrides methods, such <see cref="BinaryWriter.Write(int)" />
    /// and <see cref="BinaryWriter.Write(double)" /> and more,
    /// for writing <see cref="ByteOrder.BigEndian" /> values in the BigEndian format.
    /// </remarks>
    public class BEBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BEBinaryWriter"/> class.
        /// </summary>
        public BEBinaryWriter() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BEBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The supplied stream.</param>
        /// <exception cref="T:System.ArgumentNullException">output is null. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The stream does not support writing, or the stream is already closed. </exception>
        public BEBinaryWriter(Stream output) : base(output) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BEBinaryWriter"/> class.
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
            base.Write(BitTweaks.ReverseByteOrder(value));
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream  using BigEndian encoding
        /// and advances the stream position by two bytes.
        /// </summary>
        /// <param name="value">The two-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        [CLSCompliant(false)]
        public override void Write(ushort value)
        {
            base.Write(BitTweaks.ReverseByteOrder(value));
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
            base.Write(BitTweaks.ReverseByteOrder(value));
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream using BigEndian encoding
        /// and advances the stream position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        [CLSCompliant(false)]
        public override void Write(uint value)
        {
            base.Write(BitTweaks.ReverseByteOrder(value));
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
            base.Write(BitTweaks.ReverseByteOrder(value));
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream using BigEndian encoding
        /// and advances the stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        [CLSCompliant(false)]
        public override void Write(ulong value)
        {
            base.Write(BitTweaks.ReverseByteOrder(value));
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
            base.Write(BitTweaks.ReverseByteOrder(value));
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
            base.Write(BitTweaks.ReverseByteOrder(value));
        }
    }
}
