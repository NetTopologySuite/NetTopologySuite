using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class CoordinateArraySequenceTest : CoordinateSequenceTestBase
    {
        protected override ICoordinateSequenceFactory CsFactory
        {
            get { return CoordinateArraySequenceFactory.Instance; }
        }
    }
}