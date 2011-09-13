//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

// Still need additional tests on ReadOnlyMultiDictionaryBase.

using System;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Wintellect.PowerCollections;

namespace Wintellect.PowerCollections.Tests
{
    class ReadWriteTestMultiDictionary<TKey, TValue> : MultiDictionaryBase<TKey, TValue>
    {
        List<TKey> keys;
        List<List<TValue>> values;

        public ReadWriteTestMultiDictionary(List<TKey> keys, List<List<TValue>> values)
        {
            this.keys = keys;
            this.values = values;
        }

        public override void Clear()
        {
            keys = new List<TKey>();
            values = new List<List<TValue>>();
        }

        public override int Count
        {
            get
            {
                return keys.Count;
            }
        }

        public override void Add(TKey key, TValue value)
        {
            int index = keys.IndexOf(key);
            if (index >= 0) {
                values[index].Add(value);
            }
            else {
                keys.Add(key);
                values.Add(new List<TValue>(new TValue[] { value }));
            }
        }

        public override bool Remove(TKey key)
        {
            int index = keys.IndexOf(key);

            if (index >= 0) {
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }
            else {
                return false;
            }
        }

        public override bool Remove(TKey key, TValue value)
        {
            int index = keys.IndexOf(key);

            if (index >= 0) {
                int valIndex = values[index].IndexOf(value);
                if (valIndex >= 0) {
                    values[index].RemoveAt(valIndex);
                    if (values[index].Count == 0)
                        Remove(key);
                    return true;
                }
            }

            return false;
        }

        public override bool Contains(TKey key, TValue value)
        {
            int index = keys.IndexOf(key);

            if (index >= 0) {
                int valIndex = values[index].IndexOf(value);
                if (valIndex >= 0) {
                    return true;
                }
            }

            return false;
        }

        protected override bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
        {
            int index = keys.IndexOf(key);

            if (index >= 0) {
                values = this.values[index].GetEnumerator();
                return true;
            }
            else {
                values = null;
                return false;
            }
        }

        protected override IEnumerator<TKey> EnumerateKeys()
        {
            for (int i = 0; i < keys.Count; ++i) 
                yield return keys[i];
        }
    }

    class ReadOnlyTestMultiDictionary<TKey, TValue> : ReadOnlyMultiDictionaryBase<TKey, TValue>
    {
        List<TKey> keys;
        List<List<TValue>> values;

        public ReadOnlyTestMultiDictionary(List<TKey> keys, List<List<TValue>> values)
        {
            this.keys = keys;
            this.values = values;
        }

        public override int Count
        {
            get
            {
                return keys.Count;
            }
        }

        public override bool Contains(TKey key, TValue value)
        {
            int index = keys.IndexOf(key);

            if (index >= 0) {
                int valIndex = values[index].IndexOf(value);
                if (valIndex >= 0) {
                    return true;
                }
            }

            return false;
        }

        protected override bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
        {
            int index = keys.IndexOf(key);

            if (index >= 0) {
                values = this.values[index].GetEnumerator();
                return true;
            }
            else {
                values = null;
                return false;
            }
        }

        protected override IEnumerator<TKey> EnumerateKeys()
        {
            for (int i = 0; i < keys.Count; ++i)
                yield return keys[i];
        }
    }

    [TestFixture]
    public class MultiDictionaryBaseTests
    {
        // Check the contents of a Multi-Dictionary non-destructively. Keys and Values must be in order.
        internal static void CheckOrderedMultiDictionaryContents<TKey, TValue>(MultiDictionaryBase<TKey, TValue> dict, TKey[] keys, TValue[][] values, TKey nonKey, TValue nonValue, BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            int iKey, iValue;
            ICollection<TValue> getValues;

            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };

            // Check Count.
            Assert.AreEqual(keys.Length, dict.Count);

            // Check indexer, ContainsKey, Contains, TryGetValue for each key.
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                Assert.IsTrue(dict.ContainsKey(keys[iKey]));
                Assert.IsTrue(dict.Contains(new KeyValuePair<TKey, ICollection<TValue>>(keys[iKey], values[iKey])));

                bool b = ((IDictionary<TKey, ICollection<TValue>>)dict).TryGetValue(keys[iKey], out getValues);
                Assert.IsTrue(b);
                iValue = 0;
                foreach(TValue val in getValues) {
                    Assert.IsTrue(valueEquals(values[iKey][iValue], val));
                    ++iValue;
                }

                iValue = 0;
                foreach (TValue val in values[iKey]) {
                    Assert.IsTrue(dict.Contains(keys[iKey], val));
                    ++iValue;
                }

                iValue = 0;
                foreach (TValue val in dict[keys[iKey]]) {
                    Assert.IsTrue(valueEquals(values[iKey][iValue], val));
                    ++iValue;
                }
                Assert.IsTrue(iValue == values[iKey].Length);
            }

            // Check Keys collection.
            iKey = 0;
            foreach (TKey key in dict.Keys) {
                Assert.IsTrue(keyEquals(keys[iKey], key));
                ++iKey;
            }
            Assert.IsTrue(iKey == keys.Length);
            InterfaceTests.TestReadonlyCollectionGeneric<TKey>(dict.Keys, keys, true, null);

            // Check Values collection
            iKey = 0; iValue = 0;
            int valueCount = 0;
            foreach (TValue val in dict.Values) {
                Assert.IsTrue(valueEquals(values[iKey][iValue], val));
                ++iValue;
                if (iValue == values[iKey].Length) {
                    iValue = 0;
                    ++iKey;
                }
                ++valueCount;
            }
            Assert.IsTrue(iKey == keys.Length);

            int a = 0;
            TValue[] vals = new TValue[valueCount];
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                for (iValue = 0; iValue < values[iKey].Length; ++iValue) {
                    vals[a++] = values[iKey][iValue];
                }
            }
            InterfaceTests.TestReadonlyCollectionGeneric<TValue>(dict.Values, vals, true, null);

            // Check KeyValuePairs collection.
            iKey = 0; iValue = 0;
            valueCount = 0;
            foreach (KeyValuePair<TKey,TValue> pair in dict.KeyValuePairs) {
                Assert.IsTrue(keyEquals(keys[iKey], pair.Key));
                Assert.IsTrue(valueEquals(values[iKey][iValue], pair.Value));
                ++iValue;
                if (iValue == values[iKey].Length) {
                    iValue = 0;
                    ++iKey;
                }
                ++valueCount;
            }
            Assert.IsTrue(iKey == keys.Length);

            a = 0;
            KeyValuePair<TKey,TValue>[] pairs = new KeyValuePair<TKey,TValue>[valueCount];
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                for (iValue = 0; iValue < values[iKey].Length; ++iValue) {
                    pairs[a++] = new KeyValuePair<TKey,TValue>(keys[iKey], values[iKey][iValue]);
                }
            }
            InterfaceTests.TestReadonlyCollectionGeneric<KeyValuePair<TKey,TValue>>(dict.KeyValuePairs, pairs, true, null);

            // Tests Contains, ContainsKey, TryGetValue for wrong values.
            Assert.IsFalse(dict.ContainsKey(nonKey));
            Assert.IsFalse(((IDictionary<TKey, ICollection<TValue>>)dict).TryGetValue(nonKey, out getValues));
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                Assert.IsFalse(dict.Contains(keys[iKey], nonValue));
                Assert.IsFalse(dict.Contains(new KeyValuePair<TKey, ICollection<TValue>>(keys[iKey], new TValue[1] { nonValue })));
            }

            // Test IDictionary<TKey,ICollection<TValue>> implementation
            InterfaceTests.TestMultiDictionaryGeneric<TKey,TValue>(dict, keys, values, nonKey, nonValue, true, null, null);
        }

        // Check the contents of a ReadOnly Multi-Dictionary non-destructively. Keys and Values must be in order.
        internal static void CheckOrderedReadOnlyMultiDictionaryContents<TKey, TValue>(ReadOnlyMultiDictionaryBase<TKey, TValue> dict, TKey[] keys, TValue[][] values, TKey nonKey, TValue nonValue, string name, BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
        {
            int iKey, iValue;
            ICollection<TValue> getValues;

            if (keyEquals == null)
                keyEquals = delegate(TKey x, TKey y) { return object.Equals(x, y); };
            if (valueEquals == null)
                valueEquals = delegate(TValue x, TValue y) { return object.Equals(x, y); };

            // Check Count.
            Assert.AreEqual(keys.Length, dict.Count);

            // Check indexer, ContainsKey, Contains, TryGetValue for each key.
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                Assert.IsTrue(dict.ContainsKey(keys[iKey]));
                Assert.IsTrue(dict.Contains(new KeyValuePair<TKey, ICollection<TValue>>(keys[iKey], values[iKey])));

                bool b = ((IDictionary<TKey,ICollection<TValue>>)dict).TryGetValue(keys[iKey], out getValues);
                Assert.IsTrue(b);
                iValue = 0;
                foreach (TValue val in getValues) {
                    Assert.IsTrue(valueEquals(values[iKey][iValue], val));
                    ++iValue;
                }

                iValue = 0;
                foreach (TValue val in values[iKey]) {
                    Assert.IsTrue(dict.Contains(keys[iKey], val));
                    ++iValue;
                }

                iValue = 0;
                foreach (TValue val in dict[keys[iKey]]) {
                    Assert.IsTrue(valueEquals(values[iKey][iValue], val));
                    ++iValue;
                }
                Assert.IsTrue(iValue == values[iKey].Length);
            }

            // Check Keys collection.
            iKey = 0;
            foreach (TKey key in dict.Keys) {
                Assert.IsTrue(keyEquals(keys[iKey], key));
                ++iKey;
            }
            Assert.IsTrue(iKey == keys.Length);
            InterfaceTests.TestReadonlyCollectionGeneric<TKey>(dict.Keys, keys, true, null);

            // Check Values collection
            iKey = 0; iValue = 0;
            int valueCount = 0;
            foreach (TValue val in dict.Values) {
                Assert.IsTrue(valueEquals(values[iKey][iValue], val));
                ++iValue;
                if (iValue == values[iKey].Length) {
                    iValue = 0;
                    ++iKey;
                }
                ++valueCount;
            }
            Assert.IsTrue(iKey == keys.Length);

            int a = 0;
            TValue[] vals = new TValue[valueCount];
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                for (iValue = 0; iValue < values[iKey].Length; ++iValue) {
                    vals[a++] = values[iKey][iValue];
                }
            }
            InterfaceTests.TestReadonlyCollectionGeneric<TValue>(dict.Values, vals, true, null);

            // Check KeyValuePairs collection.
            iKey = 0; iValue = 0;
            valueCount = 0;
            foreach (KeyValuePair<TKey, TValue> pair in dict.KeyValuePairs) {
                Assert.IsTrue(keyEquals(keys[iKey], pair.Key));
                Assert.IsTrue(valueEquals(values[iKey][iValue], pair.Value));
                ++iValue;
                if (iValue == values[iKey].Length) {
                    iValue = 0;
                    ++iKey;
                }
                ++valueCount;
            }
            Assert.IsTrue(iKey == keys.Length);

            a = 0;
            KeyValuePair<TKey, TValue>[] pairs = new KeyValuePair<TKey, TValue>[valueCount];
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                for (iValue = 0; iValue < values[iKey].Length; ++iValue) {
                    pairs[a++] = new KeyValuePair<TKey, TValue>(keys[iKey], values[iKey][iValue]);
                }
            }
            InterfaceTests.TestReadonlyCollectionGeneric<KeyValuePair<TKey, TValue>>(dict.KeyValuePairs, pairs, true, null);

            // Tests Contains, ContainsKey, TryGetValue for wrong values.
            Assert.IsFalse(dict.ContainsKey(nonKey));
            Assert.IsFalse(((IDictionary<TKey,ICollection<TValue>>)dict).TryGetValue(nonKey, out getValues));
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                Assert.IsFalse(dict.Contains(keys[iKey], nonValue));
                Assert.IsFalse(dict.Contains(new KeyValuePair<TKey, ICollection<TValue>>(keys[iKey], new TValue[1] { nonValue })));
            }

            // Test IDictionary<TKey,IEnumerable<TValue>> implementation
            InterfaceTests.TestReadOnlyMultiDictionaryGeneric<TKey, TValue>(dict, keys, values, nonKey, nonValue, true, name, null, null);
        }

        private ReadWriteTestMultiDictionary<string, int> CreateTestReadWriteDictionary()
        {
            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};
            List<string> s_list = new List<string>(s_array);
            List<List<int>> i_list = new List<List<int>>();
            foreach (int[] arr in i_array) {
                i_list.Add(new List<int>(arr));
            }

            return new ReadWriteTestMultiDictionary<string, int>(s_list, i_list);
        }

        private ReadOnlyTestMultiDictionary<string, int> CreateTestReadOnlyDictionary()
        {
            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};
            List<string> s_list = new List<string>(s_array);
            List<List<int>> i_list = new List<List<int>>();
            foreach (int[] arr in i_array) {
                i_list.Add(new List<int>(arr));
            }

            return new ReadOnlyTestMultiDictionary<string, int>(s_list, i_list);
        }

        [Test]
        public void ReadWriteDictionary()
        {
            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};

            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);

            InterfaceTests.TestReadWriteMultiDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", 113, true, "ReadWriteTestMultiDictionary", null, null);
            InterfaceTests.TestReadWriteMultiDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", 113, false, "ReadWriteTestMultiDictionary", null, null);
        }

        [Test]
        public void ReadOnlyDictionary()
        {
            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};

            ReadOnlyTestMultiDictionary<string, int> dict = CreateTestReadOnlyDictionary();

            CheckOrderedReadOnlyMultiDictionaryContents(dict, s_array, i_array, "foo", 113, "ReadOnlyTestMultiDictionary", null, null);

            InterfaceTests.TestReadOnlyMultiDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", 113, true, "ReadOnlyTestMultiDictionary", null, null);
            InterfaceTests.TestReadOnlyMultiDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", 113, false, "ReadOnlyTestMultiDictionary", null, null);
        }

        [Test]
        public void Add()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            dict.Add(new KeyValuePair<string,ICollection<int>>("Rules", new int[] {9, -8, 18}));
            dict.Add(new KeyValuePair<string, ICollection<int>>("World", new OrderedBag<int>(new int[] { })));
            dict.Add(new KeyValuePair<string, ICollection<int>>("Bizzle", new List<int>(new int[] { 3, 2, 1 })));
            dict.Add(new KeyValuePair<string, ICollection<int>>("Dazzle", new BigList<int>(new int[] { })));
            dict.Add("Eric", 16);
            dict.Add("The", 11);
            dict.Add("The", 22);
            dict.Add("Fizzle", 1);
            dict.Add("Fizzle", 11);
            dict.Add("Gizzle", -7);
            dict.Add("Bizzle", 8);

            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World", "Bizzle", "Fizzle", "Gizzle" };
            int[][] i_array = new int[][] { 
                                        new int[] { 1, 9, 11, 16 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4, 9, -8, 18}, 
                                        new int[] { 1, 2, 3, 4, 5, 6, 11, 22}, 
                                        new int[] { 8},
                                        new int[] {3, 2, 1, 8},
                                        new int[] {1, 11},
                                        new int[] {-7}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }

        [Test]
        public void AddMany()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            dict.AddMany("Rules", AlgorithmsTests.EnumerableFromArray(new int[] { 9, -8, 18 }));
            dict.AddMany("World", AlgorithmsTests.EnumerableFromArray(new int[] { }));
            dict.AddMany("Bizzle", AlgorithmsTests.EnumerableFromArray(new int[] { 3, 2, 1 }));
            dict.AddMany("Dazzle", AlgorithmsTests.EnumerableFromArray(new int[] { }));

            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World", "Bizzle" };
            int[][] i_array = new int[][] { 
                                        new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4, 9, -8, 18}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8},
                                        new int[] {3, 2, 1}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }

        [Test]
        public void Remove1()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            Assert.IsTrue(dict.Remove("Eric"));
            Assert.IsTrue(dict.Remove("Rules"));
            Assert.IsFalse(dict.Remove("Eric"));
            Assert.IsFalse(dict.Remove("foo"));
            Assert.IsTrue(dict.Remove("World", 8));
            Assert.IsTrue(dict.Remove("The", 2));
            Assert.IsTrue(dict.Remove("The", 6));
            Assert.IsFalse(dict.Remove("The", 6));
            Assert.IsFalse(dict.Remove("The", 11));

            string[] s_array = new string[] { "Clapton", "The" };
            int[][] i_array = new int[][] { 
                                        new int[] { 6, 10}, 
                                        new int[] { 1, 3, 4, 5}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }

        [Test]
        public void Remove2()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            Assert.IsTrue(dict.Remove(new KeyValuePair<string, ICollection<int>>("Eric", new int[] {9, 1, 11})));
            Assert.IsFalse(dict.Remove(new KeyValuePair<string, ICollection<int>>("Rules", new int[] { })));
            Assert.IsTrue(dict.Remove(new KeyValuePair<string, ICollection<int>>("The", new int[] { 4, 2, 11 })));
            Assert.IsFalse(dict.Remove(new KeyValuePair<string, ICollection<int>>("Clapton", new int[] { 0, 1 })));
            Assert.IsFalse(dict.Remove(new KeyValuePair<string, ICollection<int>>("foo", new int[] { 0, 1 })));

            string[] s_array = new string[] { "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 3, 5, 6}, 
                                        new int[] { 8}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }

        [Test]
        public void RemoveMany()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            Assert.AreEqual(3, dict.RemoveMany("Eric", new int[] { 9, 1, 11 }));
            Assert.AreEqual(0, dict.RemoveMany("Rules", new int[] { }));
            Assert.AreEqual(2, dict.RemoveMany("The", new int[] { 4, 2, 11 }));
            Assert.AreEqual(0, dict.RemoveMany("Clapton", new int[] { 0, 1 }));
            Assert.AreEqual(0, dict.RemoveMany("foo", new int[] { 0, 1 }));

            string[] s_array = new string[] { "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 3, 5, 6}, 
                                        new int[] { 8}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }

        [Test]
        public void Replace()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            Assert.IsTrue(dict.Replace("Eric", 18));
            Assert.IsTrue(dict.Replace("The", 7));
            Assert.IsFalse(dict.Replace("Fizzle", 100));

            string[] s_array = new string[] { "Clapton", "Rules", "World", "Eric", "The", "Fizzle" };
            int[][] i_array = new int[][] { 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 8},
                                        new int[] {18},
                                        new int[] {7},
                                        new int[] {100}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }


        [Test]
        public void ReplaceMany()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();

            Assert.IsTrue(dict.ReplaceMany("Eric", new int[] { 18, 13, 33 }));
            Assert.IsTrue(dict.ReplaceMany("The", new int[0]));
            Assert.IsFalse(dict.ReplaceMany("Fizzle", new int[] { 100, 2 }));

            string[] s_array = new string[] { "Clapton", "Rules", "World", "Eric", "Fizzle" };
            int[][] i_array = new int[][] { 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 8},
                                        new int[] {18, 13, 33},
                                        new int[] {100, 2}};

            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);
        }

        [Test]
        public void ValueCollection()
        {
            ReadWriteTestMultiDictionary<string, int> dict = CreateTestReadWriteDictionary();
            ICollection<int> valueColl = dict["Eric"];

            Assert.AreEqual(3, valueColl.Count);
            valueColl.Add(19);
            valueColl.Add(-4);
            Assert.IsTrue(valueColl.Remove(1));
            Assert.IsTrue(valueColl.Remove(19));
            valueColl.Add(12);

            string[] s_array = new string[] { "Eric", "Clapton", "Rules", "The", "World" };
            int[][] i_array = new int[][] { new int[] { 9, 11, -4, 12 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};
            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "foo", 113, null, null);

            dict.Remove("Eric", 12);
            dict.Add("Eric", 19);
            InterfaceTests.TestReadWriteCollectionGeneric(valueColl, new int[] { 9, 11, -4, 19 }, true);

            dict.Remove("Eric");
            InterfaceTests.TestReadWriteCollectionGeneric(valueColl, new int[] { }, true);
            InterfaceTests.TestReadWriteCollectionGeneric(dict["BananaZip"], new int[] { }, true);

            dict["The"].Clear();
            Assert.IsFalse(dict.ContainsKey("The"));

            valueColl = dict["Foo"];
            valueColl.Add(3);
            valueColl.Add(4);
            valueColl.Add(5);

            s_array = new string[] { "Clapton", "Rules", "World", "Foo"};
            i_array = new int[][] { 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 8}, 
                                        new int[] { 3, 4, 5}};
            CheckOrderedMultiDictionaryContents(dict, s_array, i_array, "fizzle", 113, null, null);

            ICollection<int> valueColl2 = dict["Foo"];
            Assert.IsFalse(object.ReferenceEquals(valueColl, valueColl2));

            valueColl2.Add(11);
            valueColl.Add(19);
            Assert.IsTrue(Algorithms.EqualCollections(valueColl, valueColl2));
        }

        [Test]
        public void ReadOnlyValueCollection()
        {
            ReadOnlyTestMultiDictionary<string, int> dict = CreateTestReadOnlyDictionary();
            ICollection<int> valueColl = dict["Eric"];

            InterfaceTests.TestReadonlyCollectionGeneric(valueColl, new int[] { 1, 9, 11 }, true, null);
        }

        [Test]
        public void ConvertToString()
        {
            string[] s_array = { "Eric", "null", "Rules", "The", "World" };
            int[][] i_array = { new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};
            List<string> s_list = new List<string>(s_array);
            List<List<int>> i_list = new List<List<int>>();
            foreach (int[] arr in i_array) {
                i_list.Add(new List<int>(arr));
            }

            ReadWriteTestMultiDictionary<string, int> dict = new ReadWriteTestMultiDictionary<string, int>(s_list, i_list);

            string s = dict.ToString();
            Assert.AreEqual("{Eric->(1,9,11), null->(6,10), Rules->(4), The->(1,2,3,4,5,6), World->(8)}", s);

            ReadOnlyTestMultiDictionary<string, int> dict2 = new ReadOnlyTestMultiDictionary<string, int>(s_list, i_list);

            s = dict2.ToString();
            Assert.AreEqual("{Eric->(1,9,11), null->(6,10), Rules->(4), The->(1,2,3,4,5,6), World->(8)}", s);

            ReadOnlyTestMultiDictionary<string, int> dict3 = new ReadOnlyTestMultiDictionary<string, int>(new List<string>(), new List<List<int>>());

            s = dict3.ToString();
            Assert.AreEqual("{}", s);
        }

        [Test]
        public void DebuggerDisplay()
        {
            string[] s_array = { "Eric", "null", "Rules", "The", "World" };
            int[][] i_array = { new int[] { 1, 9, 11 }, 
                                        new int[] { 6, 10}, 
                                        new int[] { 4}, 
                                        new int[] { 1, 2, 3, 4, 5, 6}, 
                                        new int[] { 8}};
            List<string> s_list = new List<string>(s_array);
            List<List<int>> i_list = new List<List<int>>();
            foreach (int[] arr in i_array) {
                i_list.Add(new List<int>(arr));
            }

            ReadWriteTestMultiDictionary<string, int> dict = new ReadWriteTestMultiDictionary<string, int>(s_list, i_list);

            string s = dict.DebuggerDisplayString();
            Assert.AreEqual("{Eric->(1,9,11), null->(6,10), Rules->(4), The->(1,2,3,4,5,6), World->(8)}", s);

            ReadOnlyTestMultiDictionary<string, int> dict2 = new ReadOnlyTestMultiDictionary<string, int>(s_list, i_list);

            s = dict2.DebuggerDisplayString();
            Assert.AreEqual("{Eric->(1,9,11), null->(6,10), Rules->(4), The->(1,2,3,4,5,6), World->(8)}", s);

            ReadWriteTestMultiDictionary<string, int> dict3 = new ReadWriteTestMultiDictionary<string, int>(new List<string>(), new List<List<int>>());

            s = dict3.DebuggerDisplayString();
            Assert.AreEqual("{}", s);

            ReadOnlyTestMultiDictionary<string, int> dict4 = new ReadOnlyTestMultiDictionary<string, int>(new List<string>(), new List<List<int>>());

            s = dict4.DebuggerDisplayString();
            Assert.AreEqual("{}", s);

            ReadWriteTestMultiDictionary<string, int> dict5 = new ReadWriteTestMultiDictionary<string, int>(new List<string>(), new List<List<int>>());
            for (int i = 0; i < 20; ++i) {
                for (int j = 0; j < i; ++j) {
                    dict5.Add(string.Format("foo{0}bar", i), j);
                }
            }

            s = dict5.DebuggerDisplayString();
            Assert.AreEqual("{foo1bar->(0), foo2bar->(0,1), foo3bar->(0,1,2), foo4bar->(0,1,2,3), foo5bar->(0,1,2,3,4), foo6bar->(0,1,2,3,4,5), foo7bar->(0,1,2,3,4,5,6), foo8bar->(0,1,2,3,4,5,6,7), foo9bar->(0,1,2,3,4,5,6,7,8), foo10bar->(0,1,2,3,4,5,6,7,8,9), foo11bar->(0,1,2,3,4,5,6,7,8,9,10), ...}", s);

            s_list = new List<string>(dict5.Keys);
            i_list = new List<List<int>>();
            foreach (string key in s_list) 
                i_list.Add(new List<int>(dict5[key]));

            ReadOnlyTestMultiDictionary<string, int> dict6 = new ReadOnlyTestMultiDictionary<string, int>(s_list, i_list);

            s = dict6.DebuggerDisplayString();
            Assert.AreEqual("{foo1bar->(0), foo2bar->(0,1), foo3bar->(0,1,2), foo4bar->(0,1,2,3), foo5bar->(0,1,2,3,4), foo6bar->(0,1,2,3,4,5), foo7bar->(0,1,2,3,4,5,6), foo8bar->(0,1,2,3,4,5,6,7), foo9bar->(0,1,2,3,4,5,6,7,8), foo10bar->(0,1,2,3,4,5,6,7,8,9), foo11bar->(0,1,2,3,4,5,6,7,8,9,10), ...}", s);

        }

    }
}
