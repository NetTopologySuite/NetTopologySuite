using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

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
        protected IPrecisionModel PrecModel = null;
        private readonly Dimensions _dim = new Dimensions();
        private int _nPts = 100;

        // default is no rotation.
        private double _rotationAngle;

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
            PrecModel = geomFact.PrecisionModel;
        }

        /// <summary>
        /// Gets/Sets the location of the shape by specifying the base coordinate
        /// (which in most cases is the
        /// lower left point of the envelope containing the shape).
        /// </summary>
        public Coordinate Base  
        {
            set { _dim.Base = value; }
        }

        /// <summary>
        /// Gets/Sets the location of the shape by specifying the centre of
        /// the shape's bounding box.
        /// </summary>
        public Coordinate Centre
        {
            set { _dim.Centre = value; }
        }

        /// <summary>
        /// Gets or sets the envelope of the shape
        /// </summary>
        public Envelope Envelope
        {
            get { return _dim.Envelope; }
            set { _dim.Envelope = value; }
        }

        /// <summary>
        /// Gets/Sets the total number of points in the created Geometry.
        /// </summary>
        public int NumPoints
        {
            get { return _nPts; }
            set { _nPts = value; }
        }

        /// <summary>
        /// Gets/Sets the size of the extent of the shape in both x and y directions.        
        /// </summary>                
        public double Size
        {
            set { _dim.Size = value; }
        }

        /// <summary>
        /// Gets/Sets the width of the shape.
        /// </summary>
        public double Width
        {
            get { return _dim.Width; }
            set { _dim.Width = value; }
        }

        /// <summary>
        /// Gets/Sets the height of the shape.
        /// </summary>
        public double Height
        {
            get { return _dim.Height; }
            set { _dim.Height = value; }
        }

        /// <summary>
        /// Gets/Sets the rotation angle, in radians, to use for the shape.
        /// The rotation is applied relative to the centre of the shape.
        /// </summary>        
        public double Rotation
        {
            get { return _rotationAngle; }
            set { _rotationAngle = value; }
        }

        protected IGeometry Rotate(IGeometry geom)
        {
            if (_rotationAngle != 0.0)
            {
                var centre = _dim.Centre;
                var trans = AffineTransformation.RotationInstance(_rotationAngle,
                    centre.X, centre.Y);
                geom.Apply(trans);
            }
            return geom;
        }
        protected Coordinate CreateCoord(double x, double y)
        {
            var p = new Coordinate(x, y);
            PrecModel.MakePrecise(p);
            return p;
        }

        protected Coordinate CreateCoordTrans(double x, double y, Coordinate trans)
        {
            return CreateCoord(x + trans.X, y + trans.Y);
        }

        /// <summary>
        /// Creates a rectangular <c>Polygon</c>.
        /// </summary>
        /// <returns>A rectangular polygon.</returns>
        public IPolygon CreateRectangle()
        {
            int i;
            var ipt = 0;
            var nSide = _nPts / 4;
            if (nSide < 1) nSide = 1;
            var xSegLen = _dim.Envelope.Width / nSide;
            var ySegLen = _dim.Envelope.Height / nSide;

            var pts = new Coordinate[4 * nSide + 1];
            var env = _dim.Envelope;            

            for (i = 0; i < nSide; i++) 
            {
                var x = env.MinX + i * xSegLen;
                var y = env.MinY;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                var x = env.MaxX;
                var y = env.MinY + i * ySegLen;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                var x = env.MaxX - i * xSegLen;
                var y = env.MaxY;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++) 
            {
                var x = env.MinX;
                var y = env.MaxY - i * ySegLen;
                pts[ipt++] = CreateCoord(x, y);
            }
            pts[ipt] = new Coordinate(pts[0]);

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring, null);
            return (IPolygon) Rotate(poly);
        }

        /// <summary>
        /// Creates a circular <c>Polygon</c>.
        /// </summary>
        /// <returns>A circular polygon.</returns>
        public IPolygon CreateCircle()
        {
            var env = _dim.Envelope;
            var xRadius = env.Width / 2.0;
            var yRadius = env.Height / 2.0;

            var centreX = env.MinX + xRadius;
            var centreY = env.MinY + yRadius;

            var pts = new Coordinate[_nPts + 1];
            var iPt = 0;
            for (var i = 0; i < _nPts; i++) 
            {
                var ang = i * (2 * Math.PI / _nPts);
                var x = xRadius * Math.Cos(ang) + centreX;
                var y = yRadius * Math.Sin(ang) + centreY;
                var pt = CreateCoord(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring, null);
            return (IPolygon) Rotate(poly);
        }

        /// <summary>
        /// Creates an elliptical <c>Polygon</c>.
        /// If the supplied envelope is square the 
        /// result will be a circle. 
        /// </summary>
        /// <returns>An an ellipse or circle.</returns>
        public IPolygon CeateEllipse()
        {
            var env = _dim.Envelope;
            var xRadius = env.Width / 2.0;
            var yRadius = env.Height / 2.0;

            var centreX = env.MinX + xRadius;
            var centreY = env.MinY + yRadius;

            var pts = new Coordinate[_nPts + 1];
            var iPt = 0;
            for (int i = 0; i < _nPts; i++)
            {
                var ang = i * (2 * Math.PI / _nPts);
                var x = xRadius * Math.Cos(ang) + centreX;
                var y = yRadius * Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = new Coordinate(pts[0]);

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring, null);
            return (IPolygon) Rotate(poly);
        }

        /// <summary>
        /// Creates a squircular <see cref="Polygon"/>.
        /// </summary>
        /// <returns>a squircle</returns>
        public IPolygon CreateSquircle()
        {
            return CreateSupercircle(4);
        }

        /// <summary>
        /// Creates a supercircular <see cref="Polygon"/>
        /// of a given positive power.
        /// </summary>
        /// <returns>a supercircle</returns>
        public IPolygon CreateSupercircle(double power)
        {
            var recipPow = 1.0 / power;

            var radius = _dim.MinSize / 2;
            var centre = _dim.Centre;

            var r4 = Math.Pow(radius, power);
            var y0 = radius;

            var xyInt = Math.Pow(r4 / 2, recipPow);

            var nSegsInOct = _nPts / 8;
            var totPts = nSegsInOct * 8 + 1;
            var pts = new Coordinate[totPts];
            var xInc = xyInt / nSegsInOct;

            for (var i = 0; i <= nSegsInOct; i++)
            {
                var x = 0.0;
                var y = y0;
                if (i != 0)
                {
                    x = xInc * i;
                    var x4 = Math.Pow(x, power);
                    y = Math.Pow(r4 - x4, recipPow);
                }
                pts[i] = CreateCoordTrans(x, y, centre);
                pts[2 * nSegsInOct - i] = CreateCoordTrans(y, x, centre);

                pts[2 * nSegsInOct + i] = CreateCoordTrans(y, -x, centre);
                pts[4 * nSegsInOct - i] = CreateCoordTrans(x, -y, centre);

                pts[4 * nSegsInOct + i] = CreateCoordTrans(-x, -y, centre);
                pts[6 * nSegsInOct - i] = CreateCoordTrans(-y, -x, centre);

                pts[6 * nSegsInOct + i] = CreateCoordTrans(-y, x, centre);
                pts[8 * nSegsInOct - i] = CreateCoordTrans(-x, y, centre);
            }
            pts[pts.Length - 1] = new Coordinate(pts[0]);

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring, null);
            return (IPolygon) Rotate(poly);
        }

        /// <summary>
        /// Creates a elliptical arc, as a LineString.
        /// </summary><remarks>
        /// The arc is always created in a counter-clockwise direction.
        /// </remarks>
        /// <param name="startAng">Start angle in radians</param>
        /// <param name="angExtent">Size of angle in radians</param>
        /// <returns></returns>
        public ILineString CreateArc(double startAng, double angExtent)
        {
            var env = _dim.Envelope;
            var xRadius = env.Width / 2.0;
            var yRadius = env.Height / 2.0;

            var centreX = env.MinX + xRadius;
            var centreY = env.MinY + yRadius;

            var angSize = angExtent;
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            var angInc = angSize / (_nPts - 1);

            var pts = new Coordinate[_nPts];
            var iPt = 0;
            for (var i = 0; i < _nPts; i++) 
            {
                var ang = startAng + i * angInc;
                var x = xRadius * Math.Cos(ang) + centreX;
                var y = yRadius * Math.Sin(ang) + centreY;
                var pt = CreateCoord(x, y);
                pts[iPt++] = pt;
            }
            var line = GeomFact.CreateLineString(pts);
            return (ILineString) Rotate(line);
        }

        ///<summary>
        /// Creates an elliptical arc polygon.
        ///</summary>
        /// <remarks>
        /// The polygon is formed from the specified arc of an ellipse
        /// and the two radii connecting the endpoints to the centre of the ellipse.
        /// </remarks>
        /// <param name="startAng">Start angle in radians</param>
        /// <param name="angExtent">Size of angle in radians</param>
        /// <returns>An elliptical arc polygon</returns>
        public IPolygon CreateArcPolygon(double startAng, double angExtent)
        {
            var env = _dim.Envelope;
            var xRadius = env.Width / 2.0;
            var yRadius = env.Height / 2.0;

            var centreX = env.MinX + xRadius;
            var centreY = env.MinY + yRadius;

            var angSize = angExtent;
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            var angInc = angSize / (_nPts - 1);
            // var check = angInc * nPts;
            // var checkEndAng = startAng + check;

            var pts = new Coordinate[_nPts + 2];

            var iPt = 0;
            pts[iPt++] = CreateCoord(centreX, centreY);
            for (var i = 0; i < _nPts; i++)
            {
                var ang = startAng + angInc * i;

                var x = xRadius * Math.Cos(ang) + centreX;
                var y = yRadius * Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = CreateCoord(centreX, centreY);
            var ring = GeomFact.CreateLinearRing(pts);
            var geom = GeomFact.CreatePolygon(ring, null);
            return (IPolygon) Rotate(geom);
        }

        protected class Dimensions
        {
            private Coordinate _base;

            public Coordinate Base
            {
                get { return _base; }
                set { _base = value; }
            }

            private Coordinate _centre;

            public Coordinate Centre
            {
                get
                {
                    if (_centre == null)
                    {
                        _centre = (Base != null)
                                      ? new Coordinate(Base.X + Width * 0.5d, Base.Y + Height * 0.5d)
                                      : new Coordinate(0, 0);
                    }
                    return _centre;
                }
                set { _centre = value; }
            }

            private double _width;

           public double Width
            {
                get { return _width; }
                set { _width = value; }
            }

            private double _height;

           public double Height
            {
                get { return _height; }
                set { _height = value; }
            }                                  

            public double Size
            {
                set
                {
                    Height = value;
                    Width = value;
                }
            }

            public double MinSize
            {
                get
                {
                    return Math.Min(Width, Height);
                }                
            }

            public Envelope Envelope
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
                set
                {
    	            _width = value.Width;
                    _height = value.Height;
                    _base = new Coordinate(value.MinX, value.MinY);
                    _centre = new Coordinate(value.Centre);
                }
            }
        }
    }
}
