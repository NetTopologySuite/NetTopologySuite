using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Operation.Linemerge;
using NUnit.Framework;
#if BUFFERED
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;

#endif

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Linemerge
{
    /// <summary> 
    /// Example of using the <see cref="LineMerger{TCoordinate}"/> class to 
    /// sew together a set of fully noded linestrings.
    /// </summary>	
    [TestFixture]
    public class LineMergeExample
    {
        private static IWktGeometryReader _reader;

        static LineMergeExample()
        {
            InitBlock();
        }

        internal virtual IEnumerable<IGeometry<coord>> Data
        {
            get
            {
                yield return Read("LINESTRING (220 160, 240 150, 270 150, 290 170)");
                yield return Read("LINESTRING (60 210, 30 190, 30 160)");
                yield return Read("LINESTRING (70 430, 100 430, 120 420, 140 400)");
                yield return Read("LINESTRING (160 310, 160 280, 160 250, 170 230)");
                yield return Read("LINESTRING (170 230, 180 210, 200 180, 220 160)");
                yield return Read("LINESTRING (30 160, 40 150, 70 150)");
                yield return Read("LINESTRING (160 310, 200 330, 220 340, 240 360)");
                yield return Read("LINESTRING (140 400, 150 370, 160 340, 160 310)");
                yield return Read("LINESTRING (160 310, 130 300, 100 290, 70 270)");
                yield return Read("LINESTRING (240 360, 260 390, 260 410, 250 430)");
                yield return Read("LINESTRING (70 150, 100 180, 100 200)");
                yield return Read("LINESTRING (70 270, 60 260, 50 240, 50 220, 60 210)");
                yield return Read("LINESTRING (100 200, 90 210, 60 210)");
            }
        }

        private static void InitBlock()
        {
            _reader =
                new WktReader<coord>(
                    GeometryServices.GetGeometryFactory(PrecisionModelType.DoubleFloating),
                    null);
        }

        [STAThread]
        public static void Main(String[] args)
        {
            LineMergeExample test = new LineMergeExample();

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
            IEnumerable<IGeometry<coord>> lineStrings = Data;

            LineMerger<coord> lineMerger
                = new LineMerger<coord>();

            lineMerger.Add(lineStrings);

            IEnumerable<ILineString<coord>> mergedLineStrings
                = lineMerger.MergedLineStrings;

            Console.WriteLine("Lines formed (" + Enumerable.Count(mergedLineStrings) + "):");

            foreach (Object obj in mergedLineStrings)
            {
                Console.WriteLine(obj);
            }
        }


        internal virtual IGeometry<coord> Read(String lineWKT)
        {
            try
            {
                IGeometry<coord> geom
                    = _reader.Read(lineWKT) as IGeometry<coord>;
                return geom;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return null;
        }
    }
}