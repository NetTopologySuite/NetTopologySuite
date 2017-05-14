//#define Disposable
using System;
using System.IO;
using System.Text;

namespace NetTopologySuite.IO.Streams
{
    public class ByteStreamProvider : IStreamProvider
#if Disposable
        , IDisposable
#endif
    {
        public ByteStreamProvider(string kind)
        {
            Kind = kind;
            Buffer = new byte[] { };
            UnderlyingStreamIsReadonly = false;
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="kind">The kind of stream</param>
        /// <param name="text">A text to store</param>
        /// <param name="encoding">An encoding to get the bytes.
        ///  If <value>null</value>, <see cref="Encoding.Default"/> is used</param>
        public ByteStreamProvider(string kind, string text, Encoding encoding = null)
            :this(kind, (encoding ?? Encoding.Default).GetBytes(text), -1, true)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="kind">The kind of stream</param>
        /// <param name="stream">The stream</param>
        /// <param name="isReadonly">A value indicating whether the contents are readonly</param>
        public ByteStreamProvider(string kind, Stream stream, bool isReadonly = false)
            :this(kind, ReadFully(stream), -1, isReadonly)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="kind">The kind of stream</param>
        /// <param name="bytes">The array of bytes</param>
        /// <param name="maxLength">The maximum length of the</param>
        /// <param name="isReadonly">A value indicating whether the contents are readonly</param>
        public ByteStreamProvider(string kind, byte[] bytes, int maxLength = -1, bool isReadonly = false)
        {
            Kind = kind;
            MaxLength = maxLength == -1 ? bytes.Length : maxLength;
            if (maxLength > -1)
            {
                Buffer = new byte[MaxLength];
                Length = Math.Min(bytes.Length, MaxLength);
                System.Buffer.BlockCopy(bytes, 0, Buffer, 0, Length);
            }
            else
            {
                Buffer = bytes;
                Length = bytes.Length;
            }

            UnderlyingStreamIsReadonly = isReadonly;
        }

        /// <summary>
        /// Gets a value indicating the maximum size of the <see cref="Buffer"/>
        /// </summary>
        public int MaxLength { get; private set; }

        /// <summary>
        /// Gets a value indicating the used range of the <see cref="Buffer"/>
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Copy the stream to a byte array
        /// </summary>
        /// <param name="input">The stream to copy to a byte array</param>
        /// <returns>byte array</returns>
        private static byte[] ReadFully(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);
            if (input is MemoryStream)
            {
                return ((MemoryStream)input).ToArray();
            }

            using (var ms = new MemoryStream())
            {
                // 81920: largest multiple of 4096 that doesn't go on the LOH.
                // 4096 is a common internal buffer size.
                byte[] array = new byte[81920];
                int count;
                while ((count = input.Read(array, 0, array.Length)) != 0)
                {
                    ms.Write(array, 0, count);
                }

                return ms.ToArray();
            }
        }


        /// <summary>
        /// Array of bytes 
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets a value indicating that the underlying stream is read-only
        /// </summary>
        public bool UnderlyingStreamIsReadonly { get; private set; }

        /// <summary>
        /// Function to return a Stream of the bytes
        /// </summary>
        /// <returns>An opened stream</returns>
        public Stream OpenRead()
        {
            return new ByteStream(this, false);
        }

        /// <summary>
        /// Function to open the underlying stream for writing purposes
        /// </summary>
        /// <remarks>If <see cref="UnderlyingStreamIsReadonly"/> is not <value>true</value> 
        /// this method shall fail</remarks>
        /// <returns>An opened stream</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="UnderlyingStreamIsReadonly"/> is <value>true</value></exception>
        public Stream OpenWrite(bool truncate)
        {
            if (UnderlyingStreamIsReadonly)
                throw new InvalidOperationException();

            if (truncate)
            {
                Buffer = new byte[MaxLength];
                Length = 0;
            }

            return new ByteStream(this, true);
        }

        /// <summary>
        /// Gets a value indicating the kind of stream
        /// </summary>
        public string Kind { get; private set; }

#if Disposable
        #region IDisposable Support
        private bool _isDisposed; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Buffer = null;
                    UnderlyingStreamIsReadonly = true;
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
#endif
        private class ByteStream : MemoryStream
        {
            public ByteStream(ByteStreamProvider provider, bool writable)
                :base(provider.Buffer, 0, writable?provider.MaxLength:provider.Length, writable, true)
            {
                if (writable)
                    base.SetLength(provider.Length);
                Provider = provider;
            }
            private ByteStreamProvider Provider { get; set; }

            public override void Flush()
            {
                Provider.Length = (int)base.Length;
                base.Flush();
            }

            public override void SetLength(long value)
            {
                if (value > Provider.MaxLength)
                    throw new ArgumentException("Cannot set length to be greater than Provider.MaxLength");

                Provider.Length = (int)value;
                base.SetLength(value);
            }
        }
    }
}