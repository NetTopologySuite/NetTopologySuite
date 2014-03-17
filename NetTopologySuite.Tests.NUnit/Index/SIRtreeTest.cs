using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
//using NetTopologySuite.IO;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;
using SIRtree = NetTopologySuite.Index.Strtree.SIRtree<object>;
using AbstractNode = NetTopologySuite.Index.Strtree.AbstractNode<NetTopologySuite.Index.Strtree.Interval, object>;
namespace NetTopologySuite.Tests.NUnit.Index
{
    [TestFixtureAttribute]
    public class SIRtreeTest
    {
        private class TestTree : SIRtree
        {
            public TestTree(int nodeCapacity)
                : base(nodeCapacity)
            {
            }

            public new AbstractNode Root()
            {
                return base.Root;
            }

            public new IList<IBoundable<NetTopologySuite.Index.Strtree.Interval, object>> BoundablesAtLevel(int level)
            {
                return base.BoundablesAtLevel(level);
            }
        }

        [TestAttribute]
        public void Test()
        {
            TestTree t = new TestTree(2);
            t.Insert(2, 6, "A");
            t.Insert(2, 4, "B");
            t.Insert(2, 3, "C");
            t.Insert(2, 4, "D");
            t.Insert(0, 1, "E");
            t.Insert(2, 4, "F");
            t.Insert(5, 6, "G");
            t.Build();
            Assert.AreEqual(2, t.Root().Level);
            Assert.AreEqual(4, t.BoundablesAtLevel(0).Count);
            Assert.AreEqual(2, t.BoundablesAtLevel(1).Count);
            Assert.AreEqual(1, t.BoundablesAtLevel(2).Count);
            Assert.AreEqual(1, t.Query(0.5, 0.5).Count);
            Assert.AreEqual(0, t.Query(1.5, 1.5).Count);
            Assert.AreEqual(2, t.Query(4.5, 5.5).Count);
        }

        [TestAttribute]
        public void TestEmptyTree()
        {
            TestTree t = new TestTree(2);
            t.Build();
            Assert.AreEqual(0, t.Root().Level);
            Assert.AreEqual(1, t.BoundablesAtLevel(0).Count);
            Assert.AreEqual(0, t.BoundablesAtLevel(1).Count);
            Assert.AreEqual(0, t.BoundablesAtLevel(-1).Count);
            Assert.AreEqual(0, t.Query(0.5, 0.5).Count);
        }
    }
}
