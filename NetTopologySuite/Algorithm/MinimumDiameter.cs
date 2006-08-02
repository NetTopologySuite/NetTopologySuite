using System;
using System.Collections;
using System.Text;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the minimum diameter of a <c>Geometry</c>.
    /// The minimum diameter is defined to be the
    /// width of the smallest band that contains the point,
    /// where a band is a strip of the plane defined
    /// by two parallel lines.
    /// This can be thought of as the smallest hole that the point can be
    /// moved through, with a single rotation.
    /// The first step in the algorithm is computing the convex hull of the Geometry.
    /// If the input Geometry is known to be convex, a hint can be supplied to
    /// avoid this computation.
    /// </summary>
    public class MinimumDiameter
    {
        private readonly Geometry inputGeom;
        private readonly bool isConvex;

        private LineSegment minBaseSeg = new LineSegment();
        private Coordinate minWidthPt = null;
        private int minPtIndex;
        private double minWidth = 0.0;

        /// <summary> 
        /// Compute a minimum diameter for a giver <c>Geometry</c>.
        /// </summary>
        /// <param name="inputGeom">a Geometry.</param>
        public MinimumDiameter(Geometry inputGeom) : this(inputGeom, false) { }

        /// <summary> 
        /// Compute a minimum diameter for a giver <c>Geometry</c>,
        /// with a hint if
        /// the Geometry is convex
        /// (e.g. a convex Polygon or LinearRing,
        /// or a two-point LineString, or a Point).
        /// </summary>
        /// <param name="inputGeom">a Geometry which is convex.</param>
        /// <param name="isConvex"><c>true</c> if the input point is convex.</param>
        public MinimumDiameter(Geometry inputGeom, bool isConvex)
        {
            this.inputGeom = inputGeom;
            this.isConvex = isConvex;
        }

        /// <summary> 
        /// Gets the length of the minimum diameter of the input Geometry.
        /// </summary>
        /// <returns>The length of the minimum diameter.</returns>
        public virtual double Length
        {
            get
            {
                ComputeMinimumDiameter();
                return minWidth;
            }
        }

        /// <summary>
        /// Gets the <c>Coordinate</c> forming one end of the minimum diameter.
        /// </summary>
        /// <returns>A coordinate forming one end of the minimum diameter.</returns>
        public virtual Coordinate WidthCoordinate
        {
            get
            {
                ComputeMinimumDiameter();
                return minWidthPt;
            }
        }

        /// <summary>
        /// Gets the segment forming the base of the minimum diameter.
        /// </summary>
        /// <returns>The segment forming the base of the minimum diameter.</returns>
        public virtual LineString SupportingSegment
        {
            get
            {
                ComputeMinimumDiameter();
                return inputGeom.Factory.CreateLineString(new Coordinate[] { minBaseSeg.P0, minBaseSeg.P1 });
            }
        }

        /// <summary>
        /// Gets a <c>LineString</c> which is a minimum diameter.
        /// </summary>
        /// <returns>A <c>LineString</c> which is a minimum diameter.</returns>
        public virtual LineString Diameter
        {
            get
            {
                ComputeMinimumDiameter();

                // return empty linearRing if no minimum width calculated
                if (minWidthPt == null)
                    return inputGeom.Factory.CreateLineString((Coordinate[])null);

                Coordinate basePt = minBaseSeg.Project(minWidthPt);
                return inputGeom.Factory.CreateLineString(new Coordinate[] { basePt, minWidthPt });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeMinimumDiameter()
        {
            // check if computation is cached
            if (minWidthPt != null)
                return;

            if (isConvex) ComputeWidthConvex(inputGeom);
            else
            {
                Geometry convexGeom = (new ConvexHull(inputGeom)).GetConvexHull();
                ComputeWidthConvex(convexGeom);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        private void ComputeWidthConvex(Geometry geom)
        {
            Coordinate[] pts = null;
            if (geom is Polygon)
                pts = ((Polygon)geom).ExteriorRing.Coordinates;
            else pts = geom.Coordinates;

            // special cases for lines or points or degenerate rings
            if (pts.Length == 0) 
            {
                minWidth = 0.0;
                minWidthPt = null;
                minBaseSeg = null;
            }
            else if (pts.Length == 1) 
            {
                minWidth = 0.0;
                minWidthPt = pts[0];
                minBaseSeg.P0 = pts[0];
                minBaseSeg.P1 = pts[0];
            }
            else if (pts.Length == 2 || pts.Length == 3) 
            {
                minWidth = 0.0;
                minWidthPt = pts[0];
                minBaseSeg.P0 = pts[0];
                minBaseSeg.P1 = pts[1];
            }
            else ComputeConvexRingMinDiameter(pts);
        }

        /// <summary> 
        /// Compute the width information for a ring of <c>Coordinate</c>s.
        /// Leaves the width information in the instance variables.
        /// </summary>
        /// <param name="pts"></param>
        private void ComputeConvexRingMinDiameter(Coordinate[] pts)
        {
            // for each segment in the ring
            minWidth = Double.MaxValue;
            int currMaxIndex = 1;

            LineSegment seg = new LineSegment();
            // compute the max distance for all segments in the ring, and pick the minimum
            for (int i = 0; i < pts.Length - 1; i++) 
            {
                seg.P0 = pts[i];
                seg.P1 = pts[i + 1];
                currMaxIndex = FindMaxPerpDistance(pts, seg, currMaxIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="seg"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private int FindMaxPerpDistance(Coordinate[] pts, LineSegment seg, int startIndex)
        {
            double maxPerpDistance = seg.DistancePerpendicular(pts[startIndex]);
            double nextPerpDistance = maxPerpDistance;
            int maxIndex = startIndex;
            int nextIndex = maxIndex;
            while (nextPerpDistance >= maxPerpDistance) 
            {
                maxPerpDistance = nextPerpDistance;
                maxIndex = nextIndex;

                nextIndex = NextIndex(pts, maxIndex);
                nextPerpDistance = seg.DistancePerpendicular(pts[nextIndex]);
            }

            // found maximum width for this segment - update global min dist if appropriate
            if (maxPerpDistance < minWidth) 
            {
                minPtIndex = maxIndex;
                minWidth = maxPerpDistance;
                minWidthPt = pts[minPtIndex];
                minBaseSeg = new LineSegment(seg);        
            }
            return maxIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static int NextIndex(Coordinate[] pts, int index)
        {
            index++;
            if (index >= pts.Length) index = 0;
            return index;
        }
    }
}
