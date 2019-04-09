using System;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Creates geometries which are shaped like multi-armed stars with each arm shaped like a sine wave.
    /// These kinds of geometries are useful as a more complex geometry for testing algorithms.
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    public class SineStarFactory : GeometricShapeFactory
    {
        /// <summary>
        /// Creates a sine star with the given parameters.
        /// </summary>
        /// <param name="origin">The origin point.</param>
        /// <param name="size">The size of the star.</param>
        /// <param name="nPts">The number of points in the star.</param>
        /// <param name="nArms">The number of arms to generate.</param>
        /// <param name="armLengthRatio">The arm length ratio.</param>
        /// <returns>A sine star shape.</returns>
        public static Geometry Create(Coordinate origin, double size, int nPts, int nArms, double armLengthRatio)
        {
            var gsf = new SineStarFactory
            {
                Centre = origin,
                Size = size,
                NumPoints = nPts,
                ArmLengthRatio = armLengthRatio,
                NumArms = nArms,
            };
            var poly = gsf.CreateSineStar();
            return poly;
        }

        /// <summary>
        /// Creates a factory which will create sine stars using the default <see cref="GeometryFactory"/>
        /// </summary>
        public SineStarFactory()
            : this(new GeometryFactory())
        {

        }

        /// <summary>
        /// Creates a factory which will create sine stars using the given <see cref="GeometryFactory"/>
        /// </summary>
        /// <param name="geomFact">The factory to use</param>
        public SineStarFactory(GeometryFactory geomFact)
            : base(geomFact)
        {
            NumArms = 8;
            ArmLengthRatio = 0.5;
        }

        /// <summary>Gets/Sets the number of arms in the star</summary>
        public int NumArms { get; set; }

        /// <summary>
        /// Gets or sets the ratio of the length of each arm to the radius of the star.
        /// A smaller number makes the arms shorter.
        /// </summary>
        /// <remarks>Value should be between 0.0 and 1.0</remarks>
        public double ArmLengthRatio { get; set; }

        /// <summary>
        /// Generates the geometry for the sine star
        /// </summary>
        /// <returns>The geometry representing the sine star</returns>
        public Geometry CreateSineStar()
        {
            var env = Envelope;
            double radius = env.Width / 2.0;

            double armRatio = ArmLengthRatio;
            if (armRatio < 0.0)
                armRatio = 0.0;
            if (armRatio > 1.0)
                armRatio = 1.0;

            double armMaxLen = armRatio * radius;
            double insideRadius = (1 - armRatio) * radius;

            double centreX = env.MinX + radius;
            double centreY = env.MinY + radius;

            var pts = new Coordinate[NumPoints + 1];
            int iPt = 0;
            for (int i = 0; i < NumPoints; i++)
            {
                // the fraction of the way through the current arm - in [0,1]
                double ptArcFrac = (i / (double)NumPoints) * NumArms;
                double armAngFrac = ptArcFrac - Math.Floor(ptArcFrac);

                // the angle for the current arm - in [0,2Pi]
                // (each arm is a complete sine wave cycle)
                double armAng = 2 * Math.PI * armAngFrac;
                // the current length of the arm
                double armLenFrac = (Math.Cos(armAng) + 1.0) / 2.0;

                // the current radius of the curve (core + arm)
                double curveRadius = insideRadius + armMaxLen * armLenFrac;

                // the current angle of the curve
                double ang = i * (2 * Math.PI / NumPoints);
                double x = curveRadius * Math.Cos(ang) + centreX;
                double y = curveRadius * Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = pts[0].Copy();

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring);
            return poly;
        }
    }
}
