using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a planar triangle, and provides methods for calculating various
    /// properties of triangles.
    /// </summary>
    public struct Triangle<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {

        ///<summary>
        /// Tests whether a triangle is acute.
        /// A triangle is acute iff all interior angles are acute.
        /// This is a strict test - right triangles will return <tt>false</tt>
        /// A triangle which is not acute is either right or obtuse.
        /// Note: this implementation is not robust for angles very close to 90 degrees.
        ///</summary>
        ///<param name="a">a vertex of the triangle</param>
        ///<param name="b">a vertex of the triangle</param>
        ///<param name="c">a vertex of the triangle</param>
        ///<returns>true if the triangle is acute</returns>
        public static Boolean IsAcute(TCoordinate a, TCoordinate b, TCoordinate c)
        {
            if (!Angle<TCoordinate>.IsAcute(a, b, c)) return false;
            if (!Angle<TCoordinate>.IsAcute(b, c, a)) return false;
            if (!Angle<TCoordinate>.IsAcute(c, a, b)) return false;
            return true;
        }

        ///**
        // * Computes the line which is the perpendicular bisector of the
        // * line segment a-b.
        // *
        // * @param a a point
        // * @param b another point
        // * @return the perpendicular bisector, as an HCoordinate
        // */
        public static TCoordinate PerpendicularBisector(ICoordinateFactory<TCoordinate> factory, TCoordinate a, TCoordinate b)
        {
            // returns the perpendicular bisector of the line segment ab
            Double dx = b[Ordinates.X] - a[Ordinates.X];
            Double dy = b[Ordinates.Y] - a[Ordinates.Y];
            TCoordinate l1 = factory.Create(a[Ordinates.X] + dx/2.0, a[Ordinates.Y] + dy/2.0);
            TCoordinate l2 = factory.Create(a[Ordinates.X] - dy + dx/2.0, a[Ordinates.Y] + dx + dy/2.0);
            return l1.Cross(l2);
        }

        ///**
        // * Computes the circumcentre of a triangle.
        // * The circumcentre is the centre of the circumcircle,
        // * the smallest circle which encloses the triangle.
        // *
        // * @param a a vertx of the triangle
        // * @param b a vertx of the triangle
        // * @param c a vertx of the triangle
        // * @return the circumcentre of the triangle
        // */
        public static TCoordinate Circumcentre(ICoordinateFactory<TCoordinate> factory, TCoordinate a, TCoordinate b, TCoordinate c)
        {
            // compute the perpendicular bisector of chord ab
            TCoordinate cab = PerpendicularBisector(factory, a, b);
            // compute the perpendicular bisector of chord bc
            TCoordinate cbc = PerpendicularBisector(factory, b, c);
            // compute the intersection of the bisectors (circle radii)
            TCoordinate cc;
            try
            {
                TCoordinate hcc = cab.Cross(cbc);
                cc = factory.Dehomogenize(hcc);
            }
            catch (NotRepresentableException ex)
            {
                // MD - not sure what we can do to prevent this (robustness problem)
                // Idea - can we condition which edges we choose?
                throw new ArgumentException(ex.Message);
            }
            return cc;

        }

        ///<summary>
        /// Computes the incentre of a triangle.
        /// The <i>inCentre</i> of a triangle is the point which is equidistant
        /// from the sides of the triangle.
        /// It is also the point at which the bisectors
        /// of the triangle's angles meet.
        /// It is the centre of the triangle's <i>incircle</i>,
        /// which is the unique circle that is tangent to each of the triangle's three sides.
        ///</summary>
        ///<param name="factory"></param>
        ///<param name="a">a vertx of the triangle</param>
        ///<param name="b">a vertx of the triangle</param>
        ///<param name="c">a vertx of the triangle</param>
        ///<returns>the point which is the incentre of the triangle</returns>
        public static TCoordinate InCentre(ICoordinateFactory<TCoordinate>factory, TCoordinate a, TCoordinate b, TCoordinate c)
        {
            // the lengths of the sides, labelled by their opposite vertex
            Double len0 = b.Distance(c);
            Double len1 = a.Distance(c);
            Double len2 = a.Distance(b);
            Double circum = len0 + len1 + len2;

            Double inCentreX = (len0 * a[Ordinates.X] + len1 * b[Ordinates.X] + len2 * c[Ordinates.X]) / circum;
            Double inCentreY = (len0 * a[Ordinates.Y] + len1 * b[Ordinates.Y] + len2 * c[Ordinates.Y]) / circum;
            return factory.Create(inCentreX, inCentreY);
        }

        /**
         * Computes the centroid (centre of mass) of a triangle.
         * This is also the point at which the triangle's three
         * medians intersect (a triangle median is the segment from a vertex of the triangle to the
         * midpoint of the opposite side).
         * The centroid divides each median in a ratio of 2:1.
         * The centroid always lies within the triangle.
         *
         *
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @return the centroid of the triangle
         */
        public static TCoordinate Centroid(ICoordinateFactory<TCoordinate> factory, TCoordinate a, TCoordinate b, TCoordinate c)
        {
            Double x = (a[Ordinates.X] + b[Ordinates.X] + c[Ordinates.X]) / 3;
            Double y = (a[Ordinates.Y] + b[Ordinates.Y] + c[Ordinates.Y]) / 3;
            return factory.Create(x, y);
        }

        /**
         * Computes the length of the longest side of a triangle
         *
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @return the length of the longest side of the triangle
         */
        public static Double LongestSideLength(TCoordinate a, TCoordinate b, TCoordinate c)
        {
            Double lenAB = a.Distance(b);
            Double lenBC = b.Distance(c);
            Double lenCA = c.Distance(a);
            Double maxLen = lenAB;
            if (lenBC > maxLen)
                maxLen = lenBC;
            if (lenCA > maxLen)
                maxLen = lenCA;
            return maxLen;
        }

        /**
         * Computes the point at which the bisector of the angle ABC
         * cuts the segment AC.
         *
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @return the angle bisector cut point
         */
        public static TCoordinate AngleBisector(ICoordinateFactory<TCoordinate> factory, TCoordinate a, TCoordinate b, TCoordinate c)
        {
            /**
             * Uses the fact that the lengths of the parts of the split segment
             * are proportional to the lengths of the adjacent triangle sides
             */
            Double len0 = b.Distance(a);
            Double len2 = b.Distance(c);
            Double frac = len0 / (len0 + len2);
            Double dx = c[Ordinates.X] - a[Ordinates.X];
            Double dy = c[Ordinates.Y] - a[Ordinates.Y];

            TCoordinate splitPt = factory.Create(a[Ordinates.X] + frac * dx,
                                                 a[Ordinates.Y] + frac * dy);
            return splitPt;
        }

        /**
         * Computes the 2D area of a triangle.
         * The area value is always non-negative.
         *
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @return the area of the triangle
         * 
         * @see signedArea
         */
        public static Double Area(TCoordinate a, TCoordinate b, TCoordinate c)
        {
            return Math.Abs(((c[Ordinates.X] - a[Ordinates.X]) * (b[Ordinates.Y] - a[Ordinates.Y]) - (b[Ordinates.X] - a[Ordinates.X]) * (c[Ordinates.Y] - a[Ordinates.Y])) / 2);
        }

        /**
         * Computes the signed 2D area of a triangle.
         * The area value is positive if the triangle is oriented CW,
         * and negative if it is oriented CCW.
         * <p>
         * The signed area value can be used to determine point orientation, but 
         * the implementation in this method
         * is susceptible to round-off errors.  
         * Use {@link CGAlgorithms#orientationIndex)} for robust orientation
         * calculation.
         *
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @return the signed 2D area of the triangle
         * 
         * @see CGAlgorithms#orientationIndex
         */
        public static Double SignedArea(TCoordinate a, TCoordinate b, TCoordinate c)
        {
            /**
             * Uses the formula 1/2 * | u x v |
             * where
             * 	u,v are the side vectors of the triangle
             *  x is the vector cross-product
             * For 2D vectors, this formual simplifies to the expression below
             */
            return ((c[Ordinates.X] - a[Ordinates.X]) * (b[Ordinates.Y] - a[Ordinates.Y]) - (b[Ordinates.X] - a[Ordinates.X]) * (c[Ordinates.Y] - a[Ordinates.Y])) / 2;
        }

        /**
         * Computes the 3D area of a triangle.
         * The value computed is alway non-negative.
         * 
       * @param a a vertex of the triangle
       * @param b a vertex of the triangle
       * @param c a vertex of the triangle
       * @return the 3D area of the triangle
         */
        public static double Area3D(TCoordinate a, TCoordinate b, TCoordinate c)
        {
            /**
             * Uses the formula 1/2 * | u x v |
             * where
             * 	u,v are the side vectors of the triangle
             *  x is the vector cross-product
             */
            // side vectors u and v
            Double ux = b[Ordinates.X] - a[Ordinates.X];
            Double uy = b[Ordinates.Y] - a[Ordinates.Y];
            Double uz = b[Ordinates.Z] - a[Ordinates.Z];

            Double vx = c[Ordinates.X] - a[Ordinates.X];
            Double vy = c[Ordinates.Y] - a[Ordinates.Y];
            Double vz = c[Ordinates.Z] - a[Ordinates.Z];

            // cross-product = u x v 
            Double crossx = uy * vz - uz * vy;
            Double crossy = uz * vx - ux * vz;
            Double crossz = ux * vy - uy * vx;

            // tri area = 1/2 * | u x v |
            Double absSq = crossx * crossx + crossy * crossy + crossz * crossz;
            Double area3D = Math.Sqrt(absSq) / 2d;

            return area3D;
        }
	
        
        private readonly ICoordinateFactory<TCoordinate> _coordFactory;
        private readonly TCoordinate _p0;
        private readonly TCoordinate _p1;
        private readonly TCoordinate _p2;

        public Triangle(ICoordinateFactory<TCoordinate> coordFactory,
                        TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            _coordFactory = coordFactory;
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
        }

        public TCoordinate P0
        {
            get { return _p0; }
        }

        public TCoordinate P1
        {
            get { return _p1; }
        }

        public TCoordinate P2
        {
            get { return _p2; }
        }

        /// <summary>
        /// The InCenter of a triangle is the point which is equidistant
        /// from the sides of the triangle.  
        /// This is also the point at which the bisectors
        /// of the angles meet.
        /// </summary>
        /// <returns>
        /// The point which is the InCenter of the triangle.
        /// </returns>
        public TCoordinate InCenter
        {
            get
            {
                return InCentre(_coordFactory, P0, P1, P2);
                // the lengths of the sides, labeled by their opposite vertex
                Double len0 = P1.Distance(P2);
                Double len1 = P0.Distance(P2);
                Double len2 = P0.Distance(P1);
                Double circum = len0 + len1 + len2;

                Double inCenterX = (len0*P0[Ordinates.X] +
                                    len1*P1[Ordinates.X] +
                                    len2*P2[Ordinates.X])/circum;

                Double inCenterY = (len0*P0[Ordinates.Y] +
                                    len1*P1[Ordinates.Y] +
                                    len2*P2[Ordinates.Y])/circum;

                return _coordFactory.Create(inCenterX, inCenterY);
            }
        }

    }
}