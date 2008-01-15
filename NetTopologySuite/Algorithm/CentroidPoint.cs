using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of a point point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// Compute the average of all points.
    /// </remarks>
    public class CentroidPoint<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                             IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ICoordinateFactory<TCoordinate> _factory;
        private Int32 _pointCount = 0;
        private TCoordinate _centSum;

        public CentroidPoint(ICoordinateFactory<TCoordinate> factory)
        {
            _factory = factory;
            _centSum = _factory.Create();
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
            _centSum = _factory.Create(_centSum[Ordinates.X] + point[Ordinates.X],
                                        _centSum[Ordinates.Y] + point[Ordinates.Y]);
        }

        public TCoordinate Centroid
        {
            get
            {
                Double x = _centSum[Ordinates.X] / _pointCount;
                Double y = _centSum[Ordinates.Y] / _pointCount;
                return _factory.Create(x, y);
            }
        }
    }
}