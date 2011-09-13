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
using System.Text;
using NUnit.Framework;

namespace Wintellect.PowerCollections.Tests
{
    /// <summary>
    /// Tests for the Triple struct.
    /// </summary>
    [TestFixture]
    public class TripleTests
    {
        /// <summary>
        /// Class that doesn't implement any IComparable.
        /// </summary>
        class Unorderable
        {
            public override bool Equals(object obj)
            {
                return obj is Unorderable;
            }

            public override int GetHashCode()
            {
                return 42;
            }
        }

        /// <summary>
        /// Comparable that compares ints, sorting odds before evens.
        /// </summary>
        class OddEvenComparable : System.IComparable
        {
            public int val;

            public OddEvenComparable(int v)
            {
                val = v;
            }

            public int CompareTo(object other)
            {
                int e1 = val;
                int e2 = ((OddEvenComparable)other).val;
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

            public override bool Equals(object obj)
            {
                if (obj is OddEvenComparable)
                    return CompareTo((OddEvenComparable)obj) == 0;
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return val.GetHashCode();
            }

        }

        /// <summary>
        /// Comparable that compares ints, sorting odds before evens.
        /// </summary>
        class GOddEvenComparable : System.IComparable<GOddEvenComparable>
        {
            public int val;

            public GOddEvenComparable(int v)
            {
                val = v;
            }

            public int CompareTo(GOddEvenComparable other)
            {
                int e1 = val;
                int e2 = other.val;
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

            public override bool Equals(object other)
            {
                return (other is GOddEvenComparable) && CompareTo((GOddEvenComparable)other) == 0;
            }

            public override int GetHashCode()
            {
                return val.GetHashCode();
            }
        }

        /// <summary>
        /// Test basic Triple creation.  
        /// </summary>
        [Test]
        public void Creation()
        {
            Triple<int, double, string> p1 = new Triple<int, double, string>();

            Assert.AreEqual(0, p1.First);
            Assert.AreEqual(0.0, p1.Second);
            Assert.IsNull(p1.Third);

            Triple<int, string, char> p2 = new Triple<int, string, char>(42, "hello", 'X');

            Assert.AreEqual(42, p2.First);
            Assert.AreEqual("hello", p2.Second);
            Assert.AreEqual('X', p2.Third);

            Triple<string, object, IEnumerable<int>> p3 = new Triple<string, object, IEnumerable<int>>();

            Assert.IsNull(p3.First);
            Assert.IsNull(p3.Second);
            Assert.IsNull(p3.Third);

            object o = new object();
            Triple<Triple<string, int, char>, object, Pair<double, float>> p4 =
                new Triple<Triple<string, int, char>, object, Pair<double, float>>(new Triple<string, int, char>("foo", 12, 'X'), o, new Pair<double,float>(3.45, -1.2F));
            Triple<string, int, char> p5 = p4.First;

            Assert.AreEqual("foo", p5.First);
            Assert.AreEqual(12, p5.Second);
            Assert.AreEqual('X', p5.Third);
            Assert.AreSame(o, p4.Second);
            Assert.AreEqual(3.45, p4.Third.First);
            Assert.AreEqual(-1.2F, p4.Third.Second);
        }

        /// <summary>
        /// Test get and set of First and Second, and Third.
        /// </summary>
        [Test]
        public void Elements()
        {
            Triple<int, string, double> p1 = new Triple<int, string, double>();
            string s = new string('z', 3);

            p1.First = 217;
            p1.Second = s;
            p1.Third = 3.14;
            Assert.AreEqual(217, p1.First);
            Assert.AreSame(s, p1.Second);
            Assert.AreEqual(3.14, p1.Third);

            object o = new System.Collections.BitArray(4);
            Triple<string, int, object> p2 = new Triple<string, int, object>("hello", 1, new System.Text.StringBuilder());
            p2.Second = 212;
            p2.First = s;
            p2.Third = o;

            Assert.AreEqual(212, p2.Second);
            Assert.AreSame(s, p2.First);
            Assert.AreSame(o, p2.Third);
            p2.First = null;
            Assert.IsNull(p2.First);
            p2.Third = null;
            Assert.IsNull(p2.Third);
        }

        [Test]
        public void Equals()
        {
            Triple<int, string, double> p1 = new Triple<int, string, double>(42, new string('z', 3), 4.5);
            Triple<int, string, double> p2 = new Triple<int, string, double>(53, new string('z', 3), 4.5);
            Triple<int, string, double> p3 = new Triple<int, string, double>(42, new string('z', 4), 2.1);
            Triple<int, string, double> p4 = new Triple<int, string, double>(42, new string('z', 3), 4.5);
            Triple<int, string, double> p5 = new Triple<int, string, double>(122, new string('y', 3), 3.14);
            Triple<int, string, double> p6 = new Triple<int, string, double>(122, null, 3.14);
            Triple<int, string, double> p7 = new Triple<int, string, double>(122, null, 3.14);
            bool f;

            f = p1.Equals(p2); Assert.IsFalse(f);
            f = p1.Equals(p3); Assert.IsFalse(f);
            f = p1.Equals(p4); Assert.IsTrue(f);
            f = p1.Equals(p5); Assert.IsFalse(f);
            f = p1.Equals("hi"); Assert.IsFalse(f);
            f = p6.Equals(p7); Assert.IsTrue(f);
            f = p1 == p2; Assert.IsFalse(f);
            f = p1 == p3; Assert.IsFalse(f);
            f = p1 == p4; Assert.IsTrue(f);
            f = p1 == p5; Assert.IsFalse(f);
            f = p6 == p7; Assert.IsTrue(f);
            f = p1 != p2; Assert.IsTrue(f);
            f = p1 != p3; Assert.IsTrue(f);
            f = p1 != p4; Assert.IsFalse(f);
            f = p1 != p5; Assert.IsTrue(f);
            f = p6 != p7; Assert.IsFalse(f);
        }

        [Test]
        public void HashCode()
        {
            Triple<int, string, double> p1 = new Triple<int, string, double>(42, new string('z', 3), 4.5);
            Triple<int, string, double> p2 = new Triple<int, string, double>(53, new string('z', 3), 4.5);
            Triple<int, string, double> p3 = new Triple<int, string, double>(42, new string('z', 4), 2.1);
            Triple<int, string, double> p4 = new Triple<int, string, double>(42, new string('z', 3), 4.5);
            Triple<int, string, double> p5 = new Triple<int, string, double>(122, new string('y', 3), 3.14);
            Triple<int, string, double> p6 = new Triple<int, string, double>(122, null, 3.14);
            Triple<int, string, double> p7 = new Triple<int, string, double>(122, null, 3.14);

            int h1 = p1.GetHashCode();
            int h2 = p2.GetHashCode();
            int h3 = p3.GetHashCode();
            int h4 = p4.GetHashCode();
            int h5 = p5.GetHashCode();
            int h6 = p6.GetHashCode();
            int h7 = p7.GetHashCode();

            bool f;
            f = h1 == h2; Assert.IsFalse(f);
            f = h1 == h3; Assert.IsFalse(f);
            f = h1 == h4; Assert.IsTrue(f);
            f = h1 == h5; Assert.IsFalse(f);
            f = h6 == h7; Assert.IsTrue(f);
        }

        [Test]
        public void Stringize()
        {
            Triple<int, string, StringBuilder> p1 = new Triple<int, string, StringBuilder>(42, new string('z', 3), new StringBuilder("foo"));
            Triple<int, string, StringBuilder> p2 = new Triple<int, string, StringBuilder>(0, "hello", null);
            Triple<int, string, StringBuilder> p3 = new Triple<int, string, StringBuilder>(-122, null, new StringBuilder());
            Triple<string, int, StringBuilder> p4 = new Triple<string, int, StringBuilder>(null, 11, new StringBuilder("Eric"));

            Assert.AreEqual("First: 42, Second: zzz, Third: foo", p1.ToString());
            Assert.AreEqual("First: 0, Second: hello, Third: null", p2.ToString());
            Assert.AreEqual("First: -122, Second: null, Third: ", p3.ToString());
            Assert.AreEqual("First: null, Second: 11, Third: Eric", p4.ToString());
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Type \"Wintellect.PowerCollections.Tests.TripleTests+Unorderable\" does not implement IComparable<Wintellect.PowerCollections.Tests.TripleTests+Unorderable> or IComparable.")]
        public void UncomparableFirst()
        {
            Triple<Unorderable, int, string> triple1, triple2;
            triple1 = new Triple<Unorderable, int, string>(new Unorderable(), 5, "hello");
            triple2 = new Triple<Unorderable, int, string>(new Unorderable(), 7, "world");
            int compare = triple1.CompareTo(triple2);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Type \"Wintellect.PowerCollections.Tests.TripleTests+Unorderable\" does not implement IComparable<Wintellect.PowerCollections.Tests.TripleTests+Unorderable> or IComparable.")]
        public void UncomparableSecond()
        {
            Triple<int, Unorderable, string> triple1, triple2;
            triple1 = new Triple<int, Unorderable, string>(3, new Unorderable(), "Eric");
            triple2 = new Triple<int, Unorderable, string>(3, new Unorderable(), "Clapton");
            int compare = triple1.CompareTo(triple2);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Type \"Wintellect.PowerCollections.Tests.TripleTests+Unorderable\" does not implement IComparable<Wintellect.PowerCollections.Tests.TripleTests+Unorderable> or IComparable.")]
        public void UncomparableThird()
        {
            Triple<int, string, Unorderable> triple1, triple2;
            triple1 = new Triple<int, string, Unorderable>(3, "Oasis", new Unorderable());
            triple2 = new Triple<int, string, Unorderable>(3, "Oasis", new Unorderable());
            int compare = triple1.CompareTo(triple2);
        }

        [Test]
        public void EqualUncomparable()
        {
            Triple<Unorderable, int, string> triple1, triple2, triple3;
            triple1 = new Triple<Unorderable, int, string>(new Unorderable(), 5, "hello");
            triple2 = new Triple<Unorderable, int, string>(new Unorderable(), 7, "world");
            triple3 = new Triple<Unorderable, int, string>(new Unorderable(), 5, "hello");
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsTrue(triple1.Equals(triple3));
            Assert.IsFalse(triple1 == triple2);
            Assert.IsTrue(triple1 == triple3);

            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());
            Assert.IsTrue(triple1.GetHashCode() == triple3.GetHashCode());
        }

        [Test]
        public void NongenericComparable()
        {
            Triple<int, OddEvenComparable, string> triple1, triple2;

            triple1 = new Triple<int, OddEvenComparable, string>(4, new OddEvenComparable(7), "B");
            triple2 = new Triple<int, OddEvenComparable, string>(7, new OddEvenComparable(3), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) < 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, OddEvenComparable, string>(4, new OddEvenComparable(7), "B");
            triple2 = new Triple<int, OddEvenComparable, string>(4, new OddEvenComparable(2), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) < 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, OddEvenComparable, string>(4, new OddEvenComparable(7), "A");
            triple2 = new Triple<int, OddEvenComparable, string>(4, new OddEvenComparable(7), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) == 0);
            Assert.IsTrue(triple1.Equals(triple2));
            Assert.IsTrue(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, OddEvenComparable, string>(7, new OddEvenComparable(7), "B");
            triple2 = new Triple<int, OddEvenComparable, string>(4, new OddEvenComparable(2), "C");
            Assert.IsTrue(triple1.CompareTo(triple2) > 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, OddEvenComparable, string>(0, new OddEvenComparable(8), "A");
            triple2 = new Triple<int, OddEvenComparable, string>(0, new OddEvenComparable(2), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) > 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            Triple<OddEvenComparable, int, string> triple3, triple4;

            triple3 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(7), 4,"A");
            triple4 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(3), 7, "B");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(7), 4, "B");
            triple4 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(2), 4, "A");
            Assert.IsTrue(triple3.CompareTo(triple4) < 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(7), 4, "C");
            triple4 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(7), 4, "C");
            Assert.IsTrue(triple3.CompareTo(triple4) == 0);
            Assert.IsTrue(triple3.Equals(triple4));
            Assert.IsTrue(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(2), 7, "A");
            triple4 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(7), 4, "B");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(8), 0, "A");
            triple4 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(2), 0,"A");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(2), 4, "A");
            triple4 = new Triple<OddEvenComparable, int, string>(new OddEvenComparable(2), 3, "B");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            Triple<int, string, OddEvenComparable> triple5, triple6;

            triple5 = new Triple<int, string, OddEvenComparable>(4, "B", new OddEvenComparable(7));
            triple6 = new Triple<int, string, OddEvenComparable>(7, "A", new OddEvenComparable(3));
            Assert.IsTrue(triple5.CompareTo(triple6) < 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, OddEvenComparable>(4, "A", new OddEvenComparable(7));
            triple6 = new Triple<int, string, OddEvenComparable>(4, "A", new OddEvenComparable(2));
            Assert.IsTrue(triple5.CompareTo(triple6) < 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, OddEvenComparable>(4, "A", new OddEvenComparable(7));
            triple6 = new Triple<int, string, OddEvenComparable>(4, "A", new OddEvenComparable(7));
            Assert.IsTrue(triple5.CompareTo(triple6) == 0);
            Assert.IsTrue(triple5.Equals(triple6));
            Assert.IsTrue(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, OddEvenComparable>(7, "B", new OddEvenComparable(7));
            triple6 = new Triple<int, string, OddEvenComparable>(4, "C", new OddEvenComparable(2));
            Assert.IsTrue(triple5.CompareTo(triple6) > 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, OddEvenComparable>(0, "A", new OddEvenComparable(8));
            triple6 = new Triple<int, string, OddEvenComparable>(0, "A", new OddEvenComparable(2));
            Assert.IsTrue(triple5.CompareTo(triple6) > 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());
        }

        [Test]
        public void GenericComparable()
        {
            Triple<int, GOddEvenComparable, string> triple1, triple2;

            triple1 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "B");
            triple2 = new Triple<int, GOddEvenComparable, string>(7, new GOddEvenComparable(3), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) < 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "B");
            triple2 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(2), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) < 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "A");
            triple2 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) == 0);
            Assert.IsTrue(triple1.Equals(triple2));
            Assert.IsTrue(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, GOddEvenComparable, string>(7, new GOddEvenComparable(7), "B");
            triple2 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(2), "C");
            Assert.IsTrue(triple1.CompareTo(triple2) > 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            triple1 = new Triple<int, GOddEvenComparable, string>(0, new GOddEvenComparable(8), "A");
            triple2 = new Triple<int, GOddEvenComparable, string>(0, new GOddEvenComparable(2), "A");
            Assert.IsTrue(triple1.CompareTo(triple2) > 0);
            Assert.IsFalse(triple1.Equals(triple2));
            Assert.IsFalse(triple1.GetHashCode() == triple2.GetHashCode());

            Triple<GOddEvenComparable, int, string> triple3, triple4;

            triple3 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(7), 4, "A");
            triple4 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(3), 7, "B");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(7), 4, "B");
            triple4 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(2), 4, "A");
            Assert.IsTrue(triple3.CompareTo(triple4) < 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(7), 4, "C");
            triple4 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(7), 4, "C");
            Assert.IsTrue(triple3.CompareTo(triple4) == 0);
            Assert.IsTrue(triple3.Equals(triple4));
            Assert.IsTrue(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(2), 7, "A");
            triple4 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(7), 4, "B");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(8), 0, "A");
            triple4 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(2), 0, "A");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            triple3 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(2), 4, "A");
            triple4 = new Triple<GOddEvenComparable, int, string>(new GOddEvenComparable(2), 3, "B");
            Assert.IsTrue(triple3.CompareTo(triple4) > 0);
            Assert.IsFalse(triple3.Equals(triple4));
            Assert.IsFalse(triple3.GetHashCode() == triple4.GetHashCode());

            Triple<int, string, GOddEvenComparable> triple5, triple6;

            triple5 = new Triple<int, string, GOddEvenComparable>(4, "B", new GOddEvenComparable(7));
            triple6 = new Triple<int, string, GOddEvenComparable>(7, "A", new GOddEvenComparable(3));
            Assert.IsTrue(triple5.CompareTo(triple6) < 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, GOddEvenComparable>(4, "A", new GOddEvenComparable(7));
            triple6 = new Triple<int, string, GOddEvenComparable>(4, "A", new GOddEvenComparable(2));
            Assert.IsTrue(triple5.CompareTo(triple6) < 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, GOddEvenComparable>(4, "A", new GOddEvenComparable(7));
            triple6 = new Triple<int, string, GOddEvenComparable>(4, "A", new GOddEvenComparable(7));
            Assert.IsTrue(triple5.CompareTo(triple6) == 0);
            Assert.IsTrue(triple5.Equals(triple6));
            Assert.IsTrue(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, GOddEvenComparable>(7, "B", new GOddEvenComparable(7));
            triple6 = new Triple<int, string, GOddEvenComparable>(4, "C", new GOddEvenComparable(2));
            Assert.IsTrue(triple5.CompareTo(triple6) > 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());

            triple5 = new Triple<int, string, GOddEvenComparable>(0, "A", new GOddEvenComparable(8));
            triple6 = new Triple<int, string, GOddEvenComparable>(0, "A", new GOddEvenComparable(2));
            Assert.IsTrue(triple5.CompareTo(triple6) > 0);
            Assert.IsFalse(triple5.Equals(triple6));
            Assert.IsFalse(triple5.GetHashCode() == triple6.GetHashCode());
        }

        [Test]
        public void DictionaryKey()
        {
            OrderedDictionary<Triple<string, int, double>, string> dict1 = new OrderedDictionary<Triple<string, int, double>, string>();

            dict1[new Triple<string, int, double>("foo", 12, 3.14)] = "hello";
            dict1[new Triple<string, int, double>("zebra", 1, 2.4)] = "long";
            dict1[new Triple<string, int, double>("zebra", 1, 9.1)] = "strange";
            dict1[new Triple<string, int, double>("zzz", 14, 8.3)] = "trip";
            dict1[new Triple<string, int, double>("foo", 16, 0.0)] = "goodbye";
            dict1[new Triple<string, int, double>("foo", 12, 3.14)] = "another";

            string[] s_array = { "another", "goodbye", "long", "strange", "trip" };

            Assert.AreEqual(5, dict1.Count);
            int i = 0;
            foreach (string s in dict1.Values) {
                Assert.AreEqual(s_array[i], s);
                ++i;
            }
        }

        [Test]
        public void IComparable()
        {
            Triple<int, GOddEvenComparable, string> triple1, triple2;
            IComparable comp;
            object o;

            triple1 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "B");
            triple2 = new Triple<int, GOddEvenComparable, string>(7, new GOddEvenComparable(3), "A");
            comp = triple1;
            o = triple2;
            Assert.IsTrue(comp.CompareTo(o) < 0);

            triple1 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "B");
            triple2 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(2), "A");
            comp = triple1;
            o = triple2;
            Assert.IsTrue(comp.CompareTo(o) < 0);

            triple1 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "A");
            triple2 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(7), "A");
            comp = triple1;
            o = triple2;
            Assert.IsTrue(comp.CompareTo(o) == 0);

            triple1 = new Triple<int, GOddEvenComparable, string>(7, new GOddEvenComparable(7), "B");
            triple2 = new Triple<int, GOddEvenComparable, string>(4, new GOddEvenComparable(2), "C");
            comp = triple1;
            o = triple2;
            Assert.IsTrue(comp.CompareTo(o) > 0);

            try {
                int i = comp.CompareTo("foo");
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [Test]
        public void Serialize()
        {
            Triple<int, string, string> p1 = new Triple<int, string,string>(-12, "hello", "world");
            Triple<string, string, double> p2 = new Triple<string, string, double>("hi", "elvis", 11);
            Triple<int, string,string> s1 = (Triple<int, string,string>)InterfaceTests.SerializeRoundTrip(p1);
            Triple<string, string, double> s2 = (Triple<string, string, double>)InterfaceTests.SerializeRoundTrip(p2);
            Assert.AreEqual(p1, s1);
            Assert.AreEqual(p2, s2);
        }
    }
}

