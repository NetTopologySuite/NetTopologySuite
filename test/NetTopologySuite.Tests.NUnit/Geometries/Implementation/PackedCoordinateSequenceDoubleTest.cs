#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class PackedCoordinateSequenceDoubleTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory { get => PackedCoordinateSequenceFactory.DoubleFactory; }
    }
}
