using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    [Serializable]
    public class CoordinateStructSequence : CoordinateArraySequence
    {
        public CoordinateStructSequence(ICoordinate[] coordinates) : base(coordinates) {}

        public CoordinateStructSequence(Int32 size) : base(size) {}

        public CoordinateStructSequence(ICoordinateSequence coordSeq) : base(coordSeq) {}

        public override ICoordinate GetCoordinateCopy(Int32 i)
        {
            return new CoordinateStruct(coordinates[i]);
        }

        public override object Clone()
        {
            ICoordinate[] cloneCoordinates = GetClonedCoordinates();
            return new CoordinateStructSequence(cloneCoordinates);
        }
    }
}