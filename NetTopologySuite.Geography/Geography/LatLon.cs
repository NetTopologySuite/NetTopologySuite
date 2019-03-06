using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geography
{
    public struct LatLon 
    {
        public LatLon(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public LatLon(Coordinate c)
        {
            Lon = c.X;
            Lat = c.Y;
        }

        public double Lat { get; }

        public double Lon { get; }

        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo,"LL({0:R},{1:R})", Lat, Lon);
        }

        public static implicit operator LatLon(Coordinate c)
        {
            return new LatLon(c);
        }

        public static implicit operator Coordinate(LatLon ll)
        {
            return new Coordinate(ll.Lon, ll.Lat);
        }


        public static ICoordinateSequence CreateSequence(IEnumerable<LatLon> lls)
        {
            return null;
        }
    }

    public static class LatLonExtensions
    {
        public static LatLon[] ToLatLonArray(this ICoordinateSequence sequence)
        {
            if (sequence == null || sequence.Count == 0)
                return new LatLon[0];

            var res = new LatLon[sequence.Count];
            for (int i = 0; i < sequence.Count; i++)
                res[i] = sequence.GetCoordinate(i);

            return res;
        }
    }
}
