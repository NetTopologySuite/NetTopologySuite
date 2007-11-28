using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

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
    public class Polygon<TCoordinate> : Geometry<TCoordinate>, IPolygon<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <see cref="Polygon{TCoordinate"/>.
        /// </summary>
        public static readonly IPolygon<TCoordinate> Empty = new GeometryFactory<TCoordinate>().CreatePolygon(null, null);

        /// <summary>
        /// The exterior boundary, or <see langword="null" /> if this <see cref="Polygon{TCoordinate}" />
        /// is the empty point.
        /// </summary>
        private readonly ILinearRing<TCoordinate> _shell;

        /// <summary>
        /// The interior boundaries, if any.
        /// </summary>
        protected IEnumerable<ILinearRing<TCoordinate>> _holes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon{TCoordinate}"/> class.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />
        /// , or <see langword="null" /> or empty <see cref="LinearRing{TCoordinate}" />s if the empty
        /// point is to be created.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Polygon(ILinearRing<TCoordinate> shell, IEnumerable<ILinearRing<TCoordinate>> holes) : this(shell, holes, DefaultFactory) { }

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> with the given exterior boundary and
        /// interior boundaries.
        /// </summary>       
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />
        /// , or <see langword="null" /> or empty <see cref="LinearRing{TCoordinate}" />s if the empty
        /// point is to be created.
        /// </param>
        public Polygon(ILinearRing<TCoordinate> shell, IEnumerable<ILinearRing<TCoordinate>> holes, IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (shell == null)
            {
                shell = Factory.CreateLinearRing(null);
            }

            if (holes == null)
            {
                holes = new ILinearRing<TCoordinate>[] {};
            }

            if (shell.IsEmpty && HasNonEmptyElements(holes))
            {
                throw new ArgumentException("shell is empty but holes are not");
            }

            _shell = shell;
            _holes = holes;
        }

        public override IEnumerable<TCoordinate> Coordinates
        {
            get
            {
                if (IsEmpty)
                {
                    return new ICoordinate[] {};
                }

                ICoordinate[] coordinates = new ICoordinate[NumPoints];
                Int32 k = -1;
                ICoordinate[] shellCoordinates = _shell.Coordinates;
                
                for (Int32 x = 0; x < shellCoordinates.Length; x++)
                {
                    k++;
                    coordinates[k] = shellCoordinates[x];
                }

                for (Int32 i = 0; i < _holes.Length; i++)
                {
                    ICoordinate[] childCoordinates = _holes[i].Coordinates;

                    for (Int32 j = 0; j < childCoordinates.Length; j++)
                    {
                        k++;
                        coordinates[k] = childCoordinates[j];
                    }
                }

                return coordinates;
            }
        }

        public override Int32 NumPoints
        {
            get
            {
                Int32 numPoints = _shell.NumPoints;

                for (Int32 i = 0; i < _holes.Length; i++)
                {
                    numPoints += _holes[i].NumPoints;
                }

                return numPoints;
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

        public ILineString ExteriorRing
        {
            get { return _shell; }
        }

        public Int32 NumInteriorRings
        {
            get { return _holes.Length; }
        }

        public ILineString[] InteriorRings
        {
            get { return _holes; }
        }

        public ILineString GetInteriorRingN(Int32 n)
        {
            return _holes[n];
        }

        public override string GeometryType
        {
            get { return "Polygon"; }
        }

        public override Double Area
        {
            get
            {
                Double area = 0.0;
                area += Math.Abs(CGAlgorithms.SignedArea(_shell.Coordinates));
               
                for (Int32 i = 0; i < _holes.Length; i++)
                {
                    area -= Math.Abs(CGAlgorithms.SignedArea(_holes[i].Coordinates));
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

                for (Int32 i = 0; i < _holes.Length; i++)
                {
                    len += _holes[i].Length;
                }

                return len;
            }
        }

        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection(null);
                }

                ILinearRing[] rings = new ILinearRing[_holes.Length + 1];
                rings[0] = _shell;
                
                for (Int32 i = 0; i < _holes.Length; i++)
                {
                    rings[i + 1] = _holes[i];
                }

                if (rings.Length <= 1)
                {
                    return Factory.CreateLinearRing(rings[0].CoordinateSequence);
                }

                return Factory.CreateMultiLineString(rings);
            }
        }

        protected override IExtents ComputeExtentsInternal()
        {
            return _shell.EnvelopeInternal;
        }

        public override Boolean EqualsExact(IGeometry other, Double tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            IPolygon otherPolygon = (IPolygon) other;
            IGeometry thisShell = _shell;
            IGeometry otherPolygonShell = otherPolygon.Shell;

            if (!thisShell.EqualsExact(otherPolygonShell, tolerance))
            {
                return false;
            }

            if (_holes.Length != otherPolygon.Holes.Length)
            {
                return false;
            }

            if (_holes.Length != otherPolygon.Holes.Length)
            {
                return false;
            }

            for (Int32 i = 0; i < _holes.Length; i++)
            {
                if (!(_holes[i]).EqualsExact(otherPolygon.Holes[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        public override void Apply(ICoordinateFilter filter)
        {
            _shell.Apply(filter);

            for (Int32 i = 0; i < _holes.Length; i++)
            {
                _holes[i].Apply(filter);
            }
        }

        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            _shell.Apply(filter);

            for (Int32 i = 0; i < _holes.Length; i++)
            {
                _holes[i].Apply(filter);
            }
        }

        public override object Clone()
        {
            Polygon poly = (Polygon) base.Clone();
            poly.shell = (LinearRing) _shell.Clone();
            poly.holes = new ILinearRing[_holes.Length];
            for (Int32 i = 0; i < _holes.Length; i++)
            {
                poly.holes[i] = (LinearRing) _holes[i].Clone();
            }
            return poly;
        }

        public override IGeometry ConvexHull()
        {
            return ExteriorRing.ConvexHull();
        }

        public override void Normalize()
        {
            Normalize(_shell, true);

            foreach (ILinearRing hole in Holes)
            {
                Normalize(hole, false);
            }

            Array.Sort(_holes);
        }

        protected internal override Int32 CompareToSameClass(object o)
        {
            LinearRing thisShell = (LinearRing) _shell;
            ILinearRing otherShell = ((IPolygon) o).Shell;
            return thisShell.CompareToSameClass(otherShell);
        }

        private void Normalize(ILinearRing ring, Boolean clockwise)
        {
            if (ring.IsEmpty)
            {
                return;
            }

            ICoordinate[] uniqueCoordinates = new ICoordinate[ring.Coordinates.Length - 1];
            Array.Copy(ring.Coordinates, 0, uniqueCoordinates, 0, uniqueCoordinates.Length);
            ICoordinate minCoordinate = CoordinateArrays.MinCoordinate(ring.Coordinates);
            CoordinateArrays.Scroll(uniqueCoordinates, minCoordinate);
            Array.Copy(uniqueCoordinates, 0, ring.Coordinates, 0, uniqueCoordinates.Length);
            ring.Coordinates[uniqueCoordinates.Length] = uniqueCoordinates[0];
           
            if (CGAlgorithms.IsCCW(ring.Coordinates) == clockwise)
            {
                CoordinateArrays.Reverse(ring.Coordinates);
            }
        }

        public override Boolean IsRectangle
        {
            get
            {
                if (NumInteriorRings != 0)
                {
                    return false;
                }

                if (Shell == null)
                {
                    return false;
                }

                if (Shell.NumPoints != 5)
                {
                    return false;
                }

                // check vertices have correct values
                ICoordinateSequence seq = Shell.CoordinateSequence;
                Extents env = (Extents) ExtentsInternal;
               
                for (Int32 i = 0; i < 5; i++)
                {
                    Double x = seq.GetX(i);

                    if (!(x == env.MinX || x == env.MaxX))
                    {
                        return false;
                    }

                    Double y = seq.GetY(i);

                    if (!(y == env.MinY || y == env.MaxY))
                    {
                        return false;
                    }
                }

                // check vertices are in right order
                Double prevX = seq.GetX(0);
                Double prevY = seq.GetY(0);
              
                for (Int32 i = 1; i <= 4; i++)
                {
                    Double x = seq.GetX(i);
                    Double y = seq.GetY(i);

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
        public Polygon(LinearRing shell, GeometryFactory factory) : this(shell, null, factory) {}

        /// <summary>
        /// Constructs a <see cref="Polygon{TCoordinate}" /> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />,
        /// or <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(LinearRing shell) : this(shell, null, DefaultFactory) {}

        public ILinearRing Shell
        {
            get { return _shell; }
        }

        public ILinearRing[] Holes
        {
            get { return _holes; }
        }

        /*END ADDED BY MPAUL42 */
    }
}