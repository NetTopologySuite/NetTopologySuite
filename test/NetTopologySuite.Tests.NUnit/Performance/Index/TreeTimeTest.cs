using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    public class TreeTimeTest
    {
        public const int NUM_ITEMS = 10000;

        [Test]
        public void Test()
        {
            int n = 10000;
            var items = IndexTester<object>.CreateGridItems(n, t => new object());
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Dummy run to ensure classes are loaded before real run");
            Console.WriteLine("----------------------------------------------");
            Run(items);
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Real run");
            Console.WriteLine("----------------------------------------------");
            Run(items);
        }

        private IList<IndexResult> Run<T>(IList<Tuple<Envelope, T>> items)
        {
            var indexResults = new List<IndexResult>();
            Console.WriteLine($"# items = {items.Count}");
            indexResults.Add(Run(new QuadtreeIndex<T>(), items));
            indexResults.Add(Run(new STRtreeIndex<T>(10), items));
            //indexResults.add(run(new QXtreeIndex(), n));
            indexResults.Add(Run(new EnvelopeListIndex(), items.Select(t => Tuple.Create(t.Item1, t.Item1)).ToArray()));
            return indexResults;
        }

        private IndexResult Run<T>(IIndex<T> index, IList<Tuple<Envelope, T>> items)
        {
            return new IndexTester<T>(index).TestAll(items);
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
