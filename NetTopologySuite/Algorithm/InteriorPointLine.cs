using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
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
        private ICoordinate centroid = null;
        private Double minDistance = Double.MaxValue;
        private ICoordinate interiorPoint = null;

        public InteriorPointLine(IGeometry g)
        {
            centroid = g.Centroid.Coordinate;
            AddInterior(g);

            if (interiorPoint == null)
            {
                AddEndpoints(g);
            }
        }

        public ICoordinate InteriorPoint
        {
            get { return interiorPoint; }
        }

        /// <summary>
        /// Tests the interior vertices (if any)
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddInterior(IGeometry geom)
        {
            if (geom is ILineString)
            {
                AddInterior(geom.Coordinates);
            }
            else if (geom is IGeometryCollection)
            {
                IGeometryCollection gc = (IGeometryCollection) geom;

                foreach (IGeometry geometry in gc.Geometries)
                {
                    AddInterior(geometry);
                }
            }
        }

        private void AddInterior(ICoordinate[] pts)
        {
            for (Int32 i = 1; i < pts.Length - 1; i++)
            {
                Add(pts[i]);
            }
        }

        /// <summary> 
        /// Tests the endpoint vertices
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddEndpoints(IGeometry geom)
        {
            if (geom is ILineString)
            {
                AddEndpoints(geom.Coordinates);
            }
            else if (geom is IGeometryCollection)
            {
                IGeometryCollection gc = (IGeometryCollection) geom;

                foreach (IGeometry geometry in gc.Geometries)
                {
                    AddEndpoints(geometry);
                }
            }
        }

        private void AddEndpoints(ICoordinate[] pts)
        {
            Add(pts[0]);
            Add(pts[pts.Length - 1]);
        }

        private void Add(ICoordinate point)
        {
            Double dist = point.Distance(centroid);

            if (dist < minDistance)
            {
                interiorPoint = new Coordinate(point);
                minDistance = dist;
            }
        }
    }
}