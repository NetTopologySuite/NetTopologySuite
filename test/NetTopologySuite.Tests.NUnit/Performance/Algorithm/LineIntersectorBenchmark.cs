using System;
using System.Diagnostics;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Algorithm;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    public class LineIntersectorBenchmark
    {
        [Test]
        public void Run()
        {
            Exercise(new NonRobustLineIntersector());
            Exercise(new RobustLineIntersector());
        }

        private void Exercise(LineIntersector lineIntersector)
        {
            Console.WriteLine(lineIntersector.GetType().Name);
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                ExerciseOnce(lineIntersector);
            }
            sw.Stop();
            Console.WriteLine($"Milliseconds elapsed: {sw.ElapsedMilliseconds}");
        }

        private void ExerciseOnce(LineIntersector lineIntersector)
        {
            var p1 = new Coordinate(10, 10);
            var p2 = new Coordinate(20, 20);
            var q1 = new Coordinate(20, 10);
            var q2 = new Coordinate(10, 20);
            var x = new Coordinate(15, 15);
            lineIntersector.ComputeIntersection(p1, p2, q1, q2);
            int intersectionNum = lineIntersector.IntersectionNum;
            var intersection = lineIntersector.GetIntersection(0);
            bool isProper = lineIntersector.IsProper;
            bool hasIntersection = lineIntersector.HasIntersection;

            p1 = new Coordinate(10, 10);
            p2 = new Coordinate(20, 10);
            q1 = new Coordinate(22, 10);
            q2 = new Coordinate(30, 10);
            lineIntersector.ComputeIntersection(p1, p2, q1, q2);
            isProper = lineIntersector.IsProper;
            isProper = lineIntersector.HasIntersection;

            p1 = new Coordinate(10, 10);
            p2 = new Coordinate(20, 10);
            q1 = new Coordinate(20, 10);
            q2 = new Coordinate(30, 10);
            lineIntersector.ComputeIntersection(p1, p2, q1, q2);
            isProper = lineIntersector.IsProper;
            isProper = lineIntersector.HasIntersection;

            p1 = new Coordinate(10, 10);
            p2 = new Coordinate(20, 10);
            q1 = new Coordinate(15, 10);
            q2 = new Coordinate(30, 10);
            lineIntersector.ComputeIntersection(p1, p2, q1, q2);
            isProper = lineIntersector.IsProper;
            isProper = lineIntersector.HasIntersection;

            p1 = new Coordinate(30, 10);
            p2 = new Coordinate(20, 10);
            q1 = new Coordinate(10, 10);
            q2 = new Coordinate(30, 10);
            lineIntersector.ComputeIntersection(p1, p2, q1, q2);
            isProper = lineIntersector.HasIntersection;

            lineIntersector.ComputeIntersection(new Coordinate(100, 100), new Coordinate(10, 100),
                new Coordinate(100, 10), new Coordinate(100, 100));
            isProper = lineIntersector.HasIntersection;
            intersectionNum = lineIntersector.IntersectionNum;

            lineIntersector.ComputeIntersection(new Coordinate(190, 50), new Coordinate(120, 100),
                new Coordinate(120, 100), new Coordinate(50, 150));
            isProper = lineIntersector.HasIntersection;
            intersectionNum = lineIntersector.IntersectionNum;
            intersection = lineIntersector.GetIntersection(1);

            lineIntersector.ComputeIntersection(new Coordinate(180, 200), new Coordinate(160, 180),
                new Coordinate(220, 240), new Coordinate(140, 160));
            isProper = lineIntersector.HasIntersection;
            intersectionNum = lineIntersector.IntersectionNum;

            lineIntersector.ComputeIntersection(new Coordinate(30, 10), new Coordinate(30, 30),
                new Coordinate(10, 10), new Coordinate(90, 11));
            isProper = lineIntersector.HasIntersection;
            intersectionNum = lineIntersector.IntersectionNum;
            isProper = lineIntersector.IsProper;

            lineIntersector.ComputeIntersection(new Coordinate(10, 30), new Coordinate(10, 0),
                new Coordinate(11, 90), new Coordinate(10, 10));
            isProper = lineIntersector.HasIntersection;
            intersectionNum = lineIntersector.IntersectionNum;
            isProper = lineIntersector.IsProper;
        }
    }
}
