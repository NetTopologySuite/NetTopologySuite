using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Polygonize
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
        /// <returns>Containing EdgeRing, if there is one <br/>
        /// or <value>null</value> if no containing EdgeRing is found.</returns>
        public static EdgeRing FindEdgeRingContaining(EdgeRing testEr, IList<EdgeRing> shellList)
        {
            var testRing = testEr.Ring;
            var testEnv = testRing.EnvelopeInternal;
            //var testPt = testRing.GetCoordinateN(0);

            EdgeRing minShell = null;
            Envelope minShellEnv = null;
            foreach (var tryShell in shellList)
            {
                var tryShellRing = tryShell.Ring;
                var tryShellEnv = tryShellRing.EnvelopeInternal;
                if (minShell != null)
                    minShellEnv = minShell.Ring.EnvelopeInternal;

                // the hole envelope cannot equal the shell envelope
                // (also guards against testing rings against themselves)
                if (tryShellEnv.Equals(testEnv)) continue;
                // hole must be contained in shell
                if (!tryShellEnv.Contains(testEnv)) continue;

                var testPt = CoordinateArrays.PointNotInList(testRing.Coordinates, tryShellRing.Coordinates);
                var isContained = CGAlgorithms.IsPointInRing(testPt, tryShellRing.Coordinates);

                // check if this new containing ring is smaller than the current minimum ring
                if (isContained)
                {
                    if (minShell == null || minShellEnv.Contains(tryShellEnv))
                    {
                        minShell = tryShell;
                        minShellEnv = minShell.Ring.EnvelopeInternal;
                    }
                }
            }
            return minShell;
        }

        /// <summary>
        /// Finds a point in a list of points which is not contained in another list of points.
        /// </summary>
        /// <param name="testPts">The <c>Coordinate</c>s to test.</param>
        /// <param name="pts">An array of <c>Coordinate</c>s to test the input points against.</param>
        /// <returns>A <c>Coordinate</c> from <c>testPts</c> which is not in <c>pts</c>, <br/>
        /// or <value>null</value>.</returns>
        [Obsolete("Use CoordinateArrays.PointNotInList instead")]
        public static Coordinate PointNotInList(Coordinate[] testPts, Coordinate[] pts)
        {
            foreach (Coordinate testPt in testPts)
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
        [Obsolete]
        public static bool IsInList(Coordinate pt, Coordinate[] pts)
        {
            foreach (Coordinate p in pts)
                if (pt.Equals(p))
                    return true;
            return true;
        }


        /**
         * Traverses a ring of DirectedEdges, accumulating them into a list.
         * This assumes that all dangling directed edges have been removed
         * from the graph, so that there is always a next dirEdge.
         *
         * @param startDE the DirectedEdge to start traversing at
         * @return a List of DirectedEdges that form a ring
         */

        public static List<DirectedEdge> FindDirEdgesInRing(PolygonizeDirectedEdge startDE)
        {
            var de = startDE;
            var edges = new List<DirectedEdge>();
            do
            {
                edges.Add(de);
                de = de.Next;
                Utilities.Assert.IsTrue(de != null, "found null DE in ring");
                Utilities.Assert.IsTrue(de == startDE || !de.IsInRing, "found DE already in ring");
            } while (de != startDE);
            return edges;
        }


        private readonly IGeometryFactory _factory;
        private readonly List<DirectedEdge> _deList = new List<DirectedEdge>();
        private DirectedEdge lowestEdge = null;

        // cache the following data for efficiency
        private ILinearRing _ring;

        private Coordinate[] _ringPts;
        private List<ILinearRing> _holes;
        private EdgeRing _shell;
        private bool _isHole;
        private bool _isProcessed;
        private bool _isIncludedSet;
        private bool _isIncluded = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public EdgeRing(IGeometryFactory factory)
        {
            _factory = factory;
        }

        public void Build(PolygonizeDirectedEdge startDE)
        {
            PolygonizeDirectedEdge de = startDE;
            do
            {
                Add(de);
                de.Ring = this;
                de = de.Next;
                Utilities.Assert.IsTrue(de != null, "found null DE in ring");
                Utilities.Assert.IsTrue(de == startDE || !de.IsInRing, "found DE already in ring");
            } while (de != startDE);
        }


        /// <summary>
        /// Adds a DirectedEdge which is known to form part of this ring.
        /// </summary>
        /// <param name="de">The DirectedEdge to add.</param>
        private void Add(DirectedEdge de)
        {
            _deList.Add(de);
        }

        /// <summary>
        /// Tests whether this ring is a hole.
        /// Due to the way the edges in the polyongization graph are linked,
        /// a ring is a hole if it is oriented counter-clockwise.
        /// </summary>
        /// <returns><c>true</c> if this ring is a hole.</returns>
        public bool IsHole
        {
            get { return _isHole; }
        }

        ///<summary>
        /// Computes whether this ring is a hole.
        /// Due to the way the edges in the polyongization graph are linked,
        /// a ring is a hole if it is oriented counter-clockwise.
        /// </summary>
        public void ComputeHole()
        {
            var ring = Ring;
            _isHole = CGAlgorithms.IsCCW(ring.Coordinates);
        }

        /// <summary>
        /// Adds a hole to the polygon formed by this ring.
        /// </summary>
        /// <param name="hole">The LinearRing forming the hole.</param>
        public void AddHole(ILinearRing hole)
        {
            if (_holes == null)
                _holes = new List<ILinearRing>();
            _holes.Add(hole);
        }

        /// <summary>
        /// Adds a hole to the polygon formed by this ring.
        /// </summary>
        /// <param name="holeER">the <see cref="ILinearRing"/> forming the hole.</param>
        public void AddHole(EdgeRing holeER)
        {
            holeER.Shell = this;
            var hole = holeER.Ring;
            if (_holes == null)
                _holes = new List<ILinearRing>();
            _holes.Add(hole);
        }

        /// <summary>
        /// Computes and returns the Polygon formed by this ring and any contained holes.
        /// </summary>
        public IPolygon Polygon
        {
            get
            {
                ILinearRing[] holeLR = null;
                if (_holes != null)
                {
                    holeLR = new ILinearRing[_holes.Count];
                    for (int i = 0; i < _holes.Count; i++)
                        holeLR[i] = _holes[i];
                }
                IPolygon poly = _factory.CreatePolygon(_ring, holeLR);
                return poly;
            }
        }

        /// <summary>
        /// Tests if the <see cref="ILinearRing" /> ring formed by this edge ring is topologically valid.
        /// </summary>
        /// <return>true if the ring is valid.</return>
        public bool IsValid
        {
            get
            {
                Coordinate[] tempcoords = Coordinates;
                tempcoords = null;
                if (_ringPts.Length <= 3)
                    return false;
                ILinearRing tempring = Ring;
                tempring = null;
                return _ring.IsValid;
            }
        }

        public bool IsIncludedSet
        {
            get { return _isIncludedSet; }
        }

        public bool IsIncluded
        {
            get { return _isIncluded; }
            set
            {
                _isIncluded = value;
                _isIncludedSet = true;
            }
        }

        /// <summary>
        /// Computes and returns the list of coordinates which are contained in this ring.
        /// The coordinatea are computed once only and cached.
        /// </summary>
        private Coordinate[] Coordinates
        {
            get
            {
                if (_ringPts == null)
                {
                    CoordinateList coordList = new CoordinateList();
                    foreach (DirectedEdge de in _deList)
                    {
                        PolygonizeEdge edge = (PolygonizeEdge) de.Edge;
                        AddEdge(edge.Line.Coordinates, de.EdgeDirection, coordList);
                    }
                    _ringPts = coordList.ToCoordinateArray();
                }
                return _ringPts;
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
                Coordinate[] tempcoords = Coordinates;
                tempcoords = null;
                return _factory.CreateLineString(_ringPts);
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
                if (_ring != null)
                    return _ring;
                Coordinate[] tempcoords = Coordinates;
                try
                {
                    _ring = _factory.CreateLinearRing(_ringPts);
                }
                catch (Exception)
                {
                }
                return _ring;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="isForward"></param>
        /// <param name="coordList"></param>
        private static void AddEdge(Coordinate[] coords, bool isForward, CoordinateList coordList)
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

        /// <summary>
        /// Gets or sets a value indicating the containing shell ring of a ring that has been determined to be a hole.
        /// </summary>
        public EdgeRing Shell
        {
            get { return IsHole ? _shell : this; }
            private set { _shell = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this ring has a shell assigned to it.
        /// </summary>
        public bool HasShell
        {
            get { return _shell != null; }
        }

        /// <summary>
        /// Tests whether this ring is an outer hole.
        /// A hole is an outer hole if it is not contained by a shell.
        /// </summary>
        public bool IsOuterHole
        {
            get
            {
                if (!_isHole) return false;
                return !HasShell;
            }
        }

        /// <summary>
        /// Tests whether this ring is an outer shell.
        /// </summary>
        public bool IsOuterShell
        {
            get { return OuterHole != null; }
        }

        public EdgeRing OuterHole
        {
            get
            {
                if (IsHole) return null;
                /*
                 * A shell is an outer shell if any edge is also in an outer hole.
                 * A hole is an outer hole if it is not contained by a shell.
                 */
                for (int i = 0; i < _deList.Count; i++)
                {
                    PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) _deList[i];
                    EdgeRing adjRing = ((PolygonizeDirectedEdge) de.Sym).Ring;
                    if (adjRing.IsOuterHole) return adjRing;
                }
                return null;
            }
        }

        /// <summary>
        /// Updates the included status for currently non-included shells
        /// based on whether they are adjacent to an included shell.
        /// </summary>
        internal void UpdateIncluded()
        {
            if (IsHole) return;
            for (int i = 0; i < _deList.Count; i++)
            {
                PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) _deList[i];
                EdgeRing adjShell = ((PolygonizeDirectedEdge) de.Sym).Ring.Shell;

                if (adjShell != null && adjShell.IsIncludedSet)
                {
                    // adjacent ring has been processed, so set included to inverse of adjacent included
                    IsIncluded = !adjShell.IsIncluded;
                    return;
                }
            }
        }

        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        public override String ToString()
        {
            return WKTWriter.ToLineString(new CoordinateArraySequence(Coordinates));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this ring has been processed.
        /// </summary>
        public bool IsProcessed
        {
            get { return _isProcessed; }
            set { _isProcessed = value; }
        }

        /// <summary>
        /// Compares EdgeRings based on their envelope,
        /// using the standard lexicographic ordering.
        /// This ordering is sufficient to make edge ring sorting deterministic.
        /// </summary>
        /// <author>mbdavis</author>
        public class EnvelopeComparator : IComparer<EdgeRing>
        {
            public int Compare(EdgeRing r0, EdgeRing r1)
            {
                return r0.Ring.Envelope.CompareTo(r1.Ring.Envelope);

            }
        }
    }
}

