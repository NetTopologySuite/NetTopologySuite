using System;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    ///     Creates geometries which are shaped like multi-armed stars with each arm shaped like a sine wave.
    ///     These kinds of geometries are useful as a more complex geometry for testing algorithms.
    /// </summary>
    /// <author>
    ///     Martin Davis
    /// </author>
    public class SineStarFactory : GeometricShapeFactory
    {
        /// <summary>
        ///     Creates a factory which will create sine stars using the default <see cref="IGeometryFactory" />
        /// </summary>
        public SineStarFactory()
            : this(new GeometryFactory())
        {
        }

        /// <summary>
        ///     Creates a factory which will create sine stars using the given <see cref="IGeometryFactory" />
        /// </summary>
        /// <param name="geomFact">The factory to use</param>
        public SineStarFactory(IGeometryFactory geomFact)
            : base(geomFact)
        {
            NumArms = 8;
            ArmLengthRatio = 0.5;
        }

        ///<summary>Gets/Sets the number of arms in the star</summary>
        public int NumArms { get; set; }

        /// <summary>
        ///     Sets the ration of the length of each arm to the distance from the tip of the arm to the centre of the star.
        /// </summary>
        /// <remarks>Value should be between 0.0 and 1.0</remarks>
        public double ArmLengthRatio { get; set; }

        /// <summary>
        ///     Generates the geometry for the sine star
        /// </summary>
        /// <returns>The geometry representing the sine star</returns>
        public IGeometry CreateSineStar()
        {
            var env = Envelope;
            var radius = env.Width/2.0;

            var armRatio = ArmLengthRatio;
            if (armRatio < 0.0)
                armRatio = 0.0;
            if (armRatio > 1.0)
                armRatio = 1.0;

            var armMaxLen = armRatio*radius;
            var insideRadius = (1 - armRatio)*radius;

            var centreX = env.MinX + radius;
            var centreY = env.MinY + radius;

            var pts = new Coordinate[NumPoints + 1];
            var iPt = 0;
            for (var i = 0; i < NumPoints; i++)
            {
                // the fraction of the way thru the current arm - in [0,1]
                var ptArcFrac = i/(double) NumPoints*NumArms;
                var armAngFrac = ptArcFrac - Math.Floor(ptArcFrac);

                // the angle for the current arm - in [0,2Pi]  
                // (each arm is a complete sine wave cycle)
                var armAng = 2*Math.PI*armAngFrac;
                // the current length of the arm
                var armLenFrac = (Math.Cos(armAng) + 1.0)/2.0;

                // the current radius of the curve (core + arm)
                var curveRadius = insideRadius + armMaxLen*armLenFrac;

                // the current angle of the curve
                var ang = i*(2*Math.PI/NumPoints);
                var x = curveRadius*Math.Cos(ang) + centreX;
                var y = curveRadius*Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = new Coordinate(pts[0]);

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring, null);
            return poly;
        }
    }
}