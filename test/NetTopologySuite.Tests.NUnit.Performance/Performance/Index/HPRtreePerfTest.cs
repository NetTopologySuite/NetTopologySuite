using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.HPRtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    [TestFixture, Category("Stress")]
    public class HPRtreePerfTest : PerformanceTestCase
    {
        private const int NODE_SIZE = 32;
        private const int ITEM_ENV_SIZE = 10;
        private const int QUERY_ENV_SIZE = 40;

        private HPRtree<object> _index;

        public HPRtreePerfTest() : base(nameof(HPRtreePerfTest))
        {
            RunSize = new [] { 100, 10000, 100000 };
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(HPRtreePerfTest));
        }

        public override void StartRun(int size)
        {
            Console.WriteLine($"----- Tree size: {size}");

            _index = new HPRtree<object>(NODE_SIZE);
            int side = (int)Math.Sqrt(size);
            LoadGrid(side, _index);

            var sw = new Stopwatch();
            sw.Start();
            _index.Build();
            sw.Stop();
            Console.WriteLine($"Build time = {sw.ElapsedMilliseconds}ms");
        }

        private static void LoadGrid(int side, ISpatialIndex<object> index)
        {
            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    var env = new Envelope(i, i + ITEM_ENV_SIZE, j, j + ITEM_ENV_SIZE);
                    index.Insert(env, i + "-" + j);
                }
            }
        }

        public void RunQueries()
        {
            var visitor = new CountItemVisitor<object>();

            int size = _index.Count;
            int side = (int)Math.Sqrt(size);
            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    var env = new Envelope(i, i + QUERY_ENV_SIZE, j, j + QUERY_ENV_SIZE);
                    _index.Query(env, visitor);
                }
            }
            Console.WriteLine($"Total query result items = {visitor.Count}");
        }
    }
}
