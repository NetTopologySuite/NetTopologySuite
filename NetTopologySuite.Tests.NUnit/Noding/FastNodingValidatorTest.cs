using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    [TestFixture]
    public class FastNodingValidatorTest : GeometryTestCase
    {

        private static readonly string[] VERTEX_INT = new string[] {
            "LINESTRING (100 100, 200 200, 300 300)"
                ,"LINESTRING (100 300, 200 200)"
        };
        private static readonly string[] INTERIOR_INT = new string[] {
            "LINESTRING (100 100, 300 300)"
                ,"LINESTRING (100 300, 300 100)"
        };
        private static readonly string[] NO_INT = new string[] {
            "LINESTRING (100 100, 200 200)"
                ,"LINESTRING (200 200, 300 300)"
                ,"LINESTRING (100 300, 200 200)"
        };
        private static readonly string[] SELF_INTERIOR_INT = new string[] {
            "LINESTRING (100 100, 300 300, 300 100, 100 300)"
        };
        private static readonly string[] SELF_VERTEX_INT = new string[] {
            "LINESTRING (100 100, 200 200, 300 300, 400 200, 200 200)"
        };

        [Test]
        public void TestInteriorIntersection()
        {
            CheckValid(INTERIOR_INT, false);
            CheckIntersection(INTERIOR_INT, "POINT(200 200)");
        }

        [Test]
        public void TestVertexIntersection()
        {
            CheckValid(VERTEX_INT, false);
            //checkIntersection(VERTEX_INT, "POINT(200 200)");
        }

        [Test]
        public void TestNoIntersection()
        {
            CheckValid(NO_INT, true);
        }

        [Test]
        public void TestSelfInteriorIntersection()
        {
            CheckValid(SELF_INTERIOR_INT, false);
        }

        [Test]
        public void TestSelfVertexIntersection()
        {
            CheckValid(SELF_VERTEX_INT, false);
        }

        private void CheckValid(string[] inputWKT, bool isValidExpected)
        {
            var input = ReadList(inputWKT);
            var segStrings = ToSegmentStrings(input);
            var fnv = new FastNodingValidator(segStrings);
            bool isValid = fnv.IsValid;

            Assert.IsTrue(isValidExpected == isValid);
        }

        private void CheckIntersection(string[] inputWKT, string expectedWKT)
        {
            var input = ReadList(inputWKT);
            var expected = Read(expectedWKT);
            var pts = expected.Coordinates;
            var intPtsExpected = new CoordinateList(pts);

            var segStrings = ToSegmentStrings(input);
            var intPtsActual = FastNodingValidator.ComputeIntersections(segStrings);

            bool isSameNumberOfIntersections = intPtsExpected.Count == intPtsActual.Count;
            Assert.True(isSameNumberOfIntersections);

            CheckIntersections(intPtsActual, intPtsExpected);
        }

        private void CheckIntersections(IList<Coordinate> intPtsActual, IList<Coordinate> intPtsExpected)
        {
            //TODO: sort intersections so they can be compared
            for (int i = 0; i < intPtsActual.Count; i++)
            {
                var ptActual = intPtsActual[i];
                var ptExpected = intPtsExpected[i];

                bool isEqual = ptActual.Equals2D(ptExpected);
                Assert.IsTrue(isEqual);
            }
        }

        private static List<ISegmentString> ToSegmentStrings(IEnumerable<IGeometry> geoms)
        {
            var segStrings = new List<ISegmentString>();
            foreach (var geom in geoms) {
                segStrings.AddRange(SegmentStringUtil.ExtractSegmentStrings(geom));
            }
            return segStrings;
        }

    }
}
