#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Precision
{
    public class CommonBitsOpTest : GeometryTestCase
    {
        /// <summary>
        /// Tests an issue where CommonBitsRemover was not persisting changes to some kinds of CoordinateSequences
        /// </summary>
        [Test]
        public void TestPackedCoordinateSequence() {
            var pcsFactory = new GeometryFactory(PackedCoordinateSequenceFactory.DoubleFactory);
            var geom0 = Read(pcsFactory, "POLYGON ((210 210, 210 220, 220 220, 220 210, 210 210))");
            var geom1 = Read("POLYGON ((225 225, 225 215, 215 215, 215 225, 225 225))");
            var cbo = new CommonBitsOp(true);
            var result = cbo.Intersection(geom0, geom1);
            var expected = geom0.Intersection(geom1);
            //Geometry expected = read("POLYGON ((220 215, 215 215, 215 220, 220 220, 220 215))");
            CheckEqual(expected, result);
        }
    }
}