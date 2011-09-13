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
using NUnit.Framework;
using Wintellect.PowerCollections.Tests;

namespace Wintellect.PowerCollections.Tests
{
    using MyInt = OrderedDictionaryTests.MyInt;

    [TestFixture]
    public class MultiDictionaryTests
    {
        // Check the contents of a Multi-Dictionary non-destructively. Keys and Values must be in order.
        internal static void CheckMultiDictionaryContents<TKey, TValue>(MultiDictionary<TKey, TValue> dict, TKey[] keys, TValue[][] values, TKey nonKey, TValue nonValue, BinaryPredicate<TKey> keyEquals, BinaryPredicate<TValue> valueEquals)
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
                InterfaceTests.TestEnumerableElementsAnyOrder(getValues, values[iKey]);

                iValue = 0;
                foreach (TValue val in values[iKey]) {
                    Assert.IsTrue(dict.Contains(keys[iKey], val));
                    ++iValue;
                }
                Assert.IsTrue(iValue == values[iKey].Length);

                iValue = 0;
                getValues = dict[keys[iKey]];
                InterfaceTests.TestEnumerableElementsAnyOrder(getValues, values[iKey]);
            }

            // Check Keys collection.
            InterfaceTests.TestReadonlyCollectionGeneric<TKey>(dict.Keys, keys, false, null, keyEquals);

            // Check Values collection
            int a = 0;
            TValue[] vals = new TValue[dict.Values.Count];
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                for (iValue = 0; iValue < values[iKey].Length; ++iValue) {
                    vals[a++] = values[iKey][iValue];
                }
            }
            Assert.AreEqual(dict.Values.Count, a);
            InterfaceTests.TestReadonlyCollectionGeneric<TValue>(dict.Values, vals, false, null, valueEquals);

            // Check KeyValuePairs collection.
            a = 0;
            KeyValuePair<TKey, TValue>[] pairs = new KeyValuePair<TKey, TValue>[dict.Values.Count];
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                for (iValue = 0; iValue < values[iKey].Length; ++iValue) {
                    pairs[a++] = new KeyValuePair<TKey, TValue>(keys[iKey], values[iKey][iValue]);
                }
            }
            InterfaceTests.TestReadonlyCollectionGeneric<KeyValuePair<TKey, TValue>>(dict.KeyValuePairs, pairs, false, null, InterfaceTests.KeyValueEquals<TKey,TValue>(keyEquals,valueEquals));

            // Tests Contains, ContainsKey, TryGetValue for wrong values.
            Assert.IsFalse(dict.ContainsKey(nonKey));
            Assert.IsFalse(((IDictionary<TKey, ICollection<TValue>>)dict).TryGetValue(nonKey, out getValues));
            for (iKey = 0; iKey < keys.Length; ++iKey) {
                Assert.IsFalse(dict.Contains(keys[iKey], nonValue));
                Assert.IsFalse(dict.Contains(new KeyValuePair<TKey, ICollection<TValue>>(keys[iKey], new TValue[1] { nonValue })));
            }

            // Test IDictionary<TKey,IEnumerable<TValue>> implementation
            InterfaceTests.TestReadWriteMultiDictionaryGeneric<TKey, TValue>(dict, keys, values, nonKey, nonValue, false, "MultiDictionary", keyEquals, valueEquals);
        }

        // Do random add,remove,replaces and create an array.
        private int[,] AddRemoveRandom(Random rand, MultiDictionary<int, string> dict, bool useDups, int iter)
        {
            const int MAXKEY = 100, MAXVAL = 50;
            int[,] counts = new int[MAXKEY, MAXVAL];

            for (int x = 0; x < iter; ++x) {
                int key = rand.Next(MAXKEY);
                int val = rand.Next(MAXVAL);
                string valString = string.Format("A{0:0000}", val);

                if (counts[key, val] == 0) {
                    if (rand.Next(30) == 0) {
                        // Do a replace
                        dict.Replace(key, valString);
                        for (int i = 0; i < MAXVAL; ++i)
                            counts[key, i] = 0;
                        counts[key, val] = 1;
                    }
                    else {
                        // Do an add
                        dict.Add(key, valString);
                        counts[key, val] = 1;
                    }
                }
                else {
                    if (rand.Next(30) == 0) {
                        // Do a replace
                        dict.Replace(key, valString);
                        for (int i = 0; i < MAXVAL; ++i)
                            counts[key, i] = 0;
                        counts[key, val] = 1;
                    }
                    else if (rand.Next(5) < 2) {
                        // Do an add
                        dict.Add(key, valString);
                        if (useDups)
                            counts[key, val] += 1;
                    }
                    else {
                        // Do a remove
                        dict.Remove(key, valString);
                        counts[key, val] -= 1;
                    }
                }
            }

            return counts;
        }

        // Check an ordered multi-dictionary against an array.
        private void CheckAgainstArray(MultiDictionary<int, string> dict, int[,] array)
        {
            List<string[]> values = new List<string[]>();
            List<int> keys = new List<int>();
            List<string> vals = new List<string>();

            for (int i = 0; i < array.GetLength(0); ++i) {
                bool hasval = false;
                for (int j = 0; j < array.GetLength(1); ++j) {
                    if (array[i, j] > 0) {
                        hasval = true;
                        for (int x = 0; x < array[i, j]; ++x)
                            vals.Add(string.Format("A{0:0000}", j));
                    }
                }

                if (hasval) {
                    keys.Add(i);
                    values.Add(vals.ToArray());
                    vals.Clear();
                }
            }

            int[] keysArray = keys.ToArray();
            string[][] valsArray = values.ToArray();

            CheckMultiDictionaryContents<int, string>(dict,
                keysArray,
                valsArray,
                -1, "Foo", null, null);
        }

        [Test]
        public void RandomAdd()
        {
            Random rand = new Random(14);
            MultiDictionary<int, string> dict = new MultiDictionary<int, string>(true);

            int[,] array = AddRemoveRandom(rand, dict, true, 3000);
            CheckAgainstArray(dict, array);
        }


        [Test]
        public void Add()
        {
            // Test without duplicate values.
            MultiDictionary<string, double> dict1 = new MultiDictionary<string, double>(false);

            dict1.Add("foo", 3.5);
            dict1.Add("foo", -1.2);
            dict1.Add(null, 11.1);
            dict1.Add("foo", 8.8);
            dict1.Add(null, 11.1);
            dict1.Add("bar", 9.8);
            dict1.Add("foo", 8.8);
            dict1.Add("gib", 7.1);
            dict1.Add("S", -9);
            dict1.Add(null, 5.5);

            CheckMultiDictionaryContents<string, double>(dict1,
                new string[] { null, "bar", "foo", "gib", "S" },
                new double[][] { new double[] { 5.5, 11.1 }, new double[] { 9.8 }, new double[] { -1.2, 3.5, 8.8 }, new double[] { 7.1 }, new double[] { -9 } },
                "zip", -100, null, null);

            // Test with duplicate values.
            dict1 = new MultiDictionary<string, double>(true);

            dict1.Add("foo", 3.5);
            dict1.Add("foo", -1.2);
            dict1.Add(null, 11.1);
            dict1.Add("foo", 8.8);
            dict1.Add(null, 11.1);
            dict1.Add("bar", 9.8);
            dict1.Add("foo", 8.8);
            dict1.Add("gib", 7.1);
            dict1.Add("S", -9);
            dict1.Add(null, 5.5);

            CheckMultiDictionaryContents<string, double>(dict1,
                new string[] { null, "bar", "foo", "gib", "S" },
                new double[][] { new double[] { 5.5, 11.1, 11.1 }, new double[] { 9.8 }, new double[] { -1.2, 3.5, 8.8, 8.8 }, new double[] { 7.1 }, new double[] { -9 } },
                "zip", -100, null, null);

            // Test duplicate values with distinct equal values.
            MultiDictionary<string, string> dict2 = new MultiDictionary<string, string>(false, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);
            dict2.Add("foo", "BAR");
            dict2.Add("Foo", "bar");
            InterfaceTests.TestEnumerableElementsAnyOrder<string>(dict2.Keys, new string[] { "foo" });
            InterfaceTests.TestEnumerableElementsAnyOrder<string>(dict2["FOO"], new string[] { "bar" });
            dict2 = new MultiDictionary<string, string>(true, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);
            dict2.Add("foo", "BAR");
            dict2.Add("Foo", "bar");
            InterfaceTests.TestEnumerableElementsAnyOrder<string>(dict2.Keys, new string[] { "foo" });
            InterfaceTests.TestEnumerableElementsAnyOrder<string>(dict2["FOO"], new string[] { "BAR", "bar" });
            InterfaceTests.TestEnumerableElementsAnyOrder<KeyValuePair<string, string>>
                (dict2.KeyValuePairs, new KeyValuePair<string, string>[] { new KeyValuePair<string,string>("foo", "BAR"),
                  new KeyValuePair<string,string>("foo", "bar")}, InterfaceTests.KeyValueEquals<string, string>());
        }

        [Test]
        public void AddMany1()
        {
            // Test without duplicate values.
            MultiDictionary<string, double> dict1 = new MultiDictionary<string, double>(false, StringComparer.InvariantCultureIgnoreCase);

            dict1.AddMany("foo", AlgorithmsTests.EnumerableFromArray(new double[] { 9.8, 1.2, -9, 9.8, -9, 4 }));
            dict1.AddMany("hi", new double[0]);
            dict1.AddMany("FOO", AlgorithmsTests.EnumerableFromArray(new double[] { 8, -9 }));

            Assert.AreEqual(1, dict1.Count);
            Assert.IsTrue(dict1.ContainsKey("foo"));
            Assert.IsFalse(dict1.ContainsKey("hi"));
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1.Keys, new string[] { "foo" });
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1["fOo"], new double[] { -9, 1.2, 4, 8, 9.8 });
            InterfaceTests.TestEnumerableElementsAnyOrder<KeyValuePair<string, double>>
                (dict1.KeyValuePairs, new KeyValuePair<string, double>[] { 
                            new KeyValuePair<string,double>("foo", -9),
                            new KeyValuePair<string,double>("foo", 1.2),
                            new KeyValuePair<string,double>("foo", 4),
                            new KeyValuePair<string,double>("foo",8),
                            new KeyValuePair<string,double>("foo",9.8)
                            });

            // Test with duplicate values
            dict1 = new MultiDictionary<string, double>(true, StringComparer.InvariantCultureIgnoreCase);

            dict1.AddMany("foo", AlgorithmsTests.EnumerableFromArray(new double[] { 9.8, 1.2, -9, 9.8, -9, 4 }));
            dict1.AddMany("hi", new double[0]);
            dict1.AddMany("a", new double[] { 2, 1, 2 });
            dict1.AddMany("FOO", AlgorithmsTests.EnumerableFromArray(new double[] { 8, -9 }));

            Assert.AreEqual(2, dict1.Count);
            Assert.IsTrue(dict1.ContainsKey("foo"));
            Assert.IsFalse(dict1.ContainsKey("hi"));
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1.Keys, new string[] { "a", "foo" });
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1["fOo"], new double[] { -9, -9, -9, 1.2, 4, 8, 9.8, 9.8 });
            InterfaceTests.TestEnumerableElementsAnyOrder<KeyValuePair<string, double>>
                (dict1.KeyValuePairs, new KeyValuePair<string, double>[] { 
                            new KeyValuePair<string,double>("a", 1),
                            new KeyValuePair<string,double>("a", 2),
                            new KeyValuePair<string,double>("a", 2),
                            new KeyValuePair<string,double>("foo", -9),
                            new KeyValuePair<string,double>("foo", -9),
                            new KeyValuePair<string,double>("foo", -9),
                            new KeyValuePair<string,double>("foo", 1.2),
                            new KeyValuePair<string,double>("foo", 4),
                            new KeyValuePair<string,double>("foo",8),
                            new KeyValuePair<string,double>("foo",9.8),
                            new KeyValuePair<string,double>("foo",9.8)
                            });
        }

        [Test]
        public void Replace()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 3);
            dict1.Add("foo", 1);

            dict1.Replace("foo", 13);
            dict1.Replace("z", 19);
            dict1.Replace("hello", 193);
            dict1.Replace("foo", 123);
            dict1.Add("foo", 123);

            CheckMultiDictionaryContents(dict1,
                new string[] { "bar", "foo", "hello", "z" },
                new int[][] { new int[] { 7, 8 }, new int[] { 123, 123 }, new int[] { 193 }, new int[] { 19 } },
                "sailor", 19921, null, null);
        }

        [Test]
        public void ReplaceMany()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(false);

            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 3);
            dict1.Add("foo", 1);
            dict1.Add("bill", 9);

            dict1.ReplaceMany("bill", new int[0]);
            dict1.ReplaceMany("foo", new int[] { 13, 4 });
            dict1.ReplaceMany("z", new int[] { 19 });
            dict1.ReplaceMany("hello", new int[] { 193, -11, 193 });
            dict1.ReplaceMany("goodbye", new int[0]);
            dict1.ReplaceMany("foo", new int[] { 123, 0, 4 });
            dict1.Add("foo", 29);

            CheckMultiDictionaryContents(dict1,
                new string[] { "bar", "foo", "hello", "z" },
                new int[][] { new int[] { 7, 8 }, new int[] { 0, 4, 29, 123 }, new int[] { -11, 193 }, new int[] { 19 } },
                "sailor", 19921, null, null);
        }

        [Test]
        public void RemoveKey()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 10);
            dict1.Add("z", 3);
            dict1.Add("foo", 4);
            dict1.Add("bill", 9);

            Assert.IsTrue(dict1.ContainsKey("bill"));
            Assert.IsTrue(dict1.ContainsKey("foo"));
            Assert.IsTrue(dict1.ContainsKey("z"));

            Assert.IsTrue(dict1.Remove("bill"));
            Assert.IsFalse(dict1.Remove("bill"));
            Assert.IsFalse(dict1.Remove("smell"));
            Assert.IsTrue(dict1.Remove("foo"));

            CheckMultiDictionaryContents(dict1,
                new string[] { "bar", "z" },
                new int[][] { new int[] { 7, 8 }, new int[] { 3, 3, 10 } },
                "sailor", 19921, null, null);
        }

        [Test]
        public void RemoveManyKeys()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 10);
            dict1.Add("z", 3);
            dict1.Add("foo", 4);
            dict1.Add("bill", 9);

            Assert.IsTrue(dict1.ContainsKey("bill"));
            Assert.IsTrue(dict1.ContainsKey("foo"));
            Assert.IsTrue(dict1.ContainsKey("z"));

            Assert.AreEqual(2, dict1.RemoveMany(new string[] { "bill", "smell", "foo", "bill" }));

            CheckMultiDictionaryContents(dict1,
                new string[] { "bar", "z" },
                new int[][] { new int[] { 7, 8 }, new int[] { 3, 3, 10 } },
                "sailor", 19921, null, null);
        }

        [Test]
        public void Remove()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 10);
            dict1.Add("z", 3);
            dict1.Add("foo", 4);
            dict1.Add("bill", 9);
            dict1.Add("foo", 4);

            Assert.IsTrue(dict1.Remove("foo", 4));
            Assert.IsTrue(dict1.Remove("foo", 4));
            Assert.IsTrue(dict1.Remove("z", 10));
            Assert.IsFalse(dict1.Remove("z", 10));
            Assert.IsFalse(dict1.Remove("foo", 11));
            Assert.IsFalse(dict1.Remove(null, 0));
            Assert.IsTrue(dict1.Remove("bill", 9));

            CheckMultiDictionaryContents(dict1,
                new string[] { "bar", "foo", "z" },
                new int[][] { new int[] { 7, 8 }, new int[] { 4, 6 }, new int[] { 3, 3 } },
                "sailor", 19921, null, null);
        }

        [Test]
        public void RemoveMany1()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("bill", 7);
            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 10);
            dict1.Add("z", 3);
            dict1.Add("foo", 4);
            dict1.Add("bill", 9);
            dict1.Add("foo", 4);

            Assert.AreEqual(2, dict1.RemoveMany("foo", new int[] { 4, 11, 4 }));
            Assert.AreEqual(1, dict1.RemoveMany("z", new int[] { 9, 2, 10 }));
            Assert.AreEqual(0, dict1.RemoveMany("z", new int[] { 10, 16, 144, 10 }));
            Assert.AreEqual(0, dict1.RemoveMany("foo", new int[0]));
            Assert.AreEqual(0, dict1.RemoveMany(null, new int[2] { 1, 2 }));
            Assert.AreEqual(2, dict1.RemoveMany("bill", new int[] { 9, 7 }));

            CheckMultiDictionaryContents(dict1,
                new string[] { "bar", "foo", "z" },
                new int[][] { new int[] { 7, 8 }, new int[] { 4, 6 }, new int[] { 3, 3 } },
                "sailor", 19921, null, null);
        }

        [Test]
        public void Clear()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add("bill", 7);
            dict1.Add("foo", 4);
            dict1.Add("bar", 7);
            dict1.Add("foo", 6);
            dict1.Add("z", 3);
            dict1.Add("bar", 8);
            dict1.Add("z", 10);
            dict1.Add(null, 3);
            dict1.Add("foo", 4);
            dict1.Add("bill", 9);
            dict1.Add("foo", 4);

            dict1.Clear();

            Assert.AreEqual(0, dict1.Count);
            Assert.IsFalse(dict1.ContainsKey("foo"));
            Assert.IsFalse(dict1.ContainsKey("z"));
            Assert.IsFalse(dict1.ContainsKey(null));
            Assert.AreEqual(0, Algorithms.Count(dict1.Keys));
            Assert.AreEqual(0, Algorithms.Count(dict1.Values));
            Assert.AreEqual(0, Algorithms.Count(dict1.KeyValuePairs));

            CheckMultiDictionaryContents(dict1, new string[0], new int[0][], "foo", 4, null, null);
        }

        [Test]
        public void Count()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add(null, 7);
            dict1.Add("bar", 11);
            dict1.Add("foo", 7);
            dict1.Add(null, 7);
            dict1.Add("hello", 11);
            dict1.Add("foo", 4);
            Assert.AreEqual(4, dict1.Count);

            MultiDictionary<string, int> dict2 = new MultiDictionary<string, int>(false);

            dict2.Add("foo", 4);
            dict2.Add(null, 7);
            dict2.Add("bar", 11);
            dict2.Add("foo", 7);
            dict2.Add(null, 7);
            dict2.Add("hello", 11);
            dict2.Add("foo", 4);
            Assert.AreEqual(4, dict2.Count);

            dict2.Remove("foo");
            Assert.AreEqual(3, dict2.Count);

            dict2.Clear();
            Assert.AreEqual(0, dict2.Count);
        }

        [Test]
        public void ContainsKey()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add(null, 7);
            dict1.Add("bar", 11);
            dict1.Add("foo", 7);
            dict1.Add(null, 7);
            dict1.Add("hello", 11);
            dict1.Add("foo", 4);

            Assert.IsTrue(dict1.ContainsKey(null));
            Assert.IsTrue(dict1.ContainsKey("foo"));
            Assert.IsTrue(dict1.ContainsKey("bar"));
            Assert.IsTrue(dict1.ContainsKey("hello"));
            dict1.Remove("hello", 11);
            Assert.IsFalse(dict1.ContainsKey("hello"));
            dict1.Remove(null, 7);
            Assert.IsTrue(dict1.ContainsKey(null));
            dict1.Remove(null, 7);
            Assert.IsFalse(dict1.ContainsKey(null));
        }

        [Test]
        public void Contains()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);

            dict1.Add("foo", 4);
            dict1.Add(null, 7);
            dict1.Add("bar", 11);
            dict1.Add("foo", 7);
            dict1.Add(null, 7);
            dict1.Add("hello", 11);
            dict1.Add("foo", 4);

            Assert.IsTrue(dict1.Contains(null, 7));
            Assert.IsTrue(dict1.Contains("foo", 4));
            Assert.IsTrue(dict1.Contains("bar", 11));
            Assert.IsTrue(dict1.Contains("hello", 11));
            Assert.IsFalse(dict1.Contains("HELLO", 11));
            Assert.IsFalse(dict1.Contains("bar", 12));
            Assert.IsFalse(dict1.Contains("foo", 0));
            dict1.Remove("hello", 11);
            Assert.IsFalse(dict1.Contains("hello", 11));
            dict1.Remove(null, 7);
            Assert.IsTrue(dict1.Contains(null, 7));
            dict1.Remove(null, 7);
            Assert.IsFalse(dict1.Contains(null, 7));
        }

        [Test]
        public void KeysCollection()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(false, StringComparer.InvariantCultureIgnoreCase);

            dict1.Add("foo", 4);
            dict1.Add(null, 2);
            dict1.Add("bar", 3);
            dict1.Add("sailor", 0);
            dict1.Add("FOO", 9);
            dict1.Add("b", 7);
            dict1.Add("Foo", -1);
            dict1.Add("BAR", 3);
            dict1.Remove("b", 7);

            InterfaceTests.TestReadonlyCollectionGeneric<string>(dict1.Keys, new string[] { null, "bar", "foo", "sailor" }, false, null);

            Assert.IsTrue(dict1.Keys.Contains("foo"));
            Assert.IsTrue(dict1.Keys.Contains("Foo"));
            Assert.IsTrue(dict1.Keys.Contains(null));
            Assert.IsTrue(dict1.Keys.Contains("Sailor"));
            Assert.IsFalse(dict1.Keys.Contains("banana"));

            MultiDictionary<string, int> dict2 = new MultiDictionary<string, int>(false, StringComparer.InvariantCultureIgnoreCase);
            InterfaceTests.TestEnumerableElementsAnyOrder(dict2.Keys, new string[] { });
        }

        [Test]
        public void ValuesCollection1()
        {
            MultiDictionary<double, string> dict = new MultiDictionary<double, string>(false, EqualityComparer<double>.Default, StringComparer.InvariantCultureIgnoreCase);

            dict.Add(7, "Gizzle");
            dict.Add(4, "foo");
            dict.Add(6, "Foo");
            dict.Add(3, "FOO");
            dict.Add(3, "baz");
            dict.Add(3, "bar");
            dict.Add(4, "FOo");
            dict.Add(3, "BAZ");
            dict.Add(5, "bAZ");
            dict.Add(7, "hello");
            dict.Add(7, "foo");

            ICollection<string> vals = dict.Values;

            string[] expected = {
                "bar", "BAZ", "FOO", "FOo", "bAZ", "Foo", "foo", "Gizzle", "hello"};

            InterfaceTests.TestReadonlyCollectionGeneric<string>(vals, expected, false, null);

            Assert.IsTrue(vals.Contains("gizzle"));
            Assert.IsTrue(vals.Contains("FOO"));
            Assert.IsTrue(vals.Contains("fOO"));
            Assert.IsTrue(vals.Contains("hello"));
            Assert.IsTrue(vals.Contains("bar"));
            Assert.IsTrue(vals.Contains("BAR"));
            Assert.IsFalse(vals.Contains("qatar"));
        }

        [Test]
        public void ValuesCollection2()
        {
            MultiDictionary<double, string> dict = new MultiDictionary<double, string>(true, EqualityComparer<double>.Default, StringComparer.InvariantCultureIgnoreCase);

            dict.Add(7, "Gizzle");
            dict.Add(4, "foo");
            dict.Add(6, "Foo");
            dict.Add(3, "FOO");
            dict.Add(3, "baz");
            dict.Add(3, "bar");
            dict.Add(4, "FOo");
            dict.Add(3, "BAZ");
            dict.Add(5, "bAZ");
            dict.Add(7, "hello");
            dict.Add(7, "foo");

            ICollection<string> vals = dict.Values;

            string[] expected = {
                "bar", "baz", "BAZ", "FOO", "foo", "FOo", "bAZ", "Foo", "foo", "Gizzle", "hello"};

            InterfaceTests.TestReadonlyCollectionGeneric<string>(vals, expected, false, null);

            Assert.IsTrue(vals.Contains("gizzle"));
            Assert.IsTrue(vals.Contains("FOO"));
            Assert.IsTrue(vals.Contains("fOO"));
            Assert.IsTrue(vals.Contains("hello"));
            Assert.IsTrue(vals.Contains("bar"));
            Assert.IsTrue(vals.Contains("BAR"));
            Assert.IsFalse(vals.Contains("qatar"));
        }

        [Test]
        public void KeyValuesCollection1()
        {
            MultiDictionary<string, string> dict = new MultiDictionary<string, string>(false, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

            dict.Add("7A", "Gizzle");
            dict.Add("4a", "foo");
            dict.Add("6A", "Foo");
            dict.Add("3a", "FOO");
            dict.Add("3A", "baz");
            dict.Add("3a", "bar");
            dict.Add("4a", "FOo");
            dict.Add("3A", "BAZ");
            dict.Add("5a", "bAZ");
            dict.Add("7a", "hello");
            dict.Add("7A", "foo");

            ICollection<KeyValuePair<string, string>> pairs = dict.KeyValuePairs;

            string[] expectedKeys = {
                "3a", "3a", "3a", "4a", "5a", "6A", "7A", "7A", "7A"};
            string[] expectedVals = {
                "bar", "BAZ", "FOO", "FOo", "bAZ", "Foo", "foo", "Gizzle", "hello"};
            KeyValuePair<string, string>[] expectedPairs = new KeyValuePair<string, string>[expectedKeys.Length];
            for (int i = 0; i < expectedKeys.Length; ++i)
                expectedPairs[i] = new KeyValuePair<string, string>(expectedKeys[i], expectedVals[i]);

            InterfaceTests.TestReadonlyCollectionGeneric<KeyValuePair<string, string>>(pairs, expectedPairs, false, null);

            Assert.IsTrue(pairs.Contains(new KeyValuePair<string, string>("3a", "baz")));
            Assert.IsTrue(pairs.Contains(new KeyValuePair<string, string>("3A", "baz")));
            Assert.IsTrue(pairs.Contains(new KeyValuePair<string, string>("6a", "foo")));
            Assert.IsFalse(pairs.Contains(new KeyValuePair<string, string>("7A", "bar")));
        }

        [Test]
        public void KeyValuesCollection2()
        {
            MultiDictionary<string, string> dict = new MultiDictionary<string, string>(true, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

            dict.Add("7A", "Gizzle");
            dict.Add("4A", "foo");
            dict.Add("6A", "Foo");
            dict.Add("3a", "FOO");
            dict.Add("3A", "baz");
            dict.Add("3a", "bar");
            dict.Add("4a", "FOo");
            dict.Add("3a", "BAZ");
            dict.Add("5a", "bAZ");
            dict.Add("7a", "hello");
            dict.Add("7A", "foo");

            ICollection<KeyValuePair<string, string>> pairs = dict.KeyValuePairs;

            string[] expectedKeys = {
            "3a", "3a", "3a", "3a", "4A", "4A", "5a", "6A", "7A", "7A", "7A"};
            string[] expectedVals = {
            "bar", "baz", "BAZ", "FOO", "foo", "FOo", "bAZ", "Foo", "foo", "Gizzle", "hello"};
            KeyValuePair<string, string>[] expectedPairs = new KeyValuePair<string, string>[expectedKeys.Length];
            for (int i = 0; i < expectedKeys.Length; ++i)
                expectedPairs[i] = new KeyValuePair<string, string>(expectedKeys[i], expectedVals[i]);

            InterfaceTests.TestReadonlyCollectionGeneric<KeyValuePair<string, string>>(pairs, expectedPairs, false, null);

            Assert.IsTrue(pairs.Contains(new KeyValuePair<string, string>("3a", "baz")));
            Assert.IsTrue(pairs.Contains(new KeyValuePair<string, string>("3A", "baz")));
            Assert.IsTrue(pairs.Contains(new KeyValuePair<string, string>("6a", "foo")));
            Assert.IsFalse(pairs.Contains(new KeyValuePair<string, string>("7A", "bar")));
        }

        [Test]
        public void Indexer()
        {
            MultiDictionary<string, string> dict1 = new MultiDictionary<string, string>(true, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

            dict1.Add("foo", "BAR");
            dict1.Add(null, "hello");
            dict1.Add("Hello", "sailor");
            dict1.Add(null, "hi");
            dict1.Add("foo", "bar");
            dict1.Add("HELLO", null);
            dict1.Add("foo", "a");
            dict1.Add("Foo", "A");
            dict1.Add("trail", "mix");

            InterfaceTests.TestEnumerableElementsAnyOrder(dict1[null], new string[] { "hello", "hi" });
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1["hELLo"], new string[] { null, "sailor" });
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1["foo"], new string[] { "a", "A", "BAR", "bar" });
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1["trail"], new string[] { "mix" });
            InterfaceTests.TestEnumerableElementsAnyOrder(dict1["nothing"], new string[] { });
        }

        [Test]
        public void GetValueCount()
        {
            MultiDictionary<string, string> dict1 = new MultiDictionary<string, string>(true, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

            dict1.Add("foo", "BAR");
            dict1.Add(null, "hello");
            dict1.Add("Hello", "sailor");
            dict1.Add(null, "hi");
            dict1.Add("foo", "bar");
            dict1.Add("HELLO", null);
            dict1.Add("foo", "a");
            dict1.Add("Foo", "A");
            dict1.Add("hello", null);
            dict1.Add("trail", "mix");

            Assert.AreEqual(2, dict1[null].Count);
            Assert.AreEqual(3, dict1["hELLo"].Count);
            Assert.AreEqual(4, dict1["foo"].Count);
            Assert.AreEqual(1, dict1["trail"].Count);
            Assert.AreEqual(0, dict1["nothing"].Count);

            dict1 = new MultiDictionary<string, string>(false, StringComparer.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

            dict1.Add("foo", "BAR");
            dict1.Add(null, "hello");
            dict1.Add("Hello", "sailor");
            dict1.Add(null, "hi");
            dict1.Add("foo", "bar");
            dict1.Add("HELLO", null);
            dict1.Add("foo", "a");
            dict1.Add("Foo", "A");
            dict1.Add("hello", null);
            dict1.Add("trail", "mix");

            Assert.AreEqual(2, dict1[null].Count);
            Assert.AreEqual(2, dict1["hELLo"].Count);
            Assert.AreEqual(2, dict1["foo"].Count);
            Assert.AreEqual(1, dict1["trail"].Count);
            Assert.AreEqual(0, dict1["nothing"].Count);

        }

        [Test]
        public void IMultiDictionaryInterface()
        {
            MultiDictionary<string, string> dict1 = new MultiDictionary<string, string>(true);

            dict1.Add("foo", "bar");
            dict1.Add(null, "hello");
            dict1.Add("hello", "sailor");
            dict1.Add(null, "hi");
            dict1.Add("foo", "bar");
            dict1.Add("hello", null);
            dict1.Add("foo", "a");
            dict1.Add("foo", "a");
            dict1.Add("hello", null);
            dict1.Add("trail", "mix");

            CheckMultiDictionaryContents<string, string>(dict1,
                new string[] { null, "foo", "hello", "trail" },
                new string[][] { new string[] { "hello", "hi" }, new string[] { "a", "a", "bar", "bar" }, new string[] { null, null, "sailor" }, new string[] { "mix" } },
                "zippy", "pinhead", null, null);

            dict1 = new MultiDictionary<string, string>(false);

            dict1.Add("foo", "bar");
            dict1.Add(null, "hello");
            dict1.Add("hello", "sailor");
            dict1.Add(null, "hi");
            dict1.Add("foo", "bar");
            dict1.Add("hello", null);
            dict1.Add("foo", "a");
            dict1.Add("foo", "a");
            dict1.Add("hello", null);

            dict1.Add("trail", "mix");
            CheckMultiDictionaryContents<string, string>(dict1,
                new string[] { null, "foo", "hello", "trail" },
                new string[][] { new string[] { "hello", "hi" }, new string[] { "a", "bar" }, new string[] { null, "sailor" }, new string[] { "mix" } },
                "zippy", "pinhead", null, null);

        }

        [Test]
        public void CustomComparer()
        {
            IEqualityComparer<string> firstLetterComparer = new FirstLetterComparer();

            MultiDictionary<string, string> dict1 = new MultiDictionary<string, string>(false, firstLetterComparer);

            dict1.Add("hello", "AAA");
            dict1.Add("hi", "aaa");
            dict1.Add("qubert", "hello");
            dict1.Add("queztel", "hello");
            dict1.Add("alpha", "omega");
            dict1.Add("alzabar", "oz");

            InterfaceTests.TestEnumerableElementsAnyOrder(dict1.KeyValuePairs, new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("qubert", "hello"),
                new KeyValuePair<string,string>("hello", "aaa"),
                new KeyValuePair<string,string>("hello", "AAA"),
                new KeyValuePair<string,string>("alpha", "omega"),
                new KeyValuePair<string,string>("alpha", "oz")});

            InterfaceTests.TestEnumerableElementsAnyOrder(dict1.Keys, new string[] { "qubert", "hello", "alpha" });

            MultiDictionary<string, string> dict2 = new MultiDictionary<string, string>(false, StringComparer.InvariantCultureIgnoreCase, firstLetterComparer);

            dict2.Add("qubert", "dinosaur");
            dict2.Add("Hello", "AAA");
            dict2.Add("Hi", "aaa");
            dict2.Add("qubert", "hello");
            dict2.Add("queztel", "hello");
            dict2.Add("alpha", "omega");
            dict2.Add("Alpha", "oz");
            dict2.Add("qubert", "hippy");

            InterfaceTests.TestEnumerableElementsAnyOrder(dict2.KeyValuePairs, new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("alpha", "oz"),
                new KeyValuePair<string,string>("Hello", "AAA"),
                new KeyValuePair<string,string>("Hi", "aaa"),
                new KeyValuePair<string,string>("qubert", "hippy"),
                new KeyValuePair<string,string>("qubert", "dinosaur"),
                new KeyValuePair<string,string>("queztel", "hello")});
        }

        [Test]
        public void NotComparable1()
        {
            // This should work -- all types are comparable in a hash way via object.Equals and object.GetHashCode.
            MultiDictionary<OrderedDictionaryTests.UncomparableClass1, string> dict1 = new MultiDictionary<OrderedDictionaryTests.UncomparableClass1, string>(false);
        }

        [Test]
        public void NotComparable2()
        {
            // This should work -- all types are comparable in a hash way via object.Equals and object.GetHashCode.
            MultiDictionary<string, OrderedDictionaryTests.UncomparableClass2> dict2 = new MultiDictionary<string, OrderedDictionaryTests.UncomparableClass2>(true);
        }

        class FirstLetterComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                if (x == null)
                    return y == null;
                else if (x.Length == 0)
                    return (y != null && y.Length == 0);
                else {
                    if (y == null || y.Length == 0)
                        return false;
                    else
                        return x[0] == y[0];
                }
            }

            public int GetHashCode(string obj)
            {
                if (obj == null)
                    return 0x12383;
                else if (obj.Length == 0)
                    return 17;
                else
                    return obj[0].GetHashCode();
            }
        }

        [Test]
        public void Clone()
        {
            IEqualityComparer<string> firstLetterComparer = new FirstLetterComparer();

            MultiDictionary<string, string> dict1 = new MultiDictionary<string, string>(false, StringComparer.InvariantCultureIgnoreCase, firstLetterComparer);

            dict1.Add("qubert", "dinosaur");
            dict1.Add("Hello", "AAA");
            dict1.Add("Hi", "aaa");
            dict1.Add("Qubert", "hello");
            dict1.Add("queztel", "hello");
            dict1.Add("Alpha", "omega");
            dict1.Add("alpha", "oz");
            dict1.Add("qubert", "hippy");

            MultiDictionary<string, string> dict2 = dict1.Clone();

            Assert.IsTrue(dict1 != dict2);

            dict2.Add("qubert", "hoover");
            dict2.Remove("queztel");
            dict2.Add("hello", "banana");

            InterfaceTests.TestEnumerableElementsAnyOrder(dict1.KeyValuePairs, new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("Alpha", "oz"),
                new KeyValuePair<string,string>("Hello", "AAA"),
                new KeyValuePair<string,string>("Hi", "aaa"),
                new KeyValuePair<string,string>("qubert", "hippy"),
                new KeyValuePair<string,string>("qubert", "dinosaur"),
                new KeyValuePair<string,string>("queztel", "hello")});

            InterfaceTests.TestEnumerableElementsAnyOrder(dict2.KeyValuePairs, new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("Alpha", "oz"),
                new KeyValuePair<string,string>("Hello", "banana"),
                new KeyValuePair<string,string>("Hello", "AAA"),
                new KeyValuePair<string,string>("Hi", "aaa"),
                new KeyValuePair<string,string>("qubert", "hoover"),
                new KeyValuePair<string,string>("qubert", "dinosaur")});

            dict2 = ((MultiDictionary<string, string>)((ICloneable)dict1).Clone());

            Assert.IsTrue(dict1 != dict2);

            dict2.Add("qubert", "hoover");
            dict2.Remove("queztel");
            dict2.Add("hello", "banana");

            InterfaceTests.TestEnumerableElementsAnyOrder(dict2.KeyValuePairs, new KeyValuePair<string, string>[] {
                new KeyValuePair<string,string>("Alpha", "oz"),
                new KeyValuePair<string,string>("Hello", "banana"),
                new KeyValuePair<string,string>("Hello", "AAA"),
                new KeyValuePair<string,string>("Hi", "aaa"),
                new KeyValuePair<string,string>("qubert", "hoover"),
                new KeyValuePair<string,string>("qubert", "dinosaur")});

            MultiDictionary<string, int> dict4 = new MultiDictionary<string, int>(true);
            MultiDictionary<string, int> dict5;
            dict5 = dict4.Clone();
            Assert.IsFalse(dict4 == dict5);
            Assert.IsTrue(dict4.Count == 0 && dict5.Count == 0);
            dict4.Add("hello", 1);
            Assert.IsTrue(dict4.Count == 1 && dict5.Count == 0);
            dict5.Add("hi", 7);
            dict4.Clear();
            Assert.IsTrue(dict4.Count == 0 && dict5.Count == 1);
        }

        void CompareClones<K, V>(MultiDictionary<K, V> d1, MultiDictionary<K, V> d2)
        {
            IEnumerable<KeyValuePair<K, V>> e1 = d1.KeyValuePairs;
            IEnumerable<KeyValuePair<K, V>> e2 = d2.KeyValuePairs;
            KeyValuePair<K, V>[] pairs1 = Algorithms.ToArray<KeyValuePair<K, V>>(e1), pairs2 = Algorithms.ToArray<KeyValuePair<K, V>>(e2);
            bool[] found = new bool[pairs2.Length];

            // Check that the arrays are equal, but not reference equals (e.g., have been cloned).
            Assert.IsTrue(pairs1.Length == pairs2.Length);
            foreach (KeyValuePair<K, V> p1 in pairs1) {
                bool f = false;
                for (int i = 0; i < pairs2.Length; ++i) {
                    if (!found[i] && object.Equals(p1.Key, pairs2[i].Key) && object.Equals(p1.Value, pairs2[i].Value)) {
                        found[i] = true;
                        f = true;
                        Assert.IsTrue(p1.Key == null || !object.ReferenceEquals(p1.Key, pairs2[i].Key));
                        Assert.IsTrue(p1.Value == null || !object.ReferenceEquals(p1.Value, pairs2[i].Value));
                        break;
                    }
                }
                Assert.IsTrue(f);
            }
        }

        private class MyIntComparer : IEqualityComparer<MyInt>
        {
            public bool Equals(MyInt x, MyInt y)
            {
                if (x == null)
                    return y == null;
                else if (y == null)
                    return x == null;
                else
                    return x.value == y.value;
            }

            public int GetHashCode(MyInt obj)
            {
                if (obj == null)
                    return 0x42834E;
                else
                    return obj.value.GetHashCode();
            }
        }

        [Test]
        public void CloneContents()
        {
            MyIntComparer comparer = new MyIntComparer();
            MultiDictionary<int, MyInt> dict1 = new MultiDictionary<int, MyInt>(true, EqualityComparer<int>.Default, comparer);

            dict1.Add(4, new MyInt(143));
            dict1.Add(7, new MyInt(2));
            dict1.Add(11, new MyInt(9));
            dict1.Add(7, new MyInt(119));
            dict1.Add(18, null);
            dict1.Add(4, new MyInt(16));
            dict1.Add(7, null);
            dict1.Add(7, new MyInt(119));
            MultiDictionary<int, MyInt> dict2 = dict1.CloneContents();
            CompareClones(dict1, dict2);

            MultiDictionary<MyInt, int> dict3 = new MultiDictionary<MyInt, int>(false, comparer);

            dict3.Add(new MyInt(4), 143);
            dict3.Add(new MyInt(7), 2);
            dict3.Add(new MyInt(11), 9);
            dict3.Add(new MyInt(7), 119);
            dict3.Add(new MyInt(18), 0);
            dict3.Add(new MyInt(4), 16);
            dict3.Add(null, 11);
            dict3.Add(new MyInt(7), 119);

            MultiDictionary<MyInt, int> dict4 = dict3.CloneContents();
            CompareClones(dict3, dict4);

            MultiDictionary<UtilTests.CloneableStruct, UtilTests.CloneableStruct> dict5 = new MultiDictionary<UtilTests.CloneableStruct, UtilTests.CloneableStruct>(true);
            dict5.Add(new UtilTests.CloneableStruct(7), new UtilTests.CloneableStruct(-14));
            dict5.Add(new UtilTests.CloneableStruct(16), new UtilTests.CloneableStruct(13));
            dict5.Add(new UtilTests.CloneableStruct(7), new UtilTests.CloneableStruct(-14));
            dict5.Add(new UtilTests.CloneableStruct(7), new UtilTests.CloneableStruct(31415));
            dict5.Add(new UtilTests.CloneableStruct(1111), new UtilTests.CloneableStruct(0));
            MultiDictionary<UtilTests.CloneableStruct, UtilTests.CloneableStruct> dict6 = dict5.CloneContents();

            Assert.IsTrue(dict5.Count == dict6.Count);

            IEnumerable<KeyValuePair<UtilTests.CloneableStruct, UtilTests.CloneableStruct>> e1 = dict5.KeyValuePairs;
            IEnumerable<KeyValuePair<UtilTests.CloneableStruct, UtilTests.CloneableStruct>> e2 = dict6.KeyValuePairs;
            KeyValuePair<UtilTests.CloneableStruct, UtilTests.CloneableStruct>[] pairs1 = Algorithms.ToArray<KeyValuePair<UtilTests.CloneableStruct, UtilTests.CloneableStruct>>(e1), pairs2 = Algorithms.ToArray<KeyValuePair<UtilTests.CloneableStruct, UtilTests.CloneableStruct>>(e2);
            bool[] found = new bool[pairs2.Length];

            // Check that the arrays are equal, but not reference equals (e.g., have been cloned).
            Assert.IsTrue(pairs1.Length == pairs2.Length);
            foreach (KeyValuePair<UtilTests.CloneableStruct, UtilTests.CloneableStruct> p1 in pairs1) {
                bool f = false;
                for (int i = 0; i < pairs2.Length; ++i) {
                    if (!found[i] && object.Equals(p1.Key, pairs2[i].Key) && object.Equals(p1.Value, pairs2[i].Value)) {
                        found[i] = true;
                        f = true;
                        Assert.IsFalse(p1.Key.Identical(pairs2[i].Key));
                        Assert.IsFalse(p1.Value.Identical(pairs2[i].Value));
                        break;
                    }
                }
                Assert.IsTrue(f);
            }

        }

        class NotCloneable { }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CantCloneContents()
        {
            MultiDictionary<int, NotCloneable> dict1 = new MultiDictionary<int, NotCloneable>(true);

            dict1[4] = new NotCloneable[] { new NotCloneable() };
            dict1[5] = new NotCloneable[] { new NotCloneable(), new NotCloneable() };

            MultiDictionary<int, NotCloneable> dict2 = dict1.CloneContents();
        }


        [Test]
        public void FailFastEnumerator()
        {
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true);
            bool throwNow;

            dict1.Add("foo", 12);
            dict1.Add("foo", 15);
            dict1.Add("foo", 3);
            dict1.Add("foo", 12);
            dict1.Add("bar", 1);
            dict1.Add("bar", 17);

            throwNow = false;
            try {
                foreach (KeyValuePair<string, int> pair in dict1.KeyValuePairs) {
                    throwNow = false;
                    if (pair.Key == "foo") {
                        dict1.Replace("bar", 19);
                        throwNow = true;
                    }
                }
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(throwNow);
            }

            throwNow = false;
            try {
                foreach (string key in dict1.Keys) {
                    throwNow = false;
                    if (key == "foo") {
                        dict1.Add("grump", 117);
                        throwNow = true;
                    }
                }
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(throwNow);
            }

            throwNow = false;
            try {
                foreach (int value in dict1["foo"]) {
                    throwNow = false;
                    if (value == 12) {
                        dict1.Remove("grump", 117);
                        throwNow = true;
                    }
                }
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(throwNow);
            }

            throwNow = false;
            try {
                foreach (string key in dict1.Keys) {
                    throwNow = false;
                    if (key == "foo") {
                        dict1.Clear();
                        throwNow = true;
                    }
                }
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(throwNow);
            }

        }

        [Test]
        public void KeyComparerProperty()
        {
            IEqualityComparer<int> comparer1 = new GOddEvenEqualityComparer();
            MultiDictionary<int, string> dict1 = new MultiDictionary<int, string>(false, comparer1);
            Assert.AreSame(comparer1, dict1.KeyComparer);
            MultiDictionary<decimal, string> dict2 = new MultiDictionary<decimal, string>(true);
            Assert.AreSame(EqualityComparer<decimal>.Default, dict2.KeyComparer);
            MultiDictionary<string, string> dict3 = new MultiDictionary<string, string>(true, StringComparer.OrdinalIgnoreCase, StringComparer.CurrentCulture);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, dict3.KeyComparer);

            Assert.AreSame(dict1.KeyComparer, dict1.Clone().KeyComparer);
            Assert.AreSame(dict2.KeyComparer, dict2.Clone().KeyComparer);
            Assert.AreSame(dict3.KeyComparer, dict3.Clone().KeyComparer);
        }

        [Test]
        public void ValueComparerProperty()
        {
            IEqualityComparer<int> comparer1 = new GOddEvenEqualityComparer();
            MultiDictionary<string, int> dict1 = new MultiDictionary<string, int>(true, StringComparer.InvariantCulture, comparer1);
            Assert.AreSame(comparer1, dict1.ValueComparer);
            MultiDictionary<string, decimal> dict2 = new MultiDictionary<string, decimal>(false);
            Assert.AreSame(EqualityComparer<decimal>.Default, dict2.ValueComparer);
            MultiDictionary<string, string> dict3 = new MultiDictionary<string, string>(true, StringComparer.InvariantCulture, StringComparer.OrdinalIgnoreCase);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, dict3.ValueComparer);

            Assert.AreSame(dict1.ValueComparer, dict1.Clone().ValueComparer);
            Assert.AreSame(dict2.ValueComparer, dict2.Clone().ValueComparer);
            Assert.AreSame(dict3.ValueComparer, dict3.Clone().ValueComparer);
        }

        [Test]
        public void SerializeStrings()
        {
            MultiDictionary<string,double> d = new MultiDictionary<string,double>(true,StringComparer.InvariantCultureIgnoreCase);

            d.Add("hEllo", 13);
            d.Add("foo", 7);
            d.Add("world", -9.5);
            d.Add("hello", 11);
            d.Add("elvis", 0.9);
            d.Add("ELVIS", 1.4);
            d.Add(null, 1.4);
            d.Add("FOO", 7);
            d.Add("hello", 12);

            MultiDictionary<string, double> result = (MultiDictionary<string, double>)InterfaceTests.SerializeRoundTrip(d);

            CheckMultiDictionaryContents<String,double>(result, 
                new string[] {"FOO", "WORLD", "Hello", "eLVis", null},
                new double[][] {new double[]{7,7}, new double[]{-9.5}, new double[] {11,12,13}, new double[] {0.9, 1.4}, new double[] {1.4}}, 
                "zippy", 123, StringComparer.InvariantCultureIgnoreCase.Equals, null);
        }

        [Serializable]
        class UniqueStuff
        {
            public InterfaceTests.Unique[] keys;
            public InterfaceTests.Unique[] values;
            public MultiDictionary<InterfaceTests.Unique, InterfaceTests.Unique> dict;
        }


        [Test]
        public void SerializeUnique()
        {
            UniqueStuff d = new UniqueStuff(), result = new UniqueStuff();

            d.keys = new InterfaceTests.Unique[] {
                new InterfaceTests.Unique("1"), new InterfaceTests.Unique("2"), new InterfaceTests.Unique("3"), new InterfaceTests.Unique("4"), new InterfaceTests.Unique("5")
            };

            d.values = new InterfaceTests.Unique[] {
                new InterfaceTests.Unique("a"), new InterfaceTests.Unique("b"), new InterfaceTests.Unique("c"), new InterfaceTests.Unique("d"), new InterfaceTests.Unique("e"), new InterfaceTests.Unique("f"), new InterfaceTests.Unique("g")
            };


            d.dict = new MultiDictionary<InterfaceTests.Unique, InterfaceTests.Unique>(true);

            d.dict.Add(d.keys[2], d.values[4]);
            d.dict.Add(d.keys[0], d.values[0]);
            d.dict.Add(d.keys[1], d.values[1]);
            d.dict.Add(d.keys[2], d.values[2]);
            d.dict.Add(d.keys[3], d.values[5]);
            d.dict.Add(d.keys[3], d.values[6]);
            d.dict.Add(d.keys[4], d.values[6]);
            d.dict.Add(d.keys[0], d.values[0]);
            d.dict.Add(d.keys[2], d.values[3]);

            result = (UniqueStuff)InterfaceTests.SerializeRoundTrip(d);

            CheckMultiDictionaryContents<InterfaceTests.Unique, InterfaceTests.Unique>(result.dict,
                result.keys,
                new InterfaceTests.Unique[][] { new InterfaceTests.Unique[] { result.values[0], result.values[0] }, new InterfaceTests.Unique[] { result.values[1] }, new InterfaceTests.Unique[] { result.values[2], result.values[3], result.values[4] }, new InterfaceTests.Unique[] { result.values[5], result.values[6] }, new InterfaceTests.Unique[] { result.values[6] } },
                new InterfaceTests.Unique("foo"), new InterfaceTests.Unique("bar"), null, null);

            for (int i = 0; i < result.keys.Length; ++i) {
                if (result.keys[i] != null)
                    Assert.IsFalse(object.Equals(result.keys[i], d.keys[i]));
            }

            for (int i = 0; i < result.values.Length; ++i) {
                if (result.values[i] != null)
                    Assert.IsFalse(object.Equals(result.values[i], d.values[i]));
            }
        }
        
    }
}

