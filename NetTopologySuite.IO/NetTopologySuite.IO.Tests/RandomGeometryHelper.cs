using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.IO.Tests
{
    public class RandomGeometryHelper
    {
        public static readonly Random RND = new Random(9987462);

        public RandomGeometryHelper(IGeometryFactory factory)
        {
            Factory = factory;
            //_geometricShapeFactory = new SineStarFactory(factory);
            CreateCoordinate = () => new Coordinate();
            Ordinates = Ordinates.XY;
            MinX = -180;
            MaxX = 180;
            MinY = -90;
            MaxY = 90;
        }

        public Ordinates Ordinates { get; set; }

        public double MinX { get; set; }

        public double MaxX { get; set; }

        public double MinY { get; set; }

        public double MaxY { get; set; }

        protected CreateCoordinateDelegate CreateCoordinate { get; set; }

        /// <summary>
        /// Gets a random coordinate
        /// </summary>
        protected Coordinate RandomCoordinate
        {
            get
            {
                Coordinate c = CreateCoordinate();
                c.X = RandomOrdinate(Ordinate.X);
                c.Y = RandomOrdinate(Ordinate.Y);
                if ((Ordinates & Ordinates.Z) == Ordinates.Z)
                    c.Z = RandomOrdinate(Ordinate.Z);
                //if ((Ordinates & Ordinates.M) == Ordinates.M)
                //    c.M = RandomOrdinate(Ordinate.M);

                return c;
            }
        }

        protected Coordinate[] RandomCoordinates
        {
            get
            {
                var pts = new Coordinate[RND.Next(4, 15)];
                for (var i = 0; i < pts.Length; i++)
                {
                    pts[i] = RandomCoordinate;
                    _factory.PrecisionModel.MakePrecise(pts[i]);
                }

                return pts;
            }
        }

        private IGeometryFactory _factory;

        public int SRID
        {
            get { return Factory.SRID; }
            set
            {
                var pm = Factory.PrecisionModel;
                Factory = new GeometryFactory(pm, value);
            }
        }

        /// <summary>
        /// Gets the factory to create the random geometries
        /// </summary>
        public IGeometryFactory Factory
        {
            get { return _factory; }
            set
            {
                if (value != null)
                {
                    _factory = value;
                    _geometricShapeFactory = new SineStarFactory(_factory);
                }
            }
        }

        /// <summary>
        /// Gets a point with random ordinates
        /// </summary>
        public IPoint Point { get { return Factory.CreatePoint(RandomCoordinate); } }

        /// <summary>
        /// Gets a multipoint of a random number of points with random ordinates
        /// </summary>
        public IMultiPoint MultiPoint
        {
            get
            {
                return Factory.CreateMultiPoint(RandomCoordinates);
            }
        }

        public ILineString LineString
        {
            get { return Factory.CreateLineString(RandomCoordinates); }
        }

        public IMultiLineString MultiLineString
        {
            get
            {
                var lineStrings = new ILineString[RND.Next(5, 9)];
                for (var i = 0; i < lineStrings.Length; i++)
                    lineStrings[i] = LineString;
                return Factory.CreateMultiLineString(lineStrings);
            }
        }

        private SineStarFactory _geometricShapeFactory;

        public IPolygon Polygon
        {
            get
            {
                lock (_geometricShapeFactory)
                {
                    _geometricShapeFactory.Centre = RandomCoordinate;
                    _geometricShapeFactory.Height = 40 * RND.NextDouble();
                    _geometricShapeFactory.Width = 40 * RND.NextDouble();
                    switch (RND.Next(0, 6))
                    {
                        case 0:
                            return _geometricShapeFactory.CreateArcPolygon(2 * Math.PI * RND.NextDouble(),
                                                                           2 * Math.PI * RND.NextDouble());
                        default:
                            var poly = _geometricShapeFactory.CreateCircle();
                            var distance = -0.25 * Math.Min(_geometricShapeFactory.Height, _geometricShapeFactory.Width);
                            var buffer = (IPolygon)poly.Buffer(distance);
                            return _factory.CreatePolygon(poly.Shell, new[] { buffer.Shell });

                        case 2:
                            return _geometricShapeFactory.CreateRectangle();
                        case 3:
                            //return _geometricShapeFactory.CreateSineStar();
                            return _geometricShapeFactory.CreateSquircle();
                        case 4:
                            return _geometricShapeFactory.CreateSupercircle(RND.NextDouble() * 3);
                    }
                }
            }
        }

        public IMultiPolygon MultiPolygon
        {
            get
            {
                var polys = new IPolygon[RND.Next(3, 8)];
                for (var i = 0; i < polys.Length; i++)
                    polys[i] = Polygon;
                var mp = Factory.CreateMultiPolygon(polys);
                var mpUnion = mp.Union();
                var multiPolygon = mpUnion as IMultiPolygon;
                return multiPolygon ?? Factory.CreateMultiPolygon(new[] { (IPolygon)mpUnion} );
            }
        }

        public IGeometryCollection GeometryCollection
        {
            get
            {
                var polys = new IGeometry[RND.Next(3, 8)];
                for (var i = 0; i < polys.Length; i++)
                    polys[i] = Geometry;
                return Factory.CreateGeometryCollection(polys);
            }
        }

        public IGeometry Geometry
        {
            get
            {
                switch (RND.Next(0, 7))
                {
                    case 0:
                        return Point;
                    case 1:
                        return LineString;
                    case 2:
                        return Polygon;
                    case 3:
                        return MultiPoint;
                    case 4:
                        return MultiLineString;
                    case 5:
                        return MultiPolygon;
                    case 6:
                        return GeometryCollection;
                    default:
                        return Geometry;
                }
            }
        }

        /// <summary>
        /// Delegate function to create Coordinate instances. This is particulary handy for m-ordinate coordinate classes
        /// </summary>
        /// <returns></returns>
        protected delegate Coordinate CreateCoordinateDelegate();

        /// <summary>
        /// Function to create a random ordinates. For x-ordinates the values are limited to <see cref="MinX"/> and <see cref="MaxX"/>. For y-Ordinates the bounds are <see cref="MinY"/> and <see cref="MaxY"/>. z- and m-ordinates are limited to the range [0; 1000[.
        /// </summary>
        /// <param name="ordinate">The ordinate to obtain</param>
        /// <returns>A random value that should represent an ordinate value.</returns>
        private double RandomOrdinate(Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return MinX + RND.NextDouble() * (MaxX - MinX);
                case Ordinate.Y:
                    return MinY + RND.NextDouble() * (MaxY - MinY);
                default:
                    return RND.NextDouble() * 1000;
            }
        }
    }
}