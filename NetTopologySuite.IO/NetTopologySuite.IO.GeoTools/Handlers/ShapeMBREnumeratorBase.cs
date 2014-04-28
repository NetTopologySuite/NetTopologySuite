using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetTopologySuite.IO.Handlers
{
    internal abstract class ShapeMBREnumeratorBase : IEnumerator<MBRInfo>
    {
        protected ShapeMBREnumeratorBase(BigEndianBinaryReader reader)
        {
            Reader = reader;
            Reset();
        }

        ~ShapeMBREnumeratorBase()
        {
            Reader.Close();
        }

        protected BigEndianBinaryReader Reader { get; private set; }

        public MBRInfo Current
        {
            get;
            private set;
        }

        public void Dispose()
        {
            Reader.Close();
            GC.SuppressFinalize(this);
        }

        object System.Collections.IEnumerator.Current
        {
            get { return ((IEnumerator<MBRInfo>)this).Current; }
        }

        public bool MoveNext()
        {
            if (Reader.BaseStream.Position >= Reader.BaseStream.Length)
            {
                return false;
            }

            long currShapeOffset;
            int currShapeIndex;
            int currShapeLengthInWords;

            if (!ReadNextNonNullShape(out currShapeOffset, out currShapeIndex, out currShapeLengthInWords))
            {
                return false;
            }

            int numOfBytesRead;

            Envelope currEnv = ReadCurrentEnvelope(out numOfBytesRead);

            Current = new MBRInfo(currEnv, currShapeOffset, currShapeIndex);

            // Take the total size of the shape, substract already read bytes and the size of the shape type.
            int numOfBytesToSkip = (currShapeLengthInWords * 2) - numOfBytesRead - 4;

            if (numOfBytesToSkip != 0)
            {
                Reader.BaseStream.Seek(numOfBytesToSkip, SeekOrigin.Current);
            }

            return true;
        }

        public void Reset()
        {
            Reader.BaseStream.Seek(100, SeekOrigin.Begin);
        }

        protected abstract Envelope ReadCurrentEnvelope(out int numOfBytesRead);

        /// <summary>
        /// Keep reading shapes until we find a non-null one.
        /// </summary>
        /// <param name="CurrShapeOffset"></param>
        /// <param name="CurrShapeIndex"></param>
        /// <param name="CurrShapeLengthInWords"></param>
        /// <returns> False if reached end of file without finding one, otherwise true. </returns>
        private bool ReadNextNonNullShape(out long CurrShapeOffset, out int CurrShapeIndex, out int CurrShapeLengthInWords)
        {
            int currShapeType;

            do
            {
                CurrShapeOffset = Reader.BaseStream.Position;

                // Read shape index - substract 1 for a 0-based index.
                CurrShapeIndex = Reader.ReadInt32BE() - 1;

                CurrShapeLengthInWords = Reader.ReadInt32BE();

                currShapeType = Reader.ReadInt32();
            } while (Reader.BaseStream.Position < Reader.BaseStream.Length &&
                     currShapeType == (int)ShapeGeometryType.NullShape);

            return (Reader.BaseStream.Position < Reader.BaseStream.Length &&
                    currShapeType != (int)ShapeGeometryType.NullShape);
        }
    }
}
