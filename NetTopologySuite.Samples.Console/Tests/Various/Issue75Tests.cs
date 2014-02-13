using GeoAPI.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    using Geometries;

    [TestFixture]
    internal class Issue75Tests
    {
        [Test, Category("Issue75")]
        public void EqualsThrowsInvalidCastExceptionBugFix()
        {
            Point point = new Point(1.0, 1.0);
            Coordinate coordinate = new Coordinate(-1.0, -1.0);
            bool condition = point.Equals(coordinate);
            Assert.IsFalse(condition);
        }
    }
}