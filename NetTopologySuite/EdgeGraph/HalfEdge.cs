using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
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
    /// </summary>
    /// <remarks>
    /// By design HalfEdges carry minimal information
    /// about the actual usage of the graph they represent.
    /// They can be subclassed to carry more information if required.
    /// </remarks>
    /// <remarks>
    /// HalfEdges form a complete and consistent data structure by themselves,
    /// but an <see cref="EdgeGraph"/> is useful to allow retrieving edges
    /// by vertex and edge location, as well as ensuring
    /// edges are created and linked appropriately.
    /// </remarks>
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
            e0.Init(e1);
            return e0;
        }

        /// <summary>
        /// Initialize a symmetric pair of HalfEdges.
        /// Intended for use by <see cref="EdgeGraph"/> subclasses.
        /// The edges are initialized to have each other
        /// as the <see cref="Sym"/> edge, and to have <see cref="Next"/> pointers
        /// which point to edge other.
        /// This effectively creates a graph containing a single edge.
        /// </summary>
        /// <param name="e0">a HalfEdge</param>
        /// <param name="e1">a symmetric HalfEdge</param>
        /// <returns>the initialized edge e0</returns>
        public static HalfEdge Init(HalfEdge e0, HalfEdge e1)
        {
            // ensure only newly created edges can be initialized, to prevent information loss
            if (e0.Sym != null || e1.Sym != null ||
                e0.Next != null || e1.Next != null)
                throw new ArgumentException("Edges are already initialized");
            e0.Init(e1);
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

        protected virtual void Init(HalfEdge e)
        {
            Sym = e;
            e.Sym = this;
            // set next ptrs for a single segment
            Next = e;
            e.Next = this;
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
        /// Gets the symmetric pair edge of this edge.
        /// </summary>
        public HalfEdge Sym
        {
            get => _sym;
            private set => _sym = value;
        }

        /// <summary>
        /// Gets the next edge CCW around the
        /// destination vertex of this edge.
        /// If the vertex has degree 1 then this is the <b>sym</b> edge.
        /// </summary>
        public HalfEdge Next
        {
            get => _next;
            private set => _next = value;
        }

        /// <summary>
        /// Returns the edge previous to this one
        /// (with dest being the same as this orig).
        /// </summary>
        public HalfEdge Prev => Sym.Next.Sym;

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
        /// into the ring of edges around the origin vertex of this edge.
        /// The inserted edge must have the same origin as this edge.
        /// </summary>
        /// <param name="e">the edge to insert</param>
        public void Insert(HalfEdge e)
        {
            // if no other edge around origin
            if (ONext == this)
            {
                // set linkage so ring is correct
                InsertAfter(e);
                return;
            }

            // otherwise, find edge to insert after
            int ecmp = CompareTo(e);
            var ePrev = this;
            do
            {
                var oNext = ePrev.ONext;
                int cmp = oNext.CompareTo(e);
                if (cmp != ecmp || oNext == this)
                {
                    ePrev.InsertAfter(e);
                    return;
                }
                ePrev = oNext;
            } while (ePrev != this);
            Assert.ShouldNeverReachHere();
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
        /// since the angle calculation is susceptible to roundoff error.
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
            double dx = DeltaX;
            double dy = DeltaY;
            double dx2 = e.DeltaX;
            double dy2 = e.DeltaY;

            // same vector
            if (dx == dx2 && dy == dy2)
                return 0;

            double quadrant = QuadrantOp.Quadrant(dx, dy);
            double quadrant2 = QuadrantOp.Quadrant(dx2, dy2);

            // if the vectors are in different quadrants, determining the ordering is trivial
            if (quadrant > quadrant2)
                return 1;
            if (quadrant < quadrant2)
                return -1;
            // vectors are in the same quadrant
            // Check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return (int)Orientation.Index(e.Orig, e.Dest, Dest);
        }

        /// <summary>
        /// The X component of the distance between the orig and dest vertices.
        /// </summary>
        public double DeltaX => Sym.Orig.X - Orig.X;

        /// <summary>
        /// The Y component of the distance between the orig and dest vertices.
        /// </summary>
        public double DeltaY => Sym.Orig.Y - Orig.Y;

        /// <summary>
        /// Computes a string representation of a HalfEdge.
        /// </summary>
        public override string ToString()
        {
            return string.Format("HE({0} {1}, {2} {3})", Orig.X, Orig.Y, Sym.Orig.X, Sym.Orig.Y);
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