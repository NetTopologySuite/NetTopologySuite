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
    public struct HotPixel<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li;

        private readonly TCoordinate _pt;
        private readonly TCoordinate _originalPt;

        //private readonly TCoordinate _p0Scaled;
        //private readonly TCoordinate _p1Scaled;

        private readonly Double _scaleFactor;

        private readonly Double _minx;
        private readonly Double _maxx;
        private readonly Double _miny;
        private readonly Double _maxy;

        /*
         * The corners of the hot pixel, in the order:
         *  10
         *  23
         */
        private readonly TCoordinate _corner0;
        private readonly TCoordinate _corner1;
        private readonly TCoordinate _corner2;
        private readonly TCoordinate _corner3;

        private Extents<TCoordinate> _safeExtents;
        private readonly ICoordinateFactory<TCoordinate> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotPixel{TCoordinate}"/> class.
        /// </summary>
        public HotPixel(TCoordinate pt, Double scaleFactor, 
            LineIntersector<TCoordinate> li, ICoordinateFactory<TCoordinate> factory)
        {
            _originalPt = pt;
            _pt = pt;
            _scaleFactor = scaleFactor;
            _li = li;
            _minx = _miny = _maxx = _maxy = 0;
            _corner0 = _corner1 = _corner2 = _corner3 = default(TCoordinate);
            _safeExtents = null;
            _factory = factory;

            if (scaleFactor != 1.0)
            {
                _pt = _factory.Create(scale(pt[Ordinates.X]), scale(pt[Ordinates.Y]));
                //_p0Scaled = new TCoordinate();
                //_p1Scaled = new TCoordinate();
            }

            Double tolerance = 0.5;
            _minx = pt[Ordinates.X] - tolerance;
            _maxx = pt[Ordinates.X] + tolerance;
            _miny = pt[Ordinates.Y] - tolerance;
            _maxy = pt[Ordinates.Y] + tolerance;

            _corner0 = _factory.Create(_maxx, _maxy);
            _corner1 = _factory.Create(_minx, _maxy);
            _corner2 = _factory.Create(_minx, _miny);
            _corner3 = _factory.Create(_maxx, _miny);
        }

        public TCoordinate Coordinate
        {
            get { return _originalPt; }
        }

        /// <summary>
        /// Returns a "safe" envelope that is guaranteed to contain the 
        /// hot pixel.
        /// </summary>
        public IExtents<TCoordinate> GetSafeExtents(IGeometryFactory<TCoordinate> geoFactory)
        {
            if (_safeExtents == null)
            {
                Double safeTolerance = 0.75 / _scaleFactor;

                _safeExtents = new Extents<TCoordinate>(
                    geoFactory,
                    _originalPt[Ordinates.X] - safeTolerance, 
                    _originalPt[Ordinates.X] + safeTolerance,
                    _originalPt[Ordinates.Y] - safeTolerance, 
                    _originalPt[Ordinates.Y] + safeTolerance);
            }

            return _safeExtents;
        }

        public Boolean Intersects(LineSegment<TCoordinate> segment)
        {
            return Intersects(segment.P0, segment.P1);
        }

        public Boolean Intersects(TCoordinate p0, TCoordinate p1)
        {
            if (_scaleFactor == 1.0)
            {
                return IntersectsScaled(p0, p1);
            }

            TCoordinate p0Scaled = copyScaled(p0);
            TCoordinate p1Scaled = copyScaled(p1);
            return IntersectsScaled(p0Scaled, p1Scaled);
        }

        public Boolean IntersectsScaled(TCoordinate p0, TCoordinate p1)
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

        private Double scale(Double val)
        {
            return Math.Round(val * _scaleFactor);
        }

        private TCoordinate copyScaled(TCoordinate p)
        {
            return _factory.Create(scale(p[Ordinates.X]), scale(p[Ordinates.Y]));
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

            Intersection<TCoordinate> intersection;

            intersection = _li.ComputeIntersection(p0, p1, _corner0, _corner1);

            if (intersection.IsProper)
            {
                return true;
            }

            intersection = _li.ComputeIntersection(p0, p1, _corner1, _corner2);

            if (intersection.IsProper)
            {
                return true;
            }

            if (intersection.HasIntersection)
            {
                intersectsLeft = true;
            }

            intersection = _li.ComputeIntersection(p0, p1, _corner2, _corner3);

            if (intersection.IsProper)
            {
                return true;
            }

            if (intersection.HasIntersection)
            {
                intersectsBottom = true;
            }

            intersection = _li.ComputeIntersection(p0, p1, _corner3, _corner0);

            if (intersection.IsProper)
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
            Intersection<TCoordinate> intersection;

            intersection = _li.ComputeIntersection(p0, p1, _corner0, _corner1);

            if (intersection.HasIntersection)
            {
                return true;
            }

            intersection = _li.ComputeIntersection(p0, p1, _corner1, _corner2);

            if (intersection.HasIntersection)
            {
                return true;
            }

            intersection = _li.ComputeIntersection(p0, p1, _corner2, _corner3);

            if (intersection.HasIntersection)
            {
                return true;
            }

            intersection = _li.ComputeIntersection(p0, p1, _corner3, _corner0);

            if (intersection.HasIntersection)
            {
                return true;
            }

            return false;
        }
    }
}