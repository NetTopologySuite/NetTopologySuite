using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// The overlay noder does the following:
    /// <list type="bullet">
    /// <item><description>Extracts input edges, and attaches topological information</description></item>
    /// <item><description>if clipping is enabled, handles clipping or limiting input geometry</description></item>
    /// <item><description>chooses a Noder based on provided precision model, unless a custom one is supplied</description></item>
    /// <item><description>calls the chosen Noder, with precision model</description></item>
    /// <item><description>removes any fully collapsed noded edges</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    class OverlayNoder
    {

        /*
         * Limiting is skipped for Lines with few vertices,
         * to avoid additional copying.
         */
        private const int MinLimitPts = 20;

        /*
         * Indicates whether floating precision noder output is validated.
         */
        private const bool IsNodingValidated = true;
  
        private static INoder CreateFixedPrecisionNoder(PrecisionModel pm)
        {
            //Noder noder = new MCIndexSnapRounder(pm);
            //Noder noder = new SimpleSnapRounder(pm);
            var noder = new FastSnapRounder(pm);
            return noder;
        }

        private static INoder CreateFloatingPrecisionNoder(bool doValidation)
        {
            var mcNoder = new MCIndexNoder();
            var li = new RobustLineIntersector();
            mcNoder.SegmentIntersector = new IntersectionAdder(li);

            INoder noder = mcNoder;
            if (doValidation)
            {
                noder = new ValidatingNoder(mcNoder);
            }
            return noder;
        }

        private readonly PrecisionModel _pm;
        private readonly List<ISegmentString> _segStrings = new List<ISegmentString>();
        private INoder _customNoder;
        private bool _hasEdgesA;
        private bool _hasEdgesB;

        private Envelope _clipEnv;
        private RingClipper _clipper;
        private LineLimiter _limiter;

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="pm">A precision model</param>
        public OverlayNoder(PrecisionModel pm)
        {
            _pm = pm;
        }

        /// <summary>
        /// Gets or sets a noder appropriate for the precision model supplied.
        /// This is one of:
        /// <list type="bullet">
        /// <item><term>Fixed precision:</term><description>a snap-rounding noder (which should be fully robust)</description></item>
        /// <item><term>Floating precision:</term><description>a conventional noder (which may be non-robust).
        /// In this case, a validation step is applied to the output from the noder.</description></item>
        /// </list>
        /// </summary>
        public INoder Noder
        {
            get
            {
                if (_customNoder != null) return _customNoder;
                if (_pm.IsFloating)
                    return CreateFloatingPrecisionNoder(IsNodingValidated);
                return CreateFixedPrecisionNoder(_pm);
            }
            set => _customNoder = value;
        }

        [Obsolete("Use Noder property")]
        public void setNoder(INoder noder)
        {
           Noder = noder;
        }

        [Obsolete("Use ClipEnvelope property")]
        public void setClipEnvelope(Envelope clipEnv)
        {
            this._clipEnv = clipEnv;
            _clipper = new RingClipper(clipEnv);
            _limiter = new LineLimiter(clipEnv);
        }

        public Envelope ClipEnvelope
        {
            get => _clipEnv;
            set
            {
                _clipEnv = value;
                _clipper = value != null ? new RingClipper(value) : null;
                _limiter = value != null ? new LineLimiter(value) : null;
            }
        }

        public IList<ISegmentString> Node()
        {
            var noder = Noder;
            //Noder noder = getSRNoder();
            //Noder noder = getSimpleNoder(false);
            //Noder noder = getSimpleNoder(true);
            noder.ComputeNodes(_segStrings);

            var nodedSS = noder.GetNodedSubstrings();

            ScanForEdges(nodedSS);

            return nodedSS;
        }

        /// <summary>
        /// Records if each geometry has edges present after noding.
        /// If a geometry has collapsed to a point due to low precision,
        /// no edges will be present.
        /// </summary>
        /// <param name="segStrings">Noded edges to scan</param>
        private void ScanForEdges(IEnumerable<ISegmentString> segStrings)
        {
            foreach (var ss in segStrings)
            {
                var info = (EdgeSourceInfo)ss.Context;
                int geomIndex = info.Index;
                if (geomIndex == 0)
                    _hasEdgesA = true;
                else if (geomIndex == 1)
                {
                    _hasEdgesB = true;
                }
                // short-circuit if both have been found
                if (_hasEdgesA && _hasEdgesB) return;
            }
        }

        /// <summary>
        /// Reports whether there are noded edges
        /// for the given input geometry.
        /// If there are none, this indicates that either
        /// the geometry was empty, or has completely collapsed
        /// (because it is smaller than the noding precision).
        /// </summary>
        /// <param name="geomIndex">index of the input geometry</param>
        /// <returns><c>true</c> if there are edges for the geometry</returns>
        public bool HasEdgesFor(int geomIndex)
        {
            if (geomIndex == 0) return _hasEdgesA;
            return _hasEdgesB;
        }

        /**
         * Gets a noder appropriate for the precision model supplied.
         * This is one of:
         * <ul>
         * <li>Fixed precision: a snap-rounding noder (which should be fully robust)
         * <li>Floating precision: a conventional nodel (which may be non-robust).
         * In this case, a validation step is applied to the output from the noder.
         * </ul> 
         * 
         * @return
         */
        [Obsolete("Use Noder property")]
        private INoder getNoder()
        {
            return Noder;
        }

        public void Add(Geometry g, int geomIndex)
        {
            if (g == null || g.IsEmpty) return;

            if (IsClippedCompletely(g.EnvelopeInternal))
                return;

            if (g is Polygon pl)                 AddPolygon(pl, geomIndex);
            // LineString also handles LinearRings
            else if (g is LineString ls)         AddLine(ls, geomIndex);
            else if (g is MultiLineString ml)    AddCollection(ml, geomIndex);
            else if (g is MultiPolygon mp)       AddCollection(mp, geomIndex);
            else if (g is GeometryCollection gc) AddCollection(gc, geomIndex);
            else
            {
                // ignore Point geometries - they are handled elsewhere
            }
        }

        private void AddCollection(GeometryCollection gc, int geomIndex)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                Add(g, geomIndex);
            }
        }

        private void AddPolygon(Polygon poly, int geomIndex)
        {
            var shell = poly.ExteriorRing;
            AddPolygonRing((LinearRing)shell, false, geomIndex);

            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = poly.GetInteriorRingN(i);

                // Holes are topologically labelled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CW)
                AddPolygonRing((LinearRing)hole, true, geomIndex);
            }
        }

        /// <summary>
        /// Adds a polygon ring to the graph.
        /// </summary>
        /// <remarks>Empty rings are ignored.</remarks>
        private void AddPolygonRing(LinearRing ring, bool isHole, int index)
        {
            // don't add empty lines
            if (ring.IsEmpty) return;

            if (IsClippedCompletely(ring.EnvelopeInternal))
                return;

            var pts = Clip(ring);

            /*
             * Don't add edges that collapse to a point
             */
            if (pts.Length < 2)
            {
                return;
            }

            //if (pts.length < ring.getNumPoints()) System.out.println("Ring clipped: " + ring.getNumPoints() + " => " + pts.length);

            int depthDelta = ComputeDepthDelta(ring, isHole);
            var info = new EdgeSourceInfo(index, depthDelta, isHole);
            AddEdge(pts, info);
        }

        private static int ComputeDepthDelta(LinearRing ring, bool isHole)
        {
            /*
             * Compute the orientation of the ring, to
             * allow assigning side interior/exterior labels correctly.
             * JTS canonical orientation is that shells are CW, holes are CCW.
             * 
             * It is important to compute orientation on the original ring,
             * since topology collapse can make the orientation computation give the wrong answer.
             */
            bool isCCW = Orientation.IsCCW(ring.CoordinateSequence);
            /*
             * Compute whether ring is in canonical orientation or not.
             * Canonical orientation for the overlay process is
             * Shells : CW, Holes: CCW
             */
            bool isOriented;
            if (!isHole)
                isOriented = !isCCW;
            else
                isOriented = isCCW;
            /*
             * Depth delta can now be computed. 
             * Canonical depth delta is 1 (Exterior on L, Interior on R).
             * It is flipped to -1 if the ring is oppositely oriented.
             */
            int depthDelta = isOriented ? 1 : -1;
            return depthDelta;
        }

        private void AddLine(LineString line, int geomIndex)
        {
            // don't add empty lines
            if (line.IsEmpty) return;

            if (IsClippedCompletely(line.EnvelopeInternal))
                return;

            if (IsToBeLimited(line))
            {
                var sections = Limit(line);
                foreach (var pts in sections)
                {
                    AddLine(pts, geomIndex);
                }
            }
            else
            {
                AddLine(line.Coordinates, geomIndex);
            }
        }

        private void AddLine(Coordinate[] pts, int geomIndex)
        {
            /*
             * Don't add edges that collapse to a point
             */
            if (pts.Length < 2)
            {
                return;
            }

            var info = new EdgeSourceInfo(geomIndex);
            AddEdge(pts, info);
        }

        private void AddEdge(Coordinate[] pts, EdgeSourceInfo info)
        {
            var ss = new NodedSegmentString(pts, info);
            _segStrings.Add(ss);
        }


        /// <summary>
        /// Tests whether a geometry (represented by its envelope)
        /// lies completely outside the clip extent(if any).
        /// </summary>
        /// <param name="env">The geometry envelope</param>
        /// <returns><c>true</c> if the geometry envelope is outside the clip extent.</returns>
        private bool IsClippedCompletely(Envelope env)
        {
            if (_clipEnv == null) return false;
            return _clipEnv.Disjoint(env);
        }

        /// <summary>
        /// If clipper is present,
        /// clip the line to the clip extent.
        /// <para/>
        /// If clipping is enabled, then every ring MUST
        /// be clipped, to ensure that holes are clipped to
        /// be inside the shell.
        /// This means it is not possible to skip
        /// clipping for rings with few vertices.
        /// </summary>
        /// <param name="ring">The line to clip</param>
        /// <returns>The points in the clipped ring</returns>
        private Coordinate[] Clip(LinearRing ring)
        {
            var pts = ring.Coordinates;
            if (_clipper == null)
            {
                return pts;
            }
            var env = ring.EnvelopeInternal;
            /*
             * If line is completely contained then no need to clip
             */
            if (_clipEnv.Covers(env))
            {
                return pts;
            }
            return _clipper.clip(pts);
        }

        /// <summary>
        /// Tests whether it is worth limiting a line.
        /// Lines that have few vertices or are covered
        /// by the clip extent do not need to be limited.
        /// </summary>
        /// <param name="line">The line to test</param>
        /// <returns><c>true</c> if the line should be limited</returns>
        private bool IsToBeLimited(LineString line)
        {
            var pts = line.Coordinates;
            if (_limiter == null || pts.Length <= MinLimitPts)
            {
                return false;
            }
            var env = line.EnvelopeInternal;
            /**
             * If line is completely contained then no need to limit
             */
            if (_clipEnv.Covers(env))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// If limiter is provided,
        /// limit the line to the clip envelope.
        /// </summary>
        /// <param name="line">The line to clip</param>
        /// <returns>the point sections in the clipped line</returns>
        private IEnumerable<Coordinate[]> Limit(LineString line)
        {
            var pts = line.Coordinates;
            return _limiter.Limit(pts);
        }

        /*

        // rounding is carried out by Noder, if needed

        private Coordinate[] round(Coordinate[] pts)  {

          CoordinateList noRepeatCoordList = new CoordinateList();

          for (int i = 0; i < pts.length; i++) {
            Coordinate coord = new Coordinate(pts[i]);

            // MD - disable for now to test improved snap-rounding
            //makePrecise(coord);
            noRepeatCoordList.add(coord, false);
          }
          Coordinate[] reducedPts = noRepeatCoordList.toCoordinateArray();
          return reducedPts;
        }  

        private void makePrecise(Coordinate coord) {
          // this allows clients to avoid rounding if needed by the noder
          if (pm != null)
            pm.makePrecise(coord);
        }

        private Coordinate[] round(Coordinate[] pts, int minLength)  {
          CoordinateList noRepeatCoordList = new CoordinateList();

          for (int i = 0; i < pts.length; i++) {
            Coordinate coord = new Coordinate(pts[i]);
            pm.makePrecise(coord);
            noRepeatCoordList.add(coord, false);
          }
          Coordinate[] reducedPts = noRepeatCoordList.toCoordinateArray();
          if (minLength > 0 && reducedPts.length < minLength) {
            return pad(reducedPts, minLength);
          }
          return reducedPts;
        }


        private static Coordinate[] pad(Coordinate[] pts, int minLength) {
          Coordinate[] pts2 = new Coordinate[minLength];
          for (int i = 0; i < minLength; i++) {
            if (i < pts.length) {
              pts2[i] = pts[i];
            }
            else {
              pts2[i] = pts[pts.length - 1];
            }
          }
          return pts2;
        }
        */
    }
}
