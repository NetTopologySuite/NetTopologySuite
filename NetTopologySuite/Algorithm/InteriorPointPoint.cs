using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes a point in the interior of an point point.
    /// Algorithm:
    /// Find a point which is closest to the centroid of the point.
    /// </summary>
    public class InteriorPointPoint
    {
        private ICoordinate centroid;
        private Double minDistance = Double.MaxValue;
        private ICoordinate interiorPoint = null;

        public InteriorPointPoint(IGeometry g)
        {
            centroid = g.Centroid.Coordinate;
            Add(g);
        }

        /// <summary> 
        /// Tests the point(s) defined by a Geometry for the best inside 
        /// point. If a Geometry is not of dimension 0 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(IGeometry geom)
        {
            if (geom is IPoint)
            {
                Add(geom.Coordinate);
            }
            else if (geom is IGeometryCollection)
            {
                IGeometryCollection gc = (IGeometryCollection) geom;

                foreach (IGeometry geometry in gc.Geometries)
                {
                    Add(geometry);
                }
            }
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

        public ICoordinate InteriorPoint
        {
            get { return interiorPoint; }
        }
    }
}