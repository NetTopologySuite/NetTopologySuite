using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Simplify;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.NTS
{
    [TestFixture]
    public class Issues
    {
        [Test]
        public void Issue60()
        {
            ICoordinateFactory<Coordinate> cf = GeometryUtils.CoordFac;
            ICoordinateSequenceFactory<Coordinate> csf = GeometryUtils.CoordSeqFac;

            List<Coordinate> list = new List<Coordinate>(
                new Coordinate[] { cf.Create(10, 10), cf.Create(10, 20), cf.Create(20, 20),
                                    cf.Create(20, 10), cf.Create(30, 10)});
            ICoordinateSequence<Coordinate> coordSeq = csf.Create(list);
            Console.WriteLine(string.Format("Input: {0}", coordSeq));
            try
            {
                ICoordinateSequence outputCoords = DouglasPeuckerLineSimplifier<Coordinate>.Simplify(coordSeq, 5);
                Console.WriteLine(string.Format("Output: {0}", outputCoords));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        [Test]
        public void Issue61()
        {
            ICoordinateFactory<Coordinate> coordFactory = GeometryUtils.CoordFac;
            IGeometryFactory<Coordinate> geomFactory
                = new GeometryFactory<Coordinate>(
                    new CoordinateSequenceFactory(coordFactory as CoordinateFactory));
            double tolerance = 63.0;

            IGeometry < Coordinate > inputGeometry = geomFactory.WktReader.Read("LINESTRING (2783 2949, 2788 -1237, 764 -2410, -1589 -2274, -2724 2451, 2783 2949)");
            IGeometry<Coordinate> buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);

            inputGeometry = geomFactory.WktReader.Read(@"LINESTRING (3155 91, 2975 -3041, 979 -2893, -1497 -4490, -2664 -2422, -1486 -490,
-4033 1558, -2373 3682, 368 3548, 3155 91)");
            buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);
            inputGeometry = geomFactory.WktReader.Read(@"LINESTRING (3282 89, 3062 -2335, -104 -1722, -3146 -2745, -2676 990, -1014 1831, -508
4821, 664 987, 3282 89)
");
            buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);
            inputGeometry = geomFactory.WktReader.Read(@"LINESTRING (6757 5207, 6602 3743, 3575 4282, 3294 4897, 3623 6276, 4925 7132, 5947
6943, 6757 5207)
");
            buffer = inputGeometry.Buffer(tolerance);
            Console.WriteLine(buffer);
        }

    }
}
