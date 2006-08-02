using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <c>SegmentString</c>s by
    /// performing a brute-force comparison of every segment to every other one.
    /// This has n^2 performance, so is too slow for use on large numbers
    /// of segments.
    /// </summary>
    public class SimpleNoder : Noder
    {
        /// <summary>
        /// 
        /// </summary>
        public SimpleNoder() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSegStrings"></param>
        /// <returns></returns>
        public override IList Node(IList inputSegStrings)
        {
            for (IEnumerator i0 = inputSegStrings.GetEnumerator(); i0.MoveNext(); ) 
            {
                SegmentString edge0 = (SegmentString)i0.Current;
                for (IEnumerator i1 = inputSegStrings.GetEnumerator(); i1.MoveNext(); ) 
                {
                    SegmentString edge1 = (SegmentString) i1.Current;
                    ComputeIntersects(edge0, edge1);
                }
            }
            IList nodedSegStrings = GetNodedEdges(inputSegStrings);
            return nodedSegStrings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        private void ComputeIntersects(SegmentString e0, SegmentString e1)
        {
            Coordinate[] pts0 = e0.Coordinates;
            Coordinate[] pts1 = e1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++) 
                for (int i1 = 0; i1 < pts1.Length - 1; i1++) 
                    segInt.ProcessIntersections(e0, i0, e1, i1);                        
        }
    }
}
