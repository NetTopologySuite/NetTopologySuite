using System;
using System.IO;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
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

        private Coordinate _p0, _p1;  // points of initial line segment
        private double _dx, _dy;      // the direction vector for this edge from its starting point
        private Quadrant _quadrant;

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
            _p0 = p0;
            _p1 = p1;
            _dx = p1.X - p0.X;
            _dy = p1.Y - p0.Y;
            _quadrant = new Quadrant(_dx, _dy);
            Assert.IsTrue(! (_dx == 0 && _dy == 0), "EdgeEnd with identical endpoints found");
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
        public Coordinate Coordinate => _p0;

        /// <summary>
        ///
        /// </summary>
        public Coordinate DirectedCoordinate => _p1;

        /// <summary>
        ///
        /// </summary>
        [Obsolete("Use QuadrantEx")]
        public int Quadrant => _quadrant.Value;

        /// <summary>
        /// Gets a value indicating the <c>Quadrant</c> this <c>EdgeEnd</c> lies in.
        /// </summary>
        public Quadrant QuadrantEx => _quadrant;

        /// <summary>
        ///
        /// </summary>
        public double Dx => _dx;

        /// <summary>
        ///
        /// </summary>
        public double Dy => _dy;

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
        /// <para/>
        /// Using the obvious algorithm of simply computing the angle is not robust,
        /// since the angle calculation is obviously susceptible to round off.
        /// <para/>
        /// A robust algorithm is:
        /// <list type="bullet">
        /// <item><description>first compare the quadrant.  If the quadrants
        /// are different, it it trivial to determine which vector is "greater".</description></item>
        /// <item><description>if the vectors lie in the same quadrant, the computeOrientation function
        /// can be used to decide the relative orientation of the vectors.</description></item>
        /// </list>
        /// </summary>
        /// <param name="e">An EdgeEnd</param>
        /// <returns>The <see cref="OrientationIndex"/> of <paramref name="e"/> compared to <c>this</c> <see cref="EdgeEnd"/>.</returns>
        public int CompareDirection(EdgeEnd e)
        {
            if (_dx == e._dx && _dy == e._dy)
                return 0;
            // if the rays are in different quadrants, determining the ordering is trivial
            if (_quadrant > e._quadrant)
                return 1;
            if (_quadrant < e._quadrant)
                return -1;
            // vectors are in the same quadrant - check relative orientation of direction vectors
            // this is > e if it is CCW of e
            return (int)Orientation.Index(e._p0, e._p1, _p1);
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
            double angle = Math.Atan2(_dy, _dx);
            string fullname = GetType().FullName;
            int lastDotPos = fullname.LastIndexOf('.');
            string name = fullname.Substring(lastDotPos + 1);
            outstream.Write("  " + name + ": " + _p0 + " - " + _p1 + " " + _quadrant + ":" + angle + "   " + _label);
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            double angle = Math.Atan2(_dy, _dx);
            string className = GetType().Name;
            //var lastDotPos = className.LastIndexOf('.');
            //var name = className.Substring(lastDotPos + 1);
            return "  " + className + ": " + _p0 + " - " + _p1 + " " + _quadrant + ":" + angle + "   " + _label;
        }
    }
}
