//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

using System;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Wintellect.PowerCollections.Tests {
	
	/// <summary>
	/// An item type used when testing the RedBlackTree.
	/// </summary>
	struct TestItem
	{
		public TestItem(string key)
		{
			this.key = key;
			this.data = 0;
		}
		public TestItem(string key, int data)
		{
			this.key = key;
			this.data = data;
		}
		public string key;
		public int data;

		public override string ToString()
		{
			return string.Format("Key:{0} Data:{1}", key, data);
		}

	}

	/// <summary>
	/// Tests for testing the RedBlackTree class, using NUnit.
	/// </summary>
	[TestFixture]
	public class RedBlackTreeTests
	{
		internal RedBlackTree<TestItem> tree;

        internal class DataComparer : System.Collections.Generic.IComparer<TestItem>
        {
            public int Compare(TestItem x, TestItem y)
            {
                return string.Compare(x.key, y.key);
            }

            public bool Equals(TestItem x, TestItem y)
            {
                throw new NotSupportedException();
            }

            public int GetHashCode(TestItem obj)
            {
                throw new NotSupportedException();
            }
        }

		/// <summary>
		/// Insert a key and print/validate the tree.
		/// </summary>
		/// <param name="key"></param>
		private void InsertPrintValidate(string key) {
			InsertPrintValidate(key, 0, DuplicatePolicy.ReplaceFirst);
		}

		private void InsertPrintValidate(string key, int data) {
			InsertPrintValidate(key, data, DuplicatePolicy.ReplaceFirst);
		}

        private void InsertPrintValidate(string key, int data, DuplicatePolicy dupPolicy)
        {
            TestItem oldData;
            tree.Insert(new TestItem(key, data), dupPolicy, out oldData);
#if DEBUG
            tree.Print();
            tree.Validate();
#endif //DEBUG
        }

        private void InsertPrintValidate(string key, int data, DuplicatePolicy dupPolicy, int expectedoldData)
        {
            TestItem oldData;
            tree.Insert(new TestItem(key, data), dupPolicy, out oldData);
#if DEBUG
            tree.Print();
			tree.Validate();
#endif //DEBUG
            Assert.AreEqual(expectedoldData, oldData.data);
        }

        /// <summary>
		/// Insert a key and validate the tree.
		/// </summary>
		/// <param name="key"></param>
		private void InsertValidate(string key) {
			InsertValidate(key, 0, DuplicatePolicy.InsertLast);
		}

		private void InsertValidate(string key, int data) {
			InsertValidate(key, data, DuplicatePolicy.InsertLast);
		}

        private void InsertValidate(string key, int data, DuplicatePolicy dupPolicy)
        {
            TestItem oldData;
            tree.Insert(new TestItem(key, data), dupPolicy, out oldData);
#if DEBUG
            tree.Validate();
#endif //DEBUG
        }

        private void InsertValidate(string key, int data, DuplicatePolicy dupPolicy, int expectedOldData)
		{
            TestItem oldData;
            tree.Insert(new TestItem(key, data), dupPolicy, out oldData);
#if DEBUG
			tree.Validate();
#endif //DEBUG
            Assert.AreEqual(expectedOldData, oldData.data);
        }

        /// <summary>
		/// Delete a key, check the data in the deleted key, print and validate.
		/// </summary>
		/// <param name="key">Key to delete.</param>
		/// <param name="data">Expected data in the deleted key.</param>
		private void DeletePrintValidate(string key, int data) {
			DeletePrintValidate(key, data, true);
		}

		private void DeletePrintValidate(string key, int data, bool first) {
			TestItem itemFound;
			int countBefore = tree.ElementCount;
			bool success = tree.Delete(new TestItem(key), 
				                                 first ? true : false, 
				                                 out itemFound);
#if DEBUG
			tree.Print();
#endif //DEBUG
            Assert.IsTrue(success, "Key to delete wasn't found");
            Assert.AreEqual(data, itemFound.data, "Data in deleted key was incorrect.");
			int countAfter = tree.ElementCount;
			Assert.AreEqual(countBefore - 1, countAfter, "Count of elements incorrect after deletion");
#if DEBUG
			tree.Validate();
#endif //DEBUG
		}

        private void GlobalDeletePrintValidate(string key, int data, bool first)
        {
            TestItem itemFound;
            int countBefore = tree.ElementCount;
            bool success = tree.DeleteItemFromRange(tree.EntireRangeTester, first, out itemFound);
#if DEBUG
            tree.Print();
#endif //DEBUG
            Assert.IsTrue(success, "Key to delete wasn't found");
            Assert.AreEqual(key, itemFound.key, "Key in deleted key was incorrect.");
            Assert.AreEqual(data, itemFound.data, "Data in deleted key was incorrect.");
            int countAfter = tree.ElementCount;
            Assert.AreEqual(countBefore - 1, countAfter, "Count of elements incorrect after deletion");
#if DEBUG
            tree.Validate();
#endif //DEBUG
        }

        private void FindFirstKey(string key, int value)
        {
            TestItem itemFound;
			bool found = tree.Find(new TestItem(key), true, false, out itemFound);
			Assert.IsTrue(found, "Key was not found in the tree");
			Assert.AreEqual(value, itemFound.data, "Wrong value found in the tree");
            int foundIndex = tree.FindIndex(new TestItem(key), true);
            Assert.IsTrue(foundIndex >= 0);
            Assert.AreEqual(value, tree.GetItemByIndex(foundIndex).data);
		}

		private void FindLastKey(string key, int value) {
			TestItem itemFound;
			bool found = tree.Find(new TestItem(key), false, false, out itemFound);
			Assert.IsTrue(found, "Key was not found in the tree");
			Assert.AreEqual(value, itemFound.data, "Wrong value found in the tree");
            int foundIndex = tree.FindIndex(new TestItem(key), false);
            Assert.IsTrue(foundIndex >= 0);
            Assert.AreEqual(value, tree.GetItemByIndex(foundIndex).data);
        }

		private void FindOnlyKey(string key, int value) {
			FindFirstKey(key, value);
			FindLastKey(key, value);
		}

        private bool FindReplaceKey(string key, int newValue, int expectedOldValue)
        {
            TestItem itemFound;
            bool found = tree.Find(new TestItem(key, newValue), true, true, out itemFound);
            Assert.AreEqual(expectedOldValue, itemFound.data);
            return found;
        }

        /// <summary>
		/// Test creation of the tree.
		/// </summary>
		[Test] public void Create() {
			tree = new RedBlackTree<TestItem>(new DataComparer());
#if DEBUG
			tree.Validate();
#endif //DEBUG
		}

		/// <summary>
		/// Insert values into tree to test the basic insertion algorithm. Validate
		/// and print the tree after each step.
		/// </summary>
		[Test] public void NormalInsert() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertPrintValidate("m");
			InsertPrintValidate("b");
			InsertPrintValidate("t");
			InsertPrintValidate("o");
			InsertPrintValidate("z");
			InsertPrintValidate("g");
			InsertPrintValidate("a5");
			InsertPrintValidate("c");
			InsertPrintValidate("a2");
			InsertPrintValidate("a7");
			InsertPrintValidate("i");
			InsertPrintValidate("h");
			Assert.AreEqual(12, tree.ElementCount, "Wrong number of items in the tree.");
		}

		/// <summary>
		/// Insert values into tree and then find values in the tree.
		/// </summary>
		[Test] public void NormalFind() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertValidate("m", 101);
			FindOnlyKey("m", 101);
			InsertValidate("b", 102);
			InsertValidate("t", 103);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			InsertValidate("o", 104);
			FindOnlyKey("b", 102);
			InsertValidate("z", 105);
			InsertValidate("g", 106);
			FindOnlyKey("g", 106);
			InsertValidate("a5", 107);
			InsertValidate("c", 8);
			InsertValidate("a2", 9);
			FindOnlyKey("z", 105);
			InsertValidate("a7", 10);
			InsertValidate("i", 11);
			InsertValidate("h", 112);

			Assert.AreEqual(12, tree.ElementCount, "Wrong number of items in the tree.");

			FindOnlyKey("m", 101);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			FindOnlyKey("o", 104);
			FindOnlyKey("z", 105);
			FindOnlyKey("g", 106);
			FindOnlyKey("a5", 107);
			FindOnlyKey("c", 8);
			FindOnlyKey("a2", 9);
			FindOnlyKey("a7", 10);
			FindOnlyKey("i", 11);
			FindOnlyKey("h", 112);
		}

        /// <summary>
        /// Test find with the replace option..
        /// </summary>
        [Test]
        public void FindReplace()
        {
            bool b;
            tree = new RedBlackTree<TestItem>(new DataComparer());

            InsertValidate("m", 101);
            FindOnlyKey("m", 101);
            InsertValidate("b", 102);
            InsertValidate("t", 103);
            b = FindReplaceKey("b", 202, 102); Assert.IsTrue(b);
            FindOnlyKey("t", 103);
            InsertValidate("o", 104);
            FindOnlyKey("b", 202);
            InsertValidate("z", 105);
            InsertValidate("g", 106);
            FindOnlyKey("g", 106);
            b = FindReplaceKey("a5", 77, 0); Assert.IsFalse(b);
            b = FindReplaceKey("a5", 134, 0); Assert.IsFalse(b);
            b = FindReplaceKey("m", 201, 101); Assert.IsTrue(b);
            InsertValidate("a5", 107);
            InsertValidate("c", 8);
            InsertValidate("a2", 9);
            FindOnlyKey("z", 105);
            b = FindReplaceKey("m", 301, 201); Assert.IsTrue(b);
            InsertValidate("a7", 10);
            b = FindReplaceKey("a5", 207, 107); Assert.IsTrue(b);
            InsertValidate("i", 11);
            InsertValidate("h", 112);
            b = FindReplaceKey("z", 205, 105); Assert.IsTrue(b);
            b = FindReplaceKey("g", 206, 106); Assert.IsTrue(b);
            b = FindReplaceKey("g", 306, 206); Assert.IsTrue(b);

            Assert.AreEqual(12, tree.ElementCount, "Wrong number of items in the tree.");

            FindOnlyKey("m", 301);
            FindOnlyKey("b", 202);
            FindOnlyKey("t", 103);
            FindOnlyKey("o", 104);
            FindOnlyKey("z", 205);
            FindOnlyKey("g", 306);
            FindOnlyKey("a5", 207);
            FindOnlyKey("c", 8);
            FindOnlyKey("a2", 9);
            FindOnlyKey("a7", 10);
            FindOnlyKey("i", 11);
            FindOnlyKey("h", 112);
        }

        /// <summary>
        /// Check that deletion works.
		/// </summary>
		[Test] public void Delete() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

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
			DeletePrintValidate("a7", 10);
			DeletePrintValidate("i", 11);
		}

		/// <summary>
		/// Test that deleting a non-present value works right.  A bug was found where the root isn't
		/// colored black in this case.
		/// </summary>
		[Test]
		public void DeleteNotPresent()
		{
            int dummy;
            RedBlackTree<int> t = new RedBlackTree<int>(Comparers.DefaultComparer<int>());

			t.Insert(3, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(1, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(5, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(3, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(2, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(2, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(3, DuplicatePolicy.ReplaceFirst, out dummy);
            t.Insert(4, DuplicatePolicy.ReplaceFirst, out dummy);

            bool b;
			int d;

#if DEBUG
			t.Print();
#endif //DEBUG

			b = t.Delete(1, true, out d);
			Assert.IsTrue(b);
#if DEBUG
			t.Print();
			t.Validate();
#endif //DEBUG

			b = t.Delete(1, true, out d);
			Assert.IsFalse(b);
#if DEBUG
			t.Print();
			t.Validate();
#endif //DEBUG

			b = t.Delete(int.MinValue, true, out d);
			Assert.IsFalse(b);
#if DEBUG
			t.Print();
			t.Validate();
#endif //DEBUG

			b = t.Delete(3, true, out d);
			Assert.IsTrue(b);
#if DEBUG
			t.Print();
			t.Validate();
#endif //DEBUG

			b = t.Delete(3, true, out d);
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
			tree = new RedBlackTree<TestItem>(new DataComparer());
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
			InsertValidate("c");

			string[] keys = new string[] {"a2", "a5", "a7",	"b",	"c",	"c", "g",	"h",	"i", "m",	"o", "o", "p", "t" };
			int i = 0;
			foreach (TestItem item in tree)
			{
				Assert.AreEqual(item.key, keys[i], "Keys weren't enumerated in order");
				++i;
			}

			i = 0;

			foreach (TestItem item in tree.EnumerateRangeReversed(tree.EntireRangeTester))
			{
				Assert.AreEqual(item.key, keys[tree.ElementCount - i - 1], "Keys weren't enumerated in reverse order");
				++i;
			}
		
		}

        private void CheckEnumerateRange(RedBlackTree<TestItem> tree, bool useFirst, string first, bool useLast, string last, string[] keys)
        {
            int i = 0;
            TestItem firstItem, lastItem;

            foreach (TestItem item in tree.EnumerateRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)))) {
                Assert.AreEqual(item.key, keys[i], "Keys weren't enumerated in order");
                ++i;
            }

            i = 0;
            foreach (TestItem item in tree.EnumerateRangeReversed(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)))) {
                Assert.AreEqual(item.key, keys[keys.Length - i - 1], "Keys weren't enumerated in reverse order");
                ++i;
            }

            if (i != 0) {
                int foundFirst = tree.FirstItemInRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)), out firstItem);
                Assert.IsTrue(foundFirst >= 0);
                Assert.AreEqual(keys[0], firstItem.key);
                Assert.AreEqual(keys[0], tree.GetItemByIndex(foundFirst).key);
                int foundLast = tree.LastItemInRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)), out lastItem);
                Assert.IsTrue(foundLast >= 0);
                Assert.AreEqual(keys[i-1], lastItem.key);
                Assert.AreEqual(keys[i-1], tree.GetItemByIndex(foundLast).key);
                Assert.AreEqual(i, foundLast - foundFirst + 1);
            }
            else {
                Assert.IsTrue(tree.FirstItemInRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)), out firstItem) < 0);
                Assert.IsTrue(tree.LastItemInRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)), out lastItem) < 0);
            }

            Assert.AreEqual(keys.Length, tree.CountRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last))));
        }

        [Test]
        public void EnumerateAndCountRange()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());
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
            InsertValidate("c");

            CheckEnumerateRange(tree, true, "a7", true, "o", new string[] { "a7", "b", "c", "c", "g", "h", "i", "m" });
            CheckEnumerateRange(tree, true, "c", true, "c", new string[] { });
            CheckEnumerateRange(tree, true, "c", true, "c1", new string[] { "c", "c" });
            CheckEnumerateRange(tree, true, "a", true, "a1", new string[0] { });
            CheckEnumerateRange(tree, true, "j", true, "k", new string[0] { });
            CheckEnumerateRange(tree, true, "z", true, "a", new string[0] { });
            CheckEnumerateRange(tree, true, "h5", true, "z", new string[] { "i", "m", "o", "o", "p", "t" });
            CheckEnumerateRange(tree, true, "a3", true, "a8", new string[] { "a5", "a7" });
            CheckEnumerateRange(tree, true, "a", true, "z", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckEnumerateRange(tree, false, "m", false, "n", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckEnumerateRange(tree, true, "c", false, "n", new string[] { "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckEnumerateRange(tree, true, "c1", false, "n", new string[] { "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckEnumerateRange(tree, false, "m", true, "o", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m" });
            CheckEnumerateRange(tree, false, "m", true, "o3", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o"});
        }

        private void CheckEnumerateRange2(RedBlackTree<TestItem> tree, bool firstInclusive, string first, bool lastInclusive, string last, System.Collections.Generic.IList<string> keys)
        {
            int i = 0;
            TestItem firstItem, lastItem;

            foreach (TestItem item in tree.EnumerateRange(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive))) {
                Assert.AreEqual(item.key, keys[i], "Keys weren't enumerated in order");
                ++i;
            }

            i = 0;
            foreach (TestItem item in tree.EnumerateRangeReversed(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive))) {
                Assert.AreEqual(item.key, keys[keys.Count - i - 1], "Keys weren't enumerated in reverse order");
                ++i;
            }

            if (i != 0) {
                int foundFirst = tree.FirstItemInRange(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive), out firstItem);
                Assert.IsTrue(foundFirst >= 0);
                Assert.AreEqual(keys[0], firstItem.key);
                Assert.AreEqual(keys[0], tree.GetItemByIndex(foundFirst).key);
                int foundLast = tree.LastItemInRange(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive), out lastItem);
                Assert.IsTrue(foundLast >= 0);
                Assert.AreEqual(keys[i - 1], lastItem.key);
                Assert.AreEqual(keys[i - 1], tree.GetItemByIndex(foundLast).key);
                Assert.AreEqual(i, foundLast - foundFirst + 1);
            }
            else {
                Assert.IsTrue(tree.FirstItemInRange(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive), out firstItem) < 0);
                Assert.IsTrue(tree.LastItemInRange(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive), out lastItem) < 0);
            }

            Assert.AreEqual(keys.Count, tree.CountRange(tree.DoubleBoundedRangeTester(new TestItem(first), firstInclusive, new TestItem(last), lastInclusive)));
        }

        [Test]
        public void EnumerateAndCountRange2()
        {
            Random rand = new Random(112);
            for (int iter = 0; iter < ITERATIONS; ++iter) {
                tree = new RedBlackTree<TestItem>(new DataComparer());
                int[] a = CreateRandomArray(iter, LENGTH, LENGTH * 10, false);
                InsertArray(a, DuplicatePolicy.InsertLast);

                string[] strs = Array.ConvertAll<int, string>(a, StringFromInt);
                Array.Sort(strs);

                for (int k = 0; k < 10; ++k) {
                    string lower, upper;
                    do {
                        lower = StringFromInt(rand.Next(LENGTH * 10));
                        upper = StringFromInt(rand.Next(LENGTH * 10));
                    } while (string.Compare(lower, upper) >= 0);

                    bool lowerInclusive = (rand.Next(2) == 0);
                    bool upperInclusive = (rand.Next(2) == 0);

                    int lowerIndex, upperIndex;
                    if (lowerInclusive)
                        lowerIndex = Algorithms.FindFirstIndexWhere(strs, delegate(string x) { return string.Compare(x, lower) >= 0; });
                    else
                        lowerIndex = Algorithms.FindFirstIndexWhere(strs, delegate(string x) { return string.Compare(x, lower) > 0; });
                    if (upperInclusive)
                        upperIndex = Algorithms.FindLastIndexWhere(strs, delegate(string x) { return string.Compare(x, upper) <= 0; });
                    else
                        upperIndex = Algorithms.FindLastIndexWhere(strs, delegate(string x) { return string.Compare(x, upper) < 0; });

                    CheckEnumerateRange2(tree, lowerInclusive, lower, upperInclusive, upper, Algorithms.Range(strs, lowerIndex, upperIndex - lowerIndex + 1));
                }
            }
        }

        private void CheckDeleteRange(RedBlackTree<TestItem> tree, bool useFirst, string first, bool useLast, string last, string[] keys)
        {
            int i = 0;
            int count = tree.ElementCount;
            int deletedCount = tree.DeleteRange(tree.BoundedRangeTester(useFirst, new TestItem(first), useLast, new TestItem(last)));
            Assert.AreEqual(keys.Length, count - deletedCount);

            foreach (TestItem item in tree) {
                Assert.AreEqual(item.key, keys[i], "Keys weren't enumerated in order");
                ++i;
            }
        }

        [Test]
        public void DeleteRange()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());
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
            InsertValidate("c");

            CheckDeleteRange(tree.Clone(), true, "a7", true, "o", new string[] { "a2", "a5", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "c", true, "c", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "c", true, "c1", new string[] { "a2", "a5", "a7", "b", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "a", true, "a1", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "j", true, "k", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "z", true, "a", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "h5", true, "z", new string[] { "a2", "a5", "a7", "b", "c", "c", "g", "h" });
            CheckDeleteRange(tree.Clone(), true, "a3", true, "a8", new string[] { "a2", "b", "c", "c", "g", "h", "i", "m", "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), true, "a", true, "z", new string[] { });
            CheckDeleteRange(tree.Clone(), false, "m", false, "n", new string[] { });
            CheckDeleteRange(tree.Clone(), true, "c", false, "n", new string[] { "a2", "a5", "a7", "b" });
            CheckDeleteRange(tree.Clone(), true, "c1", false, "n", new string[] { "a2", "a5", "a7", "b", "c", "c" });
            CheckDeleteRange(tree.Clone(), false, "m", true, "o", new string[] { "o", "o", "p", "t" });
            CheckDeleteRange(tree.Clone(), false, "m", true, "o3", new string[] { "p", "t" });
        }

        [Test]
        public void CountEqual()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());
            InsertValidate("m");
            InsertValidate("c");
            InsertValidate("b");
            InsertValidate("c");
            InsertValidate("t");
            InsertValidate("o");
            InsertValidate("z");
            InsertValidate("g");
            InsertValidate("a5");
            InsertValidate("c");
            InsertValidate("a2");
            InsertValidate("a7");
            InsertValidate("c");
            InsertValidate("i");
            InsertValidate("h");
            InsertValidate("o");
            InsertValidate("c");

            Assert.AreEqual(5, tree.CountRange(tree.EqualRangeTester(new TestItem("c"))));
            Assert.AreEqual(2, tree.CountRange(tree.EqualRangeTester(new TestItem("o"))));
            Assert.AreEqual(1, tree.CountRange(tree.EqualRangeTester(new TestItem("z"))));
            Assert.AreEqual(1, tree.CountRange(tree.EqualRangeTester(new TestItem("m"))));
            Assert.AreEqual(1, tree.CountRange(tree.EqualRangeTester(new TestItem("a2"))));
            Assert.AreEqual(0, tree.CountRange(tree.EqualRangeTester(new TestItem("e"))));
        }

        [Test]
        public void EnumerateEqual()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());
            InsertValidate("m", 1);
            InsertValidate("c", 2);
            InsertValidate("b", 3);
            InsertValidate("c", 4);
            InsertValidate("t", 5);
            InsertValidate("o", 6);
            InsertValidate("z", 7);
            InsertValidate("g", 8);
            InsertValidate("a5", 9);
            InsertValidate("c", 10);
            InsertValidate("a2", 11);
            InsertValidate("a7", 12);
            InsertValidate("c", 13);
            InsertValidate("i", 14);
            InsertValidate("h", 15);
            InsertValidate("o", 16);
            InsertValidate("c", 17);

            InterfaceTests.TestEnumerableElements(tree.EnumerateRange(tree.EqualRangeTester(new TestItem("c", 0))),
                new TestItem[] { new TestItem("c", 2), new TestItem("c", 4), new TestItem("c", 10), new TestItem("c", 13), new TestItem("c", 17) });
            InterfaceTests.TestEnumerableElements(tree.EnumerateRange(tree.EqualRangeTester(new TestItem("o", 0))),
                new TestItem[] { new TestItem("o", 6), new TestItem("o", 16)});
            InterfaceTests.TestEnumerableElements(tree.EnumerateRange(tree.EqualRangeTester(new TestItem("g", 0))),
                new TestItem[] { new TestItem("g", 8) });
            InterfaceTests.TestEnumerableElements(tree.EnumerateRange(tree.EqualRangeTester(new TestItem("qqq", 0))),
                new TestItem[] {  });
        }

        [Test]
        public void FindIndex()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());
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
            InsertValidate("c");

            int index;

            index = tree.FindIndex(new TestItem("a7"), true);
            Assert.AreEqual(2, index);
            index = tree.FindIndex(new TestItem("a7"), false);
            Assert.AreEqual(2, index);

            index = tree.FindIndex(new TestItem("a2"), true);
            Assert.AreEqual(0, index);
            index = tree.FindIndex(new TestItem("a2"), false);
            Assert.AreEqual(0, index);

            index = tree.FindIndex(new TestItem("t"), true);
            Assert.AreEqual(13, index);
            index = tree.FindIndex(new TestItem("t"), false);
            Assert.AreEqual(13, index);

            index = tree.FindIndex(new TestItem("n"), true);
            Assert.AreEqual(-1, index);
            index = tree.FindIndex(new TestItem("n"), false);
            Assert.AreEqual(-1, index);

            index = tree.FindIndex(new TestItem("o"), true);
            Assert.AreEqual(10, index);
            index = tree.FindIndex(new TestItem("o"), false);
            Assert.AreEqual(11, index);
        }

        /// <summary>
		/// Insert values into tree using replace policy and then find values in the tree.
		/// </summary>
		[Test] public void ReplaceFind() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertValidate("m", 101, DuplicatePolicy.ReplaceFirst, 0);
			FindOnlyKey("m", 101);
			InsertValidate("b", 102, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("t", 103, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("m", 201, DuplicatePolicy.ReplaceFirst, 101);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			InsertValidate("o", 104, DuplicatePolicy.ReplaceFirst, 0);
			FindOnlyKey("b", 102);
			InsertValidate("z", 105, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("g", 106, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("b", 202, DuplicatePolicy.ReplaceFirst, 102);
			FindOnlyKey("g", 106);
			InsertValidate("g", 206, DuplicatePolicy.ReplaceFirst, 106);
			InsertValidate("a5", 107, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("t", 203, DuplicatePolicy.ReplaceFirst, 103);
			InsertValidate("c", 8, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("a2", 9, DuplicatePolicy.ReplaceFirst, 0);
			FindOnlyKey("z", 105);
			InsertValidate("a7", 10, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("i", 11, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("h", 112, DuplicatePolicy.ReplaceFirst, 0);
			InsertValidate("z", 205, DuplicatePolicy.ReplaceFirst, 105);
			InsertValidate("a2", 209, DuplicatePolicy.ReplaceFirst, 9);
			InsertValidate("c", 208, DuplicatePolicy.ReplaceFirst, 8);
			InsertValidate("i", 211, DuplicatePolicy.ReplaceFirst, 11);
			InsertValidate("h", 212, DuplicatePolicy.ReplaceFirst, 112);

			Assert.AreEqual(12, tree.ElementCount, "Wrong number of items in the tree.");

			FindOnlyKey("m", 201);
			FindOnlyKey("b", 202);
			FindOnlyKey("t", 203);
			FindOnlyKey("o", 104);
			FindOnlyKey("z", 205);
			FindOnlyKey("g", 206);
			FindOnlyKey("a5", 107);
			FindOnlyKey("c", 208);
			FindOnlyKey("a2", 209);
			FindOnlyKey("a7", 10);
			FindOnlyKey("i", 211);
			FindOnlyKey("h", 212);
		}	

		/// <summary>
		/// Insert values into tree using "do-nothing" policy and then find values in the tree.
		/// </summary>
		[Test] public void DoNothingFind() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertValidate("m", 101, DuplicatePolicy.DoNothing, 0);
			FindOnlyKey("m", 101);
			InsertValidate("b", 102, DuplicatePolicy.DoNothing, 0);
			InsertValidate("t", 103, DuplicatePolicy.DoNothing, 0);
			InsertValidate("m", 201, DuplicatePolicy.DoNothing, 101);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			InsertValidate("o", 104, DuplicatePolicy.DoNothing, 0);
			FindOnlyKey("b", 102);
			InsertValidate("z", 105, DuplicatePolicy.DoNothing, 0);
			InsertValidate("g", 106, DuplicatePolicy.DoNothing, 0);
			InsertValidate("b", 202, DuplicatePolicy.DoNothing, 102);
			FindOnlyKey("g", 106);
			InsertValidate("g", 206, DuplicatePolicy.DoNothing, 106);
			InsertValidate("a5", 107, DuplicatePolicy.DoNothing, 0);
			InsertValidate("t", 203, DuplicatePolicy.DoNothing, 103);
			InsertValidate("c", 8, DuplicatePolicy.DoNothing, 0);
			InsertValidate("a2", 9, DuplicatePolicy.DoNothing, 0);
			FindOnlyKey("z", 105);
			InsertValidate("a7", 10, DuplicatePolicy.DoNothing, 0);
			InsertValidate("i", 11, DuplicatePolicy.DoNothing, 0);
			InsertValidate("h", 112, DuplicatePolicy.DoNothing, 0);
			InsertValidate("z", 205, DuplicatePolicy.DoNothing, 105);
			InsertValidate("a2", 209, DuplicatePolicy.DoNothing, 9);
			InsertValidate("c", 208, DuplicatePolicy.DoNothing, 8);
			InsertValidate("i", 211, DuplicatePolicy.DoNothing, 11);
			InsertValidate("h", 212, DuplicatePolicy.DoNothing, 112);
            InsertValidate("m", 401, DuplicatePolicy.DoNothing, 101);

            Assert.AreEqual(12, tree.ElementCount, "Wrong number of items in the tree.");

			FindOnlyKey("m", 101);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			FindOnlyKey("o", 104);
			FindOnlyKey("z", 105);
			FindOnlyKey("g", 106);
			FindOnlyKey("a5", 107);
			FindOnlyKey("c", 8);
			FindOnlyKey("a2", 9);
			FindOnlyKey("a7", 10);
			FindOnlyKey("i", 11);
			FindOnlyKey("h", 112);
		}	

		/// <summary>
		/// Insert values into tree using insert-first policy and then find values in the tree.
		/// </summary>
		[Test] public void InsertFirstFind() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertValidate("m", 101, DuplicatePolicy.InsertFirst, 0);
			FindOnlyKey("m", 101);
			InsertValidate("b", 102, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("t", 103, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("m", 201, DuplicatePolicy.InsertFirst, 101);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			InsertValidate("o", 104, DuplicatePolicy.InsertFirst, 0);
			FindOnlyKey("b", 102);
			InsertValidate("z", 105, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("g", 106, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("b", 202, DuplicatePolicy.InsertFirst, 102);
			FindOnlyKey("g", 106);
			InsertValidate("g", 206, DuplicatePolicy.InsertFirst, 106);
			InsertValidate("a5", 107, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("t", 203, DuplicatePolicy.InsertFirst, 103);
			InsertValidate("c", 8, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("a2", 9, DuplicatePolicy.InsertFirst, 0);
			FindOnlyKey("z", 105);
			InsertValidate("a7", 10, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("i", 11, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("h", 112, DuplicatePolicy.InsertFirst, 0);
			InsertValidate("z", 205, DuplicatePolicy.InsertFirst, 105);
			InsertValidate("a2", 209, DuplicatePolicy.InsertFirst, 9);
			InsertValidate("c", 208, DuplicatePolicy.InsertFirst, 8);
			InsertValidate("i", 211, DuplicatePolicy.InsertFirst, 11);
			InsertValidate("h", 212, DuplicatePolicy.InsertFirst, 112);

			Assert.AreEqual(21, tree.ElementCount, "Wrong number of items in the tree.");

			FindLastKey("m", 101);
			FindLastKey("b", 102);
			FindLastKey("t", 103);
			FindLastKey("o", 104);
			FindLastKey("z", 105);
			FindLastKey("g", 106);
			FindLastKey("a5", 107);
			FindLastKey("c", 8);
			FindLastKey("a2", 9);
			FindLastKey("a7", 10);
			FindLastKey("i", 11);
			FindLastKey("h", 112);
			
			FindFirstKey("m", 201);
			FindFirstKey("b", 202);
			FindFirstKey("t", 203);
			FindFirstKey("o", 104);
			FindFirstKey("z", 205);
			FindFirstKey("g", 206);
			FindFirstKey("a5", 107);
			FindFirstKey("c", 208);
			FindFirstKey("a2", 209);
			FindFirstKey("a7", 10);
			FindFirstKey("i", 211);
			FindFirstKey("h", 212);

            CheckAllIndices();
		}
	
		/// <summary>
		/// Insert values into tree using insert-last policy and then find values in the tree.
		/// </summary>
		[Test] public void InsertLastFind() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertValidate("m", 101, DuplicatePolicy.InsertLast, 0);
			FindOnlyKey("m", 101);
			InsertValidate("b", 102, DuplicatePolicy.InsertLast, 0);
			InsertValidate("t", 103, DuplicatePolicy.InsertLast, 0);
			InsertValidate("m", 201, DuplicatePolicy.InsertLast, 101);
			FindOnlyKey("b", 102);
			FindOnlyKey("t", 103);
			InsertValidate("o", 104, DuplicatePolicy.InsertLast, 0);
			FindOnlyKey("b", 102);
			InsertValidate("z", 105, DuplicatePolicy.InsertLast, 0);
			InsertValidate("g", 106, DuplicatePolicy.InsertLast, 0);
			InsertValidate("b", 202, DuplicatePolicy.InsertLast, 102);
			FindOnlyKey("g", 106);
			InsertValidate("g", 206, DuplicatePolicy.InsertLast, 106);
			InsertValidate("a5", 107, DuplicatePolicy.InsertLast, 0);
			InsertValidate("t", 203, DuplicatePolicy.InsertLast, 103);
			InsertValidate("c", 8, DuplicatePolicy.InsertLast, 0);
			InsertValidate("a2", 9, DuplicatePolicy.InsertLast, 0);
			FindOnlyKey("z", 105);
			InsertValidate("a7", 10, DuplicatePolicy.InsertLast, 0);
			InsertValidate("i", 11, DuplicatePolicy.InsertLast, 0);
			InsertValidate("h", 112, DuplicatePolicy.InsertLast, 0);
			InsertValidate("z", 205, DuplicatePolicy.InsertLast, 105);
			InsertValidate("a2", 209, DuplicatePolicy.InsertLast, 9);
			InsertValidate("c", 208, DuplicatePolicy.InsertLast, 8);
			InsertValidate("i", 211, DuplicatePolicy.InsertLast, 11);
			InsertValidate("h", 212, DuplicatePolicy.InsertLast, 112);

			Assert.AreEqual(21, tree.ElementCount, "Wrong number of items in the tree.");

			FindFirstKey("m", 101);
			FindFirstKey("b", 102);
			FindFirstKey("t", 103);
			FindFirstKey("o", 104);
			FindFirstKey("z", 105);
			FindFirstKey("g", 106);
			FindFirstKey("a5", 107);
			FindFirstKey("c", 8);
			FindFirstKey("a2", 9);
			FindFirstKey("a7", 10);
			FindFirstKey("i", 11);
			FindFirstKey("h", 112);
			
			FindLastKey("m", 201);
			FindLastKey("b", 202);
			FindLastKey("t", 203);
			FindLastKey("o", 104);
			FindLastKey("z", 205);
			FindLastKey("g", 206);
			FindLastKey("a5", 107);
			FindLastKey("c", 208);
			FindLastKey("a2", 209);
			FindLastKey("a7", 10);
			FindLastKey("i", 211);
			FindLastKey("h", 212);
        
            CheckAllIndices();
        }
	
		/// <summary>
		/// Insert values into tree delete values in the tree, making sure first/last works.
		/// </summary>
		[Test] public void DeleteFirstLast() {
			tree = new RedBlackTree<TestItem>(new DataComparer());

			InsertValidate("m", 101, DuplicatePolicy.InsertFirst);
			InsertValidate("b", 102, DuplicatePolicy.InsertFirst);
			InsertValidate("t", 103, DuplicatePolicy.InsertFirst);
			InsertValidate("m", 201, DuplicatePolicy.InsertFirst);
			InsertValidate("o", 104, DuplicatePolicy.InsertFirst);
			InsertValidate("z", 105, DuplicatePolicy.InsertFirst);
			InsertValidate("g", 106, DuplicatePolicy.InsertFirst);
			InsertValidate("b", 202, DuplicatePolicy.InsertFirst);
			InsertValidate("g", 206, DuplicatePolicy.InsertFirst);
			InsertValidate("m", 301, DuplicatePolicy.InsertFirst);
			InsertValidate("a5", 107, DuplicatePolicy.InsertFirst);
			InsertValidate("t", 203, DuplicatePolicy.InsertFirst);
			InsertValidate("c", 8, DuplicatePolicy.InsertFirst);
			InsertValidate("a2", 9, DuplicatePolicy.InsertFirst);
			InsertValidate("a7", 10, DuplicatePolicy.InsertFirst);
			InsertValidate("i", 11, DuplicatePolicy.InsertFirst);
			InsertValidate("h", 112, DuplicatePolicy.InsertFirst);
			InsertValidate("m", 401, DuplicatePolicy.InsertFirst);
			InsertValidate("z", 205, DuplicatePolicy.InsertFirst);
			InsertValidate("a2", 209, DuplicatePolicy.InsertFirst);
			InsertValidate("c", 208, DuplicatePolicy.InsertFirst);
			InsertValidate("m", 501, DuplicatePolicy.InsertFirst);
			InsertValidate("i", 211, DuplicatePolicy.InsertFirst);
			InsertValidate("h", 212, DuplicatePolicy.InsertFirst);
			InsertValidate("z", 305, DuplicatePolicy.InsertFirst);

#if DEBUG
			tree.Print();
#endif //DEBUG
			DeletePrintValidate("m", 101, false);
			DeletePrintValidate("m", 501, true);
			DeletePrintValidate("m", 201, false);
			FindFirstKey("m", 401);
			FindLastKey("m", 301);
			DeletePrintValidate("m", 401, true);
			DeletePrintValidate("m", 301, true);

			DeletePrintValidate("z", 305, true);
			DeletePrintValidate("z", 205, true);
			DeletePrintValidate("z", 105, true);
        
            CheckAllIndices();
        }

        /// <summary>
        /// Delete values in the tree, making sure global first/last works.
        /// </summary>
        [Test]
        public void GlobalDeleteFirstLast()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());

            InsertValidate("a", 101, DuplicatePolicy.InsertLast);
            InsertValidate("b", 102, DuplicatePolicy.InsertLast);
            InsertValidate("t", 103, DuplicatePolicy.InsertLast);
            InsertValidate("a", 201, DuplicatePolicy.InsertLast);
            InsertValidate("o", 104, DuplicatePolicy.InsertLast);
            InsertValidate("z", 105, DuplicatePolicy.InsertLast);
            InsertValidate("g", 106, DuplicatePolicy.InsertLast);
            InsertValidate("b", 202, DuplicatePolicy.InsertLast);
            InsertValidate("g", 206, DuplicatePolicy.InsertLast);
            InsertValidate("a", 301, DuplicatePolicy.InsertLast);
            InsertValidate("a5", 107, DuplicatePolicy.InsertLast);
            InsertValidate("t", 203, DuplicatePolicy.InsertLast);
            InsertValidate("c", 8, DuplicatePolicy.InsertLast);
            InsertValidate("a2", 9, DuplicatePolicy.InsertLast);
            InsertValidate("a7", 10, DuplicatePolicy.InsertLast);
            InsertValidate("i", 11, DuplicatePolicy.InsertLast);
            InsertValidate("h", 112, DuplicatePolicy.InsertLast);
            InsertValidate("a", 401, DuplicatePolicy.InsertLast);
            InsertValidate("z", 205, DuplicatePolicy.InsertLast);
            InsertValidate("a2", 209, DuplicatePolicy.InsertLast);
            InsertValidate("c", 208, DuplicatePolicy.InsertLast);
            InsertValidate("a", 501, DuplicatePolicy.InsertLast);
            InsertValidate("i", 211, DuplicatePolicy.InsertLast);
            InsertValidate("h", 212, DuplicatePolicy.InsertLast);
            InsertValidate("z", 305, DuplicatePolicy.InsertLast);

            GlobalDeletePrintValidate("a", 101, true);
            TestItem firstItem;
            Assert.IsTrue(tree.FirstItemInRange(tree.EntireRangeTester, out firstItem) == 0);
            Assert.AreEqual("a", firstItem.key);
            Assert.AreEqual(201, firstItem.data);
            GlobalDeletePrintValidate("a", 201, true);
            FindFirstKey("a", 301);
            GlobalDeletePrintValidate("a", 301, true);

            DeletePrintValidate("z", 305, false);
            FindLastKey("z", 205);
            DeletePrintValidate("z", 205, false);
            TestItem lastItem;
            Assert.IsTrue(tree.LastItemInRange(tree.EntireRangeTester, out lastItem) == tree.ElementCount - 1);
            Assert.AreEqual("z", lastItem.key);
            Assert.AreEqual(105, lastItem.data);
            DeletePrintValidate("z", 105, false);

            CheckAllIndices();
        }

        private void CheckAllIndices()
        {
#if DEBUG
            tree.Validate();
#endif //DEBUG

            TestItem[] items = new TestItem[tree.ElementCount];

            int i = 0;
            foreach (TestItem item in tree)
                items[i++] = item;

            for (i = tree.ElementCount - 1; i >= 0; i -= 2) {
                TestItem item = tree.GetItemByIndex(i);
                Assert.AreEqual(item.key, items[i].key);
                Assert.AreEqual(item.data, items[i].data);
            }

            for (i = tree.ElementCount - 2; i >= 0; i -= 2) {
                TestItem item = tree.GetItemByIndex(i);
                Assert.AreEqual(item.key, items[i].key);
                Assert.AreEqual(item.data, items[i].data);
            }

#if DEBUG
            tree.Validate();
#endif //DEBUG
        }

        const int LENGTH = 400;			// length of each random array of values.
        const int ITERATIONS = 30;		// number of iterations

		/// <summary>
		/// Create a random array of values.
		/// </summary>
		/// <param name="seed">Seed for random number generators</param>
		/// <param name="length">Length of array</param>
		/// <param name="max">Maximum value of number. Should be much 
		/// greater than length.</param>
		/// <param name="allowDups">Whether to allow duplicate elements.</param>
		/// <returns></returns>
		private int[] CreateRandomArray(int seed, int length, int max, bool allowDups) {
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
		/// <param name="dupPolicy">The DuplicatePolicy to use when inserting.</param>
		private void InsertArray(int[] a, DuplicatePolicy dupPolicy)
		{
            TestItem dummy;
            for (int i = 0; i < a.Length; ++i) {
				string s = StringFromInt(a[i]);
				tree.Insert(new TestItem(s, i), dupPolicy, out dummy);
#if DEBUG
				tree.Validate();
#endif //DEBUG
            }
		}

		private string StringFromInt(int i) {
			return string.Format("e{0}", i);
		}

		/// <summary>
		/// Insert LENGTH items in random order into the tree and validate
		/// it. Do this ITER times.
		/// </summary>
		[Test] public void InsertRandom() {
			for (int iter = 0; iter < ITERATIONS; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				int[] a = CreateRandomArray(iter, LENGTH, LENGTH * 10, true);
				InsertArray(a, DuplicatePolicy.InsertLast);
#if DEBUG
				tree.Validate();
#endif //DEBUG
                CheckAllIndices();

                Assert.AreEqual(LENGTH, tree.ElementCount, "Wrong number of items in the tree.");
			}
		}

		/// <summary>
		/// Insert LENGTH items in random order into the tree and then find them all
		/// Do this ITER times.
		/// </summary>
		[Test] public void FindRandom() {
			for (int iter = 0; iter < ITERATIONS; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				int[] a = CreateRandomArray(iter + 1000, LENGTH, LENGTH * 10, false);

				InsertArray(a, DuplicatePolicy.InsertLast);
#if DEBUG
				tree.Validate();
#endif //DEBUG
				Assert.AreEqual(LENGTH, tree.ElementCount, "Wrong number of items in the tree.");

				for (int el = 0; el < a.Length; ++el) {
					FindOnlyKey(StringFromInt(a[el]), el);
				}
			}
		}

		/// <summary>
		/// Insert LENGTH items in random order into the tree using the "replace" policy, 
		/// and then find them all.
		/// Do this ITER times.
		/// </summary>
		[Test] public void ReplaceFindRandom() {
			for (int iter = 0; iter < ITERATIONS; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				int[] a = CreateRandomArray(iter + 2000, LENGTH, LENGTH / 5, true);

				InsertArray(a, DuplicatePolicy.ReplaceFirst);
#if DEBUG
				tree.Validate();
#endif //DEBUG
                CheckAllIndices();

				for (int el = 0; el < a.Length; ++el) {
					FindOnlyKey(StringFromInt(a[el]), Array.LastIndexOf(a, a[el]));
				}
			}
		}	

		/// <summary>
		/// Insert LENGTH items in random order into the tree using the "replace" policy, 
		/// and then find them all.
		/// Do this ITER times.
		/// </summary>
		[Test] public void DoNothingFindRandom() {

            for (int iter = 0; iter < ITERATIONS; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				int[] a = CreateRandomArray(iter + 3000, LENGTH, LENGTH / 5, true);

				InsertArray(a, DuplicatePolicy.DoNothing);
#if DEBUG
				tree.Validate();
#endif //DEBUG
                CheckAllIndices();

				for (int el = 0; el < a.Length; ++el) {
					FindOnlyKey(StringFromInt(a[el]), Array.IndexOf(a, a[el]));
				}
			}
		}	

		/// <summary>
		/// Insert LENGTH items in random order into the tree using the "replace" policy, 
		/// and then find them all.
		/// Do this ITER times.
		/// </summary>
		[Test] public void InsertFirstFindRandom() {
			for (int iter = 0; iter < ITERATIONS; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				int[] a = CreateRandomArray(iter + 3000, LENGTH, LENGTH / 5, true);

				InsertArray(a, DuplicatePolicy.InsertFirst);
#if DEBUG
				tree.Validate();
#endif //DEBUG
                CheckAllIndices();
                Assert.AreEqual(LENGTH, tree.ElementCount, "Element count is wrong.");

				for (int el = 0; el < a.Length; ++el) {
					FindFirstKey(StringFromInt(a[el]), Array.LastIndexOf(a, a[el]));
					FindLastKey(StringFromInt(a[el]), Array.IndexOf(a, a[el]));
				}
			}
		}		

		/// <summary>
		/// Insert LENGTH items in random order into the tree using the "replace" policy, 
		/// and then find them all.
		/// Do this ITER times.
		/// </summary>
		[Test] public void InsertLastFindRandom() {
			for (int iter = 0; iter < ITERATIONS; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				int[] a = CreateRandomArray(iter + 4000, LENGTH, LENGTH / 5, true);

				InsertArray(a, DuplicatePolicy.InsertLast);
#if DEBUG
				tree.Validate();
#endif //DEBUG
                CheckAllIndices();
                Assert.AreEqual(LENGTH, tree.ElementCount, "Element count is wrong.");

				for (int el = 0; el < a.Length; ++el) {
					FindFirstKey(StringFromInt(a[el]), Array.IndexOf(a, a[el]));
					FindLastKey(StringFromInt(a[el]), Array.LastIndexOf(a, a[el]));
				}
			}
		}
		
		/// <summary>
		/// Insert and delete items from the tree at random, finally removing all
		/// the items that are in the tree. Validate the tree after each step.
		/// </summary>
		[Test] public void DeleteRandom() {
			for (int iter = 0; iter < ITERATIONS / 10; ++iter) {
				tree = new RedBlackTree<TestItem>(new DataComparer());
				bool[] a = new bool[LENGTH];
				Random rand = new Random(iter + 5000);
				TestItem itemFound;
			
				for (int i = 0; i < LENGTH * 10; ++i) {
					int v = rand.Next(LENGTH);
					string key = StringFromInt(v);
					if (a[v]) {
						// Already in the tree. Make sure we can find it, then delete it.
						bool b = tree.Find(new TestItem(key), true, false, out itemFound);
						Assert.IsTrue(b, "Couldn't find key in tree");
						Assert.AreEqual(v, itemFound.data, "Data is incorrect");
						b = tree.Delete(new TestItem(key), true, out itemFound);
						Assert.IsTrue(b, "Couldn't delete key in tree");
						Assert.AreEqual(v, itemFound.data, "Data is incorrect");
#if DEBUG
						tree.Validate();
#endif //DEBUG
                        CheckAllIndices();
                        a[v] = false;
					}
					else if (i < LENGTH * 7) {
						// Not in tree. Try to find and delete it. Then Add it.
                        bool b = tree.Find(new TestItem(key), true, false, out itemFound);
                        Assert.IsFalse(b, "Key shouldn't be in tree");
                        b = tree.Delete(new TestItem(key), true, out itemFound);
                        Assert.IsFalse(b);
                        TestItem dummy;
                        b = tree.Insert(new TestItem(key, v), DuplicatePolicy.ReplaceFirst, out dummy);
                        Assert.IsTrue(b, "Key shouldn't be in tree");
#if DEBUG
                        tree.Validate();
#endif //DEBUG
                        CheckAllIndices();
                        a[v] = true;
					}
				}

				for (int v = 0; v < LENGTH; ++v) {
					string key = StringFromInt(v);
					if (a[v]) {
						// Already in the tree. Make sure we can find it, then delete it.
						bool b = tree.Find(new TestItem(key), true, false, out itemFound);
						Assert.IsTrue(b, "Couldn't find key in tree");
						Assert.AreEqual(v, itemFound.data, "Data is incorrect");
						b = tree.Delete(new TestItem(key), true, out itemFound);
						Assert.IsTrue(b, "Couldn't delete key in tree");
						Assert.AreEqual(v, itemFound.data, "Data is incorrect");
#if DEBUG
						tree.Validate();
#endif //DEBUG
						a[v] = false;
					}
				}
			}
		}

        [Test]
        public void ChangeDuringEnumerate()
        {
            TestItem dummy;
            tree = new RedBlackTree<TestItem>(new DataComparer());

            InsertValidate("foo", 3);
            InsertValidate("bar", 4);
            InsertValidate("bingo", 5);
            InsertValidate("biff", 6);
            InsertValidate("zip", 7);
            InsertValidate("zap", 8);

            int i = 0;
            try {
                foreach (TestItem item in tree) {
                    ++i;
                    if (i == 4)
                        InsertValidate("hello", 23);
                }
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            Assert.AreEqual(4, i);     // should have stopped right away.
            Assert.AreEqual(7, tree.ElementCount);   // element should have been found.
            FindOnlyKey("hello", 23);  // element should have been inserted.
#if DEBUG
            tree.Validate();
#endif //DEBUG

            i = 0;
            try {
                foreach (TestItem item in tree.EnumerateRangeReversed(tree.BoundedRangeTester(true, new TestItem("biff", 0), true, new TestItem("zap", 0)))) {
                    ++i;
                    if (i == 3)
                        DeletePrintValidate("hello", 23);
                }
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            Assert.AreEqual(3, i);     // should have stopped right away.
            Assert.AreEqual(6, tree.ElementCount);   // element should have been deleted.
            Assert.IsFalse(tree.Find(new TestItem("hello", 0), true, false, out dummy));
#if DEBUG
            tree.Validate();
#endif //DEBUG
        }

        [Test]
        public void Clone()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());

            InsertValidate("foo", 3);
            InsertValidate("bar", 4);
            InsertValidate("bingo", 5);
            InsertValidate("biff", 6);
            InsertValidate("zip", 7);
            InsertValidate("zap", 8);

            RedBlackTree<TestItem> clone = tree.Clone();
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
            foreach(TestItem item in clone) {
                Assert.AreEqual(s_array[i], item.key);
                ++i;
            }

            tree = new RedBlackTree<TestItem>(new DataComparer());

            clone = tree.Clone();
            Assert.AreEqual(0, clone.ElementCount);
#if DEBUG
            clone.Validate();
#endif //DEBUG
        }

        [Test]
        public void GetByIndexExceptions()
        {
            tree = new RedBlackTree<TestItem>(new DataComparer());

            InsertValidate("foo", 3);
            InsertValidate("bar", 4);
            InsertValidate("bingo", 5);
            InsertValidate("biff", 6);
            InsertValidate("zip", 7);
            InsertValidate("zap", 8);

            try {
                TestItem item = tree.GetItemByIndex(-1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                TestItem item = tree.GetItemByIndex(6);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                TestItem item = tree.GetItemByIndex(Int32.MaxValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                TestItem item = tree.GetItemByIndex(Int32.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            tree = new RedBlackTree<TestItem>(new DataComparer());

            try {
                TestItem item = tree.GetItemByIndex(0);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

    }

}

