using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class PackedCoordinateSequenceFloatTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory { get => PackedCoordinateSequenceFactory.FloatFactory; }

        [Test]
        public void Test4dCoordinateSequence()
        {
            var cs = new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Float)
                .Create(new [] { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f }, 4);
            Assert.AreEqual(2.0, cs.GetOrdinate(0, Ordinate.Z));
            Assert.AreEqual(3.0, cs.GetOrdinate(0, Ordinate.M));
        }
    }
}
