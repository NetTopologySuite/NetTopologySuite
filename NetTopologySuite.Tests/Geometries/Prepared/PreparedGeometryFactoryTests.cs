using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.Geometries.Prepared
{
    public class PreparedGeometryFactoryTests
    {
        private static readonly BufferedCoordinateFactory _coordFact = new BufferedCoordinateFactory();

        private static readonly GeometryFactory<BufferedCoordinate> _geomFact =
            new GeometryFactory<BufferedCoordinate>(new BufferedCoordinateSequenceFactory(_coordFact));

        [Fact]
        public void Test01()
        {
            var pgf =
                new PreparedGeometryFactory<BufferedCoordinate>();
            Assert.NotNull(pgf);
        }

        [Fact]
        public void Test2()
        {
            IGeometry<BufferedCoordinate> geom = null;
            Assert.Throws(typeof(NullReferenceException), delegate
                                                               {
                                                                   PreparedGeometryFactory<BufferedCoordinate>.Prepare(
                                                                       geom);
                                                               }
                )
                ;
        }

        [Fact]
        public void Test03()
        {
            IGeometry<BufferedCoordinate> geom = null;
            var pgf = new PreparedGeometryFactory<BufferedCoordinate>();

            Assert.Throws(typeof(NullReferenceException), delegate { pgf.Create(geom); }
                )
                ;
        }

        [Fact]
        public void Test04()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.CreateGeometry(_geomFact.CreatePoint(_coordFact.Create(10.0, 10.0)));
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }

        [Fact]
        public void Test18()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.CreatePoint(_coordFact.Create(1.234, 4.567));
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }

        [Fact]
        public void Test20()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.WktReader.Read("LINESTRING (0 0, 5 5, 10 5, 10 10)");
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }

        [Fact]
        public void Test22()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.WktReader.Read("POLYGON((0 10, 5 5, 10 5, 15 10, 10 15, 5 15, 0 10))");
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }

        [Fact]
        public void Test24()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.WktReader.Read("MULTIPOINT(0 0, 5 5, 10 10, 15 15, 20 20)");
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }

        [Fact]
        public void Test26()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.WktReader.Read("MULTILINESTRING ((20 120, 120 20), (20 20, 120 120)))");
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }

        [Fact]
        public void Test28()
        {
            IGeometry<BufferedCoordinate> geom =
                _geomFact.WktReader.Read(
                    "MULTIPOLYGON(((0 0, 10 0, 10 10, 0 10, 0 0),(2 2, 2 6, 6 4, 2 2)),((60 60, 60 50, 70 40, 60 60)))");
            IPreparedGeometry<BufferedCoordinate> pgeom =
                PreparedGeometryFactory<BufferedCoordinate>.Prepare(geom);
            Assert.NotNull(pgeom);
            Assert.True(pgeom.Geometry.EqualsExact(geom));
        }
    }
}
