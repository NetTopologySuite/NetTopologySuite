using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleSegmentStringsSnapper
    {
        private int nSnaps = 0;

        /// <summary>
        /// 
        /// </summary>
        public SimpleSegmentStringsSnapper() { }

        /// <summary>
        /// 
        /// </summary>
        public virtual int NumSnaps
        {
            get
            {
                return nSnaps;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="ss"></param>
        /// <param name="testAllSegments"></param>
        public virtual void ComputeNodes(IList edges, SegmentSnapper ss, bool testAllSegments)
        {
            nSnaps = 0;

            for (IEnumerator i0 = edges.GetEnumerator(); i0.MoveNext(); ) 
            {
                SegmentString edge0 = (SegmentString) i0.Current;
                for (IEnumerator i1 = edges.GetEnumerator(); i1.MoveNext(); ) 
                {
                    SegmentString edge1 = (SegmentString) i1.Current;
                    if (testAllSegments || edge0 != edge1)
                        ComputeSnaps(edge0, edge1, ss);
                }
            }
        }

        /// <summary> 
        /// Performs a brute-force comparison of every segment in each SegmentString.
        /// This has n^2 performance.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        /// <param name="ss"></param>
        private void ComputeSnaps(SegmentString e0, SegmentString e1, SegmentSnapper ss)
        {
            Coordinate[] pts0 = e0.Coordinates;
            Coordinate[] pts1 = e1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
            {
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                {
                    bool isNodeAdded = ss.AddSnappedNode(pts0[i0], e1, i1);
                    if (isNodeAdded) nSnaps++;
                }
            }
        }
    }
}
