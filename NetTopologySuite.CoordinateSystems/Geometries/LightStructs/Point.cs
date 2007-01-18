using System;
using System.Collections.Generic;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Geometries.LightStructs
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct Point : ICloneable
    {
        public static Point Null = new Point(Coordinate.Null);

        private Coordinate coordinate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        public Point(Coordinate coordinate) : 
            this(coordinate.X, coordinate.Y, coordinate.Z) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(double x, double y) : this(x, y, Double.NaN) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Point(double x, double y, double z)
        {
            this.coordinate = new Coordinate(x, y, z);            
        }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate Coordinate
        {
            get { return coordinate; }
            set { coordinate = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double X
        {
            get { return coordinate.X; }
            set { coordinate.X = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Y
        {
            get { return coordinate.Y; }
            set { coordinate.Y = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Z
        {
            get { return coordinate.Z; }
            set { coordinate.Z = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Point p = new Point();
            p.Coordinate = (Coordinate) Coordinate.Clone();
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Coordinate.ToString();
        }
    }
}
