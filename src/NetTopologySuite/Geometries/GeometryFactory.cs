using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Supplies a set of utility methods for building Geometry objects
    /// from lists of Coordinates.
    /// </summary>
    /// <remarks>
    /// Note that the factory constructor methods do <b>not</b> change the input coordinates in any way.
    /// In particular, they are not rounded to the supplied <c>PrecisionModel</c>.
    /// It is assumed that input Coordinates meet the given precision.
    /// <para/>
    /// Instances of this class are thread-safe.
    /// </remarks>
    [Serializable]
    public class GeometryFactory
    {
        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />c
        /// <c> == </c> <see cref="PrecisionModels.Floating" />.
        /// </summary>
        public static readonly GeometryFactory Default = new GeometryFactory();

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.Floating" />.
        /// </summary>
        /// <remarks>A shortcut for <see cref="Default" />.</remarks>
        public static readonly GeometryFactory Floating = Default;

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.FloatingSingle" />.
        /// </summary>
        public static readonly GeometryFactory FloatingSingle = new GeometryFactory(PrecisionModel.FloatingSingle.Value);

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.Fixed" />.
        /// </summary>
        public static readonly GeometryFactory Fixed = new GeometryFactory(PrecisionModel.Fixed.Value);

        private readonly PrecisionModel _precisionModel;

        /// <summary>
        /// Returns the PrecisionModel that Geometries created by this factory
        /// will be associated with.
        /// </summary>
        public PrecisionModel PrecisionModel => _precisionModel;

        private readonly CoordinateSequenceFactory _coordinateSequenceFactory;

        /// <summary>
        ///
        /// </summary>
        public CoordinateSequenceFactory CoordinateSequenceFactory => _coordinateSequenceFactory;

        private readonly int _srid;

        /// <summary>
        /// The SRID value defined for this factory.
        /// </summary>
        public int SRID => _srid;

        /// <summary>
        /// Gets a value indicating the geometry overlay function set to use
        /// </summary>
        /// <returns>A geometry overlay function set.</returns>
        internal GeometryOverlay GeometryOverlay
        {
            get { return GeometryServices.GeometryOverlay; }
        }

        /// <summary>
        /// Gets a value indicating the geometry relation function set to use
        /// </summary>
        /// <returns>A geometry relation function set.</returns>
        internal GeometryRelate GeometryRelate
        {
            get { return GeometryServices.GeometryRelate; }
        }

        /// <summary>
        /// Gets a value indicating the geometry overlay function set to use
        /// </summary>
        /// <returns>A geometry overlay function set.</returns>
        internal CoordinateEqualityComparer CoordinateEqualityComparer
        {
            get { return GeometryServices.CoordinateEqualityComparer; }
        }

        [NonSerialized]
        private NtsGeometryServices _services;

        /// <summary>
        /// Gets a value indicating the <see cref="NtsGeometryServices"/> object that created this factory.
        /// </summary>
        public NtsGeometryServices GeometryServices
        {
            get => _services;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="exemplar"></param>
        /// <returns></returns>
        public static Point CreatePointFromInternalCoord(Coordinate coord, Geometry exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(coord);
            return exemplar.Factory.CreatePoint(coord);
        }

        /// <summary>
        /// Constructs a <c>GeometryFactory</c> that generates Geometries having the given
        /// <paramref name="precisionModel">precision model</paramref>, <paramref name="srid">spatial-reference ID</paramref>,
        /// <paramref name="coordinateSequenceFactory">CoordinateSequence</paramref> and
        /// <paramref name="services"><c>NtsGeometryServices</c></paramref>.
        /// </summary>
        /// <param name="precisionModel">A precision model</param>
        /// <param name="srid">A spatial reference id</param>
        /// <param name="coordinateSequenceFactory">A coordinate sequence factory</param>
        /// <param name="services"><c>NtsGeometryServices</c> object creating this factory</param>
        public GeometryFactory(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory,
            NtsGeometryServices services)
        {
            _precisionModel = precisionModel;
            _coordinateSequenceFactory = coordinateSequenceFactory;
            _srid = srid;
            _services = services;
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// PrecisionModel, spatial-reference ID, and CoordinateSequence implementation.
        /// </summary>
        public GeometryFactory(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory)
            : this(precisionModel, srid, coordinateSequenceFactory, NtsGeometryServices.Instance)
        {
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a double-precision floating PrecisionModel and a
        /// spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory(CoordinateSequenceFactory coordinateSequenceFactory) :
            this(new PrecisionModel(), 0, coordinateSequenceFactory) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// {PrecisionModel} and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        public GeometryFactory(PrecisionModel precisionModel) :
            this(precisionModel, 0, NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// <c>PrecisionModel</c> and spatial-reference ID, and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        /// <param name="srid">The SRID to use.</param>
        public GeometryFactory(PrecisionModel precisionModel, int srid) :
            this(precisionModel, srid, NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having a floating
        /// PrecisionModel and a spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory() : this(new PrecisionModel(), 0) { }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="points">The <c>IEnumerable</c> of Points to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static Point[] ToPointArray(IEnumerable<Geometry> points)
        {
            // PERF: these ToFooArray methods could use .Cast<T>() instead of
            // .Select(geom => (T)geom), but the latter happens to be optimized
            // for input types that carry length information so that the array
            // can be allocated at the right size right away.
            return points.Select(geom => (Point)geom).ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="geometries">The <c>IEnumerable</c> of <c>Geometry</c>'s to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static Geometry[] ToGeometryArray(IEnumerable<Geometry> geometries)
        {
            return geometries.ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="lineStrings">The <c>IEnumerable</c> of LineStrings to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static LineString[] ToLineStringArray(IEnumerable<Geometry> lineStrings)
        {
            return lineStrings.Select(geom => (LineString)geom).ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="linearRings">The <c>IEnumerable</c> of LinearRings to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static LinearRing[] ToLinearRingArray(IEnumerable<Geometry> linearRings)
        {
            return linearRings.Select(geom => (LinearRing)geom).ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="polygons">The <c>IEnumerable</c> of Polygons to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static Polygon[] ToPolygonArray(IEnumerable<Geometry> polygons)
        {
            return polygons.Select(geom => (Polygon)geom).ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="multiPoints">The <c>IEnumerable</c> of MultiPoints to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static MultiPoint[] ToMultiPointArray(IEnumerable<Geometry> multiPoints)
        {
            return multiPoints.Select(geom => (MultiPoint)geom).ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="multiLineStrings">The <c>IEnumerable</c> of MultiLineStrings to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static MultiLineString[] ToMultiLineStringArray(IEnumerable<Geometry> multiLineStrings)
        {
            return multiLineStrings.Select(geom => (MultiLineString)geom).ToArray();
        }

        /// <summary>
        /// Converts the <c>IEnumerable</c> to an array.
        /// </summary>
        /// <param name="multiPolygons">The <c>IEnumerable</c> of MultiPolygons to convert.</param>
        /// <returns>The <c>IEnumerable</c> in array format.</returns>
        public static MultiPolygon[] ToMultiPolygonArray(IEnumerable<Geometry> multiPolygons)
        {
            return multiPolygons.Select(geom => (MultiPolygon)geom).ToArray();
        }

        /// <summary>
        /// Creates a <see cref="Geometry"/> with the same extent as the given envelope.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Geometry returned is guaranteed to be valid.
        /// To provide this behavior, the following cases occur:
        /// </para>
        /// <para>
        /// If the <c>Envelope</c> is:
        /// <ul>
        /// <li>null returns an empty <see cref="Point"/></li>
        /// <li>a point returns a non-empty <see cref="Point"/></li>
        /// <li>a line returns a two-point <see cref="LineString"/></li>
        /// <li>a rectangle returns a <see cref="Polygon"/> whose points are (minx, maxy), (minx, maxy), (maxx, maxy), (maxx, miny).</li>
        /// </ul>
        /// </para>
        /// </remarks>
        /// <param name="envelope">The <c>Envelope</c></param>
        /// <returns>
        /// An empty <c>Point</c> (for null <c>Envelope</c>s), a <c>Point</c> (when min x = max x and min y = max y) or a <c>Polygon</c> (in all other cases)
        /// </returns>
        public virtual Geometry ToGeometry(Envelope envelope)
        {
            // null envelope - return empty point geometry
            if (envelope.IsNull)
                return CreatePoint((CoordinateSequence)null);

            // point?
            if (envelope.MinX == envelope.MaxX && envelope.MinY == envelope.MaxY)
                return CreatePoint(new Coordinate(envelope.MinX, envelope.MinY));

            // vertical or horizontal line?
            if (envelope.MinX == envelope.MaxX
                    || envelope.MinY == envelope.MaxY)
            {
                return CreateLineString(new[]
                    {
                        new Coordinate(envelope.MinX, envelope.MinY),
                        new Coordinate(envelope.MaxX, envelope.MaxY)
                    });
            }

            // return CW polygon
            var ring = CreateLinearRing(new[]
            {
                new Coordinate(envelope.MinX, envelope.MinY),
                new Coordinate(envelope.MinX, envelope.MaxY),
                new Coordinate(envelope.MaxX, envelope.MaxY),
                new Coordinate(envelope.MaxX, envelope.MinY),
                new Coordinate(envelope.MinX, envelope.MinY)
            });
            return CreatePolygon(ring, null);
        }

        /// <summary>
        /// Creates an empty Point
        /// </summary>
        /// <returns>
        /// An empty Point
        /// </returns>
        public Point CreatePoint()
        {
            return CreatePoint(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }

        /// <summary>
        /// Creates a Point using the given Coordinate.
        /// A <c>null</c> coordinate creates an empty Geometry.
        /// </summary>
        /// <param name="coordinate">a Coordinate, or null</param>
        /// <returns>A <see cref="Point"/> object</returns>
        public Point CreatePoint(Coordinate coordinate)
        {
            return CreatePoint(coordinate != null ? CoordinateSequenceFactory.Create(new[] { coordinate }) : null);
        }

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        /// <param name="coordinates">a CoordinateSequence (possibly empty), or null</param>
        /// <returns>A <see cref="Point"/> object</returns>
        public virtual Point CreatePoint(CoordinateSequence coordinates)
        {
            return new Point(coordinates, this);
        }

        /// <summary>
        /// Creates an empty LineString
        /// </summary>
        /// <returns>An empty LineString</returns>
        public LineString CreateLineString()
        {
            return CreateLineString(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }

        /// <summary>
        /// Creates a LineString using the given Coordinates.
        /// A null or empty array creates an empty LineString.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns>A <see cref="LineString"/> object</returns>
        public LineString CreateLineString(Coordinate[] coordinates)
        {
            return CreateLineString(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a LineString using the given CoordinateSequence.
        /// A null or empty CoordinateSequence creates an empty LineString.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or null.</param>
        /// <returns>A <see cref="LineString"/> object</returns>
        public virtual LineString CreateLineString(CoordinateSequence coordinates)
        {
            return new LineString(coordinates, this);
        }

        /// <summary>Creates an empty LinearRing</summary>
        /// <returns>An empty LinearRing</returns>
        public LinearRing CreateLinearRing()
        {
            return CreateLinearRing(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }
        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>Coordinates</c>; a null or empty array
        /// creates an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns>A <see cref="LinearRing"/> object</returns>
        /// <exception cref="ArgumentException"> If the ring is not closed, or has too few points</exception>
        public LinearRing CreateLinearRing(Coordinate[] coordinates)
        {
            return CreateLinearRing(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>CoordinateSequence</c>; a null or empty CoordinateSequence
        /// creates an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or null.</param>
        /// <returns>A <see cref="LinearRing"/> object</returns>
        /// <exception cref="ArgumentException"> If the ring is not closed, or has too few points</exception>
        public virtual LinearRing CreateLinearRing(CoordinateSequence coordinates)
        {
            return new LinearRing(coordinates, this);
        }

        /// <summary>Creates an empty Polygon</summary>
        /// <returns>An empty Polygon</returns>
        public Polygon CreatePolygon()
        {
            return CreatePolygon(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary and
        /// interior boundaries.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <c>Polygon</c>, or
        /// <c>null</c> or empty <c>LinearRing</c> s if
        /// the empty point is to be created.
        /// </param>
        /// <returns>A <see cref="Polygon"/> object</returns>
        public virtual Polygon CreatePolygon(LinearRing shell, LinearRing[] holes)
        {
            return new Polygon(shell, holes, this);
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="coordinates">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>A <see cref="Polygon"/> object</returns>
        /// <exception cref="ArgumentException">If the boundary ring is invalid</exception>
        public virtual Polygon CreatePolygon(CoordinateSequence coordinates)
        {
            return CreatePolygon(CreateLinearRing(coordinates));
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="coordinates">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>A <see cref="Polygon"/> object</returns>
        /// <exception cref="ArgumentException">If the boundary ring is invalid</exception>
        public virtual Polygon CreatePolygon(Coordinate[] coordinates)
        {
            return CreatePolygon(CreateLinearRing(coordinates));
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>the created Polygon</returns>
        /// <exception cref="ArgumentException">If the boundary ring is invalid</exception>
        public virtual Polygon CreatePolygon(LinearRing shell)
        {
            return CreatePolygon(shell, null);
        }

        /// <summary>Creates an empty MultiPoint</summary>
        /// <returns>An empty MultiPoint</returns>
        public MultiPoint CreateMultiPoint()
        {
            return new MultiPoint(null, this);
        }

        /// <summary>
        /// Creates a <see cref="MultiPoint"/> using the given Points.
        /// A null or empty array will  create an empty MultiPoint.
        /// </summary>
        /// <param name="point">An array (without null elements), or an empty array, or <c>null</c>.</param>
        /// <returns>A <see cref="MultiPoint"/> object</returns>
        public virtual MultiPoint CreateMultiPoint(Point[] point)
        {
            return new MultiPoint(point, this);
        }

        /// <summary>
        /// Creates a <see cref="MultiPoint"/> using the given Coordinates.
        /// A null or empty array will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">An array (without null elements), or an empty array, or <c>null</c></param>
        /// <returns>A <see cref="MultiPoint"/> object</returns>
        public MultiPoint CreateMultiPointFromCoords(Coordinate[] coordinates)
        {
            return CreateMultiPoint(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a <see cref="MultiPoint"/> using the given CoordinateSequence.
        /// A null or empty CoordinateSequence will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or <c>null</c>.</param>
        /// <returns>A <see cref="MultiPoint"/> object</returns>
        public MultiPoint CreateMultiPoint(CoordinateSequence coordinates)
        {
            if (coordinates == null)
                coordinates = CoordinateSequenceFactory.Create(new Coordinate[] { });

            var points = new List<Point>();
            for (int i = 0; i < coordinates.Count; i++)
            {
                var seq = CoordinateSequenceFactory.Create(1, coordinates.Dimension, coordinates.Measures);
                CoordinateSequences.Copy(coordinates, i, seq, 0, 1);
                points.Add(CreatePoint(seq));
            }
            return CreateMultiPoint(points.ToArray());
        }

        /// <summary>Creates an empty MultiLineString</summary>
        /// <returns>An empty MultiLineString</returns>
        public MultiLineString CreateMultiLineString()
        {
            return new MultiLineString(null, this);
        }

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the given <c>LineStrings</c>; a null or empty
        /// array will create an empty MultiLineString.
        /// </summary>
        /// <param name="lineStrings">LineStrings, each of which may be empty but not null-</param>
        /// <returns>A <see cref="MultiLineString"/> object</returns>
        public virtual MultiLineString CreateMultiLineString(LineString[] lineStrings)
        {
            return new MultiLineString(lineStrings, this);
        }

        /// <summary>Creates an empty MultiPolygon</summary>
        /// <returns>An empty MultiPolygon</returns>
        public MultiPolygon CreateMultiPolygon()
        {
            return new MultiPolygon(null, this);
        }

        /// <summary>
        /// Creates a <c>MultiPolygon</c> using the given <c>Polygons</c>; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        /// <returns>A <see cref="MultiPolygon"/> object</returns>
        public virtual MultiPolygon CreateMultiPolygon(Polygon[] polygons)
        {
            return new MultiPolygon(polygons, this);
        }

        /// <summary>Creates an empty GeometryCollection</summary>
        /// <returns>An empty GeometryCollection</returns>
        public GeometryCollection CreateGeometryCollection()
        {
            return new GeometryCollection(null, this);
        }

        /// <summary>
        /// Creates a <c>GeometryCollection</c> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">an array of Geometries, each of which may be empty but not null, or null</param>
        /// <returns>A <see cref="GeometryCollection"/> object</returns>
        public virtual GeometryCollection CreateGeometryCollection(Geometry[] geometries)
        {
            return new GeometryCollection(geometries, this);
        }

        /// <summary>
        /// Build an appropriate <c>Geometry</c>, <c>MultiGeometry</c>, or
        /// <c>GeometryCollection</c> to contain the <c>Geometry</c>s in
        /// it.
        /// </summary>
        /// <remarks>
        ///  If <c>geomList</c> contains a single <c>Polygon</c>,
        /// the <c>Polygon</c> is returned.<br/>
        ///  If <c>geomList</c> contains several <c>Polygon</c>s, a
        /// <c>MultiPolygon</c> is returned.<br/>
        ///  If <c>geomList</c> contains some <c>Polygon</c>s and
        /// some <c>LineString</c>s, a <c>GeometryCollection</c> is
        /// returned.<br/>
        ///  If <c>geomList</c> is empty, an empty <c>GeometryCollection</c>
        /// is returned.
        /// Note that this method does not "flatten" Geometries in the input, and hence if
        /// any MultiGeometries are contained in the input a GeometryCollection containing
        /// them will be returned.
        /// </remarks>
        /// <param name="geomList">The <c>Geometry</c> to combine.</param>
        /// <returns>
        /// A <see cref="Geometry"/> of the "smallest", "most type-specific"
        /// class that can contain the elements of <c>geomList</c>.
        /// </returns>
        public virtual Geometry BuildGeometry(IEnumerable<Geometry> geomList)
        {
            var geoms = new List<Geometry>();

            /*
             * Determine some facts about the geometries in the list
             */
            Type geomClass = null;
            bool isHeterogeneous = false;
            bool hasGeometryCollection = false;

            Geometry geom0 = null;
            foreach (var geom in geomList)
            {
                geoms.Add(geom);
                if (geom == null) continue;
                geom0 = geom;

                var partClass = geom.GetType();
                if (geomClass == null)
                    geomClass = partClass;
                if (partClass != geomClass)
                    isHeterogeneous = true;
                if (geom is GeometryCollection)
                    hasGeometryCollection = true;
            }

            /*
             * Now construct an appropriate geometry to return
             */

            // for the empty point, return an empty GeometryCollection
            if (geomClass == null)
                return CreateGeometryCollection(null);

            // for heterogenous collection of geometries or if it contains a GeometryCollection, return a GeometryCollection
            if (isHeterogeneous || hasGeometryCollection)
                return CreateGeometryCollection(geoms.ToArray());

            // at this point we know the collection is homogenous.
            // Determine the type of the result from the first Geometry in the list
            // this should always return a point, since otherwise an empty collection would have already been returned
            bool isCollection = geoms.Count > 1;

            if (isCollection)
            {
                if (geom0 is Polygon)
                    return CreateMultiPolygon(ToPolygonArray(geoms));
                if (geom0 is LineString)
                    return CreateMultiLineString(ToLineStringArray(geoms));
                if (geom0 is Point)
                    return CreateMultiPoint(ToPointArray(geoms));
                Assert.ShouldNeverReachHere("Unhandled class: " + geom0.GetType().FullName);
            }
            return geom0;
        }

        /// <summary>
        /// Creates an empty atomic geometry of the given dimension.
        /// If passed a dimension of <see cref="Dimension.False"/>
        /// will create an empty <see cref="GeometryCollection"/>.
        /// </summary>
        /// <param name="dimension">The required dimension (<see cref="Dimension.False"/>, <see cref="Dimension.Point"/>, <see cref="Dimension.Curve"/> or <see cref="Dimension.Surface"/>)</param>
        /// <returns>An empty atomic geometry of given dimension</returns>
        public Geometry CreateEmpty(Dimension dimension)
        {
            switch (dimension)
            {
                case Dimension.False: return CreateGeometryCollection();
                case Dimension.Point: return CreatePoint();
                case Dimension.Curve: return CreateLineString();
                case Dimension.Surface: return CreatePolygon();
                default:
                    throw new ArgumentOutOfRangeException($"Invalid dimension: {dimension}");
            }
        }

        /// <summary>
        /// Creates a deep copy of the input <see cref="Geometry"/>.
        /// The <see cref="Geometries.CoordinateSequenceFactory"/> defined for this factory
        /// is used to copy the <see cref="CoordinateSequence"/>s
        /// of the input geometry.
        /// <para/>
        /// This is a convenient way to change the <tt>CoordinateSequence</tt>
        /// used to represent a geometry, or to change the
        /// factory used for a geometry.
        /// <para/>
        /// <see cref="Geometry.Copy()"/> can also be used to make a deep copy,
        /// but it does not allow changing the CoordinateSequence type.
        /// </summary>
        /// <param name="g">The geometry</param>
        /// <returns>A deep copy of the input geometry, using the CoordinateSequence type of this factory</returns>
        /// <seealso cref="Geometry.Copy"/>
        public virtual Geometry CreateGeometry(Geometry g)
        {
            // NOTE: don't move lambda to a separate variable!
            //       make a variable and you've broke WinPhone build.
            var operation = new GeometryEditor.CoordinateSequenceOperation((x, y) => _coordinateSequenceFactory.Create(x));
            var editor = new GeometryEditor(this);
            return editor.Edit(g, operation);
        }

        /// <summary>
        /// Returns a new <see cref="GeometryFactory"/> whose <see cref="GeometryFactory.SRID"/> is
        /// the given value and whose other values and behavior are, as near as we possibly can make
        /// it, the same as our own.
        /// </summary>
        /// <param name="srid">
        /// The <see cref="GeometryFactory.SRID"/> for the result.
        /// </param>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public virtual GeometryFactory WithSRID(int srid)
        {
            return _srid == srid
                ? this
                : _services.CreateGeometryFactory(_precisionModel, srid, _coordinateSequenceFactory);
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return $"{GetType().Name}[PM={PrecisionModel}, SRID={SRID}, CSFactory={CoordinateSequenceFactory.GetType().Name}, GeometryOverlay:{GeometryOverlay}]";
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context)
        {
            _services = NtsGeometryServices.Instance;
        }
    }
}
