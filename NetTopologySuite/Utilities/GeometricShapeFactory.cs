using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
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
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly Dimensions _dim;
        private Int32 _pointCount = 100;

        ///// <summary>
        ///// Create a shape factory which will create shapes using the default GeometryFactory.
        ///// </summary>
        //public GeometricShapeFactory() : this(new GeometryFactory<TCoordinate>()) { }

        /// <summary>
        /// Create a shape factory which will create shapes using the given GeometryFactory.
        /// </summary>
        /// <param name="geoFactory">The factory to use.</param>
        public GeometricShapeFactory(IGeometryFactory<TCoordinate> geoFactory)
        {
            _geoFactory = geoFactory;
            _dim = new Dimensions(geoFactory);
        }

        /// <summary>
        /// Gets or sets the location of the shape by specifying the base coordinate
        /// (which in most cases is the lower left point of the envelope containing the shape).
        /// </summary>
        public TCoordinate Base
        {
            get { return _dim.Base; }
            set { _dim.Base = value; }
        }

        /// <summary>
        /// Gets or sets the location of the shape by specifying the center of
        /// the shape's bounding box.
        /// </summary>
        public TCoordinate Center
        {
            get { return _dim.Center; }
            set { _dim.Center = value; }
        }

        /// <summary>
        /// Gets or sets the total number of points in the created Geometry.
        /// </summary>
        public Int32 PointCount
        {
            get { return _pointCount; }
            set { _pointCount = value; }
        }

        /// <summary>
        /// Gets/Sets the size of the extent of the shape in both x and y directions.        
        /// </summary>                
        public Double Size
        {
            get { return _dim.Size; }
            set { _dim.Size = value; }
        }

        /// <summary>
        /// Gets/Sets the width of the shape.
        /// </summary>
        public Double Width
        {
            get { return _dim.Width; }
            set { _dim.Width = value; }
        }

        /// <summary>
        /// Gets/Sets the height of the shape.
        /// </summary>
        public Double Height
        {
            get { return _dim.Height; }
            set { _dim.Height = value; }
        }

        /// <summary>
        /// Creates a rectangular <see cref="Polygon{TCoordinate}" />.
        /// </summary>
        /// <returns>A rectangular polygon.</returns>
        public IPolygon<TCoordinate> CreateRectangle()
        {
            Int32 i;
            Int32 ipt = 0;
            Int32 nSide = _pointCount / 4;

            if (nSide < 1)
            {
                nSide = 1;
            }

            Double xSegLen = _dim.Extents.Width / nSide;
            Double ySegLen = _dim.Extents.Height / nSide;

            TCoordinate[] pts = new TCoordinate[4 * nSide + 1];
            Extents<TCoordinate> extents = _dim.Extents;

            for (i = 0; i < nSide; i++)
            {
                Double x = extents.Min[Ordinates.X] + i * xSegLen;
                Double y = extents.Min[Ordinates.Y];
                pts[ipt++] = _geoFactory.CoordinateFactory.Create(x, y);
            }

            for (i = 0; i < nSide; i++)
            {
                Double x = extents.Max[Ordinates.X];
                Double y = extents.Min[Ordinates.Y] + i * ySegLen;
                pts[ipt++] = _geoFactory.CoordinateFactory.Create(x, y);
            }

            for (i = 0; i < nSide; i++)
            {
                Double x = extents.Max[Ordinates.X] - i * xSegLen;
                Double y = extents.Max[Ordinates.Y];
                pts[ipt++] = _geoFactory.CoordinateFactory.Create(x, y);
            }

            for (i = 0; i < nSide; i++)
            {
                Double x = extents.Min[Ordinates.X];
                Double y = extents.Max[Ordinates.Y] - i * ySegLen;
                pts[ipt++] = _geoFactory.CoordinateFactory.Create(x, y);
            }

            pts[ipt++] = _geoFactory.CoordinateFactory.Create(pts[0]);

            ILinearRing<TCoordinate> ring = _geoFactory.CreateLinearRing(pts);
            IPolygon<TCoordinate> poly = _geoFactory.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a circular <see cref="Polygon{TCoordinate}" />.
        /// </summary>
        /// <returns>A circular polygon.</returns>
        public IPolygon<TCoordinate> CreateCircle()
        {
            Extents<TCoordinate> extents = _dim.Extents;
            Double xRadius = extents.Width / 2.0;
            Double yRadius = extents.Height / 2.0;

            Double centerX = extents.Min[Ordinates.X] + xRadius;
            Double centerY = extents.Min[Ordinates.Y] + yRadius;

            TCoordinate[] pts = new TCoordinate[_pointCount + 1];
            Int32 iPt = 0;

            for (Int32 i = 0; i < _pointCount; i++)
            {
                Double ang = i * (2 * Math.PI / _pointCount);
                Double x = xRadius * Math.Cos(ang) + centerX;
                Double y = yRadius * Math.Sin(ang) + centerY;
                TCoordinate pt = _geoFactory.CoordinateFactory.Create(x, y);
                pts[iPt++] = pt;
            }

            pts[iPt] = pts[0];

            ILinearRing<TCoordinate> ring = _geoFactory.CreateLinearRing(pts);
            IPolygon<TCoordinate> poly = _geoFactory.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a elliptical arc, as a <see cref="ILineString{TCoordinate}"/>.
        /// </summary>
        public ILineString<TCoordinate> CreateArc(Double startAng, Double endAng)
        {
            Extents<TCoordinate> extents = _dim.Extents;

            Double xRadius = extents.Width / 2.0;
            Double yRadius = extents.Height / 2.0;

            Double centerX = extents.Min[Ordinates.X] + xRadius;
            Double centerY = extents.Min[Ordinates.Y] + yRadius;

            Double angSize = (endAng - startAng);

            if (angSize <= 0.0 || angSize > 2 * Math.PI)
            {
                angSize = 2 * Math.PI;
            }

            Double angInc = angSize / _pointCount;

            TCoordinate[] pts = new TCoordinate[_pointCount];
            Int32 iPt = 0;

            for (Int32 i = 0; i < _pointCount; i++)
            {
                Double ang = startAng + i * angInc;
                Double x = xRadius * Math.Cos(ang) + centerX;
                Double y = yRadius * Math.Sin(ang) + centerY;
                TCoordinate pt = _geoFactory.CoordinateFactory.Create(x, y);
                pt = _geoFactory.PrecisionModel.MakePrecise(pt);
                pts[iPt++] = pt;
            }

            ILineString<TCoordinate> line = _geoFactory.CreateLineString(pts);
            return line;
        }

        private class Dimensions
        {
            private TCoordinate _base;
            private TCoordinate _center;
            private Double _width;
            private Double _height;
            private readonly IGeometryFactory<TCoordinate> _geoFactory;

            public Dimensions(IGeometryFactory<TCoordinate> geoFactory)
            {
                _geoFactory = geoFactory;
            }

            public TCoordinate Base
            {
                get { return _base; }
                set { _base = value; }
            }

            public TCoordinate Center
            {
                get { return _center; }
                set { _center = value; }
            }

            public Double Width
            {
                get { return _width; }
                set { _width = value; }
            }

            public Double Height
            {
                get { return _height; }
                set { _height = value; }
            }

            public Double Size
            {
                get { return Math.Max(Width, Height); }
                set
                {
                    Height = value;
                    Width = value;
                }
            }

            public Extents<TCoordinate> Extents
            {
                get
                {
                    if (!Coordinates<TCoordinate>.IsEmpty(Base))
                    {
                        Double x = Base[Ordinates.X], y = Base[Ordinates.Y];
                        return new Extents<TCoordinate>(_geoFactory,
                            x, x + Width, y, y + Height);
                    }

                    if (!Coordinates<TCoordinate>.IsEmpty(Center))
                    {
                        Double x = Center[Ordinates.X], y = Center[Ordinates.Y];
                        return new Extents<TCoordinate>(
                            _geoFactory,
                            x - Width / 2, x + Width / 2,
                            y - Height / 2, y + Height / 2);
                    }

                    return new Extents<TCoordinate>(_geoFactory, 0, Width, 0, Height);
                }
            }
        }
    }
}