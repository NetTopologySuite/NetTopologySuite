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
    // A simple read-write collection.
    class ReadWriteTestCollection<T> : CollectionBase<T>
    {
        private List<T> items;

        public ReadWriteTestCollection(T[] items)
        {
            this.items = new List<T>(items);
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public override void Add(T item)
        {
            items.Add(item);
        }

        public override bool Remove(T item)
        {
            return items.Remove(item);
        }

        public override void Clear()
        {
            items.Clear();
        }
    }

    [TestFixture]
    public class CollectionBaseTests
    {
        // A simple read-only collection.
        class ReadOnlyTestCollection<T> : ReadOnlyCollectionBase<T>
        {
            private T[] items;

            public ReadOnlyTestCollection(T[] items)
            {
                this.items = items;
            }

            public override int Count
            {
                get { return items.Length; }
            }

            public override IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < items.Length; ++i)
                    yield return items[i];
            }
        }

        [Test]
        public void ReadOnlyCollection()
        {
            string[] s = { "Hello", "Goodbye", "Eric", "Clapton", "Rules" };

            ReadOnlyTestCollection<string> coll = new ReadOnlyTestCollection<string>(s);

            InterfaceTests.TestCollection<string>((ICollection)coll, s, true);
            InterfaceTests.TestReadonlyCollectionGeneric<string>((ICollection<string>)coll, s, true, "ReadOnlyTestCollection");
        }

        [Test]
        public void ReadWriteCollection()
        {
            string[] s = { "Hello", "Goodbye", "Eric", "Clapton", "Rules" };

            ReadWriteTestCollection<string> coll = new ReadWriteTestCollection<string>(s);

            InterfaceTests.TestCollection<string>((ICollection)coll, s, true);
            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)coll, s, true);
        }

        [Test]
        public void ConvertToString()
        {
            string[] array = { "Hello", "Goodbye", null, "Clapton", "Rules" };
            string s;

            ReadWriteTestCollection<string> coll1 = new ReadWriteTestCollection<string>(array);
            s = coll1.ToString();
            Assert.AreEqual("{Hello,Goodbye,null,Clapton,Rules}", s);

            ReadOnlyTestCollection<string> coll2 = new ReadOnlyTestCollection<string>(array);
            s = coll2.ToString();
            Assert.AreEqual("{Hello,Goodbye,null,Clapton,Rules}", s);

            ReadWriteTestCollection<string> coll3 = new ReadWriteTestCollection<string>(new string[0]);
            s = coll3.ToString();
            Assert.AreEqual("{}", s);

            ReadOnlyTestCollection<string> coll4= new ReadOnlyTestCollection<string>(new string[0]);
            s = coll4.ToString();
            Assert.AreEqual("{}", s);

            ReadWriteTestCollection<int> coll5 = new ReadWriteTestCollection<int>(new int[] { 1, 2, 3 });
            s = coll5.ToString();
            Assert.AreEqual("{1,2,3}", s);

            ReadOnlyTestCollection<int> coll6 = new ReadOnlyTestCollection<int>(new int[] { 1, 2, 3 });
            s = coll6.ToString();
            Assert.AreEqual("{1,2,3}", s);


        }

        [Test]
        public void DebuggerDisplay()
        {
            string[] array = { "Hello", "Goodbye", null, "Clapton", "Rules" };
            string s;

            ReadWriteTestCollection<string> coll1 = new ReadWriteTestCollection<string>(array);
            s = coll1.DebuggerDisplayString();
            Assert.AreEqual("{Hello,Goodbye,null,Clapton,Rules}", s);

            ReadOnlyTestCollection<string> coll2 = new ReadOnlyTestCollection<string>(array);
            s = coll2.DebuggerDisplayString();
            Assert.AreEqual("{Hello,Goodbye,null,Clapton,Rules}", s);

            ReadWriteTestCollection<string> coll3 = new ReadWriteTestCollection<string>(new string[0]);
            s = coll3.DebuggerDisplayString();
            Assert.AreEqual("{}", s);

            ReadOnlyTestCollection<string> coll4 = new ReadOnlyTestCollection<string>(new string[0]);
            s = coll4.DebuggerDisplayString();
            Assert.AreEqual("{}", s);

            ReadWriteTestCollection<int> coll5 = new ReadWriteTestCollection<int>(new int[] { 1, 2, 3 });
            s = coll5.DebuggerDisplayString();
            Assert.AreEqual("{1,2,3}", s);

            ReadOnlyTestCollection<int> coll6 = new ReadOnlyTestCollection<int>(new int[] { 1, 2, 3 });
            s = coll6.DebuggerDisplayString();
            Assert.AreEqual("{1,2,3}", s);

            int[] bigarray = new int[1000];
            for (int i = 0; i < bigarray.Length; ++i)
                bigarray[i] = i;

            string expected = "{0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,...}";

            ReadWriteTestCollection<int> coll7 = new ReadWriteTestCollection<int>(bigarray);
            s = coll7.DebuggerDisplayString();
            Assert.AreEqual(expected, s);

            ReadOnlyTestCollection<int> coll8 = new ReadOnlyTestCollection<int>(bigarray);
            s = coll8.DebuggerDisplayString();
            Assert.AreEqual(expected, s);
        }

        // Tests the built-in List<T> class. Makes sure that our tests are reasonable.
        [Test]
        public void CheckList()
        {
            string[] s = { "Hello", "Goodbye", "Eric", "Clapton", "Rules" };

            List<string> coll = new List<string>(s);

            InterfaceTests.TestCollection<string>((ICollection)coll, s, true);
            InterfaceTests.TestReadWriteCollectionGeneric<string>((ICollection<string>)coll, s, true);

            IList<string> ro = new List<string>(s).AsReadOnly();
            InterfaceTests.TestReadonlyCollectionGeneric<string>(ro, s, true, null);
        }

        // Tests the Keys and Values collections of Dictionary. Makes sure that our tests are reasonable.
        [Test]  
        public void CheckDictionaryKeyValues()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict["Eric"] = 3;
            dict["Clapton"] = 1;
            dict["Rules"] = 4;
            dict["The"] = 1;
            dict["Universe"] = 5;

            InterfaceTests.TestCollection<string>(dict.Keys, new string[] { "Eric", "Clapton", "Rules", "The", "Universe" }, false);
            InterfaceTests.TestReadonlyCollectionGeneric<string>(dict.Keys, new string[] { "Eric", "Clapton", "Rules", "The", "Universe" }, false, null);
            InterfaceTests.TestCollection<int>(dict.Values, new int[] { 1, 1, 3, 4, 5 }, false);
            InterfaceTests.TestReadonlyCollectionGeneric<int>(dict.Values, new int[] { 1, 1, 3, 4, 5 }, false, null);
        }

        [Test]
        public void Exists()
        {
            ReadWriteTestCollection<double> coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsTrue(coll1.Exists(delegate(double d) { return d > 100; }));
            Assert.IsTrue(coll1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(coll1.Exists(delegate(double d) { return d < -10.0; }));
            coll1.Clear();
            Assert.IsFalse(coll1.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));

            ReadOnlyTestCollection<double> coll2 = new ReadOnlyTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsTrue(coll2.Exists(delegate(double d) { return d > 100; }));
            Assert.IsTrue(coll2.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
            Assert.IsFalse(coll2.Exists(delegate(double d) { return d < -10.0; }));
            coll2 = new ReadOnlyTestCollection<double>(new double[] {  });
            Assert.IsFalse(coll2.Exists(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void TrueForAll()
        {
            ReadWriteTestCollection<double> coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsFalse(coll1.TrueForAll(delegate(double d) { return d > 100; }));
            Assert.IsFalse(coll1.TrueForAll(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.IsTrue(coll1.TrueForAll(delegate(double d) { return d > -10; }));
            Assert.IsTrue(coll1.TrueForAll(delegate(double d) { return Math.Abs(d) < 200; }));
            coll1.Clear();
            Assert.IsTrue(coll1.TrueForAll(delegate(double d) { return Math.Abs(d) == 0.04; }));

            ReadOnlyTestCollection<double> coll2 = new ReadOnlyTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.IsFalse(coll2.TrueForAll(delegate(double d) { return d > 100; }));
            Assert.IsFalse(coll2.TrueForAll(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.IsTrue(coll2.TrueForAll(delegate(double d) { return d > -10; }));
            Assert.IsTrue(coll2.TrueForAll(delegate(double d) { return Math.Abs(d) < 200; }));
            coll2 = new ReadOnlyTestCollection<double>(new double[] { });
            Assert.IsTrue(coll2.TrueForAll(delegate(double d) { return Math.Abs(d) == 0.04; }));
        }

        [Test]
        public void CountWhere()
        {
            ReadWriteTestCollection<double> coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.AreEqual(0, coll1.CountWhere(delegate(double d) { return d > 200; }));
            Assert.AreEqual(6, coll1.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.AreEqual(8, coll1.CountWhere(delegate(double d) { return d > -10; }));
            Assert.AreEqual(4, coll1.CountWhere(delegate(double d) { return Math.Abs(d) > 5; }));
            coll1.Clear();
            Assert.AreEqual(0, coll1.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));

            ReadOnlyTestCollection<double> coll2 = new ReadOnlyTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -7.6, -0.04, 1.78, 10.11, 187.4 });

            Assert.AreEqual(0, coll2.CountWhere(delegate(double d) { return d > 200; }));
            Assert.AreEqual(6, coll2.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
            Assert.AreEqual(8, coll2.CountWhere(delegate(double d) { return d > -10; }));
            Assert.AreEqual(4, coll2.CountWhere(delegate(double d) { return Math.Abs(d) > 5; }));
            coll2 = new ReadOnlyTestCollection<double>(new double[] { });
            Assert.AreEqual(0, coll2.CountWhere(delegate(double d) { return Math.Abs(d) < 10; }));
        }

        [Test]
        public void FindAll()
        {
            ReadWriteTestCollection<double> coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            double[] expected = { 7.6, -7.6, 10.11, 187.4 };
            int i;

            i = 0;
            foreach (double x in coll1.FindAll(delegate(double d) { return Math.Abs(d) > 5; })) {
                Assert.AreEqual(expected[i], x);
                ++i;
            }
            Assert.AreEqual(expected.Length, i);

            ReadOnlyTestCollection<double> coll2 = new ReadOnlyTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            expected = new double[] { 7.6, -7.6, 10.11, 187.4 };

            i = 0;
            foreach (double x in coll2.FindAll(delegate(double d) { return Math.Abs(d) > 5; })) {
                Assert.AreEqual(expected[i], x);
                ++i;
            }
            Assert.AreEqual(expected.Length, i);
        }

        [Test]
        public void RemoveAll()
        {
            ReadWriteTestCollection<double> coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });

            coll1.RemoveAll(delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestReadWriteCollectionGeneric(coll1, new double[] { 4.5, 1.2,  -0.04, 1.78 }, true, null);

            coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            coll1.RemoveAll(delegate(double d) { return d == 0; });
            InterfaceTests.TestReadWriteCollectionGeneric(coll1, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }, true, null);

            coll1 = new ReadWriteTestCollection<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            coll1.RemoveAll(delegate(double d) { return d < 200; });
            Assert.AreEqual(0, coll1.Count);
        }

        [Test]
        public void ForEach()
        {
            ReadWriteTestCollection<string> coll1 = new ReadWriteTestCollection<string>(new string[] { "foo", "bar", "hello", "sailor" });
            string s = "";
            coll1.ForEach(delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "!foo!bar!hello!sailor");

            ReadOnlyTestCollection<string> coll2 = new ReadOnlyTestCollection<string>(new string[] { "foo", "bar", "hello", "sailor" });
            s = "";
            coll2.ForEach(delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "!foo!bar!hello!sailor");

            coll1 = new ReadWriteTestCollection<string>(new string[] {  });
            s = "";
            coll1.ForEach(delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "");

            coll2 = new ReadOnlyTestCollection<string>(new string[] { });
            s = "";
            coll2.ForEach(delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "");
        }

        [Test]
        public void ConvertAll()
        {
            int[] array = new int[400];
            for (int i = 0; i < array.Length; ++i)
                array[i] = i;
            ReadWriteTestCollection<int> coll1 = new ReadWriteTestCollection<int>(array);
            IEnumerable<string> result1;

            result1 = coll1.ConvertAll<string>(delegate(int x) { return (x * 2).ToString(); });
            string[] expected = new string[400];
            for (int i = 0; i < 400; ++i)
                expected[i] = (2 * i).ToString();
            InterfaceTests.TestEnumerableElements<string>(result1, expected);

            coll1 = new ReadWriteTestCollection<int>(new int[0]);
            result1 = coll1.ConvertAll<string>(delegate(int x) { return (x * 2).ToString(); });
            InterfaceTests.TestEnumerableElements<string>(result1, new string[0]);

            ReadOnlyTestCollection<int> coll2 = new ReadOnlyTestCollection<int>(array);
            IEnumerable<string> result2;

            result2 = coll2.ConvertAll<string>(delegate(int x) { return (x * 2).ToString(); });
            InterfaceTests.TestEnumerableElements<string>(result2, expected);

            coll2 = new ReadOnlyTestCollection<int>(new int[0]);
            result2 = coll2.ConvertAll<string>(delegate(int x) { return (x * 2).ToString(); });
            InterfaceTests.TestEnumerableElements<string>(result2, new string[0]);
        }

        [Test]
        public void AsReadOnly()
        {
            int[] elements = new int[400];
            for (int i = 0; i < 400; ++i)
                elements[i] = i;

            ReadWriteTestCollection<int> coll1 = new ReadWriteTestCollection<int>(elements);
            ICollection<int> coll2 = coll1.AsReadOnly();

            InterfaceTests.TestReadonlyCollectionGeneric<int>(coll2, elements, true, null);

            coll1.Add(27);
            coll1.Add(199);

            elements = new int[402];
            coll2 = coll1.AsReadOnly();

            for (int i = 0; i < 400; ++i)
                elements[i] = i;

            elements[400] = 27;
            elements[401] = 199;

            InterfaceTests.TestReadonlyCollectionGeneric<int>(coll2, elements, true, null);

            coll1 = new ReadWriteTestCollection<int>(new int[0]);
            coll2 = coll1.AsReadOnly();
            InterfaceTests.TestReadonlyCollectionGeneric<int>(coll2, new int[0], true, null);
            coll1.Add(4);
            InterfaceTests.TestReadonlyCollectionGeneric<int>(coll2, new int[] { 4 }, true, null);
        }

    }
}

