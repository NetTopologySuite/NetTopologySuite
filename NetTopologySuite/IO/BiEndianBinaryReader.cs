using System;
using System.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Extends the <see cref="BinaryReader" /> class to allow reading values in the specified format.
    /// </summary>
    /// <remarks>
    /// While <see cref="BiEndianBinaryReader" /> extends <see cref="BinaryReader" />
    /// adding methods for reading integer values (<see cref="ReadInt32" />)
    /// and double values (<see cref="ReadDouble" />) in the specified format,
    /// this implementation overrides methods, such <see cref="BinaryReader.ReadInt32" />
    /// and <see cref="BinaryReader.ReadDouble" /> and more,
    /// for reading values in the specified by <see cref="Endianess"/> format.
    /// </remarks>
    public class BiEndianBinaryReader : BinaryReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiEndianBinaryReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public BiEndianBinaryReader(Stream stream) : base(stream) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiEndianBinaryReader"/> class.
        /// </summary>
        /// <param name="input">The supplied stream.</param>
        /// <param name="endianess">The byte order.</param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        public BiEndianBinaryReader(Stream input, ByteOrder endianess) : base(input)
            => Endianess = endianess;

        /// <summary>
        /// Encoding type
        /// </summary>
        public ByteOrder Endianess { get; set; }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the specified encoding
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
            short result = base.ReadInt16();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the specified encoding
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
            ushort result = base.ReadUInt16();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the specified encoding
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
            int result = base.ReadInt32();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the specified encoding
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
            uint result = base.ReadUInt32();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the specified encoding
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
            long result = base.ReadInt64();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the specified encoding
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
            ulong result = base.ReadUInt64();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the specified encoding
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
            float result = base.ReadSingle();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using the specified encoding
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
            double result = base.ReadDouble();
            return (Endianess == ByteOrder.BigEndian)
                ? BitTweaks.ReverseByteOrder(result)
                : result;
        }

        /// <summary>
        /// Reads a string from the current stream.
        /// The string is prefixed with the length, encoded as an integer seven bits at a time.
        /// </summary>
        /// <returns>The string being read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        public override string ReadString()
        {
            if (Endianess == ByteOrder.BigEndian)
                throw new NotSupportedException();
            return base.ReadString();
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
        public override decimal ReadDecimal()
        {
            if (Endianess == ByteOrder.BigEndian)
                throw new NotSupportedException();
            return base.ReadDecimal();
        }
    }
}