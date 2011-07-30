using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    //Tests are exposed by SpatialIndexTestCase type
    public class STRtreeTestCase : SpatialIndexTestCase
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

        private GeometryFactory factory = new GeometryFactory();

        protected override ISpatialIndex<object> CreateSpatialIndex()
        {
            return (ISpatialIndex)new STRtree(4);
        }

        [Test]
        public void TestCreateParentsFromVerticalSlice()
        {
            DoTestCreateParentsFromVerticalSlice(3, 2, 2, 1);
            DoTestCreateParentsFromVerticalSlice(4, 2, 2, 2);
            DoTestCreateParentsFromVerticalSlice(5, 2, 2, 1);
        }

        [Test]
        public void TestDisallowedInserts()
        {
            STRtree t = new STRtree(5);
            t.Insert(new Envelope(0, 0, 0, 0), new Object());
            t.Insert(new Envelope(0, 0, 0, 0), new Object());
            t.Query(new Envelope());
            try {
                t.Insert(new Envelope(0, 0, 0, 0), new Object());
                Assert.IsTrue(false);
            }
            catch (NetTopologySuite.Utilities.AssertionFailedException e)
            {
                Assert.IsTrue(true);
            }
        }

        [Test]
        public void TestQuery()
        {
            var geometries = new List<IGeometry>();
            geometries.Add(factory.CreateLineString(new Coordinate[]{
                new Coordinate(0, 0), new Coordinate(10, 10)}));
            geometries.Add(factory.CreateLineString(new Coordinate[]{
                new Coordinate(20, 20), new Coordinate(30, 30)}));
            geometries.Add(factory.CreateLineString(new Coordinate[]{
                new Coordinate(20, 20), new Coordinate(30, 30)}));
            STRtree t = new STRtree(4);
            foreach (var g in geometries) {
                t.Insert(g.EnvelopeInternal, new Object());
            }
            t.Build();
            try {
                Assert.AreEqual(1, t.Query(new Envelope(5, 6, 5, 6)).Count);
                Assert.AreEqual(0, t.Query(new Envelope(20, 30, 0, 10)).Count);
                Assert.AreEqual(2, t.Query(new Envelope(25, 26, 25, 26)).Count);
                Assert.AreEqual(3, t.Query(new Envelope(0, 100, 0, 100)).Count);
            }
            catch (Exception x) {
                Console.WriteLine(x.Message);
                throw x;
            }
        }

        [Test]
        public void TestVerticalSlices() 
        {
            DoTestVerticalSlices(3, 2, 2, 1);
            DoTestVerticalSlices(4, 2, 2, 2);
            DoTestVerticalSlices(5, 3, 2, 1);
        }

        private void DoTestCreateParentsFromVerticalSlice(int childCount,
            int nodeCapacity, int expectedChildrenPerParentBoundable,
            int expectedChildrenOfLastParent)
        {
            STRtreeDemo.TestTree t = new STRtreeDemo.TestTree(nodeCapacity);
            IList<object> parentBoundables
                    = t.CreateParentBoundablesFromVerticalSlice(ItemWrappers(childCount), 0);
            for (int i = 0; i < parentBoundables.Count - 1; i++) {//-1
                AbstractNode parentBoundable = (AbstractNode) parentBoundables[i];
                Assert.AreEqual(expectedChildrenPerParentBoundable, parentBoundable.ChildBoundables.Count);
            }
            AbstractNode lastParent = (AbstractNode) parentBoundables[parentBoundables.Count - 1];
            Assert.AreEqual(expectedChildrenOfLastParent, lastParent.ChildBoundables.Count);
        }

        private void DoTestVerticalSlices(int itemCount, int sliceCount,
            int expectedBoundablesPerSlice, int expectedBoundablesOnLastSlice)
        {
            STRtreeDemo.TestTree t = new STRtreeDemo.TestTree(2);
            IList<object>[] slices = t.VerticalSlices(ItemWrappers(itemCount), sliceCount);
            Assert.AreEqual(sliceCount, slices.Length);
            for (int i = 0; i < sliceCount - 1; i++) {//-1
                Assert.AreEqual(expectedBoundablesPerSlice, slices[i].Count);
            }
            Assert.AreEqual(expectedBoundablesOnLastSlice, slices[sliceCount - 1].Count);
        }

        private IList<object> ItemWrappers(int size) {
            List<object> itemWrappers = new List<object>();
            for (int i = 0; i < size; i++) {
                itemWrappers.Add(new ItemBoundable(new Envelope(0, 0, 0, 0), new Object()));
            }
            return itemWrappers;
        }
    }
}
