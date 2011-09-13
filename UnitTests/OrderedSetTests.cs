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
    public class OrderedSetTests
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
        public void RandomAddDelete() {
            const int SIZE = 5000;
            bool[] present = new bool[SIZE];
            Random rand = new Random();
            OrderedSet<int> set1 = new OrderedSet<int>();
            bool b;

            // Add and delete values at random.
            for (int i = 0; i < SIZE * 10; ++i) {
                int v = rand.Next(SIZE);
                if (present[v]) {
                    Assert.IsTrue(set1.Contains(v));
                    b = set1.Remove(v);
                    Assert.IsTrue(b);
                    present[v] = false;
                }
                else {
                    Assert.IsFalse(set1.Contains(v));
                    b = set1.Add(v);
                    Assert.IsFalse(b);
                    present[v] = true;
                }
            }

            // Make sure the set has all the correct values in order.
            int last = -1;
            int index = 0;
            foreach (int v in set1) {
                Assert.IsTrue(v > last);
                Assert.AreEqual(v, set1[index]);
                Assert.AreEqual(index, set1.IndexOf(v));
                for (int i = last + 1; i < v; ++i)
                    Assert.IsFalse(present[i]);
                Assert.IsTrue(present[v]);
                last = v;
                ++index;
            }

            int count = 0;
            foreach (bool x in present)
                if (x)
                    ++count;
            Assert.AreEqual(count, set1.Count);

            int[] vals = new int[count];
            int j = 0;
            for (int i = 0; i < present.Length; ++i)
                if (present[i])
                    vals[j++] = i;

            InterfaceTests.TestReadOnlyListGeneric<int>(set1.AsList(), vals, null);

            int[] array = set1.ToArray();
            Assert.IsTrue(Algorithms.EqualCollections(array, vals));
        }

        [Test]
        public void ICollectionInterface()
        {
            string[] s_array = {"Foo", "Eric", "Clapton", "hello", "goodbye", "C#"};
            OrderedSet<string> set1 = new OrderedSet<string>();

            foreach (string s in s_array)
                set1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestCollection<string>((ICollection) set1, s_array, true);
        }


        [Test]
        public void GenericICollectionInterface()
        {
            string[] s_array = { "Foo", "Eric", "Clapton", "hello", "goodbye", "C#", "Java" };
            OrderedSet<string> set1 = new OrderedSet<string>();

            foreach (string s in s_array)
                set1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)set1, s_array, true, null);
        }

        [Test]
        public void Add()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = set1.Add("hello"); Assert.IsFalse(b);
            b = set1.Add("foo"); Assert.IsFalse(b);
            b = set1.Add(""); Assert.IsFalse(b);
            b = set1.Add("HELLO"); Assert.IsTrue(b);
            b = set1.Add("foo"); Assert.IsTrue(b);
            b = set1.Add(null); Assert.IsFalse(b);
            b = set1.Add("Hello"); Assert.IsTrue(b);
            b = set1.Add("Eric"); Assert.IsFalse(b);
            b = set1.Add(null); Assert.IsTrue(b);

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new string[] { null, "", "Eric", "foo", "Hello" }, true, null);
        }

        [Test]
        public void GetItemByIndex()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = set1.Add("hello"); Assert.IsFalse(b);
            b = set1.Add("foo"); Assert.IsFalse(b);
            b = set1.Add(""); Assert.IsFalse(b);
            b = set1.Add("HELLO"); Assert.IsTrue(b);
            b = set1.Add("foo"); Assert.IsTrue(b);
            b = set1.Add(null); Assert.IsFalse(b);
            b = set1.Add("Hello"); Assert.IsTrue(b);
            b = set1.Add("Eric"); Assert.IsFalse(b);
            b = set1.Add(null); Assert.IsTrue(b);

            Assert.AreEqual(set1[0], null);
            Assert.AreEqual(set1[1], "");
            Assert.AreEqual(set1[2], "Eric");
            Assert.AreEqual(set1[3], "foo");
            Assert.AreEqual(set1[4], "Hello");

            try {
                string s = set1[-1];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                string s = set1[5];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                string s = set1[Int32.MaxValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                string s = set1[Int32.MinValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void IndexOf()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = set1.Add("hello"); Assert.IsFalse(b);
            b = set1.Add("foo"); Assert.IsFalse(b);
            b = set1.Add(""); Assert.IsFalse(b);
            b = set1.Add("HELLO"); Assert.IsTrue(b);
            b = set1.Add("foo"); Assert.IsTrue(b);
            b = set1.Add(null); Assert.IsFalse(b);
            b = set1.Add("Hello"); Assert.IsTrue(b);
            b = set1.Add("Eric"); Assert.IsFalse(b);
            b = set1.Add(null); Assert.IsTrue(b);

            Assert.AreEqual(0, set1.IndexOf(null));
            Assert.AreEqual(1, set1.IndexOf(""));
            Assert.AreEqual(2, set1.IndexOf("ERIC"));
            Assert.AreEqual(3, set1.IndexOf("foo"));
            Assert.AreEqual(4, set1.IndexOf("HELlo"));
            Assert.AreEqual(-1, set1.IndexOf("goodbye"));
            Assert.AreEqual(-1, set1.IndexOf("zipf"));
        }

        [Test]
        public void AsList()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = set1.Add("hello"); Assert.IsFalse(b);
            b = set1.Add("foo"); Assert.IsFalse(b);
            b = set1.Add(""); Assert.IsFalse(b);
            b = set1.Add("HELLO"); Assert.IsTrue(b);
            b = set1.Add("foo"); Assert.IsTrue(b);
            b = set1.Add(null); Assert.IsFalse(b);
            b = set1.Add("Hello"); Assert.IsTrue(b);
            b = set1.Add("Eric"); Assert.IsFalse(b);
            b = set1.Add(null); Assert.IsTrue(b);

            InterfaceTests.TestReadOnlyListGeneric(set1.AsList(), new string[] { null, "", "Eric", "foo", "Hello" }, null);

            OrderedSet<string> set2 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestReadOnlyListGeneric(set2.AsList(), new string[] { }, null);

        }

        [Test]
        public void CountAndClear()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);

            Assert.AreEqual(0, set1.Count);
            set1.Add("hello"); Assert.AreEqual(1, set1.Count);
            set1.Add("foo"); Assert.AreEqual(2, set1.Count);
            set1.Add(""); Assert.AreEqual(3, set1.Count);
            set1.Add("HELLO"); Assert.AreEqual(3, set1.Count);
            set1.Add("foo"); Assert.AreEqual(3, set1.Count);
            set1.Add(null); Assert.AreEqual(4, set1.Count);
            set1.Add("Hello"); Assert.AreEqual(4, set1.Count);
            set1.Add("Eric"); Assert.AreEqual(5, set1.Count);
            set1.Add(null); Assert.AreEqual(5, set1.Count);
            set1.Clear();
            Assert.AreEqual(0, set1.Count);

            bool found = false;
            foreach (string s in set1)
                found = true;

            Assert.IsFalse(found);
        }

        [Test]
        public void Remove()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;

            b = set1.Remove("Eric"); Assert.IsFalse(b);
            b = set1.Add("hello"); Assert.IsFalse(b);
            b = set1.Add("foo"); Assert.IsFalse(b);
            b = set1.Add(""); Assert.IsFalse(b);
            b = set1.Remove("HELLO"); Assert.IsTrue(b);
            b = set1.Remove("hello"); Assert.IsFalse(b);
            b = set1.Remove(null); Assert.IsFalse(b);
            b = set1.Add("Hello"); Assert.IsFalse(b);
            b = set1.Add("Eric"); Assert.IsFalse(b);
            b = set1.Add(null); Assert.IsFalse(b);
            b = set1.Remove(null); Assert.IsTrue(b);
            b = set1.Add("Eric"); Assert.IsTrue(b);
            b = set1.Remove("eRic"); Assert.IsTrue(b);
            b = set1.Remove("eRic"); Assert.IsFalse(b);
            set1.Clear();
            b = set1.Remove(""); Assert.IsFalse(b);

        }

        [Test]
        public void TryGetItem()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            bool b;
            string s;

            b = set1.TryGetItem("Eric", out s); Assert.IsFalse(b); Assert.IsNull(s);
            b = set1.Add(null); Assert.IsFalse(b);
            b = set1.Add("hello"); Assert.IsFalse(b);
            b = set1.Add("foo"); Assert.IsFalse(b);
            b = set1.Add(""); Assert.IsFalse(b);
            b = set1.TryGetItem("HELLO", out s); Assert.IsTrue(b); Assert.AreEqual("hello", s);
            b = set1.Remove("hello"); Assert.IsTrue(b);
            b = set1.TryGetItem("HELLO", out s); Assert.IsFalse(b); Assert.IsNull(s);
            b = set1.TryGetItem("foo", out s); Assert.IsTrue(b); Assert.AreEqual("foo", s);
            b = set1.Add("Eric"); Assert.IsFalse(b);
            b = set1.TryGetItem("eric", out s); Assert.IsTrue(b); Assert.AreEqual("Eric", s);
            b = set1.TryGetItem(null, out s); Assert.IsTrue(b); Assert.IsNull(s);
            set1.Clear();
            b = set1.TryGetItem("foo", out s); Assert.IsFalse(b); Assert.IsNull(s);

        }

        [Test]
        public void ToArray()
        {
            string[] s_array = { "Foo", "Eric", "Clapton", "hello", "goodbye", "C#" };
            OrderedSet<string> set1 = new OrderedSet<string>();

            string[] a1 = set1.ToArray();
            Assert.IsNotNull(a1);
            Assert.AreEqual(0, a1.Length);

            foreach (string s in s_array)
                set1.Add(s);
            string[] a2 = set1.ToArray();

            Array.Sort(s_array);

            Assert.AreEqual(s_array.Length, a2.Length);
            for (int i = 0; i < s_array.Length; ++i)
                Assert.AreEqual(s_array[i], a2[i]);
        }

        [Test]
        public void AddMany()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            set1.Add("foo");
            set1.Add("Eric");
            set1.Add("Clapton");
            string[] s_array = { "FOO", "x", "elmer", "fudd", "Clapton", null };
            set1.AddMany(s_array);

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new string[] { null, "Clapton", "elmer", "Eric", "FOO", "fudd", "x" }, true, null);
        }

        [Test]
        public void RemoveMany()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);

            set1.Add("foo");
            set1.Add("Eric");
            set1.Add("Clapton");
            set1.Add(null);
            set1.Add("fudd");
            set1.Add("elmer");
            string[] s_array = { "FOO", "jasmine", "eric", null };
            int count = set1.RemoveMany(s_array);
            Assert.AreEqual(3, count);

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new string[] { "Clapton", "elmer", "fudd" }, true, null);

            set1.Clear();
            set1.Add("foo");
            set1.Add("Eric");
            set1.Add("Clapton");
            set1.Add(null);
            set1.Add("fudd");
            count = set1.RemoveMany(set1);
            Assert.AreEqual(5, count);
            Assert.AreEqual(0, set1.Count);
        }

        [Test]
        public void Exists()
        {
            OrderedSet<double> set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsTrue(set1.Exists(delegate(double d) { return d > 100; }));
            Assert.IsTrue(set1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(set1.Exists(delegate(double d) { return d < -10.0; }));
            set1.Clear();
            Assert.IsFalse(set1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void TrueForAll()
        {
            OrderedSet<double> set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsFalse(set1.TrueForAll(delegate(double d) { return d > 100; }));
            Assert.IsFalse(set1.TrueForAll(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.IsTrue(set1.TrueForAll(delegate(double d) { return d > -10; }));
            Assert.IsTrue(set1.TrueForAll(delegate(double d) { return Math.Abs(d) < 200; }));
            set1.Clear();
            Assert.IsTrue(set1.TrueForAll(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void CountWhere()
        {
            OrderedSet<double> set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.AreEqual(0, set1.CountWhere(delegate(double d) { return d > 200; }));
            Assert.AreEqual(6, set1.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.AreEqual(8, set1.CountWhere(delegate(double d) { return d > -10; }));
            Assert.AreEqual(4, set1.CountWhere(delegate(double d) { return Math.Abs(d) > 5; }));
            set1.Clear();
            Assert.AreEqual(0, set1.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
        }

        [Test]
        public void RemoveAll()
        {
            OrderedSet<double> set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });

            set1.RemoveAll(delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new double[] { -0.04, 1.2, 1.78, 4.5 }, true, null);

            set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }); 
            set1.RemoveAll(delegate(double d) { return d == 0; });
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new double[] { -7.6, -0.04, 1.2, 1.78, 4.5, 7.6, 10.11, 187.4 }, true, null);

            set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            set1.RemoveAll(delegate(double d) { return d < 200; });
            Assert.AreEqual(0, set1.Count);
        }

        [Test]
        public void FindAll()
        {
            OrderedSet<double> set1 = new OrderedSet<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            double[] expected = {-7.6,7.6,10.11, 187.4};
            int i;

            i = 0;
            foreach (double x in set1.FindAll(delegate(double d) { return Math.Abs(d) > 5; })) {
                Assert.AreEqual(expected[i], x);
                ++i;
            }
            Assert.AreEqual(expected.Length, i);
        }

        [Test]
        public void IsDisjointFrom()
        {
            Set<int> set1 = new Set<int>(new int[] { 6, 7, 1, 11, 9, 3, 8 });
            Set<int> set2 = new Set<int>();
            Set<int> set3 = new Set<int>();
            Set<int> set4 = new Set<int>(new int[] { 9, 1, 8, 3, 7, 6, 11 });
            Set<int> set5 = new Set<int>(new int[] { 17, 3, 12, 10 });
            Set<int> set6 = new Set<int>(new int[] { 19, 14, 0, 2 });

            Assert.IsFalse(set1.IsDisjointFrom(set1));
            Assert.IsTrue(set2.IsDisjointFrom(set2));

            Assert.IsTrue(set1.IsDisjointFrom(set2));
            Assert.IsTrue(set2.IsDisjointFrom(set1));

            Assert.IsTrue(set2.IsDisjointFrom(set3));
            Assert.IsTrue(set3.IsDisjointFrom(set2));

            Assert.IsFalse(set1.IsDisjointFrom(set4));
            Assert.IsFalse(set4.IsDisjointFrom(set1));

            Assert.IsFalse(set1.IsDisjointFrom(set5));
            Assert.IsFalse(set5.IsDisjointFrom(set1));

            Assert.IsTrue(set1.IsDisjointFrom(set6));
            Assert.IsTrue(set6.IsDisjointFrom(set1));

            Assert.IsTrue(set5.IsDisjointFrom(set6));
            Assert.IsTrue(set6.IsDisjointFrom(set5));
        }

        [Test]
        public void Intersection()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9});
            OrderedSet<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.IntersectionWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 3, 5, 7, 9 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.IntersectionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 1, 3, 5, 7, 9 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.Intersection(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 3, 5, 7, 9 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.Intersection(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 3, 5, 7, 9 }, true, null);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.IntersectionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, true, null);

            set1 = setDigits.Clone();
            set3 = set1.Intersection(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, true, null);
        }

        [Test]
        public void Union()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            OrderedSet<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.UnionWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.UnionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.Union(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.Union(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.UnionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, true, null);

            set1 = setDigits.Clone();
            set3 = set1.Union(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, true, null);
        }

        [Test]
        public void SymmetricDifference()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            OrderedSet<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.SymmetricDifferenceWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.SymmetricDifferenceWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.SymmetricDifference(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.SymmetricDifference(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.SymmetricDifferenceWith(set1);
            Assert.AreEqual(0, set1.Count);

            set1 = setDigits.Clone();
            set3 = set1.SymmetricDifference(set1);
            Assert.AreEqual(0, set3.Count);
        }

        [Test]
        public void Difference()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            OrderedSet<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.DifferenceWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.DifferenceWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 2, 4, 6, 8 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.Difference(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 11, 13, 15, 17, 19, 21, 23, 25 }, true, null);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.Difference(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 2, 4, 6, 8 }, true, null);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.DifferenceWith(set1);
            Assert.AreEqual(0, set1.Count);

            set1 = setDigits.Clone();
            set3 = set1.Difference(set1);
            Assert.AreEqual(0, set3.Count);
        }

        [Test]
        public void Subset()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 3, 6, 7, 8, 9, 10 });
            OrderedSet<int> set2 = new OrderedSet<int>();
            OrderedSet<int> set3 = new OrderedSet<int>(new int[] { 3, 8, 9 });
            OrderedSet<int> set4 = new OrderedSet<int>(new int[] { 3, 8, 9 });
            OrderedSet<int> set5 = new OrderedSet<int>(new int[] { 1, 2, 6, 8, 9, 10 });

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
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 6, 7, 1, 11, 9, 3, 8 });
            OrderedSet<int> set2 = new OrderedSet<int>();
            OrderedSet<int> set3 = new OrderedSet<int>();
            OrderedSet<int> set4 = new OrderedSet<int>(new int[] { 9, 11, 1, 3, 6, 7, 8, 14 });
            OrderedSet<int> set5 = new OrderedSet<int>(new int[] { 3, 6, 7, 11, 14, 8, 9 });
            OrderedSet<int> set6 = new OrderedSet<int>(new int[] { 1, 3, 6, 7, 8, 10, 11 });
            OrderedSet<int> set7 = new OrderedSet<int>(new int[] { 9, 1, 8, 3, 7, 6, 11 });

            Assert.IsTrue(set1.IsEqualTo(set1));
            Assert.IsTrue(set2.IsEqualTo(set2));

            Assert.IsTrue(set2.IsEqualTo(set3));
            Assert.IsTrue(set3.IsEqualTo(set2));

            Assert.IsTrue(set1.IsEqualTo(set7));
            Assert.IsTrue(set7.IsEqualTo(set1));

            Assert.IsFalse(set1.IsEqualTo(set2));
            Assert.IsFalse(set2.IsEqualTo(set1));

            Assert.IsFalse(set1.IsEqualTo(set4));
            Assert.IsFalse(set4.IsEqualTo(set1));

            Assert.IsFalse(set1.IsEqualTo(set5));
            Assert.IsFalse(set5.IsEqualTo(set1));

            Assert.IsFalse(set1.IsEqualTo(set6));
            Assert.IsFalse(set6.IsEqualTo(set1));

            Assert.IsFalse(set5.IsEqualTo(set6));
            Assert.IsFalse(set6.IsEqualTo(set5));

            Assert.IsFalse(set5.IsEqualTo(set7));
            Assert.IsFalse(set7.IsEqualTo(set5));
        }

        [Test]
        public void Clone()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 7, 9, 11, 13, 15, -17, 19, -21 });
            OrderedSet<int> set2, set3;

            set2 = set1.Clone();
            set3 = (OrderedSet<int>)((ICloneable)set1).Clone();

            Assert.IsFalse(set2 == set1);
            Assert.IsFalse(set3 == set1);

            // Modify set1, make sure set2, set3 don't change.
            set1.Remove(9);
            set1.Remove(-17);
            set1.Add(8);

            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { -21, -17, 1, 7, 9, 11, 13, 15, 19 }, true, null);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { -21, -17, 1, 7, 9, 11, 13, 15, 19 }, true, null);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons1()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, ComparersTests.CompareOddEven);
            setOdds.UnionWith(setDigits);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons2()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new GOddEvenComparer());
            setOdds.SymmetricDifferenceWith(setDigits);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons3()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(new string[] { "foo", "Bar" }, StringComparer.CurrentCulture);
            OrderedSet<string> set2 = new OrderedSet<string>(new string[] { "bada", "bing"}, StringComparer.InvariantCulture);
            set1.Intersection(set2);
        }

        [Test]
        public void ConsistentComparisons()
        {
            OrderedSet<int> setOdds = new OrderedSet<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, ComparersTests.CompareOddEven);
            OrderedSet<int> setDigits = new OrderedSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, ComparersTests.CompareOddEven);
            setOdds.UnionWith(setDigits);

            OrderedSet<string> set1 = new OrderedSet<string>(new string[] { "foo", "Bar" }, StringComparer.InvariantCulture);
            OrderedSet<string> set2 = new OrderedSet<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            set1.Difference(set2);
        }


        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NotComparable1()
        {
            OrderedSet<UncomparableClass1> set1 = new OrderedSet<UncomparableClass1>();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NotComparable2()
        {
            OrderedSet<UncomparableClass2> set1 = new OrderedSet<UncomparableClass2>();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FailFastEnumerator1()
        {
            OrderedSet<double> set1 = new OrderedSet<double>();

            double d = 1.218034;
            for (int i = 0; i < 50; ++i) {
                set1.Add(d);
                d = d * 1.3451 - .31;
            }

            // should throw once the set is modified.
            foreach (double k in set1) {
                if (k > 3.0)
                    set1.Add(1.0);
            }
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FailFastEnumerator2()
        {
            OrderedSet<double> set1 = new OrderedSet<double>();

            double d = 1.218034;
            for (int i = 0; i < 50; ++i) {
                set1.Add(d);
                d = d * 1.3451 - .31;
            }

            // should throw once the set is modified.
            foreach (double k in set1) {
                if (k > 3.0)
                    set1.Clear();
            }
        }

        // Check a View to make sure it has the right stuff.
        private void CheckView<T>(OrderedSet<T>.View view, T[] items, T nonItem)
        {
            Assert.AreEqual(items.Length, view.Count);

            T[] array = view.ToArray();      // Check ToArray
            Assert.AreEqual(items.Length, array.Length);
            for (int i = 0; i < items.Length; ++i) {
                Assert.AreEqual(items[i], array[i]);
                Assert.AreEqual(items[i], view[i]);
                Assert.AreEqual(i, view.IndexOf(items[i]));
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

            InterfaceTests.TestCollection<T>((ICollection)view, items, true);
            InterfaceTests.TestReadOnlyListGeneric<T>(view.AsList(), items, null);
            Array.Reverse(items);
            InterfaceTests.TestCollection<T>((ICollection)view.Reversed(), items, true);
            InterfaceTests.TestReadOnlyListGeneric<T>(view.Reversed().AsList(), items, null);
            Array.Reverse(items);
            InterfaceTests.TestReadWriteCollectionGeneric<T>((ICollection<T>)view, items, true, null);
        }

        // Check Range methods.
        [Test]
        public void Range()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1,3,4,6,8,9,11,14,22});

            CheckView(set1.Clone().Range(4, true, 11, false), new int[] { 4, 6, 8, 9 }, 11);
            CheckView(set1.Clone().Range(4, false, 11, false), new int[] { 6, 8, 9 }, 4);
            CheckView(set1.Clone().Range(4, false, 11, true), new int[] { 6, 8, 9, 11 }, 4);
            CheckView(set1.Clone().Range(4, true, 11, true), new int[] { 4, 6, 8, 9, 11 }, 3);
            CheckView(set1.Clone().Range(4, true, 4, false), new int[] { }, 4);
            CheckView(set1.Clone().Range(4, true, 4, true), new int[] { 4 }, 5);
            CheckView(set1.Clone().Range(11, true, 4, false), new int[] { }, 6);
            CheckView(set1.Clone().Range(11, true, 4, true), new int[] { }, 6);
            CheckView(set1.Clone().Range(0, true, 100, true), new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 }, 0);
            CheckView(set1.Clone().Range(0, false, 100, false), new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 }, 0);
            CheckView(set1.Clone().Range(1, true, 14, false), new int[] { 1, 3, 4, 6, 8, 9, 11 }, 14);
            CheckView(set1.Clone().Range(1, true, 15, false), new int[] { 1, 3, 4, 6, 8, 9, 11, 14 }, 22);
            CheckView(set1.Clone().Range(1, true, 14, true), new int[] { 1, 3, 4, 6, 8, 9, 11, 14 }, 22);
            CheckView(set1.Clone().Range(2, true, 15, false), new int[] { 3, 4, 6, 8, 9, 11, 14 }, 1);
            CheckView(set1.Clone().RangeFrom(9, true), new int[] { 9, 11, 14, 22 }, 8);
            CheckView(set1.Clone().RangeFrom(9, false), new int[] { 11, 14, 22 }, 9);
            CheckView(set1.Clone().RangeFrom(1, true), new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 }, 0);
            CheckView(set1.Clone().RangeFrom(1, false), new int[] { 3, 4, 6, 8, 9, 11, 14, 22 }, 1);
            CheckView(set1.Clone().RangeFrom(100, true), new int[] { }, 1);
            CheckView(set1.Clone().RangeFrom(100, false), new int[] { }, 1);
            CheckView(set1.Clone().RangeTo(9, false), new int[] { 1, 3, 4, 6, 8 }, 9);
            CheckView(set1.Clone().RangeTo(9, true), new int[] { 1, 3, 4, 6, 8, 9 }, 11);
            CheckView(set1.Clone().RangeTo(1, false), new int[] { }, 1);
            CheckView(set1.Clone().RangeTo(0, true), new int[] { }, 1);
            CheckView(set1.Clone().RangeTo(100, false), new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 }, 0);
            CheckView(set1.Clone().RangeTo(100, true), new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 }, 0);
        }

        // Check Range methods.
        [Test]
        public void Reversed()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 });

            CheckView(set1.Reversed(), new int[] { 22, 14, 11, 9, 8, 6, 4, 3, 1 }, 0);
        }

        [Test]
        public void ViewClear()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 });

            set1.Range(6, false, 11, true).Clear();
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 3, 4, 6, 14, 22 }, true, null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ViewAddException1()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 });

            set1.Range(3, true, 8, false).Add(8);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ViewAddException2()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 });

            set1.Range(3, true, 8, false).Add(2);
        }

        [Test]
        public void ViewAddRemove()
        {
            OrderedSet<int> set1 = new OrderedSet<int>(new int[] { 1, 3, 4, 6, 8, 9, 11, 14, 22 });

            Assert.IsFalse(set1.Range(3, true, 8, false).Remove(9));
            Assert.IsTrue(set1.Contains(9));
            Assert.IsFalse(set1.Range(3, true, 8, false).Add(7));
            Assert.IsTrue(set1.Contains(8));
            Assert.IsTrue(set1.Range(3, true, 11, false).Reversed().Remove(4));
            Assert.IsFalse(set1.Contains(4));
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

        void CompareClones<T>(OrderedSet<T> s1, OrderedSet<T> s2)
        {
            IEnumerator<T> e1 = s1.GetEnumerator();
            IEnumerator<T> e2 = s2.GetEnumerator();

            // Check that the sets are equal, but not reference equals (e.g., have been cloned).
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
            OrderedSet<MyInt> set1 = new OrderedSet<MyInt>(
                delegate(MyInt v1, MyInt v2) { 
                    if (v1 == null) {
                        return (v2 == null) ? 0 : -1;
                    }
                    else if (v2 == null)
                        return 1;
                    else
                        return v2.value.CompareTo(v1.value); 
                });

            set1.Add(new MyInt(143));
            set1.Add(new MyInt(2));
            set1.Add(new MyInt(9));
            set1.Add(null);
            set1.Add(new MyInt(14));
            set1.Add(new MyInt(111));
            OrderedSet<MyInt> set2 = set1.CloneContents();
            CompareClones(set1, set2);

            OrderedSet<int> set3 = new OrderedSet<int>(new int[] { 144, 5, 23, 1, 8 });
            OrderedSet<int> set4 = set3.CloneContents();
            CompareClones(set3, set4);

            Comparison<UtilTests.CloneableStruct> comparison = delegate(UtilTests.CloneableStruct s1, UtilTests.CloneableStruct s2) {
                return s1.value.CompareTo(s2.value);
            };
            OrderedSet<UtilTests.CloneableStruct> set5 = new OrderedSet<UtilTests.CloneableStruct>(comparison);
            set5.Add(new UtilTests.CloneableStruct(143));
            set5.Add(new UtilTests.CloneableStruct(5));
            set5.Add(new UtilTests.CloneableStruct(23));
            set5.Add(new UtilTests.CloneableStruct(1));
            set5.Add(new UtilTests.CloneableStruct(8));
            OrderedSet<UtilTests.CloneableStruct> set6 = set5.CloneContents();

            Assert.AreEqual(set5.Count, set6.Count);

            // Check that the sets are equal, but not identical (e.g., have been cloned via ICloneable).
            IEnumerator<UtilTests.CloneableStruct> e1 = set5.GetEnumerator();
            IEnumerator<UtilTests.CloneableStruct> e2 = set6.GetEnumerator();

            // Check that the sets are equal, but not reference equals (e.g., have been cloned).
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
            OrderedSet<NotCloneable> set1 = new OrderedSet<NotCloneable>();

            set1.Add(new NotCloneable());
            set1.Add(new NotCloneable());

            OrderedSet<NotCloneable> set2 = set1.CloneContents();
        }

        [Test]
        public void CustomComparison()
        {
            Comparison<int> myOrdering = ComparersTests.CompareOddEven;

            OrderedSet<int> set1 = new OrderedSet<int>(myOrdering);
            set1.Add(8);
            set1.Add(12);
            set1.Add(9);
            set1.Add(3);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(set1, new int[] { 3, 9, 8, 12 }, true, null);
        }

        [Test]
        public void CustomIComparer()
        {
            IComparer<int> myComparer = new GOddEvenComparer();

            OrderedSet<int> set1 = new OrderedSet<int>(myComparer);
            set1.Add(8);
            set1.Add(12);
            set1.Add(9);
            set1.Add(3);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(set1, new int[] { 3, 9, 8, 12 }, true, null);
        }

        [Test]
        public void ComparerProperty()
        {
            IComparer<int> comparer1 = new GOddEvenComparer();
            OrderedSet<int> set1 = new OrderedSet<int>(comparer1);
            Assert.AreSame(comparer1, set1.Comparer);
            OrderedSet<decimal> set2 = new OrderedSet<decimal>();
            Assert.AreSame(Comparer<decimal>.Default, set2.Comparer);
            OrderedSet<string> set3 = new OrderedSet<string>(StringComparer.OrdinalIgnoreCase);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, set3.Comparer);

            Comparison<int> comparison1 = ComparersTests.CompareOddEven;
            OrderedSet<int> set4 = new OrderedSet<int>(comparison1);
            OrderedSet<int> set5 = new OrderedSet<int>(comparison1);
            Assert.AreEqual(set4.Comparer, set5.Comparer);
            Assert.IsFalse(set4.Comparer == set5.Comparer);
            Assert.IsFalse(object.Equals(set4.Comparer, set1.Comparer));
            Assert.IsFalse(object.Equals(set4.Comparer, Comparer<int>.Default));
            Assert.IsTrue(set4.Comparer.Compare(7, 6) < 0);
        }


        [Test]
        public void Initialize()
        {
            Comparison<int> myOrdering = ComparersTests.CompareOddEven;
            IComparer<int> myComparer = new GOddEvenComparer();
            List<int> list = new List<int>(new int[] { 12, 3, 9, 8, 9, 3 });
            OrderedSet<int> set1 = new OrderedSet<int>(list);
            OrderedSet<int> set2 = new OrderedSet<int>(list, myOrdering);
            OrderedSet<int> set3 = new OrderedSet<int>(list, myComparer);

            InterfaceTests.TestReadWriteCollectionGeneric<int>(set1, new int[] { 3, 8, 9, 12 }, true, null);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(set2, new int[] { 3, 9, 8, 12 }, true, null);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(set3, new int[] { 3, 9, 8, 12 }, true, null);
        }


        [Test]
        public void Smallest()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(
                new string[] { "foo", null, "Foo", "Eric", "FOO", "eric", "bar" }, StringComparer.InvariantCultureIgnoreCase);

            string s;

            Assert.AreEqual(4, set1.Count);

            s = set1.GetFirst();
            Assert.IsNull(s);
            s = set1.RemoveFirst();
            Assert.IsNull(s);
            Assert.AreEqual(3, set1.Count);

            s = set1.GetFirst();
            Assert.AreEqual("bar", s);
            s = set1.RemoveFirst();
            Assert.AreEqual("bar", s);
            Assert.AreEqual(2, set1.Count);

            s = set1.GetFirst();
            Assert.AreEqual("eric", s);
            s = set1.RemoveFirst();
            Assert.AreEqual("eric", s);
            Assert.AreEqual(1, set1.Count);

            s = set1.GetFirst();
            Assert.AreEqual("FOO", s);
            s = set1.RemoveFirst();
            Assert.AreEqual("FOO", s);
            Assert.AreEqual(0, set1.Count);
        }

        [Test]
        public void Largest()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(
                new string[] { "foo", null, "Foo", "Eric", "FOO", "eric", "bar" }, StringComparer.InvariantCultureIgnoreCase);

            string s;

            Assert.AreEqual(4, set1.Count);

            s = set1.GetLast();
            Assert.AreEqual("FOO", s);
            s = set1.RemoveLast();
            Assert.AreEqual("FOO", s);
            Assert.AreEqual(3, set1.Count);

            s = set1.GetLast();
            Assert.AreEqual("eric", s);
            s = set1.RemoveLast();
            Assert.AreEqual("eric", s);
            Assert.AreEqual(2, set1.Count);

            s = set1.GetLast();
            Assert.AreEqual("bar", s);
            s = set1.RemoveLast();
            Assert.AreEqual("bar", s);
            Assert.AreEqual(1, set1.Count);

            s = set1.GetLast();
            Assert.IsNull(s);
            s = set1.RemoveLast();
            Assert.IsNull(s);
            Assert.AreEqual(0, set1.Count);
        }

        [Test]
        public void SmallestLargestException()
        {
            OrderedSet<string> set1 = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);

            try {
                set1.GetFirst();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                set1.GetLast();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                set1.RemoveFirst();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                set1.RemoveLast();
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }
        }

        [Test]
        public void SerializeStrings()
        {
            OrderedSet<string> d = new OrderedSet<string>(StringComparer.InvariantCultureIgnoreCase);

            d.Add("foo");
            d.Add("WORLD");
            d.Add("Hello");
            d.Add("eLVIs");
            d.Add("elvis");
            d.Add(null);
            d.Add("cool");
            d.AddMany(new string[] { "1", "2", "3", "4", "5", "6" });
            d.AddMany(new string[] { "7", "8", "9", "10", "11", "12" });

            OrderedSet<string> result = (OrderedSet<string>)InterfaceTests.SerializeRoundTrip(d);

            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)result, new string[] { null, "1", "10", "11", "12", "2", "3", "4", "5", "6", "7", "8", "9", "cool", "elvis", "foo", "hello", "world" }, true, StringComparer.InvariantCultureIgnoreCase.Equals);

        }


    }
}

