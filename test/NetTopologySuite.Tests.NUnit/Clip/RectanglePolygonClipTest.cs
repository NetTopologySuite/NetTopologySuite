using System;
using NetTopologySuite.Clip;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Clip
{
    public class RectangleClipPolygonTest : GeometryTestCase
    {

        [Test]
        public void TestSimple()
        {
            CheckClip(
                "POLYGON ((250 250, 250 150, 150 150, 150 250, 250 250))",
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                "POLYGON ((150 200, 200 200, 200 150, 150 150, 150 200))"
            );
        }

        [Test]
        public void TestOutside()
        {
            CheckClip(
                "POLYGON ((250 250, 250 150, 150 150, 150 250, 250 250))",
                "POLYGON ((50 100, 100 100, 100 50, 50 50, 50 100))",
                "POLYGON EMPTY"
            );
        }

        [Test]
        public void TestMultiOneOutside()
        {
            CheckClip(
                "POLYGON ((250 250, 250 150, 150 150, 150 250, 250 250))",
                "MULTIPOLYGON (((50 100, 100 100, 100 50, 50 50, 50 100)), ((200 300, 300 300, 300 200, 200 200, 200 300)))",
                "POLYGON ((200 200, 200 250, 250 250, 250 200, 200 200))"
            );
        }

        private void CheckClip(string rectWKT, string inputWKT, string expectedWKT)
        {
            var rect = Read(rectWKT);
            var input = Read(inputWKT);
            var expected = Read(expectedWKT);

            var result = RectangleClipPolygon.Clip(input, rect);

            CheckEqual(expected, result);
        }
    }
}

