using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a TaggedLineString, preserving topology
    /// (in the sense that no new intersections are introduced).
    /// Uses the recursive D-P algorithm.
    /// </summary>
    public class TaggedLineStringSimplifier<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Added readonly!
        private static readonly LineIntersector<TCoordinate> _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector();

        private readonly LineSegmentIndex<TCoordinate> _inputIndex = new LineSegmentIndex<TCoordinate>();
        private readonly LineSegmentIndex<TCoordinate> _outputIndex = new LineSegmentIndex<TCoordinate>();
        private TaggedLineString<TCoordinate> _line;
        private IList<TCoordinate> _linePts;
        private Double _distanceTolerance = 0.0;

        public TaggedLineStringSimplifier(LineSegmentIndex<TCoordinate> inputIndex,
                                          LineSegmentIndex<TCoordinate> outputIndex)
        {
            _inputIndex = inputIndex;
            _outputIndex = outputIndex;
        }

        public Double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set { _distanceTolerance = value; }
        }

        public void Simplify(TaggedLineString<TCoordinate> line)
        {
            _line = line;
            _linePts = line.ParentCoordinates;
            simplifySection(0, _linePts.Count - 1, 0);
        }

        private void simplifySection(Int32 i, Int32 j, Int32 depth)
        {
            depth += 1;
            Int32[] sectionIndex = new Int32[2];

            if ((i + 1) == j)
            {
                TaggedLineSegment<TCoordinate> newSeg = _line.Segments[i];
                _line.AddToResult(newSeg);
                // leave this segment in the input index, for efficiency
                return;
            }

            Double[] distance = new Double[1];
            Int32 furthestPtIndex = findFurthestPoint(_linePts, i, j, distance);
            Boolean isValidToFlatten = true;

            // must have enough points in the output line
            if (_line.ResultSize < _line.MinimumSize && depth < 2)
            {
                isValidToFlatten = false;
            }

            // flattening must be less than distanceTolerance
            if (distance[0] > DistanceTolerance)
            {
                isValidToFlatten = false;
            }

            // test if flattened section would cause intersection
            LineSegment<TCoordinate> candidateSeg = new LineSegment<TCoordinate>(
                _linePts[i], _linePts[j]);

            sectionIndex[0] = i;
            sectionIndex[1] = j;

            if (hasBadIntersection(_line, sectionIndex, candidateSeg))
            {
                isValidToFlatten = false;
            }

            if (isValidToFlatten)
            {
                TaggedLineSegment<TCoordinate> newSeg = flatten(i, j);
                _line.AddToResult(newSeg);
                return;
            }

            simplifySection(i, furthestPtIndex, depth);
            simplifySection(furthestPtIndex, j, depth);
        }

        private static Int32 findFurthestPoint(IList<TCoordinate> pts, Int32 i, Int32 j, Double[] maxDistance)
        {
            LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(pts[i], pts[j]);

            Double maxDist = -1.0;
            Int32 maxIndex = i;

            for (Int32 k = i + 1; k < j; k++)
            {
                TCoordinate midPt = pts[k];
                
                Double distance = seg.Distance(midPt);

                if (distance > maxDist)
                {
                    maxDist = distance;
                    maxIndex = k;
                }
            }

            maxDistance[0] = maxDist;
            return maxIndex;
        }

        private LineSegment<TCoordinate> flatten(Int32 start, Int32 end)
        {
            // make a new segment for the simplified point
            TCoordinate p0 = _linePts[start];
            TCoordinate p1 = _linePts[end];
            LineSegment<TCoordinate> newSeg = new LineSegment<TCoordinate>(p0, p1);

            // update the indexes
            remove(_line, start, end);
            _outputIndex.Add(newSeg);
            return newSeg;
        }

        /*
        * Index of section to be tested for flattening - reusable
        */
        //private Int32[] validSectionIndex = new Int32[2];

        private Boolean hasBadIntersection(TaggedLineString<TCoordinate> parentLine, Int32[] sectionIndex,
                                           LineSegment<TCoordinate> candidateSeg)
        {
            if (hasBadOutputIntersection(candidateSeg))
            {
                return true;
            }

            if (hasBadInputIntersection(parentLine, sectionIndex, candidateSeg))
            {
                return true;
            }

            return false;
        }

        private Boolean hasBadOutputIntersection(LineSegment<TCoordinate> candidateSeg)
        {
            IEnumerable<LineSegment<TCoordinate>> querySegs = _outputIndex.Query(candidateSeg);

            foreach (LineSegment<TCoordinate> querySeg in querySegs)
            {
                if (hasInteriorIntersection(querySeg, candidateSeg))
                {
                    return true;
                }
            }

            return false;
        }

        private Boolean hasBadInputIntersection(TaggedLineString<TCoordinate> parentLine, Int32[] sectionIndex,
                                                LineSegment<TCoordinate> candidateSeg)
        {
            IEnumerable<LineSegment<TCoordinate>> querySegs = _inputIndex.Query(candidateSeg);

            foreach (LineSegment<TCoordinate> querySeg in querySegs)
            {
                if (hasInteriorIntersection(querySeg, candidateSeg))
                {
                    if (isInLineSection(parentLine, sectionIndex, querySeg))
                    {
                        continue;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests whether a segment is in a section of a TaggedLineString-
        /// </summary>
        private static Boolean isInLineSection(TaggedLineString<TCoordinate> line, Int32[] sectionIndex,
                                               TaggedLineSegment<TCoordinate> seg)
        {
            // not in this line
            if (seg.Parent != line.Parent)
            {
                return false;
            }

            Int32 segIndex = seg.Index;

            if (segIndex >= sectionIndex[0] && segIndex < sectionIndex[1])
            {
                return true;
            }

            return false;
        }

        private static Boolean hasInteriorIntersection(LineSegment<TCoordinate> seg0, LineSegment<TCoordinate> seg1)
        {
            _li.ComputeIntersection(seg0.P0, seg0.P1, seg1.P0, seg1.P1);
            return _li.IsInteriorIntersection();
        }

        /// <summary>
        /// Remove the segs in the section of the line.
        /// </summary>
        private void remove(TaggedLineString<TCoordinate> line, Int32 start, Int32 end)
        {
            for (Int32 i = start; i < end; i++)
            {
                TaggedLineSegment<TCoordinate> seg = line.Segments[i];
                _inputIndex.Remove(seg);
            }
        }
    }
}