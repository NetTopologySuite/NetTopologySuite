using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

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
    public class HotPixel<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private LineIntersector<TCoordinate> _li = null;

        private TCoordinate _pt;
        private TCoordinate _originalPt;

        private TCoordinate _p0Scaled;
        private TCoordinate _p1Scaled;

        private Double _scaleFactor;

        private Double _minx;
        private Double _maxx;
        private Double _miny;
        private Double _maxy;

        /*
         * The corners of the hot pixel, in the order:
         *  10
         *  23
         */
        private TCoordinate[] corner = new TCoordinate[4];

        private Extents<TCoordinate> safeEnv = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotPixel{TCoordinate}"/> class.
        /// </summary>
        public HotPixel(TCoordinate pt, Double scaleFactor, LineIntersector<TCoordinate> li)
        {
            _originalPt = pt;
            _pt = pt;
            _scaleFactor = scaleFactor;
            _li = li;

            if (scaleFactor != 1.0)
            {
                _pt = new TCoordinate(scale(pt[Ordinates.X]), scale(pt[Ordinates.Y]));
                _p0Scaled = new TCoordinate();
                _p1Scaled = new TCoordinate();
            }

            InitCorners(_pt);
        }

        public TCoordinate Coordinate
        {
            get { return _originalPt; }
        }

        /// <summary>
        /// Returns a "safe" envelope that is guaranteed to contain the 
        /// hot pixel.
        /// </summary>
        public IExtents<TCoordinate> GetSafeEnvelope()
        {
            if (safeEnv == null)
            {
                Double safeTolerance = 0.75 / _scaleFactor;
                safeEnv = new Extents<TCoordinate>(
                    _originalPt[Ordinates.X] - safeTolerance, _originalPt[Ordinates.X] + safeTolerance,
                    _originalPt[Ordinates.Y] - safeTolerance, _originalPt[Ordinates.Y] + safeTolerance);
            }
            return safeEnv;
        }

        private void InitCorners(ICoordinate pt)
        {
            Double tolerance = 0.5;
            _minx = pt[Ordinates.X] - tolerance;
            _maxx = pt[Ordinates.X] + tolerance;
            _miny = pt[Ordinates.Y] - tolerance;
            _maxy = pt[Ordinates.Y] + tolerance;

            corner[0] = new TCoordinate(_maxx, _maxy);
            corner[1] = new TCoordinate(_minx, _maxy);
            corner[2] = new TCoordinate(_minx, _miny);
            corner[3] = new TCoordinate(_maxx, _miny);
        }

        private Double scale(Double val)
        {
            return Math.Round(val * _scaleFactor);
        }

        public Boolean Intersects(TCoordinate p0, TCoordinate p1)
        {
            if (_scaleFactor == 1.0)
            {
                return IntersectsScaled(p0, p1);
            }

            _p0Scaled = copyScaled(p0);
            _p1Scaled = copyScaled(p1);
            return IntersectsScaled(_p0Scaled, _p1Scaled);
        }

        private TCoordinate copyScaled(TCoordinate p)
        {
            return new TCoordinate(scale(p[Ordinates.X]), scale(p[Ordinates.Y]));
        }

        public Boolean IntersectsScaled(ICoordinate p0, ICoordinate p1)
        {
            Double segMinx = Math.Min(p0[Ordinates.X], p1[Ordinates.X]);
            Double segMaxx = Math.Max(p0[Ordinates.X], p1[Ordinates.X]);
            Double segMiny = Math.Min(p0[Ordinates.Y], p1[Ordinates.Y]);
            Double segMaxy = Math.Max(p0[Ordinates.Y], p1[Ordinates.Y]);

            Boolean isOutsidePixelEnv = _maxx < segMinx || _minx > segMaxx ||
                                        _maxy < segMiny || _miny > segMaxy;
            if (isOutsidePixelEnv)
            {
                return false;
            }

            Boolean intersects = intersectsToleranceSquare(p0, p1);
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
        private Boolean intersectsToleranceSquare(TCoordinate p0, TCoordinate p1)
        {
            Boolean intersectsLeft = false;
            Boolean intersectsBottom = false;

            _li.ComputeIntersection(p0, p1, corner[0], corner[1]);

            if (_li.IsProper)
            {
                return true;
            }

            _li.ComputeIntersection(p0, p1, corner[1], corner[2]);

            if (_li.IsProper)
            {
                return true;
            }

            if (_li.HasIntersection)
            {
                intersectsLeft = true;
            }

            _li.ComputeIntersection(p0, p1, corner[2], corner[3]);

            if (_li.IsProper)
            {
                return true;
            }

            if (_li.HasIntersection)
            {
                intersectsBottom = true;
            }

            _li.ComputeIntersection(p0, p1, corner[3], corner[0]);

            if (_li.IsProper)
            {
                return true;
            }

            if (intersectsLeft && intersectsBottom)
            {
                return true;
            }

            if (p0.Equals(_pt))
            {
                return true;
            }

            if (p1.Equals(_pt))
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
        private Boolean intersectsPixelClosure(TCoordinate p0, TCoordinate p1)
        {
            _li.ComputeIntersection(p0, p1, corner[0], corner[1]);
            
            if (_li.HasIntersection)
            {
                return true;
            }

            _li.ComputeIntersection(p0, p1, corner[1], corner[2]);

            if (_li.HasIntersection)
            {
                return true;
            }

            _li.ComputeIntersection(p0, p1, corner[2], corner[3]);

            if (_li.HasIntersection)
            {
                return true;
            }

            _li.ComputeIntersection(p0, p1, corner[3], corner[0]);

            if (_li.HasIntersection)
            {
                return true;
            }

            return false;
        }
    }
}