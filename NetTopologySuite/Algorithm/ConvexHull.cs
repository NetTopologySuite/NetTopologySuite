using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the convex hull of a <see cref="Geometry{TCoordinate}" />.
    /// The convex hull is the smallest convex Geometry that contains all the
    /// points in the input Geometry.
    /// Uses the Graham Scan algorithm.
    /// </summary>
    public class ConvexHull<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                             IComputable<TCoordinate>, IConvertible
    {
        private static IEnumerable<TCoordinate> ExtractCoordinates(IGeometry<TCoordinate> geom)
        {
            UniqueCoordinateArrayFilter<TCoordinate> filter = new UniqueCoordinateArrayFilter<TCoordinate>();
            geom.Apply(filter);
            return filter.Coordinates;
        }

        private readonly IGeometryFactory<TCoordinate> _geomFactory = null;
        private readonly IEnumerable<TCoordinate> _inputPts = null;

        /// <summary> 
        /// Create a new convex hull construction for the input <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        public ConvexHull(IGeometry<TCoordinate> geometry)
            : this(ExtractCoordinates(geometry), geometry.Factory) {}

        /// <summary>
        /// Create a new convex hull construction for the input 
        /// <typeparamref name="TCoordinate"/> set.
        /// </summary>
        public ConvexHull(IEnumerable<TCoordinate> points, IGeometryFactory<TCoordinate> geomFactory)
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
            if (_inputPts.Length == 0)
            {
                return _geomFactory.CreateGeometryCollection(null);
            }

            if (_inputPts.Length == 1)
            {
                return _geomFactory.CreatePoint(_inputPts[0]);
            }

            if (_inputPts.Length == 2)
            {
                return _geomFactory.CreateLineString(_inputPts);
            }

            IEnumerable<TCoordinate> reducedPts = _inputPts;
            
            // use heuristic to reduce points, if large
            if (_inputPts.Length > 50)
            {
                reducedPts = Reduce(_inputPts);
            }

            // sort points for Graham scan.
            ICoordinate[] sortedPts = PreSort(reducedPts);

            // Use Graham scan to find convex hull.
            Stack<ICoordinate> cHS = GrahamScan(sortedPts);

            // Convert stack to an array.
            ICoordinate[] cH = cHS.ToArray();

            // Convert array to appropriate output geometry.
            return LineOrPolygon(cH);
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
        private ICoordinate[] Reduce(ICoordinate[] pts)
        {
            ICoordinate[] polyPts = ComputeOctRing(_inputPts);

            // unable to compute interior polygon for some reason
            if (polyPts == null)
            {
                return _inputPts;
            }

            // add points defining polygon
            SortedSet<ICoordinate> reducedSet = new SortedSet<ICoordinate>();

            for (Int32 i = 0; i < polyPts.Length; i++)
            {
                reducedSet.Add(polyPts[i]);
            }

            /*
             * Add all unique points not in the interior poly.
             * CGAlgorithms.IsPointInRing is not defined for points actually on the ring,
             * but this doesn't matter since the points of the interior polygon
             * are forced to be in the reduced set.
             */
            for (Int32 i = 0; i < _inputPts.Length; i++)
            {
                if (!CGAlgorithms.IsPointInRing(_inputPts[i], polyPts))
                {
                    reducedSet.Add(_inputPts[i]);
                }
            }

            ICoordinate[] arr = new ICoordinate[reducedSet.Count];
            reducedSet.CopyTo(arr, 0);
            return arr;
        }

        private ICoordinate[] PreSort(ICoordinate[] pts)
        {
            ICoordinate t;

            // find the lowest point in the set. If two or more points have
            // the same minimum y coordinate choose the one with the minimu x.
            // This focal point is put in array location pts[0].
            for (Int32 i = 1; i < pts.Length; i++)
            {
                if ((pts[i].Y < pts[0].Y) || ((pts[i].Y == pts[0].Y)
                                              && (pts[i].X < pts[0].X)))
                {
                    t = pts[0];
                    pts[0] = pts[i];
                    pts[i] = t;
                }
            }

            // sort the points radially around the focal point.
            Array.Sort(pts, 1, pts.Length - 1, new RadialComparator(pts[0]));
            return pts;
        }

        private Stack<ICoordinate> GrahamScan(ICoordinate[] c)
        {
            ICoordinate p;
            Stack<ICoordinate> ps = new Stack<ICoordinate>(c.Length);
            ps.Push(c[0]);
            ps.Push(c[1]);
            ps.Push(c[2]);

            for (Int32 i = 3; i < c.Length; i++)
            {
                p = ps.Pop();

                while (CGAlgorithms.ComputeOrientation(ps.Peek(), p, c[i]) > 0)
                {
                    p = ps.Pop();
                }

                ps.Push(p);
                ps.Push(c[i]);
            }

            ps.Push(c[0]);
            return ps;
        }

        private Stack<ICoordinate> ReverseStack(Stack<ICoordinate> ps)
        {
            // Do a manual reverse of the stack
            Int32 size = ps.Count;
            ICoordinate[] tempArray = new ICoordinate[size];

            for (Int32 i = 0; i < size; i++)
            {
                tempArray[i] = ps.Pop();
            }

            Stack<ICoordinate> returnStack = new Stack<ICoordinate>(size);

            foreach (ICoordinate obj in tempArray)
            {
                returnStack.Push(obj);
            }

            return returnStack;
        }

        /// <returns>
        /// Whether the three coordinates are collinear 
        /// and c2 lies between c1 and c3 inclusive.
        /// </returns>        
        private Boolean IsBetween(ICoordinate c1, ICoordinate c2, ICoordinate c3)
        {
            if (CGAlgorithms.ComputeOrientation(c1, c2, c3) != 0)
            {
                return false;
            }

            if (c1.X != c3.X)
            {
                if (c1.X <= c2.X && c2.X <= c3.X)
                {
                    return true;
                }
                if (c3.X <= c2.X && c2.X <= c1.X)
                {
                    return true;
                }
            }

            if (c1.Y != c3.Y)
            {
                if (c1.Y <= c2.Y && c2.Y <= c3.Y)
                {
                    return true;
                }
                if (c3.Y <= c2.Y && c2.Y <= c1.Y)
                {
                    return true;
                }
            }

            return false;
        }

        private ICoordinate[] ComputeOctRing(ICoordinate[] inputPts)
        {
            ICoordinate[] octPts = ComputeOctPts(inputPts);
            CoordinateList coordList = new CoordinateList();
            coordList.Add(octPts, false);

            // points must all lie in a line
            if (coordList.Count < 3)
            {
                return null;
            }

            coordList.CloseRing();
            return coordList.ToCoordinateArray();
        }

        private ICoordinate[] ComputeOctPts(ICoordinate[] inputPts)
        {
            ICoordinate[] pts = new ICoordinate[8];

            for (Int32 j = 0; j < pts.Length; j++)
            {
                pts[j] = inputPts[0];
            }

            for (Int32 i = 1; i < inputPts.Length; i++)
            {
                if (inputPts[i].X < pts[0].X)
                {
                    pts[0] = inputPts[i];
                }

                if (inputPts[i].X - inputPts[i].Y < pts[1].X - pts[1].Y)
                {
                    pts[1] = inputPts[i];
                }

                if (inputPts[i].Y > pts[2].Y)
                {
                    pts[2] = inputPts[i];
                }

                if (inputPts[i].X + inputPts[i].Y > pts[3].X + pts[3].Y)
                {
                    pts[3] = inputPts[i];
                }

                if (inputPts[i].X > pts[4].X)
                {
                    pts[4] = inputPts[i];
                }

                if (inputPts[i].X - inputPts[i].Y > pts[5].X - pts[5].Y)
                {
                    pts[5] = inputPts[i];
                }

                if (inputPts[i].Y < pts[6].Y)
                {
                    pts[6] = inputPts[i];
                }

                if (inputPts[i].X + inputPts[i].Y < pts[7].X + pts[7].Y)
                {
                    pts[7] = inputPts[i];
                }
            }

            return pts;
        }

        /// <param name="coordinates"> The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>A 2-vertex <c>LineString</c> if the vertices are collinear; 
        /// otherwise, a <see cref="Polygon{TCoordinate}" /> with unnecessary (collinear) vertices removed. </returns>       
        private IGeometry LineOrPolygon(ICoordinate[] coordinates)
        {
            coordinates = CleanRing(coordinates);
           
            if (coordinates.Length == 3)
            {
                return _geomFactory.CreateLineString(new ICoordinate[] {coordinates[0], coordinates[1]});
            }

            ILinearRing linearRing = _geomFactory.CreateLinearRing(coordinates);
            return _geomFactory.CreatePolygon(linearRing, null);
        }

        /// <param name="original">The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>The coordinates with unnecessary (collinear) vertices removed.</returns>
        private ICoordinate[] CleanRing(ICoordinate[] original)
        {
            Equals(original[0], original[original.Length - 1]);
            List<ICoordinate> cleanedRing = new List<ICoordinate>();
            ICoordinate previousDistinctCoordinate = null;
           
            for (Int32 i = 0; i <= original.Length - 2; i++)
            {
                ICoordinate currentCoordinate = original[i];
                ICoordinate nextCoordinate = original[i + 1];
              
                if (currentCoordinate.Equals(nextCoordinate))
                {
                    continue;
                }
              
                if (previousDistinctCoordinate != null &&
                    IsBetween(previousDistinctCoordinate, currentCoordinate, nextCoordinate))
                {
                    continue;
                }
               
                cleanedRing.Add(currentCoordinate);
                previousDistinctCoordinate = currentCoordinate;
            }

            cleanedRing.Add(original[original.Length - 1]);
            return cleanedRing.ToArray();
        }

        /// <summary>
        /// Compares <see cref="Coordinate" />s for their angle and distance
        /// relative to an origin.
        /// </summary>
        private class RadialComparator : IComparer<ICoordinate>
        {
            private ICoordinate origin = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="RadialComparator"/> class.
            /// </summary>
            public RadialComparator(ICoordinate origin)
            {
                this.origin = origin;
            }

            public Int32 Compare(ICoordinate p1, ICoordinate p2)
            {
                return PolarCompare(origin, p1, p2);
            }

            private static Int32 PolarCompare(ICoordinate o, ICoordinate p, ICoordinate q)
            {
                Double dxp = p.X - o.X;
                Double dyp = p.Y - o.Y;
                Double dxq = q.X - o.X;
                Double dyq = q.Y - o.Y;

                Int32 orient = CGAlgorithms.ComputeOrientation(o, p, q);

                if (orient == CGAlgorithms.CounterClockwise)
                {
                    return 1;
                }

                if (orient == CGAlgorithms.Clockwise)
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
    }
}