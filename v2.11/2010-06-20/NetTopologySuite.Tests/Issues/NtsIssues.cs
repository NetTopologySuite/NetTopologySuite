using System;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GeoAPI.Operations.Buffer;
using NUnit.Framework;

#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif
namespace NetTopologySuite.Tests.Issues
{
    [TestFixture]
    public class NtsIssues
    {
        private static readonly IWktGeometryReader<coord> Reader = TestFactories.GeometryFactory.WktReader;

        [Test]
        public void IssueSliceGetOverlappingTriplesWithReversedCoordinateSequence()
        {
            var linestring = Reader.Read("LINESTRING (10 10, 20 20, 30 30, 40 40, 50 50)");
            var seq = linestring.Coordinates;
            var seqReversed = seq.Reversed;
            Console.WriteLine("\nReversed");
            foreach (var overlappingTriple in Slice.GetOverlappingTriples(seqReversed))
                Console.WriteLine(overlappingTriple.ToString());
            Console.WriteLine("\nOriginal");
            foreach (var overlappingTriple in Slice.GetOverlappingTriples(seq))
                Console.WriteLine(overlappingTriple.ToString());
        }


        public void Issue54()
        {
            IGeometry<coord> g =
                Reader.Read(
                    "POLYGON((906.4827 217.8143,927.6762 0.0099999999999909051,36.486899999999991 0.0099999999999909051,0.0099999999999909051 374.8819,906.4827 217.8143))");
            IGeometry<coord> buffer = g.Buffer(2.0d, 4, GeoAPI.Operations.Buffer.BufferStyle.Round);
            Console.WriteLine(buffer.ToString());
            Assert.AreEqual(buffer.GeometryType, OgcGeometryType.Polygon);
        }

        [Test]
        public void IssueKishoreViaGoogleGroups()
        {
            IPolygon<coord> poly = (IPolygon<coord>)Reader.Read("POLYGON((5 5, 95 5, 95 95, 5 95, 5 5))");
            BufferParameters bp = new BufferParameters(
                1, BufferParameters.BufferEndCapStyle.CapSquare, BufferParameters.BufferJoinStyle.JoinMitre, 5);
            IGeometry<coord> geom = GisSharpBlog.NetTopologySuite.Operation.Buffer.BufferOp_110<coord>.Buffer(poly, 5, bp);
            System.Diagnostics.Debug.WriteLine(geom.ToString());
            IPolygon<coord> result =
                (IPolygon<coord>) Reader.Read("POLYGON((0 0, 100 0, 100 100, 0 100, 0 0))");
            geom.Normalize();
            result.Normalize();

            IPolygon<coord> geom2 = (IPolygon<coord>)poly.Buffer(5, bp);
            geom2.Normalize();

            Assert.IsTrue(geom.Equals(result));
            Assert.IsTrue(geom2.Equals(result));
        }
 
    }
}
