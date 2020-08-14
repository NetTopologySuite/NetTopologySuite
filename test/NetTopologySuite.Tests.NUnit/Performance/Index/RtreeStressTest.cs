using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    public class RtreeStressTest
    {

        private const int NUM_ITEMS = 1000;
        private const int NUM_QUERY = 100000;

        private const double BASE_MIN = -1000;
        private const double BASE_MAX = 1000;
        private const double SIZE_MAX = 100;


        HPRtree<string> hpRtree;
        STRtree<string> stRtree;

        [Test, Category("Stress")]
        public void Run()
        {
            hpRtree = new HPRtree<string>();
            stRtree = new STRtree<string>();

            //loadRandom(NUM_ITEMS);
            LoadGrid(NUM_ITEMS);

            var sw = new Stopwatch();
            sw.Start();
            stRtree.Build();
            sw.Stop();
            Console.WriteLine($"STRtree build time: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            hpRtree.Build();
            sw.Stop();
            Console.WriteLine($"HPRtree build time: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            for (int i = 0; i < NUM_QUERY; i++)
            {
                QueryRandom();
            }

            sw.Stop();
            Console.WriteLine($"Query time: {sw.ElapsedMilliseconds}ms");
        }

        private void QueryRandom()
        {
            var env = RandomEnvelope(BASE_MIN, BASE_MAX, 10 * SIZE_MAX);

            var hpVisitor = new CountItemVisitor<object>();
            hpRtree.Query(env, hpVisitor);

            //List hpResult = hpRtree.query(env);
            IList<string> hprResult = null;

            //CountItemVisitor stVisitor = new CountItemVisitor();
            //stRtree.query(env, stVisitor);

            //List strResult = stRtree.query(env);
            IList<string> strResult = null;

            CheckResults(hprResult, strResult);
        }

        private void CheckResults(IList<string> hprResult, IList<string> strResult)
        {
            if (hprResult == null) return;
            if (strResult == null) return;

            Console.WriteLine($"Result size: HPR = {hprResult.Count} - STR = {strResult.Count}");

            if (hprResult.Count != strResult.Count)
            {
                Console.WriteLine("Result sizes are not equal!");
            }

        }

        private void LoadRandom(int numItems)
        {
            for (int i = 0; i < numItems; i++)
            {
                var env = RandomEnvelope(BASE_MIN, BASE_MAX, SIZE_MAX);
                Insert(env, i + "");
            }
        }

        private void LoadGrid(int numItems)
        {
            int numSide = (int) Math.Sqrt(numItems);
            double gridSize = (BASE_MAX - BASE_MIN) / numSide;
            for (int i = 0; i < numSide; i++)
            {
                for (int j = 0; j < numSide; j++)
                {
                    var env = new Envelope(
                        BASE_MIN, BASE_MIN + i * gridSize,
                        BASE_MIN, BASE_MIN + j * gridSize);
                    Insert(env, i + "-" + j);
                }
            }
        }

        private static Envelope RandomEnvelope(double baseMin, double baseMax, double size)
        {
            double x = Random(baseMin, baseMax);
            double y = Random(baseMin, baseMax);
            double sizeX = Random(size);
            double sizeY = Random(size);
            var env = new Envelope(x, x + sizeX, y, y + sizeY);
            return env;
        }

        private void Insert(Envelope env, string id)
        {
            hpRtree.Insert(env, id);
            stRtree.Insert(env, id);
        }

        private static readonly Random RND = new Random(613);

        private static double Random(double x)
        {
            return x * RND.NextDouble();
        }

        private static double Random(double x1, double x2)
        {
            double del = x2 - x1;
            return x1 + del * RND.NextDouble();
        }
    }
}
