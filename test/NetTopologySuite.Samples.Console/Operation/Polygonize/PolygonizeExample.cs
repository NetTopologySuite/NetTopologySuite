using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Operation.Poligonize
{
    /// <summary>
    /// Example of using Polygonizer class to polygonize a set of fully noded linestrings.
    /// </summary>
    public class PolygonizeExample
    {
        [STAThread]
        public static void main(string[] args)
        {
            var test = new PolygonizeExample();
            try
            {
                test.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        [Test]
        public void Test()
        {
            Run();
        }

        internal virtual void Run()
        {
            var rdr = new WKTReader();
            IList<Geometry> lines = new List<Geometry>
                                         {
                                             rdr.Read("LINESTRING (0 0 , 10 10)"),
                                             rdr.Read("LINESTRING (185 221, 100 100)"),
                                             rdr.Read("LINESTRING (185 221, 88 275, 180 316)"),
                                             rdr.Read("LINESTRING (185 221, 292 281, 180 316)"),
                                             rdr.Read("LINESTRING (189 98, 83 187, 185 221)"),
                                             rdr.Read("LINESTRING (189 98, 325 168, 185 221)")
                                         };

            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);

            var polys = polygonizer.GetPolygons();

            Console.WriteLine("Polygons formed (" + polys.Count + "):");
            foreach(var obj in polys)
                Console.WriteLine(obj);
        }

        [Test]
        public void TestPoly2()
        {
            var rdr = new WKTReader();
            var geo = rdr.Read("MULTILINESTRING((-1.328 -3.953, 2.457 4.615, 6.07 -4.022, 11.851 -1.957, 11.059 3.342, -1.362 -3.196), (-1.225 -3.333, 5.141 3.961, 12.332 -2.267))");

            var noder = new SnapRoundingNoder(new PrecisionModel(1000));
            var nss = new List<ISegmentString>(geo.NumGeometries);
            for (int i = 0; i < geo.NumGeometries; i++)
            {
                var line = geo.GetGeometryN(i);
                nss.Add(new NodedSegmentString(line.Coordinates, line));
            }
            noder.ComputeNodes(nss);

            var polygonizer = new Polygonizer(true);
            foreach (var n in noder.GetNodedSubstrings())
            {
                var line = geo.Factory.CreateLineString(n.Coordinates);
                TestContext.WriteLine(line);
                polygonizer.Add(line);
            }

            var polys = polygonizer.GetPolygons();
            TestContext.WriteLine(geo.Factory.CreateMultiPolygon((Polygon[])polys.ToArray()));
            int numPolygons = polys.Count;
            Assert.AreEqual(5, numPolygons);
        }
            
    }
}
