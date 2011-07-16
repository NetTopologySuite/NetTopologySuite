using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

#if DOTNET35
using sl = System.Linq;
#else
using sl = GeoAPI.DataStructures;
#endif

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Validates that a collection of <see cref="NodedSegmentString{TCoordinate}" />s is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class NodingValidator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li;
        private readonly List<ISegmentString<TCoordinate>> _segStrings = new List<ISegmentString<TCoordinate>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NodingValidator{TCoordinate}"/> 
        /// class.
        /// </summary>
        /// <param name="segStrings">The seg strings.</param>
        public NodingValidator(IGeometryFactory<TCoordinate> geoFactory,
                               IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geoFactory);
            _segStrings.AddRange(segStrings);
        }

        public void CheckValid()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            CheckEndPtVertexIntersections();
            sw.Stop();
            Debug.WriteLine(string.Format("CheckEndPtVertexIntersections:{0}", sw.ElapsedMilliseconds));
            sw.Start();
            CheckInteriorIntersections();
            sw.Stop();
            Debug.WriteLine(string.Format("CheckInteriorIntersections:{0}", sw.ElapsedMilliseconds));
            sw.Start();
            CheckCollapses();
            sw.Stop();
            Debug.WriteLine(string.Format("CheckCollapses:{0}", sw.ElapsedMilliseconds));
        }

        /// <summary>
        /// Checks if a segment string contains a segment pattern a-b-a (which implies a self-intersection).
        /// </summary>   
        private void CheckCollapses()
        {
            foreach (ISegmentString<TCoordinate> ss in _segStrings)
            {
                CheckCollapses(ss);
            }
        }

        private static void CheckCollapses(ISegmentString<TCoordinate> ss)
        {
            IEnumerable<TCoordinate> pts = ss.Coordinates;

            foreach (Triple<TCoordinate> triple in Slice.GetOverlappingTriples(pts))
                CheckCollapse(triple);
        }

        private static void CheckCollapse(Triple<TCoordinate> triple)
        {
            if (triple.First.Equals(triple.Third))
                throw new Exception("found non-noded collapse at: " + triple.First + ", " + triple.Second + " " + triple.Third);

        }
        private static void CheckCollapse(TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            if (p0.Equals(p2))
            {
                throw new Exception("found non-noded collapse at: " + p0 + ", " + p1 + " " + p2);
            }
        }

        /// <summary>
        /// Checks all pairs of segments for intersections at an interior point of a segment.
        /// </summary>
        private void CheckInteriorIntersections()
        {
            foreach (ISegmentString<TCoordinate> ss0 in _segStrings)
                foreach (ISegmentString<TCoordinate> ss1 in _segStrings)
                    CheckInteriorIntersections(ss0, ss1);
        }

        private void CheckInteriorIntersections(ISegmentString<TCoordinate> ss0, ISegmentString<TCoordinate> ss1)
        {
            IEnumerable<TCoordinate> pts0 = ss0.Coordinates;
            IEnumerable<TCoordinate> pts1 = ss1.Coordinates;
            Int32 i0 = 0;
            Int32 i1 = 0;
            foreach (Pair<TCoordinate> p0 in Slice.GetOverlappingPairs(pts0))
            {
                foreach (Pair<TCoordinate> p1 in Slice.GetOverlappingPairs(pts1))
                {

                    if (ss0 == ss1 && i0 == i1 ) continue;
                    CheckInteriorIntersections(p0.First, p0.Second, p1.First, p1.Second);
                    //CheckInteriorIntersections(ss0, p0.First, p0.Second, ss1, p1.First, p1.Second);
                    i1++;
                }
                i0++;
            }
        }

        private void CheckInteriorIntersections(TCoordinate p00, TCoordinate p01,
                                                TCoordinate p10, TCoordinate p11)
        //private void CheckInteriorIntersections(ISegmentString<TCoordinate> e0, TCoordinate p00, TCoordinate p01,
        //                                        ISegmentString<TCoordinate> e1, TCoordinate p10, TCoordinate p11)
        {
            //if (e0 == e1 && p00.Equals(p10) && p01.Equals(p11))
            //{
            //    return;
            //}

            Intersection<TCoordinate> intersection = _li.ComputeIntersection(p00, p01, p10, p11);

            if (intersection.HasIntersection)
            {
                if (intersection.IsProper
                    || HasInteriorIntersection(intersection, p00, p01)
                    || HasInteriorIntersection(intersection, p10, p11))
                {
                    throw new TopologyException("Found non-noded intersection at "
                                                + p00 + "-" + p01 + " and " + p10 + "-" + p11);
                }
            }
        }

        /// <returns><see langword="true"/> if there is an intersection point which is not an endpoint of the segment p0-p1.</returns>
        private static Boolean HasInteriorIntersection(Intersection<TCoordinate> intersection, TCoordinate p0,
                                                       TCoordinate p1)
        {
            for (Int32 i = 0; i < (Int32) intersection.IntersectionDegree; i++)
            {
                TCoordinate intPt = intersection.GetIntersectionPoint(i);

                if (!(intPt.Equals(p0) || intPt.Equals(p1)))
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
            foreach (ISegmentString<TCoordinate> ss in _segStrings)
            {
                ICoordinateSequence<TCoordinate> pts = ss.Coordinates;
                CheckEndPtVertexIntersections(pts.First, _segStrings);
                CheckEndPtVertexIntersections(pts.Last, _segStrings);
            }
        }

        private static void CheckEndPtVertexIntersections(TCoordinate testPt,
                                                          IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            foreach (ISegmentString<TCoordinate> ss in segStrings)
            {
                ICoordinateSequence<TCoordinate> pts = ss.Coordinates;

                Int32 pointIndex = 1; // Skip 1st
                Int32 lastIndex = ss.Coordinates.Count - 1;
                foreach (TCoordinate coordinate in sl.Enumerable.Skip(pts, 1))
                {
                    if (pointIndex >= lastIndex)
                        break;

                    if (coordinate.Equals(testPt))
                    {
                        throw new TopologyException(
                            "Found end point / interior point intersection at index "
                            + pointIndex + " :pt " + testPt);
                    }

                    pointIndex += 1;
                }
            }
        }
    }
}