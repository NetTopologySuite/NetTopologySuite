using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    internal class ShapeMBRIterator : IEnumerator<MBRInfo>
    {
        private readonly BigEndianBinaryReader m_Reader;

        public ShapeMBRIterator(BigEndianBinaryReader reader)
        {
            m_Reader = reader;
            Reset();
        }

        public MBRInfo Current
        {
            get;
            set;
        }

        public void Dispose() { }

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

            // Read length of shape from file - its written in WORD units so its actually twice the size in bytes.
            int currShapeLength = m_Reader.ReadInt32BE();

            // Skip to the MBR part of the shape.
            m_Reader.BaseStream.Seek(4, SeekOrigin.Current);

            Double xMin = m_Reader.ReadDouble();
            Double yMin = m_Reader.ReadDouble();
            Double xMax = m_Reader.ReadDouble();
            Double yMax = m_Reader.ReadDouble();

            // Skip rest of shape, multiply read size by 2 for WORD->byte conversion.
            m_Reader.BaseStream.Seek((currShapeLength * 2) - 36, SeekOrigin.Current);

            Envelope env = new Envelope(new Coordinate(xMin, yMin), new Coordinate(xMax, yMax));
            Current = new MBRInfo(env, currShapeOffset, currShapeIndex);

            return true;
        }

        public void Reset()
        {
            m_Reader.BaseStream.Seek(100, SeekOrigin.Begin);
        }
    }
}
