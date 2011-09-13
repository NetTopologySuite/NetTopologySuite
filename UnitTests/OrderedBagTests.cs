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
    public class OrderedBagTests
    {
        class ComparableClass1 : IComparable<ComparableClass1>
        {
            public int Value = 0;
            int IComparable<ComparableClass1>.CompareTo(ComparableClass1 other)
            {
                if (Value > other.Value)
                    return 1;
                else if (Value < other.Value)
                    return -1;
                else
                    return 0;
            }
        }

        class ComparableClass2 : IComparable
        {
            public int Value = 0;
            int IComparable.CompareTo(object other)
            {
                if (other is ComparableClass2) {
                    ComparableClass2 o = (ComparableClass2)other;

                    if (Value > o.Value)
                        return 1;
                    else if (Value < o.Value)
                        return -1;
                    else
                        return 0;
                }
                else
                    throw new ArgumentException("Argument of wrong type.", "other");
            }
        }

        // Not comparable, because the type parameter on ComparableClass is incorrect.
        class UncomparableClass1 : IComparable<ComparableClass1>
        {
            public int Value = 0;
            int IComparable<ComparableClass1>.CompareTo(ComparableClass1 other)
            {
                if (Value > other.Value)
                    return 1;
                else if (Value < other.Value)
                    return -1;
                else
                    return 0;
            }
        }

        class UncomparableClass2
        {
            public int Value = 0;
        }

        [Test]
        public void RandomAddDelete()
        {
            const int SIZE = 5000;
            int[] count = new int[SIZE];
            Random rand = new Random();
            OrderedBag<int> bag1 = new OrderedBag<int>();
            bool b;

            // Add and delete values at random.
            for (int i = 0; i < SIZE * 10; ++i) {
                int v = rand.Next(SIZE);

                // Check that number of copies is equal.
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

            // Make sure the bag has all the correct values in order.
            int c = 0;
            foreach (int x in count)
                c += x;
            Assert.AreEqual(c, bag1.Count);

            int[] vals = new int[c];
            int j = 0;
            for (int i = 0; i < count.Length; ++i) {
                for (int x = 0; x < count[i]; ++x)
                    vals[j++] = i;
            }

            int last = -1;
            int index = 0;
            foreach (int v in bag1) {
                Assert.IsTrue(v >= last);
                Assert.AreEqual(v, bag1[index]);
                if (v > last) {
                    Assert.AreEqual(index, bag1.IndexOf(v));
                    if (last > 0)
                        Assert.AreEqual(index - 1, bag1.LastIndexOf(last));
                }
                for (int i = last; i < v; ++i)
                    Assert.IsTrue(i < 0 || count[i] == 0);
                Assert.IsTrue(count[v] > 0);
                --count[v];
                last = v;
                ++index;
            }

            InterfaceTests.TestReadOnlyListGeneric<int>(bag1.AsList(), vals, null);

            int[] array = bag1.ToArray();
            Assert.IsTrue(Algorithms.EqualCollections(array, vals));
        }

        [Test]
        public void ICollectionInterface()
        {
            string[] s_array = { "Foo", "hello", "Eric", null, "Clapton", "hello", "goodbye", "C#", null };
            OrderedBag<string> bag1 = new OrderedBag<string>();

            foreach (string s in s_array)
                bag1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestCollection<string>((ICollection)bag1, s_array, true);
        }


        [Test]
        public void GenericICollectionInterface()
        {
            string[] s_array = { "Foo", "hello", "Eric", null, "Clapton", "hello", "goodbye", "C#", null };
            OrderedBag<string> bag1 = new OrderedBag<string>();

            foreach (string s in s_array)
                bag1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)bag1, s_array, true, null);
        }

        [Test]
        public void Add()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add("Hello"); 
            bag1.Add("foo"); 
            bag1.Add(""); 
            bag1.Add("HELLO"); 
            bag1.Add("foo"); 
            bag1.Add(null); 
            bag1.Add("hello"); 
            bag1.Add("Eric"); 
            bag1.Add(null); 

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { null, null, "", "Eric", "foo", "foo", "Hello", "HELLO", "hello" }, true, null);
        }

        [Test]
        public void GetItemByIndex()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add("Hello");
            bag1.Add("foo");
            bag1.Add("");
            bag1.Add("HELLO");
            bag1.Add("foo");
            bag1.Add(null);
            bag1.Add("hello");
            bag1.Add("Eric");
            bag1.Add(null); 

            Assert.AreEqual(bag1[0], null);
            Assert.AreEqual(bag1[1], null);
            Assert.AreEqual(bag1[2], "");
            Assert.AreEqual(bag1[3], "Eric");
            Assert.AreEqual(bag1[4], "foo");
            Assert.AreEqual(bag1[5], "foo");
            Assert.AreEqual(bag1[6], "Hello");
            Assert.AreEqual(bag1[7], "HELLO");
            Assert.AreEqual(bag1[8], "hello");

            try {
                string s = bag1[-1];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                string s = bag1[9];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                string s = bag1[Int32.MaxValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                string s = bag1[Int32.MinValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void IndexOf()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add("Hello");
            bag1.Add("foo");
            bag1.Add("");
            bag1.Add("HELLO");
            bag1.Add("foo");
            bag1.Add(null);
            bag1.Add("hello");
            bag1.Add("Eric");
            bag1.Add(null);

            Assert.AreEqual(0, bag1.IndexOf(null));
            Assert.AreEqual(1, bag1.LastIndexOf(null));
            Assert.AreEqual(2, bag1.IndexOf(""));
            Assert.AreEqual(2, bag1.LastIndexOf(""));
            Assert.AreEqual(3, bag1.IndexOf("eric"));
            Assert.AreEqual(3, bag1.LastIndexOf("Eric"));
            Assert.AreEqual(4, bag1.IndexOf("foo"));
            Assert.AreEqual(5, bag1.LastIndexOf("Foo"));
            Assert.AreEqual(6, bag1.IndexOf("heLlo"));
            Assert.AreEqual(8, bag1.LastIndexOf("hEllo"));
        }

        [Test]
        public void AsList()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

            bag1.Add("Hello");
            bag1.Add("foo");
            bag1.Add("");
            bag1.Add("HELLO");
            bag1.Add("foo");
            bag1.Add(null);
            bag1.Add("hello");
            bag1.Add("Eric");
            bag1.Add(null);

            InterfaceTests.TestReadOnlyListGeneric(bag1.AsList(), new string[] { null, null, "", "Eric", "foo", "foo", "Hello", "HELLO", "hello" }, null, StringComparer.InvariantCultureIgnoreCase.Equals);

            OrderedBag<string> bag2 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestReadOnlyListGeneric(bag2.AsList(), new string[] { }, null);

        }

        [Test]
        public void CountAndClear()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

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
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = bag1.Remove("Eric"); Assert.IsFalse(b);
            bag1.Add("hello"); 
            bag1.Add("foo"); 
            bag1.Add(null);
            bag1.Add(null);
            bag1.Add("HELLO");
            bag1.Add("Hello");
            b = bag1.Remove("HELLO"); Assert.IsTrue(b);
            InterfaceTests.TestEnumerableElements(bag1, new string[] {null, null, "foo", "hello", "HELLO"});
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
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);
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
        public void GetEqualItems()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(
                new string[] { "foo", null, "FOO", "Eric", "eric", "bar", null, "foO", "ERIC", "eric", null },
                StringComparer.InvariantCultureIgnoreCase);

            InterfaceTests.TestEnumerableElements(bag1.GetEqualItems("foo"), new string[] { "foo", "FOO", "foO" });
            InterfaceTests.TestEnumerableElements(bag1.GetEqualItems(null), new string[] { null, null, null });
            InterfaceTests.TestEnumerableElements(bag1.GetEqualItems("silly"), new string[] {  });
            InterfaceTests.TestEnumerableElements(bag1.GetEqualItems("ERic"), new string[] { "Eric", "eric", "ERIC", "eric" });
        }


        [Test]
        public void ToArray()
        {
            string[] s_array = { null, "Foo", "Eric", null, "Clapton", "hello", "Clapton", "goodbye", "C#" };
            OrderedBag<string> bag1 = new OrderedBag<string>();

            string[] a1 = bag1.ToArray();
            Assert.IsNotNull(a1);
            Assert.AreEqual(0, a1.Length);

            foreach (string s in s_array)
                bag1.Add(s);
            string[] a2 = bag1.ToArray();

            Array.Sort(s_array);

            Assert.AreEqual(s_array.Length, a2.Length);
            for (int i = 0; i < s_array.Length; ++i)
                Assert.AreEqual(s_array[i], a2[i]);
        }

        [Test]
        public void AddMany()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);
            bag1.Add("foo");
            bag1.Add("Eric");
            bag1.Add("Clapton");
            string[] s_array = { "FOO", "x", "elmer", "fudd", "Clapton", null };
            bag1.AddMany(s_array);

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { null, "Clapton", "Clapton", "elmer", "Eric", "foo", "FOO", "fudd", "x" }, true, null);

            bag1.Clear();
            bag1.Add("foo");
            bag1.Add("Eric");
            bag1.AddMany(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { "Eric", "Eric", "foo", "foo" }, true, null);
        }

        [Test]
        public void RemoveMany()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

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

            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new string[] { "Clapton", "elmer", "foo", "fudd" }, true, null);

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
            OrderedBag<double> bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

            Assert.IsTrue(bag1.Exists(delegate(double d) { return d > 100; }));
            Assert.IsTrue(bag1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(bag1.Exists(delegate(double d) { return d < -10.0; }));
            bag1.Clear();
            Assert.IsFalse(bag1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void TrueForAll()
        {
            OrderedBag<double> bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

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
            OrderedBag<double> bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

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
            OrderedBag<double> bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });

            bag1.RemoveAll(delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new double[] { -0.04, 1.2, 1.2, 1.78, 4.5 }, true, null);

            bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });
            bag1.RemoveAll(delegate(double d) { return d == 0; });
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new double[] { -7.6, -0.04, 1.2, 1.2, 1.78, 4.5, 7.6, 10.11, 187.4, 187.4 }, true, null);

            bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });
            bag1.RemoveAll(delegate(double d) { return d < 200; });
            Assert.AreEqual(0, bag1.Count);
        }

        [Test]
        public void FindAll()
        {
            OrderedBag<double> bag1 = new OrderedBag<double>(new double[] { 4.5, 187.4, 1.2, 7.6, -7.6, -0.04, 1.2, 1.78, 10.11, 187.4 });
            double[] expected = { -7.6, 7.6, 10.11, 187.4, 187.4 };
            int i;

            i = 0;
            foreach (double x in bag1.FindAll(delegate(double d) { return Math.Abs(d) > 5; })) {
                Assert.AreEqual(expected[i], x);
                ++i;
            }
            Assert.AreEqual(expected.Length, i);
        }

        [Test]
        public void IsDisjointFrom()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 3, 6, 7, 1, 1, 11, 9, 3, 8 });
            OrderedBag<int> bag2 = new OrderedBag<int>();
            OrderedBag<int> bag3 = new OrderedBag<int>();
            OrderedBag<int> bag4 = new OrderedBag<int>(new int[] { 8, 9, 1, 8, 3, 7, 6, 11, 7 });
            OrderedBag<int> bag5 = new OrderedBag<int>(new int[] { 17, 3, 12, 10, 22 });
            OrderedBag<int> bag6 = new OrderedBag<int>(new int[] { 14, 19, 14, 0, 2, 14 });

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
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            OrderedBag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.IntersectionWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, true, null);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.IntersectionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, true, null);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Intersection(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, true, null);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Intersection(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 }, true, null);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.IntersectionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, true, null);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Intersection(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, true, null);
        }

        [Test]
        public void Union()
        {
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            OrderedBag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.UnionWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, true, null);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.UnionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, true, null);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Union(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, true, null);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Union(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 }, true);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.UnionWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, true);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Union(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 }, true);
        }

        [Test]
        public void Sum()
        {
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            OrderedBag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.SumWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.SumWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Sum(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Sum(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 4, 5, 5, 5, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 9, 9, 11, 11, 13, 15, 17, 17, 19 }, true);

            // Make sure intersection with itself works.
            bag1 = bagDigits.Clone();
            bag1.SumWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 9, 9 }, true);

            bag1 = bagDigits.Clone();
            bag3 = bag1.Sum(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 5, 5, 5, 5, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 9, 9 }, true);
        }

        [Test]
        public void SymmetricDifference()
        {
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            OrderedBag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.SymmetricDifferenceWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.SymmetricDifferenceWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.SymmetricDifference(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.SymmetricDifference(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 }, true);

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
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 3, 3, 5, 7, 7, 9, 11, 11, 13, 15, 17, 17, 19 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9 });
            OrderedBag<int> bag1, bag2, bag3;

            // Algorithms work different depending on sizes, so try both ways.
            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag1.DifferenceWith(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag2.DifferenceWith(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { 2, 2, 4, 5, 6, 7, 7, 7, 7, 8 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag1.Difference(bag2);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 1, 1, 11, 11, 13, 15, 17, 17, 19 }, true);

            bag1 = bagOdds.Clone(); bag2 = bagDigits.Clone();
            bag3 = bag2.Difference(bag1);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { 2, 2, 4, 5, 6, 7, 7, 7, 7, 8 }, true);

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
            OrderedBag<int> set1 = new OrderedBag<int>(new int[] { 1, 1, 3, 6, 6, 6, 6, 7, 8, 9, 9 });
            OrderedBag<int> set2 = new OrderedBag<int>();
            OrderedBag<int> set3 = new OrderedBag<int>(new int[] { 1, 6, 6, 9, 9 });
            OrderedBag<int> set4 = new OrderedBag<int>(new int[] { 1, 6, 6, 9, 9 });
            OrderedBag<int> set5 = new OrderedBag<int>(new int[] { 1, 1, 3, 6, 6, 6, 7, 7, 8, 9, 9 });

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
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 11, 6, 9, 7, 1, 11, 9, 3, 7, 8, 7 });
            OrderedBag<int> bag2 = new OrderedBag<int>();
            OrderedBag<int> bag3 = new OrderedBag<int>();
            OrderedBag<int> bag4 = new OrderedBag<int>(new int[] { 9, 11, 1, 3, 7, 6, 7, 8, 9, 14, 7 });
            OrderedBag<int> bag5 = new OrderedBag<int>(new int[] { 11, 7, 6, 9, 8, 3, 7, 1, 11, 9, 3 });
            OrderedBag<int> bag6 = new OrderedBag<int>(new int[] { 11, 1, 9, 3, 6, 7, 8, 7, 10, 7, 11, 9 });
            OrderedBag<int> bag7 = new OrderedBag<int>(new int[] { 9, 7, 1, 9, 11, 8, 3, 7, 7, 6, 11 });

            Assert.IsTrue(bag1.IsEqualTo(bag1));
            Assert.IsTrue(bag2.IsEqualTo(bag2));

            Assert.IsTrue(bag2.IsEqualTo(bag3));
            Assert.IsTrue(bag3.IsEqualTo(bag2));

            Assert.IsTrue(bag1.IsEqualTo(bag7));
            Assert.IsTrue(bag7.IsEqualTo(bag1));

            Assert.IsFalse(bag1.IsEqualTo(bag2));
            Assert.IsFalse(bag2.IsEqualTo(bag1));

            Assert.IsFalse(bag1.IsEqualTo(bag4));
            Assert.IsFalse(bag4.IsEqualTo(bag1));

            Assert.IsFalse(bag1.IsEqualTo(bag5));
            Assert.IsFalse(bag5.IsEqualTo(bag1));

            Assert.IsFalse(bag1.IsEqualTo(bag6));
            Assert.IsFalse(bag6.IsEqualTo(bag1));

            Assert.IsFalse(bag5.IsEqualTo(bag6));
            Assert.IsFalse(bag6.IsEqualTo(bag5));

            Assert.IsFalse(bag5.IsEqualTo(bag7));
            Assert.IsFalse(bag7.IsEqualTo(bag5));
        }

        [Test]
        public void Clone()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 7, 9, 11, 7, 13, 15, -17, 19, -21, 1 });
            OrderedBag<int> bag2, bag3;

            bag2 = bag1.Clone();
            bag3 = (OrderedBag<int>)((ICloneable)bag1).Clone();

            Assert.IsFalse(bag2 == bag1);
            Assert.IsFalse(bag3 == bag1);

            // Modify bag1, make sure bag2, bag3 don't change.
            bag1.Remove(9);
            bag1.Remove(-17);
            bag1.Add(8);

            InterfaceTests.TestReadWriteCollectionGeneric(bag2, new int[] { -21, -17, 1, 1, 7, 7, 9, 11, 13, 15, 19 }, true);
            InterfaceTests.TestReadWriteCollectionGeneric(bag3, new int[] { -21, -17, 1, 1, 7, 7, 9, 11, 13, 15, 19 }, true);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons1()
        {
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, ComparersTests.CompareOddEven);
            bagOdds.UnionWith(bagDigits);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons2()
        {
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new GOddEvenComparer());
            bagOdds.SymmetricDifferenceWith(bagDigits);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons3()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(new string[] { "foo", "Bar" }, StringComparer.CurrentCulture);
            OrderedBag<string> bag2 = new OrderedBag<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            bag1.Intersection(bag2);
        }

        [Test]
        public void ConsistentComparisons()
        {
            OrderedBag<int> bagOdds = new OrderedBag<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, ComparersTests.CompareOddEven);
            OrderedBag<int> bagDigits = new OrderedBag<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, ComparersTests.CompareOddEven);
            bagOdds.UnionWith(bagDigits);

            OrderedBag<string> bag1 = new OrderedBag<string>(new string[] { "foo", "Bar" }, StringComparer.InvariantCulture);
            OrderedBag<string> bag2 = new OrderedBag<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            bag1.Difference(bag2);
        }


        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NotComparable1()
        {
            OrderedBag<UncomparableClass1> bag1 = new OrderedBag<UncomparableClass1>();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NotComparable2()
        {
            OrderedBag<UncomparableClass2> bag1 = new OrderedBag<UncomparableClass2>();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FailFastEnumerator1()
        {
            OrderedBag<double> bag1 = new OrderedBag<double>();

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
            OrderedBag<double> bag1 = new OrderedBag<double>();

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

        // Check a View to make sure it has the right stuff.
        private void CheckView<T>(OrderedBag<T>.View view, T[] items, T nonItem)
        {
            Assert.AreEqual(items.Length, view.Count);

            T[] array = view.ToArray();      // Check ToArray
            Assert.AreEqual(items.Length, array.Length);
            for (int i = 0; i < items.Length; ++i) {
                Assert.AreEqual(items[i], array[i]);
                Assert.AreEqual(items[i], view[i]);
                int index = view.IndexOf(items[i]);
                Assert.IsTrue(i == index || index < i && object.Equals(items[index], items[i]));
                index = view.LastIndexOf(items[i]);
                Assert.IsTrue(i == index || index > i && object.Equals(items[index], items[i]));
            }

            if (items.Length > 0) {
                Assert.AreEqual(items[0], view.GetFirst());
                Assert.AreEqual(items[items.Length - 1], view.GetLast());
            }
            else {
                try {
                    view.GetFirst();
                    Assert.Fail("should throw");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is InvalidOperationException);
                }

                try {
                    view.GetLast();
                    Assert.Fail("should throw");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is InvalidOperationException);
                }
            }

            Assert.IsFalse(view.Contains(nonItem));
            Assert.IsTrue(view.IndexOf(nonItem) < 0);
            Assert.IsTrue(view.LastIndexOf(nonItem) < 0);
            InterfaceTests.TestCollection<T>((ICollection)view, items, true);
            InterfaceTests.TestReadOnlyListGeneric<T>(view.AsList(), items, null);
            Array.Reverse(items);
            InterfaceTests.TestCollection<T>((ICollection)view.Reversed(), items, true);
            InterfaceTests.TestReadOnlyListGeneric<T>(view.Reversed().AsList(), items, null);
            Array.Reverse(items);
            InterfaceTests.TestReadWriteCollectionGeneric<T>((ICollection<T>)view, items, true);
        }

        // Check Range methods.
        [Test]
        public void Range()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 });

            CheckView(bag1.Clone().Range(4, true, 11, false), new int[] { 4, 4, 6, 8, 8, 9 }, 11);
            CheckView(bag1.Clone().Range(4, false, 11, false), new int[] { 6, 8, 8, 9 }, 11);
            CheckView(bag1.Clone().Range(4, true, 11, true), new int[] { 4, 4, 6, 8, 8, 9, 11 }, 14);
            CheckView(bag1.Clone().Range(4, false, 11, true), new int[] { 6, 8, 8, 9, 11 }, 4);
            CheckView(bag1.Clone().Range(4, true, 4, false), new int[] { }, 4);
            CheckView(bag1.Clone().Range(4, true, 4, true), new int[] { 4, 4}, 6);
            CheckView(bag1.Clone().Range(4, false, 4, true), new int[] {  }, 4);
            CheckView(bag1.Clone().Range(11, true, 4, false), new int[] { }, 6);
            CheckView(bag1.Clone().Range(0, true, 100, false), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 }, 0);
            CheckView(bag1.Clone().Range(0, false, 100, true), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 }, 0);
            CheckView(bag1.Clone().Range(1, true, 14, false), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11 }, 14);
            CheckView(bag1.Clone().Range(1, true, 14, true), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14 }, 22);
            CheckView(bag1.Clone().Range(1, false, 14, true), new int[] { 3, 4, 4, 6, 8, 8, 9, 11, 14 }, 22);
            CheckView(bag1.Clone().Range(1, true, 15, false), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14 }, 22);
            CheckView(bag1.Clone().Range(2, true, 15, false), new int[] { 3, 4, 4, 6, 8, 8, 9, 11, 14 }, 1);
            CheckView(bag1.Clone().RangeFrom(9, true), new int[] { 9, 11, 14, 22 }, 8);
            CheckView(bag1.Clone().RangeFrom(9, false), new int[] { 11, 14, 22 }, 9);
            CheckView(bag1.Clone().RangeFrom(1, true), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 }, 0);
            CheckView(bag1.Clone().RangeFrom(1, false), new int[] { 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 }, 1);
            CheckView(bag1.Clone().RangeFrom(100, true), new int[] { }, 1);
            CheckView(bag1.Clone().RangeTo(9, false), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8 }, 9);
            CheckView(bag1.Clone().RangeTo(9, true), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9 }, 11);
            CheckView(bag1.Clone().RangeTo(1, false), new int[] { }, 1);
            CheckView(bag1.Clone().RangeTo(1, true), new int[] { 1, 1, 1}, 3);
            CheckView(bag1.Clone().RangeTo(100, false), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 }, 0);
            CheckView(bag1.Clone().RangeTo(100, true), new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 }, 0);
        }

        // Check Range methods.
        [Test]
        public void Reversed()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 });

            CheckView(bag1.Reversed(), new int[] { 22, 14, 11, 9, 8, 8, 6, 4, 4, 3, 1, 1, 1 }, 0);
        }

        [Test]
        public void ViewClear()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 });

            bag1.Range(6, true, 11, false).Clear();
            InterfaceTests.TestReadWriteCollectionGeneric(bag1, new int[] { 1, 1, 1, 3, 4, 4, 11, 14, 22 }, true);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ViewAddException1()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 1, 3, 4, 6, 6, 6, 8, 9, 11, 14, 22 });

            bag1.Range(3, true, 8, false).Add(8);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ViewAddException2()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 1, 3, 4, 6, 6, 6, 8, 9, 11, 14, 22 });

            bag1.Range(3, true, 8, false).Add(2);
        }

        [Test]
        public void ViewAddRemove()
        {
            OrderedBag<int> bag1 = new OrderedBag<int>(new int[] { 1, 1, 1, 3, 4, 4, 6, 8, 8, 9, 11, 14, 22 });

            Assert.IsFalse(bag1.Range(3, true, 8, false).Remove(9));
            Assert.IsTrue(bag1.Contains(9));
            bag1.Range(3, true, 8, false).Add(7);
            bag1.Range(3, true, 8, false).Add(4);
            Assert.IsTrue(bag1.Contains(7));
            Assert.AreEqual(3, bag1.NumberOfCopies(4));
            Assert.IsTrue(bag1.Range(3, true, 11, false).Reversed().Remove(4));
            Assert.AreEqual(2, bag1.NumberOfCopies(4));
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
                return (obj is MyInt && ((MyInt)obj).value == value);
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

        void CompareClones<T>(OrderedBag<T> s1, OrderedBag<T> s2)
        {
            IEnumerator<T> e1 = s1.GetEnumerator();
            IEnumerator<T> e2 = s2.GetEnumerator();

            // Check that the bags are equal, but not reference equals (e.g., have been cloned).
            while (e1.MoveNext()) {
                e2.MoveNext();
                if (e1.Current == null) {
                    Assert.IsNull(e2.Current);
                }
                else {
                    Assert.IsTrue(e1.Current.Equals(e2.Current));
                    Assert.IsFalse(object.ReferenceEquals(e1.Current, e2.Current));
                }
            }
        }

        [Test]
        public void CloneContents()
        {
            OrderedBag<MyInt> bag1 = new OrderedBag<MyInt>(
                delegate(MyInt v1, MyInt v2) {
                if (v1 == null) {
                    return (v2 == null) ? 0 : -1;
                }
                else if (v2 == null)
                    return 1;
                else
                    return v2.value.CompareTo(v1.value);
            });

            MyInt mi = new MyInt(9);
            bag1.Add(new MyInt(14));
            bag1.Add(new MyInt(143));
            bag1.Add(new MyInt(2));
            bag1.Add(mi);
            bag1.Add(null);
            bag1.Add(new MyInt(14));
            bag1.Add(new MyInt(111));
            bag1.Add(mi);
            OrderedBag<MyInt> bag2 = bag1.CloneContents();
            CompareClones(bag1, bag2);

            OrderedBag<int> bag3 = new OrderedBag<int>(new int[] { 144, 1, 5, 23, 1, 8 });
            OrderedBag<int> bag4 = bag3.CloneContents();
            CompareClones(bag3, bag4);

            Comparison<UtilTests.CloneableStruct> comparison = delegate(UtilTests.CloneableStruct s1, UtilTests.CloneableStruct s2) {
                return s1.value.CompareTo(s2.value);
            };
            OrderedBag<UtilTests.CloneableStruct> bag5 = new OrderedBag<UtilTests.CloneableStruct>(comparison);
            bag5.Add(new UtilTests.CloneableStruct(143));
            bag5.Add(new UtilTests.CloneableStruct(1));
            bag5.Add(new UtilTests.CloneableStruct(23));
            bag5.Add(new UtilTests.CloneableStruct(1));
            bag5.Add(new UtilTests.CloneableStruct(8));
            OrderedBag<UtilTests.CloneableStruct> bag6 = bag5.CloneContents();

            Assert.AreEqual(bag5.Count, bag6.Count);

            // Check that the bags are equal, but not identical (e.g., have been cloned via ICloneable).
            IEnumerator<UtilTests.CloneableStruct> e1 = bag5.GetEnumerator();
            IEnumerator<UtilTests.CloneableStruct> e2 = bag6.GetEnumerator();

            // Check that the bags are equal, but not reference equals (e.g., have been cloned).
            while (e1.MoveNext()) {
                e2.MoveNext();
                Assert.IsTrue(e1.Current.Equals(e2.Current));
                Assert.IsFalse(e1.Current.Identical(e2.Current));
            }
        }

        class NotCloneable { }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CantCloneContents()
        {
            OrderedBag<NotCloneable> bag1 = new OrderedBag<NotCloneable>();

            bag1.Add(new NotCloneable());
            bag1.Add(new NotCloneable());

            OrderedBag<NotCloneable> bag2 = bag1.CloneContents();
        }

        [Test]
        public void CustomComparison()
        {
            Comparison<int> myOrdering = ComparersTests.CompareOddEven;

            OrderedBag<int> bag1 = new OrderedBag<int>(myOrdering);
            bag1.Add(8);
            bag1.Add(12);
            bag1.Add(9);
            bag1.Add(9);
            bag1.Add(3);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(bag1, new int[] { 3, 9, 9, 8, 12 }, true);
        }

        [Test]
        public void CustomIComparer()
        {
            IComparer<int> myComparer = new GOddEvenComparer();

            OrderedBag<int> bag1 = new OrderedBag<int>(myComparer);
            bag1.Add(3);
            bag1.Add(8);
            bag1.Add(12);
            bag1.Add(9);
            bag1.Add(3);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(bag1, new int[] { 3, 3, 9, 8, 12 }, true);
        }

        [Test]
        public void ComparerProperty()
        {
            IComparer<int> comparer1 = new GOddEvenComparer();
            OrderedBag<int> bag1 = new OrderedBag<int>(comparer1);
            Assert.AreSame(comparer1, bag1.Comparer);
            OrderedBag<decimal> bag2 = new OrderedBag<decimal>();
            Assert.AreSame(Comparer<decimal>.Default, bag2.Comparer);
            OrderedBag<string> bag3 = new OrderedBag<string>(StringComparer.OrdinalIgnoreCase);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, bag3.Comparer);

            Comparison<int> comparison1 = ComparersTests.CompareOddEven;
            OrderedBag<int> bag4 = new OrderedBag<int>(comparison1);
            OrderedBag<int> bag5 = new OrderedBag<int>(comparison1);
            Assert.AreEqual(bag4.Comparer, bag5.Comparer);
            Assert.IsFalse(bag4.Comparer == bag5.Comparer);
            Assert.IsFalse(object.Equals(bag4.Comparer, bag1.Comparer));
            Assert.IsFalse(object.Equals(bag4.Comparer, Comparer<int>.Default));
            Assert.IsTrue(bag4.Comparer.Compare(7, 6) < 0);
        }

        [Test]
        public void Initialize()
        {
            Comparison<int> myOrdering = ComparersTests.CompareOddEven;
            IComparer<int> myComparer = new GOddEvenComparer();
            List<int> list = new List<int>(new int[] { 12, 3, 9, 8, 9, 3 });
            OrderedBag<int> bag1 = new OrderedBag<int>(list);
            OrderedBag<int> bag2 = new OrderedBag<int>(list, myOrdering);
            OrderedBag<int> bag3 = new OrderedBag<int>(list, myComparer);

            InterfaceTests.TestReadWriteCollectionGeneric<int>(bag1, new int[] { 3, 3, 8, 9, 9, 12 }, true);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(bag2, new int[] { 3, 3, 9, 9, 8, 12 }, true);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(bag3, new int[] { 3, 3, 9, 9, 8, 12 }, true);
        }

        [Test]
        public void DistinctItems()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(
                new string[] { "foo", null, "Foo", "Eric", "FOO", "eric", "bar" }, StringComparer.InvariantCultureIgnoreCase);

            InterfaceTests.TestEnumerableElements(bag1.DistinctItems(), new string[] { null, "bar", "Eric", "foo" });

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
        public void Smallest()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(
                new string[] { "foo", null, "Foo", "Eric", "FOO", "eric", "bar" }, StringComparer.InvariantCultureIgnoreCase);

            string s;

            Assert.AreEqual(7, bag1.Count);

            s = bag1.GetFirst();
            Assert.IsNull(s);
            s = bag1.RemoveFirst();
            Assert.IsNull(s);
            Assert.AreEqual(6, bag1.Count);

            s = bag1.GetFirst();
            Assert.AreEqual("bar", s);
            s = bag1.RemoveFirst();
            Assert.AreEqual("bar", s);
            Assert.AreEqual(5, bag1.Count);

            s = bag1.GetFirst();
            Assert.AreEqual("Eric", s);
            s = bag1.RemoveFirst();
            Assert.AreEqual("Eric", s);
            Assert.AreEqual(4, bag1.Count);

            s = bag1.GetFirst();
            Assert.AreEqual("eric", s);
            s = bag1.RemoveFirst();
            Assert.AreEqual("eric", s);
            Assert.AreEqual(3, bag1.Count);
        }

        [Test]
        public void Largest()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(
                new string[] { "foo", null, "Foo", "Eric", "FOO", "eric", "bar" }, StringComparer.InvariantCultureIgnoreCase);

            string s;

            Assert.AreEqual(7, bag1.Count);

            s = bag1.GetLast();
            Assert.AreEqual("FOO", s);
            s = bag1.RemoveLast();
            Assert.AreEqual("FOO", s);
            Assert.AreEqual(6, bag1.Count);

            s = bag1.GetLast();
            Assert.AreEqual("Foo", s);
            s = bag1.RemoveLast();
            Assert.AreEqual("Foo", s);
            Assert.AreEqual(5, bag1.Count);

            s = bag1.GetLast();
            Assert.AreEqual("foo", s);
            s = bag1.RemoveLast();
            Assert.AreEqual("foo", s);
            Assert.AreEqual(4, bag1.Count);

            s = bag1.GetLast();
            Assert.AreEqual("eric", s);
            s = bag1.RemoveLast();
            Assert.AreEqual("eric", s);
            Assert.AreEqual(3, bag1.Count);
        }

        [Test]
        public void SmallestLargestException()
        {
            OrderedBag<string> bag1 = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

            try {
                bag1.GetFirst();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                bag1.GetLast();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                bag1.RemoveFirst();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                bag1.RemoveLast();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }
        }

        [Test]
        public void SerializeStrings()
        {
            OrderedBag<string> d = new OrderedBag<string>(StringComparer.InvariantCultureIgnoreCase);

            d.Add(null);
            d.Add("hello");
            d.Add("foo");
            d.Add("WORLD");
            d.Add("Hello");
            d.Add("eLVIs");
            d.Add("elvis");
            d.Add(null);
            d.Add("cool");
            d.AddMany(new string[] { "1", "2", "3", "4", "5", "6" });
            d.AddMany(new string[] { "7", "8", "9", "10", "11", "12" });

            OrderedBag<string> result = (OrderedBag<string>)InterfaceTests.SerializeRoundTrip(d);

            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)result, 
                new string[] { null, null, "1", "10", "11", "12", "2", "3", "4", "5", "6", "7", "8", "9", "cool", "eLVIs", "elvis", "foo", "hello", "Hello", "WORLD" }, true);
        }


    }
}

