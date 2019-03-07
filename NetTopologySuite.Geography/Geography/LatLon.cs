using System;
using System.Globalization;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geography
{
    public struct LatLon 
    {
        /// <summary>
        /// Initializes this LatLon struct with the provided <paramref name="lat"/> and <paramref name="lon"/> values
        /// </summary>
        /// <param name="lat">A latitude</param>
        /// <param name="lon">A longitude</param>
        public LatLon(double lat, double lon)
        {
            if (lat < -90 || lat > 90)
                throw new ArgumentException("latitude outside of valid range", nameof(lat));
            if (lon < -180 || lon > 180)
                throw new ArgumentException("longitude outside of valid range", nameof(lon));

            Lat = lat;
            Lon = lon;
        }

        /// <summary>
        /// Initializes this LatLon struct with <see cref="Lat"/>=<see cref="Coordinate.Z"/> and
        /// <see cref="Lon"/>=<see cref="Coordinate.X"/> values of <paramref name="coordinate"/> values
        /// </summary>
        /// <param name="lat">A latitude</param>
        /// <param name="lon">A longitude</param>
        public LatLon(Coordinate coordinate)
            :this(coordinate.Y, coordinate.X)
        {
        }

        /// <summary>
        /// Gets a value indicating the latitude
        /// </summary>
        public double Lat { get; }

        /// <summary>
        /// Gets a value indicating the longitude
        /// </summary>
        public double Lon { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo,"LL({0:R},{1:R})", Lat, Lon);
        }

        /// <summary>
        /// Implict conversion operator to transform a <see cref="Coordinate"/> into a <see cref="LatLon"/>
        /// </summary>
        /// <param name="coordinate">A coordinate</param>
        public static implicit operator LatLon(Coordinate coordinate)
        {
            return new LatLon(coordinate);
        }

        /// <summary>
        /// Implict conversion operator to transform a <see cref="LatLon"/> into a <see cref="Coordinate"/>
        /// </summary>
        /// <param name="ll">A LatLon</param>
        public static implicit operator Coordinate(LatLon ll)
        {
            return new Coordinate(ll.Lon, ll.Lat);
        }
    }
}
