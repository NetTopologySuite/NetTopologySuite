using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
//using NetTopologySuite.IO;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    [TestFixture]
    public class SIRtreeTestCase
    {
        private class TestTree : SIRtree
        {
            public TestTree(int nodeCapacity)
                : base(nodeCapacity)
            {
            }

            public AbstractNode Root()
            {
                return base.Root;
            }

            public IList<object> BoundablesAtLevel(int level)
            {
                return base.BoundablesAtLevel(level);
            }
        }

        [Test]
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

        [Test]
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
