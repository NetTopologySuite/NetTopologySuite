using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class PackedCoordinateSequenceFloatTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory { get => PackedCoordinateSequenceFactory.FloatFactory; }
    }
}
