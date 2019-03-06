using System;
using NetTopologySuite.Geography;

namespace NetTopologySuite.Algorithm
{
    public static class LatLonDistance
    {
        /// <summary>
        /// Conversion factor degrees to radians
        /// </summary>
        const double DegToRad = Math.PI / 180d; //0.01745329252; // Convert Degrees to Radians

        /// <summary>
        /// Meters per inch
        /// </summary>
        const double MetersPerInch = 0.0254;

        /// <summary>
        /// Meters per mile
        /// </summary>
        const double MetersPerMile = 1609.347219;

        /// <summary>
        /// Miles per degree at equator
        /// </summary>
        const double MilesPerDegreeAtEquator = 69.171;

        /// <summary>
        /// Meters per degree at equator
        /// </summary>
        const double MetersPerDegreeAtEquator = MetersPerMile * MilesPerDegreeAtEquator;

        /// <summary>
        /// Calculate the distance between 2 points on the great circle
        /// </summary>
        /// <param name="ll1">Lat/lon of the 1st point</param>
        /// <param name="ll2">Lat/lon of the 2nd point</param>
        /// <returns>The distance in meters</returns>
        public static double Distance(LatLon ll1, LatLon ll2)
        {
            double lonDistance = DiffLongitude(ll1.Lon, ll2.Lon);
            double arg1 = Math.Sin(ll1.Lat * DegToRad) * Math.Sin(ll2.Lat * DegToRad);
            double arg2 = Math.Cos(ll1.Lat * DegToRad) * Math.Cos(ll2.Lat * DegToRad) * Math.Cos(lonDistance * DegToRad);

            return MetersPerDegreeAtEquator * Math.Acos(arg1 + arg2) / DegToRad;
        }

        /// <summary>
        /// Calculate the difference between two longitudinal values
        /// </summary>
        /// <param name="lon1">The first longitude value in degrees</param>
        /// <param name="lon2">The second longitude value in degrees</param>
        /// <returns>The distance in degrees</returns>
        private static double DiffLongitude(double lon1, double lon2)
        {
            double diff;

            if (lon1 > 180.0)
                lon1 = 360.0 - lon1;
            if (lon2 > 180.0)
                lon2 = 360.0 - lon2;

            if ((lon1 >= 0.0) && (lon2 >= 0.0))
                diff = lon2 - lon1;
            else if ((lon1 < 0.0) && (lon2 < 0.0))
                diff = lon2 - lon1;
            else
            {
                // different hemispheres
                if (lon1 < 0)
                    lon1 = -1 * lon1;
                if (lon2 < 0)
                    lon2 = -1 * lon2;
                diff = lon1 + lon2;
                if (diff > 180.0)
                    diff = 360.0 - diff;
            }

            return diff;
        }
    }
}

