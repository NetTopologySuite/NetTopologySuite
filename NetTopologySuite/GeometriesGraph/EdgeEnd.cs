using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Utilities;
namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Models the end of an edge incident on a node.
    /// </summary>
    /// <remarks>
    /// <para>
    /// EdgeEnds have a direction determined by the direction of the ray from the initial
    /// point to the next point.
    /// </para>
    /// <para>
    /// EdgeEnds are IComparable under the ordering  "a has a greater angle with the x-axis than b".
    /// This ordering is used to sort EdgeEnds around a node.
    /// </para>
    /// </remarks>
    public class EdgeEnd : IComparable<EdgeEnd>
    {
        /// <summary>
        ///
        /// </summary>
        private Label _label;
        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        protected EdgeEnd(Edge edge)
        {
            this.Edge = edge;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public EdgeEnd(Edge edge, Coordinate p0, Coordinate p1) :
            this(edge, p0, p1, null) { }
        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="label"></param>
        public EdgeEnd(Edge edge, Coordinate p0, Coordinate p1, Label label)
            : this(edge)
        {
            Init(p0, p1);
            _label = label;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        protected void Init(Coordinate p0, Coordinate p1)
        {
            Coordinate = p0;
            DirectedCoordinate = p1;
            Dx = p1.X - p0.X;
            Dy = p1.Y - p0.Y;
            Quadrant = QuadrantOp.Quadrant(Dx, Dy);
            Assert.IsTrue(! (Dx == 0 && Dy == 0), "EdgeEnd with identical endpoints found");
        }
        /// <summary>
        ///
        /// </summary>
        public Edge Edge { get; protected set; }
        /// <summary>
        ///
        /// </summary>
        public Label Label
        {
            get => _label;
            protected set => _label = value;
        }
        /// <summary>
        ///
        /// </summary>
        public Coordinate Coordinate { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public Coordinate DirectedCoordinate { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public int Quadrant { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public double Dx { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public double Dy { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public Node Node { get; set; }
        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public int CompareTo(EdgeEnd e)
        {
            //EdgeEnd e = (EdgeEnd) obj;
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
            if (Dx == e.Dx && Dy == e.Dy)
                return 0;
            // if the rays are in different quadrants, determining the ordering is trivial
            if (Quadrant > e.Quadrant)
                return 1;
            if (Quadrant < e.Quadrant)
                return -1;
            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return (int)Orientation.Index(e.Coordinate, e.DirectedCoordinate, DirectedCoordinate);
        }
        /// <summary>
        /// Subclasses should override this if they are using labels
        /// </summary>
        /// <param name="boundaryNodeRule"></param>
        public virtual void ComputeLabel(IBoundaryNodeRule boundaryNodeRule) { }
        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public virtual void Write(StreamWriter outstream)
        {
            var angle = Math.Atan2(Dy, Dx);
            var fullname = GetType().FullName;
            var lastDotPos = fullname.LastIndexOf('.');
            var name = fullname.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + Coordinate + " - " + DirectedCoordinate + " " + Quadrant + ":" + angle + "   " + _label);
        }
        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            var angle = Math.Atan2(Dy, Dx);
            var className = GetType().Name;
            //var lastDotPos = className.LastIndexOf('.');
            //var name = className.Substring(lastDotPos + 1);
            return "  " + className + ": " + Coordinate + " - " + DirectedCoordinate + " " + Quadrant + ":" + angle + "   " + _label;
        }
    }
}
