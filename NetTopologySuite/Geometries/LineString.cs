using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Operation;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <c>LineString</c>.
    /// </summary>  
    [Serializable]
    public class LineString : Geometry, ILineString
    {
        /// <summary>
        /// Represents an empty <c>LineString</c>.
        /// </summary>
        public static readonly ILineString Empty = new GeometryFactory().CreateLineString(new ICoordinate[] {});

        /// <summary>  
        /// The points of this <c>LineString</c>.
        /// </summary>
        private ICoordinateSequence points;

        public override ICoordinate[] Coordinates
        {
            get { return points.ToCoordinateArray(); }
        }

        public ICoordinateSequence CoordinateSequence
        {
            get { return points; }
        }

        /// <param name="points">
        /// The points of the linestring, or <c>null</c>
        /// to create the empty point. Consecutive points may not be equal.
        /// </param>
        public LineString(ICoordinateSequence points, IGeometryFactory factory) : base(factory)
        {
            if (points == null)
            {
                points = factory.CoordinateSequenceFactory.Create(new ICoordinate[] {});
            }

            if (points.Count == 1)
            {
                throw new ArgumentException("point array must contain 0 or >1 elements", "points");
            }

            this.points = points;
        }

        public ICoordinate GetCoordinateN(Int32 n)
        {
            return points.GetCoordinate(n);
        }

        public override ICoordinate Coordinate
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return points.GetCoordinate(0);
            }
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Curve; }
        }

        public override Dimensions BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return Dimensions.False;
                }

                return Dimensions.Point;
            }
        }

        public override Boolean IsEmpty
        {
            get { return points.Count == 0; }
        }

        public override Int32 NumPoints
        {
            get { return points.Count; }
        }

        public IPoint GetPointN(Int32 n)
        {
            return Factory.CreatePoint(points.GetCoordinate(n));
        }

        public IPoint StartPoint
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return GetPointN(0);
            }
        }

        public IPoint EndPoint
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return GetPointN(NumPoints - 1);
            }
        }

        public virtual Boolean IsClosed
        {
            get
            {
                if (IsEmpty)
                {
                    return false;
                }

                return GetCoordinateN(0).Equals2D(GetCoordinateN(NumPoints - 1));
            }
        }

        public Boolean IsRing
        {
            get { return IsClosed && IsSimple; }
        }

        public override string GeometryType
        {
            get { return "LineString"; }
        }

        /// <summary>  
        /// Returns the length of this <c>LineString</c>
        /// </summary>
        /// <returns>The length of the polygon.</returns>
        public override Double Length
        {
            get { return CGAlgorithms.Length(points); }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp()).IsSimple(this); }
        }

        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection(null);
                }

                if (IsClosed)
                {
                    return Factory.CreateMultiPoint((ICoordinate[]) null);
                }

                return Factory.CreateMultiPoint(new IPoint[] {StartPoint, EndPoint});
            }
        }

        /// <summary>
        /// Creates a <see cref="LineString" /> whose coordinates are in the reverse order of this objects.
        /// </summary>
        /// <returns>A <see cref="LineString" /> with coordinates in the reverse order.</returns>
        public ILineString Reverse()
        {
            ICoordinateSequence seq = (ICoordinateSequence) points.Clone();

            // Personalized implementation using Array.Reverse: maybe it's faster?
            ICoordinate[] array = seq.ToCoordinateArray();
            Array.Reverse(array);
            return Factory.CreateLineString(array);
        }

        /// <summary>
        /// Returns true if the given point is a vertex of this <c>LineString</c>.
        /// </summary>
        /// <param name="pt">The <c>Coordinate</c> to check.</param>
        /// <returns><c>true</c> if <c>pt</c> is one of this <c>LineString</c>'s vertices.</returns>
        public Boolean IsCoordinate(ICoordinate pt)
        {
            for (Int32 i = 0; i < points.Count; i++)
            {
                if (points.GetCoordinate(i).Equals(pt))
                {
                    return true;
                }
            }

            return false;
        }

        protected override IExtents ComputeEnvelopeInternal()
        {
            if (IsEmpty)
            {
                return new Extents();
            }

            //Convert to array, then access array directly, to avoid the function-call overhead
            //of calling Getter millions of times. ToArray may be inefficient for
            //non-BasicCoordinateSequence CoordinateSequences. [Jon Aquino]
            ICoordinate[] coordinates = points.ToCoordinateArray();
            Double minx = coordinates[0].X;
            Double miny = coordinates[0].Y;
            Double maxx = coordinates[0].X;
            Double maxy = coordinates[0].Y;
            
            for (Int32 i = 1; i < coordinates.Length; i++)
            {
                minx = minx < coordinates[i].X ? minx : coordinates[i].X;
                maxx = maxx > coordinates[i].X ? maxx : coordinates[i].X;
                miny = miny < coordinates[i].Y ? miny : coordinates[i].Y;
                maxy = maxy > coordinates[i].Y ? maxy : coordinates[i].Y;
            }

            return new Extents(minx, maxx, miny, maxy);
        }

        public override Boolean EqualsExact(IGeometry other, Double tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            ILineString otherLineString = (ILineString) other;
          
            if (points.Count != otherLineString.NumPoints)
            {
                return false;
            }

            for (Int32 i = 0; i < points.Count; i++)
            {
                if (!Equal(points.GetCoordinate(i), otherLineString.GetCoordinateN(i), tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        public override void Apply(ICoordinateFilter filter)
        {
            for (Int32 i = 0; i < points.Count; i++)
            {
                filter.Filter(points.GetCoordinate(i));
            }
        }

        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
        }

        public override object Clone()
        {
            LineString ls = (LineString) base.Clone();
            ls.points = (ICoordinateSequence) points.Clone();
            return ls;
        }

        /// <summary> 
        /// Normalizes a <c>LineString</c>.  A normalized linestring
        /// has the first point which is not equal to it's reflected point
        /// less than the reflected point.
        /// </summary>
        public override void Normalize()
        {
            for (Int32 i = 0; i < points.Count/2; i++)
            {
                Int32 j = points.Count - 1 - i;

                // skip equal points on both ends
                if (!points.GetCoordinate(i).Equals(points.GetCoordinate(j)))
                {
                    if (points.GetCoordinate(i).CompareTo(points.GetCoordinate(j)) > 0)
                    {
                        CoordinateArrays.Reverse(Coordinates);
                    }

                    return;
                }
            }
        }

        protected internal override Int32 CompareToSameClass(object o)
        {
            LineString line = (LineString) o;
            // MD - optimized implementation
            Int32 i = 0;
            Int32 j = 0;

            while (i < points.Count && j < line.points.Count)
            {
                Int32 comparison = points.GetCoordinate(i).CompareTo(line.points.GetCoordinate(j));
                
                if (comparison != 0)
                {
                    return comparison;
                }

                i++;
                j++;
            }

            if (i < points.Count)
            {
                return 1;
            }

            if (j < line.points.Count)
            {
                return -1;
            }

            return 0;
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LineString"/> class.
        /// </summary>        
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        /// <param name="points">The coordinates used for create this <see cref="LineString" />.</param>
        public LineString(ICoordinate[] points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) {}

        public ICoordinate this[Int32 n]
        {
            get { return points.GetCoordinate(n); }
            set
            {
                points.SetOrdinate(n, Ordinates.X, value.X);
                points.SetOrdinate(n, Ordinates.Y, value.Y);
                points.SetOrdinate(n, Ordinates.Z, value.Z);
            }
        }

        public Int32 Count
        {
            get { return points.Count; }
        }

        /// <summary>
        /// Returns the value of the angle between the <see cref="StartPoint" />
        /// and the <see cref="EndPoint" />.
        /// </summary>
        public Double Angle
        {
            get
            {
                Double deltaX = EndPoint.X - StartPoint.X;
                Double deltaY = EndPoint.Y - StartPoint.Y;
                Double length = Math.Sqrt(deltaX*deltaX + deltaY*deltaY);
                Double angleRAD = Math.Asin(Math.Abs(EndPoint.Y - StartPoint.Y)/length);
                Double angle = (angleRAD*180)/Math.PI;

                if (((StartPoint.X < EndPoint.X) && (StartPoint.Y > EndPoint.Y)) ||
                    ((StartPoint.X > EndPoint.X) && (StartPoint.Y < EndPoint.Y)))
                {
                    angle = 360 - angle;
                }

                return angle;
            }
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}