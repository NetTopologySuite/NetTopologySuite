using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Index.Bintree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Bintree
{
    public class BinTreeCorrectTest
    {

        const int NUM_ITEMS = 20000;
        const double MIN_EXTENT = -1000.0;
        const double MAX_EXTENT = 1000.0;

        private readonly IntervalList _intervalList = new IntervalList();
        private readonly Bintree<Interval> _btree = new Bintree<Interval>();


        [Test]
        public void Run()
        {
            Fill();
            TestContext.WriteLine($"depth = {_btree.Depth},  size = {_btree.Count}");
            RunQueries();
        }

        private void Fill()
        {
            CreateGrid(NUM_ITEMS);
        }

        void CreateGrid(int nGridCells)
        {
            int gridSize = (int) Math.Sqrt((double) nGridCells);
            gridSize += 1;
            double extent = MAX_EXTENT - MIN_EXTENT;
            double gridInc = extent / gridSize;
            double cellSize = 2 * gridInc;

            for (int i = 0; i < gridSize; i++)
            {
                double x = MIN_EXTENT + gridInc * i;
                var interval = new Interval(x, x + cellSize);
                _btree.Insert(interval, interval);
                _intervalList.Add(interval);
            }
        }

        void RunQueries()
        {
            int nGridCells = 100;
            int cellSize = (int) Math.Sqrt((double) NUM_ITEMS);
            double extent = MAX_EXTENT - MIN_EXTENT;
            double queryCellSize = 2.0 * extent / cellSize;

            QueryGrid(nGridCells, queryCellSize);

            //queryGrid(200);
        }

        void QueryGrid(int nGridCells, double cellSize)
        {
            var sw = new Stopwatch();
            sw.Start();

            int gridSize = (int) Math.Sqrt((double) nGridCells);
            gridSize += 1;
            double extent = MAX_EXTENT - MIN_EXTENT;
            double gridInc = extent / gridSize;

            for (int i = 0; i < gridSize; i++)
            {
                double x = MIN_EXTENT + gridInc * i;
                var interval = new Interval(x, x + cellSize);
                QueryTest(interval);
                //queryTime(env);
            }

            TestContext.WriteLine($"Time = {sw.ElapsedMilliseconds}ms.");
        }

        void QueryTime(Interval interval)
        {
            //List finalList = getOverlapping(q.query(env), env);

            var eList = _intervalList.Query(interval);
        }

        void QueryTest(Interval interval)
        {
            var candidateList = _btree.Query(interval);
            var finalList = GetOverlapping(candidateList, interval);

            var eList = _intervalList.Query(interval);
            TestContext.WriteLine(finalList.Count);

            if (finalList.Count != eList.Count)
                throw new AssertionException("queries do not match");
        }

        private IList<Interval> GetOverlapping(IList<Interval> items, Interval searchInterval)
        {
            var result = new List<Interval>();
            for (int i = 0; i < items.Count; i++)
            {
                var interval = (Interval) items[i];
                if (interval.Overlaps(searchInterval)) result.Add(interval);
            }

            return result;
        }



        private class IntervalList
        {
            private readonly List<Interval> _list = new List<Interval>();

            public void Add(Interval interval)
            {
                _list.Add(interval);
            }

            public IList<Interval> Query(Interval searchInterval)
            {
                var result = new List<Interval>();
                foreach (var interval in _list)
                {
                    if (interval.Overlaps(searchInterval))
                        result.Add(interval);
                }

                return result;
            }


        }
    }
}
