using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class DirectedEdge : EdgeEnd
    {
        /// <summary>
        /// Computes the factor for the change in depth when moving from one location to another.
        /// E.g. if crossing from the Interior to the Exterior the depth decreases, so the factor is -1.
        /// </summary>
        public static Int32 DepthFactor(Locations currLocation, Locations nextLocation)
        {
            if (currLocation == Locations.Exterior && nextLocation == Locations.Interior)
            {
                return 1;
            }
            else if (currLocation == Locations.Interior && nextLocation == Locations.Exterior)
            {
                return -1;
            }

            return 0;
        }

        protected Boolean isForward;

        private Boolean isInResult = false;
        private Boolean isVisited = false;

        private DirectedEdge sym; // the symmetric edge
        private DirectedEdge next; // the next edge in the edge ring for the polygon containing this edge
        private DirectedEdge nextMin; // the next edge in the MinimalEdgeRing that contains this edge
        private EdgeRing edgeRing; // the EdgeRing that this edge is part of
        private EdgeRing minEdgeRing; // the MinimalEdgeRing that this edge is part of

        /// <summary> 
        /// The depth of each side (position) of this edge.
        /// The 0 element of the array is never used.
        /// </summary>
        private Int32[] depth = {0, -999, -999};

        public DirectedEdge(Edge edge, Boolean isForward) : base(edge)
        {
            this.isForward = isForward;

            if (isForward)
            {
                Init(edge.GetCoordinate(0), edge.GetCoordinate(1));
            }

            else
            {
                Int32 n = edge.NumPoints - 1;
                Init(edge.GetCoordinate(n), edge.GetCoordinate(n - 1));
            }

            ComputeDirectedLabel();
        }

        public Boolean InResult
        {
            get { return isInResult; }
            set { isInResult = value; }
        }

        public Boolean IsInResult
        {
            get { return isInResult; }
        }

        public Boolean Visited
        {
            get { return isVisited; }
            set { isVisited = value; }
        }

        public Boolean IsVisited
        {
            get { return isVisited; }
        }

        public EdgeRing EdgeRing
        {
            get { return edgeRing; }
            set { edgeRing = value; }
        }

        public EdgeRing MinEdgeRing
        {
            get { return minEdgeRing; }
            set { minEdgeRing = value; }
        }

        public Int32 GetDepth(Positions position)
        {
            return depth[(Int32) position];
        }

        public void SetDepth(Positions position, Int32 depthVal)
        {
            if (depth[(Int32) position] != -999)
            {
                if (depth[(Int32) position] != depthVal)
                {
                    throw new TopologyException("assigned depths do not match", Coordinate);
                }
            }
            depth[(Int32) position] = depthVal;
        }

        public Int32 DepthDelta
        {
            get
            {
                Int32 depthDelta = edge.DepthDelta;

                if (!IsForward)
                {
                    depthDelta = -depthDelta;
                }

                return depthDelta;
            }
        }

        /// <summary>
        /// VisitedEdge get property returns <c>true</c> if bot Visited 
        /// and Sym.Visited are <c>true</c>.
        /// VisitedEdge set property marks both DirectedEdges attached to a given Edge.
        /// This is used for edges corresponding to lines, which will only
        /// appear oriented in a single direction in the result.
        /// </summary>
        public Boolean VisitedEdge
        {
            get { return Visited && sym.Visited; }
            set
            {
                Visited = value;
                sym.Visited = value;
            }
        }

        public Boolean IsForward
        {
            get { return isForward; }
        }

        public DirectedEdge Sym
        {
            get { return sym; }
            set { sym = value; }
        }

        public DirectedEdge Next
        {
            get { return next; }
            set { next = value; }
        }

        public DirectedEdge NextMin
        {
            get { return nextMin; }
            set { nextMin = value; }
        }

        /// <summary>
        /// This edge is a line edge if
        /// at least one of the labels is a line label
        /// any labels which are not line labels have all Locations = Exterior.
        /// </summary>
        public Boolean IsLineEdge
        {
            get
            {
                Boolean isLine = label.IsLine(0) || label.IsLine(1);
                Boolean isExteriorIfArea0 =
                    !label.IsArea(0) || label.AllPositionsEqual(0, Locations.Exterior);
                Boolean isExteriorIfArea1 =
                    !label.IsArea(1) || label.AllPositionsEqual(1, Locations.Exterior);
                return isLine && isExteriorIfArea0 && isExteriorIfArea1;
            }
        }

        /// <summary> 
        /// This is an interior Area edge if
        /// its label is an Area label for both Geometries
        /// and for each Geometry both sides are in the interior.
        /// </summary>
        /// <returns><c>true</c> if this is an interior Area edge.</returns>
        public Boolean IsInteriorAreaEdge
        {
            get
            {
                Boolean isInteriorAreaEdge = true;

                for (Int32 i = 0; i < 2; i++)
                {
                    if (!(label.IsArea(i)
                          && label.GetLocation(i, Positions.Left) == Locations.Interior
                          && label.GetLocation(i, Positions.Right) == Locations.Interior))
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
            label = new Label(edge.Label);

            if (!isForward)
            {
                label.Flip();
            }
        }

        /// <summary> 
        /// Set both edge depths.  
        /// One depth for a given side is provided.  
        /// The other is computed depending on the Location 
        /// transition and the depthDelta of the edge.
        /// </summary>
        public void SetEdgeDepths(Positions position, Int32 depth)
        {
            // get the depth transition delta from R to Curve for this directed Edge
            Int32 depthDelta = Edge.DepthDelta;
           
            if (!isForward)
            {
                depthDelta = -depthDelta;
            }

            // if moving from Curve to R instead of R to Curve must change sign of delta
            Int32 directionFactor = 1;

            if (position == Positions.Left)
            {
                directionFactor = -1;
            }

            Positions oppositePos = Position.Opposite(position);
            Int32 delta = depthDelta*directionFactor;
            Int32 oppositeDepth = depth + delta;
            SetDepth(position, depth);
            SetDepth(oppositePos, oppositeDepth);
        }

        /// <summary> 
        /// Set both edge depths.  One depth for a given side is provided.  The other is
        /// computed depending on the Location transition and the depthDelta of the edge.
        /// </summary>
        [Obsolete("Use SetEdgeDepths instead")]
        public void OLDSetEdgeDepths(Positions position, Int32 depth)
        {
            Int32 depthDelta = Edge.DepthDelta;
            Locations loc = label.GetLocation(0, position);
            Positions oppositePos = Position.Opposite(position);
            Locations oppositeLoc = label.GetLocation(0, oppositePos);
            Int32 delta = Math.Abs(depthDelta)*DepthFactor(loc, oppositeLoc);
            Int32 oppositeDepth = depth + delta;
            SetDepth(position, depth);
            SetDepth(oppositePos, oppositeDepth);
        }

        public override void Write(StreamWriter outstream)
        {
            base.Write(outstream);
            outstream.Write(" " + depth[(Int32) Positions.Left] + "/" + depth[(Int32) Positions.Right]);
            outstream.Write(" (" + DepthDelta + ")");

            if (isInResult)
            {
                outstream.Write(" inResult");
            }
        }

        public void WriteEdge(StreamWriter outstream)
        {
            Write(outstream);
            outstream.Write(" ");

            if (isForward)
            {
                edge.Write(outstream);
            }
            else
            {
                edge.WriteReverse(outstream);
            }
        }
    }
}