using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    [SetUpFixture]
    public class PclSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            GeoAPI.NetTopologySuiteBootstrapper.Bootstrap();
        }
    }
}