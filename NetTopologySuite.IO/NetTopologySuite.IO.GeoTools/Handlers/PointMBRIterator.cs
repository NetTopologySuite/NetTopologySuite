using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    internal class PointMBRIterator : IEnumerator<MBRInfo>
    {
        private readonly BigEndianBinaryReader m_Reader;

        public PointMBRIterator(BigEndianBinaryReader reader)
        {
            m_Reader = reader;
            Reset();
        }

        ~PointMBRIterator()
        {
            m_Reader.Close();
        }

        public MBRInfo Current
        {
            get;
            set;
        }

        public void Dispose()
        {
            m_Reader.Close();
            GC.SuppressFinalize(this);
        }

        object IEnumerator.Current
        {
            get { return ((IEnumerator<Envelope>)this).Current; }
        }

        public bool MoveNext()
        {
            if (m_Reader.BaseStream.Position >= m_Reader.BaseStream.Length)
            {
                return false;
            }

            // Save location of shape metadata.
            long currShapeOffset = m_Reader.BaseStream.Position;

            // Read shape index - substract 1 for a 0-based index.
            int currShapeIndex = m_Reader.ReadInt32BE() - 1;

            int currPointLengthInWords = m_Reader.ReadInt32BE();

            // Skip shape type.
            m_Reader.BaseStream.Seek(4, SeekOrigin.Current);

            double x = m_Reader.ReadDouble();
            double y = m_Reader.ReadDouble();

            // Calculate whether or not there is any data to skip, take the total shape size and substract already read data sizes.
            int numOfBytesToSkip = (currPointLengthInWords * 2) - (8 + 8 + 4);

            if (numOfBytesToSkip != 0)
            {
                m_Reader.BaseStream.Seek(numOfBytesToSkip, SeekOrigin.Current);
            }

            Current = new MBRInfo(new Envelope(new Coordinate(x, y)), currShapeOffset, currShapeIndex);

            return true;
        }

        public void Reset()
        {
            m_Reader.BaseStream.Seek(100, SeekOrigin.Begin);
        }
    }
}
