using System;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A LineIntersector is an algorithm that can both test whether
    /// two line segments intersect and compute the intersection point
    /// if they do.
    /// The intersection point may be computed in a precise or non-precise manner.
    /// Computing it precisely involves rounding it to an integer.  (This assumes
    /// that the input coordinates have been made precise by scaling them to
    /// an integer grid.)
    /// </summary>
    public abstract class LineIntersector 
    {
        /// <summary>
        /// 
        /// </summary>
        public const int DontIntersect = 0;
        
        /// <summary>
        /// 
        /// </summary>
        public const int DoIntersect = 1;
        
        /// <summary>
        /// 
        /// </summary>
        public const int Collinear = 2;

        /// <summary> 
        /// Computes the "edge distance" of an intersection point p along a segment.
        /// The edge distance is a metric of the point along the edge.
        /// The metric used is a robust and easy to compute metric function.
        /// It is not equivalent to the usual Euclidean metric.
        /// It relies on the fact that either the x or the y ordinates of the
        /// points in the edge are unique, depending on whether the edge is longer in
        /// the horizontal or vertical direction.
        /// NOTE: This function may produce incorrect distances
        /// for inputs where p is not precisely on p1-p2
        /// (E.g. p = (139,9) p1 = (139,10), p2 = (280,1) produces distanct 0.0, which is incorrect.
        /// My hypothesis is that the function is safe to use for points which are the
        /// result of rounding points which lie on the line, but not safe to use for truncated points.
        /// </summary>
        public static double ComputeEdgeDistance(ICoordinate p, ICoordinate p0, ICoordinate p1)
        {
            double dx = Math.Abs(p1.X - p0.X);
            double dy = Math.Abs(p1.Y - p0.Y);

            double dist = -1.0;   // sentinel value
            if (p.Equals(p0)) 
                dist = 0.0;            
            else if (p.Equals(p1)) 
            {
                if (dx > dy)
                     dist = dx;
                else dist = dy;
            }
            else 
            {
                double pdx = Math.Abs(p.X - p0.X);
                double pdy = Math.Abs(p.Y - p0.Y);
                if (dx > dy)
                     dist = pdx;
                else dist = pdy;

                // <FIX>: hack to ensure that non-endpoints always have a non-zero distance
                if (dist == 0.0 && ! p.Equals(p0))                
                    dist = Math.Max(pdx, pdy);
                
            }
            Assert.IsTrue(!(dist == 0.0 && ! p.Equals(p0)), "Bad distance calculation");
            return dist;
        }

        /// <summary>
        /// This function is non-robust, since it may compute the square of large numbers.
        /// Currently not sure how to improve this.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double NonRobustComputeEdgeDistance(ICoordinate p, ICoordinate p1, ICoordinate p2)
        {
            double dx = p.X - p1.X;
            double dy = p.Y - p1.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);   // dummy value
            Assert.IsTrue(! (dist == 0.0 && ! p.Equals(p1)), "Invalid distance calculation");
            return dist;
        }

        /// <summary>
        /// 
        /// </summary>
        protected int result;
        
        /// <summary>
        /// 
        /// </summary>
        protected ICoordinate[,] inputLines = new ICoordinate[2, 2];
        
        /// <summary>
        /// 
        /// </summary>
        protected ICoordinate[] intPt = new ICoordinate[2];

        /// <summary> 
        /// The indexes of the endpoints of the intersection lines, in order along
        /// the corresponding line
        /// </summary>
        protected int[,] intLineIndex;

        /// <summary>
        /// 
        /// </summary>
        protected bool isProper;
        
        /// <summary>
        /// 
        /// </summary>
        protected ICoordinate pa;
        
        /// <summary>
        /// 
        /// </summary>
        protected ICoordinate pb;

        /// <summary> 
        /// If MakePrecise is true, computed intersection coordinates will be made precise
        /// using <c>Coordinate.MakePrecise</c>.
        /// </summary>
        protected IPrecisionModel precisionModel = null;

        /// <summary>
        /// 
        /// </summary>
        public LineIntersector() 
        {
            intPt[0] = new Coordinate();
            intPt[1] = new Coordinate();
            // alias the intersection points for ease of reference
            pa = intPt[0];
            pb = intPt[1];
            result = 0;
        }

        /// <summary>
        /// Force computed intersection to be rounded to a given precision model
        /// </summary>        
        [Obsolete("Use PrecisionModel instead")]
        public IPrecisionModel MakePrecise
        {            
            set
            {
                precisionModel = value;
            }
        }

        /// <summary> 
        /// Force computed intersection to be rounded to a given precision model.
        /// No getter is provided, because the precision model is not required to be specified.
        /// </summary>
        public IPrecisionModel PrecisionModel
        {            
            set
            {
                this.precisionModel = value;
            }
        }

        /// <summary> 
        /// Compute the intersection of a point p and the line p1-p2.
        /// This function computes the bool value of the hasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of <c>p</c>.
        /// </summary>
        public abstract void ComputeIntersection(ICoordinate p, ICoordinate p1, ICoordinate p2);

        /// <summary>
        /// 
        /// </summary>
        protected bool IsCollinear 
        {
            get
            {
                return result == Collinear;
            }
        }

        /// <summary>
        /// Computes the intersection of the lines p1-p2 and p3-p4.
        /// This function computes both the bool value of the hasIntersection test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        public void ComputeIntersection(ICoordinate p1, ICoordinate p2, ICoordinate p3, ICoordinate p4) 
        {
            inputLines[0,0] = p1;
            inputLines[0,1] = p2;
            inputLines[1,0] = p3;
            inputLines[1,1] = p4;
            result = ComputeIntersect(p1, p2, p3, p4);        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public abstract int ComputeIntersect(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inputLines[0, 0]).Append("-");
            sb.Append(inputLines[0, 1]).Append(" ");
            sb.Append(inputLines[1, 0]).Append("-");
            sb.Append(inputLines[1, 1]).Append(" : ");

            if (IsEndPoint)  sb.Append(" endpoint");
            if (isProper)    sb.Append(" proper");
            if (IsCollinear) sb.Append(" collinear");

            return sb.ToString();                        
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsEndPoint 
        {
            get
            {
                return HasIntersection && !isProper;
            }
        }

        /// <summary> 
        /// Tests whether the input geometries intersect.
        /// </summary>
        /// <returns><c>true</c> if the input geometries intersect.</returns>
        public bool HasIntersection
        {
            get
            {
                return result != DontIntersect;
            }
        }

        /// <summary>
        /// Returns the number of intersection points found.  This will be either 0, 1 or 2.
        /// </summary>
        public int IntersectionNum
        {
            get 
            { 
                return result; 
            }
        }

        /// <summary> 
        /// Returns the intIndex'th intersection point.
        /// </summary>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>The intIndex'th intersection point.</returns>
        public ICoordinate GetIntersection(int intIndex)  
        { 
            return intPt[intIndex]; 
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ComputeIntLineIndex() 
        {
            if (intLineIndex == null) 
            {
                intLineIndex = new int[2, 2];
                ComputeIntLineIndex(0);
                ComputeIntLineIndex(1);
            }
        }

        /// <summary> 
        /// Test whether a point is a intersection point of two line segments.
        /// Note that if the intersection is a line segment, this method only tests for
        /// equality with the endpoints of the intersection segment.
        /// It does not return true if the input point is internal to the intersection segment.
        /// </summary>
        /// <returns><c>true</c> if the input point is one of the intersection points.</returns>
        public bool IsIntersection(ICoordinate pt) 
        {
            for (int i = 0; i < result; i++) 
                if (intPt[i].Equals2D(pt)) 
                    return true;                        
            return false;
        }

        /// <summary> 
        /// Tests whether either intersection point is an interior point of one of the input segments.
        /// </summary>
        /// <returns>
        /// <c>true</c> if either intersection point is in the interior of one of the input segment.
        /// </returns>
        public bool IsInteriorIntersection()
        {
            if (IsInteriorIntersection(0)) 
                return true;
            if (IsInteriorIntersection(1)) 
                return true;
            return false;
        }

        /// <summary>
        /// Tests whether either intersection point is an interior point of the specified input segment.
        /// </summary>
        /// <returns> 
        /// <c>true</c> if either intersection point is in the interior of the input segment.
        /// </returns>
        public bool IsInteriorIntersection(int inputLineIndex)
        {
            for (int i = 0; i < result; i++)
                if (!(intPt[i].Equals2D(inputLines[inputLineIndex, 0]) || 
                      intPt[i].Equals2D(inputLines[inputLineIndex, 1])))                                   
                    return true;                
            return false;
        }

        /// <summary>
        /// Tests whether an intersection is proper.
        /// The intersection between two line segments is considered proper if
        /// they intersect in a single point in the interior of both segments
        /// (e.g. the intersection is a single point and is not equal to any of the endpoints). 
        /// The intersection between a point and a line segment is considered proper
        /// if the point lies in the interior of the segment (e.g. is not equal to either of the endpoints).
        /// </summary>
        /// <returns><c>true</c>  if the intersection is proper.</returns>
        public bool IsProper 
        {
            get
            {
                return HasIntersection && isProper;
            }
        }

        /// <summary> 
        /// Computes the intIndex'th intersection point in the direction of
        /// a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>
        /// The intIndex'th intersection point in the direction of the specified input line segment.
        /// </returns>
        public ICoordinate GetIntersectionAlongSegment(int segmentIndex, int intIndex) 
        {
            // lazily compute int line array
            ComputeIntLineIndex();
            return intPt[intLineIndex[segmentIndex, intIndex]];
        }

        /// <summary>
        /// Computes the index of the intIndex'th intersection point in the direction of
        /// a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>
        /// The index of the intersection point along the segment (0 or 1).
        /// </returns>
        public int GetIndexAlongSegment(int segmentIndex, int intIndex) 
        {
            ComputeIntLineIndex();
            return intLineIndex[segmentIndex, intIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentIndex"></param>
        protected void ComputeIntLineIndex(int segmentIndex) 
        {
            double dist0 = GetEdgeDistance(segmentIndex, 0);
            double dist1 = GetEdgeDistance(segmentIndex, 1);
            if (dist0 > dist1) 
            {
                intLineIndex[segmentIndex, 0] = 0;
                intLineIndex[segmentIndex, 1] = 1;
            }
            else
            {
                intLineIndex[segmentIndex, 0] = 1;
                intLineIndex[segmentIndex, 1] = 0;
            }
        }

        /// <summary> 
        /// Computes the "edge distance" of an intersection point along the specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">is 0 or 1.</param>
        /// <param name="intIndex">is 0 or 1.</param>
        /// <returns>The edge distance of the intersection point.</returns>
        public double GetEdgeDistance(int segmentIndex, int intIndex) 
        {
            double dist = ComputeEdgeDistance(intPt[intIndex], inputLines[segmentIndex, 0], inputLines[segmentIndex, 1]);
            return dist;
        }
    }
}