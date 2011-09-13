//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************


#region Using directives

using System;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;

#endregion

namespace Wintellect.PowerCollections.Tests
{
    [TestFixture]
    public class BagTests
    {
        [Test]
        public void RandomAddDelete()
        {
            const int SIZE = 5000; 
            int[] count = new int[SIZE];
            Random rand = new Random(3);
            Bag<int> bag1 = new Bag<int>();
            bool b;

            // Add and delete values at random.
            for (int i = 0; i < SIZE * 100; ++i) {
                int v = rand.Next(SIZE);

                Assert.AreEqual(count[v], bag1.NumberOfCopies(v));
                if (count[v] > 0)
                    Assert.IsTrue(bag1.Contains(v));

                if (count[v] == 0 || rand.Next(2) == 1) {
                    // Add to the bag.
                    bag1.Add(v);
                    count[v] += 1;
                }
                else {
                    // Remove from the bag.
                    b = bag1.Remove(v);
                    Assert.IsTrue(b);
                    count[v] -= 1;
                }
            }

            // Make sure the bag has all the correct values, not necessarily in order.
            int c = 0;
            foreach (int x in count)
                c += x;
            Assert.AreEqual(c, bag1.Count);

            foreach (int v in bag1) {
                --count[v];
            }

            foreach (int x in count)
                Assert.AreEqual(0, x);
        }

        [Test]
        public void ICollectionInterface()
        {
            string[] s_array = { "Foo", "hello", "Eric", null, "Clapton", "hello", "goodbye", "C#", null };
            Bag<string> bag1 = new Bag<string>();

            foreach (string s in s_array)
                bag1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestCollection<string>((ICollection)bag1, s_array, false);
        }


        [Test]
        public void GenericICollectionInterface()
        {
            string[] s_array = { "Foo", "hello", "Eric", null, "Clapton", "hello", "goodbye", "C#", null };
            Bag<string> bag1 = new Bag<string>();

            foreach (string s in s_array)
                bag1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)bag1, s_array, false);
        }

        [Test]
        public void Add()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add("Hello");
            bag1.Add("foo");
            bag1.Add("");
            bag1.Add("HELLO");
            bag1.Add("foo");
            bag1.Add(null);
            bag1.Add("hello");
            bag1.Add("Eric");
            bag1.Add(null);

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { null, null, "", "Eric", "foo", "foo", "Hello", "Hello", "Hello" }, false);
        }

        [Test]
        public void AddRepresentative()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.AddRepresentative("Hello");
            bag1.AddRepresentative("foo");
            bag1.AddRepresentative("");
            bag1.AddRepresentative("HELLO");
            bag1.AddRepresentative("Foo");
            bag1.AddRepresentative("foo");
            bag1.AddRepresentative(null);
            bag1.AddRepresentative("hello");
            bag1.AddRepresentative("Eric");
            bag1.AddRepresentative(null);

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { null, null, "", "Eric", "foo", "foo", "foo", "hello", "hello", "hello" }, false);
        }

        [Test]
        public void CountAndClear()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);

            Assert.AreEqual(0, bag1.Count);
            bag1.Add("hello"); Assert.AreEqual(1, bag1.Count);
            bag1.Add("foo"); Assert.AreEqual(2, bag1.Count);
            bag1.Add(""); Assert.AreEqual(3, bag1.Count);
            bag1.Add("HELLO"); Assert.AreEqual(4, bag1.Count);
            bag1.Add("foo"); Assert.AreEqual(5, bag1.Count);
            bag1.Remove(""); Assert.AreEqual(4, bag1.Count);
            bag1.Add(null); Assert.AreEqual(5, bag1.Count);
            bag1.Add("Hello"); Assert.AreEqual(6, bag1.Count);
            bag1.Add("Eric"); Assert.AreEqual(7, bag1.Count);
            bag1.RemoveAllCopies("hElLo"); Assert.AreEqual(4, bag1.Count);
            bag1.Add(null); Assert.AreEqual(5, bag1.Count);
            bag1.Clear();
            Assert.AreEqual(0, bag1.Count);

            bool found = false;
            foreach (string s in bag1)
                found = true;

            Assert.IsFalse(found);
        }

        [Test]
        public void Remove()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = bag1.Remove("Eric"); Assert.IsFalse(b);
            bag1.Add("hello");
            bag1.Add("foo");
            bag1.Add(null);
            bag1.Add(null);
            bag1.Add("HELLO");
            bag1.Add("Hello");
            b = bag1.Remove("hello"); Assert.IsTrue(b);
            InterfaceTests.TestCollection(bag1, new string[] { null, null, "foo", "hello", "hello" }, false);
            b = bag1.Remove("Hello"); Assert.IsTrue(b);
            b = bag1.Remove(null); Assert.IsTrue(b);
            b = bag1.Remove(null); Assert.IsTrue(b);
            b = bag1.Remove(null); Assert.IsFalse(b);
            bag1.Add("Hello");
            bag1.Add("Eric");
            bag1.Add(null);
            b = bag1.Remove(null); Assert.IsTrue(b);
            bag1.Add("ERIC");
            b = bag1.Remove("eRic"); Assert.IsTrue(b);
            b = bag1.Remove("eRic"); Assert.IsTrue(b);
            bag1.Clear();
            b = bag1.Remove(""); Assert.IsFalse(b);
        }

        [Test]
        public void RemoveAllCopies()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);
            int i;

            i = bag1.RemoveAllCopies("Eric"); Assert.AreEqual(0, i);
            bag1.Add("hello");
            bag1.Add("foo");
            bag1.Add(null);
            bag1.Add(null);
            bag1.Add("hello");
            bag1.Add(null);
            i = bag1.RemoveAllCopies("HELLO"); Assert.AreEqual(2, i);
            i = bag1.RemoveAllCopies("Hello"); Assert.AreEqual(0, i);
            i = bag1.RemoveAllCopies(null); Assert.AreEqual(3, i);
            bag1.Add("Hello");
            bag1.Add("Eric");
            bag1.Add(null);
            i = bag1.RemoveAllCopies(null); Assert.AreEqual(1, i);
            bag1.Add("ERIC");
            i = bag1.RemoveAllCopies("eRic"); Assert.AreEqual(2, i);
        }

        [Test]
        public void NumberOfCopies()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add(null);
            bag1.Add("hello");
            bag1.Add("foo");
            Assert.AreEqual(1, bag1.NumberOfCopies("hello"));
            Assert.AreEqual(1, bag1.NumberOfCopies("FOO"));
            Assert.AreEqual(1, bag1.NumberOfCopies(null));
            bag1.Add(null);
            bag1.Add("HELLO");
            bag1.Add("Hello");
            Assert.AreEqual(3, bag1.NumberOfCopies("hello"));
            Assert.AreEqual(2, bag1.NumberOfCopies(null));
            bag1.Remove(null);
            bag1.Remove(null);
            Assert.AreEqual(0, bag1.NumberOfCopies(null));
        }

        [Test]
        public void GetRepresentative()
        {
            Bag<string> bag1 = new Bag<string>(
                new string[] { "foo", null, "FOO", "Eric", "eric", "bar", null, "foO", "ERIC", "eric", null },
                StringComparer.InvariantCultureIgnoreCase);

            int count;
            string rep;

            count = bag1.GetRepresentativeItem("Foo", out rep);
            Assert.AreEqual(3, count);
            Assert.AreEqual("foo", rep);

            count = bag1.GetRepresentativeItem(null, out rep);
            Assert.AreEqual(3, count);
            Assert.AreEqual(null, rep);

            count = bag1.GetRepresentativeItem("silly", out rep);
            Assert.AreEqual(0, count);
            Assert.AreEqual("silly", rep);

            count = bag1.GetRepresentativeItem("ERic", out rep);
            Assert.AreEqual(4, count);
            Assert.AreEqual("Eric", rep);

            count = bag1.GetRepresentativeItem("BAR", out rep);
            Assert.AreEqual(1, count);
            Assert.AreEqual("bar", rep);
        }

        [Test]
        public void ChangeNumberOfCopies()
        {
            Bag<string> bag1 = new Bag<string>(
                new string[] { "foo", null, "FOO", "Eric", "eric", "bar", null, "foO", "ERIC", "eric", null },
                StringComparer.InvariantCultureIgnoreCase);

            bag1.ChangeNumberOfCopies("Foo", 7);
            bag1.ChangeNumberOfCopies(null, 0);
            bag1.ChangeNumberOfCopies("eRIC", 0);
            bag1.ChangeNumberOfCopies("silly", 2);
            bag1.ChangeNumberOfCopies("BAR", 1);

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { "foo", "foo", "foo", "foo", "foo", "foo", "foo", "bar", "silly", "silly" }, false);
        }

        [Test]
        public void ToArray()
        {
            string[] s_array = { null, "Foo", "Eric", null, "Clapton", "hello", "Clapton", "goodbye", "C#" };
            Bag<string> bag1 = new Bag<string>();

            string[] a1 = bag1.ToArray();
            Assert.IsNotNull(a1);
            Assert.AreEqual(0, a1.Length);

            foreach (string s in s_array)
                bag1.Add(s);
            string[] a2 = bag1.ToArray();

            Array.Sort(s_array);
            Array.Sort(a2);

            Assert.AreEqual(s_array.Length, a2.Length);
            for (int i = 0; i < s_array.Length; ++i)
                Assert.AreEqual(s_array[i], a2[i]);
        }

        [Test]
        public void AddMany()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);
            bag1.Add("foo");
            bag1.Add("Eric");
            bag1.Add("Clapton");
            string[] s_array = { "FOO", "x", "elmer", "fudd", "Clapton", null };
            bag1.AddMany(s_array);

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { null, "Clapton", "Clapton", "elmer", "Eric", "foo", "foo", "fudd", "x" }, false);

            bag1.Clear();
            bag1.Add("foo");
            bag1.Add("Eric");
            bag1.AddMany(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { "Eric", "Eric", "foo", "foo" }, false);
        }

        [Test]
        public void RemoveMany()
        {
            Bag<string> bag1 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add("foo");
            bag1.Add("Eric");
            bag1.Add("Clapton");
            bag1.Add(null);
            bag1.Add("Foo");
            bag1.Add("fudd");
            bag1.Add("elmer");
            string[] s_array = { "FOO", "jasmine", "eric", null };
            int count = bag1.RemoveMany(s_array);
            Assert.AreEqual(3, count);

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { "Clapton", "elmer", "foo", "fudd" }, false);

            bag1.Clear();
            bag1.Add("foo");
            bag1.Add("Eric");
            bag1.Add("Clapton");
            bag1.Add(null);
            bag1.Add("Foo");
            count = bag1.RemoveMany(bag1);
            Assert.AreEqual(5, count);
            Assert.AreEqual(0, bag1.Count);
        }

        [Test]
        public void Exists()
        {
            Bag<double> bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

            Assert.IsTrue(bag1.Exists(delegate(double d) { return d > 100; }));
            Assert.IsTrue(bag1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(bag1.Exists(delegate(double d) { return d < -10.0; }));
            bag1.Clear();
            Assert.IsFalse(bag1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void TrueForAll()
        {
            Bag<double> bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

            Assert.IsFalse(bag1.TrueForAll(delegate(double d) { return d > 100; }));
            Assert.IsFalse(bag1.TrueForAll(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.IsTrue(bag1.TrueForAll(delegate(double d) { return d > -10; }));
            Assert.IsTrue(bag1.TrueForAll(delegate(double d) { return Math.Abs(d) < 200; }));
            bag1.Clear();
            Assert.IsTrue(bag1.TrueForAll(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void CountWhere()
        {
            Bag<double> bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

            Assert.AreEqual(0, bag1.CountWhere(delegate(double d) { return d > 200; }));
            Assert.AreEqual(7, bag1.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.AreEqual(10, bag1.CountWhere(delegate(double d) { return d > -10; }));
            Assert.AreEqual(5, bag1.CountWhere(delegate(double d) { return Math.Abs(d) > 5; }));
            bag1.Clear();
            Assert.AreEqual(0, bag1.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
        }

        [Test]
        public void RemoveAll()
        {
            Bag<double> bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

            bag1.RemoveAll(delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new double[] { -0.04, 1.2, 1.2, 1.78, 4.5 }, false);

            bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });
            bag1.RemoveAll(delegate(double d) { return d == 0; });
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new double[] { -7.6, -0.04, 1.2, 1.2, 1.78, 4.5, 7.6, 10.11, 187.4, 187.4 }, false);

            bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });
            bag1.RemoveAll(delegate(double d) { return d < 200; });
            Assert.AreEqual(0, bag1.Count);
        }

        [Test]
        public void FindAll()
        {
            Bag<double> bag1 = new Bag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });
            double[] expected = { -7.6, 7.6, 10.11, 187.4, 187.4 };

            InterfaceTests.TestEnumerableElementsAnyOrder(bag1.FindAll(delegate(double d) { return Math.Abs(d) > 5; }), expected);
        }

        [Test]
        public void IsDisjointFrom()
        {
            Bag<int> bag1 = new Bag<int>(new int[] { 3, 6, 7, 1, 1, 11, 9, 3, 8 });
            Bag<int> bag2 = new Bag<int>();
            Bag<int> bag3 = new Bag<int>();
            Bag<int> bag4 = new Bag<int>(new int[] { 8, 9, 1, 8, 3, 7, 6, 11, 7 });
            Bag<int> bag5 = new Bag<int>(new int[] { 17, 3, 12, 10, 22 });
            Bag<int> bag6 = new Bag<int>(new int[] { 14, 19, 14, 0, 2, 14 });

            Assert.IsFalse(bag1.IsDisjointFrom(bag1));
            Assert.IsTrue(bag2.IsDisjointFrom(bag2));

            Assert.IsTrue(bag1.IsDisjointFrom(bag2));
            Assert.IsTrue(bag2.IsDisjointFrom(bag1));

            Assert.IsTrue(bag2.IsDisjointFrom(bag3));
            Assert.IsTrue(bag3.IsDisjointFrom(bag2));

            Assert.IsFalse(bag1.IsDisjointFrom(bag4));
            Assert.IsFalse(bag4.IsDisjointFrom(bag1));

            Assert.IsFalse(bag1.IsDisjointFrom(bag5));
            Assert.IsFalse(bag5.IsDisjointFrom(bag1));

            Assert.IsTrue(bag1.IsDisjointFrom(bag6));
            Assert.IsTrue(bag6.IsDisjointFrom(bag1));

            Assert.IsTrue(bag5.IsDisjointFrom(bag6));
            Assert.IsTrue(bag6.IsDisjointFrom(bag5));
        }

        [Test]
        public void Intersection()
        {
            Bag<int> bagOdds = new Bag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            Bag<int> bagDigits = new Bag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            Bag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.IntersectionWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.IntersectionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Intersection(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Intersection(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, false);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.IntersectionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, false);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Intersection(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, false);
        }

        [Test]
        public void Union()
        {
            Bag<int> bagOdds = new Bag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            Bag<int> bagDigits = new Bag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            Bag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.UnionWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.UnionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Union(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Union(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.UnionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, false);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Union(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, false);
        }

        [Test]
        public void Sum()
        {
            Bag<int> bagOdds = new Bag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            Bag<int> bagDigits = new Bag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            Bag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.SumWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.SumWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Sum(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Sum(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, false);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.SumWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 9, 9 }, false);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Sum(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 9, 9 }, false);
        }

        [Test]
        public void SymmetricDifference()
        {
            Bag<int> bagOdds = new Bag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            Bag<int> bagDigits = new Bag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            Bag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.SymmetricDifferenceWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.SymmetricDifferenceWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.SymmetricDifference(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.SymmetricDifference(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, false);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.SymmetricDifferenceWith(bag1);
            Assert.AreEqual(0, bag1.Count);

            bag1 = bagDigits.Clone();
            bag3 = bag1.SymmetricDifference(bag1);
            Assert.AreEqual(0, bag3.Count);
        }

        [Test]
        public void Difference()
        {
            Bag<int> bagOdds = new Bag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            Bag<int> bagDigits = new Bag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            Bag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.DifferenceWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.DifferenceWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 2, 2, 4, 5, 6, 7, 7, 7, 7, 8 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Difference(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 11, 11, 13, 15, 17, 17, 19 }, false);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Difference(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 2, 2, 4, 5, 6, 7, 7, 7, 7, 8 }, false);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.DifferenceWith(bag1);
            Assert.AreEqual(0, bag1.Count);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Difference(bag1);
            Assert.AreEqual(0, bag3.Count);
        }

        [Test]
        public void Subset()
        {
            Bag<int> set1 = new Bag<int>(new int[] { 1, 1, 3, 6, 6, 6, 6, 7, 8, 9, 9 });
            Bag<int> set2 = new Bag<int>();
            Bag<int> set3 = new Bag<int>(new int[] { 1, 6, 6, 9, 9 });
            Bag<int> set4 = new Bag<int>(new int[] { 1, 6, 6, 9, 9 });
            Bag<int> set5 = new Bag<int>(new int[] { 1, 1, 3, 6, 6, 6, 7, 7, 8, 9, 9 });

            Assert.IsTrue(set1.IsSupersetOf(set2));
            Assert.IsTrue(set2.IsSubsetOf(set1));
            Assert.IsTrue(set1.IsProperSupersetOf(set2));
            Assert.IsTrue(set2.IsProperSubsetOf(set1));

            Assert.IsTrue(set1.IsSupersetOf(set3));
            Assert.IsTrue(set3.IsSubsetOf(set1));
            Assert.IsTrue(set1.IsProperSupersetOf(set3));
            Assert.IsTrue(set3.IsProperSubsetOf(set1));

            Assert.IsFalse(set3.IsSupersetOf(set1));
            Assert.IsFalse(set1.IsSubsetOf(set3));
            Assert.IsFalse(set3.IsProperSupersetOf(set1));
            Assert.IsFalse(set1.IsProperSubsetOf(set3));

            Assert.IsFalse(set1.IsSupersetOf(set5));
            Assert.IsFalse(set5.IsSupersetOf(set1));
            Assert.IsFalse(set1.IsSubsetOf(set5));
            Assert.IsFalse(set5.IsSubsetOf(set1));
            Assert.IsFalse(set1.IsProperSupersetOf(set5));
            Assert.IsFalse(set5.IsProperSupersetOf(set1));
            Assert.IsFalse(set1.IsProperSubsetOf(set5));
            Assert.IsFalse(set5.IsProperSubsetOf(set1));

            Assert.IsTrue(set3.IsSupersetOf(set4));
            Assert.IsTrue(set3.IsSubsetOf(set4));
            Assert.IsFalse(set3.IsProperSupersetOf(set4));
            Assert.IsFalse(set3.IsProperSubsetOf(set4));

            Assert.IsTrue(set1.IsSupersetOf(set1));
            Assert.IsTrue(set1.IsSubsetOf(set1));
            Assert.IsFalse(set1.IsProperSupersetOf(set1));
            Assert.IsFalse(set1.IsProperSubsetOf(set1));
        }

        [Test]
        public void IsEqualTo()
        {
            Bag<int> set1 = new Bag<int>(new int[] { 1, 1, 3, 6, 6, 6, 6, 7, 8, 9, 9 });
            Bag<int> set2 = new Bag<int>();
            Bag<int> set3 = new Bag<int>(new int[] { 1, 6, 6, 9, 9 });
            Bag<int> set4 = new Bag<int>(new int[] { 1, 6, 6, 9, 9 });
            Bag<int> set5 = new Bag<int>(new int[] { 1, 1, 3, 6, 6, 6, 7, 7, 8, 9, 9 });
            Bag<int> set6 = new Bag<int>();

            Assert.IsFalse(set1.IsEqualTo(set5));
            Assert.IsFalse(set5.IsEqualTo(set1));
            Assert.IsTrue(set3.IsEqualTo(set4));
            Assert.IsTrue(set4.IsEqualTo(set3));
            Assert.IsTrue(set1.IsEqualTo(set1));
            Assert.IsTrue(set2.IsEqualTo(set6));
            Assert.IsFalse(set1.IsEqualTo(set2));
            Assert.IsFalse(set2.IsEqualTo(set1));
        }

        [Test]
        public void Clone()
        {
            Bag<int> bag1 = new Bag<int>(new int[] { 1, 7, 9, 11, 7, 13, 15, -17, 19, -21, 1 });
            Bag<int> bag2, bag3;

            bag2 = bag1.Clone();
            bag3 = (Bag<int>)((ICloneable)bag1).Clone();

            Assert.IsFalse(bag2 == bag1);
            Assert.IsFalse(bag3 == bag1);

            // Modify bag1, make sure bag2, bag3 don't change.
            bag1.Remove(9);
            bag1.Remove(-17);
            bag1.Add(8);

            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { -21, -17, 1, 1, 7, 7, 9, 11, 13, 15, 19 }, false);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { -21, -17, 1, 1, 7, 7, 9, 11, 13, 15, 19 }, false);

            bag1 = new Bag<int>();
            bag2 = bag1.Clone();
            Assert.IsFalse(bag2 == bag1);
            Assert.IsTrue(bag1.Count == 0 && bag2.Count == 0);

        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons1()
        {
            Bag<int> bagOdds = new Bag<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            Bag<int> bagDigits = new Bag<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new GOddEvenEqualityComparer());
            bagOdds.SymmetricDifferenceWith(bagDigits);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons2()
        {
            Bag<string> bag1 = new Bag<string>(new string[] { "foo", "Bar" }, StringComparer.CurrentCulture);
            Bag<string> bag2 = new Bag<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            bag1.Intersection(bag2);
        }

        [Test]
        public void ConsistentComparisons()
        {
            Bag<string> bag1 = new Bag<string>(new string[] { "foo", "Bar" }, StringComparer.InvariantCulture);
            Bag<string> bag2 = new Bag<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            bag1.Difference(bag2);
        }


        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FailFastEnumerator1()
        {
            Bag<double> bag1 = new Bag<double>();

            double d = 1.218034;
            for (int i = 0; i < 50; ++i) {
                bag1.Add(d);
                d = d * 1.3451 - .31;
            }

            // should throw once the bag is modified.
            foreach (double k in bag1) {
                if (k > 3.0)
                    bag1.Add(1.0);
            }
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FailFastEnumerator2()
        {
            Bag<double> bag1 = new Bag<double>();

            double d = 1.218034;
            for (int i = 0; i < 50; ++i) {
                bag1.Add(d);
                d = d * 1.3451 - .31;
            }

            // should throw once the bag is modified.
            foreach (double k in bag1) {
                if (k > 3.0)
                    bag1.Clear();
            }
        }


        // Simple class for testing cloning.
        class MyInt : ICloneable
        {
            public int value;
            public MyInt(int value)
            {
                this.value = value;
            }

            public object Clone()
            {
                return new MyInt(value);
            }

            public override bool Equals(object obj)
            {
                return (obj != null && obj is MyInt && ((MyInt)obj).value == value);
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        void CompareClones<T>(Bag<T> s1, Bag<T> s2)
        {
            Assert.AreEqual(s1.Count, s2.Count);

            // Check that the sets are equal, but not reference equals (e.g., have been cloned).
            foreach (T item in s1) {
                int found = 0;
                foreach (T other in s2) {
                    if (object.Equals(item, other)) {
                        found += 1;
                        if (item != null)
                            Assert.IsFalse(object.ReferenceEquals(item, other));
                    }
                }
                Assert.AreEqual(s1.NumberOfCopies(item), found);
            }
        }

        [Test]
        public void CloneContents()
        {
            Bag<MyInt> bag1 = new Bag<MyInt>();

            MyInt mi = new MyInt(9);
            bag1.Add(new MyInt(14));
            bag1.Add(new MyInt(143));
            bag1.Add(new MyInt(2));
            bag1.Add(mi);
            bag1.Add(null);
            bag1.Add(new MyInt(14));
            bag1.Add(new MyInt(111));
            bag1.Add(mi);
            Bag<MyInt> bag2 = bag1.CloneContents();
            CompareClones(bag1, bag2);

            Bag<int> bag3 = new Bag<int>(new int[] { 144, 1, 5, 23, 1, 8 });
            Bag<int> bag4 = bag3.CloneContents();
            CompareClones(bag3, bag4);

            Bag<UtilTests.CloneableStruct> bag5 = new Bag<UtilTests.CloneableStruct>();
            bag5.Add(new UtilTests.CloneableStruct(143));
            bag5.Add(new UtilTests.CloneableStruct(1));
            bag5.Add(new UtilTests.CloneableStruct(23));
            bag5.Add(new UtilTests.CloneableStruct(1));
            bag5.Add(new UtilTests.CloneableStruct(8));
            Bag<UtilTests.CloneableStruct> bag6 = bag5.CloneContents();

            Assert.AreEqual(bag5.Count, bag6.Count);

            // Check that the bags are equal, but not identical (e.g., have been cloned via ICloneable).
            foreach (UtilTests.CloneableStruct item in bag5) {
                int found = 0;
                foreach (UtilTests.CloneableStruct other in bag6) {
                    if (object.Equals(item, other)) {
                        found += 1;
                        Assert.IsFalse(item.Identical(other));
                    }
                }
                Assert.AreEqual(bag5.NumberOfCopies(item), found);
            }

        }

        class NotCloneable { }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CantCloneContents()
        {
            Bag<NotCloneable> bag1 = new Bag<NotCloneable>();

            bag1.Add(new NotCloneable());
            bag1.Add(new NotCloneable());

            Bag<NotCloneable> bag2 = bag1.CloneContents();
        }

        // Strange comparer that uses modulo arithmetic.
        class ModularComparer : IEqualityComparer<int>
        {
            private int mod;

            public ModularComparer(int mod)
            {
                this.mod = mod;
            }

            public bool Equals(int x, int y)
            {
                return (x % mod) == (y % mod);
            }

            public int GetHashCode(int obj)
            {
                return (obj % mod).GetHashCode();
            }
        }

        [Test]
        public void CustomIComparer()
        {
            IEqualityComparer<int> myComparer = new ModularComparer(5);

            Bag<int> bag1 = new Bag<int>(myComparer);
            bag1.Add(3);
            bag1.Add(8);
            bag1.Add(12);
            bag1.Add(9);
            bag1.Add(13);
            bag1.Add(17);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(bag1, new int[] { 3, 3, 3, 9, 12, 12 }, false);
        }

        [Test]
        public void ComparerProperty()
        {
            IEqualityComparer<int> comparer1 = new ModularComparer(5);
            Bag<int> bag1 = new Bag<int>(comparer1);
            Assert.AreSame(comparer1, bag1.Comparer);
            Bag<decimal> bag2 = new Bag<decimal>();
            Assert.AreSame(EqualityComparer<decimal>.Default, bag2.Comparer);
            Bag<string> bag3 = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);
            Assert.AreSame(StringComparer.InvariantCultureIgnoreCase, bag3.Comparer);
        }

        [Test]
        public void Initialize()
        {
            List<int> list = new List<int>(new int[] { 12, 3, 9, 8, 9 });
            Bag<int> set1 = new Bag<int>(list);
            Bag<int> set2 = new Bag<int>(list, new ModularComparer(6));

            InterfaceTests.TestReadWriteCollectionGeneric<int>(set1, new int[] { 3, 8, 9, 9, 12 }, false);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(set2, new int[] { 3, 3, 3, 8, 12 }, false);
        }

        [Test]
        public void DistinctItems()
        {
            Bag<string> bag1 = new Bag<string>(
                new string[] { "foo", null, "Foo", "Eric", "FOO", "eric", "bar" }, StringComparer.InvariantCultureIgnoreCase);

            InterfaceTests.TestEnumerableElementsAnyOrder(bag1.DistinctItems(), new string[] { null, "bar", "Eric", "foo" });

            // Make sure enumeration stops on change.
            int count = 0;
            try {
                foreach (string s in bag1.DistinctItems()) {
                    if (count == 2)
                        bag1.Add("zippy");
                    ++count;
                }
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(3, count);
            }
        }

        [Test]
        public void SerializeStrings()
        {
            Bag<string> d = new Bag<string>(StringComparer.InvariantCultureIgnoreCase);

            d.Add("foo");
            d.Add("world");
            d.Add("hello");
            d.Add("elvis");
            d.Add("ELVIS");
            d.Add(null);
            d.Add("Foo");
            d.AddMany(new string[] { "1", "2", "3", "4", "5", "6" });
            d.AddMany(new string[] { "7", "8", "9", "1", "2", "3" });

            Bag<string> result = (Bag<string>)InterfaceTests.SerializeRoundTrip(d);


            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)result, new string[] { "1", "2", "3", "4", "5", "6", "elvis", "elvis", "hello", "foo", "foo", "WORLD", null, "7", "8", "9", "1", "2", "3" }, false, StringComparer.InvariantCultureIgnoreCase.Equals);

        }

        [Serializable]
        class UniqueStuff
        {
            public InterfaceTests.Unique[] objects;
            public Bag<InterfaceTests.Unique> bag;
        }


        [Test]
        public void SerializeUnique()
        {
            UniqueStuff d = new UniqueStuff(), result = new UniqueStuff();
            InterfaceTests.Unique u1 = new InterfaceTests.Unique("cool"), u2 = new InterfaceTests.Unique("elvis");

            d.objects = new InterfaceTests.Unique[] { 
                new InterfaceTests.Unique("1"), new InterfaceTests.Unique("2"), new InterfaceTests.Unique("3"), new InterfaceTests.Unique("4"), new InterfaceTests.Unique("5"), new InterfaceTests.Unique("6"), 
                u1, u2, new InterfaceTests.Unique("hello"), new InterfaceTests.Unique("foo"), new InterfaceTests.Unique("world"), u2, new InterfaceTests.Unique(null), null,
                new InterfaceTests.Unique("7"), new InterfaceTests.Unique("8"), new InterfaceTests.Unique("9"), u1, u2, new InterfaceTests.Unique("3") };
            d.bag = new Bag<InterfaceTests.Unique>();

            d.bag.Add(d.objects[9]);
            d.bag.Add(d.objects[10]);
            d.bag.Add(d.objects[8]);
            d.bag.Add(d.objects[11]);
            d.bag.Add(d.objects[7]);
            d.bag.Add(d.objects[12]);
            d.bag.Add(d.objects[6]);
            d.bag.Add(d.objects[13]);
            d.bag.AddMany(new InterfaceTests.Unique[] { d.objects[0], d.objects[1], d.objects[2], d.objects[3], d.objects[4], d.objects[5] });
            d.bag.AddMany(new InterfaceTests.Unique[] { d.objects[14], d.objects[15], d.objects[16], d.objects[17], d.objects[18], d.objects[19] });

            result = (UniqueStuff)InterfaceTests.SerializeRoundTrip(d);

            InterfaceTests.TestReadWriteCollectionGeneric<InterfaceTests.Unique>(result.bag, result.objects, false);

            for (int i = 0; i < result.objects.Length; ++i) {
                if (result.objects[i] != null)
                    Assert.IsFalse(object.Equals(result.objects[i], d.objects[i]));
            }
        }


    }
}

