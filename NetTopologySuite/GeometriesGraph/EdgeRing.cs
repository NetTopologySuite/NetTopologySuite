using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public abstract class EdgeRing
    {
        /// <summary>
        /// The directed edge which starts the list of edges for this EdgeRing.
        /// </summary>
        protected DirectedEdge startDe;

        private Int32 maxNodeDegree = -1;
        private IList edges = new ArrayList(); // the DirectedEdges making up this EdgeRing
        private IList pts = new ArrayList();

        private Label label = new Label(Locations.Null);
                      // label stores the locations of each point on the face surrounded by this ring

        private ILinearRing ring; // the ring created for this EdgeRing
        private Boolean isHole;
        private EdgeRing shell; // if non-null, the ring is a hole and this EdgeRing is its containing shell
        private ArrayList holes = new ArrayList(); // a list of EdgeRings which are holes in this EdgeRing

        protected IGeometryFactory geometryFactory;

        public EdgeRing(DirectedEdge start, IGeometryFactory geometryFactory)
        {
            this.geometryFactory = geometryFactory;
            ComputePoints(start);
            ComputeRing();
        }

        public Boolean IsIsolated
        {
            get { return label.GeometryCount == 1; }
        }

        public Boolean IsHole
        {
            get { return isHole; }
        }

        public ICoordinate GetCoordinate(Int32 i)
        {
            return (ICoordinate) pts[i];
        }

        public ILinearRing LinearRing
        {
            get { return ring; }
        }

        public Label Label
        {
            get { return label; }
        }

        public Boolean IsShell
        {
            get { return shell == null; }
        }

        public EdgeRing Shell
        {
            get { return shell; }
            set
            {
                shell = value;
                if (value != null)
                {
                    shell.AddHole(this);
                }
            }
        }

        public void AddHole(EdgeRing ring)
        {
            holes.Add(ring);
        }

        public IPolygon ToPolygon(IGeometryFactory geometryFactory)
        {
            ILinearRing[] holeLR = new ILinearRing[holes.Count];

            for (Int32 i = 0; i < holes.Count; i++)
            {
                holeLR[i] = ((EdgeRing) holes[i]).LinearRing;
            }

            IPolygon poly = geometryFactory.CreatePolygon(LinearRing, holeLR);
            return poly;
        }

        /// <summary>
        /// Compute a LinearRing from the point list previously collected.
        /// Test if the ring is a hole (i.e. if it is CCW) and set the hole flag
        /// accordingly.
        /// </summary>
        public void ComputeRing()
        {
            if (ring != null)
            {
                return; // don't compute more than once
            }

            ICoordinate[] coord = new ICoordinate[pts.Count];

            for (Int32 i = 0; i < pts.Count; i++)
            {
                coord[i] = (ICoordinate) pts[i];
            }

            ring = geometryFactory.CreateLinearRing(coord);
            isHole = CGAlgorithms.IsCCW(ring.Coordinates);
        }

        public abstract DirectedEdge GetNext(DirectedEdge de);

        public abstract void SetEdgeRing(DirectedEdge de, EdgeRing er);

        /// <summary> 
        /// Returns the list of DirectedEdges that make up this EdgeRing.
        /// </summary>
        public IList Edges
        {
            get { return edges; }
        }

        /// <summary> 
        /// Collect all the points from the DirectedEdges of this ring into a contiguous list.
        /// </summary>
        protected void ComputePoints(DirectedEdge start)
        {
            startDe = start;
            DirectedEdge de = start;
            Boolean isFirstEdge = true;

            do
            {
                Assert.IsTrue(de != null, "found null Directed Edge");

                if (de.EdgeRing == this)
                {
                    throw new TopologyException("Directed Edge visited twice during ring-building at " + de.Coordinate);
                }

                edges.Add(de);
                Label label = de.Label;
                Assert.IsTrue(label.IsArea());
                MergeLabel(label);
                AddPoints(de.Edge, de.IsForward, isFirstEdge);
                isFirstEdge = false;
                SetEdgeRing(de, this);
                de = GetNext(de);
            } while (de != startDe);
        }

        public Int32 MaxNodeDegree
        {
            get
            {
                if (maxNodeDegree < 0)
                {
                    ComputeMaxNodeDegree();
                }

                return maxNodeDegree;
            }
        }

        private void ComputeMaxNodeDegree()
        {
            maxNodeDegree = 0;
            DirectedEdge de = startDe;

            do
            {
                Node node = de.Node;
                Int32 degree = ((DirectedEdgeStar) node.Edges).GetOutgoingDegree(this);

                if (degree > maxNodeDegree)
                {
                    maxNodeDegree = degree;
                }

                de = GetNext(de);
            } while (de != startDe);

            maxNodeDegree *= 2;
        }

        public void SetInResult()
        {
            DirectedEdge de = startDe;

            do
            {
                de.Edge.InResult = true;
                de = de.Next;
            } while (de != startDe);
        }

        protected void MergeLabel(Label deLabel)
        {
            MergeLabel(deLabel, 0);
            MergeLabel(deLabel, 1);
        }

        /// <summary> 
        /// Merge the RHS label from a DirectedEdge into the label for this EdgeRing.
        /// The DirectedEdge label may be null.  This is acceptable - it results
        /// from a node which is NOT an intersection node between the Geometries
        /// (e.g. the end node of a LinearRing).  In this case the DirectedEdge label
        /// does not contribute any information to the overall labelling, and is simply skipped.
        /// </summary>
        protected void MergeLabel(Label deLabel, Int32 geomIndex)
        {
            Locations loc = deLabel.GetLocation(geomIndex, Positions.Right);

            // no information to be had from this label
            if (loc == Locations.Null)
            {
                return;
            }

            // if there is no current RHS value, set it
            if (label.GetLocation(geomIndex) == Locations.Null)
            {
                label.SetLocation(geomIndex, loc);
                return;
            }
        }

        protected void AddPoints(Edge edge, Boolean isForward, Boolean isFirstEdge)
        {
            ICoordinate[] edgePts = edge.Coordinates;

            if (isForward)
            {
                Int32 startIndex = 1;

                if (isFirstEdge)
                {
                    startIndex = 0;
                }

                for (Int32 i = startIndex; i < edgePts.Length; i++)
                {
                    pts.Add(edgePts[i]);
                }
            }
            else
            {
                // is backward
                Int32 startIndex = edgePts.Length - 2;

                if (isFirstEdge)
                {
                    startIndex = edgePts.Length - 1;
                }

                for (Int32 i = startIndex; i >= 0; i--)
                {
                    pts.Add(edgePts[i]);
                }
            }
        }

        /// <summary> 
        /// This method will cause the ring to be computed.
        /// It will also check any holes, if they have been assigned.
        /// </summary>
        public Boolean ContainsPoint(ICoordinate p)
        {
            ILinearRing shell = LinearRing;
            IExtents env = shell.EnvelopeInternal;

            if (!env.Contains(p))
            {
                return false;
            }

            if (!CGAlgorithms.IsPointInRing(p, shell.Coordinates))
            {
                return false;
            }

            for (IEnumerator i = holes.GetEnumerator(); i.MoveNext();)
            {
                EdgeRing hole = (EdgeRing) i.Current;
                if (hole.ContainsPoint(p))
                {
                    return false;
                }
            }

            return true;
        }
    }
}