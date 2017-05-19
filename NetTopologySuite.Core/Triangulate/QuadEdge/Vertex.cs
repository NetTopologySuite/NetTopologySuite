using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    ///     Models a site (node) in a <see cref="QuadEdgeSubdivision" />.
    ///     The sites can be points on a line string representing a
    ///     linear site.
    ///     <para />
    ///     The vertex can be considered as a vector with a norm, length, inner product, cross
    ///     product, etc. Additionally, point relations (e.g., is a point to the left of a line, the circle
    ///     defined by this point and two others, etc.) are also defined in this class.
    ///     <para />
    ///     It is common to want to attach user-defined data to
    ///     the vertices of a subdivision.
    ///     One way to do this is to subclass <tt>Vertex</tt>
    ///     to carry any desired information.
    /// </summary>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class Vertex : IEquatable<Vertex>
    {
        private const int LEFT = 0;
        private const int RIGHT = 1;
        private const int BEYOND = 2;
        private const int BEHIND = 3;
        private const int BETWEEN = 4;
        private const int ORIGIN = 5;
        private const int DESTINATION = 6;

        // private int edgeNumber = -1;

        /// <summary>
        ///     Creates an instance of this class using the given x- and y-ordinate valuse
        /// </summary>
        /// <param name="x">x-ordinate value</param>
        /// <param name="y">y-ordinate value</param>
        public Vertex(double x, double y)
        {
            Coordinate = new Coordinate(x, y);
        }

        /// <summary>
        ///     Creates an instance of this class using the given x-, y- and z-ordinate values
        /// </summary>
        /// <param name="x">x-ordinate value</param>
        /// <param name="y">y-ordinate value</param>
        /// <param name="z">z-ordinate value</param>
        public Vertex(double x, double y, double z)
        {
            Coordinate = new Coordinate(x, y, z);
        }

        /// <summary>
        ///     Creates an instance of this class using a clone of the given <see cref="Coordinate" />.
        /// </summary>
        /// <param name="p">The coordinate</param>
        public Vertex(Coordinate p)
        {
            Coordinate = new Coordinate(p);
        }

        /// <summary>
        ///     Gets the x-ordinate value
        /// </summary>
        public double X => Coordinate.X;

        /// <summary>
        ///     Gets the y-ordinate value
        /// </summary>
        public double Y => Coordinate.Y;

        /// <summary>
        ///     Gets the z-ordinate value
        /// </summary>
        public double Z
        {
            get { return Coordinate.Z; }
            set { Coordinate.Z = value; }
        }

        /// <summary>
        ///     Gets the coordinate
        /// </summary>
        public Coordinate Coordinate { get; }

        public bool Equals(Vertex x)
        {
            if ((Coordinate.X == x.X) && (Coordinate.Y == x.Y))
                return true;
            return false;
        }

        public override string ToString()
        {
            return "POINT (" + Coordinate.X + " " + Coordinate.Y + ")";
        }

        public bool Equals(Vertex x, double tolerance)
        {
            if (Coordinate.Distance(x.Coordinate) < tolerance)
                return true;
            return false;
        }

        public int Classify(Vertex p0, Vertex p1)
        {
            var p2 = this;
            var a = p1.Sub(p0);
            var b = p2.Sub(p0);

            var sa = a.CrossProduct(b);
            if (sa > 0.0)
                return LEFT;
            if (sa < 0.0)
                return RIGHT;
            if ((a.X*b.X < 0.0) || (a.Y*b.Y < 0.0))
                return BEHIND;
            if (a.Magnitude() < b.Magnitude())
                return BEYOND;
            if (p0.Equals(p2))
                return ORIGIN;
            if (p1.Equals(p2))
                return DESTINATION;
            return BETWEEN;
        }

        /// <summary>
        ///     Computes the cross product k = u X v.
        /// </summary>
        /// <param name="v">a vertex</param>
        /// <returns>returns the magnitude of u X v</returns>
        private double CrossProduct(Vertex v)
        {
            return Coordinate.X*v.Y - Coordinate.Y*v.X;
        }

        /// <summary>
        ///     Computes the inner or dot product
        /// </summary>
        /// <param name="v">A vertex</param>
        /// <returns>The dot product u.v</returns>
        private double Dot(Vertex v)
        {
            return Coordinate.X*v.X + Coordinate.Y*v.Y;
        }

        /// <summary>
        ///     Computes the scalar product c(v)
        /// </summary>
        /// <param name="c">A vertex</param>
        /// <returns>The scaled vector</returns>
        private Vertex Times(double c)
        {
            return new Vertex(c*Coordinate.X, c*Coordinate.Y);
        }

        /* Vector addition */

        private Vertex Sum(Vertex v)
        {
            return new Vertex(Coordinate.X + v.X, Coordinate.Y + v.Y);
        }

        /* and subtraction */

        private Vertex Sub(Vertex v)
        {
            return new Vertex(Coordinate.X - v.X, Coordinate.Y - v.Y);
        }

        /* magnitude of vector */

        private double Magnitude()
        {
            return Math.Sqrt(Coordinate.X*Coordinate.X + Coordinate.Y*Coordinate.Y);
        }

        /* returns k X v (cross product). this is a vector perpendicular to v */

        private Vertex Cross()
        {
            return new Vertex(Coordinate.Y, -Coordinate.X);
        }

        /** ************************************************************* */
        /***********************************************************************************************
        * Geometric primitives /
        **********************************************************************************************/

        /// <summary>
        ///     Tests if this is inside the circle defined by the points a, b, c. This test uses simple
        ///     double-precision arithmetic, and thus may not be robust.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>true if this vertex is inside the circumcircle (a, b, c)</returns>
        public bool IsInCircle(Vertex a, Vertex b, Vertex c)
        {
            return TrianglePredicate.IsInCircleRobust(a.Coordinate, b.Coordinate, c.Coordinate, Coordinate);
            // non-robust - best to not use
            //return TrianglePredicate.isInCircle(a.p, b.p, c.p, this.p);
        }

        /// <summary>
        ///     Tests whether the triangle formed by this vertex and two
        ///     other vertices is in CCW orientation.
        /// </summary>
        /// <param name="b">a vertex</param>
        /// <param name="c">a vertex</param>
        /// <returns>true if the triangle is oriented CCW</returns>
        private bool IsCcw(Vertex b, Vertex c)
        {
            /*
            // test code used to check for robustness of triArea 
            boolean isCCW = (b.p.x - p.x) * (c.p.y - p.y) 
                          - (b.p.y - p.y) * (c.p.x - p.x) > 0;
            //boolean isCCW = triArea(this, b, c) > 0;
            boolean isCCWRobust = CGAlgorithms.orientationIndex(p, b.p, c.p) == CGAlgorithms.COUNTERCLOCKWISE; 
            if (isCCWRobust != isCCW)
                System.out.println("CCW failure");
            //
             */

            // is equal to the signed area of the triangle
            return (b.Coordinate.X - Coordinate.X)*(c.Coordinate.Y - Coordinate.Y)
                   - (b.Coordinate.Y - Coordinate.Y)*(c.Coordinate.X - Coordinate.X) > 0;

            // original rolled code
            //boolean isCCW = triArea(this, b, c) > 0;
            //return isCCW;
        }

        internal bool RightOf(QuadEdge e)
        {
            return IsCcw(e.Dest, e.Orig);
        }

        private bool LeftOf(QuadEdge e)
        {
            return IsCcw(e.Orig, e.Dest);
        }

        private static HCoordinate Bisector(Vertex a, Vertex b)
        {
            // returns the perpendicular bisector of the line segment ab
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var l1 = new HCoordinate(a.X + dx/2.0, a.Y + dy/2.0, 1.0);
            var l2 = new HCoordinate(a.X - dy + dx/2.0, a.Y + dx + dy/2.0, 1.0);
            return new HCoordinate(l1, l2);
        }

        private static double Distance(Vertex v1, Vertex v2)
        {
            return Math.Sqrt(Math.Pow(v2.X - v1.X, 2.0)
                             + Math.Pow(v2.Y - v1.Y, 2.0));
        }

        /// <summary>
        ///     Computes the value of the ratio of the circumradius to shortest edge. If smaller than some
        ///     given tolerance B, the associated triangle is considered skinny. For an equal lateral
        ///     triangle this value is 0.57735. The ratio is related to the minimum triangle angle theta by:
        ///     circumRadius/shortestEdge = 1/(2sin(theta)).
        /// </summary>
        /// <param name="b">second vertex of the triangle</param>
        /// <param name="c">third vertex of the triangle</param>
        /// <returns>ratio of circumradius to shortest edge.</returns>
        public double CircumRadiusRatio(Vertex b, Vertex c)
        {
            var x = CircleCenter(b, c);
            var radius = Distance(x, b);
            var edgeLength = Distance(this, b);
            var el = Distance(b, c);
            if (el < edgeLength)
                edgeLength = el;
            el = Distance(c, this);
            if (el < edgeLength)
                edgeLength = el;
            return radius/edgeLength;
        }

        /// <summary>
        ///     returns a new vertex that is mid-way between this vertex and another end point.
        /// </summary>
        /// <param name="a">the other end point.</param>
        /// <returns>the point mid-way between this and that.</returns>
        public Vertex MidPoint(Vertex a)
        {
            var xm = (Coordinate.X + a.X)/2.0;
            var ym = (Coordinate.Y + a.Y)/2.0;
            var zm = (Coordinate.Z + a.Z)/2.0;
            return new Vertex(xm, ym, zm);
        }

        /// <summary>
        ///     Computes the centre of the circumcircle of this vertex and two others.
        /// </summary>
        /// <param name="b" />
        /// <param name="c" />
        /// <returns>the Coordinate which is the circumcircle of the 3 points.</returns>
        public Vertex CircleCenter(Vertex b, Vertex c)
        {
            var a = new Vertex(X, Y);
            // compute the perpendicular bisector of cord ab
            var cab = Bisector(a, b);
            // compute the perpendicular bisector of cord bc
            var cbc = Bisector(b, c);
            // compute the intersection of the bisectors (circle radii)
            var hcc = new HCoordinate(cab, cbc);
            Vertex cc = null;
            try
            {
                cc = new Vertex(hcc.GetX(), hcc.GetY());
            }
            catch (NotRepresentableException nre)
            {
#if !PCL
                Debug.WriteLine("a: " + a + "  b: " + b + "  c: " + c);
                Debug.WriteLine(nre);
#endif
                //throw;
            }
            return cc;
        }

        /// <summary>
        ///     For this vertex enclosed in a triangle defined by three vertices v0, v1 and v2, interpolate
        ///     a z value from the surrounding vertices.
        /// </summary>
        public double InterpolateZValue(Vertex v0, Vertex v1, Vertex v2)
        {
            var x0 = v0.X;
            var y0 = v0.Y;
            var a = v1.X - x0;
            var b = v2.X - x0;
            var c = v1.Y - y0;
            var d = v2.Y - y0;
            var det = a*d - b*c;
            var dx = X - x0;
            var dy = Y - y0;
            var t = (d*dx - b*dy)/det;
            var u = (-c*dx + a*dy)/det;
            var z = v0.Z + t*(v1.Z - v0.Z) + u*(v2.Z - v0.Z);
            return z;
        }

        /// <summary>
        ///     Interpolates the Z-value (height) of a point enclosed in a triangle
        ///     whose vertices all have Z values.
        ///     The containing triangle must not be degenerate
        ///     (in other words, the three vertices must enclose a
        ///     non-zero area).
        /// </summary>
        /// <param name="p">The point to interpolate the Z value of</param>
        /// <param name="v0">A vertex of a triangle containing the <paramref name="p" /></param>
        /// <param name="v1">A vertex of a triangle containing the <paramref name="p" /></param>
        /// <param name="v2">A vertex of a triangle containing the <paramref name="p" /></param>
        /// <returns>The interpolated Z-value (height) of the point</returns>
        public static double InterpolateZ(Coordinate p, Coordinate v0, Coordinate v1, Coordinate v2)
        {
            var x0 = v0.X;
            var y0 = v0.Y;
            var a = v1.X - x0;
            var b = v2.X - x0;
            var c = v1.Y - y0;
            var d = v2.Y - y0;
            var det = a*d - b*c;
            var dx = p.X - x0;
            var dy = p.Y - y0;
            var t = (d*dx - b*dy)/det;
            var u = (-c*dx + a*dy)/det;
            var z = v0.Z + t*(v1.Z - v0.Z) + u*(v2.Z - v0.Z);
            return z;
        }

        /// <summary>
        ///     Computes the interpolated Z-value for a point p lying on the segment p0-p1
        /// </summary>
        /// <param name="p">The point to interpolate the Z value of</param>
        /// <param name="p0">A vertex of the segment <paramref name="p" /> is lying on</param>
        /// <param name="p1">A vertex of the segment <paramref name="p" /> is lying on</param>
        /// <returns>The interpolated Z-value (height) of the point</returns>
        public static double InterpolateZ(Coordinate p, Coordinate p0, Coordinate p1)
        {
            var segLen = p0.Distance(p1);
            var ptLen = p.Distance(p0);
            var dz = p1.Z - p0.Z;
            var pz = p0.Z + dz*(ptLen/segLen);
            return pz;
        }
    }
}