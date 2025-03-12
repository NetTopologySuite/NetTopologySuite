using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    [TestFixture, Category("Stress")]
    internal class FlatBushPerfTest : PerformanceTestCase
    {
        private const int NUM_ITEMS = 1_000_000;
        private const int NUM_QUERIES = 1_000;
        private const int RANDOM_SEED = 13;

        private Envelope[] _items;
        private Envelope[] _queries;
        private HPRtree<Envelope> _hprtree;
        private STRtree<Envelope> _strtree;

        public FlatBushPerfTest() : base(nameof(FlatBushPerfTest))
        {
            RunSize = new int[] { 1, 10, (int)(100 * Math.Sqrt(0.1)) };
            RunIterations = 1;
        }

        private static Envelope RandomBox(Random random, double boxSize)
        {
            double x = random.NextDouble() * (100d - boxSize);
            double y = random.NextDouble() * (100d - boxSize);
            double x2 = x + random.NextDouble() * boxSize;
            double y2 = y + random.NextDouble() * boxSize;
            return new Envelope(x, x2, y, y2);
        }

        public override void SetUp()
        {
            base.SetUp();

            var random = new Random(RANDOM_SEED);
            _items = new Envelope[NUM_ITEMS];

            for (int i = 0; i < NUM_ITEMS; i++)
            {
                _items[i] = RandomBox(random, 1);
            }

            // warmup the jvm by building once and running queries
            WarmupQueries(CreateIndex(new HPRtree<Envelope>(), u => u.Build()));
            WarmupQueries(CreateIndex(new STRtree<Envelope>(), u => u.Build()));

            var sw = new Stopwatch();
            sw.Start();
            _hprtree = CreateIndex(new HPRtree<Envelope>(), u => u.Build());
            sw.Stop();
            TestContext.WriteLine($"HPRTree Build time = {sw.ElapsedMilliseconds}ms.");

            sw.Restart();
            _strtree = CreateIndex(new STRtree<Envelope>(), u => u.Build());
            sw.Stop();
            TestContext.WriteLine($"STRTree Build time = {sw.ElapsedMilliseconds}ms.");
        }

        private T CreateIndex<T>(T index, Action<T> builder) where T : ISpatialIndex<Envelope>
        {
            foreach (var env in _items)
            {
                index.Insert(env, env);
            }
            builder.Invoke(index);
            return index;
        }

        private static void WarmupQueries(ISpatialIndex<Envelope> index)
        {
            var random = new Random(RANDOM_SEED);
            var visitor = new CountItemVisitor<Envelope>();
            for (int i = 0; i < NUM_QUERIES; i++)
            {
                index.Query(RandomBox(random, 1), visitor);
            }
        }

        public override void StartRun(int size)
        {
            TestContext.WriteLine($"----- Query size: {size}");
            var random = new Random(RANDOM_SEED);
            _queries = new Envelope[NUM_QUERIES];
            for (int i = 0; i < NUM_QUERIES; i++)
            {
                _queries[i] = RandomBox(random, size);
            }
        }

        public void RunQueriesHPR()
        {
            var visitor = new CountItemVisitor<Envelope>();
            foreach (var box in _queries)
            {
                _hprtree.Query(box, visitor);
            }
            TestContext.WriteLine($"HPRTree query result {visitor.Count} items");
        }

        public void RunQueriesSTR()
        {
            var visitor = new CountItemVisitor<Envelope>();
            foreach (var box in _queries)
            {
                _strtree.Query(box, visitor);
            }
            TestContext.WriteLine($"STRTree query result {visitor.Count} items");
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(FlatBushPerfTest));

            //var sw = new Stopwatch();
            //RunQueriesSTR();
            //sw.Stop();
            //TestContext.WriteLine($"HPRTree Build time = {sw.ElapsedMilliseconds}ms.");
            //sw.Restart();
            //RunQueriesHPR();
            //sw.Stop();
            //TestContext.WriteLine($"STRTree Query time = {sw.ElapsedMilliseconds}ms.");
        }
    }
}
