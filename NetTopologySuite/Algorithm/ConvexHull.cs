using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
#if !DOTNET40
using GeoAPI.DataStructures.Collections.Generic;
#endif
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

#if DOTNET35
using sl = System.Linq;
#endif

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the convex hull of a <see cref="Geometry{TCoordinate}" />.
    /// The convex hull is the smallest convex Geometry that contains all the
    /// points in the input Geometry.
    /// Uses the Graham Scan algorithm.
    /// </summary>
    public class ConvexHull<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        //private static IEnumerable<TCoordinate> extractCoordinates(IGeometry<TCoordinate> geom)
        //{
        //    UniqueCoordinateArrayFilter<TCoordinate> filter = new UniqueCoordinateArrayFilter<TCoordinate>();
        //    geom.Apply(filter);
        //    return filter.Coordinates;
        //}

        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private readonly ICoordinateSequence<TCoordinate> _inputPts;

        /// <summary> 
        /// Create a new convex hull construction for the input <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        public ConvexHull(IGeometry<TCoordinate> geometry)
            : this(geometry.Coordinates.WithoutDuplicatePoints(), geometry.Factory)
        {
        }

        /// <summary>
        /// Create a new convex hull construction for the input 
        /// <typeparamref name="TCoordinate"/> set.
        /// </summary>
        public ConvexHull(ICoordinateSequence<TCoordinate> points, IGeometryFactory<TCoordinate> geomFactory)
        {
            _inputPts = points;
            _geomFactory = geomFactory;
        }

        /// <summary> 
        /// Returns a <see cref="Geometry{TCoordinate}"/> that represents the convex hull of the input point.
        /// The point will contain the minimal number of points needed to
        /// represent the convex hull.  In particular, no more than two consecutive
        /// points will be collinear.
        /// </summary>
        /// <returns> 
        /// If the convex hull contains 3 or more points, a <see cref="Polygon{TCoordinate}" />;
        /// 2 points, a <c>LineString</c>;
        /// 1 point, a <c>Point</c>;
        /// 0 points, an empty <see cref="GeometryCollection{TCoordinate}" />.
        /// </returns>
        public IGeometry<TCoordinate> GetConvexHull()
        {
            if (_inputPts.Count == 0)
            {
                return _geomFactory.CreateGeometryCollection(null);
            }

            if (_inputPts.Count == 1)
            {
                return _geomFactory.CreatePoint(_inputPts[0]);
            }

            if (_inputPts.Count == 2)
            {
                return _geomFactory.CreateLineString(_inputPts);
            }

            ICoordinateSequence<TCoordinate> reducedPts = _inputPts;

            // use heuristic to reduce points, if large
            if (_inputPts.Count > 50)
            {
                reducedPts = reduce(_inputPts);
            }

            // sort points for Graham scan.
            IEnumerable<TCoordinate> sortedPts = preSort(reducedPts);

            // Use Graham scan to find convex hull.
            Stack<TCoordinate> cHS = grahamScan(sortedPts);

            // Convert array to appropriate output geometry.
            return lineOrPolygon(cHS);
        }

        /// <summary>
        /// Uses a heuristic to reduce the number of points scanned to compute the hull.
        /// The heuristic is to find a polygon guaranteed to
        /// be in (or on) the hull, and eliminate all points inside it.
        /// A quadrilateral defined by the extremal points
        /// in the four orthogonal directions
        /// can be used, but even more inclusive is
        /// to use an octilateral defined by the points in the 8 cardinal directions.
        /// Note that even if the method used to determine the polygon vertices
        /// is not 100% robust, this does not affect the robustness of the convex hull.
        /// </summary>
        private ICoordinateSequence<TCoordinate> reduce(ICoordinateSequence<TCoordinate> pts)
        {
            IEnumerable<TCoordinate> polyPts = computeOctRing(pts);

            // unable to compute interior polygon for some reason
            if (polyPts == null)
            {
                return pts;
            }

            // add points defining polygon
            SortedSet<TCoordinate> reducedSet = new SortedSet<TCoordinate>();

            foreach (TCoordinate pt in polyPts)
            {
                reducedSet.Add(pt);
            }

            /*
             * Add all unique points not in the interior poly.
             * CGAlgorithms.IsPointInRing is not defined for points actually on the ring,
             * but this doesn't matter since the points of the interior polygon
             * are forced to be in the reduced set.
             */
            foreach (TCoordinate pt in pts)
            {
                if (!CGAlgorithms<TCoordinate>.IsPointInRing(pt, polyPts))
                {
                    reducedSet.Add(pt);
                }
            }

            return _geomFactory.CoordinateSequenceFactory.Create(reducedSet);
        }

        private static ICoordinateSequence<TCoordinate> preSort(ICoordinateSequence<TCoordinate> pts)
        {
            // find the lowest point in the set. If two or more points have
            // the same minimum y coordinate choose the one with the minimum x.
            // This focal point is put in array location first.
            for (int i = 1; i < pts.Count; i++)
            {
                TCoordinate coordinate = pts[i];

                if ((coordinate[Ordinates.Y] < pts[0][Ordinates.Y]) ||
                    ((coordinate[Ordinates.Y] == pts[0][Ordinates.Y]) && (coordinate[Ordinates.X] < pts[0][Ordinates.X])))
                {
                    TCoordinate t = pts[0];
                    pts[0] = coordinate;
                    pts[i] = t;
                }
            }

            // sort the points radially around the focal point.
            pts.Sort(1, pts.Count - 1, new RadialComparator(pts[0]));
            return pts;
        }

        private static Stack<TCoordinate> grahamScan(IEnumerable<TCoordinate> c)
        {
            TCoordinate p;
            Stack<TCoordinate> ps = new Stack<TCoordinate>();
            Triple<TCoordinate> triple = Slice.GetTriple(c).Value;

            ps.Push(triple.First);
            ps.Push(triple.Second);
            ps.Push(triple.Third);

            foreach (TCoordinate coordinate in sl.Enumerable.Skip(c, 3))
            {
                p = ps.Pop();

                while (CGAlgorithms<TCoordinate>.ComputeOrientation(ps.Peek(), p, coordinate) > 0)
                {
                    p = ps.Pop();
                }

                ps.Push(p);
                ps.Push(coordinate);
            }

            ps.Push(triple.First);
            return ps;
        }

        //private Stack<TCoordinate> reverseStack(Stack<TCoordinate> ps)
        //{
        //    // Do a manual reverse of the stack
        //    Int32 size = ps.TotalItemCount;
        //    ICoordinate[] tempArray = new ICoordinate[size];

        //    for (Int32 i = 0; i < size; i++)
        //    {
        //        tempArray[i] = ps.Pop();
        //    }

        //    Stack<ICoordinate> returnStack = new Stack<ICoordinate>(size);

        //    foreach (ICoordinate obj in tempArray)
        //    {
        //        returnStack.Push(obj);
        //    }

        //    return returnStack;
        //}

        /// <returns>
        /// Whether the three coordinates are collinear 
        /// and c2 lies between c1 and c3 inclusive.
        /// </returns>        
        private static Boolean isBetween(TCoordinate c1, TCoordinate c2, TCoordinate c3)
        {
            if (CGAlgorithms<TCoordinate>.ComputeOrientation(c1, c2, c3) != 0)
            {
                return false;
            }

            if (c1[Ordinates.X] != c3[Ordinates.X])
            {
                if (c1[Ordinates.X] <= c2[Ordinates.X] && c2[Ordinates.X] <= c3[Ordinates.X])
                {
                    return true;
                }

                if (c3[Ordinates.X] <= c2[Ordinates.X] && c2[Ordinates.X] <= c1[Ordinates.X])
                {
                    return true;
                }
            }

            if (c1[Ordinates.Y] != c3[Ordinates.Y])
            {
                if (c1[Ordinates.Y] <= c2[Ordinates.Y] && c2[Ordinates.Y] <= c3[Ordinates.Y])
                {
                    return true;
                }

                if (c3[Ordinates.Y] <= c2[Ordinates.Y] && c2[Ordinates.Y] <= c1[Ordinates.Y])
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<TCoordinate> computeOctRing(IEnumerable<TCoordinate> inputPts)
        {
            IEnumerable<TCoordinate> octPts = computeOctPts(inputPts);
            ICoordinateSequence<TCoordinate> coords
                = _geomFactory.CoordinateSequenceFactory.Create(octPts, false);

            // points must all lie in a line
            if (coords.Count < 3)
            {
                return null;
            }

            coords.CloseRing();
            return coords;
        }

        private static IEnumerable<TCoordinate> computeOctPts(IEnumerable<TCoordinate> inputPts)
        {
            TCoordinate[] pts = new TCoordinate[8];

            TCoordinate first = Slice.GetFirst(inputPts);

            for (Int32 j = 0; j < pts.Length; j++)
            {
                pts[j] = first;
            }

            foreach (TCoordinate coordinate in inputPts)
            {
                Double x = coordinate[Ordinates.X];
                Double y = coordinate[Ordinates.Y];

                if (x < pts[0][Ordinates.X])
                {
                    pts[0] = coordinate;
                }

                if (x - y < pts[1][Ordinates.X] - pts[1][Ordinates.Y])
                {
                    pts[1] = coordinate;
                }

                if (y > pts[2][Ordinates.Y])
                {
                    pts[2] = coordinate;
                }

                if (x + y > pts[3][Ordinates.X] + pts[3][Ordinates.Y])
                {
                    pts[3] = coordinate;
                }

                if (x > pts[4][Ordinates.X])
                {
                    pts[4] = coordinate;
                }

                if (x - y > pts[5][Ordinates.X] - pts[5][Ordinates.Y])
                {
                    pts[5] = coordinate;
                }

                if (y < pts[6][Ordinates.Y])
                {
                    pts[6] = coordinate;
                }

                if (x + y < pts[7][Ordinates.X] + pts[7][Ordinates.Y])
                {
                    pts[7] = coordinate;
                }
            }

            return pts;
        }

        /// <param name="coordinates"> The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>
        /// A 2-vertex <c>LineString</c> if the vertices are collinear; 
        /// otherwise, a <see cref="Polygon{TCoordinate}" /> with unnecessary (collinear) vertices removed.
        /// </returns>       
        private IGeometry<TCoordinate> lineOrPolygon(IEnumerable<TCoordinate> coordinates)
        {
            coordinates = cleanRing(coordinates);

            if (!Slice.CountGreaterThan(coordinates, 3))
            {
                Pair<TCoordinate> points = Slice.GetPair(coordinates).Value;
                return _geomFactory.CreateLineString(points.First, points.Second);
            }

            ILinearRing<TCoordinate> linearRing = _geomFactory.CreateLinearRing(coordinates);
            return _geomFactory.CreatePolygon(linearRing, null);
        }

        /// <param name="original">The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>The coordinates with unnecessary (collinear) vertices removed.</returns>
        private IEnumerable<TCoordinate> cleanRing(IEnumerable<TCoordinate> original)
        {
            Debug.Assert(Equals(Slice.GetFirst(original), Slice.GetLast(original)));

            List<TCoordinate> cleanedRing = new List<TCoordinate>();
            TCoordinate previousDistinctCoordinate = default(TCoordinate);

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(original))
            {
                TCoordinate currentCoordinate = pair.First;
                TCoordinate nextCoordinate = pair.Second;

                if (currentCoordinate.Equals(nextCoordinate))
                {
                    continue;
                }

                if (!Coordinates<TCoordinate>.IsEmpty(previousDistinctCoordinate) &&
                    isBetween(previousDistinctCoordinate, currentCoordinate, nextCoordinate))
                {
                    continue;
                }

                cleanedRing.Add(currentCoordinate);
                previousDistinctCoordinate = currentCoordinate;
            }

            cleanedRing.Add(Slice.GetLast(original));
            return cleanedRing;
        }

        #region Nested type: RadialComparator

        /// <summary>
        /// Compares <typeparamref name="TCoordinate" />s for their angle and distance
        /// relative to an origin.
        /// </summary>
        private class RadialComparator : IComparer<TCoordinate>
        {
            private readonly TCoordinate _origin;

            /// <summary>
            /// Initializes a new instance of the <see cref="RadialComparator"/> class.
            /// </summary>
            public RadialComparator(TCoordinate origin)
            {
                _origin = origin;
            }

            #region IComparer<TCoordinate> Members

            public Int32 Compare(TCoordinate p1, TCoordinate p2)
            {
                return polarCompare(_origin, p1, p2);
            }

            #endregion

            private static Int32 polarCompare(TCoordinate o, TCoordinate p, TCoordinate q)
            {
                Double dxp = p[Ordinates.X] - o[Ordinates.X];
                Double dyp = p[Ordinates.Y] - o[Ordinates.Y];
                Double dxq = q[Ordinates.X] - o[Ordinates.X];
                Double dyq = q[Ordinates.Y] - o[Ordinates.Y];

                Orientation orient = CGAlgorithms<TCoordinate>.ComputeOrientation(o, p, q);

                if (orient == Orientation.CounterClockwise)
                {
                    return 1;
                }

                if (orient == Orientation.Clockwise)
                {
                    return -1;
                }

                // points are collinear - check distance
                Double op = dxp*dxp + dyp*dyp;
                Double oq = dxq*dxq + dyq*dyq;

                if (op < oq)
                {
                    return -1;
                }

                if (op > oq)
                {
                    return 1;
                }

                return 0;
            }
        }

        #endregion
    }
}