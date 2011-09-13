//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

using System;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;

namespace Wintellect.PowerCollections.Tests
{
    using System.Globalization;

    [TestFixture]
    public class AlgorithmsTests
    {
        // Create an IEnumerable that enumerates an array. Make sure that only enumerable stuff
        // is used and no downcasts to ICollection are taken advantage of.
        public static IEnumerable<T> EnumerableFromArray<T>(T[] array)
        {
            foreach (T t in array)
                yield return t;
        }

        [Test]
        public void Exists()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });
            IEnumerable<double> coll2 = EnumerableFromArray(new double[] {  });

            Assert.IsTrue(Algorithms.Exists(coll1, delegate(double d) { return d > 100; }));
            Assert.IsTrue(Algorithms.Exists(coll1, delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(Algorithms.Exists(coll1, delegate(double d) { return d < -10.0; }));
            Assert.IsFalse(Algorithms.Exists(coll2, delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void TrueForAll()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });
            IEnumerable<double> coll2 = EnumerableFromArray(new double[] {  });

            Assert.IsFalse(Algorithms.TrueForAll(coll1, delegate(double d) { return d > 100; }));
            Assert.IsFalse(Algorithms.TrueForAll(coll1, delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.IsTrue(Algorithms.TrueForAll(coll1, delegate(double d) { return d > -10; }));
            Assert.IsTrue(Algorithms.TrueForAll(coll1, delegate(double d) { return Math.Abs(d) < 200; }));
            Assert.IsTrue(Algorithms.TrueForAll(coll2, delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void CountWhere()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });
            IEnumerable<double> coll2 = EnumerableFromArray(new double[] {  });

            Assert.AreEqual(0, Algorithms.CountWhere(coll1, delegate(double d) { return d > 200; }));
            Assert.AreEqual(6, Algorithms.CountWhere(coll1, delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.AreEqual(8, Algorithms.CountWhere(coll1, delegate(double d) { return d > -10; }));
            Assert.AreEqual(4, Algorithms.CountWhere(coll1, delegate(double d) { return Math.Abs(d) > 5; }));
            Assert.AreEqual(0, Algorithms.CountWhere(coll2, delegate(double d) { return Math.Abs(d) < 10; }));
        }

        [Test]
        public void Count()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });
            IEnumerable<double> coll2 = EnumerableFromArray(new double[] { });
            IEnumerable<double> coll3 = new List<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });
            IEnumerable<double> coll4 = new List<double>(new double[] { });

            Assert.AreEqual(8, Algorithms.Count(coll1));
            Assert.AreEqual(0, Algorithms.Count(coll2));
            Assert.AreEqual(8, Algorithms.Count(coll3));
            Assert.AreEqual(0, Algorithms.Count(coll4));
        }

        [Test]
        public void RemoveWhenTrueCollection()
        {
            List<double> d_list = new List<double>(new double[]{ 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            ICollection<double> removed;

            removed = Algorithms.RemoveWhere((ICollection<double>)d_list, delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestReadWriteCollectionGeneric(d_list, new double[] { 4.5, 1.2, -0.04, 1.78}, true);
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] { 7.6, -7.6, 10.11, 187.4 }, true);

            d_list = new List<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            removed = Algorithms.RemoveWhere((ICollection<double>)d_list, delegate(double d) { return d == 0; });
            InterfaceTests.TestReadWriteCollectionGeneric(d_list, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }, true);
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] {}, true);

            d_list = new List<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            removed = Algorithms.RemoveWhere((ICollection<double>)d_list, delegate(double d) { return d < 200; });
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }, true);
            Assert.AreEqual(0, d_list.Count);

        }

        [Test]
        public void RemoveWhenTrueList()
        {
            IList<double> d_list = new List<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            ICollection<double> removed;

            removed = Algorithms.RemoveWhere(d_list, delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestListGeneric(d_list, new double[] { 4.5, 1.2, -0.04, 1.78 });
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] { 7.6, -7.6, 10.11, 187.4 }, true);

            d_list = new List<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            removed = Algorithms.RemoveWhere(d_list, delegate(double d) { return d == 0; });
            InterfaceTests.TestListGeneric(d_list, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] { }, true);

            d_list = new List<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            removed = Algorithms.RemoveWhere(d_list, delegate(double d) { return d < 200; });
            InterfaceTests.TestReadWriteCollectionGeneric<double>(removed, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }, true);
            Assert.AreEqual(0, d_list.Count);

            d_list = new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 };
            removed = Algorithms.RemoveWhere<double>(d_list, delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestEnumerableElements<double>(d_list, new double[] { 4.5, 1.2, -0.04, 1.78, 0, 0, 0, 0 });
            InterfaceTests.TestReadWriteCollectionGeneric<double>(removed, new double[] { 7.6, -7.6, 10.11, 187.4 }, true);
        }

        [Test]
        public void FindAll()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            double[] expected = { 7.6, -7.6, 10.11, 187.4 };
            int i;

            i = 0;
            foreach (double x in Algorithms.FindWhere(coll1, delegate(double d) { return Math.Abs(d) > 5; })) {
                Assert.AreEqual(expected[i], x);
                ++i;
            }
            Assert.AreEqual(expected.Length, i);
        }

        [Test]
        public void Convert()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            IEnumerable<int> result1 = Algorithms.Convert<double, int>(coll1, delegate(double x) { return (int)Math.Round(x); });
            InterfaceTests.TestEnumerableElements(result1, new int[] { 4, 1, 8, 0, -8, 2, 10, 187 });

            IEnumerable<double> coll2 = EnumerableFromArray(new double[0]);
            IEnumerable<int> result2 = Algorithms.Convert<double, int>(coll2, delegate(double x) { return (int)Math.Round(x); });
            InterfaceTests.TestEnumerableElements(result2, new int[0]);

            IEnumerable<double> coll3 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            IEnumerable<double> result3 = Algorithms.Convert<double, double>(coll3, Math.Abs);
            InterfaceTests.TestEnumerableElements(result3, new double[] { 4.5, 1.2, 7.6, 0.04, 7.6, 1.78, 10.11, 187.4 });
        }

        [Test]
        public void ForEach()
        {
            IEnumerable<string> coll1 = EnumerableFromArray(new string[] { "foo", "bar", "hello", "sailor" });
            string s = "";
            Algorithms.ForEach(coll1, delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "!foo!bar!hello!sailor");

            IEnumerable<string> coll2 = EnumerableFromArray(new string[0]);
            s = "";
            Algorithms.ForEach(coll2, delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "");
        }

        [Test]
        public void Partition()
        {
            List<double> list;
            int index;
            Predicate<double> isNegative = delegate(double x) { return x < 0; };

            list = new List<double>();
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(0, index);
            Assert.AreEqual(0, list.Count);

            list = new List<double>(new double[] { -3.1 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(1, index);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(-3.1, list[0]);

            list = new List<double>(new double[] { 3.1 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(0, index);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(3.1, list[0]);

            list = new List<double>(new double[] { -2, 3.1 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(1, index);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(-2, list[0]);
            Assert.AreEqual(3.1, list[1]);

            list = new List<double>(new double[] { 2, -3.1 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(1, index);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(-3.1, list[0]);
            Assert.AreEqual(2, list[1]);

            list = new List<double>(new double[] { -2, -3.1 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(2, index);
            InterfaceTests.TestEnumerableElementsAnyOrder(list, new double[] { -2, -3.1 });

            list = new List<double>(new double[] { 2, 3.1 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(0, index);
            InterfaceTests.TestEnumerableElementsAnyOrder(list, new double[] { 2, 3.1 });

            list = new List<double>(new double[] { 2, 6, -8, -7, 3, -1, -2, 4, 2 });
            index = Algorithms.Partition(list, isNegative);
            Assert.AreEqual(4, index);
            InterfaceTests.TestEnumerableElementsAnyOrder(list.GetRange(0, index), new double[] { -8, -7, -1, -2 });
            InterfaceTests.TestEnumerableElementsAnyOrder(list.GetRange(index, list.Count - index), new double[] {2, 6, 3, 4, 2 });

            double[] array = new double[] { 2, 6, -8, -7, 3, -1, -2, 4, 2 };
            index = Algorithms.Partition(array, isNegative);
            Assert.AreEqual(4, index);
            InterfaceTests.TestEnumerableElementsAnyOrder(Algorithms.Range(array, 0, index), new double[] { -8, -7, -1, -2 });
            InterfaceTests.TestEnumerableElementsAnyOrder(Algorithms.Range(array, index, list.Count - index), new double[] { 2, 6, 3, 4, 2 });
        }

        [Test]
        public void StablePartition()
        {
            List<double> list;
            int index;
            Predicate<double> isNegative = delegate(double x) { return x < 0; };

            list = new List<double>();
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(0, index);
            Assert.AreEqual(0, list.Count);

            list = new List<double>(new double[] { -3.1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(1, index);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(-3.1, list[0]);

            list = new List<double>(new double[] { 3.1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(0, index);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(3.1, list[0]);

            list = new List<double>(new double[] { -2, 3.1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(1, index);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(-2, list[0]);
            Assert.AreEqual(3.1, list[1]);

            list = new List<double>(new double[] { 2, -3.1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(1, index);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(-3.1, list[0]);
            Assert.AreEqual(2, list[1]);

            list = new List<double>(new double[] { -2, -3.1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(2, index);
            InterfaceTests.TestEnumerableElements(list, new double[] { -2, -3.1 });

            list = new List<double>(new double[] { 2, 3.1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(0, index);
            InterfaceTests.TestEnumerableElements(list, new double[] { 2, 3.1 });

            list = new List<double>(new double[] { 2, 6, -8, -7, 3, -1, -2, 4, 2 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(4, index);
            InterfaceTests.TestEnumerableElements(list.GetRange(0, index), new double[] { -8, -7, -1, -2 });
            InterfaceTests.TestEnumerableElements(list.GetRange(index, list.Count - index), new double[] { 2, 6, 3, 4, 2 });

            double[] array = { 2, 6, -8, -7, 3, -1, -2, 4, 2 };
            index = Algorithms.StablePartition(array, isNegative);
            Assert.AreEqual(4, index);
            InterfaceTests.TestEnumerableElements(Algorithms.Range(array, 0, index), new double[] { -8, -7, -1, -2 });
            InterfaceTests.TestEnumerableElements(Algorithms.Range(array, index, list.Count - index), new double[] { 2, 6, 3, 4, 2 });

            list = new List<double>(new double[] { 2, 6, 9, 1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(0, index);
            InterfaceTests.TestEnumerableElements(list.GetRange(0, index), new double[] {  });
            InterfaceTests.TestEnumerableElements(list.GetRange(index, list.Count - index), new double[] { 2, 6, 9, 1 });

            list = new List<double>(new double[] { -2, -6, -9, -1 });
            index = Algorithms.StablePartition(list, isNegative);
            Assert.AreEqual(4, index);
            InterfaceTests.TestEnumerableElements(list.GetRange(0, index), new double[] { -2, -6, -9, -1 });
            InterfaceTests.TestEnumerableElements(list.GetRange(index, list.Count - index), new double[] {  });
        }

        [Test]
        public void ToArrayEnumerable()
        {
            IEnumerable<double> coll1 = EnumerableFromArray(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            double[] array1 = Algorithms.ToArray(coll1);
            double[] expected = { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 };
            Assert.AreEqual(expected.Length, array1.Length);
            for (int i = 0; i < array1.Length; ++i) {
                Assert.AreEqual(expected[i], array1[i]);
            }
        }

        [Test]
        public void ToArrayCollection()
        {
            ICollection<double> coll1 = new List<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            double[] array1 = Algorithms.ToArray(coll1);
            double[] expected = { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 };
            Assert.AreEqual(expected.Length, array1.Length);
            for (int i = 0; i < array1.Length; ++i) {
                Assert.AreEqual(expected[i], array1[i]);
            }
        }

        [Test]
        public void CountEqual()
        {
            IEnumerable<string> coll1 = EnumerableFromArray(new string[] { "foo", "bar", "eric", "Eric", "BAR", "ERIC", "eric" });
            IEnumerable<int> coll2 = EnumerableFromArray(new int[0]);

            Assert.AreEqual(1, Algorithms.CountEqual(coll1, "foo"));
            Assert.AreEqual(2, Algorithms.CountEqual(coll1, "eric"));
            Assert.AreEqual(0, Algorithms.CountEqual(coll1, "clapton"));
            Assert.AreEqual(1, Algorithms.CountEqual(coll1, "foo", StringComparer.CurrentCultureIgnoreCase));
            Assert.AreEqual(4, Algorithms.CountEqual(coll1, "eric", StringComparer.CurrentCultureIgnoreCase));
            Assert.AreEqual(0, Algorithms.CountEqual(coll1, "clapton", StringComparer.CurrentCultureIgnoreCase));
            Assert.AreEqual(0, Algorithms.CountEqual(coll2, 4));
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NCopiesOfFailure()
        {
            IEnumerable<double> coll1 = Algorithms.NCopiesOf(-1, 3.4);
            foreach (double d in coll1)
                ;
        }

        [Test]
        public void NCopiesOf()
        {
            IEnumerable<double> coll1 = Algorithms.NCopiesOf(17, 3.4);

            int count = 0;
            foreach (double d in coll1) {
                ++count;
                Assert.AreEqual(3.4, d);
            }
            Assert.AreEqual(17, count);

            IEnumerable<int> coll2 = Algorithms.NCopiesOf(0, 4);
            foreach (int i in coll2) {
                Assert.Fail();
            }
        }

        [Test]
        public void Concatenate()
        {
            string[] coll1 = { "hello", "there", "sailor" };
            List<string> coll2 = new List<string>(new string[] { "eric", "clapton" });
            OrderedSet<string> coll3 = new OrderedSet<string>(new string[] { "ghi", "xyz", "abc" });

            Assert.IsTrue(Algorithms.EqualCollections<string>(Algorithms.Concatenate<string>(coll1, new string[0], coll2, coll3, coll1),
                new string[] { "hello", "there", "sailor", "eric", "clapton", "abc", "ghi", "xyz", "hello", "there", "sailor" }));
            Assert.AreEqual(0, Algorithms.ToArray(Algorithms.Concatenate<string>()).Length);
            Assert.IsTrue(Algorithms.EqualCollections<string>(Algorithms.Concatenate(coll3),
                new string[] { "abc", "ghi", "xyz" }));
        }

        [Test]
        public void Replace1()
        {
            IEnumerable<string> enum1 = EnumerableFromArray(new string[0]);
            IEnumerable<string> enum2 = EnumerableFromArray(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });

            IEnumerable<string> result1 = Algorithms.Replace(enum1, "foo", "bar");
            InterfaceTests.TestEnumerableElements(result1, new string[0]);

            result1 = Algorithms.Replace(enum2, "foo", "baz");
            InterfaceTests.TestEnumerableElements(result1, new string[] { "baz", "bar", "FOO", "hello", "hello", "sailor", "baz", "bar" });

            result1 = Algorithms.Replace(enum2, "foo", "bar");
            InterfaceTests.TestEnumerableElements(result1, new string[] { "bar", "bar", "FOO", "hello", "hello", "sailor", "bar", "bar" });

            result1 = Algorithms.Replace(enum2, "X", "bar");
            InterfaceTests.TestEnumerableElements(result1, new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });

            result1 = Algorithms.Replace(enum2, "bar", "X");
            InterfaceTests.TestEnumerableElements(result1, new string[] { "foo", "X", "FOO", "hello", "hello", "sailor", "foo", "X" });

            result1 = Algorithms.Replace(enum2, "foo", "X", StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElements(result1, new string[] { "X", "bar", "X", "hello", "hello", "sailor", "X", "bar" });
        }

        [Test]
        public void Replace2()
        {
            IEnumerable<string> enum1 = EnumerableFromArray(new string[0]);
            IEnumerable<string> enum2 = EnumerableFromArray(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });

            IEnumerable<string> result1 = Algorithms.Replace(enum1, delegate(string x) { return x == "foo";}, "bar");
            InterfaceTests.TestEnumerableElements(result1, new string[0]);

            result1 = Algorithms.Replace(enum2, delegate(string x) { return x[0] == 'h'; }, "baz");
            InterfaceTests.TestEnumerableElements(result1, new string[] { "foo", "bar", "FOO", "baz", "baz", "sailor", "foo", "bar", "baz" });

            result1 = Algorithms.Replace(enum2, delegate(string x) { return x.Length == 3; }, "X");
            InterfaceTests.TestEnumerableElements(result1, new string[] { "X", "X", "X", "hello", "hello", "sailor", "X", "X", "hi" });

            result1 = Algorithms.Replace(enum2, delegate(string x) { return x.Length == 3; }, null);
            InterfaceTests.TestEnumerableElements(result1, new string[] { null, null, null, "hello", "hello", "sailor", null, null, "hi"});

            result1 = Algorithms.Replace(enum2, delegate(string x) { x = "zip"; return false; }, null);
            InterfaceTests.TestEnumerableElements(result1, new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });
        }

        [Test]
        public void ReplaceInPlace1()
        {
            IList<string> list1 = new List<string>(new string[0]);
            IList<string> list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });

            Algorithms.ReplaceInPlace(list1, "foo", "bar");
            InterfaceTests.TestListGeneric(list1, new string[0]);

            Algorithms.ReplaceInPlace(list2, "foo", "baz");
            InterfaceTests.TestListGeneric(list2, new string[] { "baz", "bar", "FOO", "hello", "hello", "sailor", "baz", "bar" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });
            Algorithms.ReplaceInPlace(list2, "foo", "bar");
            InterfaceTests.TestListGeneric(list2, new string[] { "bar", "bar", "FOO", "hello", "hello", "sailor", "bar", "bar" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });
            Algorithms.ReplaceInPlace(list2, "X", "bar");
            InterfaceTests.TestListGeneric(list2, new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });
            Algorithms.ReplaceInPlace(list2, "bar", "X");
            InterfaceTests.TestListGeneric(list2, new string[] { "foo", "X", "FOO", "hello", "hello", "sailor", "foo", "X" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" });
            Algorithms.ReplaceInPlace(list2, "foo", "X", StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list2, new string[] { "X", "bar", "X", "hello", "hello", "sailor", "X", "bar" });

            string[] array1 = new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" };
            Algorithms.ReplaceInPlace(array1, "bar", "X");
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "X", "FOO", "hello", "hello", "sailor", "foo", "X" });

            array1 = new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar" };
            Algorithms.ReplaceInPlace(array1, "foo", "X", StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "X", "bar", "X", "hello", "hello", "sailor", "X", "bar" });
        }

        [Test]
        public void ReplaceInPlace2()
        {
            IList<string> list1 = new List<string>(new string[0]);

            Algorithms.ReplaceInPlace(list1, delegate(string x) { return x == "foo"; }, "bar");
            InterfaceTests.TestListGeneric(list1, new string[0]);

            IList<string> list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });
            Algorithms.ReplaceInPlace(list2, delegate(string x) { return x[0] == 'h'; }, "baz");
            InterfaceTests.TestListGeneric(list2, new string[] { "foo", "bar", "FOO", "baz", "baz", "sailor", "foo", "bar", "baz" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });
            Algorithms.ReplaceInPlace(list2, delegate(string x) { return x.Length == 3; }, "X");
            InterfaceTests.TestListGeneric(list2, new string[] { "X", "X", "X", "hello", "hello", "sailor", "X", "X", "hi" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });
            Algorithms.ReplaceInPlace(list2, delegate(string x) { return x.Length == 3; }, null);
            InterfaceTests.TestListGeneric(list2, new string[] { null, null, null, "hello", "hello", "sailor", null, null, "hi" });

            list2 = new List<string>(new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });
            Algorithms.ReplaceInPlace(list2, delegate(string x) { x = "zip"; return false; }, null);
            InterfaceTests.TestListGeneric(list2, new string[] { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" });

            string[] array1 = { "foo", "bar", "FOO", "hello", "hello", "sailor", "foo", "bar", "hi" };
            Algorithms.ReplaceInPlace(array1, delegate(string x) { return x.Length == 3; }, "X");
            InterfaceTests.TestEnumerableElements(array1, new string[] { "X", "X", "X", "hello", "hello", "sailor", "X", "X", "hi" });

        }

        [Test]
        public void EqualCollections1()
        {
            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            IEnumerable<string> coll2 = EnumerableFromArray(new string[0]);
            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "hello", "there" });
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "hello", "there", "sailor" });
            IEnumerable<string> coll5 = EnumerableFromArray(new string[] { "Hello", "There", "Sailor" });

            Assert.IsTrue(Algorithms.EqualCollections(coll4, coll4));
            Assert.IsTrue(Algorithms.EqualCollections(coll1, coll2));
            Assert.IsTrue(Algorithms.EqualCollections(coll4, new List<string>(new string[] { "hello", "there", "sailor" })));
            Assert.IsFalse(Algorithms.EqualCollections(coll4, coll5));
            Assert.IsTrue(Algorithms.EqualCollections(coll4, coll5, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.EqualCollections(coll3, coll4));
            Assert.IsFalse(Algorithms.EqualCollections(coll4, coll3));
            Assert.IsFalse(Algorithms.EqualCollections(coll4, coll1));
            Assert.IsFalse(Algorithms.EqualCollections(coll1, coll3));
        }

        [Test]
        public void EqualCollections2()
        {
            BinaryPredicate<int> pred = delegate(int x, int y) { return x <= y; };

            IEnumerable<int> coll1 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll2 = EnumerableFromArray(new int[] { 4, 8, 1 });
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 7, 19, 3 });
            IEnumerable<int> coll4 = EnumerableFromArray(new int[] { 7, 19, 3, 11 });

            Assert.IsTrue(Algorithms.EqualCollections(coll1, coll1, pred));
            Assert.IsTrue(Algorithms.EqualCollections(coll2, coll3, pred));
            Assert.IsFalse(Algorithms.EqualCollections(coll2, coll4, pred));
            Assert.IsFalse(Algorithms.EqualCollections(coll3, coll2, pred));
        }

        [Test]
        public void LexicographicalCompare1()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll2 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 4, 8, 1 });
            IEnumerable<int> coll4 = EnumerableFromArray(new int[] { 4, 8, 3 });
            IEnumerable<int> coll5 = EnumerableFromArray(new int[] { 3, 8, 2 });
            IEnumerable<int> coll6 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });
            IEnumerable<int> coll7 = EnumerableFromArray(new int[] { 4, 8, 0, 9 });
            IEnumerable<int> coll8 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });

            Assert.IsTrue(Algorithms.LexicographicalCompare(coll1, coll2) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll2, coll1) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll3) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll1, coll3) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll1) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll4) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll4, coll3) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll5) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll5, coll3) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll6) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll3) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll7) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll7, coll6) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll8) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll8, coll6) == 0);
        }


        [Test]
        public void GetLexicographicalComparer1()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll2 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 4, 8, 1 });
            IEnumerable<int> coll4 = EnumerableFromArray(new int[] { 4, 8, 3 });
            IEnumerable<int> coll5 = EnumerableFromArray(new int[] { 3, 8, 2 });
            IEnumerable<int> coll6 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });
            IEnumerable<int> coll7 = EnumerableFromArray(new int[] { 4, 8, 0, 9 });
            IEnumerable<int> coll8 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });

            IComparer<IEnumerable<int>> comparer = Algorithms.GetLexicographicalComparer<int>();

            Assert.IsTrue(comparer.Compare(coll1, coll2) == 0);
            Assert.IsTrue(comparer.Compare(coll2, coll1) == 0);
            Assert.IsTrue(comparer.Compare(coll3, coll3) == 0);
            Assert.IsTrue(comparer.Compare(coll1, coll3) < 0);
            Assert.IsTrue(comparer.Compare(coll3, coll1) > 0);
            Assert.IsTrue(comparer.Compare(coll3, coll4) < 0);
            Assert.IsTrue(comparer.Compare(coll4, coll3) > 0);
            Assert.IsTrue(comparer.Compare(coll3, coll5) > 0);
            Assert.IsTrue(comparer.Compare(coll5, coll3) < 0);
            Assert.IsTrue(comparer.Compare(coll3, coll6) < 0);
            Assert.IsTrue(comparer.Compare(coll6, coll3) > 0);
            Assert.IsTrue(comparer.Compare(coll6, coll7) > 0);
            Assert.IsTrue(comparer.Compare(coll7, coll6) < 0);
            Assert.IsTrue(comparer.Compare(coll6, coll8) == 0);
            Assert.IsTrue(comparer.Compare(coll8, coll6) == 0);
        }

        [Test]
        public void LexicographicalCompare2()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll2 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 4, 8, 1 });
            IEnumerable<int> coll4 = EnumerableFromArray(new int[] { 4, 8, 3 });
            IEnumerable<int> coll5 = EnumerableFromArray(new int[] { 5, 8, 2 });
            IEnumerable<int> coll6 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });
            IEnumerable<int> coll7 = EnumerableFromArray(new int[] { 4, 8, 0, 9 });
            IEnumerable<int> coll8 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });

            Assert.IsTrue(Algorithms.LexicographicalCompare(coll1, coll2, ComparersTests.CompareOddEven) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll2, coll1, ComparersTests.CompareOddEven) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll3, ComparersTests.CompareOddEven) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll1, coll3, ComparersTests.CompareOddEven) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll1, ComparersTests.CompareOddEven) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll4, ComparersTests.CompareOddEven) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll4, coll3, ComparersTests.CompareOddEven) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll5, ComparersTests.CompareOddEven) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll5, coll3, ComparersTests.CompareOddEven) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll6, ComparersTests.CompareOddEven) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll3, ComparersTests.CompareOddEven) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll7, ComparersTests.CompareOddEven) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll7, coll6, ComparersTests.CompareOddEven) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll8, ComparersTests.CompareOddEven) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll8, coll6, ComparersTests.CompareOddEven) == 0);
        }

        [Test]
        public void GetLexicographicalComparer2()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll2 = EnumerableFromArray(new int[0]);
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 4, 8, 1 });
            IEnumerable<int> coll4 = EnumerableFromArray(new int[] { 4, 8, 3 });
            IEnumerable<int> coll5 = EnumerableFromArray(new int[] { 5, 8, 2 });
            IEnumerable<int> coll6 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });
            IEnumerable<int> coll7 = EnumerableFromArray(new int[] { 4, 8, 0, 9 });
            IEnumerable<int> coll8 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });

            IComparer<IEnumerable<int>> comparer = Algorithms.GetLexicographicalComparer<int>(ComparersTests.CompareOddEven);

            Assert.IsTrue(comparer.Compare(coll1, coll2) == 0);
            Assert.IsTrue(comparer.Compare(coll2, coll1) == 0);
            Assert.IsTrue(comparer.Compare(coll3, coll3) == 0);
            Assert.IsTrue(comparer.Compare(coll1, coll3) < 0);
            Assert.IsTrue(comparer.Compare(coll3, coll1) > 0);
            Assert.IsTrue(comparer.Compare(coll3, coll4) < 0);
            Assert.IsTrue(comparer.Compare(coll4, coll3) > 0);
            Assert.IsTrue(comparer.Compare(coll3, coll5) > 0);
            Assert.IsTrue(comparer.Compare(coll5, coll3) < 0);
            Assert.IsTrue(comparer.Compare(coll3, coll6) < 0);
            Assert.IsTrue(comparer.Compare(coll6, coll3) > 0);
            Assert.IsTrue(comparer.Compare(coll6, coll7) < 0);
            Assert.IsTrue(comparer.Compare(coll7, coll6) > 0);
            Assert.IsTrue(comparer.Compare(coll6, coll8) == 0);
            Assert.IsTrue(comparer.Compare(coll8, coll6) == 0);
        }

        [Test]
        public void LexicographicalCompare3()
        {
            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            IEnumerable<string> coll2 = EnumerableFromArray(new string[0]);
            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "eric", "foo", "a" });
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "Eric", "Foo", "a"});
            IEnumerable<string> coll5 = EnumerableFromArray(new string[] { "Eric", "B", "c" });
            IEnumerable<string> coll6 = EnumerableFromArray(new string[] { "Eric", "FOO", "a", "b" });
            IEnumerable<string> coll7 = EnumerableFromArray(new string[] { "Snap", "eric" });

            Assert.IsTrue(Algorithms.LexicographicalCompare(coll1, coll2, StringComparer.InvariantCultureIgnoreCase) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll2, coll1, StringComparer.InvariantCultureIgnoreCase) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll3, StringComparer.InvariantCultureIgnoreCase) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll1, coll3, StringComparer.InvariantCultureIgnoreCase) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll1, StringComparer.InvariantCultureIgnoreCase) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll4, coll3, StringComparer.InvariantCultureIgnoreCase) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll4, StringComparer.InvariantCultureIgnoreCase) == 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll5, StringComparer.InvariantCultureIgnoreCase) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll5, coll3, StringComparer.InvariantCultureIgnoreCase) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll6, StringComparer.InvariantCultureIgnoreCase) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll6, coll3, StringComparer.InvariantCultureIgnoreCase) > 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll3, coll7, StringComparer.InvariantCultureIgnoreCase) < 0);
            Assert.IsTrue(Algorithms.LexicographicalCompare(coll7, coll3, StringComparer.InvariantCultureIgnoreCase) > 0);
        }


        [Test]
        public void GetLexicographicalComparer3()
        {
            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            IEnumerable<string> coll2 = EnumerableFromArray(new string[0]);
            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "eric", "foo", "a" });
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "Eric", "Foo", "a" });
            IEnumerable<string> coll5 = EnumerableFromArray(new string[] { "Eric", "B", "c" });
            IEnumerable<string> coll6 = EnumerableFromArray(new string[] { "Eric", "FOO", "a", "b" });
            IEnumerable<string> coll7 = EnumerableFromArray(new string[] { "Snap", "eric" });

            IComparer<IEnumerable<string>> comparer = Algorithms.GetLexicographicalComparer(StringComparer.InvariantCultureIgnoreCase);

            Assert.IsTrue(comparer.Compare(coll1, coll2) == 0);
            Assert.IsTrue(comparer.Compare(coll2, coll1) == 0);
            Assert.IsTrue(comparer.Compare(coll3, coll3) == 0);
            Assert.IsTrue(comparer.Compare(coll1, coll3) < 0);
            Assert.IsTrue(comparer.Compare(coll3, coll1) > 0);
            Assert.IsTrue(comparer.Compare(coll4, coll3) == 0);
            Assert.IsTrue(comparer.Compare(coll3, coll4) == 0);
            Assert.IsTrue(comparer.Compare(coll3, coll5) > 0);
            Assert.IsTrue(comparer.Compare(coll5, coll3) < 0);
            Assert.IsTrue(comparer.Compare(coll3, coll6) < 0);
            Assert.IsTrue(comparer.Compare(coll6, coll3) > 0);
            Assert.IsTrue(comparer.Compare(coll3, coll7) < 0);
            Assert.IsTrue(comparer.Compare(coll7, coll3) > 0);
        }
        /*
                [Test]
                public void LexicographicsEqualityComparer1()
                {
                    IEnumerable<int> coll1 = EnumerableFromArray(new int[0]);
                    IEnumerable<int> coll2 = EnumerableFromArray(new int[0]);
                    IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 4, 8, 1 });
                    IEnumerable<int> coll4 = EnumerableFromArray(new int[] { 4, 8, 3 });
                    IEnumerable<int> coll5 = EnumerableFromArray(new int[] { 3, 8, 2 });
                    IEnumerable<int> coll6 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });
                    IEnumerable<int> coll7 = EnumerableFromArray(new int[] { 4, 8, 0, 9 });
                    IEnumerable<int> coll8 = EnumerableFromArray(new int[] { 4, 8, 1, 9 });

                    IEqualityComparer<IEnumerable<int>> comparer = Algorithms.LexicographicalEqualityComparer<int>();

                    Assert.IsTrue(comparer.Equals(coll6, coll8));
                    Assert.IsTrue(comparer.Equals(coll2, coll1));
                    Assert.IsTrue(comparer.Equals(coll3, coll3));
                    Assert.IsFalse(comparer.Equals(coll6, coll7));
                    Assert.IsFalse(comparer.Equals(coll3, coll4));

                    Assert.IsTrue(comparer.GetHashCode(coll6) == comparer.GetHashCode(coll8));
                    Assert.IsTrue(comparer.GetHashCode(coll2) == comparer.GetHashCode(coll1));
                    Assert.IsTrue(comparer.GetHashCode(coll3) == comparer.GetHashCode(coll3));
                    Assert.IsFalse(comparer.GetHashCode(coll6) == comparer.GetHashCode(coll7));
                    Assert.IsFalse(comparer.GetHashCode(coll3) == comparer.GetHashCode(coll4));
                }
         * 
                [Test]
                public void LexicographicalEqualityComparer3()
                {
                    IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
                    IEnumerable<string> coll2 = EnumerableFromArray(new string[0]);
                    IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "eric", "foo", "a" });
                    IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "Eric", "Foo", "a" });
                    IEnumerable<string> coll5 = EnumerableFromArray(new string[] { "Eric", "B", "c" });
                    IEnumerable<string> coll6 = EnumerableFromArray(new string[] { "Eric", "FOO", "a", "b" });
                    IEnumerable<string> coll7 = EnumerableFromArray(new string[] { "Snap", "eric" });

                    IEqualityComparer<IEnumerable<string>> comparer = Algorithms.LexicographicalEqualityComparer(StringComparer.InvariantCultureIgnoreCase);

                    Assert.IsTrue(comparer.Equals(coll3, coll4) );
                    Assert.IsTrue(comparer.Equals(coll1, coll2) );
                    Assert.IsFalse(comparer.Equals(coll3, coll5) );
                    Assert.IsFalse(comparer.Equals(coll4, coll6) );

                    Assert.IsTrue(comparer.GetHashCode(coll3) == comparer.GetHashCode(coll4));
                    Assert.IsTrue(comparer.GetHashCode(coll1) == comparer.GetHashCode(coll2));
                    Assert.IsFalse(comparer.GetHashCode(coll3) == comparer.GetHashCode(coll5));
                    Assert.IsFalse(comparer.GetHashCode(coll4) == comparer.GetHashCode(coll6));
                }
        */
        [Test]
        public void Maximum()
        {
            // Empty collection.
            string maxS;
            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);

            try {
                maxS = Algorithms.Maximum(coll1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                maxS = Algorithms.Maximum(coll1, StringComparer.CurrentCulture);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                maxS = Algorithms.Maximum(coll1, delegate(string s1, string s2) { return string.CompareOrdinal(s1, s2);});
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            // float max
            double maxD;
            IEnumerable<double> coll2 = EnumerableFromArray(new double[] { 7.6, -1.2, 19.2, 0, 178.3, -5.4, -17.8});
            maxD = Algorithms.Maximum(coll2);
            Assert.AreEqual(maxD, 178.3);

            // int max
            int maxI;
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] {7, 4, 1, -8, 19, 8, 43, 38, 1, 38, 4});
            maxI = Algorithms.Maximum(coll3, ComparersTests.CompareOddEven);
            Assert.AreEqual(maxI, 38);
            maxI = Algorithms.Maximum(coll3, new GOddEvenComparer());
            Assert.AreEqual(maxI, 38);

            // string max
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "foo", "Hello", "a" });
            maxS = Algorithms.Maximum(coll4);
            Assert.AreEqual("Hello", maxS);
            maxS = Algorithms.Maximum(coll4, StringComparer.Ordinal);
            Assert.AreEqual("foo", maxS);

            // one elements
            IEnumerable<string> coll5 = EnumerableFromArray(new string[] {"elvis"});
            maxS = Algorithms.Maximum(coll5);
            Assert.AreEqual("elvis", maxS);
        }


        [Test]
        public void IndexOfMaximum()
        {
            // Empty collection.
            int maxIndex;
            IList<string> coll1 = new List<string>(new string[0]);

            // empty collection
            maxIndex = Algorithms.IndexOfMaximum(coll1);
            Assert.AreEqual(-1, maxIndex);
            maxIndex = Algorithms.IndexOfMaximum(coll1, StringComparer.CurrentCulture);
            Assert.AreEqual(-1, maxIndex);
            maxIndex = Algorithms.IndexOfMaximum(coll1, delegate(string s1, string s2) { return string.CompareOrdinal(s1, s2); });
            Assert.AreEqual(-1, maxIndex);

            // float max
            IList<double> coll2 = new Deque<double>(new double[] { 7.6, -1.2, 19.2, 0, 178.3, -5.4, -17.8 });
            maxIndex = Algorithms.IndexOfMaximum(coll2);
            Assert.AreEqual(maxIndex, 4);

            // int max
            IList<int> coll3 = new BigList<int>(new int[] { 7, 4, 1, -8, 19, 8, 43, 38, 1, 38, 4 });
            maxIndex = Algorithms.IndexOfMaximum(coll3, ComparersTests.CompareOddEven);
            Assert.AreEqual(maxIndex, 7);
            maxIndex = Algorithms.IndexOfMaximum(coll3, new GOddEvenComparer());
            Assert.AreEqual(maxIndex, 7);

            // string max
            IList<string> coll4 = new string[] { "foo", "Hello", "a" };
            maxIndex = Algorithms.IndexOfMaximum(coll4);
            Assert.AreEqual(maxIndex, 1);
            maxIndex = Algorithms.IndexOfMaximum(coll4, StringComparer.Ordinal);
            Assert.AreEqual(maxIndex, 0);

            // one elements
            IList<string> coll5 = new BigList<string>(new string[] { "elvis" });
            maxIndex = Algorithms.IndexOfMaximum(coll5);
            Assert.AreEqual(maxIndex, 0);
        }

        [Test]
        public void Minimum()
        {
            // Empty collection.
            string minS;
            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);

            try {
                minS = Algorithms.Minimum(coll1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                minS = Algorithms.Minimum(coll1, StringComparer.CurrentCulture);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                minS = Algorithms.Minimum(coll1, delegate(string s1, string s2) { return string.CompareOrdinal(s1, s2); });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            // float min
            double minD;
            IEnumerable<double> coll2 = EnumerableFromArray(new double[] { 7.6, -1.2, 19.2, 0, 178.3, -5.4, -17.8 });
            minD = Algorithms.Minimum(coll2);
            Assert.AreEqual(minD, -17.8);

            // int min
            int minI;
            IEnumerable<int> coll3 = EnumerableFromArray(new int[] { 7, 4, 1, -8, 19, 8, 43, 38, 1, 38, 4 });
            minI = Algorithms.Minimum(coll3, ComparersTests.CompareOddEven);
            Assert.AreEqual(minI, 1);
            minI = Algorithms.Minimum(coll3, new GOddEvenComparer());
            Assert.AreEqual(minI, 1);

            // string min
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "foo", "Hello", "a", null });
            minS = Algorithms.Minimum(coll4);
            Assert.AreEqual(null, minS);
            minS = Algorithms.Minimum(coll4, StringComparer.Ordinal);
            Assert.AreEqual(null, minS);

            // string min
            IEnumerable<string> coll5 = EnumerableFromArray(new string[] { "foo", "Hello", "a", "zip" });
            minS = Algorithms.Minimum(coll5);
            Assert.AreEqual("a", minS);
            minS = Algorithms.Minimum(coll5, StringComparer.Ordinal);
            Assert.AreEqual("Hello", minS);

            // one elements
            IEnumerable<string> coll6 = EnumerableFromArray(new string[] { null });
            minS = Algorithms.Minimum(coll6);
            Assert.AreEqual(null, minS);
        }

        [Test]
        public void IndexOfMinimum()
        {
            // Empty collection.
            int minIndex;
            IList<string> coll1 = new List<string>(new string[0]);

            // empty collection
            minIndex = Algorithms.IndexOfMinimum(coll1);
            Assert.AreEqual(-1, minIndex);
            minIndex = Algorithms.IndexOfMinimum(coll1, StringComparer.CurrentCulture);
            Assert.AreEqual(-1, minIndex);
            minIndex = Algorithms.IndexOfMinimum(coll1, delegate(string s1, string s2) { return string.CompareOrdinal(s1, s2); });
            Assert.AreEqual(-1, minIndex);

            // float max
            IList<double> coll2 = new Deque<double>(new double[] { 7.6, -1.2, 19.2, 0, 178.3, -5.4, -17.8 });
            minIndex = Algorithms.IndexOfMinimum(coll2);
            Assert.AreEqual(minIndex, 6);

            // int max
            IList<int> coll3 = new BigList<int>(new int[] { 7, 4, 1, -8, 19, 8, 43, 38, 1, 38, 4 });
            minIndex = Algorithms.IndexOfMinimum(coll3, ComparersTests.CompareOddEven);
            Assert.AreEqual(minIndex, 2);
            minIndex = Algorithms.IndexOfMinimum(coll3, new GOddEvenComparer());
            Assert.AreEqual(minIndex, 2);

            // string max
            IList<string> coll4 = new string[] { "foo", "Hello", "a" };
            minIndex = Algorithms.IndexOfMinimum(coll4);
            Assert.AreEqual(minIndex, 2);
            minIndex = Algorithms.IndexOfMinimum(coll4, StringComparer.Ordinal);
            Assert.AreEqual(minIndex, 1);

            // one elements
            IList<string> coll5 = new BigList<string>(new string[] { "elvis" });
            minIndex = Algorithms.IndexOfMinimum(coll5);
            Assert.AreEqual(minIndex, 0);
        }

        [Test]
        public void Fill()
        {
            BigList<string> coll1 = new BigList<string>(new string[4] { "foo", null, "baz", "elvis" });
            Algorithms.Fill(coll1, "xyzzy");
            InterfaceTests.TestListGeneric(coll1, new string[] { "xyzzy", "xyzzy", "xyzzy", "xyzzy" });

            List<string> coll2 = new List<string>();
            Algorithms.Fill(coll2, "xyzzy");
            InterfaceTests.TestListGeneric(coll2, new string[] { });

            IList<string> coll3 = new List<string>(new string[] { "hi", "there" }).AsReadOnly();
            try {
                Algorithms.Fill(coll3, "xyzzy");
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            BigList<int> coll4 = new BigList<int>(new int[] { 2 }, 10000);
            Algorithms.Fill(coll4, 42);
            Assert.AreEqual(coll4.Count, 10000);
            foreach (int x in coll4)
                Assert.AreEqual(42, x);
        }

        [Test]
        public void FillArray()
        {
            string[] coll1 = { "foo", null, "baz", "elvis" };
            Algorithms.Fill(coll1, "xyzzy");
            InterfaceTests.TestEnumerableElements(coll1, new string[] { "xyzzy", "xyzzy", "xyzzy", "xyzzy" });

            string[] coll2 = { };
            Algorithms.Fill(coll2, "xyzzy");
            InterfaceTests.TestEnumerableElements(coll2, new string[] { });
        }

        [Test]
        public void FillRange()
        {
            BigList<string> coll1 = new BigList<string>(new string[4] { "foo", null, "baz", "elvis" });
            Algorithms.FillRange(coll1, 1, 2, "xyzzy");
            InterfaceTests.TestListGeneric(coll1, new string[] { "foo", "xyzzy", "xyzzy", "elvis" });
            Algorithms.FillRange(coll1, 0, 4, "ben");
            InterfaceTests.TestListGeneric(coll1, new string[] { "ben", "ben", "ben", "ben" });
            Algorithms.FillRange(coll1, 1, 0, "foo");
            InterfaceTests.TestListGeneric(coll1, new string[] { "ben", "ben", "ben", "ben" });
            Algorithms.FillRange(coll1, 0, 2, null);
            InterfaceTests.TestListGeneric(coll1, new string[] { null, null, "ben", "ben" });

            List<string> coll2 = new List<string>();
            Algorithms.FillRange(coll2, 0, 0, "xyzzy");
            InterfaceTests.TestListGeneric(coll2, new string[] { });

            IList<string> coll3 = new List<string>(new string[] { "hi", "there" }).AsReadOnly();
            try {
                Algorithms.FillRange(coll3, 1, 1, "xyzzy");
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            BigList<int> coll4 = new BigList<int>(new int[] { 2 }, 10000);
            Algorithms.FillRange(coll4, 1, 9998, 42);
            Assert.AreEqual(coll4.Count, 10000);
            int i = 0;
            foreach (int x in coll4) {
                if (i == 0 || i == 9999)
                    Assert.AreEqual(2, x);
                else
                    Assert.AreEqual(42, x);

                ++i;
            }

            try {
                Algorithms.FillRange(coll4, -1, 2, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                Algorithms.FillRange(coll4,3, -2, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                Algorithms.FillRange(coll4, 9999, 2, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void FillArrayRange()
        {
            string[] coll1 = { "foo", null, "baz", "elvis" };
            Algorithms.FillRange(coll1, 1, 2, "xyzzy");
            InterfaceTests.TestEnumerableElements(coll1, new string[] { "foo", "xyzzy", "xyzzy", "elvis" });
            Algorithms.FillRange(coll1, 0, 4, "ben");
            InterfaceTests.TestEnumerableElements(coll1, new string[] { "ben", "ben", "ben", "ben" });
            Algorithms.FillRange(coll1, 1, 0, "foo");
            InterfaceTests.TestEnumerableElements(coll1, new string[] { "ben", "ben", "ben", "ben" });
            Algorithms.FillRange(coll1, 0, 2, null);
            InterfaceTests.TestEnumerableElements(coll1, new string[] { null, null, "ben", "ben" });

            string[] coll2 = { };
            Algorithms.FillRange(coll2, 0, 0, "xyzzy");
            InterfaceTests.TestEnumerableElements(coll2, new string[] { });

            int[] coll4 = new int[10000];

            try {
                Algorithms.FillRange(coll4, -1, 2, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                Algorithms.FillRange(coll4, 3, -2, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                Algorithms.FillRange(coll4, 9999, 2, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void RemoveDuplicates()
        {
            IEnumerable<string> coll1 = EnumerableFromArray(new string[] { "foo", "foo", "foo", "big", "foo", "super", "super", null, null, null, "hello", "there", "sailor", "sailor" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll1), new string[] { "foo", "big", "foo", "super", null, "hello", "there", "sailor" });

            IEnumerable<string> coll2 = EnumerableFromArray(new string[] {  });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll2), new string[] {  });

            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "foo"});
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll3), new string[] { "foo" });

            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "foo", "foo" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll4), new string[] { "foo" });

            IEnumerable<string> coll5 = EnumerableFromArray(new string[] { "foo", "bar", "bar" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll5), new string[] { "foo", "bar" });

            IEnumerable<string> coll6 = EnumerableFromArray(new string[] { "foo", "foo", "bar" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll6), new string[] { "foo", "bar" });

            IEnumerable<string> coll11 = EnumerableFromArray(new string[] { "foo", "Foo", "FOO", "big", "foo", "SUPER", "super", null, null, null, "hello", "there", "sailor", "sailor" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll11, StringComparer.InvariantCultureIgnoreCase), new string[] { "foo", "big", "foo", "SUPER", null, "hello", "there", "sailor" });

            IEnumerable<string> coll12 = EnumerableFromArray(new string[] { });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll12, StringComparer.InvariantCultureIgnoreCase), new string[] { });

            IEnumerable<string> coll13 = EnumerableFromArray(new string[] { "foo" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll13, StringComparer.InvariantCultureIgnoreCase), new string[] { "foo" });

            IEnumerable<string> coll14 = EnumerableFromArray(new string[] { "Foo", "foo" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll14, StringComparer.InvariantCultureIgnoreCase), new string[] { "Foo" });

            IEnumerable<string> coll15 = EnumerableFromArray(new string[] { "foo", "bAr", "bar" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll15, StringComparer.InvariantCultureIgnoreCase), new string[] { "foo", "bAr" });

            IEnumerable<string> coll16 = EnumerableFromArray(new string[] { "foo", "fOo", "bar" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll16, StringComparer.InvariantCultureIgnoreCase), new string[] { "foo", "bar" });

            BinaryPredicate<string> pred = delegate(string x, string y) {
                return x[0] == y[0];
            };

            IEnumerable<string> coll17 = EnumerableFromArray(new string[] { "fiddle", "faddle", "bar", "bing", "deli", "zippy", "zack", "zorch" });
            InterfaceTests.TestEnumerableElements(Algorithms.RemoveDuplicates(coll17, pred), new string[] { "fiddle", "bar" , "deli", "zippy"});
        }

        [Test]
        public void RemoveDuplicatesInPlace()
        {
            IList<string> list1 = new List<string>(new string[] { "foo", "foo", "foo", "big", "foo", "super", "super", null, null, null, "hello", "there", "sailor", "sailor" });
            Algorithms.RemoveDuplicatesInPlace(list1);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "big", "foo", "super", null, "hello", "there", "sailor" });

            IList<string> list2 = new List<string>(new string[] { });
            Algorithms.RemoveDuplicatesInPlace(list2);
            InterfaceTests.TestListGeneric(list2, new string[] { });

            IList<string> list3 = new List<string>(new string[] { "foo" });
            Algorithms.RemoveDuplicatesInPlace(list3);
            InterfaceTests.TestListGeneric(list3, new string[] { "foo" });

            IList<string> list4 = new List<string>(new string[] { "foo", "foo" });
            Algorithms.RemoveDuplicatesInPlace(list4);
            InterfaceTests.TestListGeneric(list4, new string[] { "foo" });

            IList<string> list5 = new List<string>(new string[] { "foo", "bar", "bar" });
            Algorithms.RemoveDuplicatesInPlace(list5);
            InterfaceTests.TestListGeneric(list5, new string[] { "foo", "bar" });

            IList<string> list6 = new List<string>(new string[] { "foo", "foo", "bar" });
            Algorithms.RemoveDuplicatesInPlace(list6);
            InterfaceTests.TestListGeneric(list6, new string[] { "foo", "bar" });

            IList<string> list11 = new List<string>(new string[] { "foo", "Foo", "FOO", "big", "foo", "SUPER", "super", null, null, null, "hello", "there", "sailor", "sailor" });
            Algorithms.RemoveDuplicatesInPlace(list11, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list11, new string[] { "foo", "big", "foo", "SUPER", null, "hello", "there", "sailor" });

            IList<string> list12 = new List<string>(new string[] { });
            Algorithms.RemoveDuplicatesInPlace(list12, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list12, new string[] { });

            IList<string> list13 = new List<string>(new string[] { "foo" });
            Algorithms.RemoveDuplicatesInPlace(list13, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list13, new string[] { "foo" });

            IList<string> list14 = new List<string>(new string[] { "Foo", "foo" });
            Algorithms.RemoveDuplicatesInPlace(list14, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list14, new string[] { "Foo" });

            IList<string> list15 = new List<string>(new string[] { "foo", "bAr", "bar" });
            Algorithms.RemoveDuplicatesInPlace(list15, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list15, new string[] { "foo", "bAr" });

            IList<string> list16 = new List<string>(new string[] { "foo", "fOo", "bar" });
            Algorithms.RemoveDuplicatesInPlace(list16, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestListGeneric(list16, new string[] { "foo", "bar" });

            IList<string> list21 = Algorithms.ReadWriteList<string>(new string[] { "foo", "foo", "foo", "big", "foo", "super", "super", null, null, null, "hello", "there", "sailor", "sailor" });
            Algorithms.RemoveDuplicatesInPlace(list21);
            InterfaceTests.TestEnumerableElements(list21, new string[] { "foo", "big", "foo", "super", null, "hello", "there", "sailor", null, null, null, null, null, null});

            string[] array1 = { "foo", "foo", "foo", "big", "foo", "super", "super", null, null, null, "hello", "there", "sailor", "sailor" };
            Algorithms.RemoveDuplicatesInPlace(array1);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "big", "foo", "super", null, "hello", "there", "sailor", null, null, null, null, null, null });

            array1 = new string[] { "foo", "Foo", "FOO", "big", "foo", "SUPER", "super", null, null, null, "hello", "there", "sailor", "sailor" };
            Algorithms.RemoveDuplicatesInPlace(array1, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "big", "foo", "SUPER", null, "hello", "there", "sailor", null, null, null, null, null, null });

            BinaryPredicate<string> pred = delegate(string x, string y) {
                return x[0] == y[0];
            };

            IList<string> list30 = new List<string>(new string[] { "fiddle", "faddle", "bar", "bing", "deli", "zippy", "zack", "zorch" });
            Algorithms.RemoveDuplicatesInPlace(list30, pred);
            InterfaceTests.TestEnumerableElements(list30, new string[] { "fiddle", "bar", "deli", "zippy" });
        }

        [Test]
        public void FirstConsecutiveEqual()
        {
            int index;

            IList<string> list1 = new List<string>();
            index = Algorithms.FirstConsecutiveEqual(list1, 1);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveEqual(list1, 2);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveEqual(list1, 3);
            Assert.AreEqual(-1, index);

            IList<string> list2 = new List<string>(new string[] { "hello", "hello", "hello", "sailor", "bar", "bar", "bar", "bar" });
            index = Algorithms.FirstConsecutiveEqual(list2, 1);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list2, 2);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list2, 3);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list2, 4);
            Assert.AreEqual(4, index);
            index = Algorithms.FirstConsecutiveEqual(list2, 5);
            Assert.AreEqual(-1, index);

            IList<string> list3 = new List<string>(new string[] { "bar", "hello", "hello", "sailor", "bar", "bar", "bar", "bar", "foo", "foo", "foo" });
            index = Algorithms.FirstConsecutiveEqual(list3, 1);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list3, 2);
            Assert.AreEqual(1, index);
            index = Algorithms.FirstConsecutiveEqual(list3, 3);
            Assert.AreEqual(4, index);
            index = Algorithms.FirstConsecutiveEqual(list3, 4);
            Assert.AreEqual(4, index);
            index = Algorithms.FirstConsecutiveEqual(list3, 5);
            Assert.AreEqual(-1, index);

            IList<string> list4 = new List<string>(new string[] { "bar", "hELlo", "hello", "sailor", "Bar", "bar", "BAR", "bar", "foo", "FOO", "foo" });
            index = Algorithms.FirstConsecutiveEqual(list4, 1, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list4, 2, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(1, index);
            index = Algorithms.FirstConsecutiveEqual(list4, 3, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(4, index);
            index = Algorithms.FirstConsecutiveEqual(list4, 4, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(4, index);
            index = Algorithms.FirstConsecutiveEqual(list4, 5, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(-1, index);

            IList<string> list5 = new List<string>(new string[] { "bar", "BAR", "bar", "Bar" });
            index = Algorithms.FirstConsecutiveEqual(list5, 1, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list5, 2, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list5, 3, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list5, 4, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list5, 5, StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(-1, index);

            BinaryPredicate<string> pred = delegate(string x, string y) {
                return x[0] == y[0];
            };

            IList<string> list6  = new List<string>(new string[] { "bar", "banana", "fuji", "fiddle", "faddle", "lemon" });
            index = Algorithms.FirstConsecutiveEqual(list6, 2, pred);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveEqual(list6, 3, pred);
            Assert.AreEqual(2, index);
            index = Algorithms.FirstConsecutiveEqual(list6, 4, pred);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void FirstConsecutiveWhere()
        {
            int index;

            Predicate<int> isOdd = delegate(int x) { return (x & 1) == 1; };
            Predicate<int> isEven = delegate(int x) { return (x & 1) == 0; };

            IList<int> list1 = new List<int>();
            index = Algorithms.FirstConsecutiveWhere(list1, 1, isOdd);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveWhere(list1, 2, isOdd);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveWhere(list1, 3, isOdd);
            Assert.AreEqual(-1, index);

            IList<int> list2 = new List<int>(new int[] { 3, 7, 5, 1 });
            index = Algorithms.FirstConsecutiveWhere(list2, 1, isOdd);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveWhere(list2, 2, isOdd);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveWhere(list2, 3, isOdd);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveWhere(list2, 4, isOdd);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveWhere(list2, 5, isOdd);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveWhere(list2, 1, isEven);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveWhere(list2, 2, isEven);
            Assert.AreEqual(-1, index);

            //                                                                  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
            IList<int> list3 = new List<int>(new int[] { 3, 0, 6, 2, 1, 9, 4, 1, 8, 6, 8, 12, 4,  3,  5,  3,  1,  5,  7 });
            index = Algorithms.FirstConsecutiveWhere(list3, 1, isOdd);
            Assert.AreEqual(0, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 2, isOdd);
            Assert.AreEqual(4, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 3, isOdd);
            Assert.AreEqual(13, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 4, isOdd);
            Assert.AreEqual(13, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 5, isOdd);
            Assert.AreEqual(13, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 6, isOdd);
            Assert.AreEqual(13, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 7, isOdd);
            Assert.AreEqual(-1, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 1, isEven);
            Assert.AreEqual(1, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 2, isEven);
            Assert.AreEqual(1, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 3, isEven);
            Assert.AreEqual(1, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 4, isEven);
            Assert.AreEqual(8, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 5, isEven);
            Assert.AreEqual(8, index);
            index = Algorithms.FirstConsecutiveWhere(list3, 6, isEven);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void Copy1()
        {
            IList<string> list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            IEnumerable<string> enum1 = EnumerableFromArray(new string[] { "hello", "Sailor" });
            Algorithms.Copy(enum1, list1, 3);
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum2 = EnumerableFromArray(new string[0]);
            Algorithms.Copy(enum2, list1, 1);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum3 = EnumerableFromArray(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(enum3, list1, 4);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "a1", "a2", "a3", "a4" });

            IEnumerable<string> enum4 = EnumerableFromArray(new string[] { "b1", "b2", "b3", "b4" });
            Algorithms.Copy(enum4, list1, 8);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "a1", "a2", "a3", "a4", "b1", "b2", "b3", "b4" });

            Algorithms.Copy(enum1, list1, 0);
            InterfaceTests.TestListGeneric(list1, new string[] { "hello", "Sailor", "baz", "hello", "a1", "a2", "a3", "a4", "b1", "b2", "b3", "b4" });

            IList<string> list2 = new List<string>();
            Algorithms.Copy(enum1, list2, 0);
            InterfaceTests.TestListGeneric(list2, new string[] { "hello", "Sailor"});
        }

        [Test]
        public void CopyArray1()
        {
            string[] array1 = { "foo", "bar", "baz", "smell", "the", "glove" };
            IEnumerable<string> enum1 = EnumerableFromArray(new string[] { "hello", "Sailor" });
            Algorithms.Copy(enum1, array1, 3);
            InterfaceTests.TestEnumerableElements<string>(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum2 = EnumerableFromArray(new string[0]);
            Algorithms.Copy(enum2, array1, 1);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum3 = EnumerableFromArray(new string[] { "a1", "a2", "a3", "a4" });
            try {
                Algorithms.Copy(enum3, array1, 4);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "baz", "hello", "a1", "a2"});

            IEnumerable<string> enum4 = EnumerableFromArray(new string[] { "b1", "b2", "b3", "b4" });
            Algorithms.Copy(enum4, array1, 2);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "b1", "b2", "b3", "b4" });

            Algorithms.Copy(enum1, array1, 0);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "hello", "Sailor", "b1", "b2", "b3", "b4" });

            string[] array2 = {};
            Algorithms.Copy(EnumerableFromArray(new string[0]), array2, 0);
            InterfaceTests.TestEnumerableElements(array2, new string[] { });
        }

        [Test]
        public void Copy2()
        {
            IList<string> list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            IEnumerable<string> enum1 = EnumerableFromArray(new string[] { "hello", "Sailor" });
            Algorithms.Copy(enum1, list1, 3, 2);
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum2 = EnumerableFromArray(new string[0]);
            Algorithms.Copy(enum2, list1, 1, 5);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            Algorithms.Copy(enum1, list1, 2, 0);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum3 = EnumerableFromArray(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(enum3, list1, 4, 1);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "a1", "glove" });

            IEnumerable<string> enum4 = EnumerableFromArray(new string[] { "b1", "b2", "b3", "b4" });
            Algorithms.Copy(enum4, list1, 5, 3);
            InterfaceTests.TestListGeneric(list1, new string[] { "foo", "bar", "baz", "hello", "a1", "b1", "b2", "b3" });

            Algorithms.Copy(enum1, list1, 0, 1);
            InterfaceTests.TestListGeneric(list1, new string[] { "hello", "bar", "baz", "hello", "a1", "b1", "b2", "b3" });

            IList<string> list2 = new List<string>();
            Algorithms.Copy(enum1, list2, 0, 1);
            InterfaceTests.TestListGeneric(list2, new string[] { "hello" });
        }

        [Test]
        public void CopyArray2()
        {
            string[] array1 = { "foo", "bar", "baz", "smell", "the", "glove" };
            IEnumerable<string> enum1 = EnumerableFromArray(new string[] { "hello", "Sailor" });
            Algorithms.Copy(enum1, array1, 3, 2);
            InterfaceTests.TestEnumerableElements<string>(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum2 = EnumerableFromArray(new string[0]);
            Algorithms.Copy(enum2, array1, 1, 5);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            Algorithms.Copy(enum1, array1, 2, 0);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            IEnumerable<string> enum3 = EnumerableFromArray(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(enum3, array1, 4, 1);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "baz", "hello", "a1", "glove" });

            IEnumerable<string> enum4 = EnumerableFromArray(new string[] { "b1", "b2", "b3", "b4" });
            try {
                Algorithms.Copy(enum4, array1, 5, 3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            InterfaceTests.TestEnumerableElements(array1, new string[] { "foo", "bar", "baz", "hello", "a1", "glove" });

            Algorithms.Copy(enum1, array1, 0, 1);
            InterfaceTests.TestEnumerableElements(array1, new string[] { "hello", "bar", "baz", "hello", "a1", "glove" });

            string[] array2 = { };
            Algorithms.Copy(enum1, array2, 0, 0);
            InterfaceTests.TestEnumerableElements(array2, new string[] {  });
        }

        [Test]
        public void Copy3()
        {
            IList<string> list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            IList<string> list2 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(list1, 1, list2, 2, 4);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "a1", "a2", "bar", "baz", "smell", "the" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.Copy(list1, 1, list1, 2, 5);
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "foo", "bar", "bar", "baz", "smell", "the", "glove" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.Copy(list1, 0, list1, 2, 3);
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "foo", "bar", "foo", "bar", "baz", "glove" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.Copy(list1, 1, list1, 0, 5);
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "bar", "baz", "smell", "the", "glove", "glove" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.Copy(list1, 1, list1, 2, 3);
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "foo", "bar", "bar", "baz", "smell", "glove" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            list2 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(list1, 1, list2, 4, 7);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "a1", "a2", "a3", "a4", "bar", "baz", "smell", "the", "glove" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            list2 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(list1, 3, list2, 0, 7);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "smell", "the", "glove", "a4" });

            list1 = new List<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            list2 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            Algorithms.Copy(list1, 3, list2, 0, 1);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "smell", "a2", "a3", "a4" });
        }

        [Test]
        public void CopyArray3()
        {
            string[] array1 = { "foo", "bar", "baz", "smell", "the", "glove" };
            string[] array2 = { "a1", "a2", "a3", "a4" };
            Algorithms.Copy(array1, 1, array2, 2, 2);
            InterfaceTests.TestListGeneric<string>(array2, new string[] { "a1", "a2", "bar", "baz" });

            array1 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(array1, 1, array1, 2, 3);
            InterfaceTests.TestListGeneric<string>(array1, new string[] { "foo", "bar", "bar", "baz", "smell",  "glove" });

            array1 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(array1, 0, array1, 2, 3);
            InterfaceTests.TestListGeneric<string>(array1, new string[] { "foo", "bar", "foo", "bar", "baz", "glove" });

            array1 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(array1, 1, array1, 0, 5);
            InterfaceTests.TestListGeneric<string>(array1, new string[] { "bar", "baz", "smell", "the", "glove", "glove" });

            array1 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(array1, 1, array1, 2, 3);
            InterfaceTests.TestListGeneric<string>(array1, new string[] { "foo", "bar", "bar", "baz", "smell", "glove" });

            array1 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            array2 = new string[] { "a1", "a2", "a3", "a4" };
            Algorithms.Copy(array1, 3, array2, 0, 4);
            InterfaceTests.TestListGeneric<string>(array2, new string[] { "smell", "the", "glove", "a4" });

            array1 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            array2 = new string[] { "a1", "a2", "a3", "a4" };
            Algorithms.Copy(array1, 3, array2, 0, 1);
            InterfaceTests.TestListGeneric<string>(array2, new string[] { "smell", "a2", "a3", "a4" });
        }

        [Test]
        public void CopyArray4()
        {
            List<string> list1 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            string[] array2 = { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(list1, 1, array2, 2, 2);
            InterfaceTests.TestListGeneric<string>(array2, new string[] { "foo", "bar", "a2", "a3", "the", "glove" });

            list1 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            array2 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(list1, 1, array2, 2, 4);
            InterfaceTests.TestListGeneric<string>(array2, new string[] { "foo", "bar", "a2", "a3", "a4", "glove" });

            list1 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            array2 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.Copy(list1, 1, array2, 4, 2);
            InterfaceTests.TestListGeneric<string>(array2, new string[] { "foo", "bar", "baz", "smell", "a2", "a3" });

            list1 = new List<string>(new string[] { "a1", "a2", "a3", "a4" });
            array2 = new string[] { "foo", "bar", "baz", "smell", "the", "glove" };
            try {
                Algorithms.Copy(list1, 1, array2, 4, 3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void Reverse1()
        {
            IList<string> list1 = new List<string>();
            InterfaceTests.TestEnumerableElements(Algorithms.Reverse(list1), new string[0]);

            IList<string> list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Reverse(list2), new string[] { "glove", "the", "smell", "baz", "bar", "foo" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });

            IList<string> list3 = new BigList<string>(new string[] { "foo", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Reverse(list3), new string[] { "glove", "the", "smell", "baz", "foo" });
            InterfaceTests.TestListGeneric<string>(list3, new string[] { "foo", "baz", "smell", "the", "glove" });
        }

        [Test]
        public void Reverse2()
        {
            IList<string> list1 = new List<string>();
            Algorithms.ReverseInPlace(list1);
            InterfaceTests.TestEnumerableElements(list1, new string[0]);

            IList<string> list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.ReverseInPlace(list2);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "glove", "the", "smell", "baz", "bar", "foo" });

            IList<string> list3 = new BigList<string>(new string[] { "foo", "baz", "smell", "the", "glove" });
            Algorithms.ReverseInPlace(list3);
            InterfaceTests.TestListGeneric<string>(list3, new string[] { "glove", "the", "smell", "baz", "foo" });

            string[] array1 =  { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.ReverseInPlace(array1);
            InterfaceTests.TestEnumerableElements<string>(list2, new string[] { "glove", "the", "smell", "baz", "bar", "foo" });

        }

        [Test]
        public void Rotate1()
        {
            IList<string> list1 = new List<string>();
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list1, 3), new string[0]);
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list1, 0), new string[0]);
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list1, -1), new string[0]);

            IList<string> list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list2, 1), new string[] { "bar", "baz", "smell", "the", "glove", "foo" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list2, 0), new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list2, 10), new string[] { "the", "glove", "foo", "bar", "baz", "smell"});
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list2, -2), new string[] { "the", "glove", "foo", "bar", "baz", "smell" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list2, -13), new string[] { "glove", "foo", "bar", "baz", "smell", "the" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list2, 6), new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });

            IList<string> list3 = new BigList<string>(new string[] { "foo", "baz", "smell", "the", "glove" });
            InterfaceTests.TestEnumerableElements(Algorithms.Rotate(list3, -3), new string[] { "smell", "the", "glove", "foo", "baz" });
            InterfaceTests.TestListGeneric<string>(list3, new string[] { "foo", "baz", "smell", "the", "glove" });
        }

        [Test]
        public void Rotate2()
        {
            IList<string> list1 = new List<string>();
            Algorithms.RotateInPlace(list1, 3);
            InterfaceTests.TestListGeneric<string>(list1, new string[0]);
            Algorithms.RotateInPlace(list1, 0);
            InterfaceTests.TestListGeneric<string>(list1, new string[0]);
            Algorithms.RotateInPlace(list1, -1);
            InterfaceTests.TestListGeneric<string>(list1, new string[0]);

            IList<string> list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list2, 1);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "bar", "baz", "smell", "the", "glove", "foo" });

            list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list2, 0);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });

            list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list2, 10);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "the", "glove", "foo", "bar", "baz", "smell" });

            list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list2, -2);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "the", "glove", "foo", "bar", "baz", "smell" });

            list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list2, -13);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "glove", "foo", "bar", "baz", "smell", "the" });

            list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list2, 6);
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "baz", "smell", "the", "glove" });

            IList<string> list3 = new BigList<string>(new string[] { "foo", "baz", "smell", "the", "glove" });
            Algorithms.RotateInPlace(list3, -3);
            InterfaceTests.TestListGeneric<string>(list3, new string[] { "smell", "the", "glove", "foo", "baz" });

            string[] array1 =  { "foo", "bar", "baz", "smell", "the", "glove" };
            Algorithms.RotateInPlace(array1, -2);
            InterfaceTests.TestEnumerableElements<string>(array1, new string[] { "the", "glove", "foo", "bar", "baz", "smell" });

        }

        [Test]
        public void RandomShuffle1()
        {
            const int ITER = 100000;

            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            string[] result1 = Algorithms.RandomShuffle(coll1);
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            IEnumerable<string> coll2 = EnumerableFromArray(new string[1] { "foo" });
            string[] result2 = Algorithms.RandomShuffle(coll2);
            InterfaceTests.TestEnumerableElements<string>(result2, new string[1] { "foo" });

            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "foo", "bar" });
            string[] result3 = Algorithms.RandomShuffle(coll3);
            InterfaceTests.TestEnumerableElementsAnyOrder<string>(result3, new string[] { "foo", "bar" });

            int[,] count = new int[6,6];
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "a", "b", "c", "d", "e", "f" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomShuffle(coll4);
                InterfaceTests.TestEnumerableElementsAnyOrder<string>(result4, new string[] { "a", "b", "c", "d", "e", "f" });

                for (int i = 0; i < 6; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 6; ++i)
                for (int j = 0; j < 6; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 6) * 0.97 && count[i, j] < (ITER / 6) * 1.03);
                }
        }

        [Test]
        public void RandomShuffle2()
        {
            const int ITER = 100000;

            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            string[] result1 = Algorithms.RandomShuffle(coll1, new Random(167));
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            IEnumerable<string> coll2 = EnumerableFromArray(new string[1] { "foo" });
            string[] result2 = Algorithms.RandomShuffle(coll2, new Random(167));
            InterfaceTests.TestEnumerableElements<string>(result2, new string[1] { "foo" });

            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "foo", "bar" });
            string[] result3 = Algorithms.RandomShuffle(coll3, new Random(167));
            InterfaceTests.TestEnumerableElementsAnyOrder<string>(result3, new string[] { "foo", "bar" });

            Random rand = new Random(199);
            int[,] count = new int[6, 6];
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "a", "b", "c", "d", "e", "f" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomShuffle(coll4, rand);
                InterfaceTests.TestEnumerableElementsAnyOrder<string>(result4, new string[] { "a", "b", "c", "d", "e", "f" });

                for (int i = 0; i < 6; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 6; ++i)
                for (int j = 0; j < 6; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 6) * 0.97 && count[i, j] < (ITER / 6) * 1.03);
                }
        }


        [Test]
        public void RandomShuffle3()
        {
            const int ITER = 100000;

            IList<string> list1 = new List<string>();
            Algorithms.RandomShuffleInPlace(list1);
            InterfaceTests.TestListGeneric<string>(list1, new string[0]);

            IList<string> list2 = new List<string>(new string[1] { "foo" });
            Algorithms.RandomShuffleInPlace(list2);
            InterfaceTests.TestListGeneric<string>(list2, new string[1] { "foo" });

            IList<string> list3 = new List<string>(new string[] { "foo", "bar" });
            Algorithms.RandomShuffleInPlace(list3);
            InterfaceTests.TestEnumerableElementsAnyOrder(list3, new string[] { "foo", "bar" });

            int[,] count = new int[6, 6];
            for (int iter = 0; iter < ITER; ++iter) {
                IList<string> list4 = new List<string>(new string[] { "a", "b", "c", "d", "e", "f" });
                Algorithms.RandomShuffleInPlace(list4);
                InterfaceTests.TestEnumerableElementsAnyOrder(list4, new string[] { "a", "b", "c", "d", "e", "f" });

                for (int i = 0; i < 6; ++i)
                    count[i, list4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 6; ++i)
                for (int j = 0; j < 6; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 6) * 0.97 && count[i, j] < (ITER / 6) * 1.03);
                }
        }


        [Test]
        public void RandomShuffle4()
        {
            const int ITER = 100000;

            IList<string> list1 = new List<string>();
            Algorithms.RandomShuffleInPlace(list1, new Random(1874));
            InterfaceTests.TestListGeneric<string>(list1, new string[0]);

            IList<string> list2 = new List<string>(new string[1] { "foo" });
            Algorithms.RandomShuffleInPlace(list2, new Random(1));
            InterfaceTests.TestListGeneric<string>(list2, new string[1] { "foo" });

            IList<string> list3 = new List<string>(new string[] { "foo", "bar" });
            Algorithms.RandomShuffleInPlace(list3, new Random(11998));
            InterfaceTests.TestEnumerableElementsAnyOrder(list3, new string[] { "foo", "bar" });

            Random rand = new Random(110);
            int[,] count = new int[6, 6];
            for (int iter = 0; iter < ITER; ++iter) {
                IList<string> list4 = new List<string>(new string[] { "a", "b", "c", "d", "e", "f" });
                Algorithms.RandomShuffleInPlace(list4, rand);
                InterfaceTests.TestEnumerableElementsAnyOrder(list4, new string[] { "a", "b", "c", "d", "e", "f" });

                for (int i = 0; i < 6; ++i)
                    count[i, list4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 6; ++i)
                for (int j = 0; j < 6; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 6) * 0.97 && count[i, j] < (ITER / 6) * 1.03);
                }
        }

        [Test]
        public void RandomShuffle5()
        {
            const int ITER = 100000;

            string[] array1 = new string[0];
            Algorithms.RandomShuffleInPlace(array1);
            InterfaceTests.TestEnumerableElements<string>(array1, new string[0]);

            string[] array2 =  { "foo" };
            Algorithms.RandomShuffleInPlace(array2);
            InterfaceTests.TestEnumerableElements<string>(array2, new string[1] { "foo" });

            string[] array3 = { "foo", "bar" };
            Algorithms.RandomShuffleInPlace(array3);
            InterfaceTests.TestEnumerableElementsAnyOrder(array3, new string[] { "foo", "bar" });

            int[,] count = new int[6, 6];
            for (int iter = 0; iter < ITER; ++iter) {
                string[] array4 = { "a", "b", "c", "d", "e", "f" };
                Algorithms.RandomShuffleInPlace(array4);
                InterfaceTests.TestEnumerableElementsAnyOrder(array4, new string[] { "a", "b", "c", "d", "e", "f" });

                for (int i = 0; i < 6; ++i)
                    count[i, array4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 6; ++i)
                for (int j = 0; j < 6; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 6) * 0.97 && count[i, j] < (ITER / 6) * 1.03);
                }
        }


        [Test]
        public void RandomSubset1()
        {
            const int ITER = 100000;

            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            string[] result1 = Algorithms.RandomSubset(coll1, 0);
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            IEnumerable<string> coll2 = EnumerableFromArray(new string[1] { "foo" });
            string[] result2 = Algorithms.RandomSubset(coll2, 1);
            InterfaceTests.TestEnumerableElements<string>(result2, new string[1] { "foo" });

            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "foo", "bar" });
            string[] result3 = Algorithms.RandomSubset(coll3, 1);
            Assert.IsTrue(result3.Length == 1);
            Assert.IsTrue(result3[0] == "foo" || result3[0] == "bar");

            int[,] count = new int[3, 7];
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "a", "b", "c", "d", "e", "f", "g" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(coll4, 3);

                for (int i = 0; i < 3; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 7; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 7) * 0.97 && count[i, j] < (ITER / 7) * 1.03);
                }

            count = new int[5, 5];
            coll4 = EnumerableFromArray(new string[] { "a", "b", "c", "d", "e"});
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(coll4, 5);

                for (int i = 0; i < 5; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 5) * 0.97 && count[i, j] < (ITER / 5) * 1.03);
                }
        }

        [Test]
        public void RandomSubset2()
        {
            const int ITER = 100000;

            IEnumerable<string> coll1 = EnumerableFromArray(new string[0]);
            string[] result1 = Algorithms.RandomSubset(coll1, 0, new Random(181));
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            IEnumerable<string> coll2 = EnumerableFromArray(new string[1] { "foo" });
            string[] result2 = Algorithms.RandomSubset(coll2, 1, new Random(199));
            InterfaceTests.TestEnumerableElements<string>(result2, new string[1] { "foo" });

            IEnumerable<string> coll3 = EnumerableFromArray(new string[] { "foo", "bar" });
            string[] result3 = Algorithms.RandomSubset(coll3, 1, new Random(11999));
            Assert.IsTrue(result3.Length == 1);
            Assert.IsTrue(result3[0] == "foo" || result3[0] == "bar");

            Random rand = new Random(1973);
            int[,] count = new int[3, 7];
            IEnumerable<string> coll4 = EnumerableFromArray(new string[] { "a", "b", "c", "d", "e", "f", "g" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(coll4, 3, rand);

                for (int i = 0; i < 3; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 7; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 7) * 0.97 && count[i, j] < (ITER / 7) * 1.03);
                }

            count = new int[5, 5];
            coll4 = EnumerableFromArray(new string[] { "a", "b", "c", "d", "e" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(coll4, 5, rand);

                for (int i = 0; i < 5; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 5) * 0.97 && count[i, j] < (ITER / 5) * 1.03);
                }
        }

        [Test]
        public void RandomSubset3()
        {
            const int ITER = 100000;

            IList<string> list1 = new List<string>(new string[0]);
            string[] result1 = Algorithms.RandomSubset(list1, 0);
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            IList<string> list2 = new List<string>(new string[1] { "foo" });
            string[] result2 = Algorithms.RandomSubset(list2, 1);
            InterfaceTests.TestEnumerableElements<string>(result2, new string[1] { "foo" });

            IList<string> list3 = new List<string>(new string[] { "foo", "bar" });
            string[] result3 = Algorithms.RandomSubset(list3, 1);
            Assert.IsTrue(result3.Length == 1);
            Assert.IsTrue(result3[0] == "foo" || result3[0] == "bar");

            int[,] count = new int[3, 7];
            IList<string> list4 = new List<string>(new string[] { "a", "b", "c", "d", "e", "f", "g" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(list4, 3);

                for (int i = 0; i < 3; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 7; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 7) * 0.97 && count[i, j] < (ITER / 7) * 1.03);
                }

            count = new int[5, 5];
            list4 = new List<string>(new string[] { "a", "b", "c", "d", "e" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(list4, 5);

                for (int i = 0; i < 5; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 5) * 0.97 && count[i, j] < (ITER / 5) * 1.03);
                }
        }

        [Test]
        public void RandomSubset4()
        {
            const int ITER = 100000;

            IList<string> list1 = new List<string>(new string[0]);
            string[] result1 = Algorithms.RandomSubset(list1, 0, new Random(998));
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            IList<string> list2 = new List<string>(new string[1] { "foo" });
            string[] result2 = Algorithms.RandomSubset(list2, 1, new Random(12));
            InterfaceTests.TestEnumerableElements<string>(result2, new string[1] { "foo" });

            IList<string> list3 = new List<string>(new string[] { "foo", "bar" });
            string[] result3 = Algorithms.RandomSubset(list3, 1, new Random(1100));
            Assert.IsTrue(result3.Length == 1);
            Assert.IsTrue(result3[0] == "foo" || result3[0] == "bar");

            Random rand = new Random(9987);
            int[,] count = new int[3, 7];
            IList<string> list4 = new List<string>(new string[] { "a", "b", "c", "d", "e", "f", "g" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(list4, 3, rand);

                for (int i = 0; i < 3; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 7; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 7) * 0.97 && count[i, j] < (ITER / 7) * 1.03);
                }

            count = new int[5, 5];
            list4 = new List<string>(new string[] { "a", "b", "c", "d", "e" });
            for (int iter = 0; iter < ITER; ++iter) {
                string[] result4 = Algorithms.RandomSubset(list4, 5, rand);

                for (int i = 0; i < 5; ++i)
                    count[i, result4[i][0] - 'a'] += 1;
            }

            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j) {
                    Assert.IsTrue(count[i, j] > (ITER / 5) * 0.97 && count[i, j] < (ITER / 5) * 1.03);
                }
        }

        [Test]
        public void GeneratePermutations()
        {
            List<char> list = new List<char>();
            Set<string> set = new Set<string>();
            char[] array;
            string s;

            // Test permutations of 0 elements.
            foreach (char[] entry in Algorithms.GeneratePermutations(list)) {
                Assert.Fail("Shouldn't be any permutations");
            }

            // Test 1 through 7.
            for (int length = 1; length <= 7; ++length) {
                set.Clear();
                list.Clear();
                for (int i = 0; i < length; ++i)
                    list.Add((char)('A' + i));
                array = list.ToArray();

                foreach (char[] entry in Algorithms.GeneratePermutations(list)) {
                    InterfaceTests.TestEnumerableElementsAnyOrder((IEnumerable<char>)entry, array);
                    s = new String(entry);
                    //Console.WriteLine(s);
                    Assert.IsFalse(set.Contains(s));
                    set.Add(s);
                }

                int factorial = 1;
                for (int i = 1; i <= length; ++i)
                    factorial *= i;
                Assert.AreEqual(factorial, set.Count);
            }
        }

        [Test]
        public void GenerateSortedPermutations1()
        {
            List<char> list = new List<char>();
            Set<string> set = new Set<string>();
            char[] array;
            string s, prev;

            // Test permutations of 0 elements.
            foreach (char[] entry in Algorithms.GeneratePermutations(list)) {
                Assert.Fail("Shouldn't be any permutations");
            }

            // Test 1 through 7.
            for (int length = 1; length <= 7; ++length) {
                set.Clear();
                list.Clear();
                prev = null; s = null;
                for (int i = 0; i < length; ++i)
                    list.Add((char)('A' + i));
                array = list.ToArray();

                foreach (char[] entry in Algorithms.GenerateSortedPermutations(list)) {
                    prev = s;
                    InterfaceTests.TestEnumerableElementsAnyOrder((IEnumerable<char>)entry, array);
                    s = new String(entry);
                    Assert.IsFalse(set.Contains(s));
                    set.Add(s);

                    if (prev != null)
                        Assert.IsTrue(string.CompareOrdinal(prev, s) < 0);
                }

                int factorial = 1;
                for (int i = 1; i <= length; ++i)
                    factorial *= i;
                Assert.AreEqual(factorial, set.Count);
            }

            // Test some with equal elements too.
            set.Clear();
            list.Clear();
            prev = null; s = null;
            list.AddRange(new char[] { 'A', 'B', 'B', 'C', 'C' });
            array = list.ToArray();

            foreach (char[] entry in Algorithms.GenerateSortedPermutations(list)) {
                prev = s;
                InterfaceTests.TestEnumerableElementsAnyOrder((IEnumerable<char>)entry, array);
                s = new String(entry);
                Assert.IsFalse(set.Contains(s));
                set.Add(s);

                if (prev != null)
                    Assert.IsTrue(string.CompareOrdinal(prev, s) < 0);
            }
            Assert.AreEqual(30, set.Count);

            set.Clear();
            list.Clear();
            prev = null; s = null;
            list.AddRange(new char[] { 'A', 'A', 'A', 'A' });
            array = list.ToArray();

            foreach (char[] entry in Algorithms.GenerateSortedPermutations(list)) {
                prev = s;
                InterfaceTests.TestEnumerableElementsAnyOrder((IEnumerable<char>)entry, array);
                s = new String(entry);
                Assert.IsFalse(set.Contains(s));
                set.Add(s);

                if (prev != null)
                    Assert.IsTrue(string.CompareOrdinal(prev, s) < 0);
            }
            Assert.AreEqual(1, set.Count);

            Comparison<char> caseInsensitiveComparison = delegate(char x, char y) {
                x = char.ToLower(x);
                y = char.ToLower(y);
                return x.CompareTo(y);
            };

            set.Clear();
            list.Clear();
            prev = null; s = null;
            list.AddRange(new char[] { 'A', 'B', 'C', 'a', 'b' });
            array = list.ToArray();

            foreach (char[] entry in Algorithms.GenerateSortedPermutations(list, caseInsensitiveComparison)) {
                prev = s;
                InterfaceTests.TestEnumerableElementsAnyOrder((IEnumerable<char>)entry, array);
                s = new String(entry);
                Assert.IsFalse(set.Contains(s));
                set.Add(s);
                
                if (prev != null)
                    Assert.IsTrue(string.Compare(prev, s, StringComparison.CurrentCultureIgnoreCase) < 0);
            }
            Assert.AreEqual(30, set.Count);

        }

        [Test]
        public void ReadOnlyExceptions()
        {
            IList<string> list1 = new List<string>(new string[] { "foo", "bar", "baz" }).AsReadOnly();

            try {
                Algorithms.Fill(list1, "elvis");
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.Copy<string>(new string[] { "hello", "sailor" }, list1, 2);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.Copy<string>(new string[] { "hello", "sailor" }, list1, 2, 3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.Copy<string>(new string[] { "hello", "sailor" }, 0, list1, 2, 3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.ReverseInPlace(list1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.ReplaceInPlace(list1, (string)null, "");
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.ReplaceInPlace(list1, delegate(string x) { return x == null; }, "");
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.RemoveDuplicatesInPlace(list1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.RandomShuffleInPlace(list1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.SortInPlace(list1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.StableSortInPlace(list1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.RemoveWhere(list1, delegate(string x) { return x == null; });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.Partition(list1, delegate(string x) { return x == null; });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                Algorithms.StablePartition(list1, delegate(string x) { return x == null; });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [Test]
        public void SetIntersection()
        {
            IEnumerable<int> enumOdds = EnumerableFromArray<int>(new int[] { 3, 5, 7, 7, 9, 1, 11, 13, 3, 15, 17, 1, 3, 11, 17, 19, 1 });
            IEnumerable<int> enumDigits = EnumerableFromArray<int>(new int[] { 2, 1, 3, 7, 7, 2, 4, 7, 5, 9, 5, 6, 7, 3, 7, 7, 3, 8 });
            IEnumerable<int> result;

            // Algorithms work different depending on sizes, so try both ways.
            result = Algorithms.SetIntersection(enumOdds, enumDigits);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 3, 5, 7, 3, 1, 7, 9, 3 });

            result = Algorithms.SetIntersection(enumDigits, enumOdds);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 1, 3, 3, 3, 5, 7, 7, 9 });

            IEnumerable<string> set1 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set2 = EnumerableFromArray<string>(new string[] {"CHEESE", "bAgEL", "BAGEL",  null, "pancakes", null});
            IEnumerable<string> res = Algorithms.SetIntersection(set1, set2, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElementsAnyOrder(res, new string[] { "cheese", null, "bagel", "bagel" }, StringComparer.InvariantCultureIgnoreCase.Equals);
        }

        [Test]
        public void SetUnion()
        {
            IEnumerable<int> enumOdds = EnumerableFromArray<int>(new int[] { 3, 5, 7, 7, 9, 1, 11, 13, 3, 15, 17, 1, 3, 11, 17, 19, 1 });
            IEnumerable<int> enumDigits = EnumerableFromArray<int>(new int[] { 2, 1, 3, 7, 7, 2, 4, 7, 5, 9, 5, 6, 7, 3, 7, 7, 3, 8 });
            IEnumerable<int> result;

            // Algorithms work different depending on sizes, so try both ways.
            result = Algorithms.SetUnion(enumOdds, enumDigits);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 });

            result = Algorithms.SetUnion(enumDigits, enumOdds);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 1, 1, 1, 2, 2, 3, 3, 3, 4, 5, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 11, 11, 13, 15, 17, 17, 19 });

            IEnumerable<string> set1 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set2 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> res = Algorithms.SetUnion(set1, set2, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElementsAnyOrder(res, new string[] { "apple", "apple", "banana", "cheese", null, null, "bagel", "bagel", "meat", "pancakes" }, StringComparer.InvariantCultureIgnoreCase.Equals);
        }

        [Test]
        public void SetDifference()
        {
            IEnumerable<int> enumOdds = EnumerableFromArray<int>(new int[] { 3, 5, 7, 7, 9, 1, 11, 13, 3, 15, 17, 1, 3, 11, 17, 19, 1 });
            IEnumerable<int> enumDigits = EnumerableFromArray<int>(new int[] { 2, 1, 3, 7, 7, 2, 4, 7, 5, 9, 5, 6, 7, 3, 7, 7, 3, 8 });
            IEnumerable<int> result;

            // Algorithms work different depending on sizes, so try both ways.
            result = Algorithms.SetDifference(enumOdds, enumDigits);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 1, 1, 11, 11, 13, 15, 17, 17, 19 });

            result = Algorithms.SetDifference(enumDigits, enumOdds);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 2, 2, 4, 5, 6, 7, 7, 7, 7, 8 });

            IEnumerable<string> set1 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set2 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> res = Algorithms.SetDifference(set1, set2, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElementsAnyOrder(res, new string[] { "APPLE", "APPLE", "BANANA", "MEAT" }, StringComparer.InvariantCultureIgnoreCase.Equals);
            res = Algorithms.SetDifference(set2, set1, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElementsAnyOrder(res, new string[] { null, "PANCAKES" }, StringComparer.InvariantCultureIgnoreCase.Equals);
        }

        [Test]
        public void SetSymmetricDifference()
        {
            IEnumerable<int> enumOdds = EnumerableFromArray<int>(new int[] { 3, 5, 7, 7, 9, 1, 11, 13, 3, 15, 17, 1, 3, 11, 17, 19, 1 });
            IEnumerable<int> enumDigits = EnumerableFromArray<int>(new int[] { 2, 1, 3, 7, 7, 2, 4, 7, 5, 9, 5, 6, 7, 3, 7, 7, 3, 8 });
            IEnumerable<int> result;

            // Algorithms work different depending on sizes, so try both ways.
            result = Algorithms.SetSymmetricDifference(enumOdds, enumDigits);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 });

            result = Algorithms.SetSymmetricDifference(enumDigits, enumOdds);
            InterfaceTests.TestEnumerableElementsAnyOrder(result, new int[] { 1, 1, 2, 2, 4, 5, 6, 7, 7, 7, 7, 8, 11, 11, 13, 15, 17, 17, 19 });

            IEnumerable<string> set1 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set2 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> res = Algorithms.SetSymmetricDifference(set1, set2, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElementsAnyOrder(res, new string[] { "APPLE", "APPLE", null, "MEAT", "PANCAKES", "BANANA" }, StringComparer.InvariantCultureIgnoreCase.Equals);
        }

        [Test]
        public void DisjointSets()
        {
            IEnumerable<int> set1 = EnumerableFromArray<int>(new int[] { 13, 11, 16, 16, 17, 16, 18, 11, 19, 16, 19 });
            IEnumerable<int> set2 = EnumerableFromArray<int>(new int[0]);
            IEnumerable<int> set3 = EnumerableFromArray<int>(new int[] { 6, 9, 6, 9, 1 });
            IEnumerable<int> set4 = EnumerableFromArray<int>(new int[] { 6, 6, 1, 9, 9 });
            IEnumerable<int> set5 = EnumerableFromArray<int>(new int[] { 7, 3, 6, 7 });

            Assert.IsTrue(Algorithms.DisjointSets(set2, set1));
            Assert.IsTrue(Algorithms.DisjointSets(set1, set2));

            Assert.IsTrue(Algorithms.DisjointSets(set3, set1));
            Assert.IsTrue(Algorithms.DisjointSets(set1, set3));

            Assert.IsTrue(Algorithms.DisjointSets(set5, set1));
            Assert.IsTrue(Algorithms.DisjointSets(set1, set5));

            Assert.IsFalse(Algorithms.DisjointSets(set5, set3));
            Assert.IsFalse(Algorithms.DisjointSets(set3, set5));

            Assert.IsFalse(Algorithms.DisjointSets(set3, set4));
            Assert.IsFalse(Algorithms.DisjointSets(set4, set3));

            Assert.IsFalse(Algorithms.DisjointSets(set3, set3));

            Assert.IsTrue(Algorithms.DisjointSets(set2, set2));

            IEnumerable<string> set10 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set11 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> set12 = EnumerableFromArray<string>(new string[] { "aPple", "foo", "waffles", "Waffles" });
            Assert.IsFalse(Algorithms.DisjointSets(set10, set11, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.DisjointSets(set10, set12, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsTrue(Algorithms.DisjointSets(set11, set12, StringComparer.InvariantCultureIgnoreCase));
        }

        [Test]
        public void EqualSets()
        {
            IEnumerable<int> set1 = EnumerableFromArray<int>(new int[] { 1, 3, 6, 9, 3, 1, 6 });
            IEnumerable<int> set2 = EnumerableFromArray<int>(new int[] { 9, 3, 6, 9, 1, 6 });
            IEnumerable<int> set3 = EnumerableFromArray<int>(new int[0]);
            IEnumerable<int> set4 = EnumerableFromArray<int>(new int[] { 6, 9, 6, 9, 1 });
            IEnumerable<int> set5 = EnumerableFromArray<int>(new int[] { 6, 6, 1, 9, 9 });

            Assert.IsFalse(Algorithms.EqualSets(set2, set1));
            Assert.IsFalse(Algorithms.EqualSets(set1, set2));

            Assert.IsFalse(Algorithms.EqualSets(set3, set1));
            Assert.IsFalse(Algorithms.EqualSets(set1, set3));

            Assert.IsTrue(Algorithms.EqualSets(set4, set5));
            Assert.IsTrue(Algorithms.EqualSets(set5, set4));

            Assert.IsFalse(Algorithms.EqualSets(set1, set4));
            Assert.IsFalse(Algorithms.EqualSets(set4, set1));

            Assert.IsFalse(Algorithms.EqualSets(set3, set4));
            Assert.IsFalse(Algorithms.EqualSets(set4, set3));

            Assert.IsTrue(Algorithms.EqualSets(set3, set3));

            Assert.IsTrue(Algorithms.EqualSets(set2, set2));

            IEnumerable<string> set10 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set11 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> set12 = EnumerableFromArray<string>(new string[] { null, "PANCAKES", "bagel", "bagel", "cheese", null });
            Assert.IsFalse(Algorithms.EqualSets(set10, set11, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.EqualSets(set10, set12, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsTrue(Algorithms.EqualSets(set11, set12, StringComparer.InvariantCultureIgnoreCase));
        }

        [Test]
        public void GetSetEqualityComparer()
        {
            IEqualityComparer<IEnumerable<int>> comparer = Algorithms.GetSetEqualityComparer<int>();

            IEnumerable<int> set1 = EnumerableFromArray<int>(new int[] { 1, 3, 6, 9, 3, 1, 6 });
            IEnumerable<int> set2 = EnumerableFromArray<int>(new int[] { 9, 3, 6, 9, 1, 6 });
            IEnumerable<int> set3 = EnumerableFromArray<int>(new int[0]);
            IEnumerable<int> set4 = EnumerableFromArray<int>(new int[] { 6, 9, 6, 9, 1 });
            IEnumerable<int> set5 = EnumerableFromArray<int>(new int[] { 6, 6, 1, 9, 9 });

            Assert.IsFalse(comparer.Equals(set2, set1));
            Assert.IsFalse(comparer.Equals(set1, set2));
            Assert.IsFalse(comparer.GetHashCode(set1) == comparer.GetHashCode(set2));

            Assert.IsFalse(comparer.Equals(set3, set1));
            Assert.IsFalse(comparer.Equals(set1, set3));
            Assert.IsFalse(comparer.GetHashCode(set1) == comparer.GetHashCode(set3));

            Assert.IsTrue(comparer.Equals(set4, set5));
            Assert.IsTrue(comparer.Equals(set5, set4));
            Assert.IsTrue(comparer.GetHashCode(set4) == comparer.GetHashCode(set5));

            Assert.IsFalse(comparer.Equals(set1, set4));
            Assert.IsFalse(comparer.Equals(set4, set1));
            Assert.IsFalse(comparer.GetHashCode(set4) == comparer.GetHashCode(set1));

            Assert.IsFalse(comparer.Equals(set3, set4));
            Assert.IsFalse(comparer.Equals(set4, set3));
            Assert.IsFalse(comparer.GetHashCode(set4) == comparer.GetHashCode(set3));

            Assert.IsTrue(comparer.Equals(set3, set3));

            Assert.IsTrue(comparer.Equals(set2, set2));

            IEqualityComparer<IEnumerable<string>> comparer2 = Algorithms.GetSetEqualityComparer<string>(StringComparer.InvariantCultureIgnoreCase);
            IEnumerable<string> set10 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set11 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> set12 = EnumerableFromArray<string>(new string[] { null, "PANCAKES", "bagel", "bagel", "cheese", null });
            Assert.IsFalse(comparer2.Equals(set10, set11));
            Assert.IsFalse(comparer2.Equals(set10, set11));
            Assert.IsFalse(comparer2.GetHashCode(set10) == comparer2.GetHashCode(set11));

            Assert.IsFalse(comparer2.Equals(set10, set12));
            Assert.IsFalse(comparer2.Equals(set10, set12));
            Assert.IsFalse(comparer2.GetHashCode(set10) == comparer2.GetHashCode(set12));

            Assert.IsTrue(comparer2.Equals(set11, set12));
            Assert.IsTrue(comparer2.Equals(set11, set12));
            Assert.IsTrue(comparer2.GetHashCode(set11) == comparer2.GetHashCode(set12));
        }

        [Test]
        public void Subset()
        {
            IEnumerable<int> set1 = EnumerableFromArray<int>(new int[] { 1, 3, 6, 9, 3, 1, 6 });
            IEnumerable<int> set2 = EnumerableFromArray<int>(new int[] { 9, 3, 6, 9, 1, 6 });
            IEnumerable<int> set3 = EnumerableFromArray<int>(new int[0]);
            IEnumerable<int> set4 = EnumerableFromArray<int>(new int[] { 6, 9, 6, 9, 1 });
            IEnumerable<int> set5 = EnumerableFromArray<int>(new int[] { 6, 6, 1, 9, 9 });

            Assert.IsFalse(Algorithms.IsSubsetOf(set2, set1));
            Assert.IsFalse(Algorithms.IsSubsetOf(set1, set2));

            Assert.IsFalse(Algorithms.IsSubsetOf(set2, set4));
            Assert.IsTrue(Algorithms.IsSubsetOf(set4, set2));

            Assert.IsFalse(Algorithms.IsSubsetOf(set4, set1));
            Assert.IsFalse(Algorithms.IsSubsetOf(set1, set4));

            Assert.IsTrue(Algorithms.IsSubsetOf(set4, set5));
            Assert.IsTrue(Algorithms.IsSubsetOf(set5, set4));

            Assert.IsTrue(Algorithms.IsSubsetOf(set3, set2));
            Assert.IsFalse(Algorithms.IsSubsetOf(set2, set3));

            Assert.IsTrue(Algorithms.IsSubsetOf(set1, set1));
            Assert.IsTrue(Algorithms.IsSubsetOf(set3, set3));

            IEnumerable<string> set10 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set11 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> set12 = EnumerableFromArray<string>(new string[] { null, "PANCAKES", "bagel", "bagel", "cheese", null });
            IEnumerable<string> set13 = EnumerableFromArray<string>(new string[] { "cheese", "meat", "APPLE", "bAGel" });

            Assert.IsFalse(Algorithms.IsSubsetOf(set10, set11, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.IsSubsetOf(set11, set10, StringComparer.InvariantCultureIgnoreCase));

            Assert.IsTrue(Algorithms.IsSubsetOf(set11, set12, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsTrue(Algorithms.IsSubsetOf(set12, set11, StringComparer.InvariantCultureIgnoreCase));

            Assert.IsFalse(Algorithms.IsSubsetOf(set10, set13, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsTrue(Algorithms.IsSubsetOf(set13, set10, StringComparer.InvariantCultureIgnoreCase));
        }

        [Test]
        public void ProperSubset()
        {
            IEnumerable<int> set1 = EnumerableFromArray<int>(new int[] { 1, 3, 6, 9, 3, 1, 6 });
            IEnumerable<int> set2 = EnumerableFromArray<int>(new int[] { 9, 3, 6, 9, 1, 6 });
            IEnumerable<int> set3 = EnumerableFromArray<int>(new int[0]);
            IEnumerable<int> set4 = EnumerableFromArray<int>(new int[] { 6, 9, 6, 9, 1 });
            IEnumerable<int> set5 = EnumerableFromArray<int>(new int[] { 6, 6, 1, 9, 9 });

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set2, set1));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set1, set2));

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set2, set4));
            Assert.IsTrue(Algorithms.IsProperSubsetOf(set4, set2));

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set4, set1));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set1, set4));

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set4, set5));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set5, set4));

            Assert.IsTrue(Algorithms.IsProperSubsetOf(set3, set2));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set2, set3));

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set1, set1));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set3, set3));

            IEnumerable<string> set10 = EnumerableFromArray<string>(new string[] { "apple", "banana", "BAGEL", "APPLE", "cheese", null, "meat", "bAGEL" });
            IEnumerable<string> set11 = EnumerableFromArray<string>(new string[] { "CHEESE", "bAgEL", "BAGEL", null, "pancakes", null });
            IEnumerable<string> set12 = EnumerableFromArray<string>(new string[] { null, "PANCAKES", "bagel", "bagel", "cheese", null });
            IEnumerable<string> set13 = EnumerableFromArray<string>(new string[] { "cheese", "meat", "APPLE", "bAGel" });

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set10, set11, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set11, set10, StringComparer.InvariantCultureIgnoreCase));

            Assert.IsFalse(Algorithms.IsProperSubsetOf(set11, set12, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set12, set11, StringComparer.InvariantCultureIgnoreCase));

            Assert.IsTrue(Algorithms.IsProperSubsetOf(set13, set10, StringComparer.InvariantCultureIgnoreCase));
            Assert.IsFalse(Algorithms.IsProperSubsetOf(set10, set13, StringComparer.InvariantCultureIgnoreCase));
        }

        [Test]
        public void CartesianProduct()
        {
            IEnumerable<int> first = EnumerableFromArray(new int[] { 1, 8, 4 });
            IEnumerable<string> second = EnumerableFromArray(new string[] { "foo", "bar" });
            Pair<int, string>[] expected = {new Pair<int,string>(1, "foo"), new Pair<int,string>(1, "bar"), new Pair<int,string>(8, "foo"), 
                new Pair<int,string>(8, "bar"), new Pair<int,string>(4, "foo"), new Pair<int,string>(4, "bar")};

            InterfaceTests.TestEnumerableElementsAnyOrder(Algorithms.CartesianProduct(first, second), expected);

            second = EnumerableFromArray(new string[0]);
            InterfaceTests.TestEnumerableElementsAnyOrder(Algorithms.CartesianProduct(first, second), new Pair<int, string>[0]);
        }

        [Test]
        public void ReadOnlyList()
        {
            IList<string> list1 = new BigList<string>(new string[] { "foo", "bar", "hello", "sailor" });
            IList<string> result1 = Algorithms.ReadOnly(list1);
            InterfaceTests.TestReadOnlyListGeneric<string>(result1, new string[] { "foo", "bar", "hello", "sailor" }, "read-only list");

            IList<string> list2 = new Deque<string>(new string[] { });
            IList<string> result2 = Algorithms.ReadOnly(list2);
            InterfaceTests.TestReadOnlyListGeneric<string>(result2, new string[] { }, "read-only list");

            IList<string> list3 = new List<string>(new string[] { "foo", "bar", "hello", "sailor" }).AsReadOnly();
            IList<string> result3 = Algorithms.ReadOnly(list3);
            InterfaceTests.TestReadOnlyListGeneric<string>(result3, new string[] { "foo", "bar", "hello", "sailor" }, null);
            Assert.AreSame(list3, result3);
        }

        [Test]
        public void ReadOnlyCollection()
        {
            ICollection<string> collection1 = new OrderedSet<string>(new string[] { "foo", "bar", "hello", "sailor" });
            ICollection<string> result1 = Algorithms.ReadOnly(collection1);
            InterfaceTests.TestReadonlyCollectionGeneric<string>(result1, new string[] { "bar", "foo", "hello", "sailor" }, true, "read-only collection");

            ICollection<string> collection2 = new Set<string>(new string[] { });
            ICollection<string> result2 = Algorithms.ReadOnly(collection2);
            InterfaceTests.TestReadonlyCollectionGeneric<string>(result2, new string[] { }, true, "read-only collection");
        }

        [Test]
        public void ReadOnlyDictionary()
        {
            IDictionary<string, int> dict1 = new Dictionary<string, int>();
            dict1["foo"] = 12;
            dict1["zap"] = 123;
            dict1["HELLO"] = -1;
            IDictionary<string, int> result1 = Algorithms.ReadOnly(dict1);
            InterfaceTests.TestReadOnlyDictionaryGeneric<string, int>(result1, new string[] { "foo", "zap", "HELLO" }, new int[] { 12, 123, -1 },
                                    "fizzle", false, null, null, null);

            IDictionary<string, string> dict2 = new OrderedDictionary<string, string>();
            dict2["foo"] = "hi";
            dict2["zap"] = null;
            dict2[null] = "there";
            IDictionary<string, string> result2 = Algorithms.ReadOnly(dict2);
            InterfaceTests.TestReadOnlyDictionaryGeneric<string, string>(result2, new string[] { null, "foo", "zap" }, new string[] { "there", "hi", null },
                                    "fizzle", true, null, null, null);

            IDictionary<string, string> result3 = Algorithms.ReadOnly(result2);
            Assert.AreSame(result2, result3);

            IDictionary<string, string> dict4 = null;
            IDictionary<string, string> result4 = Algorithms.ReadOnly(dict4);
            Assert.IsNull(result4);

            IDictionary<int, string> dict5 = new Dictionary<int, string>();
            IDictionary<int,string> result5 = Algorithms.ReadOnly(dict5);
            InterfaceTests.TestReadOnlyDictionaryGeneric<int,string>(result5, new int[0], new string[0],
                                    0, true, null, null, null);
        }

        [Test]
        public void AddTypingList()
        {
            IList list1 = new ArrayList(new string[] { "foo", "bar", "hello", "sailor" });
            InterfaceTests.TestList<string>(list1, new string[] { "foo", "bar", "hello", "sailor" });
            IList<string> result1 = Algorithms.TypedAs<string>(list1);
            InterfaceTests.TestListGeneric<string>(result1, new string[] { "foo", "bar", "hello", "sailor" });

            IList list2 = new ArrayList(new string[] {  });
            InterfaceTests.TestList<string>(list2, new string[] {  });
            IList<string> result2 = Algorithms.TypedAs<string>(list2);
            InterfaceTests.TestListGeneric<string>(result2, new string[] {  });

            IList list3 = null;
            IList<string> result3 = Algorithms.TypedAs<string>(list3);
            Assert.IsNull(result3);
        }

        [Test]
        public void AddTypingCollection()
        {
            ICollection coll1 = new ArrayList(new string[] { "foo", "bar", "hello", "sailor" });
            InterfaceTests.TestCollection<string>(coll1, new string[] { "foo", "bar", "hello", "sailor" }, true);
            ICollection<string> result1 = Algorithms.TypedAs<string>(coll1);
            InterfaceTests.TestReadonlyCollectionGeneric<string>(result1, new string[] { "foo", "bar", "hello", "sailor" }, true, "strongly-typed Collection");

            ICollection coll2 = new ArrayList(new string[] { });
            InterfaceTests.TestCollection<string>(coll2, new string[] { }, true);
            ICollection<string> result2 = Algorithms.TypedAs<string>(coll2);
            InterfaceTests.TestReadonlyCollectionGeneric<string>(result2, new string[] { }, true, "strongly-typed Collection");

            ICollection coll3 = null;
            ICollection<string> result3 = Algorithms.TypedAs<string>(coll3);
            Assert.IsNull(result3);
        }

        [Test]
        public void AddTypingEnumerable()
        {
            IEnumerable enum1 = new ArrayList(new string[] { "foo", "bar", "hello", "sailor" });
            IEnumerable<string> result1 = Algorithms.TypedAs<string>(enum1);
            InterfaceTests.TestEnumerableElements<string>(result1, new string[] { "foo", "bar", "hello", "sailor" });

            IEnumerable enum2 = new ArrayList(new string[] { });
            IEnumerable<string> result2 = Algorithms.TypedAs<string>(enum2);
            InterfaceTests.TestEnumerableElements<string>(result2, new string[] { });

            IEnumerable enum3 = null;
            IEnumerable<string> result3 = Algorithms.TypedAs<string>(enum3);
            Assert.IsNull(result3);
        }

        [Test]
        public void RemoveTypingList()
        {
            IList<string> list1 = new IListWrapper<string>(new BigList<string>(new string[] { "foo", "bar", "hello", "sailor" }));
            InterfaceTests.TestListGeneric<string>(list1, new string[] { "foo", "bar", "hello", "sailor" });
            IList result1 = Algorithms.Untyped(list1);
            Assert.IsFalse(result1 == list1);
            InterfaceTests.TestList<string>(result1, new string[] { "foo", "bar", "hello", "sailor" });

            IList<string> list2 = new BigList<string>(new string[] { "foo", "bar", "hello", "sailor" });
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "foo", "bar", "hello", "sailor" });
            IList result2 = Algorithms.Untyped(list2);
            Assert.IsTrue(result2 == list2);   // should have just cast it away
            InterfaceTests.TestList<string>(result2, new string[] { "foo", "bar", "hello", "sailor" });

            IList<string> list3 = null;
            IList result3 = Algorithms.Untyped(list3);
            Assert.IsNull(result3);
        }

        [Test]
        public void RemoveTypingCollection()
        {
            ICollection<string> coll1 = new ICollectionWrapper<string>(new OrderedSet<string>(new string[] { "foo", "bar", "hello", "sailor" }));
            ICollection result1 = Algorithms.Untyped(coll1);
            Assert.IsFalse(result1 == coll1);
            InterfaceTests.TestCollection<string>(result1, new string[] { "bar", "foo", "hello", "sailor" }, true);

            ICollection<string> coll2 = new OrderedSet<string>(new string[] { "foo", "bar", "hello", "sailor" });
            ICollection result2 = Algorithms.Untyped(coll2);
            Assert.IsTrue(result2 == coll2);   // should have just cast it away
            InterfaceTests.TestCollection<string>(result2, new string[] { "bar", "foo", "hello", "sailor" }, true);

            ICollection<string> coll3 = null;
            ICollection result3 = Algorithms.Untyped(coll3);
            Assert.IsNull(result3);
        }

        [Test]
        public void Range()
        {
            IList<int> main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            IList<int> range = Algorithms.Range(main, 2, 4);

            InterfaceTests.TestListGeneric(range, new int[] { 2, 3, 4, 5 });

            main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = Algorithms.Range(main, 2, 4);
            range[1] = 7;
            range.Add(99);
            Assert.AreEqual(5, range.Count);
            range.RemoveAt(0);
            Assert.AreEqual(4, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 7, 4, 5, 99, 6, 7 });
            main[3] = 11;
            InterfaceTests.TestEnumerableElements(range, new int[] {7, 11, 5, 99});

            main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = Algorithms.Range(main, 5, 3);
            Assert.AreEqual(3, range.Count);
            main.Remove(6);
            main.Remove(5);
            Assert.AreEqual(1, range.Count);
            Assert.AreEqual(7, range[0]);

            main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = Algorithms.Range(main, 8, 0);
            range.Add(8);
            range.Add(9);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            InterfaceTests.TestEnumerableElements(range, new int[] { 8, 9 });

            main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = Algorithms.Range(main, 0, 4);
            range.Clear();
            Assert.AreEqual(0, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 4, 5, 6, 7 });
            range.Add(100);
            range.Add(101);
            InterfaceTests.TestEnumerableElements(main, new int[] { 100, 101, 4, 5, 6, 7 });

            main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = Algorithms.Range(main, 8, 0);
            InterfaceTests.TestListGeneric(range, new int[] {  });

            main = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }).AsReadOnly();
            range = Algorithms.Range(main, 2, 4);
            InterfaceTests.TestReadOnlyListGeneric(range, new int[] { 2, 3, 4, 5 }, null);
        }

        [Test]
        public void RangeExceptions()
        {
            IList<int> list = new BigList<int>(new int[] { 1 }, 100);
            IList<int> range;

            try {
                range = Algorithms.Range(list, 3, 98);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, -1, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 0, int.MaxValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 1, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 45, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 0, 101);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 100, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, int.MinValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, int.MaxValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }
        }

        [Test]
        public void ArrayRange()
        {
            int[] main = { 0, 1, 2, 3, 4, 5, 6, 7 };
            IList<int> range = Algorithms.Range(main, 2, 4);

            InterfaceTests.TestReadWriteListGeneric(range, new int[] { 2, 3, 4, 5 });

            main = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            range = Algorithms.Range(main, 2, 4);
            range[1] = 7;
            range.Add(99);
            Assert.AreEqual(5, range.Count);
            range.RemoveAt(0);
            Assert.AreEqual(4, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 7, 4, 5, 99, 6, 0 });
            main[3] = 11;
            InterfaceTests.TestEnumerableElements(range, new int[] { 7, 11, 5, 99 });


            main = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            range = Algorithms.Range(main, 3, 0);
            range.Add(8);
            range.Add(9);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 2, 8, 9, 3, 4, 5 });
            InterfaceTests.TestEnumerableElements(range, new int[] { 8, 9 });

            main = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            range = Algorithms.Range(main, 0, 4);
            range.Clear();
            Assert.AreEqual(0, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 4, 5, 6, 7, 0, 0, 0, 0 });
            range.Add(100);
            range.Add(101);
            InterfaceTests.TestEnumerableElements(main, new int[] { 100, 101, 4, 5, 6, 7, 0, 0 });

            main = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            range = Algorithms.Range(main, 8, 0);
            range.Insert(0, 11);
            Assert.AreEqual(0, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        }

        [Test]
        public void ArrayRangeExceptions()
        {
            int[] list = new int[100];
            IList<int> range;

            try {
                range = Algorithms.Range(list, 3, 98);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, -1, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 0, int.MaxValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 1, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 45, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 0, 101);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, 100, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, int.MinValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = Algorithms.Range(list, int.MaxValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("start", ((ArgumentOutOfRangeException)e).ParamName);
            }
        }

        [Test]
        public void FindFirstWhere()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[] { 4, 8, 1, 3, 4, 9 });
            int result;

            result = Algorithms.FindFirstWhere(coll1, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(1, result);

            result = Algorithms.FindFirstWhere(coll1, delegate(int x) { return (x & 1) == 0; });
            Assert.AreEqual(4, result);

            result = Algorithms.FindFirstWhere(coll1, delegate(int x) { return x > 10; });
            Assert.AreEqual(0, result);

            IEnumerable<int> coll2 = EnumerableFromArray(new int[] { });
            result = Algorithms.FindFirstWhere(coll2, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(0, result);
        }


        [Test]
        public void TryFindFirstWhere()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[] { 4, 8, 1, 3, 4, 9 });
            bool found;
            int result;

            found = Algorithms.TryFindFirstWhere(coll1, delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(1, result);

            found = Algorithms.TryFindFirstWhere(coll1, delegate(int x) { return (x & 1) == 0; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(4, result);

            found = Algorithms.TryFindFirstWhere(coll1, delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            IEnumerable<int> coll2 = EnumerableFromArray(new int[] { });
            found = Algorithms.TryFindFirstWhere(coll2, delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);
        }


        [Test]
        public void TryFindLastWhere1()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[] { 4, 8, 1, 3, 6, 9 });
            bool found;
            int result;

            found = Algorithms.TryFindLastWhere(coll1, delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(9, result);

            found = Algorithms.TryFindLastWhere(coll1, delegate(int x) { return (x & 1) == 0; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(6, result);

            found = Algorithms.TryFindLastWhere(coll1, delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            IEnumerable<int> coll2 = EnumerableFromArray(new int[] { });
            found = Algorithms.TryFindLastWhere(coll2, delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TryFindLastWhere2()
        {
            IList<int> list1 = new List<int>(new int[] { 4, 8, 1, 3, 6, 9 });
            bool found;
            int result;

            found = Algorithms.TryFindLastWhere(list1, delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(9, result);

            found = Algorithms.TryFindLastWhere(list1, delegate(int x) { return (x & 1) == 0; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(6, result);

            found = Algorithms.TryFindLastWhere(list1, delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            IList<int> list2 = new List<int>(new int[] { });
            found = Algorithms.TryFindLastWhere(list2, delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindLastWhere1()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[] { 4, 8, 1, 3, 6, 9 });
            int result;

            result = Algorithms.FindLastWhere(coll1, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(9, result);

            result = Algorithms.FindLastWhere(coll1, delegate(int x) { return (x & 1) == 0; });
            Assert.AreEqual(6, result);

            result = Algorithms.FindLastWhere(coll1, delegate(int x) { return x > 10; });
            Assert.AreEqual(0, result);

            IEnumerable<int> coll2 = EnumerableFromArray(new int[] { });
            result = Algorithms.FindLastWhere(coll2, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindLastWhere2()
        {
            IList<int> list1 = new List<int>(new int[] { 4, 8, 1, 3, 6, 9 });
            int result;

            result = Algorithms.FindLastWhere(list1, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(9, result);

            result = Algorithms.FindLastWhere(list1, delegate(int x) { return (x & 1) == 0; });
            Assert.AreEqual(6, result);

            result = Algorithms.FindLastWhere(list1, delegate(int x) { return x > 10; });
            Assert.AreEqual(0, result);

            IList<int> list2 = new List<int>(new int[] { });
            result = Algorithms.FindLastWhere(list2, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindAll2()
        {
            IEnumerable<int> coll1 = EnumerableFromArray(new int[] { 4, 8, 1, 3, 6, 9 });
            IEnumerable<int> found;

            found = Algorithms.FindWhere(coll1, delegate(int x) { return (x & 1) == 1; });
            InterfaceTests.TestEnumerableElements(found, new int[] { 1, 3, 9 });

            found = Algorithms.FindWhere(coll1, delegate(int x) { return (x & 1) == 0; });
            InterfaceTests.TestEnumerableElements(found, new int[] { 4, 8, 6 });

            found = Algorithms.FindWhere(coll1, delegate(int x) { return x > 10; });
            InterfaceTests.TestEnumerableElements(found, new int[] {  });

            found = Algorithms.FindWhere(coll1, delegate(int x) { return x < 10; });
            InterfaceTests.TestEnumerableElements(found, new int[] { 4, 8, 1, 3, 6, 9 });

            IEnumerable<int> coll2 = EnumerableFromArray(new int[] { });
            found = Algorithms.FindWhere(coll2, delegate(int x) { return (x & 1) == 1; });
            InterfaceTests.TestEnumerableElements(found, new int[] { });
        }

        [Test]
        public void FindFirstIndexWhere()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 3, 4, 9 });
            int index;

            index = Algorithms.FindFirstIndexWhere(coll1, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(2, index);

            index = Algorithms.FindFirstIndexWhere(coll1, delegate(int x) { return (x & 1) == 0; });
            Assert.AreEqual(0, index);

            index = Algorithms.FindFirstIndexWhere(coll1, delegate(int x) { return x > 10; });
            Assert.AreEqual(-1, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.FindFirstIndexWhere(coll2, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(-1, index);
        }


        [Test]
        public void FindLastIndexWhere()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 3, 6, 9 });
            int index;

            index = Algorithms.FindLastIndexWhere(coll1, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(5, index);

            index = Algorithms.FindLastIndexWhere(coll1, delegate(int x) { return (x & 1) == 0; });
            Assert.AreEqual(4, index);

            index = Algorithms.FindLastIndexWhere(coll1, delegate(int x) { return x > 10; });
            Assert.AreEqual(-1, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.FindLastIndexWhere(coll2, delegate(int x) { return (x & 1) == 1; });
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void FindIndicesWhere()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 3, 6, 9 });
            IEnumerable<int> result;

            result = Algorithms.FindIndicesWhere(coll1, delegate(int x) { return (x & 1) == 1; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 2, 3, 5 });

            result = Algorithms.FindIndicesWhere(coll1, delegate(int x) { return (x & 1) == 0; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 1, 4});

            result = Algorithms.FindIndicesWhere(coll1, delegate(int x) { return x < 10; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 1, 2, 3, 4, 5 });

            result = Algorithms.FindIndicesWhere(coll1, delegate(int x) { return x == 6; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 4 });

            result = Algorithms.FindIndicesWhere(coll1, delegate(int x) { return x > 10; });
            InterfaceTests.TestEnumerableElements(result, new int[] {  });

            IList<int> coll2 = new BigList<int>();
            result = Algorithms.FindIndicesWhere(coll2, delegate(int x) { return (x & 1) == 1; });
            InterfaceTests.TestEnumerableElements(result, new int[] { });
        }

        [Test]
        public void FirstIndexOf()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            int index;

            index = Algorithms.FirstIndexOf(coll1, 1);
            Assert.AreEqual(2, index);

            index = Algorithms.FirstIndexOf(coll1, 4);
            Assert.AreEqual(0, index);

            index = Algorithms.FirstIndexOf(coll1, 9);
            Assert.AreEqual(5, index);

            index = Algorithms.FirstIndexOf(coll1, 11);
            Assert.AreEqual(-1, index);

            index = Algorithms.FirstIndexOf(coll1, 6, new Mod2EqualityComparer());
            Assert.AreEqual(0, index);

            index = Algorithms.FirstIndexOf(coll1,11, new Mod2EqualityComparer());
            Assert.AreEqual(2, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.FirstIndexOf(coll2, 0);
            Assert.AreEqual(-1, index);
        }


        [Test]
        public void LastIndexOf()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            int index;

            index = Algorithms.LastIndexOf(coll1, 1);
            Assert.AreEqual(3, index);

            index = Algorithms.LastIndexOf(coll1, 4);
            Assert.AreEqual(4, index);

            index = Algorithms.LastIndexOf(coll1, 9);
            Assert.AreEqual(5, index);

            index = Algorithms.LastIndexOf(coll1, 11);
            Assert.AreEqual(-1, index);

            index = Algorithms.LastIndexOf(coll1, 6, new Mod2EqualityComparer());
            Assert.AreEqual(4, index);

            index = Algorithms.LastIndexOf(coll1, 11, new Mod2EqualityComparer());
            Assert.AreEqual(5, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.LastIndexOf(coll2, 0);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void IndicesOf()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            IEnumerable<int> result;

            result = Algorithms.IndicesOf(coll1, 1);
            InterfaceTests.TestEnumerableElements(result, new int[] { 2, 3 });

            result = Algorithms.IndicesOf(coll1, 4);
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 4 });

            result = Algorithms.IndicesOf(coll1, 9);
            InterfaceTests.TestEnumerableElements(result, new int[] { 5 });

            result = Algorithms.IndicesOf(coll1, 8);
            InterfaceTests.TestEnumerableElements(result, new int[] { 1 });

            result = Algorithms.IndicesOf(coll1, 11);
            InterfaceTests.TestEnumerableElements(result, new int[] { });

            result = Algorithms.IndicesOf(coll1, 11, new Mod2EqualityComparer());
            InterfaceTests.TestEnumerableElements(result, new int[] { 2, 3, 5 });


            IList<int> coll2 = new BigList<int>();
            result = Algorithms.IndicesOf(coll2, 0);
            InterfaceTests.TestEnumerableElements(result, new int[] { });
        }

        [Test]
        public void FirstOneOf1()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            int index;

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 4, 1 }));
            Assert.AreEqual(0, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 1, 8,9 ,  }));
            Assert.AreEqual(1, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 11, 17, 1, 9 }));
            Assert.AreEqual(2, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 9 }));
            Assert.AreEqual(5, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 9 }));
            Assert.AreEqual(5, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 11 }));
            Assert.AreEqual(-1, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { }));
            Assert.AreEqual(-1, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 11, 14 }), new Mod2EqualityComparer());
            Assert.AreEqual(0, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 11, 13, 15, 19, -1 }), new Mod2EqualityComparer());
            Assert.AreEqual(2, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.FirstIndexOfMany(coll2, EnumerableFromArray(new int[] { 3, 7, 9 }));
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void LastOneOf1()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            int index;

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 4, 1 }));
            Assert.AreEqual(4, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 1, 8 }));
            Assert.AreEqual(3, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 11, 17, 1, 9 }));
            Assert.AreEqual(5, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 1 }));
            Assert.AreEqual(3, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 8 }));
            Assert.AreEqual(1, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 11 }));
            Assert.AreEqual(-1, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { }));
            Assert.AreEqual(-1, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 11, 14 }), new Mod2EqualityComparer());
            Assert.AreEqual(5, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 12, -2, 18, 104 }), new Mod2EqualityComparer());
            Assert.AreEqual(4, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.LastIndexOfMany(coll2, EnumerableFromArray(new int[] { 3, 7, 9 }));
            Assert.AreEqual(-1, index);
        }


        [Test]
        public void AllOneOf1()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            IEnumerable<int> result;

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 4, 1 }));
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 2, 3, 4 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 1, 8 }));
            InterfaceTests.TestEnumerableElements(result, new int[] { 1, 2, 3});

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 11, 17, 1, 9 }));
            InterfaceTests.TestEnumerableElements(result, new int[] { 2, 3, 5 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 1 }));
            InterfaceTests.TestEnumerableElements(result, new int[] { 2, 3 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 8 }));
            InterfaceTests.TestEnumerableElements(result, new int[] { 1 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 11 }));
            InterfaceTests.TestEnumerableElements(result, new int[] { });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { }));
            InterfaceTests.TestEnumerableElements(result, new int[] { });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 11, 14 }), new Mod2EqualityComparer());
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 1, 2, 3, 4, 5 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 12, -2, 18, 104 }), new Mod2EqualityComparer());
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 1, 4 });

            IList<int> coll2 = new BigList<int>();
            result = Algorithms.IndicesOfMany(coll2, EnumerableFromArray(new int[] { 3, 7, 9 }));
            InterfaceTests.TestEnumerableElements(result, new int[] {  });
        }


        [Test]
        public void FirstOneOf2()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            int index;

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 6, 11 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(0, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 8, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(2, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 11, 17, 1, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(5, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 10 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(1, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(2, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { 13, 5, 0 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(-1, index);

            index = Algorithms.FirstIndexOfMany(coll1, EnumerableFromArray(new int[] { }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(-1, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.FirstIndexOfMany(coll2, EnumerableFromArray(new int[] { 3, 7, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void LastOneOf2()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            int index;

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 6, 11 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(5, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 8, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(3, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 17, 1, 6 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(4, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 10 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(1, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(5, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { 13, 5, 0 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(-1, index);

            index = Algorithms.LastIndexOfMany(coll1, EnumerableFromArray(new int[] { }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(-1, index);

            IList<int> coll2 = new BigList<int>();
            index = Algorithms.LastIndexOfMany(coll2, EnumerableFromArray(new int[] { 3, 7, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            Assert.AreEqual(-1, index);
        }


        [Test]
        public void AllOneOf2()
        {
            IList<int> coll1 = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9 });
            IEnumerable<int> result;

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 6, 11 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 1, 4, 5 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 3, 8, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 2, 3 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 3, 17, 1, 6 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 0, 1, 2, 3, 4 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 10 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] { 1 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 3, 7, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] {2, 3, 5 });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { 13, 5, 0 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] {  });

            result = Algorithms.IndicesOfMany(coll1, EnumerableFromArray(new int[] { }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] {  });

            IList<int> coll2 = new BigList<int>();
            result = Algorithms.IndicesOfMany(coll2, EnumerableFromArray(new int[] { 3, 7, 9 }), delegate(int x, int y) { return Math.Abs(x - y) == 2; });
            InterfaceTests.TestEnumerableElements(result, new int[] {  });
        }

        [Test]
        public void SearchForSubsequence()
        {
            IList<char> list1 = new List<char>(Algorithms.TypedAs<char>("banaramamississippiabacabqrstrst"));
            int index;

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("issip"));
            Assert.AreEqual(12, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("b"));
            Assert.AreEqual(0, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("banara"));
            Assert.AreEqual(0, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("banaramamississippiabacabqrstrst"));
            Assert.AreEqual(0, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("acab"));
            Assert.AreEqual(21, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("mississippi"));
            Assert.AreEqual(8, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("strstr"));
            Assert.AreEqual(-1, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("banana"));
            Assert.AreEqual(-1, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>("x"));
            Assert.AreEqual(-1, index);

            index = Algorithms.SearchForSubsequence(list1, Algorithms.TypedAs<char>(""));
            Assert.AreEqual(0, index);

            IList<char> list2 = new List<char>(Algorithms.TypedAs<char>("abababacaba"));
            index = Algorithms.SearchForSubsequence(list2, Algorithms.TypedAs<char>("ababacab"));
            Assert.AreEqual(2, index);

            index = Algorithms.SearchForSubsequence(list2, Algorithms.TypedAs<char>("babacaba"));
            Assert.AreEqual(3, index);

            IList<string> list3 = new List<string>(new string[] { "foo", "BAR", "FOO", "bar", "foO", "baR", "bAZ", "FOO", "BAR" });
            index = Algorithms.SearchForSubsequence(list3, EnumerableFromArray(new string[] { "bar", "foO", "baR" }));
            Assert.AreEqual(3, index);

            index = Algorithms.SearchForSubsequence(list3, EnumerableFromArray(new string[] { "bar", "foO", "baR" }), StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(1, index);

            index = Algorithms.SearchForSubsequence(list3, EnumerableFromArray(new string[] { "fat", "big", "bad", "Fiddle" }), delegate(string x, string y) { return x[0] == y[0]; });
            Assert.AreEqual(4, index);

            IList<int> list4 = new List<int>();
            index = Algorithms.SearchForSubsequence(list4, EnumerableFromArray(new int[] { 3, 4, 5 }));
            Assert.AreEqual(-1, index);

            index = Algorithms.SearchForSubsequence(list4, EnumerableFromArray(new int[] { }));
            Assert.AreEqual(0, index);

        }

        [Test]
        public void ReverseComparer()
        {
            IComparer<int> comparer = Algorithms.GetReverseComparer(new GOddEvenComparer());

            Assert.IsTrue(comparer.Compare(7, 6) > 0);
            Assert.IsTrue(comparer.Compare(7, 8) > 0);
            Assert.IsTrue(comparer.Compare(12, 11) < 0);
            Assert.IsTrue(comparer.Compare(12, 143) < 0);
            Assert.IsTrue(comparer.Compare(5, 7) > 0);
            Assert.IsTrue(comparer.Compare(9, 5) < 0);
            Assert.IsTrue(comparer.Compare(6, 8) > 0);
            Assert.IsTrue(comparer.Compare(14, -8) < 0);
            Assert.IsTrue(comparer.Compare(0, 0) == 0);
            Assert.IsTrue(comparer.Compare(-3, -3) == 0);

            IComparer<int> comparer2 = Algorithms.GetReverseComparer(new GOddEvenComparer());
            Assert.IsTrue(comparer.Equals(comparer2));
            Assert.IsTrue(comparer.GetHashCode() == comparer2.GetHashCode());
        }

        [Test]
        public void ReverseComparison()
        {
            Comparison<int> comparison = Algorithms.GetReverseComparison<int>(ComparersTests.CompareOddEven);

            Assert.IsTrue(comparison(7, 6) > 0);
            Assert.IsTrue(comparison(7, 8) > 0);
            Assert.IsTrue(comparison(12, 11) < 0);
            Assert.IsTrue(comparison(12, 143) < 0);
            Assert.IsTrue(comparison(5, 7) > 0);
            Assert.IsTrue(comparison(9, 5) < 0);
            Assert.IsTrue(comparison(6, 8) > 0);
            Assert.IsTrue(comparison(14, -8) < 0);
            Assert.IsTrue(comparison(0, 0) == 0);
            Assert.IsTrue(comparison(-3, -3) == 0);
        }

        [Test]
        public void Sort()
        {
            IEnumerable<int> enum1 = EnumerableFromArray<int>(new int[] { 8, 1, 5, 2, 4, 1, 10, -5, 3, 1, 8, 7, 12, -5, 2});
            IEnumerable<int> result1 = Algorithms.Sort(enum1);
            InterfaceTests.TestEnumerableElements(enum1, new int[] { 8, 1, 5, 2, 4, 1, 10, -5, 3, 1, 8, 7, 12, -5, 2 });
            InterfaceTests.TestEnumerableElements(result1, new int[] { -5, -5, 1, 1, 1, 2, 2, 3, 4, 5, 7, 8, 8, 10, 12});

            IEnumerable<int> enum2 = new List<int>(new int[] { 8, 1, 5, 2, 4, 1, 10, -5, 3, 1, 8, 7, 12, -5, 2 });
            IEnumerable<int> result2 = Algorithms.Sort(enum2);
            InterfaceTests.TestEnumerableElements(enum2, new int[] { 8, 1, 5, 2, 4, 1, 10, -5, 3, 1, 8, 7, 12, -5, 2 });
            InterfaceTests.TestEnumerableElements(result2, new int[] { -5, -5, 1, 1, 1, 2, 2, 3, 4, 5, 7, 8, 8, 10, 12 });

            IEnumerable<int> enum3 = new List<int>(new int[] {  });
            IEnumerable<int> result3 = Algorithms.Sort(enum3);
            InterfaceTests.TestEnumerableElements(result3, new int[] { });

            IEnumerable<string> enum4 = EnumerableFromArray<string>(new string[] { "foo", "A", "l", "missy", "Fiddle", "diddle", "xxx", "YYY" });
            IEnumerable<string> result4 = Algorithms.Sort(enum4, StringComparer.Ordinal);
            InterfaceTests.TestEnumerableElements(result4, new string[] { "A", "Fiddle", "YYY", "diddle", "foo", "l", "missy","xxx"});
            result4 = Algorithms.Sort(enum4, Algorithms.GetReverseComparer(StringComparer.InvariantCultureIgnoreCase));
            InterfaceTests.TestEnumerableElements(result4, new string[] { "YYY", "xxx", "missy", "l", "foo", "Fiddle", "diddle", "A"});

            IEnumerable<double> enum5 = EnumerableFromArray<double>(new double[] { 4.2, -8.7, 1, 0, 1, -6.7, -3, 2.8, 9, -7.1, 7.2 });
            IEnumerable<double> result5 = Algorithms.Sort(enum5, delegate(double x, double y) {
                x = Math.Abs(x); y = Math.Abs(y);
                if (x < y)
                    return -1;
                else if (x > y)
                    return 1;
                else
                    return 0;
            });
            InterfaceTests.TestEnumerableElements(result5, new double[] { 0, 1, 1, 2.8, -3, 4.2, -6.7, -7.1, 7.2, -8.7, 9 });
        }

        [Test]
        public void MergeSorted()
        {
            IEnumerable<int> enum1 = EnumerableFromArray(new int[] { 3, 6, 7, 7, 8, 10, 17, 18 });
            IEnumerable<int> enum2 = EnumerableFromArray(new int[] { 7, 9, 17, 18, 19, 27 });
            IEnumerable<int> enum3 = EnumerableFromArray(new int[] { 2, 6, 15, 29 });
            IEnumerable<int> merged = Algorithms.MergeSorted(enum1, enum2, enum3);
            IEnumerable<int> sorted = Algorithms.Sort(Algorithms.Concatenate(enum1, enum2, enum3));
            InterfaceTests.TestEnumerableElements(merged, Algorithms.ToArray(sorted));

            enum1 = EnumerableFromArray(new int[] { 1, 7, 9, 11, 13, 1002 });
            enum2 = EnumerableFromArray(new int[] { });
            enum3 = EnumerableFromArray(new int[] { 15, 17, 19});
            merged = Algorithms.MergeSorted(enum1, enum2, enum3);
            sorted = Algorithms.Sort(Algorithms.Concatenate(enum1, enum2, enum3));
            InterfaceTests.TestEnumerableElements(merged, Algorithms.ToArray(sorted));

            enum1 = EnumerableFromArray(new int[] {  });
            enum2 = EnumerableFromArray(new int[] { });
            enum3 = EnumerableFromArray(new int[] { });
            merged = Algorithms.MergeSorted(enum1, enum2, enum3);
            InterfaceTests.TestEnumerableElements(merged, new int [] {});

            Random rand = new Random(13);
            int[] a1 = new int[rand.Next(1000)], a2 = new int[rand.Next(1000)], a3 = new int[rand.Next(1000)], a4 = new int[rand.Next(1000)];
            for (int i = 0; i < a1.Length; ++i)
                a1[i] = rand.Next(1000);
            for (int i = 0; i < a2.Length; ++i)
                a2[i] = rand.Next(1000);
            for (int i = 0; i < a3.Length; ++i)
                a3[i] = rand.Next(1000);
            for (int i = 0; i < a4.Length; ++i)
                a4[i] = rand.Next(1000);
            enum1 = Algorithms.Sort((IEnumerable<int>)a1);
            enum2 = Algorithms.Sort((IEnumerable<int>)a2);
            enum3 = Algorithms.Sort((IEnumerable<int>)a3);
            IEnumerable<int> enum4 = Algorithms.Sort((IEnumerable<int>)a4);
            merged = Algorithms.MergeSorted(enum1, enum2, enum3, enum4);
            sorted = Algorithms.Sort(Algorithms.Concatenate(enum1, enum2, enum3, enum4));
            InterfaceTests.TestEnumerableElements(merged, Algorithms.ToArray(sorted));

            enum1 = EnumerableFromArray(new int[] { 3, 6, 7, 7, 8, 10, 17, 18 });
            enum2 = EnumerableFromArray(new int[] { 7, 9, 17, 18, 19, 27 });
            merged = Algorithms.MergeSorted(enum1, enum2);
            sorted = Algorithms.Sort(Algorithms.Concatenate(enum1, enum2));
            InterfaceTests.TestEnumerableElements(merged, Algorithms.ToArray(sorted));

            merged = Algorithms.MergeSorted(enum1);
            InterfaceTests.TestEnumerableElements(merged, Algorithms.ToArray(enum1));

            merged = Algorithms.MergeSorted<int>();
            InterfaceTests.TestEnumerableElements(merged, new int[] {});

            a1 = new int[rand.Next(1000)]; a2 = new int[rand.Next(1000)]; a3 = new int[rand.Next(1000)]; a4 = new int[rand.Next(1000)];
            for (int i = 0; i < a1.Length; ++i)
                a1[i] = rand.Next(1000);
            for (int i = 0; i < a2.Length; ++i)
                a2[i] = rand.Next(1000);
            for (int i = 0; i < a3.Length; ++i)
                a3[i] = rand.Next(1000);
            for (int i = 0; i < a4.Length; ++i)
                a4[i] = rand.Next(1000);
            IComparer<int> comp = new GOddEvenComparer();
            enum1 = Algorithms.Sort((IEnumerable<int>)a1, comp);
            enum2 = Algorithms.Sort((IEnumerable<int>)a2, comp);
            enum3 = Algorithms.Sort((IEnumerable<int>)a3, comp);
            enum4 = Algorithms.Sort((IEnumerable<int>)a4, comp);
            merged = Algorithms.MergeSorted(comp, enum1, enum2, enum3, enum4);
            sorted = Algorithms.Sort(Algorithms.Concatenate(enum1, enum2, enum3, enum4), comp);
            InterfaceTests.TestEnumerableElements(merged, Algorithms.ToArray(sorted));

            IEnumerable<string> str1 = EnumerableFromArray<string>(new string[] { "foo", "fiddle", "g1", "igloo"});
            IEnumerable<string> str2 = EnumerableFromArray<string>(new string[] { "fast", "gross", "g3", "horse", "splurge" });
            IEnumerable<string> str3 = EnumerableFromArray<string>(new string[] { "finagle", "gimpy", "hippo", "rascal" });
            IEnumerable<string> mergeStr = Algorithms.MergeSorted<string>(delegate(string x, string y) { if (x[0] < y[0]) return -1; else if (x[0] > y[0]) return 1; else return 0; },
                str1, str2, str3);
            InterfaceTests.TestEnumerableElements(mergeStr, new string[] { "foo", "fiddle", "fast", "finagle", "g1", "gross", "g3", "gimpy", "horse", "hippo", "igloo", "rascal", "splurge" });
        }

        [Test]
        public void SortInPlace1()
        {
            const int SIZE = 1000;
            const int MAX = 750;
            const int ITER = 100;

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                IList<int> list = new BigList<int>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(rand.Next(MAX));
                int[] copy = Algorithms.ToArray(list);

                Algorithms.SortInPlace(list);
                Array.Sort(copy);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }

        [Test]
        public void SortInPlace2()
        {
            const int SIZE = 1000;
            const int ITER = 100;

            Comparison<double> comp = delegate(double x, double y) {
                x = Math.Abs(x);
                y = Math.Abs(y);
                return x.CompareTo(y);
            };

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                IList<double> list = new BigList<double>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(rand.NextDouble() - 0.5);
                double[] copy = Algorithms.ToArray(list);

                Algorithms.SortInPlace(list, comp);
                Array.Sort(copy, comp);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }

        [Test]
        public void SortInPlace3()
        {
            string[] strings = { "foo", "fOo2", "Fuzzle", "FOb", "zander", "Alphabet", "QuiRk" };

            const int SIZE = 17;
            const int ITER = 500;

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                IComparer<string> comp = (iter % 2 == 0) ? StringComparer.InvariantCultureIgnoreCase : StringComparer.Ordinal;
                IList<string> list = new Deque<string>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(strings[rand.Next(strings.Length)]);
                string[] copy = Algorithms.ToArray(list);

                Algorithms.SortInPlace(list, comp);
                Array.Sort(copy, comp);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }


        [Test]
        public void SortInPlace4()
        {
            const int SIZE = 1000;
            const int ITER = 100;

            Comparison<double> comp = delegate(double x, double y) {
                x = Math.Abs(x);
                y = Math.Abs(y);
                return x.CompareTo(y);
            };

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                int size = rand.Next(SIZE);
                double[] array = new double[size];
                for (int i = 0; i < size; ++i)
                    array[i] = rand.NextDouble() - 0.5;
                IList<double> list = new List<double> (array);
                double[] copy = Algorithms.ToArray(list);

                Algorithms.SortInPlace(list, comp);
                Array.Sort(copy, comp);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }

        [Test]
        public void SortInPlace5()
        {
            IList<int> list = new List<int>(new int[] { });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { });

            list = new List<int>(new int[] { 3 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 3 });

            list = new List<int>(new int[] { 1, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2 });

            list = new List<int>(new int[] { 2, 1 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2 });

            list = new List<int>(new int[] { 2, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 2, 2 });

            list = new List<int>(new int[] { 1, 2, 3 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new List<int>(new int[] { 2, 1, 3 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new List<int>(new int[] { 1, 3, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new List<int>(new int[] { 3, 1, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new List<int>(new int[] { 3, 2, 1 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new List<int>(new int[] { 2, 3, 1 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new List<int>(new int[] { 1, 2, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 2 });

            list = new List<int>(new int[] { 2, 1, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 2 });

            list = new List<int>(new int[] { 2, 2, 1 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 2 });

            list = new List<int>(new int[] { 1, 1, 2 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 1, 2 });

            list = new List<int>(new int[] { 2, 1, 1 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 1, 2 });

            list = new List<int>(new int[] { 1, 2, 1 });
            Algorithms.SortInPlace(list);
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 1, 2 });
        }

        [Test]
        public void BinarySearch1()
        {
            const int SIZE = 100;
            const int MAX = 30;
            const int ITER = 1000;

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                IList<int> list = new List<int>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(rand.Next(MAX));
                Algorithms.SortInPlace(list);
                int find = rand.Next(MAX * 5 / 4) - MAX / 8;
                int index, length;

                length = Algorithms.BinarySearch(list, find, out index);

                for (int i = 0; i < length; ++i)
                    Assert.AreEqual(find, list[index + i]);

                // Check that initial bound is OK.
                if (index == 0) {
                    if (length == 0)
                        Assert.IsTrue(size == 0 || list[index] > find);
                    else
                        Assert.IsTrue(list[index] == find);
                }
                else {
                    if (length == 0) {
                        if (index >= size)
                            Assert.IsTrue(index == size && (size == 0 || list[index - 1] < find));
                        else
                            Assert.IsTrue(list[index] > find && list[index - 1] < find);
                    }
                    else
                        Assert.IsTrue(list[index] == find && list[index - 1] < find);
                }

                // Check final bound is OK.
                if (length > 0) {
                    int last = index + length - 1;
                    if (last < size - 1)
                        Assert.IsTrue(list[last] == find && list[last + 1] > find);
                    else
                        Assert.IsTrue(list[last] == find);
                }
            }
        }

        [Test]
        public void BinarySearch2()
        {
            IList<String> list = new List<String>(new String[] { "foo", "Giraffe", "gorge", "HELLO", "hello", "number", "NUMber", "ooze" });
            int length, index;

            length = Algorithms.BinarySearch(list, "GIRAFFE", StringComparer.InvariantCultureIgnoreCase, out index);
            Assert.AreEqual(1, length);
            Assert.AreEqual(1, index);

            length = Algorithms.BinarySearch(list, "hEllo", StringComparer.InvariantCultureIgnoreCase, out index);
            Assert.AreEqual(2, length);
            Assert.AreEqual(3, index);

            length = Algorithms.BinarySearch(list, "OODLE", StringComparer.InvariantCultureIgnoreCase, out index);
            Assert.AreEqual(0, length);
            Assert.AreEqual(7, index);

            length = Algorithms.BinarySearch(list, "zorch", StringComparer.InvariantCultureIgnoreCase, out index);
            Assert.AreEqual(0, length);
            Assert.AreEqual(8, index);

            length = Algorithms.BinarySearch(list, "FOO", StringComparer.InvariantCultureIgnoreCase, out index);
            Assert.AreEqual(1, length);
            Assert.AreEqual(0, index);
        }

        struct KeyWithOrder<T>: IComparable<KeyWithOrder<T>> where T: IComparable<T>
        {
            public T value;
            public int order;

            public KeyWithOrder(T value, int order)
            {
                this.value = value;
                this.order = order;
            }

            public int CompareTo(KeyWithOrder<T> other)
            {
                return value.CompareTo(other.value);
            }

            public bool Equals(KeyWithOrder<T> other)
            {
                return value.Equals(other.value);
            }
        }

        [Test]
        public void StableSort1()
        {
            const int SIZE = 1000;
            const int MAX = 50;
            const int ITER = 100;
            Random rand = new Random(42);
            KeyWithOrder<int>[] array = new KeyWithOrder<int>[SIZE];

            for (int iter = 0; iter < ITER; ++iter) {

                for (int i = 0; i < SIZE; ++i) {
                    array[i] = new KeyWithOrder<int>(rand.Next(MAX), i);
                }

                IEnumerable<KeyWithOrder<int>> en = EnumerableFromArray(array);
                IEnumerable<KeyWithOrder<int>> result = Algorithms.StableSort(en);

                bool atFirst = true;
                KeyWithOrder<int> prev = new KeyWithOrder<int>();
                foreach (KeyWithOrder<int> x in result) {
                    if (!atFirst) {
                        if (x.value == prev.value) {
                            Assert.IsTrue(prev.order < x.order);
                        }
                        else {
                            Assert.IsTrue(prev.value < x.value);
                        }
                    }

                    atFirst = false;
                    prev = x;
                }

                result = Algorithms.StableSort(en, Algorithms.GetReverseComparer(Comparer<KeyWithOrder<int>>.Default));

                atFirst = true;
                foreach (KeyWithOrder<int> x in result) {
                    if (!atFirst) {
                        if (x.value == prev.value) {
                            Assert.IsTrue(prev.order < x.order);
                        }
                        else {
                            Assert.IsTrue(prev.value > x.value);
                        }
                    }

                    atFirst = false;
                    prev = x;
                }
            }
        }

        [Test]
        public void StableSortInPlace()
        {
            const int SIZE = 1000;
            const int MAX = 50;
            const int ITER = 100;
            Random rand = new Random(42);
            KeyWithOrder<int>[] array = new KeyWithOrder<int>[SIZE];

            for (int iter = 0; iter < ITER; ++iter) {

                for (int i = 0; i < SIZE; ++i) {
                    array[i] = new KeyWithOrder<int>(rand.Next(MAX), i);
                }

                Algorithms.StableSortInPlace<KeyWithOrder<int>>(array);

                bool atFirst = true;
                KeyWithOrder<int> prev = new KeyWithOrder<int>();
                foreach (KeyWithOrder<int> x in array) {
                    if (!atFirst) {
                        if (x.value == prev.value) {
                            Assert.IsTrue(prev.order < x.order);
                        }
                        else {
                            Assert.IsTrue(prev.value < x.value);
                        }
                    }

                    atFirst = false;
                    prev = x;
                }

                for (int i = 0; i < SIZE; ++i) {
                    array[i] = new KeyWithOrder<int>(rand.Next(MAX), i);
                }

                Algorithms.StableSortInPlace<KeyWithOrder<int>>(array, Algorithms.GetReverseComparer(Comparer<KeyWithOrder<int>>.Default));

                atFirst = true;
                foreach (KeyWithOrder<int> x in array) {
                    if (!atFirst) {
                        if (x.value == prev.value) {
                            Assert.IsTrue(prev.order < x.order);
                        }
                        else {
                            Assert.IsTrue(prev.value > x.value);
                        }
                    }

                    atFirst = false;
                    prev = x;
                }
            }
        }

        [Test]
        public void GetIdentityComparer()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("h"); builder.Append("e"); builder.Append("l"); builder.Append("l"); builder.Append("o");

            string hello1 = builder.ToString();
            string hello2 = "hello";
            string hello3 = hello2;
            Assert.IsTrue(hello1 == hello2);
            Assert.IsTrue(hello1.Equals(hello2));
            Assert.IsTrue(hello1.GetHashCode() == hello2.GetHashCode());
            Assert.IsFalse((object)hello1 == (object)hello2);

            IEqualityComparer<string> comparer = Algorithms.GetIdentityComparer<string>();

            Assert.IsFalse(comparer.Equals(hello1,hello2));
            Assert.IsFalse(comparer.GetHashCode(hello1) == comparer.GetHashCode(hello2));
            Assert.IsTrue(comparer.Equals(hello3,hello2));
            Assert.IsTrue(comparer.GetHashCode(hello3) == comparer.GetHashCode(hello2));

            IEqualityComparer<string> comparer2 = Algorithms.GetIdentityComparer<string>();
            Assert.IsTrue(comparer2.Equals(comparer));
        }

        private static int CompareOddEven(int e1, int e2)
        {
            if ((e1 & 1) == 1 && (e2 & 1) == 0)
                return -1;
            else if ((e1 & 1) == 0 && (e2 & 1) == 1)
                return 1;
            else if (e1 < e2)
                return -1;
            else if (e1 > e2)
                return 1;
            else
                return 0;
        }

        [Test]
        public void GetComparerFromComparison()
        {
            IComparer<int> comparer = Algorithms.GetComparerFromComparison<int>(CompareOddEven);

            Assert.IsTrue(comparer.Compare(7, 6) < 0);
            Assert.IsTrue(comparer.Compare(7, 8) < 0);
            Assert.IsTrue(comparer.Compare(12, 11) > 0);
            Assert.IsTrue(comparer.Compare(12, 143) > 0);
            Assert.IsTrue(comparer.Compare(5, 7) < 0);
            Assert.IsTrue(comparer.Compare(9, 5) > 0);
            Assert.IsTrue(comparer.Compare(6, 8) < 0);
            Assert.IsTrue(comparer.Compare(14, -8) > 0);
            Assert.IsTrue(comparer.Compare(0, 0) == 0);
            Assert.IsTrue(comparer.Compare(-3, -3) == 0);
        }

        private class OddEvenComparer : IComparer<int>
        {
            public int Compare(int e1, int e2)
            {
                if ((e1 & 1) == 1 && (e2 & 1) == 0)
                    return -1;
                else if ((e1 & 1) == 0 && (e2 & 1) == 1)
                    return 1;
                else if (e1 < e2)
                    return -1;
                else if (e1 > e2)
                    return 1;
                else
                    return 0;
            }
        }

        [Test]
        public void GetComparisonFromComparer()
        {
            Comparison<int> comparison = Algorithms.GetComparisonFromComparer<int>(new OddEvenComparer());

            Assert.IsTrue(comparison(7, 6) < 0);
            Assert.IsTrue(comparison(7, 8) < 0);
            Assert.IsTrue(comparison(12, 11) > 0);
            Assert.IsTrue(comparison(12, 143) > 0);
            Assert.IsTrue(comparison(5, 7) < 0);
            Assert.IsTrue(comparison(9, 5) > 0);
            Assert.IsTrue(comparison(6, 8) < 0);
            Assert.IsTrue(comparison(14, -8) > 0);
            Assert.IsTrue(comparison(0, 0) == 0);
            Assert.IsTrue(comparison(-3, -3) == 0);
        }

        [Test]
        public void GetDictionaryConverter()
        {
            OrderedDictionary<string, int> dict1 = new OrderedDictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            dict1["foo"] = 7;
            dict1["bar"] = 11;
            dict1["HELLO"] = 18;

            IEnumerable<string> coll1 = EnumerableFromArray<string>(new string[] { "FOO", "hi", null, "bar", "hello", "7" });

            IEnumerable<int> result1 = Algorithms.Convert(coll1, Algorithms.GetDictionaryConverter(dict1));
            InterfaceTests.TestEnumerableElements(result1, new int[] { 7, 0, 0, 11, 18, 0 });

            IEnumerable<int> result2 = Algorithms.Convert(coll1, Algorithms.GetDictionaryConverter(dict1, -42));
            InterfaceTests.TestEnumerableElements(result2, new int[] { 7, -42, -42, 11, 18, -42 });
        }

        [Test]
        public void ToStringEnumerable()
        {
            string[] array = { "Hello", "Goodbye", null, "Clapton", "Rules" };
            string s;

            Assert.AreEqual("null", Algorithms.ToString<decimal>(null));
            Assert.AreEqual("null", Algorithms.ToString<decimal>(null, true, "[[", "; ", "]]"));

            IEnumerable<string> coll1 = EnumerableFromArray<string>(array);
            s = Algorithms.ToString(coll1);
            Assert.AreEqual("{Hello,Goodbye,null,Clapton,Rules}", s);

            IEnumerable<string> coll2 = EnumerableFromArray<string>(array);
            s = Algorithms.ToString(coll2, true, "[[", "; ", "]]");
            Assert.AreEqual("[[Hello; Goodbye; null; Clapton; Rules]]", s);

            IEnumerable<string> coll3 = EnumerableFromArray<string>(new string[0]);
            s = Algorithms.ToString(coll3);
            Assert.AreEqual("{}", s);

            IEnumerable<string> coll4 = EnumerableFromArray<string>(new string[0]);
            s = Algorithms.ToString(coll4, false, "[[", "; ", "]]");
            Assert.AreEqual("[[]]", s);

            IEnumerable<int> coll5 = EnumerableFromArray<int>(new int[] { 1, 2, 3 });
            s = Algorithms.ToString(coll5);
            Assert.AreEqual("{1,2,3}", s);

            IEnumerable<int> coll6 = EnumerableFromArray<int>(new int[] { 1, 2, 3 });
            s = Algorithms.ToString(coll6, true, "[[", "; ", "]]");
            Assert.AreEqual("[[1; 2; 3]]", s);

            IEnumerable<object> coll7 = EnumerableFromArray<object>(new object[] {
                "hello", 
                8,
                EnumerableFromArray<int>(new int[] {1,2,3}),
                -8.9});
            s = Algorithms.ToString(coll7);
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.AreEqual(s, string.Format("{{hello,8,{{1,2,3}},-8{0}9}}", separator));
            s = Algorithms.ToString(coll7, true, "[[", "; ", "]]");
            Assert.AreEqual(s, String.Format("[[hello; 8; [[1; 2; 3]]; -8{0}9]]", separator));
            s = Algorithms.ToString(coll7, false, "[[", "; ", "]]");
            Console.WriteLine(s);
            Assert.AreEqual(s, String.Format("[[hello; 8; Wintellect.PowerCollections.Tests.AlgorithmsTests+<EnumerableFromArray>d__0`1[System.Int32]; -8{0}9]]", separator));
        }

        [Test]
        public void ToStringDictionary()
        {
            string[] s_array = { "Eric", "Clapton", null, "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };
            object[] o_array = { 1, "hi", new int[] {4,5}, null, new List<string>(new string[] {"foo", "bar", "baz"}) };
            string s;

            Assert.AreEqual("null", Algorithms.ToString<string,int>(null));

            OrderedDictionary<string, int> dict1 = new OrderedDictionary<string, int>();
            for (int i = 0; i < s_array.Length; ++i) 
                dict1.Add(s_array[i], i_array[i]);

            s = Algorithms.ToString(dict1);
            Assert.AreEqual(s, "{null->6, Clapton->5, Eric->1, The->5, World->19}", s);

            OrderedDictionary<string, object> dict2 = new OrderedDictionary<string, object>();
            for (int i = 0; i < s_array.Length; ++i) 
                dict2.Add(s_array[i], o_array[i]);

            s = Algorithms.ToString(dict2);
            Assert.AreEqual(s, "{null->{4,5}, Clapton->hi, Eric->1, The->null, World->{foo,bar,baz}}", s);
        }

        [Test]
        public void ReadWriteList()
        {
            int[] array1 = { 3, 1, -9, 0, 8, 1, 5, 11 };
            IList<int> list1 = Algorithms.ReadWriteList(array1);

            InterfaceTests.TestEnumerableElements(list1, new int[] { 3, 1, -9, 0, 8, 1, 5, 11 });
            Assert.AreEqual(8, list1.Count);
            for (int i = 0; i < list1.Count; ++i)
                Assert.AreEqual(array1[i], list1[i]);

            // make sure changes to the array are reflected in the list.
            array1[1] = 18;
            array1[7] = 9;
            InterfaceTests.TestEnumerableElements(list1, new int[] { 3, 18, -9, 0, 8, 1, 5, 9 });

            // make sure changes to the list are reflected in the array.
            list1[0] = -1;
            list1[7] = 14;
            list1[4] = 2;
            InterfaceTests.TestEnumerableElements(array1, new int[] { -1, 18, -9, 0, 2, 1, 5, 14 });

            // Test Insert
            list1.Insert(3, 112);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -1, 18, -9, 112, 0, 2, 1, 5});
            InterfaceTests.TestEnumerableElements(list1, new int[] { -1, 18, -9, 112, 0, 2, 1, 5 });

            list1.Insert(0, -6);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -6, -1, 18, -9, 112, 0, 2, 1});
            InterfaceTests.TestEnumerableElements(list1, new int[] { -6, -1, 18, -9, 112, 0, 2, 1});

            list1.Insert(7, 11);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -6, -1, 18, -9, 112, 0, 2, 11 });
            InterfaceTests.TestEnumerableElements(list1, new int[] { -6, -1, 18, -9, 112, 0, 2, 11 });

            list1.Insert(8, 119);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -6, -1, 18, -9, 112, 0, 2, 11 });
            InterfaceTests.TestEnumerableElements(list1, new int[] { -6, -1, 18, -9, 112, 0, 2, 11 });

            // Test RemoveAt
            list1.RemoveAt(7);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -6, -1, 18, -9, 112, 0, 2, 0 });
            InterfaceTests.TestEnumerableElements(list1, new int[] { -6, -1, 18, -9, 112, 0, 2, 0 });

            list1.RemoveAt(2);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -6, -1, -9, 112, 0, 2, 0, 0 });
            InterfaceTests.TestEnumerableElements(list1, new int[] { -6, -1, -9, 112, 0, 2, 0, 0 });

            list1.RemoveAt(0);
            InterfaceTests.TestEnumerableElements(array1, new int[] { -1, -9, 112, 0, 2, 0, 0, 0 });
            InterfaceTests.TestEnumerableElements(list1, new int[] { -1, -9, 112, 0, 2, 0, 0, 0 });

            // Check general list stuff
            array1[5] = 19;
            array1[6] = 2;
            array1[7] = 11;
            InterfaceTests.TestListGeneric<int>(list1, new int[] { -1, -9, 112, 0, 2, 19, 2, 11 });

            // Check properties.
            Assert.IsFalse(list1.IsReadOnly);
            Assert.IsTrue(((IList)list1).IsFixedSize);
        }
    }

    /// <summary>
    /// Wrapper for IList&lt;T&gt; so that no casting to another interface can be done.
    /// </summary>
    class IListWrapper<T> : IList<T>
    {
        private IList<T> wrapped;

        public IListWrapper(IList<T> wrapped)
        {
            this.wrapped = wrapped;
        }

        int IList<T>.IndexOf(T item)
        {
            return wrapped.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            wrapped.Insert(index, item);
        }

        void IList<T>.RemoveAt(int index)
        {
            wrapped.RemoveAt(index);
        }

        T IList<T>.this[int index]
        {
            get
            {
                return wrapped[index];
            }
            set
            {
                wrapped[index] = value;
            }
        }

        void ICollection<T>.Add(T item)
        {
            wrapped.Add(item);
        }

        void ICollection<T>.Clear()
        {
            wrapped.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return wrapped.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            wrapped.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return wrapped.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return wrapped.IsReadOnly; }
        }

        bool ICollection<T>.Remove(T item)
        {
            return wrapped.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return wrapped.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)wrapped).GetEnumerator();
        }
    }

    /// <summary>
    /// Wrapped for ICollection&lt;T&gt; so that no casting to another interface can be done.
    /// </summary>
    class ICollectionWrapper<T> : ICollection<T>
    {
        private ICollection<T> wrapped;

        public ICollectionWrapper(ICollection<T> wrapped)
        {
            this.wrapped = wrapped;
        }

        void ICollection<T>.Add(T item)
        {
            wrapped.Add(item);
        }

        void ICollection<T>.Clear()
        {
            wrapped.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return wrapped.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            wrapped.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return wrapped.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return wrapped.IsReadOnly; }
        }

        bool ICollection<T>.Remove(T item)
        {
            return wrapped.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return wrapped.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)wrapped).GetEnumerator();
        }
    }

    class Mod2EqualityComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y)
        {
            return ((x & 1) == (y & 1)) ;
        }

        public int GetHashCode(int obj)
        {
            return (obj & 1).GetHashCode();
        }
    }

}
