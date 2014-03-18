using System;
using System.IO;
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
    public class CascadedPolygonUnionFileTest
    {
        [TestAttribute]
        [CategoryAttribute("LongRunning")]
        public void TestAfrica()
        {
#if !PCL
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile(
                "NetTopologySuite.Tests.NUnit.TestData.africa.wkt");

            RunTest(filePath,
                    CascadedPolygonUnionTester.MinSimilarityMeaure);

            EmbeddedResourceManager.CleanUpTempFile(filePath);
#else
            var africa = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.africa.wkt");
            RunTest(africa, CascadedPolygonUnionTester.MinSimilarityMeaure);
#endif
        }

        [TestAttribute]
        [CategoryAttribute("LongRunning")]
        [Explicit("takes ages to complete")]
        public void TestEurope()
        {
#if !PCL
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile(
                "NetTopologySuite.Tests.NUnit.TestData.europe.wkt");

            RunTest(filePath,
                    CascadedPolygonUnionTester.MinSimilarityMeaure);

            EmbeddedResourceManager.CleanUpTempFile(filePath);
#else
            var europe = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.europe.wkt");
            RunTest(europe, CascadedPolygonUnionTester.MinSimilarityMeaure);
#endif
        }

        private static readonly CascadedPolygonUnionTester Tester = new CascadedPolygonUnionTester();


        private static void RunTest(String filename, double minimumMeasure)
        {
            var geoms = GeometryUtils.ReadWKTFile(filename);
            Assert.IsTrue(Tester.Test(geoms, minimumMeasure));
        }

#if PCL
        private static void RunTest(Stream stream, double minimumMeasure)
        {

            var geoms = GeometryUtils.ReadWKTFile(stream);
            Assert.IsTrue(Tester.Test(geoms, minimumMeasure));
        }
#endif
    }
}
