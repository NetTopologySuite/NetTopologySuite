using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue68Tests
    {
        [Test, Category("Issue68")]
        public void EqualsNullThrowsBugFix()
        {
            var polygon = new Polygon(null);
            bool result = polygon.EqualsTopologically(null);
            Assert.IsNotNull(result);
        }
    }
}