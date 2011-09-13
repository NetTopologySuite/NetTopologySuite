//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Wintellect.PowerCollections.Tests {
	/// <summary>
	/// A collection of generic tests for various interfaces.
	/// </summary>
	internal static class InterfaceTests
	{
        public static BinaryPredicate<KeyValuePair<TKey,TValue>> KeyValueEquals<TKey, TValue>(BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };

            return delegate(KeyValuePair<TKey, TValue> pair1, KeyValuePair<TKey, TValue> pair2) {
                return keyEquals(pair1.Key, pair2.Key) && valueEquals(pair1.Value, pair2.Value);
            };
        }

        public static BinaryPredicate<KeyValuePair<TKey, TValue>> KeyValueEquals<TKey, TValue>()
        {
            return KeyValueEquals<TKey, TValue>(null, null);
        }

        public static BinaryPredicate<ICollection<T>> CollectionEquals<T>(BinaryPredicate<T> equals, bool inOrder)
        {
            if (inOrder) {
                return delegate(ICollection<T> enum1, ICollection<T> enum2) {
                    if (enum1 == null || enum2 == null)
                        return (enum1 == enum2);
                    else
                        return Algorithms.EqualCollections(enum1, enum2, equals);
                };
            }
            else {
                return delegate(ICollection<T> enum1, ICollection<T> enum2) {
                    if (enum1 == null || enum2 == null)
                        return (enum1 == enum2);

                    T[] expected = Algorithms.ToArray(enum2);
                    bool[] found = new bool[expected.Length];
                    int i = 0;
                    foreach (T item in enum1) {
                        int index;
                        for (index = 0; index < expected.Length; ++index) {
                            if (!found[index] && equals(expected[index], item))
                                break;
                        }
                        if (index >= expected.Length)
                            return false;
                        if (!equals(expected[index], item))
                            return false;
                        found[index] = true;
                        ++i;
                    }
                    if (expected.Length != i)
                        return false;
                    else
                        return true;
                };
            }
        }


       /// <summary>
       /// Test an IEnumerable should contain the given values in order
       /// </summary>
       public static void TestEnumerableElements<T>(IEnumerable<T> e, T[] expected)
       {
            TestEnumerableElements<T>(e, expected, null);
       }

       public static void TestEnumerableElements<T>(IEnumerable<T> e, T[] expected, BinaryPredicate<T> equals) 
       {
           if (equals == null)
               equals = delegate(T x, T y) { return object.Equals(x, y); };

           int i = 0;
           foreach (T item in e) {
               Assert.IsTrue(equals(expected[i], item));
               ++i;
           }
           Assert.AreEqual(expected.Length, i);
       }

       /// <summary>
       /// Test an IEnumerable should contain the given values in any order
       /// </summary>
       public static void TestEnumerableElementsAnyOrder<T>(IEnumerable<T> e, T[] expected)
       {
            TestEnumerableElementsAnyOrder<T>(e, expected, null);
       }

       public static void TestEnumerableElementsAnyOrder<T>(IEnumerable<T> e, T[] expected, BinaryPredicate<T> equals)
       {
           if (equals == null)
               equals = delegate(T x, T y) { return object.Equals(x, y); };

           bool[] found = new bool[expected.Length];
           int i = 0;
           foreach (T item in e) {
               int index;
               for (index = 0; index < expected.Length; ++index) {
                   if (!found[index] && equals(expected[index], item))
                       break;
               } 
               Assert.IsTrue(index < expected.Length);
               Assert.IsTrue(equals(expected[index], item));
               found[index] = true;
               ++i;
           }
           Assert.AreEqual(expected.Length, i);
       }

       /// <summary>
       ///  Test an ICollection that should contain the given values, possibly in order.
		/// </summary>
		/// <param name="coll">ICollection to test. </param>
		/// <param name="valueArray">The values that should be in the collection.</param>
		/// <param name="mustBeInOrder">Must the values be in order?</param>
		public static void TestCollection<T>(ICollection coll, T[] valueArray, bool mustBeInOrder)
		{
			T[] values = (T[])valueArray.Clone();		// clone the array so we can destroy it.

			// Check ICollection.Count.
			Assert.AreEqual(values.Length, coll.Count);

			// Check ICollection.GetEnumerator().
			int i = 0, j;

			foreach (T s in coll)
			{
				if (mustBeInOrder)
				{
					Assert.AreEqual(values[i], s);
				}
				else
				{
					bool found = false;

					for (j = 0; j < values.Length; ++j)
					{
                        if (object.Equals(values[j],s))
						{
							found = true;
							values[j] = default(T);
							break;
						}
					}

					Assert.IsTrue(found);
				}

				++i;
			}

			// Check IsSyncronized, SyncRoot.
			Assert.IsFalse(coll.IsSynchronized);
			Assert.IsNotNull(coll.SyncRoot);

			// Check CopyTo.
			values = (T[])valueArray.Clone();		// clone the array so we can destroy it.

			T[] newKeys = new T[coll.Count + 2];

			coll.CopyTo(newKeys, 1);
			for (i = 0, j = 1; i < coll.Count; ++i, ++j)
			{
				if (mustBeInOrder)
				{
					Assert.AreEqual(values[i], newKeys[j]);
				}
				else
				{
					bool found = false;

					for (int k = 0; k < values.Length; ++k)
					{
						if (object.Equals(values[k], newKeys[j]))
						{
							found = true;
                            values[k] = default(T);
                            break;
						}
					}

					Assert.IsTrue(found);
				}
			}

			// Shouldn't have disturbed the values around what was filled in.
			Assert.AreEqual(default(T), newKeys[0]);
			Assert.AreEqual(default(T), newKeys[coll.Count + 1]);

			// Check CopyTo exceptions.
            if (coll.Count > 0) {
			    try
			    {
				    coll.CopyTo(null, 0);
				    Assert.Fail("Copy to null should throw exception");
			    }
			    catch (Exception e)
			    {
				    Assert.IsTrue(e is ArgumentNullException);
			    }
			    try
			    {
				    coll.CopyTo(newKeys, 3);
				    Assert.Fail("CopyTo should throw argument exception");
			    }
			    catch (Exception e)
			    {
				    Assert.IsTrue(e is ArgumentException);
			    }
                try {
                    coll.CopyTo(newKeys, -1);
                    Assert.Fail("CopyTo should throw argument out of range exception");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is ArgumentOutOfRangeException);
                }
            }

    }

        /// /// <summary>
		///  Test an ICollection&lt;string&gt; that should contain the given values, possibly in order. Checks only the following items:
		///     GetEnumerator, CopyTo, Count, Contains
		/// </summary>
		/// <param name="coll">ICollection to test. </param>
		/// <param name="valueArray">The elements that should be in the collection.</param>
		/// <param name="mustBeInOrder">Must the elements be in order?</param>
        /// <param name="equals">Predicate to test for equality; null for default.</param>
		private static void TestCollectionGeneric<T>(ICollection<T> coll, T[] values, bool mustBeInOrder, BinaryPredicate<T> equals)
		{
            if (equals == null)
                equals = delegate(T x, T y) { return object.Equals(x, y); };

            bool[] used = new bool[values.Length];

			// Check ICollection.Count.
			Assert.AreEqual(values.Length, coll.Count);

			// Check ICollection.GetEnumerator().
			int i = 0, j;

			foreach (T s in coll)
			{
				if (mustBeInOrder)
				{
					Assert.IsTrue(equals(values[i], s));
				}
				else
				{
					bool found = false;

					for (j = 0; j < values.Length; ++j)
					{
						if (!used[j] && equals(values[j],s))
						{
							found = true;
                            used[j] = true;
							break;
						}
					}

					Assert.IsTrue(found);
				}

				++i;
			}

            // Check Contains
            foreach (T s in values) {
                Assert.IsTrue(coll.Contains(s));
            }

            // Check CopyTo.
            used = new bool[values.Length];

			T[] newKeys = new T[coll.Count + 2];

			coll.CopyTo(newKeys, 1);
			for (i = 0, j = 1; i < coll.Count; ++i, ++j)
			{
				if (mustBeInOrder)
				{
					Assert.IsTrue(equals(values[i], newKeys[j]));
				}
				else
				{
					bool found = false;

					for (int k = 0; k < values.Length; ++k)
					{
						if (!used[k] && equals(values[k], newKeys[j]))
						{
							found = true;
                            used[k] = true;
							break;
						}
					}

					Assert.IsTrue(found);
				}
			}

			// Shouldn't have distubed the values around what was filled in.
			Assert.IsTrue(equals(default(T), newKeys[0]));
            Assert.IsTrue(equals(default(T), newKeys[coll.Count + 1]));

            if (coll.Count != 0)
			{
				// Check CopyTo exceptions.
				try
				{
					coll.CopyTo(null, 0);
					Assert.Fail("Copy to null should throw exception");
				}
				catch (Exception e)
				{
					Assert.IsTrue(e is ArgumentNullException);
				}
				try
				{
					coll.CopyTo(newKeys, 3);
					Assert.Fail("CopyTo should throw argument exception");
				}
				catch (Exception e)
				{
                    Assert.IsTrue(e is ArgumentException);
				}
                try {
                    coll.CopyTo(newKeys, -1);
                    Assert.Fail("CopyTo should throw argument out of range exception");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is ArgumentOutOfRangeException);
                }
            }
        }

        // Check collection read-only exceptions
        private static void CheckReadonlyCollectionException(Exception e, string name)
        {
            Assert.IsTrue(e is NotSupportedException);
            if (name != null)
                Assert.AreEqual(string.Format(Strings.CannotModifyCollection, name), e.Message);
        }

        /// <summary>
		///  Test a readonly ICollection&lt;string&gt; that should contain the given values, possibly in order. Checks only the following items:
		///     GetEnumerator, CopyTo, Count, Contains, IsReadOnly
		/// </summary>
		/// <param name="coll">ICollection&lt;T&gt; to test. </param>
		/// <param name="valueArray">The values that should be in the collection.</param>
		/// <param name="mustBeInOrder">Must the value be in order?</param>
		/// <param name="name">Expected name of the collection, or null for don't check.</param>
        public static void TestReadonlyCollectionGeneric<T>(ICollection<T> coll, T[] valueArray, bool mustBeInOrder, string name)
        {
            TestReadonlyCollectionGeneric<T>(coll, valueArray, mustBeInOrder, null, null);
        }

        public static void TestReadonlyCollectionGeneric<T>(ICollection<T> coll, T[] valueArray, bool mustBeInOrder, string name, BinaryPredicate<T> equals)
        {
            TestCollectionGeneric<T>(coll, valueArray, mustBeInOrder, equals);
      
            // Test read-only flag.
            Assert.IsTrue(coll.IsReadOnly);

            // Check that Clear throws correct exception
            if (coll.Count > 0) {
                try {
                    coll.Clear();
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }
            }

            // Check that Add throws correct exception
            try {
                coll.Add(default(T));
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            // Check throws correct exception
            try {
                coll.Remove(default(T));
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

        }

        /// <summary>
        ///  Test a read-write ICollection&lt;string&gt; that should contain the given values, possibly in order. Destroys the collection in the process.
        /// </summary>
        /// <param name="coll">ICollection to test. </param>
        /// <param name="valueArray">The values that should be in the collection.</param>
        /// <param name="mustBeInOrder">Must the values be in order?</param>
        public static void TestReadWriteCollectionGeneric<T>(ICollection<T> coll, T[] valueArray, bool mustBeInOrder)
        {
            TestReadWriteCollectionGeneric<T>(coll, valueArray, mustBeInOrder, null);
        }

        public static void TestReadWriteCollectionGeneric<T>(ICollection<T> coll, T[] valueArray, bool mustBeInOrder, BinaryPredicate<T> equals)
        {
            TestCollectionGeneric<T>(coll, valueArray, mustBeInOrder, equals);

            // Test read-only flag.
            Assert.IsFalse(coll.IsReadOnly);

            // Clear and Count.
            coll.Clear();
            Assert.AreEqual(0, coll.Count);

            // Add all the items back.
            foreach (T item in valueArray) 
                coll.Add(item);
            Assert.AreEqual(valueArray.Length, coll.Count);
            TestCollectionGeneric<T>(coll, valueArray, mustBeInOrder, equals);

            // Remove all the items again.
            foreach (T item in valueArray) 
                coll.Remove(item);
            Assert.AreEqual(0, coll.Count);
        }

        /// <summary>
        /// Test an IDictionary that should contains the given keys and values, possibly in order.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys</typeparam>
        /// <typeparam name="TValue">Type of the values</typeparam>
        /// <param name="dict">IDictionary to test</param>
        /// <param name="keys">key values for the dictionary</param>
        /// <param name="values">values for the dictionary</param>
        /// <param name="nonKey">A TKey that isn't in the dictionary</param>
        /// <param name="mustBeInOrder">True if the entries must be in order.</param>
        public static void TestDictionary<TKey, TValue>(IDictionary dict, TKey[] keys, TValue[] values, TKey nonKey, bool mustBeInOrder) 
        {
            // Check Count.
            Assert.AreEqual(keys.Length, dict.Count);

            // Check containment.
            for (int i = 0; i < keys.Length; ++i) {
                Assert.IsTrue(dict.Contains(keys[i]));
                Assert.AreEqual(dict[keys[i]], values[i]);
            }

            Assert.IsFalse(dict.Contains(nonKey));
            Assert.IsNull(dict[nonKey]);

            Assert.IsFalse(dict.Contains(new object()));
            Assert.IsNull(dict[new object()]);

            // Check synchronization
            Assert.IsFalse(dict.IsSynchronized);
            Assert.IsNotNull(dict.SyncRoot);
            
            // Check Keys, Values collections
            TestCollection<TKey>(dict.Keys, keys, mustBeInOrder);
            TestCollection<TValue>(dict.Values, values, mustBeInOrder);

            // Check DictionaryEnumerator.
            int count = 0;
            bool[] found = new bool[keys.Length];

            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext()) {
                DictionaryEntry entry = enumerator.Entry;

                Assert.AreEqual(enumerator.Entry.Key, enumerator.Key);
                Assert.AreEqual(enumerator.Entry.Value, enumerator.Value);
                Assert.AreEqual(((DictionaryEntry)(enumerator.Current)).Key, enumerator.Key);
                Assert.AreEqual(((DictionaryEntry)(enumerator.Current)).Value, enumerator.Value);

                // find the entry.
                if (mustBeInOrder) {
                    Assert.AreEqual(keys[count], enumerator.Key);
                    Assert.AreEqual(values[count], enumerator.Value);
                }
                else {
                    for (int i = 0; i < keys.Length; ++i) {
                        if ((!found[i]) && object.Equals(keys[i], enumerator.Key) && object.Equals(values[i], enumerator.Value)) {
                            found[i] = true;
                        }
                    }
                }
                ++count;
            }
            Assert.AreEqual(count, keys.Length);
            if (!mustBeInOrder)
                for (int i = 0; i < keys.Length; ++i)
                    Assert.IsTrue(found[i]);
        }

        /// <summary>
        /// Test an read-only IDictionary that should contains the given keys and values, possibly in order.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys</typeparam>
        /// <typeparam name="TValue">Type of the values</typeparam>
        /// <param name="dict">IDictionary to test</param>
        /// <param name="keys">key values for the dictionary</param>
        /// <param name="values">values for the dictionary</param>
        /// <param name="mustBeInOrder">True if the entries must be in order.</param>
        /// <param name="nonKey">A TKey that isn't in the dictionary</param>
        /// <param name="name">Name of the dictionary, used in exceptions.</param>
        public static void TestReadOnlyDictionary<TKey, TValue>(IDictionary dict, TKey[] keys, TValue[] values, TKey nonKey, bool mustBeInOrder, string name)
        {
            DictionaryEntry[] entries = new DictionaryEntry[keys.Length];
            for (int i = 0; i < keys.Length; ++i)
                entries[i] = new DictionaryEntry(keys[i], values[i]);

            TestCollection<DictionaryEntry>((ICollection)dict, entries, mustBeInOrder);

            TestDictionary<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder);

            Assert.IsTrue(dict.IsReadOnly);
            Assert.IsTrue(dict.IsFixedSize);

            // Check exceptions.
            try {
                dict.Clear();
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            try {
                dict.Add(keys[0], values[0]);
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            try {
                dict.Remove(keys[0]);
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            try {
                dict[keys[0]] = values[0];
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }
        }

        /// <summary>
        /// Test an read-write IDictionary that should contains the given keys and values, possibly in order.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys</typeparam>
        /// <typeparam name="TValue">Type of the values</typeparam>
        /// <param name="dict">IDictionary to test</param>
        /// <param name="keys">key values for the dictionary</param>
        /// <param name="values">values for the dictionary</param>
        /// <param name="mustBeInOrder">True if the entries must be in order.</param>
        /// <param name="nonKey">A TKey that isn't in the dictionary</param>
        /// <param name="name">Name of the dictionary, used in exceptions.</param>
        public static void TestReadWriteDictionary<TKey, TValue>(IDictionary dict, TKey[] keys, TValue[] values, TKey nonKey, bool mustBeInOrder, string name)
        {
            DictionaryEntry[] entries = new DictionaryEntry[keys.Length];
            for (int i = 0; i < keys.Length; ++i)
                entries[i] = new DictionaryEntry(keys[i], values[i]);

            TestCollection<DictionaryEntry>((ICollection)dict, entries, mustBeInOrder);
            TestDictionary<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder);

            Assert.IsFalse(dict.IsReadOnly);
            Assert.IsFalse(dict.IsFixedSize);

            // Check exceptions for adding existing elements.
            for (int i = 0; i < keys.Length; ++i) {
                try {
                    dict.Add(keys[i], values[i]);
                    Assert.Fail("should have thrown exception");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is ArgumentException);
                }
            }

            // Check Clear.
            dict.Clear();
            Assert.AreEqual(0, dict.Count);

            // Check Add with incorrect types.
            try {
                dict.Add(new object(), values[0]);
                Assert.Fail("should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                dict.Add(keys[0], new object());
                Assert.Fail("should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            // Check Add().
            for (int i = 0; i < keys.Length; ++i)
                dict.Add(keys[i], values[i]);

            TestCollection<DictionaryEntry>((ICollection)dict, entries, mustBeInOrder);
            TestDictionary<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder);

            // Check Remove. 2nd remove should do nothing.
            for (int i = 0; i < keys.Length; ++i) {
                dict.Remove(keys[i]);
                dict.Remove(keys[i]);
            }

            // Remove with incorrect type.
            dict.Remove(new object());

            Assert.AreEqual(0, dict.Count);

            // Check indexer with incorrect types.
            try {
                dict[new object()] = values[0];
                Assert.Fail("should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            try {
                dict[keys[0]] = new object();
                Assert.Fail("should have thrown exception");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }

            // Check adding via the indexer
            for (int i = 0; i < keys.Length; ++i)
                dict[keys[i]] = values[i];

            TestCollection<DictionaryEntry>((ICollection)dict, entries, mustBeInOrder);
            TestDictionary<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder);
        }

        /// <summary>
        /// Test an generic IDictionary&lt;K,V&gt; that should contains the given keys and values, possibly in order.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys</typeparam>
        /// <typeparam name="TValue">Type of the values</typeparam>
        /// <param name="dict">IDictionary&lt;K,V&gt; to test</param>
        /// <param name="keys">key values for the dictionary</param>
        /// <param name="values">values for the dictionary</param>
        /// <param name="nonKey">A TKey that isn't in the dictionary</param>
        /// <param name="mustBeInOrder">True if the entries must be in order.</param>
        public static void TestDictionaryGeneric<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey[] keys, TValue[] values, TKey nonKey, bool mustBeInOrder, BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            bool result;
            TValue val;

            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };

            // Check Count.
            Assert.AreEqual(keys.Length, dict.Count);

            // Check containment.
            for (int i = 0; i < keys.Length; ++i) {
                Assert.IsTrue(dict.ContainsKey(keys[i]));
                Assert.IsTrue(valueEquals(values[i], dict[keys[i]]));
                result = dict.TryGetValue(keys[i], out val);
                Assert.IsTrue(result);
                Assert.IsTrue(valueEquals(values[i], val));
            }

            Assert.IsFalse(dict.ContainsKey(nonKey));
            result = dict.TryGetValue(nonKey, out val);
            Assert.IsFalse(result);
            Assert.AreEqual(default(TValue), val);
            
            try {
                TValue v = dict[nonKey];
                Assert.Fail("Should throw.");
            }
            catch (Exception e) {
                Assert.IsTrue(e is KeyNotFoundException);
            }

            // Check Keys, Values collections
            TestReadonlyCollectionGeneric<TKey>(dict.Keys, keys, mustBeInOrder, null, keyEquals);
            TestReadonlyCollectionGeneric<TValue>(dict.Values, values, mustBeInOrder, null, valueEquals);
        }

        /// <summary>
        /// Test an read-only IDictionary&lt;K,V&gt; that should contains the given keys and values, possibly in order.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys</typeparam>
        /// <typeparam name="TValue">Type of the values</typeparam>
        /// <param name="dict">IDictionary&lt;K,V&gt; to test</param>
        /// <param name="keys">key values for the dictionary</param>
        /// <param name="values">values for the dictionary</param>
        /// <param name="mustBeInOrder">True if the entries must be in order.</param>
        /// <param name="nonKey">A TKey that isn't in the dictionary</param>
        /// <param name="name">Name of the dictionary, used in exceptions.</param>
        public static void TestReadOnlyDictionaryGeneric<TKey, TValue>(IDictionary<TKey,TValue> dict, TKey[] keys, TValue[] values, TKey nonKey, bool mustBeInOrder, string name,
            BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };

            KeyValuePair<TKey, TValue>[] entries = new KeyValuePair<TKey, TValue>[keys.Length];
            for (int i = 0; i < keys.Length; ++i)
                entries[i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);

            TestDictionaryGeneric<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder, keyEquals, valueEquals);
            TestReadonlyCollectionGeneric<KeyValuePair<TKey,TValue>>((ICollection<KeyValuePair<TKey,TValue>>)dict, entries, mustBeInOrder, name, KeyValueEquals(keyEquals, valueEquals));

            // Check exceptions.
            try {
                dict.Clear();
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            if (keys.Length > 0) {
                try {
                    dict.Add(keys[0], values[0]);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }

                try {
                    dict.Remove(keys[0]);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }

                try {
                    dict[keys[0]] = values[0];
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }
            }
        }

        /// <summary>
        /// Test an read-write IDictionary&lt;K,V&gt; that should contains the given keys and values, possibly in order.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys</typeparam>
        /// <typeparam name="TValue">Type of the values</typeparam>
        /// <param name="dict">IDictionary&lt;K,V&gt; to test</param>
        /// <param name="keys">key values for the dictionary</param>
        /// <param name="values">values for the dictionary</param>
        /// <param name="mustBeInOrder">True if the entries must be in order.</param>
        /// <param name="nonKey">A TKey that isn't in the dictionary</param>
        /// <param name="name">Name of the dictionary, used in exceptions.</param>
        public static void TestReadWriteDictionaryGeneric<TKey, TValue>(IDictionary<TKey,TValue> dict, TKey[] keys, TValue[] values, TKey nonKey, bool mustBeInOrder, string name, 
            BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };

            KeyValuePair<TKey, TValue>[] entries = new KeyValuePair<TKey, TValue>[keys.Length];
            for (int i = 0; i < keys.Length; ++i)
                entries[i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);

            TestDictionaryGeneric<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder, keyEquals, valueEquals);
            TestCollectionGeneric<KeyValuePair<TKey, TValue>>((ICollection<KeyValuePair<TKey, TValue>>)dict, entries, mustBeInOrder, KeyValueEquals(keyEquals, valueEquals));

            Assert.IsFalse(dict.IsReadOnly);

            // Check exceptions for adding existing elements.
            for (int i = 0; i < keys.Length; ++i) {
                try {
                    dict.Add(keys[i], values[i]);
                    Assert.Fail("should have thrown exception");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is ArgumentException);
                }
            }

            // Check Clear.
            dict.Clear();
            Assert.AreEqual(0, dict.Count);

            // Check Add().
            for (int i = 0; i < keys.Length; ++i)
                dict.Add(keys[i], values[i]);

            TestDictionaryGeneric<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder, keyEquals, valueEquals);
            TestCollectionGeneric<KeyValuePair<TKey, TValue>>((ICollection<KeyValuePair<TKey, TValue>>)dict, entries, mustBeInOrder, KeyValueEquals(keyEquals, valueEquals));

            // Check Remove. 2nd remove should return false.
            for (int i = 0; i < keys.Length; ++i) {
                Assert.IsTrue(dict.Remove(keys[i]));
                Assert.IsFalse(dict.Remove(keys[i]));
            }

            Assert.AreEqual(0, dict.Count);

            // Check adding via the indexer
            for (int i = 0; i < keys.Length; ++i)
                dict[keys[i]] = values[i];

            TestDictionaryGeneric<TKey, TValue>(dict, keys, values, nonKey, mustBeInOrder, keyEquals, valueEquals);
            TestCollectionGeneric<KeyValuePair<TKey, TValue>>((ICollection<KeyValuePair<TKey, TValue>>)dict, entries, mustBeInOrder, KeyValueEquals(keyEquals, valueEquals));
        }

        /// <summary>
        /// Test read-only IList&lt;T&gt; that should contain the given values, possibly in order. Does not change
        /// the list. Does not force the list to be read-only.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll">IList&lt;T&gt; to test. </param>
        /// <param name="valueArray">The values that should be in the list.</param>
        public static void TestListGeneric<T>(IList<T> coll, T[] valueArray)
        {
            TestListGeneric<T>(coll, valueArray, null);
        }

        public static void TestListGeneric<T>(IList<T> coll, T[] valueArray, BinaryPredicate<T> equals)
        {
            if (equals == null)
                equals = delegate(T x, T y) { return object.Equals(x, y); };

            // Check basic read-only collection stuff.
            TestCollectionGeneric<T>(coll, valueArray, true, equals);

            // Check the indexer getter and IndexOf, backwards
            for (int i = coll.Count - 1; i >= 0; --i) {
                Assert.IsTrue(equals(valueArray[i], coll[i]));
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && equals(coll[index], valueArray[i])));
            }

            // Check the indexer getter and IndexOf, forwards
            for (int i = 0; i < valueArray.Length ; ++i) {
                Assert.IsTrue(equals(valueArray[i], coll[i]));
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && equals(coll[index], valueArray[i])));
            }

            // Check the indexer getter and IndexOf, jumping by 3s
            for (int i = 0; i < valueArray.Length; i += 3) {
                Assert.IsTrue(equals(valueArray[i], coll[i]));
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && equals(coll[index], valueArray[i])));
            }

            // Check exceptions from index out of range.
            try {
                T dummy = coll[-1];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[int.MinValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[-2];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[coll.Count];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[int.MaxValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        /// <summary>
        /// Test read-only non-generic IList that should contain the given values, possibly in order. Does not change
        /// the list. Does not force the list to be read-only.
        /// </summary>
        /// <param name="coll">IList to test. </param>
        /// <param name="valueArray">The values that should be in the list.</param>
        public static void TestList<T>(IList coll, T[] valueArray)
        {
            // Check basic read-only collection stuff.
            TestCollection<T>(coll, valueArray, true);

            // Check the indexer getter and IndexOf, backwards
            for (int i = coll.Count - 1; i >= 0; --i) {
                Assert.AreEqual(valueArray[i], coll[i]);
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(coll.Contains(valueArray[i]));
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[i])));
            }

            // Check the indexer getter and IndexOf, forwards
            for (int i = 0; i < valueArray.Length; ++i) {
                Assert.AreEqual(valueArray[i], coll[i]);
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(coll.Contains(valueArray[i]));
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[i])));
            }

            // Check the indexer getter and IndexOf, jumping by 3s
            for (int i = 0; i < valueArray.Length; i += 3) {
                Assert.AreEqual(valueArray[i], coll[i]);
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(coll.Contains(valueArray[i]));
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[i])));
            }

            // Check exceptions from index out of range.
            try {
                object dummy = coll[-1];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[int.MinValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[-2];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[coll.Count];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[int.MaxValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            // Check bad type.
            if (typeof(T) != typeof(object)) {
                int index = coll.IndexOf(new object());
                Assert.AreEqual(-1, index);

                bool b = coll.Contains(new object());
                Assert.IsFalse(b);
            }
        }

        /// <summary>
        ///  Test a read-write IList&lt;T&gt; that should contain the given values, possibly in order. Destroys the collection in the process.
        /// </summary>
        /// <param name="coll">IList&lt;T&gt; to test. </param>
        /// <param name="valueArray">The values that should be in the list.</param>
        public static void TestReadWriteListGeneric<T>(IList<T> coll, T[] valueArray)
        {
            TestReadWriteListGeneric<T>(coll, valueArray, null);
        }

        public static void TestReadWriteListGeneric<T>(IList<T> coll, T[] valueArray, BinaryPredicate<T> equals)
        {
            if (equals == null)
                equals = delegate(T x, T y) { return object.Equals(x, y); };

            TestListGeneric(coll, valueArray, equals);     // Basic read-only list stuff.

            // Check the indexer getter.
            T[] save = new T[coll.Count];
            for (int i = coll.Count - 1; i >= 0; --i) {
                Assert.AreEqual(valueArray[i], coll[i]);
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[i])));
                save[i] = coll[i];
            }

            // Check the setter by reversing the list.
            for (int i = 0; i < coll.Count / 2; ++i) {
                T temp = coll[i];
                coll[i] = coll[coll.Count - 1 - i];
                coll[coll.Count - 1 - i] = temp;
            }

            for (int i = 0; i < coll.Count; ++i ) {
                Assert.AreEqual(valueArray[coll.Count - 1 - i], coll[i]);
                int index = coll.IndexOf(valueArray[coll.Count - 1 - i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[coll.Count - 1 - i])));
            }

            // Reverse back
            for (int i = 0; i < coll.Count / 2; ++i) {
                T temp = coll[i];
                coll[i] = coll[coll.Count - 1 - i];
                coll[coll.Count - 1 - i] = temp;
            }

            T item = valueArray.Length > 0 ? valueArray[valueArray.Length / 2] : default(T);
            // Check exceptions from index out of range.
            try {
                coll[-1] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll[int.MinValue] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[-2];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll[coll.Count] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[coll.Count];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                T dummy = coll[int.MaxValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll[int.MaxValue] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.Insert(-1, item);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.Insert(coll.Count + 1, item);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.Insert(int.MaxValue, item);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(coll.Count);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(-1);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(int.MaxValue);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(coll.Count);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            // Insert at the beginning.
            coll.Insert(0, item);
            Assert.AreEqual(coll[0], item);
            Assert.AreEqual(valueArray.Length + 1, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i + 1]);

            // Insert at the end
            coll.Insert(valueArray.Length + 1, item);
            Assert.AreEqual(coll[valueArray.Length + 1], item);
            Assert.AreEqual(valueArray.Length + 2, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i + 1]);

            // Delete at the beginning.
            coll.RemoveAt(0);
            Assert.AreEqual(coll[valueArray.Length], item);
            Assert.AreEqual(valueArray.Length + 1, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i]);

            // Delete at the end.
            coll.RemoveAt(valueArray.Length);
            Assert.AreEqual(valueArray.Length, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i]);

            // Insert at the middle.
            coll.Insert(valueArray.Length / 2, item);
            Assert.AreEqual(valueArray.Length + 1, coll.Count);
            Assert.AreEqual(item, coll[valueArray.Length / 2]);
            for (int i = 0; i < valueArray.Length; ++i) {
                if (i < valueArray.Length / 2)
                    Assert.AreEqual(valueArray[i], coll[i]);
                else
                    Assert.AreEqual(valueArray[i], coll[i+1]);
            }

            // Delete at the middle.
            coll.RemoveAt(valueArray.Length / 2);
            Assert.AreEqual(valueArray.Length, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i]);

            // Delete all from the middle.
            for (int i = 0; i < valueArray.Length; ++i)
                coll.RemoveAt(coll.Count / 2);
            Assert.AreEqual(0, coll.Count);

            // Build up in order.
            for (int i = 0; i < save.Length; ++i) {
                coll.Insert(i, save[i]);
            }

            TestListGeneric(coll, valueArray, equals);     // Basic read-only list stuff.

            coll.Clear();
            Assert.AreEqual(0, coll.Count);

            // Build up in reverse order.
            for (int i = 0; i < save.Length; ++i) {
                coll.Insert(0, save[save.Length - 1 - i]);
            }
            TestListGeneric(coll, valueArray, equals);     // Basic read-only list stuff.

            // Check read-write collection stuff.
            TestReadWriteCollectionGeneric<T>(coll, valueArray, true);
        }

        /// <summary>
        ///  Test a read-write non-generic IList that should contain the given values, possibly in order. Destroys the collection in the process.
        /// </summary>
        /// <param name="coll">IList to test. </param>
        /// <param name="valueArray">The values that should be in the list.</param>
        public static void TestReadWriteList<T>(IList coll, T[] valueArray)
        {
            TestList(coll, valueArray);     // Basic read-only list stuff.

            // Check read only
            Assert.IsFalse(coll.IsReadOnly);
            Assert.IsFalse(coll.IsReadOnly);

            // Check the indexer getter.
            T[] save = new T[coll.Count];
            for (int i = coll.Count - 1; i >= 0; --i) {
                Assert.AreEqual(valueArray[i], coll[i]);
                int index = coll.IndexOf(valueArray[i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[i])));
                save[i] = (T) coll[i];
            }

            // Check the setter by reversing the list.
            for (int i = 0; i < coll.Count / 2; ++i) {
                T temp = (T) coll[i];
                coll[i] = coll[coll.Count - 1 - i];
                coll[coll.Count - 1 - i] = temp;
            }

            for (int i = 0; i < coll.Count; ++i ) {
                Assert.AreEqual(valueArray[coll.Count - 1 - i], coll[i]);
                int index = coll.IndexOf(valueArray[coll.Count - 1 - i]);
                Assert.IsTrue(index >= 0);
                Assert.IsTrue(index == i || (index < i && object.Equals(coll[index], valueArray[coll.Count - 1 - i])));
            }

            // Reverse back
            for (int i = 0; i < coll.Count / 2; ++i) {
                T temp = (T) coll[i];
                coll[i] = coll[coll.Count - 1 - i];
                coll[coll.Count - 1 - i] = temp;
            }

            T item = valueArray.Length > 0 ? valueArray[valueArray.Length / 2] : default(T);
            // Check exceptions from index out of range.
            try {
                coll[-1] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll[int.MinValue] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[-2];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll[coll.Count] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[coll.Count];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                object dummy = coll[int.MaxValue];
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll[int.MaxValue] = item;
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.Insert(-1, item);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.Insert(coll.Count + 1, item);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.Insert(int.MaxValue, item);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(coll.Count);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(-1);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(int.MaxValue);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
            try {
                coll.RemoveAt(coll.Count);
                Assert.Fail("Should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            // Check operations with bad type.
            if (typeof(T) != typeof(object)) {
                try {
                    coll.Add(new object());
                    Assert.Fail("should throw");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is ArgumentException);
                }

                try {
                    coll.Insert(0, new object());
                    Assert.Fail("should throw");
                }
                catch (Exception e) {
                    Assert.IsTrue(e is ArgumentException);
                }

                int index = coll.IndexOf(new object());
                Assert.AreEqual(-1, index);

                coll.Remove(new object());
            }

            // Insert at the beginning.
            coll.Insert(0, item);
            Assert.AreEqual(coll[0], item);
            Assert.AreEqual(valueArray.Length + 1, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i + 1]);

            // Insert at the end
            coll.Insert(valueArray.Length + 1, item);
            Assert.AreEqual(coll[valueArray.Length + 1], item);
            Assert.AreEqual(valueArray.Length + 2, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i + 1]);

            // Delete at the beginning.
            coll.RemoveAt(0);
            Assert.AreEqual(coll[valueArray.Length], item);
            Assert.AreEqual(valueArray.Length + 1, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i]);

            // Delete at the end.
            coll.RemoveAt(valueArray.Length);
            Assert.AreEqual(valueArray.Length, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i]);

            // Insert at the middle.
            coll.Insert(valueArray.Length / 2, item);
            Assert.AreEqual(valueArray.Length + 1, coll.Count);
            Assert.AreEqual(item, coll[valueArray.Length / 2]);
            for (int i = 0; i < valueArray.Length; ++i) {
                if (i < valueArray.Length / 2)
                    Assert.AreEqual(valueArray[i], coll[i]);
                else
                    Assert.AreEqual(valueArray[i], coll[i+1]);
            }

            // Delete at the middle.
            coll.RemoveAt(valueArray.Length / 2);
            Assert.AreEqual(valueArray.Length, coll.Count);
            for (int i = 0; i < valueArray.Length; ++i)
                Assert.AreEqual(valueArray[i], coll[i]);

            // Delete all from the middle.
            for (int i = 0; i < valueArray.Length; ++i)
                coll.RemoveAt(coll.Count / 2);
            Assert.AreEqual(0, coll.Count);

            // Build up in order.
            for (int i = 0; i < save.Length; ++i) {
                coll.Insert(i, save[i]);
            }

            TestList<T>(coll, valueArray);     // Basic read-only list stuff.

            coll.Clear();
            Assert.AreEqual(0, coll.Count);

            // Build up in order with Add
            for (int i = 0; i < save.Length; ++i) {
                coll.Add(save[i]);
            }

            TestList<T>(coll, valueArray);     // Basic read-only list stuff.

            // Remove in order with Remove.
            for (int i = 0; i < valueArray.Length; ++i) {
                coll.Remove(valueArray[i]);
            }

            Assert.AreEqual(0, coll.Count);

            // Build up in reverse order with Insert
            for (int i = 0; i < save.Length; ++i) {
                coll.Insert(0, save[save.Length - 1 - i]);
            }
            TestList<T>(coll, valueArray);     // Basic read-only list stuff.

            // Check read-write collection stuff.
            TestCollection<T>(coll, valueArray, true);
        }

        /// <summary>
        /// Test read-only IList&lt;T&gt; that should contain the given values, possibly in order. Does not change
        /// the list. Forces the list to be read-only.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll">IList&lt;T&gt; to test. </param>
        /// <param name="valueArray">The values that should be in the list.</param>
        /// <param name="name">Name of the collection, for exceptions. Null to not check.</param>
        public static void TestReadOnlyListGeneric<T>(IList<T> coll, T[] valueArray, string name)
        {
            TestReadOnlyListGeneric<T>(coll, valueArray, name, null);
        }

        public static void TestReadOnlyListGeneric<T>(IList<T> coll, T[] valueArray, string name, BinaryPredicate<T> equals)
        {
            if (equals == null)
                equals = delegate(T x, T y) { return object.Equals(x, y); };

            // Basic list stuff.
            TestListGeneric<T>(coll, valueArray, equals);
            TestReadonlyCollectionGeneric<T>(coll, valueArray, true, name, equals);

            // Check read only and fixed size bits.
            Assert.IsTrue(coll.IsReadOnly);

            // Check exceptions.
            if (coll.Count > 0) {
                try {
                    coll.Clear();
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }
            }

            try {
                coll.Insert(0, default(T));
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            if (coll.Count > 0) {
                try {
                    coll.RemoveAt(0);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }

                try {
                    coll[0] = default(T);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }
            }
        }


        /// <summary>
        /// Test read-only non-generic IList; that should contain the given values, possibly in order. Does not change
        /// the list. Forces the list to be read-only.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll">IList to test. </param>
        /// <param name="valueArray">The values that should be in the list.</param>
        /// <param name="name">Name of the collection, for exceptions. Null to not check.</param>
        public static void TestReadOnlyList<T>(IList coll, T[] valueArray, string name)
        {
            // Basic list stuff.
            TestList<T>(coll, valueArray);
            TestCollection<T>(coll, valueArray, true);

            // Check read only and fixed size bits.
            Assert.IsTrue(coll.IsReadOnly);
            Assert.IsTrue(coll.IsFixedSize);

            // Check exceptions.
            try {
                coll.Clear();
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            // Check exceptions.
            try {
                coll.Clear();
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            try {
                coll.Insert(0, default(T));
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            try {
                coll.Add(default(T));
                Assert.Fail("Should throw exception");
            }
            catch (Exception e) {
                CheckReadonlyCollectionException(e, name);
            }

            if (coll.Count > 0) {
                try {
                    coll.RemoveAt(0);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }

                try {
                    coll.Remove(coll[0]);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }

                try {
                    coll[0] = default(T);
                    Assert.Fail("Should throw exception");
                }
                catch (Exception e) {
                    CheckReadonlyCollectionException(e, name);
                }
            }
        }

        public static void TestReadWriteMultiDictionaryGeneric<TKey, TValue>(IDictionary<TKey, ICollection<TValue>> dict, TKey[] keys, TValue[][] values, TKey nonKey, TValue nonValue, bool mustBeInOrder, string name,
            BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };
            BinaryPredicate<ICollection<TValue>> valueCollectionEquals = CollectionEquals(valueEquals, mustBeInOrder);

            TestReadWriteDictionaryGeneric<TKey, ICollection<TValue>>(dict, keys, values, nonKey, mustBeInOrder, name, keyEquals, valueCollectionEquals);
        }

        public static void TestReadOnlyMultiDictionaryGeneric<TKey, TValue>(IDictionary<TKey, ICollection<TValue>> dict, TKey[] keys, TValue[][] values, TKey nonKey, TValue nonValue, bool mustBeInOrder, string name,
            BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };
            BinaryPredicate<ICollection<TValue>> valueCollectionEquals = CollectionEquals(valueEquals, mustBeInOrder);

            TestReadOnlyDictionaryGeneric<TKey, ICollection<TValue>>(dict, keys, values, nonKey, mustBeInOrder, name, keyEquals, valueCollectionEquals);
        }

        public static void TestMultiDictionaryGeneric<TKey, TValue>(IDictionary<TKey, ICollection<TValue>> dict, TKey[] keys, TValue[][] values, TKey nonKey, TValue nonValue, bool mustBeInOrder, 
            BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };
            BinaryPredicate<ICollection<TValue>> valueCollectionEquals = CollectionEquals(valueEquals, mustBeInOrder);

            TestDictionaryGeneric<TKey, ICollection<TValue>>(dict, keys, values, nonKey, mustBeInOrder, keyEquals, valueCollectionEquals);
        }

        /// <summary>
        /// This class has Equal and GetHashCode semantics for identity semantics.
        /// </summary>
        [Serializable]
        public class Unique
        {
            public string val;

            public Unique(string v)
            {
                val = v;
            }

            public override string ToString()
            {
                return val;
            }

            static public bool EqualValues(Unique x, Unique y)
            {
                if (x == null || y == null)
                    return x == y;
                else 
                    return string.Equals(x.val, y.val);
            }
        }

        /// <summary>
        /// Round-trip serialize and deserialize an object.
        /// </summary>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <returns>Result of the serialization.</returns>
        public static object SerializeRoundTrip(object objToSerialize)
        {
            object result;

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream("TestSerialization.bin", FileMode.Create, FileAccess.Write, FileShare.None)) {
                formatter.Serialize(stream, objToSerialize);
            }

            formatter = new BinaryFormatter();
            using (Stream stream = new FileStream("TestSerialization.bin", FileMode.Open, FileAccess.Read, FileShare.Read)) {
                result = formatter.Deserialize(stream);
            }

            return result;
        }
    }
}

