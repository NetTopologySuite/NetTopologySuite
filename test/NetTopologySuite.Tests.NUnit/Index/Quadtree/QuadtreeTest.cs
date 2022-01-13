/*
 * Copyright (c) 2016 Martin Davis.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * and Eclipse Distribution License v. 1.0 which accompanies this distribution.
 * The Eclipse Public License is available at http://www.eclipse.org/legal/epl-v10.html
 * and the Eclipse Distribution License is available at
 *
 * http://www.eclipse.org/org/documents/edl-v10.php.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Quadtree
{
    public class QuadtreeTest
    {
        [Test]
        public void TestSpatialIndex()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new Quadtree<object>() };
            tester.Init();
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

        [Test]
        public void TestSerialization()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new Quadtree<object>() };
            tester.Init();

            TestContext.WriteLine("\n\nTest with original data\n");
            tester.Run();
            var tree1 = (Quadtree<object>)tester.SpatialIndex;
            byte[] data = SerializationUtility.Serialize(tree1);
            var tree2 = SerializationUtility.Deserialize<Quadtree<object>>(data);
            tester.SpatialIndex = tree2;

            TestContext.WriteLine("\n\nTest with deserialized data\n");
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

        [Test]
        public void TestNullQuery()
        {
            var qt = new Quadtree<string>();
            var result1 = qt.Query(null);
            Assert.That(result1.Count, Is.EqualTo(0));

            qt.Insert(new Envelope(0, 10, 0, 10), "some data");
            var result2 = qt.Query(null);
            Assert.That(result2.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestConcurrent()
        {
            var tree = new Quadtree<BoundedString>();
            var cts = new CancellationTokenSource();

            try
            {
                Task.Run(() => AlterTree(tree, cts.Token));
                Task.Run(() => AlterTree(tree, cts.Token));
                Task.Run(() => AlterTree(tree, cts.Token));
                Task.Run(() => QueryTree(tree, cts.Token));
                Task.Run(() => AlterTree(tree, cts.Token));
                Task.Run(() => AlterTree(tree, cts.Token));
                Task.Run(() => AlterTree(tree, cts.Token));
                Task.Run(() => QueryTree(tree, cts.Token));

                Thread.Sleep(2000);
                cts.Cancel();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(true);
            }    
        }

        private class BoundedString
        {
            public string Value { get; set; }
            public Envelope Bound { get; set; }
        }

        private void AlterTree(Quadtree<BoundedString> tree, CancellationToken token)
        {
            var rnd = new Random();
            while(!token.IsCancellationRequested)
            {
                while (!token.IsCancellationRequested)
                {
                    var env = RndEnvelope(rnd);
                    switch (rnd.Next(0, 10))
                    {
                        default:
                            tree.Insert(env, new BoundedString { Value = env.Centre.ToString(), Bound = env });
                            break;
                        case 9:
                            var toRemove = tree.Query(env);
                            foreach (var item in toRemove)
                                tree.Remove(item.Bound, item);
#if DEBUG
                            TestContext.WriteLine($"Removed {toRemove.Count} items.");
#endif
                            break;
                    }
                    Thread.Sleep(rnd.Next(2, 7));
                }

            }
        }

        private void QueryTree(Quadtree<BoundedString> tree, CancellationToken token) {

            var rnd = new Random();
            while (!token.IsCancellationRequested)
            {
                var env = RndEnvelope(rnd);
                var items = tree.Query(env);
#if DEBUG
                TestContext.WriteLine($"Query returned {items.Count} items.");
#endif
                Thread.Sleep(15);
            }
        }

        private static Envelope RndEnvelope(Random rnd)
        {
            var c1 = new Coordinate(-180 + rnd.NextDouble() * 360, -90 + rnd.NextDouble() * 180);
            var c2 = new Coordinate(-180 + rnd.NextDouble() * 360, -90 + rnd.NextDouble() * 180);
            return new Envelope(c1, c2);
        }

    }
}
