using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Represents a polygon with linear edges, which may include holes.
    /// The outer boundary (shell)
    /// and inner boundaries (holes) of the polygon are represented by {@link LinearRing}s.
    /// The boundary rings of the polygon may have any orientation.
    /// Polygons are closed, simple geometries by definition.
    /// <para/>
    /// The polygon model conforms to the assertions specified in the
    /// <a href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
    /// Specification for SQL</a>.
    /// <para/>
    /// A <c>Polygon</c> is topologically valid if and only if:
    /// <list type="bullet">
    /// <item><description>the coordinates which define it are valid coordinates</description></item>
    /// <item><description>the linear rings for the shell and holes are valid
    /// (i.e. are closed and do not self-intersect)</description></item>
    /// <item><description>holes touch the shell or another hole at at most one point
    /// (which implies that the rings of the shell and holes must not cross)</description></item>
    /// <item><description>the interior of the polygon is connected,
    /// or equivalently no sequence of touching holes
    /// makes the interior of the polygon disconnected
    /// (i.e. effectively split the polygon into two pieces).</description></item>
    /// </list>
    /// </summary>
    [Serializable]
    public class Polygon : Geometry, IPolygonal
    {
        /// <summary>
        /// Represents an empty <c>Polygon</c>.
        /// </summary>
        public static readonly Polygon Empty = new GeometryFactory().CreatePolygon();

        /// <summary>
        /// The exterior boundary, or <c>null</c> if this <c>Polygon</c>
        /// is the empty point.
        /// </summary>
        private LinearRing _shell;

        /// <summary>
        /// The interior boundaries, if any.
        /// </summary>
        private LinearRing[] _holes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon"/> class.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <c>Polygon</c>
        /// , or <c>null</c> or empty <c>LinearRing</c>s if the empty
        /// point is to be created.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Polygon(LinearRing shell, LinearRing[] holes) : this(shell, holes, DefaultFactory) { }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary and
        /// interior boundaries.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <c>Polygon</c>
        /// , or <c>null</c> or empty <c>LinearRing</c>s if the empty
        /// point is to be created.
        /// </param>
        /// <param name="factory"></param>
        public Polygon(LinearRing shell, LinearRing[] holes, GeometryFactory factory) : base(factory)
        {
            if (shell == null)
                shell = Factory.CreateLinearRing();
            if (holes == null)
                holes = new LinearRing[] { };
            if (HasNullElements<LinearRing>(holes))
                throw new ArgumentException("holes must not contain null elements");
            if (shell.IsEmpty && HasNonEmptyElements(holes))
                throw new ArgumentException("shell is empty but holes are not");

            _shell = shell;
            _holes = holes;
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.Polygon;

        /// <summary>
        /// Returns a vertex of this <c>Geometry</c>
        /// (usually, but not necessarily, the first one).
        /// </summary>
        /// <remarks>
        /// The returned coordinate should not be assumed to be an actual Coordinate object used in the internal representation.
        /// </remarks>
        /// <returns>a Coordinate which is a vertex of this <c>Geometry</c>.</returns>
        /// <returns><c>null</c> if this Geometry is empty.
        /// </returns>
        public override Coordinate Coordinate => _shell.Coordinate;

        /// <summary>
        /// Returns an array containing the values of all the vertices for
        /// this geometry.
        /// </summary>
        /// <remarks>
        /// If the geometry is a composite, the array will contain all the vertices
        /// for the components, in the order in which the components occur in the geometry.
        /// <para>
        /// In general, the array cannot be assumed to be the actual internal
        /// storage for the vertices.  Thus modifying the array
        /// may not modify the geometry itself.
        /// Use the <see cref="CoordinateSequence.SetOrdinate(int, int, double)"/> method
        /// (possibly on the components) to modify the underlying data.
        /// If the coordinates are modified,
        /// <see cref="Geometry.GeometryChanged"/> must be called afterwards.
        /// </para>
        /// </remarks>
        /// <returns>The vertices of this <c>Geometry</c>.</returns>
        /// <seealso cref="Geometry.GeometryChanged"/>
        /// <seealso cref="CoordinateSequence.SetOrdinate(int, int, double)"/>
        /// <seealso cref="CoordinateSequence.SetOrdinate(int, Ordinate, double)"/>
        public override Coordinate[] Coordinates
        {
            get
            {
                if (IsEmpty)
                    return new Coordinate[] { };
                var coordinates = new Coordinate[NumPoints];
                int k = -1;
                var shellCoordinates = _shell.Coordinates;
                for (int x = 0; x < shellCoordinates.Length; x++)
                {
                    k++;
                    coordinates[k] = shellCoordinates[x];
                }
                for (int i = 0; i < _holes.Length; i++)
                {
                    var childCoordinates = _holes[i].Coordinates;
                    for (int j = 0; j < childCoordinates.Length; j++)
                    {
                        k++;
                        coordinates[k] = childCoordinates[j];
                    }
                }
                return coordinates;
            }
        }

        /// <summary>
        /// Gets an array of <see cref="double"/> ordinate values
        /// </summary>
        /// <param name="ordinate">The ordinate index</param>
        /// <returns>An array of ordinate values</returns>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty)
                return new double[0];

            var ordinateFlag = (Ordinates)(1 << (int)ordinate);
            if ((_shell.CoordinateSequence.Ordinates & ordinateFlag) != ordinateFlag)
                return CreateArray(NumPoints, Coordinate.NullOrdinate);

            double[] result = new double[NumPoints];
            double[] ordinates = _shell.GetOrdinates(ordinate);
            Array.Copy(ordinates, 0, result, 0, ordinates.Length);
            int offset = ordinates.Length;
            foreach (var linearRing in _holes)
            {
                ordinates = linearRing.GetOrdinates(ordinate);
                Array.Copy(ordinates, 0, result, offset, ordinates.Length);
                offset += ordinates.Length;
            }

            return result;
        }

        /// <summary>
        /// Returns the count of this <c>Geometry</c>s vertices. The <c>Geometry</c>
        /// s contained by composite <c>Geometry</c>s must be
        /// Geometry's; that is, they must implement <c>NumPoints</c>.
        /// </summary>
        /// <returns>The number of vertices in this <c>Geometry</c>.</returns>
        public override int NumPoints
        {
            get
            {
                int numPoints = _shell.NumPoints;
                for (int i = 0; i < _holes.Length; i++)
                    numPoints += _holes[i].NumPoints;
                return numPoints;
            }
        }

        /// <summary>
        /// Returns the dimension of this geometry.
        /// </summary>
        /// <remarks>
        /// The dimension of a geometry is is the topological
        /// dimension of its embedding in the 2-D Euclidean plane.
        /// In the NTS spatial model, dimension values are in the set {0,1,2}.
        /// <para>
        /// Note that this is a different concept to the dimension of
        /// the vertex <see cref="Coordinate"/>s.
        /// The geometry dimension can never be greater than the coordinate dimension.
        /// For example, a 0-dimensional geometry (e.g. a Point)
        /// may have a coordinate dimension of 3 (X,Y,Z).
        /// </para>
        /// </remarks>
        /// <returns>
        /// The topological dimensions of this geometry
        /// </returns>
        public override Dimension Dimension => Dimension.Surface;

        /// <summary>
        /// Returns the dimension of this <c>Geometry</c>s inherent boundary.
        /// </summary>
        /// <returns>
        /// The dimension of the boundary of the class implementing this
        /// interface, whether or not this object is the empty point. Returns
        /// <c>Dimension.False</c> if the boundary is the empty point.
        /// </returns>
        /// NOTE: make abstract and remove setter
        public override Dimension BoundaryDimension => Dimension.Curve;

        /// <summary>
        ///
        /// </summary>
        public override bool IsEmpty => _shell.IsEmpty;

        /// <summary>
        ///
        /// </summary>
        public LineString ExteriorRing => _shell;

        /// <summary>
        ///
        /// </summary>
        public int NumInteriorRings => _holes.Length;

        /// <summary>
        ///
        /// </summary>
        public LineString[] InteriorRings => _holes.ToArray<LineString>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public LineString GetInteriorRingN(int n)
        {
            return _holes[n];
        }

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"Polygon"</returns>
        public override string GeometryType => Geometry.TypeNamePolygon;

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.Polygon;

        /// <summary>
        /// Returns the area of this <c>Polygon</c>
        /// </summary>
        /// <returns></returns>
        public override double Area
        {
            get
            {
                double area = 0.0;
                area += Algorithm.Area.OfRing(_shell.CoordinateSequence);
                for (int i = 0; i < _holes.Length; i++)
                    area -= Algorithm.Area.OfRing(_holes[i].CoordinateSequence);
                return area;
            }
        }

        /// <summary>
        /// Returns the perimeter of this <c>Polygon</c>.
        /// </summary>
        /// <returns></returns>
        public override double Length
        {
            get
            {
                double len = 0.0;
                len += _shell.Length;
                for (int i = 0; i < _holes.Length; i++)
                    len += _holes[i].Length;
                return len;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty)
                    return Factory.CreateMultiLineString();

                var rings = new LineString[_holes.Length + 1];
                rings[0] = _shell;
                for (int i = 0; i < _holes.Length; i++)
                    rings[i + 1] = _holes[i];
                // create LineString or MultiLineString as appropriate
                if (rings.Length <= 1)
                    return Factory.CreateLinearRing(rings[0].CoordinateSequence);
                return Factory.CreateMultiLineString(rings);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override Envelope ComputeEnvelopeInternal()
        {
            return _shell.EnvelopeInternal;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;

            var otherPolygon = (Polygon) other;
            var thisShell = _shell;
            var otherPolygonShell = otherPolygon.Shell;
            if (!thisShell.EqualsExact(otherPolygonShell, tolerance))
                return false;
            if (_holes.Length != otherPolygon.Holes.Length)
                return false;
            for (int i = 0; i < _holes.Length; i++)
                if (!(_holes[i]).EqualsExact(otherPolygon.Holes[i], tolerance))
                    return false;
            return true;
        }

        /// <inheritdoc cref="Geometry.Apply(ICoordinateFilter)"/>
        public override void Apply(ICoordinateFilter filter)
        {
            _shell.Apply(filter);
            for (int i = 0; i < _holes.Length; i++)
                _holes[i].Apply(filter);
        }

        /// <inheritdoc cref="Geometry.Apply(ICoordinateSequenceFilter)"/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            _shell.Apply(filter);
            if (!filter.Done)
            {
                for (int i = 0; i < _holes.Length; i++)
                {
                    (_holes[i]).Apply(filter);
                    if (filter.Done)
                        break;
                }
            }
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <inheritdoc />
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _shell.Apply(filter);
            if (!filter.Done)
            {
                foreach (var hole in _holes)
                {
                    hole.Apply(filter);
                    if (filter.Done)
                    {
                        break;
                    }
                }
            }

            if (filter.GeometryChanged)
            {
                GeometryChanged();
            }
        }

        /// <inheritdoc cref="Geometry.Apply(IGeometryFilter)"/>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
        }

        /// <inheritdoc cref="Geometry.Apply(IGeometryComponentFilter)"/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            _shell.Apply(filter);
            for (int i = 0; i < _holes.Length; i++)
                _holes[i].Apply(filter);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()
        {
            var shellCopy = (LinearRing) _shell.Copy();
            var holesCopy = new LinearRing[_holes.Length];
            for (int i = 0; i < _holes.Length; i++)
                holesCopy[i] = (LinearRing) _holes[i].Copy();
            return new Polygon(shellCopy, holesCopy, Factory);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override Geometry ConvexHull()
        {
            return ExteriorRing.ConvexHull();
        }

        /// <summary>
        ///
        /// </summary>
        public override void Normalize()
        {
            _shell = Normalized(_shell, true);
            for (int i = 0; i < _holes.Length; i++)
                _holes[i] = Normalized(_holes[i], false);
            Array.Sort(_holes);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object o)
        {
            var poly = (Polygon)o;

            var thisShell = _shell;
            var otherShell = poly._shell;
            int shellComp = thisShell.CompareToSameClass(otherShell);
            if (shellComp != 0) return shellComp;

            int nHole1 = NumInteriorRings;
            int nHole2 = poly.NumInteriorRings;
            int i = 0;
            while (i < nHole1 && i < nHole2)
            {
                var thisHole = (LinearRing)GetInteriorRingN(i);
                var otherHole = (LinearRing)poly.GetInteriorRingN(i);
                int holeComp = thisHole.CompareToSameClass(otherHole);
                if (holeComp != 0) return holeComp;
                i++;
            }
            if (i < nHole1) return 1;
            if (i < nHole2) return -1;
            return 0;

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object other, IComparer<CoordinateSequence> comparer)
        {
            var poly = (Polygon)other;

            var thisShell = _shell;
            var otherShell = poly.Shell;
            int shellComp = thisShell.CompareToSameClass(otherShell, comparer);
            if (shellComp != 0) return shellComp;

            int nHole1 = NumInteriorRings;
            int nHole2 = poly.NumInteriorRings;
            int i = 0;
            while (i < nHole1 && i < nHole2)
            {
                var thisHole = (LinearRing)GetInteriorRingN(i);
                var otherHole = (LinearRing)poly.GetInteriorRingN(i);
                int holeComp = thisHole.CompareToSameClass(otherHole, comparer);
                if (holeComp != 0) return holeComp;
                i++;
            }
            if (i < nHole1) return 1;
            if (i < nHole2) return -1;
            return 0;
        }

        private static LinearRing Normalized(LinearRing ring, bool clockwise)
        {
            var res = (LinearRing)ring.Copy();
            Normalize(res, clockwise);
            return res;
        }
        
        private static void Normalize(LinearRing ring, bool clockwise)
        {
            if (ring.IsEmpty)
                return;

            var seq = ring.CoordinateSequence;
            int minCoordinateIndex = CoordinateSequences.MinCoordinateIndex(seq, 0, seq.Count - 2);
            CoordinateSequences.Scroll(seq, minCoordinateIndex, true);
            if (Orientation.IsCCW(seq) == clockwise)
                CoordinateSequences.Reverse(seq);
        }

        /// <summary>
        /// Tests whether this is a rectangular <see cref="Polygon"/>.
        /// </summary>
        /// <returns><c>true</c> if the geometry is a rectangle.</returns>
        public override bool IsRectangle
        {
            get
            {
                if (NumInteriorRings != 0) return false;
                if (Shell == null) return false;
                if (Shell.NumPoints != 5) return false;

                // check vertices have correct values
                var seq = Shell.CoordinateSequence;
                var env = EnvelopeInternal;
                for (int i = 0; i < 5; i++)
                {
                    double x = seq.GetX(i);
                    if (!(x == env.MinX || x == env.MaxX))
                        return false;

                    double y = seq.GetY(i);
                    if (!(y == env.MinY || y == env.MaxY))
                        return false;
                }

                // check vertices are in right order
                double prevX = seq.GetX(0);
                double prevY = seq.GetY(0);
                for (int i = 1; i <= 4; i++)
                {
                    double x = seq.GetX(i);
                    double y = seq.GetY(i);

                    bool xChanged = x != prevX;
                    bool yChanged = y != prevY;

                    if (xChanged == yChanged)
                        return false;

                    prevX = x;
                    prevY = y;
                }
                return true;
            }
        }

        /// <inheritdoc cref="Geometry.Reverse"/>
        [Obsolete("Call Geometry.Reverse()")]
#pragma warning disable 809
        public override Geometry Reverse()
        {
            return base.Reverse();
        }
#pragma warning restore 809

        /// <summary>
        /// The actual implementation of the <see cref="Geometry.Reverse"/> function for <c>POLYGON</c>s
        /// </summary>
        /// <returns>A reversed geometry</returns>
        protected override Geometry ReverseInternal()
        {
#pragma warning disable 618
            var shell = (LinearRing)ExteriorRing.Reverse();
            var holes = new LinearRing[NumInteriorRings];
            for (int i = 0; i < holes.Length; i++)
                holes[i] = (LinearRing)GetInteriorRingN(i).Reverse();
#pragma warning restore 618

            return Factory.CreatePolygon(shell, holes);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// polygon is to be created.
        /// </param>
        /// <param name="factory"></param>
        public Polygon(LinearRing shell, GeometryFactory factory) : this(shell, null, factory) { }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(LinearRing shell) : this(shell, null, DefaultFactory) { }

        /// <summary>
        ///
        /// </summary>
        public LinearRing Shell
        {
            get => _shell;
            private set => _shell = value;
        }

        /// <summary>
        ///
        /// </summary>
        public LinearRing[] Holes
        {
            get => _holes;
            private set => _holes = value;
        }

        /*END ADDED BY MPAUL42 */

    }

    public static class CoordinateSequenceEx
    {
        public static int GetHashCode(this CoordinateSequence sequence, int baseValue, Func<int, int> operation)
        {
            if (sequence!=null && sequence.Count > 0)
            {
                for (int i = 0; i < sequence.Count; i++)
                    baseValue = operation(baseValue) + sequence.GetX(i).GetHashCode();
            }
            return baseValue;
        }

    }
}
