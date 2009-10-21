using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate
{
    ///<summary>
    /// Models a constraint segment in a triangulation. 
    /// A constraint segment is an oriented straight line segment between a start point and an end point.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class Segment<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private LineSegment<TCoordinate> _ls;
        private Object _data;

        ///<summary>
        /// Creates a new instance for the given ordinates.
        ///</summary>
        ///<param name="factory">a coordinate factory</param>
        ///<param name="x1">x-ordinate of 1st point</param>
        ///<param name="y1">y-ordinate of 1st point</param>
        ///<param name="z1">z-ordinate of 1st point</param>
        ///<param name="x2">x-ordinate of 2nd point</param>
        ///<param name="y2">y-ordinate of 2nd point</param>
        ///<param name="z2">z-ordinate of 2nd point</param>
        public Segment(ICoordinateFactory<TCoordinate> factory, double x1, double y1, double z1, double x2, double y2, double z2)
            :this(factory.Create3D(x1, y1, z1), factory.Create3D(x2, y2, z2))
        {
        }

        /** 
         *  
         */
        ///<summary>
        /// Creates a new instance for the given ordinates, with associated external data.
        ///</summary>
        ///<param name="factory">a coordinate factory</param>
        ///<param name="x1">x-ordinate of 1st point</param>
        ///<param name="y1">y-ordinate of 1st point</param>
        ///<param name="z1">z-ordinate of 1st point</param>
        ///<param name="x2">x-ordinate of 2nd point</param>
        ///<param name="y2">y-ordinate of 2nd point</param>
        ///<param name="z2">z-ordinate of 2nd point</param>
        ///<param name="data">external data</param>
        public Segment(ICoordinateFactory<TCoordinate> factory, double x1, double y1, double z1, double x2, double y2, double z2, Object data)
            :this(factory.Create3D(x1, y1, z1),factory.Create3D(x2, y2, z2), data)
        {
        }

        ///<summary>
        /// Creates a new instance for the given points, with associated external data.
        ///</summary>
        ///<param name="p0">the start point</param>
        ///<param name="p1">the end point</param>
        ///<param name="data">an external data object</param>
        public Segment(TCoordinate p0, TCoordinate p1, Object data)
        {
            _ls = new LineSegment<TCoordinate>(p0, p1);
            _data = data;
        }

        ///<summary>
        /// Creates a new instance for the given points.
        ///</summary>
        ///<param name="p0"></param>
        ///<param name="p1"></param>
        public Segment(TCoordinate p0, TCoordinate p1)
            :this(p0, p1, null)
        {
        }

        ///<summary>
        /// Gets the start coordinate of the segment
        ///</summary>
        public TCoordinate Start
        {
            get { return _ls.P0; }
        }

        ///<summary>
        /// Gets the end TCoordinate of the segment
        ///</summary>
        public TCoordinate End
        {
            get { return _ls.P1; }
        }

        ///<summary>
        ///  Gets the start X ordinate of the segment
        ///</summary>
        public Double StartX
        {
            get { return Start[Ordinates.X];}
        }

        ///<summary>
        ///  Gets the start Y ordinate of the segment
        ///</summary>
        public double StartY
        {
            get { return Start[Ordinates.Y]; }
        }

        ///<summary>
        ///  Gets the start Z ordinate of the segment
        ///</summary>
        public double StartZ
        {
            get { return Start[Ordinates.Z]; }
        }

        ///<summary>
        ///  Gets the end X ordinate of the segment
        ///</summary>
        public double EndX
        {
            get { return End[Ordinates.X]; }
        }

        ///<summary>
        ///  Gets the end Y ordinate of the segment
        ///</summary>
        public double EndY
        {
            get { return End[Ordinates.Y]; }
        }

        ///<summary>
        ///  Gets the end Z ordinate of the segment
        ///</summary>
        public double EndZ
        {
            get { return End[Ordinates.Z]; }
        }

        ///<summary>
        /// Gets a <see cref="LineSegment{TCoordinate}"/> modelling this segment.
        ///</summary>
        public LineSegment<TCoordinate> LineSegment
        {
            get { return _ls; }
        }

        ///<summary>
        /// Gets/Sets the external _data associated with this segment
        ///</summary>
        public Object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        ///<summary>
        /// Determines whether two segments are topologically equal. I.e. equal up to orientation.
        ///</summary>
        ///<param name="otherLs">a segment</param>
        ///<returns>true if the segments are topologically equal</returns>
        public Boolean EqualsTopologically(Segment<TCoordinate> otherLs)
        {
            return _ls.EqualsTopologically(otherLs.LineSegment);
        }

        ///<summary>
        /// Computes the intersection point between this segment and another one.
        ///</summary>
        ///<param name="factory">factory to compute intersection point</param> 
        ///<param name="otherLs">a segment</param>
        ///<returns>the intersection point, or <value>null</value> if there is none</returns>
        public TCoordinate Intersection(IGeometryFactory<TCoordinate> factory, Segment<TCoordinate> otherLs)
        {
            return _ls.Intersection(otherLs.LineSegment, factory);
        }

        public override String ToString()
        {
            return _ls.ToString();
        }
    }
}
