#nullable disable
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
        [Test]
        [Category("LongRunning")]
        public void TestAfrica()
        {
            var africa = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.africa.wkt");
            RunTest(africa, CascadedPolygonUnionTester.MinSimilarityMeaure);
        }

        [Test]
        [Category("LongRunning")]
        [Explicit("takes ages to complete")]
        public void TestEurope()
        {
            var europe = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.europe.wkt");
            RunTest(europe, CascadedPolygonUnionTester.MinSimilarityMeaure);
        }

        private static readonly CascadedPolygonUnionTester Tester = new CascadedPolygonUnionTester();

        private static void RunTest(Stream stream, double minimumMeasure)
        {
            var geoms = GeometryUtils.ReadWKTFile(stream);
            Assert.IsTrue(Tester.Test(geoms, minimumMeasure));
        }
    }
}
