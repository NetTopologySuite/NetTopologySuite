using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.Polygon;
using NetTopologySuite.Triangulate.Tri;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Algorithm.Axis
{
    /// <summary>
    /// Constructs an approximation to the medial axis of a Polygon,
    /// as a set of linestrings representing the medial axis graph.
    /// </summary>
    /// <author>Martin Davis</author>
    public class ApproximateMedialAxis
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Geometry MedialAxis(Geometry geom)
        {
            if (!(geom is Polygon pg))
                throw new ArgumentException("Must be a polygon", nameof(geom));

            var tt = new ApproximateMedialAxis(pg);
            return tt.Compute();
        }

        /*
         //-- Testing only
        public static Geometry axisPointSegment(Geometry pt, Geometry seg) {
          Coordinate p = pt.getCoordinate();
          Coordinate[] pts = seg.getCoordinates();
          Coordinate axisPt = medialAxisPoint(p, pts[0], pts[1]);
          return pt.getFactory().createPoint(axisPt);
        }
        */

        private readonly Polygon _inputPolygon;
        private readonly GeometryFactory _geomFact;

        private IDictionary<Tri, AxisNode> nodeMap = new Dictionary<Tri, AxisNode>();
        private Stack<AxisNode> nodeQue = new Stack<AxisNode>();

        public ApproximateMedialAxis(Polygon polygon)
        {
            _inputPolygon = polygon;
            _geomFact = _inputPolygon.Factory;
        }

        private Geometry Compute()
        {
            var cdt = new ConstrainedDelaunayTriangulator(_inputPolygon);
            var tris = cdt.GetTriangles();

            var lines = ConstructLines(tris);
            return _geomFact.CreateMultiLineString(GeometryFactory.ToLineStringArray(lines));
        }

        private List<LineString> ConstructLines(IEnumerable<Tri> tris)
        {
            var lines = new List<LineString>();
            foreach (var tri in tris)
            {
                if (tri.NumAdjacent == 1)
                {
                    lines.Add(ConstructLeafLine(tri));
                }
            }
            while (nodeQue.Count > 0)
            {
                var node = nodeQue.Peek();
                if (node.IsPathComplete)
                {
                    nodeQue.Pop();
                    node.AddInternalLines(lines, _geomFact);
                    //--- done with this node
                }
                else
                {
                    var path = ConstructPath(node);
                    if (path != null)
                        lines.Add(path);
                    //-- node is left in queue for further processing
                }
            }
            return lines;
        }

        private LineString ConstructLeafLine(Tri triStart)
        {
            int eAdj = IndexOfAdjacent(triStart);

            int vOpp = Tri.OppVertex(eAdj);
            var startPt = triStart.GetCoordinate(vOpp);
            var edgePt = AngleBisector(triStart, vOpp);

            return ConstructPath(triStart, eAdj, startPt, edgePt);
        }

        private LineString ConstructPath(AxisNode node)
        {
            var tri = node.Tri;
            int freeEdge = node.NonPathEdge;
            //Coordinate exitPt = node.getPathPoint(freeEdge);
            var startPt = node.CreatePathPoint(freeEdge);

            var triNext = tri.GetAdjacent(freeEdge);
            /*
             * If next tri is a node as well, queue it.
             * No path is constructed, since node internal lines connect
             */
            if (triNext.NumAdjacent == 3)
            {
                int adjNext = triNext.GetIndex(tri);
                AddNodePathPoint(triNext, adjNext, startPt);
                return null;
            }
            return ConstructPath(tri, freeEdge, startPt, null);
        }

        private LineString ConstructPath(Tri triStart, int eStart,
            Coordinate p0, Coordinate p1)
        {
            var pts = new List<Coordinate>();
            if (p0 != null) pts.Add(p0);
            if (p1 != null) pts.Add(p1);

            var triNext = triStart.GetAdjacent(eStart);
            int eAdjNext = triNext.GetIndex(triStart);
            ExtendPath(triNext, eAdjNext, pts);

            return _geomFact.CreateLineString(CoordinateArrays.ToCoordinateArray(pts));
        }

        private void ExtendPath(Tri tri, int edgeEntry, List<Coordinate> pts)
        {
            //if (pts.size() > 100) return;

            //TODO: make this iterative instead of recursive

            int numAdj = tri.NumAdjacent;
            if (numAdj == 3)
            {
                AddNodePathPoint(tri, edgeEntry, pts[pts.Count - 1]);
                //--- path terminates at a node (3-adj tri)
                return;
            }
            if (numAdj < 2)
            {
                //-- leaf node - should never happen, has already been processed
                return;
            }

            //--- now are only dealing with 2-Adj triangles
            int eAdj = IndexOfAdjacentOther(tri, edgeEntry);
            if (false && IsTube(tri, eAdj))
            {
                /*
                 * This triangle and the next one form a "tube"
                 * so use both to construct the medial line.
                 */
                var tri2 = tri.GetAdjacent(eAdj);
                var p = ExitPointTube(tri, tri2);
                pts.Add(p);

                int eAdj2 = tri2.GetIndex(tri);
                int eOpp2 = IndexOfAdjacentOther(tri2, eAdj2);
                var triN = tri2.GetAdjacent(eOpp2);
                int eOppN = triN.GetIndex(tri2);
                ExtendPath(triN, eOppN, pts);
            }
            else
            {
                /*
                 * A "wedge" triangle (with one boundary edge).
                 */
                var p = ExitPointWedge(tri, eAdj);
                pts.Add(p);
                var triN = tri.GetAdjacent(eAdj);
                int eAdjN = triN.GetIndex(tri);
                ExtendPath(triN, eAdjN, pts);
            }
        }

        private void AddNodePathPoint(Tri tri, int edgeEntry, Coordinate pt)
        {
            var node = nodeMap[tri];
            if (node == null)
            {
                node = new AxisNode(tri);
                nodeMap[tri] = node;
            }
            node.AddPathPoint(edgeEntry, pt);
            nodeQue.Push(node);
        }

        private Coordinate ExitPointWedge(Tri tri, int eExit)
        {
            int eBdy = IndexOfNonAdjacent(tri);
            var pt = tri.GetCoordinate(Tri.OppVertex(eBdy));
            var p0 = tri.GetCoordinate(eBdy);
            var p1 = tri.GetCoordinate(Tri.Next(eBdy));
            if (Tri.Next(eBdy) != eExit)
            {
                p0 = tri.GetCoordinate(Tri.Next(eBdy));
                p1 = tri.GetCoordinate(eBdy);
            }
            /*
             * Midpoint produces a straighter line in nearly-parallel corridors, 
             * but is more see-sawed elsewhere. 
             */

            return tri.MidPoint(eExit);
            //return medialAxisPoint(pt, p0, p1);
        }

        /// <summary>
        /// Computes medial axis point on exit edge of a "tube".
        /// </summary>
        /// <param name="tri1">The first triangle in the tube</param>
        /// <param name="tri2">The second triangle in the tube</param>
        /// <returns>The medial axis exit point of tube</returns>
        private Coordinate ExitPointTube(Tri tri1, Tri tri2)
        {

            int eBdy1 = IndexOfNonAdjacent(tri1);
            int eBdy2 = IndexOfNonAdjacent(tri2);
            //--- Case eBdy1 is eEntry.next
            var p00 = tri1.GetCoordinate(eBdy1);
            var p01 = tri1.GetCoordinate(Tri.Next(eBdy1));
            var p10 = tri2.GetCoordinate(Tri.Next(eBdy2));
            var p11 = tri2.GetCoordinate(eBdy2);

            int eAdj1 = tri1.GetIndex(tri2);
            if (Tri.Next(eBdy1) != eAdj1)
            {
                p00 = tri1.GetCoordinate(Tri.Next(eBdy1));
                p01 = tri1.GetCoordinate(eBdy1);
                p10 = tri2.GetCoordinate(eBdy2);
                p11 = tri2.GetCoordinate(Tri.Next(eBdy2));
            }
            var axisPoint = MedialAxisPoint(p00, p01, p10, p11);
            return axisPoint;
        }

        private const double MEDIAL_AXIS_EPS = .01;

        /// <summary>
        /// Computes the approximate point where the medial axis
        /// between two line segments
        /// intersects the line between the ends of the segments.
        /// </summary>
        /// <param name="p00">the start vertex of segment 0</param>
        /// <param name="p01">the end vertex of segment 0</param>
        /// <param name="p10">the start vertex of segment 1</param>
        /// <param name="p11">the end vertex of segment 1</param>
        /// <returns>The approximate medial axis point</returns>
        private static Coordinate MedialAxisPoint(
            Coordinate p00, Coordinate p01,
            Coordinate p10, Coordinate p11)
        {
            double endFrac0 = 0;
            double endFrac1 = 1;
            double eps = 0.0;
            var edgeExit = new LineSegment(p01, p11);
            double edgeLen = edgeExit.Length;
            Coordinate axisPt = null;
            do
            {
                double midFrac = (endFrac0 + endFrac1) / 2;
                axisPt = edgeExit.PointAlong(midFrac);
                double dist0 = DistanceComputer.PointToSegment(axisPt, p00, p01);
                double dist1 = DistanceComputer.PointToSegment(axisPt, p10, p11);
                if (dist0 > dist1)
                {
                    endFrac1 = midFrac;
                }
                else
                {
                    endFrac0 = midFrac;
                }
                eps = Math.Abs(dist0 - dist1) / edgeLen;
            }
            while (eps > MEDIAL_AXIS_EPS);
            return axisPt;
        }

        /// <summary>
        /// Computes the approximate point where the medial axis
        /// between a point and a line segment
        /// intersects the line between the point and the segment endpoint
        /// </summary>
        /// <param name="p">The point</param>
        /// <param name="p0">the start vertex of the segment</param>
        /// <param name="p1">the end vertex of the segment</param>
        /// <returns>The approximate medial axis point</returns>
        private static Coordinate MedialAxisPoint(Coordinate p, Coordinate p0, Coordinate p1)
        {
            double endFrac0 = 0;
            double endFrac1 = 1;
            double eps = 0.0;
            var edgeExit = new LineSegment(p, p1);
            double edgeLen = edgeExit.Length;
            Coordinate axisPt = null;
            do
            {
                double midFrac = (endFrac0 + endFrac1) / 2;
                axisPt = edgeExit.PointAlong(midFrac);
                double distPt = p.Distance(axisPt);
                double distSeg = DistanceComputer.PointToSegment(axisPt, p0, p1);
                if (distPt > distSeg)
                {
                    endFrac1 = midFrac;
                }
                else
                {
                    endFrac0 = midFrac;
                }
                eps = Math.Abs(distSeg - distPt) / edgeLen;
            }
            while (eps > MEDIAL_AXIS_EPS);
            return axisPt;
        }

        /// <summary>
        /// Tests if a triangle and its adjacent tri form a "tube",
        /// where the opposite edges of the triangles are on the boundary.
        /// </summary>
        /// <param name="tri">The triangle to test</param>
        /// <param name="eAdj">The edge adjacent to the next triangle</param>
        /// <returns><c>true</c> if the two triangles form a tube</returns>
        private static bool IsTube(Tri tri, int eAdj)
        {
            var triNext = tri.GetAdjacent(eAdj);
            if (triNext.NumAdjacent != 2)
                return false;

            int eBdy = IndexOfNonAdjacent(tri);
            int vOppBdy = Tri.OppVertex(eBdy);
            var pOppBdy = tri.GetCoordinate(vOppBdy);

            int eBdyN = IndexOfNonAdjacent(triNext);
            int vOppBdyN = Tri.OppVertex(eBdyN);
            var pOppBdyN = triNext.GetCoordinate(vOppBdyN);

            return !pOppBdy.Equals2D(pOppBdyN);
        }

        private static int IndexOfAdjacent(Tri tri)
        {
            for (int i = 0; i < 3; i++)
            {
                if (tri.HasAdjacent(i))
                    return i;
            }
            return -1;
        }

        private static int IndexOfAdjacentOther(Tri tri, int e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i != e && tri.HasAdjacent(i))
                    return i;
            }
            return -1;
        }

        private static int IndexOfNonAdjacent(Tri tri)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!tri.HasAdjacent(i))
                    return i;
            }
            return -1;
        }

        private static Coordinate AngleBisector(Tri tri, int v)
        {
            return Triangle.AngleBisector(
                tri.GetCoordinate(Tri.Prev(v)),
                tri.GetCoordinate(v),
                tri.GetCoordinate(Tri.Next(v))
                );
        }
    }

    class AxisNode
    {

        private readonly Tri _tri;
        /*
         * Axis path points along tri edges
         */
        private Coordinate p0;
        private Coordinate p1;
        private Coordinate p2;
        private bool isLinesAdded = false;

        public AxisNode(Tri tri)
        {
            _tri = tri;
        }

        public Tri Tri => _tri;

        public void AddPathPoint(int edgeIndex, Coordinate p)
        {
            switch (edgeIndex)
            {
                case 0: p0 = p; return;
                case 1: p1 = p; return;
                case 2: p2 = p; return;
            }
        }

        public Coordinate CreatePathPoint(int edgeIndex)
        {
            var pt = _tri.MidPoint(edgeIndex);
            AddPathPoint(edgeIndex, pt);
            return pt;
        }

        public Coordinate GetPathPoint(int edgeIndex)
        {
            switch (edgeIndex)
            {
                case 0: return p0;
                case 1: return p1;
                case 2: return p2;
            }
            return null;
        }

        public bool IsPathComplete => NumPaths == 3;

        public int NumPaths
        {
            get
            {
                int num = 0;
                if (p0 != null) num++;
                if (p1 != null) num++;
                if (p2 != null) num++;
                return num;
            }
        }

        public int NonPathEdge
        {
            get
            {
                if (p0 == null) return 0;
                if (p1 == null) return 1;
                if (p2 == null) return 2;
                return -1;
            }
        }

        public void AddInternalLines(List<LineString> lines, GeometryFactory geomFact)
        {
            //Assert.assertTrue( isPathComplete() );
            if (isLinesAdded) return;
            var cc = Circumcentre();
            if (Intersects(cc))
            {
                AddInternalLines(cc, -1, lines, geomFact);
            }
            else
            {
                AddInternalLinesToEdge(lines, geomFact);
            }
            isLinesAdded = true;
        }

        /*
        //--- Using cc int point isn't as good as midpoint
        private void fillEdgePoints(int longEdge, Coordinate cc) {
          if (p0 == null) p0 = medialPoint(0, longEdge, cc);
          if (p1 == null) p1 = medialPoint(1, longEdge, cc);
          if (p2 == null) p2 = medialPoint(2, longEdge, cc);
        }

        private Coordinate medialPoint(int edge, int longEdge, Coordinate cc) {
          if (edge != longEdge) {
            return tri.midpoint(edge);
          }
          return intersection(
              tri.getEdgeStart(edge), tri.getEdgeEnd(edge),
              tri.getCoordinate( Tri.oppVertex(edge) ), cc);
        }

        private Coordinate intersection(Coordinate p00, Coordinate p01, Coordinate p10, Coordinate p11) {
          LineIntersector li = new RobustLineIntersector();
          li.computeIntersection(p00, p01, p10, p11);
          return li.getIntersection(0);
        }
      */

        private void AddInternalLinesToEdge(List<LineString> lines, GeometryFactory geomFact)
        {
            int nodeEdge = LongEdge();
            var nodePt = GetPathPoint(nodeEdge);
            AddInternalLines(nodePt, nodeEdge, lines, geomFact);
        }

        private void AddInternalLines(Coordinate p, int skipEdge, List<LineString> lines, GeometryFactory geomFact)
        {
            if (skipEdge != 0) AddLine(p0, p, geomFact, lines);
            if (skipEdge != 1) AddLine(p1, p, geomFact, lines);
            if (skipEdge != 2) AddLine(p2, p, geomFact, lines);
        }

        private bool Intersects(Coordinate p)
        {
            return Triangle.Intersects(_tri.GetCoordinate(0),
                _tri.GetCoordinate(1), _tri.GetCoordinate(2), p);
        }

        private Coordinate Circumcentre()
        {
            return Triangle.Circumcentre(_tri.GetCoordinate(0),
                _tri.GetCoordinate(1), _tri.GetCoordinate(2));
        }

        /// <summary>Edge index opposite obtuse angle, if any</summary>
        /// <returns>Edge index of longest edge</returns>
        private int LongEdge()
        {
            int e = 0;
            if (EdgeLen(1) > EdgeLen(e))
            {
                e = 1;
            }
            if (EdgeLen(2) > EdgeLen(e))
            {
                e = 2;
            }
            return e;
        }

        private double EdgeLen(int i)
        {
            return _tri.GetCoordinate(i).Distance(_tri.GetCoordinate(Tri.Next(i)));
        }

        private static void AddLine(Coordinate p0, Coordinate p1,
            GeometryFactory geomFact, List<LineString> lines)
        {
            var line = geomFact.CreateLineString(new Coordinate[] {
      p0.Copy(), p1.Copy()
    });
            lines.Add(line);
        }

    }

}
