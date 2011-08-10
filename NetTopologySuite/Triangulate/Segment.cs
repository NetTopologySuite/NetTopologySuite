using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate
{

    /**
     * Models a constraint segment in a triangulation.
     * A constraint segment is an oriented straight line segment between a start point
     * and an end point.
     * 
     * @author David Skea
     * @author Martin Davis
     */

    public class Segment
    {
        private readonly LineSegment _ls;
        private Object _data;

        /** 
         * Creates a new instance for the given ordinates.
         */

        public Segment(double x1, double y1, double z1, double x2, double y2, double z2)
            :this(new Coordinate(x1, y1, z1), new Coordinate(x2, y2, z2))
        {
        }

        /** 
         * Creates a new instance for the given ordinates,  with associated external data. 
         */

        public Segment(double x1, double y1, double z1, double x2, double y2, double z2, Object data)
            : this(new Coordinate(x1, y1, z1), new Coordinate(x2, y2, z2), data)
        {
        }

        /** 
         * Creates a new instance for the given points, with associated external data.
         * 
         * @param p0 the start point
         * @param p1 the end point
         * @param data an external data object
         */

        public Segment(ICoordinate p0, ICoordinate p1, Object data)
        {
            _ls = new LineSegment(p0, p1);
            _data = data;
        }

        /** 
         * Creates a new instance for the given points.
         * 
         * @param p0 the start point
         * @param p1 the end point
         */

        public Segment(ICoordinate p0, ICoordinate p1)
        {
            _ls = new LineSegment(p0, p1);
        }

        /**
         * Gets the start coordinate of the segment
         * 
         * @return a Coordinate
         */

        public ICoordinate Start
        {
            get {return _ls.GetCoordinate(0);}
        }

        /**
         * Gets the end coordinate of the segment
         * 
         * @return a Coordinate
         */

        public ICoordinate End
        {
            get {return _ls.GetCoordinate(1);}
        }

        /**
         * Gets the start X ordinate of the segment
         * 
         * @return the X ordinate value
         */

        public double StartX
        {
            get
            {
                var p = _ls.GetCoordinate(0);
                return p.X;
            }
        }

        /**
         * Gets the start Y ordinate of the segment
         * 
         * @return the Y ordinate value
         */

        public double StartY
        {
            get
            {
                var p = _ls.GetCoordinate(0);
                return p.X;
            }
        }

        /**
         * Gets the start Z ordinate of the segment
         * 
         * @return the Z ordinate value
         */

        public double StartZ
        {
            get
            {
                var p = _ls.GetCoordinate(0);
                return p.Z;
            }
        }

        /**
         * Gets the end X ordinate of the segment
         * 
         * @return the X ordinate value
         */

        public double EndX
        {
            get
            {
                var p = _ls.GetCoordinate(1);
                return p.X;
            }
        }

        /**
         * Gets the end Y ordinate of the segment
         * 
         * @return the Y ordinate value
         */

        public double EndY
        {
            get
            {
                var p = _ls.GetCoordinate(1);
                return p.Y;
            }
        }

        /**
         * Gets the end Z ordinate of the segment
         * 
         * @return the Z ordinate value
         */

        public double EndZ
        {
            get
            {
                var p = _ls.GetCoordinate(1);
                return p.Z;
            }
        }

        /**
         * Gets a <tt>LineSegment</tt> modelling this segment.
         * 
         * @return a LineSegment
         */

        public LineSegment LineSegment
        {
           get {return _ls;}
        }

        /**
         * Gets or sets the external data associated with this segment
         * 
         * @return a data object
         */

        public Object Data
        {
            get {return _data;}
            set
            {
                _data = value;
            }
        }

        /**
         * Determines whether two segments are topologically equal.
         * I.e. equal up to orientation.
         * 
         * @param s a segment
         * @return true if the segments are topologically equal
         */

        public bool EqualsTopologically(Segment s)
        {
            return _ls.EqualsTopologically(s.LineSegment);
        }

        /**
         * Computes the intersection point between this segment and another one.
         * 
         * @param s a segment
         * @return the intersection point, or <code>null</code> if there is none
         */

        public ICoordinate Intersection(Segment s)
        {
            return _ls.Intersection(s.LineSegment);
        }

        /**
         * Computes a string representation of this segment.
         * 
         * @return a string
         */

        public override String ToString()
        {
            return _ls.ToString();
        }
    }
}