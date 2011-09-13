//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

#endregion

namespace Wintellect.PowerCollections.Tests
{

    /// <summary>
    /// Class that doesn't implement any IComparable.
    /// </summary>
    class Unorderable
    {
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
                return CompareTo((OddEvenComparable) obj) == 0;
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
    }

    /// <summary>
    /// Comparable that compares ints, equating odds with odds and evens with evrents..
    /// </summary>
    class GOddEvenEquatable : System.IEquatable<GOddEvenEquatable>
    {
        public int val;

        public GOddEvenEquatable(int v)
        {
            val = v;
        }

        public bool Equals(GOddEvenEquatable other)
        {
            return ((this.val & 1) == (other.val & 1));
        }

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }
    }

    /// <summary>
    /// Comparer that compares ints, sorting odds before evens.
    /// </summary>
    class OddEvenComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            int e1 = (int)x;
            int e2 = (int)y;
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

    /// <summary>
    /// Comparer that compares ints, sorting odds before evens.
    /// </summary>
    class GOddEvenComparer : System.Collections.Generic.IComparer<int>
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

        public override bool Equals(object obj)
        {
            return (obj is GOddEvenComparer);
        }

        public override int GetHashCode()
        {
            return 123569;
        }
    }

    /// <summary>
    /// Comparer that compares ints, sorting odds before evens.
    /// </summary>
    class GOddEvenEqualityComparer : System.Collections.Generic.IEqualityComparer<int>
    {
        public bool Equals(int e1, int e2)
        {
            return ((e1 & 1) == (e2 & 1));
        }

        public int GetHashCode(int i)
        {
            return i;
        }

        public override bool Equals(object obj)
        {
            return (obj is GOddEvenComparer);
        }

        public override int GetHashCode()
        {
            return 123569;
        }
    }

    /// <summary>
    /// Tests for the Comparers class
    /// </summary>
    [TestFixture]
    public class ComparersTests
    {
        // Comparison function 
        public static int CompareOddEven(int e1, int e2)
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

        public static bool FirstCharEqual(string s1, string s2)
        {
            if (s1 == null || s2 == null || s1.Length == 0 || s2.Length == 0) {
                if (s1 != null && s1.Length > 0)
                    return false;
                if (s2 != null && s2.Length > 0)
                    return false;
                return true;
            }

            return s1[0] == s2[0];
        }



        [Test]
        public void ComparerFromComparison()
        {
            IComparer<int> comparer = Comparers.ComparerFromComparison<int>(CompareOddEven);

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

        [Test]
        public void ComparerKeyValueFromComparerKey()
        {
            IComparer<KeyValuePair<int, string>> comparer = Comparers.ComparerKeyValueFromComparerKey<int, string>(new GOddEvenComparer());

            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(7, "foo"), new KeyValuePair<int, string>(6, "bar")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(7, "bar"), new KeyValuePair<int, string>(8, "foo")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(12, "baz"), new KeyValuePair<int, string>(11, "baz")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(12, "a"), new KeyValuePair<int, string>(143, "foo")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(5, "b"), new KeyValuePair<int, string>(7, "d")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(9, "c"), new KeyValuePair<int, string>(5, "c")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(6, "e"), new KeyValuePair<int, string>(8, "b")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(14, "f"), new KeyValuePair<int, string>(-8, "a")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(0, "g"), new KeyValuePair<int, string>(0, "r")) == 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(-3, "q"), new KeyValuePair<int, string>(-3, "f")) == 0);
        }

        [Test]
        public void EqualityComparerKeyValueFromComparerKey()
        {
            IEqualityComparer<KeyValuePair<int, string>> comparer = Comparers.EqualityComparerKeyValueFromComparerKey<int, string>(new GOddEvenEqualityComparer());

            Assert.IsTrue(comparer.Equals(new KeyValuePair<int, string>(0, "g"), new KeyValuePair<int, string>(2, "r")));
            Assert.IsTrue(comparer.Equals(new KeyValuePair<int, string>(-1, "g"), new KeyValuePair<int, string>(7, "w")));
            Assert.IsFalse(comparer.Equals(new KeyValuePair<int, string>(-3, "q"), new KeyValuePair<int, string>(4, "q")));
        }

        [Test]
        public void ComparerPairFromKeyValueComparers()
        {
            IComparer<KeyValuePair<int, string>> comparer = Comparers.ComparerPairFromKeyValueComparers<int, string>(new GOddEvenComparer(), StringComparer.InvariantCultureIgnoreCase);

            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(7, "foo"), new KeyValuePair<int, string>(6, "bar")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(7, "bar"), new KeyValuePair<int, string>(8, "foo")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(12, "baz"), new KeyValuePair<int, string>(11, "baz")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(12, "a"), new KeyValuePair<int, string>(143, "foo")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(5, "b"), new KeyValuePair<int, string>(7, "d")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(9, "c"), new KeyValuePair<int, string>(5, "c")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(6, "e"), new KeyValuePair<int, string>(8, "b")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(14, "f"), new KeyValuePair<int, string>(-8, "a")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(0, "g"), new KeyValuePair<int, string>(0, "r")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(-3, "q"), new KeyValuePair<int, string>(-3, "f")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(0, "g"), new KeyValuePair<int, string>(0, "R")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(-3, "q"), new KeyValuePair<int, string>(-3, "F")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(0, "Foo"), new KeyValuePair<int, string>(0, "foo")) == 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(-3, "q"), new KeyValuePair<int, string>(-3, "q")) == 0);
        }

        [Test]
        public void ComparerKeyValueFromComparisonKey()
        {
            IComparer<KeyValuePair<int, string>> comparer = Comparers.ComparerKeyValueFromComparisonKey<int, string>(CompareOddEven);

            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(7, "foo"), new KeyValuePair<int, string>(6, "bar")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(7, "bar"), new KeyValuePair<int, string>(8, "foo")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(12, "baz"), new KeyValuePair<int, string>(11, "baz")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(12, "a"), new KeyValuePair<int, string>(143, "foo")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(5, "b"), new KeyValuePair<int, string>(7, "d")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(9, "c"), new KeyValuePair<int, string>(5, "c")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(6, "e"), new KeyValuePair<int, string>(8, "b")) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(14, "f"), new KeyValuePair<int, string>(-8, "a")) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(0, "g"), new KeyValuePair<int, string>(0, "r")) == 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<int, string>(-3, "q"), new KeyValuePair<int, string>(-3, "f")) == 0);
        }

        [Test]
        public void DefaultComparerNonGeneric()
        {
            IComparer<OddEvenComparable> comparer = Comparers.DefaultComparer<OddEvenComparable>();

            Assert.IsTrue(comparer.Compare(new OddEvenComparable(7), new OddEvenComparable(6)) < 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(7), new OddEvenComparable(8)) < 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(12), new OddEvenComparable(11)) > 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(12), new OddEvenComparable(143)) > 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(5), new OddEvenComparable(7)) < 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(9), new OddEvenComparable(5)) > 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(6), new OddEvenComparable(8)) < 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(14), new OddEvenComparable(-8)) > 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(0), new OddEvenComparable(0)) == 0);
            Assert.IsTrue(comparer.Compare(new OddEvenComparable(-3), new OddEvenComparable(-3)) == 0);
        }

        [Test]
        public void DefaultComparerGeneric()
        {
            IComparer<GOddEvenComparable> comparer = Comparers.DefaultComparer<GOddEvenComparable>();

            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(7), new GOddEvenComparable(6)) < 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(7), new GOddEvenComparable(8)) < 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(12), new GOddEvenComparable(11)) > 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(12), new GOddEvenComparable(143)) > 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(5), new GOddEvenComparable(7)) < 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(9), new GOddEvenComparable(5)) > 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(6), new GOddEvenComparable(8)) < 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(14), new GOddEvenComparable(-8)) > 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(0), new GOddEvenComparable(0)) == 0);
            Assert.IsTrue(comparer.Compare(new GOddEvenComparable(-3), new GOddEvenComparable(-3)) == 0);
        }

        [Test]
        public void DefaultKeyValueComparerNonGeneric()
        {
            IComparer<KeyValuePair<OddEvenComparable, short>> comparer = Comparers.DefaultKeyValueComparer<OddEvenComparable, short>();

            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(7), 12), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(6), 19)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(7), 133), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(8), 1)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(12), 34), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(11), 3)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(12), 8), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(143), 6)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(5), 1), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(7), 0)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(9), 3), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(5), 23)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(6), 5), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(8), 4)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(14), 1), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(-8), 0)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(0), 4), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(0), 17)) == 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(-3), 2), new KeyValuePair<OddEvenComparable, short>(new OddEvenComparable(-3), 1)) == 0);
        }

        [Test]
        public void DefaultKeyValueComparerGeneric()
        {
            IComparer<KeyValuePair<GOddEvenComparable, short>> comparer = Comparers.DefaultKeyValueComparer<GOddEvenComparable, short>();

            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(7), 12), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(6), 19)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(7), 133), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(8), 1)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(12), 34), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(11), 3)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(12), 8), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(143), 6)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(5), 1), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(7), 0)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(9), 3), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(5), 23)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(6), 5), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(8), 4)) < 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(14), 1), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(-8), 0)) > 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(0), 4), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(0), 17)) == 0);
            Assert.IsTrue(comparer.Compare(new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(-3), 2), new KeyValuePair<GOddEvenComparable, short>(new GOddEvenComparable(-3), 1)) == 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Type \"Wintellect.PowerCollections.Tests.Unorderable\" does not implement IComparable<Wintellect.PowerCollections.Tests.Unorderable> or IComparable.")]
        public void UnorderableType2()
        {
            IComparer<Unorderable> ordering = Comparers.DefaultComparer<Unorderable>();
        }
    }
}

