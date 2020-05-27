using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Represents a single edge in a topology graph,
     * carrying the topology information 
     * derived from the two parent geometries.
     * The edge may be the result of the merger of 
     * two or more edges which happen to have the same underlying linework
     * (although possibly different orientations).  
     * In this case the topology information is 
     * derived from the merging of the information in the 
     * constituent edges.
     * 
     * @author mdavis
     *
     */
    class Edge
    {

        public static List<Edge> createEdges(IEnumerable<ISegmentString> segStrings)
        {
            var edges = new List<Edge>();
            foreach (var ss in segStrings)
            {
                var pts = ss.Coordinates;

                // don't create edges from collapsed lines
                // TODO: perhaps convert these to points to be included in overlay?
                if (IsCollapsed(pts)) continue;

                var info = (EdgeSourceInfo)ss.Context;
                edges.Add(new Edge(pts, info));
            }
            return edges;
        }

        /**
         * Tests if the given point sequence
         * is a collapsed line.
         * A collapsed edge has fewer than two distinct points.
         * 
         * @param pts the point sequence to check
         * @return true if the points form a collapsed line
         */
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

        private readonly Coordinate[] _pts;

        private Dimension aDim = OverlayLabel.DIM_UNKNOWN;
        private int aDepthDelta = 0;
        private bool aIsHole = false;

        private Dimension bDim = OverlayLabel.DIM_UNKNOWN;
        private int bDepthDelta = 0;
        private bool bIsHole = false;

        public Edge(Coordinate[] pts, EdgeSourceInfo info)
        {
            _pts = pts;
            copyInfo(info);
        }

        public Coordinate[] getCoordinates()
        {
            return _pts;
        }

        public Coordinate getCoordinate(int index)
        {
            return _pts[index];
        }

        public int Count
        {
            get => _pts.Length;
        }

        public bool direction()
        {
            var pts = getCoordinates();
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

            return cmp == -1 ? true : false;
        }

        /**
         * Compares two coincident edges to determine
         * whether they have the same or opposite direction.
         * 
         * @param edge1 an edge
         * @param edge2 an edge
         * @return true if the edges have the same direction, false if not
         */
        public bool relativeDirection(Edge edge2)
        {
            // assert: the edges match (have the same coordinates up to direction)
            if (!getCoordinate(0).Equals2D(edge2.getCoordinate(0)))
                return false;
            if (!getCoordinate(1).Equals2D(edge2.getCoordinate(1)))
                return false;
            return true;
        }

        public Dimension dimension(int geomIndex)
        {
            if (geomIndex == 0) return aDim;
            return bDim;
        }

        public OverlayLabel createLabel()
        {
            var lbl = new OverlayLabel();
            InitLabel(lbl, 0, aDim, aDepthDelta, aIsHole);
            InitLabel(lbl, 1, bDim, bDepthDelta, bIsHole);
            return lbl;
        }

        /**
         * Populates the label for an edge resulting from an input geometry.
         * 
         * <ul>
         * <li>If the edge is not part of the input, the label is left as NOT_PART
         * <li>If input is an Area and the edge is on the boundary
         * (which may include some collapses),
         * edge is marked as an AREA edge and side locations are assigned
         * <li>If input is an Area and the edge is collapsed
         * (depth delta = 0), 
         * the label is set to COLLAPSE.
         * The location will be determined later
         * by evaluating the final graph topology.
         * <li>If input is a Line edge is set to a LINE edge.
         * For line edges the line location is not significant
         * (since there is no parent area for which to determine location).
         * </ul>
         * 
         * @param lbl
         * @param geomIndex
         * @param dim
         * @param depthDelta
         */
        private static void InitLabel(OverlayLabel lbl, int geomIndex, Dimension dim, int depthDelta, bool isHole)
        {
            var dimLabel = LabelDim(dim, depthDelta);

            switch (dimLabel)
            {
                case OverlayLabel.DIM_NOT_PART:
                    lbl.initNotPart(geomIndex);
                    break;
                case OverlayLabel.DIM_BOUNDARY:
                    lbl.initBoundary(geomIndex, LocationLeft(depthDelta), LocationRight(depthDelta), isHole);
                    break;
                case OverlayLabel.DIM_COLLAPSE:
                    lbl.initCollapse(geomIndex, isHole);
                    break;
                case OverlayLabel.DIM_LINE:
                    lbl.initLine(geomIndex);
                    break;
            }
        }

        private static Dimension LabelDim(Dimension dim, int depthDelta)
        {
            if (dim == Dimension.False)
                return OverlayLabel.DIM_NOT_PART;

            if (dim == Dimension.Curve)
                return OverlayLabel.DIM_LINE;

            // assert: dim is A
            bool isCollapse = depthDelta == 0;
            if (isCollapse) return OverlayLabel.DIM_COLLAPSE;

            return OverlayLabel.DIM_BOUNDARY;
        }

        private bool IsHole(int index)
        {
            if (index == 0)
                return aIsHole;
            return bIsHole;
        }

        private bool IsBoundary(int geomIndex)
        {
            if (geomIndex == 0) return aDim == OverlayLabel.DIM_BOUNDARY;
            return bDim == OverlayLabel.DIM_BOUNDARY;
        }

        /**
         * Tests whether the edge is part of a shell in the given geometry.
         * This is only the case if the edge is a boundary.
         * 
         * @param geomIndex the index of the geometry
         * @return true if this edge is a boundary and part of a shell
         */
        private bool isShell(int geomIndex)
        {
            if (geomIndex == 0)
            {
                return aDim == OverlayLabel.DIM_BOUNDARY && !aIsHole;
            }
            return bDim == OverlayLabel.DIM_BOUNDARY && !bIsHole;
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

        private void copyInfo(EdgeSourceInfo info)
        {
            if (info.Index == 0)
            {
                aDim = info.Dimension;
                aIsHole = info.IsHole;
                aDepthDelta = info.DepthDelta;
            }
            else
            {
                bDim = info.Dimension;
                bIsHole = info.IsHole;
                bDepthDelta = info.DepthDelta;
            }
        }

        /**
         * Merges an edge into this edge,
         * updating the topology info accordingly.
         * 
         * @param edge
         */
        public void merge(Edge edge)
        {
            /**
             * Marks this
             * as a shell edge if any contributing edge is a shell.
             * Update hole status first, since it depends on edge dim
             */
            aIsHole = IsHoleMerged(0, this, edge);
            bIsHole = IsHoleMerged(1, this, edge);

            if (edge.aDim > aDim) aDim = edge.aDim;
            if (edge.bDim > bDim) bDim = edge.bDim;

            bool relDir = relativeDirection(edge);
            int flipFactor = relDir ? 1 : -1;
            aDepthDelta += flipFactor * edge.aDepthDelta;
            bDepthDelta += flipFactor * edge.bDepthDelta;
            /*
            if (aDepthDelta > 1) {
              Debug.println(this);
            }
            */
        }

        private static bool IsHoleMerged(int geomIndex, Edge edge1, Edge edge2)
        {
            // TOD: this might be clearer with tri-state logic for isHole?
            bool isShell1 = edge1.isShell(geomIndex);
            bool isShell2 = edge2.isShell(geomIndex);
            bool isShellMerged = isShell1 || isShell2;
            // flip since isHole is stored
            return !isShellMerged;
        }

        public override string ToString()
        {

            string ptsStr = ToStringPts(_pts);

            string aInfo = infoString(0, aDim, aIsHole, aDepthDelta);
            string bInfo = infoString(1, bDim, bIsHole, bDepthDelta);

            return "Edge( " + ptsStr + " ) "
                + aInfo + "/" + bInfo;
        }
        public string toLineString()
        {
            return WKTWriter.ToLineString(_pts);
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

        public static string infoString(int index, Dimension dim, bool isHole, int depthDelta)
        {
            return
                (index == 0 ? "A:" : "B:")
                + OverlayLabel.dimensionSymbol(dim)
                + ringRoleSymbol(dim, isHole)
                + depthDelta.ToString();  // force to string
        }

        public static string ringRoleSymbol(Dimension dim, bool isHole)
        {
            if (HasAreaParent(dim)) return "" + OverlayLabel.ringRoleSymbol(isHole);
            return "";
        }

        private static bool HasAreaParent(Dimension dim)
        {
            return dim == OverlayLabel.DIM_BOUNDARY || dim == OverlayLabel.DIM_COLLAPSE;
        }
    }

}
