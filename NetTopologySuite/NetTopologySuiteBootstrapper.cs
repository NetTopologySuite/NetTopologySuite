namespace NetTopologySuite
{
    /// <summary>
    /// A utility class to register NTS as <see cref="GeometryServiceProvider.Instance"/>.
    /// </summary>
    public class NetTopologySuiteBootstrapper
    {
        /// <summary>
        /// Method to register NTS as GeoAPI implementation to use.
        /// </summary>
        public static void Bootstrap()
        {
            GeometryServiceProvider.Instance = NtsGeometryServices.Instance;
        }
    }
}
