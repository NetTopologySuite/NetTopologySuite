using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Implementation;
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
    /// </remarks>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class GeometryFactory : IGeometryFactory
    {
        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.Floating" />.
        /// </summary>
        public static readonly IGeometryFactory Default = new GeometryFactory();

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.Floating" />.
        /// </summary>
        /// <remarks>A shortcut for <see cref="GeometryFactory.Default" />.</remarks>
        public static readonly IGeometryFactory Floating = Default;

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.FloatingSingle" />.
        /// </summary>
        public static readonly IGeometryFactory FloatingSingle = new GeometryFactory(new PrecisionModel(PrecisionModels.FloatingSingle));

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" />
        /// <c> == </c> <see cref="PrecisionModels.Fixed" />.
        /// </summary>
        public static readonly IGeometryFactory Fixed = new GeometryFactory(new PrecisionModel(PrecisionModels.Fixed));

        private readonly IPrecisionModel _precisionModel;

        /// <summary>
        /// Returns the PrecisionModel that Geometries created by this factory
        /// will be associated with.
        /// </summary>
        public IPrecisionModel PrecisionModel => _precisionModel;

        private readonly ICoordinateSequenceFactory _coordinateSequenceFactory;

        /// <summary>
        ///
        /// </summary>
        public ICoordinateSequenceFactory CoordinateSequenceFactory => _coordinateSequenceFactory;

        private readonly int _srid;

        /// <summary>
        /// The SRID value defined for this factory.
        /// </summary>
        public int SRID => _srid;

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="exemplar"></param>
        /// <returns></returns>
        public static IPoint CreatePointFromInternalCoord(Coordinate coord, IGeometry exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(coord);
            return exemplar.Factory.CreatePoint(coord);
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// PrecisionModel, spatial-reference ID, and CoordinateSequence implementation.
        /// </summary>
        public GeometryFactory(IPrecisionModel precisionModel, int srid, ICoordinateSequenceFactory coordinateSequenceFactory)
        {
            _precisionModel = precisionModel;
            _coordinateSequenceFactory = coordinateSequenceFactory;
            _srid = srid;
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a double-precision floating PrecisionModel and a
        /// spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory(ICoordinateSequenceFactory coordinateSequenceFactory) :
            this(new PrecisionModel(), 0, coordinateSequenceFactory) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// {PrecisionModel} and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        public GeometryFactory(IPrecisionModel precisionModel) :
            this(precisionModel, 0, GetDefaultCoordinateSequenceFactory()) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// <c>PrecisionModel</c> and spatial-reference ID, and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        /// <param name="srid">The SRID to use.</param>
        public GeometryFactory(IPrecisionModel precisionModel, int srid) :
            this(precisionModel, srid, GetDefaultCoordinateSequenceFactory()) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having a floating
        /// PrecisionModel and a spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory() : this(new PrecisionModel(), 0) { }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="points">The <c>ICollection</c> of Points to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static IPoint[] ToPointArray(ICollection<IGeometry> points)
        {
            var list = new IPoint[points.Count];
            int i = 0;
            foreach (IPoint p in points)
                list[i++] = p;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="geometries">The <c>ICollection</c> of <c>Geometry</c>'s to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static IGeometry[] ToGeometryArray(ICollection<IGeometry> geometries)
        {
            var list = new IGeometry[geometries.Count];
            int i = 0;
            foreach (var g in geometries)
                list[i++] = g;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="lineStrings">The <c>ICollection</c> of LineStrings to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static ILineString[] ToLineStringArray(ICollection<IGeometry> lineStrings)
        {
            var list = new ILineString[lineStrings.Count];
            int i = 0;
            foreach (ILineString ls in lineStrings)
                list[i++] = ls;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="linearRings">The <c>ICollection</c> of LinearRings to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static ILinearRing[] ToLinearRingArray(ICollection<IGeometry> linearRings)
        {
            var list = new ILinearRing[linearRings.Count];
            int i = 0;
            foreach (ILinearRing lr in linearRings)
                list[i++] = lr;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="polygons">The <c>ICollection</c> of Polygons to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static IPolygon[] ToPolygonArray(ICollection<IGeometry> polygons)
        {
            var list = new IPolygon[polygons.Count];
            int i = 0;
            foreach (IPolygon p in polygons)
                list[i++] = p;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="multiPoints">The <c>ICollection</c> of MultiPoints to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static IMultiPoint[] ToMultiPointArray(ICollection<IGeometry> multiPoints)
        {
            var list = new IMultiPoint[multiPoints.Count];
            int i = 0;
            foreach (IMultiPoint mp in multiPoints)
                list[i++] = mp;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="multiLineStrings">The <c>ICollection</c> of MultiLineStrings to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static IMultiLineString[] ToMultiLineStringArray(ICollection<IGeometry> multiLineStrings)
        {
            var list = new IMultiLineString[multiLineStrings.Count];
            int i = 0;
            foreach (IMultiLineString mls in multiLineStrings)
                list[i++] = mls;
            return list;
        }

        /// <summary>
        /// Converts the <c>ICollection</c> to an array.
        /// </summary>
        /// <param name="multiPolygons">The <c>ICollection</c> of MultiPolygons to convert.</param>
        /// <returns>The <c>ICollection</c> in array format.</returns>
        public static IMultiPolygon[] ToMultiPolygonArray(ICollection<IGeometry> multiPolygons)
        {
            var list = new IMultiPolygon[multiPolygons.Count];
            int i = 0;
            foreach (IMultiPolygon mp in multiPolygons)
                list[i++] = mp;
            return list;
        }

        /// <summary>
        /// Creates a <see cref="IGeometry"/> with the same extent as the given envelope.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Geometry returned is guaranteed to be valid.
        /// To provide this behavior, the following cases occur:
        /// </para>
        /// <para>
        /// If the <c>Envelope</c> is:
        /// <ul>
        /// <li>null returns an empty <see cref="IPoint"/></li>
        /// <li>a point returns a non-empty <see cref="IPoint"/></li>
        /// <li>a line returns a two-point <see cref="ILineString"/></li>
        /// <li>a rectangle returns a <see cref="IPolygon"/> whose points are (minx, maxy), (minx, maxy), (maxx, maxy), (maxx, miny).</li>
        /// </ul>
        /// </para>
        /// </remarks>
        /// <param name="envelope">The <c>Envelope</c></param>
        /// <returns>
        /// An empty <c>Point</c> (for null <c>Envelope</c>s), a <c>Point</c> (when min x = max x and min y = max y) or a <c>Polygon</c> (in all other cases)
        /// </returns>
        public virtual IGeometry ToGeometry(Envelope envelope)
        {
            // null envelope - return empty point geometry
            if (envelope.IsNull)
                return CreatePoint((ICoordinateSequence)null);

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
        public IPoint CreatePoint()
        {
            return CreatePoint(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }

        /// <summary>
        /// Creates a Point using the given Coordinate.
        /// A <c>null</c> coordinate creates an empty Geometry.
        /// </summary>
        /// <param name="coordinate">a Coordinate, or null</param>
        /// <returns>A <see cref="IPoint"/> object</returns>
        public IPoint CreatePoint(Coordinate coordinate)
        {
            return CreatePoint(coordinate != null ? CoordinateSequenceFactory.Create(new[] { coordinate }) : null);
        }

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        /// <param name="coordinates">a CoordinateSequence (possibly empty), or null</param>
        /// <returns>A <see cref="IPoint"/> object</returns>
        public IPoint CreatePoint(ICoordinateSequence coordinates)
        {
            return new Point(coordinates, this);
        }

        /// <summary>Creates an empty LineString</summary>
        /// <returns>An empty LineString</returns>
        public ILineString CreateLineString()
        {
            return CreateLineString(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }

        /// <summary>
        /// Creates a LineString using the given Coordinates.
        /// A null or empty array creates an empty LineString.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns>A <see cref="ILineString"/> object</returns>
        public ILineString CreateLineString(Coordinate[] coordinates)
        {
            return CreateLineString(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a LineString using the given CoordinateSequence.
        /// A null or empty CoordinateSequence creates an empty LineString.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or null.</param>
        /// <returns>A <see cref="ILineString"/> object</returns>
        public ILineString CreateLineString(ICoordinateSequence coordinates)
        {
            return new LineString(coordinates, this);
        }

        /// <summary>Creates an empty LinearRing</summary>
        /// <returns>An empty LinearRing</returns>
        public ILinearRing CreateLinearRing()
        {
            return CreateLinearRing(CoordinateSequenceFactory.Create(0, CoordinateSequenceFactory.Ordinates));
        }
        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>Coordinates</c>; a null or empty array
        /// creates an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns>A <see cref="ILinearRing"/> object</returns>
        /// <exception cref="ArgumentException"> If the ring is not closed, or has too few points</exception>
        public ILinearRing CreateLinearRing(Coordinate[] coordinates)
        {
            return CreateLinearRing(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>CoordinateSequence</c>; a null or empty CoordinateSequence
        /// creates an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or null.</param>
        /// <returns>A <see cref="ILinearRing"/> object</returns>
        /// <exception cref="ArgumentException"> If the ring is not closed, or has too few points</exception>
        public ILinearRing CreateLinearRing(ICoordinateSequence coordinates)
        {
            return new LinearRing(coordinates, this);
        }

        /// <summary>Creates an empty Polygon</summary>
        /// <returns>An empty Polygon</returns>
        public IPolygon CreatePolygon()
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
        /// <returns>A <see cref="IPolygon"/> object</returns>
        public virtual IPolygon CreatePolygon(ILinearRing shell, ILinearRing[] holes)
        {
            return new Polygon(shell, holes, this);
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="coordinates">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>A <see cref="IPolygon"/> object</returns>
        /// <exception cref="ArgumentException">If the boundary ring is invalid</exception>
        public virtual IPolygon CreatePolygon(ICoordinateSequence coordinates)
        {
            return CreatePolygon(CreateLinearRing(coordinates));
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="coordinates">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>A <see cref="IPolygon"/> object</returns>
        /// <exception cref="ArgumentException">If the boundary ring is invalid</exception>
        public virtual IPolygon CreatePolygon(Coordinate[] coordinates)
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
        public virtual IPolygon CreatePolygon(ILinearRing shell)
        {
            return CreatePolygon(shell, null);
        }

        /// <summary>Creates an empty MultiPoint</summary>
        /// <returns>An empty MultiPoint</returns>
        public IMultiPoint CreateMultiPoint()
        {
            return new MultiPoint(null, this);
        }

        /// <summary>
        /// Creates a <see cref="IMultiPoint"/> using the given Points.
        /// A null or empty array will  create an empty MultiPoint.
        /// </summary>
        /// <param name="point">An array (without null elements), or an empty array, or <c>null</c>.</param>
        /// <returns>A <see cref="IMultiPoint"/> object</returns>
        public IMultiPoint CreateMultiPoint(IPoint[] point)
        {
            return new MultiPoint(point, this);
        }

        /// <summary>
        /// Creates a <see cref="IMultiPoint"/> using the given Coordinates.
        /// A null or empty array will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">An array (without null elements), or an empty array, or <c>null</c></param>
        /// <returns>A <see cref="IMultiPoint"/> object</returns>
        [Obsolete("Use CreateMultiPointFromCoords")]
        public IMultiPoint CreateMultiPoint(Coordinate[] coordinates)
        {
            return CreateMultiPointFromCoords(coordinates);
        }

        /// <summary>
        /// Creates a <see cref="IMultiPoint"/> using the given Coordinates.
        /// A null or empty array will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">An array (without null elements), or an empty array, or <c>null</c></param>
        /// <returns>A <see cref="IMultiPoint"/> object</returns>
        public IMultiPoint CreateMultiPointFromCoords(Coordinate[] coordinates)
        {
            return CreateMultiPoint(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a <see cref="IMultiPoint"/> using the given CoordinateSequence.
        /// A null or empty CoordinateSequence will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or <c>null</c>.</param>
        /// <returns>A <see cref="IMultiPoint"/> object</returns>
        public IMultiPoint CreateMultiPoint(ICoordinateSequence coordinates)
        {
            if (coordinates == null)
                coordinates = CoordinateSequenceFactory.Create(new Coordinate[] { });

            var points = new List<IPoint>();
            for (int i = 0; i < coordinates.Count; i++)
            {
                var seq = CoordinateSequenceFactory.Create(1, coordinates.Ordinates);
                CoordinateSequences.Copy(coordinates, i, seq, 0, 1);
                points.Add(CreatePoint(seq));
            }
            return CreateMultiPoint(points.ToArray());
        }

        /// <summary>Creates an empty MultiLineString</summary>
        /// <returns>An empty MultiLineString</returns>
        public IMultiLineString CreateMultiLineString()
        {
            return new MultiLineString(null, this);
        }

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the given <c>LineStrings</c>; a null or empty
        /// array will create an empty MultiLineString.
        /// </summary>
        /// <param name="lineStrings">LineStrings, each of which may be empty but not null-</param>
        /// <returns>A <see cref="IMultiLineString"/> object</returns>
        public IMultiLineString CreateMultiLineString(ILineString[] lineStrings)
        {
            return new MultiLineString(lineStrings, this);
        }

        /// <summary>Creates an empty MultiPolygon</summary>
        /// <returns>An empty MultiPolygon</returns>
        public IMultiPolygon CreateMultiPolygon()
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
        /// <returns>A <see cref="IMultiPolygon"/> object</returns>
        public IMultiPolygon CreateMultiPolygon(IPolygon[] polygons)
        {
            return new MultiPolygon(polygons, this);
        }

        /// <summary>Creates an empty GeometryCollection</summary>
        /// <returns>An empty GeometryCollection</returns>
        public IGeometryCollection CreateGeometryCollection()
        {
            return new GeometryCollection(null, this);
        }

        /// <summary>
        /// Creates a <c>GeometryCollection</c> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">an array of Geometries, each of which may be empty but not null, or null</param>
        /// <returns>A <see cref="IGeometryCollection"/> object</returns>
        public IGeometryCollection CreateGeometryCollection(IGeometry[] geometries)
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
        /// A <see cref="IGeometry"/> of the "smallest", "most type-specific"
        /// class that can contain the elements of <c>geomList</c>.
        /// </returns>
        public IGeometry BuildGeometry(ICollection<IGeometry> geomList)
        {
            /**
             * Determine some facts about the geometries in the list
             */
            Type geomClass = null;
            bool isHeterogeneous = false;
            bool hasGeometryCollection = false;

            IGeometry geom0 = null;
            foreach (var geom in geomList)
            {
                if (geom == null) continue;
                geom0 = geom;

                var partClass = geom.GetType();
                if (geomClass == null)
                    geomClass = partClass;
                if (partClass != geomClass)
                    isHeterogeneous = true;
                if (geom is IGeometryCollection)
                    hasGeometryCollection = true;
            }

            /**
             * Now construct an appropriate geometry to return
             */

            // for the empty point, return an empty GeometryCollection
            if (geomClass == null)
                return CreateGeometryCollection(null);

            // for heterogenous collection of geometries or if it contains a GeometryCollection, return a GeometryCollection
            if (isHeterogeneous || hasGeometryCollection)
                return CreateGeometryCollection(ToGeometryArray(geomList));

            // at this point we know the collection is homogenous.
            // Determine the type of the result from the first Geometry in the list
            // this should always return a point, since otherwise an empty collection would have already been returned
            bool isCollection = geomList.Count > 1;

            if (isCollection)
            {
                if (geom0 is IPolygon)
                    return CreateMultiPolygon(ToPolygonArray(geomList));
                if (geom0 is ILineString)
                    return CreateMultiLineString(ToLineStringArray(geomList));
                if (geom0 is IPoint)
                    return CreateMultiPoint(ToPointArray(geomList));
                Assert.ShouldNeverReachHere("Unhandled class: " + geom0.GetType().FullName);
            }
            return geom0;
        }

        /// <summary>
        /// Creates a deep copy of the input <see cref="IGeometry"/>.
        /// The <see cref="ICoordinateSequenceFactory"/> defined for this factory
        /// is used to copy the <see cref="ICoordinateSequence"/>s
        /// of the input geometry.
        /// <para/>
        /// This is a convenient way to change the <tt>CoordinateSequence</tt>
        /// used to represent a geometry, or to change the
        /// factory used for a geometry.
        /// <para/>
        /// <see cref="IGeometry.Copy()"/> can also be used to make a deep copy,
        /// but it does not allow changing the CoordinateSequence type.
        /// </summary>
        /// <param name="g">The geometry</param>
        /// <returns>A deep copy of the input geometry, using the CoordinateSequence type of this factory</returns>
        /// <seealso cref="IGeometry.Copy"/>
        public IGeometry CreateGeometry(IGeometry g)
        {
            // NOTE: don't move lambda to a separate variable!
            //       make a variable and you've broke WinPhone build.
            var operation = new GeometryEditor.CoordinateSequenceOperation((x, y) => _coordinateSequenceFactory.Create(x));
            var editor = new GeometryEditor(this);
            return editor.Edit(g, operation);
        }

        private static ICoordinateSequenceFactory GetDefaultCoordinateSequenceFactory()
        {
            return CoordinateArraySequenceFactory.Instance;
        }
    }
}
