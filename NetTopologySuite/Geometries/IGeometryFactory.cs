using System;
using System.Collections.Generic;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// Supplies a set of utility methods for building Geometry objects 
    /// from lists of Coordinates.
    /// </summary>
    public interface IGeometryFactory
    {
        /// <summary>
        /// Gets the coordinate sequence factory to use when creating geometries.
        /// </summary>
        ICoordinateSequenceFactory CoordinateSequenceFactory { get; }

        /// <summary>
        /// Gets the spatial reference id to assign when creating geometries
        /// </summary>
        int SRID { get; }
        
        /// <summary>
        /// Gets the PrecisionModel that Geometries created by this factory
        /// will be associated with.
        /// </summary>
        IPrecisionModel PrecisionModel { get; }

        /// <summary>  
        /// Build an appropriate <c>Geometry</c>, <c>MultiGeometry</c>, or
        /// <c>GeometryCollection</c> to contain the <c>Geometry</c>s in
        /// it.
        /// </summary>
        /// <remarks>
        ///  If <c>geomList</c> contains a single <c>Polygon</c>,
        /// the <c>Polygon</c> is returned.
        ///  If <c>geomList</c> contains several <c>Polygon</c>s, a
        /// <c>MultiPolygon</c> is returned.
        ///  If <c>geomList</c> contains some <c>Polygon</c>s and
        /// some <c>LineString</c>s, a <c>GeometryCollection</c> is
        /// returned.
        ///  If <c>geomList</c> is empty, an empty <c>GeometryCollection</c>
        /// is returned.
        /// </remarks>
        /// <param name="geomList">The <c>Geometry</c> to combine.</param>
        /// <returns>
        /// A <c>Geometry</c> of the "smallest", "most type-specific" 
        /// class that can contain the elements of <c>geomList</c>.
        /// </returns>
        IGeometry BuildGeometry(ICollection<IGeometry> geomList);

        /// <summary>
        /// Returns a clone of g based on a CoordinateSequence created by this
        /// GeometryFactory's CoordinateSequenceFactory.        
        /// </summary>
        IGeometry CreateGeometry(IGeometry g);

        /// <summary>
        /// Creates an empty Point
        /// </summary>
        /// <returns>An empty Point</returns>
        IPoint CreatePoint();

        /// <summary>
        /// Creates a Point using the given Coordinate; a null Coordinate will create
        /// an empty Geometry.
        /// </summary>
        /// <param name="coordinate">The coordinate</param>
        /// <returns>A Point</returns>
        IPoint CreatePoint(Coordinate coordinate);

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        /// <param name="coordinates">The coordinate sequence.</param>
        /// <returns>A Point</returns>
        IPoint CreatePoint(ICoordinateSequence coordinates);

        /// <summary>
        /// Creates an empty LineString
        /// </summary>
        /// <returns>An empty LineString</returns>
        ILineString CreateLineString();

        /// <summary> 
        /// Creates a LineString using the given Coordinates; a null or empty array will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns>A LineString</returns>
        ILineString CreateLineString(Coordinate[] coordinates);

        /// <summary> 
        /// Creates a LineString using the given Coordinates; a null or empty array will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns>A LineString</returns>
        ILineString CreateLineString(ICoordinateSequence coordinates);

        /// <summary>
        /// Creates an empty LinearRing
        /// </summary>
        /// <returns>An empty LinearRing</returns>
        ILinearRing CreateLinearRing();
        
        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>Coordinates</c>; a null or empty array will
        /// create an empty LinearRing. The points must form a closed and simple
        /// LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        ILinearRing CreateLinearRing(Coordinate[] coordinates);

        /// <summary> 
        /// Creates a <c>LinearRing</c> using the given <c>CoordinateSequence</c>; a null or empty CoordinateSequence will
        /// create an empty LinearRing. The points must form a closed and simple
        /// LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        ILinearRing CreateLinearRing(ICoordinateSequence coordinates);

        /// <summary>
        /// Creates an empty Polygon
        /// </summary>
        /// <returns>An empty Polygon</returns>
        IPolygon CreatePolygon();

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
        /// <returns></returns>
        IPolygon CreatePolygon(ILinearRing shell, ILinearRing[] holes);

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="coordinates">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>The polygon</returns>
        IPolygon CreatePolygon(ICoordinateSequence coordinates);

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="coordinates">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>The polygon</returns>
        IPolygon CreatePolygon(Coordinate[] coordinates);

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">the outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty geometry is to be created.</param>
        /// <returns>The polygon</returns>
        IPolygon CreatePolygon(ILinearRing shell);

        /// <summary>
        /// Creates an empty MultiPoint
        /// </summary>
        /// <returns>An empty MultiPoint</returns>
        IMultiPoint CreateMultiPoint();

        /// <summary> 
        /// Creates a <see cref="IMultiPoint"/> using the given Coordinates.
        /// A null or empty array will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">An array (without null elements), or an empty array, or <c>null</c></param>
        /// <returns>A <see cref="IMultiPoint"/> object</returns>
        IMultiPoint CreateMultiPointFromCoords(Coordinate[] coordinates);

        /// <summary> 
        /// Creates a <see cref="IMultiPoint"/> using the given Points.
        /// A null or empty array will  create an empty MultiPoint.
        /// </summary>
        /// <param name="point">An array (without null elements), or an empty array, or <c>null</c>.</param>
        /// <returns>A <see cref="IMultiPoint"/> object</returns>
        IMultiPoint CreateMultiPoint(IPoint[] point);

        /// <summary> 
        /// Creates a <see cref="IMultiPoint"/> using the given CoordinateSequence.
        /// A null or empty CoordinateSequence will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or <c>null</c>.</param>
        IMultiPoint CreateMultiPoint(ICoordinateSequence coordinates);

        /// <summary>
        /// Creates an empty MultiLineString
        /// </summary>
        /// <returns>An empty MultiLineString</returns>
        IMultiLineString CreateMultiLineString();

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the given <c>LineStrings</c>; a null or empty
        /// array will create an empty MultiLineString.
        /// </summary>
        /// <param name="lineStrings">LineStrings, each of which may be empty but not null-</param>
        IMultiLineString CreateMultiLineString(ILineString[] lineStrings);

        /// <summary>
        /// Creates an empty MultiPolygon
        /// </summary>
        /// <returns>An empty MultiPolygon</returns>
        IMultiPolygon CreateMultiPolygon();

        /// <summary>
        /// Creates a <c>MultiPolygon</c> using the given <c>Polygons</c>; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        IMultiPolygon CreateMultiPolygon(IPolygon[] polygons);

        /// <summary>
        /// Creates an empty GeometryCollection
        /// </summary>
        /// <returns>An empty GeometryCollection</returns>
        IGeometryCollection CreateGeometryCollection();

        /// <summary>
        /// Creates a <c>GeometryCollection</c> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">Geometries, each of which may be empty but not null.</param>
        IGeometryCollection CreateGeometryCollection(IGeometry[] geometries);

        /// <summary>
        /// Creates a <see cref="IGeometry"/> with the same extent as the given envelope.
        /// </summary>
        IGeometry ToGeometry(Envelope envelopeInternal);
    }
}
