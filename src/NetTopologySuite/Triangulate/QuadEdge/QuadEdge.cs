using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// A class that represents the edge data structure which implements the quadedge algebra.
    /// The quadedge algebra was described in a well-known paper by Guibas and Stolfi,
    /// "Primitives for the manipulation of general subdivisions and the computation of Voronoi diagrams",
    /// <i>ACM Transactions on Graphics</i>, 4(2), 1985, 75-123.
    /// <para>
    /// Each edge object is part of a quartet of 4 edges,
    /// linked via their <tt>Rot</tt> references.
    /// Any edge in the group may be accessed using a series of <see cref="Rot"/> operations.
    /// Quadedges in a subdivision are linked together via their <tt>Next</tt> references.
    /// The linkage between the quadedge quartets determines the topology
    /// of the subdivision.
    /// </para>
    /// <para>
    /// The edge class does not contain separate information for vertices or faces; a vertex is implicitly
    /// defined as a ring of edges (created using the <tt>Next</tt> field).
    /// </para>
    /// </summary>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class QuadEdge
    {
        /// <summary>
        /// Creates a new QuadEdge quartet from <see cref="Vertex"/>o to <see cref="Vertex"/> d.
        /// </summary>
        /// <param name="o">the origin Vertex</param>
        /// <param name="d">the destination Vertex</param>
        /// <returns>the new QuadEdge quartet</returns>
        public static QuadEdge MakeEdge(Vertex o, Vertex d)
        {
            var q0 = new QuadEdge();
            var q1 = new QuadEdge();
            var q2 = new QuadEdge();
            var q3 = new QuadEdge();

            q0.Rot = q1;
            q1.Rot = q2;
            q2.Rot = q3;
            q3.Rot = q0;

            q0.SetNext(q0);
            q1.SetNext(q3);
            q2.SetNext(q2);
            q3.SetNext(q1);

            var baseQE = q0;
            baseQE.Orig = o;
            baseQE.Dest = d;
            return baseQE;
        }

        /// <summary>
        /// Creates a new QuadEdge connecting the destination of a to the origin of
        /// b, in such a way that all three have the same left face after the
        /// connection is complete. Additionally, the data pointers of the new edge
        /// are set.
        /// </summary>
        /// <returns>the connected edge</returns>
        public static QuadEdge Connect(QuadEdge a, QuadEdge b)
        {
            var e = MakeEdge(a.Dest, b.Orig);
            Splice(e, a.LNext);
            Splice(e.Sym, b);
            return e;
        }

        /// <summary>
        /// Splices two edges together or apart.
        /// Splice affects the two edge rings around the origins of a and b, and, independently, the two
        /// edge rings around the left faces of <tt>a</tt> and <tt>b</tt>.
        /// In each case, (i) if the two rings are distinct,
        /// Splice will combine them into one, or (ii) if the two are the same ring, Splice will break it
        /// into two separate pieces. Thus, Splice can be used both to attach the two edges together, and
        /// to break them apart.
        /// </summary>
        /// <param name="a">an edge to splice</param>
        /// <param name="b">an edge to splice</param>
        public static void Splice(QuadEdge a, QuadEdge b)
        {
            var alpha = a.ONext.Rot;
            var beta = b.ONext.Rot;

            var t1 = b.ONext;
            var t2 = a.ONext;
            var t3 = beta.ONext;
            var t4 = alpha.ONext;

            a.SetNext(t1);
            b.SetNext(t2);
            alpha.SetNext(t3);
            beta.SetNext(t4);
        }

        /// <summary>
        /// Turns an edge counterclockwise inside its enclosing quadrilateral.
        /// </summary>
        /// <param name="e">the quadedge to turn</param>
        public static void Swap(QuadEdge e)
        {
            var a = e.OPrev;
            var b = e.Sym.OPrev;
            Splice(e, a);
            Splice(e.Sym, b);
            Splice(e, a.LNext);
            Splice(e.Sym, b.LNext);
            e.Orig = a.Dest;
            e.Dest = b.Dest;
        }

        // the dual of this edge, directed from right to left
        private Vertex _vertex; // The vertex that this edge represents
        private QuadEdge _next; // A reference to a connected edge
//    private int      visitedKey = 0;

        /// <summary>
        /// Quadedges must be made using {@link makeEdge},
        /// to ensure proper construction.
        /// </summary>
        private QuadEdge()
        {

        }

        /// <summary>
        /// Gets the primary edge of this quadedge and its <tt>sym</tt>.
        /// The primary edge is the one for which the origin
        /// and destination coordinates are ordered
        /// according to the standard <see cref="Coordinate"/> ordering
        /// </summary>
        /// <returns>the primary quadedge</returns>
        public QuadEdge GetPrimary()
        {
            if (Orig.Coordinate.CompareTo(Dest.Coordinate) <= 0)
                return this;
            return Sym;
        }

        /// <summary>
        /// Gets or sets the external data value for this edge.
        /// </summary>
        /// <remarks>
        /// an object containing external data
        /// </remarks>
        public object Data { set; get; }

        /// <summary>
        /// Marks this quadedge as being deleted.
        /// This does not free the memory used by
        /// this quadedge quartet, but indicates
        /// that this edge no longer participates
        /// in a subdivision.
        /// </summary>
        public void Delete()
        {
            Rot = null;
        }

        /// <summary>
        /// Tests whether this edge has been deleted.
        /// </summary>
        /// <returns>true if this edge has not been deleted.</returns>
        public bool IsLive => Rot != null;

        /// <summary>
        /// Sets the connected edge
        /// </summary>
        /// <param name="next">edge</param>
        public void SetNext(QuadEdge next)
        {
            _next = next;
        }

        /***************************************************************************
         * QuadEdge Algebra
         ***************************************************************************
         */

        /// <summary>
        /// Gets the dual of this edge, directed from its right to its left.
        /// </summary>
        /// <remarks>Gets or Sets the rotated edge</remarks>
        public QuadEdge Rot { get; private set; }

        /// <summary>
        /// Gets the dual of this edge, directed from its left to its right.
        /// </summary>
        /// <remarks>Gets the inverse rotated edge.</remarks>
        public QuadEdge InvRot => Rot.Sym;

        /// <summary>
        /// Gets the edge from the destination to the origin of this edge.
        /// </summary>
        /// <remarks>Gets the sym of the edge.</remarks>
        public QuadEdge Sym => Rot.Rot;

        /// <summary>
        /// Gets the next CCW edge around the origin of this edge.
        /// </summary>
        /// <remarks>Gets the next linked edge.</remarks>
        public QuadEdge ONext => _next;

        /// <summary>
        /// Gets the next CW edge around (from) the origin of this edge.
        /// </summary>
        /// <remarks>Gets the previous edge.</remarks>
        public QuadEdge OPrev => Rot._next.Rot;

        /// <summary>
        /// Gets the next CCW edge around (into) the destination of this edge.
        /// </summary>
        /// <remarks>Get the next destination edge.</remarks>
        public QuadEdge DNext => Sym.ONext.Sym;

        /// <summary>
        /// Gets the next CW edge around (into) the destination of this edge.
        /// </summary>
        /// <remarks>Get the previous destination edge.</remarks>
        public QuadEdge DPrev => InvRot.ONext.InvRot;

        /// <summary>
        /// Gets the CCW edge around the left face following this edge.
        /// </summary>
        /// <remarks>Gets the next left face edge.</remarks>
        public QuadEdge LNext => InvRot.ONext.Rot;

        /// <summary>
        /// Gets the CCW edge around the left face before this edge.
        /// </summary>
        /// <remarks>Get the previous left face edge.</remarks>
        public QuadEdge LPrev => _next.Sym;

        /// <summary>
        /// Gets the edge around the right face ccw following this edge.
        /// </summary>
        /// <remarks>Gets the next right face edge.</remarks>
        public QuadEdge RNext => Rot._next.InvRot;

        /// <summary>
        /// Gets the edge around the right face ccw before this edge.
        /// </summary>
        /// <remarks>Gets the previous right face edge.</remarks>
        public QuadEdge RPrev => Sym.ONext;

        /***********************************************************************************************
         * Data Access
         **********************************************************************************************/
        /*
        /// <summary>
        /// Sets the vertex for this edge's origin
        /// </summary>
        /// <param name="o">the origin vertex</param>
        internal void SetOrig(Vertex o)
        {
            _vertex = o;
        }
         */

        /*
        /// <summary>
        /// Sets the vertex for this edge's destination
        /// </summary>
        /// <param name="d">the destination vertex</param>
        internal void SetDest(Vertex d)
        {
            Sym.Orig = d;
        }
         */

        /// <summary>
        /// Gets or sets the vertex for the edge's origin
        /// </summary>
        /// <remarks>Gets the origin vertex</remarks>
        public Vertex Orig
        {
            get => _vertex;
            internal set => _vertex = value;
        }

        /// <summary>
        /// Gets or sets the vertex for the edge's destination
        /// </summary>
        /// <remarks>Gets the destination vertex</remarks>
        public Vertex Dest
        {
            get => Sym.Orig;
            internal set => Sym.Orig = value;
        }

        /// <summary>
        /// Gets the length of the geometry of this quadedge.
        /// </summary>
        /// <remarks>Gets the length of the quadedge</remarks>
        public double Length => Orig.Coordinate.Distance(Dest.Coordinate);

        /// <summary>
        /// Tests if this quadedge and another have the same line segment geometry,
        /// regardless of orientation.
        /// </summary>
        /// <param name="qe">a quadedge</param>
        /// <returns>true if the quadedges are based on the same line segment regardless of orientation</returns>
        public bool EqualsNonOriented(QuadEdge qe)
        {
            if (EqualsOriented(qe))
                return true;
            if (EqualsOriented(qe.Sym))
                return true;
            return false;
        }

        /// <summary>
        /// Tests if this quadedge and another have the same line segment geometry
        /// with the same orientation.
        /// </summary>
        /// <param name="qe">a quadedge</param>
        /// <returns>true if the quadedges are based on the same line segment</returns>
        public bool EqualsOriented(QuadEdge qe)
        {
            if (Orig.Coordinate.Equals2D(qe.Orig.Coordinate)
                && Dest.Coordinate.Equals2D(qe.Dest.Coordinate))
                return true;
            return false;
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the
        /// geometry of this edge.
        /// </summary>
        /// <returns>a LineSegment</returns>
        public LineSegment ToLineSegment()
        {
            return new LineSegment(_vertex.Coordinate, Dest.Coordinate);
        }

        /// <summary>
        /// Converts this edge to a WKT two-point <tt>LINESTRING</tt> indicating
        /// the geometry of this edge.
        /// </summary>
        /// <returns>a String representing this edge's geometry</returns>
        public override string ToString()
        {
            var p0 = _vertex.Coordinate;
            var p1 = Dest.Coordinate;
            return WKTWriter.ToLineString(p0, p1);
        }
    }
}
