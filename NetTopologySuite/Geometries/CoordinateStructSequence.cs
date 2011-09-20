using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
//#if!SILVERLIGHT
    [Serializable]
//#endif
    public class CoordinateStructSequence : CoordinateArraySequence 
    {
        public CoordinateStructSequence(Coordinate[] coordinates) : base(coordinates) { }
        
        public CoordinateStructSequence(int size) : base(size) { }

        public CoordinateStructSequence(ICoordinateSequence coordSeq) : base(coordSeq) { }

        public override Coordinate GetCoordinateCopy(int i)
        {
            return new Coordinate(coordinates[i]);
        }

        public override object Clone() 
        {
            Coordinate[] cloneCoordinates = GetClonedCoordinates();
            return new CoordinateStructSequence(cloneCoordinates);
        }
    }
}
