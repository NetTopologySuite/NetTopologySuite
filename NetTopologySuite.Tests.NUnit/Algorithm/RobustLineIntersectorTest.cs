using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class RobustLineIntersectorTest
    {
        RobustLineIntersector i = new RobustLineIntersector();

        [TestAttribute]
        public void Test2Lines() {
            RobustLineIntersector i = new RobustLineIntersector();
            Coordinate p1 = new Coordinate(10, 10);
            Coordinate p2 = new Coordinate(20, 20);
            Coordinate q1 = new Coordinate(20, 10);
            Coordinate q2 = new Coordinate(10, 20);
            Coordinate x = new Coordinate(15, 15);
            i.ComputeIntersection(p1, p2, q1, q2);
            Assert.AreEqual(RobustLineIntersector.DoIntersect, i.IntersectionNum);
            Assert.AreEqual(1, i.IntersectionNum);
            Assert.AreEqual(x, i.GetIntersection(0));
            Assert.IsTrue(i.IsProper);
            Assert.IsTrue(i.HasIntersection);
        }

        [TestAttribute]
        public void TestCollinear1() {
            RobustLineIntersector i = new RobustLineIntersector();
            Coordinate p1 = new Coordinate(10, 10);
            Coordinate p2 = new Coordinate(20, 10);
            Coordinate q1 = new Coordinate(22, 10);
            Coordinate q2 = new Coordinate(30, 10);
            i.ComputeIntersection(p1, p2, q1, q2);
            Assert.AreEqual(RobustLineIntersector.DontIntersect, i.IntersectionNum);
            Assert.IsTrue(!i.IsProper);
            Assert.IsTrue(!i.HasIntersection);
        }

        [TestAttribute]
        public void TestCollinear2() {
            RobustLineIntersector i = new RobustLineIntersector();
            Coordinate p1 = new Coordinate(10, 10);
            Coordinate p2 = new Coordinate(20, 10);
            Coordinate q1 = new Coordinate(20, 10);
            Coordinate q2 = new Coordinate(30, 10);
            i.ComputeIntersection(p1, p2, q1, q2);
            Assert.AreEqual(RobustLineIntersector.DoIntersect, i.IntersectionNum);
            Assert.IsTrue(!i.IsProper);
            Assert.IsTrue(i.HasIntersection);
        }

        [TestAttribute]
        public void TestCollinear3() {
            RobustLineIntersector i = new RobustLineIntersector();
            Coordinate p1 = new Coordinate(10, 10);
            Coordinate p2 = new Coordinate(20, 10);
            Coordinate q1 = new Coordinate(15, 10);
            Coordinate q2 = new Coordinate(30, 10);
            i.ComputeIntersection(p1, p2, q1, q2);
            Assert.AreEqual(RobustLineIntersector.Collinear, i.IntersectionNum);
            Assert.IsTrue(!i.IsProper);
            Assert.IsTrue(i.HasIntersection);
        }

        [TestAttribute]
        public void TestCollinear4() {
            RobustLineIntersector i = new RobustLineIntersector();
            Coordinate p1 = new Coordinate(30, 10);
            Coordinate p2 = new Coordinate(20, 10);
            Coordinate q1 = new Coordinate(10, 10);
            Coordinate q2 = new Coordinate(30, 10);
            i.ComputeIntersection(p1, p2, q1, q2);
            Assert.AreEqual(RobustLineIntersector.Collinear, i.IntersectionNum);
            Assert.IsTrue(i.HasIntersection);
        }

        [TestAttribute]
        public void TestEndpointIntersection() {
            i.ComputeIntersection(new Coordinate(100, 100), new Coordinate(10, 100),
            new Coordinate(100, 10), new Coordinate(100, 100));
            Assert.IsTrue(i.HasIntersection);
            Assert.AreEqual(1, i.IntersectionNum);
        }

        [TestAttribute]
        public void TestEndpointIntersection2() {
            i.ComputeIntersection(new Coordinate(190, 50), new Coordinate(120, 100),
            new Coordinate(120, 100), new Coordinate(50, 150));
            Assert.IsTrue(i.HasIntersection);
            Assert.AreEqual(1, i.IntersectionNum);
            Assert.AreEqual(new Coordinate(120, 100), i.GetIntersection(1));
        }

        [TestAttribute]
        public void TestOverlap() {
            i.ComputeIntersection(new Coordinate(180, 200), new Coordinate(160, 180),
            new Coordinate(220, 240), new Coordinate(140, 160));
            Assert.IsTrue(i.HasIntersection);
            Assert.AreEqual(2, i.IntersectionNum);
        }

        [TestAttribute]
        public void TestIsProper1() {
            i.ComputeIntersection(new Coordinate(30, 10), new Coordinate(30, 30),
            new Coordinate(10, 10), new Coordinate(90, 11));
            Assert.IsTrue(i.HasIntersection);
            Assert.AreEqual(1, i.IntersectionNum);
            Assert.IsTrue(i.IsProper);
        }

        [TestAttribute]
        public void TestIsProper2() {
            i.ComputeIntersection(new Coordinate(10, 30), new Coordinate(10, 0),
            new Coordinate(11, 90), new Coordinate(10, 10));
            Assert.IsTrue(i.HasIntersection);
            Assert.AreEqual(1, i.IntersectionNum);
            Assert.IsTrue(!i.IsProper);
        }

        [TestAttribute]
        public void TestIsCCW() {
            Assert.AreEqual(1, CGAlgorithms.ComputeOrientation(
            new Coordinate(-123456789, -40),
            new Coordinate(0, 0),
            new Coordinate(381039468754763d, 123456789)));
        }

        [TestAttribute]
        public void TestIsCCW2() {
            Assert.AreEqual(0, CGAlgorithms.ComputeOrientation(
            new Coordinate(10, 10),
            new Coordinate(20, 20),
            new Coordinate(0, 0)));
            Assert.AreEqual(0, NonRobustCGAlgorithms.ComputeOrientation(
            new Coordinate(10, 10),
            new Coordinate(20, 20),
            new Coordinate(0, 0)));
        }

        [TestAttribute]
        public void TestA() {
            Coordinate p1 = new Coordinate(-123456789, -40);
            Coordinate p2 = new Coordinate(381039468754763d, 123456789);
            Coordinate q  = new Coordinate(0, 0);
            ILineString l = new GeometryFactory().CreateLineString(new Coordinate[] {p1, p2});
            IPoint p = new GeometryFactory().CreatePoint(q);
            Assert.AreEqual(false, l.Intersects(p));
            Assert.AreEqual(false, CGAlgorithms.IsOnLine(q, new Coordinate[] { p1, p2 }));
            Assert.AreEqual(-1, CGAlgorithms.ComputeOrientation(p1, p2, q));
        }

    }
}