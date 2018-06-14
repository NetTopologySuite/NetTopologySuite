using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
//using NetTopologySuite.IO;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Implements a "hot pixel" as used in the Snap Rounding algorithm.
    /// A hot pixel contains the interior of the tolerance square and the boundary
    /// minus the top and right segments.
    /// The hot pixel operations are all computed in the integer domain
    /// to avoid rounding problems.
    /// </summary>
    public class HotPixel
    {
        private readonly LineIntersector _li;

        private readonly Coordinate _pt;
        private readonly Coordinate _originalPt;

        private readonly Coordinate _p0Scaled;
        private readonly Coordinate _p1Scaled;

        private readonly double _scaleFactor;

        private double _minx;
        private double _maxx;
        private double _miny;
        private double _maxy;

        /*
         * The corners of the hot pixel, in the order:
         *  10
         *  23
         */
        private readonly Coordinate[] _corner = new Coordinate[4];

        private Envelope _safeEnv;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotPixel"/> class.
        /// </summary>
        /// <param name="pt">The coordinate at the center of the hot pixel</param>
        /// <param name="scaleFactor">The scale factor determining the pixel size</param>
        /// <param name="li">THe intersector to use for testing intersection with line segments</param>
        public HotPixel(Coordinate pt, double scaleFactor, LineIntersector li)
        {
            _originalPt = pt;
            _pt = pt;
            _scaleFactor = scaleFactor;
            _li = li;

            if (scaleFactor <= 0d)
                throw new ArgumentException("Scale factor must be non-zero");

            if (scaleFactor != 1.0)
            {
                _pt = new Coordinate(Scale(pt.X), Scale(pt.Y));
                _p0Scaled = new Coordinate();
                _p1Scaled = new Coordinate();
            }
            InitCorners(_pt);
        }

        /// <summary>
        /// Gets the coordinate this hot pixel is based at.
        /// </summary>
        public Coordinate Coordinate => _originalPt;

        private const double SafeEnvelopeExpansionFactor = 0.75d;

        /// <summary>
        /// Returns a "safe" envelope that is guaranteed to contain the hot pixel.
        /// The envelope returned will be larger than the exact envelope of the pixel.
        /// </summary>
        /// <returns>An envelope which contains the pixel</returns>
        public Envelope GetSafeEnvelope()
        {
            if (_safeEnv == null)
            {
                double safeTolerance = SafeEnvelopeExpansionFactor / _scaleFactor;
                _safeEnv = new Envelope(_originalPt.X - safeTolerance, _originalPt.X + safeTolerance,
                                       _originalPt.Y - safeTolerance, _originalPt.Y + safeTolerance);
            }
            return _safeEnv;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pt"></param>
        private void InitCorners(Coordinate pt)
        {
            const double tolerance = 0.5;
            _minx = pt.X - tolerance;
            _maxx = pt.X + tolerance;
            _miny = pt.Y - tolerance;
            _maxy = pt.Y + tolerance;

            _corner[0] = new Coordinate(_maxx, _maxy);
            _corner[1] = new Coordinate(_minx, _maxy);
            _corner[2] = new Coordinate(_minx, _miny);
            _corner[3] = new Coordinate(_maxx, _miny);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private double Scale(double val)
        {
            return Math.Round(val * _scaleFactor);
        }

        /// <summary>
        /// Tests whether the line segment (p0-p1)
        /// intersects this hot pixel.
        /// </summary>
        /// <param name="p0">The first coordinate of the line segment to test</param>
        /// <param name="p1">The second coordinate of the line segment to test</param>
        /// <returns>true if the line segment intersects this hot pixel.</returns>
        public bool Intersects(Coordinate p0, Coordinate p1)
        {
            if (_scaleFactor == 1.0)
                return IntersectsScaled(p0, p1);

            CopyScaled(p0, _p0Scaled);
            CopyScaled(p1, _p1Scaled);
            return IntersectsScaled(_p0Scaled, _p1Scaled);
        }

        /// <summary>
        /// Tests whether the line segment (p0-p1)
        /// intersects this hot pixel.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="pScaled"></param>
        private void CopyScaled(Coordinate p, Coordinate pScaled)
        {
            pScaled.X = Scale(p.X);
            pScaled.Y = Scale(p.Y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public bool IntersectsScaled(Coordinate p0, Coordinate p1)
        {
            double segMinx = Math.Min(p0.X, p1.X);
            double segMaxx = Math.Max(p0.X, p1.X);
            double segMiny = Math.Min(p0.Y, p1.Y);
            double segMaxy = Math.Max(p0.Y, p1.Y);

            bool isOutsidePixelEnv = _maxx < segMinx || _minx > segMaxx ||
                                     _maxy < segMiny || _miny > segMaxy;
            if (isOutsidePixelEnv)
                return false;
            bool intersects = IntersectsToleranceSquare(p0, p1);

            //Assert.IsTrue(!(isOutsidePixelEnv && intersects), "Found bad envelope test");
            return intersects;
        }

        /// <summary>
        /// Tests whether the segment p0-p1 intersects the hot pixel tolerance square.
        /// Because the tolerance square point set is partially open (along the
        /// top and right) the test needs to be more sophisticated than
        /// simply checking for any intersection.
        /// However, it can take advantage of the fact that the hot pixel edges
        /// do not lie on the coordinate grid.
        /// It is sufficient to check if any of the following occur:
        ///  - a proper intersection between the segment and any hot pixel edge.
        ///  - an intersection between the segment and BOTH the left and bottom hot pixel edges
        /// (which detects the case where the segment intersects the bottom left hot pixel corner).
        ///  - an intersection between a segment endpoint and the hot pixel coordinate.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        private bool IntersectsToleranceSquare(Coordinate p0, Coordinate p1)
        {
            bool intersectsLeft = false;
            bool intersectsBottom = false;
            //Console.WriteLine("Hot Pixel: " + WKTWriter.ToLineString(corner));
            //Console.WriteLine("Line: " + WKTWriter.ToLineString(p0, p1));

            _li.ComputeIntersection(p0, p1, _corner[0], _corner[1]);
            if (_li.IsProper) return true;

            _li.ComputeIntersection(p0, p1, _corner[1], _corner[2]);
            if (_li.IsProper) return true;
            if (_li.HasIntersection) intersectsLeft = true;

            _li.ComputeIntersection(p0, p1, _corner[2], _corner[3]);
            if (_li.IsProper) return true;
            if (_li.HasIntersection) intersectsBottom = true;

            _li.ComputeIntersection(p0, p1, _corner[3], _corner[0]);
            if (_li.IsProper) return true;

            if (intersectsLeft && intersectsBottom) return true;

            if (p0.Equals(_pt)) return true;
            if (p1.Equals(_pt)) return true;

            return false;
        }

        /// <summary>
        /// Test whether the given segment intersects
        /// the closure of this hot pixel.
        /// This is NOT the test used in the standard snap-rounding
        /// algorithm, which uses the partially closed tolerance square instead.
        /// This routine is provided for testing purposes only.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        private bool IntersectsPixelClosure(Coordinate p0, Coordinate p1)
        {
            _li.ComputeIntersection(p0, p1, _corner[0], _corner[1]);
            if (_li.HasIntersection) return true;
            _li.ComputeIntersection(p0, p1, _corner[1], _corner[2]);
            if (_li.HasIntersection) return true;
            _li.ComputeIntersection(p0, p1, _corner[2], _corner[3]);
            if (_li.HasIntersection) return true;
            _li.ComputeIntersection(p0, p1, _corner[3], _corner[0]);
            if (_li.HasIntersection) return true;
            return false;
        }

        ///<summary>
        /// Adds a new node (equal to the snap pt) to the specified segment
        /// if the segment passes through the hot pixel
        ///</summary>
        /// <param name="segStr"></param>
        /// <param name="segIndex"></param>
        /// <returns><c>true</c> if a node was added to the segment</returns>
        public bool AddSnappedNode(INodableSegmentString segStr, int segIndex)
        {
            var coords = segStr.Coordinates;
            var p0 = coords[segIndex];
            var p1 = coords[segIndex + 1];

            if (Intersects(p0, p1))
            {
                //System.out.println("snapped: " + snapPt);
                //System.out.println("POINT (" + snapPt.x + " " + snapPt.y + ")");
                segStr.AddIntersection(Coordinate, segIndex);

                return true;
            }
            return false;
        }
    }
}