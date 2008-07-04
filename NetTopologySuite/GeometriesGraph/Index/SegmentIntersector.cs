using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// 
    /// </summary>
    public class SegmentIntersector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool IsAdjacentSegments(int i1, int i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /*
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private bool hasIntersection = false;
        private bool hasProper = false;
        private bool hasProperInterior = false;

        // the proper intersection point found
        private ICoordinate properIntersectionPoint = null;

        private LineIntersector li;
        private bool includeProper;
        private bool recordIsolated;
        private int numIntersections = 0;

        /// <summary>
        /// Testing only.
        /// </summary>
        public int numTests = 0;

        private ICollection[] bdyNodes;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="li"></param>
        /// <param name="includeProper"></param>
        /// <param name="recordIsolated"></param>
        public SegmentIntersector(LineIntersector li, bool includeProper, bool recordIsolated)
        {
            this.li = li;
            this.includeProper = includeProper;
            this.recordIsolated = recordIsolated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bdyNodes0"></param>
        /// <param name="bdyNodes1"></param>
        public void SetBoundaryNodes(ICollection bdyNodes0, ICollection bdyNodes1)
        {
            bdyNodes = new ICollection[2];
            bdyNodes[0] = bdyNodes0;
            bdyNodes[1] = bdyNodes1;
        }

        /// <returns> 
        /// The proper intersection point, or <c>null</c> if none was found.
        /// </returns>
        public ICoordinate ProperIntersectionPoint
        {
            get
            {
                return properIntersectionPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasIntersection
        {
            get
            {
                return hasIntersection;
            }
        }

        /// <summary>
        /// A proper intersection is an intersection which is interior to at least two
        /// line segments.  Note that a proper intersection is not necessarily
        /// in the interior of the entire Geometry, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the Geometry.
        /// </summary>
        public bool HasProperIntersection
        {
            get
            {
                return hasProper; 
            }
        }

        /// <summary> 
        /// A proper interior intersection is a proper intersection which is not
        /// contained in the set of boundary nodes set for this SegmentIntersector.
        /// </summary>
        public bool HasProperInteriorIntersection
        {
            get
            {
                return hasProperInterior;
            }
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        private bool IsTrivialIntersection(Edge e0, int segIndex0, Edge e1, int segIndex1)
        {
            if (ReferenceEquals(e0, e1))
            {
                if (li.IntersectionNum == 1)
                {
                    if (IsAdjacentSegments(segIndex0, segIndex1))
                        return true;
                    if (e0.IsClosed)
                    {
                        int maxSegIndex = e0.NumPoints - 1;
                        if ((segIndex0 == 0 && segIndex1 == maxSegIndex) ||
                            (segIndex1 == 0 && segIndex0 == maxSegIndex))                        
                                return true;                        
                    }
                }
            }
            return false;
        }

        /// <summary> 
        /// This method is called by clients of the EdgeIntersector class to test for and add
        /// intersections for two segments of the edges being intersected.
        /// Note that clients (such as MonotoneChainEdges) may choose not to intersect
        /// certain pairs of segments for efficiency reasons.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void AddIntersections(Edge e0, int segIndex0, Edge e1, int segIndex1)
        {            
            // if (e0 == e1 && segIndex0 == segIndex1) 
            if (ReferenceEquals(e0, e1) && segIndex0 == segIndex1)
                return;             // Diego Guidi say's: Avoid overload equality, i use references equality, otherwise TOPOLOGY ERROR!
                            
            numTests++;
            ICoordinate p00 = e0.Coordinates[segIndex0];
            ICoordinate p01 = e0.Coordinates[segIndex0 + 1];
            ICoordinate p10 = e1.Coordinates[segIndex1];
            ICoordinate p11 = e1.Coordinates[segIndex1 + 1];
            li.ComputeIntersection(p00, p01, p10, p11);            
            /*
             *  Always record any non-proper intersections.
             *  If includeProper is true, record any proper intersections as well.
             */
            if (li.HasIntersection)
            {
                if (recordIsolated)
                {
                    e0.Isolated = false;
                    e1.Isolated = false;
                }                
                numIntersections++;
                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    hasIntersection = true;
                    if (includeProper || !li.IsProper)
                    {                     
                        e0.AddIntersections(li, segIndex0, 0);
                        e1.AddIntersections(li, segIndex1, 1);
                    }
                    if (li.IsProper)
                    {
                        properIntersectionPoint = (ICoordinate) li.GetIntersection(0).Clone();
                        hasProper = true;
                        if (!IsBoundaryPoint(li, bdyNodes))
                            hasProperInterior = true;                        
                    }                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="li"></param>
        /// <param name="bdyNodes"></param>
        /// <returns></returns>
        private bool IsBoundaryPoint(LineIntersector li, ICollection[] bdyNodes)
        {
            if (bdyNodes == null) 
                return false;
            if (IsBoundaryPoint(li, bdyNodes[0]))
                return true;
            if (IsBoundaryPoint(li, bdyNodes[1])) 
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="li"></param>
        /// <param name="bdyNodes"></param>
        /// <returns></returns>
        private bool IsBoundaryPoint(LineIntersector li, ICollection bdyNodes)
        {
            for (IEnumerator i = bdyNodes.GetEnumerator(); i.MoveNext(); )
            {
                Node node = (Node) i.Current;
                ICoordinate pt = node.Coordinate;
                if (li.IsIntersection(pt)) 
                    return true;
            }
            return false;
        }
    }
}
