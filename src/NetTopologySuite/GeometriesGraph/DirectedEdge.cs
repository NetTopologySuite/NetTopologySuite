using System;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    ///
    /// </summary>
    public class DirectedEdge : EdgeEnd
    {
        /// <summary>
        /// Computes the factor for the change in depth when moving from one location to another.
        /// E.g. if crossing from the <see cref="Location.Interior"/> to the <see cref="Location.Exterior"/>
        /// the depth decreases, so the factor is -1.
        /// </summary>
        /// <param name="currLocation">The current location</param>
        /// <param name="nextLocation">The next location</param>
        /// <returns>Change of depth moving from <paramref name="currLocation"/> to <paramref name="nextLocation"/></returns>
        public static int DepthFactor(Location currLocation, Location nextLocation)
        {
            if (currLocation == Location.Exterior && nextLocation == Location.Interior)
                return 1;
            else if (currLocation == Location.Interior && nextLocation == Location.Exterior)
                return -1;
            return 0;
        }


        private bool _isForward;

        private bool _isInResult;
        private bool _isVisited;

        private DirectedEdge _sym; // the symmetric edge

        /// <summary>
        /// The depth of each side (position) of this edge.
        /// The 0 element of the array is never used.
        /// </summary>
        private readonly int[] _depth = { 0, -999, -999 };

        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="isForward"></param>
        public DirectedEdge(Edge edge, bool isForward) : base(edge)
        {
            _isForward = isForward;
            if (isForward)
                Init(edge.GetCoordinate(0), edge.GetCoordinate(1));
            else
            {
                int n = edge.NumPoints - 1;
                Init(edge.GetCoordinate(n), edge.GetCoordinate(n-1));
            }
            ComputeDirectedLabel();
        }

        /// <summary>
        ///
        /// </summary>
        public bool InResult
        {
            get => _isInResult;
            set => _isInResult = value;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsInResult => _isInResult;

        /// <summary>
        ///
        /// </summary>
        public bool Visited
        {
            get => _isVisited;
            set => _isVisited = value;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsVisited => _isVisited;

        /// <summary>
        ///
        /// </summary>
        public EdgeRing EdgeRing { get; set; }

        /// <summary>
        ///
        /// </summary>
        public EdgeRing MinEdgeRing { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        [Obsolete("Use GetDepth(Geometries.Position)")]
        public int GetDepth(Positions position) => GetDepth(new Geometries.Position((int)position));

        /// <summary>
        ///
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetDepth(Geometries.Position position)
        {
            return _depth[(int)position];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="position"></param>
        /// <param name="depthVal"></param>
        [Obsolete("Use SetDepth(Geometries.Position, int)")]
        public void SetDepth(Positions position, int depthVal) => SetDepth(new Geometries.Position((int)position), depthVal);

        /// <summary>
        /// Set depth for a position
        /// </summary>
        /// <param name="position">The position to update</param>
        /// <param name="depthVal">The depth at the provided position</param>
        public void SetDepth(Geometries.Position position, int depthVal)
        {
            if (_depth[(int)position] != -999)
                if (_depth[(int)position] != depthVal)
                    throw new TopologyException("assigned depths do not match", Coordinate);
            _depth[(int)position] = depthVal;
        }

        /// <summary>
        ///
        /// </summary>
        public int DepthDelta
        {
            get
            {
                int depthDelta = Edge.DepthDelta;
                if (!IsForward)
                    depthDelta = -depthDelta;
                return depthDelta;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if both Visited
        /// and Sym.Visited are <c>true</c>.
        /// <para/>
        /// Setting the property marks both DirectedEdges attached to a given Edge.
        /// <para/>
        /// This is used for edges corresponding to lines, which will only
        /// appear oriented in a single direction in the result.
        /// </summary>
        /// <returns></returns>
        public bool VisitedEdge
        {
            get => Visited && _sym.Visited;
            set
            {
                Visited = value;
                _sym.Visited = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsForward
        {
            get => _isForward;
            protected set => _isForward = value;
        }

        /// <summary>
        ///
        /// </summary>
        public DirectedEdge Sym
        {
            get => _sym;
            set => _sym = value;
        }

        /// <summary>
        ///
        /// </summary>
        public DirectedEdge Next { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DirectedEdge NextMin { get; set; }

        /// <summary>
        /// Gets a value indicating if this edge is a line edge.
        /// <para/>It is if
        /// <list type="bullet">
        /// <item><description>at least one of the labels is a line label</description></item>
        /// <item><description>any labels which are not line labels have all Location = Exterior.</description></item>
        /// </list>
        /// </summary>
        /// <returns><c>true</c> if edge is a line edge</returns>
        public bool IsLineEdge
        {
            get
            {
                bool isLine = Label.IsLine(0) || Label.IsLine(1);
                bool isExteriorIfArea0 =
                    !Label.IsArea(0) || Label.AllPositionsEqual(0, Location.Exterior);
                bool isExteriorIfArea1 =
                    !Label.IsArea(1) || Label.AllPositionsEqual(1, Location.Exterior);
                return isLine && isExteriorIfArea0 && isExteriorIfArea1;
            }
        }

        /// <summary>
        /// This is an interior Area edge if
        /// its label is an Area label for both Geometries
        /// and for each Geometry both sides are in the interior.
        /// </summary>
        /// <returns><c>true</c> if this is an interior Area edge.</returns>
        public bool IsInteriorAreaEdge
        {
            get
            {
                bool isInteriorAreaEdge = true;
                for (int i = 0; i < 2; i++)
                {
                    if (!(Label.IsArea(i)
                        && Label.GetLocation(i, Geometries.Position.Left)  == Location.Interior
                        && Label.GetLocation(i, Geometries.Position.Right) == Location.Interior))
                    {
                        isInteriorAreaEdge = false;
                    }
                }
                return isInteriorAreaEdge;
            }
        }

        /// <summary>
        /// Compute the label in the appropriate orientation for this DirEdge.
        /// </summary>
        private void ComputeDirectedLabel()
        {
            Label = new Label(Edge.Label);
            if (!_isForward)
                Label.Flip();
        }

        /// <summary>
        /// Set both edge depths.
        /// One depth for a given side is provided.
        /// The other is computed depending on the Location
        /// transition and the depthDelta of the edge.
        /// </summary>
        /// <param name="position">The position to update</param>
        /// <param name="depth">The depth at the provided position</param>
        [Obsolete("Use SetEdgeDepths(Topology.Postion, int)")]
        public void SetEdgeDepths(Positions position, int depth)
        {
            SetEdgeDepths(new Geometries.Position((int)position), depth);
        }

        /// <summary>
        /// Set both edge depths.
        /// One depth for a given side is provided.
        /// The other is computed depending on the Location
        /// transition and the depthDelta of the edge.
        /// </summary>
        /// <param name="position">The position to update</param>
        /// <param name="depth">The depth at the provided position</param>
        public void SetEdgeDepths(Geometries.Position position, int depth)
        {
            // get the depth transition delta from R to Curve for this directed Edge
            int depthDelta = Edge.DepthDelta;
            if (!_isForward)
                depthDelta = -depthDelta;

            // if moving from Curve to R instead of R to Curve must change sign of delta
            int directionFactor = 1;
            if (position == Geometries.Position.Left)
                directionFactor = -1;

            var oppositePos = position.Opposite;
            int delta = depthDelta * directionFactor;
            int oppositeDepth = depth + delta;
            SetDepth(position, depth);
            SetDepth(oppositePos, oppositeDepth);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public override void Write(StreamWriter outstream)
        {
            base.Write(outstream);
            outstream.Write(" " + _depth[(int)Geometries.Position.Left] + "/" + _depth[(int)Geometries.Position.Right]);
            outstream.Write(" (" + DepthDelta + ")");
            if (_isInResult)
                outstream.Write(" inResult");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void WriteEdge(StreamWriter outstream)
        {
            Write(outstream);
            outstream.Write(" ");
            if (_isForward)
                 Edge.Write(outstream);
            else Edge.WriteReverse(outstream);
        }
    }
}
