using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Contains a list of consecutive line segments which can be used to node the segments.
    /// The line segments are represented by an array of <c>Coordinate</c>s.
    /// </summary>
    public class SegmentString
    {
        private SegmentNodeList eiList = null;
        private Coordinate[] pts;
        private Object context;
        private bool isIsolated;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="context"></param>
        public SegmentString(Coordinate[] pts, Object context)
        {
            this.eiList = new SegmentNodeList(this);
            this.pts = pts;
            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Object Context
        {
            get
            {
                return context;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual SegmentNodeList IntersectionList
        {
            get
            {
                return eiList;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Count
        {
            get
            {
                return pts.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual Coordinate GetCoordinate(int i) 
        {
            return pts[i]; 
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Coordinate[] Coordinates
        {
            get
            {
                return pts;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Isolated
        {
            get
            {
                return this.isIsolated;
            }
            set
            {
                this.isIsolated = value; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsIsolated
        {
            get
            {
                return isIsolated;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsClosed
        {
            get
            {
                return pts[0].Equals(pts[pts.Length - 1]);
            }
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        public virtual void AddIntersections(LineIntersector li, int segmentIndex, int geomIndex)
        {
            for (int i = 0; i < li.IntersectionNum; i++)
                AddIntersection(li, segmentIndex, geomIndex, i);
        }

        /// <summary>
        /// Add an SegmentNode for intersection intIndex.
        /// An intersection that falls exactly on a vertex
        /// of the SegmentString is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        /// <param name="intIndex"></param>
        public virtual void AddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
        {
            Coordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            double dist = li.GetEdgeDistance(geomIndex, intIndex);
            AddIntersection(intPt, segmentIndex, dist);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        /// <param name="intIndex"></param>
        [Obsolete("Use AddIntersection instead!")]
        public virtual void OLDAddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
        {
            Coordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            int normalizedSegmentIndex = segmentIndex;
            double dist = li.GetEdgeDistance(geomIndex, intIndex);
            // normalize the intersection point location
            int nextSegIndex = normalizedSegmentIndex + 1;
            if (nextSegIndex < pts.Length)
            {
                Coordinate nextPt = pts[nextSegIndex];
                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                    dist = 0.0;
                }
            }
        }

        /// <summary>
        /// Add an EdgeIntersection for intersection intIndex.
        /// An intersection that falls exactly on a vertex of the edge is normalized
        /// to use the higher of the two possible segmentIndexes
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        public virtual void AddIntersection(Coordinate intPt, int segmentIndex)
        {
            double dist = LineIntersector.ComputeEdgeDistance(intPt, pts[segmentIndex], pts[segmentIndex + 1]);
            AddIntersection(intPt, segmentIndex, dist);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="dist"></param>
        public virtual void AddIntersection(Coordinate intPt, int segmentIndex, double dist)
        {
            int normalizedSegmentIndex = segmentIndex;            
            // normalize the intersection point location
            int nextSegIndex = normalizedSegmentIndex + 1;
            if (nextSegIndex < pts.Length)
            {
                Coordinate nextPt = pts[nextSegIndex];
                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                    dist = 0.0;
                }
            }
            /*
            * Add the intersection point to edge intersection list.
            */
            SegmentNode ei = eiList.Add(intPt, normalizedSegmentIndex, dist);          
        }
    }
}
