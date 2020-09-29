using System.Collections.Generic;

using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Operation.OverlayNG
{
    internal class OverlayEdge : HalfEdge
    {
        /// <summary>
        /// Creates a single OverlayEdge.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="lbl"></param>
        /// <param name="direction"></param>
        /// <returns>A new edge based on the given coordinates and direction.</returns>
        public static OverlayEdge CreateEdge(Coordinate[] pts, OverlayLabel lbl, bool direction)
        {
            Coordinate origin;
            Coordinate dirPt;
            if (direction)
            {
                origin = pts[0];
                dirPt = pts[1];
            }
            else
            {
                int ilast = pts.Length - 1;
                origin = pts[ilast];
                dirPt = pts[ilast - 1];
            }
            return new OverlayEdge(origin, dirPt, direction, lbl, pts);
        }

        public static OverlayEdge CreateEdgePair(Coordinate[] pts, OverlayLabel lbl)
        {
            var e0 = CreateEdge(pts, lbl, true);
            var e1 = CreateEdge(pts, lbl, false);
            e0.Link(e1);
            return e0;
        }

        /// <summary>
        /// Gets a <see cref="IComparer{T}"/> which sorts by the origin Coordinates.
        /// </summary>
        public static IComparer<OverlayEdge> NodeComparator => Comparer<OverlayEdge>.Create((e1, e2) => e1.Orig.CompareTo(e2.Orig));

        public OverlayEdge(Coordinate orig, Coordinate dirPt, bool direction, OverlayLabel label, Coordinate[] pts) : base(orig)
        {
            DirectionPt = dirPt;
            IsForward = direction;
            Coordinates = pts;
            Label = label;
        }

        /// <summary>
        /// <c>true</c> indicates direction is forward along segString<br/>
        /// <c>false</c> is reverse direction<br/>
        /// The label must be interpreted accordingly.
        /// </summary>
        public bool IsForward { get; }

        protected override Coordinate DirectionPt { get; }

        public OverlayLabel Label { get; }

        public Location GetLocation(int index, Position position)
        {
            return Label.GetLocation(index, position, IsForward);
        }

        public Coordinate Coordinate
        {
            get => Orig;
        }

        public Coordinate[] Coordinates { get; }

        public Coordinate[] CoordinatesOriented
        {
            get
            {
                if (IsForward)
                {
                    return Coordinates;
                }

                var copy = (Coordinate[])Coordinates.Clone();
                CoordinateArrays.Reverse(copy);
                return copy;
            }
        }

        /// <summary>
        /// Adds the coordinates of this edge to the given list,
        /// in the direction of the edge.
        /// Duplicate coordinates are removed
        /// (which means that this is safe to use for a path 
        /// of connected edges in the topology graph).
        /// </summary>
        /// <param name="coords">The coordinate list to add to</param>
        public void AddCoordinates(CoordinateList coords)
        {
            bool isFirstEdge = coords.Count > 0;
            if (IsForward)
            {
                int startIndex = 1;
                if (isFirstEdge) startIndex = 0;
                for (int i = startIndex; i < Coordinates.Length; i++)
                {
                    coords.Add(Coordinates[i], false);
                }
            }
            else
            { // is backward
                int startIndex = Coordinates.Length - 2;
                if (isFirstEdge) startIndex = Coordinates.Length - 1;
                for (int i = startIndex; i >= 0; i--)
                {
                    coords.Add(Coordinates[i], false);
                }
            }
        }

        /// <summary>
        /// ets the symmetric pair edge of this edge.
        /// </summary>
        /// <returns>The symmetric pair edge</returns>
        public OverlayEdge SymOE
        {
            get => (OverlayEdge)Sym;
        }

        /// <summary>
        /// Gets the next edge CCW around the origin of this edge,
        /// with the same origin.<br/>
        /// If the origin vertex has degree 1 then this is the edge itself.
        /// </summary>
        /// <returns>
        /// The next edge around the origin
        /// </returns>
        public OverlayEdge ONextOE
        {
            get => (OverlayEdge)ONext;
        }

        public bool IsInResultArea { get; private set; }

        public bool IsInResultAreaBoth
        {
            get => IsInResultArea && SymOE.IsInResultArea;
        }

        public void UnmarkFromResultAreaBoth()
        {
            IsInResultArea = false;
            SymOE.IsInResultArea = false;
        }

        public void MarkInResultArea()
        {
            IsInResultArea = true;
        }

        public void MarkInResultAreaBoth()
        {
            IsInResultArea = true;
            SymOE.IsInResultArea = true;
        }

        public bool IsInResultLine { get; private set; }

        public void MarkInResultLine()
        {
            IsInResultLine = true;
            SymOE.IsInResultLine = true;
        }

        public bool IsInResult
        {
            get => IsInResultArea || IsInResultLine;
        }

        public bool IsInResultEither
        {
            get => IsInResult || SymOE.IsInResult;
        }

        /// <summary>
        /// Gets or sets a link to next edge in the result ring.
        /// The origin of the edge is the dest of this edge.
        /// </summary>
        public OverlayEdge NextResult { get; set; }


        public bool IsResultLinked
        {
            get => NextResult != null;
        }

        public OverlayEdge NextResultMax { get; set; }

        public bool IsResultMaxLinked
        {
            get => NextResultMax != null;
        }

        public bool IsVisited { get; set; }

        public void MarkVisitedBoth()
        {
            IsVisited = true;
            SymOE.IsVisited = true;
        }

        public OverlayEdgeRing EdgeRing { get; set; }

        public MaximalEdgeRing MaxEdgeRing { get; set; }

        public override string ToString()
        {
            var orig = Orig;
            var dest = Dest;
            string dirPtStr = (Coordinates.Length > 2)
                ? ", " + WKTWriter.Format(DirectionPt)
                    : "";

            return "OE( " + WKTWriter.Format(orig)
                + dirPtStr
                + " .. " + WKTWriter.Format(dest)
                + " ) "
                + Label.ToString(IsForward)
                + ResultSymbol
                + " / Sym: " + SymOE.Label.ToString(SymOE.IsForward)
                + SymOE.ResultSymbol
                ;
        }

        private string ResultSymbol
        {
            get
            {
                if (IsInResultArea) return " resA";
                if (IsInResultLine) return " resL";
                return "";
            }
        }
    }
}
