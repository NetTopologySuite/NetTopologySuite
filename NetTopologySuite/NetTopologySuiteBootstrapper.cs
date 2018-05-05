namespace GeoAPI
{
    /// <summary>
    /// A utility class to register NTS as GeoAPI implementation to use
    /// </summary>
    /// <remarks>
    /// Only necessary on platforms where GeoAPI cannot use reflection to do
    /// this automatically.
    /// </remarks>
    public class NetTopologySuiteBootstrapper
    {
        /// <summary>
        /// Method to register NTS as GeoAPI implementation to use.
        /// </summary>
        public static void Bootstrap()
        {
            GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }

        internal static class ModuleInitializer
        {
            public static void Initialize() => GeometryServiceProvider.SetInstanceIfNotAlreadySetDirectly(NetTopologySuite.NtsGeometryServices.Instance);
        }
    }
}
