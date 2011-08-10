using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Triangulate.QuadEdge
{
/**
 * A class that represents the edge data structure which implements the quadedge algebra. 
 * The quadedge algebra was described in a well-known paper by Guibas and Stolfi,
 * "Primitives for the manipulation of general subdivisions and the computation of Voronoi diagrams", 
 * <i>ACM Transactions on Graphics</i>, 4(2), 1985, 75-123.
 * <p>
 * Each edge object is part of a quartet of 4 edges,
 * linked via their <tt>rot</tt> references.
 * Any edge in the group may be accessed using a series of {@link #rot()} operations.
 * Quadedges in a subdivision are linked together via their <tt>next</tt> references.
 * The linkage between the quadedge quartets determines the topology
 * of the subdivision. 
 * <p>
 * The edge class does not contain separate information for vertice or faces; a vertex is implicitly
 * defined as a ring of edges (created using the <tt>next</tt> field).
 * 
 * @author David Skea
 * @author Martin Davis
 */

    public class QuadEdge
    {
        /**
     * Creates a new QuadEdge quartet from {@link Vertex} o to {@link Vertex} d.
     * 
     * @param o
     *          the origin Vertex
     * @param d
     *          the destination Vertex
     * @return the new QuadEdge quartet
     */

        public static QuadEdge MakeEdge(Vertex o, Vertex d)
        {
            QuadEdge q0 = new QuadEdge();
            QuadEdge q1 = new QuadEdge();
            QuadEdge q2 = new QuadEdge();
            QuadEdge q3 = new QuadEdge();

            q0.Rot = q1;
            q1.Rot = q2;
            q2.Rot = q3;
            q3.Rot = q0;

            q0.setNext(q0);
            q1.setNext(q3);
            q2.setNext(q2);
            q3.setNext(q1);

            QuadEdge
            baseQE = q0;
            baseQE.setOrig(o);
            baseQE.setDest(d);
            return baseQE;
        }

        /**
     * Creates a new QuadEdge connecting the destination of a to the origin of
     * b, in such a way that all three have the same left face after the
     * connection is complete. Additionally, the data pointers of the new edge
     * are set.
     * 
     * @return the connected edge.
     */

        public static QuadEdge Connect(QuadEdge a, QuadEdge b)
        {
            QuadEdge e = MakeEdge(a.Dest, b.Orig);
            Splice(e, a.lNext());
            Splice(e.Sym(), b);
            return e;
        }

        /**
     * Splices two edges together or apart.
     * Splice affects the two edge rings around the origins of a and b, and, independently, the two
     * edge rings around the left faces of <tt>a</tt> and <tt>b</tt>. 
     * In each case, (i) if the two rings are distinct,
     * Splice will combine them into one, or (ii) if the two are the same ring, Splice will break it
     * into two separate pieces. Thus, Splice can be used both to attach the two edges together, and
     * to break them apart.
     * 
     * @param a an edge to splice
     * @param b an edge to splice
     * 
     */

        public static void Splice(QuadEdge a, QuadEdge b)
        {
            QuadEdge alpha = a.oNext().Rot;
            QuadEdge beta = b.oNext().Rot;

            QuadEdge t1 = b.oNext();
            QuadEdge t2 = a.oNext();
            QuadEdge t3 = beta.oNext();
            QuadEdge t4 = alpha.oNext();

            a.setNext(t1);
            b.setNext(t2);
            alpha.setNext(t3);
            beta.setNext(t4);
        }

        /**
     * Turns an edge counterclockwise inside its enclosing quadrilateral.
     * 
     * @param e the quadedge to turn
     */

        public static void Swap(QuadEdge e)
        {
            QuadEdge a = e.oPrev();
            QuadEdge b = e.Sym().oPrev();
            Splice(e, a);
            Splice(e.Sym(), b);
            Splice(e, a.lNext());
            Splice(e.Sym(), b.lNext());
            e.setOrig(a.Dest);
            e.setDest(b.Dest);
        }

        // the dual of this edge, directed from right to left
        private Vertex _vertex; // The vertex that this edge represents
        private QuadEdge _next; // A reference to a connected edge
        private Object _data;
//    private int      visitedKey = 0;

        /**
     * Quadedges must be made using {@link makeEdge}, 
     * to ensure proper construction.
     */

        private QuadEdge()
        {

        }

        /**
     * Gets the primary edge of this quadedge and its <tt>sym</tt>.
     * The primary edge is the one for which the origin
     * and destination coordinates are ordered
     * according to the standard {@link Coordinate} ordering
     * 
     * @return the primary quadedge
     */

        public QuadEdge GetPrimary()
        {
            if (Orig.Coordinate.CompareTo(Dest.Coordinate) <= 0)
                return this;
            return Sym();
        }

        /**
     * Gets or sets the external data value for this edge.
     * 
     * @param data an object containing external data
     */

        public object Data
        {
            set { _data = value; }
            get { return _data; }
        }

        /**
     * Marks this quadedge as being deleted.
     * This does not free the memory used by
     * this quadedge quartet, but indicates
     * that this edge no longer participates
     * in a subdivision.
     *
     */

        public void delete()
        {
            Rot = null;
        }

        /**
     * Tests whether this edge has been deleted.
     * 
     * @return true if this edge has not been deleted.
     */

        public bool IsLive
        {
            get { return Rot != null; }
        }


        /**
     * Sets the connected edge
     * 
     * @param nextEdge edge
     */

        public void setNext(QuadEdge next)
        {
            this._next = next;
        }

        /***************************************************************************
     * QuadEdge Algebra 
     ***************************************************************************
     */

        /**
     * Gets the dual of this edge, directed from its right to its left.
     * 
     * @return the rotated edge
     */
        internal QuadEdge Rot { get; set; }

        /**
     * Gets the dual of this edge, directed from its left to its right.
     * 
     * @return the inverse rotated edge.
     */
        private QuadEdge InvRot()
        {
            return Rot.Sym();
        }

        /**
     * Gets the edge from the destination to the origin of this edge.
     * 
     * @return the sym of the edge
     */

        internal QuadEdge Sym()
        {
            return Rot.Rot;
        }
        
        /**
     * Gets the next CCW edge around the origin of this edge.
     * 
     * @return the next linked edge.
     */

        internal QuadEdge oNext()
        {
            return _next;
        }

        /**
     * Gets the next CW edge around (from) the origin of this edge.
     * 
     * @return the previous edge.
     */

        internal QuadEdge oPrev()
        {
            return Rot._next.Rot;
        }

        /**
     * Gets the next CCW edge around (into) the destination of this edge.
     * 
     * @return the next destination edge.
     */
        private QuadEdge dNext()
        {
            return Sym().oNext().Sym();
        }

        /**
     * Gets the next CW edge around (into) the destination of this edge.
     * 
     * @return the previous destination edge.
     */

        internal QuadEdge dPrev()
        {
            return InvRot().oNext().InvRot();
        }

        /**
     * Gets the CCW edge around the left face following this edge.
     * 
     * @return the next left face edge.
     */

        internal QuadEdge lNext()
        {
            return this.InvRot().oNext().Rot;
        }

        /**
     * Gets the CCW edge around the left face before this edge.
     * 
     * @return the previous left face edge.
     */
        private  QuadEdge lPrev()
        {
            return _next.Sym();
        }

        /**
     * Gets the edge around the right face ccw following this edge.
     * 
     * @return the next right face edge.
     */
        private QuadEdge rNext()
        {
            return Rot._next.InvRot();
        }

        /**
     * Gets the edge around the right face ccw before this edge.
     * 
     * @return the previous right face edge.
     */
        private QuadEdge rPrev()
        {
            return this.Sym().oNext();
        }

        /***********************************************************************************************
     * Data Access
     **********************************************************************************************/
        /**
     * Sets the vertex for this edge's origin
     * 
     * @param o the origin vertex
     */

        private void setOrig(Vertex o)
        {
            _vertex = o;
        }

        /**
     * Sets the vertex for this edge's destination
     * 
     * @param d the destination vertex
     */

        private void setDest(Vertex d)
        {
            Sym().setOrig(d);
        }

        /**
     * Gets the vertex for the edge's origin
     * 
     * @return the origin vertex
     */

        public Vertex Orig
        {
            get { return _vertex; }
        }

        /**
     * Gets the vertex for the edge's destination
     * 
     * @return the destination vertex
     */

        public Vertex Dest
        {
            get { return Sym().Orig; }
        }

        private 

        /**
     * Gets the length of the geometry of this quadedge.
     * 
     * @return the length of the quadedge
     */

        public double GetLength()
        {
            return Orig.Coordinate.Distance(Dest.Coordinate);
        }

        /**
     * Tests if this quadedge and another have the same line segment geometry, 
     * regardless of orientation.
     * 
     * @param qe a quadege
     * @return true if the quadedges are based on the same line segment regardless of orientation
     */

        public bool EqualsNonOriented(QuadEdge qe)
        {
            if (EqualsOriented(qe))
                return true;
            if (EqualsOriented(qe.Sym()))
                return true;
            return false;
        }

        /**
     * Tests if this quadedge and another have the same line segment geometry
     * with the same orientation.
     * 
     * @param qe a quadege
     * @return true if the quadedges are based on the same line segment
     */

        public bool EqualsOriented(QuadEdge qe)
        {
            if (Orig.Coordinate.Equals2D(qe.Orig.Coordinate)
                && Dest.Coordinate.Equals2D(qe.Dest.Coordinate))
                return true;
            return false;
        }

        /**
     * Creates a {@link LineSegment} representing the
     * geometry of this edge.
     * 
     * @return a LineSegment
     */

        public LineSegment ToLineSegment()
        {
            return new LineSegment(_vertex.Coordinate, Dest.Coordinate);
        }

        /**
     * Converts this edge to a WKT two-point <tt>LINESTRING</tt> indicating 
     * the geometry of this edge.
     * 
     * @return a String representing this edge's geometry
     */

        public override String ToString()
        {
            var p0 = _vertex.Coordinate;
            var p1 = Dest.Coordinate;
            return WKTWriter.ToLineString(p0, p1);
        }
    }
}