using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    /// <summary>
    /// Large-scale tests of <see cref="CascadedPolygonUnion"/>
    /// using synthetic datasets.
    /// </summary>
    /// <author>mbdavis</author>
    public class CascadedPolygonUnionTest
    {
        readonly GeometryFactory _geomFact = new GeometryFactory();

        [TestAttribute]
        public void TestBoxes()
        {
            RunTest(GeometryUtils.ReadWKT(
                    new[] {
  			            "POLYGON ((80 260, 200 260, 200 30, 80 30, 80 260))",
  			            "POLYGON ((30 180, 300 180, 300 110, 30 110, 30 180))",
  			            "POLYGON ((30 280, 30 150, 140 150, 140 280, 30 280))"
  		            }),
                    CascadedPolygonUnionTester.MinSimilarityMeaure);
        }

        [TestAttribute]
        public void TestDiscs1()
        {
            var geoms = CreateDiscs(5, 0.7);

            Console.WriteLine(_geomFact.BuildGeometry(geoms));

            RunTest(geoms,
  		            CascadedPolygonUnionTester.MinSimilarityMeaure);
        }

        [TestAttribute]
        public void TestDiscs2()
        {
            var geoms = CreateDiscs(5, 0.55);

            Console.WriteLine(_geomFact.BuildGeometry(geoms));

            RunTest(geoms,
  		            CascadedPolygonUnionTester.MinSimilarityMeaure);
        }


        // TODO: add some synthetic tests

        private static CascadedPolygonUnionTester tester = new CascadedPolygonUnionTester();

        private void RunTest(IList<IGeometry> geoms, double minimumMeasure)
        {
            Assert.IsTrue(tester.Test(geoms, minimumMeasure));
        }

        private IList<IGeometry> CreateDiscs(int num, double radius)
        {
            var geoms = new List<IGeometry>();
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    Coordinate pt = new Coordinate(i, j);
                    IGeometry ptGeom = _geomFact.CreatePoint(pt);
                    IGeometry disc = ptGeom.Buffer(radius);
                    geoms.Add(disc);
                }
            }
            return geoms;
        }
    }
}
