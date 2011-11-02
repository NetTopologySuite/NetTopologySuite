using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class CoordinateArraySequenceTest : CoordinateSequenceTestBase
    {
        public CoordinateArraySequenceTest()
        {
            base.csFactory = CoordinateArraySequenceFactory.Instance;
        }
    }
}