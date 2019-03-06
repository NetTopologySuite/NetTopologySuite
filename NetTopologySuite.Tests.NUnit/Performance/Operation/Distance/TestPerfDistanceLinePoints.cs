using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Distance
{
    /**
     * Tests performance of {@link IndexedFacetDistance} versus standard
     * {@link DistanceOp}
     * using a grid of points to a target set of lines
     *
     * @author Martin Davis
     *
     */

    public class TestPerfDistanceLinesPoints
    {
        private static readonly bool USE_INDEXED_DIST = true;

        private static readonly GeometryFactory geomFact = new GeometryFactory();

        private static readonly int MAX_ITER = 1;
        private static readonly int NUM_TARGET_ITEMS = 4000;
        private static readonly double EXTENT = 1000;
        private static readonly int NUM_PTS_SIDE = 100;

        private bool verbose = true;

        [TestAttribute, CategoryAttribute("Stress")]
        public void Test()
        {

            //test(200);
            //if (true) return;

            //    test(5000);
            //    test(8001);

            //test(50);
            Test(100);
            //Test(200);
            //Test(500);
            //Test(1000);
            //test(5000);
            //test(10000);
            //test(50000);
            //test(100000);
        }

        public void Test(int num)
        {
            //Geometry lines = createLine(EXTENT, num);
            var target = CreateDiagonalCircles(EXTENT, NUM_TARGET_ITEMS);
            var pts = CreatePoints(target.EnvelopeInternal, num);

            /*
            Geometry target = loadData("C:\\data\\martin\\proj\\jts\\testing\\distance\\bc_coast.wkt");
            Envelope bcEnv_Albers = new Envelope(-45838, 1882064, 255756, 1733287);
            Geometry[] pts = createPoints(bcEnv_Albers, num);
            */
            Test(pts, target);
        }

        /*
        private void xtest(int num)
        {
            var target = LoadData("C:\\proj\\JTS\\test\\g2e\\ffmwdec08.wkt");
            var bcEnv_Albers = new Envelope(-45838, 1882064, 255756, 1733287);
            var pts = CreatePoints(bcEnv_Albers, num);

            Test(pts, target);
        }
        */

        private void Test(IGeometry[] pts, IGeometry target)
        {
            if (verbose)
                Console.WriteLine("Query points = " + pts.Length
                                  + "     Target points = " + target.NumPoints);
            //    if (! verbose) System.out.print(num + ", ");

            double dist = 0.0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < MAX_ITER; i++)
            {
                ComputeDistance(pts, target);
            }
            sw.Stop();
            if (!verbose)
                Console.WriteLine(sw.ElapsedMilliseconds);
            if (verbose)
            {
                string name = USE_INDEXED_DIST ? "IndexedFacetDistance" : "Distance";
                Console.WriteLine(name + " - Run time: " + sw.ElapsedMilliseconds);
                Console.WriteLine();
            }
        }

        private static void ComputeDistance(IGeometry[] pts, IGeometry geom)
        {
            IndexedFacetDistance bbd = null;
            if (USE_INDEXED_DIST)
                bbd = new IndexedFacetDistance(geom);
            for (int i = 0; i < pts.Length; i++)
            {
                if (USE_INDEXED_DIST)
                {
                    double dist = bbd.Distance(pts[i]);
                    //        double dist = bbd.getDistanceWithin(pts[i].getCoordinate(), 100000);
                }
                else
                {
                    double dist = geom.Distance(pts[i]);
                }
            }
        }

        private static IGeometry CreateDiagonalCircles(double extent, int nSegs)
        {
            var circles = new IPolygon[nSegs];
            double inc = extent/nSegs;
            for (int i = 0; i < nSegs; i++)
            {
                double ord = i*inc;
                var p = new Coordinate(ord, ord);
                var pt = geomFact.CreatePoint(p);
                circles[i] = (IPolygon) pt.Buffer(inc/2);
            }
            return geomFact.CreateMultiPolygon(circles);

        }

        private IGeometry CreateLine(double extent, int nSegs)
        {
            var pts =
                new Coordinate[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, extent),
                        new Coordinate(extent, extent),
                        new Coordinate(extent, 0)

                    };
            var outline = geomFact.CreateLineString(pts);
            double inc = extent/nSegs;
            return Densifier.Densify(outline, inc);

        }

        private IGeometry CreateDiagonalLine(double extent, int nSegs)
        {
            var pts = new Coordinate[nSegs + 1];
            pts[0] = new Coordinate(0, 0);
            double inc = extent/nSegs;
            for (int i = 1; i <= nSegs; i++)
            {
                double ord = i*inc;
                pts[i] = new Coordinate(ord, ord);
            }
            return geomFact.CreateLineString(pts);
        }

        private static IGeometry[] CreatePoints(Envelope extent, int nPtsSide)
        {
            var pts = new IGeometry[nPtsSide*nPtsSide];
            int index = 0;
            double xinc = extent.Width/nPtsSide;
            double yinc = extent.Height/nPtsSide;
            for (int i = 0; i < nPtsSide; i++)
            {
                for (int j = 0; j < nPtsSide; j++)
                {
                    pts[index++] = geomFact.CreatePoint(
                        new Coordinate(
                            extent.MinX + i*xinc,
                            extent.MinY + j*yinc));
                }
            }
            return pts;
        }

        /*
        private static IGeometry LoadData(String file)
        {
            var geoms = LoadWKT(file);
            return geomFact.BuildGeometry(geoms);
        }

        private static IList<IGeometry> LoadWKT(String filename)
        {
            var rdr = new WKTReader(geomFact);
            var fileRdr = new WKTFileReader(filename, rdr);
            return fileRdr.Read();
        }
        */
    }
}
