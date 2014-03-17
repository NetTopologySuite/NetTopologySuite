using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Triangulate;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Triangulate
{
    public class BasicFunctionalityTest
    {
        private const string NTS = "POLYGON ((0 0, 0 140, 220 140, 220 0, 0 0), " +
            "(10 120, 10.722892984206778 123.73299952016072, " +
            "12.78705708350404 126.92628720768715, 15.894058348765082 129.11818200940593, " +
            "19.594690896674148 129.99178285046074, 23.353922466029093 129.42078574705238, " +
            "26.628248631330855 127.48774465919216, 28.94427190999916 124.47213595499957, " +
            "60 62.36067977499789, 60 120, 60.76120467488713 123.8268343236509, " +
            "62.928932188134524 127.07106781186548, 66.1731656763491 129.23879532511287, " +
            "70 130, 73.8268343236509 129.23879532511287, 75 128.45491108143625, " +
            "76.1731656763491 129.23879532511287, 80 130, 110 130, 140 130, " +
            "143.8268343236509 129.23879532511287, 147.07106781186548 127.07106781186548, " +
            "149.23879532511287 123.8268343236509, 150 120, 149.7563920050868 118.77529990657533, " +
            "151.2698653266467 120.57898717483984, 153.9303097578365 122.81136511581886, " +
            "161.4494964168583 127.15256955749211, 164.713014640189 128.3403930497698, " +
            "173.2635182233307 129.84807753012208, 175 130, 200 130, 203.8268343236509 " +
            "129.23879532511287, 207.07106781186548 127.07106781186548, 209.23879532511287 " +
            "123.8268343236509, 210 120, 209.23879532511287 116.1731656763491, " +
            "207.07106781186548 112.92893218813452, 203.8268343236509 110.76120467488713, " +
            "200 110, 175.87488663525926 110, 169.9224599701969 108.950426578261, " +
            "165.4573462044979 106.37249194367239, 162.14321732110722 102.42286694057128, " +
            "160.3798061746948 97.5779346345886, 160.3798061746948 92.42206536541137, " +
            "162.1432173211072 87.57713305942873, 165.4573462044979 83.62750805632763, " +
            "169.9224599701969 81.049573421739, 176.7364817766693 79.84807753012208, " +
            "185.286985359811 78.3403930497698, 188.55050358314173 77.1525695574921, " +
            "196.06969024216352 72.81136511581883, 198.73013467335326 70.57898717483982, " +
            "204.31107952580075 63.92787609686536, 206.04756130247006 60.92020143325668, " +
            "209.0171200331643 52.76140587492995, 209.62019382530522 49.34120444167325, " +
            "209.62019382530522 40.658795558326744, 209.0171200331643 37.23859412507005, " +
            "206.04756130247006 29.079798566743296, 204.31107952580075 26.07212390313459, " +
            "198.73013467335326 19.421012825160147, 196.06969024216346 17.18863488418115, " +
            "188.55050358314165 12.847430442507887, 185.28698535981098 11.659606950230206, " +
            "176.7364817766693 10.15192246987792, 175 10, 150 10, 146.1731656763491 10.761204674887136, " +
            "142.92893218813452 12.928932188134524, 140.76120467488713 16.173165676349104, 140 20, " +
            "140.76120467488713 23.8268343236509, 142.92893218813452 27.071067811865476, " +
            "146.1731656763491 29.238795325112868, 150 30, 174.12511336474077 30, " +
            "180.0775400298031 31.049573421738998, 184.5426537955021 33.62750805632761, " +
            "187.8567826788928 37.57713305942872, 189.62019382530522 42.4220653654114, " +
            "189.62019382530522 47.57793463458859, 187.85678267889278 52.42286694057128, " +
            "184.54265379550213 56.372491943672365, 180.07754002980312 58.95042657826101, " +
            "173.26351822333072 60.15192246987792, 164.713014640189 61.65960695023021, " +
            "161.4494964168583 62.8474304425079, 153.9303097578365 67.18863488418117, " +
            "151.26986532664674 69.42101282516016, 145.68892047419925 76.07212390313462, " +
            "143.95243869752994 79.07979856674335, 140.9828799668357 87.2385941250701, " +
            "140.3798061746948 90.65879555832676, 140.3798061746948 99.34120444167326, " +
            "140.98287996683572 102.76140587492995, 143.91633489196772 110.82100704271807, " +
            "143.8268343236509 110.76120467488713, 140 110, 120 110, 120 20, 119.23879532511287 " +
            "16.1731656763491, 117.07106781186548 12.928932188134524, 113.8268343236509 10.761204674887132, " +
            "110 10, 106.1731656763491 10.761204674887132, 102.92893218813452 12.928932188134524, " +
            "100.76120467488713 16.1731656763491, 100 20, 100 110, 80 110, 80 20, " +
            "79.27710701579322 16.267000479839275, 77.21294291649596 13.073712792312847, " +
            "74.10594165123493 10.881817990594055, 70.40530910332586 10.008217149539268, " +
            "66.64607753397091 10.579214252947615, 63.37175136866915 12.512255340807839, " +
            "61.05572809000084 15.52786404500042, 30 77.63932022500211, 30 20, " +
            "29.238795325112868 16.1731656763491, 27.071067811865476 12.928932188134524, " +
            "23.8268343236509 10.761204674887132, 20 10, 16.173165676349104 10.761204674887132, " +
            "12.928932188134524 12.928932188134524, 10.761204674887132 16.1731656763491, 10 20, 10 120))";

        private WKTReader _wktReader;

        [SetUpAttribute]
        public void SetUp()
        {
            _wktReader = new WKTReader();
        }

        [TestAttribute]
        public void Test1()
        {
            IGeometry geom = _wktReader.Read("POLYGON ((0 0, 0 10, 4 10, 4 8, 6 8, 6 10, 10 10, 10 0, 0 0))");
            DelaunayTriangulationBuilder dtb = new DelaunayTriangulationBuilder();
            dtb.SetSites(geom);
            IMultiLineString resultEdges = dtb.GetEdges(geom.Factory);
            Console.WriteLine(resultEdges.AsText());
            IGeometryCollection resultTriangles = dtb.GetTriangles(geom.Factory);
            Console.WriteLine(resultTriangles.AsText());
        }
        [TestAttribute]
        public void Test2()
        {
            IGeometry geom = _wktReader.Read("POLYGON ((0 0, 0 10, 4 10, 4 8, 6 8, 6 10, 10 10, 10 0, 0 0))");
            ConformingDelaunayTriangulationBuilder dtb = new ConformingDelaunayTriangulationBuilder();
            dtb.SetSites(geom);
            IMultiLineString resultEdges = dtb.GetEdges(geom.Factory);
            Console.WriteLine(resultEdges.AsText());
            IGeometryCollection resultTriangles = dtb.GetTriangles(geom.Factory);
            Console.WriteLine(resultTriangles.AsText());
        }


        [TestAttribute /*, ExpectedException() */]
        public void TestInvertedItalicNTS()
        {
            AffineTransformationBuilder atb = new AffineTransformationBuilder(
                new Coordinate(0, 0),
                new Coordinate(50, 0),
                new Coordinate(0, 100),
                new Coordinate(0, 0),
                new Coordinate(50, 0),
                new Coordinate(20, 100));

            IGeometry geom = _wktReader.Read(NTS);

            //Apply italic effect
            geom = atb.GetTransformation().Transform(geom);
            Console.WriteLine(geom.AsText());

            //Setup 
            DelaunayTriangulationBuilder dtb = new DelaunayTriangulationBuilder();
            dtb.SetSites(geom);
            IMultiLineString result = dtb.GetEdges(geom.Factory);
            Console.WriteLine(result.AsText());
        }

        [TestAttribute]
        public void TestInvertedNTSConforming()
        {
            IGeometry geom = _wktReader.Read(NTS);
            Console.WriteLine(geom.AsText());

            IGeometry constraint = ((IPolygon)geom).GetInteriorRingN(0);
            constraint = geom.Factory.CreatePolygon((ILinearRing)constraint, null);
            constraint = ((IPolygon)constraint.Buffer(-1)).Shell;
            Coordinate[] coordinates = constraint.Coordinates;
            coordinates[coordinates.Length - 1].X -= 1e-7;
            coordinates[coordinates.Length - 1].Y -= 1e-7;

            constraint = geom.Factory.CreateLineString(coordinates);
            Console.WriteLine(constraint.AsText());

            //Setup 
            ConformingDelaunayTriangulationBuilder dtb = new ConformingDelaunayTriangulationBuilder { Constraints = constraint };
            dtb.SetSites(geom);
            IMultiLineString result = dtb.GetEdges(geom.Factory);
            Console.WriteLine(result.AsText());


        }

        [TestAttribute]
        public void TestInvertedItalicNTSConforming()
        {
            AffineTransformationBuilder atb = new AffineTransformationBuilder(
                new Coordinate(0, 0), new Coordinate(50, 0), new Coordinate(0, 100),
                new Coordinate(0, 0), new Coordinate(50, 0), new Coordinate(20, 100));

            IGeometry geom = _wktReader.Read(NTS);

            //Apply italic effect
            geom = atb.GetTransformation().Transform(geom);
            Console.WriteLine(geom.AsText());

            IGeometry constraint = ((IPolygon)geom).GetInteriorRingN(0);
            constraint = geom.Factory.CreatePolygon((ILinearRing)constraint, null);
            constraint = ((IPolygon)constraint.Buffer(-1)).Shell;
            Coordinate[] coordinates = constraint.Coordinates;
            coordinates[coordinates.Length - 1].X -= 1e-7;
            coordinates[coordinates.Length - 1].Y -= 1e-7;

            constraint = geom.Factory.CreateLineString(coordinates);
            Console.WriteLine(constraint.AsText());

            //Setup 
            ConformingDelaunayTriangulationBuilder dtb = new ConformingDelaunayTriangulationBuilder { Constraints = constraint };
            dtb.SetSites(geom);
            IMultiLineString result = dtb.GetEdges(geom.Factory);
            Console.WriteLine(result.AsText());


        }
    }
}