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
                nodes[i] = new PriorityQueueNode<double, object>(null);
                q.Enqueue(nodes[i], random.NextDouble());
            }

            CollectionAssert.AreEquivalent(nodes, q);
        }

        [Test]
        public void TestLongSequenceOfOperations()
        {
            AlternativePriorityQueue<double, int> q = new AlternativePriorityQueue<double, int>();
            addRandomItems(q, 15);
            CheckOrder(q, nodesToKeep: 3);
            addRandomItems(q, 33);
            CheckOrder(q);
            q.Clear();
            addRandomItems(q, 13);
            q.ChangePriority(q.Head, 1.1);
            q.ChangePriority(q.Head, 1.2);
            q.ChangePriority(q.Head, 1.3);
            CheckOrder(q, nodesToKeep: 0);
        }

        [Test]
        public void TestCopiedQueue()
        {
            AlternativePriorityQueue<double, int> q1 = new AlternativePriorityQueue<double, int>();
            addRandomItems(q1, 178);
            AlternativePriorityQueue<double, int> q2 = new AlternativePriorityQueue<double, int>(q1);
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
            IComparer<double> comparer = new BackwardsDoubleComparer();
            AlternativePriorityQueue<double, int> q = new AlternativePriorityQueue<double, int>(comparer);
            addRandomItems(q, 15);
            CheckOrder(q, reversed: true);
        }

        [Test]
        public void TestContainsConsistency()
        {
            AlternativePriorityQueue<double, int> q = new AlternativePriorityQueue<double, int>();
            addRandomItems(q, 150);

            PriorityQueueNode<double, int> missingNode = new PriorityQueueNode<double, int>(14);
            PriorityQueueNode<double, int> presentNode = new PriorityQueueNode<double, int>(14);
            q.Enqueue(presentNode, 0.5);

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
            AlternativePriorityQueue<double, double> q = new AlternativePriorityQueue<double, double>();

            q.Enqueue(new PriorityQueueNode<double, double>(1), 1);
            q.Enqueue(new PriorityQueueNode<double, double>(10), 10);
            q.Enqueue(new PriorityQueueNode<double, double>(5), 5);
            q.Enqueue(new PriorityQueueNode<double, double>(8), 8);
            q.Enqueue(new PriorityQueueNode<double, double>(-1), -1);
            CheckOrder(q);
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        [Test]
        public void TestOrderRandom1()
        {
            AlternativePriorityQueue<double, int> q = new AlternativePriorityQueue<double, int>();
            addRandomItems(q, 100);
            CheckOrder(q);
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        private void addRandomItems<TData>(AlternativePriorityQueue<double, TData> q, int num)
        {
            var random = new Random();

            for (int i = 0; i < num; i++)
            {
                double priority = random.NextDouble();
                q.Enqueue(new PriorityQueueNode<double, TData>(default(TData)), priority);
            }
        }

        // Copied from PriorityQueueTest, to ensure consistency.
        private void CheckOrder<TData>(AlternativePriorityQueue<double, TData> q, int nodesToKeep = 0, bool reversed = false)
        {
            double curr = 0;
            bool first = true;

            while (q.Count > nodesToKeep)
            {
                double next = q.Dequeue().Priority;
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

        private class BackwardsDoubleComparer : Comparer<double>
        {
            public override int Compare(double x, double y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
