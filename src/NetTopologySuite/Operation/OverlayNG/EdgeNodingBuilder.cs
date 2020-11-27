using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Builds a set of noded, unique, labelled Edges from
    /// the edges of the two input geometries.
    /// <para/>
    /// It performs the following steps:
    /// <list type="bullet">
    /// <item><description>Extracts input edges, and attaches topological information</description></item>
    /// <item><description>if clipping is enabled, handles clipping or limiting input geometry</description></item>
    /// <item><description>chooses a <see cref="INoder"/> based on provided precision model, unless a custom one is supplied</description></item>
    /// <item><description>calls the chosen Noder, with precision model</description></item>
    /// <item><description>removes any fully collapsed noded edges</description></item>
    /// <item><description>builds <see cref="Edge"/>s and merges them</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class EdgeNodingBuilder
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
            var noder = new SnapRoundingNoder(pm);
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
        private readonly List<ISegmentString> _inputEdges = new List<ISegmentString>();
        private INoder _customNoder;

        private Envelope _clipEnv;
        private RingClipper _clipper;
        private LineLimiter _limiter;

        private readonly bool[] _hasEdges = new bool[2];

        /// <summary>
        /// Creates a new builder, with an optional custom noder.
        /// If the noder is not provided, a suitable one will
        /// be used based on the supplied precision model.
        /// </summary>
        /// <param name="pm">The precision model to use</param>
        /// <param name="noder">An optional noder to use (may be null)</param>
        public EdgeNodingBuilder(PrecisionModel pm, INoder noder)
        {
            _pm = pm;
            _customNoder = noder;
        }

        /// <summary>
        /// Gets or sets a noder appropriate for the precision model supplied.<br/>
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
                if (OverlayUtility.IsFloating(_pm))
                    return CreateFloatingPrecisionNoder(IsNodingValidated);
                return CreateFixedPrecisionNoder(_pm);
            }
            set => _customNoder = value;
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
            return _hasEdges[geomIndex];
        }

        /// <summary>
        /// Creates a set of labelled {Edge}s.
        /// representing the fully noded edges of the input geometries.
        /// Coincident edges (from the same or both geometries)
        /// are merged along with their labels
        /// into a single unique, fully labelled edge.
        /// </summary>
        /// <param name="geom0">The first geometry</param>
        /// <param name="geom1">The second geometry</param>
        /// <returns>The noded, merged, labelled edges</returns>
        public IList<Edge> Build(Geometry geom0, Geometry geom1)
        {
            Add(geom0, 0);
            Add(geom1, 1);
            var nodedEdges = Node(_inputEdges);

            /*
             * Merge the noded edges to eliminate duplicates.
             * Labels are combined.
             */
            var mergedEdges = EdgeMerger.Merge(nodedEdges);
            return mergedEdges;
        }

        /// <summary>
        /// Nodes a set of segment strings and creates {@link Edge}s from the result.
        /// The input segment strings each carry a {@link EdgeSourceInfo} object,
        /// which is used to provide source topology info to the constructed Edges
        /// (and is then discarded).
        /// </summary>
        private List<Edge> Node(IList<ISegmentString> segStrings)
        {
            var noder = Noder;
            noder.ComputeNodes(segStrings);

            var nodedSS = noder.GetNodedSubstrings();

            //scanForEdges(nodedSS);

            var edges = CreateEdges(nodedSS);

            return edges;
        }

        private List<Edge> CreateEdges(IEnumerable<ISegmentString> segStrings)
        {
            var edges = new List<Edge>();
            foreach (var ss in segStrings)
            {
                var pts = ss.Coordinates;

                // don't create edges from collapsed lines
                if (Edge.IsCollapsed(pts)) continue;

                var info = (EdgeSourceInfo)ss.Context;
                /*
                 * Record that a non-collapsed edge exists for the parent geometry
                 */
                _hasEdges[info.Index] = true;

                edges.Add(new Edge(pts, info));
            }
            return edges;
        }

        private void Add(Geometry g, int geomIndex)
        {
            if (g == null || g.IsEmpty) return;

            if (IsClippedCompletely(g.EnvelopeInternal))
                return;

            if (g is Polygon pl)                 AddPolygon(pl, geomIndex);
            // LineString also handles LinearRings
            else if (g is LineString ls)         AddLine(ls, geomIndex);
            else if (g is MultiLineString ml)    AddCollection(ml, geomIndex);
            else if (g is MultiPolygon mp)       AddCollection(mp, geomIndex);
            else if (g is GeometryCollection gc) AddGeometryCollection(gc, geomIndex, g.Dimension);
            // ignore Point geometries - they are handled elsewhere
        }

        private void AddGeometryCollection(GeometryCollection gc, int geomIndex, Dimension expectedDim)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                // check for mixed-dimension input, which is not supported
                if (g.Dimension != expectedDim)
                {
                    throw new ArgumentException("Overlay input is mixed-dimension", nameof(gc));
                }
                Add(g, geomIndex);
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
            var env = ring.EnvelopeInternal;

            /*
             * If no clipper or ring is completely contained then no need to clip.
             * But repeated points must be removed to ensure correct noding.
             */
            if (_clipper == null || _clipEnv.Covers(env))
            {
                return RemoveRepeatedPoints(ring);
            }
            return _clipper.Clip(pts);
        }

        /// <summary>
        /// Removes any repeated points from a linear component.
        /// This is required so that noding can be computed correctly.
        /// </summary>
        /// <param name="line">The line to process</param>
        /// <returns>The points of the line with repeated points removed</returns>
        private static Coordinate[] RemoveRepeatedPoints(LineString line)
        {
            var pts = line.Coordinates;
            return CoordinateArrays.RemoveRepeatedPoints(pts);
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

        /// <summary>
        /// Adds a line geometry, limiting it if enabled,
        /// and otherwise removing repeated points.
        /// </summary>
        /// <param name="line">The line to add</param>
        /// <param name="geomIndex">The index of the parent geometry</param>
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
                var ptsNoRepeat = RemoveRepeatedPoints(line);
                AddLine(ptsNoRepeat, geomIndex);
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
            _inputEdges.Add(ss);
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
            /*
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
    }
}
