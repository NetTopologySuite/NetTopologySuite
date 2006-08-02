using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Index.Strtree;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <c>SegmentString</c>s using a index based
    /// on <c>MonotoneChain</c>s and a <c>ISpatialIndex</c>.
    /// The <c>ISpatialIndex</c> used should be something that supports
    /// envelope (range) queries efficiently (such as a <c>Quadtree</c>
    /// or <c>STRtree</c>.
    /// </summary>
    public class MCQuadtreeNoder : Noder
    {
        private IList chains = new ArrayList();
        private ISpatialIndex index= new STRtree();
        private int idCounter = 0;

        // statistics
        private int nOverlaps = 0;

        /// <summary>
        /// 
        /// </summary>
        public MCQuadtreeNoder() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSegStrings"></param>
        /// <returns></returns>
        public override IList Node(IList inputSegStrings)
        {
            for (IEnumerator i = inputSegStrings.GetEnumerator(); i.MoveNext(); ) 
                Add((SegmentString) i.Current);            
            IntersectChains();            
            IList nodedSegStrings = GetNodedEdges(inputSegStrings);
            return nodedSegStrings;
        }

        /// <summary>
        /// 
        /// </summary>
        private void IntersectChains()
        {
            MonotoneChainOverlapAction overlapAction = new SegmentOverlapAction(segInt);

            for (IEnumerator i = chains.GetEnumerator(); i.MoveNext(); ) 
            {
                MonotoneChain queryChain = (MonotoneChain) i.Current;
                IList overlapChains = index.Query(queryChain.Envelope);                
                for (IEnumerator j = overlapChains.GetEnumerator(); j.MoveNext(); ) 
                {
                    MonotoneChain testChain = (MonotoneChain) j.Current;                    
                    /*
                    * following test makes sure we only compare each pair of chains once
                    * and that we don't compare a chain to itself
                    */
                    if (testChain.Id > queryChain.Id) 
                    {
                        queryChain.ComputeOverlaps(testChain, overlapAction);
                        nOverlaps++;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStr"></param>
        private void Add(SegmentString segStr)
        {
            IList segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            for (IEnumerator i = segChains.GetEnumerator(); i.MoveNext(); ) 
            {
                MonotoneChain mc = (MonotoneChain) i.Current;
                mc.Id = idCounter++;
                index.Insert(mc.Envelope, mc);
                chains.Add(mc);
            }                        
        }

        /// <summary>
        /// 
        /// </summary>
        public class SegmentOverlapAction : MonotoneChainOverlapAction
        {
            private SegmentIntersector si = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="si"></param>
            public SegmentOverlapAction(SegmentIntersector si)
            {
                this.si = si;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mc1"></param>
            /// <param name="start1"></param>
            /// <param name="mc2"></param>
            /// <param name="start2"></param>
            public override void Overlap(MonotoneChain mc1, int start1, MonotoneChain mc2, int start2)
            {
                SegmentString ss1 = (SegmentString) mc1.Context;
                SegmentString ss2 = (SegmentString) mc2.Context;
                si.ProcessIntersections(ss1, start1, ss2, start2);
            }
        }
    }
}
