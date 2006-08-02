using System;
using System.Collections;
using System.Text;
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
        private Coordinate centroid = null;
        private double minDistance = Double.MaxValue;
        private Coordinate interiorPoint = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        public InteriorPointLine(Geometry g)
        {
            centroid = g.Centroid.Coordinate;
            AddInterior(g);

            if (interiorPoint == null)                
                AddEndpoints(g);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Coordinate InteriorPoint
        {
            get
            {
                return interiorPoint;
            }
        }

        /// <summary>
        /// Tests the interior vertices (if any)
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddInterior(Geometry geom)
        {
            if(geom is LineString) 
                AddInterior(geom.Coordinates);            
            else if(geom is GeometryCollection) 
            {
                GeometryCollection gc = (GeometryCollection)geom;
                foreach (Geometry geometry in gc.Geometries)
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
        private void AddEndpoints(Geometry geom)
        {
            if(geom is LineString) 
                AddEndpoints(geom.Coordinates);   
            else if(geom is GeometryCollection) 
            {
                GeometryCollection gc = (GeometryCollection)geom;
                foreach (Geometry geometry in gc.Geometries)
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
            double dist = point.Distance(centroid);
            if (dist < minDistance)
            {
                interiorPoint = new Coordinate(point);
                minDistance = dist;
            }
        }
    }
}
