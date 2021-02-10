using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class PackedCoordinateSequenceDoubleTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory { get => PackedCoordinateSequenceFactory.DoubleFactory; }

        [Test]
        public void Test4dCoordinateSequence()
        {
            var cs = new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Double)
                .Create(new[] { 0.0d, 1.0d, 2.0d, 3.0d, 4.0d, 5.0d, 6.0d, 7.0d }, 4, 1);
            Assert.AreEqual(2.0, cs.GetOrdinate(0, Ordinate.Z));
            Assert.AreEqual(3.0, cs.GetOrdinate(0, Ordinate.M));
        }

    }
}
