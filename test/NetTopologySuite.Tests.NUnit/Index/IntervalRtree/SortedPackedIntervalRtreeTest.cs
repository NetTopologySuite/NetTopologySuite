using NetTopologySuite.Index;
using NetTopologySuite.Index.IntervalRTree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.IntervalRtree
{
    public class SortedPackedIntervalRtreeTest
    {
        /**
         * See JTS GH Issue #19.
         * Used to infinite-loop on empty geometries.
         * 
         */
        [Test]
        public void TestEmpty()
        {
            var spitree = new SortedPackedIntervalRTree<object>();
            var visitor = new ArrayListVisitor<object>();
            Assert.That(() => spitree.Query(0, 1, visitor), Throws.Nothing);
        }
    }
}
