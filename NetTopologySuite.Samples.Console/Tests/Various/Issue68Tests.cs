using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue68Tests
    {
        [Test]
        public void EqualsNullThrowsBugFix()
        {
            var polygon = new Polygon(null);
            var result = polygon.Equals((IGeometry)null);
            Assert.IsNotNull(result);
        }
    }
}