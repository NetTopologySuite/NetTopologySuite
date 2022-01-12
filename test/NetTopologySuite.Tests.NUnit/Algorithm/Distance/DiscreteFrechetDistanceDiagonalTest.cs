using NetTopologySuite.Algorithm.Distance;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Distance
{
    public class DiscreteFrechetDistanceDiagonalTest
    {
        [Test]
        public void Test1x1()
        {
            CheckDiagonal(1, 1, XY(0, 0));
        }

        [Test]
        public void Test2x2()
        {
            CheckDiagonal(2, 2, XY(0, 0, 1, 1));
        }

        [Test]
        public void Test3x3()
        {
            CheckDiagonal(3, 3, XY(0, 0, 1, 1, 2, 2));
        }

        [Test]
        public void Test3x4()
        {
            CheckDiagonal(3, 4, XY(0, 0, 1, 1, 1, 2, 2, 3));
        }

        [Test]
        public void Test3x5()
        {
            CheckDiagonal(3, 5, XY(0, 0, 0, 1, 1, 2, 1, 3, 2, 4));
        }

        [Test]
        public void Test3x6()
        {
            CheckDiagonal(3, 6, XY(0, 0, 0, 1, 1, 2, 1, 3, 2, 4, 2, 5));
        }

        [Test]
        public void Test6x2()
        {
            CheckDiagonal(6, 2, XY(0, 0, 1, 0, 2, 0, 3, 1, 4, 1, 5, 1));
        }

        [Test]
        public void Test2x6()
        {
            CheckDiagonal(2, 6, XY(0, 0, 0, 1, 0, 2, 1, 3, 1, 4, 1, 5));
        }

        private void CheckDiagonal(int cols, int rows, int[] xyExpected)
        {
            int[] xy = DiscreteFrechetDistance.BresenhamDiagonal(cols, rows);
            Assert.That(xyExpected.Length, Is.EqualTo(xy.Length));
            for (int i = 0; i < xy.Length; i++)
            {
                Assert.That(xyExpected[i], Is.EqualTo(xy[i]));
            }
        }

        private static int[] XY(params int[] ord)
        {
            return ord;
        }
    }
}
