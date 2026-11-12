using System;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Utilities;
using Point = NetTopologySuite.Geometries.Point;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Constructs the Maximum Inscribed Circle for a
    /// polygonal <see cref="Geometry"/>, up to a specified tolerance
    /// (which can be specified or determined automatically).
    /// The Maximum Inscribed Circle is determined by a point in the interior of the area
    /// which has the farthest distance from the area boundary,
    /// along with a boundary point at that distance.
    /// <para/>
    /// In the context of geography the center of the Maximum Inscribed Circle
    /// is known as the <b>Pole of Inaccessibility</b>.
    /// A cartographic use case is to determine a suitable point
    /// to place a map label within a polygon.
    /// <para/>
    /// The radius length of the Maximum Inscribed Circle is a
    /// measure of how "narrow" a polygon is. It is the
    /// distance at which the negative buffer becomes empty.
    /// The class supports testing whether a polygon is "narrower"
    /// than a specified distance via
    /// <see cref="IsRadiusWithin(Geometry, double)"/> or <see cref="IsRadiusWithin(double)"/>.
    /// Testing for the maximum radius is generally much faster
    /// than computing the actual radius value, since short-circuiting
    /// is used to limit the approximation iterations.
    /// <para/>
    /// The class supports polygons with holes and multipolygons.
    /// <para/>
    /// For small polygons (currently triangles and convex quadrilaterals)
    /// the MIC is determined exactly.
    /// For other polygons the implementation uses a successive-approximation technique
    /// over a grid of square cells covering the area geometry.
    /// The grid is refined using a branch-and-bound algorithm.
    /// Point containment and distance are computed in a performant
    /// way by using spatial indexes.
    /// <h3>Future Enhancements</h3>
    /// <list type="bullet">
    /// <item><description>Support a polygonal constraint on placement of center</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <see cref="LargestEmptyCircle"/>
    /// <see cref="InteriorPoint"/>
    /// <see cref="Centroid"/>
    public class MaximumInscribedCircle
    {
        /// <summary>
        /// Computes the center point of the Maximum Inscribed Circle
        /// of a polygonal geometry.
        /// </summary>
        /// <param name="polygonal">A polygonal geometry</param>
        /// <returns>The center point of the maximum inscribed circle</returns>
        public static Point GetCenter(Geometry polygonal)
        {
            var mic = new MaximumInscribedCircle(polygonal);
            return mic.GetCenter();
        }

        /// <summary>
        /// Computes the center point of the Maximum Inscribed Circle
        /// of a polygonal geometry, up to a given tolerance distance.
        /// </summary>
        /// <param name="polygonal">A polygonal geometry</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>The center point of the maximum inscribed circle</returns>
        public static Point GetCenter(Geometry polygonal, double tolerance)
        {
            var mic = new MaximumInscribedCircle(polygonal, tolerance);
            return mic.GetCenter();
        }

        /// <summary>
        /// Computes a radius line of the Maximum Inscribed Circle
        /// of a polygonal geometry.
        /// </summary>
        /// <param name="polygonal">A polygonal geometry</param>
        /// <returns>A 2-point line from the center to a point on the circle</returns>
        public static LineString GetRadiusLine(Geometry polygonal)
        {
            var mic = new MaximumInscribedCircle(polygonal);
            return mic.GetRadiusLine();
        }

        /// <summary>
        /// Computes a radius line of the Maximum Inscribed Circle
        /// of a polygonal geometry, up to a given tolerance distance.
        /// </summary>
        /// <param name="polygonal">A polygonal geometry</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>A line from the center to a point on the circle</returns>
        public static LineString GetRadiusLine(Geometry polygonal, double tolerance)
        {
            var mic = new MaximumInscribedCircle(polygonal, tolerance);
            return mic.GetRadiusLine();
        }

        /// <summary>
        /// Tests if the radius of the maximum inscribed circle
        /// is no longer than the specified distance.
        /// The approximation tolerance is determined automatically
        /// as a fraction of the <paramref name="maxRadius"/> value.
        /// </summary>
        /// <param name="polygonal">A polygonal geometry</param>
        /// <param name="maxRadius">The radius value to test</param>
        /// <returns><c>true</c> if the max in-circle radius is no longer than <paramref name="maxRadius"/></returns>
        public static bool IsRadiusWithin(Geometry polygonal, double maxRadius)
        {
            var mic = new MaximumInscribedCircle(polygonal, -1);
            return mic.IsRadiusWithin(maxRadius);
        }

        /// <summary>
        /// Computes the maximum number of iterations allowed.
        /// Uses a heuristic based on the size of the input geometry
        /// and the tolerance distance.
        /// A smaller tolerance distance allows more iterations.
        /// This is a rough heuristic, intended
        /// to prevent huge iterations for very thin geometries.
        /// </summary>
        internal static long ComputeMaximumIterations(Geometry geom, double toleranceDist)
        {
            double diam = geom.EnvelopeInternal.Diameter;
            double tolDist = toleranceDist <= 0 ? 0.5 * diam * AutoToleranceFraction : toleranceDist;
            double ncells = diam / tolDist;
            //-- Using log of ncells allows control over number of iterations
            int factor = (int)Math.Log(ncells);
            if (factor < 1) factor = 1;
            return 2000 + 2000 * factor;
        }

        //-- used for IsRadiusWithin
        private const double MaxRadiusFraction = 0.0001;

        //-- empirically determined to balance accuracy and speed
        private const double AutoToleranceFraction = 0.001;

        private readonly Geometry _inputGeom;
        private double _tolerance;

        private readonly GeometryFactory _factory;
        private IndexedPointInAreaLocator _ptLocater;
        private IndexedFacetDistance _indexedDistance;
        private Cell _centerCell;
        private Coordinate _centerPt;
        private Coordinate _radiusPt;
        private Point _centerPoint;
        private Point _radiusPoint;
        private double _maximumRadius = -1;

        /// <summary>
        /// Creates a new instance of a Maximum Inscribed Circle computation.
        /// The approximation tolerance is determined automatically.
        /// </summary>
        /// <param name="polygonal">An areal geometry</param>
        /// <exception cref="ArgumentException">Thrown if the input geometry is non-polygonal or empty.</exception>
        public MaximumInscribedCircle(Geometry polygonal)
            : this(polygonal, 0.0)
        {
        }

        /// <summary>
        /// Creates a new instance of a Maximum Inscribed Circle computation,
        /// with an approximation tolerance distance.
        /// A zero tolerance automatically determines an approximation tolerance.
        /// </summary>
        /// <param name="polygonal">An areal geometry</param>
        /// <param name="tolerance">The distance tolerance for computing the centre point.
        /// A zero value triggers auto-tolerance. The value is validated when computation runs.</param>
        /// <exception cref="ArgumentException">Thrown if the input geometry is non-polygonal or empty.</exception>
        public MaximumInscribedCircle(Geometry polygonal, double tolerance)
        {
            if (!(polygonal is IPolygonal))
                throw new ArgumentException("Input geometry must be a Polygon or MultiPolygon");

            if (polygonal.IsEmpty)
                throw new ArgumentException("Empty input geometry is not supported");

            _inputGeom = polygonal;
            _factory = polygonal.Factory;
            _tolerance = tolerance;
        }

        /// <summary>
        /// Tests if the radius of the maximum inscribed circle
        /// is no longer than the specified distance.
        /// This method determines the distance tolerance automatically
        /// as a fraction of <paramref name="maxRadius"/>.
        /// After this method is called, <see cref="GetCenter"/> and <see cref="GetRadiusPoint"/>
        /// provide locations demonstrating where the radius exceeds the specified maximum.
        /// </summary>
        /// <param name="maxRadius">The (non-negative) radius value to test</param>
        /// <returns><c>true</c> if the max in-circle radius is no longer than <paramref name="maxRadius"/></returns>
        public bool IsRadiusWithin(double maxRadius)
        {
            if (maxRadius < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRadius), "Radius length must be non-negative");

            //-- handle 0 corner case, to provide maximum domain
            if (maxRadius == 0)
                return false;

            _maximumRadius = maxRadius;

            //-- If the envelope is smaller than the diameter, the inscribed radius cannot exceed maxRadius.
            var env = _inputGeom.EnvelopeInternal;
            double maxDiam = 2 * _maximumRadius;
            if (env.Width < maxDiam || env.Height < maxDiam)
                return true;

            _tolerance = maxRadius * MaxRadiusFraction;
            Compute();
            double radius = _centerPt.Distance(_radiusPt);
            return radius <= _maximumRadius;
        }

        /// <summary>
        /// Gets the center point of the maximum inscribed circle
        /// (up to the tolerance distance).
        /// </summary>
        /// <returns>The center point of the maximum inscribed circle</returns>
        public Point GetCenter()
        {
            Compute();
            return _centerPoint;
        }

        /// <summary>
        /// Gets a point defining the radius of the Maximum Inscribed Circle.
        /// This is a point on the boundary which is
        /// nearest to the computed center of the Maximum Inscribed Circle.
        /// The line segment from the center to this point
        /// is a radius of the constructed circle, and this point
        /// lies on the boundary of the circle.
        /// </summary>
        /// <returns>A point defining the radius of the Maximum Inscribed Circle</returns>
        public Point GetRadiusPoint()
        {
            Compute();
            return _radiusPoint;
        }

        /// <summary>
        /// Gets a line representing a radius of the Maximum Inscribed Circle.
        /// </summary>
        /// <returns>A line from the center of the circle to a point on the edge</returns>
        public LineString GetRadiusLine()
        {
            Compute();
            return _factory.CreateLineString(new[] { _centerPt.Copy(), _radiusPt.Copy() });
        }

        /// <summary>
        /// Computes the signed distance from a point to the area boundary.
        /// Points outside the polygon are assigned a negative distance.
        /// </summary>
        private double DistanceToBoundary(Point p)
        {
            double dist = _indexedDistance.Distance(p);
            bool isOutside = Location.Exterior == _ptLocater.Locate(p.Coordinate);
            return isOutside ? -dist : dist;
        }

        private double DistanceToBoundary(double x, double y)
        {
            var coord = new Coordinate(x, y);
            var pt = _factory.CreatePoint(coord);
            return DistanceToBoundary(pt);
        }

        private void Compute()
        {
            //-- check if already computed
            if (_centerPt != null) return;

            //-- Handle flat geometries.
            if (_inputGeom.Area == 0.0)
            {
                var c = _inputGeom.Coordinate.Copy();
                CreateResult(c, c.Copy());
                return;
            }

            //-- Optimization for small simple convex polygons.
            if (ExactMaxInscribedCircle.IsSupported(_inputGeom))
            {
                var centreRadius = ExactMaxInscribedCircle.ComputeRadius((Polygon)_inputGeom);
                CreateResult(centreRadius[0], centreRadius[1]);
                return;
            }

            ComputeApproximation();
        }

        private void CreateResult(Coordinate c, Coordinate r)
        {
            _centerPt = c;
            _radiusPt = r;
            _centerPoint = _factory.CreatePoint(_centerPt);
            _radiusPoint = _factory.CreatePoint(_radiusPt);
        }

        private void ComputeApproximation()
        {
            if (_tolerance < 0)
                throw new ArgumentException("Tolerance must be non-negative");

            _ptLocater = new IndexedPointInAreaLocator(_inputGeom);
            _indexedDistance = new IndexedFacetDistance(_inputGeom.Boundary);

            // Priority queue of cells, ordered by maximum distance from boundary
            var cellQueue = new PriorityQueue<Cell>();

            CreateInitialGrid(_inputGeom.EnvelopeInternal, cellQueue);

            // initial candidate center point
            var farthestCell = CreateInteriorPointCell(_inputGeom);

            //-- Carry out the branch-and-bound search of the cell space.
            long maxIter = ComputeMaximumIterations(_inputGeom, _tolerance);
            long iter = 0;
            while (!cellQueue.IsEmpty() && iter < maxIter)
            {
                iter++;
                // pick the most promising cell from the queue
                var cell = cellQueue.Poll();

                // update the circle center cell if the candidate is further from the boundary
                if (cell.Distance > farthestCell.Distance)
                {
                    farthestCell = cell;
                }

                //-- Search termination when checking IsRadiusWithin predicate.
                if (_maximumRadius >= 0)
                {
                    //-- Found an inside point further than max radius.
                    if (farthestCell.Distance > _maximumRadius)
                        break;
                    //-- No cells can have larger radius.
                    if (cell.MaxDistance < _maximumRadius)
                        break;
                }

                /*
                 * Refine this cell if the potential distance improvement
                 * is greater than the required tolerance.
                 * Otherwise the cell is pruned (not investigated further),
                 * since no point in it is further than
                 * the current farthest distance (up to tolerance).
                 *
                 * The tolerance can be automatically determined
                 * as a fraction of the current farthest distance.
                 * For a very small actual MIC distance this may cause many iterations,
                 * but the iter limit prevents an infinite loop.
                 */
                double requiredTol = _tolerance > 0
                    ? _tolerance
                    : farthestCell.Distance * AutoToleranceFraction;

                double potentialIncrease = cell.MaxDistance - farthestCell.Distance;
                if (potentialIncrease < requiredTol)
                    break;

                // refine the cell into four sub-cells
                double h2 = cell.HSide / 2;
                cellQueue.Add(CreateCell(cell.X - h2, cell.Y - h2, h2));
                cellQueue.Add(CreateCell(cell.X + h2, cell.Y - h2, h2));
                cellQueue.Add(CreateCell(cell.X - h2, cell.Y + h2, h2));
                cellQueue.Add(CreateCell(cell.X + h2, cell.Y + h2, h2));
            }

            //-- the farthest cell is the best approximation to the MIC center
            _centerCell = farthestCell;
            _centerPt = new Coordinate(_centerCell.X, _centerCell.Y);
            _centerPoint = _factory.CreatePoint(_centerPt);
            // compute radius point
            var nearestPts = _indexedDistance.NearestPoints(_centerPoint);
            _radiusPt = nearestPts[0].Copy();
            _radiusPoint = _factory.CreatePoint(_radiusPt);
        }

        /// <summary>
        /// Initializes the queue with a cell covering the extent of the area.
        /// </summary>
        private void CreateInitialGrid(Envelope env, PriorityQueue<Cell> cellQueue)
        {
            double cellSize = Math.Max(env.Width, env.Height);
            double hSide = cellSize / 2.0;

            //-- Check for flat collapsed input and if so short-circuit. Result will just be centroid.
            if (cellSize == 0) return;

            var centre = env.Centre;
            cellQueue.Add(CreateCell(centre.X, centre.Y, hSide));
        }

        private Cell CreateCell(double x, double y, double hSide)
        {
            return new Cell(x, y, hSide, DistanceToBoundary(x, y));
        }

        // create a cell at an interior point
        private Cell CreateInteriorPointCell(Geometry geom)
        {
            var p = geom.InteriorPoint;
            double hSide = geom.EnvelopeInternal.Diameter;
            return new Cell(p.X, p.Y, hSide, DistanceToBoundary(p));
        }

        /// <summary>
        /// A square grid cell centered on a given point,
        /// with a given half-side size, and having a given distance
        /// to the area boundary.
        /// The maximum possible distance from any point in the cell to the
        /// boundary can be computed, and is used
        /// as the ordering and upper-bound function in
        /// the branch-and-bound algorithm.
        /// </summary>
        private class Cell : IComparable<Cell>
        {
            private const double Sqrt2 = 1.4142135623730951;

            public Cell(double x, double y, double hSide, double distanceToBoundary)
            {
                X = x;
                Y = y;
                HSide = hSide;
                Distance = distanceToBoundary;
                MaxDistance = distanceToBoundary + hSide * Sqrt2;
            }

            public double MaxDistance { get; }
            public double Distance { get; }
            public double HSide { get; }
            public double X { get; }
            public double Y { get; }

            public Envelope GetEnvelope() => new Envelope(X - HSide, X + HSide, Y - HSide, Y + HSide);

            /// <summary>
            /// Inverts natural ordering so the largest <see cref="MaxDistance"/> is at the front of the queue.
            /// </summary>
            public int CompareTo(Cell o)
            {
                if (o == null) return -1;
                return -MaxDistance.CompareTo(o.MaxDistance);
            }
        }
    }
}
