using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class VertexSequencePackedRtreeTest
    {
        [Test]
        public void Test1()
        {
            var tree = CreateSPRtree(1, 1);
            CheckQuery(tree, 1, 1, 4, 4, result(0));
        }

        [Test]
        public void Test2()
        {
            var tree = CreateSPRtree(0, 0, 1, 1);
            CheckQuery(tree, 1, 1, 4, 4, result(1));
        }

        [Test]
        public void Test6()
        {
            var tree = CreateSPRtree(0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5);
            CheckQuery(tree, 2, 2, 4, 4, result(2, 3, 4));
            CheckQuery(tree, 0, 0, 0, 0, result(0));
        }

        [Test]
        public void Test10()
        {
            var tree = CreateSPRtree(0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10);
            CheckQuery(tree, 2, 2, 4, 4, result(2, 3, 4));
            CheckQuery(tree, 7, 7, 8, 8, result(7, 8));
            CheckQuery(tree, 0, 0, 0, 0, result(0));
        }

        [Test]
        public void Test6WithDups()
        {
            var tree = CreateSPRtree(0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1, 0, 0);
            CheckQuery(tree, 2, 2, 4, 4, result(2, 3, 4, 6, 7, 8));
            CheckQuery(tree, 0, 0, 0, 0, result(0, 10));
        }

        private void CheckQuery(VertexSequencePackedRtree tree,
            double xmin, double ymin, double xmax, double ymax, int[] expected)
        {
            var env = new Envelope(xmin, xmax, ymin, ymax);
            int[] result = tree.Query(env);
            Assert.That(result.Length, Is.EqualTo(expected.Length));
            Assert.That(isEqualResult(expected, result));
        }

        private bool isEqualResult(int[] expected, int[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                if (expected[i] != result[i])
                    return false;

            }
            return true;
        }

        private int[] result(params int[] i)
        {
            return i;
        }

        private VertexSequencePackedRtree CreateSPRtree(params int[] ords)
        {
            int numCoord = ords.Length / 2;
            var pt = new Coordinate[numCoord];
            for (int i = 0; i < numCoord; i++)
            {
                pt[i] = new Coordinate(ords[2 * i], ords[2 * i + 1]);
            }
            return new VertexSequencePackedRtree(pt);
        }
    }
}
