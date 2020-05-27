using System;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

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
     * (denoted by the <code>dim</code> field).
     * Each state has some additional information about the edge.
     * <ul>
     * <li>A <b>Boundary</b> edge of an input Area (polygon)
     *   <ul>
     *   <li><code>dim</code> = DIM_BOUNDARY</li>
     *   <li><code>locLeft, locRight</code> : the locations of the edge sides for the input Area</li>
     *   <li><code>isHole</code> : whether the 
     * edge was in a shell or a hole</li>
     *   </ul>
     * </li>
     * <li>A <b>Collapsed</b> edge of an input Area 
     * (which had two or more parent edges)
     *   <ul>
     *   <li><code>dim</code> = DIM_COLLAPSE</li>
     *   <li><code>locLine</code> : the location of the 
     * edge relative to the input Area</li>
     *   <li><code>isHole</code> : whether some 
     * contributing edge was in a shell (<code>false</code>), 
     * or otherwise that all were in holes</li> (<code>true</code>)
     *   </ul>
     * </li>
     * <li>An edge from an input <b>Line</b>
     *   <ul>
     *   <li><code>dim</code> = DIM_LINE</li>
     *   <li><code>locLine</code> : initialized to LOC_UNKNOWN, 
     *          to simplify logic.</li>
     *   </ul>
     * </li>
     * <li>An edge which is <b>Not Part</b> of an input geometry
     * (and thus must be part of the other geometry).
     *   <ul>
     *   <li><code>dim</code> = NOT_PART</li>
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
    class OverlayLabel
    {

        private const char SYM_UNKNOWN = '#';
        private const char SYM_BOUNDARY = 'B';
        private const char SYM_COLLAPSE = 'C';
        private const char SYM_LINE = 'L';

        public const Dimension DIM_UNKNOWN = Dimension.Unknown;
        public const Dimension DIM_NOT_PART = DIM_UNKNOWN;
        public const Dimension DIM_LINE = Dimension.Curve;
        public const Dimension DIM_BOUNDARY = Dimension.Surface;
        public const Dimension DIM_COLLAPSE = Dimension.Collapse;

        /**
         * Indicates that the location is currently unknown
         */
        public static Location LOC_UNKNOWN = Location.Null;


        private Dimension aDim = DIM_NOT_PART;
        private bool aIsHole;
        private Location aLocLeft = LOC_UNKNOWN;
        private Location aLocRight = LOC_UNKNOWN;
        private Location aLocLine = LOC_UNKNOWN;

        private Dimension bDim = DIM_NOT_PART;
        private bool bIsHole;
        private Location bLocLeft = LOC_UNKNOWN;
        private Location bLocRight = LOC_UNKNOWN;
        private Location bLocLine = LOC_UNKNOWN;


        public OverlayLabel(int index, Location locLeft, Location locRight, bool isHole)
        {
            initBoundary(index, locLeft, locRight, isHole);
        }

        public OverlayLabel(int index)
        {
            initLine(index);
        }

        public OverlayLabel()
        {
        }

        public OverlayLabel(OverlayLabel lbl)
        {
            this.aLocLeft = lbl.aLocLeft;
            this.aLocRight = lbl.aLocRight;
            this.aLocLine = lbl.aLocLine;
            this.aDim = lbl.aDim;
            this.aIsHole = lbl.aIsHole;

            this.bLocLeft = lbl.bLocLeft;
            this.bLocRight = lbl.bLocRight;
            this.bLocLine = lbl.bLocLine;
            this.bDim = lbl.bDim;
            this.bIsHole = lbl.bIsHole;
        }

        public Dimension dimension(int index)
        {
            if (index == 0)
                return aDim;
            return bDim;
        }

        public void initBoundary(int index, Location locLeft, Location locRight, bool isHole)
        {
            if (index == 0)
            {
                aDim = DIM_BOUNDARY;
                aIsHole = isHole;
                aLocLeft = locLeft;
                aLocRight = locRight;
                aLocLine = Location.Interior;
            }
            else
            {
                bDim = DIM_BOUNDARY;
                bIsHole = isHole;
                bLocLeft = locLeft;
                bLocRight = locRight;
                bLocLine = Location.Interior;
            }
        }

        public void initCollapse(int index, bool isHole)
        {
            if (index == 0)
            {
                aDim = DIM_COLLAPSE;
                aIsHole = isHole;
            }
            else
            {
                bDim = DIM_COLLAPSE;
                bIsHole = isHole;
            }
        }

        public void initLine(int index)
        {
            if (index == 0)
            {
                aDim = DIM_LINE;
                aLocLine = LOC_UNKNOWN;
            }
            else
            {
                bDim = DIM_LINE;
                bLocLine = LOC_UNKNOWN;
            }
        }

        public void initNotPart(int index)
        {
            // this assumes locations are initialized to UNKNOWN
            if (index == 0)
            {
                aDim = DIM_NOT_PART;
            }
            else
            {
                bDim = DIM_NOT_PART;
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

        /**
         * Sets the line location.
         * 
         * This is used to set the locations for linear edges 
         * encountered during area label propagation.
         * 
         * @param index source to update
         * @param loc location to set
         */
        public void setLocationLine(int index, Location loc)
        {
            if (index == 0)
            {
                aLocLine = loc;
            }
            else
            {
                bLocLine = loc;
            }
        }

        public void setLocationAll(int index, Location loc)
        {
            if (index == 0)
            {
                aLocLine = loc;
                aLocLeft = loc;
                aLocRight = loc;
            }
            else
            {
                bLocLine = loc;
                bLocLeft = loc;
                bLocRight = loc;
            }
        }

        public void setLocationCollapse(int index)
        {
            var loc = isHole(index) ? Location.Interior : Location.Exterior;
            if (index == 0)
            {
                aLocLine = loc;
            }
            else
            {
                bLocLine = loc;
            }
        }

        /**
         * Tests whether at least one of the sources is a Line.
         * 
         * @return true if at least one source is a line
         */
        public bool isLine()
        {
            return aDim == DIM_LINE || bDim == DIM_LINE;
        }

        public bool isLine(int index)
        {
            if (index == 0)
            {
                return aDim == DIM_LINE;
            }
            return bDim == DIM_LINE;
        }

        public bool isLinear(int index)
        {
            if (index == 0)
            {
                return aDim == DIM_LINE || aDim == DIM_COLLAPSE;
            }
            return bDim == DIM_LINE || bDim == DIM_COLLAPSE;
        }

        public bool isKnown(int index)
        {
            if (index == 0)
            {
                return aDim != DIM_UNKNOWN;
            }
            return bDim != DIM_UNKNOWN;
        }

        public bool isNotPart(int index)
        {
            if (index == 0)
            {
                return aDim == DIM_NOT_PART;
            }
            return bDim == DIM_NOT_PART;
        }

        public bool isBoundaryEither()
        {
            return aDim == DIM_BOUNDARY || bDim == DIM_BOUNDARY;
        }

        public bool isBoundaryBoth()
        {
            return aDim == DIM_BOUNDARY && bDim == DIM_BOUNDARY;
        }

        /**
         * Tests if the label is for a collapsed
         * edge of an area 
         * which is coincident with the boundary of the other area.
         * 
         * @return true if the label is for a collapse coincident with a boundary
         */
        public bool isBoundaryCollapse()
        {
            if (isLine()) return false;
            return !isBoundaryBoth();
        }

        public bool isBoundary(int index)
        {
            if (index == 0)
            {
                return aDim == DIM_BOUNDARY;
            }
            return bDim == DIM_BOUNDARY;
        }

        public bool isLineLocationUnknown(int index)
        {
            if (index == 0)
            {
                return aLocLine == LOC_UNKNOWN;
            }
            else
            {
                return bLocLine == LOC_UNKNOWN;
            }
        }

        /**
         * Tests if a line edge is inside 
         * @param index
         * @return
         */
        public bool isLineInArea(int index)
        {
            if (index == 0)
            {
                return aLocLine == Location.Interior;
            }
            return bLocLine == Location.Interior;
        }

        public bool isHole(int index)
        {
            if (index == 0)
            {
                return aIsHole;
            }
            else
            {
                return bIsHole;
            }
        }

        public bool isCollapse(int index)
        {
            return dimension(index) == DIM_COLLAPSE;
        }

        public Location getLineLocation(int index)
        {
            if (index == 0)
            {
                return aLocLine;
            }
            else
            {
                return bLocLine;
            }
        }

        /**
         * Tests if a line is in the interior of a source geometry.
         * 
         * @param index source geometry
         * @return true if the label is a line and is interior
         */
        public bool isLineInterior(int index)
        {
            if (index == 0)
            {
                return aLocLine == Location.Interior;
            }
            return bLocLine == Location.Interior;
        }

        public Location getLocation(int index, Positions position, bool isForward)
        {
            if (index == 0)
            {
                switch (position)
                {
                    case Positions.Left: return isForward ? aLocLeft : aLocRight;
                    case Positions.Right: return isForward ? aLocRight : aLocLeft;
                    case Positions.On: return aLocLine;
                }
            }
            switch (position)
            {
                case Positions.Left: return isForward ? bLocLeft : bLocRight;
                case Positions.Right: return isForward ? bLocRight : bLocLeft;
                case Positions.On: return bLocLine;
            }
            return LOC_UNKNOWN;
        }

        /**
         * Gets the location for this label for either
         * a Boundary or a Line edge.
         * This supports a simple determination of
         * whether the edge should be included as a result edge.
         * 
         * @param index the source index
         * @param position the position for a boundary label
         * @param isForward the direction for a boundary label
         * @return the location for the specified position
         */
        public Location getLocationBoundaryOrLine(int index, Positions position, bool isForward)
        {
            if (isBoundary(index))
            {
                return getLocation(index, position, isForward);
            }
            return getLineLocation(index);
        }

        /**
         * Gets the linear location for the given source.
         * 
         * @param index the source index
         * @return the linear location for the source
         */
        public Location getLocation(int index)
        {
            if (index == 0)
            {
                return aLocLine;
            }
            return bLocLine;
        }

        public bool hasSides(int index)
        {
            if (index == 0)
            {
                return aLocLeft != LOC_UNKNOWN
                    || aLocRight != LOC_UNKNOWN;
            }
            return bLocLeft != LOC_UNKNOWN
                || bLocRight != LOC_UNKNOWN;
        }

        public OverlayLabel copy()
        {
            return new OverlayLabel(this);
        }

        public OverlayLabel copyFlip()
        {
            var lbl = new OverlayLabel();

            lbl.aLocLeft = this.aLocRight;
            lbl.aLocRight = this.aLocLeft;
            lbl.aLocLine = this.aLocLine;
            lbl.aDim = this.aDim;

            lbl.bLocLeft = this.bLocRight;
            lbl.bLocRight = this.bLocLeft;
            lbl.bLocLine = this.bLocLine;
            lbl.bDim = this.bDim;

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
            if (isBoundary(index))
            {
                buf.Append(LocationUtility.ToLocationSymbol(getLocation(index, Positions.Left, isForward)));
                buf.Append(LocationUtility.ToLocationSymbol(getLocation(index, Positions.Right, isForward)));
            }
            else
            {
                buf.Append(LocationUtility.ToLocationSymbol(index == 0 ? aLocLine : bLocLine));
            }
            if (isKnown(index))
                buf.Append(dimensionSymbol(index == 0 ? aDim : bDim));
            if (isCollapse(index))
            {
                buf.Append(ringRoleSymbol(index == 0 ? aIsHole : bIsHole));
            }
            return buf.ToString();
        }

        public static char ringRoleSymbol(bool isHole)
        {
            return isHole ? 'h' : 's';
        }

        public static char dimensionSymbol(Dimension dim)
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
