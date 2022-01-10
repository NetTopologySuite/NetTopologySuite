using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    /**
     * Tests for the effect of buffer parameter values.
     * 
     * @author Martin Davis
     *
     */
    public class BufferParameterTest : GeometryTestCase
    {

        [Test]
        public void TestQuadSegsNeg()
        {
            CheckBuffer("LINESTRING (20 20, 80 20, 80 80)",
                10.0, -99,
                "POLYGON ((70 30, 70 80, 80 90, 90 80, 90 20, 80 10, 20 10, 10 20, 20 30, 70 30))");
        }

        [Test]
        public void TestQuadSegs0()
        {
            CheckBuffer("LINESTRING (20 20, 80 20, 80 80)",
                10.0, 0,
                "POLYGON ((70 30, 70 80, 80 90, 90 80, 90 20, 80 10, 20 10, 10 20, 20 30, 70 30))");
        }

        [Test]
        public void TestQuadSegs1()
        {
            CheckBuffer("LINESTRING (20 20, 80 20, 80 80)",
                10.0, 1,
                "POLYGON ((70 30, 70 80, 80 90, 90 80, 90 20, 80 10, 20 10, 10 20, 20 30, 70 30))");
        }

        [Test]
        public void TestQuadSegs2()
        {
            CheckBuffer("LINESTRING (20 20, 80 20, 80 80)",
                10.0, 2,
                "POLYGON ((70 30, 70 80, 72.92893218813452 87.07106781186548, 80 90, 87.07106781186548 87.07106781186548, 90 80, 90 20, 87.07106781186548 12.928932188134524, 80 10, 20 10, 12.928932188134523 12.928932188134524, 10 20, 12.928932188134524 27.071067811865476, 20 30, 70 30))");
        }

        [Test]
        public void TestQuadSegs2Bevel()
        {
            CheckBuffer("LINESTRING (20 20, 80 20, 80 80)",
                10.0, 2, NetTopologySuite.Operation.Buffer.JoinStyle.Bevel,
                "POLYGON ((70 30, 70 80, 72.92893218813452 87.07106781186548, 80 90, 87.07106781186548 87.07106781186548, 90 80, 90 20, 80 10, 20 10, 12.928932188134523 12.928932188134524, 10 20, 12.928932188134524 27.071067811865476, 20 30, 70 30))");
        }


        private void CheckBuffer(string wkt, double dist, int quadSegs, string wktExpected)
        {
            CheckBuffer(wkt, dist, quadSegs, JoinStyle.Round, wktExpected);
        }

        private void CheckBuffer(string wkt, double dist, int quadSegs, JoinStyle joinStyle, string wktExpected)
        {
            var param = new BufferParameters
            {
                QuadrantSegments = quadSegs,
                JoinStyle = joinStyle
            };
            var geom = Read(wkt);
            var result = BufferOp.Buffer(geom, dist, param);
            var expected = Read(wktExpected);
            CheckEqual(expected, result);
        }
    }

}
