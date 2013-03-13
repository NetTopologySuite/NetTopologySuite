using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class GeometriesFixture
    {
        [Test]
        public void get_interior_point_of_empty_point()
        {
            ICoordinateSequence sequence = CoordinateArraySequenceFactory.Instance.Create(0, Ordinates.XY);
            IGeometryFactory factory = GeometryFactory.Default;
            IGeometry empty = factory.CreatePoint(sequence);
            Assert.That(empty, Is.Not.Null);
            Assert.That(empty.IsValid, Is.True);
            Assert.That(empty.IsEmpty, Is.True);

            IPoint interior = empty.InteriorPoint;
            Assert.That(interior, Is.Not.Null);
            Assert.That(interior.IsValid, Is.True);
            Assert.That(interior.IsEmpty, Is.True);            
        }
    }
}
