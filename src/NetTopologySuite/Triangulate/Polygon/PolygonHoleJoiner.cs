using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.Linq;
namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Transforms a polygon with holes into a single self-touching ring
    /// by connecting holes to the exterior shell or to another hole.
    /// The holes are added from the lowest upwards.
    /// As the resulting shell develops, a hole might be added to what was
    /// originally another hole.
    /// </summary>
    class PolygonHoleJoiner
    {

        public static Geometries.Polygon JoinAsPolygon(Geometries.Polygon inputPolygon)
        {
            return inputPolygon.Factory.CreatePolygon(Join(inputPolygon));
        }

        public static Coordinate[] Join(Geometries.Polygon inputPolygon)
        {
            var joiner = new PolygonHoleJoiner(inputPolygon);
            return joiner.Compute();
        }

        private const double EPS = 1.0E-4;

        private List<Coordinate> shellCoords;
        // orderedCoords is a copy of shellCoords for sort purposes
        private SortedSetEx<Coordinate> orderedCoords;

        // Key: starting end of the cut; Value: list of the other end of the cut
        private Dictionary<Coordinate, List<Coordinate>> cutMap;
        private ISegmentSetMutualIntersector polygonIntersector;

        private Geometries.Polygon inputPolygon;

        public PolygonHoleJoiner(Geometries.Polygon inputPolygon)
        {
            this.inputPolygon = inputPolygon;
            polygonIntersector = CreatePolygonIntersector(inputPolygon);
        }

        /// <summary>
        /// Computes the joined ring.
        /// </summary>
        /// <returns>The points in the joined ring</returns>
        public Coordinate[] Compute()
        {
            //--- copy the input polygon shell coords
            shellCoords = RingCoordinates(inputPolygon.ExteriorRing);
            if (inputPolygon.NumInteriorRings != 0)
            {
                JoinHoles();
            }
            return shellCoords.ToArray();
        }

        private static List<Coordinate> RingCoordinates(LineString ring)
        {
            var coords = ring.Coordinates;
            var coordList = new List<Coordinate>();
            foreach (var p in coords)
            {
                coordList.Add(p);
            }
            return coordList;
        }

        private void JoinHoles()
        {
            orderedCoords = new SortedSetEx<Coordinate>();
            foreach (var coord in shellCoords)
                orderedCoords.Add(coord);

            cutMap = new Dictionary<Coordinate, List<Coordinate>>();
            var orderedHoles = sortHoles(inputPolygon);
            for (int i = 0; i < orderedHoles.Count; i++)
            {
                JoinHole(orderedHoles[i]);
            }
        }

        /// <summary>
        /// Joins a single hole to the current shellRing.
        /// </summary>
        /// <param name="hole">The hole to join</param>
        private void JoinHole(LinearRing hole)
        {
            /*
             * 1) Get a list of HoleVertex Index. 
             * 2) Get a list of ShellVertex. 
             * 3) Get the pair that has the shortest distance between them. 
             * This pair is the endpoints of the cut 
             * 4) The selected ShellVertex may occurs multiple times in
             * shellCoords[], so find the proper one and add the hole after it.
             */
            var holeCoords = hole.Coordinates;
            var holeLeftVerticesIndex = GetLeftMostVertex(hole);
            var holeCoord = holeCoords[holeLeftVerticesIndex[0]];
            var shellCoordsList = GetLeftShellVertex(holeCoord);
            var shellCoord = shellCoordsList[0];
            int shortestHoleVertexIndex = 0;
            //--- pick the shell-hole vertex pair that gives the shortest distance
            if (Math.Abs(shellCoord.X - holeCoord.X) < EPS)
            {
                double shortest = double.MaxValue;
                for (int i = 0; i < holeLeftVerticesIndex.Count; i++)
                {
                    for (int j = 0; j < shellCoordsList.Count; j++)
                    {
                        double currLength = Math.Abs(shellCoordsList[j].Y - holeCoords[holeLeftVerticesIndex[i]].Y);
                        if (currLength < shortest)
                        {
                            shortest = currLength;
                            shortestHoleVertexIndex = i;
                            shellCoord = shellCoordsList[j];
                        }
                    }
                }
            }
            int shellVertexIndex = GetShellCoordIndex(shellCoord,
                holeCoords[holeLeftVerticesIndex[shortestHoleVertexIndex]]);
            AddHoleToShell(shellVertexIndex, holeCoords, holeLeftVerticesIndex[shortestHoleVertexIndex]);
        }

        /// <summary>
        /// Get the i'th <paramref name="shellVertex"/> in <see cref="shellCoords"/> that the current should add after
        /// </summary>
        /// <param name="shellVertex">The coordinate of the shell vertex</param>
        /// <param name="holeVertex">The coordinate of the hole vertex</param>
        /// <returns>The i'th shellvertex</returns>
        private int GetShellCoordIndex(Coordinate shellVertex, Coordinate holeVertex)
        {
            int numSkip = 0;
            var newValueList = new List<Coordinate>();
            newValueList.Add(holeVertex);
            if (cutMap.ContainsKey(shellVertex))
            {
                foreach (var coord in cutMap[shellVertex])
                {
                    if (coord.Y < holeVertex.Y)
                    {
                        numSkip++;
                    }
                }
                cutMap[shellVertex].Add(holeVertex);
            }
            else
            {
                cutMap[shellVertex] = newValueList;
            }
            if (!cutMap.ContainsKey(holeVertex))
            {
                cutMap.Add(holeVertex, new List<Coordinate>(newValueList));
            }
            return GetShellCoordIndexSkip(shellVertex, numSkip);
        }

        /// <summary>
        /// Find the index of the coordinate in ShellCoords ArrayList,
        /// skipping over some number of matches
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="numSkip"></param>
        /// <returns></returns>
        private int GetShellCoordIndexSkip(Coordinate coord, int numSkip)
        {
            for (int i = 0; i < shellCoords.Count; i++)
            {
                if (shellCoords[i].Equals2D(coord, EPS))
                {
                    if (numSkip == 0)
                        return i;
                    numSkip--;
                }
            }
            throw new ArgumentException("Vertex is not in shellcoords", nameof(coord));
        }

        /// <summary>
        /// Gets a list of shell vertices that could be used to join with the hole.
        /// This list contains only one item if the chosen vertex does not share the same
        /// x value with <paramref name="holeCoord"/>
        /// </summary>
        /// <param name="holeCoord">The hole coordinate</param>
        /// <returns>A list of candidate join vertices</returns>
        private List<Coordinate> GetLeftShellVertex(Coordinate holeCoord)
        {
            var list = new List<Coordinate>();
            var closest = orderedCoords.Above(holeCoord);
            while (closest.X == holeCoord.X) {
                closest = orderedCoords.Above(closest);
            }
            do {
                closest = orderedCoords.Below(closest);
            } while (!IsJoinable(holeCoord, closest) && !closest.Equals(orderedCoords.Min));
            list.Add(closest);
            if (closest.X != holeCoord.X)
                return list;
            double chosenX = closest.X;
            list.Clear();
            while (chosenX == closest.X)
            {
                list.Add(closest);
                closest = orderedCoords.Below(closest);
                if (closest == null)
                    return list;
            }
            return list;
        }

        /**
         * Determine if a line segment between a hole vertex
         * and a shell vertex lies inside the input polygon.
         * 
         * @param holeCoord a hole coordinate
         * @param shellCoord a shell coordinate
         * @return true if the line lies inside the polygon
         */

        private bool IsJoinable(Coordinate holeCoord, Coordinate shellCoord)
        {
            /*
             * Since the line runs between a hole and the shell,
             * it is inside the polygon if it does not cross the polygon boundary.
             */
            bool isJoinable = !CrossesPolygon(holeCoord, shellCoord);
            /*
            //--- slow code for testing only
            LineString join = geomFact.createLineString(new Coordinate[] { holeCoord, shellCoord });
            boolean isJoinableSlow = inputPolygon.covers(join)
            if (isJoinableSlow != isJoinable) {
              System.out.println(WKTWriter.toLineString(holeCoord, shellCoord));
            }
            //Assert.isTrue(isJoinableSlow == isJoinable);
            */
            return isJoinable;
        }

        /**
         * Tests whether a line segment crosses the polygon boundary.
         * 
         * @param p0 a vertex
         * @param p1 a vertex
         * @return true if the line segment crosses the polygon boundary
         */
        private bool CrossesPolygon(Coordinate p0, Coordinate p1)
        {
            var segString = new BasicSegmentString(
                new Coordinate[] { p0, p1 }, null);
            var segStrings = new List<ISegmentString>();
            segStrings.Add(segString);

            var segInt = new SegmentIntersectionDetector();
            segInt.FindProper = true;
            polygonIntersector.Process(segStrings, segInt);

            return segInt.HasProperIntersection;
        }

        /**
         * Add hole at proper position in shell coordinate list.
         * Also adds hole points to ordered coordinates.
         * 
         * @param shellVertexIndex
         * @param holeCoords
         * @param holeVertexIndex
         */
        private void AddHoleToShell(int shellVertexIndex, Coordinate[] holeCoords, int holeVertexIndex)
        {
            var newCoords = new List<Coordinate>();
            newCoords.Add(new Coordinate(shellCoords[shellVertexIndex]));
            int nPts = holeCoords.Length - 1;
            int i = holeVertexIndex;
            do
            {
                newCoords.Add(new Coordinate(holeCoords[i]));
                i = (i + 1) % nPts;
            } while (i != holeVertexIndex);
            newCoords.Add(new Coordinate(holeCoords[holeVertexIndex]));
            shellCoords.InsertRange(shellVertexIndex, newCoords);
            foreach (var coord in newCoords)
                orderedCoords.Add(coord);
        }

        /**
         * Sort the holes by minimum X, minimum Y.
         * 
         * @param poly polygon that contains the holes
         * @return a list of ordered hole geometry
         */
        private List<LinearRing> sortHoles(Geometries.Polygon poly)
        {
            var holes = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                holes.Add((LinearRing)poly.GetInteriorRingN(i));
            }
            holes.Sort(EnvelopeComparer.Instance);
            return holes;
        }

        /**
         * Gets a list of indices of the leftmost vertices in a ring.
         * 
         * @param geom the hole ring
         * @return index of the left most vertex
         */
        private List<int> GetLeftMostVertex(LinearRing ring)
        {
            var coords = ring.Coordinates;
            var list = new List<int>();
            double minX = ring.EnvelopeInternal.MinX;
            for (int i = 0; i < coords.Length; i++)
            {
                if (Math.Abs(coords[i].X - minX) < EPS)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        private ISegmentSetMutualIntersector CreatePolygonIntersector(Geometries.Polygon polygon)
        {
            var polySegStrings = SegmentStringUtil.ExtractSegmentStrings(polygon);
            return new MCIndexSegmentSetMutualIntersector(polySegStrings);
        }

        /**
         * 
         * @author mdavis
         *
         */
        private class EnvelopeComparer : IComparer<Geometry>
        {
            public static EnvelopeComparer Instance = new EnvelopeComparer();

            private EnvelopeComparer() { }
            public int Compare(Geometry o1, Geometry o2)
            {
                var e1 = o1.EnvelopeInternal;
                var e2 = o2.EnvelopeInternal;
                return e1.CompareTo(e2);
            }
        }
    }
}
