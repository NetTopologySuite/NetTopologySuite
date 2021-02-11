using System;
using NUnit.Framework;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue269
    {
        [Test, Description("GitHub issue #269")]
        public void Test2Lines()
        {
            var p1 = new Coordinate(10, 10);
            var p2 = new Coordinate(20, 20);
            var q1 = new Coordinate(20, 10);
            var q2 = new Coordinate(10, 20);

            // Create intersector
            var i = new Algorithm.RobustLineIntersector();

            // Note: remove when ComputeIntersect is protected.
            // Fails because RobustLineLineIntersector is not properly initialized when calling
            // this function immediately
            Assert.That(() => i.ComputeIntersect(p1, p2, q1, q2), Throws.InstanceOf<NullReferenceException>());

            // Proper way
            i.ComputeIntersection(p1, p2, q1, q2);
            Assert.That(i.HasIntersection, Is.True);
            Assert.That(i.IsProper, Is.True);

            // Note: remove when ComputeIntersect is protected.
            // Now ComputeIntersect passes.
            Assert.That(i.ComputeIntersect(p1, p2, q1, q2), Is.EqualTo(Algorithm.LineIntersector.PointIntersection));
        }
    }
}
