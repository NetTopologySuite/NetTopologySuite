using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes a point in the interior of an point point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// Find a point which is closest to the centroid of the point.
    /// </remarks>
    public class InteriorPointPoint<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly TCoordinate _centroid;
        private readonly ICoordinateFactory<TCoordinate> _factory;
        private TCoordinate _interiorPoint;
        private Double _minDistance = Double.MaxValue;

        public InteriorPointPoint(IGeometry<TCoordinate> g)
        {
            _factory = g.Factory.CoordinateFactory;
            _centroid = g.Centroid.Coordinate;
            add(g);
        }

        public TCoordinate InteriorPoint
        {
            get { return _interiorPoint; }
        }

        /// <summary> 
        /// Tests the point(s) defined by a Geometry for the best inside 
        /// point. If a Geometry is not of dimension 0 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void add(IGeometry<TCoordinate> geom)
        {
            if (geom is IPoint<TCoordinate>)
            {
                IPoint<TCoordinate> point = geom as IPoint<TCoordinate>;
                add(point.Coordinate);
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;

                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    add(geometry);
                }
            }
        }

        private void add(TCoordinate point)
        {
            Double dist = point.Distance(_centroid);

            if (dist < _minDistance)
            {
                _interiorPoint = _factory.Create(point);
                _minDistance = dist;
            }
        }
    }
}