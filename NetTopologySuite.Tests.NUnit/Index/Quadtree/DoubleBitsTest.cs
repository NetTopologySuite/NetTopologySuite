using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Quadtree
{
    [TestFixture]
    public class DoubleBitsTest
    {
        [Test]
        public void TestExponent()
        {
            Assert.IsTrue(DoubleBits.GetExponent(-1) == 0);
            Assert.IsTrue(DoubleBits.GetExponent(8.0) == 3);
            Assert.IsTrue(DoubleBits.GetExponent(128.0) == 7);
        }
    }
}