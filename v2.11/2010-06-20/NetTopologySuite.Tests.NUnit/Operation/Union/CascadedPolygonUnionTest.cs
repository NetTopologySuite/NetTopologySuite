using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    /**
     * Large-scale tests of {@link CascadedPolygonUnion}
     * using synthetic datasets.
     * 
     * @author mbdavis
     *
     */
    [TestFixture]
    public class CascadedPolygonUnionTest
    {
        [Test]
        public void TestBoxes()
        {
            runTest(new List<IGeometry<Coordinate>>(GeometryUtils.ReadWKT(
                    new String[] {
  				"POLYGON ((80 260, 200 260, 200 30, 80 30, 80 260))",
  				"POLYGON ((30 180, 300 180, 300 110, 30 110, 30 180))",
  				"POLYGON ((30 280, 30 150, 140 150, 140 280, 30 280))"
  			})),
                    CascadedPolygonUnionTester.MIN_SIMILARITY_MEAURE);
        }
        [Test]
        public void TestDiscs1()
        {
            IList<IGeometry<Coordinate>> geoms = CreateDiscs(5, 0.7);

            Console.WriteLine(GeometryUtils.GeometryFactory.BuildGeometry(geoms));

            runTest(geoms,
                    CascadedPolygonUnionTester.MIN_SIMILARITY_MEAURE);
        }

        [Test]
        public void TestDiscs2()
        {
            IList<IGeometry<Coordinate>> geoms = CreateDiscs(5, 0.55);

            Console.WriteLine(GeometryUtils.GeometryFactory.BuildGeometry(geoms));

            runTest(geoms,
                    CascadedPolygonUnionTester.MIN_SIMILARITY_MEAURE);
        }


        // TODO: add some synthetic tests

        private static CascadedPolygonUnionTester tester = new CascadedPolygonUnionTester();

        private void runTest(IList<IGeometry<Coordinate>> geoms, double minimumMeasure)
        {
            Assert.IsTrue(tester.Test(geoms, minimumMeasure));
        }

        private static IList<IGeometry<Coordinate>> CreateDiscs(int num, double radius)
        {
            List<IGeometry<Coordinate>> geoms = new List<IGeometry<Coordinate>>();
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    Coordinate pt = GeometryUtils.CoordFac.Create(i, j);
                    IGeometry<Coordinate> ptGeom = GeometryUtils.GeometryFactory.CreatePoint(pt);
                    IGeometry<Coordinate> disc = ptGeom.Buffer(radius);
                    geoms.Add(disc);
                }
            }
            return geoms;
        }
    }
}