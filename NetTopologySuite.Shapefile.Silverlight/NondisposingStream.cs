using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Shapefile
{
    public class NondisposingStream : Stream
    {
        Stream Inner { get; set; }
        public NondisposingStream(Stream inner)
        {
            Inner = inner;
        }

        public override bool CanRead
        {
            get { return Inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return Inner.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return Inner.CanWrite; }
        }

        public override void Flush()
        {
            Inner.Flush();
        }

        public override long Length
        {
            get { return Inner.Length; }
        }

        public override long Position
        {
            get
            {
                return Inner.Position;
            }
            set
            {
                Inner.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Inner.Write(buffer, offset, count);
        }
    }
}
