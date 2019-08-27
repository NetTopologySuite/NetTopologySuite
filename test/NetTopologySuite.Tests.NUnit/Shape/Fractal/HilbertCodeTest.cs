using NetTopologySuite.Shape.Fractal;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Shape.Fractal
{
    [TestFixture]
    public class HilbertCodeTest
    {
        [TestCase(0, 1)]
        [TestCase(1, 4)]
        [TestCase(2, 16)]
        [TestCase(3, 64)]
        [TestCase(4, 256)]
        [TestCase(5, 1024)]
        [TestCase(6, 4096)]
        public void TestSize(int level, int expectedSize)
        {
            Assert.That(HilbertCode.Size(level), Is.EqualTo(expectedSize));
        }

        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 2)]
        [TestCase(13, 2)]
        [TestCase(15, 2)]
        [TestCase(16, 2)]
        [TestCase(17, 3)]
        [TestCase(63, 3)]
        [TestCase(64, 3)]
        [TestCase(65, 4)]
        [TestCase(255, 4)]
        [TestCase(256, 4)]
        public void TestLevel(int size, int expectedLevel)
        {
            Assert.That(HilbertCode.Level(size), Is.EqualTo(expectedLevel));
        }

        [TestCase(1, 0, 0, 0)]
        [TestCase(1, 1, 0, 1)]
        [TestCase(3, 0, 0, 0)]
        [TestCase(3, 1, 0, 1)]
        [TestCase(4, 0, 0, 0)]
        [TestCase(4, 1, 1, 0)]
        [TestCase(4, 24, 6, 2)]
        [TestCase(4, 255, 15, 0)]
        [TestCase(5, 124, 8, 6)]
        public void TestDecode(int order, int index, int x, int y)
        {
            var p = HilbertCode.Decode(order, index);
            Assert.That(p.X, Is.EqualTo(x));
            Assert.That(p.Y, Is.EqualTo(y));
        }

        [TestCase(4)]
        [TestCase(5)]
        public void TestDecodeEncode(int level)
        {
            int n = HilbertCode.Size(level);
            for (int i = 0; i < n; i++)
            {
                var p = HilbertCode.Decode(level, i);
                int encode = HilbertCode.Encode(level, (int)p.X, (int)p.Y);
                Assert.That(encode, Is.EqualTo(i));
            }
        }
    }
}
