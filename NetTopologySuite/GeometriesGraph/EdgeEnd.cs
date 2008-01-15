using System;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// Models the end of an edge incident on a node.
    /// </summary>
    /// <remarks>
    /// <see cref="EdgeEnd{TCoordinate}"/>s have a direction determined by the direction of the ray 
    /// from the initial point to the next point.
    /// EdgeEnds are <see cref="IComparable{EdgeEnd}"/> under the ordering
    /// "a has a greater angle with the x-axis than b".
    /// This ordering is used to sort EdgeEnds around a node.
    /// </remarks>
    public class EdgeEnd<TCoordinate> : IComparable<EdgeEnd<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private Edge<TCoordinate> _edge = null;
        private Label? _label;
        private Node<TCoordinate> _origin; // the node this edge end originates at
        private TCoordinate _p0, _p1; // points of initial line segment
        private TCoordinate _direction; // the direction vector for this edge from its starting point
        private Quadrants _quadrant;

        protected EdgeEnd(Edge<TCoordinate> edge)
        {
            _edge = edge;
        }

        public EdgeEnd(Edge<TCoordinate> edge, TCoordinate p0, TCoordinate p1) :
            this(edge, p0, p1, null) { }

        public EdgeEnd(Edge<TCoordinate> edge, TCoordinate p0, TCoordinate p1, Label? label)
            : this(edge)
        {
            Init(p0, p1);
            _label = label;
        }

        protected void Init(TCoordinate p0, TCoordinate p1)
        {
            _p0 = p0;
            _p1 = p1;
            //Double dx = p1[Ordinates.X] - p0[Ordinates.X];
            //Double dy = p1[Ordinates.Y] - p0[Ordinates.Y];
            //_direction = _edge.Coordinates.CoordinateFactory.Create(dx, dy);
            _direction = p1.Subtract(p0);
            _quadrant = QuadrantOp<TCoordinate>.Quadrant(_direction);

            Assert.IsTrue(!_direction.Equals(((ICoordinate)_direction).Zero), 
                "EdgeEnd with identical endpoints found.");
        }

        public Edge<TCoordinate> Edge
        {
            get { return _edge; }
            protected set { _edge = value; }
        }

        public Label? Label
        {
            get { return _label; }
            set { _label = value; }
        }

        public TCoordinate Coordinate
        {
            get { return _p0; }
        }

        public TCoordinate DirectedCoordinate
        {
            get { return _p1; }
        }

        public Quadrants Quadrant
        {
            get { return _quadrant; }
        }

        //public Double Dx
        //{
        //    get { return dx; }
        //}

        //public Double Dy
        //{
        //    get { return dy; }
        //}

        public TCoordinate Direction
        {
            get { return _direction; }
        }

        public Node<TCoordinate> Node
        {
            get { return _origin; }
            set { _origin = value; }
        }

        public Int32 CompareTo(EdgeEnd<TCoordinate> other)
        {
            return CompareDirection(other);
        }

        /// <summary> 
        /// Implements the total order relation:
        /// a has a greater angle with the positive x-axis than b.
        /// Using the obvious algorithm of simply computing the angle is not robust,
        /// since the angle calculation is obviously susceptible to roundoff.
        /// A robust algorithm is:
        /// - first compare the quadrant.  If the quadrants
        /// are different, it it trivial to determine which vector is "greater".
        /// - if the vectors lie in the same quadrant, the computeOrientation function
        /// can be used to decide the relative orientation of the vectors.
        /// </summary>
        public Int32 CompareDirection(EdgeEnd<TCoordinate> e)
        {
            if (Direction.Equals(e.Direction))
            {
                return 0;
            }

            // if the rays are in different quadrants, determining the ordering is trivial
            if (_quadrant > e.Quadrant)
            {
                return 1;
            }

            if (_quadrant < e.Quadrant)
            {
                return -1;
            }

            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return (Int32)CGAlgorithms<TCoordinate>.ComputeOrientation(
                e.Coordinate, e.DirectedCoordinate, _p1);
        }

        /// <summary>
        /// Subclasses should override this if they are using labels
        /// </summary>
        public virtual void ComputeLabel() { }

        public virtual void Write(StreamWriter outstream)
        {
            Double angle = Math.Atan2(_direction[Ordinates.Y], _direction[Ordinates.X]);
            string fullname = GetType().FullName;
            Int32 lastDotPos = fullname.LastIndexOf('.');
            string name = fullname.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + _p0 + " - " + _p1 + " " + _quadrant + ":" + angle + "   " + Label);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(('['));
            sb.Append(_p0[Ordinates.X]);
            sb.Append((' '));
            sb.Append(_p1[Ordinates.Y]);
            sb.Append((']'));
            return sb.ToString();
        }
    }
}