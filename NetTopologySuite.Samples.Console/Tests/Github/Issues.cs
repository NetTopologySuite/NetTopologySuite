using GeoAPI.Geometries;
using NetTopologySuite.Index.KdTree;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [NUnit.Framework.TestFixture]
    public class Issues
    {
        [Test(Description = "GitHub pull request #97")]
        public void TestNearestNeighbor2()
        {
            var kd = new KdTree<string>();
            const int Count = 8;

            for (var row = 0; row < Count; ++row)
            {
                for (var column = 0; column < Count; ++column)
                {
                    kd.Insert(new Coordinate(column, row), (column * 100 + row).ToString());
                }
            }

            var testCoordinate = new Coordinate(Count / 2, Count / 2);
            var res = kd.NearestNeighbor(testCoordinate);

            Assert.AreEqual(testCoordinate, res.Coordinate);
        }
    }
}