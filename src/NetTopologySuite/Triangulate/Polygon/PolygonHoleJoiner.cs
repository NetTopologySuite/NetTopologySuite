using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Transforms a polygon with holes into a single self-touching (invalid) ring
    /// by connecting holes to the exterior shell or to another hole.
    /// The holes are added from the lowest upwards.
    /// As the resulting shell develops, a hole might be added to what was
    /// originally another hole.
    /// <para/>
    /// There is no attempt to optimize the quality of the join lines.
    /// In particular, a hole which already touches at a vertex may be
    /// joined at a different vertex.
    /// </summary>
    public class PolygonHoleJoiner
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

        private List<Coordinate> _shellCoords;

        // Note: _orderedCoords is a TreeSet in JTS which has functionality not
        //       provided by dotnet's SortedSet. Thus _orderedCoords is split into
        //       HashSet _orderedCoords and _orderedCoordsArray for which
        //       Above, Below and Min are added.

        // orderedCoords is a copy of shellCoords for sort purposes
        private HashSet<Coordinate> _orderedCoords;
        // orderedCoordsArray is a sorted array of the coordinates stored in orderedCoords
        private Coordinate[] _orderedCoordsArray;

        // Key: starting end of the cut; Value: list of the other end of the cut
        private Dictionary<Coordinate, List<Coordinate>> _cutMap;
        private readonly ISegmentSetMutualIntersector _polygonIntersector;

        private readonly Geometries.Polygon _inputPolygon;

        public PolygonHoleJoiner(Geometries.Polygon inputPolygon)
        {
            _inputPolygon = inputPolygon;
            _polygonIntersector = CreatePolygonIntersector(inputPolygon);
        }

        /// <summary>
        /// Computes the joined ring.
        /// </summary>
        /// <returns>The points in the joined ring</returns>
        public Coordinate[] Compute()
        {
            //--- copy the input polygon shell coords
            _shellCoords = RingCoordinates(_inputPolygon.ExteriorRing);
            if (_inputPolygon.NumInteriorRings != 0)
            {
                JoinHoles();
            }
            return _shellCoords.ToArray();
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
            _orderedCoords = new HashSet<Coordinate>();
            foreach (var coord in _shellCoords)
                AddOrderedCoord(coord);

            _cutMap = new Dictionary<Coordinate, List<Coordinate>>();
            var orderedHoles = SortHoles(_inputPolygon);
            for (int i = 0; i < orderedHoles.Count; i++)
            {
                JoinHole(orderedHoles[i]);
            }
        }

        /// <summary>
        /// Adds a coordinate to the <see cref="_orderedCoords"/> set and
        /// clears the <see cref="_orderedCoordsArray"/> array.
        /// </summary>
        /// <param name="coord">A coordinate</param>
        private void AddOrderedCoord(Coordinate coord)
        {
            _orderedCoords.Add(coord);
            _orderedCoordsArray = null;

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
        /// Get the i'th <paramref name="shellVertex"/> in <see cref="_shellCoords"/> that the current should add after
        /// </summary>
        /// <param name="shellVertex">The coordinate of the shell vertex</param>
        /// <param name="holeVertex">The coordinate of the hole vertex</param>
        /// <returns>The i'th shellvertex</returns>
        private int GetShellCoordIndex(Coordinate shellVertex, Coordinate holeVertex)
        {
            int numSkip = 0;
            var newValueList = new List<Coordinate>();
            newValueList.Add(holeVertex);
            if (_cutMap.ContainsKey(shellVertex))
            {
                foreach (var coord in _cutMap[shellVertex])
                {
                    if (coord.Y < holeVertex.Y)
                    {
                        numSkip++;
                    }
                }
                _cutMap[shellVertex].Add(holeVertex);
            }
            else
            {
                _cutMap[shellVertex] = newValueList;
            }
            if (!_cutMap.ContainsKey(holeVertex))
            {
                _cutMap.Add(holeVertex, new List<Coordinate>(newValueList));
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
            for (int i = 0; i < _shellCoords.Count; i++)
            {
                if (_shellCoords[i].Equals2D(coord, EPS))
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
            if (_orderedCoordsArray == null)
                _orderedCoordsArray = _orderedCoords.OrderBy(x => x.X).ToArray();

            var list = new List<Coordinate>();
            var closest = Above(holeCoord);
            while (closest.X == holeCoord.X) {
                closest = Above(closest);
            }
            do {
                closest = Below(closest);
            } while (!IsJoinable(holeCoord, closest) && !closest.Equals(Min));
            list.Add(closest);
            if (closest.X != holeCoord.X)
                return list;
            double chosenX = closest.X;
            list.Clear();
            while (chosenX == closest.X)
            {
                list.Add(closest);
                closest = Below(closest);
                if (closest == null)
                    return list;
            }
            return list;
        }

        /// <summary>
        /// Determine if a line segment between a hole vertex
        /// and a shell vertex lies inside the input polygon.
        /// </summary>
        /// <param name="holeCoord">A hole coordinate</param>
        /// <param name="shellCoord">A shell coordinate</param>
        /// <returns><c>true</c> if the line lies inside the polygon</returns>
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

        /// <summary>
        /// Tests whether a line segment crosses the polygon boundary.
        /// </summary>
        /// <param name="p0">A vertex</param>
        /// <param name="p1">A vertex</param>
        /// <returns><c>true</c> if the line segment crosses the polygon boundary</returns>
        private bool CrossesPolygon(Coordinate p0, Coordinate p1)
        {
            var segString = new BasicSegmentString(
                new Coordinate[] { p0, p1 }, null);
            var segStrings = new List<ISegmentString>();
            segStrings.Add(segString);

            var segInt = new SegmentIntersectionDetector();
            segInt.FindProper = true;
            _polygonIntersector.Process(segStrings, segInt);

            return segInt.HasProperIntersection;
        }

        /// <summary>
        /// Add hole at proper position in shell coordinate list.
        /// Also adds hole points to ordered coordinates.
        /// </summary>
        /// <param name="shellVertexIndex"></param>
        /// <param name="holeCoords"></param>
        /// <param name="holeVertexIndex"></param>
        private void AddHoleToShell(int shellVertexIndex, Coordinate[] holeCoords, int holeVertexIndex)
        {
            var newCoords = new List<Coordinate>();
            newCoords.Add(new Coordinate(_shellCoords[shellVertexIndex]));
            int nPts = holeCoords.Length - 1;
            int i = holeVertexIndex;
            do
            {
                newCoords.Add(new Coordinate(holeCoords[i]));
                i = (i + 1) % nPts;
            } while (i != holeVertexIndex);
            newCoords.Add(new Coordinate(holeCoords[holeVertexIndex]));
            _shellCoords.InsertRange(shellVertexIndex, newCoords);

            foreach (var coord in newCoords)
                AddOrderedCoord(coord);
        }

        /// <summary>
        /// Sort the holes by minimum X, minimum Y.
        /// </summary>
        /// <param name="poly">Polygon that contains the holes</param>
        /// <returns>A list of ordered hole geometry</returns>
        private List<LinearRing> SortHoles(Geometries.Polygon poly)
        {
            var holes = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                holes.Add((LinearRing)poly.GetInteriorRingN(i));
            }
            holes.Sort(EnvelopeComparer.Instance);
            return holes;
        }

        /// <summary>
        /// Gets a list of indices of the leftmost vertices in a ring.
        /// </summary>
        /// <param name="ring">The hole ring</param>
        /// <returns>Index of the left most vertex</returns>
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

        #region Functionality from TreeSet

        private Coordinate Above(Coordinate coordinate)
        {
            if (_orderedCoordsArray == null)
                throw new InvalidOperationException("_orderedCoordsArray not initialized");

            int index = Array.BinarySearch(_orderedCoordsArray, coordinate);
            if (index < 0)
            {
                // Convert to index of item just higher than coordinate
                index = ~index;
            }
            else
            {
                // We have a match, need to increase index to get next higher value
                index++;
            }

            if (index < _orderedCoordsArray.Length)
                return _orderedCoordsArray[index];
            return null;
        }

        private Coordinate Below(Coordinate coordinate)
        {
            if (_orderedCoordsArray == null)
                throw new InvalidOperationException("_orderedCoordsArray not initialized");

            int index = Array.BinarySearch(_orderedCoordsArray, coordinate);
            if (index < 0)
                index = ~index;

            // We want the index of the item below
            index--;
            if (index >= 0)
                return _orderedCoordsArray[index];
            return null;
        }

        private Coordinate Min
        {
            get
            {
                if (_orderedCoordsArray == null)
                    throw new InvalidOperationException("_orderedCoordsArray not initialized");

                return _orderedCoordsArray[0];
            }
        }

        #endregion
    }
}
