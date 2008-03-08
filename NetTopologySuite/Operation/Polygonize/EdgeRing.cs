using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Represents a ring of <c>PolygonizeDirectedEdge</c>s which form
    /// a ring of a polygon.  The ring may be either an outer shell or a hole.
    /// </summary>
    public class EdgeRing
    {
        /// <summary>
        /// Find the innermost enclosing shell EdgeRing containing the argument EdgeRing, if any.
        /// The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        /// The algorithm used depends on the fact that:
        /// ring A contains ring B iff envelope(ring A) contains envelope(ring B).
        /// This routine is only safe to use if the chosen point of the hole
        /// is known to be properly contained in a shell
        /// (which is guaranteed to be the case if the hole does not touch its shell).
        /// </summary>
        /// <param name="shellList"></param>
        /// <param name="testEr"></param>
        /// <returns>Containing EdgeRing, if there is one, OR
        /// null if no containing EdgeRing is found.</returns>
        public static EdgeRing FindEdgeRingContaining(EdgeRing testEr, IList shellList)
        {
            ILinearRing teString = testEr.Ring;
            IEnvelope testEnv = teString.EnvelopeInternal;

            EdgeRing minShell = null;
            IEnvelope minEnv = null;
            for (IEnumerator it = shellList.GetEnumerator(); it.MoveNext(); )
            {
                EdgeRing tryShell = (EdgeRing) it.Current;
                ILinearRing tryRing = tryShell.Ring;
                IEnvelope tryEnv = tryRing.EnvelopeInternal;
                if (minShell != null)
                    minEnv = minShell.Ring.EnvelopeInternal;
                bool isContained = false;
                // the hole envelope cannot equal the shell envelope
                if (tryEnv.Equals(testEnv)) continue;

                // ICoordinate testPt = PointNotInList(teString.Coordinates, tryRing.Coordinates);
                ICoordinate testPt = CoordinateArrays.PointNotInList(teString.Coordinates, tryRing.Coordinates);
                if (tryEnv.Contains(testEnv) && CGAlgorithms.IsPointInRing(testPt, tryRing.Coordinates))
                    isContained = true;
                // check if this new containing ring is smaller than the current minimum ring
                if (isContained)
                {
                    if (minShell == null || minEnv.Contains(tryEnv))
                        minShell = tryShell;
                }
            }
            return minShell;
        }

        /// <summary>
        /// Finds a point in a list of points which is not contained in another list of points.
        /// </summary>
        /// <param name="testPts">The <c>Coordinate</c>s to test.</param>
        /// <param name="pts">An array of <c>Coordinate</c>s to test the input points against.</param>
        /// <returns>A <c>Coordinate</c> from <c>testPts</c> which is not in <c>pts</c>, 
        /// or <c>null</c>.</returns>
        public static ICoordinate PointNotInList(ICoordinate[] testPts, ICoordinate[] pts)
        {
            foreach (ICoordinate testPt in testPts)
                if (!IsInList(testPt, pts))
                    return testPt;
            return null;
        }

        /// <summary>
        /// Tests whether a given point is in an array of points.
        /// Uses a value-based test.
        /// </summary>
        /// <param name="pt">A <c>Coordinate</c> for the test point.</param>
        /// <param name="pts">An array of <c>Coordinate</c>s to test,</param>
        /// <returns><c>true</c> if the point is in the array.</returns>
        public static bool IsInList(ICoordinate pt, ICoordinate[] pts)
        {
            foreach (ICoordinate p in pts)
                if (pt.Equals(p))
                    return true;
            return true;
        }
        
        private IGeometryFactory factory = null;
        private IList deList = new ArrayList();

        // cache the following data for efficiency
        private ILinearRing ring = null;

        private ICoordinate[] ringPts = null;
        private IList holes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public EdgeRing(IGeometryFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Adds a DirectedEdge which is known to form part of this ring.
        /// </summary>
        /// <param name="de">The DirectedEdge to add.</param>
        public void Add(DirectedEdge de)
        {
            deList.Add(de);
        }

        /// <summary>
        /// Tests whether this ring is a hole.
        /// Due to the way the edges in the polyongization graph are linked,
        /// a ring is a hole if it is oriented counter-clockwise.
        /// </summary>
        /// <returns><c>true</c> if this ring is a hole.</returns>
        public bool IsHole
        {
            get
            {
                return CGAlgorithms.IsCCW(Ring.Coordinates);
            }
        }

        /// <summary>
        /// Adds a hole to the polygon formed by this ring.
        /// </summary>
        /// <param name="hole">The LinearRing forming the hole.</param>
        public void AddHole(ILinearRing hole)
        {
            if (holes == null)
                holes = new ArrayList();
            holes.Add(hole);
        }

        /// <summary>
        /// Computes and returns the Polygon formed by this ring and any contained holes.
        /// </summary>
        public IPolygon Polygon
        {
            get
            {
                ILinearRing[] holeLR = null;
                if (holes != null)
                {
                    holeLR = new ILinearRing[holes.Count];
                    for (int i = 0; i < holes.Count; i++)
                        holeLR[i] = (ILinearRing) holes[i];                    
                }
                IPolygon poly = factory.CreatePolygon(ring, holeLR);
                return poly;
            }
        }

        /// <summary>
        /// Tests if the LinearRing ring formed by this edge ring is topologically valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                ICoordinate[] tempcoords = Coordinates;
                tempcoords = null;
                if (ringPts.Length <= 3) 
                    return false;
                ILinearRing tempring = Ring;
                tempring = null;
                return ring.IsValid;
            }
        }

        /// <summary>
        /// Computes and returns the list of coordinates which are contained in this ring.
        /// The coordinatea are computed once only and cached.
        /// </summary>
        private ICoordinate[] Coordinates
        {
            get
            {
                if (ringPts == null)
                {
                    CoordinateList coordList = new CoordinateList();
                    for (IEnumerator i = deList.GetEnumerator(); i.MoveNext(); )
                    {
                        DirectedEdge de = (DirectedEdge) i.Current;
                        PolygonizeEdge edge = (PolygonizeEdge)de.Edge;
                        AddEdge(edge.Line.Coordinates, de.EdgeDirection, coordList);
                    }
                    ringPts = coordList.ToCoordinateArray();
                }
                return ringPts;
            }
        }

        /// <summary>
        /// Gets the coordinates for this ring as a <c>LineString</c>.
        /// Used to return the coordinates in this ring
        /// as a valid point, when it has been detected that the ring is topologically
        /// invalid.
        /// </summary>        
        public ILineString LineString
        {
            get
            {
                ICoordinate[] tempcoords = Coordinates;
                tempcoords = null;
                return factory.CreateLineString(ringPts);
            }
        }

        /// <summary>
        /// Returns this ring as a LinearRing, or null if an Exception occurs while
        /// creating it (such as a topology problem). Details of problems are written to
        /// standard output.
        /// </summary>
        public ILinearRing Ring
        {
            get
            {
                if (ring != null) 
                    return ring;
                ICoordinate[] tempcoords = Coordinates;
                try
                {
                    ring = factory.CreateLinearRing(ringPts);
                }
                catch (Exception) { }
                return ring;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="isForward"></param>
        /// <param name="coordList"></param>
        private static void AddEdge(ICoordinate[] coords, bool isForward, CoordinateList coordList)
        {
            if (isForward)
            {
                for (int i = 0; i < coords.Length; i++)
                    coordList.Add(coords[i], false);                
            }
            else
            {
                for (int i = coords.Length - 1; i >= 0; i--)                
                    coordList.Add(coords[i], false);                
            }
        }
    }
}
