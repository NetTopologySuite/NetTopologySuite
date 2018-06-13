using System;
using System.IO;
using System.Text;
using GeoAPI.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Extends the <see cref="BinaryReader" /> class to allow reading values in the BigEndian format.
    /// </summary>
    /// <remarks>
    /// While <see cref="BEBinaryReader" /> extends <see cref="BinaryReader" />
    /// adding methods for reading integer values (<see cref="BEBinaryReader.ReadInt32" />)
    /// and double values (<see cref="BEBinaryReader.ReadDouble" />) in the BigEndian format,
    /// this implementation overrides methods, such <see cref="BinaryReader.ReadInt32" />
    /// and <see cref="BinaryReader.ReadDouble" /> and more,
    /// for reading <see cref="ByteOrder.BigEndian" /> values in the BigEndian format.
    /// </remarks>
    [Obsolete("Use " + nameof(BiEndianBinaryReader))]
    public class BEBinaryReader : BinaryReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BEBinaryReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public BEBinaryReader(Stream stream)  : base(stream) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BEBinaryReader"/> class.
        /// </summary>
        /// <param name="input">The supplied stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <exception cref="T:System.ArgumentNullException">encoding is null. </exception>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        public BEBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using big endian encoding
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        public override short ReadInt16()
        {
            return BitTweaks.ReverseByteOrder(base.ReadInt16());
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using big endian encoding
        /// and advances the position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        [CLSCompliant(false)]
        public override ushort ReadUInt16()
        {
            return BitTweaks.ReverseByteOrder(base.ReadUInt16());
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using big endian encoding
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        public override int ReadInt32()
        {
            return BitTweaks.ReverseByteOrder(base.ReadInt32());
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using big endian encoding
        /// and advances the position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        [CLSCompliant(false)]
        public override uint ReadUInt32()
        {
            return BitTweaks.ReverseByteOrder(base.ReadUInt32());
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using big endian encoding
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        public override long ReadInt64()
        {
            return BitTweaks.ReverseByteOrder(base.ReadInt64());
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using big endian encoding
        /// and advances the position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        [CLSCompliant(false)]
        public override ulong ReadUInt64()
        {
            return BitTweaks.ReverseByteOrder(base.ReadUInt64());
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using big endian encoding
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte floating point value read from the current stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        public override float ReadSingle()
        {
            return BitTweaks.ReverseByteOrder(base.ReadSingle());
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using big endian encoding
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte floating point value read from the current stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        public override double ReadDouble()
        {
            return BitTweaks.ReverseByteOrder(base.ReadDouble());
        }

        /// <summary>
        /// Reads a string from the current stream.
        /// The string is prefixed with the length, encoded as an integer seven bits at a time.
        /// </summary>
        /// <returns>The string being read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        [Obsolete("Not implemented")]
        public override string ReadString()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a decimal value from the current stream
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <returns>
        /// A decimal value read from the current stream.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        [Obsolete("Not implemented")]
        public override decimal ReadDecimal()
        {
            throw new NotImplementedException();
        }
    }
}
