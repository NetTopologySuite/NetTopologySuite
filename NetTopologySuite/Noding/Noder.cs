using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes all intersections between segments in a set of <c>SegmentString</c>s.
    /// Intersections found are represented as <c>SegmentNode</c>s and add to the
    /// <c>SegmentString</c>s in which they occur.
    /// </summary>
    public abstract class Noder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        public static IList GetNodedEdges(IList segStrings)
        {
            IList resultEdgelist = new ArrayList();
            GetNodedEdges(segStrings, resultEdgelist);
            return resultEdgelist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="resultEdgelist"></param>
        public static void GetNodedEdges(IList segStrings, IList resultEdgelist)
        {
            for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
            {
                SegmentString ss = (SegmentString)i.Current;                
                ss.IntersectionList.AddSplitEdges(resultEdgelist);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected SegmentIntersector segInt;

        /// <summary>
        /// 
        /// </summary>
        public Noder() { }

        /// <summary>
        /// 
        /// </summary>
        public virtual SegmentIntersector SegmentIntersector
        {
            get
            {
                return segInt;
            }
            set
            {
                segInt = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        public abstract IList Node(IList segStrings);
    }
}
