using System;
using System.Collections;
using System.Text;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of a point point.
    /// Algorithm:
    /// Compute the average of all points.
    /// </summary>
    public class CentroidPoint
    {
        private int ptCount = 0;
        private Coordinate centSum = new Coordinate();

        /// <summary>
        /// 
        /// </summary>
        public CentroidPoint() { }

        /// <summary> 
        /// Adds the point(s) defined by a Geometry to the centroid total.
        /// If the point is not of dimension 0 it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(Geometry geom)
        {
            if (geom is Point)             
                Add((Coordinate) geom.Coordinate);

            else if(geom is GeometryCollection) 
            {
                GeometryCollection gc = (GeometryCollection)geom;
                foreach (Geometry geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary> 
        /// Adds the length defined by a coordinate.
        /// </summary>
        /// <param name="pt">A coordinate.</param>
        public void Add(Coordinate pt)
        {
            ptCount += 1;
            centSum.X += pt.X;
            centSum.Y += pt.Y;
        }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate Centroid
        {
            get
            {
                Coordinate cent = new Coordinate();
                cent.X = centSum.X / ptCount;
                cent.Y = centSum.Y / ptCount;
                return cent;
            }
        }
    }   
}
