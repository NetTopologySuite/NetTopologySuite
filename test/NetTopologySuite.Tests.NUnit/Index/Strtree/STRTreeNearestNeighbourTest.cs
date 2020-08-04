using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Strtree
{
    public class STRtreeNearestNeighbourTest : GeometryTestCase
    {


        private const string POINTS_B = "MULTIPOINT( 5 5, 15 15, 5 15, 15 5, 8 8)";
        private const string POINTS_A = "MULTIPOINT( 0 0, 10 10, 0 10, 10 0, 9 9)";

        [Test]
        public void TestNearestNeighboursEmpty()
        {
            var tree = new STRtree<Geometry>();
            object[] nn = tree.NearestNeighbour(new GeometryItemDistance());
            Assert.That(nn, Is.Null);
        }

        [Test]
        public void TestNearestNeighbours()
        {
            CheckNN(POINTS_A,
                "MULTIPOINT(9 9, 10 10)");
        }

        [Test]

        public void TestNearestNeighbourSingleItem()
        {
            CheckNN("POINT( 5 5 )", "POINT( 5 5 )");
        }

        [Test]

        public void TestNearestNeighbours2()
        {
            CheckNN(
                POINTS_A,
                POINTS_B,
                "POINT( 9 9 )",
                "POINT( 8 8 )");
        }

        [Test]
        public void TestWithinDistance()
        {
            CheckWithinDistance(POINTS_A, POINTS_B, 2, true);
            CheckWithinDistance(POINTS_A, POINTS_B, 1, false);
        }

        private void CheckNN(string wktItems, string wktExpected)
        {
            var items = Read(wktItems);

            var tree = CreateTree(items);
            var nearest = tree.NearestNeighbour(new GeometryItemDistance());

            if (wktExpected == null)
            {
                Assert.That(nearest, Is.Null);
                return;
            }

            var expected = Read(wktExpected);
            bool isFound = IsEqualUnordered(nearest, expected.GetGeometryN(0), expected.GetGeometryN(1));
            Assert.That(isFound, Is.True);
        }

        private void CheckNN(string wktItems1, string wktItems2,
            string wktExpected1, string wktExpected2)
        {
            var items1 = Read(wktItems1);
            var items2 = Read(wktItems2);
            var expected1 = Read(wktExpected1);
            var expected2 = Read(wktExpected2);

            var tree1 = CreateTree(items1);
            var tree2 = CreateTree(items2);

            var nearest = tree1.NearestNeighbour(tree2, new GeometryItemDistance());

            bool isFound = isEqual(nearest, expected1, expected2);
            Assert.That(isFound, Is.True);
        }

        private void CheckWithinDistance(string wktItems1, string wktItems2,
            double distance, bool expected)
        {
            var items1 = Read(wktItems1);
            var items2 = Read(wktItems2);

            var tree1 = CreateTree(items1);
            var tree2 = CreateTree(items2);

            bool result = tree1.IsWithinDistance(tree2, new GeometryItemDistance(), distance);

            Assert.That(result, Is.EqualTo(expected));
        }

        private bool IsEqualUnordered(IList<Geometry> items, Geometry g1, Geometry g2)
        {
            return (isEqual(items, g1, g2) || isEqual(items, g2, g1));
        }

        private bool isEqual(IList<Geometry> items, Geometry g1, Geometry g2)
        {
            if (g1.EqualsExact((Geometry) items[0])
                && g2.EqualsExact((Geometry) items[1]))
                return true;
            return false;
        }

        private STRtree<Geometry> CreateTree(Geometry items)
        {
            var tree = new STRtree<Geometry>();
            for (int i = 0; i < items.NumGeometries; i++)
            {
                var item = items.GetGeometryN(i);
                tree.Insert(item.EnvelopeInternal, item);
            }

            return tree;
        }

        [Test]
        public void TestKNearestNeighbors()
        {
            int topK = 1000;
            int totalRecords = 10000;
            var geometryFactory = new GeometryFactory();
            var coordinate = new Coordinate(10.1, -10.1);
            var queryCenter = geometryFactory.CreatePoint(coordinate);
            int valueRange = 1000;
            var testDataset = new List<Geometry>();
            var correctData = new List<Geometry>();
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

            var strtree = new STRtree<Geometry>();
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
            var testTopK = strtree.NearestNeighbour(queryCenter.EnvelopeInternal, queryCenter,
                new GeometryItemDistance(), topK);
            var topKList = new List<Geometry>(testTopK);
            topKList.Sort(distanceComparator);
            /*
             * Check the difference between correct result and test result. The difference should be 0.
             */
            int difference = 0;
            for (int i = 0; i < topK; i++)
            {
                if (distanceComparator.Compare(correctData[i], topKList[i]) != 0)
                {
                    difference++;
                }
            }

            Assert.That(difference, Is.Zero);
        }
    }

}
