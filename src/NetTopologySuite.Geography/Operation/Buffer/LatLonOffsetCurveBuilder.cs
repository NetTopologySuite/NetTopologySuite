using NetTopologySuite.Geography.Lib;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;

namespace NetTopologySuite.Geography.Operation.Buffer
{
    public class LatLonOffsetCurveBuilder : OffsetCurveBuilder
    {
        private readonly Geodesic _geodesic;

        public LatLonOffsetCurveBuilder(Geodesic geodesic, 
            PrecisionModel precisionModel, BufferParameters bufParams)
            : base(precisionModel, bufParams)
        {
        }
         
    }
}
