#define simple
using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Coordinates;

#if simple
using NUnit.Framework;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using CoordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#else
using Coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using CoordSeqFac = NetTopologySuite.Coordinates.Simple.BufferedCoordinateSequenceFactory;
#endif

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Poligonize
{
    /// <summary>  
    /// Example of using Polygonizer class to polygonize a set of fully noded linestrings.
    /// </summary>	
    [TestFixture]
    public class PolygonizeExample
    {
        [STAThread]
        public static void Main(String[] args)
        {
            PolygonizeExample test = new PolygonizeExample();
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
        public void Run()
        {
            IGeometryFactory<Coord> geoFactory =
                new GeometryFactory<Coord>(new CoordSeqFac());
            WktReader<Coord> rdr
                = new WktReader<Coord>(geoFactory);
            List<IGeometry<Coord>> lines
                = new List<IGeometry<Coord>>();

            // isolated edge
            lines.Add(rdr.Read("LINESTRING (0 0 , 10 10)"));
            lines.Add(rdr.Read("LINESTRING (185 221, 100 100)")); //dangling edge
            lines.Add(rdr.Read("LINESTRING (185 221, 88 275, 180 316)"));
            lines.Add(rdr.Read("LINESTRING (185 221, 292 281, 180 316)"));
            lines.Add(rdr.Read("LINESTRING (189 98, 83 187, 185 221)"));
            lines.Add(rdr.Read("LINESTRING (189 98, 325 168, 185 221)"));

            Polygonizer<Coord> polygonizer
                = new Polygonizer<Coord>();
            polygonizer.Add(lines);

            IList<IPolygon<Coord>> polys = polygonizer.Polygons;

            Console.WriteLine("Polygons formed (" + polys.Count + "):");
            foreach (object obj in polys)
            {
                Console.WriteLine(obj);
            }
        }
    }
}