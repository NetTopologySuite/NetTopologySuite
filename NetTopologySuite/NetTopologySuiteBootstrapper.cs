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
            // this is a method with a special name in a class with a special name; we don't call it
            // directly. Instead, ModuleInit.Fody recognizes this and adds a module initializer that
            // calls this method when NTS is loaded, before anything else of ours can run.
            public static void Initialize() => GeometryServiceProvider.SetInstanceIfNotAlreadySetDirectly(NetTopologySuite.NtsGeometryServices.Instance);
        }
    }
}
