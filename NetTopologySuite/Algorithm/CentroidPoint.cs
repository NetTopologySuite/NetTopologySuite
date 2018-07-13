using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the centroid of a point point.
    /// Algorithm:
    /// Compute the average of all points.
    /// </summary>
    [Obsolete("Use Centroid instead")]
    public class CentroidPoint
    {
        private int _ptCount;
        private readonly Coordinate _centSum = new Coordinate();

        /// <summary>
        /// Adds the point(s) defined by a Geometry to the centroid total.
        /// If the point is not of dimension 0 it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(IGeometry geom)
        {
            if (geom is IPoint)
                Add(geom.Coordinate);

            else if(geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                {
                    Add(geometry);
                }
            }
        }

        /// <summary>
        /// Adds the length defined by a coordinate.
        /// </summary>
        /// <param name="pt">A coordinate.</param>
        public void Add(Coordinate pt)
        {
            _ptCount += 1;
            _centSum.X += pt.X;
            _centSum.Y += pt.Y;
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate Centroid
        {
            get
            {
                var cent = new Coordinate();
                cent.X = _centSum.X / _ptCount;
                cent.Y = _centSum.Y / _ptCount;
                return cent;
            }
        }
    }
}
