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
    public class BigListTests
    {
        // Append a bunch of items one after each other. Then
        // index and foreach to make sure that they are in order.
        [Test]
        public void AppendItem()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();
            int i;

            for (i = 1; i <= SIZE; ++i) {
                biglist1.Add(i);
#if DEBUG
                if (i % 50 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 123 == 5)
                    biglist1.Clone();
            }

#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist1)
                Assert.AreEqual(i++, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist2)
                Assert.AreEqual(i++, x);
        }

        // Prepend a bunch of items one after each other. Then
        // index and foreach to make sure that they are in order.
        [Test]
        public void PrependItem()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();
            int i;

            for (i = 1; i <= SIZE; ++i) {
                biglist1.AddToFront(i);
#if DEBUG
                if (i % 50 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 123 == 5)
                    biglist1.Clone();
            }

#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[SIZE - i]);
            }

            i = SIZE;
            foreach (int x in biglist1)
                Assert.AreEqual(i--, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[SIZE - i]);
            }

            i = SIZE;
            foreach (int x in biglist2)
                Assert.AreEqual(i--, x);
        }

        // Try Create an enumerable.
        [Test]
        public void CreateFromEnumerable()
        {
            const int SIZE = 8000;
            int[] items = new int[SIZE];
            BigList<int> biglist1;
            int i;

            for (i = 0; i < SIZE; ++i)
                items[i] = i + 1;
            biglist1 = new BigList<int>(items);

#if DEBUG
            biglist1.Validate();
#endif //DEBUG
            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist1)
                Assert.AreEqual(i++, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist2)
                Assert.AreEqual(i++, x);
        }


        [Test]
        public void CreateFromEnumerable2()
        {
            int[] array = new int[0];
            BigList<int> biglist1 = new BigList<int>(array);
            Assert.AreEqual(0, biglist1.Count);
        }

        [Test]
        public void AppendAll()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            int i = 1, j = 0;
            while (i <= SIZE) {
                int[] array = new int[j];
                for (int x = 0; x < j; ++x)
                    array[x] = i + x;
                biglist1.AddRange(array);
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist1)
                Assert.AreEqual(i++, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist2)
                Assert.AreEqual(i++, x);
        }

        [Test]
        public void PrependAll()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            int i = 1, j = 0;
            while (i <= SIZE) {
                int[] array = new int[j];
                for (int x = 0; x < j; ++x)
                    array[j - x - 1] = i + x;
                biglist1.AddRangeToFront(array);
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[size - i]);
            }

            i = size;
            foreach (int x in biglist1)
                Assert.AreEqual(i--, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[size - i]);
            }

            i = size;
            foreach (int x in biglist2)
                Assert.AreEqual(i--, x);
        }

        [Test]
        public void AppendBigList()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            int i = 1, j = 0;
            while (i <= SIZE) {
                int[] array = new int[j];
                for (int x = 0; x < j; ++x)
                    array[x] = i + x;
                BigList<int> biglistOther = new BigList<int>(array);
                if (j % 3 == 0)
                    biglistOther.Clone();
                biglist1.AddRange(biglistOther);
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist1)
                Assert.AreEqual(i++, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist2)
                Assert.AreEqual(i++, x);
        }

        [Test]
        public void AppendBigList2()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            biglist1.Add(1);
            biglist1.Add(2);
            int i = 3, j = 11;
            while (i <= SIZE) {
                int[] array = new int[j];
                BigList<int> biglistOther = new BigList<int>();
                for (int x = 0; x < j; ++x)
                    biglistOther.AddToFront(i + (j - x - 1));
                if (j % 7 == 0)
                    biglistOther.Clone();
                biglist1.AddRange(biglistOther);
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist1)
                Assert.AreEqual(i++, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist2)
                Assert.AreEqual(i++, x);
        }

        [Test]
        public void PrependBigList()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            int i = 1, j = 0;
            while (i <= SIZE) {
                int[] array = new int[j];
                for (int x = 0; x < j; ++x)
                    array[j - x - 1] = i + x;
                BigList<int> biglistOther = new BigList<int>(array);
                if (j % 3 == 0)
                    biglistOther.Clone();
                biglist1.AddRangeToFront(biglistOther);
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[size - i]);
            }

            i = size;
            foreach (int x in biglist1)
                Assert.AreEqual(i--, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[size - i]);
            }

            i = size;
            foreach (int x in biglist2)
                Assert.AreEqual(i--, x);
        }

        [Test]
        public void AddBigListEnd()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            int i = 1, j = 0;
            while (i <= SIZE) {
                int[] array = new int[j];
                for (int x = 0; x < j; ++x)
                    array[x] = i + x;
                BigList<int> biglistOther = new BigList<int>(array);
                if (j % 3 == 0)
                    biglistOther.Clone();
                biglist1 = biglist1 + biglistOther;
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist1)
                Assert.AreEqual(i++, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[i - 1]);
            }

            i = 1;
            foreach (int x in biglist2)
                Assert.AreEqual(i++, x);
        }

        [Test]
        public void AddBigListBeginning()
        {
            const int SIZE = 8000;
            BigList<int> biglist1 = new BigList<int>();

            int i = 1, j = 0;
            while (i <= SIZE) {
                int[] array = new int[j];
                for (int x = 0; x < j; ++x)
                    array[j - x - 1] = i + x;
                BigList<int> biglistOther = new BigList<int>(array);
                if (j % 3 == 0)
                    biglistOther.Clone();
                biglist1 = biglistOther + biglist1;
#if DEBUG
                if (i % 30 == 0)
                    biglist1.Validate();
#endif //DEBUG
                if (i % 13 <= 2)
                    biglist1.Clone();
                i += j;
                j += 1;
                if (j == 20)
                    j = 0;
            }
            int size = i - 1;

            Assert.AreEqual(size, biglist1.Count);
#if DEBUG
            biglist1.Validate();
#endif //DEBUG

            for (i = 1; i <= size; ++i) {
                Assert.AreEqual(i, biglist1[size - i]);
            }

            i = size;
            foreach (int x in biglist1)
                Assert.AreEqual(i--, x);

            BigList<int> biglist2 = biglist1.Clone();

            for (i = 1; i <= SIZE; ++i) {
                Assert.AreEqual(i, biglist1[size - i]);
            }

            i = size;
            foreach (int x in biglist2)
                Assert.AreEqual(i--, x);
        }

        [Test]
        public void Count()
        {
            BigList<int> list1, list2, list3, list4, list5, list6, list7, list8;

            list1 = new BigList<int>();
            list2 = new BigList<int>(new int[0]);
            list3 = list2 + list1;
            Assert.AreEqual(0, list1.Count);
#if DEBUG
            list1.Validate();
#endif //DEBUG
            Assert.AreEqual(0, list2.Count);
#if DEBUG
            list2.Validate();
#endif //DEBUG
            Assert.AreEqual(0, list3.Count);
#if DEBUG
            list3.Validate();
#endif //DEBUG
            list4 = new BigList<int>(new int[2145]);
            Assert.AreEqual(2145, list4.Count);
#if DEBUG
            list4.Validate();
#endif //DEBUG
            list5 = list4.GetRange(1003, 423);
            Assert.AreEqual(423, list5.Count);
#if DEBUG
            list5.Validate();
#endif //DEBUG
            list6 = list4.GetRange(1, 5);
            Assert.AreEqual(5, list6.Count);
#if DEBUG
            list6.Validate();
#endif //DEBUG
            list7 = list5 + list6;
            Assert.AreEqual(428, list7.Count);
#if DEBUG
            list7.Validate();
#endif //DEBUG
            list8 = list7.GetRange(77, 0);
            Assert.AreEqual(0, list8.Count);
#if DEBUG
            list8.Validate();
#endif //DEBUG
            list6.Clear();
            Assert.AreEqual(0, list6.Count);
#if DEBUG
            list6.Validate();
#endif //DEBUG
        }

        BigList<int> CreateList(int start, int length)
        {
            if (length < 24) {
                int[] array = new int[length];
                for (int i = 0; i < length; ++i)
                    array[i] = i + start;
                return new BigList<int>(array);
            }
            else {
                int split = length / 5 * 2;
                return CreateList(start, split) + CreateList(start + split, length - split);
            }
        }


        [Test]
        public void GetRange()
        {
            BigList<int> list1, list2, list3, list4, list5;
            list1 = new BigList<int>();

            list2 = list1.GetRange(4, 0);  // 0 length range permitted anywhere.
            Assert.AreEqual(0, list2.Count);

            list3 = new BigList<int>(new int[] { 1, 2, 3, 4, 5 });
            list4 = list3.GetRange(2, 3);
            InterfaceTests.TestEnumerableElements(list4, new int[] { 3, 4, 5 });
            list5 = list3.GetRange(0, 3);
            InterfaceTests.TestEnumerableElements(list5, new int[] { 1, 2, 3 });
            list3[3] = 7;
            list4[1] = 2;
            list5[2] = 9;
            InterfaceTests.TestEnumerableElements(list4, new int[] { 3, 2, 5 });
            InterfaceTests.TestEnumerableElements(list5, new int[] { 1, 2, 9 });

            list1 = CreateList(0, 132);
            list2 = list1.GetRange(27, 53);
            for (int i = 0; i < 53; ++i)
                Assert.AreEqual(27 + i, list2[i]);
            int y = 27;
            foreach (int x in list2)
                Assert.AreEqual(y++, x);

            list3 = list2.GetRange(4, 27);
            for (int i = 0; i < 27; ++i)
                Assert.AreEqual(31 + i, list3[i]);
            y = 31;
            foreach (int x in list3)
                Assert.AreEqual(y++, x);
        }

        [Test]
        public void GetRangeExceptions()
        {
            BigList<int> list1 = CreateList(0, 100);

            try {
                list1.GetRange(3, 98);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(-1, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(0, int.MaxValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(1, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(45, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(0, 101);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(100, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(int.MinValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.GetRange(int.MaxValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

        }

        [Test]
        public void ConcatLeaf()
        {
            BigList<int> list1, list2, list3;
            list1 = CreateList(0, 5);
            list2 = CreateList(5, 7);
            list3 = list1 + list2;
            list1[3] = -1;
            list2[4] = -1;
            for (int i = 0; i < list3.Count; ++i)
                Assert.AreEqual(i, list3[i]);
        }

        [Test]
        public void PrependLeaf()
        {
            BigList<int> list1, list2, list3;

            list1 = new BigList<int>();
            for (int i = 2; i < 50; ++i)
                list1.Add(i);
            list1.AddToFront(1);
            list1.AddToFront(0);
            list3 = list1.Clone();
            list2 = CreateList(0, 2);
            list1.AddRangeToFront(list2);
            list1[17] = -1;
            for (int i = 0; i < 50; ++i)
                Assert.AreEqual(i, list3[i]);
        }


        [Test]
        public void Indexer()
        {
            BigList<int> list1, list2, list3;
            int i;

            list1 = new BigList<int>();
            for (i = 0; i < 100; ++i)
                list1.Add(i);
            for (i = 99; i >= 0; --i)
                Assert.AreEqual(i, list1[i]);

            list2 = list1.Clone();
            for (i = 44; i < 88; ++i)
                list1[i] = i * 2;
#if DEBUG
            list1.Print();
            list2.Print();
#endif //DEBUG
            for (i = 99; i >= 0; --i) {
                Assert.AreEqual(i, list2[i]);
                list2[i] = 99 * i;
#if DEBUG
                list2.Print();
#endif //DEBUG
            }
            for (i = 44; i < 88; ++i)
                Assert.AreEqual(i * 2, list1[i]);


            list1 = new BigList<int>();
            list2 = new BigList<int>();
            i = 0;
            while (i < 55)
                list1.Add(i++);
            while (i < 100)
                list2.Add(i++);
            list3 = list1 + list2;
            for (i = 0; i < 100; ++i)
                list3[i] = i * 2;
            for (i = 0; i < list1.Count; ++i)
                Assert.AreEqual(i, list1[i]);
            for (i = 0; i < list2.Count; ++i)
                Assert.AreEqual(i + 55, list2[i]);

            list1.Clear();
            i = 0;
            while (i < 100)
                list1.Add(i++);
            list1.AddRange(CreateList(100, 400));
            for (i = 100; i < 200; ++i)
                list1[i] = -1;
            list2 = list1.GetRange(33, 200);
            for (i = 0; i < list2.Count; ++i) {
                if (i < 67 || i >= 167)
                    Assert.AreEqual(i + 33, list2[i]);
                else
                    Assert.AreEqual(-1, list2[i]);
            }

            for (i = 22; i < 169; ++i)
                list1[i] = 187 * i;
            for (i = 0; i < list2.Count; ++i) {
                if (i < 67 || i >= 167)
                    Assert.AreEqual(i + 33, list2[i]);
                else
                    Assert.AreEqual(-1, list2[i]);
            }
            for (i = 168; i >= 22; --i)
                Assert.AreEqual(187 * i, list1[i]);

            list1.Clear();
            list1.Add(1);
            list1.Add(2);
            list1.Add(3);
            Assert.AreEqual(1, list1[0]);
            Assert.AreEqual(2, list1[1]);
            Assert.AreEqual(3, list1[2]);
            list2 = list1.Clone();
            list1[1] = 4;
            list2[0] = 11;
            Assert.AreEqual(11, list2[0]);
            Assert.AreEqual(2, list2[1]);
            Assert.AreEqual(3, list2[2]);
            Assert.AreEqual(1, list1[0]);
            Assert.AreEqual(4, list1[1]);
            Assert.AreEqual(3, list1[2]);
        }

        [Test]
        public void IndexerExceptions()
        {
            BigList<int> list1;
            int x;

            list1 = new BigList<int>();
            try {
                list1[0] = 1;
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                x = list1[0];
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            list1 = new BigList<int>(new int[] { 1, 2, 3 });

            list1 = new BigList<int>();
            try {
                list1[-1] = 1;
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                x = list1[-1];
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            list1 = new BigList<int>();
            try {
                list1[3] = 1;
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                x = list1[3];
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            list1 = new BigList<int>();
            try {
                list1[int.MaxValue] = 1;
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                x = list1[int.MaxValue];
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            list1 = new BigList<int>();
            try {
                list1[int.MinValue] = 1;
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                x = list1[int.MinValue];
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void InsertItem()
        {
            BigList<int> list1, list2, list3;

            list1 = new BigList<int>();
            list1.Insert(0, 34);
            Assert.AreEqual(1, list1.Count);
            Assert.AreEqual(34, list1[0]);
            list1.Insert(1, 78);
            list1.Insert(0, 11);
            list1.Insert(1, 13);
            InterfaceTests.TestEnumerableElements<int>(list1, new int[] { 11, 13, 34, 78 });
#if DEBUG
            list1.Validate();
#endif //DEBUG

            list2 = CreateList(0, 100);
            int j = 300;
            for (int i = 0; i < list2.Count; i += 3)
                list2.Insert(i, j++);
#if DEBUG
            list2.Validate();
#endif //DEBUG
            int k = 0;
            j = 300;
            for (int i = 0; i < list2.Count; ++i) {
                if (i % 3 == 0) {
                    Assert.AreEqual(j++, list2[i]);
                }
                else {
                    Assert.AreEqual(k++, list2[i]);
                }
            }

            list3 = new BigList<int>();
            for (int i = 0; i < 32; ++i)
                list3.Add(i);

            list3.Insert(24, 101);
            list3.Insert(16, 100);
            list3.Insert(8, 102);
            InterfaceTests.TestEnumerableElements<int>(list3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 102, 8, 9, 10, 11, 12, 13, 14, 15, 100, 16, 17, 18, 19, 20, 21, 22, 23, 101, 24, 25, 26, 27,  28, 29, 30, 31 });
        }

        [Test]
        public void InsertList()
        {
            BigList<int> list2, list3;
            list2 = CreateList(0, 20);
            list3 = CreateList(-10, 10);
            list2.InsertRange(0, list3);
            list2.InsertRange(17, list3);
            InterfaceTests.TestEnumerableElements<int>(list2, new int[] { -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 });
#if DEBUG
            list2.Validate();
#endif //DEBUG

            list2 = CreateList(0, 20);
            list3 = CreateList(-10, 2);
            list2.InsertRange(0, list3);
            list2.InsertRange(17, list3);
            InterfaceTests.TestEnumerableElements<int>(list2, new int[] { -10, -9,  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, -10, -9, 15, 16, 17, 18, 19 });
#if DEBUG
            list2.Validate();
#endif //DEBUG
        }

        [Test]
        public void InsertEnumerable()
        {
            BigList<int> list2;
            IEnumerable<int> e1;
            list2 = CreateList(0, 20);
            e1 = new int[] {-10, -9, -8, -7, -6, -5, -4, -3, -2, -1};
            list2.InsertRange(0, e1);
            list2.InsertRange(17, e1);
            InterfaceTests.TestEnumerableElements<int>(list2, new int[] { -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 });
#if DEBUG
            list2.Validate();
#endif //DEBUG

            list2 = CreateList(0, 20);
            e1 = new int[] { -10, -9 };
            list2.InsertRange(0, e1);
            list2.InsertRange(17, e1);
            InterfaceTests.TestEnumerableElements<int>(list2, new int[] { -10, -9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, -10, -9, 15, 16, 17, 18, 19 });
#if DEBUG
            list2.Validate();
#endif //DEBUG

            list2 = new BigList<int>(new int[] { 1, 2, 3, 4, 5 });
            list2.InsertRange(2, new int[] { 9, 8});
            InterfaceTests.TestEnumerableElements<int>(list2, new int[] { 1, 2, 9, 8, 3, 4, 5});

            list2 = new BigList<int>();
            list2.Add(1);
            list2.Add(2);
            list2.InsertRange(1, new int[] { 6, 5, 4 });
            list2.InsertRange(2, new int[] { 9, 8 });
            InterfaceTests.TestEnumerableElements<int>(list2, new int[] { 1, 6, 9, 8, 5, 4, 2 });
        }

        [Test]
        public void InsertExceptions()
        {
            BigList<int> list1, list2;
            list1 = CreateList(0, 10);
            list2 = CreateList(4, 5);

            try {
                list1.Insert(-1, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1.Insert(11, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1.InsertRange(-1, list2);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1.InsertRange(11, list2);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1.InsertRange(-1, new int[] { 3, 4, 5 });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1.InsertRange(11, new int[] { 3, 4, 5 });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }
        }

        [Test]
        public void RemoveAt()
        {
            BigList<int> list1 = new BigList<int>();
            for (int i = 0; i < 100; ++i)
                list1.Add(i);

            for (int i = 0; i < 50; ++i)
                list1.RemoveAt(50);

            list1.RemoveAt(0);

            for (int i = 1; i < list1.Count; i += 2)
                list1.RemoveAt(i);

            InterfaceTests.TestEnumerableElements<int>(list1, new int[] { 1, 3, 4, 6, 7, 9, 10, 12, 13, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 31, 33, 34, 36, 37, 39, 40, 42, 43, 45, 46, 48, 49 });

            list1 = CreateList(0, 100);

            for (int i = 0; i < 50; ++i)
                list1.RemoveAt(50);

            list1.RemoveAt(0);

            for (int i = 1; i < list1.Count; i += 2)
                list1.RemoveAt(i);

            InterfaceTests.TestEnumerableElements<int>(list1, new int[] { 1, 3, 4, 6, 7, 9, 10, 12, 13, 15, 16, 18, 19, 21, 22, 24, 25, 27, 28, 30, 31, 33, 34, 36, 37, 39, 40, 42, 43, 45, 46, 48, 49 });
        }

        [Test]
        public void RemoveRange()
        {
            BigList<int> list1 = new BigList<int>();
            for (int i = 0; i < 200; ++i)
                list1.Add(i);

            list1.RemoveRange(0, 5);
            list1.RemoveRange(194, 1);
            list1.RemoveRange(50, 0);
            list1.RemoveRange(30, 25);
            list1.RemoveRange(120, 37);

            InterfaceTests.TestEnumerableElements<int>(list1, new int[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 
                32, 33, 34, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 
                96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 
                125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 187, 188, 189, 
                190, 191, 192, 193, 194, 195, 196, 197, 198 });

            list1 = CreateList(0, 200);

            list1.RemoveRange(0, 5);
            list1.RemoveRange(194, 1);
            list1.RemoveRange(50, 0);
            list1.RemoveRange(30, 25);
            list1.RemoveRange(120, 37);

            InterfaceTests.TestEnumerableElements<int>(list1, new int[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 
                32, 33, 34, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 
                96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 
                125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 187, 188, 189, 
                190, 191, 192, 193, 194, 195, 196, 197, 198 });

        }

        [Test]
        public void RemoveRangeExceptions()
        {
            BigList<int> list1 = CreateList(0, 100);

            try {
                list1.RemoveRange(3, 98);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(-1, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(0, int.MaxValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(1, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(45, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(0, 101);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(100, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(int.MinValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                list1.RemoveRange(int.MaxValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }
        }

        [Test]
        public void AddToSelf()
        {
            BigList<int> list1 = new BigList<int>();

            for (int i = 0; i < 20; ++i)
                list1.Add(i);

            list1.AddRange(list1);
#if DEBUG
            list1.Validate();
#endif //DEBUG
            Assert.AreEqual(40, list1.Count);
            for (int i = 0; i < 40; ++i)
                Assert.AreEqual(i % 20, list1[i]);

            list1.Clear();
            for (int i = 0; i < 20; ++i)
                list1.Add(i);

            list1.AddRangeToFront(list1);
#if DEBUG
            list1.Validate();
#endif //DEBUG
            Assert.AreEqual(40, list1.Count);
            for (int i = 0; i < 40; ++i)
                Assert.AreEqual(i % 20, list1[i]);


            list1.Clear();
            for (int i = 0; i < 20; ++i)
                list1.Add(i);

            list1.InsertRange(7, list1);
#if DEBUG
            list1.Validate();
#endif //DEBUG
            Assert.AreEqual(40, list1.Count);
            for (int i = 0; i < 40; ++i) {
                if (i < 7)
                    Assert.AreEqual(i, list1[i]);
                else if (i >= 7 && i < 27)
                    Assert.AreEqual(i - 7, list1[i]);
                else if (i >= 27)
                    Assert.AreEqual(i - 20, list1[i]);
            }
        }

        void CheckListAndBigList(List<int> l1, BigList<int> l2, Random rand)
        {
            Assert.AreEqual(l1.Count, l2.Count);

            for (int i = 0; i < l1.Count; ++i) {
                Assert.AreEqual(l1[i], l2[i]);
            }

            int j = 0;
            foreach (int x in l2) {
                Assert.AreEqual(l1[j++], x);
            }

            int start = rand.Next(l1.Count);
            int count = rand.Next(l1.Count - start);

            j = start;
            foreach (int x in l2.Range(start, count)) {
                Assert.AreEqual(l1[j++], x);
            }
            Assert.AreEqual(j, start+count);
        }

        [Test]
        public void GenericIListInterface()
        {
            BigList<int> list = new BigList<int>();
            int[] array = new int[0];
            InterfaceTests.TestReadWriteListGeneric<int>((IList<int>)list, array);

            list = CreateList(0, 5);
            array = new int[5];
            for (int i = 0; i < array.Length; ++i)
                array[i] = i;
            InterfaceTests.TestReadWriteListGeneric<int>((IList<int>)list, array);

            list = CreateList(0, 300);
            array = new int[300];
            for (int i = 0; i < array.Length; ++i)
                array[i] = i;
            InterfaceTests.TestReadWriteListGeneric<int>((IList<int>)list, array);
        }

        [Test]
        public void IListInterface()
        {
            BigList<int> list = new BigList<int>();
            int[] array = new int[0];
            InterfaceTests.TestReadWriteList<int>((IList)list, array);

            list = CreateList(0, 5);
            array = new int[5];
            for (int i = 0; i < array.Length; ++i)
                array[i] = i;
            InterfaceTests.TestReadWriteList<int>((IList)list, array);

            list = CreateList(0, 300);
            array = new int[300];
            for (int i = 0; i < array.Length; ++i)
                array[i] = i;
            InterfaceTests.TestReadWriteList<int>((IList)list, array);
        }

        [Test]
        public void Clone()
        {
            BigList<int> list1, list2, list3, list4;

            list1 = new BigList<int>();
            list2 = list1.Clone();
            list3 = (BigList<int>)(((ICloneable)list1).Clone());
            list4 = new BigList<int>(list1);
            InterfaceTests.TestListGeneric<int>(list2, new int[0], null);
            InterfaceTests.TestListGeneric<int>(list3, new int[0], null);
            InterfaceTests.TestListGeneric<int>(list4, new int[0], null);
            list1.Add(5);
            InterfaceTests.TestListGeneric<int>(list2, new int[0], null);
            InterfaceTests.TestListGeneric<int>(list3, new int[0], null);
            InterfaceTests.TestListGeneric<int>(list4, new int[0], null);

            int[] array = {0, 1, 2, 3, 4};
            list1 = CreateList(0, 5);
            list2 = list1.Clone();
            list3 = (BigList<int>)(((ICloneable)list1).Clone());
            list4 = new BigList<int>(list1);
            InterfaceTests.TestListGeneric<int>(list2, array, null);
            InterfaceTests.TestListGeneric<int>(list3, array, null);
            InterfaceTests.TestListGeneric<int>(list4, array, null);
            list2[3] = -1;
            InterfaceTests.TestListGeneric<int>(list1, array, null);
            InterfaceTests.TestListGeneric<int>(list3, array, null);
            InterfaceTests.TestListGeneric<int>(list4, array, null);

            array = new int[100];
            for (int i = 0; i < 100; ++i)
                array[i] = i;
            list1 = CreateList(0, 100);
            list2 = list1.Clone();
            list3 = (BigList<int>)(((ICloneable)list1).Clone());
            list4 = new BigList<int>(list1);
            InterfaceTests.TestListGeneric<int>(list2, array, null);
            InterfaceTests.TestListGeneric<int>(list3, array, null);
            InterfaceTests.TestListGeneric<int>(list4, array, null);
            list4.Clear();
            InterfaceTests.TestListGeneric<int>(list1, array, null);
            InterfaceTests.TestListGeneric<int>(list2, array, null);
            InterfaceTests.TestListGeneric<int>(list3, array, null);
        }

        // Simple class for testing cloning.
        class MyInt : ICloneable
        {
            public int value;
            public MyInt(int value)
            {
                this.value = value;
            }

            public object Clone()
            {
                return new MyInt(value);
            }

            public override bool Equals(object obj)
            {
                return (obj is MyInt && ((MyInt)obj).value == value);
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        void CompareClones<T>(BigList<T> s1, BigList<T> s2)
        {
            IEnumerator<T> e1 = s1.GetEnumerator();
            IEnumerator<T> e2 = s2.GetEnumerator();

            Assert.IsTrue(s1.Count == s2.Count);

            // Check that the lists are equal, but not reference equals (e.g., have been cloned).
            while (e1.MoveNext()) {
                e2.MoveNext();
                if (e1.Current == null) {
                    Assert.IsNull(e2.Current);
                }
                else {
                    Assert.IsTrue(e1.Current.Equals(e2.Current));
                    Assert.IsFalse(object.ReferenceEquals(e1.Current, e2.Current));
                }
            }
        }

        [Test]
        public void CloneContents()
        {
            BigList<MyInt> list1 = new BigList<MyInt>();

            list1.Add(new MyInt(143));
            list1.Add(new MyInt(2));
            list1.Add(new MyInt(9));
            list1.Add(null);
            list1.Add(new MyInt(2));
            list1.Add(new MyInt(111));
            BigList<MyInt> list2 = list1.CloneContents();
            CompareClones(list1, list2);

            BigList<int> list3 = new BigList<int>(new int[] { 144, 5, 23, 1, 0, 8 });
            BigList<int> list4 = list3.CloneContents();
            CompareClones(list3, list4);

            BigList<UtilTests.CloneableStruct> list5 = new BigList<UtilTests.CloneableStruct>();
            list5.Add(new UtilTests.CloneableStruct(143));
            list5.Add(new UtilTests.CloneableStruct(5));
            list5.Add(new UtilTests.CloneableStruct(23));
            list5.Add(new UtilTests.CloneableStruct(1));
            list5.Add(new UtilTests.CloneableStruct(8));
            BigList<UtilTests.CloneableStruct> list6 = list5.CloneContents();

            Assert.AreEqual(list5.Count, list6.Count);

            // Check that the lists are equal, but not identical (e.g., have been cloned via ICloneable).
            IEnumerator<UtilTests.CloneableStruct> e1 = list5.GetEnumerator();
            IEnumerator<UtilTests.CloneableStruct> e2 = list6.GetEnumerator();

            // Check that the lists are equal, but not reference equals (e.g., have been cloned).
            while (e1.MoveNext()) {
                e2.MoveNext();
                Assert.IsTrue(e1.Current.Equals(e2.Current));
                Assert.IsFalse(e1.Current.Identical(e2.Current));
            }
        }

        class NotCloneable { }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CantCloneContents()
        {
            BigList<NotCloneable> list1 = new BigList<NotCloneable>();

            list1.Add(new NotCloneable());
            list1.Add(new NotCloneable());

            BigList<NotCloneable> list2 = list1.CloneContents();
        }



        [Test]
        public void MultiCopies()
        {
            BigList<int> list1;

            // Check empty special case.
            list1 = new BigList<int>(new int[] { 1, 2, 3 }, 0);
            InterfaceTests.TestListGeneric<int>(list1, new int[0], null);
            list1 = new BigList<int>(new int[] { }, 5);
            InterfaceTests.TestListGeneric<int>(list1, new int[0], null);
            list1 = new BigList<int>(new BigList<int>(new int[] { 1, 2, 3 }), 0);
            InterfaceTests.TestListGeneric<int>(list1, new int[0], null);
            list1 = new BigList<int>(new BigList<int>(), 5);
            InterfaceTests.TestListGeneric<int>(list1, new int[0], null);

            // Small cases.
            list1 = new BigList<int>(new int[] { 1, 2, 3, 4 }, 7);
            InterfaceTests.TestListGeneric<int>(list1, new int[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 }, null);
            list1[7] = 12;
            InterfaceTests.TestListGeneric<int>(list1, new int[] { 1, 2, 3, 4, 1, 2, 3, 12, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 }, null);
            list1 = new BigList<int>(new BigList<int>(new int[] { 1, 2, 3, 4 }), 7);
            list1[17] = 13;
            InterfaceTests.TestListGeneric<int>(list1, new int[] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 13, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 }, null);

            // Large cases.
            int i;
            list1 = new BigList<int>(new int[] { 0, 1, 2 }, 1789345);
            Assert.AreEqual(list1.Count, 1789345 * 3);
            list1[1765] = 12;
            i = 0;
            foreach (int x in list1) {
                if (i != 1765)
                    Assert.AreEqual(i % 3, x);
                else
                    Assert.AreEqual(12, x);
                ++i;
            }

            list1 = new BigList<int>(new BigList<int>(new int[] { 0, 1, 2 }, 1354), 1789);
            Assert.AreEqual(list1.Count, 1354 * 1789 * 3);
            list1[17645] = 12;
            i = 0;
            foreach (int x in list1) {
                if (i != 17645)
                    Assert.AreEqual(i % 3, x);
                else
                    Assert.AreEqual(12, x);
                ++i;
            }

            list1 = new BigList<int>(new int[] { 6 }, int.MaxValue - 1);
            Assert.AreEqual(int.MaxValue - 1, list1.Count);

            try {
                list1 = new BigList<int>((BigList<int>)null, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentNullException);
            }

            try {
                list1 = new BigList<int>((IEnumerable<int>)null, 5);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentNullException);
            }

            try {
                list1 = new BigList<int>(new int[] { 1, 2, 3 }, -1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1 = new BigList<int>(new BigList<int>(new int[] { 1, 2, 3 }), -1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
            }

            try {
                list1 = new BigList<int>(new BigList<int>(new int[] { 1, 2, 3 }), 2000000000);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                list1 = new BigList<int>(new int[] { 1, 2, 3 }, 1000000000);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }
        }

        [Test]
        public void AsReadOnly()
        {
            BigList<int> list1 = CreateList(0, 400);
            int[] elements = new int[400];
            IList<int> list2 = list1.AsReadOnly();

            for (int i = 0; i < 400; ++i)
                elements[i] = i;

            InterfaceTests.TestReadOnlyListGeneric<int>(list2, elements, null);

            list1.Add(27);
            list1.AddToFront(98);
            list1[17] = 9;

            elements = new int[402];
            list2 = list1.AsReadOnly();

            for (int i = 0; i < 401; ++i)
                elements[i] = i -1;

            elements[0] = 98;
            elements[401] = 27;
            elements[17] = 9;

            InterfaceTests.TestReadOnlyListGeneric<int>(list2, elements, null);

            list1 = new BigList<int>();
            list2 = list1.AsReadOnly();
            InterfaceTests.TestReadOnlyListGeneric<int>(list2, new int[0], null);
            list1.Add(4);
            InterfaceTests.TestReadOnlyListGeneric<int>(list2, new int[] { 4 }, null);
        }

        [Test]
        public void Exists()
        {
            BigList<int> list1 = CreateList(0, 400);
            list1[57] = 7123;

            bool result;

            result = list1.Exists(delegate(int x) { return x > 7120; });
            Assert.IsTrue(result);
            result = list1.Exists(delegate(int x) { return x > 7124; });
            Assert.IsFalse(result);
            result = list1.Exists(delegate(int x) { return x % 187 == 44; });
            Assert.IsTrue(result);
            result = list1.Exists(delegate(int x) { return x >= 0; });
            Assert.IsTrue(result);

            list1 = new BigList<int>();
            result = list1.Exists(delegate(int x) { return true; });
            Assert.IsFalse(result);
        }

        [Test]
        public void TrueForAll()
        {
            BigList<int> list1 = CreateList(0, 400);
            list1[57] = 7123;

            bool result;

            result = list1.TrueForAll(delegate(int x) { return x < 7124; });
            Assert.IsTrue(result);
            result = list1.TrueForAll(delegate(int x) { return x < 500; });
            Assert.IsFalse(result);
            result = list1.TrueForAll(delegate(int x) { return x % 187 == 44; });
            Assert.IsFalse(result);
            result = list1.TrueForAll(delegate(int x) { return x >= 0; });
            Assert.IsTrue(result);

            list1 = new BigList<int>();
            result = list1.TrueForAll(delegate(int x) { return false; });
            Assert.IsTrue(result);

            List<int> list2 = new List<int>();
            result = list2.TrueForAll(delegate(int x) { return false; });
            Assert.IsTrue(result);
        }

        [Test]
        public void ConvertAll()
        {
            BigList<int> list1 = CreateList(0, 400);
            BigList<string> list2;

            list2 = list1.ConvertAll<string>(delegate(int x) { return (x * 2).ToString(); });
            string[] expected = new string[400];
            for (int i = 0; i < 400; ++i)
                expected[i] = (2 * i).ToString();
#if DEBUG
            list2.Validate();
#endif //DEBUG
            InterfaceTests.TestReadWriteListGeneric<string>(list2, expected);

            list1 = new BigList<int>();
            list2 = list1.ConvertAll<string>(delegate(int x) { return (x * 2).ToString(); });
#if DEBUG
            list2.Validate();
#endif //DEBUG
            InterfaceTests.TestReadWriteListGeneric<string>(list2, new string[0]);
        }

        [Test]
        public void Find()
        {
            BigList<int> list1 = new BigList<int>(new int[] { 4, 8, 1, 3, 4, 9 });
            bool found;
            int result;

            Assert.AreEqual(1, list1.Find(delegate(int x) { return (x & 1) == 1; }));
            found = list1.TryFind(delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(1, result);

            Assert.AreEqual(4, list1.Find(delegate(int x) { return (x & 1) == 0; }));
            found = list1.TryFind(delegate(int x) { return (x & 1) == 0; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(4, result);

            Assert.AreEqual(0, list1.Find(delegate(int x) { return x > 10; }));
            found = list1.TryFind(delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            list1 = new BigList<int>(new int[] { 4, 0, 1, 3, 4, 9 });

            Assert.AreEqual(0, list1.Find(delegate(int x) { return x < 3; }));
            found = list1.TryFind(delegate(int x) { return x < 3; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(0, result);

            Assert.AreEqual(0, list1.Find(delegate(int x) { return x > 10; }));
            found = list1.TryFind(delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            list1 = new BigList<int>();

            Assert.AreEqual(0, list1.Find(delegate(int x) { return x < 3; }));
            found = list1.TryFind(delegate(int x) { return x < 3; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindLast()
        {
            BigList<int> list1 = new BigList<int>(new int[] { 4, 8, 1, 3, 2, 9 });
            bool found;
            int result;

            Assert.AreEqual(9, list1.FindLast(delegate(int x) { return (x & 1) == 1; }));
            found = list1.TryFindLast(delegate(int x) { return (x & 1) == 1; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(9, result);

            Assert.AreEqual(2, list1.FindLast(delegate(int x) { return (x & 1) == 0; }));
            found = list1.TryFindLast(delegate(int x) { return (x & 1) == 0; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(2, result);

            Assert.AreEqual(0, list1.FindLast(delegate(int x) { return x > 10; }));
            found = list1.TryFindLast(delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            list1 = new BigList<int>(new int[] { 4, 8, 1, 3, 0, 9 });

            Assert.AreEqual(0, list1.FindLast(delegate(int x) { return x < 3; }));
            found = list1.TryFindLast(delegate(int x) { return x < 3; }, out result);
            Assert.IsTrue(found);
            Assert.AreEqual(0, result);

            Assert.AreEqual(0, list1.FindLast(delegate(int x) { return x > 10; }));
            found = list1.TryFindLast(delegate(int x) { return x > 10; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);

            list1 = new BigList<int>();

            Assert.AreEqual(0, list1.FindLast(delegate(int x) { return x < 3; }));
            found = list1.TryFindLast(delegate(int x) { return x < 3; }, out result);
            Assert.IsFalse(found);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void FindAll()
        {
            BigList<int> list1 = new BigList<int>(new int[] { 4, 8, 1, 3, 6, 9 });
            IEnumerable<int> found;

            found = list1.FindAll(delegate(int x) { return (x & 1) == 1; });
            InterfaceTests.TestEnumerableElements(found, new int[] { 1, 3, 9 });

            found = list1.FindAll(delegate(int x) { return (x & 1) == 0; });
            InterfaceTests.TestEnumerableElements(found, new int[] { 4, 8, 6 });

            found = list1.FindAll(delegate(int x) { return x > 10; });
            InterfaceTests.TestEnumerableElements(found, new int[] { });

            found = list1.FindAll(delegate(int x) { return x < 10; });
            InterfaceTests.TestEnumerableElements(found, new int[] { 4, 8, 1, 3, 6, 9 });

            list1 = new BigList<int>();
            found = list1.FindAll(delegate(int x) { return (x & 1) == 1; });
            InterfaceTests.TestEnumerableElements(found, new int[] { });
        }

        [Test]
        public void FindIndex()
        {
            BigList<int> list1 = new BigList<int>(new int[] { 4, 2, 1, 3, 9, 4 });

            Assert.AreEqual(2, list1.FindIndex(delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(0, list1.FindIndex(delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(-1, list1.FindIndex(delegate(int x) { return x > 10; }));
            Assert.AreEqual(4, list1.FindIndex(delegate(int x) { return x > 5; }));
            Assert.AreEqual(3, list1.FindIndex(3, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(5, list1.FindIndex(3, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(-1, list1.FindIndex(5, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(2, list1.FindIndex(1, 4, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(3, list1.FindIndex(3, 2, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(-1, list1.FindIndex(3, 2, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(-1, list1.FindIndex(3, 0, delegate(int x) { return (x & 1) == 1; }));

            list1 = new BigList<int>(new int[] { 4, 0, 1, 3, 4, 9 });

            Assert.AreEqual(1, list1.FindIndex(delegate(int x) { return x < 3; }));
            Assert.AreEqual(-1, list1.FindIndex(delegate(int x) { return x > 10; }));

            list1 = new BigList<int>();

            Assert.AreEqual(-1, list1.FindIndex(delegate(int x) { return x < 3; }));
        }

        [Test]
        public void FindLastIndex()
        {
            BigList<int> list1 = new BigList<int>(new int[] { 4, 2, 1, 3, 9, 4 });

            Assert.AreEqual(4, list1.FindLastIndex(delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(5, list1.FindLastIndex(delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(-1, list1.FindLastIndex(delegate(int x) { return x > 10; }));
            Assert.AreEqual(2, list1.FindLastIndex(delegate(int x) { return x < 3; }));
            Assert.AreEqual(3, list1.FindLastIndex(3, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(1, list1.FindLastIndex(3, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(-1, list1.FindLastIndex(1, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(1, list1.FindLastIndex(1, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(-1, list1.FindLastIndex(3, delegate(int x) { return x > 10; }));
            Assert.AreEqual(0, list1.FindLastIndex(3, delegate(int x) { return x > 3; }));

            list1 = new BigList<int>(new int[] { 4, 0, 8, 3, 4, 9 });

            Assert.AreEqual(3, list1.FindLastIndex(delegate(int x) { return x < 4; }));
            Assert.AreEqual(1, list1.FindLastIndex(delegate(int x) { return x < 1; }));
            Assert.AreEqual(-1, list1.FindLastIndex(delegate(int x) { return x > 10; }));
            Assert.AreEqual(3, list1.FindLastIndex(3, 1, delegate(int x) { return (x & 1) == 1; }));
            Assert.AreEqual(-1, list1.FindLastIndex(3, 1, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(4, list1.FindLastIndex(5, 3, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(2, list1.FindLastIndex(3, 3, delegate(int x) { return (x & 1) == 0; }));
            Assert.AreEqual(2, list1.FindLastIndex(4, 4, delegate(int x) { return x > 7; }));
            Assert.AreEqual(-1, list1.FindLastIndex(3, 4, delegate(int x) { return x > 8; }));

            list1 = new BigList<int>();

            Assert.AreEqual(-1, list1.FindLastIndex(delegate(int x) { return x < 3; }));
        }

        [Test]
        public void IndexOf()
        {
            BigList<int> list = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9, 7, 11, 4, 9, 1, 7, 19, 1, 7 });
            int index;

            index = list.IndexOf(1);
            Assert.AreEqual(2, index);

            index = list.IndexOf(4);
            Assert.AreEqual(0, index);

            index = list.IndexOf(9);
            Assert.AreEqual(5, index);

            index = list.IndexOf(12);
            Assert.AreEqual(-1, index);

            index = list.IndexOf(1, 10);
            Assert.AreEqual(10, index);

            index = list.IndexOf(1, 11);
            Assert.AreEqual(13, index);

            index = list.IndexOf(1, 10);
            Assert.AreEqual(10, index);

            index = list.IndexOf(7, 12);
            Assert.AreEqual(14, index);

            index = list.IndexOf(9, 3, 3);
            Assert.AreEqual(5, index);

            index = list.IndexOf(9, 3, 2);
            Assert.AreEqual(-1, index);

            index = list.IndexOf(4, 5, 5);
            Assert.AreEqual(8, index);

            index = list.IndexOf(7, 11, 4);
            Assert.AreEqual(11, index);

            index = list.IndexOf(7, 12, 3);
            Assert.AreEqual(14, index);


            list = new BigList<int>();
            index = list.IndexOf(1);
            Assert.AreEqual(-1, index);
        }

        // Just overrides equal -- no comparison, ordering, or hash code.
#pragma warning disable 659  // missing GetHashCode
        sealed class MyDouble
        {
            double val;

            public MyDouble(double value)
            {
                val = value;
            }

            public override bool Equals(object obj)
            {
                return (obj is MyDouble && ((MyDouble)obj).val == this.val);
            }
        }
#pragma warning restore 659

        [Test]
        public void IndexOf2()
        {
            BigList<MyDouble> list = new BigList<MyDouble>(new MyDouble[] { new MyDouble(4), new MyDouble(8), new MyDouble(1), new MyDouble(1), new MyDouble(4), new MyDouble(9)});
            int index;

            index = list.IndexOf(new MyDouble(1));
            Assert.AreEqual(2, index);

            index = list.IndexOf(new MyDouble(1.1));
            Assert.IsTrue(index < 0);

            index = list.IndexOf(new MyDouble(4), 2);
            Assert.AreEqual(4, index);

            index = list.IndexOf(new MyDouble(8), 2);
            Assert.IsTrue(index < 0);

            index = list.IndexOf(new MyDouble(1), 1, 3);
            Assert.AreEqual(2, index);

            index = list.IndexOf(new MyDouble(4), 1, 3);
            Assert.IsTrue(index < 0);
        }

        [Test]
        public void LastIndexOf()
        {
            //                                                                        0  1  2  3  4  5  6  7   8  9 10 11 12 13 14
            BigList<int> list = new BigList<int>(new int[] { 4, 8, 1, 1, 4, 9, 7, 11, 4, 9, 1, 7, 19, 1, 7 });
            int index;

            index = list.LastIndexOf(1);
            Assert.AreEqual(13, index);

            index = list.LastIndexOf(4);
            Assert.AreEqual(8, index);

            index = list.LastIndexOf(9);
            Assert.AreEqual(9, index);

            index = list.LastIndexOf(12);
            Assert.AreEqual(-1, index);

            index = list.LastIndexOf(1, 13);
            Assert.AreEqual(13, index);

            index = list.LastIndexOf(1, 12);
            Assert.AreEqual(10, index);

            index = list.LastIndexOf(1, 1);
            Assert.AreEqual(-1, index);

            index = list.LastIndexOf(7, 12);
            Assert.AreEqual(11, index);

            index = list.LastIndexOf(7, 6);
            Assert.AreEqual(6, index);

            index = list.LastIndexOf(9, 5, 3);
            Assert.AreEqual(5, index);

            index = list.LastIndexOf(9, 8, 5);
            Assert.AreEqual(5, index);

            index = list.LastIndexOf(9, 3, 2);
            Assert.AreEqual(-1, index);

            index = list.LastIndexOf(4, 5, 6);
            Assert.AreEqual(4, index);

            index = list.LastIndexOf(4, 3, 4);
            Assert.AreEqual(0, index);

            index = list.LastIndexOf(1, 14, 3);
            Assert.AreEqual(13, index);

            index = list.LastIndexOf(1, 0, 0);
            Assert.AreEqual(-1, index);

            index = list.LastIndexOf(1, 14, 0);
            Assert.AreEqual(-1, index);

            list = new BigList<int>();
            index = list.LastIndexOf(1);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void LastIndexOf2()
        {
            BigList<MyDouble> list = new BigList<MyDouble>(new MyDouble[] { new MyDouble(4), new MyDouble(8), new MyDouble(1), new MyDouble(1), new MyDouble(4), new MyDouble(9) });
            int index;

            index = list.LastIndexOf(new MyDouble(1));
            Assert.AreEqual(3, index);

            index = list.LastIndexOf(new MyDouble(1.1));
            Assert.IsTrue(index < 0);

            index = list.LastIndexOf(new MyDouble(4), 2);
            Assert.AreEqual(0, index);

            index = list.LastIndexOf(new MyDouble(9), 2);
            Assert.IsTrue(index < 0);

            index = list.LastIndexOf(new MyDouble(1), 4, 3);
            Assert.AreEqual(3, index);

            index = list.LastIndexOf(new MyDouble(8), 4, 3);
            Assert.IsTrue(index < 0);
        }

        [Test]
        public void Range()
        {
            BigList<int> main = new BigList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            IList<int> range = main.Range(2, 4);

            InterfaceTests.TestListGeneric(range, new int[] { 2, 3, 4, 5 }, null);

            main = new BigList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = main.Range(2, 4);
            range[1] = 7;
            range.Add(99);
            Assert.AreEqual(5, range.Count);
            range.RemoveAt(0);
            Assert.AreEqual(4, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 7, 4, 5, 99, 6, 7 });
            main[3] = 11;
            InterfaceTests.TestEnumerableElements(range, new int[] { 7, 11, 5, 99 });

            main = new BigList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = main.Range(5, 3);
            Assert.AreEqual(3, range.Count);
            main.Remove(6);
            main.Remove(5);
            Assert.AreEqual(1, range.Count);
            Assert.AreEqual(7, range[0]);

            main = new BigList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = main.Range(8, 0);
            range.Add(8);
            range.Add(9);
            InterfaceTests.TestEnumerableElements(main, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            InterfaceTests.TestEnumerableElements(range, new int[] { 8, 9 });

            main = new BigList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = main.Range(0, 4);
            range.Clear();
            Assert.AreEqual(0, range.Count);
            InterfaceTests.TestEnumerableElements(main, new int[] { 4, 5, 6, 7 });
            range.Add(100);
            range.Add(101);
            InterfaceTests.TestEnumerableElements(main, new int[] { 100, 101, 4, 5, 6, 7 });

            main = new BigList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            range = main.Range(8, 0);
            InterfaceTests.TestListGeneric(range, new int[] { }, null);
        }

        [Test]
        public void RangeExceptions()
        {
            BigList<int> list = new BigList<int>(new int[] { 1 }, 100);
            IList<int> range;

            try {
                range = list.Range(3, 98);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(-1, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(0, int.MaxValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(1, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(45, int.MinValue);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(0, 101);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("count", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(100, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(int.MinValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }

            try {
                range = list.Range(int.MaxValue, 1);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentOutOfRangeException);
                Assert.AreEqual("index", ((ArgumentOutOfRangeException)e).ParamName);
            }
        }

        [Test]
        public void Reverse()
        {
            BigList<string> list1 = new BigList<string>();
            list1.Reverse();
            InterfaceTests.TestEnumerableElements(list1, new string[0]);

            BigList<string> list2 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            list2.Reverse();
            InterfaceTests.TestListGeneric<string>(list2, new string[] { "glove", "the", "smell", "baz", "bar", "foo" }, null);

            BigList<string> list3 = new BigList<string>(new string[] { "foo", "baz", "smell", "the", "glove" });
            list3.Reverse();
            InterfaceTests.TestListGeneric<string>(list3, new string[] { "glove", "the", "smell", "baz", "foo" }, null);

            BigList<string> list4 = new BigList<string>(new string[] { "foo", "baz", "smell", "the", "glove" });
            list4.Reverse(1, 4);
            InterfaceTests.TestListGeneric<string>(list4, new string[] { "foo", "glove", "the", "smell", "baz" }, null);

            BigList<string> list5 = new BigList<string>(new string[] { "foo", "bar", "baz", "smell", "the", "glove" });
            list5.Reverse(3, 2);
            InterfaceTests.TestListGeneric<string>(list5, new string[] { "foo", "bar", "baz", "the", "smell", "glove" }, null);
        }

        private void CheckArray<T>(T[] actual, T[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < actual.Length; ++i) {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [Test]
        public void CopyTo1()
        {
            string[] array1 = { "foo", "bar", "baz", "smell", "the", "glove" };
            BigList<string> list1 = new BigList<string>(new string[] { "hello", "Sailor" });
            list1.CopyTo(array1);
            CheckArray<string>(array1, new string[] { "hello", "Sailor", "baz", "smell", "the", "glove" });

            BigList<string> list2 = new BigList<string>();
            list2.CopyTo(array1);
            CheckArray<string>(array1, new string[] { "hello", "Sailor", "baz", "smell", "the", "glove" });

            BigList<string> list3 = new BigList<string>(new string[] { "a1", "a2", "a3", "a4" });
            list3.CopyTo(array1);
            CheckArray<string>(array1, new string[] { "a1", "a2", "a3", "a4", "the", "glove" });

            BigList<string> list4 = new BigList<string>(new string[] { "b1", "b2", "b3", "b4", "b5", "b6" });
            list4.CopyTo(array1);
            CheckArray<string>(array1, new string[] { "b1", "b2", "b3", "b4", "b5", "b6" });

            list1.CopyTo(array1);
            CheckArray<string>(array1, new string[] { "hello", "Sailor", "b3", "b4", "b5", "b6" });

            BigList<string> list5 = new BigList<string>();
            string[] array2 = new string[0];
            list5.CopyTo(array2);
            CheckArray<string>(array2, new string[] { });
        }

        [Test]
        public void CopyTo2()
        {
            string[] array1 = { "foo", "bar", "baz", "smell", "the", "glove" };
            BigList<string> list1 = new BigList<string>(new string[] { "hello", "Sailor" });
            list1.CopyTo(array1, 3);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            BigList<string> list2 = new BigList<string>();
            list2.CopyTo(array1, 1);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "hello", "Sailor", "glove" });

            BigList<string> list3 = new BigList<string>(new string[] { "a1", "a2", "a3", "a4" });
            list3.CopyTo(array1, 2);
            CheckArray<string>(array1, new string[] { "foo", "bar", "a1", "a2", "a3", "a4" });

            BigList<string> list4 = new BigList<string>(new string[] { "b1", "b2", "b3", "b4", "b5", "b6" });
            list4.CopyTo(array1, 0);
            CheckArray<string>(array1, new string[] { "b1", "b2", "b3", "b4", "b5", "b6" });

            list1.CopyTo(array1, 4);
            CheckArray<string>(array1, new string[] { "b1", "b2", "b3", "b4", "hello", "Sailor" });

            BigList<string> list5 = new BigList<string>();
            string[] array2 = new string[0];
            list5.CopyTo(array2, 0);
            CheckArray<string>(array2, new string[] {  });
        }

        [Test]
        public void CopyTo3()
        {
            string[] array1 = { "foo", "bar", "baz", "smell", "the", "glove" };
            BigList<string> list1 = new BigList<string>(new string[] { "hello", "Sailor" });
            list1.CopyTo(1, array1, 3, 1);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "the", "glove" });
            list1.CopyTo(0, array1, 5, 1);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "the", "hello" });
            list1.CopyTo(2, array1, 6, 0);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "the", "hello" });

            BigList<string> list2 = new BigList<string>();
            list2.CopyTo(0, array1, 1, 0);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "the", "hello" });
            list2.CopyTo(0, array1, 0, 0);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "the", "hello" });
            list2.CopyTo(0, array1, 6, 0);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "the", "hello" });

            BigList<string> list3 = new BigList<string>(new string[] { "a1", "a2", "a3", "a4" });
            list3.CopyTo(1, array1, 4, 2);
            CheckArray<string>(array1, new string[] { "foo", "bar", "baz", "Sailor", "a2", "a3" });

            BigList<string> list4 = new BigList<string>(new string[] { "b1", "b2", "b3", "b4", "b5", "b6" });
            list4.CopyTo(0, array1, 0, 6);
            CheckArray<string>(array1, new string[] { "b1", "b2", "b3", "b4", "b5", "b6" });

            BigList<string> list5 = new BigList<string>();
            string[] array2 = new string[0];
            list5.CopyTo(0, array2, 0, 0);
            CheckArray<string>(array2, new string[] { });
        }

        [Test]
        public void FailFastEnumerator()
        {
            BigList<string> biglist1 = new BigList<string>(new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" });
            int i = 0;

            try {
                foreach (string s in biglist1) {
                    ++i;
                    Assert.IsTrue(i < 4);
                    if (i == 3)
                        biglist1.Add("hi");
                }
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            i = 0;
            try {
                foreach (string s in biglist1) {
                    ++i;
                    Assert.IsTrue(i < 4);
                    if (i == 3)
                        biglist1.AddToFront("hi");
                }
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            i = 0;
            try {
                foreach (string s in biglist1) {
                    ++i;
                    Assert.IsTrue(i < 4);
                    if (i == 3)
                        biglist1.RemoveRange(2, 4);
                }
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            i = 0;
            try {
                foreach (string s in biglist1) {
                    ++i;
                    Assert.IsTrue(i < 4);
                    if (i == 3)
                        biglist1[5] = "hi";
                }
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            i = 0;
            try {
                foreach (string s in biglist1) {
                    ++i;
                    Assert.IsTrue(i < 4);
                    if (i == 3)
                        biglist1.Clear();
                }
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }
        }

        [Test]
        public void ForEach()
        {
            BigList<string> list1 = new BigList<string>(new string[] { "foo", "bar", "hello", "sailor" });
            string s = "";
            list1.ForEach(delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "!foo!bar!hello!sailor");

            BigList<string> list2 = new BigList<string>();
            s = "";
            list2.ForEach(delegate(string x) { s += "!" + x; });
            Assert.AreEqual(s, "");
        }

        [Test]
        public void RemoveAll()
        {
            BigList<double> d_list = new BigList<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            ICollection<double> removed;

            removed = d_list.RemoveAll(delegate(double d) { return Math.Abs(d) > 5; });
            InterfaceTests.TestListGeneric(d_list, new double[] { 4.5, 1.2, -0.04, 1.78 }, null);
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] { 7.6, -7.6, 10.11, 187.4 }, true, null);

            d_list = new BigList<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            removed = d_list.RemoveAll(delegate(double d) { return d == 0; });
            InterfaceTests.TestListGeneric(d_list, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }, null);
            InterfaceTests.TestReadWriteCollectionGeneric(removed, new double[] { }, true, null);

            d_list = new BigList<double>(new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 });
            removed = d_list.RemoveAll(delegate(double d) { return d < 200; });
            InterfaceTests.TestReadWriteCollectionGeneric<double>(removed, new double[] { 4.5, 1.2, 7.6, -0.04, -7.6, 1.78, 10.11, 187.4 }, true, null);
            Assert.AreEqual(0, d_list.Count);

            d_list = new BigList<double>();
            removed = d_list.RemoveAll(delegate(double d) { return d < 200; });
            InterfaceTests.TestReadWriteCollectionGeneric<double>(removed, new double[] {  }, true, null);
            Assert.AreEqual(0, d_list.Count);
        }

        [Test]
        public void BinarySearch1()
        {
            const int SIZE = 100;
            const int MAX = 30;
            const int ITER = 1000;

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                BigList<int> list = new BigList<int>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(rand.Next(MAX));
                list.Sort();
                int find = rand.Next(MAX * 5 / 4) - MAX / 8;

                int index = list.BinarySearch(find);
                bool found = true;
                if (index < 0) {
                    found = false;
                    index = ~index;
                }

                if (index == 0) {
                    if (! found)
                        Assert.IsTrue(size == 0 || list[index] > find);
                    else
                        Assert.IsTrue(list[index] == find);
                }
                else {
                    if (! found) {
                        if (index >= size)
                            Assert.IsTrue(index == size && (size == 0 || list[index - 1] < find));
                        else
                            Assert.IsTrue(list[index] > find && list[index - 1] < find);
                    }
                    else
                        Assert.IsTrue(list[index] == find && list[index - 1] < find);
                }
            }
        }

        [Test]
        public void BinarySearch2()
        {
            BigList<String> list = new BigList<String>(new String[] { "foo", "Giraffe", "gorge", "HELLO", "hello", "number", "NUMber", "ooze" });
            int index;

            index = list.BinarySearch("GIRAFFE", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(1, index);

            index = list.BinarySearch("hEllo", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(3, index);

            index = list.BinarySearch("OODLE", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(~7, index);

            index = list.BinarySearch("zorch", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(~8, index);

            index = list.BinarySearch("FOO", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual(0, index);
        }


        [Test]
        public void Sort1()
        {
            const int SIZE = 1000;
            const int MAX = 750;
            const int ITER = 100;

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                BigList<int> list = new BigList<int>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(rand.Next(MAX));
                int[] copy = list.ToArray();

                list.Sort();
                Array.Sort(copy);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }

        [Test]
        public void Sort2()
        {
            const int SIZE = 1000;
            const int ITER = 100;

            Comparison<double> comp = delegate(double x, double y) {
                x = Math.Abs(x);
                y = Math.Abs(y);
                return x.CompareTo(y);
            };

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                BigList<double> list = new BigList<double>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(rand.NextDouble() - 0.5);
                double[] copy = list.ToArray();

                list.Sort(comp);
                Array.Sort(copy, comp);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }

        [Test]
        public void Sort3()
        {
            string[] strings = { "foo", "fOo2", "Fuzzle", "FOb", "zander", "Alphabet", "QuiRk" };

            const int SIZE = 17;
            const int ITER = 500;

            Random rand = new Random(12);

            for (int iter = 0; iter < ITER; ++iter) {
                IComparer<string> comp = (iter % 2 == 0) ? StringComparer.InvariantCultureIgnoreCase : StringComparer.Ordinal;
                BigList<string> list = new BigList<string>();
                int size = rand.Next(SIZE);
                for (int i = 0; i < size; ++i)
                    list.Add(strings[rand.Next(strings.Length)]);
                string[] copy = list.ToArray();

                list.Sort(comp);
                Array.Sort(copy, comp);

                InterfaceTests.TestEnumerableElements(list, copy);
            }
        }

        [Test]
        public void Sort4()
        {
            BigList<int> list = new BigList<int>(new int[] { });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { });

            list = new BigList<int>(new int[] { 3 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 3 });

            list = new BigList<int>(new int[] { 1, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2 });

            list = new BigList<int>(new int[] { 2, 1 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2 });

            list = new BigList<int>(new int[] { 2, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 2, 2 });

            list = new BigList<int>(new int[] { 1, 2, 3 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new BigList<int>(new int[] { 2, 1, 3 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new BigList<int>(new int[] { 1, 3, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new BigList<int>(new int[] { 3, 1, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new BigList<int>(new int[] { 3, 2, 1 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new BigList<int>(new int[] { 2, 3, 1 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 3 });

            list = new BigList<int>(new int[] { 1, 2, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 2 });

            list = new BigList<int>(new int[] { 2, 1, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 2 });

            list = new BigList<int>(new int[] { 2, 2, 1 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 2, 2 });

            list = new BigList<int>(new int[] { 1, 1, 2 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 1, 2 });

            list = new BigList<int>(new int[] { 2, 1, 1 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 1, 2 });

            list = new BigList<int>(new int[] { 1, 2, 1 });
            list.Sort();
            InterfaceTests.TestEnumerableElements(list, new int[] { 1, 1, 2 });
        }



        [Test]
        public void TooLarge()
        {
            BigList<int> listMaxSize = new BigList<int>(new int[] { 6 }, int.MaxValue - 1);
            Assert.AreEqual(int.MaxValue - 1, listMaxSize.Count);

            try {
                listMaxSize.Add(3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.AddToFront(3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.Insert(123456, 3);
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            listMaxSize = new BigList<int>(new int[] { 6 }, int.MaxValue - 16);
            Assert.AreEqual(int.MaxValue - 16, listMaxSize.Count);

            try {
                listMaxSize.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.AddRangeToFront(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.AddRange(new BigList<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }));
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.AddRangeToFront(new BigList<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }));
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.InsertRange(12345, new BigList<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }));
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize.InsertRange(123456, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }

            try {
                listMaxSize = listMaxSize + listMaxSize;
                Assert.Fail("should throw");
            }
            catch (Exception e) {
                Assert.IsTrue(e is InvalidOperationException);
            }
        }

        /// <summary>
        /// This test does a bunch of random operations on a set of tree-lists,
        /// and a parallel set of lists, and check that the results never differ. This is 
        /// primarily useful for flushing out subtle bugs in the setting of the shared
        /// bit.
        /// </summary>
        [Test]
        public void RandomTwiddle()
        {
            const int NUMLISTS = 8;
            const int ITER = 20000;

            Random rand = new Random(13);
            BigList<int>[] biglists = new BigList<int>[NUMLISTS];
            List<int>[] lists = new List<int>[NUMLISTS];
            for (int i = 0; i < NUMLISTS; ++i) {
                biglists[i] = new BigList<int>();
                lists[i] = new List<int>();
            }

            int whichlist1, whichlist2, whichlist3;
            int index, count, value;
            int[] array;

            for (int iter = 0; iter < ITER; ++iter) {
                // Uncomment the below and update the iteration counts to debug a problem.
               /*  if (iter >= 100) {
                    Console.WriteLine("------------- ITERATION {0} ----------------", iter);
                    for (int i = 0; i < NUMLISTS; ++i) {
                        Console.Write("List {0}: ", i);
                        biglists[i].Print();
                    }
                } 

                if (iter == 113)
                    Console.WriteLine("bad stuff about to happen");   */


                switch (rand.Next(22)) {
                case 0:
                case 1:
                case 2:
                case 3:
                    // Change one element.
                    whichlist1 = rand.Next(NUMLISTS);
                    if (biglists[whichlist1].Count != 0) {
                        index = rand.Next(biglists[whichlist1].Count);
                        value = rand.Next(1000);
                        biglists[whichlist1][index] = value;
                        lists[whichlist1][index] = value;
                    }
                    break;

                case 4:
                    // Add two lists and put into the third.
                    whichlist1 = rand.Next(NUMLISTS);
                    whichlist2 = rand.Next(NUMLISTS);
                    whichlist3 = rand.Next(NUMLISTS);
                    biglists[whichlist1] = biglists[whichlist2] + biglists[whichlist3];
                    List<int> temp = new List<int>(lists[whichlist2]);
                    temp.AddRange(lists[whichlist3]);
                    lists[whichlist1] = temp;
                    break;

                case 5:
                    // Append one list to another.
                    whichlist1 = rand.Next(NUMLISTS);
                    whichlist2 = rand.Next(NUMLISTS);
                    biglists[whichlist1].AddRange(biglists[whichlist2]);
                    lists[whichlist1].AddRange(lists[whichlist2]);
                    break;

                case 6:
                    // Preepend one list to another.
                    whichlist1 = rand.Next(NUMLISTS);
                    whichlist2 = rand.Next(NUMLISTS);
                    biglists[whichlist1].AddRangeToFront(biglists[whichlist2]);
                    lists[whichlist1].InsertRange(0, lists[whichlist2]);
                    break;

                case 7:
                    // Append an enumerable.
                    whichlist1 = rand.Next(NUMLISTS);
                    array = new int[rand.Next(20)];
                    for (int i = 0; i < array.Length; ++i)
                        array[i] = rand.Next(1000);
                    biglists[whichlist1].AddRange(array);
                    lists[whichlist1].AddRange(array);
                    break;

                case 8:
                    // Prepend an enumerable.
                    whichlist1 = rand.Next(NUMLISTS);
                    array = new int[rand.Next(20)];
                    for (int i = 0; i < array.Length; ++i)
                        array[i] = rand.Next(1000);
                    biglists[whichlist1].AddRangeToFront(array);
                    lists[whichlist1].InsertRange(0, array);
                    break;

                case 9:
                case 10:
                    // Add one element
                    whichlist1 = rand.Next(NUMLISTS);
                    value = rand.Next(1000);
                    biglists[whichlist1].Add(value);
                    lists[whichlist1].Add(value);
                    break;

                case 11:
                case 12:
                    // Prepend one element
                    whichlist1 = rand.Next(NUMLISTS);
                    value = rand.Next(1000);
                    biglists[whichlist1].AddToFront(value);
                    lists[whichlist1].Insert(0, value);
                    break;

                case 13:
                case 14:
                    // Insert one element
                    whichlist1 = rand.Next(NUMLISTS);
                    value = rand.Next(1000);
                    index = rand.Next(lists[whichlist1].Count + 1);
                    biglists[whichlist1].Insert(index, value);
                    lists[whichlist1].Insert(index, value);
                    break;

                case 15: 
                    // Insert one list inside another
                    whichlist1 = rand.Next(NUMLISTS);
                    whichlist2 = rand.Next(NUMLISTS);
                    index = rand.Next(lists[whichlist1].Count + 1);
                    biglists[whichlist1].InsertRange(index, biglists[whichlist2]);
                    lists[whichlist1].InsertRange(index, lists[whichlist2]);
                    break;

                case 16:
                    // Clone.
                    whichlist1 = rand.Next(NUMLISTS);
                    whichlist2 = rand.Next(NUMLISTS);
                    biglists[whichlist1] = biglists[whichlist2].Clone();
                    lists[whichlist1] = lists[whichlist2].GetRange(0, lists[whichlist2].Count);
                    break;

                case 17:
                case 18:
                    // GetRange.
                    whichlist1 = rand.Next(NUMLISTS);
                    whichlist2 = rand.Next(NUMLISTS);
                    index = rand.Next(biglists[whichlist2].Count);
                    count = rand.Next(biglists[whichlist2].Count - index);
                    biglists[whichlist1] = biglists[whichlist2].GetRange(index, count);
                    lists[whichlist1] = lists[whichlist2].GetRange(index, count);
                    break;

                case 19:
                case 20:
                    // RemoveRange.
                    whichlist1 = rand.Next(NUMLISTS);
                    if (biglists[whichlist1].Count > 0) {
                        index = rand.Next(biglists[whichlist1].Count);
                        count = rand.Next(biglists[whichlist1].Count - index);
                        biglists[whichlist1].RemoveRange(index, count);
                        lists[whichlist1].RemoveRange(index, count);
                    }
                    break;

                case 21:
                    // RemoveItem
                    whichlist1 = rand.Next(NUMLISTS);
                    if (biglists[whichlist1].Count > 0) {
                        index = rand.Next(biglists[whichlist1].Count);
                        biglists[whichlist1].RemoveRange(index, 1);
                        lists[whichlist1].RemoveRange(index, 1);
                    }
                    break;
            }

            /* if (iter >= 113) {
                Console.WriteLine("------------- ITERATION {0} ----------------", iter);
                for (int i = 0; i < NUMLISTS; ++i) {
                    Console.Write("List {0}: ", i);
                    biglists[i].Print();
                }
            } */

            // Now validate all the lists.
            for (int i = 0; i < NUMLISTS; ++i) {
                CheckListAndBigList(lists[i], biglists[i], rand);
#if DEBUG
                biglists[i].Validate();
#endif //DEBUG
                if (lists[i].Count > 1000) {
                    lists[i].Clear();
                    biglists[i].Clear();
                }
            }
        }
    }

    [Test]
    public void SerializeStrings()
    {
        BigList<string> d = new BigList<string>();

        d.AddToFront("foo");
        d.Add("world");
        d.AddToFront("hello");
        d.Add("elvis");
        d.AddToFront("elvis");
        d.Add(null);
        d.AddToFront("cool");
        d.AddRange(new string[] { "1", "2", "3", "4", "5", "6" });
        d.AddRange(new string[] { "7", "8", "9", "10", "11", "12" });

        BigList<string> result = (BigList<string>)InterfaceTests.SerializeRoundTrip(d);

        InterfaceTests.TestReadWriteListGeneric<string>((IList<string>)result, new string[] { "cool", "elvis", "hello", "foo", "world", "elvis", null, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });

    }

    [Serializable]
    class UniqueStuff
    {
        public InterfaceTests.Unique[] objects;
        public BigList<InterfaceTests.Unique> list;
    }


    [Test]
    public void SerializeUnique1()
    {
        UniqueStuff d = new UniqueStuff(), result = new UniqueStuff();

        d.objects = new InterfaceTests.Unique[] { 
                new InterfaceTests.Unique("1"), new InterfaceTests.Unique("2"), new InterfaceTests.Unique("3"), new InterfaceTests.Unique("4"), new InterfaceTests.Unique("5"), new InterfaceTests.Unique("6"), 
                new InterfaceTests.Unique("cool"), new InterfaceTests.Unique("elvis"), new InterfaceTests.Unique("hello"), new InterfaceTests.Unique("foo"), new InterfaceTests.Unique("world"), new InterfaceTests.Unique("elvis"), new InterfaceTests.Unique(null), null,
                new InterfaceTests.Unique("7"), new InterfaceTests.Unique("8"), new InterfaceTests.Unique("9"), new InterfaceTests.Unique("10"), new InterfaceTests.Unique("11"), new InterfaceTests.Unique("12") };
        d.list = new BigList<InterfaceTests.Unique>();

        d.list.AddToFront(d.objects[9]);
        d.list.Add(d.objects[10]);
        d.list.AddToFront(d.objects[8]);
        d.list.Add(d.objects[11]);
        d.list.AddToFront(d.objects[7]);
        d.list.Add(d.objects[12]);
        d.list.AddToFront(d.objects[6]);
        d.list.Add(d.objects[13]);
        d.list.InsertRange(0, new InterfaceTests.Unique[] { d.objects[0], d.objects[1], d.objects[2], d.objects[3], d.objects[4], d.objects[5] });
        d.list.AddRange(new InterfaceTests.Unique[] { d.objects[14], d.objects[15], d.objects[16], d.objects[17], d.objects[18], d.objects[19] });

        result = (UniqueStuff)InterfaceTests.SerializeRoundTrip(d);

        InterfaceTests.TestReadWriteListGeneric(result.list, result.objects);

        for (int i = 0; i < result.objects.Length; ++i) {
            if (result.objects[i] != null)
                Assert.IsFalse(object.Equals(result.objects[i], d.objects[i]));
        }
    }

        [Test]
        public void SerializeUnique2()
        {
            const int LEN = 1387;
            UniqueStuff d = new UniqueStuff(), result = new UniqueStuff();

            d.objects = new InterfaceTests.Unique[LEN];
            for (int i = 0; i < LEN; ++i) 
                d.objects[i] = new InterfaceTests.Unique(i.ToString());
            d.list = new BigList<InterfaceTests.Unique>();
            for (int i = 0; i < LEN; ++i)
                d.list.Add(d.objects[i]);

            result = (UniqueStuff)InterfaceTests.SerializeRoundTrip(d);

            InterfaceTests.TestReadWriteListGeneric(result.list, result.objects);

            for (int i = 0; i < result.objects.Length; ++i) {
                if (result.objects[i] != null)
                    Assert.IsFalse(object.Equals(result.objects[i], d.objects[i]));
            }
        }

}
}

