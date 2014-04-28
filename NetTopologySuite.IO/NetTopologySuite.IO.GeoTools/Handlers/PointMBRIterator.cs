using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    internal class PointMBRIterator : ShapeMBREnumeratorBase
    {
        public PointMBRIterator(BigEndianBinaryReader reader)
            : base(reader)
        { }

        protected override Envelope ReadCurrentEnvelope(out int numOfBytesRead)
        {
            double x = Reader.ReadDouble();
            double y = Reader.ReadDouble();

            numOfBytesRead = 16;

            return new Envelope(new Coordinate(x, y));
        }
    }
}
