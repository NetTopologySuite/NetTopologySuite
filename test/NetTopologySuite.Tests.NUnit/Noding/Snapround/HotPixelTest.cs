using NetTopologySuite.Geometries;
using NetTopologySuite.Noding.Snapround;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding.Snaparound
{
    public class HotPixelTest
    {
        [Test]
        public void TestBelow()
        {
            CheckIntersects(false, 1, 1, 100,
                1, 0.98, 3, 0.5);
        }

        [Test]
        public void TestAbove()
        {
            CheckIntersects(false, 1, 1, 100,
                1, 1.011, 3, 1.5);
        }

        [Test]
        public void TestRightSideVerticalTouchAbove()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                1.25, 1.25, 1.25, 2);
        }

        [Test]
        public void TestRightSideVerticalTouchBelow()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                1.25, 0, 1.25, 1.15);
        }

        [Test]
        public void TestRightSideVerticalOverlap()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                1.25, 0, 1.25, 1.5);
        }

        //-----------------------------

        [Test]
        public void TestTopSideHorizontalTouchRight()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                1.25, 1.25, 2, 1.25);
        }

        [Test]
        public void TestTopSideHorizontalTouchLeft()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                0, 1.25, 1.15, 1.25);
        }

        [Test]
        public void TestTopSideHorizontalOverlap()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                0, 1.25, 1.9, 1.25);
        }

        //-----------------------------

        [Test]
        public void TestLeftSideVerticalTouchAbove()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                1.15, 1.25, 1.15, 2);
        }

        [Test]
        public void TestLeftSideVerticalOverlap()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                1.15, 0, 1.15, 1.8);
        }

        [Test]
        public void TestLeftSideVerticalTouchBelow()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                1.15, 0, 1.15, 1.15);
        }

        [Test]
        public void TestLeftSideCrossRight()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0, 1.19, 2, 1.21);
        }

        [Test]
        public void TestLeftSideCrossTop()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0.8, 0.8, 1.3, 1.39);
        }

        [Test]
        public void TestLeftSideCrossBottom()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                1, 1.5, 1.3, 0.9);
        }

        //-----------------------------

        [Test]
        public void TestBottomSideHorizontalTouchRight()
        {
            CheckIntersects(false, 1.2, 1.2, 10,
                1.25, 1.15, 2, 1.15);
        }

        [Test]
        public void TestBottomSideHorizontalTouchLeft()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0, 1.15, 1.15, 1.15);
        }

        [Test]
        public void TestBottomSideHorizontalOverlapLeft()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0, 1.15, 1.2, 1.15);
        }

        [Test]
        public void TestBottomSideHorizontalOverlap()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0, 1.15, 1.9, 1.15);
        }

        [Test]
        public void TestBottomSideHorizontalOverlapRight()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                1.2, 1.15, 1.4, 1.15);
        }

        [Test]
        public void TestBottomSideCrossRight()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                1.1, 1, 1.4, 1.4);
        }

        [Test]
        public void TestBottomSideCrossTop()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                1.1, 0.9, 1.3, 1.6);
        }

        //-----------------------------

        [Test]
        public void TestDiagonalDown()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0.9, 1.5, 1.4, 1);
        }

        [Test]
        public void TestDiagonalUp()
        {
            CheckIntersects(true, 1.2, 1.2, 10,
                0.9, 0.9, 1.5, 1.5);
        }

        //-----------------------------
        // Test segments entering through a corner and terminating inside pixel

        [Test]
        public void TestCornerULEndInside()
        {
            CheckIntersects(true, 1, 1, 10,
                0.7, 1.3, 0.98, 1.02);
        }

        [Test]
        public void TestCornerLLEndInside()
        {
            CheckIntersects(true, 1, 1, 10,
                0.8, 0.8, 0.98, 0.98);
        }

        [Test]
        public void TestCornerURStartInside()
        {
            CheckIntersects(true, 1, 1, 10,
                1.02, 1.02, 1.3, 1.3);
        }

        [Test]
        public void TestCornerLRStartInside()
        {
            CheckIntersects(true, 1, 1, 10,
                1.02, 0.98, 1.3, 0.7);
        }

        //-----------------------------
        // Test segments tangent to a corner

        [Test]
        public void TestCornerLLTangent()
        {
            CheckIntersects(true, 1, 1, 10,
                0.9, 1, 1, 0.9);
        }

        [Test]
        public void TestCornerLLTangentNoTouch()
        {
            CheckIntersects(false, 1, 1, 10,
                0.9, 0.9, 1, 0.9);
        }

        [Test]
        public void TestCornerULTangent()
        {
            // does not intersect due to open top
            CheckIntersects(false, 1, 1, 10,
                0.9, 1, 1, 1.1);
        }

        [Test]
        public void TestCornerURTangent()
        {
            // does not intersect due to open top
            CheckIntersects(false, 1, 1, 10,
                1, 1.1, 1.1, 1);
        }

        [Test]
        public void TestCornerLRTangent()
        {
            // does not intersect due to open right side
            CheckIntersects(false, 1, 1, 10,
                1, 0.9, 1.1, 1);
        }

        [Test]
        public void TestCornerULTouchEnd()
        {
            // does not intersect due to bounding box check for open top
            CheckIntersects(false, 1, 1, 10,
                0.9, 1.1, 0.95, 1.05);
        }


//================================================

        private void CheckIntersects(bool expected,
            double x, double y, double scale,
            double x1, double y1, double x2, double y2)
        {
            var hp = new HotPixel(new Coordinate(x, y), scale);
            var p1 = new Coordinate(x1, y1);
            var p2 = new Coordinate(x2, y2);
            bool actual = hp.Intersects(p1, p2);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }

}
