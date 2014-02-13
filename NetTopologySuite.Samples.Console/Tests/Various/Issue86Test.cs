using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue86Test
    {
        [Test, Category("Issue86")]
        [Ignore("Different conversion methods produce different lengths")]
        public void Test()
        {
            var x = LonLat_to_Merc(new Coordinate(37.8686517, 47.94573));
            var y = LonLat_to_Merc(new Coordinate(37.8686533, 47.9457283));
            var x1 = new Coordinate(37.8686517, 47.94573);
            var y1 = new Coordinate(37.8686533, 47.9457283);
            LineString s = new LineString(new Coordinate[] { x, y });
            Console.WriteLine(s.Length);
            double f = GetDistance(x1, y1);
            Console.WriteLine(f);
            Assert.AreEqual(s.Length, f,0.01);
        }
        private double GetDistance(Coordinate x, Coordinate y)
        {
            double lat1 = x.X * Math.PI / 180;
            double lat2 = y.X * Math.PI / 180;
            double deltaLat = Math.Abs(lat2 - lat1) * Math.PI / 180; ;
            double deltaLon = Math.Abs(y.Y - x.Y) * Math.PI / 180; ;
            double R = 6371000.0;

            double a = Math.Pow(Math.Sin(deltaLat / 2), 2.0) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow((Math.Sin(deltaLon / 2)), 2.0);
            double b = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * b;
        }
        private Coordinate LonLat_to_Merc(Coordinate lonlat)
        {
            double a = 6378137.00;
            double b = 6356752.31424517930;
            double e = Math.Sqrt(1 - Math.Pow(b / a, 2.0));

            lonlat.X = a * (lonlat.X * Math.PI / 180);
            lonlat.Y = a * Math.Log(Math.Tan(Math.PI / 4 + (lonlat.Y * Math.PI / 180) / 2) * Math.Pow((1 - e * Math.Sin((lonlat.Y * Math.PI / 180))) / (1 + e * Math.Sin((lonlat.Y * Math.PI / 180))), e / 2));
            return lonlat;
        }
    }
}