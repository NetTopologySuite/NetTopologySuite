using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents a directed edge in a <see cref="PlanarGraph{TCoordinate}"/>. 
    /// </summary>
    /// <remarks>
    /// A <see cref="DirectedEdge{TCoordinate}"/> may or
    /// may not have a reference to a parent <see cref="Edge{TCoordinate}"/> (some applications of
    /// planar graphs may not require explicit Edge objects to be created). Usually
    /// a client using a <see cref="PlanarGraph{TCoordinate}"/> will subclass 
    /// <see cref="DirectedEdge{TCoordinate}"/> to add its own application-specific 
    /// data and methods.    
    /// </remarks>
    public class DirectedEdge<TCoordinate> : GraphComponent<TCoordinate>, IComparable<DirectedEdge<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Returns a set containing the parent <see cref="Edge"/> 
        /// (possibly <see langword="null"/>) for each of the given 
        /// <see cref="DirectedEdge{TCoordinate}"/>s.
        /// </summary>
        public static IEnumerable<Edge<TCoordinate>> ToEdges(IEnumerable<DirectedEdge<TCoordinate>> dirEdges)
        {
            foreach (DirectedEdge<TCoordinate> directedEdge in dirEdges)
            {
                yield return directedEdge._parentEdge;
            }
        }

        private Edge<TCoordinate> _parentEdge;
        private readonly Node<TCoordinate> _from;
        private readonly Node<TCoordinate> _to;
        private readonly TCoordinate _p0, _p1;
        private DirectedEdge<TCoordinate> _sym = null; // optional
        private readonly Boolean _edgeDirection;
        private readonly Quadrants _quadrant;
        private readonly Double _angle;

        /// <summary>
        /// Constructs a DirectedEdge connecting the <c>from</c> node to the
        /// <c>to</c> node.
        /// </summary>
        /// <param name="directionPt">
        /// Specifies this DirectedEdge's direction (given by an imaginary
        /// line from the <c>from</c> node to <c>directionPt</c>).
        /// </param>
        /// <param name="edgeDirection">
        /// Whether this DirectedEdge's direction is the same as or
        /// opposite to that of the parent Edge (if any).
        /// </param>
        public DirectedEdge(Node<TCoordinate> from, Node<TCoordinate> to, TCoordinate directionPt, Boolean edgeDirection)
        {
            _from = from;
            _to = to;
            _edgeDirection = edgeDirection;
            _p0 = from.Coordinate;
            _p1 = directionPt;
            Double dx = _p1[Ordinates.X] - _p0[Ordinates.X];
            Double dy = _p1[Ordinates.Y] - _p0[Ordinates.Y];
            _quadrant = QuadrantOp<TCoordinate>.Quadrant(dx, dy);
            _angle = Math.Atan2(dy, dx);
        }

        /// <summary>
        /// Returns this DirectedEdge's parent Edge, or null if it has none.
        /// Associates this DirectedEdge with an Edge (possibly null, indicating no associated
        /// Edge).
        /// </summary>
        public Edge<TCoordinate> Edge
        {
            get { return _parentEdge; }
            set { _parentEdge = value; }
        }

        /// <summary>
        /// Returns 0, 1, 2, or 3, indicating the quadrant in which this DirectedEdge's
        /// orientation lies.
        /// </summary>
        public Quadrants Quadrant
        {
            get { return _quadrant; }
        }

        /// <summary>
        /// Returns a point to which an imaginary line is drawn from the from-node to
        /// specify this DirectedEdge's orientation.
        /// </summary>
        public TCoordinate DirectionVector
        {
            get { return _p1; }
        }

        /// <summary>
        /// Returns whether the direction of the parent Edge (if any) is the same as that
        /// of this Directed Edge.
        /// </summary>
        public Boolean EdgeDirection
        {
            get { return _edgeDirection; }
        }

        /// <summary>
        /// Returns the node from which this DirectedEdge leaves.
        /// </summary>
        public Node<TCoordinate> FromNode
        {
            get { return _from; }
        }

        /// <summary>
        /// Returns the node to which this DirectedEdge goes.
        /// </summary>
        public Node<TCoordinate> ToNode
        {
            get { return _to; }
        }

        /// <summary>
        /// Returns the coordinate of the from-node.
        /// </summary>
        public TCoordinate Coordinate
        {
            get { return _from.Coordinate; }
        }

        /// <summary>
        /// Returns the angle that the start of this DirectedEdge makes with the
        /// positive x-axis, in radians.
        /// </summary>
        public Double Angle
        {
            get { return _angle; }
        }

        /// <summary>
        /// Returns the symmetric DirectedEdge -- the other DirectedEdge associated with
        /// this DirectedEdge's parent Edge.
        /// Sets this DirectedEdge's symmetric DirectedEdge, which runs in the opposite
        /// direction.
        /// </summary>
        public DirectedEdge<TCoordinate> Sym
        {
            get { return _sym; }
            set { _sym = value; }
        }

        /// <summary>
        /// Returns 1 if this DirectedEdge has a greater angle with the
        /// positive x-axis than b", 0 if the DirectedEdges are collinear, and -1 otherwise.
        /// Using the obvious algorithm of simply computing the angle is not robust,
        /// since the angle calculation is susceptible to roundoff. A robust algorithm
        /// is:
        /// first compare the quadrants. If the quadrants are different, it it
        /// trivial to determine which vector is "greater".
        /// if the vectors lie in the same quadrant, the robust
        /// <c>RobustCGAlgorithms.ComputeOrientation(Coordinate, Coordinate, Coordinate)</c>
        /// function can be used to decide the relative orientation of the vectors.
        /// </summary>
        public Int32 CompareTo(DirectedEdge<TCoordinate> other)
        {
            return (Int32) CompareDirection(other);
        }

        /// <summary>
        /// Returns 1 if this DirectedEdge has a greater angle with the
        /// positive x-axis than b", 0 if the DirectedEdges are collinear, and -1 otherwise.
        /// </summary>
        /// <remarks>
        /// Using the obvious algorithm of simply computing the angle is not robust,
        /// since the angle calculation is susceptible to roundoff. A robust algorithm
        /// is to first compare the quadrants. If the quadrants are different, it it
        /// trivial to determine which vector is "greater". 
        /// If the vectors lie in the same quadrant, the robust
        /// <see cref="CGAlgorithms{TCoordinate}.ComputeOrientation"/>
        /// function can be used to decide the relative orientation of the vectors.
        /// </remarks>
        public Orientation CompareDirection(DirectedEdge<TCoordinate> e)
        {
            // if the rays are in different quadrants, determining the ordering is trivial
            if (_quadrant > e.Quadrant)
            {
                return Orientation.Left;
            }

            if (_quadrant < e.Quadrant)
            {
                return Orientation.Right;
            }

            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return CGAlgorithms<TCoordinate>.ComputeOrientation(e._p0, e._p1, _p1);
        }

        /// <summary>
        /// Writes a detailed String representation of this DirectedEdge to the given PrintStream.
        /// </summary>
        public void Write(StreamWriter outstream)
        {
            String className = GetType().FullName;
            Int32 lastDotPos = className.LastIndexOf('.');
            String name = className.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + _p0 + " - " + _p1 + " " + _quadrant + ":" + _angle);
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public override Boolean IsRemoved
        {
            get { return _parentEdge == null; }
        }

        public override String ToString()
        {
            return "DirectedEdge: " + _p0 + " - " + _p1 + " " + _quadrant + ":" + _angle;
        }

        /// <summary>
        /// Removes this directed edge from its containing graph.
        /// </summary>
        internal void Remove()
        {
            _sym = null;
            _parentEdge = null;
        }
    }
}