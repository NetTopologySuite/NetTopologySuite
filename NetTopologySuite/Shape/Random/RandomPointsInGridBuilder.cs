using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Shape.Random
{
    /// <summary>
    /// Creates random point sets where the points
    /// are constrained to lie in the cells of a grid.
    /// </summary>
    /// <author>mbdavis</author>
    public class RandomPointsInGridBuilder : GeometricShapeBuilder
    {
        protected static readonly System.Random Rnd = new System.Random();

        private bool _isConstrainedToCircle;

        /// <summary>
        /// Create a builder which will create shapes using the default
        /// <see cref="GeometryFactory"/>.
        /// </summary>
        public RandomPointsInGridBuilder()
            : this(new GeometryFactory())
        {
        }

        /// <summary>
        /// Create a builder which will create shapes using the given
        /// <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="geomFact">The factory to use</param>
        public RandomPointsInGridBuilder(GeometryFactory geomFact)
            : base(geomFact)
        {
        }

        /// <summary>
        /// Gets or sets whether generated points are constrained to lie
        /// within a circle contained within each grid cell.
        /// This provides greater separation between points
        /// in adjacent cells.
        /// <para/>
        /// The default is to not be constrained to a circle.
        /// </summary>
        public bool ConstrainedToCircle
        {
            get => _isConstrainedToCircle;
            set => _isConstrainedToCircle = value;
        }

        /// <summary>
        /// Gets or sets the fraction of the grid cell side which will be treated as
        /// a gutter, in which no points will be created.
        /// <para/>
        /// The provided value is clamped to the range [0.0, 1.0].
        /// </summary>
        public double GutterFraction { get; set; }

        /// <summary>
        /// Gets the <see cref="MultiPoint"/> containing the generated point
        /// </summary>
        /// <returns>A MultiPoint</returns>
        public override Geometry GetGeometry()
        {
            int nCells = (int)Math.Sqrt(NumPoints) + 1;

            // ensure that at least numPts points are generated
            if (nCells * nCells < NumPoints)
                nCells += 1;

            double gridDX = Extent.Width / nCells;
            double gridDY = Extent.Height / nCells;

            double gutterFrac = MathUtil.Clamp(GutterFraction, 0.0, 1.0);
            double gutterOffsetX = gridDX * gutterFrac / 2;
            double gutterOffsetY = gridDY * gutterFrac / 2;
            double cellFrac = 1.0 - gutterFrac;
            double cellDX = cellFrac * gridDX;
            double cellDY = cellFrac * gridDY;

            var pts = new Coordinate[nCells * nCells];
            int index = 0;
            for (int i = 0; i < nCells; i++)
            {
                for (int j = 0; j < nCells; j++)
                {
                    double orgX = Extent.MinX + i * gridDX + gutterOffsetX;
                    double orgY = Extent.MinY + j * gridDY + gutterOffsetY;
                    pts[index++] = RandomPointInCell(orgX, orgY, cellDX, cellDY);
                }
            }
            return GeomFactory.CreateMultiPointFromCoords(pts);
        }

        private Coordinate RandomPointInCell(double orgX, double orgY, double xLen, double yLen)
        {
            return _isConstrainedToCircle
                ? RandomPointInCircle(orgX, orgY, xLen, yLen)
                : RandomPointInGridCell(orgX, orgY, xLen, yLen);
        }

        private Coordinate RandomPointInGridCell(double orgX, double orgY, double xLen, double yLen)
        {
            double x = orgX + xLen * Rnd.NextDouble();
            double y = orgY + yLen * Rnd.NextDouble();
            return CreateCoord(x, y);
        }

        private static Coordinate RandomPointInCircle(double orgX, double orgY, double width, double height)
        {
            double centreX = orgX + width / 2;
            double centreY = orgY + height / 2;

            double rndAng = 2 * Math.PI * Rnd.NextDouble();
            double rndRadius = Rnd.NextDouble();
            // use square root of radius, since area is proportional to square of radius
            double rndRadius2 = Math.Sqrt(rndRadius);
            double rndX = width / 2 * rndRadius2 * Math.Cos(rndAng);
            double rndY = height / 2 * rndRadius2 * Math.Sin(rndAng);

            double x0 = centreX + rndX;
            double y0 = centreY + rndY;
            return new Coordinate(x0, y0);
        }
    }
}