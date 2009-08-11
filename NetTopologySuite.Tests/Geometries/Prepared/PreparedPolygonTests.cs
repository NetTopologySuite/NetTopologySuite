
using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.Geometries.Prepared
{
    public class PreparedPolygonTests
    {
        private const int MaxIter = 100000;
        private static readonly BufferedCoordinateFactory _coordFact = new BufferedCoordinateFactory(10000d);



        private static readonly GeometryFactory<BufferedCoordinate> _geomFact =
            new GeometryFactory<BufferedCoordinate>(new BufferedCoordinateSequenceFactory(_coordFact));

        [Fact]
        public void ComputePi()
        {
            IGeometry<BufferedCoordinate> circle = createCircle(9);
            IPreparedGeometry<BufferedCoordinate> prepCircle =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(circle);

            int count = 0;
            int inCount = 0;
            DateTime start;
            double approxPi;
            for (int n = 0; n < 10; n++)
            {
                count = 0;
                inCount = 0;
                start = DateTime.Now;
                for (int i = 0; i < MaxIter; i++)
                {
                    count++;
                    IPoint<BufferedCoordinate> randPt = CreateRandomPoint();
                    if (prepCircle.Intersects(randPt))
                        inCount++;
                }
                approxPi = 4.0*inCount/count;

                Console.WriteLine(string.Format("Duration PreparedGeometry: {0} ({1}, {2})",
                                              DateTime.Now.Subtract(start), inCount, approxPi));
            }

            count = 0;
            inCount = 0;
            for (int i = 0; i < MaxIter; i++)
            {
                count++;
                IPoint<BufferedCoordinate> randPt = CreateRandomPoint();
                if (circle.Intersects(randPt))
                {
                    inCount++;
                    Assert.True(prepCircle.Intersects(randPt));
                }
                else
                {
                    Assert.False(prepCircle.Intersects(randPt));
                }
            }

            Console.WriteLine("Geometry.Intersects and PreparedGeometry.Intersects produce same result");

            count = 0;
            inCount = 0;
            start = DateTime.Now;
            for (int i = 0; i < MaxIter; i++)
            {
                count++;
                IPoint<BufferedCoordinate> randPt = CreateRandomPoint();
                if (circle.Intersects(randPt))
                    inCount++;
            }
            approxPi = 4.0*inCount/count;

            Console.WriteLine(string.Format("Duration just Geometry: {0} ({1}, {2})",
                                          DateTime.Now.Subtract(start), inCount, approxPi));

        }

        private static IGeometry<BufferedCoordinate> createCircle(int pts)
        {
            ICoordinateFactory<BufferedCoordinate> cf = _geomFact.CoordinateFactory;
            IGeometry<BufferedCoordinate> centrePt = _geomFact.CreatePoint(cf.Create(0.5d, 0.5d));
            return centrePt.Buffer(0.5, pts);

            IGeometry<BufferedCoordinate> tmp = _geomFact.WktReader.Read(
                @"LINESTRING (1 0.5, 0.992403876506104 0.413175911166535, 0.969846310392954 0.328989928337166, 0.933012701892219 0.25, 0.883022221559489 0.17860619515673, 0.82139380484327 0.116977778440511, 0.75 0.0669872981077807, 0.671010071662834 0.0301536896070458, 0.586824088833465 0.00759612349389599, 0.5 0, 0.413175911166535 0.00759612349389599, 0.328989928337166 0.0301536896070458, 0.25 0.0669872981077806, 0.17860619515673 0.116977778440511, 0.116977778440511 0.17860619515673, 0.0669872981077809 0.25, 0.030153689607046 0.328989928337165, 0.0075961234938961 0.413175911166534, 0 0.499999999999999, 0.00759612349389582 0.586824088833464, 0.0301536896070455 0.671010071662834, 0.0669872981077801 0.749999999999999, 0.11697777844051 0.821393804843269, 0.17860619515673 0.883022221559488, 0.249999999999999 0.933012701892219, 0.328989928337164 0.969846310392954, 0.413175911166534 0.992403876506104, 0.499999999999999 1, 0.586824088833464 0.992403876506104, 0.671010071662833 0.969846310392955, 0.749999999999998 0.933012701892
22, 0.821393804843268 0.88302222155949, 0.883022221559488 0.821393804843271, 0.933012701892218 0.750000000000002, 0.969846310392953 0.671010071662836, 0.992403876506104 0.586824088833467, 1 0.5)");

            return _geomFact.CreatePolygon(tmp.Coordinates);
        }

        private static IPoint<BufferedCoordinate> CreateRandomPoint()
        {
            ICoordinateFactory<BufferedCoordinate> cf = _geomFact.CoordinateFactory;
            var random = new Random();
            return _geomFact.CreatePoint(cf.Create(random.NextDouble(), random.NextDouble()));
        }
    }
}
