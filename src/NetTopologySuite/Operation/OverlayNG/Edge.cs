using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Represents the linework for edges in a topology graph,
    /// derived from(up to) two parent geometries.
    /// An edge may be the result of the merging of
    /// two or more edges which have the same linework
    /// (although possibly different orientations).
    /// In this case the topology information is
    /// derived from the merging of the information in the
    /// source edges.<br/>
    /// Merged edges can occur in the following situations
    /// <list type="bullet">
    /// <item><description>Due to coincident edges of polygonal or linear geometries.</description></item>
    /// <item><description>Due to topology collapse caused by snapping or rounding
    /// of polygonal geometries.</description></item>
    /// </list>
    /// The source edges may have the same parent geometry,
    /// or different ones, or a mix of the two.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class Edge
    {
        /// <summary>
        /// Tests if the given point sequence
        /// is a collapsed line.<para/>
        /// A collapsed edge has fewer than two distinct points.
        /// </summary>
        /// <param name="pts">The point sequence to check</param>
        /// <returns><c>true</c> if the points form a collapsed line</returns>
        public static bool IsCollapsed(Coordinate[] pts)
        {
            if (pts.Length < 2) return true;
            // zero-length line
            if (pts[0].Equals2D(pts[1])) return true;
            // TODO: is pts > 2 with equal points ever expected?
            if (pts.Length > 2)
            {
                if (pts[pts.Length - 1].Equals2D(pts[pts.Length - 2])) return true;
            }
            return false;
        }

        private Dimension _aDim = OverlayLabel.DIM_UNKNOWN;
        private int _aDepthDelta;
        private bool _aIsHole;

        private Dimension _bDim = OverlayLabel.DIM_UNKNOWN;
        private int _bDepthDelta;
        private bool _bIsHole;

        public Edge(Coordinate[] pts, EdgeSourceInfo info)
        {
            Coordinates = pts;
            CopyInfo(info);
        }

        public Coordinate[] Coordinates { get; }

        public Coordinate GetCoordinate(int index)
        {
            return Coordinates[index];
        }

        public int Count
        {
            get => Coordinates.Length;
        }

        public bool Direction
        {
            get
            {
                var pts = Coordinates;
                if (pts.Length < 2)
                {
                    throw new InvalidOperationException("Edge must have >= 2 points");
                }
                var p0 = pts[0];
                var p1 = pts[1];

                var pn0 = pts[pts.Length - 1];
                var pn1 = pts[pts.Length - 2];

                int cmp = 0;
                int cmp0 = p0.CompareTo(pn0);
                if (cmp0 != 0) cmp = cmp0;

                if (cmp == 0)
                {
                    int cmp1 = p1.CompareTo(pn1);
                    if (cmp1 != 0) cmp = cmp1;
                }

                if (cmp == 0)
                {
                    throw new InvalidOperationException("Edge direction cannot be determined because endpoints are equal");
                }

                return cmp == -1;
            }
        }

        /// <summary>
        /// Compares two coincident edges to determine
        /// whether they have the same or opposite direction.
        /// </summary>
        /// <param name="edge">An edge</param>
        /// <returns><c>true</c> if the edges have the same direction, <c>false</c> if not</returns>
        public bool RelativeDirection(Edge edge)
        {
            // assert: the edges match (have the same coordinates up to direction)
            if (!GetCoordinate(0).Equals2D(edge.GetCoordinate(0)))
                return false;
            if (!GetCoordinate(1).Equals2D(edge.GetCoordinate(1)))
                return false;
            return true;
        }

        public OverlayLabel CreateLabel()
        {
            var lbl = new OverlayLabel();
            InitLabel(lbl, 0, _aDim, _aDepthDelta, _aIsHole);
            InitLabel(lbl, 1, _bDim, _bDepthDelta, _bIsHole);
            return lbl;
        }

        /// <summary>
        /// Populates the label for an edge resulting from an input geometry.
        /// <para/>
        /// <list type="bullet">
        /// <item><description>If the edge is not part of the input, the label is left as <see cref="OverlayLabel.DIM_NOT_PART"/></description></item>
        /// <item><description>If input is an Area and the edge is on the boundary (which may include some collapses), edge is marked as an <see cref="OverlayLabel.DIM_BOUNDARY"/> edge and side locations are assigned</description></item>
        /// <item><description>If input is an Area and the edge is collapsed (depth delta = 0), the label is set to <see cref="OverlayLabel.DIM_COLLAPSE"/>. The location will be determined later by evaluating the final graph topology.</description></item>
        /// <item><description>If input is a Line edge is set to a <see cref="OverlayLabel.DIM_LINE"/> edge. For line edges the line location is not significant (since there is no parent area for which to determine location).</description></item>
        /// </list>
        /// </summary>
        private static void InitLabel(OverlayLabel lbl, int geomIndex, Dimension dim, int depthDelta, bool isHole)
        {
            var dimLabel = LabelDim(dim, depthDelta);

            switch (dimLabel)
            {
                case OverlayLabel.DIM_NOT_PART:
                    lbl.InitNotPart(geomIndex);
                    break;
                case OverlayLabel.DIM_BOUNDARY:
                    lbl.InitBoundary(geomIndex, LocationLeft(depthDelta), LocationRight(depthDelta), isHole);
                    break;
                case OverlayLabel.DIM_COLLAPSE:
                    lbl.InitCollapse(geomIndex, isHole);
                    break;
                case OverlayLabel.DIM_LINE:
                    lbl.InitLine(geomIndex);
                    break;
            }
        }

        private static Dimension LabelDim(Dimension dim, int depthDelta)
        {
            if (dim == Geometries.Dimension.False)
                return OverlayLabel.DIM_NOT_PART;

            if (dim == Geometries.Dimension.Curve)
                return OverlayLabel.DIM_LINE;

            // assert: dim is A
            bool isCollapse = depthDelta == 0;
            if (isCollapse) return OverlayLabel.DIM_COLLAPSE;

            return OverlayLabel.DIM_BOUNDARY;
        }

        /// <summary>
        /// Tests whether the edge is part of a shell in the given geometry.
        /// This is only the case if the edge is a boundary.
        /// </summary>
        /// <param name="geomIndex">The index of the geometry</param>
        /// <returns><c>true</c> if this edge is a boundary and part of a shell</returns>
        private bool IsShell(int geomIndex)
        {
            if (geomIndex == 0)
            {
                return _aDim == OverlayLabel.DIM_BOUNDARY && !_aIsHole;
            }
            return _bDim == OverlayLabel.DIM_BOUNDARY && !_bIsHole;
        }

        private static Location LocationRight(int depthDelta)
        {
            int delSign = DepthDeltaSign(depthDelta);
            switch (delSign)
            {
                case 0: return OverlayLabel.LOC_UNKNOWN;
                case 1: return Location.Interior;
                case -1: return Location.Exterior;
            }
            return OverlayLabel.LOC_UNKNOWN;
        }

        private static Location LocationLeft(int depthDelta)
        {
            // TODO: is it always safe to ignore larger depth deltas?
            int delSign = DepthDeltaSign(depthDelta);
            switch (delSign)
            {
                case 0: return OverlayLabel.LOC_UNKNOWN;
                case 1: return Location.Exterior;
                case -1: return Location.Interior;
            }
            return OverlayLabel.LOC_UNKNOWN;
        }

        private static int DepthDeltaSign(int depthDel)
        {
            if (depthDel > 0) return 1;
            if (depthDel < 0) return -1;
            return 0;
        }

        private void CopyInfo(EdgeSourceInfo info)
        {
            if (info.Index == 0)
            {
                _aDim = info.Dimension;
                _aIsHole = info.IsHole;
                _aDepthDelta = info.DepthDelta;
            }
            else
            {
                _bDim = info.Dimension;
                _bIsHole = info.IsHole;
                _bDepthDelta = info.DepthDelta;
            }
        }

        /// <summary>
        /// Merges an edge into this edge,
        /// updating the topology info accordingly.
        /// </summary>
        /// <param name="edge">The edge to merge</param>
        public void Merge(Edge edge)
        {
            /*
             * Marks this
             * as a shell edge if any contributing edge is a shell.
             * Update hole status first, since it depends on edge dim
             */
            _aIsHole = IsHoleMerged(0, this, edge);
            _bIsHole = IsHoleMerged(1, this, edge);

            if (edge._aDim > _aDim) _aDim = edge._aDim;
            if (edge._bDim > _bDim) _bDim = edge._bDim;

            bool relDir = RelativeDirection(edge);
            int flipFactor = relDir ? 1 : -1;
            _aDepthDelta += flipFactor * edge._aDepthDelta;
            _bDepthDelta += flipFactor * edge._bDepthDelta;
            /*
            if (aDepthDelta > 1) {
              Debug.println(this);
            }
            */
        }

        private static bool IsHoleMerged(int geomIndex, Edge edge1, Edge edge2)
        {
            // TOD: this might be clearer with tri-state logic for isHole?
            bool isShell1 = edge1.IsShell(geomIndex);
            bool isShell2 = edge2.IsShell(geomIndex);
            bool isShellMerged = isShell1 || isShell2;
            // flip since isHole is stored
            return !isShellMerged;
        }

        public override string ToString()
        {

            string ptsStr = ToStringPts(Coordinates);

            string aInfo = InfoString(0, _aDim, _aIsHole, _aDepthDelta);
            string bInfo = InfoString(1, _bDim, _bIsHole, _bDepthDelta);

            return "Edge( " + ptsStr + " ) "
                + aInfo + "/" + bInfo;
        }

        public string ToLineString()
        {
            return WKTWriter.ToLineString(Coordinates);
        }

        private static string ToStringPts(Coordinate[] pts)
        {
            var orig = pts[0];
            var dest = pts[pts.Length - 1];
            string dirPtStr = (pts.Length > 2)
                ? ", " + WKTWriter.Format(pts[1])
                    : "";
            string ptsStr = WKTWriter.Format(orig)
                + dirPtStr
                + " .. " + WKTWriter.Format(dest);
            return ptsStr;
        }

        public static string InfoString(int index, Dimension dim, bool isHole, int depthDelta)
        {
            return
                (index == 0 ? "A:" : "B:")
                + OverlayLabel.DimensionSymbol(dim)
                + RingRoleSymbol(dim, isHole)
                + depthDelta.ToString();  // force to string
        }

        private static string RingRoleSymbol(Dimension dim, bool isHole)
        {
            if (HasAreaParent(dim)) return "" + OverlayLabel.RingRoleSymbol(isHole);
            return "";
        }

        private static bool HasAreaParent(Dimension dim)
        {
            return dim == OverlayLabel.DIM_BOUNDARY || dim == OverlayLabel.DIM_COLLAPSE;
        }
    }

}
