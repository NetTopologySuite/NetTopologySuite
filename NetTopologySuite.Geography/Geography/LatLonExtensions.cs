using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geography
{
    public static class LatLonExtensions
    {
        public static LatLon GetLatLon(this ICoordinateSequence self, int index)
        {
            return (LatLon)self.GetCoordinate(index);
        }

        public static void SetLatLon(this ICoordinateSequence self, int index, LatLon ll)
        {
            self.SetOrdinate(index, Ordinate.Y, ll.Lat);
            self.SetOrdinate(index, Ordinate.Y, ll.Lon);
        }

        public static IEnumerable<LatLon> GetLatLons(this ICoordinateSequence self)
        {
            if (self == null)
                yield break;

            for (int i = 0; i < self.Count; i++)
                yield return self.GetLatLon(i);
        }

        public static ICoordinateSequence Create(this ICoordinateSequenceFactory self, IList<LatLon> lls)
        {
            var llList = new List<Coordinate>();
            foreach (var ll in lls)
                llList.Add(ll);
            return self.Create(llList.ToArray());
        }

        public static LatLon GetLatLon(this Coordinate self)
        {
            return self;
        }

        public static LatLon GetLatLon(this IGeometry self)
        {
            return self.Coordinate;
        }

        public static IPoint CreateGeoPoint(this IGeometryFactory self, LatLon ll)
        {
            if (self.SRID != 4326)
                throw new ArgumentException("Factory has invalid SRID", nameof(self));

            return self.CreatePoint(self.CoordinateSequenceFactory.Create(new[] { ll }));
        }

        public static ILineString CreateGeoLineString(this IGeometryFactory self, IList<LatLon> lls)
        {
            if (self.SRID != 4326)
                throw new ArgumentException("Factory has invalid SRID", nameof(self));

            return self.CreateLineString(self.CoordinateSequenceFactory.Create(lls));
        }

        public static IPolygon CreateGeoPolygon(this IGeometryFactory self, IList<LatLon> lls)
        {
            if (self.SRID != 4326)
                throw new ArgumentException("Factory has invalid SRID", nameof(self));

            var ring = self.CreateLinearRing(self.CoordinateSequenceFactory.Create(lls));
            return self.CreatePolygon(ring, null);
        }

        public static double GetLength(this IEnumerable<LatLon> self)
        {
            double res = 0;
            var last = new LatLon();
            int i = 0;
            foreach(var ll in self)
            {
                if (i > 0)
                    res += LatLonDistance.Distance(last, ll);
                last = ll;
                i++;
            }
            return res;
        }
    }
}
