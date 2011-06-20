using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Index.KdTree;
using GisSharpBlog.NetTopologySuite.Triangulate.Quadedge;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
/**
 * Computes a Conforming Delaunay Triangulation over a set of sites and a set of
 * linear constraints.
 * <p>
 * A conforming Delaunay triangulation is a true Delaunay triangulation. In it
 * each constraint segment is present as a union of one or more triangulation
 * edges. Constraint segments may be subdivided into two or more triangulation
 * edges by the insertion of additional sites. The additional sites are called
 * Steiner points, and are necessary to allow the segments to be faithfully
 * reflected in the triangulation while maintaining the Delaunay property.
 * Another way of stating this is that in a conforming Delaunay triangulation
 * every constraint segment will be the union of a subset of the triangulation
 * edges (up to tolerance).
 * <p>
 * A Conforming Delaunay triangulation is distinct from a Constrained Delaunay triangulation.
 * A Constrained Delaunay triangulation is not necessarily fully Delaunay, 
 * and it contains the constraint segments exactly as edges of the triangulation.
 * <p>
 * A typical usage pattern for the triangulator is:
 * <pre>
 * 	 ConformingDelaunayTriangulator cdt = new ConformingDelaunayTriangulator(sites, tolerance);
 * 
 *   // optional	
 *   cdt.setSplitPointFinder(splitPointFinder);
 *   cdt.setVertexFactory(vertexFactory);
 *   
 *	 cdt.setConstraints(segments, new ArrayList(vertexMap.values()));
 *	 cdt.formInitialDelaunay();
 *	 cdt.EnforceConstraints();
 *	 subdiv = cdt.getSubdivision();
 * </pre>
 * 
 * @author David Skea
 * @author Martin Davis
 */
///<summary>
///</summary>
///<typeparam name="TCoordinate"></typeparam>
public class ConformingDelaunayTriangulator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible 
{
	private static IExtents<TCoordinate> ComputeVertexEnvelope(IGeometryFactory<TCoordinate> geomFactory, IEnumerable<Vertex<TCoordinate>> vertices) 
    {
		IExtents<TCoordinate> ext = geomFactory.CreateExtents();
	    foreach (Vertex<TCoordinate> vertex in vertices)
            ext.ExpandToInclude(vertex.Coordinate);
		return ext;
	}

    private readonly IGeometryFactory<TCoordinate> _geometryFactory;
    private readonly ICoordinateFactory<TCoordinate> _coordFactory;

	private readonly List<Vertex<TCoordinate>> _initialVertices; // List<Vertex>
	private readonly List<Vertex<TCoordinate>> _segVertices = new List<Vertex<TCoordinate>>(); // List<Vertex>

	// MD - using a Set doesn't seem to be much faster
	// private Set segments = new HashSet();
    private readonly List<Segment<TCoordinate>> _segments = new List<Segment<TCoordinate>>();
	private QuadEdgeSubdivision<TCoordinate> _subdiv;
	private IncrementalDelaunayTriangulator<TCoordinate> _incDel;
	private IGeometry<TCoordinate> _convexHull;
    private IConstraintSplitPointFinder<TCoordinate> _splitFinder;// = new NonEncroachingSplitPointFinder<TCoordinate>();
	private readonly KdTree<TCoordinate> _kdt;
	private IConstraintVertexFactory<TCoordinate> _vertexFactory;

	// allPointsEnv expanded by a small buffer
	private IExtents<TCoordinate> _computeAreaEnv;
	// records the last split point computed, for error reporting
	private TCoordinate _splitPt;

	private readonly double _tolerance; // defines if two sites are the same.

	///<summary>
	/// Creates a Conforming Delaunay Triangulation based on the given
	/// unconstrained initial vertices. The initial vertex set should not contain
	/// any vertices which appear in the constraint set.
	///</summary>
	/// <param name="geometryFactory">a factory used to create geometries</param>
	///<param name="initialVertices">a collection of <see cref="ConstraintVertex{TCoordinate}"/></param>
	///<param name="tolerance">the distance tolerance below which points are considered identical</param>
	public ConformingDelaunayTriangulator(IGeometryFactory<TCoordinate> geometryFactory,
        IEnumerable<Vertex<TCoordinate>> initialVertices,
			double tolerance)
    {
	    _geometryFactory = geometryFactory;
	    _coordFactory = geometryFactory.CoordinateFactory;
		_initialVertices = new List<Vertex<TCoordinate>>(initialVertices);
		_tolerance = tolerance;
		_kdt = new KdTree<TCoordinate>(tolerance);
	    _splitFinder = new NonEncroachingSplitPointFinder<TCoordinate>(_coordFactory);
	}

	///<summary>
	/// Sets the constraints to be conformed to by the computed triangulation.
	/// The unique set of vertices (as {@link ConstraintVertex}es)
	/// forming the constraints must also be supplied.
	/// Supplying it explicitly allows the ConstraintVertexes to be initialized
	/// appropriately(e.g. with external data), and avoids re-computing the unique set
	/// if it is already available.
	///</summary>
	///<param name="segments">a list of the constraint <see cref="Segment{TCoordinate}"/>s</param>
	///<param name="segVertices">the set of unique <see cref="ConstraintVertex{TCoordinate}"/>es referenced by the segments</param>
	public void SetConstraints(IEnumerable<Segment<TCoordinate>> segments, IEnumerable<Vertex<TCoordinate>> segVertices)
    {
		_segments.AddRange(segments);
		_segVertices.AddRange(segVertices);
	}

	///<summary>
	/// Gets/Sets the <see cref="IConstraintSplitPointFinder{TCoordinate}"/> to be used during constraint enforcement.
	/// Different splitting strategies may be appropriate for special situations. 
	///</summary>
	public IConstraintSplitPointFinder<TCoordinate> SplitPointFinder
    {
		get { return _splitFinder;}
        set { _splitFinder = value;}
	}

    ///<summary>
	/// Gets the tolerance value used to construct the triangulation.
	///</summary>
	public Double Tolerance
	{
		get { return _tolerance;}
	}
	
	///<summary>
	/// Gets the <see cref="IConstraintVertexFactory{TCoordinate}"/> used to create new constraint vertices at split points.
	///</summary>
	public IConstraintVertexFactory<TCoordinate> VertexFactory
    {
		get { return _vertexFactory; }
        set { _vertexFactory = value; }
	}

	///<summary>
	/// Gets the <see cref="QuadEdgeSubdivision{TCoordinate}"/> which represents the triangulation.
	///</summary>
	///<returns></returns>
	public QuadEdgeSubdivision<TCoordinate> GetSubdivision() 
    {
		return _subdiv;
	}

	///<summary>
	/// Gets the <see cref="KdTree{TCoordinate}"/> which contains the vertices of the triangulation.
	///</summary>
	///<returns></returns>
	public KdTree<TCoordinate> KdTree
    {
        get { return _kdt; }
	}

	///<summary>
	/// Gets the sites (vertices) used to initialize the triangulation.
	///</summary>
	public List<Vertex<TCoordinate>> InitialVertices
    {
        get { return _initialVertices; }
    }

	///<summary>
	/// Gets the <see cref="Segment{TCoordinate}"/>{@link Segment}s which represent the constraints.
	///</summary>
	public List<Segment<TCoordinate>> ConstraintSegments
    {
		get {return _segments;}
	}

	///<summary> 
	/// Gets the convex hull of all the sites in the triangulation, including constraint vertices. Only valid after the constraints have been enforced.
	///</summary>
	public IGeometry<TCoordinate> ConvexHull
    {
		get { return _convexHull; }
	}

	// ==================================================================

	private void ComputeBoundingBox() 
    {
		IExtents<TCoordinate> vertexEnv = ComputeVertexEnvelope(_geometryFactory, _initialVertices);
        IExtents<TCoordinate> segEnv = ComputeVertexEnvelope(_geometryFactory, _segVertices);

        IExtents<TCoordinate> allPointsEnv = _geometryFactory.CreateExtents(vertexEnv);
		allPointsEnv.ExpandToInclude(segEnv);

		double deltaX = allPointsEnv.GetSize(Ordinates.X) * 0.2;
		double deltaY = allPointsEnv.GetSize(Ordinates.Y) * 0.2;

		double delta = Math.Max(deltaX, deltaY);

		_computeAreaEnv = _geometryFactory.CreateExtents(allPointsEnv);
		_computeAreaEnv.ExpandBy(delta);
	}

	private void ComputeConvexHull()
    {
		ConvexHull<TCoordinate> hull = new ConvexHull<TCoordinate>(_geometryFactory.CoordinateSequenceFactory.Create(GetPoints()).WithoutDuplicatePoints(), _geometryFactory);
		_convexHull = hull.GetConvexHull();
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

	private IEnumerable<TCoordinate> GetPoints()
    {
	    foreach (Vertex<TCoordinate> initialVertex in _initialVertices)
	        yield return initialVertex.Coordinate;

        foreach (Vertex<TCoordinate> segVertex in _segVertices)
            yield return segVertex.Coordinate;
	}

	private ConstraintVertex<TCoordinate> CreateVertex(TCoordinate p)
	{
	    ConstraintVertex<TCoordinate> v = _vertexFactory != null ? _vertexFactory.CreateVertex(p, null) : new ConstraintVertex<TCoordinate>(p);
	    return v;
	}

    ///<summary>Creates a vertex on a constraint segment</summary>
    /// <param name="p">the location of the vertex to create</param>
    /// <param name="seg">the constraint segment it lies on</param>
    /// <returns>the new constraint vertex</returns>
	private ConstraintVertex<TCoordinate> CreateVertex(TCoordinate p, Segment<TCoordinate> seg)
    {
		ConstraintVertex<TCoordinate> v;
		if (_vertexFactory != null)
			v = _vertexFactory.CreateVertex(p, seg);
		else
			v = new ConstraintVertex<TCoordinate>(p);
		v.IsOnConstraint = true;
		return v;
	}

    ///<summary>Inserts all sites in a collection</summary>
    /// <param name="vertices">a collection of ConstraintVertex</param>
	private void InsertSites(IEnumerable<Vertex<TCoordinate>> vertices)
    {
		//Debug.println("Adding sites: " + vertices.size());
	    foreach (Vertex<TCoordinate> vertex in vertices)
	    {
	        ConstraintVertex<TCoordinate> v = (ConstraintVertex<TCoordinate>) vertex;
	        InsertSite(v);
	    }
	}

	private ConstraintVertex<TCoordinate> InsertSite(ConstraintVertex<TCoordinate> v)
    {
		KdNode<TCoordinate> kdnode = _kdt.Insert(v.Coordinate, v);
		if (!kdnode.IsRepeated)
        {
			_incDel.InsertSite(v);
		} 
        else 
        {
			ConstraintVertex<TCoordinate> snappedV = (ConstraintVertex<TCoordinate>) kdnode.Data;
			snappedV.Merge(v);
			return snappedV;
			// testing
			// if ( v.isOnConstraint() && ! currV.isOnConstraint()) {
			// System.out.println(v);
			// }
		}
		return v;
	}

	///<summary>
	/// Inserts a site into the triangulation, maintaining the conformal Delaunay property.
	/// This can be used to further refine the triangulation if required
	/// (e.g. to approximate the medial axis of the constraints,
	/// or to improve the grading of the triangulation).
	///</summary>
	///<param name="p"></param>
	public void InsertSite(TCoordinate p)
    {
		InsertSite(CreateVertex(p));
	}

	// ==================================================================

	///<summary>
	/// Computes the Delaunay triangulation of the initial sites.
	///</summary>
	public void FormInitialDelaunay()
    {
		ComputeBoundingBox();
		_subdiv = new QuadEdgeSubdivision<TCoordinate>(_geometryFactory, _computeAreaEnv, _tolerance);
		_subdiv.Locator = new LastFoundQuadEdgeLocator<TCoordinate>(_subdiv);
		_incDel = new IncrementalDelaunayTriangulator<TCoordinate>(_subdiv);
		InsertSites(_initialVertices);
	}

	// ==================================================================

	private const int MaxSplitIteration = 99;

	/**
	 * 
	 * 
	 * @throws ConstraintEnforcementException
	 *           
	 */
	///<summary>
	/// Enforces the supplied constraints into the triangulation.
	///</summary>
	///<exception cref="ConstraintEnforcementException{TCoordinate}">if the constraints cannot be enforced</exception>
	public void EnforceConstraints()
    {
		AddConstraintVertices();
		// if (true) return;

		int count = 0;
		int splits;
		do
        {
			splits = EnforceGabriel(_segments);

			count++;
            //Debug.println("Iter: " + count + "   Splits: " + splits
            //        + "   Current # segments = " + _segments.size());
		} while (splits > 0 && count < MaxSplitIteration);
		if (count == MaxSplitIteration) {
            //Debug.println("ABORTED! Too many iterations while enforcing constraints");
            //if (!Debug.isDebugging())
				throw new ConstraintEnforcementException<TCoordinate>(
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
	 * private List findMissingConstraints() { List missingSegs = new ArrayList();
	 * for (int i = 0; i < segments.size(); i++) { Segment s = (Segment)
	 * segments.get(i); QuadEdge q = subdiv.locate(s.getStart(), s.getEnd()); if
	 * (q == null) missingSegs.add(s); } return missingSegs; }
	 */

	private int EnforceGabriel(List<Segment<TCoordinate>> segsToInsert)
    {
		List<Segment<TCoordinate>> newSegments = new List<Segment<TCoordinate>>();
		int splits = 0;
        List<Segment<TCoordinate>> segsToRemove = new List<Segment<TCoordinate>>();

		/**
		 * On each iteration must always scan all constraint (sub)segments, since
		 * some constraints may be rebroken by Delaunay triangle flipping caused by
		 * insertion of another constraint. However, this process must converge
		 * eventually, with no splits remaining to find.
		 */
	    foreach (Segment<TCoordinate> seg in segsToInsert)
	    {
	        TCoordinate encroachPt = FindNonGabrielPoint(seg);
			if (Equals(encroachPt, null) || encroachPt.IsEmpty)
				continue;

			// compute split point
			_splitPt = _splitFinder.FindSplitPoint(seg, encroachPt);
			ConstraintVertex<TCoordinate> splitVertex = CreateVertex(_splitPt, seg);

			// DebugFeature.addLineSegment(DEBUG_SEG_SPLIT, encroachPt, splitPt, "");
			// Debug.println(WKTWriter.toLineString(encroachPt, splitPt));

			/**
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
			ConstraintVertex<TCoordinate> insertedVertex = InsertSite(splitVertex);
            if (!insertedVertex.Equals2D(_splitPt))
            {
                Debug.WriteLine("Split pt snapped to: " + insertedVertex);
                // throw new ConstraintEnforcementException("Split point snapped to
                // existing point
                // (tolerance too large or constraint interior narrow angle?)",
                // splitPt);
            }

            // split segment and record the new halves
            Segment<TCoordinate> s1 = new Segment<TCoordinate>(_coordFactory, seg.StartX, seg.StartY, seg.StartZ, 
                splitVertex.X, splitVertex.Y, splitVertex.Z, seg.Data);
            Segment<TCoordinate> s2 = new Segment<TCoordinate>(_coordFactory,splitVertex.X, splitVertex.Y, splitVertex.Z,
                seg.EndX, seg.EndY, seg.EndZ, seg.Data);

            newSegments.Add(s1);
            newSegments.Add(s2);
            segsToRemove.Add(seg);

            splits = splits + 1;
        }

	    foreach (Segment<TCoordinate> seg in segsToRemove)
	        segsToInsert.Remove(seg);
		segsToInsert.AddRange(newSegments);

		return splits;
	}

//	public static final String DEBUG_SEG_SPLIT = "C:\\proj\\CWB\\test\\segSplit.jml";

	/**
	 * Given a set of points stored in the kd-tree and a line segment defined by
	 * two points in this set, finds a {@link Coordinate} in the circumcircle of
	 * the line segment, if one exists. This is called the Gabriel point - if none
	 * exists then the segment is said to have the Gabriel condition. Uses the
	 * heuristic of finding the non-Gabriel point closest to the midpoint of the
	 * segment.
	 * 
	 * @param p
	 *          start of the line segment
	 * @param q
	 *          end of the line segment
	 * @return a point which is non-Gabriel
	 * @return null if no point is non-Gabriel
	 */
	private TCoordinate FindNonGabrielPoint(Segment<TCoordinate> seg)
    {
	    TCoordinate p = seg.Start;
		TCoordinate q = seg.End;
		// Find the mid point on the line and compute the radius of enclosing circle
        TCoordinate midPt = _coordFactory.Create((p[Ordinates.X] + q[Ordinates.X]) / 2.0, (p[Ordinates.Y] + q[Ordinates.Y]) / 2.0);
		Double segRadius = p.Distance(midPt);

		// compute envelope of circumcircle
		IExtents<TCoordinate> env = _geometryFactory.CreateExtents(midPt, midPt);
		env.ExpandBy(segRadius);

        // Find all points in envelope
		// For each point found, test if it falls strictly in the circle
		// find closest point
		TCoordinate closestNonGabriel = default(TCoordinate);
		Double minDist = Double.MaxValue;

	    foreach (KdNode<TCoordinate> nextNode in _kdt.Query(env))
	    {
			TCoordinate testPt = nextNode.Coordinate;
			// ignore segment endpoints
            if (Equals2D(testPt, p) || Equals2D(testPt, q))
				continue;

			double testRadius = midPt.Distance(testPt);
			if (testRadius < segRadius) {
				// double testDist = seg.distance(testPt);
				double testDist = testRadius;
				if (Equals(closestNonGabriel, null) || testDist < minDist) {
					closestNonGabriel = testPt;
					minDist = testDist;
				}
			}
		}
		return closestNonGabriel;
	}

    private Boolean Equals2D(TCoordinate one, TCoordinate other)
    {
        if (ReferenceEquals(one, other))
            return true;

        if (Equals(one, null) && !Equals(other, null))
            return false;

        if (Equals(other, null) && !Equals(one, null))
            return false;

        return one[Ordinates.X] == other[Ordinates.X] &&
               one[Ordinates.Y] == other[Ordinates.Y];
    }

}}
