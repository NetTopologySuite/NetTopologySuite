using System;
using System.Collections.Generic;
using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.IO;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.OverlayNg
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

        private readonly Coordinate[] _pts;


        /// <summary>
        /// <c>true</c> indicates direction is forward along segString<br/>
        /// <c>false</c> is reverse direction<br/>
        /// The label must be interpreted accordingly.
        /// </summary>
        private readonly bool _direction;
        private readonly Coordinate _dirPt;
        private readonly OverlayLabel _label;

        private bool _isInResultArea;
        private bool _isInResultLine;
        private bool _isVisited;

        /// <summary>
        /// Link to next edge in the result ring.
        /// The origin of the edge is the dest of this edge.
        /// </summary>
        private OverlayEdge _nextResultEdge;

        private OverlayEdgeRing _edgeRing;

        private MaximalEdgeRing _maxEdgeRing;

        private OverlayEdge _nextResultMaxEdge;


        public OverlayEdge(Coordinate orig, Coordinate dirPt, bool direction, OverlayLabel label, Coordinate[] pts) : base(orig)
        {
            _dirPt = dirPt;
            _direction = direction;
            _pts = pts;
            _label = label;
        }

        public bool IsForward
        {
            get => _direction;
        }

        protected override Coordinate DirectionPt
        {
            get => _dirPt;
        }

        public OverlayLabel Label
        {
            get => _label;
        }

        [Obsolete("Use GetLocation(int, Geometries.Position)")]
        public Location GetLocation(int index, Positions position)
            => GetLocation(index, (Position) position);

        public Location GetLocation(int index, Position position)
        {
            return _label.GetLocation(index, position, _direction);
        }

        public Coordinate Coordinate
        {
            get => Orig;
        }

        public Coordinate[] Coordinates
        {
            get => _pts;
        }

        public Coordinate[] CoordinatesOriented
        {
            get
            {
                if (_direction)
                {
                    return _pts;
                }

                var copy = (Coordinate[])_pts.Clone();
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
            if (_direction)
            {
                int startIndex = 1;
                if (isFirstEdge) startIndex = 0;
                for (int i = startIndex; i < _pts.Length; i++)
                {
                    coords.Add(_pts[i], false);
                }
            }
            else
            { // is backward
                int startIndex = _pts.Length - 2;
                if (isFirstEdge) startIndex = _pts.Length - 1;
                for (int i = startIndex; i >= 0; i--)
                {
                    coords.Add(_pts[i], false);
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

        public bool IsInResultArea
        {
            get => _isInResultArea;
            private set => _isInResultArea = value;
        }

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
            _isInResultArea = true;
            SymOE._isInResultArea = true;
        }

        public bool IsInResultLine
        {
            get => _isInResultLine;
            private set => _isInResultLine = value;
        }

        public void MarkInResultLine()
        {
            _isInResultLine = true;
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

        public OverlayEdge NextResult
        {
            get => _nextResultEdge;
            set => _nextResultEdge = value;
        }


        public bool IsResultLinked
        {
            get => _nextResultEdge != null;
        }

        public OverlayEdge NextResultMax
        {
            get => _nextResultMaxEdge;
            set => _nextResultMaxEdge = value;
        }

        public bool IsResultMaxLinked
        {
            get => _nextResultMaxEdge != null;
        }

        public bool IsVisited
        {
            get => _isVisited;
            set => _isVisited = value;
        }

        public void MarkVisitedBoth()
        {
            IsVisited = true;
            SymOE._isVisited = true;
        }

        public OverlayEdgeRing EdgeRing
        {
            get => _edgeRing;
            set => _edgeRing = value;
        }

        public MaximalEdgeRing MaxEdgeRing { get => _maxEdgeRing; set => _maxEdgeRing = value; }

        public override string ToString()
        {
            var orig = Orig;
            var dest = Dest;
            string dirPtStr = (_pts.Length > 2)
                ? ", " + WKTWriter.Format(DirectionPt)
                    : "";

            return "OE( " + WKTWriter.Format(orig)
                + dirPtStr
                + " .. " + WKTWriter.Format(dest)
                + " ) "
                + _label.ToString(_direction)
                + ResultSymbol
                + " / Sym: " + SymOE.Label.ToString(SymOE._direction)
                + SymOE.ResultSymbol
                ;
        }

        private string ResultSymbol
        {
            get
            {
                if (_isInResultArea) return " resA";
                if (_isInResultLine) return " resL";
                return "";
            }
        }
    }
}
