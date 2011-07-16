using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if !DOTNET40
using C5;
#endif
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate.Quadedge
{
    /// <summary>
    /// A class that contains the {@link QuadEdge}s representing a planar
    /// subdivision that models a triangulation. 
    /// The subdivision is constructed using the
    /// quadedge algebra defined in the classs {@link QuadEdge}. 
    /// All metric calculations
    /// are done in the {@link Vertex} class.
    /// In addition to a triangulation, subdivisions
    /// support extraction of Voronoi diagrams.
    /// This is easily accomplished, since the Voronoi diagram is the dual
    /// of the Delaunay triangulation.
    /// <p>
    /// Subdivisions can be provided with a tolerance value. Inserted vertices which
    /// are closer than this value to vertices already in the subdivision will be
    /// ignored. Using a suitable tolerance value can prevent robustness failures
    /// from happening during Delaunay triangulation.
    /// <p>
    /// Subdivisions maintain a <b>frame</b> triangle around the client-created
    /// edges. The frame is used to provide a bounded "container" for all edges
    /// within a TIN. Normally the frame edges, frame connecting edges, and frame
    /// triangles are not included in client processing.
    /// 
    /// @author David Skea
    /// @author Martin Davis
    ////
    /// </summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public class QuadEdgeSubdivision<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {

	    ///<summary>
	    /// Gets the edges for the triangle to the left of the given <see cref="QuadEdge{TCoordinate}"/>.
	    ///</summary>
	    /// <param name="startQe"></param>
	    /// <param name="triEdge"></param>
        public static void GetTriangleEdges(QuadEdge<TCoordinate> startQe, out QuadEdge<TCoordinate>[] triEdge)
	    {
	        triEdge = new QuadEdge<TCoordinate>[3];
	        triEdge[0] = startQe;
	        triEdge[1] = triEdge[0].Next;
	        triEdge[2] = triEdge[1].Next;
	        if (triEdge[2].Next != triEdge[0])
	            throw new ArgumentException("Edges do not form a triangle");
	    }

        private const double EdgeCoincidenceTolFactor = 1000;

	// debugging only - preserve current subdiv statically
	// private static QuadEdgeSubdivision currentSubdiv;

	// used for edge extraction to ensure edge uniqueness
	private int _visitedKey;
	private readonly List<QuadEdge<TCoordinate>> _quadEdges = new List<QuadEdge<TCoordinate>>();
	private readonly QuadEdge<TCoordinate> _startingEdge;
	private readonly double _tolerance;
	private readonly double _edgeCoincidenceTolerance;
	private readonly Vertex<TCoordinate>[] _frameVertex = new Vertex<TCoordinate>[3];
	private IExtents<TCoordinate> _frameEnv;
	private IQuadEdgeLocator<TCoordinate> _locator;

        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private readonly ICoordinateFactory<TCoordinate> _coordFactory;
	/**
	 * 
	 * 
	 * 
	 * 
	 * @param env
	 *          
	 * @param tolerance
	 *          
	 */
	///<summary>
	/// Creates a new instance of a quad-edge subdivision based on a frame triangle
	/// that encloses a supplied bounding box. A new super-bounding box that
	/// contains the triangle is computed and stored.
	///</summary>
	///<param name="geomFactory">the factory to create geometries</param>
	///<param name="env">the bouding box to surround</param>
	///<param name="tolerance">the tolerance value for determining if two sites are equal</param>
	public QuadEdgeSubdivision(IGeometryFactory<TCoordinate> geomFactory, IExtents<TCoordinate> env, Double tolerance) {
		// currentSubdiv = this;

	    _coordFactory = geomFactory.CoordinateFactory;
	    _geomFactory = geomFactory;

        _tolerance = tolerance;
		_edgeCoincidenceTolerance = tolerance / EdgeCoincidenceTolFactor;

		CreateFrame(env);
		
		_startingEdge = InitSubdiv();
		_locator = new LastFoundQuadEdgeLocator<TCoordinate>(this);
	}

	private void CreateFrame(IExtents<TCoordinate> env)
	{
	    Double minY = env.Min[Ordinates.Y], maxY = env.Max[Ordinates.Y];
	    Double minX = env.Min[Ordinates.X];
	    Double maxX = env.Max[Ordinates.X];

        Double deltaX = maxX - minX;
		Double deltaY = maxY - minY;
		Double offset;
		if (deltaX > deltaY) {
			offset = deltaX * 10.0;
		} else {
			offset = deltaY * 10.0;
		}

		_frameVertex[0] = new Vertex<TCoordinate>(_coordFactory, (maxX + minX) / 2.0, maxY + offset);
		_frameVertex[1] = new Vertex<TCoordinate>(_coordFactory, minX - offset, minY - offset);
		_frameVertex[2] = new Vertex<TCoordinate>(_coordFactory, maxX + offset, minY - offset);

	    _frameEnv = _geomFactory.CreateExtents(_frameVertex[0].Coordinate, _frameVertex[1].Coordinate);
	    _frameEnv.ExpandToInclude(_frameVertex[2].Coordinate);
	}
	
	private QuadEdge<TCoordinate> InitSubdiv()
	{
		// build initial subdivision from frame
		QuadEdge<TCoordinate> ea = MakeEdge(_frameVertex[0], _frameVertex[1]);
		QuadEdge<TCoordinate> eb = MakeEdge(_frameVertex[1], _frameVertex[2]);
		QuadEdge<TCoordinate>.Splice(ea.Sym(), eb);
		QuadEdge<TCoordinate> ec = MakeEdge(_frameVertex[2], _frameVertex[0]);
		QuadEdge<TCoordinate>.Splice(eb.Sym(), ec);
		QuadEdge<TCoordinate>.Splice(ec.Sym(), ea);
		return ea;
	}
	

        ///<summary>
	/// Gets the vertex-equality tolerance value used in this subdivision
	///</summary>
	public Double Tolerance
    {
        get { return _tolerance; }
    }


        ///<summary>
	    /// Gets the envelope of the Subdivision (including the frame).
	    ///</summary>
	    public IExtents<TCoordinate> Extents
        {
		    get { return _geomFactory.CreateExtents(_frameEnv); }
        }


        ///<summary>
	/// Gets the collection of base {@link Quadedge}s (one for every pair of
	/// vertices which is connected).
	///</summary>
	public IEnumerable<QuadEdge<TCoordinate>> Edges
    {
		get { return _quadEdges; }
    }

	///<summary>
	/// Gets/Sets the <see cref="IQuadEdgeLocator{TCoordinate}"/> to use for locating containing triangles
	///</summary>
	public IQuadEdgeLocator<TCoordinate> Locator
    {
		get { return _locator; }
		set { _locator = value; }
    }

	///<summary>Creates a new quadedge, recording it in the edges list.
	///</summary>
	///<param name="o"></param>
	///<param name="d"></param>
	///<returns></returns>
	public QuadEdge<TCoordinate> MakeEdge(Vertex<TCoordinate> o, Vertex<TCoordinate> d) {
		QuadEdge<TCoordinate> q = QuadEdge<TCoordinate>.MakeEdge(o, d);
		_quadEdges.Add(q);
		return q;
	}

	///<summary>Creates a new QuadEdge connecting the destination of a to the origin of b,
	/// in such a way that all three have the same left face after the connection
	/// is complete. The quadedge is recorded in the edges list.
	///</summary>
	///<param name="a"></param>
	///<param name="b"></param>
	///<returns></returns>
	public QuadEdge<TCoordinate> Connect(QuadEdge<TCoordinate> a, QuadEdge<TCoordinate> b) {
		QuadEdge<TCoordinate> q = QuadEdge<TCoordinate>.Connect(a, b);
		_quadEdges.Add(q);
		return q;
	}


        ///<summary> Deletes a quadedge from the subdivision. Linked quadedges are updated to reflect the deletion.
	///</summary>
	///<param name="e">the quadedge to delete</param>
	public void Delete(QuadEdge<TCoordinate> e)
    {
		QuadEdge<TCoordinate>.Splice(e, e.OriginPrev);
		QuadEdge<TCoordinate>.Splice(e.Sym(), e.Sym().OriginPrev);

		QuadEdge<TCoordinate> eSym = e.Sym();
		QuadEdge<TCoordinate> eRot = e.Rot;
		QuadEdge<TCoordinate> eRotSym = e.Rot.Sym();

		// this is inefficient on an ArrayList, but this method should be called infrequently
		_quadEdges.Remove(e);
		_quadEdges.Remove(eSym);
		_quadEdges.Remove(eRot);
		_quadEdges.Remove(eRotSym);

		e.Delete();
		eSym.Delete();
		eRot.Delete();
		eRotSym.Delete();
	}


        ///<summary>
	/// Locates an edge of a triangle which contains a location specified by a Vertex v.
	/// The edge returned has the property that either v is on e, or e is an edge of a 
	/// triangle containing v.
	/// The search starts from startEdge amd proceeds on the general direction of v.
	/// <para>
	/// This locate algorithm relies on the subdivision being Delaunay. For non-Delaunay
	/// subdivisions, this may loop for ever.
	/// </para>
	///</summary>
	///<param name="v">the location to search for</param>
	///<param name="startEdge">an edge of the subdivision to start searching at</param>
	///<returns>a QuadEdge which contains v, or is on the edge of a triangle containing v</returns>
	///<exception cref="LocateFailureException{TCoordinate}">if the location algorithm fails to converge in a reasonable number of iterations</exception>
	public QuadEdge<TCoordinate> LocateFromEdge(Vertex<TCoordinate> v, QuadEdge<TCoordinate> startEdge) {
		int iter = 0;
		int maxIter = _quadEdges.Count;

		QuadEdge<TCoordinate> e = startEdge;

		while (true) 
        {
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
				throw new LocateFailureException<TCoordinate>(e.ToLineSegment());
				// String msg = "Locate failed to converge (at edge: " + e + ").
				// Possible causes include invalid Subdivision topology or very close
				// sites";
				// System.err.println(msg);
				// dumpTriangles();
			}

			if ((v.Equals(e.Origin)) || (v.Equals(e.Destination)))
				break;

            if (v.RightOf(e))
            {
		        e = e.Sym();
		    } 
            else if (!v.RightOf(e.OriginNext))
            {
		        e = e.OriginNext;
		    }
            else if (!v.RightOf(e.DestinationPrev)) 
            {
		        e = e.DestinationPrev;
		    }
            else 
            {
		        // on edge or in triangle containing edge
		        break;
		    }
		}
		// System.out.println("Locate count: " + iter);
		return e;
	}

	///<summary>
	/// Finds a quadedge of a triangle containing a location specified by a <see cref="Vertex{TCoordinate}"/>, if one exists.
	///</summary>
	///<param name="v">the vertex to locate</param>
	///<returns>a quadedge on the edge of a triangle which touches or contains the location </returns>
	/// <returns>null if no such triangle exists</returns>
	public QuadEdge<TCoordinate> Locate(Vertex<TCoordinate> v)
    {
		return _locator.Locate(v);
	}

	///<summary>
	/// Finds a quadedge of a triangle containing a location specified by a <see cref="TCoordinate"/>, if one exists.
	///</summary>
	///<param name="p">the Coordinate to locate</param>
	///<returns></returns>
    ///<returns>a quadedge on the edge of a triangle which touches or contains the location </returns>
    ///<returns>null if no such triangle exists</returns>
    public QuadEdge<TCoordinate> Locate(TCoordinate p)
    {
		return _locator.Locate(new Vertex<TCoordinate>(p));
	}

	///<summary>
	/// Locates the edge between the given vertices, if it exists in the subdivision.
	///</summary>
	///<param name="p0">a coordinate</param>
	///<param name="p1">another coordinate</param>
	///<returns>the edge joining the coordinates, if present</returns>
    ///<returns>null if no such triangle exists</returns>
    public QuadEdge<TCoordinate> Locate(TCoordinate p0, TCoordinate p1)
    {
		// find an edge containing one of the points
		QuadEdge<TCoordinate> e = _locator.Locate(new Vertex<TCoordinate>(p0));
		if (e == null)
			return null;

		// normalize so that p0 is origin of base edge
		QuadEdge<TCoordinate> basis = e;
		if (e.Destination.Coordinate.Equals((ICoordinate2D)p0))
			basis = e.Sym();
		// check all edges around origin of base edge
		QuadEdge<TCoordinate> locEdge = basis;
		do {
			if (locEdge.Destination.Coordinate.Equals((ICoordinate2D)p1))
				return locEdge;
			locEdge = locEdge.OriginNext;
		} while (locEdge != basis);
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
	public QuadEdge<TCoordinate> InsertSite(Vertex<TCoordinate> v) {
		QuadEdge<TCoordinate> e = Locate(v);

		if ((v.Equals(e.Origin, _tolerance)) || (v.Equals(e.Destination, _tolerance))) 
        {
			return e; // point already in subdivision.
		}

		// Connect the new point to the vertices of the containing
		// triangle (or quadrilateral, if the new point fell on an
		// existing edge.)
		QuadEdge<TCoordinate> basis = MakeEdge(e.Origin, v);
		QuadEdge<TCoordinate>.Splice(basis, e);
		QuadEdge<TCoordinate> startEdge = basis;
		do {
			basis = Connect(e, basis.Sym());
			e = basis.OriginPrev;
		} while (e.LeftNext != startEdge);

		return startEdge;
	}

	/**
	 * 
	 * 
	 * @param e
	 *          
	 * @return 
	 */
	///<summary>
	/// Tests whether a QuadEdge is an edge incident on a frame triangle vertex.
	///</summary>
	///<param name="e">the edge to test</param>
	///<returns>true if the edge is connected to the frame triangle</returns>
	public Boolean IsFrameEdge(QuadEdge<TCoordinate> e) {
		if (IsFrameVertex(e.Origin) || IsFrameVertex(e.Destination))
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
	public Boolean isFrameBorderEdge(QuadEdge<TCoordinate> e) {
		// MD debugging
		QuadEdge<TCoordinate>[] leftTri;// = new QuadEdge<TCoordinate>[3];
		GetTriangleEdges(e, out leftTri);
		// System.out.println(new QuadEdgeTriangle(leftTri).toString());
		QuadEdge<TCoordinate>[] rightTri;// = new QuadEdge<TCoordinate>[3];
		GetTriangleEdges(e.Sym(), out rightTri);
		// System.out.println(new QuadEdgeTriangle(rightTri).toString());

		// check other vertex of triangle to left of edge
		Vertex<TCoordinate> vLeftTriOther = e.LeftNext.Destination;
		if (IsFrameVertex(vLeftTriOther))
			return true;
		// check other vertex of triangle to right of edge
		Vertex<TCoordinate> vRightTriOther = e.Sym().LeftNext.Destination;
		if (IsFrameVertex(vRightTriOther))
			return true;

		return false;
	}

	///<summary>Tests whether a vertex is a vertex of the outer triangle.
	///</summary>
	///<param name="v">the vertex to test</param>
	///<returns>true if the vertex is an outer triangle vertex</returns>
	public Boolean IsFrameVertex(Vertex<TCoordinate> v) {
		if (v.Equals(_frameVertex[0]))
			return true;
		if (v.Equals(_frameVertex[1]))
			return true;
		if (v.Equals(_frameVertex[2]))
			return true;
		return false;
	}

        private LineSegment<TCoordinate> _seg;

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
	///<summary>
	///</summary>
	///<param name="e"></param>
	///<param name="p"></param>
	///<returns></returns>
	public Boolean IsOnEdge(QuadEdge<TCoordinate> e, TCoordinate p) {
		_seg = new LineSegment<TCoordinate>(e.Origin.Coordinate, e.Destination.Coordinate);
		Double dist = _seg.Distance(p);
		// heuristic (hack?)
		return dist < _edgeCoincidenceTolerance;
	}

	/**
	 * Tests whether a {@link Vertex} is the start or end vertex of a
	 * {@link QuadEdge<TCoordinate>}, up to the subdivision tolerance distance.
	 * 
	 * @param e
	 * @param v
	 * @return true if the vertex is a endpoint of the edge
	 */
	///<summary>
	///</summary>
	///<param name="e"></param>
	///<param name="v"></param>
	///<returns></returns>
	public Boolean IsVertexOfEdge(QuadEdge<TCoordinate> e, Vertex<TCoordinate> v) {
		if ((v.Equals(e.Origin, _tolerance)) || (v.Equals(e.Destination, _tolerance))) {
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
   * @see #GetVertexUniqueEdges
   */
      ///<summary>
      ///</summary>
      ///<param name="includeFrame"></param>
      ///<returns></returns>
      public IEnumerable<Vertex<TCoordinate>> GetVertices(Boolean includeFrame)
      {
          foreach (QuadEdge<TCoordinate> qe in _quadEdges)
          {
              Vertex<TCoordinate> v = qe.Origin;
              if (includeFrame || IsFrameVertex(v))
                  yield return v;

              /**
               * Inspect the sym edge as well, since it is
               * possible that a vertex is only at the 
               * dest of all tracked quadedges.
               */
              Vertex<TCoordinate> vd = qe.Destination;
              if (includeFrame || ! IsFrameVertex(vd))
                  yield return vd;
          }
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
  ///<summary>
  ///</summary>
  ///<param name="includeFrame"></param>
  ///<returns></returns>
  public IEnumerable<QuadEdge<TCoordinate>> GetVertexUniqueEdges(Boolean includeFrame) 
  {
      SortedList<Int32, Vertex<TCoordinate>> visited = new SortedList<Int32, Vertex<TCoordinate>>();
      foreach (QuadEdge<TCoordinate> qe in _quadEdges)
      {
          Vertex<TCoordinate> v = qe.Origin;
          if (!visited.ContainsValue(v))
          {
              visited.Add(v.GetHashCode(), v);
              if (includeFrame || !IsFrameVertex(v))
                  yield return qe;
          }
          /**
           * Inspect the sym edge as well, since it is
           * possible that a vertex is only at the 
           * dest of all tracked quadedges.
           */
          QuadEdge<TCoordinate> qd = qe.Sym();
          Vertex<TCoordinate> vd = qd.Origin;
          //System.out.println(vd);
          if (! visited.ContainsValue(vd))
          {
      	    visited.Add(vd.GetHashCode(), vd);
            if (includeFrame || ! IsFrameVertex(vd)) {
        	    yield return qd;
            }
          }


      }
  }

	///<summary>
	/// Gets all primary quadedges in the subdivision. A primary edge is a <see cref="QuadEdge{TCoordinate}"/>
	///  which occupies the 0'th position in its array of associated quadedges.
	/// These provide the unique geometric edges of the triangulation.
	///</summary>
	///<param name="includeFrame">true if the frame edges are to be included</param>
	///<returns>a List of QuadEdges</returns>
	public List<QuadEdge<TCoordinate>> GetPrimaryEdges(Boolean includeFrame)
    {
		_visitedKey++;

        List<QuadEdge<TCoordinate>> edges = new List<QuadEdge<TCoordinate>>();
        Stack<QuadEdge<TCoordinate>> edgeStack = new Stack<QuadEdge<TCoordinate>>();
		edgeStack.Push(_startingEdge);

        HashSet<QuadEdge<TCoordinate>> visitedEdges = new HashSet<QuadEdge<TCoordinate>>();

		while (edgeStack.Count>0)
        {
			QuadEdge<TCoordinate> edge = edgeStack.Pop();
			if (! visitedEdges.Contains(edge))
            {
				QuadEdge<TCoordinate> priQE = edge.GetPrimary();

				if (includeFrame || ! IsFrameEdge(priQE))
					edges.Add(priQE);

				edgeStack.Push(edge.OriginNext);
				edgeStack.Push(edge.Sym().OriginNext);
				
				visitedEdges.Add(edge);
				visitedEdges.Add(edge.Sym());
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
	private class TriangleCircumcentreVisitor : ITriangleVisitor<TCoordinate>
	{
	    public void Visit(ICoordinateFactory<TCoordinate> factory, QuadEdge<TCoordinate>[] triEdges) 
		{
			TCoordinate a = triEdges[0].Origin.Coordinate;
			TCoordinate b = triEdges[1].Origin.Coordinate;
			TCoordinate c = triEdges[2].Origin.Coordinate;
			
			// TODO: choose the most accurate circumcentre based on the edges
			TCoordinate cc = Triangle<TCoordinate>.Circumcentre(factory, a, b, c);
			Vertex<TCoordinate> ccVertex = new Vertex<TCoordinate>(cc);

            // save the circumcentre as the origin for the dual edges originating in this triangle
			for (int i = 0; i < 3; i++)
            {
				triEdges[i].Rot.Origin = ccVertex;
			}
		}
	}

	/*****************************************************************************
	 * Visitors
	 ****************************************************************************/

	public void VisitTriangles(ITriangleVisitor<TCoordinate> triVisitor,
			Boolean includeFrame) 
    {
		_visitedKey++;

		// visited flag is used to record visited edges of triangles
		// setVisitedAll(false);
		Stack<QuadEdge<TCoordinate>> edgeStack = new Stack<QuadEdge<TCoordinate>>();
		edgeStack.Push(_startingEdge);

        HashSet<QuadEdge<TCoordinate>> visitedEdges = new HashSet<QuadEdge<TCoordinate>>();
		
		while (edgeStack.Count>0) 
        {
			QuadEdge<TCoordinate> edge = edgeStack.Pop();
			if (! visitedEdges.Contains(edge)) {
				QuadEdge<TCoordinate>[] triEdges = FetchTriangleToVisit(edge, edgeStack,
						includeFrame, visitedEdges);
				if (triEdges != null)
					triVisitor.Visit(_coordFactory, triEdges);
			}
		}
	}

	/**
	 * The quadedges forming a single triangle.
     * Only one visitor is allowed to be active at a
	 * time, so this is safe.
	 */
    private readonly QuadEdge<TCoordinate>[] _triEdges = new QuadEdge<TCoordinate>[3];

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
	private QuadEdge<TCoordinate>[] FetchTriangleToVisit(QuadEdge<TCoordinate> edge, Stack<QuadEdge<TCoordinate>> edgeStack,
			Boolean includeFrame, HashSet<QuadEdge<TCoordinate>> visitedEdges)
    {
		QuadEdge<TCoordinate> curr = edge;
		int edgeCount = 0;
		Boolean isFrame = false;
		do {
			_triEdges[edgeCount] = curr;

			if (IsFrameEdge(curr))
				isFrame = true;
			
			// push sym edges to visit next
			QuadEdge<TCoordinate> sym = curr.Sym();
			if (! visitedEdges.Contains(sym))
				edgeStack.Push(sym);
			
			// mark this edge as visited
			visitedEdges.Add(curr);
			
			edgeCount++;
			curr = curr.LeftNext;
		} while (curr != edge);

		if (isFrame && !includeFrame)
			return null;
		return _triEdges;
	}

	///<summary>
	/// Gets a list of the triangles in the subdivision, specified as
	/// an array of the primary quadedges around the triangle.
	///</summary>
	///<param name="includeFrame">true if the frame triangles should be included</param>
    ///<returns>a List of <see cref="QuadEdge{TCoordinate}"/>[3] arrays</returns>
	public List<QuadEdge<TCoordinate>[]> GetTriangleEdges(Boolean includeFrame)
    {
		TriangleEdgesListVisitor visitor = new TriangleEdgesListVisitor();
		VisitTriangles(visitor, includeFrame);
		return visitor.TriangleEdges;
	}

	private class TriangleEdgesListVisitor : ITriangleVisitor<TCoordinate>
    {
		private List<QuadEdge<TCoordinate>[]> _triList = new List<QuadEdge<TCoordinate>[]>();

		public void Visit(ICoordinateFactory<TCoordinate> coordFactory, QuadEdge<TCoordinate>[] triEdges)
        {
			QuadEdge<TCoordinate>[] clone = new QuadEdge<TCoordinate>[triEdges.Length];
		    for (int i = 0; i < triEdges.Length; i++)
		    {
		        clone[i] = triEdges[i]; //.Clone();
		    }
            _triList.Add(clone);
		}

		public List<QuadEdge<TCoordinate>[]> TriangleEdges
        {
			get { return _triList; }
		}
	}

    ///<summary>
    /// Gets a list of the triangles in the subdivision, specified as an array of the triangle <see cref="Vertex{TCoordinate}"/>es.
    ///</summary>
    ///<param name="includeFrame">true if the frame triangles should be included </param>
    ///<returns>a List of Vertex[3] arrays</returns>
    public List<Vertex<TCoordinate>[]> GetTriangleVertices(Boolean includeFrame)
    {
		TriangleVertexListVisitor visitor = new TriangleVertexListVisitor();
		VisitTriangles(visitor, includeFrame);
		return visitor.GetTriangleVertices;
	}

	private class TriangleVertexListVisitor : ITriangleVisitor<TCoordinate> 
    {
		private readonly List<Vertex<TCoordinate>[]> _triList = new List<Vertex<TCoordinate>[]>();

		public void Visit(ICoordinateFactory<TCoordinate> factory, QuadEdge<TCoordinate>[] triEdges)
        {
			_triList.Add(new Vertex<TCoordinate>[] { triEdges[0].Origin, triEdges[1].Origin,
					triEdges[2].Origin });
		}

		public List<Vertex<TCoordinate>[]> GetTriangleVertices
        {
			get { return _triList; }
        }
	}

	///<summary>
	/// Gets the coordinates for each triangle in the subdivision as an array.
	///</summary>
	///<param name="includeFrame">true if the frame triangles should be included</param>
	///<returns>a list of Coordinate[4] representing each triangle</returns>
	public List<TCoordinate[]> GetTriangleCoordinates(Boolean includeFrame)
    {
		TriangleCoordinatesVisitor visitor = new TriangleCoordinatesVisitor();
		VisitTriangles(visitor, includeFrame);
		return visitor.Triangles;
	}

	private class TriangleCoordinatesVisitor : ITriangleVisitor<TCoordinate>
    {
		private readonly CoordinateList<TCoordinate> coordList = new CoordinateList<TCoordinate>();

        private readonly List<TCoordinate[]> triCoords = new List<TCoordinate[]>();

		public void Visit(ICoordinateFactory<TCoordinate> coordinateFactory, QuadEdge<TCoordinate>[] triEdges) 
        {
			coordList.Clear();
			for (int i = 0; i < 3; i++)
            {
				Vertex<TCoordinate> v = triEdges[i].Origin;
				coordList.Add(v.Coordinate);
			}
			if (coordList.Count > 0) 
            {
				coordList.CloseRing();
				TCoordinate[] pts = coordList.ToArray();
				if (pts.Length != 4) 
                {
                    //String loc = "";
                    //if (pts.Length >= 2)
                    //    loc = WKTWriter.toLineString(pts[0], pts[1]);
                    //else {
                    //    if (pts.length >= 1)
                    //        loc = WKTWriter.toPoint(pts[0]);
                    //}

                    //// Assert.isTrue(pts.length == 4, "Too few points for visited triangle at " + loc);
                    ////com.vividsolutions.jts.util.Debug.println("too few points for triangle at " + loc);
					return;
				}

				triCoords.Add(pts);
			}
		}

        public List<TCoordinate[]> Triangles
        {
            get { return triCoords; }
        }
	}

	/**
	 * Gets the geometry for the edges in the subdivision as a {@link MultiLineString}
	 * containing 2-point lines.
	 * 
	 * @param geomFact the GeometryFactory to use
	 * @return a MultiLineString
	 */
	///<summary>
	///</summary>
	///<returns></returns>
	public IGeometry<TCoordinate> GetEdges()
    {
		List<QuadEdge<TCoordinate>> quadEdges = GetPrimaryEdges(false);
		ILineString<TCoordinate>[] edges = new LineString<TCoordinate>[quadEdges.Count];

        int i = 0;
	    foreach (QuadEdge<TCoordinate> qe in quadEdges)
	        edges[i++] = _geomFactory.CreateLineString(new TCoordinate[] {qe.Origin.Coordinate, qe.Destination.Coordinate});

		return _geomFactory.CreateMultiLineString(edges);
	}

	/**
	 * Gets the geometry for the triangles in a triangulated subdivision as a {@link GeometryCollection}
	 * of triangular {@link Polygon}s.
	 * 
	 * @param geomFact the GeometryFactory to use
	 * @return a GeometryCollection of triangular Polygons
	 */
	///<summary>
	/// 
	///</summary>
	///<returns></returns>
	public IGeometry<TCoordinate> GetTriangles()
    {
		List<TCoordinate[]> triPtsList = GetTriangleCoordinates(false);
		IPolygon<TCoordinate>[] tris = new IPolygon<TCoordinate>[triPtsList.Count];
		int i = 0;
	    foreach (TCoordinate[] triPt in triPtsList)
	        tris[i++] = _geomFactory.CreatePolygon(_geomFactory.CreateLinearRing(triPt), null);

		return _geomFactory.CreateGeometryCollection(tris);
	}

  ///<summary>
  /// Gets the cells in the Voronoi diagram for this triangulation.
  /// The cells are returned as .
  /// The userData of each polygon is set to be the <see cref="TCoordinate"/>
  /// of the cell site.  This allows easily associating external
  /// data associated with the sites to the cells.
  ///</summary>
  ///<returns>a <see cref="IMultiPolygon{TCoordinate}"/></returns>
  public IGeometry<TCoordinate> GetVoronoiDiagram()
  {
    return _geomFactory.CreateGeometryCollection(GetVoronoiCellPolygons());   
  }
  
  ///<summary>
  /// Gets a List of <see cref="IPolygon{TCoordinate}"/>s for the Voronoi cells of this triangulation.
  /// The UserData of each polygon is set to be the <see cref="TCoordinate"/> of the cell site.
  /// This allows easily associating external data associated with the sites to the cells.
  ///</summary>
  ///<returns>an enumeration of polygons</returns>
  public IEnumerable<IGeometry<TCoordinate>> GetVoronoiCellPolygons()
  {
  	/*
  	 * Compute circumcentres of triangles as vertices for dual edges.
  	 * Precomputing the circumcentres is more efficient, 
  	 * and more importantly ensures that the computed centres
  	 * are consistent across the Voronoi cells.
  	 */ 
  	VisitTriangles(new TriangleCircumcentreVisitor(), true);

      foreach (QuadEdge<TCoordinate> qe in GetVertexUniqueEdges(false)    )
      {
          yield return GetVoronoiCellPolygon(qe);
      }
  }
  
  ///<summary>Gets the Voronoi cell around a site specified by the origin of a QuadEdge.
  /// The UserData of the polygon is set to be the <see cref="TCoordinate"/> of the site.
  /// This allows attaching external data associated with the site to this cell polygon.
  ///</summary>
  ///<param name="qe">qe a quadedge originating at the cell site</param>
  ///<returns>a polygon indicating the cell extent</returns>
  public IPolygon<TCoordinate> GetVoronoiCellPolygon(QuadEdge<TCoordinate> qe)
  {
    ICoordinateSequence<TCoordinate> cellPts = _geomFactory.CoordinateSequenceFactory.Create(CoordinateDimensions.Two);
    QuadEdge<TCoordinate> startQE = qe;
    do {
    	// use previously computed circumcentre
    	TCoordinate cc = qe.Rot.Origin.Coordinate;
      cellPts.Add(cc);
      
      // move to next triangle CW around vertex
      qe = qe.OriginPrev;
    } while (qe != startQE);

      cellPts.CloseRing();
      IPolygon<TCoordinate> cellPoly = _geomFactory.CreatePolygon(_geomFactory.CreateLinearRing(cellPts), null);
    
      Vertex<TCoordinate> v = startQE.Origin;
      cellPoly.UserData = v;
      return cellPoly;
  }
        
    }
}
