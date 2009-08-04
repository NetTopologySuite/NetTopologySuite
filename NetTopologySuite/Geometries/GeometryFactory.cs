using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

#if DOTNET35
using System.Linq;
#endif

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Supplies a set of utility methods for building Geometry objects 
    /// from lists of Coordinates.
    /// </summary>            
    [Serializable]
    public class GeometryFactory<TCoordinate> : IGeometryFactory<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        #region Static precision models

        ///// <summary>
        ///// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        ///// <c> == </c> <see cref="PrecisionModelType.Floating" />.
        ///// </summary>
        //public static readonly IGeometryFactory<TCoordinate> Default = new GeometryFactory<TCoordinate>();

        ///// <summary>
        ///// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        ///// <c> == </c> <see cref="PrecisionModelType.DoubleFloating" />.
        ///// </summary>
        //public static IGeometryFactory<TCoordinate> CreateFloatingPrecision(
        //    ICoordinateSequenceFactory<TCoordinate> coordSequenceFactory)
        //{
        //    return new GeometryFactory<TCoordinate>(coordSequenceFactory);
        //}

        ///// <summary>
        ///// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        ///// <c> == </c> <see cref="PrecisionModelType.SingleFloating" />.
        ///// </summary>
        //public static IGeometryFactory<TCoordinate> CreateFloatingSinglePrecision(
        //    ICoordinateSequenceFactory<TCoordinate> coordSequenceFactory)
        //{
        //    return new GeometryFactory<TCoordinate>(
        //        coordSequenceFactory.CoordinateFactory.CreatePrecisionModel(PrecisionModelType.SingleFloating),
        //        null, coordSequenceFactory);
        //}


        ///// <summary>
        ///// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        ///// <c> == </c> <see cref="PrecisionModelType.Fixed" />.
        ///// </summary>
        //public static IGeometryFactory<TCoordinate> CreateFixedPrecision(
        //    ICoordinateSequenceFactory<TCoordinate> coordSequenceFactory, Double scale)
        //{
        //    return new GeometryFactory<TCoordinate>(
        //        coordSequenceFactory.CoordinateFactory.CreatePrecisionModel(scale),
        //        null, coordSequenceFactory);
        //}

        #endregion

        #region Fields

        private readonly ICoordinateFactory<TCoordinate> _coordinateFactory;
        private readonly ICoordinateSequenceFactory<TCoordinate> _coordinateSequenceFactory;
        //private readonly IPrecisionModel<TCoordinate> _precisionModel;
        private ICoordinateSystem<TCoordinate> _spatialReference;
        private String _srid;
        private IWkbReader<TCoordinate> _wkbDecoder;
        private IWkbWriter<TCoordinate> _wkbEncoder;
        private IWktGeometryReader<TCoordinate> _wktDecoder;
        private IWktGeometryWriter<TCoordinate> _wktEncoder;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates 
        /// geometries having the given <see cref="ICoordinateSequenceFactory{TCoordinate}"/> 
        /// implementation and the given spatial-reference ID and the given 
        /// <see cref="ICoordinateSystem{TCoordinate}"/>.
        /// </summary>
        /// <param name="coordinateSequenceFactory">The coordinate factory to use.</param>
        /// <param name="srid">An id code for a given spatial reference.</param>
        /// <param name="spatialReference">The spatial reference system for the created geometries.</param>   
        protected GeometryFactory(String srid,
                                  ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory,
                                  ICoordinateSystem<TCoordinate> spatialReference)
        {
            if (coordinateSequenceFactory == null)
            {
                throw new ArgumentNullException("coordinateSequenceFactory");
            }

            //_precisionModel = precisionModel;
            _coordinateSequenceFactory = coordinateSequenceFactory;
            _coordinateFactory = coordinateSequenceFactory.CoordinateFactory;
            _srid = srid ?? (spatialReference == null ? null : spatialReference.AuthorityCode);
            _spatialReference = spatialReference;
            _wktEncoder = new WktWriter<TCoordinate>();
            _wktDecoder = new WktReader<TCoordinate>(this, null);
            _wkbEncoder = new WkbWriter<TCoordinate>();
            _wkbDecoder = new WkbReader<TCoordinate>(this);
        }

        /// <summary>
        /// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates 
        /// geometries having the given <see cref="ICoordinateSequenceFactory{TCoordinate}"/> 
        /// implementation and the given spatial-reference ID and a <see langword="null"/> 
        /// <see cref="ICoordinateSystem{TCoordinate}"/>.
        /// </summary>  
        /// <remarks>
        /// The <see cref="IPrecisionModel{TCoordinate}"/> for this factory is 
        /// gotten from <paramref name="coordinateSequenceFactory"/>.
        /// </remarks>
        public GeometryFactory(String srid,
                               ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
            : this(srid, coordinateSequenceFactory, null)
        {
        }

        ///// <summary>
        ///// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates geometries having the given
        ///// <see cref="IPrecisionModel{TCoordinate}"/> and spatial-reference ID, 
        ///// and the default CoordinateSequence implementation.
        ///// </summary>
        ///// <param name="precisionModel">The PrecisionModel to use.</param>
        ///// <param name="srid">The SRID to use.</param>
        //public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel, Int32? srid)
        //    : this(precisionModel, srid, null) { }

        ///// <summary>
        ///// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates geometries having the given
        ///// <see cref="IPrecisionModel{TCoordinate}"/> and the default 
        ///// <see cref="ICoordinateSequenceFactory{TCoordinate}"/> implementation.
        ///// </summary>
        ///// <param name="precisionModel">
        ///// The <see cref="IPrecisionModel{TCoordinate}"/> to use.
        ///// </param>
        //public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel)
        //    : this(precisionModel, null, null) { }

        //public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory,
        //                       Int32? srid,
        //                       ICoordinateSystem<TCoordinate> spatialReference)
        //    : this(srid,
        //           coordinateSequenceFactory,
        //           spatialReference) { }

        ///// <summary>
        ///// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates geometries having the given
        ///// <see cref="ICoordinateSequenceFactory{TCoordinate}"/> implementation, 
        ///// a double-precision floating <see cref="IPrecisionModel{TCoordinate}"/>, 
        ///// and the given spatial-reference ID.
        ///// </summary>
        //public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory,
        //                       Int32? srid)
        //    : this(coordinateSequenceFactory, srid, null) { }

        /// <summary>
        /// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates 
        /// geometries having the given <see cref="ICoordinateSequenceFactory{TCoordinate}"/> 
        /// implementation and a <see langword="null"/> spatial-reference ID and the given 
        /// <see cref="ICoordinateSystem{TCoordinate}"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="IPrecisionModel{TCoordinate}"/> for this factory is 
        /// gotten from <paramref name="coordinateSequenceFactory"/>.
        /// </remarks>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory,
                               ICoordinateSystem<TCoordinate> spatialReference)
            : this(null, coordinateSequenceFactory, spatialReference)
        {
        }

        /// <summary>
        /// Constructs a <see cref="GeometryFactory{TCoordinate}"/> that generates 
        /// geometries having the given CoordinateSequence implementation, and a
        /// <see langword="null"/> spatial-reference ID and a <see langword="null"/> 
        /// <see cref="ICoordinateSystem{TCoordinate}"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="IPrecisionModel{TCoordinate}"/> for this factory is 
        /// gotten from <paramref name="coordinateSequenceFactory"/>.
        /// </remarks>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
            : this(null, coordinateSequenceFactory, null)
        {
        }

        #endregion

        #region IGeometryFactory<TCoordinate> Members

        /// <summary>  
        /// Build an appropriate <see cref="Geometry{TCoordinate}"/>, multi-geometry, or
        /// <see cref="GeometryCollection{TCoordinate}" /> to contain the 
        /// <see cref="Geometry{TCoordinate}"/>s in it.
        /// <example>
        /// If <paramref name="geometries"/> contains a single <see cref="Polygon{TCoordinate}" />,
        /// the <see cref="Polygon{TCoordinate}" /> is returned.
        /// If <paramref name="geometries"/> contains several <see cref="Polygon{TCoordinate}" />s, a
        /// <see cref="IMultiPolygon{TCoordinate}"/> is returned.
        /// If <paramref name="geometries"/> contains some <see cref="IPolygon{TCoordinate}" />s and
        /// some <see cref="ILineString{TCoordinate}"/>s, a 
        /// <see cref="GeometryCollection{TCoordinate}" /> is returned.
        /// If <paramref name="geometries"/> is empty, an empty 
        /// <see cref="GeometryCollection{TCoordinate}" /> is returned.
        /// Note that this method does not "flatten" geometries in the input, and hence if
        /// any multi-geometries are contained in the input an 
        /// <see cref="IGeometryCollection{TCoordinate}"/> containing them will be returned.
        /// </example>
        /// </summary>
        /// <param name="geometries">The <see cref="Geometry{TCoordinate}"/> to combine.</param>
        /// <returns>
        /// A <see cref="Geometry{TCoordinate}"/> of the "smallest", "most type-specific" 
        /// class that can contain the elements of <paramref name="geometries"/>.
        /// </returns>
        public IGeometry<TCoordinate> BuildGeometry(IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            Type geometryType = null;
            Boolean isHeterogeneous = false;

            foreach (IGeometry<TCoordinate> g in geometries)
            {
                Type partClass = g.GetType();

                if (geometryType == null)
                {
                    geometryType = partClass;
                }

                if (partClass != geometryType)
                {
                    isHeterogeneous = true;
                }
            }

            // for the empty point, return an empty GeometryCollection
            if (geometryType == null)
            {
                return CreateGeometryCollection(null);
            }

            if (isHeterogeneous)
            {
                return CreateGeometryCollection(geometries);
            }

            // at this point we know the collection is hetereogenous.
            // Determine the type of the result from the first Geometry in the list
            // this should always return a point, since otherwise an empty collection 
            // would have already been returned
            IGeometry<TCoordinate> geom0 = Slice.GetFirst(geometries);
            Boolean isCollection = Slice.CountGreaterThan(geometries, 1);

            if (isCollection)
            {
                if (geom0 is IPolygon)
                {
                    IEnumerable<IPolygon<TCoordinate>> polygons =
                        Caster.Downcast<IPolygon<TCoordinate>, IGeometry<TCoordinate>>(geometries);

                    return CreateMultiPolygon(polygons);
                }

                if (geom0 is ILineString)
                {
                    IEnumerable<ILineString<TCoordinate>> lines =
                        Caster.Downcast<ILineString<TCoordinate>, IGeometry<TCoordinate>>(geometries);

                    return CreateMultiLineString(lines);
                }

                if (geom0 is IPoint)
                {
                    IEnumerable<IPoint<TCoordinate>> points =
                        Caster.Downcast<IPoint<TCoordinate>, IGeometry<TCoordinate>>(geometries);

                    return CreateMultiPoint(points);
                }

                Assert.ShouldNeverReachHere();
            }

            return geom0;
        }

        public IGeometryFactory Clone()
        {
            return new GeometryFactory<TCoordinate>(Srid, CoordinateSequenceFactory, SpatialReference);
        }

        public IExtents<TCoordinate> CreateExtents()
        {
            return new Extents<TCoordinate>(this);
        }

        public IExtents<TCoordinate> CreateExtents(ICoordinate min, ICoordinate max)
        {
            if (min == null || min.IsEmpty)
            {
                min = max;
            }

            if (min == null || min.IsEmpty)
            {
                return CreateExtents();
            }

            if (max == null || max.IsEmpty)
            {
                max = min;
            }

            return CreateExtents(CoordinateFactory.Create(min),
                                 CoordinateFactory.Create(max));
        }

        public IExtents<TCoordinate> CreateExtents(TCoordinate min, TCoordinate max)
        {
            return new Extents<TCoordinate>(this, min, max);
        }

        public IExtents<TCoordinate> CreateExtents(IExtents extents)
        {
            return new Extents<TCoordinate>(this,
                                            CoordinateFactory.Create(extents.Min),
                                            CoordinateFactory.Create(extents.Max));
        }

        public IExtents<TCoordinate> CreateExtents(IExtents<TCoordinate> extents)
        {
            return new Extents<TCoordinate>(this, extents);
        }

        /// <summary>
        /// Converts the <see cref="IExtents{TCoordinate}"/> instance
        /// into an <see cref="IGeometry{TCoordinate}"/> with the same 
        /// coordinates.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Extents{TCoordinate}"/> is a null 
        /// <see cref="Extents{TCoordinate}"/>, returns an
        /// empty <see cref="IPoint{TCoordinate}"/>. 
        /// If the <see cref="Extents{TCoordinate}"/> 
        /// is a point, returns a non-empty <see cref="Point{TCoordinate}"/>. 
        /// If the <see cref="Extents{TCoordinate}"/> is a rectangle, returns a 
        /// <see cref="Polygon{TCoordinate}" /> whose points are (minx, miny),
        /// (maxx, miny), (maxx, maxy), (minx, maxy), (minx, miny).
        /// </remarks>
        /// <param name="envelope">
        /// The <see cref="Extents{TCoordinate}"/> to convert to a 
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </param>       
        /// <returns>
        /// An empty <see cref="Point{TCoordinate}"/> 
        /// (for null <see cref="Extents{TCoordinate}"/>s), 
        /// a <see cref="Point{TCoordinate}"/> 
        /// (when min x = max x and min y = max y) or a
        /// <see cref="Polygon{TCoordinate}" /> (in all other cases).
        /// </returns>
        /// <exception cref="TopologyException">
        /// If <c>coordinates</c> is not a closed linestring, 
        /// that is, if the first and last coordinates are not equal.
        /// </exception>
        public IGeometry<TCoordinate> ToGeometry(IExtents<TCoordinate> extents)
        {
            if (extents.IsEmpty)
            {
                return CreateEmpty();
            }

            Double xMin = extents.GetMin(Ordinates.X);
            Double xMax = extents.GetMax(Ordinates.X);
            Double yMin = extents.GetMin(Ordinates.Y);
            Double yMax = extents.GetMax(Ordinates.Y);

            ICoordinateFactory<TCoordinate> coordFactory = CoordinateFactory;

            if (xMin == xMax && yMin == yMax)
            {
                return CreatePoint(coordFactory.Create(xMin, yMin));
            }

            ILinearRing<TCoordinate> shell =
                CreateLinearRing(new[]
                                     {
                                         coordFactory.Create(xMin, yMin),
                                         coordFactory.Create(xMax, yMin),
                                         coordFactory.Create(xMax, yMax),
                                         coordFactory.Create(xMin, yMin),
                                     });

            return CreatePolygon(shell);
        }

        public ICoordinateFactory<TCoordinate> CoordinateFactory
        {
            get { return _coordinateFactory; }
        }

        public ICoordinateSequenceFactory<TCoordinate> CoordinateSequenceFactory
        {
            get { return _coordinateSequenceFactory; }
        }

        /// <summary>
        /// Creates a Point using the given Coordinate; a null Coordinate will create
        /// an empty Geometry.
        /// </summary>
        public IPoint<TCoordinate> CreatePoint(TCoordinate coordinate)
        {
            return new Point<TCoordinate>(coordinate, this);
        }

        public IPoint<TCoordinate> CreatePoint(IEnumerable<TCoordinate> coordinates)
        {
            Point<TCoordinate> point
                = new Point<TCoordinate>(Enumerable.First(coordinates), this);
            return point;
        }

        public IGeometry<TCoordinate> CreateGeometry(ICoordinateSequence<TCoordinate> coordinates,
                                                     OgcGeometryType type)
        {
            IGeometryFactory<TCoordinate> f = this;

            switch (type)
            {
                case OgcGeometryType.Point:
                    return f.CreatePoint(coordinates);
                case OgcGeometryType.LineString:
                    return f.CreateLineString(coordinates);
                case OgcGeometryType.Polygon:
                    return f.CreatePolygon(coordinates);
                case OgcGeometryType.MultiPoint:
                    return f.CreateMultiPoint(coordinates);
                case OgcGeometryType.MultiLineString:
                    throw new NotImplementedException();
                case OgcGeometryType.MultiPolygon:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public IPoint<TCoordinate> CreatePoint()
        {
            return new Point<TCoordinate>(default(TCoordinate), this);
        }

        /// <summary>
        /// Creates an <see cref="IPoint{TCoordinate}"/> using the given 
        /// <see cref="ICoordinateSequence{TCoordinate}"/>; a null or empty
        /// coordinate sequence will create an empty point.
        /// </summary>
        /// <remarks>
        /// Uses the first coordinate in <paramref name="coordinates"/> to create the point.
        /// </remarks>
        public IPoint<TCoordinate> CreatePoint(ICoordinateSequence<TCoordinate> coordinates)
        {
            return coordinates == null || coordinates.Count == 0
                       ? new Point<TCoordinate>(default(TCoordinate), this)
                       : new Point<TCoordinate>(coordinates[0], this);
        }

        public ILineString<TCoordinate> CreateLineString(params TCoordinate[] coordinates)
        {
            return CreateLineString(CoordinateSequenceFactory.Create(coordinates));
        }

        /// <summary> 
        /// Creates a LineString using the given Coordinates; a null or empty array will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        public ILineString<TCoordinate> CreateLineString(IEnumerable<TCoordinate> coordinates)
        {
            return CreateLineString(CoordinateSequenceFactory.Create(coordinates));
        }

        /// <summary>
        /// Creates a LineString using the given CoordinateSequence; a null or empty CoordinateSequence will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        public ILineString<TCoordinate> CreateLineString(ICoordinateSequence<TCoordinate> coordinates)
        {
            return new LineString<TCoordinate>(coordinates, this);
        }

        /// <summary>
        /// Creates a <see cref="LinearRing{TCoordinate}" /> using the given 
        /// <see cref="IEnumerable{TCoordinate}"/>; a <see langword="null"/> 
        /// or empty array will create an empty <see cref="LinearRing{TCoordinate}"/>. 
        /// The points must form a closed and simple linestring. 
        /// Consecutive points must not be equal.
        /// </summary>
        public ILinearRing<TCoordinate> CreateLinearRing()
        {
            return CreateLinearRing(null);
        }

        /// <summary>
        /// Creates a <see cref="LinearRing{TCoordinate}" /> using the given 
        /// <see cref="IEnumerable{TCoordinate}"/>; a <see langword="null"/> 
        /// or empty array will create an empty <see cref="LinearRing{TCoordinate}"/>. 
        /// The points must form a closed and simple linestring. 
        /// Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">
        /// An <see cref="IEnumerable{TCoordinate}"/> without null elements, 
        /// or an empty array, or <see langword="null"/>.
        /// </param>
        public ILinearRing<TCoordinate> CreateLinearRing(IEnumerable<TCoordinate> coordinates)
        {
            return CreateLinearRing(CoordinateSequenceFactory.Create(coordinates));
        }

        /// <summary> 
        /// Creates a <see cref="LinearRing{TCoordinate}" /> using the given 
        /// <see cref="ICoordinateSequence{TCoordinate}"/>; a null or empty 
        /// <see cref="ICoordinateSequence{TCoordinate}"/> will
        /// create an empty <see cref="LinearRing{TCoordinate}" />. 
        /// The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">
        /// An <see cref="ICoordinateSequence{TCoordinate}"/> possibly empty, 
        /// or <see langword="null"/>.
        /// </param>
        public ILinearRing<TCoordinate> CreateLinearRing(ICoordinateSequence<TCoordinate> coordinates)
        {
            return new LinearRing<TCoordinate>(coordinates, this);
        }

        public IPolygon<TCoordinate> CreatePolygon()
        {
            return new Polygon<TCoordinate>((ICoordinateSequence<TCoordinate>)null, this);
        }

        public IPolygon<TCoordinate> CreatePolygon(ILinearRing<TCoordinate> shell)
        {
            return new Polygon<TCoordinate>(shell, this);
        }

        /// <summary> 
        /// Constructs a <see cref="Polygon{TCoordinate}" /> with the given exterior boundary and
        /// interior boundaries.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <see cref="Polygon{TCoordinate}" />, or
        /// <see langword="null" /> or an empty <see cref="LinearRing{TCoordinate}" /> if
        /// the empty point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <see cref="Polygon{TCoordinate}" />, or
        /// <see langword="null" /> or empty <see cref="LinearRing{TCoordinate}" /> s if
        /// the empty point is to be created.        
        /// </param>
        public IPolygon<TCoordinate> CreatePolygon(ILinearRing<TCoordinate> shell,
                                                   IEnumerable<ILinearRing<TCoordinate>> holes)
        {
            return new Polygon<TCoordinate>(shell,
                                            Caster.Upcast<ILineString<TCoordinate>,
                                                ILinearRing<TCoordinate>>(holes),
                                            this);
        }

        public IPolygon<TCoordinate> CreatePolygon(ICoordinateSequence<TCoordinate> coordinates)
        {
            return new Polygon<TCoordinate>(coordinates, this);
        }

        /// <summary> 
        /// Creates an empty MultiPoint.
        /// </summary>
        public IMultiPoint<TCoordinate> CreateMultiPoint()
        {
            return new MultiPoint<TCoordinate>(this);
        }

        /// <summary> 
        /// Creates a MultiPoint using the given Points; a null or empty array will
        /// create an empty MultiPoint.
        /// </summary>
        /// <param name="point">An array without null elements, or an empty array, or null.</param>
        public IMultiPoint<TCoordinate> CreateMultiPoint(IEnumerable<IPoint<TCoordinate>> point)
        {
            return new MultiPoint<TCoordinate>(point, this);
        }

        public IMultiPoint<TCoordinate> CreateMultiPoint(params IPoint<TCoordinate>[] points)
        {
            return new MultiPoint<TCoordinate>(points, this);
        }

        /// <summary> 
        /// Creates a MultiPoint using the given Coordinates; a null or empty array will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        public IMultiPoint<TCoordinate> CreateMultiPoint(IEnumerable<TCoordinate> coordinates)
        {
            return CreateMultiPoint(CoordinateSequenceFactory.Create(coordinates));
        }

        /// <summary> 
        /// Creates a MultiPoint using the given CoordinateSequence; a null or empty CoordinateSequence will
        /// create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        public IMultiPoint<TCoordinate> CreateMultiPoint(ICoordinateSequence<TCoordinate> coordinates)
        {
            if (coordinates == null)
            {
                coordinates = CoordinateSequenceFactory.Create();
            }

            List<IPoint<TCoordinate>> points = new List<IPoint<TCoordinate>>();

            foreach (TCoordinate coordinate in coordinates)
            {
                points.Add(CreatePoint(coordinate));
            }

            return CreateMultiPoint(points);
        }

        public IMultiLineString<TCoordinate> CreateMultiLineString()
        {
            return CreateMultiLineString(null);
        }

        /// <summary>
        /// Creates a <see cref="IMultiLineString{TCoordinate}"/> using the 
        /// given <see cref="ILineString{TCoordinate}"/>s; a null or empty
        /// enumeration will create an empty <see cref="IMultiLineString{TCoordinate}"/>.
        /// </summary>
        /// <param name="lineStrings">
        /// <see cref="ILineString{TCoordinate}"/>s, each of which may be 
        /// empty but not null.
        /// </param>
        public IMultiLineString<TCoordinate> CreateMultiLineString(IEnumerable<ILineString<TCoordinate>> lineStrings)
        {
            MultiLineString<TCoordinate> mls = new MultiLineString<TCoordinate>(lineStrings, this);
            return mls;
        }

        public IMultiPolygon<TCoordinate> CreateMultiPolygon()
        {
            return new MultiPolygon<TCoordinate>(this);
        }

        /// <summary>
        /// Creates a <see cref="MultiPolygon{TCoordinate}"/> using the given 
        /// <see cref="Polygon{TCoordinate}"/>s; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> 
        /// OpenGIS Simple Features Specification for SQL.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        public IMultiPolygon<TCoordinate> CreateMultiPolygon(IEnumerable<IPolygon<TCoordinate>> polygons)
        {
            MultiPolygon<TCoordinate> mp = new MultiPolygon<TCoordinate>(polygons, this);
            return mp;
        }

        public IMultiPolygon<TCoordinate> CreateMultiPolygon(ICoordinateSequence<TCoordinate> coordinates)
        {
            MultiPolygon<TCoordinate> mp = new MultiPolygon<TCoordinate>(coordinates, this);
            return mp;
        }

        public IGeometryCollection<TCoordinate> CreateGeometryCollection()
        {
            GeometryCollection<TCoordinate> gc = new GeometryCollection<TCoordinate>(this);
            return gc;
        }

        /// <summary>
        /// Creates a <see cref="GeometryCollection{TCoordinate}" /> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">Geometries, each of which may be empty but not null.</param>
        public IGeometryCollection<TCoordinate> CreateGeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            GeometryCollection<TCoordinate> gc = new GeometryCollection<TCoordinate>(geometries, this);
            return gc;
        }

        public IGeometryCollection<TCoordinate> CreateGeometryCollection(IGeometry<TCoordinate> a,
                                                                         IGeometry<TCoordinate> b)
        {
            return new GeometryCollection<TCoordinate>(new[] { a, b }, this);
        }

        /// <returns>
        /// A clone of g based on a CoordinateSequence created by this
        /// GeometryFactory's CoordinateSequenceFactory.
        /// </returns>
        public IGeometry<TCoordinate> CreateGeometry(IGeometry<TCoordinate> g)
        {
            // could this be cached to make this more efficient? 
            // Or maybe it isn't enough overhead to bother
            GeometryEditor<TCoordinate> editor = new GeometryEditor<TCoordinate>(this);
            return editor.Edit(g, new NoOpCoordinateOperation());
        }

        /// <summary>
        /// Returns the <see cref="IPrecisionModel{TCoordinate}"/> that geometries 
        /// created by this factory will be associated with.
        /// </summary>
        public IPrecisionModel<TCoordinate> PrecisionModel
        {
            get { return _coordinateSequenceFactory.PrecisionModel; }
        }

        public ICoordinateSystem<TCoordinate> SpatialReference
        {
            get { return _spatialReference; }
            set { _spatialReference = value; }
        }

        public String Srid
        {
            get { return _srid ?? (_spatialReference == null ? null : _spatialReference.AuthorityCode); }
            set { _srid = value; }
        }

        public IWktGeometryWriter<TCoordinate> WktWriter
        {
            get { return _wktEncoder; }
            set { _wktEncoder = value; }
        }

        public IWktGeometryReader<TCoordinate> WktReader
        {
            get { return _wktDecoder; }
            set { _wktDecoder = value; }
        }

        public IWkbWriter<TCoordinate> WkbWriter
        {
            get { return _wkbEncoder; }
            set { _wkbEncoder = value; }
        }

        public IWkbReader<TCoordinate> WkbReader
        {
            get { return _wkbDecoder; }
            set { _wkbDecoder = value; }
        }

        IExtents IGeometryFactory.CreateExtents()
        {
            return CreateExtents();
        }

        IExtents IGeometryFactory.CreateExtents(IExtents first, IExtents second)
        {
            IExtents extents = CreateExtents(first.Min, first.Max);
            extents.ExpandToInclude(second);
            return extents;
        }

        IExtents IGeometryFactory.CreateExtents(IExtents first, IExtents second, IExtents third)
        {
            IExtents extents = (this as IGeometryFactory).CreateExtents(first, second);
            extents.ExpandToInclude(second);
            return extents;
        }

        IExtents IGeometryFactory.CreateExtents(params IExtents[] extents)
        {
            IExtents e = (this as IGeometryFactory).CreateExtents();

            foreach (IExtents extent in extents)
            {
                e.ExpandToInclude(extent);
            }

            return e;
        }

        IPoint2D IGeometryFactory.CreatePoint2D()
        {
            return createPointInternal(Double.NaN, Double.NaN, Double.NaN);
        }

        IPoint2D IGeometryFactory.CreatePoint2D(Double x, Double y)
        {
            return createPointInternal(x, y, Double.NaN);
        }

        IPoint2DM IGeometryFactory.CreatePoint2DM(Double x, Double y, Double m)
        {
            return createPointMInternal(x, y, Double.NaN, m);
        }

        IPoint3D IGeometryFactory.CreatePoint3D()
        {
            return createPointInternal(Double.NaN, Double.NaN, Double.NaN);
        }

        IPoint3D IGeometryFactory.CreatePoint3D(Double x, Double y, Double z)
        {
            return createPointInternal(x, y, z);
        }

        IPoint3D IGeometryFactory.CreatePoint3D(IPoint2D point2D, Double z)
        {
            return createPointInternal(point2D.X, point2D.Y, z);
        }

        IPoint3DM IGeometryFactory.CreatePoint3DM(Double x, Double y, Double z, Double m)
        {
            return createPointMInternal(x, y, z, m);
        }

        IExtents2D IGeometryFactory.CreateExtents2D(Double left, Double bottom,
                                                    Double right, Double top)
        {
            TCoordinate min = _coordinateFactory.Create(left, bottom);
            TCoordinate max = _coordinateFactory.Create(right, top);
            return (IExtents2D)CreateExtents(min, max);
        }

        IExtents2D IGeometryFactory.CreateExtents2D(Pair<Double> min, Pair<Double> max)
        {
            return (this as IGeometryFactory).CreateExtents2D(min.First,
                                                              min.Second,
                                                              max.First,
                                                              max.Second);
        }

        IExtents3D IGeometryFactory.CreateExtents3D(Double left, Double bottom,
                                                    Double front, Double right,
                                                    Double top, Double back)
        {
            return (IExtents3D)CreateExtents(_coordinateFactory.Create3D(left, bottom, front),
                                              _coordinateFactory.Create3D(right, top, back));
        }

        IExtents3D IGeometryFactory.CreateExtents3D(Triple<Double> lowerLeft, Triple<Double> upperRight)
        {
            return (IExtents3D)CreateExtents(_coordinateFactory.Create3D(lowerLeft.First,
                                                                          lowerLeft.Second,
                                                                          lowerLeft.Third),
                                              _coordinateFactory.Create3D(upperRight.First,
                                                                          upperRight.Second,
                                                                          upperRight.Third));
        }

        ICoordinateFactory IGeometryFactory.CoordinateFactory
        {
            get { return CoordinateFactory; }
        }

        ICoordinateSequenceFactory IGeometryFactory.CoordinateSequenceFactory
        {
            get { return CoordinateSequenceFactory; }
        }

        /// <summary>
        /// Gets or sets the spatial reference system to associate with the geometry.
        /// </summary>
        ICoordinateSystem IGeometryFactory.SpatialReference
        {
            get { return SpatialReference; }
            set { SpatialReference = value as ICoordinateSystem<TCoordinate>; }
        }

        IPrecisionModel IGeometryFactory.PrecisionModel
        {
            get { return PrecisionModel; }
        }

        IGeometry IGeometryFactory.BuildGeometry(IEnumerable<IGeometry> geometryList)
        {
            return BuildGeometry(Caster.Downcast<IGeometry<TCoordinate>, IGeometry>(geometryList));
        }

        IExtents IGeometryFactory.CreateExtents(ICoordinate min, ICoordinate max)
        {
            return new Extents<TCoordinate>(this,
                                            CoordinateFactory.Create(min),
                                            CoordinateFactory.Create(max));
        }

        IGeometry IGeometryFactory.CreateGeometry(IGeometry g)
        {
            return CreateGeometry((IGeometry<TCoordinate>)g);
        }

        IGeometry IGeometryFactory.CreateGeometry(ICoordinateSequence coordinates, OgcGeometryType type)
        {
            IGeometryFactory f = this;

            switch (type)
            {
                case OgcGeometryType.Point:
                    return f.CreatePoint(coordinates);
                case OgcGeometryType.LineString:
                    return f.CreateLineString(coordinates);
                case OgcGeometryType.Polygon:
                    return f.CreatePolygon(coordinates);
                case OgcGeometryType.MultiPoint:
                    return f.CreateMultiPoint(coordinates);
                case OgcGeometryType.MultiPolygon:
                    return f.CreateMultiPolygon(coordinates);
                default:
                    throw new NotImplementedException();
            }
        }

        IPoint IGeometryFactory.CreatePoint()
        {
            return CreatePoint();
        }

        IPoint IGeometryFactory.CreatePoint(ICoordinate coordinate)
        {
            return CreatePoint(_coordinateFactory.Create(coordinate));
        }

        IPoint IGeometryFactory.CreatePoint(ICoordinateSequence coordinates)
        {
            return CreatePoint(convertSequence(coordinates));
        }

        ILineString IGeometryFactory.CreateLineString()
        {
            return CreateLineString();
        }

        ILineString IGeometryFactory.CreateLineString(IEnumerable<ICoordinate> coordinates)
        {
            IEnumerable<TCoordinate> castCoords
                = Caster.Downcast<TCoordinate, ICoordinate>(coordinates);
            return CreateLineString(castCoords);
        }

        ILineString IGeometryFactory.CreateLineString(ICoordinateSequence coordinates)
        {
            return CreateLineString(convertSequence(coordinates));
        }

        ILinearRing IGeometryFactory.CreateLinearRing()
        {
            return CreateLinearRing();
        }

        ILinearRing IGeometryFactory.CreateLinearRing(IEnumerable<ICoordinate> coordinates)
        {
            IEnumerable<TCoordinate> castCoords
                = Caster.Downcast<TCoordinate, ICoordinate>(coordinates);
            return CreateLinearRing(castCoords);
        }

        ILinearRing IGeometryFactory.CreateLinearRing(ICoordinateSequence coordinates)
        {
            return CreateLinearRing((ICoordinateSequence<TCoordinate>)coordinates);
        }

        IPolygon IGeometryFactory.CreatePolygon()
        {
            return CreatePolygon();
        }

        IPolygon IGeometryFactory.CreatePolygon(IEnumerable<ICoordinate> shell)
        {
            return CreatePolygon(CreateLinearRing(Caster.Downcast<TCoordinate, ICoordinate>(shell)));
        }

        IPolygon IGeometryFactory.CreatePolygon(ILinearRing shell)
        {
            if (shell == null)
            {
                return CreatePolygon();
            }

            ILinearRing<TCoordinate> shellTyped = shell as ILinearRing<TCoordinate>;

            if (shellTyped == null)
            {
                shellTyped = CreateLinearRing(convertSequence(shell.Coordinates));
            }

            return CreatePolygon(shellTyped);
        }

        IPolygon IGeometryFactory.CreatePolygon(ILinearRing shell, IEnumerable<ILinearRing> holes)
        {
            if (shell == null)
            {
                return CreatePolygon();
            }

            ILinearRing<TCoordinate> shellTyped = shell as ILinearRing<TCoordinate>;

            if (shellTyped == null)
            {
                shellTyped = CreateLinearRing(convertSequence(shell.Coordinates));
            }

            List<ILinearRing<TCoordinate>> holesTyped = new List<ILinearRing<TCoordinate>>();

            foreach (ILinearRing hole in holes)
            {
                ILinearRing<TCoordinate> holeTyped = hole as ILinearRing<TCoordinate>;

                if (holeTyped == null)
                {
                    holeTyped = CreateLinearRing(convertSequence(hole.Coordinates));
                }

                holesTyped.Add(holeTyped);
            }

            return CreatePolygon(shellTyped, holesTyped);
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint()
        {
            return CreateMultiPoint();
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint(IEnumerable<ICoordinate> coordinates)
        {
            return CreateMultiPoint(Caster.Downcast<TCoordinate, ICoordinate>(coordinates));
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint(IEnumerable<IPoint> point)
        {
            return CreateMultiPoint(Caster.Downcast<IPoint<TCoordinate>, IPoint>(point));
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint(ICoordinateSequence coordinates)
        {
            return CreateMultiPoint((ICoordinateSequence<TCoordinate>)coordinates);
        }

        IMultiLineString IGeometryFactory.CreateMultiLineString()
        {
            return CreateMultiLineString(null);
        }

        IMultiLineString IGeometryFactory.CreateMultiLineString(IEnumerable<ILineString> lineStrings)
        {
            return CreateMultiLineString(convert(lineStrings));
        }

        IMultiPolygon IGeometryFactory.CreateMultiPolygon()
        {
            return CreateMultiPolygon();
        }

        IMultiPolygon IGeometryFactory.CreateMultiPolygon(IEnumerable<IPolygon> polygons)
        {
            return CreateMultiPolygon(Caster.Downcast<IPolygon<TCoordinate>, IPolygon>(polygons));
        }

        IGeometryCollection IGeometryFactory.CreateGeometryCollection()
        {
            return CreateGeometryCollection();
        }

        IGeometryCollection IGeometryFactory.CreateGeometryCollection(IGeometry a, IGeometry b)
        {
            IEnumerable<IGeometry<TCoordinate>> geometries =
                GenericInterfaceConverter<TCoordinate>.Convert(new[] { a, b }, this);
            return new GeometryCollection<TCoordinate>(geometries, this);
        }

        IGeometryCollection IGeometryFactory.CreateGeometryCollection(IEnumerable<IGeometry> geometries)
        {
            return CreateGeometryCollection(Caster.Downcast<IGeometry<TCoordinate>, IGeometry>(geometries));
        }

        IGeometry IGeometryFactory.ToGeometry(IExtents envelopeInternal)
        {
            return ToGeometry((IExtents<TCoordinate>)envelopeInternal);
        }

        IPolygon IGeometryFactory.CreatePolygon(ICoordinateSequence coordinates)
        {
            return CreatePolygon((ICoordinateSequence<TCoordinate>)coordinates);
        }

        IMultiPolygon IGeometryFactory.CreateMultiPolygon(ICoordinateSequence coordinates)
        {
            return CreateMultiPolygon((ICoordinateSequence<TCoordinate>)coordinates);
        }

        IWktGeometryWriter IGeometryFactory.WktWriter
        {
            get { return WktWriter; }
            set
            {
                throw new NotSupportedException(
                    "Use IGeometryFactory<TCoordinate>.WktWriter instead.");
            }
        }

        IWktGeometryReader IGeometryFactory.WktReader
        {
            get { return WktReader; }
            set
            {
                throw new NotSupportedException(
                    "Use IGeometryFactory<TCoordinate>.WktReader instead.");
            }
        }

        IWkbWriter IGeometryFactory.WkbWriter
        {
            get { return WkbWriter; }
            set
            {
                throw new NotSupportedException(
                    "Use IGeometryFactory<TCoordinate>.WkbWriter instead.");
            }
        }

        IWkbReader IGeometryFactory.WkbReader
        {
            get { return WkbReader; }
            set
            {
                throw new NotSupportedException(
                    "Use IGeometryFactory<TCoordinate>.WkbReader instead.");
            }
        }

        #endregion

        private IGeometry<TCoordinate> CreateEmpty()
        {
            return new GeometryCollection<TCoordinate>(this);
        }

        private Point<TCoordinate> createPointInternal(Double x, Double y, Double z)
        {
            if (Double.IsNaN(z))
            {
                TCoordinate coord = _coordinateFactory.Create(x, y);
                return new Point<TCoordinate>(coord, this);
            }
            else
            {
                TCoordinate coord = _coordinateFactory.Create3D(x, y, z);
                return new Point<TCoordinate>(coord, this);
            }
        }

        private PointM<TCoordinate> createPointMInternal(Double x, Double y, Double z, Double m)
        {
            if (Double.IsNaN(z))
            {
                TCoordinate coord = _coordinateFactory.Create(x, y);
                return new PointM<TCoordinate>(coord, m, this);
            }
            else
            {
                TCoordinate coord = _coordinateFactory.Create3D(x, y, z);
                return new PointM<TCoordinate>(coord, m, this);
            }
        }

        private ICoordinateSequence<TCoordinate> convertSequence(ICoordinateSequence coordinates)
        {
            return GenericInterfaceConverter<TCoordinate>.Convert(coordinates, _coordinateSequenceFactory);
        }

        private IEnumerable<ILineString<TCoordinate>> convert(IEnumerable<ILineString> lineStrings)
        {
            return GenericInterfaceConverter<TCoordinate>.Convert(lineStrings, this);
        }

        #region Nested type: NoOpCoordinateOperation

        private class NoOpCoordinateOperation : GeometryEditor<TCoordinate>.CoordinateOperation
        {
            public override IEnumerable<TCoordinate> Edit(IEnumerable<TCoordinate> coordinates,
                                                          IGeometry<TCoordinate> geometry)
            {
                return coordinates;
            }
        }

        #endregion

        //private static ICoordinateSequenceFactory<TCoordinate> getDefaultCoordinateSequenceFactory<TCoordinate>()
        //    where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
        //        IComputable<Double, TCoordinate>, IConvertible
        //{
        //    return Coordinates<TCoordinate>.DefaultCoordinateSequenceFactory;
        //}

        //public static IPoint CreatePointFromInternalCoord(ICoordinate coord, IGeometry exemplar)
        //{
        //    exemplar.PrecisionModel.MakePrecise(coord);
        //    return exemplar.Factory.CreatePoint(coord);
        //}

        #region IBoundsFactory<IExtents> Members

        IExtents GeoAPI.Indexing.IBoundsFactory<IExtents>.CreateNullBounds()
        {
            return CreateNullBounds();
        }

        public IExtents CreateMinimumSpanningBounds(IEnumerable<IExtents> bounds)
        {
            return CreateMinimumSpanningBounds(Caster.Cast<IExtents<TCoordinate>>(bounds));
        }

        #endregion

        #region IBoundsFactory<IExtents<TCoordinate>> Members

        public IExtents<TCoordinate> CreateNullBounds()
        {
            return CreateExtents();
        }

        public IExtents<TCoordinate> CreateMinimumSpanningBounds(IEnumerable<IExtents<TCoordinate>> bounds)
        {
            IExtents<TCoordinate> spanningBounds = CreateExtents();
            foreach(IExtents<TCoordinate> ext in bounds)
                spanningBounds.ExpandToInclude(ext);
            return spanningBounds;
        }

        #endregion
    }
}