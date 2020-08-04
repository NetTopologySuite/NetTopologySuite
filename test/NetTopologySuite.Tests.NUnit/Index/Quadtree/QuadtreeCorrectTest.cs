using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Quadtree
{
    public class QuadtreeCorrectTest
    {
        /*
          public static void testBinaryPower()
          {
            printBinaryPower(1004573397.0);
            printBinaryPower(100.0);
            printBinaryPower(0.234);
            printBinaryPower(0.000003455);
          }
    
          public static void printBinaryPower(double num)
          {
            BinaryPower pow2 = new BinaryPower();
            int exp = BinaryPower.exponent(num);
            double p2 = pow2.power(exp);
            System.out.println(num + " : pow2 = " +  Math.pow(2.0, exp)
                + "   exp = " + exp + "   2^exp = " + p2);
          }
        */
        const int NUM_ITEMS = 2000;
        const double MIN_EXTENT = -1000.0;
        const double MAX_EXTENT = 1000.0;

        EnvelopeList envList = new EnvelopeList();
        Quadtree<Envelope> q = new Quadtree<Envelope>();

        [Test]
        public void Tun()
        {
            Fill();
            Console.WriteLine($"depth = {q.Depth},  size = {q.Count}");
            RunQueries();
        }

        void Fill()
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
                for (int j = 0; j < gridSize; j++)
                {
                    double x = MIN_EXTENT + gridInc * i;
                    double y = MIN_EXTENT + gridInc * j;
                    var env = new Envelope(x, x + cellSize,
                        y, y + cellSize);
                    q.Insert(env, env);
                    envList.Add(env);
                }
            }
        }

        void RunQueries()
        {
            int nGridCells = 100;
            int cellSize = (int) Math.Sqrt((double) NUM_ITEMS);
            double extent = MAX_EXTENT - MIN_EXTENT;
            double queryCellSize = 2.0 * extent / cellSize;

            queryGrid(nGridCells, queryCellSize);

            //queryGrid(200);
        }

        void queryGrid(int nGridCells, double cellSize)
        {
            var sw = new Stopwatch();
            sw.Start();

            int gridSize = (int) Math.Sqrt((double) nGridCells);
            gridSize += 1;
            double extent = MAX_EXTENT - MIN_EXTENT;
            double gridInc = extent / gridSize;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    double x = MIN_EXTENT + gridInc * i;
                    double y = MIN_EXTENT + gridInc * j;
                    var env = new Envelope(x, x + cellSize,
                        y, y + cellSize);
                    QueryTest(env);
                    //queryTime(env);
                }
            }

            Console.WriteLine($"Time = {sw.ElapsedMilliseconds}ms.");
        }

        void QueryTime(Envelope env)
        {
            //List finalList = getOverlapping(q.query(env), env);

            var eList = envList.Query(env);
        }

        void QueryTest(Envelope env)
        {
            var candidateList = q.Query(env);
            var finalList = GetOverlapping(candidateList, env);

            var eList = envList.Query(env);
            //System.out.println(finalList.size());

            if (finalList.Count != eList.Count)
                throw new AssertionException("queries do not match");
        }

        private IList<Envelope> GetOverlapping(IList<Envelope> items, Envelope searchEnv)
        {
            var result = new List<Envelope>();
            for (int i = 0; i < items.Count; i++)
            {
                var env = items[i];
                if (env.Intersects(searchEnv)) result.Add(env);
            }

            return result;
        }

        private class EnvelopeList
        {
            private readonly List<Envelope> _envList = new List<Envelope>();

            public void Add(Envelope env)
            {
                _envList.Add(env);
            }

            public IList<Envelope> Query(Envelope searchEnv)
            {
                var result = new List<Envelope>();
                foreach (var env in _envList)
                {
                    if (env.Intersects(searchEnv))
                        result.Add(env);
                }

                return result;
            }


        }
    }
}
