using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Coordinates;
using Xunit;

#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif

namespace NetTopologySuite.Tests.Geometries.Prepared
{
    public class PreparedPointPredicateTests
    {
        [Fact]
        public void PA_PointInInteriorOfPoly()
        {
            IGeometry<coord> geom0 = TestFactories.GeometryFactory.WktReader.Read("POINT (100 100)");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((50 130, 150 130, 100 50, 50 130))");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Intersects(geom1));
        }

        [Fact]
        public void PA_PointOnBoundaryOfPoly()
        {
            IGeometry<coord> geom0 = TestFactories.GeometryFactory.WktReader.Read("POINT (50 100)");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((50 130, 150 130, 100 50, 50 130))");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Intersects(geom1));
        }

        [Fact]
        public void PA_PointOutsideOfPoly()
        {
            IGeometry<coord> geom0 = TestFactories.GeometryFactory.WktReader.Read("POINT (200 200)");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((50 130, 150 130, 100 50, 50 130))");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Intersects(geom1));
        }

    }

    public class PreparedPolygonPredicateTests
    {
        [Fact]
        /// <summary>
        /// P/A - 
        /// Point equal to start point of polygon
        /// </summary>
        public void Test01()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POINT (10 10)");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((10 10, 60 100, 110 10, 10 10))");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.False(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }
        [Fact]
        ///<summary>
        /// mA/L
        /// A has 2 shells touching at one vertex and one non-vertex.
        /// B passes between the shells, but is wholely contained
        /// </summary>
        public void Test02()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("MULTIPOLYGON (((100 30, 30 110, 150 110, 100 30)), ((90 110, 30 170, 140 170, 90 110)))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (90 80, 90 150)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }

        ///<summary>
        /// mA/L
        /// A has 2 shells touching at one vertex and one non-vertex.
        /// B passes between the shells, but is NOT contained (since it is slightly offset)
        /// </summary>
        [Fact]
        public void Test03()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("MULTIPOLYGON (((100 30, 30 110, 150 110, 100 30)), ((90 110, 30 170, 140 170, 90 110)))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (90.1 80, 90 150)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }

        ///<summary>
        /// mA/L
        /// 2 disjoint shells with line crossing between them
        /// </summary>
        [Fact]
        public void Test04()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("MULTIPOLYGON (((50 20, 10 70, 80 70, 50 20)),((10 90, 80 90, 50 140, 10 90)))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (50 110, 50 60)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.False(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }

        ///<summary>
        /// A/L
        /// proper intersection crossing bdy
        /// </summary>
        [Fact]
        public void Test05()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((10 10, 10 100, 120 110, 120 30, 10 10))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (60 60, 70 140)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }

        ///<summary>
        /// A/L
        /// non-proper intersection crossing bdy
        /// </summary>
        [Fact]
        public void Test06()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((10 10, 60 100, 110 10, 10 10))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (60 60, 70 140)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }

        ///<summary>
        /// A/L
        /// wholely contained
        /// </summary>
        [Fact]
        public void Test07()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((10 10, 60 100, 110 10, 10 10))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (50 30, 70 60)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }

        ///<summary>
        /// A/L
        /// contained but touching bdy at interior point
        /// </summary>
        [Fact]
        public void Test08()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((10 10, 60 100, 110 10, 10 10))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (60 10, 70 60)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }
        /// A/L
        /// line in bdy - covered but not contained
        /// </summary>
        [Fact]
        public void Test09()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((10 10, 60 100, 110 10, 10 10))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (30 10, 90 10)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }
        /// A/A
        /// A/A - two equal polygons
        /// </summary>
        [Fact]
        public void Test10()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON((20 20, 20 100, 120 100, 140 20, 20 20))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON((20 20, 20 100, 120 100, 140 20, 20 20))");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }
        ///<summary>
        /// A/L
        /// line with repeated points
        /// </summary>
        [Fact]
        public void Test11()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON((20 20, 20 100, 120 100, 140 20, 20 20))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (10 60, 50 60, 60 30, 60 30, 90 80, 90 80, 160 70)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.False(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }
        ///<summary>
        /// A/L
        /// polygon and line with repeated points
        /// </summary>
        [Fact]
        public void Test12()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON((20 20, 20 100, 120 100, 120 100, 120 100, 140 20, 140 20, 140 20, 20 20))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (10 60, 50 60, 60 30, 60 30, 90 80, 90 80, 160 70)");

            IPreparedGeometry<coord> prepGeom = PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.False(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));
        }
    }
    public class TestPreparedPredicatesWithGeometryCollection
    {
        ///<summary>
        /// Box against GC
        /// </summary>
        [Fact]
        public void Test01()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((0 0, 0 100, 200 100, 200 0, 0 0))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read(
                    "GEOMETRYCOLLECTION (POLYGON ((50 160, 110 60, 150 160, 50 160)),LINESTRING (50 40, 170 120))");

            IPreparedGeometry<coord> prepGeom =
                PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            Assert.False(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));

        }
        ///<summary>
        /// Box against GC, with containment
        /// </summary>
        [Fact]
        public void Test02()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((0 0, 0 200, 200 200, 200 0, 0 0))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read(
                    "GEOMETRYCOLLECTION (POLYGON ((50 160, 110 60, 150 160, 50 160)),LINESTRING (50 40, 170 120))");

            IPreparedGeometry<coord> prepGeom =
                PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Contains(geom1));
            Assert.True(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));

        }
        ///<summary>
        /// Polygon-with-hole against GC
        /// </summary>
        [Fact]
        public void Test03()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("POLYGON ((0 0, 0 270, 200 270, 200 0, 0 0),(30 210, 170 210, 60 20, 30 210))");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read(
                    "GEOMETRYCOLLECTION (POLYGON ((50 160, 110 60, 150 160, 50 160)),LINESTRING (50 40, 170 120))");

            IPreparedGeometry<coord> prepGeom =
                PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.False(prepGeom.Contains(geom1));
            //Assert.True(prepGeom.Covers(geom1));
            Assert.True(prepGeom.Intersects(geom1));

        }
        ///<summary>
        /// Linestring against GC
        /// </summary>
        [Fact]
        public void Test04()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (20 90, 90 190, 170 50)");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read(
                    "GEOMETRYCOLLECTION (POLYGON ((50 160, 110 60, 150 160, 50 160)),LINESTRING (50 40, 170 120))");

            IPreparedGeometry<coord> prepGeom =
                PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Intersects(geom1));

        }
        ///<summary>
        /// Linestring against GC, with containment
        /// </summary>
        [Fact]
        public void Test05()
        {
            IGeometry<coord> geom0 =
                TestFactories.GeometryFactory.WktReader.Read("LINESTRING (20 20, 100 100, 180 20)");
            IGeometry<coord> geom1 =
                TestFactories.GeometryFactory.WktReader.Read(
                    "GEOMETRYCOLLECTION (LINESTRING (40 40, 80 80),   POINT (120 80))");

            IPreparedGeometry<coord> prepGeom =
                PreparedGeometryFactory<coord>.Prepare(geom0);

            Assert.True(prepGeom.Intersects(geom1));

        }
    }
}
