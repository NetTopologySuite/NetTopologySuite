using System;
using GeoAPI.Geometries;
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
        private readonly IGeometry inputGeom;
        private readonly bool isConvex;

        private LineSegment minBaseSeg = new LineSegment();
        private ICoordinate minWidthPt = null;
        private int minPtIndex;
        private double minWidth = 0.0;

        /// <summary> 
        /// Compute a minimum diameter for a giver <c>Geometry</c>.
        /// </summary>
        /// <param name="inputGeom">a Geometry.</param>
        public MinimumDiameter(IGeometry inputGeom) 
            : this(inputGeom, false) { }

        /// <summary> 
        /// Compute a minimum diameter for a giver <c>Geometry</c>,
        /// with a hint if
        /// the Geometry is convex
        /// (e.g. a convex Polygon or LinearRing,
        /// or a two-point LineString, or a Point).
        /// </summary>
        /// <param name="inputGeom">a Geometry which is convex.</param>
        /// <param name="isConvex"><c>true</c> if the input point is convex.</param>
        public MinimumDiameter(IGeometry inputGeom, bool isConvex)
        {
            this.inputGeom = inputGeom;
            this.isConvex = isConvex;
        }

        /// <summary> 
        /// Gets the length of the minimum diameter of the input Geometry.
        /// </summary>
        /// <returns>The length of the minimum diameter.</returns>
        public double Length
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
        public ICoordinate WidthCoordinate
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
        public ILineString SupportingSegment
        {
            get
            {
                ComputeMinimumDiameter();
                return inputGeom.Factory.CreateLineString(new ICoordinate[] { minBaseSeg.P0, minBaseSeg.P1 });
            }
        }

        /// <summary>
        /// Gets a <c>LineString</c> which is a minimum diameter.
        /// </summary>
        /// <returns>A <c>LineString</c> which is a minimum diameter.</returns>
        public ILineString Diameter
        {
            get
            {
                ComputeMinimumDiameter();

                // return empty linearRing if no minimum width calculated
                if (minWidthPt == null)
                {
                    ICoordinate[] nullCoords = null;
                    return inputGeom.Factory.CreateLineString(nullCoords);
                }

                ICoordinate basePt = minBaseSeg.Project(minWidthPt);
                return inputGeom.Factory.CreateLineString(new ICoordinate[] { basePt, minWidthPt });
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
                IGeometry convexGeom = (new ConvexHull(inputGeom)).GetConvexHull();
                ComputeWidthConvex(convexGeom);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        private void ComputeWidthConvex(IGeometry geom)
        {
            ICoordinate[] pts = null;
            if (geom is IPolygon)
                 pts = ((IPolygon) geom).ExteriorRing.Coordinates;
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
        private void ComputeConvexRingMinDiameter(ICoordinate[] pts)
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
        private int FindMaxPerpDistance(ICoordinate[] pts, LineSegment seg, int startIndex)
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
        private static int NextIndex(ICoordinate[] pts, int index)
        {
            index++;
            if (index >= pts.Length) index = 0;
            return index;
        }
    }
}
