using System;
using NUnit.Framework;
using NetTopologySuite.Utilities;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    [TestFixtureAttribute]
    public class PriorityQueueTest
    {
        [TestAttribute]
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

        [TestAttribute]
        public void TestOrderRandom1()
        {
            PriorityQueue<int> q = new PriorityQueue<int>();
            addRandomItems(q, 100);
            CheckOrder(q);
        }

        private void addRandomItems(PriorityQueue<int> q, int num)
        {
            var random = new Random();

            for (int i = 0; i < num; i++)
            {
                q.Add((int)(num * random.NextDouble()));
            }
        }

        private void CheckOrder<T>(PriorityQueue<T> q)
            where T: IComparable<T>
        {
            IComparable<T> curr = null;

            while (!q.IsEmpty())
            {
                IComparable<T> next = (IComparable<T>)q.Poll();
                Console.WriteLine(next);
                if (curr == null)
                    curr = next;
                else
                    Assert.IsTrue(next.CompareTo((T)curr) >= 0);
            }
        }
    }
}