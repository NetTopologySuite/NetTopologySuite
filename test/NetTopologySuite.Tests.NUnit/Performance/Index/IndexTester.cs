using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    internal class IndexResult
    {
        public IndexResult(string indexName) { this.indexName = indexName; }
        public string indexName;
        public long loadMilliseconds;
        public long queryMilliseconds;
    }

    internal class IndexTester<T>
    {
        const int NUM_ITEMS = 2000;
        const double EXTENT_MIN = -1000.0;
        const double EXTENT_MAX = 1000.0;

        private readonly IIndex<T> _index;

        public IndexTester(IIndex<T> index)
        {
            _index = index;
        }

        internal IndexResult TestAll(IEnumerable<Tuple<Envelope, T>> items, IEnumerable<Envelope> queries)
        {
            var result = new IndexResult(_index.ToString());
            Console.Write(_index + "           ");
            GC.Collect();
            var sw = new Stopwatch();
            sw.Start();
            LoadTree(items);
            long loadTime = sw.ElapsedMilliseconds;
            result.loadMilliseconds = loadTime;
            GC.Collect();
            sw.Restart();
            //runGridQuery(1000);
            //runQuery(items);
            RunQuery(queries);
            long queryTime = sw.ElapsedMilliseconds;
            result.queryMilliseconds = queryTime;
            Console.WriteLine("  Load Time = " + loadTime + "  Query Time = " + queryTime);
            return result;
        }

        public static IList<Tuple<Envelope, T>> CreateGridItems(int nGridCells, Func<Envelope, T> createItem)
        {
            var items = new List<Tuple<Envelope, T>>();
            int gridSize = (int)Math.Sqrt((double)nGridCells);
            gridSize += 1;
            double extent = EXTENT_MAX - EXTENT_MIN;
            double gridInc = extent / gridSize;
            double cellSize = gridInc;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    double x = EXTENT_MIN + gridInc * i;
                    double y = EXTENT_MIN + gridInc * j;
                    var env = new Envelope(x, x + cellSize,
                                           y, y + cellSize);
                    items.Add(Tuple.Create(env, createItem(env)));
                }
            }
            return items;
        }

        public static IEnumerable<Envelope> CreateRandomBoxes(int n)
        {
            const int seed = 613;
            return CreateRandomBoxes(seed, n);
        }

        public static IEnumerable<Tuple<Envelope,T>> CreateRandomItems(int n, Func<Random, T> create)
        {
            const int seed = 613;
            return CreateRandomItems(seed, n, create);
        }

        public static IEnumerable<Envelope> CreateRandomBoxes(int seed, int n)
        {
            var random = new Random(seed);
            for (int i = 0; i < n; i++)
                yield return CreateBox(random);
        }

        public static IEnumerable<Tuple<Envelope, T>> CreateRandomItems(int seed, int n, Func<Random, T> create)
        {
            var random = new Random(seed);
            for (int i = 0; i < n; i++)
                yield return Tuple.Create(CreateBox(random), create(random));
        }

        private static Envelope CreateBox(Random random)
        {
            double minX = RandomDouble(random, -100, 100);
            double minY = RandomDouble(random, -100, 100);
            double sizeX = RandomDouble(random, 0.0, 10);
            double sizeY = RandomDouble(random, 0.0, 10);
            return new Envelope(minX, minX + sizeX, minY, minY + sizeY);
        }

        private static double RandomDouble(Random random, double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }

        private void LoadTree(IEnumerable<Tuple<Envelope, T>> items)
        {
            foreach (var item in items)
            {
                _index.Insert(item.Item1, item.Item2);
            }
            _index.FinishInserting();
        }

        private void RunQuery(IEnumerable<Envelope> items)
        {
            double querySize = 0.0;
            int count = 0;
            foreach (var env in items)
            {
                var list = _index.Query(env);
                Assert.Greater(list.Count, 0);
                querySize += list.Count;
                count++;
            }
            Console.WriteLine($"Avg query size = {querySize / count}");
        }

        void runGridQuery(int nGridCells)
        {
            int cellSize = (int)Math.Sqrt((double)NUM_ITEMS);
            double extent = EXTENT_MAX - EXTENT_MIN;
            double queryCellSize = 2.0 * extent / cellSize;

            QueryGrid(nGridCells, queryCellSize);
        }

        void QueryGrid(int nGridCells, double cellSize)
        {

            int gridSize = (int)Math.Sqrt((double)nGridCells);
            gridSize += 1;
            double extent = EXTENT_MAX - EXTENT_MIN;
            double gridInc = extent / gridSize;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    double x = EXTENT_MIN + gridInc * i;
                    double y = EXTENT_MIN + gridInc * j;
                    var env = new Envelope(x, x + cellSize,
                                           y, y + cellSize);
                    _index.Query(env);
                }
            }
        }

    }

}
