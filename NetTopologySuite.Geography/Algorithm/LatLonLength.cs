using GeoAPI.Geometries;
using NetTopologySuite.Geography;

namespace NetTopologySuite.Algorithm
{
    public class LatLonLength
    {
        public static double OfLine(ICoordinateSequence sequence)
        {
            double length = 0;
            var coordinates = sequence.ToCoordinateArray();
            for (int i = 1; i < coordinates.Length; i++)
                length += LatLonDistance.Distance(coordinates[i-1], coordinates[i]);

            return length;
        }
    }
}
