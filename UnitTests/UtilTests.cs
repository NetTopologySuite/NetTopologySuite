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
using System.Collections;
using NUnit.Framework;

#endregion

namespace Wintellect.PowerCollections.Tests
{
    [TestFixture]
    public class UtilTests
    {
#pragma warning disable 649
        struct StructType {
            public int i;
        }

        class ClassType {
            public int i;
        }

        public struct CloneableStruct : ICloneable
        {
            public int value;
            public int tweak;

            public CloneableStruct(int v)
            {
                value = v;
                tweak = 1;
            }

            public object Clone()
            {
                CloneableStruct newStruct;
                newStruct.value = value;
                newStruct.tweak = tweak + 1;
                return newStruct;
            }

            public bool Identical(CloneableStruct other)
            {
                return value == other.value && tweak == other.tweak;
            }

            public override bool Equals(object other)
            {
                if (! (other is CloneableStruct))
                    return false;
                CloneableStruct o = (CloneableStruct)other;

                return (o.value == value);
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }  

#pragma warning restore 649

        [Test]
        public void IsCloneableType()
        {
            bool isCloneable, isValue;

            isCloneable = Util.IsCloneableType(typeof(int), out isValue);
            Assert.IsTrue(isCloneable); Assert.IsTrue(isValue);

            isCloneable = Util.IsCloneableType(typeof(ICloneable), out isValue);
            Assert.IsTrue(isCloneable); Assert.IsFalse(isValue);

            isCloneable = Util.IsCloneableType(typeof(StructType), out isValue);
            Assert.IsTrue(isCloneable); Assert.IsTrue(isValue);

            isCloneable = Util.IsCloneableType(typeof(ClassType), out isValue);
            Assert.IsFalse(isCloneable); Assert.IsFalse(isValue);

            isCloneable = Util.IsCloneableType(typeof(ArrayList), out isValue);
            Assert.IsTrue(isCloneable); Assert.IsFalse(isValue);

            isCloneable = Util.IsCloneableType(typeof(CloneableStruct), out isValue);
            Assert.IsTrue(isCloneable); Assert.IsFalse(isValue);

            isCloneable = Util.IsCloneableType(typeof(OrderedDictionary<int, double>), out isValue);
            Assert.IsTrue(isCloneable); Assert.IsFalse(isValue);
        }

        [Test]
        public void WrapEnumerable()
        {
            IEnumerable<int> enum1 = new List<int>(new int[] { 1, 4, 5, 6, 9, 1 });
            IEnumerable<int> enum2 = Util.CreateEnumerableWrapper(enum1);
            InterfaceTests.TestEnumerableElements(enum2, new int[] { 1, 4, 5, 6, 9, 1 });
        }

        [Test]
        public void TestGetHashCode()
        {
            int r1, r2, result;
            result = Util.GetHashCode("foo", EqualityComparer<string>.Default);
            Assert.AreEqual(result, "foo".GetHashCode());
            result = Util.GetHashCode(null, EqualityComparer<string>.Default);
            Assert.AreEqual(result, 0x1786E23C);
            r1 = Util.GetHashCode("Banana", StringComparer.InvariantCultureIgnoreCase);
            r2 = Util.GetHashCode("banANA", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(r1, r2);
        }
    }
}

