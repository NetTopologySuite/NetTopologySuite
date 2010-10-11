using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of a point point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// Compute the average of all points.
    /// </remarks>
    public class CentroidPoint<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly ICoordinateFactory<TCoordinate> _factory;
        private TCoordinate _centSum;
        private Int32 _pointCount;

        public CentroidPoint(ICoordinateFactory<TCoordinate> factory)
        {
            _factory = factory;
            _centSum = _factory.Create();
        }

        public TCoordinate Centroid
        {
            get
            {
                return _pointCount == 0
                           ? _centSum
                           : ((IComputable<Double, TCoordinate>) _centSum).Divide(_pointCount);
            }
        }

        /// <summary> 
        /// Adds the point(s) defined by a Geometry to the centroid total.
        /// If the point is not of dimension 0 it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(IGeometry<TCoordinate> geom)
        {
            if (geom is IPoint<TCoordinate>)
            {
                IPoint<TCoordinate> point = geom as IPoint<TCoordinate>;
                Add(point.Coordinate);
            }

            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;

                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    Add(geometry);
                }
            }
        }

        /// <summary> 
        /// Adds the length defined by a coordinate.
        /// </summary>
        /// <param name="point">A coordinate.</param>
        public void Add(TCoordinate point)
        {
            _pointCount += 1;

            if (_centSum.IsEmpty)
            {
                _centSum = point;
            }
            else
            {
                _centSum = _centSum.Add(point);
            }
        }
    }
}