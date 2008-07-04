using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Chain;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="SegmentString" />s
    /// and adds them to each string.
    /// The <see cref="ISegmentIntersector" /> is passed to a <see cref="INoder" />.
    /// The <see cref="SegmentString.AddIntersections" /> method is called whenever the <see cref="INoder" />
    /// detects that two <see cref="SegmentString" />s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public class IntersectionAdder : ISegmentIntersector
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

        /**
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private bool hasIntersection = false;
        private bool hasProper = false;
        private bool hasProperInterior = false;
        private bool hasInterior = false;

        // the proper intersection point found
        private ICoordinate properIntersectionPoint = null;

        private LineIntersector li = null;        
        
        /// <summary>
        /// 
        /// </summary>
        public int NumIntersections = 0;
        
        /// <summary>
        /// 
        /// </summary>
        public int NumInteriorIntersections = 0;
        
        /// <summary>
        /// 
        /// </summary>
        public int NumProperIntersections = 0;

        /// <summary>
        /// 
        /// </summary>
        public int NumTests = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntersectionAdder"/> class.
        /// </summary>
        /// <param name="li"></param>
        public IntersectionAdder(LineIntersector li)
        {
            this.li = li;
        }

        /// <summary>
        /// 
        /// </summary>
        public LineIntersector LineIntersector
        {
            get
            {
                return li;
            }
        }

        /// <summary>
        /// Returns the proper intersection point, or <c>null</c> if none was found.
        /// </summary>
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
        /// in the interior of the entire <see cref="Geometry" />, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the <see cref="Geometry" />.
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
        /// contained in the set of boundary nodes set for this <see cref="ISegmentIntersector" />.
        /// </summary>
        public bool HasProperInteriorIntersection
        {
            get
            {
                return hasProperInterior;
            }
        }
        
        /// <summary>
        /// An interior intersection is an intersection which is
        /// in the interior of some segment.
        /// </summary>
        public bool HasInteriorIntersection
        {
            get
            {
                return hasInterior;
            }
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning and end segments.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        /// <returns></returns>
        private bool IsTrivialIntersection(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1)
        {
            if(e0 == e1)
            {
                if(li.IntersectionNum == 1)
                {
                    if(IsAdjacentSegments(segIndex0, segIndex1))
                        return true;
                    if (e0.IsClosed)
                    {
                        int maxSegIndex = e0.Count - 1;
                        if ( (segIndex0 == 0 && segIndex1 == maxSegIndex) || 
                             (segIndex1 == 0 && segIndex0 == maxSegIndex) )                        
                                return true;                        
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> class to process
        /// intersections for two segments of the <see cref="SegmentString" /> being intersected.
        /// Note that some clients (such as <see cref="MonotoneChain" />s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void ProcessIntersections(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1)
        {
            if (e0 == e1 && segIndex0 == segIndex1)
                return;

            NumTests++;
            ICoordinate p00 = e0.Coordinates[segIndex0];
            ICoordinate p01 = e0.Coordinates[segIndex0 + 1];
            ICoordinate p10 = e1.Coordinates[segIndex1];
            ICoordinate p11 = e1.Coordinates[segIndex1 + 1];

            li.ComputeIntersection(p00, p01, p10, p11);            
            if(li.HasIntersection)
            {                
                NumIntersections++;
                if (li.IsInteriorIntersection())
                {
                    NumInteriorIntersections++;
                    hasInterior = true;                    
                }
                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    hasIntersection = true;
                    e0.AddIntersections(li, segIndex0, 0);
                    e1.AddIntersections(li, segIndex1, 1);
                    if (li.IsProper)
                    {
                        NumProperIntersections++;
                        hasProper = true;
                        hasProperInterior = true;
                    }
                }
            }
        }
    }
}
