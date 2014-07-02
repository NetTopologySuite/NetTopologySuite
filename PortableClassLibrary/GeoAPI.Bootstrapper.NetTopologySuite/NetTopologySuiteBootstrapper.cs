namespace GeoAPI
{
    /// <summary>
    /// A utility class to register NTS as GeoAPI implementation to use
    /// </summary>
    public class NetTopologySuiteBootstrapper
    {
        /// <summary>
        /// Method to register NTS as GeoAPI implementation to use.
        /// </summary>
        public static void Bootstrap()
        {
            GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }
    }
}
