using System;
using System.Collections;
using System.Text;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// Computes various kinds of common geometric shapes.
    /// Allows various ways of specifying the location and extent of the shapes,
    /// as well as number of line segments used to form them.
    /// </summary>
    public class GeometricShapeFactory<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>, IConvertible
    {
        private GeometryFactory geomFact;
        private Dimensions dim = new Dimensions();
        private int nPts = 100;

        /// <summary>
        /// Create a shape factory which will create shapes using the default GeometryFactory.
        /// </summary>
        public GeometricShapeFactory() : this(new GeometryFactory()) { }

        /// <summary>
        /// Create a shape factory which will create shapes using the given GeometryFactory.
        /// </summary>
        /// <param name="geomFact">The factory to use.</param>
        public GeometricShapeFactory(GeometryFactory geomFact)
        {
            this.geomFact = geomFact;
        }

        /// <summary>
        /// Gets or sets the location of the shape by specifying the base coordinate
        /// (which in most cases is the
        /// lower left point of the envelope containing the shape).
        /// </summary>
        public TCoordinate Base
        {
            get
            {
                return dim.Base;
            }
            set
            {
                dim.Base = value;
            }
        }

        /// <summary>
        /// Gets or sets the location of the shape by specifying the centre of
        /// the shape's bounding box.
        /// </summary>
        public TCoordinate Center
        {
            get
            {
                return dim.Center;
            }
            set
            {
                dim.Center = value;
            }
        }

        /// <summary>
        /// Gets or sets the total number of points in the created Geometry.
        /// </summary>
        public int NumPoints
        {
            get
            {
                return nPts;
            }
            set
            {
                nPts = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the extent of the shape in both x and y directions.        
        /// </summary>                
        public double Size
        {
            get
            {
                return dim.Size;
            }
            set
            {
                dim.Size = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the shape.
        /// </summary>
        public double Width
        {
            get
            {
                return dim.Width;
            }
            set
            {
                dim.Width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the shape.
        /// </summary>
        public double Height
        {
            get
            {
                return dim.Height;
            }
            set
            {
                dim.Height = value;
            }
        }

        /// <summary>
        /// Creates a rectangular <c>Polygon</c>.
        /// </summary>
        /// <returns>A rectangular polygon.</returns>
        public IPolygon<TCoordinate> CreateRectangle()
        {
            int i;
            int ipt = 0;
            int nSide = nPts / 4;
            if (nSide < 1) nSide = 1;
            double XsegLen = dim.Extents.Width / nSide;
            double YsegLen = dim.Extents.Height / nSide;

            TCoordinate[] pts = new TCoordinate[4 * nSide + 1];
            IExtents<TCoordinate> env = dim.Extents;

            for (i = 0; i < nSide; i++)
            {
                double x = env.MinX + i * XsegLen;
                double y = env.MinY;
                pts[ipt++] = new Coordinate(x, y);
            }

            for (i = 0; i < nSide; i++)
            {
                double x = env.MaxX;
                double y = env.MinY + i * YsegLen;
                pts[ipt++] = new Coordinate(x, y);
            }

            for (i = 0; i < nSide; i++)
            {
                double x = env.MaxX - i * XsegLen;
                double y = env.MaxY;
                pts[ipt++] = new Coordinate(x, y);
            }

            for (i = 0; i < nSide; i++)
            {
                double x = env.MinX;
                double y = env.MaxY - i * YsegLen;
                pts[ipt++] = new Coordinate(x, y);
            }

            pts[ipt++] = new Coordinate(pts[0]);

            ILinearRing<TCoordinate> ring = geomFact.CreateLinearRing(pts);
            IPolygon<TCoordinate> poly = geomFact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a circular <c>Polygon</c>.
        /// </summary>
        /// <returns>A circular polygon.</returns>
        public IPolygon<TCoordinate> CreateCircle()
        {
            IExtents<TCoordinate> env = dim.Extents;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            ICoordinate[] pts = new ICoordinate[nPts + 1];
            int iPt = 0;
            for (int i = 0; i < nPts; i++)
            {
                double ang = i * (2 * Math.PI / nPts);
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                ICoordinate pt = new Coordinate(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];

            ILinearRing<TCoordinate> ring = geomFact.CreateLinearRing(pts);
            IPolygon<TCoordinate> poly = geomFact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a elliptical arc, as a LineString.
        /// </summary>
        /// <param name="startAng"></param>
        /// <param name="endAng"></param>
        /// <returns></returns>
        public ILineString<TCoordinate> CreateArc(double startAng, double endAng)
        {
            IExtents<TCoordinate> env = dim.Extents ;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            double angSize = (endAng - startAng);
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            double angInc = angSize / nPts;

            TCoordinate[] pts = new TCoordinate[nPts];
            int iPt = 0;

            for (int i = 0; i < nPts; i++)
            {
                double ang = startAng + i * angInc;
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                TCoordinate pt = new Coordinate(x, y);
                geomFact.PrecisionModel.MakePrecise(pt);
                pts[iPt++] = pt;
            }

            ILineString<TCoordinate> line = geomFact.CreateLineString(pts);
            return line;
        }

        private class Dimensions
        {
            private TCoordinate basecoord;

            public TCoordinate Base
            {
                get { return basecoord; }
                set { basecoord = value; }
            }

            private TCoordinate centre;

            public TCoordinate Center
            {
                get { return centre; }
                set { centre = value; }
            }

            private double width;

            public double Width
            {
                get { return width; }
                set { width = value; }
            }

            private double height;

            public double Height
            {
                get { return height; }
                set { height = value; }
            }

            public double Size
            {
                get
                {
                    return Math.Max(Width, Height);
                }
                set
                {
                    Height = value;
                    Width = value;
                }
            }

            public IExtents<TCoordinate> Extents
            {
                get
                {
                    if (Base != null)
                    {
                        return new Envelope(Base.X, Base.X + Width, Base.Y, Base.Y + Height);
                    }

                    if (Center != null)
                    {
                        return new Envelope(Center.X - Width / 2, Center.X + Width / 2,
                                            Center.Y - Height / 2, Center.Y + Height / 2);
                    }

                    return new Envelope(0, Width, 0, Height);
                }
            }
        }
    }
}
