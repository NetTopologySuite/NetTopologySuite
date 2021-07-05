using System;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Constructs the Maximum Inscribed Circle for a
    /// polygonal <see cref="Geometry"/>, up to a specified tolerance.
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
    /// <para/>
    /// The class supports polygons with holes and multipolygons.
    /// <para/>
    /// The implementation uses a successive-approximation technique
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

        private readonly Geometry _inputGeom;
        private readonly double _tolerance;

        private readonly GeometryFactory _factory;
        private readonly IndexedPointInAreaLocator _ptLocater;
        private readonly IndexedFacetDistance _indexedDistance;
        private Cell _centerCell;
        private Coordinate _centerPt;
        private Coordinate _radiusPt;
        private Point _centerPoint;
        private Point _radiusPoint;

        /// <summary>
        /// Creates a new instance of a Maximum Inscribed Circle computation.
        /// </summary>
        /// <param name="polygonal">An areal geometry</param>
        /// <param name="tolerance">The distance tolerance for computing the centre point
        /// (must be positive)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the tolerance is non-positive</exception>
        /// <exception cref="ArgumentException">Thrown if the input geometry is non-polygonal or empty</exception>
        public MaximumInscribedCircle(Geometry polygonal, double tolerance)
        {
            if (tolerance <= 0)
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive");

            if (!(polygonal is IPolygonal))
                throw new ArgumentException("Input geometry must be a Polygon or MultiPolygon");

            if (polygonal.IsEmpty)
                throw new ArgumentException("Empty input geometry is not supported");

            _inputGeom = polygonal;
            _factory = polygonal.Factory;
            _tolerance = tolerance;
            _ptLocater = new IndexedPointInAreaLocator(polygonal);
            _indexedDistance = new IndexedFacetDistance(polygonal.Boundary);
        }

        /// <summary>
        /// Gets the center point of the maximum inscribed circle
        /// (up to the tolerance distance).</summary>
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
        /// Computes the signed distance from a point to the area boundary.
        /// Points outside the polygon are assigned a negative distance.
        /// Their containing cells will be last in the priority queue
        /// (but may still end up being tested since they may need to be refined).
        /// </summary>
        /// <param name="p">The point to compute the distance for</param>
        /// <returns>The signed distance to the area boundary (negative indicates outside the area)</returns>
        private double DistanceToBoundary(Point p)
        {
            double dist = _indexedDistance.Distance(p);
            bool isOutide = Location.Exterior == _ptLocater.Locate(p.Coordinate);
            if (isOutide) return -dist;
            return dist;
        }

        private double DistanceToBoundary(double x, double y)
        {
            var coord = new Coordinate(x, y);
            var pt = _factory.CreatePoint(coord);
            return DistanceToBoundary(pt);
        }

        private void Compute()
        {
            // check if already computed
            if (_centerCell != null) return;

            // Priority queue of cells, ordered by maximum distance from boundary
            var cellQueue = new PriorityQueue<Cell>();

            CreateInitialGrid(_inputGeom.EnvelopeInternal, cellQueue);

            // use the area centroid as the initial candidate center point
            var farthestCell = CreateCentroidCell(_inputGeom);
            //int totalCells = cellQueue.size();

            /*
             * Carry out the branch-and-bound search
             * of the cell space
             */
            while (!cellQueue.IsEmpty())
            {
                // pick the most promising cell from the queue
                var cell = cellQueue.Poll();
                //System.out.println(factory.toGeometry(cell.getEnvelope()));

                // update the center cell if the candidate is further from the boundary
                if (cell.Distance > farthestCell.Distance)
                {
                    farthestCell = cell;
                }
                /*
                 * Refine this cell if the potential distance improvement
                 * is greater than the required tolerance.
                 * Otherwise the cell is pruned (not investigated further),
                 * since no point in it is further than
                 * the current farthest distance.
                 */
                double potentialIncrease = cell.MaxDistance - farthestCell.Distance;
                if (potentialIncrease > _tolerance)
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
            // the farthest cell is the best approximation to the MIC center
            _centerCell = farthestCell;
            _centerPt = new Coordinate(_centerCell.X, _centerCell.Y);
            _centerPoint = _factory.CreatePoint(_centerPt);
            // compute radius point
            var nearestPts = _indexedDistance.NearestPoints(_centerPoint);
            _radiusPt = nearestPts[0].Copy();
            _radiusPoint = _factory.CreatePoint(_radiusPt);
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
            double hSide = cellSize / 2.0;

            // compute initial grid of cells to cover area
            for (double x = minX; x < maxX; x += cellSize)
            {
                for (double y = minY; y < maxY; y += cellSize)
                {
                    cellQueue.Add(CreateCell(x + hSide, y + hSide, hSide));
                }
            }
        }

        private Cell CreateCell(double x, double y, double hSide)
        {
            return new Cell(x, y, hSide, DistanceToBoundary(x, y));
        }

        // create a cell centered on area centroid
        private Cell CreateCentroidCell(Geometry geom)
        {
            var p = geom.Centroid;
            return new Cell(p.X, p.Y, 0, DistanceToBoundary(p));
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
                return (int)(o.MaxDistance - this.MaxDistance);
            }
        }

    }
}
