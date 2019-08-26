using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape.Fractal
{
    /// <summary>
    /// Generates a <see cref="LineString"/> representing the Morton Curve
    /// at a given level.
    /// </summary>
    public class MortonCurveBuilder : GeometricShapeBuilder
    {
        // DEVIATION: unused in JTS.
        ////private int order = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="MortonCurveBuilder"/> class
        /// using the provided <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="geomFactory">The geometry factory to use.</param>
        public MortonCurveBuilder(GeometryFactory geomFactory)
            : base(geomFactory)
        {
            // use a null extent to indicate no transformation
            // (may be set by client)
            Extent = null;
        }

        /// <summary>
        /// Gets or sets the level of curve to generate.
        /// The level must be in the range [0 - 16].
        /// </summary>
        public int Level
        {
            get => HilbertCode.Level(NumPoints);
            set => NumPoints = HilbertCode.Size(value);
        }

        /// <inheritdoc />
        public override Geometry GetGeometry()
        {
            int level = Level;
            int nPts = HilbertCode.Size(level);

            double scale = 1;
            double baseX = 0;
            double baseY = 0;
            if (Extent != null)
            {
                var baseLine = GetSquareBaseLine();
                baseX = baseLine.MinX;
                baseY = baseLine.MinY;
                double width = baseLine.Length;
                int maxOrdinate = HilbertCode.MaxOrdinate(level);
                scale = width / maxOrdinate;
            }

            var pts = new Coordinate[nPts];
            for (int i = 0; i < nPts; i++)
            {
                var pt = HilbertCode.Decode(level, i);
                double x = Transform(pt.X, scale, baseX);
                double y = Transform(pt.Y, scale, baseY);
                pts[i] = new Coordinate(x, y);
            }

            return GeomFactory.CreateLineString(pts);
        }

        private static double Transform(double val, double scale, double offset)
        {
            return val * scale + offset;
        }
    }
}
