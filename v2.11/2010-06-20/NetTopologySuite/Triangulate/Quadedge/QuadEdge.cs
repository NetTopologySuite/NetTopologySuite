using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// A class that represents the edge data structure which implements the quadedge algebra.
    /// The quadedge algebra was described in a well-known paper by Guibas and Stolfi,
    /// "Primitives for the manipulation of general subdivisions and the computation of Voronoi diagrams",
    /// <i>ACM Transactions on Graphics</i>, 4(2), 1985, 75-123.
    /// <para>
    /// Each edge object is part of a quartet of 4 edges, linked via their <tt>rot</tt> references.
    /// Any edge in the group may be accessed using a series of {@link #rot()} operations.
    /// Quadedges in a subdivision are linked together via their <tt>next</tt> references.
    /// The linkage between the quadedge quartets determines the topology
    /// of the subdivision. 
    /// </para>
    /// <para>
    /// The edge class does not contain separate information for vertice or faces; a vertex is implicitly
    /// defined as a ring of edges (created using the <tt>next</tt> field).
    /// </para>
    /// 
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class QuadEdge<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
    {
        ///<summary>
        /// Creates a new QuadEdge quartet from {@link Vertex} o to {@link Vertex} d.
        ///</summary>
        ///<param name="o">the origin Vertex</param>
        ///<param name="d">the destination Vertex</param>
        ///<returns>the new QuadEdge quartet</returns>
        public static QuadEdge<TCoordinate> MakeEdge(Vertex<TCoordinate> o, Vertex<TCoordinate> d)
        {
            QuadEdge<TCoordinate> q0 = new QuadEdge<TCoordinate>();
            QuadEdge<TCoordinate> q1 = new QuadEdge<TCoordinate>();
            QuadEdge<TCoordinate> q2 = new QuadEdge<TCoordinate>();
            QuadEdge<TCoordinate> q3 = new QuadEdge<TCoordinate>();

            q0.Rot = q1;
            q1.Rot = q2;
            q2.Rot = q3;
            q3.Rot = q0;

            q0.Next = q0;
            q1.Next = q3;
            q2.Next = q2;
            q3.Next = q1;

            QuadEdge<TCoordinate> baseQE = q0;
            baseQE.Origin = o;
            baseQE.Destination=d;
            return baseQE;
        }

        ///<summary>
        /// Creates a new QuadEdge connecting the destination of a to the origin of
        /// b, in such a way that all three have the same left face after the
        /// connection is complete. Additionally, the data pointers of the new edge
        /// are set.
        ///</summary>
        ///<param name="a"></param>
        ///<param name="b"></param>
        ///<returns>the connected edge</returns>
        public static QuadEdge<TCoordinate> Connect(QuadEdge<TCoordinate> a, QuadEdge<TCoordinate> b)
        {
            QuadEdge<TCoordinate> e = MakeEdge(a.Destination, b.Origin);
            Splice(e, a.LeftNext);
            Splice(e.Sym(), b);
            return e;
        }

        ///<summary>
        /// Splices two edges together or apart.
        /// Splice affects the two edge rings around the origins of a and b, and, independently, the two
        /// edge rings around the left faces of <tt>a</tt> and <tt>b</tt>. 
        /// In each case, (i) if the two rings are distinct,
        /// Splice will combine them into one, or (ii) if the two are the same ring, Splice will break it
        /// into two separate pieces. Thus, Splice can be used both to attach the two edges together, and
        /// to break them apart.
        ///</summary>
        ///<param name="a"></param>
        ///<param name="b"></param>
        public static void Splice(QuadEdge<TCoordinate> a, QuadEdge<TCoordinate> b)
        {
            QuadEdge<TCoordinate> alpha = a.OriginNext.Rot;
            QuadEdge<TCoordinate> beta = b.OriginNext.Rot;

            QuadEdge<TCoordinate> t1 = b.OriginNext;
            QuadEdge<TCoordinate> t2 = a.OriginNext;
            QuadEdge<TCoordinate> t3 = beta.OriginNext;
            QuadEdge<TCoordinate> t4 = alpha.OriginNext;

            a.Next = t1;
            b.Next = t2;
            alpha.Next = t3;
            beta.Next = t4;
        }

        ///<summary>
        /// Turns an edge counterclockwise inside its enclosing quadrilateral.
        ///</summary>
        ///<param name="e">the quadedge to turn</param>
        public static void Swap(QuadEdge<TCoordinate> e)
        {
            QuadEdge<TCoordinate> a = e.OriginPrev;
            QuadEdge<TCoordinate> b = e.Sym().OriginPrev;
            Splice(e, a);
            Splice(e.Sym(), b);
            Splice(e, a.LeftNext);
            Splice(e.Sym(), b.LeftNext);
            e.Origin=a.Destination;
            e.Destination = b.Destination;
        }

        // the dual of this edge, directed from right to left
        private QuadEdge<TCoordinate> _rot;
        private Vertex<TCoordinate> _vertex;            // The vertex that this edge represents
        private QuadEdge<TCoordinate> _next;              // A reference to a connected edge
        private Object _data;
        //    private int      visitedKey = 0;

        ///<summary>
        /// Quadedges must be made using <see cref="MakeEdge"/>, to ensure proper construction.
        /// </summary>
        private QuadEdge()
        {
        }

        ///<summary>
        /// Gets the primary edge of this quadedge and its <tt>sym</tt>.
        /// The primary edge is the one for which the origin
        /// and destination coordinates are ordered
        /// according to the standard {@link Coordinate} ordering
        ///</summary>
        ///<returns>the primary quadedge</returns>
        public QuadEdge<TCoordinate> GetPrimary()
        {
            if (Origin.Coordinate.CompareTo(Destination.Coordinate) <= 0)
                return this;
            
            return Sym();
        }

        ///<summary>
        ///Gets/Sets the external data value for this edge.
        ///</summary>
        public Object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        ///<summary>
        /// Marks this quadedge as being deleted.
        /// This does not free the memory used by
        /// this quadedge quartet, but indicates
        /// that this edge no longer participates
        /// in a subdivision.
        ///</summary>
        public void Delete()
        {
            _rot = null;
        }

        ///<summary>
        /// Tests whether this edge has been deleted.
        ///</summary>
        public Boolean IsLive
        {
            get { return _rot != null; }
        }


        ///<summary>Gets/Sets the connected edge
        ///</summary>
        public QuadEdge<TCoordinate> Next
        {
            get { return _next; }
            set { _next = value; }
        }

        /***************************************************************************
         * QuadEdge Algebra 
         ***************************************************************************
         */

        ///<summary>
        /// Gets the dual of this edge, directed from its right to its left.
        ///</summary>
        public QuadEdge<TCoordinate> Rot
        {
            get { return _rot; }
            private set { _rot = value; }
        }

        ///<summary>
        /// Gets the dual of this edge, directed from its left to its right.
        ///</summary>
        ///<returns>the inverse rotated edge.</returns>
        public QuadEdge<TCoordinate> InverseRot()
        {
            return _rot.Sym();
        }

        ///<summary>
        /// Gets the edge from the destination to the origin of this edge.
        ///</summary>
        ///<returns>the sym of the edge</returns>
        public QuadEdge<TCoordinate> Sym()
        {
            return _rot.Rot;
        }

        ///<summary>
        /// Gets the next CCW edge around the origin of this edge.
        ///</summary>
        public QuadEdge<TCoordinate> OriginNext
        {
            get { return _next; }
        }

        ///<summary>
        /// Gets the next CW edge around (from) the origin of this edge.
        ///</summary>
        public QuadEdge<TCoordinate> OriginPrev
        {
            get { return _rot.Next.Rot; }
        }

        ///<summary>
        /// Gets the next CCW edge around (into) the destination of this edge.
        ///</summary>
        public QuadEdge<TCoordinate> DestinationNext
        {
            get { return Sym().OriginNext.Sym(); }
        }

        ///<summary>
        /// Gets the next CW edge around (into) the destination of this edge.
        ///</summary>
        public QuadEdge<TCoordinate> DestinationPrev
        {
            get { return InverseRot().Next.InverseRot(); }
        }

        ///<summary>
        /// Gets the CCW edge around the left face following this edge.
        ///</summary>
        public QuadEdge<TCoordinate> LeftNext
        {
            get { return InverseRot().Next.Rot; }
        }

        ///<summary>
        /// Gets the CCW edge around the left face before this edge.
        ///</summary>
        public QuadEdge<TCoordinate> LeftPrev
        {
            get { return Next.Sym(); }
        }

        ///<summary>
        /// Gets the edge around the right face ccw following this edge.
        ///</summary>
        public QuadEdge<TCoordinate> RightNext
        {
            get { return _rot.Next.InverseRot(); }
        }

        ///<summary>
        /// Gets the edge around the right face ccw before this edge.
        ///</summary>
        public QuadEdge<TCoordinate> RightPrev
        {
            get { return Sym().OriginNext; }
        }

        /***********************************************************************************************
         * Data Access
         **********************************************************************************************/
        ///<summary>
        /// Gets/Sets the vertex for this edge's origin
        ///</summary>
        public Vertex<TCoordinate> Origin
        {
            get { return _vertex; }
            /*private*/ set { _vertex = value; }
        }

        ///<summary>
        /// Sets the vertex for this edge's destination
        ///</summary>
        public Vertex<TCoordinate> Destination
        {
            get
            {
                return Sym().Origin;
            }
            set
            {
                Sym().Origin = value;
            }
        }


        ///**
        // * Gets the vertex for the edge's origin
        // * 
        // * @return the origin vertex
        // */
        //public Vertex<TCoordinate> Origin
        //{
        //    return vertex;
        //}

        ///**
        // * Gets the vertex for the edge's destination
        // * 
        // * @return the destination vertex
        // */
        //public final Vertex dest() {
        //    return sym().orig();
        //}

        ///<summary>
        /// Gets the length of the geometry of this quadedge.
        ///</summary>
        /// <returns>the length of the quadedge</returns>
        public double GetLength()
        {
            return Origin.Coordinate.Distance(Destination.Coordinate);
        }

        ///<summary>
        /// Tests if this quadedge and another have the same line segment geometry, 
        ///</summary>
        ///<param name="qe">a quadege</param>
        ///<returns>true if the quadedges are based on the same line segment regardless of orientation regardless of orientation.</returns>
        public Boolean EqualsNonOriented(QuadEdge<TCoordinate> qe)
        {
            if (EqualsOriented(qe))
                return true;
            if (EqualsOriented(qe.Sym()))
                return true;
            return false;
        }

        ///<summary>
        /// Tests if this quadedge and another have the same line segment geometry
        /// with the same orientation.
        ///</summary>
        ///<param name="qe">a quadege</param>
        ///<returns>true if the quadedges are based on the same line segment</returns>
        public Boolean EqualsOriented(QuadEdge<TCoordinate> qe)
        {
            if (Origin.Coordinate.Equals((ICoordinate2D)qe.Origin.Coordinate) &&
                Destination.Coordinate.Equals((ICoordinate2D)qe.Destination.Coordinate))
                return true;
            return false;
        }


        ///<summary>
        /// Creates a <see cref="LineSegment{TCoordinate}"/> representing the 
        /// geometry of this edge.
        /// </summary>
        ///<returns>LineSegment</returns>
        public LineSegment<TCoordinate> ToLineSegment()
        {
            return new LineSegment<TCoordinate>(Origin.Coordinate, Destination.Coordinate);
        }

        /**
         * Converts this edge to a WKT two-point <tt>LINESTRING</tt> indicating 
         * the geometry of this edge.
         * 
         * @return a String representing this edge's geometry
         */

        public override String ToString()
        {
            TCoordinate p0 = Origin.Coordinate;
            TCoordinate p1 = Destination.Coordinate;
            return String.Format("LINESTRING({0} {1})", p0, p1);
        }
    }
}
