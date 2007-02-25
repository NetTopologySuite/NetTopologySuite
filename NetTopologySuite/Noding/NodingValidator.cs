using System;
using System.Collections;
using System.Text;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Validates that a collection of <see cref="SegmentString" />s is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class NodingValidator
    {
        private LineIntersector li = new RobustLineIntersector();
        private IList segStrings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodingValidator"/> class.
        /// </summary>
        /// <param name="segStrings">The seg strings.</param>
        public NodingValidator(IList segStrings)
        {
            this.segStrings = segStrings;
        }

        /// <summary>
        /// 
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
            foreach (SegmentString ss in segStrings)
                CheckCollapses(ss);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        private void CheckCollapses(SegmentString ss)
        {
            ICoordinate[] pts = ss.Coordinates;
            for (int i = 0; i < pts.Length - 2; i++)
                CheckCollapse(pts[i], pts[i + 1], pts[i + 2]);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void CheckCollapse(ICoordinate p0, ICoordinate p1, ICoordinate p2)
        {
            if (p0.Equals(p2))
                throw new Exception("found non-noded collapse at: " + p0 + ", " + p1 + " " + p2);
        }

        /// <summary>
        /// Checks all pairs of segments for intersections at an interior point of a segment.
        /// </summary>
        private void CheckInteriorIntersections()
        {
            foreach (SegmentString ss0 in segStrings)
                foreach(SegmentString ss1 in segStrings)
                    CheckInteriorIntersections(ss0, ss1);                
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss0"></param>
        /// <param name="ss1"></param>
        private void CheckInteriorIntersections(SegmentString ss0, SegmentString ss1)
        {
            ICoordinate[] pts0 = ss0.Coordinates;
            ICoordinate[] pts1 = ss1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                    CheckInteriorIntersections(ss0, i0, ss1, i1);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        private void CheckInteriorIntersections(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1)
        {
            if (e0 == e1 && segIndex0 == segIndex1) 
                return;

            ICoordinate p00 = e0.Coordinates[segIndex0];
            ICoordinate p01 = e0.Coordinates[segIndex0 + 1];
            ICoordinate p10 = e1.Coordinates[segIndex1];
            ICoordinate p11 = e1.Coordinates[segIndex1 + 1];

            li.ComputeIntersection(p00, p01, p10, p11);
            if (li.HasIntersection)  
                if (li.IsProper || HasInteriorIntersection(li, p00, p01) || HasInteriorIntersection(li, p10, p11))
                    throw new Exception("found non-noded intersection at " + p00 + "-" + p01
                                               + " and " + p10 + "-" + p11);                            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="li"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns><c>true</c> if there is an intersection point which is not an endpoint of the segment p0-p1.</returns>
        private bool HasInteriorIntersection(LineIntersector li, ICoordinate p0, ICoordinate p1)
        {
            for (int i = 0; i < li.IntersectionNum; i++)
            {
                ICoordinate intPt = li.GetIntersection(i);
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
            foreach (SegmentString ss in segStrings)
            {
                ICoordinate[] pts = ss.Coordinates;
                CheckEndPtVertexIntersections(pts[0], segStrings);
                CheckEndPtVertexIntersections(pts[pts.Length - 1], segStrings);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testPt"></param>
        /// <param name="segStrings"></param>
        private void CheckEndPtVertexIntersections(ICoordinate testPt, IList segStrings)
        {
            foreach (SegmentString ss in segStrings)
            {
                ICoordinate[] pts = ss.Coordinates;
                for (int j = 1; j < pts.Length - 1; j++)
                    if (pts[j].Equals(testPt))
                        throw new Exception("found endpt/interior pt intersection at index " + j + " :pt " + testPt);                
            }
        }
    }
}
