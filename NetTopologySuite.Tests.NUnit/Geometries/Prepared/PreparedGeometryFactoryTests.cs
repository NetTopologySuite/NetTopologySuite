using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Coordinates;
using NUnit.Framework;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Coordinate;
using coordFac = NetTopologySuite.Coordinates.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.CoordinateSequenceFactory;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#endif

namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    public class PreparedGeometryFactoryTests
    {
        private static readonly coordFac _coordFact = new coordFac();

        private static readonly GeometryFactory<coord> _geomFact =
            new GeometryFactory<coord>(new coordSeqFac(_coordFact));

        [Test]
        public void Test01()
        {
            var pgf =
                new PreparedGeometryFactory<coord>();
            Assert.IsNotNull(pgf);
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void Test2()
        {
            IGeometry<coord> geom = null;
            PreparedGeometryFactory<coord>.Prepare(geom);
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void Test03()
        {
            IGeometry<coord> geom = null;
            var pgf = new PreparedGeometryFactory<coord>();

            pgf.Create(geom);
        }

        [Test]
        public void Test04()
        {
            IGeometry<coord> geom =
                _geomFact.CreateGeometry(_geomFact.CreatePoint(_coordFact.Create(10.0, 10.0)));
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }

        [Test]
        public void Test18()
        {
            IGeometry<coord> geom =
                _geomFact.CreatePoint(_coordFact.Create(1.234, 4.567));
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }

        [Test]
        public void Test20()
        {
            IGeometry<coord> geom =
                _geomFact.WktReader.Read("LINESTRING (0 0, 5 5, 10 5, 10 10)");
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }

        [Test]
        public void Test22()
        {
            IGeometry<coord> geom =
                _geomFact.WktReader.Read("POLYGON((0 10, 5 5, 10 5, 15 10, 10 15, 5 15, 0 10))");
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }

        [Test]
        public void Test24()
        {
            IGeometry<coord> geom =
                _geomFact.WktReader.Read("MULTIPOINT(0 0, 5 5, 10 10, 15 15, 20 20)");
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }

        [Test]
        public void Test26()
        {
            IGeometry<coord> geom =
                _geomFact.WktReader.Read("MULTILINESTRING ((20 120, 120 20), (20 20, 120 120)))");
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }

        [Test]
        public void Test28()
        {
            IGeometry<coord> geom =
                _geomFact.WktReader.Read(
                    "MULTIPOLYGON(((0 0, 10 0, 10 10, 0 10, 0 0),(2 2, 2 6, 6 4, 2 2)),((60 60, 60 50, 70 40, 60 60)))");
            IPreparedGeometry<coord> pgeom =
                PreparedGeometryFactory<coord>.Prepare(geom);
            Assert.IsNotNull(pgeom);
            Assert.IsTrue(pgeom.Geometry.EqualsExact(geom));
        }
    }
}
