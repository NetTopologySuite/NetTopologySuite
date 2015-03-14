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
            PriorityQueue<int> q = new PriorityQueue<int>();
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
            PriorityQueue<double> q = new PriorityQueue<double>();
            addRandomItems(q, 100);
            CheckOrder(q);
        }

        private void addRandomItems(PriorityQueue<double> q, int num)
        {
            var random = new Random();

            for (int i = 0; i < num; i++)
            {
                q.Add(random.NextDouble());
            }
        }

        private void CheckOrder<T>(PriorityQueue<T> q)
            where T: struct, IComparable<T>
        {
            T curr = default(T);
            bool first = true;

            while (!q.IsEmpty())
            {
                T next = q.Poll();
                Console.WriteLine(next);
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