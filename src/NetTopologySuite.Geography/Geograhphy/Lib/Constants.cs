namespace NetTopologySuite.Geography.Lib
{
    /**
     * Constants needed by GeographicLib.
     * <p>
     * Define constants specifying the WGS84 ellipsoid.
     ***********************************************************************/
    public static class Constants
    {
        /**
         * The equatorial radius of WGS84 ellipsoid (6378137 m).
         **********************************************************************/
        public const double WGS84_a = 6378137;
        /**
         * The flattening of WGS84 ellipsoid (1/298.257223563).
         **********************************************************************/
        public const double WGS84_f = 1 / 298.257223563;
    }

}
