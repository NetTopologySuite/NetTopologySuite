using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// Models a site (node) in a <see cref="QuadEdgeSubdivision"/>. 
    /// The sites can be points on a lineString representing a
    /// linear site. 
    /// The vertex can be considered as a vector with a norm, length, inner product, cross
    /// product, etc. Additionally, point relations (e.g., is a point to the left of a line, the circle
    /// defined by this point and two others, etc.) are also defined in this class.
    /// </summary>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class Vertex
    {
        private int LEFT = 0;
        private int RIGHT = 1;
        private int BEYOND = 2;
        private int BEHIND = 3;
        private int BETWEEN = 4;
        private int ORIGIN = 5;
        private int DESTINATION = 6;

        private readonly Coordinate _p;
        // private int edgeNumber = -1;

        public Vertex(double x, double y)
        {
            _p = new Coordinate(x, y);
        }

        public Vertex(double x, double y, double z)
        {
            _p = new Coordinate(x, y, z);
        }

        public Vertex(Coordinate p)
        {
            _p = new Coordinate(p);
        }

        public double X
        {
            get { return _p.X; }
        }

        public double Y
        {
            get { return _p.Y; }
        }

        public double Z
        {
            get { return _p.Z; }
            set { _p.Z = value; }
        }

        public Coordinate Coordinate
        {
            get { return _p; }
        }

        public override String ToString()
        {
            return "POINT (" + _p.X + " " + _p.Y + ")";
        }

        public bool Equals(Vertex x)
        {
            if (_p.X == x.X && _p.Y == x.Y)
            {
                return true;
            }
            return false;
        }

        public bool Equals(Vertex x, double tolerance)
        {
            if (_p.Distance(x.Coordinate) < tolerance)
            {
                return true;
            }
            return false;
        }

        public int Classify(Vertex p0, Vertex p1)
        {
            Vertex p2 = this;
            Vertex a = p1.Sub(p0);
            Vertex b = p2.Sub(p0);
            double sa = a.CrossProduct(b);
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
        /// Computes the cross product k = u X v.
        /// </summary>
        /// <param name="v">a vertex</param>
        /// <returns>returns the magnitude of u X v</returns>
        private double CrossProduct(Vertex v)
        {
            return (_p.X*v.Y - _p.Y*v.X);
        }

        /// <summary>
        /// Computes the inner or dot product
        /// </summary>
        /// <param name="v">a vertex</param>
        /// <returns>the dot product u.v</returns>
        private double Dot(Vertex v)
        {
            return (_p.X*v.X + _p.Y*v.Y);
        }

        /// <summary>
        /// Computes the scalar product c(v)
        /// </summary>
        /// <param name="c">a vertex</param>
        /// <returns>the scaled vector</returns>
        private Vertex Times(double c)
        {
            return (new Vertex(c*_p.X, c*_p.Y));
        }

        /* Vector addition */

        private Vertex Sum(Vertex v)
        {
            return (new Vertex(_p.X + v.X, _p.Y + v.Y));
        }

        /* and subtraction */

        private Vertex Sub(Vertex v)
        {
            return (new Vertex(_p.X - v.X, _p.Y - v.Y));
        }

        /* magnitude of vector */

        private double Magnitude()
        {
            return (Math.Sqrt(_p.X*_p.X + _p.Y*_p.Y));
        }

        /* returns k X v (cross product). this is a vector perpendicular to v */

        private Vertex Cross()
        {
            return (new Vertex(_p.Y, -_p.X));
        }

        /** ************************************************************* */
        /***********************************************************************************************
        * Geometric primitives /
        **********************************************************************************************/

        /// <summary>
        /// Computes twice the area of the oriented triangle (a, b, c), i.e., the area is positive if the
        /// triangle is oriented counterclockwise.
        /// </summary>
        private static double TriArea(Vertex a, Vertex b, Vertex c)
        {
            return (b._p.X - a._p.X)*(c._p.Y - a._p.Y)
                   - (b._p.Y - a._p.Y)*(c._p.X - a._p.X);
        }

        /// <summary>
        /// Tests if this is inside the circle defined by the points a, b, c. This test uses simple
        /// double-precision arithmetic, and thus may not be robust.
        /// </summary>
        /// <param name="a" />
        /// <param name="b" />
        /// <param name="c" />
        /// <returns>true if this point is inside the circle defined by the points a, b, c</returns>
        internal bool InCircle(Vertex a, Vertex b, Vertex c)
        {
            Vertex d = this;
            bool isInCircle =
                (a._p.X*a._p.X + a._p.Y*a._p.Y)*TriArea(b, c, d)
                - (b._p.X*b._p.X + b._p.Y*b._p.Y)*TriArea(a, c, d)
                + (c._p.X*c._p.X + c._p.Y*c._p.Y)*TriArea(a, b, d)
                - (d._p.X*d._p.X + d._p.Y*d._p.Y)*TriArea(a, b, c)
                > 0;
            return isInCircle;
        }

/*
  public boolean OLDinCircle(Vertex a, Vertex b, Vertex c) {
      Vertex d = this;
      boolean isInCircle = (a.getX() * a.getX() + a.getY() * a.getY()) * triArea(b, c, d)
              - (b.getX() * b.getX() + b.getY() * b.getY()) * triArea(a, c, d)
              + (c.getX() * c.getX() + c.getY() * c.getY()) * triArea(a, b, d)
              - (d.getX() * d.getX() + d.getY() * d.getY()) * triArea(a, b, c) 
              > 0;

      // boolean isInCircleRobust = checkRobustInCircle(a.p, b.p, c.p, p, isInCircle);

      // if (! isInCircle)
      // System.out.println(WKTWriter.toLineString(new CoordinateArraySequence(new Coordinate[] {
      // a.p, b.p, c.p, p })));

      return isInCircle;
  }
*/

        /// <summary>
        /// Tests whether the triangle formed by this vertex and two
        /// other vertices is in CCW orientation.
        /// </summary>
        /// <param name="b">a vertex</param>
        /// <param name="c">a vertex</param>
        /// <returns>true if the triangle is oriented CCW</returns>
        private bool IsCcw(Vertex b, Vertex c)
        {
            // is equal to the signed area of the triangle

            return (b._p.X - _p.X)*(c._p.Y - _p.Y)
                   - (b._p.Y - _p.Y)*(c._p.X - _p.X) > 0;

            // original rolled code
            //boolean isCCW = triArea(this, b, c) > 0;
            //return isCCW;

            /*
         // MD - used to check for robustness of triArea 
        boolean isCCW = triArea(this, b, c) > 0;
        boolean isCCWRobust = CGAlgorithms.orientationIndex(p, b.p, c.p) == CGAlgorithms.COUNTERCLOCKWISE; 
        if (isCCWRobust != isCCW)
        	System.out.println("CCW failure");
        return isCCW;
        //*/
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
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            HCoordinate l1 = new HCoordinate(a.X + dx/2.0, a.Y + dy/2.0, 1.0);
            HCoordinate l2 = new HCoordinate(a.X - dy + dx/2.0, a.Y + dx + dy/2.0, 1.0);
            return new HCoordinate(l1, l2);
        }

        private double Distance(Vertex v1, Vertex v2)
        {
            return Math.Sqrt(Math.Pow(v2.X - v1.X, 2.0)
                             + Math.Pow(v2.Y - v1.Y, 2.0));
        }

        /// <summary>
        /// Computes the value of the ratio of the circumradius to shortest edge. If smaller than some
        /// given tolerance B, the associated triangle is considered skinny. For an equal lateral
        /// triangle this value is 0.57735. The ratio is related to the minimum triangle angle theta by:
        /// circumRadius/shortestEdge = 1/(2sin(theta)).
        /// </summary>
        /// <param name="b">second vertex of the triangle</param>
        /// <param name="c">third vertex of the triangle</param>
        /// <returns>ratio of circumradius to shortest edge.</returns>
        public double CircumRadiusRatio(Vertex b, Vertex c)
        {
            Vertex x = this.CircleCenter(b, c);
            double radius = Distance(x, b);
            double edgeLength = Distance(this, b);
            double el = Distance(b, c);
            if (el < edgeLength)
            {
                edgeLength = el;
            }
            el = Distance(c, this);
            if (el < edgeLength)
            {
                edgeLength = el;
            }
            return radius/edgeLength;
        }

        /// <summary>
        /// returns a new vertex that is mid-way between this vertex and another end point.
        /// </summary>
        /// <param name="a">the other end point.</param>
        /// <returns>the point mid-way between this and that.</returns>
        public Vertex MidPoint(Vertex a)
        {
            double xm = (_p.X + a.X)/2.0;
            double ym = (_p.Y + a.Y)/2.0;
            double zm = (_p.Z + a.Z)/2.0;
            return new Vertex(xm, ym, zm);
        }

        /// <summary>
        /// Computes the centre of the circumcircle of this vertex and two others.
        /// </summary>
        /// <param name="b" />
        /// <param name="c" />
        /// <returns>the Coordinate which is the circumcircle of the 3 points.</returns>
        public Vertex CircleCenter(Vertex b, Vertex c)
        {
            Vertex a = new Vertex(this.X, this.Y);
            // compute the perpendicular bisector of cord ab
            HCoordinate cab = Bisector(a, b);
            // compute the perpendicular bisector of cord bc
            HCoordinate cbc = Bisector(b, c);
            // compute the intersection of the bisectors (circle radii)
            HCoordinate hcc = new HCoordinate(cab, cbc);
            Vertex cc = null;
            try
            {
                cc = new Vertex(hcc.GetX(), hcc.GetY());
            }
            catch (NotRepresentableException nre)
            {
                System.Console.WriteLine("a: " + a + "  b: " + b + "  c: " + c);
                System.Console.WriteLine(nre);
            }
            return cc;
        }

        /// <summary>
        /// For this vertex enclosed in a triangle defined by three verticies v0, v1 and v2, interpolate
        /// a z value from the surrounding vertices.
        /// </summary>
        public double InterpolateZValue(Vertex v0, Vertex v1, Vertex v2)
        {
            double x0 = v0.X;
            double y0 = v0.Y;
            double a = v1.X - x0;
            double b = v2.X - x0;
            double c = v1.Y - y0;
            double d = v2.Y - y0;
            double det = a*d - b*c;
            double dx = this.X - x0;
            double dy = this.Y - y0;
            double t = (d*dx - b*dy)/det;
            double u = (-c*dx + a*dy)/det;
            double z = v0.Z + t*(v1.Z - v0.Z) + u*(v2.Z - v0.Z);
            return z;
        }

        /// <summary>
        /// Interpolates the Z value of a point enclosed in a 3D triangle.
        /// </summary>
        public static double InterpolateZ(Coordinate p, Coordinate v0, Coordinate v1, Coordinate v2)
        {
            double x0 = v0.X;
            double y0 = v0.Y;
            double a = v1.X - x0;
            double b = v2.X - x0;
            double c = v1.Y - y0;
            double d = v2.Y - y0;
            double det = a*d - b*c;
            double dx = p.X - x0;
            double dy = p.Y - y0;
            double t = (d*dx - b*dy)/det;
            double u = (-c*dx + a*dy)/det;
            double z = v0.Z + t*(v1.Z - v0.Z) + u*(v2.Z - v0.Z);
            return z;
        }

        /// <summary>
        /// Computes the interpolated Z-value for a point p lying on the segment p0-p1
        /// </summary>  
        /// <param name="p" />
        /// <param name="p0" />
        /// <param name="p1" />
        /// <returns />
        public static double InterpolateZ(Coordinate p, Coordinate p0, Coordinate p1)
        {
            double segLen = p0.Distance(p1);
            double ptLen = p.Distance(p0);
            double dz = p1.Z - p0.Z;
            double pz = p0.Z + dz*(ptLen/segLen);
            return pz;
        }

        // /**
        // * Checks if the computed value for isInCircle is correct, using double-double precision
        // * arithmetic.
        // *
        // * @param a
        // * @param b
        // * @param c
        // * @param p
        // * @param nonRobustInCircle
        // * @return the robust value
        // */
        // private boolean checkRobustInCircle(Coordinate a, Coordinate b, Coordinate c, Coordinate p,
        // boolean nonRobustInCircle) {
        // // *
        // boolean isInCircleDD = inCircleDD(a, b, c, p);
        // boolean isInCircleCC = inCircleCC(a, b, c, p);
        //
        // Coordinate circumCentre = Triangle.circumcentre(a, b, c);
        // System.out.println("p radius diff a = "
        // + (p.distance(circumCentre) - a.distance(circumCentre)) / a.distance(circumCentre));
        //
        // if (nonRobustInCircle != isInCircleDD || nonRobustInCircle != isInCircleCC) {
        // System.out.println("inCircle robustness failure (double result = " + nonRobustInCircle
        // + ", DD result = " + isInCircleDD + ", CC result = " + isInCircleCC + ")");
        // System.out.println(WKTWriter.toLineString(new CoordinateArraySequence(new Coordinate[]{
        // a,
        // b,
        // c,
        // p})));
        // System.out.println("Circumcentre = " + WKTWriter.toPoint(circumCentre)
        // + " radius = " + a.distance(circumCentre));
        // System.out.println("p radius diff a = "
        // + (p.distance(circumCentre) - a.distance(circumCentre)));
        // System.out.println("p radius diff b = "
        // + (p.distance(circumCentre) - b.distance(circumCentre)));
        // System.out.println("p radius diff c = "
        // + (p.distance(circumCentre) - c.distance(circumCentre)));
        // }
        // return isInCircleDD;
        // }

        // /**
        // * Computes the inCircle test using the circumcentre. In general this doesn't appear to be any
        // * more robust than the standard calculation. However, there is at least one case where the
        // test
        // * point is far enough from the circumcircle that this test gives the correct answer.
        // LINESTRING
        // * (1507029.9878 518325.7547, 1507022.1120341457 518332.8225183258, 1507029.9833 518325.7458,
        // * 1507029.9896965567 518325.744909031)
        // *
        // * @param a
        // * @param b
        // * @param c
        // * @param p
        // * @return
        // */
        // private static boolean inCircleCC(Coordinate a, Coordinate b, Coordinate c, Coordinate p) {
        // Coordinate cc = Triangle.circumcentre(a, b, c);
        // double ccRadius = a.distance(cc);
        // double pRadiusDiff = p.distance(cc) - ccRadius;
        // return pRadiusDiff <= 0;
        // }
        //
        // private static boolean inCircleDD(Coordinate a, Coordinate b, Coordinate c, Coordinate p) {
        // DoubleDouble px = new DoubleDouble(p.x);
        // DoubleDouble py = new DoubleDouble(p.y);
        // DoubleDouble ax = new DoubleDouble(a.x);
        // DoubleDouble ay = new DoubleDouble(a.y);
        // DoubleDouble bx = new DoubleDouble(b.x);
        // DoubleDouble by = new DoubleDouble(b.y);
        // DoubleDouble cx = new DoubleDouble(c.x);
        // DoubleDouble cy = new DoubleDouble(c.y);
        //
        // DoubleDouble aTerm = (ax.multiply(ax).add(ay.multiply(ay))).multiply(triAreaDD(
        // bx,
        // by,
        // cx,
        // cy,
        // px,
        // py));
        // DoubleDouble bTerm = (bx.multiply(bx).add(by.multiply(by))).multiply(triAreaDD(
        // ax,
        // ay,
        // cx,
        // cy,
        // px,
        // py));
        // DoubleDouble cTerm = (cx.multiply(cx).add(cy.multiply(cy))).multiply(triAreaDD(
        // ax,
        // ay,
        // bx,
        // by,
        // px,
        // py));
        // DoubleDouble pTerm = (px.multiply(px).add(py.multiply(py))).multiply(triAreaDD(
        // ax,
        // ay,
        // bx,
        // by,
        // cx,
        // cy));
        //
        // DoubleDouble sum = aTerm.subtract(bTerm).add(cTerm).subtract(pTerm);
        // boolean isInCircle = sum.doubleValue() > 0;
        //
        // return isInCircle;
        // }

        // /**
        // * Computes twice the area of the oriented triangle (a, b, c), i.e., the area is positive if
        // the
        // * triangle is oriented counterclockwise.
        // */
        // private static DoubleDouble triAreaDD(DoubleDouble ax, DoubleDouble ay, DoubleDouble bx,
        // DoubleDouble by, DoubleDouble cx, DoubleDouble cy) {
        // return (bx.subtract(ax).multiply(cy.subtract(ay)).subtract(by.subtract(ay).multiply(
        // cx.subtract(ax))));
        // }
    }
}