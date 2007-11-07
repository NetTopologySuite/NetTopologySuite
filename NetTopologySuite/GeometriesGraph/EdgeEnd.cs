using System;
using System.IO;
using System.Text;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// Models the end of an edge incident on a node.
    /// EdgeEnds have a direction
    /// determined by the direction of the ray from the initial
    /// point to the next point.
    /// EdgeEnds are IComparable under the ordering
    /// "a has a greater angle with the x-axis than b".
    /// This ordering is used to sort EdgeEnds around a node.
    /// </summary>
    public class EdgeEnd : IComparable
    {
        /// <summary>
        /// The parent edge of this edge end.
        /// </summary>
        protected Edge edge = null;

        protected Label label = null;

        private Node node; // the node this edge end originates at
        private ICoordinate p0, p1; // points of initial line segment
        private Double dx, dy; // the direction vector for this edge from its starting point
        private Int32 quadrant;

        protected EdgeEnd(Edge edge)
        {
            this.edge = edge;
        }

        public EdgeEnd(Edge edge, ICoordinate p0, ICoordinate p1) :
            this(edge, p0, p1, null) {}

        public EdgeEnd(Edge edge, ICoordinate p0, ICoordinate p1, Label label)
            : this(edge)
        {
            Init(p0, p1);
            this.label = label;
        }

        protected void Init(ICoordinate p0, ICoordinate p1)
        {
            this.p0 = p0;
            this.p1 = p1;
            dx = p1.X - p0.X;
            dy = p1.Y - p0.Y;
            quadrant = QuadrantOp.Quadrant(dx, dy);
            Assert.IsTrue(! (dx == 0 && dy == 0), "EdgeEnd with identical endpoints found");
        }

        public Edge Edge
        {
            get { return edge; }
        }

        public Label Label
        {
            get { return label; }
        }

        public ICoordinate Coordinate
        {
            get { return p0; }
        }

        public ICoordinate DirectedCoordinate
        {
            get { return p1; }
        }

        public Int32 Quadrant
        {
            get { return quadrant; }
        }

        public Double Dx
        {
            get { return dx; }
        }

        public Double Dy
        {
            get { return dy; }
        }

        public Node Node
        {
            get { return node; }
            set { node = value; }
        }

        public Int32 CompareTo(object obj)
        {
            EdgeEnd e = (EdgeEnd) obj;
            return CompareDirection(e);
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
        public Int32 CompareDirection(EdgeEnd e)
        {
            if (dx == e.dx && dy == e.dy)
            {
                return 0;
            }

            // if the rays are in different quadrants, determining the ordering is trivial
            if (quadrant > e.quadrant)
            {
                return 1;
            }

            if (quadrant < e.quadrant)
            {
                return -1;
            }

            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return CGAlgorithms.ComputeOrientation(e.p0, e.p1, p1);
        }

        /// <summary>
        /// Subclasses should override this if they are using labels
        /// </summary>
        public virtual void ComputeLabel() {}

        public virtual void Write(StreamWriter outstream)
        {
            Double angle = Math.Atan2(dy, dx);
            string fullname = GetType().FullName;
            Int32 lastDotPos = fullname.LastIndexOf('.');
            string name = fullname.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + p0 + " - " + p1 + " " + quadrant + ":" + angle + "   " + label);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(('['));
            sb.Append(p0.X);
            sb.Append((' '));
            sb.Append(p1.Y);
            sb.Append((']'));
            return sb.ToString();
        }
    }
}