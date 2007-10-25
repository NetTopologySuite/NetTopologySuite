using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    [Serializable]
    public sealed class CoordinateStructSequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly CoordinateStructSequenceFactory instance = new CoordinateStructSequenceFactory();

        private CoordinateStructSequenceFactory() { }

        public static CoordinateStructSequenceFactory Instance
        {
            get
            {
                return instance;
            }
        }

        public ICoordinateSequence Create(ICoordinate[] coordinates) 
        {
            return new CoordinateStructSequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq) 
        {
            return new CoordinateStructSequence(coordSeq);
        }

        public ICoordinateSequence Create(int size, int dimension) 
        {
            return new CoordinateStructSequence(size);
        }
    }
}
