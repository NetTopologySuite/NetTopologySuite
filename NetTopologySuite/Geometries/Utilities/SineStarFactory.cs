using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Creates geometries which are shaped like multi-armed stars with each arm shaped like a sine wave.
    /// These kinds of geometries are useful as a more complex geometry for testing algorithms.
    /// </summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public class SineStarFactory<TCoordinate> : GeometricShapeFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private int _numArms = 8;
        private double _armLengthRatio = 0.5;

        ///<summary>
        /// Creates a factory which will create sine stars using the provided <see cref="IGeometryFactory{TCoordinate}"/>.
        ///</summary>
        public SineStarFactory(IGeometryFactory<TCoordinate> geomFactory)
            : base(geomFactory)
        {
        }

        ///<summary>
        /// Gets/Sets the number of arms in the star
        ///</summary>
        public int NumberOfArms
        {
            get { return _numArms; }
            set { _numArms = value; }
        }

        ///<summary>
        /// Gets/Sets the ration of the length of each arm to the distance from the tip
        /// of the arm to the centre of the star. Value should be between 0.0 and 1.0
        ///</summary>
        public Double ArmLengthRatio
        {
            get { return _armLengthRatio; }
            set { _armLengthRatio = value; }
        }

        ///<summary>
        /// Generates the geometry for the sine star
        ///</summary>
        public IGeometry<TCoordinate> CreateSineStar()
        {
            IExtents<TCoordinate> env = _dim.Extents;
            double radius = env.GetSize(Ordinates.X) / 2.0d;

            double armRatio = _armLengthRatio;
            if (armRatio < 0.0)
                armRatio = 0.0;
            if (armRatio > 1.0)
                armRatio = 1.0;

            double armMaxLen = armRatio * radius;
            double insideRadius = (1 - armRatio) * radius;

            double centreX = env.GetMin(Ordinates.X) + radius;
            double centreY = env.GetMin(Ordinates.Y) + radius;

            TCoordinate[] pts = new TCoordinate[PointCount + 1];
            int iPt = 0;
            for (int i = 0; i < PointCount; i++)
            {
                // the fraction of the way thru the current arm - in [0,1]
                double ptArcFrac = (i / (double)PointCount) * _numArms;
                double armAngFrac = ptArcFrac - Math.Floor(ptArcFrac);

                // the angle for the current arm - in [0,2Pi]  
                // (each arm is a complete sine wave cycle)
                double armAng = 2 * Math.PI * armAngFrac;
                // the current length of the arm
                double armLenFrac = (Math.Cos(armAng) + 1.0) / 2.0;

                // the current radius of the curve (core + arm)
                double curveRadius = insideRadius + armMaxLen * armLenFrac;

                // the current angle of the curve
                double ang = i * (2 * Math.PI / PointCount);
                double x = curveRadius * Math.Cos(ang) + centreX;
                double y = curveRadius * Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = _coordFactory.Create(pts[0]);

            ILinearRing<TCoordinate> ring = _geoFactory.CreateLinearRing(pts);
            IPolygon<TCoordinate> poly = _geoFactory.CreatePolygon(ring, null);
            return poly;
        }
    }
}