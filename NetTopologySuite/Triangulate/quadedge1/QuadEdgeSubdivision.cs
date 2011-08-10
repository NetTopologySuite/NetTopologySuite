using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Wintellect.PowerCollections;

namespace NetTopologySuite.Triangulate.QuadEdge{
/**
 * A class that contains the {@link QuadEdge}s representing a planar
 * subdivision that models a triangulation. 
 * The subdivision is constructed using the
 * quadedge algebra defined in the classs {@link QuadEdge}. 
 * All metric calculations
 * are done in the {@link Vertex} class.
 * In addition to a triangulation, subdivisions
 * support extraction of Voronoi diagrams.
 * This is easily accomplished, since the Voronoi diagram is the dual
 * of the Delaunay triangulation.
 * <p>
 * Subdivisions can be provided with a tolerance value. Inserted vertices which
 * are closer than this value to vertices already in the subdivision will be
 * ignored. Using a suitable tolerance value can prevent robustness failures
 * from happening during Delaunay triangulation.
 * <p>
 * Subdivisions maintain a <b>frame</b> triangle around the client-created
 * edges. The frame is used to provide a bounded "container" for all edges
 * within a TIN. Normally the frame edges, frame connecting edges, and frame
 * triangles are not included in client processing.
 * 
 * @author David Skea
 * @author Martin Davis
 */
public class QuadEdgeSubdivision {
	/**
	 * Gets the edges for the triangle to the left of the given {@link QuadEdge}.
	 * 
	 * @param startQE
	 * @param triEdge
	 * 
	 * @throws IllegalArgumentException
	 *           if the edges do not form a triangle
	 */
	public static void getTriangleEdges(QuadEdge startQE, QuadEdge[] triEdge) {
		triEdge[0] = startQE;
		triEdge[1] = triEdge[0].lNext();
		triEdge[2] = triEdge[1].lNext();
		if (triEdge[2].lNext() != triEdge[0])
			throw new ArgumentException("Edges do not form a triangle");
	}

	private readonly static double EDGE_COINCIDENCE_TOL_FACTOR = 1000;

	// debugging only - preserve current subdiv statically
	// private static QuadEdgeSubdivision currentSubdiv;

	// used for edge extraction to ensure edge uniqueness
	private int visitedKey = 0;
//	private Set quadEdges = new HashSet();
	private IList<QuadEdge> quadEdges = new List<QuadEdge>();
	private readonly QuadEdge _startingEdge;
	private readonly double _tolerance;
	private readonly double _edgeCoincidenceTolerance;
	private readonly Vertex[] _frameVertex = new Vertex[3];
	private IEnvelope _frameEnv;
	private IQuadEdgeLocator _locator;

	/**
	 * Creates a new instance of a quad-edge subdivision based on a frame triangle
	 * that encloses a supplied bounding box. A new super-bounding box that
	 * contains the triangle is computed and stored.
	 * 
	 * @param env
	 *          the bouding box to surround
	 * @param tolerance
	 *          the tolerance value for determining if two sites are equal
	 */
	public QuadEdgeSubdivision(IEnvelope env, double tolerance) {
		// currentSubdiv = this;
		this._tolerance = tolerance;
		_edgeCoincidenceTolerance = tolerance / EDGE_COINCIDENCE_TOL_FACTOR;

		createFrame(env);
		
		_startingEdge = initSubdiv();
		_locator = new LastFoundQuadEdgeLocator(this);
	}

	private void createFrame(IEnvelope env)
	{
		double deltaX = env.Width;
		double deltaY = env.Height;
		double offset = 0.0;
		if (deltaX > deltaY) {
			offset = deltaX * 10.0;
		} else {
			offset = deltaY * 10.0;
		}

		_frameVertex[0] = new Vertex((env.MaxX + env.MinX) / 2.0, env
				.MaxY
				+ offset);
		_frameVertex[1] = new Vertex(env.MinX - offset, env.MinY - offset);
		_frameVertex[2] = new Vertex(env.MaxX + offset, env.MinY - offset);

		_frameEnv = new Envelope(_frameVertex[0].Coordinate, _frameVertex[1]
				.Coordinate);
		_frameEnv.ExpandToInclude(_frameVertex[2].Coordinate);
	}
	
	private QuadEdge initSubdiv()
	{
		// build initial subdivision from frame
		QuadEdge ea = makeEdge(_frameVertex[0], _frameVertex[1]);
		QuadEdge eb = makeEdge(_frameVertex[1], _frameVertex[2]);
		QuadEdge.Splice(ea.Sym(), eb);
		QuadEdge ec = makeEdge(_frameVertex[2], _frameVertex[0]);
		QuadEdge.Splice(eb.Sym(), ec);
		QuadEdge.Splice(ec.Sym(), ea);
		return ea;
	}
	
	/**
	 * Gets the vertex-equality tolerance value
	 * used in this subdivision
	 * 
	 * @return the tolerance value
	 */
	public double getTolerance() {
		return _tolerance;
	}

	/**
	 * Gets the envelope of the Subdivision (including the frame).
	 * 
	 * @return the envelope
	 */
	public IEnvelope getEnvelope() {
		return new Envelope(_frameEnv);
	}

	/**
	 * Gets the collection of base {@link Quadedge}s (one for every pair of
	 * vertices which is connected).
	 * 
	 * @return a collection of QuadEdges
	 */
	public IList<QuadEdge> getEdges() {
		return quadEdges;
	}

	/**
	 * Sets the {@link QuadEdgeLocator} to use for locating containing triangles
	 * in this subdivision.
	 * 
	 * @param locator
	 *          a QuadEdgeLocator
	 */
	public void setLocator(IQuadEdgeLocator locator) {
		this._locator = locator;
	}

	/**
	 * Creates a new quadedge, recording it in the edges list.
	 * 
	 * @param o
	 * @param d
	 * @return
	 */
	public QuadEdge makeEdge(Vertex o, Vertex d) {
		QuadEdge q = QuadEdge.MakeEdge(o, d);
		quadEdges.Add(q);
		return q;
	}

	/**
	 * Creates a new QuadEdge connecting the destination of a to the origin of b,
	 * in such a way that all three have the same left face after the connection
	 * is complete. The quadedge is recorded in the edges list.
	 * 
	 * @param a
	 * @param b
	 * @return
	 */
	public QuadEdge connect(QuadEdge a, QuadEdge b) {
		QuadEdge q = QuadEdge.Connect(a, b);
		quadEdges.Add(q);
		return q;
	}

	/**
	 * Deletes a quadedge from the subdivision. Linked quadedges are updated to
	 * reflect the deletion.
	 * 
	 * @param e
	 *          the quadedge to delete
	 */
	public void delete(QuadEdge e) {
		QuadEdge.Splice(e, e.oPrev());
		QuadEdge.Splice(e.Sym(), e.Sym().oPrev());

		QuadEdge eSym = e.Sym();
		QuadEdge eRot = e.Rot;
		QuadEdge eRotSym = e.Rot.Sym();

		// this is inefficient on an ArrayList, but this method should be called infrequently
		quadEdges.Remove(e);
		quadEdges.Remove(eSym);
		quadEdges.Remove(eRot);
		quadEdges.Remove(eRotSym);

		e.delete();
		eSym.delete();
		eRot.delete();
		eRotSym.delete();
	}

	/**
	 * Locates an edge of a triangle which contains a location 
	 * specified by a Vertex v. 
	 * The edge returned has the
	 * property that either v is on e, or e is an edge of a triangle containing v.
	 * The search starts from startEdge amd proceeds on the general direction of v.
	 * <p>
	 * This locate algorithm relies on the subdivision being Delaunay. For
	 * non-Delaunay subdivisions, this may loop for ever.
	 * 
	 * @param v the location to search for
	 * @param startEdge an edge of the subdivision to start searching at
	 * @returns a QuadEdge which contains v, or is on the edge of a triangle containing v
	 * @throws LocateFailureException
	 *           if the location algorithm fails to converge in a reasonable
	 *           number of iterations
	 */
	public QuadEdge locateFromEdge(Vertex v, QuadEdge startEdge) {
		int iter = 0;
		int maxIter = quadEdges.Count;

		QuadEdge e = startEdge;

		while (true) {
			iter++;

			/**
			 * So far it has always been the case that failure to locate indicates an
			 * invalid subdivision. So just fail completely. (An alternative would be
			 * to perform an exhaustive search for the containing triangle, but this
			 * would mask errors in the subdivision topology)
			 * 
			 * This can also happen if two vertices are located very close together,
			 * since the orientation predicates may experience precision failures.
			 */
			if (iter > maxIter) {
				throw new LocateFailureException(e.ToLineSegment());
				// String msg = "Locate failed to converge (at edge: " + e + ").
				// Possible causes include invalid Subdivision topology or very close
				// sites";
				// System.err.println(msg);
				// dumpTriangles();
			}

			if ((v.equals(e.Orig)) || (v.equals(e.Dest))) {
				break;
			} else if (v.RightOf(e)) {
				e = e.Sym();
			} else if (!v.RightOf(e.oNext())) {
				e = e.oNext();
			} else if (!v.RightOf(e.dPrev())) {
				e = e.dPrev();
			} else {
				// on edge or in triangle containing edge
				break;
			}
		}
		// System.out.println("Locate count: " + iter);
		return e;
	}

	/**
	 * Finds a quadedge of a triangle containing a location 
	 * specified by a {@link Vertex}, if one exists.
	 * 
	 * @param x the vertex to locate
	 * @return a quadedge on the edge of a triangle which touches or contains the location
	 * @return null if no such triangle exists
	 */
	public QuadEdge Locate(Vertex v) {
		return _locator.Locate(v);
	}

	/**
	 * Finds a quadedge of a triangle containing a location
	 * specified by a {@link Coordinate}, if one exists.
	 * 
	 * @param p the Coordinate to locate
	 * @return a quadedge on the edge of a triangle which touches or contains the location
	 * @return null if no such triangle exists
	 */
	public QuadEdge Locate(Coordinate p) {
		return _locator.Locate(new Vertex(p));
	}

	/**
	 * Locates the edge between the given vertices, if it exists in the
	 * subdivision.
	 * 
	 * @param p0 a coordinate
	 * @param p1 another coordinate
	 * @return the edge joining the coordinates, if present
	 * @return null if no such edge exists
	 */
	public QuadEdge Locate(Coordinate p0, Coordinate p1) {
		// find an edge containing one of the points
		QuadEdge e = _locator.Locate(new Vertex(p0));
		if (e == null)
			return null;

		// normalize so that p0 is origin of base edge
		QuadEdge baseQE = e;
		if (e.Dest.Coordinate.Equals2D(p0))
			baseQE = e.Sym();
		// check all edges around origin of base edge
		QuadEdge locEdge = baseQE;
		do {
			if (locEdge.Dest.Coordinate.Equals2D(p1))
				return locEdge;
			locEdge = locEdge.oNext();
		} while (locEdge != baseQE);
		return null;
	}

	/**
	 * Inserts a new site into the Subdivision, connecting it to the vertices of
	 * the containing triangle (or quadrilateral, if the split point falls on an
	 * existing edge).
	 * <p>
	 * This method does NOT maintain the Delaunay condition. If desired, this must
	 * be checked and enforced by the caller.
	 * <p>
	 * This method does NOT check if the inserted vertex falls on an edge. This
	 * must be checked by the caller, since this situation may cause erroneous
	 * triangulation
	 * 
	 * @param v
	 *          the vertex to insert
	 * @return a new quad edge terminating in v
	 */
	public QuadEdge insertSite(Vertex v) {
		QuadEdge e = Locate(v);

		if ((v.equals(e.Orig, _tolerance)) || (v.equals(e.Dest, _tolerance))) {
			return e; // point already in subdivision.
		}

		// Connect the new point to the vertices of the containing
		// triangle (or quadrilateral, if the new point fell on an
		// existing edge.)
		QuadEdge baseQE = makeEdge(e.Orig, v);
		QuadEdge.Splice(baseQE, e);
		QuadEdge startEdge = baseQE;
		do {
			baseQE = connect(e, baseQE.Sym());
			e = baseQE.oPrev();
		} while (e.lNext() != startEdge);

		return startEdge;
	}

	/**
	 * Tests whether a QuadEdge is an edge incident on a frame triangle vertex.
	 * 
	 * @param e
	 *          the edge to test
	 * @return true if the edge is connected to the frame triangle
	 */
	public bool isFrameEdge(QuadEdge e) {
		if (isFrameVertex(e.Orig) || isFrameVertex(e.Dest))
			return true;
		return false;
	}

	/**
	 * Tests whether a QuadEdge is an edge on the border of the frame facets and
	 * the internal facets. E.g. an edge which does not itself touch a frame
	 * vertex, but which touches an edge which does.
	 * 
	 * @param e
	 *          the edge to test
	 * @return true if the edge is on the border of the frame
	 */
	public boolean isFrameBorderEdge(QuadEdge e) {
		// MD debugging
		QuadEdge[] leftTri = new QuadEdge[3];
		getTriangleEdges(e, leftTri);
		// System.out.println(new QuadEdgeTriangle(leftTri).toString());
		QuadEdge[] rightTri = new QuadEdge[3];
		getTriangleEdges(e.sym(), rightTri);
		// System.out.println(new QuadEdgeTriangle(rightTri).toString());

		// check other vertex of triangle to left of edge
		Vertex vLeftTriOther = e.lNext().dest();
		if (isFrameVertex(vLeftTriOther))
			return true;
		// check other vertex of triangle to right of edge
		Vertex vRightTriOther = e.sym().lNext().dest();
		if (isFrameVertex(vRightTriOther))
			return true;

		return false;
	}

	/**
	 * Tests whether a vertex is a vertex of the outer triangle.
	 * 
	 * @param v
	 *          the vertex to test
	 * @return true if the vertex is an outer triangle vertex
	 */
	public bool isFrameVertex(Vertex v) {
		if (v.equals(_frameVertex[0]))
			return true;
		if (v.equals(_frameVertex[1]))
			return true;
		if (v.equals(_frameVertex[2]))
			return true;
		return false;
	}

	private LineSegment seg = new LineSegment();

	/**
	 * Tests whether a {@link Coordinate} lies on a {@link QuadEdge}, up to a
	 * tolerance determined by the subdivision tolerance.
	 * 
	 * @param e
	 *          a QuadEdge
	 * @param p
	 *          a point
	 * @return true if the vertex lies on the edge
	 */
	public bool isOnEdge(QuadEdge e, Coordinate p) {
		seg.SetCoordinates(e.Orig.Coordinate, e.Dest.Coordinate);
		double dist = seg.Distance(p);
		// heuristic (hack?)
		return dist < _edgeCoincidenceTolerance;
	}

	/**
	 * Tests whether a {@link Vertex} is the start or end vertex of a
	 * {@link QuadEdge}, up to the subdivision tolerance distance.
	 * 
	 * @param e
	 * @param v
	 * @return true if the vertex is a endpoint of the edge
	 */
	public bool isVertexOfEdge(QuadEdge e, Vertex v) {
		if ((v.equals(e.Orig, _tolerance)) || (v.equals(e.Dest, _tolerance))) {
			return true;
		}
		return false;
	}

  /**
   * Gets the unique {@link Vertex}es in the subdivision,
   * including the frame vertices if desired.
   * 
	 * @param includeFrame
	 *          true if the frame vertices should be included
   * @return a collection of the subdivision vertices
   * 
   * @see #getVertexUniqueEdges
   */
  public Collection getVertices(bool includeFrame) 
  {
    Set vertices = new HashSet();
    for (Iterator i = quadEdges.iterator(); i.hasNext();) {
      QuadEdge qe = (QuadEdge) i.next();
      Vertex v = qe.orig();
      //System.out.println(v);
      if (includeFrame || ! isFrameVertex(v))
        vertices.add(v);
      
      /**
       * Inspect the sym edge as well, since it is
       * possible that a vertex is only at the 
       * dest of all tracked quadedges.
       */
      Vertex vd = qe.dest();
      //System.out.println(vd);
      if (includeFrame || ! isFrameVertex(vd))
        vertices.add(vd);
    }
    return vertices;
  }

  /**
   * Gets a collection of {@link QuadEdge}s whose origin
   * vertices are a unique set which includes
   * all vertices in the subdivision. 
   * The frame vertices can be included if required.
   * <p>
   * This is useful for algorithms which require traversing the 
   * subdivision starting at all vertices.
   * Returning a quadedge for each vertex
   * is more efficient than 
   * the alternative of finding the actual vertices
   * using {@link #getVertices) and then locating 
   * quadedges attached to them.
   * 
   * @param includeFrame true if the frame vertices should be included
   * @return a collection of QuadEdge with the vertices of the subdivision as their origins
   */
  public IList<QuadEdge> getVertexUniqueEdges(bool includeFrame) 
  {
  	var edges = new List<QuadEdge>();
    var visitedVertices = new HashSet<Vertex>();
      foreach(var qe in quadEdges)
      {
          Vertex v = qe.Orig;
      //System.out.println(v);
      if (! visitedVertices.Contains(v)) {
      	visitedVertices.Add(v);
        if (includeFrame || ! isFrameVertex(v)) {
        	edges.Add(qe);
        }
      }
      
      /**
       * Inspect the sym edge as well, since it is
       * possible that a vertex is only at the 
       * dest of all tracked quadedges.
       */
      QuadEdge qd = qe.Sym();
      Vertex vd = qd.Orig;
      //System.out.println(vd);
      if (! visitedVertices.Contains(vd)) {
      	visitedVertices.Add(vd);
        if (includeFrame || ! isFrameVertex(vd)) {
        	edges.Add(qd);
        }
      }
    }
    return edges;
  }

	/**
	 * Gets all primary quadedges in the subdivision. 
   * A primary edge is a {@link QuadEdge}
	 * which occupies the 0'th position in its array of associated quadedges. 
	 * These provide the unique geometric edges of the triangulation.
	 * 
	 * @param includeFrame true if the frame edges are to be included
	 * @return a List of QuadEdges
	 */
	public List getPrimaryEdges(boolean includeFrame) {
		visitedKey++;

		List edges = new ArrayList();
		Stack edgeStack = new Stack();
		edgeStack.push(_startingEdge);
		
		Set visitedEdges = new HashSet();

		while (!edgeStack.empty()) {
			QuadEdge edge = (QuadEdge) edgeStack.pop();
			if (! visitedEdges.contains(edge)) {
				QuadEdge priQE = edge.getPrimary();

				if (includeFrame || ! isFrameEdge(priQE))
					edges.add(priQE);

				edgeStack.push(edge.oNext());
				edgeStack.push(edge.sym().oNext());
				
				visitedEdges.add(edge);
				visitedEdges.add(edge.sym());
			}
		}
		return edges;
	}
  
  /**
   * A TriangleVisitor which computes and sets the 
   * circumcentre as the origin of the dual 
   * edges originating in each triangle.
   * 
   * @author mbdavis
   *
   */
	private static class TriangleCircumcentreVisitor implements TriangleVisitor 
	{
		public TriangleCircumcentreVisitor() {
		}

		public void visit(QuadEdge[] triEdges) 
		{
			Coordinate a = triEdges[0].orig().getCoordinate();
			Coordinate b = triEdges[1].orig().getCoordinate();
			Coordinate c = triEdges[2].orig().getCoordinate();
			
			// TODO: choose the most accurate circumcentre based on the edges
			Coordinate cc = Triangle.circumcentre(a, b, c);
			Vertex ccVertex = new Vertex(cc);
			// save the circumcentre as the origin for the dual edges originating in this triangle
			for (int i = 0; i < 3; i++) {
				triEdges[i].rot().setOrig(ccVertex);
			}
		}
	}

	/*****************************************************************************
	 * Visitors
	 ****************************************************************************/

	public void visitTriangles(TriangleVisitor triVisitor,
			boolean includeFrame) {
		visitedKey++;

		// visited flag is used to record visited edges of triangles
		// setVisitedAll(false);
		Stack edgeStack = new Stack();
		edgeStack.push(startingEdge);

		Set visitedEdges = new HashSet();
		
		while (!edgeStack.empty()) {
			QuadEdge edge = (QuadEdge) edgeStack.pop();
			if (! visitedEdges.contains(edge)) {
				QuadEdge[] triEdges = fetchTriangleToVisit(edge, edgeStack,
						includeFrame, visitedEdges);
				if (triEdges != null)
					triVisitor.visit(triEdges);
			}
		}
	}

	/**
	 * The quadedges forming a single triangle.
   * Only one visitor is allowed to be active at a
	 * time, so this is safe.
	 */
	private QuadEdge[] triEdges = new QuadEdge[3];

	/**
	 * Stores the edges for a visited triangle. Also pushes sym (neighbour) edges
	 * on stack to visit later.
	 * 
	 * @param edge
	 * @param edgeStack
	 * @param includeFrame
	 * @return the visited triangle edges
	 * @return null if the triangle should not be visited (for instance, if it is
	 *         outer)
	 */
	private QuadEdge[] fetchTriangleToVisit(QuadEdge edge, Stack edgeStack,
			boolean includeFrame, Set visitedEdges) {
		QuadEdge curr = edge;
		int edgeCount = 0;
		boolean isFrame = false;
		do {
			triEdges[edgeCount] = curr;

			if (isFrameEdge(curr))
				isFrame = true;
			
			// push sym edges to visit next
			QuadEdge sym = curr.sym();
			if (! visitedEdges.contains(sym))
				edgeStack.push(sym);
			
			// mark this edge as visited
			visitedEdges.add(curr);
			
			edgeCount++;
			curr = curr.lNext();
		} while (curr != edge);

		if (isFrame && !includeFrame)
			return null;
		return triEdges;
	}

	/**
	 * Gets a list of the triangles
	 * in the subdivision, specified as
	 * an array of the primary quadedges around the triangle.
	 * 
	 * @param includeFrame
	 *          true if the frame triangles should be included
	 * @return a List of QuadEdge[3] arrays
	 */
	public List getTriangleEdges(boolean includeFrame) {
		TriangleEdgesListVisitor visitor = new TriangleEdgesListVisitor();
		visitTriangles(visitor, includeFrame);
		return visitor.getTriangleEdges();
	}

	private static class TriangleEdgesListVisitor implements TriangleVisitor {
		private List triList = new ArrayList();

		public void visit(QuadEdge[] triEdges) {
			triList.add(triEdges.clone());
		}

		public List getTriangleEdges() {
			return triList;
		}
	}

	/**
	 * Gets a list of the triangles in the subdivision,
	 * specified as an array of the triangle {@link Vertex}es.
	 * 
	 * @param includeFrame
	 *          true if the frame triangles should be included
	 * @return a List of Vertex[3] arrays
	 */
	public List getTriangleVertices(boolean includeFrame) {
		TriangleVertexListVisitor visitor = new TriangleVertexListVisitor();
		visitTriangles(visitor, includeFrame);
		return visitor.getTriangleVertices();
	}

	private static class TriangleVertexListVisitor implements TriangleVisitor {
		private List triList = new ArrayList();

		public void visit(QuadEdge[] triEdges) {
			triList.add(new Vertex[] { triEdges[0].orig(), triEdges[1].orig(),
					triEdges[2].orig() });
		}

		public List getTriangleVertices() {
			return triList;
		}
	}

	/**
	 * Gets the coordinates for each triangle in the subdivision as an array.
	 * 
	 * @param includeFrame
	 *          true if the frame triangles should be included
	 * @return a list of Coordinate[4] representing each triangle
	 */
	public List getTriangleCoordinates(boolean includeFrame) {
		TriangleCoordinatesVisitor visitor = new TriangleCoordinatesVisitor();
		visitTriangles(visitor, includeFrame);
		return visitor.getTriangles();
	}

	private static class TriangleCoordinatesVisitor implements TriangleVisitor {
		private CoordinateList coordList = new CoordinateList();

		private List triCoords = new ArrayList();

		public TriangleCoordinatesVisitor() {
		}

		public void visit(QuadEdge[] triEdges) {
			coordList.clear();
			for (int i = 0; i < 3; i++) {
				Vertex v = triEdges[i].orig();
				coordList.add(v.getCoordinate());
			}
			if (coordList.size() > 0) {
				coordList.closeRing();
				Coordinate[] pts = coordList.toCoordinateArray();
				if (pts.length != 4) {
					String loc = "";
					if (pts.length >= 2)
						loc = WKTWriter.toLineString(pts[0], pts[1]);
					else {
						if (pts.length >= 1)
							loc = WKTWriter.toPoint(pts[0]);
					}

					// Assert.isTrue(pts.length == 4, "Too few points for visited triangle at " + loc);
					//com.vividsolutions.jts.util.Debug.println("too few points for triangle at " + loc);
					return;
				}

				triCoords.add(pts);
			}
		}

		public List getTriangles() {
			return triCoords;
		}
	}

	/**
	 * Gets the geometry for the edges in the subdivision as a {@link MultiLineString}
	 * containing 2-point lines.
	 * 
	 * @param geomFact the GeometryFactory to use
	 * @return a MultiLineString
	 */
	public Geometry getEdges(GeometryFactory geomFact) {
		List quadEdges = getPrimaryEdges(false);
		LineString[] edges = new LineString[quadEdges.size()];
		int i = 0;
		for (Iterator it = quadEdges.iterator(); it.hasNext();) {
			QuadEdge qe = (QuadEdge) it.next();
			edges[i++] = geomFact.createLineString(new Coordinate[] {
					qe.orig().getCoordinate(), qe.dest().getCoordinate() });
		}
		return geomFact.createMultiLineString(edges);
	}

	/**
	 * Gets the geometry for the triangles in a triangulated subdivision as a {@link GeometryCollection}
	 * of triangular {@link Polygon}s.
	 * 
	 * @param geomFact the GeometryFactory to use
	 * @return a GeometryCollection of triangular Polygons
	 */
	public Geometry getTriangles(GeometryFactory geomFact) {
		List triPtsList = getTriangleCoordinates(false);
		Polygon[] tris = new Polygon[triPtsList.size()];
		int i = 0;
		for (Iterator it = triPtsList.iterator(); it.hasNext();) {
			Coordinate[] triPt = (Coordinate[]) it.next();
			tris[i++] = geomFact
					.createPolygon(geomFact.createLinearRing(triPt), null);
		}
		return geomFact.createGeometryCollection(tris);
	}

	/**
	 * Gets the cells in the Voronoi diagram for this triangulation.
	 * The cells are returned as a {@link GeometryCollection} of {@link Polygon}s
   * <p>
   * The userData of each polygon is set to be the {@link Coordinate)
   * of the cell site.  This allows easily associating external 
   * data associated with the sites to the cells.
	 * 
	 * @param geomFact a geometry factory
	 * @return a GeometryCollection of Polygons
	 */
  public Geometry getVoronoiDiagram(GeometryFactory geomFact)
  {
    List vorCells = getVoronoiCellPolygons(geomFact);
    return geomFact.createGeometryCollection(GeometryFactory.toGeometryArray(vorCells));   
  }
  
	/**
	 * Gets a List of {@link Polygon}s for the Voronoi cells 
	 * of this triangulation.
   * <p>
   * The userData of each polygon is set to be the {@link Coordinate)
   * of the cell site.  This allows easily associating external 
   * data associated with the sites to the cells.
	 * 
	 * @param geomFact a geometry factory
	 * @return a List of Polygons
	 */
  public List getVoronoiCellPolygons(GeometryFactory geomFact)
  {
  	/*
  	 * Compute circumcentres of triangles as vertices for dual edges.
  	 * Precomputing the circumcentres is more efficient, 
  	 * and more importantly ensures that the computed centres
  	 * are consistent across the Voronoi cells.
  	 */ 
  	visitTriangles(new TriangleCircumcentreVisitor(), true);
  	
    List cells = new ArrayList();
    Collection edges = getVertexUniqueEdges(false);
    for (Iterator i = edges.iterator(); i.hasNext(); ) {
    	QuadEdge qe = (QuadEdge) i.next();
      cells.add(getVoronoiCellPolygon(qe, geomFact));
    }
    return cells;
  }
  
  /**
   * Gets the Voronoi cell around a site specified
   * by the origin of a QuadEdge.
   * <p>
   * The userData of the polygon is set to be the {@link Coordinate)
   * of the site.  This allows attaching external 
   * data associated with the site to this cell polygon.
   * 
   * @param qe a quadedge originating at the cell site
   * @param geomFact a factory for building the polygon
   * @return a polygon indicating the cell extent
   */
  public Polygon getVoronoiCellPolygon(QuadEdge qe, GeometryFactory geomFact)
  {
    List cellPts = new ArrayList();
    QuadEdge startQE = qe;
    do {
//    	Coordinate cc = circumcentre(qe);
    	// use previously computed circumcentre
    	Coordinate cc = qe.rot().orig().getCoordinate();
      cellPts.add(cc);
      
      // move to next triangle CW around vertex
      qe = qe.oPrev();
    } while (qe != startQE);
    
    CoordinateList coordList = new CoordinateList();
    coordList.addAll(cellPts, false);
    coordList.closeRing();
    Coordinate[] pts = coordList.toCoordinateArray();
    Polygon cellPoly = geomFact.createPolygon(geomFact.createLinearRing(pts), null);
    
    Vertex v = startQE.orig();
    cellPoly.setUserData(v.getCoordinate());
    return cellPoly;
  }
  
}}