namespace GeoAPI
{
    public class NetTopologySuiteBootstrapper
    {
        /// <summary>
        /// ToDo: 
        /// - Rethink method name (maybe Configure?)
        /// - Some sort of configuration?
        /// </summary>
        public static void Bootstrap()
        {
            GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
        }
    }
}
