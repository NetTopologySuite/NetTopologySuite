using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// Computes various kinds of common geometric shapes.
    /// Allows various ways of specifying the location and extent of the shapes,
    /// as well as number of line segments used to form them.
    /// </summary>
    public class GeometricShapeFactory
    {
        protected IGeometryFactory GeomFact;
        private readonly Dimensions _dim = new Dimensions();
        private int _nPts = 100;

        /// <summary>
        /// Create a shape factory which will create shapes using the default GeometryFactory.
        /// </summary>
        public GeometricShapeFactory() : this(new GeometryFactory()) { }

        /// <summary>
        /// Create a shape factory which will create shapes using the given GeometryFactory.
        /// </summary>
        /// <param name="geomFact">The factory to use.</param>
        public GeometricShapeFactory(IGeometryFactory geomFact)
        {
            GeomFact = geomFact;
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
                return _dim.Base;
            }
            set
            {
                _dim.Base = value;
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
                return _dim.Centre;
            }
            set
            {
                _dim.Centre = value;
            }
        }

        /// <summary>
        /// Gets the envelope of the shape
        /// </summary>
        public IEnvelope Envelope
        {
            get { return _dim.Envelope; }
        }

        /// <summary>
        /// Gets/Sets the total number of points in the created Geometry.
        /// </summary>
        public int NumPoints
        {
            get
            {
                return _nPts;
            }
            set
            {
                _nPts = value;
            }
        }

        /// <summary>
        /// Gets/Sets the size of the extent of the shape in both x and y directions.        
        /// </summary>                
        public double Size
        {
            get
            {
                return _dim.Size;
            }
            set
            {
                _dim.Size = value;
            }
        }

        /// <summary>
        /// Gets/Sets the width of the shape.
        /// </summary>
        public double Width
        {
            get
            {
                return _dim.Width;
            }
            set
            {
                _dim.Width = value;
            }
        }

        /// <summary>
        /// Gets/Sets the height of the shape.
        /// </summary>
        public double Height
        {
            get
            {
                return _dim.Height;
            }
            set
            {
                _dim.Height = value;
            }
        }

        protected ICoordinate CreateCoord(double x, double y)
        {
            ICoordinate p = new Coordinate(x, y);
            GeomFact.PrecisionModel.MakePrecise(p);
            return p;
        }

        /// <summary>
        /// Creates a rectangular <c>Polygon</c>.
        /// </summary>
        /// <returns>A rectangular polygon.</returns>
        public IPolygon CreateRectangle()
        {
            int i;
            int ipt = 0;
            int nSide = _nPts / 4;
            if (nSide < 1) nSide = 1;
            double XsegLen = _dim.Envelope.Width / nSide;
            double YsegLen = _dim.Envelope.Height / nSide;

            ICoordinate[] pts = new ICoordinate[4 * nSide + 1];
            IEnvelope env = _dim.Envelope;            

            for (i = 0; i < nSide; i++) 
            {
                double x = env.MinX + i * XsegLen;
                double y = env.MinY;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                double x = env.MaxX;
                double y = env.MinY + i * YsegLen;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                double x = env.MaxX - i * XsegLen;
                double y = env.MaxY;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                double x = env.MinX;
                double y = env.MaxY - i * YsegLen;
                pts[ipt++] = CreateCoord(x, y);
            }
            pts[ipt++] = new Coordinate(pts[0]);

            ILinearRing ring = GeomFact.CreateLinearRing(pts);
            IPolygon poly = GeomFact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a circular <c>Polygon</c>.
        /// </summary>
        /// <returns>A circular polygon.</returns>
        public IPolygon CreateCircle()
        {
            IEnvelope env = _dim.Envelope;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            ICoordinate[] pts = new ICoordinate[_nPts + 1];
            int iPt = 0;
            for (int i = 0; i < _nPts; i++) 
            {
                double ang = i * (2 * Math.PI / _nPts);
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                ICoordinate pt = CreateCoord(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];

            ILinearRing ring = GeomFact.CreateLinearRing(pts);
            IPolygon poly = GeomFact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a elliptical arc, as a LineString.
        /// </summary>
        /// <param name="startAng"></param>
        /// <param name="endAng"></param>
        /// <returns></returns>
        public ILineString CreateArc(double startAng, double endAng)
        {
            IEnvelope env = _dim.Envelope;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            double angSize = (endAng - startAng);
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            double angInc = angSize / _nPts;

            ICoordinate[] pts = new ICoordinate[_nPts];
            int iPt = 0;
            for (int i = 0; i < _nPts; i++) 
            {
                double ang = startAng + i * angInc;
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                ICoordinate pt = CreateCoord(x, y);
                pts[iPt++] = pt;
            }
            ILineString line = GeomFact.CreateLineString(pts);
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        protected class Dimensions
        {
            private ICoordinate _basecoord;

            /// <summary>
            /// 
            /// </summary>
            public ICoordinate Base
            {
                get { return _basecoord; }
                set { _basecoord = value; }
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

            private double width;

            /// <summary>
            /// 
            /// </summary>
            public double Width
            {
                get { return width; }
                set { width = value; }
            }

            private double height;

            /// <summary>
            /// 
            /// </summary>
            public double Height
            {
                get { return height; }
                set { height = value; }
            }                                  

            /// <summary>
            /// 
            /// </summary>
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

            /// <summary>
            /// 
            /// </summary>
            public IEnvelope Envelope
            {
                get
                {
                    if (Base != null)
                        return new Envelope(Base.X, Base.X + Width, Base.Y, Base.Y + Height);                    
                    if (Centre != null)
                        return new Envelope(Centre.X - Width / 2, Centre.X + Width / 2,
                                            Centre.Y - Height / 2, Centre.Y + Height / 2);                    
                    return new Envelope(0, Width, 0, Height);
                }
            }
        }
    }
}
