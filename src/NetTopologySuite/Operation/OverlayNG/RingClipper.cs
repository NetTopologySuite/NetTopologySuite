using System;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Clips a ring of points to an rectangle.
    /// Uses a variant of Cohen-Sutherland clipping.
    /// <para/>
    /// In general the output is not topologically valid.
    /// In particular, the output may contain coincident non-noded line segments
    /// along the clip rectangle sides.
    /// However, the output is sufficiently well-structured
    /// that it can be used as input to the <see cref="OverlayNG"/> algorithm
    /// (which is able to process coincident linework due
    /// to the need to handle topology collapse under precision reduction).
    /// <para/>
    /// Because of the likelihood of creating
    /// extraneous line segments along the clipping rectangle sides,
    /// this class is not suitable for clipping linestrings.
    /// <para/>
    /// The clipping envelope should be generated using <see cref="RobustClipEnvelopeComputer"/>,
    /// to ensure that intersecting line segments are not perturbed
    /// by clipping.
    /// This is required to ensure that the overlay of the
    /// clipped geometry is robust and correct (i.e. the same as
    /// if clipping was not used).
    /// </summary>
    /// <seealso cref="LineLimiter"/>
    /// <author>Martin Davis</author>
    public sealed class RingClipper
    {

        private const int BoxLeft = 3;
        private const int BoxTop = 2;
        private const int BoxRight = 1;
        private const int BoxBottom = 0;

        //private Envelope clipEnv;
        private readonly double _clipEnvMinY;
        private readonly double _clipEnvMaxY;
        private readonly double _clipEnvMinX;
        private readonly double _clipEnvMaxX;

        /// <summary>
        /// Creates a new clipper for the given envelope.
        /// </summary>
        /// <param name="clipEnv">The clipping envelope</param>
        public RingClipper(Envelope clipEnv)
        {
            if (clipEnv == null)
            {
                throw new ArgumentNullException(nameof(clipEnv));
            }

            //this.clipEnv = clipEnv;
            _clipEnvMinY = clipEnv.MinY;
            _clipEnvMaxY = clipEnv.MaxY;
            _clipEnvMinX = clipEnv.MinX;
            _clipEnvMaxX = clipEnv.MaxX;
        }

        /// <summary>
        /// Clips a list of points to the clipping rectangle box.
        /// </summary>
        /// <param name="pts">The points of the ring</param>
        /// <returns>The points of the clipped ring</returns>
        public Coordinate[] Clip(Coordinate[] pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException(nameof(pts));
            }

            for (int edgeIndex = 0; edgeIndex < 4; edgeIndex++)
            {
                bool closeRing = edgeIndex == 3;
                pts = ClipToBoxEdge(pts, edgeIndex, closeRing);
                if (pts.Length == 0) return pts;
            }
            return pts;
        }

        /// <summary>
        /// Clips line to the axis-parallel line defined by a single box edge.
        /// </summary>
        /// <param name="pts">The coordinates</param>
        /// <param name="edgeIndex">An edge index</param>
        /// <param name="closeRing">A flag indicating whether to close the ring.</param>
        /// <returns>The clipped coordinates</returns>
        private Coordinate[] ClipToBoxEdge(Coordinate[] pts, int edgeIndex, bool closeRing)
        {
            // TODO: is it possible to avoid copying array 4 times?
            var ptsClip = new CoordinateList(pts.Length);

            var p0 = pts[pts.Length - 1];
            for (int i = 0; i < pts.Length; i++)
            {
                var p1 = pts[i];
                if (IsInsideEdge(p1, edgeIndex))
                {
                    if (!IsInsideEdge(p0, edgeIndex))
                    {
                        var intPt = Intersection(p0, p1, edgeIndex);
                        ptsClip.Add(intPt, false);
                    }
                    // TODO: avoid copying so much?
                    ptsClip.Add(p1.Copy(), false);

                }
                else if (IsInsideEdge(p0, edgeIndex))
                {
                    var intPt = Intersection(p0, p1, edgeIndex);
                    ptsClip.Add(intPt, false);
                }
                // else p0-p1 is outside box, so it is dropped

                p0 = p1;
            }

            // add closing point if required
            if (closeRing && ptsClip.Count > 0)
            {
                var start = ptsClip[0];
                if (!start.Equals2D(ptsClip[ptsClip.Count - 1]))
                {
                    ptsClip.Add(start.Copy());
                }
            }
            return ptsClip.ToCoordinateArray();
        }

        /// <summary>
        /// Computes the intersection point of a segment
        /// with an edge of the clip box.
        /// The segment must be known to intersect the edge.
        /// </summary>
        /// <param name="a">First endpoint of the segment</param>
        /// <param name="b">Second endpoint of the segment</param>
        /// <param name="edgeIndex">Index of box edge</param>
        /// <returns>
        /// The intersection point with the box edge
        /// </returns>
        private Coordinate Intersection(Coordinate a, Coordinate b, int edgeIndex)
        {
            Coordinate intPt;
            switch (edgeIndex)
            {
                case BoxBottom:
                    intPt = new Coordinate(IntersectionLineY(a, b, _clipEnvMinY), _clipEnvMinY);
                    break;
                case BoxRight:
                    intPt = new Coordinate(_clipEnvMaxX, IntersectionLineX(a, b, _clipEnvMaxX));
                    break;
                case BoxTop:
                    intPt = new Coordinate(IntersectionLineY(a, b, _clipEnvMaxY), _clipEnvMaxY);
                    break;
                case BoxLeft:
                default:
                    intPt = new Coordinate(_clipEnvMinX, IntersectionLineX(a, b, _clipEnvMinX));
                    break;
            }
            return intPt;
        }

        private static double IntersectionLineY(Coordinate a, Coordinate b, double y)
        {
            double m = (b.X - a.X) / (b.Y - a.Y);
            double intercept = (y - a.Y) * m;
            return a.X + intercept;
        }

        private static double IntersectionLineX(Coordinate a, Coordinate b, double x)
        {
            double m = (b.Y - a.Y) / (b.X - a.X);
            double intercept = (x - a.X) * m;
            return a.Y + intercept;
        }

        private bool IsInsideEdge(Coordinate p, int edgeIndex)
        {
            //bool isInside = false;
            switch (edgeIndex)
            {
                case BoxBottom: // bottom
                    return /*isInside =*/ p.Y > _clipEnvMinY;
                    //break;
                case BoxRight: // right
                    return /*isInside =*/ p.X < _clipEnvMaxX;
                    //break;
                case BoxTop: // top
                    return /*isInside =*/ p.Y < _clipEnvMaxY;
                    //break;
                case BoxLeft:
                default: // left
                    return /*isInside =*/ p.X > _clipEnvMinX;
                    //break;
            }
            //return isInside;
        }

    }
}
