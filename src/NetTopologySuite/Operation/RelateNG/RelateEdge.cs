using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.RelateNG
{
    internal class RelateEdge
    {

        public const bool IS_FORWARD = true;
        public const bool IS_REVERSE = false;
  
        public static RelateEdge Create(RelateNode node, Coordinate dirPt, bool isA, Dimension dim, bool isForward)
        {
            if (dim == Geometries.Dimension.A)
                //-- create an area edge
                return new RelateEdge(node, dirPt, isA, isForward);
            //-- create line edge
            return new RelateEdge(node, dirPt, isA);
        }

        public static int FindKnownEdgeIndex(IList<RelateEdge> edges, bool isA)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e.IsKnown(isA))
                    return i;
            }
            return -1;
        }

        public static void SetAreaInterior(List<RelateEdge> edges, bool isA)
        {
            foreach (var e in edges)
            {
                e.SetAreaInterior(isA);
            }
        }

        /// <summary>
        /// The dimension of an input geometry which is not known
        /// </summary>
        public const Dimension DIM_UNKNOWN = Geometries.Dimension.Unknown;

        /// <summary>
        /// Indicates that the location is currently unknown
        /// </summary>
        private const Location LOC_UNKNOWN = Geometries.Location.Null;

        private readonly RelateNode _node;
        private readonly Coordinate _dirPt;

        private Dimension _aDim = DIM_UNKNOWN;
        private Location _aLocLeft = LOC_UNKNOWN;
        private Location _aLocRight = LOC_UNKNOWN;
        private Location _aLocLine = LOC_UNKNOWN;

        private Dimension _bDim = DIM_UNKNOWN;
        private Location _bLocLeft = LOC_UNKNOWN;
        private Location _bLocRight = LOC_UNKNOWN;
        private Location _bLocLine = LOC_UNKNOWN;

        /*
        private int aDim = DIM_UNKNOWN;
        private int aLocLeft = Location.EXTERIOR;
        private int aLocRight = Location.EXTERIOR;
        private int aLocLine = Location.EXTERIOR;

        private int bDim = DIM_UNKNOWN;
        private int bLocLeft = Location.EXTERIOR;
        private int bLocRight = Location.EXTERIOR;
        private int bLocLine = Location.EXTERIOR;
        */

        public RelateEdge(RelateNode node, Coordinate pt, bool isA, bool isForward)
        {
            _node = node;
            _dirPt = pt;
            SetLocationsArea(isA, isForward);
        }

        public RelateEdge(RelateNode node, Coordinate pt, bool isA)
        {
            _node = node;
            _dirPt = pt;
            SetLocationsLine(isA);
        }

        public RelateEdge(RelateNode node, Coordinate pt, bool isA, Location locLeft, Location locRight, Location locLine)
        {
            _node = node;
            _dirPt = pt;
            SetLocations(isA, locLeft, locRight, locLine);
        }

        private void SetLocations(bool isA, Location locLeft, Location locRight, Location locLine)
        {
            if (isA)
            {
                _aDim = Geometries.Dimension.Surface;
                _aLocLeft = locLeft;
                _aLocRight = locRight;
                _aLocLine = locLine;
            }
            else
            {
                _bDim = Geometries.Dimension.Surface;
                _bLocLeft = locLeft;
                _bLocRight = locRight;
                _bLocLine = locLine;
            }
        }

        private void SetLocationsLine(bool isA)
        {
            if (isA)
            {
                _aDim = Geometries.Dimension.Curve;
                _aLocLeft = Geometries.Location.Exterior;
                _aLocRight = Geometries.Location.Exterior;
                _aLocLine = Geometries.Location.Interior;
            }
            else
            {
                _bDim = Geometries.Dimension.Curve;
                _bLocLeft = Geometries.Location.Exterior;
                _bLocRight = Geometries.Location.Exterior   ;
                _bLocLine = Geometries.Location.Interior;
            }
        }

        private void SetLocationsArea(bool isA, bool isForward)
        {
            var locLeft = isForward ? Geometries.Location.Exterior : Geometries.Location.Interior;
            var locRight = isForward ? Geometries.Location.Interior : Geometries.Location.Exterior;
            if (isA)
            {
                _aDim = Geometries.Dimension.Curve;
                _aLocLeft = locLeft;
                _aLocRight = locRight;
                _aLocLine = Geometries.Location.Boundary;
            }
            else
            {
                _bDim = Geometries.Dimension.Curve;
                _bLocLeft = locLeft;
                _bLocRight = locRight;
                _bLocLine = Geometries.Location.Boundary;
            }
        }

        public int CompareToEdge(Coordinate edgeDirPt)
        {
            return PolygonNodeTopology.CompareAngle(_node.Coordinate, this._dirPt, edgeDirPt);
        }

        public void Merge(bool isA, Coordinate dirPt, Dimension dim, bool isForward)
        {
            var locEdge = Geometries.Location.Interior ;
            var locLeft = Geometries.Location.Exterior;
            var locRight = Geometries.Location.Exterior;
            if (dim == Geometries.Dimension.A)
            {
                locEdge = Geometries.Location.Boundary;
                locLeft = isForward ? Geometries.Location.Exterior : Geometries.Location.Interior;
                locRight = isForward ? Geometries.Location.Interior : Geometries.Location.Exterior;
            }

            if (!IsKnown(isA))
            {
                SetDimension(isA, dim);
                SetOn(isA, locEdge);
                SetLeft(isA, locLeft);
                SetRight(isA, locRight);
                return;
            }

            // Assert: node-dirpt is collinear with node-pt
            MergeDimEdgeLoc(isA, locEdge);
            MergeSideLocation(isA, Position.Left, locLeft);
            MergeSideLocation(isA, Position.Right, locRight);
        }

        /// <summary>
        /// Area edges override Line edges.
        /// Merging edges of same dimension is a no-op for
        /// the dimension and on location.
        /// But merging an area edge into a line edge
        /// sets the dimension to A and the location to BOUNDARY.
        /// </summary>
        private void MergeDimEdgeLoc(bool isA, Location locEdge)
        {
            //TODO: this logic needs work - ie handling A edges marked as Interior
            var dim = locEdge == Geometries.Location.Boundary ? Geometries.Dimension.A : Geometries.Dimension.L;
            if (dim == Geometries.Dimension.A && Dimension(isA) == Geometries.Dimension.L)
            {
                SetDimension(isA, dim);
                SetOn(isA, Geometries.Location.Boundary);
            }
        }

        private void MergeSideLocation(bool isA, Position pos, Location loc)
        {
            var currLoc = Location(isA, pos);
            //-- INTERIOR takes precedence over EXTERIOR
            if (currLoc != Geometries.Location.Interior)
            {
                SetLocation(isA, pos, loc);
            }
        }

        private void SetDimension(bool isA, Dimension dimension)
        {
            if (isA)
            {
                _aDim = dimension;
            }
            else
            {
                _bDim = dimension;
            }
        }

        public void SetLocation(bool isA, Position pos, Location loc)
        {
            switch (pos.Index)
            {
                case Position.IndexLeft:
                    SetLeft(isA, loc);
                    break;
                case Position.IndexRight:
                    SetRight(isA, loc);
                    break;
                case Position.IndexOn:
                    SetOn(isA, loc);
                    break;
            }
        }

        public void SetAllLocations(bool isA, Location loc)
        {
            SetLeft(isA, loc);
            SetRight(isA, loc);
            SetOn(isA, loc);
        }

        public void SetUnknownLocations(bool isA, Location loc)
        {
            if (!IsKnown(isA, Position.Left))
            {
                SetLocation(isA, Position.Left, loc);
            }
            if (!IsKnown(isA, Position.Right))
            {
                SetLocation(isA, Position.Right, loc);
            }
            if (!IsKnown(isA, Position.On))
            {
                SetLocation(isA, Position.On, loc);
            }
        }

        private void SetLeft(bool isA, Location loc)
        {
            if (isA)
            {
                _aLocLeft = loc;
            }
            else
            {
                _bLocLeft = loc;
            }
        }

        private void SetRight(bool isA, Location loc)
        {
            if (isA)
            {
                _aLocRight = loc;
            }
            else
            {
                _bLocRight = loc;
            }
        }

        private void SetOn(bool isA, Location loc)
        {
            if (isA)
            {
                _aLocLine = loc;
            }
            else
            {
                _bLocLine = loc;
            }
        }

        public Location Location(bool isA, Position position)
        {
            if (isA)
            {
                switch (position.Index)
                {
                    case Position.IndexLeft: return _aLocLeft;
                    case Position.IndexRight: return _aLocRight;
                    case Position.IndexOn: return _aLocLine;
                }
            }
            else
            {
                switch (position)
                {
                    case Position.IndexLeft: return _bLocLeft;
                    case Position.IndexRight: return _bLocRight;
                    case Position.IndexOn: return _bLocLine;
                }
            }
            Utilities.Assert.ShouldNeverReachHere();
            return LOC_UNKNOWN;
        }

        private Dimension Dimension(bool isA)
        {
            return isA ? _aDim : _bDim;
        }

        private bool IsKnown(bool isA)
        {
            if (isA)
                return _aDim != DIM_UNKNOWN;
            return _bDim != DIM_UNKNOWN;
        }

        private bool IsKnown(bool isA, Position pos)
        {
            return Location(isA, pos) != LOC_UNKNOWN;
        }

        public bool IsInterior(bool isA, Position position)
        {
            return Location(isA, position) == Geometries.Location.Interior;
        }

        public void SetDimLocations(bool isA, Dimension dim, Location loc)
        {
            if (isA)
            {
                _aDim = dim;
                _aLocLeft = loc;
                _aLocRight = loc;
                _aLocLine = loc;
            }
            else
            {
                _bDim = dim;
                _bLocLeft = loc;
                _bLocRight = loc;
                _bLocLine = loc;
            }
        }

        public void SetAreaInterior(bool isA)
        {
            if (isA)
            {
                _aLocLeft = Geometries.Location.Interior;
                _aLocRight = Geometries.Location.Interior;
                _aLocLine = Geometries.Location.Interior;
            }
            else
            {
                _bLocLeft = Geometries.Location.Interior;
                _bLocRight = Geometries.Location.Interior;
                _bLocLine = Geometries.Location.Interior;
            }
        }

        public override string ToString()
        {
            return $"{IO.WKTWriter.ToLineString(_node.Coordinate, _dirPt)} - {LabelString()}";
        }

        private string LabelString()
        {
            var buf = new StringBuilder();
            buf.Append("A:");
            buf.Append(LocationString(RelateGeometry.GEOM_A));
            buf.Append("/B:");
            buf.Append(LocationString(RelateGeometry.GEOM_B));
            return buf.ToString();
        }

        private string LocationString(bool isA)
        {
            var buf = new StringBuilder();
            buf.Append(LocationUtility.ToLocationSymbol(Location(isA, Position.Left)));
            buf.Append(LocationUtility.ToLocationSymbol(Location(isA, Position.On)));
            buf.Append(LocationUtility.ToLocationSymbol(Location(isA, Position.Right)));
            return buf.ToString();
        }

    }

}
