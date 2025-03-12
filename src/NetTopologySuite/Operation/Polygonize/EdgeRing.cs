using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Planargraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Represents a ring of <see cref="PolygonizeDirectedEdge"/>s which form
    /// a ring of a polygon.  The ring may be either an outer shell or a hole.
    /// </summary>
    public class EdgeRing
    {
        /// <summary>
        /// Find the innermost enclosing shell EdgeRing containing the argument EdgeRing, if any.
        /// The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        /// The algorithm used depends on the fact that:
        /// ring A contains ring B if envelope(ring A) contains envelope(ring B).
        /// This routine is only safe to use if the chosen point of the hole
        /// is known to be properly contained in a shell
        /// (which is guaranteed to be the case if the hole does not touch its shell).
        /// <para/>
        /// To improve performance of this function the caller should
        /// make the passed shellList as small as possible(e.g.
        /// by using a spatial index filter beforehand).
        /// </summary>
        /// <param name="erList"></param>
        /// <param name="testEr"></param>
        /// <returns>Containing EdgeRing or <c>null</c> if no containing EdgeRing is found.</returns>
        public static EdgeRing FindEdgeRingContaining(EdgeRing testEr, IList<EdgeRing> erList)
        {
            EdgeRing minContainingRing = null;
            foreach (var edgeRing in erList)
            {
                if (edgeRing.Contains(testEr))
                {
                    if (minContainingRing == null
                        || minContainingRing.Envelope.Contains(edgeRing.Envelope))
                    {
                        minContainingRing = edgeRing;
                    }
                }
            }
            return minContainingRing;
        }

        /// <summary>
        /// Traverses a ring of DirectedEdges, accumulating them into a list.
        /// This assumes that all dangling directed edges have been removed
        /// from the graph, so that there is always a next dirEdge.
        /// </summary>
        /// <remarks>This function has a different return type than its counterpart
        /// in JTS. Use <see cref="FindPolyDirEdgesInRing(PolygonizeDirectedEdge)"/>
        /// instead.</remarks>
        /// <param name="startDE">The DirectedEdge to start traversing at</param>
        /// <returns>A list of DirectedEdges that form a ring</returns>
        [Obsolete]
        public static List<DirectedEdge> FindDirEdgesInRing(PolygonizeDirectedEdge startDE)
        {
            var de = startDE;
            var edges = new List<DirectedEdge>();
            do
            {
                edges.Add(de);
                de = de.Next;
                Assert.IsTrue(de != null, "found null DE in ring");
                Assert.IsTrue(de == startDE || !de.IsInRing, "found DE already in ring");
            } while (de != startDE);
            return edges;
        }

        /// <summary>
        /// Traverses a ring of DirectedEdges, accumulating them into a list.
        /// This assumes that all dangling directed edges have been removed
        /// from the graph, so that there is always a next dirEdge.
        /// </summary>
        /// <remarks>This function is called <c>FindDirEdgesInRing</c> in JTS.</remarks>
        /// <param name="startDE">The DirectedEdge to start traversing at</param>
        /// <returns>A list of DirectedEdges that form a ring</returns>
        public static IList<PolygonizeDirectedEdge> FindPolyDirEdgesInRing(PolygonizeDirectedEdge startDE)
        {
            var de = startDE;
            var edges = new List<PolygonizeDirectedEdge>();
            do
            {
                edges.Add(de);
                de = de.Next;
                Assert.IsTrue(de != null, "found null DE in ring");
                Assert.IsTrue(de == startDE || !de.IsInRing, "found DE already in ring");
            } while (de != startDE);
            return edges;
        }

        private readonly GeometryFactory _factory;

        private readonly List<PolygonizeDirectedEdge> _deList = new List<PolygonizeDirectedEdge>();
        //private DirectedEdge lowestEdge = null;

        // cache the following data for efficiency
        private LinearRing _ring;
        private IndexedPointInAreaLocator locator;

        private Coordinate[] _ringPts;
        private IList<LinearRing> _holes;
        private EdgeRing _shell;
        private bool _isHole;
        private bool _isValid;
        private bool _isProcessed;
        private bool _isIncludedSet;
        private bool _isIncluded = false;

        public EdgeRing(GeometryFactory factory)
        {
            _factory = factory;
        }

        public void Build(PolygonizeDirectedEdge startDE)
        {
            var de = startDE;
            do
            {
                Add(de);
                de.Ring = this;
                de = de.Next;
                Assert.IsTrue(de != null, "found null DE in ring");
                Assert.IsTrue(de == startDE || !de.IsInRing, "found DE already in ring");
            } while (de != startDE);
        }

        /// <summary>
        /// Adds a DirectedEdge which is known to form part of this ring.
        /// </summary>
        /// <param name="de">The DirectedEdge to add.</param>
        private void Add(PolygonizeDirectedEdge de)
        {
            _deList.Add(de);
        }

        public IList<PolygonizeDirectedEdge> Edges => _deList;

        /// <summary>
        /// Tests whether this ring is a hole.
        /// Due to the way the edges in the polygonization graph are linked,
        /// a ring is a hole if it is oriented counter-clockwise.
        /// </summary>
        /// <returns><c>true</c> if this ring is a hole.</returns>
        public bool IsHole => _isHole;

        /// <summary>
        /// Computes whether this ring is a hole.
        /// Due to the way the edges in the polygonization graph are linked,
        /// a ring is a hole if it is oriented counter-clockwise.
        /// </summary>
        public void ComputeHole()
        {
            var ring = Ring;
            _isHole = Orientation.IsCCW(ring.CoordinateSequence);
            Assert.IsTrue(Orientation.IsCCW(ring.CoordinateSequence) == Orientation.IsCCW(ring.Coordinates));
        }

        /// <summary>
        /// Adds a hole to the polygon formed by this ring.
        /// </summary>
        /// <param name="hole">The LinearRing forming the hole.</param>
        public void AddHole(LinearRing hole)
        {
            if (_holes == null)
                _holes = new List<LinearRing>();
            _holes.Add(hole);
        }

        /// <summary>
        /// Adds a hole to the polygon formed by this ring.
        /// </summary>
        /// <param name="holeER">the <see cref="LinearRing"/> forming the hole.</param>
        public void AddHole(EdgeRing holeER)
        {
            holeER.Shell = this;
            var hole = holeER.Ring;
            if (_holes == null)
                _holes = new List<LinearRing>();
            _holes.Add(hole);
        }

        /// <summary>
        /// Computes and returns the Polygon formed by this ring and any contained holes.
        /// </summary>
        public Polygon Polygon
        {
            get
            {
                LinearRing[] holeLR = null;
                if (_holes != null)
                {
                    holeLR = new LinearRing[_holes.Count];
                    for (int i = 0; i < _holes.Count; i++)
                        holeLR[i] = _holes[i];
                }
                var poly = _factory.CreatePolygon(_ring, holeLR);
                return poly;
            }
        }

        /// <summary>
        /// Gets a value indicating if the <see cref="LinearRing" /> ring formed by this edge ring is topologically valid.
        /// </summary>
        /// <remarks><see cref="ComputeValid"/> must be called prior to accessing this property.</remarks>
        /// <return>true if the ring is valid.</return>
        public bool IsValid => _isValid;

        /// <summary>
        /// Computes the validity of the ring.
        /// Must be called prior to calling <see cref="IsValid"/>.
        /// </summary>
        public void ComputeValid()
        {
            var ringPts = Coordinates;
            if (ringPts.Length <= 3)
            {
                _isValid = false;
                return;
            }
            var ring = Ring;
            _isValid = ring.IsValid;
        }

        public bool IsIncludedSet => _isIncludedSet;

        public bool IsIncluded
        {
            get => _isIncluded;
            set
            {
                _isIncluded = value;
                _isIncludedSet = true;
            }
        }

        private IPointOnGeometryLocator Locator
        {
            get
            {
                if (locator == null)
                    locator = new IndexedPointInAreaLocator(Ring);

                return locator;
            }
        }

        public Location Locate(Coordinate pt)
        {
            return Locator.Locate(pt);
        }

        [Obsolete("Will be removed in a future version")]
        public bool IsInRing(Coordinate pt)
        {
            /*
             * Use an indexed point-in-polygon for performance
             */
            return Location.Exterior != Locator.Locate(pt);
        }

        /// <summary>
        /// Tests if an edgeRing is properly contained in this ring.
        /// Relies on property that edgeRings never overlap (although they may
        /// touch at single vertices).</summary>
        /// <param name="ring">The ring to test</param>
        /// <returns><c>true</c> if ring is properly contained</returns>
        private bool Contains(EdgeRing ring)
        {
            // the test envelope must be properly contained
            // (guards against testing rings against themselves)
            var env = this.Envelope;
            var testEnv = ring.Envelope;
            if (!env.ContainsProperly(testEnv))
                return false;
            return IsPointInOrOut(ring);
        }

        private bool IsPointInOrOut(EdgeRing ring)
        {
            // in most cases only one or two points will be checked
            foreach (var pt in ring.Coordinates)
            {
                var loc = Locate(pt);
                if (loc == Location.Interior)
                {
                    return true;
                }
                if (loc == Location.Exterior)
                {
                    return false;
                }
                // pt is on BOUNDARY, so keep checking for a determining location
            }
            return false;
        }

        /// <summary>
        /// Computes and returns the list of coordinates which are contained in this ring.
        /// The coordinates are computed once only and cached.
        /// </summary>
        private Coordinate[] Coordinates
        {
            get
            {
                if (_ringPts == null)
                {
                    var coordList = new CoordinateList();
                    foreach (var de in _deList)
                    {
                        var edge = (PolygonizeEdge) de.Edge;
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
        public LineString LineString
        {
            get
            {
                var tempcoords = Coordinates;
                tempcoords = null;
                return _factory.CreateLineString(_ringPts);
            }
        }

        /// <summary>
        /// Returns this ring as a LinearRing, or null if an Exception occurs while
        /// creating it (such as a topology problem).
        /// </summary>
        public LinearRing Ring
        {
            get
            {
                if (_ring != null)
                    return _ring;
                var tempcoords = Coordinates;
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

        private Envelope Envelope => Ring.EnvelopeInternal;

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
            get => IsHole ? _shell : this;
            private set => _shell = value;
        }

        /// <summary>
        /// Gets a value indicating whether this ring has a shell assigned to it.
        /// </summary>
        public bool HasShell => _shell != null;

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
        public bool IsOuterShell => OuterHole != null;

        /// <summary>
        /// Gets the outer hole of a shell, if it has one.
        /// An outer hole is one that is not contained
        /// in any other shell.
        /// Each disjoint connected group of shells
        /// is surrounded by an outer hole.
        /// </summary>
        /// <returns>The outer hole edge ring, or <c>null</c></returns>
        public EdgeRing OuterHole
        {
            get
            {
                /*
                 * Only shells can have outer holes
                 */
                if (IsHole) return null;
                /*
                 * A shell is an outer shell if any edge is also in an outer hole.
                 * A hole is an outer hole if it is not contained by a shell.
                 */
                for (int i = 0; i < _deList.Count; i++)
                {
                    var de = (PolygonizeDirectedEdge) _deList[i];
                    var adjRing = ((PolygonizeDirectedEdge) de.Sym).Ring;
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
                var de = (PolygonizeDirectedEdge) _deList[i];
                var adjShell = ((PolygonizeDirectedEdge) de.Sym).Ring.Shell;

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
        public override string ToString()
        {
            return WKTWriter.ToLineString(new CoordinateArraySequence(Coordinates));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this ring has been processed.
        /// </summary>
        public bool IsProcessed
        {
            get => _isProcessed;
            set => _isProcessed = value;
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

        /// <summary>
        /// Compares EdgeRings based on the area of their envelopes.
        /// Smaller envelopes sort before bigger ones.
        /// This effectively sorts EdgeRings in order of containment.
        /// </summary>
        /// <author>mbdavis</author>
        public class EnvelopeAreaComparator : IComparer<EdgeRing>
        {
            public int Compare(EdgeRing r0, EdgeRing r1)
            {
                return r0.Ring.Envelope.Area.CompareTo(r1.Ring.Envelope.Area);
            }
        }

    }
}

