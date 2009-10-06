using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

#if DOTNET35
using System.Linq;
#endif

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
    public class Polygon<TCoordinate> : MultiCoordinateGeometry<TCoordinate>,
                                        IPolygon<TCoordinate>,
                                        IHasGeometryComponents<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///// <summary>
        ///// Represents an empty <see cref="Polygon{TCoordinate}"/>.
        ///// </summary>
        //public static readonly IPolygon<TCoordinate> Empty =
        //    new GeometryFactory<TCoordinate>().CreatePolygon(null, null);

        /// <summary>
        /// The exterior boundary, or <see langword="null" /> if this 
        /// <see cref="Polygon{TCoordinate}" /> is the empty point.
        /// </summary>
        private readonly ILinearRing<TCoordinate> _shell;

        /// <summary>
        /// The interior boundaries, if any.
        /// </summary>
        private List<ILineString<TCoordinate>> _holes;

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
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />, 
        /// or <see langword="null" /> or empty 
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
            : this(
                shell, holes,
                ExtractGeometryFactory(Caster.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(holes)))
        {
        }

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
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or empty 
        /// <see cref="LinearRing{TCoordinate}" />s if the empty
        /// point is to be created.
        /// </param>
        public Polygon(ILinearRing<TCoordinate> shell,
                       IEnumerable<ILineString<TCoordinate>> holes,
                       IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (shell == null)
            {
                shell = Factory.CreateLinearRing(null);
            }

            Boolean hasValidHoles = (holes != null) && GeometryCollection<TCoordinate>.HasNonEmptyElements(holes);

            if (shell.IsEmpty && hasValidHoles)
            {
                throw new ArgumentException("Shell is empty but holes are not.");
            }

            _shell = shell;

            if (hasValidHoles)
            {
                _holes = new List<ILineString<TCoordinate>>();
                _holes.AddRange(holes);
            }
        }

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> 
        /// with the given exterior boundary and
        /// interior boundaries.
        /// </summary>       
        /// <param name="sequence">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty 
        /// <see cref="LinearRing{TCoordinate}" /> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="factory">
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or empty 
        /// <see cref="LinearRing{TCoordinate}" />s if the empty
        /// point is to be created.
        /// </param>
        public Polygon(ICoordinateSequence<TCoordinate> sequence,
                       IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            IEnumerable<ILinearRing<TCoordinate>> rings
                = Coordinates<TCoordinate>.CreateLinearRings(sequence, factory);

            foreach (ILinearRing<TCoordinate> ring in rings)
            {
                if (ring.IsCcw)
                {
                    if (ExteriorRing == null || ExteriorRing.IsEmpty)
                    {
                        throw new TopologyException(
                            "The coordinate sequence specifies holes without a shell.");
                    }

                    if (_holes == null)
                    {
                        _holes = new List<ILineString<TCoordinate>>();
                    }
                    _holes.Add(ring);
                }
                else
                {
                    if (_shell != null)
                    {
                        throw new TopologyException(
                            "The coordinate sequence specifies two exterior rings.");
                    }

                    _shell = ring;
                }
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
            : this(shell, null, factory)
        {
        }

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(ILinearRing<TCoordinate> shell)
            : this(shell, null, shell.Factory)
        {
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

                if (_holes != null)
                {
                    for (Int32 i = 0; i < _holes.Count; i++)
                    {
                        len += _holes[i].Length;
                    }
                }

                return len;
            }
        }

        #region IHasGeometryComponents<TCoordinate> Members

        public IEnumerable<IGeometry<TCoordinate>> Components
        {
            get
            {
                yield return _shell;

                if (_holes != null)
                {
                    foreach (ILineString<TCoordinate> hole in _holes)
                    {
                        yield return hole;
                    }
                }
            }
        }

        #endregion

        /* END ADDED BY MPAUL42: monoGIS team */

        #region IPolygon<TCoordinate> Members

        public override ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                // TODO: fix polygon coordinate sequences by using slices for _shell and _holes
                ICoordinateSequence<TCoordinate> seq =
                    Factory.CoordinateSequenceFactory.Create(_shell.Coordinates);

                if (_holes != null)
                {
                    foreach (ILineString<TCoordinate> hole in _holes)
                    {
                        seq.AddSequence(hole.Coordinates);
                    }
                }

                return seq;
            }
        }

        public override IEnumerable<TCoordinate> GetVertexes()
        {
            foreach (TCoordinate coordinate in _shell.Coordinates)
                yield return coordinate;
            if ( _holes != null )
                foreach (ILineString<TCoordinate> s in _holes)
                    foreach (TCoordinate coord in s.Coordinates)
                        yield return coord;
        }

        public override Boolean EqualsExact(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            // TODO: drop this when the get_Coordinates TODO is fixed
            if (g == null) throw new ArgumentNullException("g");

            if (!IsEquivalentClass(g))
            {
                return false;
            }

            Polygon<TCoordinate> other = g as Polygon<TCoordinate>;
            Debug.Assert(other != null);

            ICoordinateSequence<TCoordinate> otherCoords = other.Coordinates;

            return Coordinates.Equals(otherCoords, tolerance);
        }

        public override Int32 PointCount
        {
            get
            {
                Int32 count = _shell.PointCount;

                if (_holes != null)
                {
                    foreach (ILinearRing<TCoordinate> ring in _holes)
                    {
                        count += ring.PointCount;
                    }
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
            get { return _shell == null || _shell.IsEmpty; }
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
            get
            {
                if (_holes == null)
                    return 0;
                return _holes.Count;
            }
        }

        public IEnumerable<ILineString<TCoordinate>> InteriorRings
        {
            get
            {
                if (_holes == null)
                {
                    _holes = new List<ILineString<TCoordinate>>();
                }
                return _holes;
            }
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

                if (_holes != null)
                {
                    for (Int32 i = 0; i < _holes.Count; i++)
                    {
                        area -= Math.Abs(CGAlgorithms<TCoordinate>.SignedArea(_holes[i].Coordinates));
                    }
                }
                return area;
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

                if (InteriorRingsCount == 0)
                {
                    return _shell.Clone();
                }

                // Leave the explicit type parameter on Slice.Append,
                // since compiling with ToolsVersion=2.0 fails otherwise.
                IEnumerable<ILineString<TCoordinate>> lineStrings
                    = Slice.Append(InteriorRings, _shell);
                return Factory.CreateMultiLineString(lineStrings);
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


            if (InteriorRingsCount != otherPolygon.InteriorRingsCount)
            {
                return false;
            }

            if (InteriorRingsCount == 0 && otherPolygon.InteriorRingsCount == 0)
            {
                return true;
            }

            IEnumerator<ILineString<TCoordinate>> otherPolygonHoles = otherPolygon.InteriorRings.GetEnumerator();
            IEnumerator<ILineString<TCoordinate>> holes = _holes.GetEnumerator();

            while (otherPolygonHoles.MoveNext() && holes.MoveNext())
            {
                if (!holes.Current.Equals(otherPolygonHoles.Current, tolerance))
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

            if (_holes == null)
            {
                return Factory.CreatePolygon(shell);
            }

            IEnumerable<ILinearRing<TCoordinate>> holes
                = Caster.Downcast<ILinearRing<TCoordinate>, ILineString<TCoordinate>>(_holes);

            return Factory.CreatePolygon(shell, holes);
        }

        public override IGeometry<TCoordinate> ConvexHull()
        {
            return ExteriorRing.ConvexHull();
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

        // [codekaizen 2008-01-14]  temporarily commented out in order to investigate
        //                          usage of these properties. Are they redundant? 
        //                          Could they be conditionally compiled?

        //public ILinearRing<TCoordinate> Shell
        //{
        //    get { return _shell; }
        //}

        //public IList<ILineString<TCoordinate>> Holes
        //{
        //    get { return _holes; }
        //}

        /*END ADDED BY MPAUL42 */

        ILineString IPolygon.ExteriorRing
        {
            get { return _shell; }
        }


        IEnumerable<ILineString> IPolygon.InteriorRings
        {
            get
            {
                if (_holes == null)
                {
                    yield break;
                }
                foreach (ILineString<TCoordinate> ring in _holes)
                {
                    yield return ring as ILinearRing;
                }
            }
        }

        IPoint ISurface.PointOnSurface
        {
            get { return PointOnSurface; }
        }

        #endregion

        protected override Extents<TCoordinate> ComputeExtentsInternal()
        {
            Debug.Assert(_shell.Extents is Extents<TCoordinate>);
            return _shell.Extents as Extents<TCoordinate>;
        }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            IPolygon<TCoordinate> otherPolygon = other as IPolygon<TCoordinate>;
            Debug.Assert(otherPolygon != null);
            ILineString<TCoordinate> otherShell = otherPolygon.ExteriorRing;

            if (_shell is LinearRing<TCoordinate>)
            {
                return (_shell as LinearRing<TCoordinate>).CompareToSameClass(otherShell);
            }
            else
            {
                throw new NotSupportedException(
                    "The polygon exterior boundary is an ILinearRing type " +
                    "other than LinearRing<TCoordinate>, and comparison is " +
                    "not currently supported.");
            }
        }

        protected override void OnCoordinatesChanged()
        {
            CoordinatesInternal = null;
        }

        private static void normalize(ILinearRing<TCoordinate> ring, Boolean clockwise)
        {
            if (ring.IsEmpty)
            {
                return;
            }

            TCoordinate last = ring.Coordinates.Last;
            TCoordinate minCoordinate = ring.Coordinates.Minimum;
            Int32 minIndex = ring.Coordinates.IndexOf(minCoordinate);
            if (minIndex > 0)
            {
                ring.Coordinates.Scroll(minCoordinate);
                //ring.Coordinates.RemoveAt(ring.Coordinates.Count - minIndex);
                //ring.Coordinates.CloseRing();
            }

            if (CGAlgorithms<TCoordinate>.IsCCW(ring.Coordinates) == clockwise)
            {
                ring.Coordinates.Reverse();
            }
        }
    }
}