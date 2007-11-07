using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Implements a "hot pixel" as used in the Snap Rounding algorithm.
    /// </summary>
    /// <remarks>
    /// A hot pixel contains the interior of the tolerance square and the boundary
    /// minus the top and right segments.
    /// The hot pixel operations are all computed in the integer domain
    /// to avoid rounding problems.
    /// </remarks>
    public class HotPixel
    {
        private LineIntersector li = null;

        private ICoordinate pt = null;
        private ICoordinate originalPt = null;

        private ICoordinate p0Scaled = null;
        private ICoordinate p1Scaled = null;

        private Double scaleFactor;

        private Double minx;
        private Double maxx;
        private Double miny;
        private Double maxy;

        /*
         * The corners of the hot pixel, in the order:
         *  10
         *  23
         */
        private ICoordinate[] corner = new ICoordinate[4];

        private Extents safeEnv = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotPixel"/> class.
        /// </summary>
        public HotPixel(ICoordinate pt, Double scaleFactor, LineIntersector li)
        {
            originalPt = pt;
            this.pt = pt;
            this.scaleFactor = scaleFactor;
            this.li = li;

            if (scaleFactor != 1.0)
            {
                this.pt = new Coordinate(Scale(pt.X), Scale(pt.Y));
                p0Scaled = new Coordinate();
                p1Scaled = new Coordinate();
            }

            InitCorners(this.pt);
        }

        public ICoordinate Coordinate
        {
            get { return originalPt; }
        }

        /// <summary>
        /// Returns a "safe" envelope that is guaranteed to contain the 
        /// hot pixel.
        /// </summary>
        public IExtents GetSafeEnvelope()
        {
            if (safeEnv == null)
            {
                Double safeTolerance = 0.75 / scaleFactor;
                safeEnv = new Extents(originalPt.X - safeTolerance, originalPt.X + safeTolerance,
                                       originalPt.Y - safeTolerance, originalPt.Y + safeTolerance);
            }
            return safeEnv;
        }

        private void InitCorners(ICoordinate pt)
        {
            Double tolerance = 0.5;
            minx = pt.X - tolerance;
            maxx = pt.X + tolerance;
            miny = pt.Y - tolerance;
            maxy = pt.Y + tolerance;

            corner[0] = new Coordinate(maxx, maxy);
            corner[1] = new Coordinate(minx, maxy);
            corner[2] = new Coordinate(minx, miny);
            corner[3] = new Coordinate(maxx, miny);
        }

        private Double Scale(Double val)
        {
            return Math.Round(val * scaleFactor);
        }

        public Boolean Intersects(ICoordinate p0, ICoordinate p1)
        {
            if (scaleFactor == 1.0)
            {
                return IntersectsScaled(p0, p1);
            }

            CopyScaled(p0, p0Scaled);
            CopyScaled(p1, p1Scaled);
            return IntersectsScaled(p0Scaled, p1Scaled);
        }

        private void CopyScaled(ICoordinate p, ICoordinate pScaled)
        {
            pScaled.X = Scale(p.X);
            pScaled.Y = Scale(p.Y);
        }

        public Boolean IntersectsScaled(ICoordinate p0, ICoordinate p1)
        {
            Double segMinx = Math.Min(p0.X, p1.X);
            Double segMaxx = Math.Max(p0.X, p1.X);
            Double segMiny = Math.Min(p0.Y, p1.Y);
            Double segMaxy = Math.Max(p0.Y, p1.Y);

            Boolean isOutsidePixelEnv = maxx < segMinx || minx > segMaxx ||
                                        maxy < segMiny || miny > segMaxy;
            if (isOutsidePixelEnv)
            {
                return false;
            }

            Boolean intersects = IntersectsToleranceSquare(p0, p1);
            Assert.IsTrue(!(isOutsidePixelEnv && intersects), "Found bad envelope test");
            return intersects;
        }

        // Tests whether the segment p0-p1 intersects the hot pixel tolerance square.
        // Because the tolerance square point set is partially open (along the
        // top and right) the test needs to be more sophisticated than
        // simply checking for any intersection.  However, it
        // can take advantage of the fact that because the hot pixel edges
        // do not lie on the coordinate grid.  It is sufficient to check
        // if there is at least one of:
        //  - a proper intersection with the segment and any hot pixel edge.
        //  - an intersection between the segment and both the left and bottom edges.
        //  - an intersection between a segment endpoint and the hot pixel coordinate.
        private Boolean IntersectsToleranceSquare(ICoordinate p0, ICoordinate p1)
        {
            Boolean intersectsLeft = false;
            Boolean intersectsBottom = false;

            li.ComputeIntersection(p0, p1, corner[0], corner[1]);

            if (li.IsProper)
            {
                return true;
            }

            li.ComputeIntersection(p0, p1, corner[1], corner[2]);

            if (li.IsProper)
            {
                return true;
            }

            if (li.HasIntersection)
            {
                intersectsLeft = true;
            }

            li.ComputeIntersection(p0, p1, corner[2], corner[3]);

            if (li.IsProper)
            {
                return true;
            }

            if (li.HasIntersection)
            {
                intersectsBottom = true;
            }

            li.ComputeIntersection(p0, p1, corner[3], corner[0]);

            if (li.IsProper)
            {
                return true;
            }

            if (intersectsLeft && intersectsBottom)
            {
                return true;
            }

            if (p0.Equals(pt))
            {
                return true;
            }

            if (p1.Equals(pt))
            {
                return true;
            }

            return false;
        }

        // Test whether the given segment intersects
        // the closure of this hot pixel.
        // This is NOT the test used in the standard snap-rounding
        // algorithm, which uses the partially closed tolerance square instead.
        // This routine is provided for testing purposes only.
        private Boolean IntersectsPixelClosure(ICoordinate p0, ICoordinate p1)
        {
            li.ComputeIntersection(p0, p1, corner[0], corner[1]);
            
            if (li.HasIntersection)
            {
                return true;
            }

            li.ComputeIntersection(p0, p1, corner[1], corner[2]);

            if (li.HasIntersection)
            {
                return true;
            }

            li.ComputeIntersection(p0, p1, corner[2], corner[3]);

            if (li.HasIntersection)
            {
                return true;
            }

            li.ComputeIntersection(p0, p1, corner[3], corner[0]);

            if (li.HasIntersection)
            {
                return true;
            }

            return false;
        }
    }
}