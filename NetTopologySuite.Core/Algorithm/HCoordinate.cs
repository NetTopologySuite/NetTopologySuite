using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Represents a homogeneous coordinate in a 2-D coordinate space.
    /// In NTS <see cref="HCoordinate"/>s are used as a clean way
    /// of computing intersections between line segments.
    /// </summary>
    /// <author>David Skea</author>
    public class HCoordinate
    {
        ///<summary>
        /// Computes the (approximate) intersection point between two line segments using homogeneous coordinates.
        /// </summary>
        /// <remarks>
        /// Note that this algorithm is
        /// not numerically stable; i.e. it can produce intersection points which
        /// lie outside the envelope of the line segments themselves.  In order
        /// to increase the precision of the calculation input points should be normalized
        /// before passing them to this routine.
        /// </remarks>
        /// <param name="p1">1st Coordinate of 1st linesegment</param>
        /// <param name="p2">2nd Coordinate of 1st linesegment</param>
        /// <param name="q1">1st Coordinate of 2nd linesegment</param>
        /// <param name="q2">2nd Coordinate of 2nd linesegment</param>
        public static Coordinate Intersection(
            Coordinate p1, Coordinate p2,
            Coordinate q1, Coordinate q2)
        {
            // unrolled computation
            double px = p1.Y - p2.Y;
            double py = p2.X - p1.X;
            double pw = p1.X*p2.Y - p2.X*p1.Y;

            double qx = q1.Y - q2.Y;
            double qy = q2.X - q1.X;
            double qw = q1.X*q2.Y - q2.X*q1.Y;

            double x = py*qw - qy*pw;
            double y = qx*pw - px*qw;
            double w = px*qy - qx*py;

            double xInt = x/w;
            double yInt = y/w;

            if ((Double.IsNaN(xInt)) || (Double.IsInfinity(xInt)
                                         || Double.IsNaN(yInt)) || (Double.IsInfinity(yInt)))
            {
                throw new NotRepresentableException();
            }

            return new Coordinate(xInt, yInt);
        }

        /// <summary> 
        /// Computes the (approximate) intersection point between two line segments
        /// using homogeneous coordinates.
        /// Note that this algorithm is
        /// not numerically stable; i.e. it can produce intersection points which
        /// lie outside the envelope of the line segments themselves.  In order
        /// to increase the precision of the calculation input points should be normalized
        /// before passing them to this routine.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Coordinate OldIntersection(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)            
        {
            HCoordinate l1 = new HCoordinate(new HCoordinate(p1), new HCoordinate(p2));
            HCoordinate l2 = new HCoordinate(new HCoordinate(q1), new HCoordinate(q2));
            HCoordinate intHCoord = new HCoordinate(l1, l2);
            Coordinate intPt = intHCoord.Coordinate;
            return intPt;
        }

        private double _x;
        private double _y;
        private double _w;

        /// <summary>
        /// Direct access to x private field
        /// </summary>
        [Obsolete("This is a simple access to x private field: use GetX() instead.")]
        protected double X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        /// Direct access to y private field
        /// </summary>
        [Obsolete("This is a simple access to y private field: use GetY() instead.")]
        protected double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        /// Direct access to w private field
        /// </summary>
        [Obsolete("This is a simple access to w private field: how do you use this field for?...")]
        protected double W
        {
            get { return _w; }
            set { _w = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public HCoordinate()
        {
            _x = 0.0;
            _y = 0.0;
            _w = 1.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        public HCoordinate(double x, double y, double w) 
        {
            _x = x;
            _y = y;
            _w = w;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public HCoordinate(Coordinate p) 
        {
            _x = p.X;
            _y = p.Y;
            _w = 1.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public HCoordinate(HCoordinate p1, HCoordinate p2) 
        {
            _x = p1._y * p2._w - p2._y * p1._w;
            _y = p2._x * p1._w - p1._x * p2._w;
            _w = p1._x * p2._y - p2._x * p1._y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetX()
        {
            double a = _x/_w;
            if ((Double.IsNaN(a)) || (Double.IsInfinity(a))) 
                throw new NotRepresentableException();                
            return a;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetY()
        {            
            double a = _y/_w;
            if ((Double.IsNaN(a)) || (Double.IsInfinity(a))) 
                throw new NotRepresentableException();            
            return a;            
        }        

        /// <summary>
        /// 
        /// </summary>
        public Coordinate Coordinate
        {
            get 
            { 
                return new Coordinate(GetX(), GetY()); 
            }
        }

        ///<summary>
        /// Constructs a homogeneous coordinate which is the intersection of the lines <see cref="Coordinate"/>s.
        /// define by the homogenous coordinates represented by two
        ///</summary>
        /// <param name="p1">A coordinate</param>
        /// <param name="p2">A coordinate</param>
        public HCoordinate(Coordinate p1, Coordinate p2)
        {
            // optimization when it is known that w = 1
            _x = p1.Y - p2.Y;
            _y = p2.X - p1.X;
            _w = p1.X * p2.Y - p2.X * p1.Y;
        }

        public HCoordinate(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            // unrolled computation
            double px = p1.Y - p2.Y;
            double py = p2.X - p1.X;
            double pw = p1.X * p2.Y - p2.X * p1.Y;

            double qx = q1.Y - q2.Y;
            double qy = q2.X - q1.X;
            double qw = q1.X * q2.Y - q2.X * q1.Y;

            _x = py * qw - qy * pw;
            _y = qx * pw - px * qw;
            _w = px * qy - qx * py;
        }

    }
}
