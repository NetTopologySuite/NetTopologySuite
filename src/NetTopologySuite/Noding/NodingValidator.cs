using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Validates that a collection of <see cref="ISegmentString" />s is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class NodingValidator
    {
        private static readonly GeometryFactory Factory = new GeometryFactory();

        private readonly LineIntersector _li = new RobustLineIntersector();
        private readonly IList<ISegmentString> _segStrings;

        /// <summary>
        /// Creates a new validator for the given collection
        /// of <see cref="ISegmentString"/>s.
        /// </summary>
        /// <param name="segStrings">The seg strings.</param>
        public NodingValidator(IList<ISegmentString> segStrings)
        {
            _segStrings = segStrings;
        }

        /// <summary>
        /// Checks whether the supplied segment strings
        /// are correctly noded.  Throws an exception if they are not.
        /// </summary>
        public void CheckValid()
        {
            CheckEndPtVertexIntersections();
            CheckInteriorIntersections();
            CheckCollapses();
        }

        /// <summary>
        /// Checks if a segment string contains a segment pattern a-b-a (which implies a self-intersection).
        /// </summary>
        private void CheckCollapses()
        {
            foreach (var ss in _segStrings)
                CheckCollapses(ss);
        }

        private static void CheckCollapses(ISegmentString ss)
        {
            var pts = ss.Coordinates;
            for (int i = 0; i < pts.Length - 2; i++)
                CheckCollapse(pts[i], pts[i + 1], pts[i + 2]);
        }

        private static void CheckCollapse(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            if (p0.Equals(p2))
                throw new ApplicationException(string.Format(
                    "found non-noded collapse at: {0}", Factory.CreateLineString(new [] { p0, p1, p2 })));
        }

        /// <summary>
        /// Checks all pairs of segments for intersections at an interior point of a segment.
        /// </summary>
        private void CheckInteriorIntersections()
        {
            foreach (var ss0 in _segStrings)
                foreach (var ss1 in _segStrings)
                    CheckInteriorIntersections(ss0, ss1);
        }

        private void CheckInteriorIntersections(ISegmentString ss0, ISegmentString ss1)
        {
            var pts0 = ss0.Coordinates;
            var pts1 = ss1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                    CheckInteriorIntersections(ss0, i0, ss1, i1);
        }

        private void CheckInteriorIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            if (e0 == e1 && segIndex0 == segIndex1)
                return;

            var coordinates = e0.Coordinates;
            var p00 = coordinates[segIndex0];
            var p01 = coordinates[segIndex0 + 1];
            coordinates = e1.Coordinates;
            var p10 = coordinates[segIndex1];
            var p11 = coordinates[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);
            if (_li.HasIntersection)
                if (_li.IsProper || HasInteriorIntersection(_li, p00, p01) || HasInteriorIntersection(_li, p10, p11))
                    throw new ApplicationException(string.Format(
                        "found non-noded intersection at {0}-{1} and {2}-{3}", p00, p01, p10, p11));
        }

        private static bool HasInteriorIntersection(LineIntersector li, Coordinate p0, Coordinate p1)
        {
            for (int i = 0; i < li.IntersectionNum; i++)
            {
                var intPt = li.GetIntersection(i);
                if (!(intPt.Equals(p0) ||
                      intPt.Equals(p1)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks for intersections between an endpoint of a segment string
        /// and an interior vertex of another segment string
        /// </summary>
        private void CheckEndPtVertexIntersections()
        {
            foreach(var ss in _segStrings)
            {
                var pts = ss.Coordinates;
                CheckEndPtVertexIntersections(pts[0], _segStrings);
                CheckEndPtVertexIntersections(pts[pts.Length - 1], _segStrings);
            }
        }

        private static void CheckEndPtVertexIntersections(Coordinate testPt, IEnumerable<ISegmentString> segStrings)
        {
            foreach (var ss in segStrings)
            {
                var pts = ss.Coordinates;
                for (int j = 1; j < pts.Length - 1; j++)
                    if (pts[j].Equals(testPt))
                        throw new ApplicationException(string.Format(
                            "found endpt/interior pt intersection at index {0} :pt {1}", j, testPt));
            }
        }
    }
}
