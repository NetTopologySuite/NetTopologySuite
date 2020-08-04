using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class LineSegmentFunctions
    {
        public static bool SegmentIntersects(Geometry g1, Geometry g2)
        {
            var pt1 = g1.Coordinates;
            var pt2 = g2.Coordinates;
            var ri = new RobustLineIntersector();
            ri.ComputeIntersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            return ri.HasIntersection;
        }

        public static Geometry SegmentIntersection(Geometry g1, Geometry g2)
        {
            var pt1 = g1.Coordinates;
            var pt2 = g2.Coordinates;
            var ri = new RobustLineIntersector();
            ri.ComputeIntersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            switch (ri.IntersectionNum)
            {
                case 0:
                    // no intersection => return empty point
                    return g1.Factory.CreatePoint((Coordinate)null);
                case 1:
                    // return point
                    return g1.Factory.CreatePoint(ri.GetIntersection(0));
                case 2:
                    // return line
                    return g1.Factory.CreateLineString(new [] {ri.GetIntersection(0), ri.GetIntersection(1)});
            }
            return null;
        }

        public static Geometry SegmentIntersectionDd(Geometry g1, Geometry g2)
        {
            var pt1 = g1.Coordinates;
            var pt2 = g2.Coordinates;

            // first check if there actually is an intersection
            var ri = new RobustLineIntersector();
            ri.ComputeIntersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            if (!ri.HasIntersection)
            {
                // no intersection => return empty point
                return g1.Factory.CreatePoint((Coordinate)null);
            }

            var intPt = CGAlgorithmsDD.Intersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            return g1.Factory.CreatePoint(intPt);
        }

        public static Geometry LineIntersection(Geometry g1, Geometry g2)
        {
            var pt1 = g1.Coordinates;
            var pt2 = g2.Coordinates;

            var line1 = new LineSegment(pt1[0], pt1[1]);
            var line2 = new LineSegment(pt2[0], pt2[1]);

            var intPt = line1.LineIntersection(line2);
            return g1.Factory.CreatePoint(intPt);
        }

        public static Geometry LineIntersectionDD(Geometry g1, Geometry g2)
        {
            var pt1 = g1.Coordinates;
            var pt2 = g2.Coordinates;

            var intPt = CGAlgorithmsDD.Intersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            // handle parallel case
            if (double.IsNaN(intPt.X))
            {
                intPt = null;
            }
            return g1.Factory.CreatePoint(intPt);
        }
        public static Geometry ReflectPoint(Geometry g1, Geometry g2)
        {
            var line = g1.Coordinates;
            var pt = g2.Coordinate;

            var seg = new LineSegment(line[0], line[1]);
            var reflectPt = seg.Reflect(pt);

            return g1.Factory.CreatePoint(reflectPt);
        }
    }
}
