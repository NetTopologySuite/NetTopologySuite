using System;
using System.Collections.ObjectModel;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Utilities;
using Point = NetTopologySuite.Geometries.Point;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Constructs the Largest Empty Circle for a set
    /// of obstacle geometries, up to a given accuracy distance tolerance.
    /// The obstacles may be any combination of point, linear and polygonal geometries.
    /// <para/>
    /// The Largest Empty Circle (LEC) is the largest circle
    /// whose interior does not intersect with any obstacle
    /// and whose center lies within a polygonal boundary.
    /// The circle center is the point in the interior of the boundary
    /// which has the farthest distance from the obstacles
    /// (up to the accuracy of the distance tolerance).
    /// The circle itself is determined by the center point
    /// and a point lying on an obstacle determining the circle radius.
    /// <para/>
    /// The polygonal boundary may be supplied explicitly.
    /// If it is not specified the convex hull of the obstacles is used as the boundary.
    /// <para/>
    /// To compute an LEC which lies <i>wholly</i> within
    /// a polygonal boundary, include the boundary of the polygon(s) as a linear obstacle.
    /// <para/>
    /// The implementation uses a successive-approximation technique
    /// over a grid of square cells covering the obstacles and boundary.
    /// The grid is refined using a branch-and-bound algorithm.
    /// Point containment and distance are computed in a performant
    /// way by using spatial indexes.
    /// <para/>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <see cref="MaximumInscribedCircle"/>
    /// <see cref="InteriorPoint"/>
    /// <see cref="Centroid"/>
    public class LargestEmptyCircle
    {
        /// <summary>
        /// Computes the center point of the Largest Empty Circle
        /// interior-disjoint to a set of obstacles,
        /// with accuracy to a given tolerance distance.
        /// The obstacles may be any collection of points, lines and polygons.
        /// The center of the LEC lies within the convex hull of the obstacles.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>The center point of the Largest Empty Circle</returns>
        public static Point GetCenter(Geometry obstacles, double tolerance)
        {
            return GetCenter(obstacles, null, tolerance);
        }

        /// <summary>
        /// Computes the center point of the Largest Empty Circle
        /// interior-disjoint to a set of obstacles and within a polygonal boundary,
        /// with accuracy to a given tolerance distance.
        /// The obstacles may be any collection of points, lines and polygons.
        /// The center of the LEC lies within the given boundary.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles</param>
        /// <param name="boundary">A polygonal geometry to contain the LEC center</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>The center point of the Largest Empty Circle</returns>
        public static Geometries.Point GetCenter(Geometry obstacles, Geometry boundary, double tolerance)
        {
            var lec = new LargestEmptyCircle(obstacles, boundary, tolerance);
            return lec.GetCenter();
        }

        /// <summary>
        /// Computes a radius line of the Largest Empty Circle
        /// interior-disjoint to a set of obstacles,
        /// with accuracy to a given tolerance distance.
        /// The obstacles may be any collection of points, lines and polygons.
        /// The center of the LEC lies within the convex hull of the obstacles.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles (points and lines)</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>A line from the center of the circle to a point on the edge</returns>
        public static LineString GetRadiusLine(Geometry obstacles, double tolerance)
        {
            return GetRadiusLine(obstacles, null, tolerance);
        }

        /// <summary>
        /// Computes a radius line of the Largest Empty Circle
        /// interior-disjoint to a set of obstacles and within a polygonal boundary,
        /// with accuracy to a given tolerance distance.
        /// The obstacles may be any collection of points, lines and polygons.
        /// The center of the LEC lies within the given boundary.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles (points and lines)</param>
        /// <param name="boundary">A polygonal geometry to contain the LEC center</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>A line from the center of the circle to a point on the edge</returns>
        public static LineString GetRadiusLine(Geometry obstacles, Geometry boundary, double tolerance)
        {
            var lec = new LargestEmptyCircle(obstacles, boundary, tolerance);
            return lec.GetRadiusLine();
        }


        private readonly Geometry _obstacles;
        private readonly Geometry _boundary;
        private readonly double _tolerance;

        private readonly GeometryFactory _factory;
        private readonly IndexedDistanceToPoint _obstacleDistance;
        private IndexedPointInAreaLocator _boundaryPtLocater;
        private IndexedFacetDistance _boundaryDistance;
        private Envelope _gridEnv;
        private Cell _farthestCell;

        private Cell _centerCell;
        private Coordinate _centerPt;
        private Point _centerPoint;
        private Coordinate _radiusPt;
        private Point _radiusPoint;
        private Geometry _bounds;

        /// <summary>
        /// Creates a new instance of a Largest Empty Circle construction.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles (points and lines)</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        [Obsolete("Will be removed in a future version")]
        public LargestEmptyCircle(Geometry obstacles, double tolerance)
            : this(obstacles, null, tolerance)
        { }

        /// <summary>
        /// Creates a new instance of a Largest Empty Circle construction,
        /// interior-disjoint to a set of obstacle geometries
        /// and having its center within a polygonal boundary.
        /// The obstacles may be any collection of points, lines and polygons.
        /// If the boundary is null or empty the convex hull
        /// of the obstacles is used as the boundary.
        /// </summary>
        /// <param name="obstacles">A non-empty geometry representing the obstacles (points and lines)</param>
        /// <param name="boundary">A polygonal geometry to contain the LEC center (may be null or empty)</param>
        /// <param name="tolerance">The distance tolerance for computing the center point (a positive value)</param>
        public LargestEmptyCircle(Geometry obstacles, Geometry boundary, double tolerance)
        {
            if (obstacles == null || obstacles.IsEmpty)
            {
                throw new ArgumentException("Obstacles geometry is empty or null", nameof(obstacles));
            }
            if (boundary != null && !(boundary is IPolygonal)) {
                throw new ArgumentException("Boundary must be polygonal", nameof(boundary));
            }
            if (tolerance <= 0)
            {
                throw new ArgumentException(string.Format("Accuracy tolerance is non-positive: {0:R}", tolerance), nameof(tolerance));
            }
            _obstacles = obstacles;
            _boundary = boundary;
            _factory = obstacles.Factory;
            _tolerance = tolerance;
            _obstacleDistance = new IndexedDistanceToPoint(obstacles);
        }
        /// <summary>
        /// Gets the center point of the Largest Empty Circle
        /// (up to the tolerance distance).
        /// </summary>
        /// <returns>The center point of the Largest Empty Circle</returns>
        public Point GetCenter()
        {
            Compute();
            return _centerPoint;
        }

        /// <summary>
        /// Gets a point defining the radius of the Largest Empty Circle.
        /// This is a point on the obstacles which is
        /// nearest to the computed center of the Largest Empty Circle.
        /// The line segment from the center to this point
        /// is a radius of the constructed circle, and this point
        /// lies on the boundary of the circle.
        /// </summary>
        /// <returns>A point defining the radius of the Largest Empty Circle</returns>
        public Point GetRadiusPoint()
        {
            Compute();
            return _radiusPoint;
        }

        /// <summary>
        /// Gets a line representing a radius of the Largest Empty Circle.
        /// </summary>
        /// <returns>A line from the center of the circle to a point on the edge</returns>
        public LineString GetRadiusLine()
        {
            Compute();
            var radiusLine = _factory.CreateLineString(new[] {_centerPt.Copy(), _radiusPt.Copy()});
            return radiusLine;
        }

        /// <summary>
        /// Computes the signed distance from a point to the constraints
        /// (obstacles and boundary).
        /// Points outside the boundary polygon are assigned a negative distance.
        /// Their containing cells will be last in the priority queue
        /// (but will still end up being tested since they may be refined).
        /// </summary>
        /// <param name="p">The point to compute the distance for</param>
        /// <returns>The signed distance to the constraints (negative indicates outside the boundary)</returns>
        private double DistanceToConstraints(Point p)
        {
            bool isOutide = Location.Exterior == _boundaryPtLocater.Locate(p.Coordinate);
            if (isOutide)
            {
                double boundaryDist = _boundaryDistance.Distance(p);
                return -boundaryDist;
            }

            double dist = _obstacleDistance.Distance(p);
            return dist;
        }

        private double DistanceToConstraints(double x, double y)
        {
            var coord = new Coordinate(x, y);
            var pt = _factory.CreatePoint(coord);
            return DistanceToConstraints(pt);
        }

        private void InitBoundary()
        {
            _bounds = _boundary;
            if (_bounds == null || _bounds.IsEmpty)
            {
                _bounds = _obstacles.ConvexHull();
            }
            //-- the centre point must be in the extent of the boundary
            _gridEnv = _bounds.EnvelopeInternal;
            // if bounds does not enclose an area cannot create a ptLocater
            if (_bounds.Dimension >= Dimension.Surface)
            {
                _boundaryPtLocater = new IndexedPointInAreaLocator(_bounds);
                _boundaryDistance = new IndexedFacetDistance(_bounds);
            }
        }

        private void Compute()
        {
            InitBoundary();

            // check if already computed
            if (_centerCell != null) return;

            // if _boundaryPtLocater is not present then result is degenerate (represented as zero-radius circle)
            if (_boundaryPtLocater == null)
            {
                var pt = _obstacles.Coordinate;
                _centerPt = pt.Copy();
                _centerPoint = _factory.CreatePoint(pt);
                _radiusPt = pt.Copy();
                _radiusPoint = _factory.CreatePoint(pt);
                return;
            }

            // Priority queue of cells, ordered by decreasing distance from constraints
            var cellQueue = new PriorityQueue<Cell>();

            //-- grid covers extent of obstacles and boundary (if any)
            CreateInitialGrid(_gridEnv, cellQueue);

            // use the area centroid as the initial candidate center point
            _farthestCell = CreateCentroidCell(_obstacles);
            //int totalCells = cellQueue.size();

            /*
             * Carry out the branch-and-bound search
             * of the cell space
             */
            long maxIter = MaximumInscribedCircle.ComputeMaximumIterations(_bounds, _tolerance);
            long iter = 0;
            while (!cellQueue.IsEmpty() && iter < maxIter)
            {
                // Increase iteration counter
                iter++;

                // pick the cell with greatest distance from the queue
                var cell = cellQueue.Poll();
                //Console.WriteLine($"{iter}] Dist: {cell.Distance} Max D: {cell.MaxDistance} size: {cell.HSide}");

                // update the center cell if the candidate is further from the constraints
                if (cell.Distance > _farthestCell.Distance)
                {
                    _farthestCell = cell;
                }

                /*
                 * If this cell may contain a better approximation to the center 
                 * of the empty circle, then refine it (partition into subcells 
                 * which are added into the queue for further processing).
                 * Otherwise the cell is pruned (not investigated further),
                 * since no point in it can be further than the current farthest distance.
                 */
                if (MayContainCircleCenter(cell))
                {
                    // split the cell into four sub-cells
                    double h2 = cell.HSide / 2;
                    cellQueue.Add(CreateCell(cell.X - h2, cell.Y - h2, h2));
                    cellQueue.Add(CreateCell(cell.X + h2, cell.Y - h2, h2));
                    cellQueue.Add(CreateCell(cell.X - h2, cell.Y + h2, h2));
                    cellQueue.Add(CreateCell(cell.X + h2, cell.Y + h2, h2));
                    //totalCells += 4;
                }
            }

            // the farthest cell is the best approximation to the LEC center
            _centerCell = _farthestCell;
            // compute center point
            _centerPt = new Coordinate(_centerCell.X, _centerCell.Y);
            _centerPoint = _factory.CreatePoint(_centerPt);
            // compute radius point
            var nearestPts = _obstacleDistance.NearestPoints(_centerPoint);
            _radiusPt = nearestPts[0].Copy();
            _radiusPoint = _factory.CreatePoint(_radiusPt);
        }

        /// <summary>
        /// Tests whether a cell may contain the circle center,
        /// and thus should be refined (split into subcells
        /// to be investigated further.)
        /// </summary>
        /// <param name="cell">The cell to test</param>
        /// <returns><c>true</c> if the cell might contain the circle center</returns>
        private bool MayContainCircleCenter(Cell cell)
        {
            /*
             * Every point in the cell lies outside the boundary,
             * so they cannot be the center point
             */
            if (cell.IsFullyOutside)
                return false;

            /*
             * The cell is outside, but overlaps the boundary
             * so it may contain a point which should be checked.
             * This is only the case if the potential overlap distance 
             * is larger than the tolerance.
             */
            if (cell.IsOutside)
            {
                bool isOverlapSignificant = cell.MaxDistance > _tolerance;
                return isOverlapSignificant;
            }

            /*
             * Cell is inside the boundary. It may contain the center
             * if the maximum possible distance is greater than the current distance
             * (up to tolerance).
             */
            double potentialIncrease = cell.MaxDistance - _farthestCell.Distance;
            return potentialIncrease > _tolerance;
        }

        /// <summary>
        /// Initializes the queue with a cell covering
        /// the extent of the area.
        /// </summary>
        /// <param name="env">The area extent to cover</param>
        /// <param name="cellQueue">The queue to initialize</param>
        private void CreateInitialGrid(Envelope env, PriorityQueue<Cell> cellQueue)
        {
            double cellSize = env.MaxExtent;
            double hSide = cellSize / 2.0;

            // Check for flat collapsed input and if so short-circuit
            // Result will just be centroid
            if (cellSize == 0) return;

            var centre = env.Centre;
            cellQueue.Add(CreateCell(centre.X, centre.Y, hSide));
        }

        private Cell CreateCell(double x, double y, double h)
        {
            return new Cell(x, y, h, DistanceToConstraints(x, y));
        }

        // create a cell centered on area centroid
        private Cell CreateCentroidCell(Geometry geom)
        {
            var p = geom.Centroid;
            return new Cell(p.X, p.Y, 0, DistanceToConstraints(p));
        }

        /// <summary>
        /// A square grid cell centered on a given point
        /// with a given side half-length,
        /// and having a given distance from the center point to the constraints.
        /// The maximum possible distance from any point in the cell to the
        /// constraints can be computed.
        /// This is used as the ordering and upper-bound function in
        /// the branch-and-bound algorithm. 
        /// </summary>
        private class Cell : IComparable<Cell>
        {

            private const double Sqrt2 = 1.4142135623730951;

            public Cell(double x, double y, double hSide, double distanceToConstraints)
            {
                X = x; // cell center x
                Y = y; // cell center y
                HSide = hSide; // half the cell size

                // the distance from cell center to constraints
                Distance = distanceToConstraints;

                /*
                 * The maximum possible distance to the constraints for points in this cell
                 * is the center distance plus the radius (half the diagonal length).
                 */
                MaxDistance = Distance + hSide * Sqrt2;
            }

            public bool IsFullyOutside => MaxDistance < 0;

            public bool IsOutside => Distance < 0;

            public double MaxDistance { get; }

            public double Distance { get; }

            public double HSide { get; }

            public double X { get; }

            public double Y { get; }

            /// <summary>
            /// For maximum efficieny sort the PriorityQueue with largest maxDistance at front.
            /// Since AlternativePriorityQueue sorts least-first, need to invert the comparison
            /// </summary>
            public int CompareTo(Cell o)
            {
                return -MaxDistance.CompareTo(o.MaxDistance);
            }
        }

    }
}
