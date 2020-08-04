using NetTopologySuite.Shape.Fractal;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Shape.Fractal
{
    [TestFixture]
    public class MortonCodeTest
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
            Assert.That(MortonCode.Size(level), Is.EqualTo(expectedSize));
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
            Assert.That(MortonCode.Level(size), Is.EqualTo(expectedLevel));
        }

        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 0)]
        [TestCase(2, 0, 1)]
        [TestCase(3, 1, 1)]
        [TestCase(4, 2, 0)]
        [TestCase(24, 4, 2)]
        [TestCase(124, 14, 6)]
        [TestCase(255, 15, 15)]
        public void TestDecode(int index, int x, int y)
        {
            var p = MortonCode.Decode(index);
            Assert.That(p.X, Is.EqualTo(x));
            Assert.That(p.Y, Is.EqualTo(y));
        }

        [TestCase(4)]
        [TestCase(5)]
        public void TestDecodeEncode(int level)
        {
            int n = MortonCode.Size(level);
            for (int i = 0; i < n; i++)
            {
                var p = MortonCode.Decode(i);
                int encode = MortonCode.Encode((int)p.X, (int)p.Y);
                Assert.That(encode, Is.EqualTo(i));
            }
        }
    }
}
