using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    [SetUpFixture]
    public class PclSetup
    {
        [SetUp]
        public void SetUp()
        {
            GeoAPI.NetTopologySuiteBootstrapper.Bootstrap();
        }
    }
}