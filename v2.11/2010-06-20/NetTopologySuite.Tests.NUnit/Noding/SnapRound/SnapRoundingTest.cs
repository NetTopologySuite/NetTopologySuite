using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding.SnapRound
{
    [TestFixture]
    public class SnapRoundingTest
    {
        static SnapRoundingTest()
        {
            HotPixel<Coordinate>.FloatingPrecisionGeometryFactory = GeometryUtils.GeometryFactory;
            RobustLineIntersector<Coordinate>.FloatingPrecisionCoordinateFactory = GeometryUtils.CoordFac;
        }

        [Test]
        public void TestPolyWithCloseNode()
        {
            String[] polyWithCloseNode = {
      "POLYGON ((20 0, 20 160, 140 1, 160 160, 160 1, 20 0))"
    };
            RunRounding(polyWithCloseNode);
        }

        [Test]
        public void TestBadLines1()
        {
            String[] badLines1 = {
      "LINESTRING ( 171 157, 175 154, 170 154, 170 155, 170 156, 170 157, 171 158, 171 159, 172 160, 176 156, 171 156, 171 159, 176 159, 172 155, 170 157, 174 161, 174 156, 173 156, 172 156 )"
    };
            RunRounding(badLines1);
        }

        [Test]
        public void TestBadLines2()
        {
            String[] badLines2 = {
      "LINESTRING ( 175 222, 176 222, 176 219, 174 221, 175 222, 177 220, 174 220, 174 222, 177 222, 175 220, 174 221 )"
    };
            RunRounding(badLines2);
        }

        [Test]
        public void TestCollapse1()
        {
            String[] collapse1 = {
      "LINESTRING ( 362 177, 375 164, 374 164, 372 161, 373 163, 372 165, 373 164, 442 58 )"
    };
            RunRounding(collapse1);
        }

        [Test]
        public void TestBadNoding1()
        {
            String[] badNoding1 = {
      "LINESTRING ( 76 47, 81 52, 81 53, 85 57, 88 62, 89 64, 57 80, 82 55, 101 74, 76 99, 92 67, 94 68, 99 71, 103 75, 139 111 )"
    };
            RunRounding(badNoding1);
        }

        [Test]
        public void TestBadNoding1Extract()
        {
            String[] badNoding1Extract = {
      "LINESTRING ( 82 55, 101 74 )",
      "LINESTRING ( 94 68, 99 71 )",
      "LINESTRING ( 85 57, 88 62 )"
    };
            RunRounding(badNoding1Extract);
        }

        [Test]
        public void TestBadNoding1ExtractShift()
        {
            String[] badNoding1ExtractShift = {
      "LINESTRING ( 0 0, 19 19 )",
      "LINESTRING ( 12 13, 17 16 )",
      "LINESTRING ( 3 2, 6 7 )"
    };
            RunRounding(badNoding1ExtractShift);
        }

        void RunRounding(String[] wkt)
        {
            List<IGeometry<Coordinate>> geoms = FromWKT(wkt);
            Console.WriteLine("Input:");
            foreach (IGeometry<Coordinate> line in geoms)
                Console.WriteLine(string.Format("\t{0}", line));
            //PrecisionModel pm = new PrecisionModel(1.0);
            GeometryNoder<Coordinate> noder = new GeometryNoder<Coordinate>(GeometryUtils.GetScaledFactory(1d));
            List<IGeometry<Coordinate>> nodedLines = noder.Node(geoms);
            Console.WriteLine("Output:");
            foreach (IGeometry<Coordinate> line in nodedLines)
                Console.WriteLine(string.Format("\t{0}", line));
        }

        List<IGeometry<Coordinate>> FromWKT(String[] wkts)
        {
            List<IGeometry<Coordinate>> geomList = new List<IGeometry<Coordinate>>();
            //IWktGeometryReader<Coordinate> reader
            for (int i = 0; i < wkts.Length; i++)
            {
                try
                {
                    geomList.Add(GeometryUtils.Reader.Read(wkts[i]));
                }
                catch (Exception ex)
                {
                    //ex.printStackTrace();
                }
            }
            return geomList;
        }
    }
}