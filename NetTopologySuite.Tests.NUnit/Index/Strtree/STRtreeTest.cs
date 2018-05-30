using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;
using STRtree = NetTopologySuite.Index.Strtree.STRtree<object>;
using AbstractNode = NetTopologySuite.Index.Strtree.AbstractNode<GeoAPI.Geometries.Envelope, object>;
namespace NetTopologySuite.Tests.NUnit.Index.Strtree
{
    public class STRtreeTest
    {
        private class TestTree : STRtree
        {
            public TestTree(int nodeCapacity) : base(nodeCapacity) { }

            public new AbstractNode Root()
            {
                return base.Root;
            }

            public new IList<IBoundable<Envelope, object>> BoundablesAtLevel(int level)
            {
                return base.BoundablesAtLevel(level);
            }
        }

        private readonly GeometryFactory _factory = new GeometryFactory();

        [Test]
        public void TestSpatialIndex()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new STRtree(4) };
            tester.Init();
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

        [Test]
        public void TestSerialization()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new STRtree(4) };
            tester.Init();

            Console.WriteLine("\n\nTest with original data\n");
            tester.Run();
            var tree1 = (STRtree)tester.SpatialIndex;
            // create the index before serialization
            tree1.Query(new Envelope());
            var data = SerializationUtility.Serialize(tree1);
            var tree2 = SerializationUtility.Deserialize<STRtree>(data);
            tester.SpatialIndex = tree2;

            Console.WriteLine("\n\nTest with deserialized data\n");
            tester.Run();
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

        [Test]
        public void TestEmptyTreeUsingListQuery()
        {
            var tree = new STRtree();
            var list = tree.Query(new Envelope(0, 0, 1, 1));
            Assert.IsTrue(list.Count == 0);
        }

        private class ItemVisitor : IItemVisitor<object>
        {
            public void VisitItem(object item) { Assert.IsTrue(true, "Should never reach here"); }
        }

        [Test]
        public void TestEmptyTreeUsingItemVisitorQuery()
        {
            var tree = new STRtree();
            tree.Query(new Envelope(0, 0, 1, 1), new ItemVisitor());
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
            try
            {
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
            geometries.Add(_factory.CreateLineString(new Coordinate[]
                                                         {
                                                             new Coordinate(0, 0), new Coordinate(10, 10)
                                                         }));
            geometries.Add(_factory.CreateLineString(new Coordinate[]
                                                         {
                                                             new Coordinate(20, 20), new Coordinate(30, 30)
                                                         }));
            geometries.Add(_factory.CreateLineString(new Coordinate[]
                                                         {
                                                             new Coordinate(20, 20), new Coordinate(30, 30)
                                                         }));
            STRtree t = new STRtree(4);
            foreach (var g in geometries)
            {
                t.Insert(g.EnvelopeInternal, new Object());
            }
            t.Build();
            try
            {
                Assert.AreEqual(1, t.Query(new Envelope(5, 6, 5, 6)).Count);
                Assert.AreEqual(0, t.Query(new Envelope(20, 30, 0, 10)).Count);
                Assert.AreEqual(2, t.Query(new Envelope(25, 26, 25, 26)).Count);
                Assert.AreEqual(3, t.Query(new Envelope(0, 100, 0, 100)).Count);
            }
            catch (Exception x)
            {
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

        [Test]
        public void TestRemove()
        {
            var tree = new STRtree();
            tree.Insert(new Envelope(0, 10, 0, 10), "1");
            tree.Insert(new Envelope(5, 15, 5, 15), "2");
            tree.Insert(new Envelope(10, 20, 10, 20), "3");
            tree.Insert(new Envelope(15, 25, 15, 25), "4");
            tree.Remove(new Envelope(10, 20, 10, 20), "4");
            Assert.AreEqual(3, tree.Count);
        }

        [Test]
        public void TestKNearestNeighbors()
        {
            var topK = 1000;
            var totalRecords = 10000;
            var geometryFactory = new GeometryFactory();
            var coordinate = new Coordinate(10.1, -10.1);
            var queryCenter = geometryFactory.CreatePoint(coordinate);
            var valueRange = 1000;
            var testDataset = new List<IGeometry>();
            var correctData = new List<IGeometry>();
            var random = new Random();
            var distanceComparator = new GeometryDistanceComparer(queryCenter, true);
            /*
             * Generate the random test data set
             */
            for (int i = 0; i < totalRecords; i++)
            {
                coordinate = new Coordinate(-100 + random.Next(valueRange) * 1.1, random.Next(valueRange) * (-5.1));
                var spatialObject = geometryFactory.CreatePoint(coordinate);
                testDataset.Add(spatialObject);
            }
            /*
             * Sort the original data set and make sure the elements are sorted in an ascending order
             */
            testDataset.Sort(distanceComparator);
            /*
             * Get the correct top K
             */
            for (int i = 0; i < topK; i++)
            {
                correctData.Add(testDataset[i]);
            }

            var strtree = new STRtree<IGeometry>();
            for (int i = 0; i < totalRecords; i++)
            {
                strtree.Insert(testDataset[i].EnvelopeInternal, testDataset[i]);
            }
            /*
             * Shoot a random query to make sure the STR-Tree is built.
             */
            strtree.Query(new Envelope(1 + 0.1, 1 + 0.1, 2 + 0.1, 2 + 0.1));
            /*
             * Issue the KNN query.
             */
            var testTopK = strtree.NearestNeighbour(queryCenter.EnvelopeInternal, queryCenter, new GeometryItemDistance(), topK);
            var topKList = new List<IGeometry>(testTopK);
            topKList.Sort(distanceComparator);
            /*
             * Check the difference between correct result and test result. The difference should be 0.
             */
            var difference = 0;
            for (int i = 0; i < topK; i++)
            {
                if (distanceComparator.Compare(correctData[i], topKList[i]) != 0)
                {
                    difference++;
                }
            }
            Assert.That(difference, Is.EqualTo(0));
        }

        private static void DoTestCreateParentsFromVerticalSlice(int childCount,
                                                          int nodeCapacity, int expectedChildrenPerParentBoundable,
                                                          int expectedChildrenOfLastParent)
        {
            var t = new STRtreeDemo.TestTree(nodeCapacity);
            var parentBoundables
                = t.CreateParentBoundablesFromVerticalSlice(ItemWrappers(childCount), 0);
            for (int i = 0; i < parentBoundables.Count - 1; i++)
            {
                //-1
                var parentBoundable = (AbstractNode)parentBoundables[i];
                Assert.AreEqual(expectedChildrenPerParentBoundable, parentBoundable.ChildBoundables.Count);
            }
            var lastParent = (AbstractNode)parentBoundables[parentBoundables.Count - 1];
            Assert.AreEqual(expectedChildrenOfLastParent, lastParent.ChildBoundables.Count);
        }

        private static void DoTestVerticalSlices(int itemCount, int sliceCount,
                                          int expectedBoundablesPerSlice, int expectedBoundablesOnLastSlice)
        {
            var t = new STRtreeDemo.TestTree(2);
            var slices = t.VerticalSlices(ItemWrappers(itemCount), sliceCount);
            Assert.AreEqual(sliceCount, slices.Length);
            for (int i = 0; i < sliceCount - 1; i++)
            {
                //-1
                Assert.AreEqual(expectedBoundablesPerSlice, slices[i].Count);
            }
            Assert.AreEqual(expectedBoundablesOnLastSlice, slices[sliceCount - 1].Count);
        }

        private static IList<IBoundable<Envelope, object>> ItemWrappers(int size)
        {
            var itemWrappers = new List<IBoundable<Envelope, object>>();
            for (var i = 0; i < size; i++)
            {
                itemWrappers.Add(new ItemBoundable<Envelope, object>(new Envelope(0, 0, 0, 0), new Object()));
            }
            return itemWrappers;
        }
    }
}
