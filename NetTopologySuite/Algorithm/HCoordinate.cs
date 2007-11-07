using System;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Represents a homogeneous coordinate for 2-D coordinates.
    /// </summary>
    public class HCoordinate
    {
        /// <summary> 
        /// Computes the (approximate) intersection point between two line segments
        /// using homogeneous coordinates.
        /// Note that this algorithm is
        /// not numerically stable; i.e. it can produce intersection points which
        /// lie outside the envelope of the line segments themselves.  In order
        /// to increase the precision of the calculation input points should be normalized
        /// before passing them to this routine.
        /// </summary>
        public static ICoordinate Intersection(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2)
        {
            HCoordinate l1 = new HCoordinate(new HCoordinate(p1), new HCoordinate(p2));
            HCoordinate l2 = new HCoordinate(new HCoordinate(q1), new HCoordinate(q2));
            HCoordinate intHCoord = new HCoordinate(l1, l2);
            ICoordinate intPt = intHCoord.Coordinate;
            return intPt;
        }

        private Double x;
        private Double y;
        private Double w;

        /// <summary>
        /// Direct access to x private field
        /// </summary>
        [Obsolete("This is a simple access to x private field: use GetX() instead.")]
        protected Double X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Direct access to y private field
        /// </summary>
        [Obsolete("This is a simple access to y private field: use GetY() instead.")]
        protected Double Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// Direct access to w private field
        /// </summary>
        [Obsolete("This is a simple access to w private field: how do you use this field for?...")]
        protected Double W
        {
            get { return w; }
            set { w = value; }
        }

        public HCoordinate()
        {
            x = 0.0;
            y = 0.0;
            w = 1.0;
        }

        public HCoordinate(Double x, Double y, Double w)
        {
            this.x = x;
            this.y = y;
            this.w = w;
        }

        public HCoordinate(ICoordinate p)
        {
            x = p.X;
            y = p.Y;
            w = 1.0;
        }

        public HCoordinate(HCoordinate p1, HCoordinate p2)
        {
            x = p1.y * p2.w - p2.y * p1.w;
            y = p2.x * p1.w - p1.x * p2.w;
            w = p1.x * p2.y - p2.x * p1.y;
        }

        public Double GetX()
        {
            Double a = x / w;
            if ((Double.IsNaN(a)) || (Double.IsInfinity(a)))
            {
                throw new NotRepresentableException();
            }
            return a;
        }

        public Double GetY()
        {
            Double a = y / w;
            if ((Double.IsNaN(a)) || (Double.IsInfinity(a)))
            {
                throw new NotRepresentableException();
            }
            return a;
        }

        public ICoordinate Coordinate
        {
            get { return new Coordinate(GetX(), GetY()); }
        }
    }
}