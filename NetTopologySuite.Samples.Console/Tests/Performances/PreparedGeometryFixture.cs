using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Performances
{
    /// <summary>
    /// see: https://groups.google.com/d/msg/nettopologysuite/Vdis_LHdH8w/x5r-lSkiL4QJ
    /// </summary>
    [TestFixture, Explicit("a lot of RAM used!")]
    public class PreparedGeometryFixture
    {
        private const int NumShellCoords = 10;

        private readonly Random _generator;
        private readonly IGeometryFactory _factory;

        public PreparedGeometryFixture()
        {
            _generator = new Random();
            _factory = GeometryFactory.Default;
        }

        [Test]
        public void test_with_one_million_items()
        {
            TestPerformances(1000000);
        }

        [Test]
        public void test_with_ten_millions_items()
        {
            TestPerformances(10000000);
        }

        private Coordinate gimme_a_coord()
        {
            int x = _generator.Next();
            int y = _generator.Next();
            return new Coordinate(x, y);
        }

        private IEnumerable<IPolygon> create_polygons(int total)
        {
            int count = 0;
            while (count++ < total)
            {
                Coordinate[] coords = new Coordinate[NumShellCoords];
                for (int j = 0; j < NumShellCoords - 1; j++)
                    coords[j] = gimme_a_coord();
                coords[NumShellCoords - 1] = coords[0];
                yield return _factory.CreatePolygon(coords);
            }
        }

        private void TestPerformances(int total)
        {
            IPoint point = _factory.CreatePoint(gimme_a_coord());
            IEnumerable<IPolygon> polygons = create_polygons(total);
            IEnumerable<IPreparedGeometry> prepared = polygons.Select(PreparedGeometryFactory.Prepare);
            Stopwatch sw = Stopwatch.StartNew();
            int match = prepared.Count(pg => pg.Contains(point));
            sw.Stop();
            Console.WriteLine("matched '{0}' of '{1}': elapsed time: '{2}'", match, total, sw.Elapsed);
        }
    }
}
