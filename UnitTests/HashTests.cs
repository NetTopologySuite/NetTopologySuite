//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Wintellect.PowerCollections.Tests
{
    // A class for testing the "Hash" class.
    [TestFixture]
    public class HashTests
    {
        internal Hash<TestItem> hash;

        internal class DataComparer : System.Collections.Generic.IEqualityComparer<TestItem>
        {
            public bool Equals(TestItem x, TestItem y)
            {
                return string.Equals(x.key, y.key);
            }

            public int GetHashCode(TestItem obj)
            {
                return obj.key.GetHashCode();
            }
        }

        /// <summary>
        /// Insert a key and print/validate the hash.
        /// </summary>
        /// <param name="key"></param>
        private void InsertPrintValidate(string key)
        {
            InsertPrintValidate(key, 0, true);
        }

        private void InsertPrintValidate(string key, int data)
        {
            InsertPrintValidate(key, data, true);
        }

        private void InsertPrintValidate(string key, int data, bool replaceOnDuplicate)
        {
            TestItem oldData;
            hash.Insert(new TestItem(key, data), replaceOnDuplicate, out oldData);
#if DEBUG
            hash.Print();
            hash.Validate();
#endif //DEBUG
        }

        private void InsertPrintValidate(string key, int data, bool replaceOnDuplicate, int expectedoldData)
        {
            TestItem oldData;
            hash.Insert(new TestItem(key, data), replaceOnDuplicate, out oldData);
#if DEBUG
            hash.Print();
            hash.Validate();
#endif //DEBUG
            Assert.AreEqual(expectedoldData, oldData.data);
        }

        /// <summary>
        /// Insert a key and validate the hash.
        /// </summary>
        /// <param name="key"></param>
        private void InsertValidate(string key)
        {
            InsertValidate(key, 0, true);
        }

        private void InsertValidate(string key, int data)
        {
            InsertValidate(key, data, true);
        }

        private void InsertValidate(string key, int data, bool replaceOnDuplicate)
        {
            TestItem oldData;
            hash.Insert(new TestItem(key, data), replaceOnDuplicate, out oldData);
#if DEBUG
            hash.Validate();
#endif //DEBUG
        }

        private void InsertValidate(string key, int data, bool replaceOnDuplicate, int expectedOldData)
        {
            TestItem oldData;
            hash.Insert(new TestItem(key, data), replaceOnDuplicate, out oldData);
#if DEBUG
            hash.Validate();
#endif //DEBUG
            Assert.AreEqual(expectedOldData, oldData.data);
        }

        /// <summary>
        /// Delete a key, check the data in the deleted key, print and validate.
        /// </summary>
        /// <param name="key">Key to delete.</param>
        /// <param name="data">Expected data in the deleted key.</param>
        private void DeletePrintValidate(string key, int data)
        {
            TestItem itemFound;
            int countBefore = hash.ElementCount;
            bool success = hash.Delete(new TestItem(key), out itemFound);
#if DEBUG
            hash.Print();
#endif //DEBUG
            Assert.IsTrue(success, "Key to delete wasn't found");
            Assert.AreEqual(data, itemFound.data, "Data in deleted key was incorrect.");
            int countAfter = hash.ElementCount;
            Assert.AreEqual(countBefore - 1, countAfter, "Count of elements incorrect after deletion");
#if DEBUG
            hash.Validate();
#endif //DEBUG
        }

        private void FindKey(string key, int value)
        {
            TestItem itemFound;
            bool found = hash.Find(new TestItem(key), false, out itemFound);
            Assert.IsTrue(found, "Key was not found in the hash");
            Assert.AreEqual(value, itemFound.data, "Wrong value found in the hash");
        }

        private bool FindReplaceKey(string key, int newValue, int expectedOldValue)
        {
            TestItem itemFound;
            bool found = hash.Find(new TestItem(key, newValue), true, out itemFound);
            Assert.AreEqual(expectedOldValue, itemFound.data);
            return found;
        }

        /// <summary>
        /// Test creation of the hash.
        /// </summary>
        [Test]
        public void Create()
        {
            hash = new Hash<TestItem>(new DataComparer());
#if DEBUG
            hash.Print();
            hash.Validate();
#endif //DEBUG
        }

        /// <summary>
        /// Insert values into hash to test the basic insertion algorithm. Validate
        /// and print the hash after each step.
        /// </summary>
        [Test]
        public void NormalInsert()
        {
            hash = new Hash<TestItem>(new DataComparer());

            InsertPrintValidate("m");
            InsertPrintValidate("b");
            InsertPrintValidate("t");
            InsertPrintValidate("o");
            InsertPrintValidate("z");
            InsertPrintValidate("k");
            InsertPrintValidate("g");
            InsertPrintValidate("a5");
            InsertPrintValidate("c");
            InsertPrintValidate("a2");
            InsertPrintValidate("a7");
            InsertPrintValidate("i");
            InsertPrintValidate("h");
            Assert.AreEqual(13, hash.ElementCount, "Wrong number of items in the hash.");
        }

        /// <summary>
        /// Insert values into hash and then find values in the hash.
        /// </summary>
        [Test]
        public void NormalFind()
        {
            hash = new Hash<TestItem>(new DataComparer());

            InsertValidate("m", 101);
            FindKey("m", 101);
            InsertValidate("b", 102);
            InsertValidate("t", 103);
            FindKey("b", 102);
            FindKey("t", 103);
            InsertValidate("o", 104);
            FindKey("b", 102);
            InsertValidate("z", 105);
            InsertValidate("g", 106);
            FindKey("g", 106);
            InsertValidate("a5", 107);
            InsertValidate("c", 8);
            InsertValidate("a2", 9);
            FindKey("z", 105);
            InsertValidate("a7", 10);
            InsertValidate("i", 11);
            InsertValidate("h", 112);
            InsertValidate("k", 113);

            Assert.AreEqual(13, hash.ElementCount, "Wrong number of items in the hash.");

            FindKey("m", 101);
            FindKey("b", 102);
            FindKey("t", 103);
            FindKey("o", 104);
            FindKey("z", 105);
            FindKey("g", 106);
            FindKey("a5", 107);
            FindKey("c", 8);
            FindKey("a2", 9);
            FindKey("a7", 10);
            FindKey("i", 11);
            FindKey("h", 112);
            FindKey("k", 113);
        }
        /// <summary>
        /// Test find with the replace option..
        /// </summary>
        [Test]
        public void FindReplace()
        {
            bool b;
            hash = new Hash<TestItem>(new DataComparer());

            InsertValidate("m", 101);
            FindKey("m", 101);
            InsertValidate("b", 102);
            InsertValidate("t", 103);
            b = FindReplaceKey("b", 202, 102); Assert.IsTrue(b);
            FindKey("t", 103);
            InsertValidate("o", 104);
            FindKey("b", 202);
            InsertValidate("z", 105);
            InsertValidate("g", 106);
            FindKey("g", 106);
            b = FindReplaceKey("a5", 77, 0); Assert.IsFalse(b);
            b = FindReplaceKey("a5", 134, 0); Assert.IsFalse(b);
            b = FindReplaceKey("m", 201, 101); Assert.IsTrue(b);
            InsertValidate("a5", 107);
            InsertValidate("c", 8);
            InsertValidate("k", 313);
            InsertValidate("a2", 9);
            FindKey("z", 105);
            b = FindReplaceKey("m", 301, 201); Assert.IsTrue(b);
            InsertValidate("a7", 10);
            b = FindReplaceKey("a5", 207, 107); Assert.IsTrue(b);
            InsertValidate("i", 11);
            InsertValidate("h", 112);
            b = FindReplaceKey("z", 205, 105); Assert.IsTrue(b);
            b = FindReplaceKey("g", 206, 106); Assert.IsTrue(b);
            b = FindReplaceKey("g", 306, 206); Assert.IsTrue(b);
            b = FindReplaceKey("k", 513, 313);

            Assert.AreEqual(13, hash.ElementCount, "Wrong number of items in the hash.");

            FindKey("m", 301);
            FindKey("b", 202);
            FindKey("t", 103);
            FindKey("o", 104);
            FindKey("z", 205);
            FindKey("g", 306);
            FindKey("a5", 207);
            FindKey("c", 8);
            FindKey("a2", 9);
            FindKey("a7", 10);
            FindKey("i", 11);
            FindKey("h", 112);
            FindKey("k", 513);
        }

        /// <summary>
        /// Insert values into tree using "do-nothing" policy and then find values in the tree.
        /// </summary>
        [Test]
        public void DoNothingFind()
        {
            hash = new Hash<TestItem>(new DataComparer());

            InsertValidate("m", 101, false, 0);
            FindKey("m", 101);
            InsertValidate("b", 102, false, 0);
            InsertValidate("t", 103, false, 0);
            InsertValidate("m", 201, false, 101);
            FindKey("b", 102);
            FindKey("t", 103);
            InsertValidate("o", 104, false, 0);
            FindKey("b", 102);
            InsertValidate("z", 105, false, 0);
            InsertValidate("g", 106, false, 0);
            InsertValidate("b", 202, false, 102);
            FindKey("g", 106);
            InsertValidate("g", 206, false, 106);
            InsertValidate("a5", 107, false, 0);
            InsertValidate("t", 203, false, 103);
            InsertValidate("c", 8, false, 0);
            InsertValidate("a2", 9, false, 0);
            FindKey("z", 105);
            InsertValidate("a7", 10, false, 0);
            InsertValidate("i", 11, false, 0);
            InsertValidate("h", 112, false, 0);
            InsertValidate("z", 205, false, 105);
            InsertValidate("a2", 209, false, 9);
            InsertValidate("c", 208, false, 8);
            InsertValidate("i", 211, false, 11);
            InsertValidate("h", 212, false, 112);
            InsertValidate("k", 113, false, 0);
            InsertValidate("m", 401, false, 101);
            InsertValidate("k", 213, false, 113);

            Assert.AreEqual(13, hash.ElementCount, "Wrong number of items in the tree.");

            FindKey("m", 101);
            FindKey("b", 102);
            FindKey("t", 103);
            FindKey("o", 104);
            FindKey("z", 105);
            FindKey("g", 106);
            FindKey("a5", 107);
            FindKey("c", 8);
            FindKey("a2", 9);
            FindKey("a7", 10);
            FindKey("i", 11);
            FindKey("h", 112);
            FindKey("k", 113);
        }

        /// <summary>
        /// Check that deletion works.
        /// </summary>
        [Test]
        public void Delete()
        {
            hash = new Hash<TestItem>(new DataComparer());

            InsertPrintValidate("m", 101);
            DeletePrintValidate("m", 101);

            InsertPrintValidate("m", 101);
            InsertPrintValidate("b", 102);
            InsertPrintValidate("t", 103);
            DeletePrintValidate("b", 102);
            DeletePrintValidate("m", 101);
            DeletePrintValidate("t", 103);

            InsertPrintValidate("m", 101);
            InsertPrintValidate("b", 102);
            InsertPrintValidate("t", 103);
            InsertPrintValidate("o", 104);
            InsertPrintValidate("z", 105);
            InsertPrintValidate("g", 106);
            InsertPrintValidate("a5", 107);
            InsertPrintValidate("c", 8);
            InsertPrintValidate("a2", 9);
            InsertPrintValidate("a7", 10);
            InsertPrintValidate("i", 11);
            InsertPrintValidate("h", 112);
            InsertPrintValidate("k", 113);

            DeletePrintValidate("m", 101);
            DeletePrintValidate("b", 102);
            DeletePrintValidate("t", 103);
            DeletePrintValidate("o", 104);
            DeletePrintValidate("z", 105);
            DeletePrintValidate("h", 112);
            DeletePrintValidate("g", 106);
            DeletePrintValidate("a5", 107);
            DeletePrintValidate("c", 8);
            DeletePrintValidate("a2", 9);
            DeletePrintValidate("k", 113);
            DeletePrintValidate("a7", 10);
            DeletePrintValidate("i", 11);
        }

        [Test]
        public void DeleteNotPresent()
        {
            int dummy;
            Hash<int> t = new Hash<int>(EqualityComparer<int>.Default);

            t.Insert(3, true, out dummy);
            t.Insert(1, true, out dummy);
            t.Insert(5, true, out dummy);
            t.Insert(3, true, out dummy);
            t.Insert(2, true, out dummy);
            t.Insert(2, true, out dummy);
            t.Insert(3, true, out dummy);
            t.Insert(4, true, out dummy);

            bool b;
            int d;

            b = t.Delete(1, out d);
            Assert.IsTrue(b);
#if DEBUG
            t.Print();
            t.Validate();
#endif //DEBUG

            b = t.Delete(1, out d);
            Assert.IsFalse(b);
#if DEBUG
            t.Print();
            t.Validate();
#endif //DEBUG

            b = t.Delete(int.MinValue, out d);
            Assert.IsFalse(b);
#if DEBUG
            t.Print();
            t.Validate();
#endif //DEBUG

            b = t.Delete(3, out d);
            Assert.IsTrue(b);
#if DEBUG
            t.Print();
            t.Validate();
#endif //DEBUG

            b = t.Delete(3, out d);
            Assert.IsFalse(b);
#if DEBUG
            t.Print();
            t.Validate();
#endif //DEBUG
        }

        /// <summary>
        /// Insert values into tree and enumerate then to test enumeration.
		/// </summary>
        [Test]
        public void Enumerate()
        {
            hash = new Hash<TestItem>(new DataComparer());
            InsertValidate("m");
            InsertValidate("b");
            InsertValidate("t");
            InsertValidate("o");
            InsertValidate("p");
            InsertValidate("g");
            InsertValidate("a5");
            InsertValidate("c");
            InsertValidate("a2");
            InsertValidate("a7");
            InsertValidate("i");
            InsertValidate("h");
            InsertValidate("o");
            InsertValidate("l");
            InsertValidate("k");
            InsertValidate("c");

            string[] keys = new string[] { "a2", "a5", "a7", "b", "c", "g", "h", "i", "k", "l", "m", "o", "p", "t" };
            foreach (TestItem item in hash) {
                int index;
                index = Array.IndexOf(keys, item.key);
                Assert.IsTrue(index >= 0, "key not found in array");
                keys[index] = null;
            }
        }

        const int LENGTH = 500;			// length of each random array of values.
        const int ITERATIONS = 30;		    // number of iterations


        /// <summary>
        /// Create a random array of values.
        /// </summary>
        /// <param name="seed">Seed for random number generators</param>
        /// <param name="length">Length of array</param>
        /// <param name="max">Maximum value of number. Should be much 
        /// greater than length.</param>
        /// <param name="allowDups">Whether to allow duplicate elements.</param>
        /// <returns></returns>
        private int[] CreateRandomArray(int seed, int length, int max, bool allowDups)
        {
            Random rand = new Random(seed);

            int[] a = new int[length];
            for (int i = 0; i < a.Length; ++i)
                a[i] = -1;

            for (int el = 0; el < a.Length; ++el) {
                int value;
                do {
                    value = rand.Next(max);
                } while (!allowDups && Array.IndexOf(a, value) >= 0);
                a[el] = value;
            }

            return a;
        }

        /// <summary>
        /// Insert all the elements of an integer array into the tree. The
        /// values in the tree are the indexes of the array.
        /// </summary>
        /// <param name="a">Array of values to insert.</param>
        private void InsertArray(int[] a)
        {
            TestItem dummy;
            for (int i = 0; i < a.Length; ++i) {
                string s = StringFromInt(a[i]);
                hash.Insert(new TestItem(s, i), true, out dummy);
#if DEBUG
                if (i % 50 == 0)
                    hash.Validate();
#endif //DEBUG
            }
#if DEBUG
            hash.Validate();
#endif //DEBUG
        }

        private string StringFromInt(int i)
        {
            return string.Format("e{0}", i);
        }

        /// <summary>
        /// Insert LENGTH items in random order into the tree and validate
        /// it. Do this ITER times.
        /// </summary>
        [Test]
        public void InsertRandom()
        {
            for (int iter = 0; iter < ITERATIONS; ++iter) {
                hash = new Hash<TestItem>(new DataComparer());
                int[] a = CreateRandomArray(iter, LENGTH, LENGTH * 10, false);
                InsertArray(a);
#if DEBUG
                hash.Validate();
#endif //DEBUG
                Assert.AreEqual(LENGTH, hash.ElementCount, "Wrong number of items in the tree.");
            }
        }

        /// <summary>
        /// Insert LENGTH items in random order into the tree and then find them all
        /// Do this ITER times.
        /// </summary>
        [Test]
        public void FindRandom()
        {
            for (int iter = 0; iter < ITERATIONS; ++iter) {
                hash = new Hash<TestItem>(new DataComparer());
                int[] a = CreateRandomArray(iter + 1000, LENGTH, LENGTH * 10, false);

                InsertArray(a);
#if DEBUG
                hash.Validate();
#endif //DEBUG
                Assert.AreEqual(LENGTH, hash.ElementCount, "Wrong number of items in the hash.");

                for (int el = 0; el < a.Length; ++el) {
                    FindKey(StringFromInt(a[el]), el);
                }
            }
        }


        /// <summary>
        /// Insert LENGTH items in random order into the tree and then enumerate them.
        /// Do this ITER times.
        /// </summary>
        [Test]
        public void EnumerateRandom()
        {
            for (int iter = 0; iter < ITERATIONS / 10; ++iter) {
                hash = new Hash<TestItem>(new DataComparer());
                int[] a = CreateRandomArray(iter + 1000, LENGTH, LENGTH * 10, false);

                InsertArray(a);
#if DEBUG
                hash.Validate();
#endif //DEBUG
                Assert.AreEqual(LENGTH, hash.ElementCount, "Wrong number of items in the hash.");

                foreach (TestItem item in hash) {
                    int index = -1;
                    for (int i = 0; i < a.Length; ++i)
                        if (StringFromInt(a[i]) == item.key)
                            index = i;

                    Assert.IsTrue(index >= 0);
                    Assert.IsTrue(index == item.data);
                    a[index] = -1;
                }
                foreach (int i in a)
                    Assert.AreEqual(-1, i);
            }
        }

        /// <summary>
        /// Insert and delete items from the tree at random, finally removing all
        /// the items that are in the tree. Validate the tree after each step.
        /// </summary>
        [Test]
        public void DeleteRandom()
        {
            for (int iter = 0; iter < ITERATIONS / 10; ++iter) {
                hash = new Hash<TestItem>(new DataComparer());
                bool[] a = new bool[LENGTH];
                int[] value = new int[LENGTH];
                Random rand = new Random(iter + 5000);
                TestItem itemFound;

                for (int i = 0; i < LENGTH * 10; ++i) {
                    int v = rand.Next(LENGTH);
                    string key = StringFromInt(v);
                    if (a[v] && rand.Next(4) != 0) {
                        // Already in the hash. Make sure we can find it, then delete it.
                        bool b = hash.Find(new TestItem(key), false, out itemFound);
                        Assert.IsTrue(b, "Couldn't find key in hash");
                        Assert.AreEqual(value[v], itemFound.data, "Data is incorrect");
                        b = hash.Delete(new TestItem(key), out itemFound);
                        Assert.IsTrue(b, "Couldn't delete key in hash");
                        Assert.AreEqual(value[v], itemFound.data, "Data is incorrect");
#if DEBUG
                        if (i % 50 == 0)
                            hash.Validate();
#endif //DEBUG
                        a[v] = false;
                        value[v] = 0;
                    }
                    else if (i < LENGTH * 7) {
                        // Add it.
                        value[v] = rand.Next(10000) + 1;
                        bool b = hash.Find(new TestItem(key), false, out itemFound);
                        Assert.AreEqual(a[v], b);
                        TestItem dummy;
                        b = hash.Insert(new TestItem(key, value[v]), true, out dummy);
                        Assert.AreEqual(a[v], ! b);
#if DEBUG
                        if (i % 50 == 0)
                            hash.Validate();
#endif //DEBUG
                        a[v] = true;
                    }
                }

                for (int v = 0; v < LENGTH; ++v) {
                    string key = StringFromInt(v);
                    if (a[v]) {
                        // Already in the hash. Make sure we can find it, then delete it.
                        bool b = hash.Find(new TestItem(key), false, out itemFound);
                        Assert.IsTrue(b, "Couldn't find key in hash");
                        Assert.AreEqual(value[v], itemFound.data, "Data is incorrect");
                        b = hash.Delete(new TestItem(key), out itemFound);
                        Assert.IsTrue(b, "Couldn't delete key in hash");
                        Assert.AreEqual(value[v], itemFound.data, "Data is incorrect");
#if DEBUG
                        if (v % 50 == 0)
                            hash.Validate();
#endif //DEBUG
                        a[v] = false;
                    }
                }
            }

#if DEBUG
            hash.Validate();
#endif //DEBUG
        }

        [Test]
        public void Clone()
        {
            hash = new Hash<TestItem>(new DataComparer());

            InsertValidate("foo", 3);
            InsertValidate("bar", 4);
            InsertValidate("bingo", 5);
            InsertValidate("biff", 6);
            InsertValidate("zip", 7);
            InsertValidate("zap", 8);

            Hash<TestItem> clone = hash.Clone(null);
#if DEBUG
            clone.Validate();
#endif //DEBUG

            InsertValidate("a", 51);
            InsertValidate("b", 52);
            InsertValidate("c", 53);
            InsertValidate("d", 54);

#if DEBUG
            clone.Validate();
#endif //DEBUG
            Assert.AreEqual(6, clone.ElementCount);

            string[] s_array = { "bar", "biff", "bingo", "foo", "zap", "zip" };
            int i = 0;
            foreach (TestItem item in clone) {
                int index = Array.IndexOf(s_array, item.key);
                Assert.IsTrue(index >= 0);
                Assert.AreEqual(s_array[index], item.key);
                s_array[index] = null;
                ++i;
            }
            Assert.AreEqual(6, i);

            hash = new Hash<TestItem>(new DataComparer());
            clone = hash.Clone(null);
            Assert.IsTrue(hash.ElementCount == 0 && clone.ElementCount == 0);
#if DEBUG
            clone.Validate();
#endif //DEBUG
        }

        [Test]
        public void GrowShrink()
        {
            Hash<double> hash1 = new Hash<double>(EqualityComparer<double>.Default);
            double dummy;

            Random r = new Random(13);

            for (int i = 0; i < 1000; ++i) {
                bool b = hash1.Insert(r.NextDouble(), true, out dummy);
                Assert.IsTrue(b);
            }

#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 2048);

            r = new Random(13);

            for (int i = 0; i < 600; ++i) {
                bool b = hash1.Delete(r.NextDouble(), out dummy);
                Assert.IsTrue(b);
            }

#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 1024);

            for (int i = 0; i < 380; ++i) {
                bool b = hash1.Delete(r.NextDouble(), out dummy);
                Assert.IsTrue(b);
            }

#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 64);

            for (int i = 0; i < 20; ++i) {
                bool b = hash1.Delete(r.NextDouble(), out dummy);
                Assert.IsTrue(b);
            }

#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 0);

            hash1.Insert(4.5, true, out dummy);

#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 16);
        }

        [Test]
        public void LoadFactor()
        {
            Hash<double> hash1 = new Hash<double>(EqualityComparer<double>.Default);
            double dummy;

            Random r = new Random(13);

            for (int i = 0; i < 600; ++i) {
                bool b = hash1.Insert(r.NextDouble(), true, out dummy);
                Assert.IsTrue(b);
            }

#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 1024);

            hash1.LoadFactor = 0.55F;
            Assert.AreEqual(0.55F, hash1.LoadFactor);
#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 2048);

            hash1.LoadFactor = 0.9F;
            Assert.AreEqual(0.9F, hash1.LoadFactor);
#if DEBUG
            hash1.PrintStats();
            hash1.Validate();
#endif //DEBUG
            Assert.IsTrue(hash1.SlotCount == 1024);
        }
    }
}

