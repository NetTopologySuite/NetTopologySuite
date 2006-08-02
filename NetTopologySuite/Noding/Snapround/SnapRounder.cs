using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary> 
    /// Uses snap rounding to compute a rounded, noded arrangement from a
    /// set of linestrings.
    /// </summary>
    public class SnapRounder
    {
        /// <summary>
        /// 
        /// </summary>
        protected LineIntersector li = null;

        /// <summary>
        /// 
        /// </summary>
        public SnapRounder() { }

        /// <summary>
        /// 
        /// </summary>
        public virtual LineIntersector LineIntersector
        {
            get
            {
                return li;
            }
            set
            {
                li = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSegmentStrings"></param>
        /// <returns></returns>
        public virtual IList node(IList inputSegmentStrings)
        {
            IList resultSegStrings = FullyIntersectSegments(inputSegmentStrings, li);
            NodingValidator nv = new NodingValidator(resultSegStrings);
            nv.CheckValid();
            return resultSegStrings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        /// <returns></returns>
        private IList FullyIntersectSegments(IList segStrings, LineIntersector li)
        {
            SegmentIntersector si = null;
            IList inputSegStrings = segStrings;
            IList nodedSegStrings = null;
            do 
            {
                si = new SegmentIntersector(li);
                Noder noder = new SimpleNoder();
                noder.SegmentIntersector = si;
                nodedSegStrings = noder.Node(inputSegStrings);
                IList snappedSegStrings = ComputeSnaps(nodedSegStrings);        
                inputSegStrings = snappedSegStrings;
            }
            while (si.numInteriorIntersections > 0);
            return nodedSegStrings;
        }

        /// <summary> 
        /// Computes new nodes introduced as a result of snapping segments to near vertices
        /// </summary>
        /// <param name="segStrings"></param>
        private IList ComputeSnaps(IList segStrings)
        {
            IList splitSegStringList = null;
            int numSnaps;
            /*
             * Have to snap repeatedly, because snapping a line may move it enough
             * that it crosses another hot pixel.
             */
            do
            {
                SimpleSegmentStringsSnapper snapper = new SimpleSegmentStringsSnapper();
                SegmentSnapper ss = new SegmentSnapper();
                snapper.ComputeNodes(segStrings, ss, true);
                numSnaps = snapper.NumSnaps;
                // save the list of split seg Strings in case we are going to return it
                splitSegStringList = Noder.GetNodedEdges(segStrings);
                segStrings = splitSegStringList;
            } 
            while (numSnaps > 0);
            return splitSegStringList;
        }
    }
}
