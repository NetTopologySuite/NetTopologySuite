using System;
using NUnit.Framework;
using NetTopologySuite.Utilities;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    [TestFixture]
    public class PriorityQueueTest
    {
        [Test]
        public void TestOrder1()
        {
            var q = new PriorityQueue<int>();
            q.Add(1);
            q.Add(10);
            q.Add(5);
            q.Add(8);
            q.Add(-1);
            CheckOrder(q);
        }

        [Test]
        public void TestOrderRandom1()
        {
            var q = new PriorityQueue<int>();
            addRandomItems(q, 100);
            CheckOrder(q);
        }

        private void addRandomItems(PriorityQueue<int> q, int num)
        {
            var random = new Random();

            for (int i = 0; i < num; i++)
            {
                // This usually inserts lots of duplicate values in an order
                // that *tends* to be increasing, but usually has some values
                // that should bubble up near the top of the heap.
                q.Add((int)(num * random.NextDouble()));
            }
        }

        private void CheckOrder<T>(PriorityQueue<T> q)
            where T: struct, IComparable<T>
        {
            var curr = default(T);
            bool first = true;

            while (!q.IsEmpty())
            {
                var next = q.Poll();
                //System.Console.WriteLine(next);
                if (!first)
                {
                    Assert.IsTrue(next.CompareTo(curr) >= 0);
                }

                first = false;
                curr = next;
            }
        }
    }
}