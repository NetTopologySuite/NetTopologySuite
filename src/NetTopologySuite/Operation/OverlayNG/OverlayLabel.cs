using System;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// A structure recording the topological situation
    /// for an edge in a topology graph
    /// used during overlay processing.
    /// </summary>
    /// <remarks>
    /// A label contains the topological <see cref="Location"/> for
    /// one or two input geometries to an overlay operation.
    /// An input geometry may be either a Line or an Area.
    /// The label locations for each input geometry are populated
    /// with the <see cref="Location"/>
    /// for the edge <see cref="Position"/>s
    /// when they are created or once they are computed by topological evaluation.
    /// A label also records the(effective) dimension of each input geometry.
    /// For area edges the role(shell or hole)
    /// of the originating ring is recorded, to allow
    /// determination of edge handling in collapse cases.
    /// <para/>
    /// In an <see cref="OverlayGraph"/>
    /// a single label is shared between
    /// the two oppositely-oriented <see cref="OverlayEdge"/>s
    /// of a symmetric pair. 
    /// Accessors for orientation-sensitive information
    /// are parameterized by the orientation of the containing edge.
    /// <para/>
    /// For each input geometry (0 and 1), the label records
    /// that an edge is in one of the following states
    /// (identified by the<c>dim</c> field).
    /// Each state has additional information about the edge topology.
    /// <list type="bullet">
    /// <item><description>A <b>Boundary</b> edge of an Area (polygon)
    ///   <list type="bullet">
    ///     <item><description><c>dim</c> = DIM_BOUNDARY</description></item>
    ///     <item><description><c>locLeft, locRight</c> : the locations of the edge sides for the Area</description></item>
    ///         <item><description><c>locLine</c> : INTERIOR</description></item>
    ///     <item><description><c>isHole</c> : whether the edge is in a shell or a hole (the ring role)</description></item>
    ///   </list>
    ///   </description>
    /// </item>
    /// <item><description>A <b>Collapsed</b> edge of an input Area
    /// (formed by merging two or more parent edges)
    ///   <list type="bullet">
    ///     <item><description><c>dim</c> = DIM_COLLAPSE</description></item>
    ///     <item><description><c>locLine</c> : the location of the edge relative to the effective input Area
///                            (a collapsed spike is EXTERIOR, a collapsed gore or hole is INTERIOR)</description></item>
    ///     <item><description><c>isHole</c> : <c>true</c> if all parent edges are in holes;
    ///                                        <c>false</c> if some parent edge is in a shell</description></item>
    ///     
    ///   </list></description>
    /// </item>
    /// <item><description>A <b>Line</b> edge from an input line
    ///   <list type="bullet">
    ///   <item><description><c>dim</c> = DIM_LINE</description></item>
    ///   <item><description><c>locLine </c> : the location of the edge relative to the Line.
    ///   Initialized to LOC_UNKNOWN to simplify logic.</description></item>
    ///   </list></description>
    /// </item>
    /// <item><description>An edge which is <b>Not Part</b> of an input geometry
    /// (and thus must be part of the other geometry).
    ///   <list type="bullet">
    ///   <item><description><c>dim</c> = NOT_PART</description></item>
    ///   </list></description>
    /// </item>
    /// </list>
    /// Note that:
    /// <list type="bullet">
    /// <item><description>an edge cannot be both a Collapse edge and a Line edge in the same input geometry,
    /// because each input geometry must be homogeneous.</description></item>
    /// <item><description>an edge may be an Boundary edge in one input geometry
    /// and a Line or Collapse edge in the other input.</description></item>
    /// </list>
    /// </remarks>
    /// <author>Martin Davis</author>
    internal sealed class OverlayLabel
    {

        private const char SYM_UNKNOWN = '#';
        private const char SYM_BOUNDARY = 'B';
        private const char SYM_COLLAPSE = 'C';
        private const char SYM_LINE = 'L';

        /// <summary>
        /// The dimension of an input geometry which is not known.
        /// </summary>
        public const Dimension DIM_UNKNOWN = Geometries.Dimension.Unknown;

        /// <summary>
        /// The dimension of an edge which is not part of a specified input geometry.
        /// </summary>
        public const Dimension DIM_NOT_PART = DIM_UNKNOWN;

        /// <summary>
        /// The dimension of an edge which is a line.
        /// </summary>
        public const Dimension DIM_LINE = Geometries.Dimension.Curve;

        /// <summary>
        /// The dimension for an edge which is part of an input Area geometry boundary.
        /// </summary>
        public const Dimension DIM_BOUNDARY = Geometries.Dimension.Surface;

        /// <summary>
        /// The dimension for an edge which is a collapsed part of an input Area geometry boundary.
        /// A collapsed edge represents two or more line segments which have the same endpoints.
        /// They usually are caused by edges in valid polygonal geometries
        /// having their endpoints become identical due to precision reduction.
        /// </summary>
        public const Dimension DIM_COLLAPSE = Geometries.Dimension.Collapse;

        /// <summary>
        /// Indicates that the location is currently unknown
        /// </summary>
        public const Location LOC_UNKNOWN = Location.Null;


        private Dimension _aDim = DIM_NOT_PART;
        private bool _aIsHole;
        private Location _aLocLeft = LOC_UNKNOWN;
        private Location _aLocRight = LOC_UNKNOWN;
        private Location _aLocLine = LOC_UNKNOWN;

        private Dimension _bDim = DIM_NOT_PART;
        private bool _bIsHole;
        private Location _bLocLeft = LOC_UNKNOWN;
        private Location _bLocRight = LOC_UNKNOWN;
        private Location _bLocLine = LOC_UNKNOWN;

        /// <summary>
        /// Creates a label for an Area edge
        /// </summary>
        /// <param name="index">The input index of the parent geometry</param>
        /// <param name="locLeft">The location of the left side of the edge</param>
        /// <param name="locRight">The location of the right side of the edge</param>
        /// <param name="isHole">Whether the edge role is a hole or a shell</param>
        public OverlayLabel(int index, Location locLeft, Location locRight, bool isHole)
        {
            InitBoundary(index, locLeft, locRight, isHole);
        }

        /// <summary>
        /// Creates a label for a Line edge
        /// </summary>
        /// <param name="index">The input index of the parent geometry</param>
        public OverlayLabel(int index)
        {
            InitLine(index);
        }

        /// <summary>
        /// Creates an uninitialized label
        /// </summary>
        public OverlayLabel()
        {
        }

        /// <summary>
        /// Creates a label which is a copy of another label.
        /// </summary>
        /// <param name="lbl">The template label</param>
        public OverlayLabel(OverlayLabel lbl)
        {
            _aLocLeft = lbl._aLocLeft;
            _aLocRight = lbl._aLocRight;
            _aLocLine = lbl._aLocLine;
            _aDim = lbl._aDim;
            _aIsHole = lbl._aIsHole;

            _bLocLeft = lbl._bLocLeft;
            _bLocRight = lbl._bLocRight;
            _bLocLine = lbl._bLocLine;
            _bDim = lbl._bDim;
            _bIsHole = lbl._bIsHole;
        }

        /// <summary>
        /// Gets the effective dimension of the given input geometry.
        /// </summary>
        /// <param name="index">The input index of the parent geometry</param>
        /// <returns>The dimension</returns>
        /// <seealso cref="DIM_UNKNOWN"/>
        /// <seealso cref="DIM_NOT_PART"/>
        /// <seealso cref="DIM_LINE"/>
        /// <seealso cref="DIM_BOUNDARY"/>
        /// <seealso cref="DIM_COLLAPSE"/>
        public Dimension Dimension(int index)
        {
            if (index == 0)
                return _aDim;
            return _bDim;
        }

        /// <summary>
        /// Initializes the label for an input geometry which is an Area boundary.
        /// </summary>
        /// <param name="index">The input index of the parent geometry</param>
        /// <param name="locLeft">The location of the left side of the edge</param>
        /// <param name="locRight">The location of the right side of the edge</param>
        /// <param name="isHole">Whether the edge role is a hole or a shell</param>
        public void InitBoundary(int index, Location locLeft, Location locRight, bool isHole)
        {
            if (index == 0)
            {
                _aDim = DIM_BOUNDARY;
                _aIsHole = isHole;
                _aLocLeft = locLeft;
                _aLocRight = locRight;
                _aLocLine = Location.Interior;
            }
            else
            {
                _bDim = DIM_BOUNDARY;
                _bIsHole = isHole;
                _bLocLeft = locLeft;
                _bLocRight = locRight;
                _bLocLine = Location.Interior;
            }
        }

        /// <summary>
        /// Initializes the label for an edge which is the collapse of
        /// part of the boundary of an Area input geometry.
        /// <para/>
        /// The location of the collapsed edge relative to the
        /// parent area geometry is initially unknown.
        /// It must be determined from the topology of the overlay graph
        /// </summary>
        /// <param name="index">The index of the parent input geometry</param>
        /// <param name="isHole">Whether the dominant edge role is a hole or a shell</param>
        public void InitCollapse(int index, bool isHole)
        {
            if (index == 0)
            {
                _aDim = DIM_COLLAPSE;
                _aIsHole = isHole;
            }
            else
            {
                _bDim = DIM_COLLAPSE;
                _bIsHole = isHole;
            }
        }

        /// <summary>
        /// Initializes the label for an input geometry which is a Line.
        /// </summary>
        /// <param name="index">The index of the parent input geometry</param>
        public void InitLine(int index)
        {
            if (index == 0)
            {
                _aDim = DIM_LINE;
                _aLocLine = LOC_UNKNOWN;
            }
            else
            {
                _bDim = DIM_LINE;
                _bLocLine = LOC_UNKNOWN;
            }
        }

        /// <summary>
        /// Initializes the label for an edge which is not part of an input geometry.
        /// </summary>
        /// <param name="index">The index of the parent input geometry</param>
        public void InitNotPart(int index)
        {
            // this assumes locations are initialized to UNKNOWN
            if (index == 0)
            {
                _aDim = DIM_NOT_PART;
            }
            else
            {
                _bDim = DIM_NOT_PART;
            }
        }

        /// <summary>
        /// Sets the line location.
        /// <br/>
        /// This is used to set the locations for linear edges 
        /// encountered during area label propagation.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <param name="loc">Location to set</param>
        public void SetLocationLine(int index, Location loc)
        {
            if (index == 0)
            {
                _aLocLine = loc;
            }
            else
            {
                _bLocLine = loc;
            }
        }

        /// <summary>
        /// Sets the location of all postions for a given input.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <param name="loc">The location to set</param>
        public void SetLocationAll(int index, Location loc)
        {
            if (index == 0)
            {
                _aLocLine = loc;
                _aLocLeft = loc;
                _aLocRight = loc;
            }
            else
            {
                _bLocLine = loc;
                _bLocLeft = loc;
                _bLocRight = loc;
            }
        }

        /// <summary>
        /// Sets the location for a collapsed edge (the Line position)
        /// for an input geometry,
        /// depending on the ring role recorded in the label.
        /// If the input geometry edge is from a shell,
        /// the location is <see cref="Location.Exterior"/>, if it is a hole
        /// it is <see cref="Location.Interior"/>.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        public void SetLocationCollapse(int index)
        {
            var loc = IsHole(index) ? Location.Interior : Location.Exterior;
            if (index == 0)
            {
                _aLocLine = loc;
            }
            else
            {
                _bLocLine = loc;
            }
        }

        /// <summary>
        /// Tests whether at least one of the sources is a Line.
        /// </summary>
        /// <value><c>true</c> if at least one source is a line</value>
        public bool IsLine
        {
            get => _aDim == DIM_LINE || _bDim == DIM_LINE;
        }

        /// <summary>
        /// Tests whether a source is a Line.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the input is a Line</returns>
        public bool IsLineAt(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_LINE;
            }
            return _bDim == DIM_LINE;
        }

        /// <summary>
        /// Tests whether an edge is linear (a Line or a Collapse) in an input geometry.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the edge is linear</returns>
        public bool IsLinear(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_LINE || _aDim == DIM_COLLAPSE;
            }
            return _bDim == DIM_LINE || _bDim == DIM_COLLAPSE;
        }

        /// <summary>
        /// Tests whether the source of a label is known.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the source is known</returns>
        public bool IsKnown(int index)
        {
            if (index == 0)
            {
                return _aDim != DIM_UNKNOWN;
            }
            return _bDim != DIM_UNKNOWN;
        }

        /// <summary>
        /// Tests whether a label is for an edge which is not part
        /// of a given input geometry.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the edge is not part of the geometry</returns>
        public bool IsNotPart(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_NOT_PART;
            }
            return _bDim == DIM_NOT_PART;
        }

        /// <summary>
        /// Gets a value indicating if a label is for an edge which is in the boundary of either source geometry.
        /// </summary>
        /// <returns><c>true</c> if the label is a boundary for either source</returns>
        public bool IsBoundaryEither
        {
            get => _aDim == DIM_BOUNDARY || _bDim == DIM_BOUNDARY;
        }

        /// <summary>
        /// Gets a value indicating if a label is for an edge which is in the boundary of both source geometries.
        /// </summary>
        /// <returns><c>true</c> if the label is a boundary for both sources</returns>
        public bool IsBoundaryBoth
        {
            get => _aDim == DIM_BOUNDARY && _bDim == DIM_BOUNDARY;
        }

        /// <summary>
        /// Tests if the label is a collapsed edge of one area  
        /// and is a(non-collapsed) boundary edge of the other area.
        /// </summary>
        /// <value><c>true</c> if the label is for a collapse coincident with a boundary</value>
        public bool IsBoundaryCollapse
        {
            get
            {
                if (IsLine) return false;
                return !IsBoundaryBoth;
            }
        }

        /// <summary>
        /// Tests if a label is for an edge where two
        /// area touch along their boundary.
        /// </summary>
        /// <returns><c>true</c> if the edge is a boundary touch</returns>
        public bool IsBoundaryTouch
        {
            get => IsBoundaryBoth
                   && GetLocation(0, Position.Right, true) != GetLocation(1, Position.Right, true);
        }

        /// <summary>
        /// Tests if a label is for an edge which is in the boundary of a source geometry.
        /// Collapses are not reported as being in the boundary.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the label is a boundary for the source</returns>
        public bool IsBoundary(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_BOUNDARY;
            }
            return _bDim == DIM_BOUNDARY;
        }

        /// <summary>
        /// Tests whether a label is for an edge which is a boundary of one geometry
        /// and not part of the other.
        /// </summary>
        /// <returns><c>true</c> if the edge is a boundary singleton</returns>
        public bool IsBoundarySingleton 
        {
            get
            {
                if (_aDim == DIM_BOUNDARY && _bDim == DIM_NOT_PART) return true;
                if (_bDim == DIM_BOUNDARY && _aDim == DIM_NOT_PART) return true;
                return false;
            }
        }
        /// <summary>
        /// Tests if the line location for a source is unknown.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the line location is unknown</returns>
        public bool IsLineLocationUnknown(int index)
        {
            if (index == 0)
            {
                return _aLocLine == LOC_UNKNOWN;
            }
            else
            {
                return _bLocLine == LOC_UNKNOWN;
            }
        }

        /// <summary>
        /// Tests if a line edge is inside a source geometry
        /// (i.e.it has location <see cref="Location.Interior"/>).
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the line is inside the source geometry</returns>
        public bool IsLineInArea(int index)
        {
            if (index == 0)
            {
                return _aLocLine == Location.Interior;
            }
            return _bLocLine == Location.Interior;
        }

        /// <summary>
        /// Tests if the ring role of an edge is a hole.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the ring role is a hole</returns>
        public bool IsHole(int index)
        {
            if (index == 0)
            {
                return _aIsHole;
            }
            else
            {
                return _bIsHole;
            }
        }

        /// <summary>
        /// Tests if an edge is a Collapse for a source geometry.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if the label indicates the edge is a collapse for the source</returns>
        public bool IsCollapse(int index)
        {
            return Dimension(index) == DIM_COLLAPSE;
        }

        /// <summary>
        /// Tests if a label is a Collapse has location {@link Location#INTERIOR},
        /// to at least one source geometry.
        /// </summary>
        /// <returns><c>true</c> if the label is an Interior Collapse to a source geometry</returns>
        public bool IsInteriorCollapse
        {
            get
            {
                if (_aDim == DIM_COLLAPSE && _aLocLine == Location.Interior) return true;
                if (_bDim == DIM_COLLAPSE && _bLocLine == Location.Interior) return true;
                return false;
            }
        }

        /// <summary>
        /// Tests if a label is a Collapse
        /// and NotPart with location {@link Location#INTERIOR} for the other geometry.
        /// </summary>
        /// <returns><c>true</c> if the label is a Collapse and a NotPart with Location Interior</returns>
        public bool IsCollapseAndNotPartInterior
        {
            get
            {
                if (_aDim == DIM_COLLAPSE && _bDim == DIM_NOT_PART && _bLocLine == Location.Interior) return true;
                if (_bDim == DIM_COLLAPSE && _aDim == DIM_NOT_PART && _aLocLine == Location.Interior) return true;
                return false;
            }
        }

        /// <summary>
        /// Gets the line location for a source geometry.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns>The line location for the source</returns>
        public Location GetLineLocation(int index)
        {
            if (index == 0)
            {
                return _aLocLine;
            }
            else
            {
                return _bLocLine;
            }
        }

        /// <summary>
        /// Tests if a line is in the interior of a source geometry.
        /// </summary>
        /// <param name="index">The index of the source geometry</param>
        /// <returns><c>true</c> if the label is a line and is interior</returns>
        public bool IsLineInterior(int index)
        {
            if (index == 0)
            {
                return _aLocLine == Location.Interior;
            }
            return _bLocLine == Location.Interior;
        }

        /// <summary>
        /// Gets the location for a <see cref="Position"/> of an edge of a source
        /// for an edge with given orientation.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <param name="position">The position to get the location for</param>
        /// <param name="isForward"><c>true</c> if the orientation of the containing edge is forward</param>
        /// <returns>The location of the oriented position in the source</returns>
        public Location GetLocation(int index, Position position, bool isForward)
        {
            if (index == 0)
            {
                switch (position.Index)
                {
                    case Position.IndexLeft: return isForward ? _aLocLeft : _aLocRight;
                    case Position.IndexRight: return isForward ? _aLocRight : _aLocLeft;
                    case Position.IndexOn: return _aLocLine;
                }
            }
            // index == 1
            switch (position)
            {
                case Position.IndexLeft: return isForward ? _bLocLeft : _bLocRight;
                case Position.IndexRight: return isForward ? _bLocRight : _bLocLeft;
                case Position.IndexOn: return _bLocLine;
            }
            return LOC_UNKNOWN;
        }

        /// <summary>
        /// Gets the location for this label for either
        /// a Boundary or a Line edge.
        /// This supports a simple determination of
        /// whether the edge should be included as a result edge.
        /// </summary>
        /// <param name="index">The source index</param>
        /// <param name="position">The position for a boundary label</param>
        /// <param name="isForward">The direction for a boundary label</param>
        /// <returns>The location for the specified position</returns>
        public Location GetLocationBoundaryOrLine(int index, Position position, bool isForward)
        {
            if (IsBoundary(index))
            {
                return GetLocation(index, position, isForward);
            }
            return GetLineLocation(index);
        }

        /// <summary>
        /// Gets the linear location for the given source.
        /// </summary>
        /// <param name="index">The source geometry index</param>
        /// <returns>The linear location for the source</returns>
        public Location GetLocation(int index)
        {
            if (index == 0)
            {
                return _aLocLine;
            }
            return _bLocLine;
        }

        /// <summary>
        /// Tests whether this label has side position information
        /// for a source geometry.
        /// </summary>
        /// <param name="index">The index of the input geometry</param>
        /// <returns><c>true</c> if at least one side position is known</returns>
        public bool HasSides(int index)
        {
            if (index == 0)
            {
                return _aLocLeft != LOC_UNKNOWN
                    || _aLocRight != LOC_UNKNOWN;
            }
            return _bLocLeft != LOC_UNKNOWN
                || _bLocRight != LOC_UNKNOWN;
        }

        /// <summary>
        /// Creates a copy of this label
        /// </summary>
        /// <returns>A copy of this label</returns>
        public OverlayLabel Copy()
        {
            return new OverlayLabel(this);
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool isForward)
        {
            var buf = new StringBuilder();
            buf.Append("A:");
            buf.Append(LocationString(0, isForward));
            buf.Append("/B:");
            buf.Append(LocationString(1, isForward));
            return buf.ToString();
        }

        private string LocationString(int index, bool isForward)
        {
            var buf = new StringBuilder();
            if (IsBoundary(index))
            {
                buf.Append(LocationUtility.ToLocationSymbol(GetLocation(index, Position.Left, isForward)));
                buf.Append(LocationUtility.ToLocationSymbol(GetLocation(index, Position.Right, isForward)));
            }
            else
            {
                // is a linear edge
                buf.Append(LocationUtility.ToLocationSymbol(index == 0 ? _aLocLine : _bLocLine));
            }
            if (IsKnown(index))
                buf.Append(DimensionSymbol(index == 0 ? _aDim : _bDim));
            if (IsCollapse(index))
            {
                buf.Append(RingRoleSymbol(index == 0 ? _aIsHole : _bIsHole));
            }
            return buf.ToString();
        }

        /// <summary>
        /// Gets a symbol for the a ring role (Shell or Hole).
        /// </summary>
        /// <param name="isHole"><c>true</c> for a hole, <c>false</c> for a shell</param>
        /// <returns>The ring role symbol character</returns>
        public static char RingRoleSymbol(bool isHole)
        {
            return isHole ? 'h' : 's';
        }

        /// <summary>
        /// Gets the symbol for the dimension code of an edge.
        /// </summary>
        /// <param name="dim">The dimension code</param>
        /// <returns>The dimension symbol character</returns>
        public static char DimensionSymbol(Dimension dim)
        {
            switch (dim)
            {
                case DIM_LINE: return SYM_LINE;
                case DIM_COLLAPSE: return SYM_COLLAPSE;
                case DIM_BOUNDARY: return SYM_BOUNDARY;
            }
            return SYM_UNKNOWN;
        }


    }
}
