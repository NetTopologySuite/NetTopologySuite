using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    /// <summary>
    /// Large-scale tests of <see cref="CascadedPolygonUnion"/>
    /// using synthetic datasets.
    /// </summary>
    /// <author>mbdavis</author>
    [TestFixture(Ignore = true, IgnoreReason = "The CascadedPolygonUnionTester class uses classes in NetTopologySuite.Algorithm.Match which have not been migrated to NTS yet")]
    public class CascadedPolygonUnionTest
    {
        GeometryFactory geomFact = new GeometryFactory();

        [Test]
        public void TestBoxes()
        {
            RunTest(GeometryUtils.ReadWKT(
                    new String[] {
  			            "POLYGON ((80 260, 200 260, 200 30, 80 30, 80 260))",
  			            "POLYGON ((30 180, 300 180, 300 110, 30 110, 30 180))",
  			            "POLYGON ((30 280, 30 150, 140 150, 140 280, 30 280))"
  		            }),
                    CascadedPolygonUnionTester.MIN_SIMILARITY_MEAURE);
        }

        [Test]
        public void TestDiscs1()
        {
            var geoms = CreateDiscs(5, 0.7);

            Console.WriteLine(geomFact.BuildGeometry(geoms));

            RunTest(geoms,
  		            CascadedPolygonUnionTester.MIN_SIMILARITY_MEAURE);
        }

        [Test]
        public void TestDiscs2()
        {
            var geoms = CreateDiscs(5, 0.55);

            Console.WriteLine(geomFact.BuildGeometry(geoms));

            RunTest(geoms,
  		            CascadedPolygonUnionTester.MIN_SIMILARITY_MEAURE);
        }


        // TODO: add some synthetic tests

        private static CascadedPolygonUnionTester tester = new CascadedPolygonUnionTester();

        private void RunTest(IList<IGeometry> geoms, double minimumMeasure)
        {
            // TODO: Need to uncomment once the NetTopologySuite.Algorithm.Match namespace and classes are migrated to NTS
            //Assert.IsTrue(tester.Test(geoms, minimumMeasure));
        }

        private IList<IGeometry> CreateDiscs(int num, double radius)
        {
            var geoms = new List<IGeometry>();
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    ICoordinate pt = new Coordinate(i, j);
                    IGeometry ptGeom = geomFact.CreatePoint(pt);
                    IGeometry disc = ptGeom.Buffer(radius);
                    geoms.Add(disc);
                }
            }
            return geoms;
        }
    }
}
