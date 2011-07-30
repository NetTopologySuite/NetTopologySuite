using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    [TestFixture]
    public class UnaryUnionTest
    {
        GeometryFactory geomFact = new GeometryFactory();

        [Ignore("More recent versions of JTS have an overloaded version of the UnaryUnionOp.Union method that takes a geometry factory, so this test is being ignored until that functionality is migrated to NTS.  Make sure to uncomment the code in DoTest below as well")]
        public void TestEmptyCollection()
        {
            DoTest(new String[] { }, "GEOMETRYCOLLECTION EMPTY");
        }

        [Test]
        public void TestPoints()
        {
            DoTest(new String[] { "POINT (1 1)", "POINT (2 2)" }, "MULTIPOINT ((1 1), (2 2))");
        }

        [Ignore("The UnaryUnionOp.UnionWithNull method acting on multipoints and multilines yields a geometry collection, which is then unable to be used a an argument to Geometry.Union.  There appears more recent logic in UnaryUnionOp.UnionWithNull in JTS that is not yet in NTS.")]
        public void TestAll()
        {
            DoTest(new String[] { "GEOMETRYCOLLECTION (POLYGON ((0 0, 0 90, 90 90, 90 0, 0 0)),   POLYGON ((120 0, 120 90, 210 90, 210 0, 120 0)),  LINESTRING (40 50, 40 140),  LINESTRING (160 50, 160 140),  POINT (60 50),  POINT (60 140),  POINT (40 140))" },
                "GEOMETRYCOLLECTION (POINT (60 140),   LINESTRING (40 90, 40 140), LINESTRING (160 90, 160 140), POLYGON ((0 0, 0 90, 40 90, 90 90, 90 0, 0 0)), POLYGON ((120 0, 120 90, 160 90, 210 90, 210 0, 120 0)))");
        }

        private void DoTest(String[] inputWKT, String expectedWKT)
        {
            IGeometry result;
            var geoms = GeometryUtils.ReadWKT(inputWKT);
            // TODO: There is no overload of Union that takes a geometry factory yet in NTS.  This should be uncommented when the functionality is introduced from JTS
            //if (geoms.Count == 0)
            //{
            //    result = UnaryUnionOp.Union(geoms, geomFact);
            //}
            //else
            //{
                result = UnaryUnionOp.Union(geoms);
            //}

            Assert.IsTrue(GeometryUtils.IsEqual(GeometryUtils.ReadWKT(expectedWKT), result));
        }
    }
}
