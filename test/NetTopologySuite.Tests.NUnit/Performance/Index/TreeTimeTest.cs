using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    public class TreeTimeTest
    {
        public const int NUM_ITEMS = 100000;

        [Test]
        public void TestWithObject()
        {
            int n = NUM_ITEMS;
            //var items = IndexTester<object>.CreateGridItems(n, t => new object());
            var items = IndexTester<object>.CreateRandomItems(n, x => new object()).ToArray();
            var queries = IndexTester<object>.CreateRandomBoxes(n).ToArray();

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Dummy run to ensure classes are loaded before real run");
            Console.WriteLine("----------------------------------------------");
            Run(items, queries);
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Real run");
            Console.WriteLine("----------------------------------------------");
            Run(items, queries);
        }

        private IList<IndexResult> Run<T>(IList<Tuple<Envelope, T>> items, IList<Envelope> queries)
        {
            var indexResults = new List<IndexResult>();
            Console.WriteLine($"# items = {items.Count}");
            indexResults.Add(Run(new HPRtreeIndex<T>(16), items, queries));
            indexResults.Add(Run(new STRtreeIndex<T>(4), items, queries));
            //indexResults.Add(Run(new QuadtreeIndex<T>(), items));
            //indexResults.add(run(new QXtreeIndex(), n));
            //indexResults.Add(Run(new EnvelopeListIndex(), items.Select(t => Tuple.Create(t.Item1, t.Item1)).ToArray()));
            return indexResults;
        }

        private IndexResult Run<T>(IIndex<T> index, IList<Tuple<Envelope, T>> items, IList<Envelope> queries)
        {
            return new IndexTester<T>(index).TestAll(items, queries);
        }

        private class STRtreeIndex<T> : IIndex<T>
        {
            public STRtreeIndex(int nodeCapacity)
            {
                _index = new STRtree<T>(nodeCapacity);
            }

            private readonly STRtree<T> _index;

            public void Insert(Envelope itemEnv, T item)
            {
                _index.Insert(itemEnv, item);
            }

            public IList<T> Query(Envelope searchEnv)
            {
                return _index.Query(searchEnv);
            }

            public void FinishInserting()
            {
                _index.Build();
            }

            public override string ToString()
            {
                return $"STR[M={_index.NodeCapacity}]";
            }
        }

        private class HPRtreeIndex<T> : IIndex<T>
        {
            public HPRtreeIndex(int nodeCapacity)
            {
                _index = new HPRtree<T>(nodeCapacity);
                _nodeCapacity = nodeCapacity;
            }

            private readonly HPRtree<T> _index;
            private readonly int _nodeCapacity;

            public void Insert(Envelope itemEnv, T item)
            {
                _index.Insert(itemEnv, item);
            }

            public IList<T> Query(Envelope searchEnv)
            {
                return _index.Query(searchEnv);
            }

            public void FinishInserting()
            {
                _index.Build();
            }

            public override string ToString()
            {
                return $"HPR[M={_nodeCapacity}]";
            }
        }


        class QuadtreeIndex<T> : IIndex<T>
        {
            private readonly Quadtree<T> index = new Quadtree<T>();

            public void Insert(Envelope itemEnv, T item)
            {
                index.Insert(itemEnv, item);
            }

            public IList<T> Query(Envelope searchEnv)
            {
                return index.Query(searchEnv);
            }

            public void FinishInserting()
            {
            }

            public override string ToString()
            {
                return "Quad";
            }

        }

        class EnvelopeListIndex : IIndex<Envelope>
        {
            private readonly EnvelopeList _index = new EnvelopeList();

            public void Insert(Envelope itemEnv, Envelope item)
            {
                _index.Add(itemEnv);
            }

            public IList<Envelope> Query(Envelope searchEnv)
            {
                return _index.Query(searchEnv);
            }

            public void FinishInserting()
            {
            }

            public override string ToString()
            {
                return "Env";
            }
        }


    }
}
