using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes a point in the interior of an linear point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// Find an interior vertex which is closest to
    /// the centroid of the linestring.
    /// If there is no interior vertex, find the endpoint which is
    /// closest to the centroid.
    /// </remarks>
    public class InteriorPointLine<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ICoordinateFactory<TCoordinate> _factory;
        private readonly TCoordinate _centroid = default(TCoordinate);
        private Double minDistance = Double.MaxValue;
        private TCoordinate _interiorPoint = default(TCoordinate);

        public InteriorPointLine(IGeometry<TCoordinate> g)
        {
            _factory = g.Factory.CoordinateFactory;
            _centroid = g.Centroid.Coordinate;
            AddInterior(g);

            if (Coordinates<TCoordinate>.IsEmpty(_interiorPoint))
            {
                AddEndpoints(g);
            }
        }

        public TCoordinate InteriorPoint
        {
            get { return _interiorPoint; }
        }

        /// <summary>
        /// Tests the interior vertices (if any)
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddInterior(IGeometry<TCoordinate> geom)
        {
            if (geom is ILineString<TCoordinate>)
            {
                AddInterior(geom.Coordinates);
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;
                Debug.Assert(gc != null);
                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    AddInterior(geometry);
                }
            }
        }

        private void AddInterior(IEnumerable<TCoordinate> points)
        {
            foreach (TCoordinate point in points)
            {
                add(point);
            }
        }

        /// <summary> 
        /// Tests the endpoint vertices
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddEndpoints(IGeometry<TCoordinate> geom)
        {
            if (geom is ILineString<TCoordinate>)
            {
                AddEndpoints(geom.Coordinates);
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;

                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    AddEndpoints(geometry);
                }
            }
        }

        private void AddEndpoints(IEnumerable<TCoordinate> points)
        {
            add(Slice.GetFirst(points));
            add(Slice.GetLast(points));
        }

        private void add(TCoordinate point)
        {
            Double dist = point.Distance(_centroid);

            if (dist < minDistance)
            {
                _interiorPoint = _factory.Create(point);
                minDistance = dist;
            }
        }
    }
}