using System;
using System.Text;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.EdgeGraph
{
    /// <summary>
    /// Represents a directed component of an edge in an <see cref="EdgeGraph"/>.
    /// HalfEdges link vertices whose locations are defined by <see cref="Coordinate"/>s.
    /// HalfEdges start at an <b>origin</b> vertex,
    /// and terminate at a <b>destination</b> vertex.
    /// HalfEdges always occur in symmetric pairs, with the <see cref="Sym"/> method
    /// giving access to the oppositely-oriented component.
    /// HalfEdges and the methods on them form an edge algebra,
    /// which can be used to traverse and query the topology
    /// of the graph formed by the edges.
    /// <para/>
    /// To support graphs where the edges are sequences of coordinates
    /// each edge may also have a direction point supplied.
    /// This is used to determine the ordering
    /// of the edges around the origin.
    /// HalfEdges with the same origin are ordered
    /// so that the ring of edges formed by them is oriented CCW.
    /// <para/>
    /// By design HalfEdges carry minimal information
    /// about the actual usage of the graph they represent.
    /// They can be subclassed to carry more information if required.
    /// <para/>
    /// HalfEdges form a complete and consistent data structure by themselves,
    /// but an <see cref="EdgeGraph"/> is useful to allow retrieving edges
    /// by vertex and edge location, as well as ensuring
    /// edges are created and linked appropriately.
    /// </summary>
    /// <author>Martin Davis</author>
    public class HalfEdge : IComparable<HalfEdge>
    {
        /// <summary>
        ///  Creates a HalfEdge pair representing an edge
        /// between two vertices located at coordinates p0 and p1.
        /// </summary>
        /// <param name="p0">a vertex coordinate</param>
        /// <param name="p1">a vertex coordinate</param>
        /// <returns>the HalfEdge with origin at p0</returns>
        public static HalfEdge Create(Coordinate p0, Coordinate p1)
        {
            var e0 = new HalfEdge(p0);
            var e1 = new HalfEdge(p1);
            e0.Link(e1);
            return e0;
        }

        /// <summary>
        /// Initialize a symmetric pair of halfedges.
        /// Intended for use by <see cref="EdgeGraph" />
        /// subclasses.
        /// <para/>
        /// The edges are initialized to have each other
        /// as the <see cref="Sym"/> edge, and to have
        /// <see cref="Next"/> pointers which point to edge other.
        /// This effectively creates a graph containing a single edge.
        /// </summary>
        /// <param name="e0">A halfedge</param>
        /// <param name="e1">A symmetric halfedge</param>
        /// <returns>The initialized edge e0</returns>
        [Obsolete]
        public static HalfEdge Init(HalfEdge e0, HalfEdge e1)
        {
            // ensure only newly created edges can be initialized, to prevent information loss
            if (e0.Sym != null || e1.Sym != null
                               || e0.Next != null || e1.Next != null)
                throw new InvalidOperationException("Edges are already initialized");
            e0.Link(e1);
            return e0;
        }
        private readonly Coordinate _orig;
        private HalfEdge _sym;
        private HalfEdge _next;

        /// <summary>
        /// Creates an edge originating from a given coordinate.
        /// </summary>
        /// <param name="orig">the origin coordinate</param>
        public HalfEdge(Coordinate orig)
        {
            _orig = orig;
        }

        /// <summary>
        /// Links this edge with its sym (opposite) edge.
        /// This must be done for each pair of edges created.
        /// </summary>
        /// <param name="sym">The sym edge to link.</param>
        public virtual void Link(HalfEdge sym)
        {
            Sym = sym;
            sym.Sym = this;
            // set next ptrs for a single segment
            Next = sym;
            sym.Next = this;
        }

        /// <summary>
        /// Initializes this edge with <paramref name="e"/> as <see cref="Sym"/> edge.
        /// </summary>
        /// <param name="e">A symmetric half edge.</param>
        [Obsolete("Use Link")]
        protected virtual void Init(HalfEdge e)
        {
            Link(e);
        }

        /// <summary>
        /// Gets the origin coordinate of this edge.
        /// </summary>
        public Coordinate Orig => _orig;

        /// <summary>
        /// Gets the destination coordinate of this edge.
        /// </summary>
        public Coordinate Dest => Sym.Orig;

        /// <summary>
        /// Gets a value indicating the X component of the direction vector.
        /// </summary>
        /// <returns>The X component of the direction vector</returns>
        double DirectionX { get => DirectionPt.X - Orig.X; }

        /// <summary>
        /// Gets a value indicating the Y component of the direction vector.
        /// </summary>
        /// <returns>The Y component of the direction vector</returns>
        double DirectionY { get => DirectionPt.Y - Orig.Y; }

        /// <summary>
        /// Gets a value indicating the direction point of this edge.
        /// In the base case this is the dest coordinate
        /// of the edge.
        /// <para/>
        /// Subclasses may override to
        /// allow a HalfEdge to represent an edge with more than two coordinates.
        /// </summary>
        /// <returns>The direction point for the edge</returns>
        protected virtual Coordinate DirectionPt
        {
            // default is to assume edges have only 2 vertices
            // subclasses may override to provide an internal direction point
            get => Dest;
        }

        /// <summary>
        /// Gets or sets the symmetric (opposite) edge of this edge.
        /// </summary>
        public HalfEdge Sym
        {
            get => _sym;
            private set => _sym = value;
        }

        /// <summary>
        /// Gets the next edge CCW around the destination vertex of this edge.
        /// If the destination vertex has degree <c>1</c> then this is the <c>Sym</c> edge.
        /// </summary>
        /// <returns>The next outgoing edge CCW around the destination vertex</returns>
        public HalfEdge Next
        {
            get => _next;
            private set => _next = value;
        }

        /// <summary>
        /// Gets the previous edge CW around the origin
        /// vertex of this edge,
        /// with that vertex being its destination.
        /// <para/>
        /// It is always true that <c>e.Next.Prev == e</c>
        /// <para/>
        /// Note that this requires a scan of the origin edges,
        /// so may not be efficient for some uses.
        /// </summary>
        /// <returns>The previous edge CW around the origin vertex</returns>
        public HalfEdge Prev {
            get
            {
                var curr = this;
                HalfEdge prev = null;
                do {
                    prev = curr;
                    curr = curr.ONext;
                } while (curr != this);

                return prev.Sym;
            }
        }

        /// <summary>
        /// Gets the next edge CCW around the origin of this edge,
        /// with the same origin.<br/>
        /// If the origin vertex has degree <c>1</c> then this is the edge itself.
        /// <para/>
        /// <c>e.ONext</c> is equal to <c>e.Sym.Next()</c>
        /// </summary>
        /// <returns>The next edge around the origin</returns>
        public HalfEdge ONext => Sym.Next;

        /// <summary>
        /// Finds the edge starting at the origin of this edge
        /// with the given dest vertex, if any.
        /// </summary>
        /// <param name="dest">the dest vertex to search for</param>
        /// <returns>
        /// the edge with the required dest vertex,
        /// if it exists, or null
        /// </returns>
        public HalfEdge Find(Coordinate dest)
        {
            var oNext = this;
            do
            {
                if (oNext == null)
                    return null;
                if (oNext.Dest.Equals2D(dest))
                    return oNext;
                oNext = oNext.ONext;
            }
            while (oNext != this);
            return null;
        }

        /// <summary>
        /// Tests whether this edge has the given orig and dest vertices.
        /// </summary>
        /// <param name="p0">the origin vertex to test</param>
        /// <param name="p1">the destination vertex to test</param>
        /// <returns><c>true</c> if the vertices are equal to the ones of this edge</returns>
        public bool Equals(Coordinate p0, Coordinate p1)
        {
            return Orig.Equals2D(p0) && Sym.Orig.Equals(p1);
        }

        /// <summary>
        /// Inserts an edge
        /// into the ring of edges around the origin vertex of this edge,
        /// ensuring that the edges remain ordered CCW.
        /// The inserted edge must have the same origin as this edge.
        /// </summary>
        /// <param name="eAdd">the edge to insert</param>
        public void Insert(HalfEdge eAdd)
        {
            // If this is only edge at origin, insert it after this
            if (ONext == this)
            {
                // set linkage so ring is correct
                InsertAfter(eAdd);
                return;
            }

            // Scan edges until insertion point is found
            var ePrev = InsertionEdge(eAdd);
            ePrev.InsertAfter(eAdd);
        }

        /// <summary>
        /// Finds the insertion edge for a edge
        /// being added to this origin,
        /// ensuring that the star of edges
        /// around the origin remains fully CCW.
        /// </summary>
        /// <param name="eAdd">The edge being added</param>
        /// <returns>The edge to insert after</returns>
        private HalfEdge InsertionEdge(HalfEdge eAdd)
        {
            var ePrev = this;
            do
            {
                var eNext = ePrev.ONext;
                /*
                 * Case 1: General case,
                 * with eNext higher than ePrev.
                 * 
                 * Insert edge here if it lies between ePrev and eNext.  
                 */
                if (eNext.CompareTo(ePrev) > 0
                    && eAdd.CompareTo(ePrev) >= 0
                    && eAdd.CompareTo(eNext) <= 0)
                {
                    return ePrev;
                }
                /*
                 * Case 2: Origin-crossing case,
                 * indicated by eNext <= ePrev.
                 * 
                 * Insert edge here if it lies
                 * in the gap between ePrev and eNext across the origin. 
                 */
                if (eNext.CompareTo(ePrev) <= 0
                    && (eAdd.CompareTo(eNext) <= 0 || eAdd.CompareTo(ePrev) >= 0))
                {
                    return ePrev;
                }
                ePrev = eNext;
            } while (ePrev != this);
            Assert.ShouldNeverReachHere();
            return null;
        }

        /// <summary>
        /// Insert an edge with the same origin after this one.
        /// Assumes that the inserted edge is in the correct
        /// position around the ring.
        /// </summary>
        /// <param name="e">the edge to insert (with same origin)</param>
        private void InsertAfter(HalfEdge e)
        {
            Assert.IsEquals(Orig, e.Orig);
            var save = ONext;
            Sym.Next = e;
            e.Sym.Next = save;
        }

        /// <summary>
        /// Tests whether the edges around the origin
        /// are sorted correctly.
        /// Note that edges must be strictly increasing,
        /// which implies no two edges can have the same direction point.
        /// </summary>
        /// <returns><c>true</c> if the origin edges are sorted correctly
        /// </returns>
        public bool IsEdgesSorted
        {
            get
            {
                // find lowest edge at origin
                var lowest = FindLowest();
                var e = lowest;
                // check that all edges are sorted
                do
                {
                    var eNext = e.ONext;
                    if (eNext == lowest) break;
                    bool isSorted = eNext.CompareTo(e) > 0;
                    if (!isSorted)
                    {
                        //int comp = eNext.compareTo(e);
                        return false;
                    }

                    e = eNext;
                } while (e != lowest);

                return true;
            }
        }

        /// <summary>
        /// Finds the lowest edge around the origin,
        /// using the standard edge ordering.
        /// </summary>
        /// <returns>The lowest edge around the origin</returns>
        private HalfEdge FindLowest()
        {
            var lowest = this;
            var e = this.ONext;
            do
            {
                if (e.CompareTo(lowest) < 0)
                    lowest = e;
                e = e.ONext;
            } while (e != this);
            return lowest;
        }

        /// <summary>
        /// Compares edges which originate at the same vertex
        /// based on the angle they make at their origin vertex with the positive X-axis.
        /// This allows sorting edges around their origin vertex in CCW order.
        /// </summary>
        public int CompareTo(HalfEdge e)
        {
            return CompareAngularDirection(e);
        }

        /// <summary>
        /// Implements the total order relation.
        /// The angle of edge a is greater than the angle of edge b,
        /// where the angle of an edge is the angle made by
        /// the first segment of the edge with the positive x-axis.
        /// When applied to a list of edges originating at the same point,
        /// this produces a CCW ordering of the edges around the point.
        /// Using the obvious algorithm of computing the angle is not robust,
        /// since the angle calculation is susceptible to round off error.
        /// </summary>
        /// <remarks>
        /// A robust algorithm is:
        /// 1. compare the quadrants the edge vectors lie in.
        /// If the quadrants are different,
        /// it is trivial to determine which edge has a greater angle.
        /// 2. If the vectors lie in the same quadrant, the
        /// <see cref="Orientation.Index"/> function
        /// can be used to determine the relative orientation of the vectors.
        /// </remarks>
        public int CompareAngularDirection(HalfEdge e)
        {
            double dx = DirectionX;
            double dy = DirectionY;
            double dx2 = e.DirectionX;
            double dy2 = e.DirectionY;

            // same vector
            if (dx == dx2 && dy == dy2)
                return 0;

            var quadrant = new Quadrant(dx, dy);
            var quadrant2 = new Quadrant(dx2, dy2);

            /*
             * if the vectors are in different quadrants,
             * determining the ordering is trivial
             */
            if (quadrant > quadrant2) return 1;
            if (quadrant < quadrant2) return -1;

            //--- vectors are in the same quadrant
            // Check relative orientation of direction vectors
            // this is > e if it is CCW of e
            var dir1 = DirectionPt;
            var dir2 = e.DirectionPt;
            return (int)Orientation.Index(e.Orig, dir2, dir1);
        }

        /// <summary>
        /// The X component of the distance between the orig and dest vertices.
        /// </summary>
        [Obsolete("Use DirectionX")]
        public double DeltaX => DirectionX;

        /// <summary>
        /// The Y component of the distance between the orig and dest vertices.
        /// </summary>
        [Obsolete("Use DirectionY")]
        public double DeltaY => DirectionY;

        /// <summary>
        /// Computes a string representation of a HalfEdge.
        /// </summary>
        public override string ToString()
        {
            return string.Format("HE({0} {1}, {2} {3})", Orig.X, Orig.Y, Sym.Orig.X, Sym.Orig.Y);
        }

        /// <summary>
        /// Provides a string representation of the edges around
        /// the origin node of this edge.
        /// </summary>
        /// <remarks>
        /// Uses the subclass representation for each edge.
        /// </remarks>
        /// <returns>A string showing the edges around the origin</returns>
        public string ToStringNode()
        {
            var orig = Orig;
            var dest = Dest;
            var sb = new StringBuilder();
            sb.Append($"Node( {WKTWriter.Format(orig)} )\n");
            var e = this;
            do
            {
                sb.Append("  -> " + e);
                sb.Append("\n");
                e = e.Next;
            } while (e != this);
            return sb.ToString();
        }

        /// <summary>
        /// Computes the degree of the origin vertex.
        /// The degree is the number of edges
        /// originating from the vertex.
        /// </summary>
        /// <returns>the degree of the origin vertex</returns>
        public int Degree()
        {
            int degree = 0;
            var e = this;
            do
            {
                degree++;
                e = e.ONext;
            }
            while (e != this);
            return degree;
        }

        /// <summary>
        /// Finds the first node previous to this edge, if any.
        /// If no such node exists (i.e. the edge is part of a ring)
        /// then null is returned.
        /// </summary>
        /// <returns>
        /// an edge originating at the node prior to this edge, if any,
        /// or null if no node exists
        /// </returns>
        public HalfEdge PrevNode()
        {
            var e = this;
            while (e.Degree() == 2)
            {
                e = e.Prev;
                if (e == this)
                    return null;
            }
            return e;
        }
    }
}
