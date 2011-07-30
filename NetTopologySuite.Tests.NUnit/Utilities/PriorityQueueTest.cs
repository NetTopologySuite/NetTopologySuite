using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    [TestFixture(Ignore = true, IgnoreReason = "The PriorityQueue class has not been migrated to NTS yet.  Once this class exists in NTS the commented blocks below can be uncommented")]
    public class PriorityQueueTest
    {
        //[Test]
        //public void TestOrder1()
        //{
        //    PriorityQueue q = new PriorityQueue();
        //    q.Add(new Integer(1));
        //    q.Add(new Integer(10));
        //    q.Add(new Integer(5));
        //    q.Add(new Integer(8));
        //    q.Add(new Integer(-1));
        //    CheckOrder(q);
        //}

        //[Test]
        //public void TestOrderRandom1()
        //{
        //    PriorityQueue q = new PriorityQueue();
        //    addRandomItems(q, 100);
        //    CheckOrder(q);
        //}

        //private void addRandomItems(PriorityQueue q, int num)
        //{
        //    var random = new Random();

        //    for (int i = 0; i < num; i++)
        //    {
        //        q.Add((int)(num * random.NextDouble()));
        //    }
        //}

        //private void CheckOrder(PriorityQueue q)
        //{
        //    Comparable curr = null;

        //    while (! q.isEmpty()) {
        //        Comparable next = (Comparable) q.Poll();
        //        Console.WriteLine(next);
        //        if (curr == null)
        //            curr = next;
        //        else
        //            Assert.IsTrue(next.compareTo(curr) >= 0);
        //    }
        //}
    }
}