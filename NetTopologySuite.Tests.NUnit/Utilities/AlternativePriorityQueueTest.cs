using System;
using System.Collections.Generic;
using NUnit.Framework;
using NetTopologySuite.Utilities;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    [TestFixture]
    public class AlternativePriorityQueueTest
    {
        [Test]
        public void TestEnumeration()
        {
            const int NodeCount = 18;
            AlternativePriorityQueue<double, object> q = new AlternativePriorityQueue<double, object>(NodeCount);

            var random = new Random();
            var nodes = new PriorityQueueNode<double, object>[NodeCount];

            for (int i = 0; i < NodeCount; i++)
            {
                nodes[i] = new PriorityQueueNode<double, object>((object)null);
                q.Enqueue(nodes[i], random.NextDouble());
            }

            CollectionAssert.AreEquivalent(nodes, q);
        }

        [Test]
        public void TestLongSequenceOfOperations()
        {
            AlternativePriorityQueue<int, int> q = new AlternativePriorityQueue<int, int>();
            addRandomItems(q, 15);
            CheckOrder(q, nodesToKeep: 3);
            addRandomItems(q, 33);
            CheckOrder(q);
            q.Clear();
            addRandomItems(q, 13);
            q.ChangePriority(q.Head, 6);
            q.ChangePriority(q.Head, 7);
            q.ChangePriority(q.Head, 8);
            CheckOrder(q, nodesToKeep: 0);
        }

        [Test]
        public void TestCopiedQueue()
        {
            AlternativePriorityQueue<int, int> q1 = new AlternativePriorityQueue<int, int>();
            addRandomItems(q1, 178);
            AlternativePriorityQueue<int, int> q2 = new AlternativePriorityQueue<int, int>(q1);
            addRandomItems(q2, 28);
            CheckOrder(q1, nodesToKeep: 42);
            addRandomItems(q1, 39);
            CheckOrder(q2, nodesToKeep: 2);
            addRandomItems(q2, 18);

            CheckOrder(q1);
            CheckOrder(q2);
        }

        [Test]
        public void TestDifferentComparer()
        {
            IComparer<int> comparer = new BackwardsInt32Comparer();
            AlternativePriorityQueue<int, int> q = new AlternativePriorityQueue<int, int>(comparer);
            addRandomItems(q, 15);
            CheckOrder(q, reversed: true);
        }

        [Test]
        public void TestContainsConsistency()
        {
            AlternativePriorityQueue<int, int> q = new AlternativePriorityQueue<int, int>();
            addRandomItems(q, 150);

            PriorityQueueNode<int, int> missingNode = new PriorityQueueNode<int, int>(14);
            PriorityQueueNode<int, int> presentNode = new PriorityQueueNode<int, int>(14);
            q.Enqueue(presentNode, 75);

            Assert.True(q.Contains(presentNode));
            Assert.False(q.Contains(missingNode));

            q.Remove(presentNode);
            Assert.False(q.Contains(presentNode));
            CheckOrder(q);
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        [Test]
        public void TestOrder1()
        {
            AlternativePriorityQueue<int, int> q = new AlternativePriorityQueue<int, int>();

            q.Enqueue(new PriorityQueueNode<int, int>(1), 1);
            q.Enqueue(new PriorityQueueNode<int, int>(10), 10);
            q.Enqueue(new PriorityQueueNode<int, int>(5), 5);
            q.Enqueue(new PriorityQueueNode<int, int>(8), 8);
            q.Enqueue(new PriorityQueueNode<int, int>(-1), -1);
            CheckOrder(q);
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        [Test]
        public void TestOrderRandom1()
        {
            AlternativePriorityQueue<int, int> q = new AlternativePriorityQueue<int, int>();
            addRandomItems(q, 100);
            CheckOrder(q);
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        private void addRandomItems<TData>(AlternativePriorityQueue<int, TData> q, int num)
        {
            var random = new Random();

            for (int i = 0; i < num; i++)
            {
                // This usually inserts lots of duplicate values in an order
                // that *tends* to be increasing, but usually has some values
                // that should bubble up near the top of the heap.
                int priority = (int) (num * random.NextDouble());

                q.Enqueue(new PriorityQueueNode<int, TData>(default(TData)), priority);
            }
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        private void CheckOrder<TData>(AlternativePriorityQueue<int, TData> q, int nodesToKeep = 0, bool reversed = false)
        {
            int curr = 0;
            bool first = true;

            while (q.Count > nodesToKeep)
            {
                int next = q.Dequeue().Priority;
                Console.WriteLine(next);
                if (!first)
                {
                    int comparison = next.CompareTo(curr);
                    if (reversed)
                    {
                        Assert.LessOrEqual(comparison, 0);
                    }
                    else
                    {
                        Assert.GreaterOrEqual(comparison, 0);
                    }
                }

                first = false;
                curr = next;
            }
        }

        private class BackwardsInt32Comparer : Comparer<int>
        {
            public override int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
