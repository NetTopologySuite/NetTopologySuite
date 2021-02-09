namespace NetTopologySuite.Geography.Lib
{
    /**
     * Bit masks for what geodesic calculations to do.
     * <p>
     * These masks do double duty.  They specify (via the <i>outmask</i> parameter)
     * which results to return in the {@link GeodesicData} object returned by the
     * general routines {@link Geodesic#Direct(double, double, double, double, int)
     * Geodesic.Direct} and {@link Geodesic#Inverse(double, double, double, double,
     * int) Geodesic.Inverse} routines.  They also signify (via the <i>caps</i>
     * parameter) to the {@link GeodesicLine#GeodesicLine(Geodesic, double, double,
     * double, int) GeodesicLine.GeodesicLine} constructor and to {@link
     * Geodesic#Line(double, double, double, int) Geodesic.Line} what capabilities
     * should be included in the {@link GeodesicLine} object.
     **********************************************************************/
    internal class GeodesicMask
    {
        private const int CAP_NONE = 0;
        internal const int CAP_C1 = 1 << 0;
        internal const int CAP_C1p = 1 << 1;
        internal const int CAP_C2 = 1 << 2;
        internal const int CAP_C3 = 1 << 3;
        internal const int CAP_C4 = 1 << 4;
        private const int CAP_ALL = 0x1F;
        internal const int CAP_MASK = CAP_ALL;
        internal const int OUT_ALL = 0x7F80;
        internal const int OUT_MASK = 0xFF80; // Include LONG_UNROLL

        /**
         * No capabilities, no output.
         **********************************************************************/
        public const int NONE = 0;
        /**
         * Calculate latitude <i>lat2</i>.  (It's not necessary to include this as a
         * capability to {@link GeodesicLine} because this is included by default.)
         **********************************************************************/
        public const int LATITUDE = 1 << 7 | CAP_NONE;
        /**
         * Calculate longitude <i>lon2</i>.
         **********************************************************************/
        public const int LONGITUDE = 1 << 8 | CAP_C3;
        /**
         * Calculate azimuths <i>azi1</i> and <i>azi2</i>.  (It's not necessary to
         * include this as a capability to {@link GeodesicLine} because this is
         * included by default.)
         **********************************************************************/
        public const int AZIMUTH = 1 << 9 | CAP_NONE;
        /**
         * Calculate distance <i>s12</i>.
         **********************************************************************/
        public const int DISTANCE = 1 << 10 | CAP_C1;
        /**
         * All of the above, the "standard" output and capabilities.
         **********************************************************************/
        public const int STANDARD = LATITUDE | LONGITUDE |
                                    AZIMUTH | DISTANCE;
        /**
         * Allow distance <i>s12</i> to be used as <i>input</i> in the direct
         * geodesic problem.
         **********************************************************************/
        public const int DISTANCE_IN = 1 << 11 | CAP_C1 | CAP_C1p;
        /**
         * Calculate reduced length <i>m12</i>.
         **********************************************************************/
        public const int REDUCEDLENGTH = 1 << 12 | CAP_C1 | CAP_C2;
        /**
         * Calculate geodesic scales <i>M12</i> and <i>M21</i>.
         **********************************************************************/
        public const int GEODESICSCALE = 1 << 13 | CAP_C1 | CAP_C2;
        /**
         * Calculate area <i>S12</i>.
         **********************************************************************/
        public const int AREA = 1 << 14 | CAP_C4;
        /**
         * All capabilities, calculate everything.  (LONG_UNROLL is not included in
         * this mask.)
         **********************************************************************/
        public const int ALL = OUT_ALL | CAP_ALL;
        /**
         * Unroll <i>lon2</i>.
         **********************************************************************/
        public const int LONG_UNROLL = 1 << 15;
    }
}
