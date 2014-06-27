using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Represents a single point.
    /// <para/>
    /// A <c>Point</c> is topologically valid if and only if:
    /// <list type="Bullet">
    /// <item>The coordinate which defines it if any) is a valid coordinate 
    /// (i.e. does not have an <c>NaN</c> X- or Y-ordinate</item>
    /// </list>
    /// </summary>
    /// 
#if !PCL
    [Serializable]

#endif
    public class Point : Geometry, IPoint
    {
        private static readonly Coordinate EmptyCoordinate = null;

        /// <summary>
        /// Represents an empty <c>Point</c>.
        /// </summary>
        public static readonly IPoint Empty = new GeometryFactory().CreatePoint(EmptyCoordinate);

        /// <summary>  
        /// The <c>Coordinate</c> wrapped by this <c>Point</c>.
        /// </summary>
        private ICoordinateSequence _coordinates;        

        /// <summary>
        /// 
        /// </summary>
        public ICoordinateSequence CoordinateSequence
        {
            get
            {
                return _coordinates;
            }
        }             

         /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="coordinate">The coordinate used for create this <see cref="Point" />.</param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(Coordinate coordinate) :   
            this(GeometryFactory.Default.CoordinateSequenceFactory.Create(new Coordinate[] { coordinate } ),
            GeometryFactory.Default) { }

        /// <summary>
        /// Constructs a <c>Point</c> with the given coordinate.
        /// </summary>
        /// <param name="coordinates">
        /// Contains the single coordinate on which to base this <c>Point</c>,
        /// or <c>null</c> to create the empty point.
        /// </param>
        /// <param name="factory"></param>
        public Point(ICoordinateSequence coordinates, IGeometryFactory factory) : base(factory)
        {               
            if (coordinates == null) 
                coordinates = factory.CoordinateSequenceFactory.Create(new Coordinate[] { });
            NetTopologySuite.Utilities.Assert.IsTrue(coordinates.Count <= 1);
            this._coordinates = coordinates;
        }        

        /// <summary>
        /// 
        /// </summary>
        public override Coordinate[] Coordinates 
        {
            get
            {
                return IsEmpty ? new Coordinate[] { } : new Coordinate[] { this.Coordinate };
            }
        }

        public override double[] GetOrdinates(Ordinate ordinate)
        {

            if (IsEmpty)
                return new double[0];

            var ordinateFlag = OrdinatesUtility.ToOrdinatesFlag(ordinate);
            if ((_coordinates.Ordinates & ordinateFlag) != ordinateFlag)
                return new[] {Coordinate.NullOrdinate};
            
            return new [] { _coordinates.GetOrdinate(0, ordinate)};
        }

        /// <summary>
        /// 
        /// </summary>
        public override int NumPoints
        {
            get
            {
                return IsEmpty ? 0 : 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsEmpty 
        {
            get
            {
                return this.Coordinate == null;
            }
        }

        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        //public override bool IsValid
        //{
        //    get
        //    {
        //        if (!IsValidOp.IsValidCoordinate(Coordinate))
        //            return false;
        //        return true;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        public override Dimension Dimension
        {
            get
            {
                return Dimension.Point;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimension BoundaryDimension 
        {
            get
            {
                return Dimension.False;
            }
        }

        /// <summary>
        /// 
        /// </summary>        
        public double X
        {
            get
            {
                if (Coordinate == null)
                    throw new ArgumentOutOfRangeException("X called on empty Point");                
                return Coordinate.X;
            }
            set
            {
                Coordinate.X = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>        
        public double Y 
        {
            get
            {
                if (Coordinate == null)
                    throw new ArgumentOutOfRangeException("Y called on empty Point");                
                return Coordinate.Y;
            }
            set
            {
                Coordinate.Y = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Coordinate Coordinate
        {
            get
            {
                return _coordinates.Count != 0 ? _coordinates.GetCoordinate(0) : null;
            }
        }

        /// <summary>  
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"Point"</returns>
        public override string GeometryType 
        {
            get
            {
                return "Point";
            }
        }

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType
        {
            get { return OgcGeometryType.Point; }
        }

        /// <summary>
        ///Gets the boundary of this geometry.
        ///Zero-dimensional geometries have no boundary by definition,
        ///so an empty GeometryCollection is returned.
        /// </summary>
        public override IGeometry Boundary
        {
            get
            {
                return Factory.CreateGeometryCollection(null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Envelope ComputeEnvelopeInternal() 
        {
            if (IsEmpty) 
                return new Envelope();            
            return new Envelope(Coordinate.X, Coordinate.X, Coordinate.Y, Coordinate.Y);
        }

        //internal override int  GetHashCodeInternal(int baseValue, Func<int,int> operation)
        //{
        //    if (!IsEmpty)
        //        baseValue = operation(baseValue) + _coordinates.GetX(0).GetHashCode();
        //    return baseValue;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(IGeometry other, double tolerance) 
        {
            if (!IsEquivalentClass(other)) 
                return false;            
            if (IsEmpty && other.IsEmpty) 
                return true;
            if (IsEmpty != other.IsEmpty)
                return false;

            return Equal(other.Coordinate, Coordinate, tolerance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(ICoordinateFilter filter) 
        {
            if (IsEmpty) 
                return;             
            filter.Filter(Coordinate);
        }

        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (IsEmpty)
                return;
            filter.Filter(_coordinates, 0);
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryComponentFilter filter) 
        {
            filter.Filter(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Clone() 
        {
            Point p = (Point) base.Clone();
            p._coordinates = (ICoordinateSequence) _coordinates.Clone();
            return p; 
        }

        public override IGeometry Reverse()
        {
            return (IGeometry)Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Normalize() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object other) 
        {
            Point point = (Point)  other;
            return Coordinate.CompareTo(point.Coordinate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object other, IComparer<ICoordinateSequence> comparer)
        {
            return comparer.Compare(CoordinateSequence, ((IPoint) other).CoordinateSequence);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> set to </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(double x, double y, double z) : 
            this(DefaultFactory.CoordinateSequenceFactory.Create(new Coordinate[] { new Coordinate(x, y, z) }), DefaultFactory) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> set to </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(double x, double y)
            : this(DefaultFactory.CoordinateSequenceFactory.Create(new Coordinate[] { new Coordinate(x, y) }), DefaultFactory) { }

        /// <summary>
        /// 
        /// </summary>        
        public double Z
        {
            get
            {
                if (Coordinate == null)
                    throw new ArgumentOutOfRangeException("Z called on empty Point");
                return Coordinate.Z;
            }
            set 
            { 
                Coordinate.Z = value; 
            }
        }

        /* END ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// 
        /// </summary>        
        public double M
        {
            get
            {
                if (Coordinate == null)
                    throw new ArgumentOutOfRangeException("M called on empty Point");
                return Coordinate.NullOrdinate;
            }
            set
            {
                //Coordinate.M = value;
            }
        }
    
    }
}
