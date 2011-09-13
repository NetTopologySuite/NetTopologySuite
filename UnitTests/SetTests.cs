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
    [TestFixture]
    public class SetTests
    {
        [Test]
        public void RandomAddDelete()
        {
            const int SIZE = 50000;
            bool[] present = new bool[SIZE];
            Random rand = new Random();
            Set<int> set1 = new Set<int>();
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

            int count = 0;
            foreach (bool x in present)
                if (x)
                    ++count;
            Assert.AreEqual(count, set1.Count);

            // Make sure the set has all the correct values, not in order.
            foreach (int v in set1) {
                Assert.IsTrue(present[v]);
                present[v] = false;
            }

            // Make sure all were found.
            count = 0;
            foreach (bool x in present)
                if (x)
                    ++count;
            Assert.AreEqual(0, count);
        }

        [Test]
        public void ICollectionInterface()
        {
            string[] s_array = { "Foo", "Eric", "Clapton", "hello", "goodbye", "C#" };
            Set<string> set1 = new Set<string>();

            foreach (string s in s_array)
                set1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestCollection<string>((ICollection)set1, s_array, false);
        }


        [Test]
        public void GenericICollectionInterface()
        {
            string[] s_array = { "Foo", "Eric", "Clapton", "hello", "goodbye", "C#", "Java" };
            Set<string> set1 = new Set<string>();

            foreach (string s in s_array)
                set1.Add(s);

            Array.Sort(s_array);
            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)set1, s_array, false);
        }

        [Test]
        public void Add()
        {
            Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
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

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new string[] { null, "", "Eric", "foo", "Hello" }, false);
        }

        [Test]
        public void CountAndClear()
        {
            Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);

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
            Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
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
            Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
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
        public void AddMany()
        {
            Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
            set1.Add("foo");
            set1.Add("Eric");
            set1.Add("Clapton");
            string[] s_array = { "FOO", "x", "elmer", "fudd", "Clapton", null };
            set1.AddMany(s_array);

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new string[] { null, "Clapton", "elmer", "Eric", "FOO", "fudd", "x" }, false);
        }

        [Test]
        public void RemoveMany()
        {
            Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);

            set1.Add("foo");
            set1.Add("Eric");
            set1.Add("Clapton");
            set1.Add(null);
            set1.Add("fudd");
            set1.Add("elmer");
            string[] s_array = { "FOO", "jasmine", "eric", null };
            int count = set1.RemoveMany(s_array);
            Assert.AreEqual(3, count);

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new string[] { "Clapton", "elmer", "fudd" }, false);

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
            Set<double> set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsTrue(set1.Exists(delegate(double d) { return d > 100; }));
            Assert.IsTrue(set1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(set1.Exists(delegate(double d) { return d < -10.0; }));
            set1.Clear();
            Assert.IsFalse(set1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void TrueForAll()
        {
            Set<double> set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

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
            Set<double> set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

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
            Set<double> set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });

            set1.RemoveAll(delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new double[] { -0.04, 1.2, 1.78, 4.5 }, false);

            set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            set1.RemoveAll(delegate(double d) { return d == 0; });
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new double[] { -7.6, -0.04, 1.2, 1.78, 4.5, 7.6, 10.11, 187.4 }, false);

            set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            set1.RemoveAll(delegate(double d) { return d < 200; });
            Assert.AreEqual(0, set1.Count);
        }

        [Test]
        public void FindAll()
        {
            Set<double> set1 = new Set<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            double[] expected = { -7.6, 7.6, 10.11, 187.4 };
            int i;

            i = 0;
            foreach (double x in set1.FindAll(delegate(double d) { return Math.Abs(d) > 5; })) {
                int index = Array.IndexOf(expected, x);
                Assert.IsTrue(index >= 0);
                Assert.AreEqual(expected[index], x);
                expected[index] = double.NaN;
                ++i;
            }
            Assert.AreEqual(expected.Length, i);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FailFastEnumerator1()
        {
            Set<double> set1 = new Set<double>();

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
            Set<double> set1 = new Set<double>();

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

        [Test]
        public void Clone()
        {
            Set<int> set1 = new Set<int>(new int[] { 1, 7, 9, 11, 13, 15, -17, 19, -21 });
            Set<int> set2, set3;

            set2 = set1.Clone();
            set3 = (Set<int>)((ICloneable)set1).Clone();

            Assert.IsFalse(set2 == set1);
            Assert.IsFalse(set3 == set1);

            // Modify set1, make sure set2, set3 don't change.
            set1.Remove(9);
            set1.Remove(-17);
            set1.Add(8);

            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { -21, -17, 1, 7, 9, 11, 13, 15, 19 }, false);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { -21, -17, 1, 7, 9, 11, 13, 15, 19 }, false);

            set1 = new Set<int>();
            set2 = set1.Clone();
            Assert.IsFalse(set2 == set1);
            Assert.IsTrue(set1.Count == 0 && set2.Count == 0);
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

        void CompareClones<T>(Set<T> s1, Set<T> s2)
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
                Assert.AreEqual(1, found);
            }
        }



        [Test]
        public void CloneContents()
        {
            Set<MyInt> set1 = new Set<MyInt>();

            set1.Add(new MyInt(143));
            set1.Add(new MyInt(2));
            set1.Add(new MyInt(9));
            set1.Add(null);
            set1.Add(new MyInt(14));
            set1.Add(new MyInt(111));
            Set<MyInt> set2 = set1.CloneContents();
            CompareClones(set1, set2);

            Set<int> set3 = new Set<int>(new int[] { 144, 5, 23, 1, 8 });
            Set<int> set4 = set3.CloneContents();
            CompareClones(set3, set4);

            Set<UtilTests.CloneableStruct> set5 = new Set<UtilTests.CloneableStruct>();
            set5.Add(new UtilTests.CloneableStruct(143));
            set5.Add(new UtilTests.CloneableStruct(5));
            set5.Add(new UtilTests.CloneableStruct(23));
            set5.Add(new UtilTests.CloneableStruct(1));
            set5.Add(new UtilTests.CloneableStruct(8));
            Set<UtilTests.CloneableStruct> set6 = set5.CloneContents();

            Assert.AreEqual(set5.Count, set6.Count);

            // Check that the sets are equal, but not identical (e.g., have been cloned via ICloneable).
            foreach (UtilTests.CloneableStruct item in set5) {
                int found = 0;
                foreach (UtilTests.CloneableStruct other in set6) {
                    if (object.Equals(item, other)) {
                        found += 1;
                        Assert.IsFalse(item.Identical(other));
                    }
                }
                Assert.AreEqual(1, found);
            }

        }

        class NotCloneable { }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CantCloneContents()
        {
            Set<NotCloneable> set1 = new Set<NotCloneable>();

            set1.Add(new NotCloneable());
            set1.Add(new NotCloneable());

            Set<NotCloneable> set2 = set1.CloneContents();
        }

        // Strange comparer that uses modulo arithmetic.
        class ModularComparer: IEqualityComparer<int>
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
            Set<int> set1 = new Set<int>(new ModularComparer(5));
            bool b;

            b = set1.Add(4); Assert.IsFalse(b);
            b = set1.Add(11); Assert.IsFalse(b);
            b = set1.Add(9); Assert.IsTrue(b);
            b = set1.Add(15); Assert.IsFalse(b);

            Assert.IsTrue(set1.Contains(25));
            Assert.IsTrue(set1.Contains(26));
            Assert.IsFalse(set1.Contains(27));

            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 11, 9, 15 }, false);
        }

        [Test]
        public void ComparerProperty()
        {
            IEqualityComparer<int> comparer1 = new ModularComparer(5);
            Set<int> set1 = new Set<int>(comparer1);
            Assert.AreSame(comparer1, set1.Comparer);
            Set<decimal> set2 = new Set<decimal>();
            Assert.AreSame(EqualityComparer<decimal>.Default, set2.Comparer);
            Set<string> set3 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
            Assert.AreSame(StringComparer.InvariantCultureIgnoreCase, set3.Comparer);
        }

        // Simple class for testing that the generic IEquatable is used.
        class GenComparable : IEquatable<GenComparable>
        {
            public int value;
            public GenComparable(int value)
            {
                this.value = value;
            }

            public object Clone()
            {
                return new MyInt(value);
            }

            public override bool Equals(object obj)
            {
                throw new NotSupportedException();
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            public override string ToString()
            {
                return value.ToString();
            }
        
            #region IEquatable<GenComparable> Members

            bool IEquatable<GenComparable>.Equals(GenComparable other)
            {
                return this.value == other.value;
            }

            #endregion

}

        // Make sure that IEquatable<T>.Equals is used for equality comparison.
        [Test]
        public void GenericIEquatable()
        {
            Set<GenComparable> set1 = new Set<GenComparable>();
            bool b;

            b = set1.Add(new GenComparable(4)); Assert.IsFalse(b);
            b = set1.Add(new GenComparable(11)); Assert.IsFalse(b);
            b = set1.Add(new GenComparable(4)); Assert.IsTrue(b);
            b = set1.Add(new GenComparable(15)); Assert.IsFalse(b);

            Assert.IsTrue(set1.Contains(new GenComparable(4)));
            Assert.IsTrue(set1.Contains(new GenComparable(15)));
            Assert.IsFalse(set1.Contains(new GenComparable(27)));
        }

        [Test]
        public void Initialize()
        {
            List<int> list = new List<int>(new int[] { 12, 3, 9, 8, 9 });
            Set<int> set1 = new Set<int>(list);
            Set<int> set2 = new Set<int>(list, new ModularComparer(6));

            InterfaceTests.TestReadWriteCollectionGeneric<int>(set1, new int[] { 3, 8, 9, 12 }, false);
            InterfaceTests.TestReadWriteCollectionGeneric<int>(set2, new int[] { 9, 8, 12 }, false);
        }

        [Test]
        public void ToArray()
        {
            string[] s_array = { "Foo", "Eric", "Clapton", "hello", null, "goodbye", "C#" };
            Set<string> set1 = new Set<string>();

            string[] a1 = set1.ToArray();
            Assert.IsNotNull(a1);
            Assert.AreEqual(0, a1.Length);

            foreach (string s in s_array)
                set1.Add(s);
            string[] a2 = set1.ToArray();

            Array.Sort(s_array);
            Array.Sort(a2);

            Assert.AreEqual(s_array.Length, a2.Length);
            for (int i = 0; i < s_array.Length; ++i)
                Assert.AreEqual(s_array[i], a2[i]);
        }

        [Test]
        public void Subset()
        {
            Set<int> set1 = new Set<int>(new int[] { 1, 3, 6, 7, 8, 9, 10 });
            Set<int> set2 = new Set<int>();
            Set<int> set3 = new Set<int>(new int[] { 3, 8, 9 });
            Set<int> set4 = new Set<int>(new int[] { 3, 8, 9 });
            Set<int> set5 = new Set<int>(new int[] { 1, 2, 6, 8, 9, 10 });

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
            Set<int> set1 = new Set<int>(new int[] { 6, 7, 1, 11, 9, 3, 8 });
            Set<int> set2 = new Set<int>();
            Set<int> set3 = new Set<int>();
            Set<int> set4 = new Set<int>(new int[] { 9, 11, 1, 3, 6, 7, 8, 14 });
            Set<int> set5 = new Set<int>(new int[] { 3, 6, 7, 11, 14, 8, 9 });
            Set<int> set6 = new Set<int>(new int[] { 1, 3, 6, 7, 8, 10, 11 });
            Set<int> set7 = new Set<int>(new int[] { 9, 1, 8, 3, 7, 6, 11 });

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
        public void IsDisjointFrom()
        {
            Set<int> set1 = new Set<int>(new int[] { 6, 7, 1, 11, 9, 3, 8 });
            Set<int> set2 = new Set<int>();
            Set<int> set3 = new Set<int>();
            Set<int> set4 = new Set<int>(new int[] { 9, 1, 8, 3, 7, 6, 11 });
            Set<int> set5 = new Set<int>(new int[] { 17, 3, 12, 10 });
            Set<int> set6 = new Set<int>(new int[] { 19, 14, 0, 2});

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
            Set<int> setOdds = new Set<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            Set<int> setDigits = new Set<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            Set<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.IntersectionWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 3, 5, 7, 9 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.IntersectionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 1, 3, 5, 7, 9 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.Intersection(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 3, 5, 7, 9 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.Intersection(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 3, 5, 7, 9 }, false);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.IntersectionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false);

            set1 = setDigits.Clone();
            set3 = set1.Intersection(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false);
        }

        [Test]
        public void Union()
        {
            Set<int> setOdds = new Set<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            Set<int> setDigits = new Set<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            Set<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.UnionWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.UnionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.Union(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.Union(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.UnionWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false);

            set1 = setDigits.Clone();
            set3 = set1.Union(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false);
        }

        [Test]
        public void SymmetricDifference()
        {
            Set<int> setOdds = new Set<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            Set<int> setDigits = new Set<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            Set<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.SymmetricDifferenceWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.SymmetricDifferenceWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.SymmetricDifference(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.SymmetricDifference(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25 }, false);

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
            Set<int> setOdds = new Set<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            Set<int> setDigits = new Set<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            Set<int> set1, set2, set3;

            // Algorithms work different depending on sizes, so try both ways.
            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set1.DifferenceWith(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set1, new int[] { 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set2.DifferenceWith(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set2, new int[] { 2, 4, 6, 8 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set1.Difference(set2);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 11, 13, 15, 17, 19, 21, 23, 25 }, false);

            set1 = setOdds.Clone(); set2 = setDigits.Clone();
            set3 = set2.Difference(set1);
            InterfaceTests.TestReadWriteCollectionGeneric(set3, new int[] { 2, 4, 6, 8 }, false);

            // Make sure intersection with itself works.
            set1 = setDigits.Clone();
            set1.DifferenceWith(set1);
            Assert.AreEqual(0, set1.Count);

            set1 = setDigits.Clone();
            set3 = set1.Difference(set1);
            Assert.AreEqual(0, set3.Count);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons1()
        {
            Set<int> setOdds = new Set<int>(new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 });
            Set<int> setDigits = new Set<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new GOddEvenEqualityComparer());
            setOdds.SymmetricDifferenceWith(setDigits);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InconsistentComparisons2()
        {
            Set<string> set1 = new Set<string>(new string[] { "foo", "Bar" }, StringComparer.CurrentCulture);
            Set<string> set2 = new Set<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            set1.Intersection(set2);
        }

        [Test]
        public void ConsistentComparisons()
        {
            Set<string> set1 = new Set<string>(new string[] { "foo", "Bar" }, StringComparer.InvariantCulture);
            Set<string> set2 = new Set<string>(new string[] { "bada", "bing" }, StringComparer.InvariantCulture);
            set1.Difference(set2);
        }

        [Test]
        public void SerializeStrings()
        {
            Set<string> d = new Set<string>();

            d.Add("foo");
            d.Add("world");
            d.Add("hello");
            d.Add("elvis");
            d.Add("elvis");
            d.Add(null);
            d.Add("cool");
            d.AddMany(new string[] { "1", "2", "3", "4", "5", "6" });
            d.AddMany(new string[] { "7", "8", "9", "10", "11", "12" });

            Set<string> result = (Set<string>)InterfaceTests.SerializeRoundTrip(d);

            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)result, new string[] { "1", "2", "3", "4", "5", "6", "cool", "elvis", "hello", "foo", "world", null, "7", "8", "9", "10", "11", "12" }, false);

        }

        [Serializable]
        class UniqueStuff
        {
            public InterfaceTests.Unique[] objects;
            public Set<InterfaceTests.Unique> set;
        }


        [Test]
        public void SerializeUnique()
        {
            UniqueStuff d = new UniqueStuff(), result = new UniqueStuff();

            d.objects = new InterfaceTests.Unique[] { 
                new InterfaceTests.Unique("1"), new InterfaceTests.Unique("2"), new InterfaceTests.Unique("3"), new InterfaceTests.Unique("4"), new InterfaceTests.Unique("5"), new InterfaceTests.Unique("6"), 
                new InterfaceTests.Unique("cool"), new InterfaceTests.Unique("elvis"), new InterfaceTests.Unique("hello"), new InterfaceTests.Unique("foo"), new InterfaceTests.Unique("world"), new InterfaceTests.Unique("elvis"), new InterfaceTests.Unique(null), null,
                new InterfaceTests.Unique("7"), new InterfaceTests.Unique("8"), new InterfaceTests.Unique("9"), new InterfaceTests.Unique("10"), new InterfaceTests.Unique("11"), new InterfaceTests.Unique("12") };
            d.set = new Set<InterfaceTests.Unique>();

            d.set.Add(d.objects[9]);
            d.set.Add(d.objects[10]);
            d.set.Add(d.objects[8]);
            d.set.Add(d.objects[11]);
            d.set.Add(d.objects[7]);
            d.set.Add(d.objects[12]);
            d.set.Add(d.objects[6]);
            d.set.Add(d.objects[13]);
            d.set.AddMany(new InterfaceTests.Unique[] { d.objects[0], d.objects[1], d.objects[2], d.objects[3], d.objects[4], d.objects[5] });
            d.set.AddMany(new InterfaceTests.Unique[] { d.objects[14], d.objects[15], d.objects[16], d.objects[17], d.objects[18], d.objects[19] });

            result = (UniqueStuff)InterfaceTests.SerializeRoundTrip(d);

            InterfaceTests.TestReadWriteCollectionGeneric < InterfaceTests.Unique>(result.set, result.objects, false);

            for (int i = 0; i < result.objects.Length; ++i) {
                if (result.objects[i] != null)
                    Assert.IsFalse(object.Equals(result.objects[i], d.objects[i]));
            }
        }


    }
}

