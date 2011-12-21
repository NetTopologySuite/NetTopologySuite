using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
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
        private double _gutterFraction;

        /// <summary>
        /// Create a builder which will create shapes using the default
        /// <see cref="IGeometryFactory"/>.
        /// </summary>
        public RandomPointsInGridBuilder()
            : this(new GeometryFactory())
        {
        }

        /// <summary>
        /// Create a builder which will create shapes using the given
        /// <see cref="IGeometryFactory"/>.
        /// </summary>
        /// <param name="geomFact">The factory to use</param>
        public RandomPointsInGridBuilder(IGeometryFactory geomFact)
            : base(geomFact)
        {
        }

        public bool ConstrainedToCircle
        {
            get { return _isConstrainedToCircle; }
            set { _isConstrainedToCircle = value; }
        }

        public double GutterFraction
        {
            get { return _gutterFraction; }
            set { _gutterFraction = value; }
        }

        public override IGeometry GetGeometry()
        {
            var nCells = (int)Math.Sqrt(NumPoints) + 1;

            var gridDX = Extent.Width / nCells;
            var gridDY = Extent.Height / nCells;

            var gutterFrac = MathUtil.Clamp(_gutterFraction, 0.0, 1.0);
            var gutterOffsetX = gridDX * gutterFrac / 2;
            var gutterOffsetY = gridDY * gutterFrac / 2;
            var cellFrac = 1.0 - gutterFrac;
            var cellDX = cellFrac * gridDX;
            var cellDY = cellFrac * gridDY;

            var pts = new List<Coordinate>();

            for (var i = 0; i < nCells; i++)
            {
                for (var j = 0; j < nCells; j++)
                {
                    var orgX = Extent.MinX + i * gridDX + gutterOffsetX;
                    var orgY = Extent.MinY + j * gridDY + gutterOffsetY;
                    pts.Add(RandomPointInCell(orgX, orgY, cellDX, cellDY));
                }
            }
            return GeomFactory.CreateMultiPoint(pts.ToArray());
        }

        private Coordinate RandomPointInCell(double orgX, double orgY, double xLen, double yLen)
        {
            if (_isConstrainedToCircle)
            {
                RandomPointInCircle(
                    orgX,
                    orgY,
                    xLen, yLen);
            }
            return RandomPointInGridCell(orgX, orgY, xLen, yLen);
        }

        private Coordinate RandomPointInGridCell(double orgX, double orgY, double xLen, double yLen)
        {
            var x = orgX + xLen * Rnd.NextDouble();
            var y = orgY + yLen * Rnd.NextDouble();
            return CreateCoord(x, y);
        }

        private static Coordinate RandomPointInCircle(double orgX, double orgY, double width, double height)
        {
            var centreX = orgX + width / 2;
            var centreY = orgX + height / 2;

            var rndAng = 2 * Math.PI * Rnd.NextDouble();
            var rndRadius = Rnd.NextDouble();
            // use square root of radius, since area is proportional to square of radius
            var rndRadius2 = Math.Sqrt(rndRadius);
            var rndX = width / 2 * rndRadius2 * Math.Cos(rndAng);
            var rndY = height / 2 * rndRadius2 * Math.Sin(rndAng);

            var x0 = centreX + rndX;
            var y0 = centreY + rndY;
            return new Coordinate(x0, y0);
        }
    }
}