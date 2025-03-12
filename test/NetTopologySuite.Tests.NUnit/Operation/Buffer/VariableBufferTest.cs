﻿using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    public class VariableBufferTest : GeometryTestCase
    {
        private const double DEFAULT_TOLERANCE = 1.0e-2;

        [Test]
        public void TestZeroWidth()
        {
            CheckBuffer("LINESTRING( 0 0, 6 6, 10 10)",
                0, 0,
                "POLYGON EMPTY");
        }

        [Test]
        public void TestZeroLength()
        {
            CheckBuffer("LINESTRING( 10 10, 10 10 )",
                0, 0,
                "POLYGON EMPTY");
        }

        [Test]
        public void TestSegmentInverseDist()
        {
            CheckBuffer("LINESTRING (100 100, 200 100)",
                10, 1,
                "POLYGON ((100 90, 98.05 90.19, 96.17 90.76, 94.44 91.69, 92.93 92.93, 91.69 94.44, 90.76 96.17, 90.19 98.05, 90 100, 90.19 101.95, 90.76 103.83, 91.69 105.56, 92.93 107.07, 94.44 108.31, 96.17 109.24, 98.05 109.81, 100 110, 100.9 109.96, 200.09 101, 200.2 100.98, 200.38 100.92, 200.56 100.83, 200.71 100.71, 200.83 100.56, 200.92 100.38, 200.98 100.2, 201 100, 200.98 99.8, 200.92 99.62, 200.83 99.44, 200.71 99.29, 200.56 99.17, 200.38 99.08, 200.2 99.02, 200.09 99, 100.9 90.04, 100 90))"
            );
        }

        [Test]
        public void TestSegmentSameDist()
        {
            CheckBuffer("LINESTRING (100 100, 200 100)",
                10, 10,
                "POLYGON ((201.95 109.81, 203.83 109.24, 205.56 108.31, 207.07 107.07, 208.31 105.56, 209.24 103.83, 209.81 101.95, 210 100, 209.81 98.05, 209.24 96.17, 208.31 94.44, 207.07 92.93, 205.56 91.69, 203.83 90.76, 201.95 90.19, 200 90, 100 90, 98.05 90.19, 96.17 90.76, 94.44 91.69, 92.93 92.93, 91.69 94.44, 90.76 96.17, 90.19 98.05, 90 100, 90.19 101.95, 90.76 103.83, 91.69 105.56, 92.93 107.07, 94.44 108.31, 96.17 109.24, 98.05 109.81, 100 110, 200 110, 201.95 109.81))"
            );
        }

        [Test]
        public void TestOneSegment()
        {
            CheckBuffer("LINESTRING (100 100, 200 100)",
                10, 30,
                "POLYGON ((200 130, 205.85 129.42, 211.48 127.72, 216.67 124.94, 221.21 121.21, 224.94 116.67, 227.72 111.48, 229.42 105.85, 230 100, 229.42 94.15, 227.72 88.52, 224.94 83.33, 221.21 78.79, 216.67 75.06, 211.48 72.28, 205.85 70.58, 200 70, 194 70.61, 98 90.2, 96.17 90.76, 94.44 91.69, 92.93 92.93, 91.69 94.44, 90.76 96.17, 90.19 98.05, 90 100, 90.19 101.95, 90.76 103.83, 91.69 105.56, 92.93 107.07, 94.44 108.31, 96.17 109.24, 98 109.8, 194 129.39, 200 130))"
            );
        }

        [Test]
        public void TestSegments2()
        {
            CheckBuffer("LINESTRING( 0 0, 40 40, 60 -20)",
                10, 20,
                "POLYGON ((79.62 -16.1, 80 -20, 79.62 -23.9, 78.48 -27.65, 76.63 -31.11, 74.14 -34.14, 71.11 -36.63, 67.65 -38.48, 63.9 -39.62, 60 -40, 56.1 -39.62, 52.35 -38.48, 48.89 -36.63, 45.86 -34.14, 43.37 -31.11, 41.52 -27.65, 40.56 -24.72, 31.31 13.38, 6.46 -7.64, 5.56 -8.31, 3.83 -9.24, 1.95 -9.81, 0 -10, -1.95 -9.81, -3.83 -9.24, -5.56 -8.31, -7.07 -7.07, -8.31 -5.56, -9.24 -3.83, -9.81 -1.95, -10 0, -9.81 1.95, -9.24 3.83, -8.31 5.56, -7.64 6.46, 28.76 49.5, 29.59 50.41, 31.82 52.24, 34.37 53.6, 37.13 54.44, 40 54.72, 42.87 54.44, 45.63 53.6, 48.18 52.24, 50.41 50.41, 52.24 48.18, 53.53 45.8, 78.38 -12.11, 79.62 -16.1))"
            );
        }

        [Test]
        public void TestLargeDistance()
        {
            CheckBuffer("LINESTRING( 0 0, 10 10)",
                1, 200,
                "POLYGON ((206.16 -29.02, 194.78 -66.54, 176.29 -101.11, 151.42 -131.42, 121.11 -156.29, 86.54 -174.78, 49.02 -186.16, 10 -190, -29.02 -186.16, -66.54 -174.78, -101.11 -156.29, -131.42 -131.42, -156.29 -101.11, -174.78 -66.54, -186.16 -29.02, -190 10, -186.16 49.02, -174.78 86.54, -156.29 121.11, -131.42 151.42, -101.11 176.29, -66.54 194.78, -29.02 206.16, 10 210, 49.02 206.16, 86.54 194.78, 121.11 176.29, 151.42 151.42, 176.29 121.11, 194.78 86.54, 206.16 49.02, 210 10, 206.16 -29.02))"
            );
        }

        [Test]
        public void TestZeroDistanceAtVertex()
        {
            CheckBuffer("LINESTRING( 10 10, 20 20, 30 30)",
                new double[] { 5, 0, 5 },
                "MULTIPOLYGON (((5.1 10.98, 5.38 11.91, 5.84 12.78, 6.46 13.54, 7.22 14.16, 7.94 14.56, 20 20, 14.56 7.94, 14.16 7.22, 13.54 6.46, 12.78 5.84, 11.91 5.38, 10.98 5.1, 10 5, 9.02 5.1, 8.09 5.38, 7.22 5.84, 6.46 6.46, 5.84 7.22, 5.38 8.09, 5.1 9.02, 5 10, 5.1 10.98)), ((25.44 32.06, 25.84 32.78, 26.46 33.54, 27.22 34.16, 28.09 34.62, 29.02 34.9, 30 35, 30.98 34.9, 31.91 34.62, 32.78 34.16, 33.54 33.54, 34.16 32.78, 34.62 31.91, 34.9 30.98, 35 30, 34.9 29.02, 34.62 28.09, 34.16 27.22, 33.54 26.46, 32.78 25.84, 32.06 25.44, 20 20, 25.44 32.06)))"
                );
        }

        [Test]
        public void TestZeroDistancesForSegment()
        {
            CheckBuffer("LINESTRING( 10 10, 20 20, 30 30, 40 40)",
                new double[] { 5, 0, 0, 5 },
                "MULTIPOLYGON (((5.1 10.98, 5.38 11.91, 5.84 12.78, 6.46 13.54, 7.22 14.16, 7.94 14.56, 20 20, 14.56 7.94, 14.16 7.22, 13.54 6.46, 12.78 5.84, 11.91 5.38, 10.98 5.1, 10 5, 9.02 5.1, 8.09 5.38, 7.22 5.84, 6.46 6.46, 5.84 7.22, 5.38 8.09, 5.1 9.02, 5 10, 5.1 10.98)), ((35.44 42.06, 35.84 42.78, 36.46 43.54, 37.22 44.16, 38.09 44.62, 39.02 44.9, 40 45, 40.98 44.9, 41.91 44.62, 42.78 44.16, 43.54 43.54, 44.16 42.78, 44.62 41.91, 44.9 40.98, 45 40, 44.9 39.02, 44.62 38.09, 44.16 37.22, 43.54 36.46, 42.78 35.84, 42.06 35.44, 30 30, 35.44 42.06)))"
                );
        }

        // see https://github.com/locationtech/jts/issues/998
        [Test]
        public void TestIssue998_Spike()
        {
            CheckBuffer("LINESTRING (0.024520295 69.50077743, 0.000508719 74.50086084, 0 76.39546845)",
                new double[] { 6.47, 6.9, 7 },
                "POLYGON ((-6.87 77.76, -6.47 79.07, -5.82 80.28, -4.95 81.35, -3.89 82.22, -2.68 82.86, -1.37 83.26, 0 83.4, 1.37 83.26, 2.68 82.86, 3.89 82.22, 4.95 81.35, 5.82 80.28, 6.47 79.07, 6.87 77.76, 7 76.4, 6.99 76.03, 6.89 74.14, 6.88 74.08, 6.88 73.94, 6.47 68.98, 6.37 68.24, 6 67.02, 5.4 65.91, 4.6 64.93, 3.62 64.12, 2.5 63.52, 1.29 63.16, 0.02 63.03, -1.24 63.16, -2.45 63.52, -3.57 64.12, -4.55 64.93, -5.36 65.91, -5.95 67.02, -6.32 68.24, -6.42 68.91, -6.87 73.87, -6.88 74.05, -6.89 74.13, -6.99 76.02, -7 76.4, -6.87 77.76))"
                );
        }

        [Test]
        public void TestNoReverseSpike()
        {
            CheckBuffer("LINESTRING (0 70, 0 80)",
                new double[] { 4, 7 },
                "POLYGON ((-6.87 78.63, -7 80, -6.87 81.37, -6.47 82.68, -5.82 83.89, -4.95 84.95, -3.89 85.82, -2.68 86.47, -1.37 86.87, 0 87, 1.37 86.87, 2.68 86.47, 3.89 85.82, 4.95 84.95, 5.82 83.89, 6.47 82.68, 6.87 81.37, 7 80, 6.87 78.63, 6.68 77.9, 3.82 68.8, 3.7 68.47, 3.33 67.78, 2.83 67.17, 2.22 66.67, 1.53 66.3, 0.78 66.08, 0 66, -0.78 66.08, -1.53 66.3, -2.22 66.67, -2.83 67.17, -3.33 67.78, -3.7 68.47, -3.82 68.8, -6.68 77.9, -6.87 78.63))"
                );
        }

        [Test]
        public void TestNoShortCapSegments()
        {
            CheckBuffer("LINESTRING (6.85 78.25, 18 87)",
                new double[] { 5, 9 },
                "POLYGON ((11.64 93.36, 13 94.48, 14.56 95.31, 16.24 95.83, 18 96, 19.76 95.83, 21.44 95.31, 23 94.48, 24.36 93.36, 25.48 92, 26.31 90.44, 26.83 88.76, 27 87, 26.83 85.24, 26.31 83.56, 25.48 82, 24.36 80.64, 23 79.52, 21.33 78.64, 8.7 73.61, 7.83 73.35, 6.85 73.25, 5.87 73.35, 4.94 73.63, 4.07 74.09, 3.31 74.71, 2.69 75.47, 2.23 76.34, 1.95 77.27, 1.85 78.25, 1.95 79.23, 2.23 80.16, 2.78 81.15, 10.67 92.22, 11.64 93.36))"
                );
        }


        private void CheckBuffer(string wkt, double startDist, double endDist, string wktExpected)
        {
            var geom = Read(wkt);
            var result = VariableBuffer.Buffer(geom, startDist, endDist);
            //System.out.println(result);
            CheckBuffer(result, wktExpected);
        }

        private void CheckBuffer(string wkt, double[] dist, string wktExpected)
        {
            var geom = Read(wkt);
            var result = VariableBuffer.Buffer(geom, dist);
            //System.out.println(result);
            CheckBuffer(result, wktExpected);
        }

        private void CheckBuffer(Geometry actual, string wktExpected)
        {
            var expected = Read(wktExpected);
            CheckEqual(expected, actual, DEFAULT_TOLERANCE);
        }
    }
}

