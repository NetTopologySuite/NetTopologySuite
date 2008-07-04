using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// 
    /// </summary>
    public class Edge : GraphComponent
    {
        /// <summary> 
        /// Updates an IM from the label for an edge.
        /// Handles edges from both L and A geometries.
        /// </summary>
        /// <param name="im"></param>
        /// <param name="label"></param>
        public static void UpdateIM(Label label, IntersectionMatrix im)
        {
            im.SetAtLeastIfValid(label.GetLocation(0, Positions.On), label.GetLocation(1, Positions.On), Dimensions.Curve);
            if (label.IsArea())
            {
                im.SetAtLeastIfValid(label.GetLocation(0, Positions.Left), label.GetLocation(1, Positions.Left), Dimensions.Surface);
                im.SetAtLeastIfValid(label.GetLocation(0, Positions.Right), label.GetLocation(1, Positions.Right), Dimensions.Surface);
            }
        }

        private ICoordinate[] pts;
        
        private IEnvelope env;
        private EdgeIntersectionList eiList = null;
      
        private string name;
        private MonotoneChainEdge mce;
        private bool isIsolated = true;
        private Depth depth = new Depth();
        private int depthDelta = 0;   // the change in area depth from the R to Curve side of this edge

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="label"></param>
        public Edge(ICoordinate[] pts, Label label)
        {
            this.eiList = new EdgeIntersectionList(this);

            this.pts = pts;
            this.label = label;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        public Edge(ICoordinate[] pts) : this(pts, null) { }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] Points
        {
            get
            {
                return pts;
            }
            set
            {
                pts = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumPoints
        {
            get
            {
                return Points.Length; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                name = value; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get
            {
                return Points;  
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ICoordinate GetCoordinate(int i)
        {            
            return Points[i];
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICoordinate Coordinate
        {
            get
            {
                if (Points.Length > 0)
                    return Points[0];
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                // compute envelope lazily
                if (env == null) 
                {
                    env = new Envelope();
                    for (int i = 0; i < Points.Length; i++) 
                        env.ExpandToInclude(Points[i]);                
                }
                return env;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Depth Depth
        {
            get
            {
                return depth; 
            }
        }

        /// <summary>
        /// The depthDelta is the change in depth as an edge is crossed from R to L.
        /// </summary>
        /// <returns>The change in depth as the edge is crossed from R to L.</returns>
        public int DepthDelta
        {
            get
            {
                return depthDelta;  
            }
            set
            {
                depthDelta = value;
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        public int MaximumSegmentIndex
        {
            get
            {
                return Points.Length - 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EdgeIntersectionList EdgeIntersectionList
        {
            get
            {
                return eiList; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MonotoneChainEdge MonotoneChainEdge
        {
            get
            {
                if (mce == null) 
                    mce = new MonotoneChainEdge(this);
                return mce;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return Points[0].Equals(Points[Points.Length - 1]);
            }
        }

        /// <summary> 
        /// An Edge is collapsed if it is an Area edge and it consists of
        /// two segments which are equal and opposite (eg a zero-width V).
        /// </summary>
        public bool IsCollapsed
        {
            get
            {
                if (!label.IsArea()) 
                    return false;
                if (Points.Length != 3) 
                    return false;
                if (Points[0].Equals(Points[2]) )
                    return true;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public bool Isolated
        {
            get
            {
                return isIsolated;
            }
            set
            {
                isIsolated = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsIsolated
        {
            get
            {
                return isIsolated;
            }
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        public void AddIntersections(LineIntersector li, int segmentIndex, int geomIndex)
        {
            for (int i = 0; i < li.IntersectionNum; i++) 
                AddIntersection(li, segmentIndex, geomIndex, i);            
        }

        /// <summary>
        /// Add an EdgeIntersection for intersection intIndex.
        /// An intersection that falls exactly on a vertex of the edge is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        /// <param name="intIndex"></param>
        public void AddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
        {
            ICoordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            int normalizedSegmentIndex = segmentIndex;
            double dist = li.GetEdgeDistance(geomIndex, intIndex);        
            
            // normalize the intersection point location
            int nextSegIndex = normalizedSegmentIndex + 1;
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
                this.EdgeIntersectionList.Add(intPt, normalizedSegmentIndex, dist);
            }            
        }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labelling for both parent geometries.
        /// </summary>
        /// <param name="im"></param>
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
        /// <param name="o"></param>
        public override bool Equals(object o)
        {
            if (o == null)
                return false;
            if (!(o is Edge)) 
                return false;
            return Equals(o as Edge);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        /// <param name="e"></param>
        protected bool Equals(Edge e)
        {                     
            if (Points.Length != e.Points.Length)
                return false;

            bool isEqualForward = true;
            bool isEqualReverse = true;
            int iRev = Points.Length;
            for (int i = 0; i < Points.Length; i++)
            {
                if (!Points[i].Equals2D(e.Points[i]))
                    isEqualForward = false;
                if (!Points[i].Equals2D(e.Points[--iRev]))
                    isEqualReverse = false;
                if (!isEqualForward && !isEqualReverse)
                    return false;
            }
            return true;
        }   
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Edge obj1, Edge obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Edge obj1, Edge obj2)
        {
            return !(obj1 == obj2);
        }        
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <returns> 
        /// <c>true</c> if the coordinate sequences of the Edges are identical.
        /// </returns>
        /// <param name="e"></param>
        public bool IsPointwiseEqual(Edge e)
        {
            if (Points.Length != e.Points.Length) 
                return false;
            for (int i = 0; i < Points.Length; i++) 
                if (! Points[i].Equals2D(e.Points[i])) 
                    return false;                        
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(TextWriter outstream)
        {
            outstream.Write("edge " + name + ": ");
            outstream.Write("LINESTRING (");
            for (int i = 0; i < Points.Length; i++)
            {
                if (i > 0)  outstream.Write(",");
                outstream.Write(Points[i].X + " " + Points[i].Y);
            }
            outstream.Write(")  " + label + " " + depthDelta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public void WriteReverse(TextWriter outstream)
        {
            outstream.Write("edge " + name + ": ");
            for (int i = Points.Length - 1; i >= 0; i--) 
                outstream.Write(Points[i] + " ");            
            outstream.WriteLine(String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("edge " + name + ": ");
            sb.Append("LINESTRING (");
            for (int i = 0; i < Points.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(Points[i].X + " " + Points[i].Y);
            }
            sb.Append(")  " + label + " " + depthDelta);
            return sb.ToString();
        }
    }
}
