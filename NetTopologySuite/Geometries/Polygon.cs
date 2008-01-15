using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack.Interfaces;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a linear polygon, which may include holes.
    /// The shell and holes of the polygon are represented by 
    /// <see cref="ILinearRing{TCoordinates}"/>s.
    /// </summary>
    /// <remarks>
    /// In a valid polygon, holes may touch the shell or other holes at a single point.
    /// However, no sequence of touching holes may split the polygon into two pieces.
    /// The orientation of the rings in the polygon does not matter.
    /// The shell and holes must conform to the assertions specified in the
    /// <see href="http://www.opengis.org/techno/specs.htm">
    /// OpenGIS Simple Features Specification for SQL </see>.     
    /// </remarks>
    [Serializable]
    public class Polygon<TCoordinate> : Geometry<TCoordinate>, IPolygon<TCoordinate>, IHasGeometryComponents<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <see cref="Polygon{TCoordinate}"/>.
        /// </summary>
        public static readonly IPolygon<TCoordinate> Empty =
            new GeometryFactory<TCoordinate>().CreatePolygon(null, null);

        /// <summary>
        /// The exterior boundary, or <see langword="null" /> if this <see cref="Polygon{TCoordinate}" />
        /// is the empty point.
        /// </summary>
        private readonly ILinearRing<TCoordinate> _shell;

        /// <summary>
        /// The interior boundaries, if any.
        /// </summary>
        private readonly List<ILineString<TCoordinate>> _holes = new List<ILineString<TCoordinate>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon{TCoordinate}"/> class.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty 
        /// <see cref="LinearRing{TCoordinate}" /> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />
        /// , or <see langword="null" /> or empty 
        /// <see cref="LinearRing{TCoordinate}" />s if the empty
        /// point is to be created.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> 
        /// is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> 
        /// <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public Polygon(ILinearRing<TCoordinate> shell, IEnumerable<ILineString<TCoordinate>> holes)
            : this(shell, holes, DefaultFactory) {}

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> 
        /// with the given exterior boundary and
        /// interior boundaries.
        /// </summary>       
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty 
        /// <see cref="LinearRing{TCoordinate}" /> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />
        /// , or <see langword="null" /> or empty 
        /// <see cref="LinearRing{TCoordinate}" />s if the empty
        /// point is to be created.
        /// </param>
        public Polygon(ILinearRing<TCoordinate> shell, IEnumerable<ILineString<TCoordinate>> holes,
                       IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (shell == null)
            {
                shell = Factory.CreateLinearRing(null);
            }

            if (shell.IsEmpty && GeometryCollection<TCoordinate>.HasNonEmptyElements(holes))
            {
                throw new ArgumentException("Shell is empty but holes are not.");
            }

            _shell = shell;
            _holes.AddRange(holes);
        }

        public override ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                //if (IsEmpty)
                //{
                //    yield break;
                //}

                //foreach (TCoordinate coordinate in _shell.Coordinates)
                //{
                //    yield return coordinate;
                //}

                //foreach (ILinearRing<TCoordinate> ring in _holes)
                //{
                //    foreach (TCoordinate coordinate in ring.Coordinates)
                //    {
                //        yield return coordinate;
                //    }
                //}

                return null;
            }
        }

        public override Int32 PointCount
        {
            get
            {
                Int32 count = _shell.PointCount;

                foreach (ILinearRing<TCoordinate> ring in _holes)
                {
                    count += ring.PointCount;
                }

                return count;
            }
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Surface; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.Curve; }
        }

        public override Boolean IsEmpty
        {
            get { return _shell.IsEmpty; }
        }

        public override Boolean IsSimple
        {
            get { return true; }
        }

        public ILineString<TCoordinate> ExteriorRing
        {
            get { return _shell; }
        }

        public Int32 InteriorRingsCount
        {
            get { return _holes.Count; }
        }

        public IList<ILineString<TCoordinate>> InteriorRings
        {
            get { return _holes; }
        }

        //public ILineString GetInteriorRingN(Int32 n)
        //{
        //    return _holes[n];
        //}

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.Polygon; }
        }

        public override Double Area
        {
            get
            {
                Double area = 0.0;
                area += Math.Abs(CGAlgorithms<TCoordinate>.SignedArea(_shell.Coordinates));

                for (Int32 i = 0; i < _holes.Count; i++)
                {
                    area -= Math.Abs(CGAlgorithms<TCoordinate>.SignedArea(_holes[i].Coordinates));
                }

                return area;
            }
        }

        /// <summary>
        /// Returns the perimeter of this <see cref="Polygon{TCoordinate}" />.
        /// </summary>
        public override Double Length
        {
            get
            {
                Double len = 0.0;
                len += _shell.Length;

                for (Int32 i = 0; i < _holes.Count; i++)
                {
                    len += _holes[i].Length;
                }

                return len;
            }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection(null);
                }

                if(InteriorRingsCount == 0)
                {
                    return _shell.Clone();
                }
                else
                {
                    IEnumerable<ILineString<TCoordinate>> lineStrings = Slice.Append(InteriorRings, _shell as ILineString<TCoordinate>);
                    return Factory.CreateMultiLineString(lineStrings);
                }

            }
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            IPolygon<TCoordinate> otherPolygon = other as IPolygon<TCoordinate>;

            if (otherPolygon == null)
            {
                return false;
            }

            IGeometry<TCoordinate> thisShell = _shell;
            IGeometry<TCoordinate> otherPolygonShell = otherPolygon.ExteriorRing;

            if (!thisShell.Equals(otherPolygonShell, tolerance))
            {
                return false;
            }

            if (_holes.Count != otherPolygon.InteriorRings.Count)
            {
                return false;
            }

            for (Int32 i = 0; i < _holes.Count; i++)
            {
                if (!(_holes[i]).Equals(otherPolygon.InteriorRings[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        //public override void Apply(ICoordinateFilter<TCoordinate> filter)
        //{
        //    _shell.Apply(filter);

        //    foreach (ILineString<TCoordinate> lineString in _holes)
        //    {
        //        lineString.Apply(filter);
        //    }
        //}

        //public override void Apply(IGeometryFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);
        //}

        //public void Apply(IGeometryComponentFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);
        //    _shell.Apply(filter);

        //    foreach (ILineString<TCoordinate> lineString in _holes)
        //    {
        //        lineString.Apply(filter);
        //    }
        //}

        public override IGeometry<TCoordinate> Clone()
        {
            ILinearRing<TCoordinate> shell = _shell.Clone() as ILinearRing<TCoordinate>;

            return Factory.CreatePolygon(shell, cloneLines(_holes));
        }

        public override IGeometry<TCoordinate> ConvexHull()
        {
            return (ExteriorRing as ISpatialOperator<TCoordinate>).ConvexHull();
        }

        public override void Normalize()
        {
            normalize(_shell, true);

            foreach (ILinearRing<TCoordinate> ring in InteriorRings)
            {
                normalize(ring, false);
            }

            if (InteriorRingsCount > 0)
            {
                _holes.Sort();
            }
        }

        protected override Extents<TCoordinate> ComputeExtentsInternal()
        {
            Debug.Assert(_shell.Extents is Extents<TCoordinate>);
            return _shell.Extents as Extents<TCoordinate>;
        }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            Debug.Assert(other is IPolygon<TCoordinate>);
            IPolygon<TCoordinate> otherPolygon = other as IPolygon<TCoordinate>;
            ILineString<TCoordinate> otherShell = otherPolygon.ExteriorRing;
            return _shell.CompareToSameClass(otherShell);
        }

        private static IEnumerable<ILinearRing<TCoordinate>> cloneLines(List<ILineString<TCoordinate>> _holes)
        {
            throw new NotImplementedException();
        }

        private void normalize(ILinearRing<TCoordinate> ring, Boolean clockwise)
        {
            if (ring.IsEmpty)
            {
                return;
            }

            TCoordinate minCoordinate = ring.Coordinates.Minimum();
            ring.Coordinates.Scroll(minCoordinate);

            if (CGAlgorithms<TCoordinate>.IsCCW(ring.Coordinates) == clockwise)
            {
                ring.Coordinates.Reverse();
            }
        }

        public override Boolean IsRectangle
        {
            get
            {
                if (InteriorRingsCount != 0)
                {
                    return false;
                }

                if (ExteriorRing == null)
                {
                    return false;
                }

                if (ExteriorRing.PointCount != 5)
                {
                    return false;
                }

                // check vertices have correct values
                ICoordinateSequence seq = ExteriorRing.Coordinates;
                Extents<TCoordinate> env = ExtentsInternal;

                for (Int32 i = 0; i < 5; i++)
                {
                    Double x = seq[i, Ordinates.X];

                    if (!(x == env.GetMin(Ordinates.X) || x == env.GetMax(Ordinates.X)))
                    {
                        return false;
                    }

                    Double y = seq[i, Ordinates.Y];

                    if (!(y == env.GetMin(Ordinates.Y) || y == env.GetMax(Ordinates.Y)))
                    {
                        return false;
                    }
                }

                // check vertices are in right order
                Double prevX = seq[0, Ordinates.X];
                Double prevY = seq[0, Ordinates.Y];

                for (Int32 i = 1; i <= 4; i++)
                {
                    Double x = seq[i, Ordinates.X];
                    Double y = seq[i, Ordinates.Y];

                    Boolean xChanged = x != prevX;
                    Boolean yChanged = y != prevY;

                    if (xChanged == yChanged)
                    {
                        return false;
                    }

                    prevX = x;
                    prevY = y;
                }

                return true;
            }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(ILinearRing<TCoordinate> shell, IGeometryFactory<TCoordinate> factory) 
            : this(shell, null, factory) { }

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(ILinearRing<TCoordinate> shell) : this(shell, null, DefaultFactory) { }

        //public ILinearRing<TCoordinate> Shell
        //{
        //    get { return _shell; }
        //}

        //public IList<ILineString<TCoordinate>> Holes
        //{
        //    get { return _holes; }
        //}

        /*END ADDED BY MPAUL42 */

        #region IPolygon Members

        ILineString IPolygon.ExteriorRing
        {
            get { return _shell; }
        }


        IList<ILineString> IPolygon.InteriorRings
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region ISurface Members


        IPoint ISurface.PointOnSurface
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IHasGeometryComponents<TCoordinate> Members

        public IEnumerable<IGeometry<TCoordinate>> Components
        {
            get
            {
                yield return _shell;

                foreach (ILineString<TCoordinate> hole in _holes)
                {
                    yield return hole;
                }
            }
        }

        #endregion
    }
}