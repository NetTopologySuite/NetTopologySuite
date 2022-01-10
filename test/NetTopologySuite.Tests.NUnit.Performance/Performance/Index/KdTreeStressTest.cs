using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    /// <summary>
    /// Tests an issue where deep KdTrees caused a {@link StackOverflowError}
    /// when using a recursive query implementation.
    /// </summary>
    /// <remarks>
    /// See a fix for this issue in GEOS
    /// at https://github.com/libgeos/geos/pull/481.
    /// </remarks>
    /// <author>Martin Davis</author>
    public class KdTreeStressTest
    {

        // In code with recursive query 50,000 points causes StackOverflowError
        int NUM_PTS = 50000;

        [Test]
        public void Run()
        {
            TestContext.WriteLine("Loading iIndex with {0} points", NUM_PTS);
            var index = CreateUnbalancedTree(NUM_PTS);

            TestContext.WriteLine("Querying Index loaded with {0} points", NUM_PTS);
            for (int i = 0; i < NUM_PTS; i++)
            {
                var env = new Envelope(i, i + 10, 0, 1);
                index.Query(env);
            }
            TestContext.WriteLine("Queries complete\n");
        }

        /**
         * Create an unbalanced tree by loading a 
         * series of monotonically increasing points
         * 
         * @param numPts number of points to load
         * @return a new index
         */
        private KdTree<Coordinate> CreateUnbalancedTree(int numPts)
        {
            var index = new KdTree<Coordinate>();
            for (int i = 0; i < numPts; i++)
            {
                var pt = new Coordinate(i, 0);
                index.Insert(pt);
            }
            return index;
        }
    }
}
