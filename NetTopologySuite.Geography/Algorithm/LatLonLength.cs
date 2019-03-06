using GeoAPI.Geometries;
using NetTopologySuite.Geography;

namespace NetTopologySuite.Algorithm
{
    public class LatLonLength
    {
        public static double OfLine(ICoordinateSequence sequence)
        {
            double length = 0;
            var lls = sequence.ToLatLonArray();
            for (int i = 1; i < sequence.Count; i++)
                length += LatLonDistance.Distance(lls[i-1], lls[i]);

            return length;
        }
    }
}
