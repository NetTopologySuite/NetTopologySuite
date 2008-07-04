using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
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

        /// <summary>
        /// 
        /// </summary>
        protected Label label = null;

        private Node node;          // the node this edge end originates at
        private ICoordinate p0, p1;  // points of initial line segment
        private double dx, dy;      // the direction vector for this edge from its starting point
        private int quadrant;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        protected EdgeEnd(Edge edge)
        {
            this.edge = edge;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public EdgeEnd(Edge edge, ICoordinate p0, ICoordinate p1) : 
            this(edge, p0, p1, null) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="label"></param>
        public EdgeEnd(Edge edge, ICoordinate p0, ICoordinate p1, Label label)
            : this(edge)
        {
            Init(p0, p1);
            this.label = label;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        protected void Init(ICoordinate p0, ICoordinate p1)
        {
            this.p0 = p0;
            this.p1 = p1;
            dx = p1.X - p0.X;
            dy = p1.Y - p0.Y;
            quadrant = QuadrantOp.Quadrant(dx, dy);
            Assert.IsTrue(! (dx == 0 && dy == 0), "EdgeEnd with identical endpoints found");
        }

        /// <summary>
        /// 
        /// </summary>
        public Edge Edge
        {
            get
            {
                return edge;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Label Label
        {
            get
            {
                return label;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate Coordinate
        {
            get
            {
                return p0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate DirectedCoordinate
        {
            get
            {
                return p1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Quadrant
        {
            get
            {
                return quadrant;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Dx
        {
            get
            {
                return dx;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Dy
        {
            get
            {
                return dy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Node Node
        {
            get
            {
                return node;
            }
            set
            {
                this.node = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
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
        /// <param name="e"></param>
        public int CompareDirection(EdgeEnd e)
        {
            if (dx == e.dx && dy == e.dy)
                return 0;
            // if the rays are in different quadrants, determining the ordering is trivial
            if (quadrant > e.quadrant)
                return 1;
            if (quadrant < e.quadrant)
                return -1;
            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return CGAlgorithms.ComputeOrientation(e.p0, e.p1, p1);
        }

        /// <summary>
        /// Subclasses should override this if they are using labels
        /// </summary>
        public virtual void ComputeLabel() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public virtual void Write(StreamWriter outstream)
        {            
            double angle = Math.Atan2(dy, dx);
            string fullname = this.GetType().FullName;
            int lastDotPos = fullname.LastIndexOf('.');
            string name = fullname.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + p0 + " - " + p1 + " " + quadrant + ":" + angle + "   " + label);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
