using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace Open.Topology.TestRunner.Functions
{
    public static class CGAlgorithmFunctions
    {
        public static int OrientationIndex(IGeometry segment, IGeometry ptGeom)
        {
            if (segment.NumPoints != 2 || ptGeom.NumPoints != 1)
            {
                throw new ArgumentException("A must have two points and B must have one");
            }
            Coordinate[] segPt = segment.Coordinates;

            Coordinate p = ptGeom.Coordinate;
            int index = CGAlgorithms.OrientationIndex(segPt[0], segPt[1], p);
            return index;
        }

        public static int OrientationIndexDd(IGeometry segment, IGeometry ptGeom)
        {
            if (segment.NumPoints != 2 || ptGeom.NumPoints != 1)
            {
                throw new ArgumentException("A must have two points and B must have one");
            }
            Coordinate[] segPt = segment.Coordinates;

            Coordinate p = ptGeom.Coordinate;
            int index = CGAlgorithmsDD.OrientationIndex(segPt[0], segPt[1], p);
            return index;
        }

        public static bool SegmentIntersects(IGeometry g1, IGeometry g2)
        {
            Coordinate[] pt1 = g1.Coordinates;
            Coordinate[] pt2 = g2.Coordinates;
            RobustLineIntersector ri = new RobustLineIntersector();
            ri.ComputeIntersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            return ri.HasIntersection;
        }

        public static IGeometry SegmentIntersection(IGeometry g1, IGeometry g2)
        {
            Coordinate[] pt1 = g1.Coordinates;
            Coordinate[] pt2 = g2.Coordinates;
            RobustLineIntersector ri = new RobustLineIntersector();
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
                    return g1.Factory.CreateLineString(
                        new Coordinate[] {
                            ri.GetIntersection(0),
                            ri.GetIntersection(1)
                        });
            }
            return null;
        }

        public static IGeometry SegmentIntersectionDd(IGeometry g1, IGeometry g2)
        {
            Coordinate[] pt1 = g1.Coordinates;
            Coordinate[] pt2 = g2.Coordinates;

            // first check if there actually is an intersection
            RobustLineIntersector ri = new RobustLineIntersector();
            ri.ComputeIntersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            if (!ri.HasIntersection)
            {
                // no intersection => return empty point
                return g1.Factory.CreatePoint((Coordinate)null);
            }

            Coordinate intPt = CGAlgorithmsDD.Intersection(pt1[0], pt1[1], pt2[0], pt2[1]);
            return g1.Factory.CreatePoint(intPt);
        }
    }
}