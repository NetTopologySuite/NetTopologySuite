using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
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
            IComputable<TCoordinate>, IConvertible
    {
        #region Static precision models

        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.Floating" />.
        /// </summary>
        public static readonly IGeometryFactory<TCoordinate> Default = new GeometryFactory<TCoordinate>();

        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.Floating" />.
        /// </summary>
        /// <remarks>A shortcut for <see cref="GeometryFactory{TCoordinate}.Default" />.</remarks>
        public static readonly IGeometryFactory<TCoordinate> Floating = Default;

        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.FloatingSingle" />.
        /// </summary>
        public static readonly IGeometryFactory<TCoordinate> FloatingSingle =
            new GeometryFactory<TCoordinate>(new PrecisionModel<TCoordinate>(PrecisionModelType.FloatingSingle));

        /// <summary>
        /// A predefined <see cref="GeometryFactory{TCoordinate}" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModelType.Fixed" />.
        /// </summary>
        public static readonly IGeometryFactory<TCoordinate> Fixed =
            new GeometryFactory<TCoordinate>(new PrecisionModel<TCoordinate>(PrecisionModelType.Fixed));

        #endregion

        private readonly ICoordinateSequenceFactory<TCoordinate> _coordinateSequenceFactory;
        private readonly IPrecisionModel<TCoordinate> _precisionModel;
        private readonly Int32? _srid;

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
                               ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
        {
            _precisionModel = precisionModel;
            _coordinateSequenceFactory = coordinateSequenceFactory;
            _srid = srid;
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a Double-precision floating PrecisionModel and a
        /// spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory(ICoordinateSequenceFactory<TCoordinate> coordinateSequenceFactory)
            : this(new PrecisionModel<TCoordinate>(), 0, coordinateSequenceFactory) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// {PrecisionModel} and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel)
            : this(precisionModel, 0, getDefaultCoordinateSequenceFactory<TCoordinate>()) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// <see cref="PrecisionModel{TCoordinate}"/> and spatial-reference ID, and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        /// <param name="SRID">The SRID to use.</param>
        public GeometryFactory(IPrecisionModel<TCoordinate> precisionModel, Int32 SRID)
            : this(precisionModel, SRID, getDefaultCoordinateSequenceFactory<TCoordinate>()) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having a floating
        /// PrecisionModel and a spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory() : this(new PrecisionModel<TCoordinate>(), 0) { }

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

        public IExtents<TCoordinate> CreateExtents(ICoordinate min, ICoordinate max)
        {
            throw new NotImplementedException();
        }

        public IExtents<TCoordinate> CreateExtents(TCoordinate min, TCoordinate max)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the <see cref="Extents{TCoordinate}"/> is a null <see cref="Extents{TCoordinate}"/>, returns an
        /// empty <c>Point</c>. If the <see cref="Extents{TCoordinate}"/> is a point, returns
        /// a non-empty <c>Point</c>. If the <see cref="Extents{TCoordinate}"/> is a
        /// rectangle, returns a <see cref="Polygon{TCoordinate}" /> whose points are (minx, miny),
        /// (maxx, miny), (maxx, maxy), (minx, maxy), (minx, miny).
        /// </summary>
        /// <param name="envelope">The <see cref="Extents{TCoordinate}"/> to convert to a <see cref="Geometry{TCoordinate}"/>.</param>       
        /// <returns>
        /// An empty <c>Point</c> (for null <see cref="Extents{TCoordinate}"/>
        /// s), a <c>Point</c> (when min x = max x and min y = max y) or a
        /// <see cref="Polygon{TCoordinate}" /> (in all other cases)
        /// throws a <c>TopologyException</c> if <c>coordinates</c>
        /// is not a closed linestring, that is, if the first and last coordinates
        /// are not equal.
        /// </returns>
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
            get { throw new NotImplementedException(); }
        }

        public ICoordinateSequenceFactory<TCoordinate> CoordinateSequenceFactory
        {
            get { return _coordinateSequenceFactory; }
        }

        private IGeometry<TCoordinate> CreateEmpty()
        {
            return new GeometryCollection<TCoordinate>();
        }

        /// <summary>
        /// Creates a Point using the given Coordinate; a null Coordinate will create
        /// an empty Geometry.
        /// </summary>
        /// <param name="coordinate"></param>
        public IPoint<TCoordinate> CreatePoint(TCoordinate coordinate)
        {
            return new Point<TCoordinate>(coordinate);
        }

        public IPoint<TCoordinate> CreatePoint(IEnumerable<TCoordinate> coordinates)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        public IPoint<TCoordinate> CreatePoint(ICoordinateSequence<TCoordinate> coordinates)
        {
            if (coordinates.Count == 0)
            {
                return Point<TCoordinate>.Empty;
            }

            return new Point<TCoordinate>(coordinates[0], this);
        }

        public ILineString<TCoordinate> CreateLineString(params TCoordinate[] coordinates)
        {
            throw new NotImplementedException();
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
        /// Creates a <see cref="LinearRing{TCoordinate}" /> using the given <c>Coordinates</c>; a null or empty array will
        /// create an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        public ILinearRing<TCoordinate> CreateLinearRing(IEnumerable<TCoordinate> coordinates)
        {
            return CreateLinearRing(CoordinateSequenceFactory.Create(coordinates));
        }

        /// <summary> 
        /// Creates a <see cref="LinearRing{TCoordinate}" /> using the given <c>CoordinateSequence</c>; a null or empty CoordinateSequence will
        /// create an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        public ILinearRing<TCoordinate> CreateLinearRing(ICoordinateSequence<TCoordinate> coordinates)
        {
            return new LinearRing<TCoordinate>(coordinates, this);
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
                Enumerable.Upcast<ILineString<TCoordinate>, ILinearRing<TCoordinate>>(holes), 
                this);
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
        /// Creates a <c>MultiPolygon</c> using the given <c>Polygons</c>; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> 
        /// OpenGIS Simple Features Specification for SQL.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        public IMultiPolygon<TCoordinate> CreateMultiPolygon(IEnumerable<IPolygon<TCoordinate>> polygons)
        {
            return new MultiPolygon<TCoordinate>(polygons, this);
        }

        public IGeometryCollection<TCoordinate> CreateGeometryCollection()
        {
            throw new NotImplementedException();
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
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Int32? Srid
        {
            get { return _srid; }
        }

        private static ICoordinateSequenceFactory<TCoordinate> getDefaultCoordinateSequenceFactory<TCoordinate>()
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
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

        #region IGeometryFactory Members

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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        IPoint IGeometryFactory.CreatePoint(ICoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        IPoint IGeometryFactory.CreatePoint(ICoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        ILineString IGeometryFactory.CreateLineString()
        {
            throw new NotImplementedException();
        }

        ILineString IGeometryFactory.CreateLineString(IEnumerable<ICoordinate> coordinates)
        {
            throw new NotImplementedException();
        }

        ILineString IGeometryFactory.CreateLineString(ICoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        ILinearRing IGeometryFactory.CreateLinearRing()
        {
            throw new NotImplementedException();
        }

        ILinearRing IGeometryFactory.CreateLinearRing(IEnumerable<ICoordinate> coordinates)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

        public IExtents CreateExtents()
        {
            throw new NotImplementedException();
        }

        public IExtents CreateExtents(IExtents first, IExtents second)
        {
            throw new NotImplementedException();
        }

        public IExtents CreateExtents(IExtents first, IExtents second, IExtents third)
        {
            throw new NotImplementedException();
        }

        public IExtents CreateExtents(params IExtents[] extents)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}