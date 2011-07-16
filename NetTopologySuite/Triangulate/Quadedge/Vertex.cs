/*
 * The JTS Topology Suite is a collection of Java classes that
 * implement the fundamental operations required to validate a given
 * geo-spatial data set to a known topological specification.
 *
 * Copyright (C) 2001 Vivid Solutions
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * For more information, contact:
 *
 *     Vivid Solutions
 *     Suite #1A
 *     2328 Government Street
 *     Victoria BC  V8T 5G5
 *     Canada
 *
 *     (250)385-6040
 *     www.vividsolutions.com
 */


using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Algorithms;
using NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate.Quadedge
    {
    
        public enum VertexClassification
        {
            Left = 0,
            Right,
            Beyond,
            Behind,
            Between,
            Origin,
            Destination
        }

    ///<summary>
        /// Models a site (node) in a <see cref="QuadEdgeSubdivision{TCoordinate}"/>.
        /// The sites can be points on a lineString representing a linear site. 
        /// The Vertex<TCoordinate> can be considered as a vector with a norm, length, inner product, cross
        /// product, etc. Additionally, point relations (e.g., is a point to the left of a line, the circle
        /// defined by this point and two others, etc.) are also defined in this class.
        ///</summary>
        ///<typeparam name="TCoordinate"></typeparam>
        /// <typeparam name="TData"></typeparam>
        public class Vertex<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
        {

            private TCoordinate      _p;
            private readonly ICoordinateFactory<TCoordinate> _coordFactory;
            // private int edgeNumber = -1;

    ///<summary>
    ///</summary>
    ///<param name="factory"></param>
    ///<param name="x"></param>
    ///<param name="y"></param>
    public Vertex(ICoordinateFactory<TCoordinate> factory, Double x, Double y) 
    {
        _coordFactory = factory;
        _p = factory.Create(x, y);
    }

    ///<summary>
    ///</summary>
    ///<param name="factory"></param>
    ///<param name="x"></param>
    ///<param name="y"></param>
    ///<param name="z"></param>
    public Vertex(ICoordinateFactory<TCoordinate> factory,Double x, Double y, Double z)
    {
        _coordFactory = factory;
        _p = factory.Create3D(x, y, z);
    }

    ///<summary>
    ///</summary>
    ///<param name="p"></param>
    public Vertex(TCoordinate p) {
        _coordFactory = (ICoordinateFactory<TCoordinate>)p.Factory;
        _p = _coordFactory.Create(p);
    }

    ///<summary>
    ///</summary>
    public Double X
    {
        get { return _p[Ordinates.X]; }
    }

    ///<summary>
    ///</summary>
    public Double Y 
    {
        get { return _p[Ordinates.Y]; }
    }

    ///<summary>
    ///</summary>
    public Double Z
    {
        get { return _p[Ordinates.Z]; }
        set

            {
                _p = _coordFactory.Create3D(X, Y, value);
            }
    }

    ///<summary>
    ///</summary>
    public TCoordinate Coordinate
    {
        get { return _p; }
    }

    public override String ToString() {
        return _p.ToString(); //"POINT (" + X + " " + Y + ")";
    }

    public bool Equals2D(TCoordinate x)
    {
        return _p[Ordinates.X] == x[Ordinates.X] &&
               _p[Ordinates.Y] == x[Ordinates.Y];
    }
    ///<summary>
    ///</summary>
    ///<param name="x"></param>
    ///<returns></returns>
    public Boolean Equals2D(Vertex<TCoordinate> x) {

        return _p[Ordinates.X] == x.X && _p[Ordinates.Y] == x.Y;
    }

    ///<summary>
    ///</summary>
    ///<param name="x"></param>
    ///<param name="tolerance"></param>
    ///<returns></returns>
    public Boolean Equals(Vertex<TCoordinate> x, Double tolerance) 
    {
        return _p.Distance(x.Coordinate) < tolerance;
    }

    ///<summary>
    ///</summary>
    ///<param name="p0"></param>
    ///<param name="p1"></param>
    ///<returns></returns>
    public VertexClassification Classify(Vertex<TCoordinate> p0, Vertex<TCoordinate> p1)
    {
        Vertex<TCoordinate> p2 = this;
        Vertex<TCoordinate> a = p1.Sub(p0);
        Vertex<TCoordinate> b = p2.Sub(p0);
        Double sa = a.CrossProduct(b);
        if (sa > 0.0)
            return VertexClassification.Left;
        if (sa < 0.0)
            return VertexClassification.Right;
        if ((a.X * b.X < 0.0) || (a.Y * b.Y < 0.0))
            return VertexClassification.Behind;
        if (a.Magn() < b.Magn())
            return VertexClassification.Beyond;
        if (p0.Equals(p2))
            return VertexClassification.Origin;
        if (p1.Equals(p2))
            return VertexClassification.Destination;
        return VertexClassification.Between;
    }

    /**
     * Computes the cross product k = u X v.
     * 
     * @param v a vertex
     * @return returns the magnitude of u X v
     */
    private Double CrossProduct(Vertex<TCoordinate> v) 
    {
        return (X * v.Y - Y * v.X);
    }

    /**
     * Computes the inner or dot product
     * 
     * @param v, a vertex
     * @return returns the dot product u.v
     */
    private Double Dot(Vertex<TCoordinate>v)
    {
        return (X * v.X + Y * v.Y);
    }

    /**
     * Computes the scalar product c(v)
     * 
     * @param v, a vertex
     * @return returns the scaled vector
     */
    Vertex<TCoordinate>Times(double c) {
        return (new Vertex<TCoordinate>(_coordFactory, c * X, c * Y));
    }

    /* Vector addition */
    Vertex<TCoordinate>Sum(Vertex<TCoordinate>v) {
        return (new Vertex<TCoordinate>(_coordFactory, X + v.X, Y + v.Y));
    }

    /* and subtraction */
    Vertex<TCoordinate>Sub(Vertex<TCoordinate>v) {
        return (new Vertex<TCoordinate>(_coordFactory,X - v.X, Y - v.Y));
    }

    /* magnitude of vector */
    double Magn() {
        return (Math.Sqrt(X * X + Y * Y));
    }

    /* returns k X v (cross product). this is a vector perpendicular to v */
    Vertex<TCoordinate>Cross() {
        return (new Vertex<TCoordinate>(_coordFactory, Y, -X));
    }

    /** ************************************************************* */
    /***********************************************************************************************
     * Geometric primitives /
     **********************************************************************************************/

    /**
     * Computes twice the area of the oriented triangle (a, b, c), i.e., the area is positive if the
     * triangle is oriented counterclockwise.
     */
    private static Double triArea(Vertex<TCoordinate> a, Vertex<TCoordinate>b, Vertex<TCoordinate>c) {
        return (b.X - a.X) * (c.Y - a.Y) 
             - (b.Y - a.Y) * (c.X - a.X);
    }

    /**
     * Tests if this is inside the circle defined by the points a, b, c. This test uses simple
     * double-precision arithmetic, and thus may not be robust.
     * 
     * @param a
     * @param b
     * @param c
     * @return true if this point is inside the circle defined by the points a, b, c
     */
    public Boolean InCircle(Vertex<TCoordinate>a, Vertex<TCoordinate>b, Vertex<TCoordinate>c) {
      Vertex<TCoordinate>d = this;
      Boolean isInCircle = 
      	        (a.X * a.X + a.Y * a.Y) * triArea(b, c, d)
              - (b.X * b.X + b.Y * b.Y) * triArea(a, c, d)
              + (c.X * c.X + c.Y * c.Y) * triArea(a, b, d)
              - (d.X * d.X + d.Y * d.Y) * triArea(a, b, c) 
              > 0;
      return isInCircle;
    }
    
/*
  public boolean OLDinCircle(Vertex<TCoordinate>a, Vertex<TCoordinate>b, Vertex<TCoordinate>c) {
      Vertex<TCoordinate>d = this;
      boolean isInCircle = (a.X * a.X + a.Y * a.Y) * triArea(b, c, d)
              - (b.X * b.X + b.Y * b.Y) * triArea(a, c, d)
              + (c.X * c.X + c.Y * c.Y) * triArea(a, b, d)
              - (d.X * d.X + d.Y * d.Y) * triArea(a, b, c) 
              > 0;

      // boolean isInCircleRobust = checkRobustInCircle(a.p, b.p, c.p, p, isInCircle);

      // if (! isInCircle)
      // System.out.println(WKTWriter.toLineString(new CoordinateArraySequence(new Coordinate[] {
      // a.p, b.p, c.p, p })));

      return isInCircle;
  }
*/

    ///<summary>
    /// Tests whether the triangle formed by this <see cref="Vertex{TCoordinate}"/> and two
    ///</summary>
    ///<param name="b">second <see cref="Vertex{TCoordinate}"/> of the triangle</param>
    ///<param name="c">third <see cref="Vertex{TCoordinate}"/> of the triangle</param>
    ///<returns>true if the triangle is oriented CCW</returns>
    public Boolean IsCCW(Vertex<TCoordinate>b, Vertex<TCoordinate>c) 
    {
    	// is equal to the signed area of the triangle
        return (b.X - X)*(c.Y - Y)
               - (b.Y - Y)*(c.X - X) > 0;
      
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

    ///<summary>
    ///</summary>
    ///<param name="e"></param>
    ///<returns></returns>
    public Boolean RightOf(QuadEdge<TCoordinate> e) {
        return IsCCW(e.Destination, e.Origin);
    }

    ///<summary>
    ///</summary>
    ///<param name="e"></param>
    ///<returns></returns>
    public Boolean LeftOf(QuadEdge<TCoordinate> e) {
        return IsCCW(e.Origin, e.Destination);
    }

    private TCoordinate Bisector(Vertex<TCoordinate>a, Vertex<TCoordinate>b) {
        // returns the perpendicular bisector of the line segment ab
        Double dx = b.X - a.X;
        Double dy = b.Y - a.Y;
        TCoordinate l1 = _coordFactory.Homogenize(_coordFactory.Create(a.X + dx / 2.0, a.Y + dy / 2.0));//new HCoordinate(a.X + dx / 2.0, a.Y + dy / 2.0, 1.0);
        TCoordinate l2 = _coordFactory.Homogenize(_coordFactory.Create(a.X - dy + dx / 2.0, a.Y + dx + dy / 2.0)); //new HCoordinate(a.X - dy + dx / 2.0, a.Y + dx + dy / 2.0, 1.0);
        return l1.Cross(l2);//new HCoordinate(l1, l2);
    }

    private static double Distance(Vertex<TCoordinate>v1, Vertex<TCoordinate>v2) {
        return Math.Sqrt(Math.Pow(v2.X - v1.X, 2.0)
                + Math.Pow(v2.Y - v1.Y, 2.0));
    }

    ///<summary>
    /// Computes the value of the ratio of the circumradius to shortest edge. If smaller than some
    /// given tolerance B, the associated triangle is considered skinny. For an equal lateral
    /// triangle this value is 0.57735. The ratio is related to the minimum triangle angle theta by:
    /// <code>circumRadius/shortestEdge = 1/(2sin(theta))</code>.
    ///</summary>
    ///<param name="b">second <see cref="Vertex{TCoordinate}"/> of the triangle</param>
    ///<param name="c">third <see cref="Vertex{TCoordinate}"/> of the triangle</param>
    ///<returns>ratio of circumradius to shortest edge.</returns>
    public double CircumRadiusRatio(Vertex<TCoordinate>b, Vertex<TCoordinate>c)
    {
        Vertex<TCoordinate>x = CircleCenter(b, c);
        double radius = Distance(x, b);
        double edgeLength = Distance(this, b);
        double el = Distance(b, c);
        if (el < edgeLength) {
            edgeLength = el;
        }
        el = Distance(c, this);
        if (el < edgeLength) {
            edgeLength = el;
        }
        return radius / edgeLength;
    }

    ///<summary> returns a new Vertex<TCoordinate>that is mid-way between this Vertex<TCoordinate>and another end point.
    ///</summary>
    ///<param name="a">the other end point</param>
    ///<returns>the point mid-way between this and that.</returns>
    public Vertex<TCoordinate> MidPoint(Vertex<TCoordinate> a)
    {
        Double xm = (X + a.X) / 2.0;
        Double ym = (Y + a.Y) / 2.0;
        Double zm = (Z + a.Z) / 2.0;
        return new Vertex<TCoordinate>(_coordFactory, xm, ym, zm);
    }

    ///<summary>
    /// Computes the centre of the circumcircle of this <see cref="Vertex{TCoordinate}"/> and two others.
    ///</summary>
    ///<param name="b">second <see cref="Vertex{TCoordinate}"/> of the triangle</param>
    ///<param name="c">third <see cref="Vertex{TCoordinate}"/> of the triangle</param>
    ///<returns>the Coordinate which is the circumcircle of the 3 points.</returns>
    public Vertex<TCoordinate> CircleCenter(Vertex<TCoordinate> b, Vertex<TCoordinate> c)
    {
        Vertex<TCoordinate> a = new Vertex<TCoordinate>(_coordFactory, X, Y);
        // compute the perpendicular bisector of cord ab
        TCoordinate cab = Bisector(a, b);
        // compute the perpendicular bisector of cord bc
        TCoordinate cbc = Bisector(b, c);
        // compute the intersection of the bisectors (circle radii)
        TCoordinate hcc = cab.Cross(cbc);// new HCoordinate(cab, cbc);
        Vertex<TCoordinate>cc = null;
        try
        {
            cc = new Vertex<TCoordinate>(_coordFactory, hcc[Ordinates.X], hcc[Ordinates.Y]);
        }
        catch (NotRepresentableException nre)
        {
            Debug.WriteLine(String.Format("a: {0}, b: {1}, c: {2}\n{3}", a, b, c, nre));
        }
        return cc;
    }

    ///<summary>
    /// For this <see cref="Vertex{TCoordinate}"/>enclosed in a triangle defined by three verticies v0, v1 and v2, interpolate
    /// a z value from the surrounding vertices.
    ///</summary>
    ///<param name="v0"></param>
    ///<param name="v1"></param>
    ///<param name="v2"></param>
    ///<returns></returns>
    public Double InterpolateZValue(Vertex<TCoordinate>v0, Vertex<TCoordinate>v1, Vertex<TCoordinate>v2) 
    {
        double x0 = v0.X;
        double y0 = v0.Y;
        double a = v1.X - x0;
        double b = v2.X - x0;
        double c = v1.Y - y0;
        double d = v2.Y - y0;
        double det = a * d - b * c;
        double dx = X - x0;
        double dy = Y - y0;
        double t = (d * dx - b * dy) / det;
        double u = (-c * dx + a * dy) / det;
        double z = v0.Z + t * (v1.Z - v0.Z) + u * (v2.Z - v0.Z);
        return z;
    }

    /**
     * 
     */
    ///<summary>
    /// Interpolates the Z value of a point enclosed in a 3D triangle.
    ///</summary>
    ///<param name="p">The point for which z is to be interpolated</param>
    ///<param name="v0">1st Vertex<TCoordinate>of triangle</param>
    ///<param name="v1">2nd Vertex<TCoordinate>of triangle</param>
    ///<param name="v2">3rd Vertex<TCoordinate>of triangle</param>
    ///<returns>z value at point p</returns>
    public static Double InterpolateZ(TCoordinate p, TCoordinate v0, TCoordinate v1, TCoordinate v2) {
        Double x0 = v0[Ordinates.X];
        Double y0 = v0[Ordinates.Y];
        Double a = v1[Ordinates.X] - x0;
        Double b = v2[Ordinates.X] - x0;
        Double c = v1[Ordinates.Y] - y0;
        Double d = v2[Ordinates.Y] - y0;
        Double det = a * d - b * c;
        Double dx = p[Ordinates.X] - x0;
        Double dy = p[Ordinates.Y] - y0;
        Double t = (d * dx - b * dy) / det;
        Double u = (-c * dx + a * dy) / det;
        Double z = v0[Ordinates.Z] + t * (v1[Ordinates.Z] - v0[Ordinates.Z]) + u * (v2[Ordinates.Z] - v0[Ordinates.Z]);
        return z;
    }

    ///<summary>
    /// Computes the interpolated Z-value for a point p lying on the segment p0-p1
    ///</summary>
    ///<param name="p">The point for which z is to be interpolated</param>
    ///<param name="p0">1st Vertex<TCoordinate>of segment</param>
    ///<param name="p1">2nd Vertex<TCoordinate>of segment</param>
    ///<returns>z value at point p</returns>
    public static double InterpolateZ(TCoordinate p, TCoordinate p0, TCoordinate p1) {
        Double segLen = p0.Distance(p1);
        Double ptLen = p.Distance(p0);
        Double dz = p1[Ordinates.Z] - p0[Ordinates.Z];
        Double pz = p0[Ordinates.Z] + dz * (ptLen / segLen);
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
    // DoubleDouble px = new DoubleDouble(X);
    // DoubleDouble py = new DoubleDouble(Y);
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
