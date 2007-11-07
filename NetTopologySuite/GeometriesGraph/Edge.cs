using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class Edge : GraphComponent
    {
        /// <summary> 
        /// Updates an IM from the label for an edge.
        /// Handles edges from both L and A geometries.
        /// </summary>
        public static void UpdateIM(Label label, IntersectionMatrix im)
        {
            im.SetAtLeastIfValid(label.GetLocation(0, Positions.On), label.GetLocation(1, Positions.On),
                                 Dimensions.Curve);
            if (label.IsArea())
            {
                im.SetAtLeastIfValid(label.GetLocation(0, Positions.Left), label.GetLocation(1, Positions.Left),
                                     Dimensions.Surface);
                im.SetAtLeastIfValid(label.GetLocation(0, Positions.Right), label.GetLocation(1, Positions.Right),
                                     Dimensions.Surface);
            }
        }

        private ICoordinate[] pts;

        private IExtents env;
        private EdgeIntersectionList eiList = null;

        private string name;
        private MonotoneChainEdge mce;
        private Boolean isIsolated = true;
        private Depth depth = new Depth();
        private Int32 depthDelta = 0; // the change in area depth from the R to Curve side of this edge

        public Edge(ICoordinate[] pts, Label label)
        {
            eiList = new EdgeIntersectionList(this);

            this.pts = pts;
            this.label = label;
        }

        public Edge(ICoordinate[] pts) : this(pts, null) {}

        public ICoordinate[] Points
        {
            get { return pts; }
            set { pts = value; }
        }

        public Int32 NumPoints
        {
            get { return Points.Length; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ICoordinate[] Coordinates
        {
            get { return Points; }
        }

        public ICoordinate GetCoordinate(Int32 i)
        {
            return Points[i];
        }

        public override ICoordinate Coordinate
        {
            get
            {
                if (Points.Length > 0)
                {
                    return Points[0];
                }
                return null;
            }
        }

        public IExtents Envelope
        {
            get
            {
                // compute envelope lazily
                if (env == null)
                {
                    env = new Extents();

                    for (Int32 i = 0; i < Points.Length; i++)
                    {
                        env.ExpandToInclude(Points[i]);
                    }
                }

                return env;
            }
        }

        public Depth Depth
        {
            get { return depth; }
        }

        /// <summary>
        /// The depthDelta is the change in depth as an edge is crossed from R to L.
        /// </summary>
        /// <returns>The change in depth as the edge is crossed from R to L.</returns>
        public Int32 DepthDelta
        {
            get { return depthDelta; }
            set { depthDelta = value; }
        }

        public Int32 MaximumSegmentIndex
        {
            get { return Points.Length - 1; }
        }

        public EdgeIntersectionList EdgeIntersectionList
        {
            get { return eiList; }
        }

        public MonotoneChainEdge MonotoneChainEdge
        {
            get
            {
                if (mce == null)
                {
                    mce = new MonotoneChainEdge(this);
                }

                return mce;
            }
        }

        public Boolean IsClosed
        {
            get { return Points[0].Equals(Points[Points.Length - 1]); }
        }

        /// <summary> 
        /// An Edge is collapsed if it is an Area edge and it consists of
        /// two segments which are equal and opposite (eg a zero-width V).
        /// </summary>
        public Boolean IsCollapsed
        {
            get
            {
                if (!label.IsArea())
                {
                    return false;
                }

                if (Points.Length != 3)
                {
                    return false;
                }

                if (Points[0].Equals(Points[2]))
                {
                    return true;
                }

                return false;
            }
        }

        public Edge CollapsedEdge
        {
            get
            {
                ICoordinate[] newPts = new ICoordinate[2];
                newPts[0] = Points[0];
                newPts[1] = Points[1];
                Edge newe = new Edge(newPts, Label.ToLineLabel(label));
                return newe;
            }
        }

        public Boolean Isolated
        {
            get { return isIsolated; }
            set { isIsolated = value; }
        }

        public override Boolean IsIsolated
        {
            get { return isIsolated; }
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        public void AddIntersections(LineIntersector li, Int32 segmentIndex, Int32 geomIndex)
        {
            for (Int32 i = 0; i < li.IntersectionNum; i++)
            {
                AddIntersection(li, segmentIndex, geomIndex, i);
            }
        }

        /// <summary>
        /// Add an EdgeIntersection for intersection intIndex.
        /// An intersection that falls exactly on a vertex of the edge is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        public void AddIntersection(LineIntersector li, Int32 segmentIndex, Int32 geomIndex, Int32 intIndex)
        {
            ICoordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            Int32 normalizedSegmentIndex = segmentIndex;
            Double dist = li.GetEdgeDistance(geomIndex, intIndex);

            // normalize the intersection point location
            Int32 nextSegIndex = normalizedSegmentIndex + 1;

            if (nextSegIndex < Points.Length)
            {
                ICoordinate nextPt = Points[nextSegIndex];

                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                    dist = 0.0;
                }

                // Add the intersection point to edge intersection list.                
                EdgeIntersectionList.Add(intPt, normalizedSegmentIndex, dist);
            }
        }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labelling for both parent geometries.
        /// </summary>
        public override void ComputeIM(IntersectionMatrix im)
        {
            UpdateIM(label, im);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        public override Boolean Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (!(o is Edge))
            {
                return false;
            }
            return Equals(o as Edge);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        protected Boolean Equals(Edge e)
        {
            if (Points.Length != e.Points.Length)
            {
                return false;
            }

            Boolean isEqualForward = true;
            Boolean isEqualReverse = true;
            Int32 iRev = Points.Length;

            for (Int32 i = 0; i < Points.Length; i++)
            {
                if (!Points[i].Equals2D(e.Points[i]))
                {
                    isEqualForward = false;
                }

                if (!Points[i].Equals2D(e.Points[--iRev]))
                {
                    isEqualReverse = false;
                }

                if (!isEqualForward && !isEqualReverse)
                {
                    return false;
                }
            }

            return true;
        }

        public static Boolean operator ==(Edge obj1, Edge obj2)
        {
            return Equals(obj1, obj2);
        }

        public static Boolean operator !=(Edge obj1, Edge obj2)
        {
            return !(obj1 == obj2);
        }

        /// <returns> 
        /// <c>true</c> if the coordinate sequences of the Edges are identical.
        /// </returns>
        public Boolean IsPointwiseEqual(Edge e)
        {
            if (Points.Length != e.Points.Length)
            {
                return false;
            }

            for (Int32 i = 0; i < Points.Length; i++)
            {
                if (! Points[i].Equals2D(e.Points[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("edge " + name + ": ");
            outstream.Write("LINESTRING (");

            for (Int32 i = 0; i < Points.Length; i++)
            {
                if (i > 0)
                {
                    outstream.Write(",");
                }
                outstream.Write(Points[i].X + " " + Points[i].Y);
            }

            outstream.Write(")  " + label + " " + depthDelta);
        }

        public void WriteReverse(StreamWriter outstream)
        {
            outstream.Write("edge " + name + ": ");

            for (Int32 i = Points.Length - 1; i >= 0; i--)
            {
                outstream.Write(Points[i] + " ");
            }

            outstream.WriteLine(String.Empty);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("edge " + name + ": ");
            sb.Append("LINESTRING (");

            for (Int32 i = 0; i < Points.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(Points[i].X + " " + Points[i].Y);
            }

            sb.Append(")  " + label + " " + depthDelta);
            return sb.ToString();
        }
    }
}