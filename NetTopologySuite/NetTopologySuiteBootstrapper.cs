namespace NetTopologySuite
{
    /// <summary>
    /// A utility class to register NTS as <see cref="GeometryServiceProvider.Instance"/>.
    /// </summary>
    public class NetTopologySuiteBootstrapper
    {
        /// <summary>
        /// Method to register <see cref="NtsGeometryServices"/> as the provider for
        /// <see cref="GeometryServiceProvider.Instance"/>.
        /// </summary>
        public static void Bootstrap()
        {
            GeometryServiceProvider.Instance = NtsGeometryServices.Instance;
        }
    }
}
