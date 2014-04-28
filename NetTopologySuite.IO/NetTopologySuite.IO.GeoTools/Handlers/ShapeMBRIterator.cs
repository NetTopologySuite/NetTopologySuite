using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    internal class ShapeMBRIterator : ShapeMBREnumeratorBase
    {
        public ShapeMBRIterator(BigEndianBinaryReader reader)
            : base(reader)
        { }

        protected override Envelope ReadCurrentEnvelope(out int numOfBytesRead)
        {
            Double xMin = Reader.ReadDouble();
            Double yMin = Reader.ReadDouble();
            Double xMax = Reader.ReadDouble();
            Double yMax = Reader.ReadDouble();

            numOfBytesRead = 8 * 4;

            return new Envelope(new Coordinate(xMin, yMin), new Coordinate(xMax, yMax));
        }
    }
}
