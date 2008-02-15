using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace ManagedBufferedCoordinate2DTests
{
    [TestFixture]
    public class BufferedCoordinate2DFactoryTests
    {
        [Test]
        public void CreateFactorySucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Assert.IsNotNull(factory);
        }

        [Test]
        public void CreateBufferedCoordinate2DSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(5, 10);
            Assert.AreEqual(5, coord.X);
            Assert.AreEqual(10, coord.Y);
        }
    }
}
