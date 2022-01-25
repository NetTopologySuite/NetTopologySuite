using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{

    /// <summary>
    /// Computes a Conforming Delaunay Triangulation over a set of sites and a set of
    /// linear constraints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A conforming Delaunay triangulation is a true Delaunay triangulation. In it
    /// each constraint segment is present as a union of one or more triangulation
    /// edges. Constraint segments may be subdivided into two or more triangulation
    /// edges by the insertion of additional sites. The additional sites are called
    /// Steiner points, and are necessary to allow the segments to be faithfully
    /// reflected in the triangulation while maintaining the Delaunay property.
    /// Another way of stating this is that in a conforming Delaunay triangulation
    /// every constraint segment will be the union of a subset of the triangulation
    /// edges (up to tolerance).
    /// </para>
    /// <para>
    /// A Conforming Delaunay triangulation is distinct from a Constrained Delaunay triangulation.
    /// A Constrained Delaunay triangulation is not necessarily fully Delaunay,
    /// and it contains the constraint segments exactly as edges of the triangulation.
    /// </para>
    /// <para>
    /// A typical usage pattern for the triangulator is:
    /// <code>
    ///   ConformingDelaunayTriangulator cdt = new ConformingDelaunayTriangulator(sites, tolerance);
    ///
    ///   // optional
    ///   cdt.SplitPointFinder = splitPointFinder;
    ///   cdt.VertexFactory = vertexFactory;
    ///
    ///   cdt.SetConstraints(segments,  new List&lt;Vertex&gt;(vertexMap.Values));
    ///   cdt.FormInitialDelaunay();
    ///   cdt.EnforceConstraints();
    ///   subdiv = cdt.Subdivision;
    /// </code>
    /// </para>
    /// </remarks>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class ConformingDelaunayTriangulator
    {
        private static Envelope ComputeVertexEnvelope(IEnumerable<Vertex> vertices)
        {
            var env = new Envelope();
            foreach (var v in vertices)
                env.ExpandToInclude(v.Coordinate);
            return env;
        }

        private readonly IList<Vertex> _initialVertices; // List<Vertex>
        private IList<Vertex> _segVertices; // List<Vertex>

        // MD - using a Set doesn't seem to be much faster
        // private Set segments = new HashSet();
        private IList<Segment> _segments = new List<Segment>(); // List<Segment>
        private QuadEdgeSubdivision _subdiv;
        private IncrementalDelaunayTriangulator _incDel;
        private Geometry _convexHull;
        private IConstraintSplitPointFinder _splitFinder = new NonEncroachingSplitPointFinder();
        private readonly KdTree<Vertex> _kdt;
        private ConstraintVertexFactory _vertexFactory;

        // allPointsEnv expanded by a small buffer
        private Envelope _computeAreaEnv;
        // records the last split point computed, for error reporting
        private Coordinate _splitPt;

        private readonly double _tolerance; // defines if two sites are the same.

        /// <summary>
        /// Creates a Conforming Delaunay Triangulation based on the given
        /// unconstrained initial vertices. The initial vertex set should not contain
        /// any vertices which appear in the constraint set.
        /// </summary>
        /// <param name="initialVertices">a collection of <see cref="ConstraintVertex"/></param>
        /// <param name="tolerance">the distance tolerance below which points are considered identical</param>
        public ConformingDelaunayTriangulator(IEnumerable<Vertex> initialVertices,
                double tolerance)
        {
            _initialVertices = new List<Vertex>(initialVertices);
            _tolerance = tolerance;
            _kdt = new KdTree<Vertex>(tolerance);
        }

        /// <summary>
        /// Sets the constraints to be conformed to by the computed triangulation.
        /// The constraints must not contain duplicate segments (up to orientation).
        /// The unique set of vertices (as <see cref="ConstraintVertex"/>es)
        /// forming the constraints must also be supplied.
        /// Supplying it explicitly allows the ConstraintVertexes to be initialized
        /// appropriately (e.g. with external data), and avoids re-computing the unique set
        /// if it is already available.
        /// </summary>
        /// <param name="segments">list of the constraint <see cref="Segment"/>s</param>
        /// <param name="segVertices">the set of unique <see cref="ConstraintVertex"/>es referenced by the segments</param>
        public void SetConstraints(IList<Segment> segments, IList<Vertex> segVertices)
        {
            _segments = segments;
            _segVertices = segVertices;
        }

        /// <summary>
        /// Gets or sets the <see cref="IConstraintSplitPointFinder"/> to be
        /// used during constraint enforcement.
        /// Different splitting strategies may be appropriate
        /// for special situations.
        /// </summary>
        /// <remarks>the ConstraintSplitPointFinder to be used</remarks>
        public IConstraintSplitPointFinder SplitPointFinder
        {
            get => _splitFinder;
            set => _splitFinder = value;
        }

        /// <summary>
        /// Gets the tolerance value used to construct the triangulation.
        /// </summary>
        /// <remarks>a tolerance value</remarks>
        public double Tolerance => _tolerance;

        /// <summary>
        /// Gets and sets the <see cref="ConstraintVertexFactory"/> used to create new constraint vertices at split points.
        /// </summary>
        /// <remarks>Allows the setting of a custom <see cref="ConstraintVertexFactory"/> to be used
        /// to allow vertices carrying extra information to be created.
        /// </remarks>
        public ConstraintVertexFactory VertexFactory
        {
            get => _vertexFactory;
            set => _vertexFactory = value;
        }

        /// <summary>
        /// Gets the <see cref="QuadEdgeSubdivision"/> which represents the triangulation.
        /// </summary>
        public QuadEdgeSubdivision Subdivision => _subdiv;

        /// <summary>
        /// Gets the <see cref="KdTree{Vertex}"/> which contains the vertices of the triangulation.
        /// </summary>
        public KdTree<Vertex> KDT => _kdt;

        /// <summary>
        /// Gets the sites (vertices) used to initialize the triangulation.
        /// </summary>
        public IList<Vertex> InitialVertices => _initialVertices;

        /// <summary>
        /// Gets the <see cref="Segment"/>s which represent the constraints.
        /// </summary>
        public ICollection<Segment> ConstraintSegments => _segments;

        /// <summary>
        /// Gets the convex hull of all the sites in the triangulation,
        /// including constraint vertices.
        /// Only valid after the constraints have been enforced.
        /// </summary>
        /// <remarks>the convex hull of the sites</remarks>
        public Geometry ConvexHull => _convexHull;

        // ==================================================================

        private void ComputeBoundingBox()
        {
            var vertexEnv = ComputeVertexEnvelope(_initialVertices);
            var segEnv = ComputeVertexEnvelope(_segVertices);

            var allPointsEnv = new Envelope(vertexEnv);
            allPointsEnv.ExpandToInclude(segEnv);

            double deltaX = allPointsEnv.Width * 0.2;
            double deltaY = allPointsEnv.Height * 0.2;

            double delta = Math.Max(deltaX, deltaY);

            _computeAreaEnv = new Envelope(allPointsEnv);
            _computeAreaEnv.ExpandBy(delta);
        }

        private void ComputeConvexHull()
        {
            var fact = new GeometryFactory();
            var coords = GetPointArray();
            var hull = new ConvexHull(coords, fact);
            _convexHull = hull.GetConvexHull();
        }

        private Coordinate[] GetPointArray()
        {
            var pts = new Coordinate[_initialVertices.Count
                    + _segVertices.Count];
            int index = 0;
            foreach (var v in _initialVertices)
            {
                pts[index++] = v.Coordinate;
            }
            foreach (var v in _segVertices)
            {
                pts[index++] = v.Coordinate;
            }
            return pts;
        }

        private ConstraintVertex CreateVertex(Coordinate p)
        {
            ConstraintVertex v = null;
            if (_vertexFactory != null)
                v = _vertexFactory.CreateVertex(p, null);
            else
                v = new ConstraintVertex(p);
            return v;
        }

        /// <summary>
        /// Creates a vertex on a constraint segment
        /// </summary>
        /// <param name="p">the location of the vertex to create</param>
        /// <param name="seg">the constraint segment it lies on</param>
        /// <returns>the new constraint vertex</returns>
        private ConstraintVertex CreateVertex(Coordinate p, Segment seg)
        {
            ConstraintVertex v;
            if (_vertexFactory != null)
                v = _vertexFactory.CreateVertex(p, seg);
            else
                v = new ConstraintVertex(p);
            v.IsOnConstraint = true;
            return v;
        }

        /// <summary>
        /// Inserts all sites in a collection
        /// </summary>
        /// <param name="vertices">a collection of ConstraintVertex</param>
        private void InsertSites(ICollection<Vertex> vertices)
        {
            Debug.WriteLine("Adding sites: " + vertices.Count);
            foreach (var v in vertices)
            {
                InsertSite((ConstraintVertex)v);
            }
        }

        private ConstraintVertex InsertSite(ConstraintVertex v)
        {
            var kdnode = _kdt.Insert(v.Coordinate, v);
            if (!kdnode.IsRepeated)
            {
                _incDel.InsertSite(v);
            }
            else
            {
                var snappedV = (ConstraintVertex)kdnode.Data;
                snappedV.Merge(v);
                return snappedV;
                // testing
                // if ( v.isOnConstraint() && ! currV.isOnConstraint()) {
                //     System.out.println(v);
                // }
            }
            return v;
        }

        /// <summary>
        /// Inserts a site into the triangulation, maintaining the conformal Delaunay property.
        /// This can be used to further refine the triangulation if required
        /// (e.g. to approximate the medial axis of the constraints,
        /// or to improve the grading of the triangulation).
        /// </summary>
        /// <param name="p">the location of the site to insert</param>
        public void InsertSite(Coordinate p)
        {
            InsertSite(CreateVertex(p));
        }

        // ==================================================================

        /// <summary>
        /// Computes the Delaunay triangulation of the initial sites.
        /// </summary>
        public void FormInitialDelaunay()
        {
            ComputeBoundingBox();
            _subdiv = new QuadEdgeSubdivision(_computeAreaEnv, _tolerance);
            _subdiv.SetLocator(new LastFoundQuadEdgeLocator(_subdiv));
            _incDel = new IncrementalDelaunayTriangulator(_subdiv);
            InsertSites(_initialVertices);
        }

        // ==================================================================

        private const int MaxSplitIteration = 99;

        /// <summary>
        /// Enforces the supplied constraints into the triangulation.
        /// </summary>
        /// <exception cref="ConstraintEnforcementException">
        /// if the constraints cannot be enforced</exception>
        public void EnforceConstraints()
        {
            AddConstraintVertices();
            // if (true) return;

            int count = 0;
            int splits /*, oldSegmentCount = _segments.Count*/;
            do
            {
                splits = EnforceGabriel(_segments);

                count++;
                // Debug FObermaier
                // for(var i = oldSegmentCount; i < _segments.Count; i++)
                //     Console.WriteLine("Segments added: #" + i + ": " + _segments[i].LineSegment);
                // oldSegmentCount = _segments.Count;
            } while (splits > 0 && count < MaxSplitIteration);

            if (count == MaxSplitIteration)
            {
                Debug.WriteLine("ABORTED! Too many iterations while enforcing constraints");
                if (!Debugger.IsAttached)
                    throw new ConstraintEnforcementException(
                        "Too many splitting iterations while enforcing constraints.  Last split point was at: ",
                        _splitPt);
            }
        }

        private void AddConstraintVertices()
        {
            ComputeConvexHull();
            // insert constraint vertices as sites
            InsertSites(_segVertices);
        }

        /*
         * private List findMissingConstraints() {
         *     List missingSegs = new ArrayList();
         *     for (int i = 0; i < segments.size(); i++) {
         *         Segment s = (Segment)segments.get(i);
         *         QuadEdge q = subdiv.locate(s.getStart(), s.getEnd());
         *         if (q == null)
         *             missingSegs.add(s);
         *     }
         *
         *     return missingSegs;
         * }
         */

        private int EnforceGabriel(ICollection<Segment> segsToInsert)
        {
            var newSegments = new List<Segment>();
            int splits = 0;
            var segsToRemove = new List<Segment>();

            /*
             * On each iteration must always scan all constraint (sub)segments, since
             * some constraints may be rebroken by Delaunay triangle flipping caused by
             * insertion of another constraint. However, this process must converge
             * eventually, with no splits remaining to find.
             */
            foreach (var seg in segsToInsert)
            {
                // System.out.println(seg);

                var encroachPt = FindNonGabrielPoint(seg);
                // no encroachment found - segment must already be in subdivision
                if (encroachPt == null)
                    continue;

                // compute split point
                _splitPt = _splitFinder.FindSplitPoint(seg, encroachPt);
                var splitVertex = CreateVertex(_splitPt, seg);

                /*
                 * Check whether the inserted point still equals the split pt. This will
                 * not be the case if the split pt was too close to an existing site. If
                 * the point was snapped, the triangulation will not respect the inserted
                 * constraint - this is a failure. This can be caused by:
                 * <ul>
                 * <li>An initial site that lies very close to a constraint segment The
                 * cure for this is to remove any initial sites which are close to
                 * constraint segments in a preprocessing phase.
                 * <li>A narrow constraint angle which causing repeated splitting until
                 * the split segments are too small. The cure for this is to either choose
                 * better split points or "guard" narrow angles by cracking the segments
                 * equidistant from the corner.
                 * </ul>
                 */
                var insertedVertex = InsertSite(splitVertex);
                // Debugging FObermaier
                // Console.WriteLine("inserted vertex: " + insertedVertex.ToString());

                if (!insertedVertex.Coordinate.Equals2D(_splitPt))
                {
                    Debug.WriteLine("Split pt snapped to: " + insertedVertex);
                    // throw new ConstraintEnforcementException("Split point snapped to existing point (tolerance too large or constraint interior narrow angle?)", splitPt);
                }

                // split segment and record the new halves
                var s1 = new Segment(seg.StartX, seg.StartY, seg.StartZ,
                                     splitVertex.X, splitVertex.Y, splitVertex.Z,
                                     seg.Data);
                var s2 = new Segment(splitVertex.X, splitVertex.Y, splitVertex.Z,
                                     seg.EndX, seg.EndY, seg.EndZ,
                                     seg.Data);

                // Debugging FObermaier
                // Console.WriteLine("Segment " + seg.ToString() + " splitted to \n\t" + s1.ToString() + "\n\t"+ s2.ToString());
                newSegments.Add(s1);
                newSegments.Add(s2);
                segsToRemove.Add(seg);

                splits = splits + 1;
            }
            foreach (var seg in segsToRemove)
            {
                segsToInsert.Remove(seg);
            }

            foreach (var seg in newSegments)
            {
                segsToInsert.Add(seg);
            }

            return splits;
        }

        // public static final String DEBUG_SEG_SPLIT = "C:\\proj\\CWB\\test\\segSplit.jml";

        /// <summary>
        /// Given a set of points stored in the kd-tree and a line segment defined by
        /// two points in this set, finds a <see cref="Coordinate"/> in the circumcircle of
        /// the line segment, if one exists. This is called the Gabriel point - if none
        /// exists then the segment is said to have the Gabriel condition. Uses the
        /// heuristic of finding the non-Gabriel point closest to the midpoint of the
        /// segment.
        /// </summary>
        /// <param name="seg">the line segment</param>
        /// <returns>
        /// A point which is non-Gabriel,
        /// or null if no point is non-Gabriel
        /// </returns>
        private Coordinate FindNonGabrielPoint(Segment seg)
        {
            var p = seg.Start;
            var q = seg.End;
            // Find the mid point on the line and compute the radius of enclosing circle
            var midPt = new Coordinate((p.X + q.X) / 2.0, (p.Y + q.Y) / 2.0);
            double segRadius = p.Distance(midPt);

            // compute envelope of circumcircle
            var env = new Envelope(midPt);
            env.ExpandBy(segRadius);
            // Find all points in envelope
            var result = _kdt.Query(env);

            // For each point found, test if it falls strictly in the circle
            // find closest point
            Coordinate closestNonGabriel = null;
            double minDist = double.MaxValue;
            foreach (var nextNode in result)
            {
                var testPt = nextNode.Coordinate;
                // ignore segment endpoints
                if (testPt.Equals2D(p) || testPt.Equals2D(q))
                    continue;

                double testRadius = midPt.Distance(testPt);
                if (testRadius < segRadius)
                {
                    // double testDist = seg.distance(testPt);
                    double testDist = testRadius;
                    if (closestNonGabriel == null || testDist < minDist)
                    {
                        closestNonGabriel = testPt;
                        minDist = testDist;
                    }
                }
            }
            return closestNonGabriel;
        }
    }
}
