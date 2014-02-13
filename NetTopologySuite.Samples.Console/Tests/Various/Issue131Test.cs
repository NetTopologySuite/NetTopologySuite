using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue131Test
    {
        [Test, Category("Issue131")]
        public void TestNaNComparison()
        {
            const double d1 = double.NaN, d2 = double.NaN, d3 = 1;
            Assert.False(d1 == d2);
            Assert.False(d1 < d2);
            Assert.False(d1 > d2);
            Assert.False(d1 == d3);
            Assert.False(d1 < d3);
            Assert.False(d1 > d3);
            Assert.True(d1.CompareTo(d2) == 0);
            Assert.True(d1.CompareTo(d3) == -1);
            Assert.True(d3.CompareTo(d1) == 1);
        }
    }
}