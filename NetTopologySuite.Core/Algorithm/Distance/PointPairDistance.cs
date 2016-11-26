using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Algorithm.Distance
{
    /// <summary>
    /// Contains a pair of points and the distance between them.
    /// </summary>
    /// <remarks>
    /// Provides methods to update with a new point pair with either maximum or minimum distance.
    /// </remarks>
    public class PointPairDistance
    {
        private Boolean _isNull = true;

        ///<summary>
        /// Initializes to null.
        ///</summary>
        public void Initialize() { _isNull = true; }

        ///<summary>
        /// Initializes the points.
        ///</summary>
        /// <param name="p0">1st coordinate</param>
        /// <param name="p1">2nd coordinate</param>
        public void Initialize(Coordinate p0, Coordinate p1)
        {
            Coordinates[0].CoordinateValue = p0;
            Coordinates[1].CoordinateValue = p1;
            Distance = p0.Distance(p1);
            _isNull = false;
        }

        ///<summary>
        /// Initializes the points, avoiding recomputing the distance.
        ///</summary>
        /// <param name="p0">1st coordinate</param>
        /// <param name="p1">2nd coordinate</param>
        /// <param name="distance">the distance between <see paramref="p0"/> and <see paramref="p1"/></param>
        private void Initialize(Coordinate p0, Coordinate p1, double distance)
        {
            Coordinates[0].CoordinateValue = p0;
            Coordinates[1].CoordinateValue = p1;
            Distance = distance;
            _isNull = false;
        }

        /// <summary>
        /// The distance between the paired coordinates
        /// </summary>
        public double Distance { get; private set; } = Double.NaN;

        /// <summary>
        /// Returns an array containing the paired points
        /// </summary>
        public Coordinate[] Coordinates { get; } = { new Coordinate(), new Coordinate() };


        public Coordinate this[int i] => Coordinates[i];

        public void SetMaximum(PointPairDistance ptDist)
        {
            SetMaximum(ptDist.Coordinates[0], ptDist.Coordinates[1]);
        }

        public void SetMaximum(Coordinate p0, Coordinate p1)
        {
            if (_isNull)
            {
                Initialize(p0, p1);
                return;
            }
            double dist = p0.Distance(p1);
            if (dist > Distance)
                Initialize(p0, p1, dist);
        }

        public void SetMinimum(PointPairDistance ptDist)
        {
            SetMinimum(ptDist.Coordinates[0], ptDist.Coordinates[1]);
        }

        public void SetMinimum(Coordinate p0, Coordinate p1)
        {
            if (_isNull)
            {
                Initialize(p0, p1);
                return;
            }
            double dist = p0.Distance(p1);
            if (dist < Distance)
                Initialize(p0, p1, dist);
        }

        public override string ToString()
        {
  	        return WKTWriter.ToLineString(Coordinates[0], Coordinates[1]);
        }
    }
}