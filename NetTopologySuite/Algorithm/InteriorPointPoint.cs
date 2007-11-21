using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly TCoordinate _centroid;
        private Double _minDistance = Double.MaxValue;
        private TCoordinate _interiorPoint = default(TCoordinate);

        public InteriorPointPoint(IGeometry<TCoordinate> g)
        {
            _centroid = g.Centroid.Coordinate;
            Add(g);
        }

        /// <summary> 
        /// Tests the point(s) defined by a Geometry for the best inside 
        /// point. If a Geometry is not of dimension 0 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(IGeometry<TCoordinate> geom)
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

        private void Add(ICoordinate point)
        {
            Double dist = point.Distance(_centroid);

            if (dist < _minDistance)
            {
                _interiorPoint = new TCoordinate(point);
                _minDistance = dist;
            }
        }

        public ICoordinate InteriorPoint
        {
            get { return _interiorPoint; }
        }
    }
}