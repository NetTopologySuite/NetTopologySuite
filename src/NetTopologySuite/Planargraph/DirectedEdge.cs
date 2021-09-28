using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents a directed edge in a <c>PlanarGraph</c>. A DirectedEdge may or
    /// may not have a reference to a parent Edge (some applications of
    /// planar graphs may not require explicit Edge objects to be created). Usually
    /// a client using a <c>PlanarGraph</c> will subclass <c>DirectedEdge</c>
    /// to add its own application-specific data and methods.
    /// </summary>
    public class DirectedEdge : GraphComponent, IComparable
    {
        /// <summary>
        /// Returns a List containing the parent Edge (possibly null) for each of the given
        /// DirectedEdges.
        /// </summary>
        /// <param name="dirEdges"></param>
        /// <returns></returns>
        public static IList<Edge> ToEdges(IList<DirectedEdge> dirEdges)
        {
            var edges = new List<Edge>();
            foreach (var directedEdge in dirEdges)
                edges.Add(directedEdge.parentEdge);
            return edges;
        }

        protected Edge parentEdge;

        protected Node from;
        protected Node to;

        protected Coordinate p0;
        protected Coordinate p1;

        private DirectedEdge _sym;  // optional

        private readonly Quadrant _quadrant;
        private readonly double _angle;

        /// <summary>
        /// Constructs a DirectedEdge connecting the <c>from</c> node to the
        /// <c>to</c> node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="directionPt">
        /// Specifies this DirectedEdge's direction (given by an imaginary
        /// line from the <c>from</c> node to <c>directionPt</c>).
        /// </param>
        /// <param name="edgeDirection">
        /// Whether this DirectedEdge's direction is the same as or
        /// opposite to that of the parent Edge (if any).
        /// </param>
        public DirectedEdge(Node from, Node to, Coordinate directionPt, bool edgeDirection)
        {
            this.from = from;
            this.to = to;
            this.EdgeDirection = edgeDirection;
            p0 = from.Coordinate;
            p1 = directionPt;
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            _quadrant = new Quadrant(dx, dy);
            _angle = Math.Atan2(dy, dx);
        }

        /// <summary>
        /// Returns this DirectedEdge's parent Edge, or null if it has none.
        /// Associates this DirectedEdge with an Edge (possibly null, indicating no associated
        /// Edge).
        /// </summary>
        public Edge Edge
        {
            get => parentEdge;
            set => parentEdge = value;
        }

        /// <summary>
        /// Returns 0, 1, 2, or 3, indicating the quadrant in which this DirectedEdge's
        /// orientation lies.
        /// </summary>
        [Obsolete("Use QuadrantValue")]
        public int Quadrant => _quadrant.Value;

        /// <summary>
        /// Returns 0, 1, 2, or 3, indicating the quadrant in which this DirectedEdge's
        /// orientation lies.
        /// </summary>
        public Quadrant QuadrantEx => _quadrant;

        /// <summary>
        /// Returns a point to which an imaginary line is drawn from the from-node to
        /// specify this DirectedEdge's orientation.
        /// </summary>
        public Coordinate DirectionPt => p1;

        /// <summary>
        /// Returns whether the direction of the parent Edge (if any) is the same as that
        /// of this Directed Edge.
        /// </summary>
        public bool EdgeDirection { get; protected set; }

        /// <summary>
        /// Returns the node from which this DirectedEdge leaves.
        /// </summary>
        public Node FromNode => from;

        /// <summary>
        /// Returns the node to which this DirectedEdge goes.
        /// </summary>
        public Node ToNode => to;

        /// <summary>
        /// Returns the coordinate of the from-node.
        /// </summary>
        public Coordinate Coordinate => from.Coordinate;

        /// <summary>
        /// Returns the angle that the start of this DirectedEdge makes with the
        /// positive x-axis, in radians.
        /// </summary>
        public double Angle => _angle;

        /// <summary>
        /// Returns the symmetric DirectedEdge -- the other DirectedEdge associated with
        /// this DirectedEdge's parent Edge.
        /// Sets this DirectedEdge's symmetric DirectedEdge, which runs in the opposite
        /// direction.
        /// </summary>
        public DirectedEdge Sym
        {
            get => _sym;
            set => _sym = value;
        }

        /// <summary>
        /// Returns 1 if this DirectedEdge has a greater angle with the
        /// positive x-axis than b", 0 if the DirectedEdges are collinear, and -1 otherwise.
        /// Using the obvious algorithm of simply computing the angle is not robust,
        /// since the angle calculation is susceptible to round off. A robust algorithm
        /// is:
        /// first compare the quadrants. If the quadrants are different, it it
        /// trivial to determine which vector is "greater".
        /// if the vectors lie in the same quadrant, the robust
        /// <c>RobustCGAlgorithms.ComputeOrientation(Coordinate, Coordinate, Coordinate)</c>
        /// function can be used to decide the relative orientation of the vectors.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var de = (DirectedEdge) obj;
            return CompareDirection(de);
        }

        /// <summary>
        /// Returns 1 if this DirectedEdge has a greater angle with the
        /// positive x-axis than b", 0 if the DirectedEdges are collinear, and -1 otherwise.
        /// Using the obvious algorithm of simply computing the angle is not robust,
        /// since the angle calculation is susceptible to round off. A robust algorithm
        /// is:
        /// first compare the quadrants. If the quadrants are different, it it
        /// trivial to determine which vector is "greater".
        /// if the vectors lie in the same quadrant, the robust
        /// <c>RobustCGAlgorithms.ComputeOrientation(Coordinate, Coordinate, Coordinate)</c>
        /// function can be used to decide the relative orientation of the vectors.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public int CompareDirection(DirectedEdge e)
        {
            // if the rays are in different quadrants, determining the ordering is trivial
            if (_quadrant > e.QuadrantEx)
                return 1;
            if (_quadrant < e.QuadrantEx)
                return -1;
            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return (int)Orientation.Index(e.p0, e.p1, p1);
        }

        /// <summary>
        /// Writes a detailed string representation of this DirectedEdge to the given PrintStream.
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            string className = GetType().FullName;
            int lastDotPos = className.LastIndexOf('.');
            string name = className.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + p0 + " - " + p1 + " " + _quadrant + ":" + _angle);
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        /// <value></value>
        public override bool IsRemoved => parentEdge == null;

        /// <summary>
        /// Removes this directed edge from its containing graph.
        /// </summary>
        internal void Remove()
        {
            _sym = null;
            parentEdge = null;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return "DirectedEdge: " + p0 + " - " + p1 + " " + _quadrant + ":" + _angle;
        }
    }
}
