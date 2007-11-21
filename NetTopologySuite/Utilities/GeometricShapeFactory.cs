using System;
using GeoAPI.Coordinates;
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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private GeometryFactory<TCoordinate> geomFact;
        private Dimensions dim = new Dimensions();
        private Int32 nPts = 100;

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
        /// Gets/Sets the location of the shape by specifying the base coordinate
        /// (which in most cases is the
        /// lower left point of the envelope containing the shape).
        /// </summary>
        public ICoordinate Base  
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
        /// Gets/Sets the location of the shape by specifying the centre of
        /// the shape's bounding box.
        /// </summary>
        public ICoordinate Centre
        {
            get
            {
                return dim.Centre;
            }
            set
            {
                dim.Centre = value;
            }
        }

        /// <summary>
        /// Gets/Sets the total number of points in the created Geometry.
        /// </summary>
        public Int32 NumPoints
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
        /// Gets/Sets the size of the extent of the shape in both x and y directions.        
        /// </summary>                
        public Double Size
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
        /// Gets/Sets the width of the shape.
        /// </summary>
        public Double Width
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
        /// Gets/Sets the height of the shape.
        /// </summary>
        public Double Height
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
        /// Creates a rectangular <see cref="Polygon{TCoordinate}" />.
        /// </summary>
        /// <returns>A rectangular polygon.</returns>
        public IPolygon CreateRectangle()
        {
            Int32 i;
            Int32 ipt = 0;
            Int32 nSide = nPts / 4;
            if (nSide < 1) nSide = 1;
            Double XsegLen = dim.Envelope.Width / nSide;
            Double YsegLen = dim.Envelope.Height / nSide;

            ICoordinate[] pts = new ICoordinate[4 * nSide + 1];
            IExtents env = dim.Envelope;            

            for (i = 0; i < nSide; i++) 
            {
                Double x = env.MinX + i * XsegLen;
                Double y = env.MinY;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                Double x = env.MaxX;
                Double y = env.MinY + i * YsegLen;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                Double x = env.MaxX - i * XsegLen;
                Double y = env.MaxY;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                Double x = env.MinX;
                Double y = env.MaxY - i * YsegLen;
                pts[ipt++] = new Coordinate(x, y);
            }
            pts[ipt++] = new Coordinate(pts[0]);

            ILinearRing ring = geomFact.CreateLinearRing(pts);
            IPolygon poly = geomFact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a circular <see cref="Polygon{TCoordinate}" />.
        /// </summary>
        /// <returns>A circular polygon.</returns>
        public IPolygon CreateCircle()
        {
            IExtents env = dim.Envelope;
            Double xRadius = env.Width / 2.0;
            Double yRadius = env.Height / 2.0;

            Double centreX = env.MinX + xRadius;
            Double centreY = env.MinY + yRadius;

            ICoordinate[] pts = new ICoordinate[nPts + 1];
            Int32 iPt = 0;
            for (Int32 i = 0; i < nPts; i++) 
            {
                Double ang = i * (2 * Math.PI / nPts);
                Double x = xRadius * Math.Cos(ang) + centreX;
                Double y = yRadius * Math.Sin(ang) + centreY;
                ICoordinate pt = new Coordinate(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];

            ILinearRing ring = geomFact.CreateLinearRing(pts);
            IPolygon poly = geomFact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a elliptical arc, as a LineString.
        /// </summary>
        /// <param name="startAng"></param>
        /// <param name="endAng"></param>
        /// <returns></returns>
        public ILineString CreateArc(Double startAng, Double endAng)
        {
            IExtents env = dim.Envelope;
            Double xRadius = env.Width / 2.0;
            Double yRadius = env.Height / 2.0;

            Double centreX = env.MinX + xRadius;
            Double centreY = env.MinY + yRadius;

            Double angSize = (endAng - startAng);
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            Double angInc = angSize / nPts;

            ICoordinate[] pts = new ICoordinate[nPts];
            Int32 iPt = 0;
            for (Int32 i = 0; i < nPts; i++) 
            {
                Double ang = startAng + i * angInc;
                Double x = xRadius * Math.Cos(ang) + centreX;
                Double y = yRadius * Math.Sin(ang) + centreY;
                ICoordinate pt = new Coordinate(x, y);
                geomFact.PrecisionModel.MakePrecise( pt);
                pts[iPt++] = pt;
            }
            ILineString line = geomFact.CreateLineString(pts);
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        private class Dimensions
        {
            private ICoordinate basecoord;

            /// <summary>
            /// 
            /// </summary>
            public ICoordinate Base
            {
                get { return basecoord; }
                set { basecoord = value; }
            }

            private ICoordinate centre;

            /// <summary>
            /// 
            /// </summary>
            public ICoordinate Centre
            {
                get { return centre; }
                set { centre = value; }
            }

            private Double width;

            /// <summary>
            /// 
            /// </summary>
            public Double Width
            {
                get { return width; }
                set { width = value; }
            }

            private Double height;

            /// <summary>
            /// 
            /// </summary>
            public Double Height
            {
                get { return height; }
                set { height = value; }
            }                                  

            /// <summary>
            /// 
            /// </summary>
            public Double Size
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

            /// <summary>
            /// 
            /// </summary>
            public IExtents Envelope
            {
                get
                {
                    if (Base != null)
                        return new Extents(Base.X, Base.X + Width, Base.Y, Base.Y + Height);                    
                    if (Centre != null)
                        return new Extents(Centre.X - Width / 2, Centre.X + Width / 2,
                                            Centre.Y - Height / 2, Centre.Y + Height / 2);                    
                    return new Extents(0, Width, 0, Height);
                }
            }
        }
    }
}
