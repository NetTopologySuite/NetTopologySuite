using System;
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
        /// <summary>
        /// A geometry factory
        /// </summary>
        protected GeometryFactory GeomFact;

        /// <summary>
        /// A precision model
        /// </summary>
        /// 
        protected PrecisionModel PrecModel;
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
        public GeometricShapeFactory(GeometryFactory geomFact)
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
            set => _dim.Base = value;
        }

        /// <summary>
        /// Gets/Sets the location of the shape by specifying the centre of
        /// the shape's bounding box.
        /// </summary>
        public Coordinate Centre
        {
            set => _dim.Centre = value;
        }

        /// <summary>
        /// Gets or sets the envelope of the shape
        /// </summary>
        public Envelope Envelope
        {
            get => _dim.Envelope;
            set => _dim.Envelope = value;
        }

        /// <summary>
        /// Gets/Sets the total number of points in the created Geometry.
        /// </summary>
        public int NumPoints
        {
            get => _nPts;
            set => _nPts = value;
        }

        /// <summary>
        /// Gets/Sets the size of the extent of the shape in both x and y directions.
        /// </summary>
        public double Size
        {
            set => _dim.Size = value;
        }

        /// <summary>
        /// Gets/Sets the width of the shape.
        /// </summary>
        public double Width
        {
            get => _dim.Width;
            set => _dim.Width = value;
        }

        /// <summary>
        /// Gets/Sets the height of the shape.
        /// </summary>
        public double Height
        {
            get => _dim.Height;
            set => _dim.Height = value;
        }

        /// <summary>
        /// Gets/Sets the rotation angle, in radians, to use for the shape.
        /// The rotation is applied relative to the centre of the shape.
        /// </summary>
        public double Rotation
        {
            get => _rotationAngle;
            set => _rotationAngle = value;
        }

        /// <summary>
        /// Rotates a geometry by <see cref="Rotation"/> angle
        /// </summary>
        /// <param name="geom">The geometry to rotate</param>
        /// <returns>A rotated geometry</returns>
        protected Geometry Rotate(Geometry geom)
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

        /// <summary>
        /// Creates a coordinate at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordinate value</param>
        /// <returns>A coordinate</returns>
        protected Coordinate CreateCoord(double x, double y)
        {
            var p = new Coordinate(x, y);
            PrecModel.MakePrecise(p);
            return p;
        }

        /// <summary>
        /// Creates a translated coordinate at (<paramref name="x"/> + <paramref name="trans.X"/>, <paramref name="y"/> + <paramref name="trans.Y"/>)
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordinate value</param>
        /// <param name="trans">A translation vector (coordinate)</param>
        /// <returns>A coordinate</returns>
        protected Coordinate CreateCoordTrans(double x, double y, Coordinate trans)
        {
            return CreateCoord(x + trans.X, y + trans.Y);
        }

        /// <summary>
        /// Creates a rectangular <c>Polygon</c>.
        /// </summary>
        /// <returns>A rectangular polygon.</returns>
        public Polygon CreateRectangle()
        {
            int i;
            int ipt = 0;
            int nSide = _nPts / 4;
            if (nSide < 1) nSide = 1;
            double xSegLen = _dim.Envelope.Width / nSide;
            double ySegLen = _dim.Envelope.Height / nSide;

            var pts = new Coordinate[4 * nSide + 1];
            var env = _dim.Envelope;

            for (i = 0; i < nSide; i++)
            {
                double x = env.MinX + i * xSegLen;
                double y = env.MinY;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                double x = env.MaxX;
                double y = env.MinY + i * ySegLen;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                double x = env.MaxX - i * xSegLen;
                double y = env.MaxY;
                pts[ipt++] = CreateCoord(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                double x = env.MinX;
                double y = env.MaxY - i * ySegLen;
                pts[ipt++] = CreateCoord(x, y);
            }
            pts[ipt] = pts[0].Copy();

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring);
            return (Polygon) Rotate(poly);
        }

        /// <summary>
        /// Creates a circular <c>Polygon</c>.
        /// </summary>
        /// <returns>A circular polygon.</returns>
        public Polygon CreateCircle()
        {
            var env = _dim.Envelope;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            var pts = new Coordinate[_nPts + 1];
            int iPt = 0;
            for (int i = 0; i < _nPts; i++)
            {
                double ang = i * (2 * Math.PI / _nPts);
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                var pt = CreateCoord(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring);
            return (Polygon) Rotate(poly);
        }

        /// <summary>
        /// Creates an elliptical <c>Polygon</c>.
        /// If the supplied envelope is square the
        /// result will be a circle.
        /// </summary>
        /// <returns>An an ellipse or circle.</returns>
        public Polygon CreateEllipse()
        {
            var env = _dim.Envelope;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            var pts = new Coordinate[_nPts + 1];
            int iPt = 0;
            for (int i = 0; i < _nPts; i++)
            {
                double ang = i * (2 * Math.PI / _nPts);
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = pts[0].Copy();

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring);
            return (Polygon) Rotate(poly);
        }

        /// <summary>
        /// Creates a squircular <see cref="Polygon"/>.
        /// </summary>
        /// <returns>a squircle</returns>
        public Polygon CreateSquircle()
        {
            return CreateSupercircle(4);
        }

        /// <summary>
        /// Creates a supercircular <see cref="Polygon"/>
        /// of a given positive power.
        /// </summary>
        /// <returns>a supercircle</returns>
        public Polygon CreateSupercircle(double power)
        {
            double recipPow = 1.0 / power;

            double radius = _dim.MinSize / 2;
            var centre = _dim.Centre;

            double r4 = Math.Pow(radius, power);
            double y0 = radius;

            double xyInt = Math.Pow(r4 / 2, recipPow);

            int nSegsInOct = _nPts / 8;
            int totPts = nSegsInOct * 8 + 1;
            var pts = new Coordinate[totPts];
            double xInc = xyInt / nSegsInOct;

            for (int i = 0; i <= nSegsInOct; i++)
            {
                double x = 0.0;
                double y = y0;
                if (i != 0)
                {
                    x = xInc * i;
                    double x4 = Math.Pow(x, power);
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
            pts[pts.Length - 1] = pts[0].Copy();

            var ring = GeomFact.CreateLinearRing(pts);
            var poly = GeomFact.CreatePolygon(ring);
            return (Polygon) Rotate(poly);
        }

        /// <summary>
        /// Creates a elliptical arc, as a LineString.
        /// </summary><remarks>
        /// The arc is always created in a counter-clockwise direction.
        /// </remarks>
        /// <param name="startAng">Start angle in radians</param>
        /// <param name="angExtent">Size of angle in radians</param>
        /// <returns></returns>
        public LineString CreateArc(double startAng, double angExtent)
        {
            var env = _dim.Envelope;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            double angSize = angExtent;
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            double angInc = angSize / (_nPts - 1);

            var pts = new Coordinate[_nPts];
            int iPt = 0;
            for (int i = 0; i < _nPts; i++)
            {
                double ang = startAng + i * angInc;
                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                var pt = CreateCoord(x, y);
                pts[iPt++] = pt;
            }
            var line = GeomFact.CreateLineString(pts);
            return (LineString) Rotate(line);
        }

        /// <summary>
        /// Creates an elliptical arc polygon.
        /// </summary>
        /// <remarks>
        /// The polygon is formed from the specified arc of an ellipse
        /// and the two radii connecting the endpoints to the centre of the ellipse.
        /// </remarks>
        /// <param name="startAng">Start angle in radians</param>
        /// <param name="angExtent">Size of angle in radians</param>
        /// <returns>An elliptical arc polygon</returns>
        public Polygon CreateArcPolygon(double startAng, double angExtent)
        {
            var env = _dim.Envelope;
            double xRadius = env.Width / 2.0;
            double yRadius = env.Height / 2.0;

            double centreX = env.MinX + xRadius;
            double centreY = env.MinY + yRadius;

            double angSize = angExtent;
            if (angSize <= 0.0 || angSize > 2 * Math.PI)
                angSize = 2 * Math.PI;
            double angInc = angSize / (_nPts - 1);
            // var check = angInc * nPts;
            // var checkEndAng = startAng + check;

            var pts = new Coordinate[_nPts + 2];

            int iPt = 0;
            pts[iPt++] = CreateCoord(centreX, centreY);
            for (int i = 0; i < _nPts; i++)
            {
                double ang = startAng + angInc * i;

                double x = xRadius * Math.Cos(ang) + centreX;
                double y = yRadius * Math.Sin(ang) + centreY;
                pts[iPt++] = CreateCoord(x, y);
            }
            pts[iPt] = CreateCoord(centreX, centreY);
            var ring = GeomFact.CreateLinearRing(pts);
            var geom = GeomFact.CreatePolygon(ring);
            return (Polygon) Rotate(geom);
        }

        /// <summary>
        /// A dimension class for <see cref="GeometricShapeFactory"/>s
        /// </summary>
        protected class Dimensions
        {
            private Coordinate _base;

            /// <summary>
            /// Gets or sets a value indicating the base of the shapes to be created
            /// </summary>
            public Coordinate Base
            {
                get => _base;
                set => _base = value;
            }

            private Coordinate _centre;

            /// <summary>
            /// Gets or sets a value indicating the centre of the shapes to be created
            /// </summary>
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
                set => _centre = value;
            }

            private double _width;

            /// <summary>
            /// Gets or sets a value indicating the width of the <see cref="Envelope"/>.
            /// </summary>
           public double Width
            {
                get => _width;
               set => _width = value;
           }

            private double _height;

            /// <summary>
            /// Gets or sets a value indicating the height of the <see cref="Envelope"/>.
            /// </summary>
           public double Height
            {
                get => _height;
               set => _height = value;
           }

           /// <summary>
           /// Sets <see cref="Width"/> and <see cref="Height"/> to the same value
           /// </summary>
            public double Size
            {
                set
                {
                    Height = value;
                    Width = value;
                }
            }

            /// <summary>
            /// Gets a value indicating the minimum size of the shape's <see cref="Envelope"/>
            /// </summary>
            public double MinSize => Math.Min(Width, Height);

            /// <summary>
            /// Gets or sets a value indicating the bounds of the shape to be created
            /// </summary>
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
                    _centre = value.Centre.Copy();
                }
            }
        }
    }
}
