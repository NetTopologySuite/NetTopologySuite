using System;
using System.Diagnostics;
using System.Collections.Generic;
using GeoAPI.Geometries;
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
    /// 	 ConformingDelaunayTriangulator cdt = new ConformingDelaunayTriangulator(sites, tolerance);
    /// 
    ///   // optional	
    ///   cdt.setSplitPointFinder(splitPointFinder);
    ///   cdt.setVertexFactory(vertexFactory);
    ///   
    ///	 cdt.setConstraints(segments, new ArrayList(vertexMap.values()));
    ///	 cdt.formInitialDelaunay();
    ///	 cdt.enforceConstraints();
    ///	 subdiv = cdt.getSubdivision();
    /// </code>
    /// </para>
    /// </remarks>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class ConformingDelaunayTriangulator 
    {
	    private static Envelope ComputeVertexEnvelope(ICollection<Vertex> vertices)
        {
		    Envelope env = new Envelope();
            foreach (var v in vertices)
            {
			    env.ExpandToInclude(v.Coordinate);
		    }
		    return env;
	    }

	    private IList<Vertex> initialVertices; // List<Vertex>
	    private IList<Vertex> segVertices; // List<Vertex>

	    // MD - using a Set doesn't seem to be much faster
	    // private Set segments = new HashSet();
	    private IList<Segment> segments = new List<Segment>(); // List<Segment>
	    private QuadEdgeSubdivision subdiv = null;
	    private IncrementalDelaunayTriangulator incDel;
	    private IGeometry convexHull;
	    private ConstraintSplitPointFinder splitFinder = new NonEncroachingSplitPointFinder();
	    private KdTree<Vertex> kdt = null;
	    private ConstraintVertexFactory vertexFactory = null;

	    // allPointsEnv expanded by a small buffer
	    private Envelope computeAreaEnv;
	    // records the last split point computed, for error reporting
	    private ICoordinate splitPt = null;

	    private double tolerance; // defines if two sites are the same.

	    /// <summary>
	    /// Creates a Conforming Delaunay Triangulation based on the given
	    /// unconstrained initial vertices. The initial vertex set should not contain
	    /// any vertices which appear in the constraint set.
	    /// </summary>
        /// <param name="initialVertices">a collection of <see cref="ConstraintVertex"/></param>
        /// <param name="tolerance">the distance tolerance below which points are considered identical</param>
	    public ConformingDelaunayTriangulator(IList<Vertex> initialVertices,
			    double tolerance)
        {
		    this.initialVertices = new List<Vertex>(initialVertices);
		    this.tolerance = tolerance;
		    kdt = new KdTree<Vertex>(tolerance);
	    }

	    /// <summary>
	    /// Sets the constraints to be conformed to by the computed triangulation.
        /// The unique set of vertices (as <see cref="ConstraintVertex"/>es) 
	    /// forming the constraints must also be supplied.
	    /// Supplying it explicitly allows the ConstraintVertexes to be initialized
	    /// appropriately(e.g. with external data), and avoids re-computing the unique set
	    /// if it is already available.
	    /// </summary>
        /// <param name="segments">list of the constraint {@link Segment}s</param>
        /// <param name="segVertices">the set of unique <see cref="ConstraintVertex"/>es referenced by the segments</param>
	    public void SetConstraints(IList<Segment> segments, IList<Vertex> segVertices)
        {
		    this.segments = segments;
		    this.segVertices = segVertices;
	    }

	    /// <summary>
        /// Sets the <see cref="ConstraintSplitPointFinder"/> to be
	    /// used during constraint enforcement.
	    /// Different splitting strategies may be appropriate
	    /// for special situations. 
	    /// </summary>
        /// <remarks>the ConstraintSplitPointFinder to be used</remarks>
        public ConstraintSplitPointFinder SplitPointFinder
        {
            set
            {
                this.splitFinder = value;
            }
	    }

	    /// <summary>
	    /// Gets the tolerance value used to construct the triangulation.
	    /// </summary>
        /// <remarks>a tolerance value</remarks>
	    public double Tolerance
	    {
            get
            {
                return tolerance;
            }
	    }
	
	    /// <summary>
        /// Gets and sets the <see cref="ConstraintVertexFactory"/> used to create new constraint vertices at split points.
        /// </summary>
	    /// <remarks>Allows the setting of a custom {@link ConstraintVertexFactory} to be used
	    /// to allow vertices carrying extra information to be created.
        /// </remarks>
	    public ConstraintVertexFactory VertexFactory
        {
            get
            {
                return vertexFactory;
            }
            set
            {
                this.vertexFactory = value;
            }
	    }

	    /// <summary>
        /// Gets the <see cref="QuadEdgeSubdivision"/> which represents the triangulation.
	    /// </summary>
	    public QuadEdgeSubdivision Subdivision
        {
            get
            {
                return subdiv;
            }
	    }

	    /// <summary>
        /// Gets the <see cref="KdTree{Vertex}"/> which contains the vertices of the triangulation.
        /// </summary>
	    public KdTree<Vertex> KDT
        {
            get
            {
                return kdt;
            }
	    }

	    /// <summary> 
	    /// Gets the sites (vertices) used to initialize the triangulation.
        /// </summary>
	    public IList<Vertex> InitialVertices
        {
            get
            {
                return initialVertices;
            }
	    }

	    /// <summary>
        /// Gets the <see cref="Segment"/>s which represent the constraints.
	    /// </summary>
	    public ICollection<Segment> ConstraintSegments
        {
            get
            {
                return segments;
            }
	    }

	    /// <summary>
	    /// Gets the convex hull of all the sites in the triangulation,
	    /// including constraint vertices.
	    /// Only valid after the constraints have been enforced.
	    /// </summary>
        /// <remarks>the convex hull of the sites</remarks>
	    public IGeometry ConvexHull
        {
            get
            {
                return convexHull;
            }
	    }

	    // ==================================================================

	    private void ComputeBoundingBox()
        {
		    Envelope vertexEnv = ComputeVertexEnvelope(initialVertices);
		    Envelope segEnv = ComputeVertexEnvelope(segVertices);

		    Envelope allPointsEnv = new Envelope(vertexEnv);
		    allPointsEnv.ExpandToInclude(segEnv);

		    double deltaX = allPointsEnv.Width * 0.2;
		    double deltaY = allPointsEnv.Height * 0.2;

		    double delta = Math.Max(deltaX, deltaY);

		    computeAreaEnv = new Envelope(allPointsEnv);
		    computeAreaEnv.ExpandBy(delta);
	    }

	    private void ComputeConvexHull()
        {
		    GeometryFactory fact = new GeometryFactory();
		    ICoordinate[] coords = GetPointArray();
		    ConvexHull hull = new ConvexHull(coords, fact);
		    convexHull = hull.GetConvexHull();
	    }

	    // /**
	    // * Adds the segments in the Convex Hull of all sites in the input data as
	    // linear constraints.
	    // * This is required if TIN Refinement is performed. The hull segments are
	    // flagged with a
	    // unique
	    // * data object to allow distinguishing them.
	    // *
	    // * @param convexHullSegmentData the data object to attach to each convex
	    // hull segment
	    // */
	    // private void addConvexHullToConstraints(Object convexHullSegmentData) {
	    // Coordinate[] coords = convexHull.getCoordinates();
	    // for (int i = 1; i < coords.length; i++) {
	    // Segment s = new Segment(coords[i - 1], coords[i], convexHullSegmentData);
	    // addConstraintIfUnique(s);
	    // }
	    // }

	    // private void addConstraintIfUnique(Segment r) {
	    // boolean exists = false;
	    // Iterator it = segments.iterator();
	    // Segment s = null;
	    // while (it.hasNext()) {
	    // s = (Segment) it.next();
	    // if (r.equalsTopo(s)) {
	    // exists = true;
	    // }
	    // }
	    // if (!exists) {
	    // segments.add((Object) r);
	    // }
	    // }

	    private ICoordinate[] GetPointArray()
        {
		    ICoordinate[] pts = new Coordinate[initialVertices.Count
				    + segVertices.Count];
		    int index = 0;
            foreach (var v in initialVertices)
            {
			    pts[index++] = v.Coordinate;
		    }
            foreach (var v in segVertices)
            {
			    pts[index++] = v.Coordinate;
		    }
		    return pts;
	    }

	    private ConstraintVertex CreateVertex(ICoordinate p)
        {
		    ConstraintVertex v = null;
		    if (vertexFactory != null)
			    v = vertexFactory.CreateVertex(p, null);
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
	    private ConstraintVertex CreateVertex(ICoordinate p, Segment seg)
        {
		    ConstraintVertex v = null;
		    if (vertexFactory != null)
			    v = vertexFactory.CreateVertex(p, seg);
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
		    var kdnode = kdt.Insert(v.Coordinate, v);
		    if (!kdnode.IsRepeated) {
			    incDel.InsertSite(v);
		    }
            else
            {
			    ConstraintVertex snappedV = (ConstraintVertex) kdnode.Data;
			    snappedV.Merge(v);
			    return snappedV;
			    // testing
			    // if ( v.isOnConstraint() && ! currV.isOnConstraint()) {
			    // System.out.println(v);
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
		    subdiv = new QuadEdgeSubdivision(computeAreaEnv, tolerance);
		    subdiv.SetLocator(new LastFoundQuadEdgeLocator(subdiv));
		    incDel = new IncrementalDelaunayTriangulator(subdiv);
		    InsertSites(initialVertices);
	    }

	    // ==================================================================

	    private int MAX_SPLIT_ITER = 99;

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
		    int splits = 0;
		    do {
			    splits = EnforceGabriel(segments);

			    count++;
			    Debug.WriteLine("Iter: " + count + "   Splits: " + splits
					    + "   Current # segments = " + segments.Count);
		    } while (splits > 0 && count < MAX_SPLIT_ITER);
		    if (count == MAX_SPLIT_ITER) {
			    Debug.WriteLine("ABORTED! Too many iterations while enforcing constraints");
                if (!Debugger.IsAttached)
				    throw new ConstraintEnforcementException(
						    "Too many splitting iterations while enforcing constraints.  Last split point was at: ",
						    splitPt);
		    }
	    }

	    private void AddConstraintVertices()
        {
		    ComputeConvexHull();
		    // insert constraint vertices as sites
		    InsertSites(segVertices);
	    }

	    /*
	     * private List findMissingConstraints() { List missingSegs = new ArrayList();
	     * for (int i = 0; i < segments.size(); i++) { Segment s = (Segment)
	     * segments.get(i); QuadEdge q = subdiv.locate(s.getStart(), s.getEnd()); if
	     * (q == null) missingSegs.add(s); } return missingSegs; }
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

			    ICoordinate encroachPt = FindNonGabrielPoint(seg);
			    // no encroachment found - segment must already be in subdivision
			    if (encroachPt == null)
				    continue;

			    // compute split point
			    splitPt = splitFinder.FindSplitPoint(seg, encroachPt);
			    ConstraintVertex splitVertex = CreateVertex(splitPt, seg);

			    // DebugFeature.addLineSegment(DEBUG_SEG_SPLIT, encroachPt, splitPt, "");
			    // Debug.println(WKTWriter.toLineString(encroachPt, splitPt));

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
			    if (!insertedVertex.Coordinate.Equals2D(splitPt)) {
				    Debug.WriteLine("Split pt snapped to: " + insertedVertex);
				    // throw new ConstraintEnforcementException("Split point snapped to
				    // existing point
				    // (tolerance too large or constraint interior narrow angle?)",
				    // splitPt);
			    }

			    // split segment and record the new halves
			    Segment s1 = new Segment(seg.StartX, seg.StartY, seg
					    .StartZ, splitVertex.X, splitVertex.Y, splitVertex
					    .Z, seg.Data);
			    Segment s2 = new Segment(splitVertex.X, splitVertex.Y,
					    splitVertex.Z, seg.EndX, seg.EndY, seg.EndZ, seg
							    .Data);
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

    //	public static final String DEBUG_SEG_SPLIT = "C:\\proj\\CWB\\test\\segSplit.jml";

	    /// <summary>
	    /// Given a set of points stored in the kd-tree and a line segment defined by
	    /// two points in this set, finds a {@link Coordinate} in the circumcircle of
	    /// the line segment, if one exists. This is called the Gabriel point - if none
	    /// exists then the segment is said to have the Gabriel condition. Uses the
	    /// heuristic of finding the non-Gabriel point closest to the midpoint of the
	    /// segment.
	    /// </summary>
        /// <param name="seg">the line segment</param>
	    /// <returns>a point which is non-Gabriel,
	    /// or null if no point is non-Gabriel
        /// </returns>
	    private ICoordinate FindNonGabrielPoint(Segment seg)
        {
		    ICoordinate p = seg.Start;
		    ICoordinate q = seg.End;
		    // Find the mid point on the line and compute the radius of enclosing circle
		    var midPt = new Coordinate((p.X + q.X) / 2.0, (p.Y + q.Y) / 2.0);
		    double segRadius = p.Distance(midPt);

		    // compute envelope of circumcircle
		    Envelope env = new Envelope(midPt);
		    env.ExpandBy(segRadius);
		    // Find all points in envelope
		    var result = kdt.Query(env);

		    // For each point found, test if it falls strictly in the circle
		    // find closest point
		    ICoordinate closestNonGabriel = null;
		    double minDist = Double.MaxValue;
            foreach (var nextNode in result)
            {
			    ICoordinate testPt = nextNode.Coordinate;
			    // ignore segment endpoints
			    if (testPt.Equals2D(p) || testPt.Equals2D(q))
				    continue;

			    double testRadius = midPt.Distance(testPt);
			    if (testRadius < segRadius) {
				    // double testDist = seg.distance(testPt);
				    double testDist = testRadius;
				    if (closestNonGabriel == null || testDist < minDist) {
					    closestNonGabriel = testPt;
					    minDist = testDist;
				    }
			    }
		    }
		    return closestNonGabriel;
	    }

    }
}