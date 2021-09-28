using System;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Constructs the Largest Empty Circle for a set
    /// of obstacle geometries, up to a specified tolerance.
    /// The obstacles are point and line geometries.
    /// <para/>
    /// The Largest Empty Circle is the largest circle which
    /// has its center in the convex hull of the obstacles (the <i>boundary</i>),
    /// and whose interior does not intersect with any obstacle.
    /// The circle center is the point in the interior of the boundary
    /// which has the farthest distance from the obstacles (up to tolerance).
    /// The circle is determined by the center point
    /// and a point lying on an obstacle indicating the circle radius.
    /// <para/>
    /// The implementation uses a successive-approximation technique
    /// over a grid of square cells covering the obstacles and boundary.
    /// The grid is refined using a branch-and-bound algorithm.
    /// Point containment and distance are computed in a performant
    /// way by using spatial indexes.
    /// <para/>
    /// <h3>Future Enhancements</h3>
    /// <list type="bullet">
    /// <item><description>Support polygons as obstacles</description></item>
    /// <item><description>Support a client-defined boundary polygon</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <see cref="MaximumInscribedCircle"/>
    /// <see cref="InteriorPoint"/>
    /// <see cref="Centroid"/>
    public class LargestEmptyCircle
    {
        /// <summary>
        /// Computes the center point of the Largest Empty Circle
        /// within a set of obstacles, up to a given tolerance distance.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles (points and lines)</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>The center point of the Largest Empty Circle</returns>
        public static Point GetCenter(Geometry obstacles, double tolerance)
        {
            var lec = new LargestEmptyCircle(obstacles, tolerance);
            return lec.GetCenter();
        }

        /// <summary>
        /// Computes a radius line of the Largest Empty Circle
        /// within a set of obstacles, up to a given distance tolerance.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles (points and lines)</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        /// <returns>A line from the center of the circle to a point on the edge</returns>
        public static LineString GetRadiusLine(Geometry obstacles, double tolerance)
        {
            var lec = new LargestEmptyCircle(obstacles, tolerance);
            return lec.GetRadiusLine();
        }

        private readonly Geometry _obstacles;
        private readonly double _tolerance;

        private readonly GeometryFactory _factory;
        private Geometry _boundary;
        private IndexedPointInAreaLocator _ptLocater;
        private readonly IndexedFacetDistance _obstacleDistance;
        private IndexedFacetDistance _boundaryDistance;
        private Cell _farthestCell;

        private Cell _centerCell;
        private Coordinate _centerPt;
        private Point _centerPoint;
        private Coordinate _radiusPt;
        private Point _radiusPoint;

        /// <summary>
        /// Creates a new instance of a Largest Empty Circle construction.
        /// </summary>
        /// <param name="obstacles">A geometry representing the obstacles (points and lines)</param>
        /// <param name="tolerance">The distance tolerance for computing the center point</param>
        public LargestEmptyCircle(Geometry obstacles, double tolerance)
        {
            if (obstacles.IsEmpty)
            {
                throw new ArgumentException("Empty obstacles geometry is not supported");
            }

            _obstacles = obstacles;
            _factory = obstacles.Factory;
            _tolerance = tolerance;
            _obstacleDistance = new IndexedFacetDistance(obstacles);
            SetBoundary(obstacles);
        }

        /// <summary>
        /// Sets the area boundary as the convex hull
        /// of the obstacles.
        /// </summary>
        private void SetBoundary(Geometry obstacles)
        {
            // TODO: allow this to be set by client as arbitrary polygon
            this._boundary = obstacles.ConvexHull();
            // if boundary does not enclose an area cannot create a ptLocater
            if (_boundary.Dimension >= Dimension.Surface)
            {
                _ptLocater = new IndexedPointInAreaLocator(_boundary);
                _boundaryDistance = new IndexedFacetDistance(_boundary);
            }
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
            bool isOutide = Location.Exterior == _ptLocater.Locate(p.Coordinate);
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

        private void Compute()
        {
            // check if already computed
            if (_centerCell != null) return;

            // if ptLocater is not present then result is degenerate (represented as zero-radius circle)
            if (_ptLocater == null)
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

            CreateInitialGrid(_obstacles.EnvelopeInternal, cellQueue);

            // use the area centroid as the initial candidate center point
            _farthestCell = CreateCentroidCell(_obstacles);
            //int totalCells = cellQueue.size();

            /*
             * Carry out the branch-and-bound search
             * of the cell space
             */
            while (!cellQueue.IsEmpty())
            {
                // pick the cell with greatest distance from the queue
                var cell = cellQueue.Poll();

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
        /// Initializes the queue with a grid of cells covering
        /// the extent of the area.
        /// </summary>
        /// <param name="env">The area extent to cover</param>
        /// <param name="cellQueue">The queue to initialize</param>
        private void CreateInitialGrid(Envelope env, PriorityQueue<Cell> cellQueue)
        {
            double minX = env.MinX;
            double maxX = env.MaxX;
            double minY = env.MinY;
            double maxY = env.MaxY;
            double width = env.Width;
            double height = env.Height;
            double cellSize = Math.Min(width, height);
            double hSize = cellSize / 2.0;

            // compute initial grid of cells to cover area
            for (double x = minX; x < maxX; x += cellSize)
            {
                for (double y = minY; y < maxY; y += cellSize)
                {
                    cellQueue.Add(CreateCell(x + hSize, y + hSize, hSize));
                }
            }
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

            public int CompareTo(Cell o)
            {
                // A cell is greater if its maximum distance is larger.
                return (int) (o.MaxDistance - this.MaxDistance);
            }
        }

    }
}
