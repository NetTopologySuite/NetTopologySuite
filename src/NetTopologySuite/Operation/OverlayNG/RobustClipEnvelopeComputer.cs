using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Computes a robust clipping envelope for a pair of polygonal geometries.
    /// The envelope is computed to be large enough to include the full
    /// length of all geometry line segments which intersect
    /// a given target envelope.
    /// This ensures that line segments which might intersect are
    /// not perturbed when clipped using <see cref="RingClipper"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class RobustClipEnvelopeComputer
    {

        public static Envelope GetEnvelope(Geometry a, Geometry b, Envelope targetEnv)
        {
            var cec = new RobustClipEnvelopeComputer(targetEnv);
            cec.Add(a);
            cec.Add(b);
            return cec.Envelope;
        }

        private readonly Envelope _targetEnv;
        private readonly Envelope _clipEnv;

        public RobustClipEnvelopeComputer(Envelope targetEnv)
        {
            _targetEnv = targetEnv;
            _clipEnv = targetEnv.Copy();
        }

        public Envelope Envelope => _clipEnv;

        public void Add(Geometry g)
        {
            if (g == null || g.IsEmpty)
                return;

            if (g is Polygon p)
                AddPolygon(p);
            else if (g is GeometryCollection gc)
                AddCollection(gc);
        }

        private void AddCollection(GeometryCollection gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                Add(g);
            }
        }

        private void AddPolygon(Polygon poly)
        {
            var shell = poly.ExteriorRing;
            AddPolygonRing(shell);

            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = poly.GetInteriorRingN(i);
                AddPolygonRing(hole);
            }
        }

        /// <summary>
        /// Adds a polygon ring to the graph. Empty rings are ignored.
        /// </summary>
        private void AddPolygonRing(LineString ring)
        {
            // don't add empty lines
            if (ring.IsEmpty)
                return;

            var seq = ring.CoordinateSequence;
            for (int i = 1; i < seq.Count; i++)
            {
                AddSegment(seq.GetCoordinate(i - 1), seq.GetCoordinate(i));
            }
        }

        private void AddSegment(Coordinate p1, Coordinate p2)
        {
            if (IntersectsSegment(_targetEnv, p1, p2))
            {
                _clipEnv.ExpandToInclude(p1);
                _clipEnv.ExpandToInclude(p2);
            }
        }

        private static bool IntersectsSegment(Envelope env, Coordinate p1, Coordinate p2)
        {
            /*
             * This is a crude test of whether segment intersects envelope.
             * It could be refined by checking exact intersection.
             * This could be based on the algorithm in the HotPixel.intersectsScaled method.
             */
            return env.Intersects(p1, p2);
        }
    }

}
