using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// A class that contains the <see cref="QuadEdge"/>s representing a planar
    /// subdivision that models a triangulation.
    /// The subdivision is constructed using the
    /// quadedge algebra defined in the class <see cref="QuadEdge"/>.
    /// All metric calculations
    /// are done in the <see cref="Vertex"/> class.
    /// In addition to a triangulation, subdivisions
    /// support extraction of Voronoi diagrams.
    /// This is easily accomplished, since the Voronoi diagram is the dual
    /// of the Delaunay triangulation.
    /// <para>
    /// Subdivisions can be provided with a tolerance value. Inserted vertices which
    /// are closer than this value to vertices already in the subdivision will be
    /// ignored. Using a suitable tolerance value can prevent robustness failures
    /// from happening during Delaunay triangulation.
    /// </para>
    /// <para>
    /// Subdivisions maintain a <b>frame</b> triangle around the client-created
    /// edges. The frame is used to provide a bounded "container" for all edges
    /// within a TIN. Normally the frame edges, frame connecting edges, and frame
    /// triangles are not included in client processing.
    /// </para>
    /// </summary>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class QuadEdgeSubdivision
    {
        /// <summary>
        /// Gets the edges for the triangle to the left of the given <see cref="QuadEdge"/>.
        /// </summary>
        /// <param name="startQE" />
        /// <param name="triEdge" />
        /// <exception cref="ArgumentException">if the edges do not form a triangle</exception>
        public static void GetTriangleEdges(QuadEdge startQE, QuadEdge[] triEdge)
        {
            triEdge[0] = startQE;
            triEdge[1] = triEdge[0].LNext;
            triEdge[2] = triEdge[1].LNext;
            if (triEdge[2].LNext != triEdge[0])
                throw new ArgumentException("Edges do not form a triangle");
        }

        private const double EdgeCoincidenceToleranceFactor = 1000;

        // debugging only - preserve current subdiv statically
        // private static QuadEdgeSubdivision currentSubdiv;

        // used for edge extraction to ensure edge uniqueness
        private int _visitedKey;
        //private Set quadEdges = new HashSet();
        private readonly List<QuadEdge> _quadEdges = new List<QuadEdge>();
        private readonly QuadEdge _startingEdge;
        private readonly double _tolerance;
        private readonly double _edgeCoincidenceTolerance;
        private readonly Vertex[] _frameVertex = new Vertex[3];
        private Envelope _frameEnv;
        private IQuadEdgeLocator _locator;

        /// <summary>
        /// Creates a new instance of a quad-edge subdivision based on a frame triangle
        /// that encloses a supplied bounding box. A new super-bounding box that
        /// contains the triangle is computed and stored.
        /// </summary>
        /// <param name="env">the bounding box to surround</param>
        /// <param name="tolerance">the tolerance value for determining if two sites are equal</param>
        public QuadEdgeSubdivision(Envelope env, double tolerance)
        {
            // currentSubdiv = this;
            _tolerance = tolerance;
            _edgeCoincidenceTolerance = tolerance / EdgeCoincidenceToleranceFactor;

            CreateFrame(env);

            _startingEdge = InitSubdiv();
            _locator = new LastFoundQuadEdgeLocator(this);
        }

        private void CreateFrame(Envelope env)
        {
            double deltaX = env.Width;
            double deltaY = env.Height;
            double offset;
            if (deltaX > deltaY)
            {
                offset = deltaX * 10.0;
            }
            else
            {
                offset = deltaY * 10.0;
            }

            _frameVertex[0] = new Vertex((env.MaxX + env.MinX) / 2.0, env.MaxY + offset);
            _frameVertex[1] = new Vertex(env.MinX - offset, env.MinY - offset);
            _frameVertex[2] = new Vertex(env.MaxX + offset, env.MinY - offset);

            _frameEnv = new Envelope(_frameVertex[0].Coordinate, _frameVertex[1].Coordinate);
            _frameEnv.ExpandToInclude(_frameVertex[2].Coordinate);
        }

        private QuadEdge InitSubdiv()
        {
            // build initial subdivision from frame
            var ea = MakeEdge(_frameVertex[0], _frameVertex[1]);
            var eb = MakeEdge(_frameVertex[1], _frameVertex[2]);
            QuadEdge.Splice(ea.Sym, eb);
            var ec = MakeEdge(_frameVertex[2], _frameVertex[0]);
            QuadEdge.Splice(eb.Sym, ec);
            QuadEdge.Splice(ec.Sym, ea);
            return ea;
        }

        /// <summary>
        /// Gets the vertex-equality tolerance value
        /// used in this subdivision
        /// </summary>
        /// <remarks>Gets the tolerance value</remarks>
        public double Tolerance => _tolerance;

        /// <summary>
        /// Gets the envelope of the Subdivision (including the frame).
        /// </summary>
        /// <remarks>Gets the envelope</remarks>
        public Envelope Envelope => new Envelope(_frameEnv);

        /// <summary>
        /// Gets the collection of base <see cref="QuadEdge"/>s (one for every pair of
        /// vertices which is connected).
        /// </summary>
        /// <returns>a collection of QuadEdges</returns>
        public IList<QuadEdge> GetEdges()
        {
            return _quadEdges;
        }

        /// <summary>
        /// Sets the <see cref="IQuadEdgeLocator"/> to use for locating containing triangles
        /// in this subdivision.
        /// </summary>
        /// <param name="locator">a QuadEdgeLocator</param>
        public void SetLocator(IQuadEdgeLocator locator)
        {
            _locator = locator;
        }

        /// <summary>
        /// Creates a new quadedge, recording it in the edges list.
        /// </summary>
        /// <param name="o">The origin vertex</param>
        /// <param name="d">The destination vertex</param>
        /// <returns>A new quadedge</returns>
        public QuadEdge MakeEdge(Vertex o, Vertex d)
        {
            var q = QuadEdge.MakeEdge(o, d);
            _quadEdges.Add(q);
            return q;
        }

        /// <summary>
        /// Creates a new QuadEdge connecting the destination of a to the origin of b,
        /// in such a way that all three have the same left face after the connection
        /// is complete. The quadedge is recorded in the edges list.
        /// </summary>
        /// <param name="a">A quadedge</param>
        /// <param name="b">A quadedge</param>
        /// <returns>A quadedge</returns>
        public QuadEdge Connect(QuadEdge a, QuadEdge b)
        {
            var q = QuadEdge.Connect(a, b);
            _quadEdges.Add(q);
            return q;
        }

        /// <summary>
        /// Deletes a quadedge from the subdivision. Linked quadedges are updated to
        /// reflect the deletion.
        /// </summary>
        /// <param name="e">the quadedge to delete</param>
        public void Delete(QuadEdge e)
        {
            QuadEdge.Splice(e, e.OPrev);
            QuadEdge.Splice(e.Sym, e.Sym.OPrev);

            var eSym = e.Sym;
            var eRot = e.Rot;
            var eRotSym = e.Rot.Sym;

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

        /// <summary>
        /// Locates an edge of a triangle which contains a location
        /// specified by a Vertex v.
        /// The edge returned has the
        /// property that either v is on e, or e is an edge of a triangle containing v.
        /// The search starts from startEdge amd proceeds on the general direction of v.
        /// </summary>
        /// <remarks>
        /// This locate algorithm relies on the subdivision being Delaunay. For
        /// non-Delaunay subdivisions, this may loop for ever.
        /// </remarks>
        /// <param name="v">the location to search for</param>
        /// <param name="startEdge">an edge of the subdivision to start searching at</param>
        /// <returns>a QuadEdge which contains v, or is on the edge of a triangle containing v</returns>
        /// <exception cref="LocateFailureException">
        /// if the location algorithm fails to converge in a reasonable
        /// number of iterations
        /// </exception>
        public QuadEdge LocateFromEdge(Vertex v, QuadEdge startEdge)
        {
            int iter = 0;
            int maxIter = _quadEdges.Count;

            var e = startEdge;

            while (true)
            {
                iter++;

                /*
                 * So far it has always been the case that failure to locate indicates an
                 * invalid subdivision. So just fail completely. (An alternative would be
                 * to perform an exhaustive search for the containing triangle, but this
                 * would mask errors in the subdivision topology)
                 *
                 * This can also happen if two vertices are located very close together,
                 * since the orientation predicates may experience precision failures.
                 */
                if (iter > maxIter)
                {
                    throw new LocateFailureException(e.ToLineSegment());
                    // String msg = "Locate failed to converge (at edge: " + e + ").
                    // Possible causes include invalid Subdivision topology or very close
                    // sites";
                    // System.err.println(msg);
                    // dumpTriangles();
                }

                if ((v.Equals(e.Orig)) || (v.Equals(e.Dest)))
                {
                    break;
                }
                if (v.RightOf(e))
                {
                    e = e.Sym;
                }
                else if (!v.RightOf(e.ONext))
                {
                    e = e.ONext;
                }
                else if (!v.RightOf(e.DPrev))
                {
                    e = e.DPrev;
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

        /// <summary>
        /// Finds a quadedge of a triangle containing a location
        /// specified by a <see cref="Vertex"/>, if one exists.
        /// </summary>
        /// <param name="v">the vertex to locate</param>
        /// <returns>a quadedge on the edge of a triangle which touches or contains the location<br/>
        /// or null if no such triangle exists
        /// </returns>
        public QuadEdge Locate(Vertex v)
        {
            return _locator.Locate(v);
        }

        /// <summary>
        /// Finds a quadedge of a triangle containing a location
        /// specified by a <see cref="Coordinate"/>, if one exists.
        /// </summary>
        /// <param name="p">the Coordinate to locate</param>
        /// <returns>a quadedge on the edge of a triangle which touches or contains the location,
        /// or null if no such triangle exists
        /// </returns>
        public QuadEdge Locate(Coordinate p)
        {
            return _locator.Locate(new Vertex(p));
        }

        /// <summary>
        /// Locates the edge between the given vertices, if it exists in the
        /// subdivision.
        /// </summary>
        /// <param name="p0">a coordinate</param>
        /// <param name="p1">another coordinate</param>
        /// <returns>the edge joining the coordinates, if present,
        /// or null if no such edge exists
        /// </returns>
        public QuadEdge Locate(Coordinate p0, Coordinate p1)
        {
            // find an edge containing one of the points
            var e = _locator.Locate(new Vertex(p0));
            if (e == null)
                return null;

            // normalize so that p0 is origin of base edge
            var baseQE = e;
            if (e.Dest.Coordinate.Equals2D(p0))
                baseQE = e.Sym;
            // check all edges around origin of base edge
            var locEdge = baseQE;
            do
            {
                if (locEdge.Dest.Coordinate.Equals2D(p1))
                    return locEdge;
                locEdge = locEdge.ONext;
            } while (locEdge != baseQE);
            return null;
        }

        /// <summary>
        /// Inserts a new site into the Subdivision, connecting it to the vertices of
        /// the containing triangle (or quadrilateral, if the split point falls on an
        /// existing edge).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method does NOT maintain the Delaunay condition. If desired, this must
        /// be checked and enforced by the caller.
        /// </para>
        /// <para>
        /// This method does NOT check if the inserted vertex falls on an edge. This
        /// must be checked by the caller, since this situation may cause erroneous
        /// triangulation
        /// </para>
        /// </remarks>
        /// <param name="v">the vertex to insert</param>
        /// <returns>a new quad edge terminating in v</returns>
        public QuadEdge InsertSite(Vertex v)
        {
            var e = Locate(v);

            if ((v.Equals(e.Orig, _tolerance)) || (v.Equals(e.Dest, _tolerance)))
            {
                return e; // point already in subdivision.
            }

            // Connect the new point to the vertices of the containing
            // triangle (or quadrilateral, if the new point fell on an
            // existing edge.)
            var baseQE = MakeEdge(e.Orig, v);
            QuadEdge.Splice(baseQE, e);
            var startEdge = baseQE;
            do
            {
                baseQE = Connect(e, baseQE.Sym);
                e = baseQE.OPrev;
            } while (e.LNext != startEdge);

            return startEdge;
        }

        /// <summary>
        /// Tests whether a QuadEdge is an edge incident on a frame triangle vertex.
        /// </summary>
        /// <param name="e">the edge to test</param>
        /// <returns>true if the edge is connected to the frame triangle</returns>
        public bool IsFrameEdge(QuadEdge e)
        {
            if (IsFrameVertex(e.Orig) || IsFrameVertex(e.Dest))
                return true;
            return false;
        }

        /// <summary>
        /// Tests whether a QuadEdge is an edge on the border of the frame facets and
        /// the internal facets. E.g. an edge which does not itself touch a frame
        /// vertex, but which touches an edge which does.
        /// </summary>
        /// <param name="e">the edge to test</param>
        /// <returns>true if the edge is on the border of the frame</returns>
        public bool IsFrameBorderEdge(QuadEdge e)
        {
            // MD debugging
            var leftTri = new QuadEdge[3];
            GetTriangleEdges(e, leftTri);
            // System.out.println(new QuadEdgeTriangle(leftTri).ToString());
            var rightTri = new QuadEdge[3];
            GetTriangleEdges(e.Sym, rightTri);
            // System.out.println(new QuadEdgeTriangle(rightTri).ToString());

            // check other vertex of triangle to left of edge
            var vLeftTriOther = e.LNext.Dest;
            if (IsFrameVertex(vLeftTriOther))
                return true;
            // check other vertex of triangle to right of edge
            var vRightTriOther = e.Sym.LNext.Dest;
            if (IsFrameVertex(vRightTriOther))
                return true;

            return false;
        }

        /// <summary>
        /// Tests whether a vertex is a vertex of the outer triangle.
        /// </summary>
        /// <param name="v">the vertex to test</param>
        /// <returns>true if the vertex is an outer triangle vertex</returns>
        public bool IsFrameVertex(Vertex v)
        {
            if (v.Equals(_frameVertex[0]))
                return true;
            if (v.Equals(_frameVertex[1]))
                return true;
            if (v.Equals(_frameVertex[2]))
                return true;
            return false;
        }

        private readonly LineSegment seg = new LineSegment();

        /// <summary>
        /// Tests whether a {@link Coordinate} lies on a {@link QuadEdge}, up to a
        /// tolerance determined by the subdivision tolerance.
        /// </summary>
        /// <param name="e">a QuadEdge</param>
        /// <param name="p">a point</param>
        /// <returns>true if the vertex lies on the edge</returns>
        public bool IsOnEdge(QuadEdge e, Coordinate p)
        {
            seg.SetCoordinates(e.Orig.Coordinate, e.Dest.Coordinate);
            double dist = seg.Distance(p);
            // heuristic (hack?)
            return dist < _edgeCoincidenceTolerance;
        }

        /// <summary>
        /// Tests whether a <see cref="Vertex"/> is the start or end vertex of a
        /// <see cref="QuadEdge"/>, up to the subdivision tolerance distance.
        /// </summary>
        /// <param name="e" />
        /// <param name="v" />
        /// <returns>true if the vertex is a endpoint of the edge</returns>
        public bool IsVertexOfEdge(QuadEdge e, Vertex v)
        {
            if ((v.Equals(e.Orig, _tolerance)) || (v.Equals(e.Dest, _tolerance)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the unique <see cref="Vertex"/>es in the subdivision,
        /// including the frame vertices if desired.
        /// </summary>
        /// <param name="includeFrame">true if the frame vertices should be included</param>
        /// <returns>a collection of the subdivision vertices</returns>
        /// <see cref="GetVertexUniqueEdges"/>
        public IEnumerable<Vertex> GetVertices(bool includeFrame)
        {
            var vertices = new HashSet<Vertex>();

            foreach (var qe in _quadEdges)
            {
                var v = qe.Orig;
                //System.out.println(v);
                if (includeFrame || !IsFrameVertex(v))
                    vertices.Add(v);

                /*
                * Inspect the sym edge as well, since it is
                * possible that a vertex is only at the
                * dest of all tracked quadedges.
                */
                var vd = qe.Dest;
                //System.out.println(vd);
                if (includeFrame || !IsFrameVertex(vd))
                    vertices.Add(vd);
            }
            return vertices;
        }

        /// <summary>
        /// Gets a collection of <see cref="QuadEdge"/>s whose origin
        /// vertices are a unique set which includes
        /// all vertices in the subdivision.
        /// The frame vertices can be included if required.
        /// </summary>
        /// <remarks>
        /// This is useful for algorithms which require traversing the
        /// subdivision starting at all vertices.
        /// Returning a quadedge for each vertex
        /// is more efficient than
        /// the alternative of finding the actual vertices
        /// using <see cref="GetVertices"/> and then locating
        /// quadedges attached to them.
        /// </remarks>
        /// <param name="includeFrame">true if the frame vertices should be included</param>
        /// <returns>a collection of QuadEdge with the vertices of the subdivision as their origins</returns>
        public IList<QuadEdge> GetVertexUniqueEdges(bool includeFrame)
        {
            var edges = new List<QuadEdge>();
            var visitedVertices = new HashSet<Vertex>();

            foreach (var qe in _quadEdges)
            {
                var v = qe.Orig;
                //System.out.println(v);
                if (!visitedVertices.Contains(v))
                {
                    visitedVertices.Add(v);
                    if (includeFrame || !IsFrameVertex(v))
                    {
                        edges.Add(qe);
                    }
                }

                /*
                * Inspect the sym edge as well, since it is
                * possible that a vertex is only at the
                * dest of all tracked quadedges.
                */
                var qd = qe.Sym;
                var vd = qd.Orig;
                //System.out.println(vd);
                if (!visitedVertices.Contains(vd))
                {
                    visitedVertices.Add(vd);
                    if (includeFrame || !IsFrameVertex(vd))
                    {
                        edges.Add(qd);
                    }
                }
            }
            return edges;
        }

        /// <summary>
        /// Gets all primary quadedges in the subdivision.
        /// A primary edge is a <see cref="QuadEdge"/>
        /// which occupies the 0'th position in its array of associated quadedges.
        /// These provide the unique geometric edges of the triangulation.
        /// </summary>
        /// <param name="includeFrame">true if the frame edges are to be included</param>
        /// <returns>a List of QuadEdges</returns>
        public IList<QuadEdge> GetPrimaryEdges(bool includeFrame)
        {
            _visitedKey++;

            var edges = new List<QuadEdge>();
            var edgeStack = new Stack<QuadEdge>();
            edgeStack.Push(_startingEdge);

            var visitedEdges = new HashSet<QuadEdge>();

            while (edgeStack.Count > 0)
            {
                var edge = edgeStack.Pop();
                if (!visitedEdges.Contains(edge))
                {
                    var priQE = edge.GetPrimary();

                    if (includeFrame || !IsFrameEdge(priQE))
                        edges.Add(priQE);

                    edgeStack.Push(edge.ONext);
                    edgeStack.Push(edge.Sym.ONext);

                    visitedEdges.Add(edge);
                    visitedEdges.Add(edge.Sym);
                }
            }
            return edges;
        }

        /// <summary>
        /// A TriangleVisitor which computes and sets the
        /// circumcentre as the origin of the dual
        /// edges originating in each triangle.
        /// </summary>
        /// <author>mbdavis</author>
        private class TriangleCircumcentreVisitor : ITriangleVisitor
        {
            public void Visit(QuadEdge[] triEdges)
            {
                var a = triEdges[0].Orig.Coordinate;
                var b = triEdges[1].Orig.Coordinate;
                var c = triEdges[2].Orig.Coordinate;

                // TODO: choose the most accurate circumcentre based on the edges
                var cc = Triangle.CircumcentreDD(a, b, c);
                var ccVertex = new Vertex(cc);
                // save the circumcentre as the origin for the dual edges originating in this triangle
                for (int i = 0; i < 3; i++)
                {
                    triEdges[i].Rot.Orig = ccVertex;
                }
            }
        }

        /*****************************************************************************
         * Visitors
         ****************************************************************************/

        public void VisitTriangles(ITriangleVisitor triVisitor,
                                    bool includeFrame)
        {
            _visitedKey++;

            // visited flag is used to record visited edges of triangles
            // setVisitedAll(false);
            var edgeStack = new Stack<QuadEdge>();
            edgeStack.Push(_startingEdge);

            var visitedEdges = new HashSet<QuadEdge>();

            while (edgeStack.Count > 0)
            {
                var edge = edgeStack.Pop();
                if (!visitedEdges.Contains(edge))
                {
                    var triEdges =
                        FetchTriangleToVisit(edge, edgeStack, includeFrame, visitedEdges);

                    if (triEdges != null)
                        triVisitor.Visit(triEdges);
                }
            }
        }

        /// <summary>
        /// The quadedges forming a single triangle.
        /// Only one visitor is allowed to be active at a
        /// time, so this is safe.
        /// </summary>
        private readonly QuadEdge[] _triEdges = new QuadEdge[3];

        /// <summary>
        /// Stores the edges for a visited triangle. Also pushes sym (neighbour) edges
        /// on stack to visit later.
        /// </summary>
        /// <param name="edge" />
        /// <param name="edgeStack" />
        /// <param name="includeFrame" />
        /// <param name="visitedEdges"></param>
        /// <returns>the visited triangle edges,<br/>
        /// or <c>null</c> if the triangle should not be visited (for instance, if it is outer)
        /// </returns>
        private QuadEdge[] FetchTriangleToVisit(QuadEdge edge, Stack<QuadEdge> edgeStack, bool includeFrame,
            HashSet<QuadEdge> visitedEdges)
        {
            var curr = edge;
            int edgeCount = 0;
            bool isFrame = false;
            do
            {
                _triEdges[edgeCount] = curr;

                if (IsFrameEdge(curr))
                    isFrame = true;

                // push sym edges to visit next
                var sym = curr.Sym;
                if (!visitedEdges.Contains(sym))
                    edgeStack.Push(sym);

                // mark this edge as visited
                visitedEdges.Add(curr);

                edgeCount++;
                curr = curr.LNext;
            } while (curr != edge);

            if (isFrame && !includeFrame)
                return null;
            return _triEdges;
        }

        /// <summary>
        /// Gets a list of the triangles
        /// in the subdivision, specified as
        /// an array of the primary quadedges around the triangle.
        /// </summary>
        /// <param name="includeFrame">true if the frame triangles should be included</param>
        /// <returns>a List of QuadEdge[3] arrays</returns>
        public IList<QuadEdge[]> GetTriangleEdges(bool includeFrame)
        {
            var visitor = new TriangleEdgesListVisitor();
            VisitTriangles(visitor, includeFrame);
            return visitor.GetTriangleEdges();
        }

        private class TriangleEdgesListVisitor : ITriangleVisitor
        {
            private readonly IList<QuadEdge[]> _triList = new List<QuadEdge[]>();

            public void Visit(QuadEdge[] triEdges)
            {
                _triList.Add(triEdges);
            }

            public IList<QuadEdge[]> GetTriangleEdges()
            {
                return _triList;
            }
        }

        /// <summary>
        /// Gets a list of the triangles in the subdivision,
        /// specified as an array of the triangle <see cref="Vertex"/>es.
        /// </summary>
        /// <param name="includeFrame">true if the frame triangles should be included</param>
        /// <returns>a List of Vertex[3] arrays</returns>
        public IList<Vertex[]> GetTriangleVertices(bool includeFrame)
        {
            var visitor = new TriangleVertexListVisitor();
            VisitTriangles(visitor, includeFrame);
            return visitor.GetTriangleVertices();
        }

        private class TriangleVertexListVisitor : ITriangleVisitor
        {
            private readonly IList<Vertex[]> _triList = new List<Vertex[]>();

            public void Visit(QuadEdge[] triEdges)
            {
                _triList.Add(new[] { triEdges[0].Orig, triEdges[1].Orig,
                            triEdges[2].Orig });
            }

            public IList<Vertex[]> GetTriangleVertices()
            {
                return _triList;
            }
        }

        /// <summary>
        /// Gets the coordinates for each triangle in the subdivision as an array.
        /// </summary>
        /// <param name="includeFrame">true if the frame triangles should be included</param>
        /// <returns>a list of Coordinate[4] representing each triangle</returns>
        public IList<Coordinate[]> GetTriangleCoordinates(bool includeFrame)
        {
            var visitor = new TriangleCoordinatesVisitor();
            VisitTriangles(visitor, includeFrame);
            return visitor.GetTriangles();
        }

        private class TriangleCoordinatesVisitor : ITriangleVisitor
        {
            private readonly CoordinateList _coordList = new CoordinateList();

            private readonly List<Coordinate[]> _triCoords = new List<Coordinate[]>();

            public void Visit(QuadEdge[] triEdges)
            {
                _coordList.Clear();
                for (int i = 0; i < 3; i++)
                {
                    var v = triEdges[i].Orig;
                    _coordList.Add(v.Coordinate);
                }
                if (_coordList.Count > 0)
                {
                    _coordList.CloseRing();
                    var pts = _coordList.ToCoordinateArray();
                    if (pts.Length != 4)
                    {
                        //CheckTriangleSize(pts);
                        return;
                    }

                    _triCoords.Add(pts);
                }
            }

            private static void CheckTriangleSize(Coordinate[] pts)
            {
                string loc = "";
                if (pts.Length >= 2)
                    loc = WKTWriter.ToLineString(pts[0], pts[1]);
                else
                {
                    if (pts.Length >= 1)
                        loc = WKTWriter.ToPoint(pts[0]);
                }

                Assert.IsTrue(pts.Length == 4, "Too few points for visited triangle at " + loc);
            }

            public IList<Coordinate[]> GetTriangles()
            {
                return _triCoords;
            }
        }

        /// <summary>
        /// Gets the geometry for the edges in the subdivision as a <see cref="MultiLineString"/>
        /// containing 2-point lines.
        /// </summary>
        /// <param name="geomFact">the GeometryFactory to use</param>
        /// <returns>a MultiLineString</returns>
        public MultiLineString GetEdges(GeometryFactory geomFact)
        {
            var quadEdges = GetPrimaryEdges(false);
            var edges = new LineString[quadEdges.Count];
            int i = 0;
            foreach (var qe in quadEdges)
            {
                edges[i++] = geomFact.CreateLineString(new[] {
                                                        qe.Orig.Coordinate, qe.Dest.Coordinate });
            }
            return geomFact.CreateMultiLineString(edges);
        }

        /// <summary>
        /// Gets the geometry for the triangles in a triangulated subdivision as a <see cref="GeometryCollection"/>
        /// of triangular <see cref="Polygon"/>s.
        /// </summary>
        /// <param name="geomFact">the GeometryFactory to use</param>
        /// <returns>a GeometryCollection of triangular Polygons</returns>
        public GeometryCollection GetTriangles(GeometryFactory geomFact)
        {
            var triPtsList = GetTriangleCoordinates(false);
            var tris = new Polygon[triPtsList.Count];
            int i = 0;
            foreach (var triPt in triPtsList)
            {
                tris[i++] = geomFact
                            .CreatePolygon(geomFact.CreateLinearRing(triPt));
            }
            return geomFact.CreateGeometryCollection(tris);
        }

        /// <summary>
        /// Gets the cells in the Voronoi diagram for this triangulation.
        /// The cells are returned as a <see cref="GeometryCollection" /> of <see cref="Polygon"/>s
        /// </summary>
        /// <remarks>
        /// The userData of each polygon is set to be the <see cref="Coordinate" />
        /// of the cell site.  This allows easily associating external
        /// data associated with the sites to the cells.
        /// </remarks>
        /// <param name="geomFact">a geometry factory</param>
        /// <returns>a GeometryCollection of Polygons</returns>
        public GeometryCollection GetVoronoiDiagram(GeometryFactory geomFact)
        {
            var vorCells = GetVoronoiCellPolygons(geomFact);
            return geomFact.CreateGeometryCollection(GeometryFactory.ToGeometryArray(vorCells));
        }

        /// <summary>
        /// Gets a List of <see cref="Polygon"/>s for the Voronoi cells
        /// of this triangulation.
        /// </summary>
        /// <remarks>
        /// The UserData of each polygon is set to be the <see cref="Coordinate"/>
        /// of the cell site.  This allows easily associating external
        /// data associated with the sites to the cells.
        /// </remarks>
        /// <param name="geomFact">a geometry factory</param>
        /// <returns>a List of Polygons</returns>
        public IList<Geometry> GetVoronoiCellPolygons(GeometryFactory geomFact)
        {
            /*
             * Compute circumcentres of triangles as vertices for dual edges.
             * Precomputing the circumcentres is more efficient,
             * and more importantly ensures that the computed centres
             * are consistent across the Voronoi cells.
             */
            VisitTriangles(new TriangleCircumcentreVisitor(), true);

            var cells = new List<Geometry>();
            var edges = GetVertexUniqueEdges(false);
            foreach (var qe in edges)
            {
                cells.Add(GetVoronoiCellPolygon(qe, geomFact));
            }
            return cells;
        }

        /// <summary>
        /// Gets the Voronoi cell around a site specified
        /// by the origin of a QuadEdge.
        /// </summary>
        /// <remarks>
        /// The userData of the polygon is set to be the <see cref="Coordinate" />
        /// of the site.  This allows attaching external
        /// data associated with the site to this cell polygon.
        /// </remarks>
        /// <param name="qe">a quadedge originating at the cell site</param>
        /// <param name="geomFact">a factory for building the polygon</param>
        /// <returns>a polygon indicating the cell extent</returns>
        public Polygon GetVoronoiCellPolygon(QuadEdge qe, GeometryFactory geomFact)
        {
            var cellPts = new List<Coordinate>();
            var startQE = qe;

            do
            {
                // Coordinate cc = circumcentre(qe);
                // use previously computed circumcentre
                var cc = qe.Rot.Orig.Coordinate;
                cellPts.Add(cc);

                // move to next triangle CW around vertex
                qe = qe.OPrev;
            } while (qe != startQE);

            var coordList = new CoordinateList(cellPts, false);
            //coordList.AddAll(cellPts, false);
            coordList.CloseRing();

            if (coordList.Count < 4)
            {
                Debug.WriteLine(coordList);
                coordList.Add(coordList[coordList.Count - 1], true);
            }

            var pts = coordList.ToCoordinateArray();
            var cellPoly = geomFact.CreatePolygon(geomFact.CreateLinearRing(pts));

            var v = startQE.Orig;
            cellPoly.UserData = v.Coordinate;
            return cellPoly;
        }
    }
}
