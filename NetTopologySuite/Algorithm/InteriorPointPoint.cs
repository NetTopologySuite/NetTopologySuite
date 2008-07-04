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
        private double minDistance = Double.MaxValue;
        private ICoordinate interiorPoint = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        public InteriorPointPoint(IGeometry g)
        {
            centroid = g.Centroid.Coordinate;
            Add(g);
        }

        /// <summary> 
        /// Tests the point(s) defined by a Geometry for the best inside point.
        /// If a Geometry is not of dimension 0 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(IGeometry geom)
        {
            if (geom is IPoint)
                Add(geom.Coordinate);    
            else if (geom is IGeometryCollection) 
            {
                IGeometryCollection gc = (IGeometryCollection) geom;
                foreach (IGeometry geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        private void Add(ICoordinate point)
        {
            double dist = point.Distance(centroid);
            if (dist < minDistance)
            {
                interiorPoint = new Coordinate(point);
                minDistance = dist;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate InteriorPoint
        {
            get
            {
                return interiorPoint;
            }
        }
    }   
}
