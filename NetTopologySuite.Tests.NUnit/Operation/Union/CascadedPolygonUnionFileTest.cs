using System;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    /// <summary>
    /// Large-scale tests of <see cref="CascadedPolygonUnion"/>
    /// using data from files.
    /// </summary>
    /// <author>mbdavis</author>
    //[TestFixture(Ignore = true, IgnoreReason = "The CascadedPolygonUnionTester class uses classes in NetTopologySuite.Algorithm.Match which have not been migrated to NTS yet")]
    public class CascadedPolygonUnionFileTest
    {
        [Test]
        public void TestAfrica()
        {
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile(
                "NetTopologySuite.Tests.NUnit.TestData.africa.wkt");

            RunTest(filePath,
                    CascadedPolygonUnionTester.MinSimilarityMeaure);

            EmbeddedResourceManager.CleanUpTempFile(filePath);
        }

        [Test]
        public void TestEurope()
        {
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile(
                "NetTopologySuite.Tests.NUnit.TestData.europe.wkt");

            RunTest(filePath,
                    CascadedPolygonUnionTester.MinSimilarityMeaure);

            EmbeddedResourceManager.CleanUpTempFile(filePath);
        }

        private static CascadedPolygonUnionTester tester = new CascadedPolygonUnionTester();

        private void RunTest(String filename, double minimumMeasure)
        {

            var geoms = GeometryUtils.ReadWKTFile(filename);
            Assert.IsTrue(tester.Test(geoms, minimumMeasure));
        }
    }
}
