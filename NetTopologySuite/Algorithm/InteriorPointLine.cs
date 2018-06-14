using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes a point in the interior of an linear point.
    /// Algorithm:
    /// Find an interior vertex which is closest to
    /// the centroid of the linestring.
    /// If there is no interior vertex, find the endpoint which is
    /// closest to the centroid.
    /// </summary>
    public class InteriorPointLine
    {
        private readonly Coordinate _centroid;
        private double _minDistance = double.MaxValue;
        private Coordinate _interiorPoint;

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        public InteriorPointLine(IGeometry g)
        {
            _centroid = g.Centroid.Coordinate;
            AddInterior(g);

            if (_interiorPoint == null)
                AddEndpoints(g);
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate InteriorPoint => _interiorPoint;

        /// <summary>
        /// Tests the interior vertices (if any)
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddInterior(IGeometry geom)
        {
            if(geom is ILineString)
                AddInterior(geom.Coordinates);
            else if(geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    AddInterior(geometry);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        private void AddInterior(Coordinate[] pts)
        {
            for (int i = 1; i < pts.Length - 1; i++)
                Add(pts[i]);

        }

        /// <summary>
        /// Tests the endpoint vertices
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddEndpoints(IGeometry geom)
        {
            if(geom is ILineString)
                AddEndpoints(geom.Coordinates);
            else if(geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    AddEndpoints(geometry);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        private void AddEndpoints(Coordinate[] pts)
        {
            Add(pts[0]);
            Add(pts[pts.Length - 1]);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        private void Add(Coordinate point)
        {
            double dist = point.Distance(_centroid);
            if (dist < _minDistance)
            {
                _interiorPoint = new Coordinate(point);
                _minDistance = dist;
            }
        }
    }
}
