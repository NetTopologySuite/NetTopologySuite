using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Index.Chain;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Finds proper and interior intersections in a set of <see cref="ISegmentString" />s,
    /// and adds them as nodes.
    /// </summary>
    public class IntersectionFinderAdder : ISegmentIntersector
    {
        private LineIntersector li = null;
        private readonly IList _interiorIntersections = null;

        /// <summary>
        /// Creates an intersection finder which finds all proper intersections.
        /// </summary>
        /// <param name="li">The <see cref="LineIntersector" /> to use.</param>
        public IntersectionFinderAdder(LineIntersector li)
        {
            this.li = li;
            _interiorIntersections = new ArrayList();
        }

        /// <summary>
        /// 
        /// </summary>
        public IList InteriorIntersections
        {
            get
            {
                return _interiorIntersections;
            }
        }
   
        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> class to process
        /// intersections for two segments of the <see cref="ISegmentString" />s being intersected.
        /// Note that some clients (such as <see cref="MonotoneChain" />s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1 )
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) 
                return;

            ICoordinate[] coordinates0 = e0.Coordinates;
            ICoordinate p00 = coordinates0[segIndex0];
            ICoordinate p01 = coordinates0[segIndex0 + 1];
            ICoordinate[] coordinates1 = e1.Coordinates;
            ICoordinate p10 = coordinates1[segIndex1];
            ICoordinate p11 = coordinates1[segIndex1 + 1];
            li.ComputeIntersection(p00, p01, p10, p11);            

            if (li.HasIntersection)
            {
                if (li.IsInteriorIntersection())
                {
                    for(int intIndex = 0; intIndex < li.IntersectionNum; intIndex++)
                        _interiorIntersections.Add(li.GetIntersection(intIndex));
                    
                    ((NodedSegmentString)e0).AddIntersections(li, segIndex0, 0);
                    ((NodedSegmentString)e1).AddIntersections(li, segIndex1, 1);
                }
            }
        }

        ///<summary>
        /// Always process all intersections
        ///</summary>
        public Boolean Done { get { return false; } }
    }
}
