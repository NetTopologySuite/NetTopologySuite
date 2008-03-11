using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Supplies a set of utility methods for building Geometry objects 
    /// from lists of Coordinates.
    /// </summary>            
    [Serializable]
    public class GeometryFactory<TCoordinate> : IGeometryFactory<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        #region Static precision models

        ///// <summary>
        ///// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        ///// <c> == </c> <see cref="PrecisionModelType.Floating" />.
        ///// </summary>
        //public static readonly IGeometryFactory<TCoordinate> Default = new GeometryFactory<TCoordinate>();

        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.Floating" />.
        /// </summary>
        public static IGeometryFactory<TCoordinate> CreateFloatingPrecision(
            ICoordinateSequenceFactory<TCoordinate> coordSequenceFactory)
        {
            return new GeometryFactory<TCoordinate>(coordSequenceFactory);
        }

        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.FloatingSingle" />.
        /// </summary>
        public static IGeometryFactory<TCoordinate> CreateFloatingSinglePrecision(
            ICoordinateSequenceFactory<TCoordinate> coordSequenceFactory)
        {
            return new GeometryFactory<TCoordinate>(
                new PrecisionModel<TCoordinate>(PrecisionModelType.FloatingSingle),
                null, coordSequenceFactory);
        }


        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.Fixed" />.
        /// </summary>
        public static IGeometryFactory<TCoordinate> CreateFixedPrecision(
            ICoordinateSequenceFactory<TCoordinate> coordSequenceFactory)
        {
            return new GeometryFactory<TCoordinate>(
                new PrecisionModel<TCoordinate>(PrecisionModelType.Fixed),
                null, coordSequenceFactory);
        }
        
        #endregion

        private readonly ICoordinateSequenceFactory<TCoordinate> _coordinateSequenceFactory;
        private readonly ICoordinateFactory<TCoordinate> _coordinateFactory;
        private readonly IPrecisionModel<TCoordinate> _precisionModel;
        private Int32? _srid;
        private ICoordinateSystem<TCoordinate> _spatialReference;

        //public static IPoint CreatePointFromInternalCoord(ICoordinate coord, IGeometry exemplar)
        //{
        //    exemplar.PrecisionModel.MakePrecise(coord);
        //    return exemplar.Factory.CreatePoint(coord);
        //}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// PrecisionModel, spatial-reference ID, and CoordinateSequence implementation.
        /// </summary>    
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel, Int32? srid,
                               ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory,
                               ICoordinateSystem<TCoordinate> spatialReference)
        {
            if (coordinateSequenceFactory == null) throw new ArgumentNullException("coordinateSequenceFactory");

            _precisionModel = precisionModel;
            _coordinateSequenceFactory = coordinateSequenceFactory;
            _coordinateFactory = coordinateSequenceFactory.CoordinateFactory;
            _srid = srid;
            _spatialReference = spatialReference;
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// PrecisionModel, spatial-reference ID, and CoordinateSequence implementation.
        /// </summary>    
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel, Int32? srid,
                               ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
            : this(precisionModel, srid, coordinateSequenceFactory, null) {}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// <see cref="PrecisionModel{TCoordinate}"/> and spatial-reference ID, and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        /// <param name="srid">The SRID to use.</param>
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel, Int32? srid)
            : this(precisionModel, srid, getDefaultCoordinateSequenceFactory<TCoordinate>()) {}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, the given 
        /// <see cref="IPrecisionModel{TCoordinate}"/> and a <see langword="null"/> 
        /// spatial-reference ID.
        /// </summary>
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel,
                               ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
            : this(precisionModel, null, coordinateSequenceFactory) {}

        /// <summary>
        /// 
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// <see cref="IPrecisionModel{TCoordinate}"/> and the default <see cref="ICoordinateSequenceFactory{TCoordinate}"/>
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">
        /// The <see cref="IPrecisionModel{TCoordinate}"/> to use.
        /// </param>
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel)
            : this(precisionModel, null, getDefaultCoordinateSequenceFactory<TCoordinate>()) {}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a Double-precision floating 
        /// <see cref="IPrecisionModel{TCoordinate}"/>, the given spatial-reference ID 
        /// and the given <see cref="ICoordinateSystem{TCoordinate}"/>.
        /// </summary>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory, Int32? srid,
                               ICoordinateSystem<TCoordinate> spatialReference)
            : this(new PrecisionModel<TCoordinate>(), srid, coordinateSequenceFactory, spatialReference) {}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a Double-precision floating 
        /// <see cref="IPrecisionModel{TCoordinate}"/>, and the given spatial-reference ID.
        /// </summary>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory, Int32? srid)
            : this(coordinateSequenceFactory, srid, null) {}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a Double-precision floating 
        /// <see cref="IPrecisionModel{TCoordinate}"/>, a
        /// <see langword="null"/> spatial-reference ID and the given 
        /// <see cref="ICoordinateSystem{TCoordinate}"/>.
        /// </summary>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory,
                               ICoordinateSystem<TCoordinate> spatialReference)
            : this(coordinateSequenceFactory, null, spatialReference) {}

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a Double-precision floating 
        /// <see cref="IPrecisionModel{TCoordinate}"/>, and a
        /// <see langword="null"/> spatial-reference ID.
        /// </summary>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
            : this(coordinateSequenceFactory, null, null) {}

        /// <summary>  
        /// Build an appropriate <see cref="Geometry{TCoordinate}"/>, <c>MultiGeometry</c>, or
        /// <see cref="GeometryCollection{TCoordinate}" /> to contain the <see cref="Geometry{TCoordinate}"/>s in
        /// it.
        /// <example>
        ///  If <c>geomList</c> contains a single <see cref="Polygon{TCoordinate}" />,
        /// the <see cref="Polygon{TCoordinate}" /> is returned.
        ///  If <c>geomList</c> contains several <see cref="Polygon{TCoordinate}" />s, a
        /// <c>MultiPolygon</c> is returned.
        ///  If <c>geomList</c> contains some <see cref="Polygon{TCoordinate}" />s and
        /// some <c>LineString</c>s, a <see cref="GeometryCollection{TCoordinate}" /> is
        /// returned.
        ///  If <c>geomList</c> is empty, an empty <see cref="GeometryCollection{TCoordinate}" />
        /// is returned.
        /// Note that this method does not "flatten" Geometries in the input, and hence if
        /// any MultiGeometries are contained in the input a GeometryCollection containing
        /// them will be returned.
        /// </example>
        /// </summary>
        /// <param name="geometries">The <see cref="Geometry{TCoordinate}"/> to combine.</param>
        /// <returns>
        /// A <see cref="Geometry{TCoordinate}"/> of the "smallest", "most type-specific" 
        /// class that can contain the elements of <c>geomList</c>.
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
            // this should always return a point, since otherwise an empty collection would have already been returned
            IGeometry<TCoordinate> geom0 = Slice.GetFirst(geometries);
            Boolean isCollection = Slice.CountGreaterThan(geometries, 1);

            if (isCollection)
            {
                if (geom0 is IPolygon)
                {
                    IEnumerable<IPolygon<TCoordinate>> polygons =
                        Enumerable.Downcast<IPolygon<TCoordinate>, IGeometry<TCoordinate>>(geometries);

                    return CreateMultiPolygon(polygons);
                }
                else if (geom0 is ILineString)
                {
                    IEnumerable<ILineString<TCoordinate>> lines =
                        Enumerable.Downcast<ILineString<TCoordinate>, IGeometry<TCoordinate>>(geometries);

                    return CreateMultiLineString(lines);
                }
                else if (geom0 is IPoint)
                {
                    IEnumerable<IPoint<TCoordinate>> points =
                        Enumerable.Downcast<IPoint<TCoordinate>, IGeometry<TCoordinate>>(geometries);

                    return CreateMultiPoint(points);
                }

                Assert.ShouldNeverReachHere();
            }

            return geom0;
        }

        #region IGeometryFactory<TCoordinate> Members

        public IExtents<TCoordinate> CreateExtents()
        {
            return new Extents<TCoordinate>(this);
        }

        public IExtents<TCoordinate> CreateExtents(ICoordinate min, ICoordinate max)
        {
            return CreateExtents(CoordinateFactory.Create(min), CoordinateFactory.Create(max));
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
        /// If the <see cref="Extents{TCoordinate}"/> is a null 
        /// <see cref="Extents{TCoordinate}"/>, returns an
        /// empty <c>Point</c>. If the <see cref="Extents{TCoordinate}"/> 
        /// is a point, returns a non-empty <see cref="Point{TCoordinate}"/>. 
        /// If the <see cref="Extents{TCoordinate}"/> is a rectangle, returns a 
        /// <see cref="Polygon{TCoordinate}" /> whose points are (minx, miny),
        /// (maxx, miny), (maxx, maxy), (minx, maxy), (minx, miny).
        /// </summary>
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
        public IGeometry<TCoordinate> ToGeometry(IExtents<TCoordinate> envelope)
        {
            if (envelope.IsEmpty)
            {
                return CreateEmpty();
            }

            Double xMin = envelope.GetMin(Ordinates.X);
            Double xMax = envelope.GetMax(Ordinates.X);
            Double yMin = envelope.GetMin(Ordinates.Y);
            Double yMax = envelope.GetMax(Ordinates.Y);

            ICoordinateFactory<TCoordinate> factory = CoordinateFactory;

            if (xMin == xMax && yMin == yMax)
            {
                return CreatePoint(factory.Create(xMin, yMin));
            }

            return CreatePolygon(
                CreateLinearRing(new TCoordinate[]
                                     {
                                         factory.Create(xMin, yMin),
                                         factory.Create(xMax, yMin),
                                         factory.Create(xMax, yMax),
                                         factory.Create(xMin, yMin),
                                     }),
                null);
        }

        public ICoordinateFactory<TCoordinate> CoordinateFactory
        {
            get { return _coordinateFactory; }
        }

        public ICoordinateSequenceFactory<TCoordinate> CoordinateSequenceFactory
        {
            get { return _coordinateSequenceFactory; }
        }

        private IGeometry<TCoordinate> CreateEmpty()
        {
            return new GeometryCollection<TCoordinate>(this);
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
                = new Point<TCoordinate>(Slice.GetFirst(coordinates), this);
            return point;
        }

        public IPoint<TCoordinate> CreatePoint()
        {
            return new Point<TCoordinate>(default(TCoordinate), this);
        }

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        public IPoint<TCoordinate> CreatePoint(ICoordinateSequence<TCoordinate> coordinates)
        {
            if (coordinates.Count == 0)
            {
                return new Point<TCoordinate>(default(TCoordinate), this);
            }

            return new Point<TCoordinate>(coordinates[0], this);
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
                                            Enumerable.Upcast<ILineString<TCoordinate>, 
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the given <c>LineStrings</c>; a null or empty
        /// array will create an empty MultiLineString.
        /// </summary>
        /// <param name="lineStrings">LineStrings, each of which may be empty but not null-</param>
        public IMultiLineString<TCoordinate> CreateMultiLineString(IEnumerable<ILineString<TCoordinate>> lineStrings)
        {
            return new MultiLineString<TCoordinate>(lineStrings, this);
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
            return new MultiPolygon<TCoordinate>(polygons, this);
        }

        public IMultiPolygon<TCoordinate> CreateMultiPolygon(ICoordinateSequence<TCoordinate> coordinates)
        {
            return new MultiPolygon<TCoordinate>(coordinates, this);
        }

        public IGeometryCollection<TCoordinate> CreateGeometryCollection()
        {
            return new GeometryCollection<TCoordinate>(this);
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="GeometryCollection{TCoordinate}" /> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">Geometries, each of which may be empty but not null.</param>
        public IGeometryCollection<TCoordinate> CreateGeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            return new GeometryCollection<TCoordinate>(geometries, this);
        }

        /// <returns>
        /// A clone of g based on a CoordinateSequence created by this
        /// GeometryFactory's CoordinateSequenceFactory.
        /// </returns>
        public IGeometry<TCoordinate> CreateGeometry(IGeometry<TCoordinate> g)
        {
            // could this be cached to make this more efficient? Or maybe it isn't enough overhead to bother
            GeometryEditor<TCoordinate> editor = new GeometryEditor<TCoordinate>(this);
            return editor.Edit(g, new NoOpCoordinateOperation());
        }

        /// <summary>
        /// Returns the PrecisionModel that Geometries created by this factory
        /// will be associated with.
        /// </summary>
        public IPrecisionModel<TCoordinate> PrecisionModel
        {
            get { return _precisionModel; }
        }

        public ICoordinateSystem<TCoordinate> SpatialReference
        {
            get { return _spatialReference; }
            set { _spatialReference = value; }
        }

        public Int32? Srid
        {
            get { return _srid; }
            set { _srid = value; }
        }

        private static ICoordinateSequenceFactory<TCoordinate> getDefaultCoordinateSequenceFactory<TCoordinate>()
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            return Coordinates<TCoordinate>.DefaultCoordinateSequenceFactory;
        }

        private class NoOpCoordinateOperation : GeometryEditor<TCoordinate>.CoordinateOperation
        {
            public override IEnumerable<TCoordinate> Edit(IEnumerable<TCoordinate> coordinates,
                                                          IGeometry<TCoordinate> geometry)
            {
                return coordinates;
            }
        }

        #region Explicit IGeometryFactory Members

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
            return (IPoint2D)CreatePoint();
        }

        IPoint2D IGeometryFactory.CreatePoint2D(Double x, Double y)
        {
            TCoordinate coord = _coordinateFactory.Create(x, y);
            return (IPoint2D)CreatePoint(coord);
        }

        IPoint2DM IGeometryFactory.CreatePoint2DM(Double x, Double y, Double m)
        {
            TCoordinate coord = _coordinateFactory.Create(x, y, m);
            return (IPoint2DM)CreatePoint(coord);
        }

        IPoint3D IGeometryFactory.CreatePoint3D()
        {
            throw new NotImplementedException();
        }

        IPoint3D IGeometryFactory.CreatePoint3D(Double x, Double y, Double z)
        {
            throw new NotImplementedException();
        }

        IPoint3D IGeometryFactory.CreatePoint3D(IPoint2D point2D, Double z)
        {
            throw new NotImplementedException();
        }

        IPoint3DM IGeometryFactory.CreatePoint3DM(Double x, Double y, Double z, Double m)
        {
            throw new NotImplementedException();
        }

        IExtents2D IGeometryFactory.CreateExtents2D(
            Double left, Double bottom, Double right, Double top)
        {
            TCoordinate min = _coordinateFactory.Create(left, bottom);
            TCoordinate max = _coordinateFactory.Create(right, top);
            return (IExtents2D)CreateExtents(min, max);
        }

        IExtents2D IGeometryFactory.CreateExtents2D(
            Pair<Double> min, Pair<Double> max)
        {
            return (this as IGeometryFactory).CreateExtents2D(
                min.First, min.Second, max.First, max.Second);
        }

        IExtents3D IGeometryFactory.CreateExtents3D(
            Double left, Double bottom, Double front, Double right, Double top, Double back)
        {
            throw new NotImplementedException();
        }

        IExtents3D IGeometryFactory.CreateExtents3D(
            Triple<Double> lowerLeft, Triple<Double> upperRight)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        IExtents IGeometryFactory.CreateExtents(ICoordinate min, ICoordinate max)
        {
            return new Extents<TCoordinate>(this, 
                CoordinateFactory.Create(min), CoordinateFactory.Create(max));
        }

        IGeometry IGeometryFactory.CreateGeometry(IGeometry g)
        {
            throw new NotImplementedException();
        }

        IGeometry IGeometryFactory.CreateGeometry(ICoordinateSequence coordinates, OgcGeometryType type)
        {
            throw new NotImplementedException();
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
            return CreateLineString(
                Enumerable.Downcast<TCoordinate, ICoordinate>(coordinates));
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
            return CreateLinearRing(
                Enumerable.Downcast<TCoordinate, ICoordinate>(coordinates));
        }

        ILinearRing IGeometryFactory.CreateLinearRing(ICoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        IPolygon IGeometryFactory.CreatePolygon()
        {
            throw new NotImplementedException();
        }

        IPolygon IGeometryFactory.CreatePolygon(IEnumerable<ICoordinate> shell)
        {
            throw new NotImplementedException();
        }

        IPolygon IGeometryFactory.CreatePolygon(ILinearRing shell)
        {
            throw new NotImplementedException();
        }

        IPolygon IGeometryFactory.CreatePolygon(ILinearRing shell, IEnumerable<ILinearRing> holes)
        {
            throw new NotImplementedException();
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint()
        {
            throw new NotImplementedException();
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint(IEnumerable<ICoordinate> coordinates)
        {
            throw new NotImplementedException();
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint(IEnumerable<IPoint> point)
        {
            throw new NotImplementedException();
        }

        IMultiPoint IGeometryFactory.CreateMultiPoint(ICoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        IMultiLineString IGeometryFactory.CreateMultiLineString()
        {
            return new MultiLineString<TCoordinate>(null, this);
        }

        IMultiLineString IGeometryFactory.CreateMultiLineString(IEnumerable<ILineString> lineStrings)
        {
            throw new NotImplementedException();
        }

        IMultiPolygon IGeometryFactory.CreateMultiPolygon()
        {
            throw new NotImplementedException();
        }

        IMultiPolygon IGeometryFactory.CreateMultiPolygon(IEnumerable<IPolygon> polygons)
        {
            throw new NotImplementedException();
        }

        IGeometryCollection IGeometryFactory.CreateGeometryCollection()
        {
            throw new NotImplementedException();
        }

        IGeometryCollection IGeometryFactory.CreateGeometryCollection(IEnumerable<IGeometry> geometries)
        {
            throw new NotImplementedException();
        }

        IGeometry IGeometryFactory.ToGeometry(IExtents envelopeInternal)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGeometryFactory Members


        public IPolygon CreatePolygon(ICoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        public IMultiPolygon CreateMultiPolygon(ICoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        #endregion


        private ICoordinateSequence<TCoordinate> convertSequence(ICoordinateSequence coordinates)
        {
            ICoordinateSequence<TCoordinate> converted = coordinates as ICoordinateSequence<TCoordinate>;

            if (converted != null)
            {
                return converted;
            }

            converted = _coordinateSequenceFactory.Create();

            foreach (ICoordinate coordinate in coordinates)
            {
                converted.Add(coordinate);
            }

            return converted;
        }
    }
}