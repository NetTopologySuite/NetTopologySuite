using System;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * A label for a pair of {@link OverlayEdge}s which records
     * the topological information for the edge
     * in the {@link OverlayGraph} containing it.
     * The label is shared between both OverlayEdges
     * of a symmetric pair. 
     * Accessors for orientation-sensitive information
     * require the orientation of the containing OverlayEdge.
     * <p>
     * A label contains the topological {@link Location}s for 
     * the two overlay input geometries.
     * A labelled input geometry may be either a Line or an Area.
     * In both cases, the label locations are populated
     * with the locations for the edge {@link Position}s
     * once they are computed by topological evaluation.
     * The label also records the dimension of each geometry,
     * and in the case of area boundary edges, the role
     * of the originating ring (which allows
     * determination of the edge role in collapse cases).
     * <p>
     * For each input geometry, the label indicates that an edge is in one of the following states
     * (denoted by the <c>dim</c> field).
     * Each state has some additional information about the edge.
     * <ul>
     * <li>A <b>Boundary</b> edge of an input Area (polygon)
     *   <ul>
     *   <li><c>dim</c> = DIM_BOUNDARY</li>
     *   <li><c>locLeft, locRight</c> : the locations of the edge sides for the input Area</li>
     *   <li><c>isHole</c> : whether the 
     * edge was in a shell or a hole</li>
     *   </ul>
     * </li>
     * <li>A <b>Collapsed</b> edge of an input Area 
     * (which had two or more parent edges)
     *   <ul>
     *   <li><c>dim</c> = DIM_COLLAPSE</li>
     *   <li><c>locLine</c> : the location of the 
     * edge relative to the input Area</li>
     *   <li><c>isHole</c> : whether some 
     * contributing edge was in a shell (<c>false</c>), 
     * or otherwise that all were in holes</li> (<c>true</c>)
     *   </ul>
     * </li>
     * <li>An edge from an input <b>Line</b>
     *   <ul>
     *   <li><c>dim</c> = DIM_LINE</li>
     *   <li><c>locLine</c> : initialized to LOC_UNKNOWN, 
     *          to simplify logic.</li>
     *   </ul>
     * </li>
     * <li>An edge which is <b>Not Part</b> of an input geometry
     * (and thus must be part of the other geometry).
     *   <ul>
     *   <li><c>dim</c> = NOT_PART</li>
     *   </ul>
     * </li>
     * </ul>
     * Note that:
     * <ul>
     * <li>an edge cannot be both a Collapse edge and a Line edge in the same input geometry, 
     * because each input geometry must be homogeneous.
     * <li>an edge may be an Boundary edge in one input geometry 
     * and a Line or Collapse edge in the other input.
     * </ul>
     * 
     * @author Martin Davis
     *
     */
    /// <summary>
    /// A label for a pair of <see cref="OverlayEdge"/>s which records
    /// the topological information for the edge
    /// in the <see cref="OverlayGraph"/>containing it.
    /// The label is shared between both OverlayEdges
    /// of a symmetric pair. 
    /// Accessors for orientation-sensitive information
    /// require the orientation of the containing OverlayEdge.
    /// <para/>
    /// A label contains the topological <see cref="Location"/>s for 
    /// the two overlay input geometries.
    /// A labelled input geometry may be either a Line or an Area.
    /// In both cases, the label locations are populated
    /// with the locations for the edge <see cref="Position"/>s
    /// once they are computed by topological evaluation.
    /// The label also records the dimension of each geometry,
    /// and in the case of area boundary edges, the role
    /// of the originating ring (which allows
    /// determination of the edge role in collapse cases).
    /// <para/>
    /// For each input geometry, the label indicates that an edge is in one of the following states
    /// (denoted by the<c>dim</c> field).
    /// Each state has some additional information about the edge.
    /// <list type="bullet">
    /// <item><description>A <b>Boundary</b> edge of an input Area (polygon)
    ///   <list type="bullet">
    ///     <item><description><c>dim</c> = DIM_BOUNDARY</description></item>
    ///     <item><description><c>locLeft, locRight</c> : the locations of the edge sides for the input Area</description></item>
    ///     <item><description><c>isHole</c> : whether the edge was in a shell or a hole</description></item>
    ///   </list>
    ///   </description>
    /// </item>
    /// <item><description>A <b>Collapsed</b> edge of an input Area
    /// (which had two or more parent edges)
    ///   <list type="bullet">
    ///     <item><description><c>dim</c> = DIM_COLLAPSE</description></item>
    ///     <item><description><c>locLine</c> : the location of the edge relative to the input Area</description></item>
    ///     <item><description><c>isHole</c> : whether some contributing edge was in a shell(<c>false</c>), or otherwise that all were in holes (<c>true</c>)</description></item>
    ///     
    ///   </list></description>
    /// </item>
    /// <item><description>An edge from an input <b>Line</b>
    ///   <list type="bullet">
    ///   <item><description><c>dim</c> = DIM_LINE</description></item>
    ///   <item><description><c>locLine </c> : initialized to LOC_UNKNOWN, to simplify logic.</description></item>
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
    /// </summary>
    /// <author>Martin Davis</author>
    internal class OverlayLabel
    {

        private const char SYM_UNKNOWN = '#';
        private const char SYM_BOUNDARY = 'B';
        private const char SYM_COLLAPSE = 'C';
        private const char SYM_LINE = 'L';

        public const Dimension DIM_UNKNOWN = Geometries.Dimension.Unknown;
        public const Dimension DIM_NOT_PART = DIM_UNKNOWN;
        public const Dimension DIM_LINE = Geometries.Dimension.Curve;
        public const Dimension DIM_BOUNDARY = Geometries.Dimension.Surface;
        public const Dimension DIM_COLLAPSE = Geometries.Dimension.Collapse;

        /// <summary>Indicates that the location is currently unknown</summary>
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


        public OverlayLabel(int index, Location locLeft, Location locRight, bool isHole)
        {
            InitBoundary(index, locLeft, locRight, isHole);
        }

        public OverlayLabel(int index)
        {
            InitLine(index);
        }

        public OverlayLabel()
        {
        }

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

        public Dimension Dimension(int index)
        {
            if (index == 0)
                return _aDim;
            return _bDim;
        }

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

        /*
        public void initAsLine(int index, int locInArea) {
          int loc = normalizeLocation(locInArea);
          if (index == 0) {
            aDim = DIM_LINE;
            aLocLine = loc;
          }
          else {
            bDim = DIM_LINE;
            bLocLine = loc;
          }
        }
        */

        /*
         // Not needed so far
        public void setToNonPart(int index, int locInArea) {
          int loc = normalizeLocation(locInArea);
          if (index == 0) {
            aDim = DIM_NOT_PART;
            aLocInArea = loc;
            aLocLeft = loc;
            aLocRight = loc;
          }
          else {
            bDim = DIM_NOT_PART;
            bLocInArea = loc;
            aLocLeft = loc;
            aLocRight = loc;
          }
        }
        */

        /// <summary>
        /// Sets the line location.
        /// <br/>
        /// This is used to set the locations for linear edges 
        /// encountered during area label propagation.
        /// </summary>
        /// <param name="index">Source to update</param>
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

        public bool IsLineAt(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_LINE;
            }
            return _bDim == DIM_LINE;
        }

        public bool IsLinear(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_LINE || _aDim == DIM_COLLAPSE;
            }
            return _bDim == DIM_LINE || _bDim == DIM_COLLAPSE;
        }

        public bool IsKnown(int index)
        {
            if (index == 0)
            {
                return _aDim != DIM_UNKNOWN;
            }
            return _bDim != DIM_UNKNOWN;
        }

        public bool IsNotPart(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_NOT_PART;
            }
            return _bDim == DIM_NOT_PART;
        }

        public bool IsBoundaryEither
        {
            get => _aDim == DIM_BOUNDARY || _bDim == DIM_BOUNDARY;
        }

        public bool IsBoundaryBoth
        {
            get => _aDim == DIM_BOUNDARY && _bDim == DIM_BOUNDARY;
        }

        /// <summary>
        /// Tests if the label is for a collapsed
        /// edge of an area 
        /// which is coincident with the boundary of the other area.
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

        public bool IsBoundary(int index)
        {
            if (index == 0)
            {
                return _aDim == DIM_BOUNDARY;
            }
            return _bDim == DIM_BOUNDARY;
        }

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
        /// Tests if a line edge is inside 
        /// </summary>
        public bool IsLineInArea(int index)
        {
            if (index == 0)
            {
                return _aLocLine == Location.Interior;
            }
            return _bLocLine == Location.Interior;
        }

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

        public bool IsCollapse(int index)
        {
            return Dimension(index) == DIM_COLLAPSE;
        }

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
        /// <param name="index">Source index</param>
        /// <returns><c>true</c> if the label is a line and is interior</returns>
        public bool IsLineInterior(int index)
        {
            if (index == 0)
            {
                return _aLocLine == Location.Interior;
            }
            return _bLocLine == Location.Interior;
        }

        [Obsolete("Use GetLocation(int, Geometries.Position")]
        public Location GetLocation(int index, Positions position, bool isForward)
            => GetLocation(index, (Position) position, isForward);

        public Location GetLocation(int index, Position position, bool isForward)
        {
            if (index == 0)
            {
                switch (position)
                {
                    case Position.Left: return isForward ? _aLocLeft : _aLocRight;
                    case Position.Right: return isForward ? _aLocRight : _aLocLeft;
                    case Position.On: return _aLocLine;
                }
            }
            switch (position)
            {
                case Position.Left: return isForward ? _bLocLeft : _bLocRight;
                case Position.Right: return isForward ? _bLocRight : _bLocLeft;
                case Position.On: return _bLocLine;
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
        [Obsolete("Use GetLocationBoundaryOrLine(int, Geometries.Position, bool)")]
        public Location GetLocationBoundaryOrLine(int index, Positions position, bool isForward)
            => GetLocationBoundaryOrLine(index, (Position) position, isForward);

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
        /// <param name="index">The source index</param>
        /// <returns>The linear location for the source</returns>
        public Location GetLocation(int index)
        {
            if (index == 0)
            {
                return _aLocLine;
            }
            return _bLocLine;
        }

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

        public OverlayLabel Copy()
        {
            return new OverlayLabel(this);
        }

        public OverlayLabel CopyFlip()
        {
            var lbl = new OverlayLabel();

            lbl._aLocLeft = _aLocRight;
            lbl._aLocRight = _aLocLeft;
            lbl._aLocLine = _aLocLine;
            lbl._aDim = _aDim;

            lbl._bLocLeft = _bLocRight;
            lbl._bLocRight = _bLocLeft;
            lbl._bLocLine = _bLocLine;
            lbl._bDim = _bDim;

            return lbl;
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

        public static char RingRoleSymbol(bool isHole)
        {
            return isHole ? 'h' : 's';
        }

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
