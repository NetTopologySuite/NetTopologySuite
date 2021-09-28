using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    internal sealed class OverlayEdgeRing
    {
        private IndexedPointInAreaLocator _locator;
        private OverlayEdgeRing _shell;
        private readonly List<OverlayEdgeRing> _holes = new List<OverlayEdgeRing>(); // a list of EdgeRings which are holes in this EdgeRing

        public OverlayEdgeRing(OverlayEdge start, GeometryFactory geometryFactory)
        {
            Edge = start;
            Coordinates = ComputeRingPts(start);
            ComputeRing(Coordinates, geometryFactory);
        }

        public LinearRing Ring { get; private set; }

        /// <summary>
        /// Tests whether this ring is a hole.
        /// </summary>
        /// <value><c>true</c> if this ring is a hole</value>
        public bool IsHole { get; private set; }

        /// <summary>
        /// Tests whether this ring has a shell assigned to it.
        /// </summary>
        /// <value><c>true</c> if the ring has a shell</value>
        public bool HasShell
        {
            get => _shell != null;
        }

        /// <summary>
        /// Gets or sets a value indicating the shell for this ring.<br/>
        /// The shell is the ring itself if it is not a hole, otherwise its parent shell.
        /// </summary>
        public OverlayEdgeRing Shell
        {
            get { return IsHole ? _shell : this; }
            set
            {
                _shell = value;
                if (_shell != null) _shell.AddHole(this);
            }
        }

        public void AddHole(OverlayEdgeRing ring) { _holes.Add(ring); }

        private Coordinate[] ComputeRingPts(OverlayEdge start)
        {
            var edge = start;
            var pts = new CoordinateList();
            do
            {
                if (edge.EdgeRing == this)
                    throw new TopologyException("Edge visited twice during ring-building at " + edge.Coordinate, edge.Coordinate);

                //edges.add(de);
                //Debug.println(de);
                //Debug.println(de.getEdge());

                // only valid for polygonal output
                //Assert.isTrue(edge.getLabel().isBoundaryEither());

                edge.AddCoordinates(pts);
                edge.EdgeRing = this;
                if (edge.NextResult == null)
                    throw new TopologyException("Found null edge in ring", edge.Dest);

                edge = edge.NextResult;
            } while (edge != start);
            pts.CloseRing();
            return pts.ToCoordinateArray();
        }

        private void ComputeRing(Coordinate[] ringPts, GeometryFactory geometryFactory)
        {
            if (Ring != null) return;   // don't compute more than once
            Ring = geometryFactory.CreateLinearRing(ringPts);
            IsHole = Orientation.IsCCW(Ring.Coordinates);
        }

        /// <summary>
        /// Computes the list of coordinates which are contained in this ring.
        /// The coordinates are computed once only and cached.
        /// </summary>
        /// <value>An array of the <see cref="Coordinate"/>s in this ring</value>
        private Coordinate[] Coordinates { get; }

        /// <summary>
        /// Finds the innermost enclosing shell OverlayEdgeRing
        /// containing this OverlayEdgeRing, if any.
        /// The innermost enclosing ring is the <i>smallest</i> enclosing ring.
        /// The algorithm used depends on the fact that:
        /// <br/>
        /// ring A contains ring B if envelope(ring A) contains envelope(ring B)
        /// <br/>
        /// This routine is only safe to use if the chosen point of the hole
        /// is known to be properly contained in a shell
        /// (which is guaranteed to be the case if the hole does not touch its shell)
        /// <para/>
        /// To improve performance of this function the caller should 
        /// make the passed shellList as small as possible (e.g.
        /// by using a spatial index filter beforehand).
        /// </summary>
        /// <returns>The containing EdgeRing, if there is one
        /// or <c>null</c> if no containing EdgeRing is found
        /// </returns>
        public OverlayEdgeRing FindEdgeRingContaining(IEnumerable<OverlayEdgeRing> erList)
        {
            var testRing = this.Ring;
            var testEnv = testRing.EnvelopeInternal;
            var testPt = testRing.GetCoordinateN(0);

            OverlayEdgeRing minRing = null;
            Envelope minRingEnv = null;
            foreach (var tryEdgeRing in erList)
            {
                var tryRing = tryEdgeRing.Ring;
                var tryShellEnv = tryRing.EnvelopeInternal;
                // the hole envelope cannot equal the shell envelope
                // (also guards against testing rings against themselves)
                if (tryShellEnv.Equals(testEnv)) continue;

                // hole must be contained in shell
                if (!tryShellEnv.Contains(testEnv)) continue;

                testPt = CoordinateArrays.PointNotInList(testRing.Coordinates, tryEdgeRing.Coordinates);

                bool isContained = tryEdgeRing.IsInRing(testPt);

                // check if the new containing ring is smaller than the current minimum ring
                if (isContained)
                {
                    if (minRing == null
                        || minRingEnv.Contains(tryShellEnv))
                    {
                        minRing = tryEdgeRing;
                        minRingEnv = minRing.Ring.EnvelopeInternal;
                    }
                }
            }
            return minRing;
        }

        private IPointOnGeometryLocator GetLocator()
        {
            if (_locator == null)
            {
                _locator = new IndexedPointInAreaLocator(Ring);
            }
            return _locator;
        }

        public bool IsInRing(Coordinate pt)
        {
            /*
             * Use an indexed point-in-polygon for performance
             */
            return Location.Exterior != GetLocator().Locate(pt);
            //return PointLocation.isInRing(pt, getCoordinates());
        }

        public Coordinate Coordinate
        {
            get => Coordinates[0];
        }

        /// <summary>
        /// Computes the <see cref="Polygon"/> formed by this ring and any contained holes.
        /// </summary>
        /// <returns>The <see cref="Polygon"/> formed by this ring and its holes.</returns>
        public Polygon ToPolygon(GeometryFactory factory)
        {
            LinearRing[] holeLR = null;
            if (_holes != null)
            {
                holeLR = new LinearRing[_holes.Count];
                for (int i = 0; i < _holes.Count; i++)
                {
                    holeLR[i] = _holes[i].Ring;
                }
            }
            var poly = factory.CreatePolygon(Ring, holeLR);
            return poly;
        }

        public OverlayEdge Edge { get; }
    }
}
