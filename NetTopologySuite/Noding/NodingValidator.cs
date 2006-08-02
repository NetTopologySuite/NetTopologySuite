using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Validates that a collection of <c>SegmentString</c>s is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class NodingValidator
    {
        private LineIntersector li = new RobustLineIntersector();

        private IList segStrings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        public NodingValidator(IList segStrings)
        {
            this.segStrings = segStrings;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CheckValid()
        {
            CheckNoInteriorPointsSame();
            CheckProperIntersections();
        }


        /// <summary>
        /// 
        /// </summary>
        private void CheckProperIntersections()
        {
            for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
            {
                SegmentString ss0 = (SegmentString)i.Current;
                for (IEnumerator j = segStrings.GetEnumerator(); j.MoveNext(); )
                {
                    SegmentString ss1 = (SegmentString)j.Current;
                    CheckProperIntersections(ss0, ss1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss0"></param>
        /// <param name="ss1"></param>
        private void CheckProperIntersections(SegmentString ss0, SegmentString ss1)
        {
            Coordinate[] pts0 = ss0.Coordinates;
            Coordinate[] pts1 = ss1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                    CheckProperIntersections(ss0, i0, ss1, i1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        private void CheckProperIntersections(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1)
        {
            if (e0 == e1 && segIndex0 == segIndex1) 
                return;
            Coordinate p00 = e0.Coordinates[segIndex0];
            Coordinate p01 = e0.Coordinates[segIndex0 + 1];
            Coordinate p10 = e1.Coordinates[segIndex1];
            Coordinate p11 = e1.Coordinates[segIndex1 + 1];
            li.ComputeIntersection(p00, p01, p10, p11);
            if (li.HasIntersection)
            {
                if (li.IsProper || HasInteriorIntersection(li, p00, p01) || HasInteriorIntersection(li, p00, p01))                
                    throw new ApplicationException("found non-noded intersection at " + p00 + "-" + p01 + 
                                                   " and " + p10 + "-" + p11);                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="li"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns><c>true</c> if there is an intersection point which is not an endpoint of the segment p0-p1</returns>
        private bool HasInteriorIntersection(LineIntersector li, Coordinate p0, Coordinate p1)
        {
            for (int i = 0; i < li.IntersectionNum; i++)
            {
                Coordinate intPt = li.GetIntersection(i);
                if (!(intPt.Equals(p0) || intPt.Equals(p1)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckNoInteriorPointsSame()
        {
            for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
            {
                SegmentString ss = (SegmentString)i.Current;
                Coordinate[] pts = ss.Coordinates;
                CheckNoInteriorPointsSame(pts[0], segStrings);
                CheckNoInteriorPointsSame(pts[pts.Length - 1], segStrings);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testPt"></param>
        /// <param name="segStrings"></param>
        private void CheckNoInteriorPointsSame(Coordinate testPt, IList segStrings)
        {
            for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
            {
                SegmentString ss = (SegmentString)i.Current;
                Coordinate[] pts = ss.Coordinates;
                for (int j = 1; j < pts.Length - 1; j++)
                    if (pts[j].Equals(testPt))
                        throw new ApplicationException("found bad noding at index " + j + " pt " + testPt);                
            }
        }
    }
}
