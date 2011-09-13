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
using Wintellect.PowerCollections;

namespace Wintellect.PowerCollections.Tests
{
    // A simple read-only dictionary.
    class ReadOnlyTestDictionary<TKey, TValue> : ReadOnlyDictionaryBase<TKey, TValue>
    {
        private TKey[] keys;
        private TValue[] values;

        public ReadOnlyTestDictionary(TKey[] keys, TValue[] values) 
        {
            this.keys = keys;
            this.values = values;
        }

        public override int Count
        {
            get { return keys.Length; }
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < keys.Length; ++i)
                yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            int index = Array.IndexOf(keys, key);

            if (index >= 0) {
                value = values[index];
                return true;
            }
            else {
                value = default(TValue);
                return false;
            }
        }
    }

    // A simple read-write dictionary.
    class ReadWriteTestDictionary<TKey, TValue> : DictionaryBase<TKey, TValue>
    {
        private List<TKey> keys;
        private List<TValue> values;

        public ReadWriteTestDictionary(TKey[] keys, TValue[] values) 
        {
            this.keys = new List<TKey>(keys);
            this.values = new List<TValue>(values);
        }

        public override int Count
        {
            get { return keys.Count; }
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < keys.Count; ++i)
                yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            int index = keys.IndexOf(key);
            if (index < 0) {
                value = default(TValue);
                return false;
            }
            else {
                value = values[index];
                return true;
            }
        }

        public override TValue this[TKey key]
        {
            set
            {
                int index = keys.IndexOf(key);
                if (index < 0) {
                    keys.Add(key);
                    values.Add(value);
                }
                else
                    values[index] = value;
            }
        }

        public override bool Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index < 0) {
                return false;
            }
            else {
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }
        }

        public override void Clear()
        {
            keys.Clear();
            values.Clear();
        }
    }

    [TestFixture]
    public class DictionaryBaseTests
    {
        [Test]
        public void ReadOnlyDictionary()
        {
            string[] s_array = { "Eric", "Clapton", "Rules", "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };

            ReadOnlyTestDictionary<string, int> dict = new ReadOnlyTestDictionary<string, int>(s_array, i_array);

            InterfaceTests.TestReadOnlyDictionary<string, int>(dict, s_array, i_array, "foo", true, "ReadOnlyTestDictionary");
            InterfaceTests.TestReadOnlyDictionary<string, int>(dict, s_array, i_array, "foo", false, "ReadOnlyTestDictionary");
            InterfaceTests.TestReadOnlyDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", true, "ReadOnlyTestDictionary", null, null);
            InterfaceTests.TestReadOnlyDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", false, "ReadOnlyTestDictionary", null, null);
        }

        [Test]
        public void ReadWriteDictionary()
        {
            string[] s_array = { "Eric", "Clapton", "Rules", "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };

            ReadWriteTestDictionary<string, int> dict = new ReadWriteTestDictionary<string, int>(s_array, i_array);

            InterfaceTests.TestReadWriteDictionary<string, int>(dict, s_array, i_array, "foo", true, "ReadOnlyTestDictionary");
            InterfaceTests.TestReadWriteDictionary<string, int>(dict, s_array, i_array, "foo", false, "ReadOnlyTestDictionary");
            InterfaceTests.TestReadWriteDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", true, "ReadOnlyTestDictionary", null, null);
            InterfaceTests.TestReadWriteDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", false, "ReadOnlyTestDictionary", null, null);
        }

        [Test]
        public void ConvertToString()
        {
            string[] s_array = { "Eric", "Clapton", null, "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };
            string s;

            ReadWriteTestDictionary<string, int> dict1 = new ReadWriteTestDictionary<string, int>(s_array, i_array);
            s = dict1.ToString();
            Assert.AreEqual("{Eric->1, Clapton->5, null->6, The->5, World->19}", s);

            ReadOnlyTestDictionary<int, string> dict2 = new ReadOnlyTestDictionary<int, string>(i_array, s_array);
            s = dict2.ToString();
            Assert.AreEqual("{1->Eric, 5->Clapton, 6->null, 5->The, 19->World}", s);
        }

        [Test]
        public void DebuggerDisplayString()
        {
            string[] s_array = { "Eric", "Clapton", null, "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };
            string s;

            ReadWriteTestDictionary<string, int> dict1 = new ReadWriteTestDictionary<string, int>(s_array, i_array);
            s = dict1.ToString();
            Assert.AreEqual("{Eric->1, Clapton->5, null->6, The->5, World->19}", s);

            ReadOnlyTestDictionary<int, string> dict2 = new ReadOnlyTestDictionary<int, string>(i_array, s_array);
            s = dict2.ToString();
            Assert.AreEqual("{1->Eric, 5->Clapton, 6->null, 5->The, 19->World}", s);

            string[] s_big = new string[1000];
            int[] i_big = new int[1000];

            for (int i = 0; i < i_big.Length; ++i) {
                i_big[i] = i * 2 + 1;
                s_big[i] = "foo" + i.ToString() + "bar";
            }

            string expected = "{1->foo0bar, 3->foo1bar, 5->foo2bar, 7->foo3bar, 9->foo4bar, 11->foo5bar, 13->foo6bar, 15->foo7bar, 17->foo8bar, 19->foo9bar, 21->foo10bar, 23->foo11bar, 25->foo12bar, 27->foo13bar, 29->foo14bar, 31->foo15bar, 33->foo16bar, 35->foo17bar, 37->foo18bar, ...}";
 
            ReadWriteTestDictionary<int,string> dict3 = new ReadWriteTestDictionary<int, string>(i_big,s_big);
            s = dict3.DebuggerDisplayString();
            Assert.AreEqual(expected, s);

            ReadOnlyTestDictionary<int, string> dict4 = new ReadOnlyTestDictionary<int, string>(i_big,s_big);
            s = dict4.DebuggerDisplayString();
            Assert.AreEqual(expected, s);
        }

        [Test]
        public void TestDictionary()
        {
            string[] s_array = { "Eric", "Clapton", "Rules", "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };

            Dictionary<string, int> dict = new Dictionary<string, int>();
            for (int i = 0; i < s_array.Length; ++i)
                dict[s_array[i]] = i_array[i];

            InterfaceTests.TestDictionary<string, int>(dict, s_array, i_array, "foo", false);
            InterfaceTests.TestDictionaryGeneric<string, int>(dict, s_array, i_array, "foo", false, null, null);
        }

        [Test]
        public void AsReadOnly()
        {
            string[] s_array = { "Eric", "Clapton", null, "The", "World" };
            int[] i_array = { 1, 5, 6, 5, 19 };

            ReadWriteTestDictionary<string, int> dict1 = new ReadWriteTestDictionary<string, int>(s_array, i_array);
            IDictionary<string, int> dict2 = dict1.AsReadOnly();

            InterfaceTests.TestReadOnlyDictionaryGeneric<string, int>(dict2, s_array, i_array, "foo", true, null, null, null);
        }
    }
}
