using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a TaggedLineString, preserving topology
    /// (in the sense that no new intersections are introduced).
    /// Uses the recursive D-P algorithm.
    /// </summary>
    public class TaggedLineStringSimplifier
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Added readonly!
        private static readonly LineIntersector li = new RobustLineIntersector();

        private LineSegmentIndex inputIndex = new LineSegmentIndex();
        private LineSegmentIndex outputIndex = new LineSegmentIndex();
        private TaggedLineString line;
        private ICoordinate[] linePts;
        private double distanceTolerance = 0.0;        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputIndex"></param>
        /// <param name="outputIndex"></param>
        public TaggedLineStringSimplifier(LineSegmentIndex inputIndex, LineSegmentIndex outputIndex)
        {            
            this.inputIndex = inputIndex;
            this.outputIndex = outputIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public double DistanceTolerance
        {
            get 
            {
                return distanceTolerance; 
            }
            set
            {
                distanceTolerance = value; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void Simplify(TaggedLineString line)
        {
            this.line = line;
            linePts = line.ParentCoordinates;
            SimplifySection(0, linePts.Length - 1, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="depth"></param>
        private void SimplifySection(int i, int j, int depth)
        {
            depth += 1;
            int[] sectionIndex = new int[2];
            if((i+1) == j)
            {
                LineSegment newSeg = line.GetSegment(i);
                line.AddToResult(newSeg);
                // leave this segment in the input index, for efficiency
                return;
            }

            double[] distance = new double[1];
            int furthestPtIndex = FindFurthestPoint(linePts, i, j, distance);
            bool isValidToFlatten = true;

            // must have enough points in the output line
            if (line.ResultSize < line.MinimumSize && depth < 2)  
                isValidToFlatten = false;
            // flattening must be less than distanceTolerance
            if (distance[0] > DistanceTolerance)
                isValidToFlatten = false;
            // test if flattened section would cause intersection
            LineSegment candidateSeg = new LineSegment();
            candidateSeg.P0 = linePts[i];
            candidateSeg.P1 = linePts[j];
            sectionIndex[0] = i;
            sectionIndex[1] = j;
            if (HasBadIntersection(line, sectionIndex, candidateSeg)) isValidToFlatten = false;

            if (isValidToFlatten)
            {
                LineSegment newSeg = Flatten(i, j);
                line.AddToResult(newSeg);
                return;
            }
            SimplifySection(i, furthestPtIndex, depth);
            SimplifySection(furthestPtIndex, j, depth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        private int FindFurthestPoint(ICoordinate[] pts, int i, int j, double[] maxDistance)
        {
            LineSegment seg = new LineSegment();
            seg.P0 = pts[i];
            seg.P1 = pts[j];
            double maxDist = -1.0;
            int maxIndex = i;
            for (int k = i + 1; k < j; k++) 
            {
                ICoordinate midPt = pts[k];
                double distance = seg.Distance(midPt);
                if (distance > maxDist) 
                {
                    maxDist = distance;
                    maxIndex = k;
                }
            }
            maxDistance[0] = maxDist;
            return maxIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private LineSegment Flatten(int start, int end)
        {
            // make a new segment for the simplified point
            ICoordinate p0 = linePts[start];
            ICoordinate p1 = linePts[end];
            LineSegment newSeg = new LineSegment(p0, p1);
            // update the indexes
            Remove(line, start, end);
            outputIndex.Add(newSeg);
            return newSeg;
        }

        /*
        * Index of section to be tested for flattening - reusable
        */
        private int[] validSectionIndex = new int[2];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLine"></param>
        /// <param name="sectionIndex"></param>
        /// <param name="candidateSeg"></param>
        /// <returns></returns>
        private bool HasBadIntersection(TaggedLineString parentLine, int[] sectionIndex, LineSegment candidateSeg)
        {
            if (HasBadOutputIntersection(candidateSeg)) 
                return true;
            if (HasBadInputIntersection(parentLine, sectionIndex, candidateSeg)) 
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateSeg"></param>
        /// <returns></returns>
        private bool HasBadOutputIntersection(LineSegment candidateSeg)
        {
            IList querySegs = outputIndex.Query(candidateSeg);
            for (IEnumerator i = querySegs.GetEnumerator(); i.MoveNext(); ) 
            {
                LineSegment querySeg = (LineSegment) i.Current;
                if (HasInteriorIntersection(querySeg, candidateSeg)) 
                    return true;                
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLine"></param>
        /// <param name="sectionIndex"></param>
        /// <param name="candidateSeg"></param>
        /// <returns></returns>
        private bool HasBadInputIntersection(TaggedLineString parentLine, int[] sectionIndex, LineSegment candidateSeg)
        {
            IList querySegs = inputIndex.Query(candidateSeg);
            for (IEnumerator i = querySegs.GetEnumerator(); i.MoveNext(); ) 
            {
                TaggedLineSegment querySeg = (TaggedLineSegment) i.Current;
                if (HasInteriorIntersection(querySeg, candidateSeg)) 
                {
                    if (IsInLineSection(parentLine, sectionIndex, querySeg))
                        continue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether a segment is in a section of a TaggedLineString-
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sectionIndex"></param>
        /// <param name="seg"></param>
        /// <returns></returns>
        private static bool IsInLineSection(TaggedLineString line, int[] sectionIndex, TaggedLineSegment seg)
        {
            // not in this line
            if (seg.Parent != line.Parent) return false;
            int segIndex = seg.Index;
            if (segIndex >= sectionIndex[0] && segIndex < sectionIndex[1])
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg0"></param>
        /// <param name="seg1"></param>
        /// <returns></returns>
        private bool HasInteriorIntersection(LineSegment seg0, LineSegment seg1)
        {
            li.ComputeIntersection(seg0.P0, seg0.P1, seg1.P0, seg1.P1);
            return li.IsInteriorIntersection();
        }

        /// <summary>
        /// Remove the segs in the section of the line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Remove(TaggedLineString line, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                TaggedLineSegment seg = line.GetSegment(i);
                inputIndex.Remove(seg);
            }
        }
    }
}
