using System;
using NetTopologySuite.Algorithm.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Distance
{
    public class MatrixStorageImplTests
    {

        [Test]
        public void TestCsrMatrix()
        {
            var mat = new DiscreteFrechetDistance.CsrMatrix(4, 6, 0d, 8);
            RunOrderedTest(mat);
            mat = new DiscreteFrechetDistance.CsrMatrix(4, 6, 0d, 8);
            RunUnorderedTest(mat);
        }

        [Test]
        public void TestHashMapMatrix()
        {
            var mat = new DiscreteFrechetDistance.HashMapMatrix(4, 6, 0d);
            RunOrderedTest(mat);
            mat = new DiscreteFrechetDistance.HashMapMatrix(4, 6, 0d);
            RunUnorderedTest(mat);
        }
        [Test]
        public void TestRectMatrix()
        {
            var mat = new DiscreteFrechetDistance.RectMatrix(4, 6, 0d);
            RunOrderedTest(mat);
            mat = new DiscreteFrechetDistance.RectMatrix(4, 6, 0d);
            RunUnorderedTest(mat);
        }

        private static void RunOrderedTest(DiscreteFrechetDistance.MatrixStorage mat)
        {
            mat[0, 0] = 10;
            mat[0, 1] = 20;
            mat[1, 1] = 30;
            mat[1, 3] = 40;
            mat[2, 2] = 50;
            mat[2, 3] = 60;
            mat[2, 4] = 70;
            mat[3, 5] = 80;

            Assert.AreEqual(10d, mat[0, 0], "{0} -> {1} = {2} /= {3}", 0, 0, 10d, mat[0, 0]);
            Assert.AreEqual(20d, mat[0, 1], "{0} -> {1} = {2} /= {3}", 0, 1, 20d, mat[0, 1]);
            Assert.AreEqual(30d, mat[1, 1], "{0} -> {1} = {2} /= {3}", 1, 1, 30d, mat[1, 1]);
            Assert.AreEqual(40d, mat[1, 3], "{0} -> {1} = {2} /= {3}", 1, 3, 40d, mat[1, 3]);
            Assert.AreEqual(50d, mat[2, 2], "{0} -> {1} = {2} /= {3}", 2, 2, 50d, mat[2, 2]);
            Assert.AreEqual(60d, mat[2, 3], "{0} -> {1} = {2} /= {3}", 2, 3, 60d, mat[2, 3]);
            Assert.AreEqual(70d, mat[2, 4], "{0} -> {1} = {2} /= {3}", 2, 4, 70d, mat[2, 4]);
            Assert.AreEqual(80d, mat[3, 5], "{0} -> {1} = {2} /= {3}", 3, 5, 80d, mat[3, 5]);
        }

        private static void RunUnorderedTest(DiscreteFrechetDistance.MatrixStorage mat)
        {
            mat[0, 0] = 10;
            mat[3, 5] = 80;
            mat[0, 1] = 20;
            mat[2, 4] = 70;
            mat[1, 1] = 30;
            mat[2, 3] = 60;
            mat[2, 2] = 50;
            mat[1, 3] = 40;

            Assert.AreEqual(10d, mat[0, 0], "{0} -> {1} = {2} /= {3}", 0, 0, 10d, mat[0, 0]);
            Assert.AreEqual(20d, mat[0, 1], "{0} -> {1} = {2} /= {3}", 0, 1, 20d, mat[0, 1]);
            Assert.AreEqual(30d, mat[1, 1], "{0} -> {1} = {2} /= {3}", 1, 1, 30d, mat[1, 1]);
            Assert.AreEqual(40d, mat[1, 3], "{0} -> {1} = {2} /= {3}", 1, 3, 40d, mat[1, 3]);
            Assert.AreEqual(50d, mat[2, 2], "{0} -> {1} = {2} /= {3}", 2, 2, 50d, mat[2, 2]);
            Assert.AreEqual(60d, mat[2, 3], "{0} -> {1} = {2} /= {3}", 2, 3, 60d, mat[2, 3]);
            Assert.AreEqual(70d, mat[2, 4], "{0} -> {1} = {2} /= {3}", 2, 4, 70d, mat[2, 4]);
            Assert.AreEqual(80d, mat[3, 5], "{0} -> {1} = {2} /= {3}", 3, 5, 80d, mat[3, 5]);
        }
    }
}
