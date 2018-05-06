using NetTopologySuite.IO;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.DotNetCore
{
    [TestFixture]
    public sealed class GeometryServiceProviderTests
    {
        [Test]
        public void InstanceShouldBeSetAutomatically() => Assert.NotNull(new WKBReader());
    }
}
