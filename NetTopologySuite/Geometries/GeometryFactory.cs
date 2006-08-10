using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using GisSharpBlog.NetTopologySuite.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Supplies a set of utility methods for building Geometry objects from lists
    /// of Coordinates.
    /// </summary>            
    [Serializable]
    public class GeometryFactory 
    {
        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating" />.
        /// </summary>
        public static GeometryFactory Default = new GeometryFactory();

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModels.Floating" />.
        /// </summary>
        /// <remarks>A shortcut for <see cref="GeometryFactory.Default" />.</remarks>
        public static GeometryFactory Floating = Default;

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModels.FloatingSingle" />.
        /// </summary>
        public static GeometryFactory FloatingSingle = new GeometryFactory(new PrecisionModel(PrecisionModels.FloatingSingle));  

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" /> 
        /// <c> == </c> <see cref="PrecisionModels.Fixed" />.
        /// </summary>
        public static GeometryFactory Fixed = new GeometryFactory(new PrecisionModel(PrecisionModels.Fixed));
           
        private PrecisionModel precisionModel;

        /// <summary>
        /// Returns the PrecisionModel that Geometries created by this factory
        /// will be associated with.
        /// </summary>
        public virtual PrecisionModel PrecisionModel
        {
            get
            {
                return precisionModel;
            }
        }

        private ICoordinateSequenceFactory coordinateSequenceFactory;

        /// <summary>
        /// 
        /// </summary>
        public virtual ICoordinateSequenceFactory CoordinateSequenceFactory
        {
            get
            {
                return coordinateSequenceFactory;
            }
        }

        private int srid;

        /// <summary>
        /// 
        /// </summary>
        public virtual int SRID
        {
            get
            {
                return srid;
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="exemplar"></param>
        /// <returns></returns>
        public static Point CreatePointFromInternalCoord(Coordinate coord, Geometry exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(ref coord);
            return exemplar.Factory.CreatePoint(coord);
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// PrecisionModel, spatial-reference ID, and CoordinateSequence implementation.
        /// </summary>        
        /// <param name="precisionModel"></param>
        /// <param name="SRID"></param>
        /// <param name="coordinateSequenceFactory"></param>       
        public GeometryFactory(PrecisionModel precisionModel, int SRID,
                               ICoordinateSequenceFactory coordinateSequenceFactory) 
        {
            this.precisionModel = precisionModel;
            this.coordinateSequenceFactory = coordinateSequenceFactory;
            this.srid = SRID;
        }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// CoordinateSequence implementation, a double-precision floating PrecisionModel and a
        /// spatial-reference ID of 0.
        /// </summary>
        /// <param name="coordinateSequenceFactory"></param>
        public GeometryFactory(ICoordinateSequenceFactory coordinateSequenceFactory) 
            : this(new PrecisionModel(), 0, coordinateSequenceFactory) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// {PrecisionModel} and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        public GeometryFactory(PrecisionModel precisionModel) 
            : this(precisionModel, 0, GetDefaultCoordinateSequenceFactory()) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having the given
        /// <c>PrecisionModel</c> and spatial-reference ID, and the default CoordinateSequence
        /// implementation.
        /// </summary>
        /// <param name="precisionModel">The PrecisionModel to use.</param>
        /// <param name="SRID">The SRID to use.</param>
        public GeometryFactory(PrecisionModel precisionModel, int SRID) 
            : this(precisionModel, SRID, GetDefaultCoordinateSequenceFactory()) { }

        /// <summary>
        /// Constructs a GeometryFactory that generates Geometries having a floating
        /// PrecisionModel and a spatial-reference ID of 0.
        /// </summary>
        public GeometryFactory() : this(new PrecisionModel(), 0) { }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="points">The <c>List</c> of Points to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static Point[] ToPointArray(IList points) 
        {            
            return (Point[])(new ArrayList(points)).ToArray(typeof(Point));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="geometries">The list of <c>Geometry's</c> to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static Geometry[] ToGeometryArray(IList geometries) 
        {
            if (geometries == null) 
                return null;          
            return (Geometry[])(new ArrayList(geometries)).ToArray(typeof(Geometry));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="linearRings">The <c>List</c> of LinearRings to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static LinearRing[] ToLinearRingArray(IList linearRings) 
        {
            return (LinearRing[])(new ArrayList(linearRings)).ToArray(typeof(LinearRing));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="lineStrings">The <c>List</c> of LineStrings to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static LineString[] ToLineStringArray(IList lineStrings)
        {
            return (LineString[])(new ArrayList(lineStrings)).ToArray(typeof(LineString));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="polygons">The <c>List</c> of Polygons to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static Polygon[] ToPolygonArray(IList polygons)
        {
            return (Polygon[])(new ArrayList(polygons)).ToArray(typeof(Polygon));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="multiPolygons">The <c>List</c> of MultiPolygons to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static MultiPolygon[] ToMultiPolygonArray(IList multiPolygons)
        {
            return (MultiPolygon[])(new ArrayList(multiPolygons)).ToArray(typeof(MultiPolygon));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="multiLineStrings">The <c>List</c> of MultiLineStrings to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static MultiLineString[] ToMultiLineStringArray(IList multiLineStrings)
        {
            return (MultiLineString[])(new ArrayList(multiLineStrings)).ToArray(typeof(MultiLineString));
        }

        /// <summary>
        /// Converts the <c>List</c> to an array.
        /// </summary>
        /// <param name="multiPoints">The <c>List</c> of MultiPoints to convert.</param>
        /// <returns>The <c>List</c> in array format.</returns>
        public static MultiPoint[] ToMultiPointArray(IList multiPoints)
        {
            return (MultiPoint[])(new ArrayList(multiPoints)).ToArray(typeof(MultiPoint));
        }

        /// <summary>
        /// If the <c>Envelope</c> is a null <c>Envelope</c>, returns an
        /// empty <c>Point</c>. If the <c>Envelope</c> is a point, returns
        /// a non-empty <c>Point</c>. If the <c>Envelope</c> is a
        /// rectangle, returns a <c>Polygon</c> whose points are (minx, miny),
        /// (maxx, miny), (maxx, maxy), (minx, maxy), (minx, miny).
        /// </summary>
        /// <param name="envelope">The <c>Envelope</c> to convert to a <c>Geometry</c>.</param>       
        /// <returns>
        /// An empty <c>Point</c> (for null <c>Envelope</c>
        /// s), a <c>Point</c> (when min x = max x and min y = max y) or a
        /// <c>Polygon</c> (in all other cases)
        /// throws a <c>TopologyException</c> if <c>coordinates</c>
        /// is not a closed linestring, that is, if the first and last coordinates
        /// are not equal.
        /// </returns>
        public virtual Geometry ToGeometry(Envelope envelope) 
        {
            if (envelope.IsNull) 
                return CreatePoint((ICoordinateSequence)null);            

            if (envelope.MinX == envelope.MaxX && envelope.MinY == envelope.MaxY) 
                return CreatePoint(new Coordinate(envelope.MinX, envelope.MinY));            

            return CreatePolygon(
                CreateLinearRing(new Coordinate[]
                {
                    new Coordinate(envelope.MinX, envelope.MinY),
                    new Coordinate(envelope.MaxX, envelope.MinY),
                    new Coordinate(envelope.MaxX, envelope.MaxY),
                    new Coordinate(envelope.MinX, envelope.MaxY),
                    new Coordinate(envelope.MinX, envelope.MinY),
                }), 
                null);
        }

        /// <summary>
        /// Creates a Point using the given Coordinate; a null Coordinate will create
        /// an empty Geometry.
        /// </summary>
        /// <param name="coordinate"></param>
        public virtual Point CreatePoint(Coordinate coordinate) 
        {
            return CreatePoint(coordinate != null ? 
                CoordinateSequenceFactory.Create(new Coordinate[] { coordinate }) : null);
        }

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        /// <param name="coordinates"></param>
        public virtual Point CreatePoint(ICoordinateSequence coordinates) 
        {
  	        return new Point(coordinates, this);
        }

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the given <c>LineStrings</c>; a null or empty
        /// array will create an empty MultiLineString.
        /// </summary>
        /// <param name="lineStrings">LineStrings, each of which may be empty but not null-</param>
        public virtual MultiLineString CreateMultiLineString(LineString[] lineStrings) 
        {
  	        return new MultiLineString(lineStrings, this);
        }

        /// <summary>
        /// Creates a <c>GeometryCollection</c> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">Geometries, each of which may be empty but not null.</param>
        public virtual GeometryCollection CreateGeometryCollection(Geometry[] geometries) 
        {
  	        return new GeometryCollection(geometries, this);
        }

        /// <summary>
        /// Creates a <c>MultiPolygon</c> using the given <c>Polygons</c>; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        public virtual MultiPolygon CreateMultiPolygon(Polygon[] polygons) 
        {
            return new MultiPolygon(polygons, this);
        }

        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>Coordinates</c>; a null or empty array will
        /// create an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        public virtual LinearRing CreateLinearRing(Coordinate[] coordinates) 
        {
            return CreateLinearRing(coordinates != null ? 
                CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary> 
        /// Creates a <c>LinearRing</c> using the given <c>CoordinateSequence</c>; a null or empty CoordinateSequence will
        /// create an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        public virtual LinearRing CreateLinearRing(ICoordinateSequence coordinates) 
        {
            return new LinearRing(coordinates, this);
        }

        /// <summary> 
        /// Creates a MultiPoint using the given Points; a null or empty array will
        /// create an empty MultiPoint.
        /// </summary>
        /// <param name="point">An array without null elements, or an empty array, or null.</param>
        public virtual MultiPoint CreateMultiPoint(Point[] point) 
        {
  	        return new MultiPoint(point, this);
        }

        /// <summary> 
        /// Creates a MultiPoint using the given Coordinates; a null or empty array will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        public virtual MultiPoint CreateMultiPoint(Coordinate[] coordinates) 
        {
            return CreateMultiPoint(coordinates != null ? 
                CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary> 
        /// Creates a MultiPoint using the given CoordinateSequence; a null or empty CoordinateSequence will
        /// create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        public virtual MultiPoint CreateMultiPoint(ICoordinateSequence coordinates) 
        {
            if (coordinates == null) 
                coordinates = CoordinateSequenceFactory.Create(new Coordinate[] { });            

            List<Point> points = new List<Point>();
            for (int i = 0; i < coordinates.Count; i++) 
                points.Add(CreatePoint(coordinates.GetCoordinate(i)));            

            return CreateMultiPoint(points.ToArray());
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
        /// <returns></returns>
        public virtual Polygon CreatePolygon(LinearRing shell, LinearRing[] holes) 
        {
            return new Polygon(shell, holes, this);
        }    

        /// <summary>  
        /// Build an appropriate <c>Geometry</c>, <c>MultiGeometry</c>, or
        /// <c>GeometryCollection</c> to contain the <c>Geometry</c>s in
        /// it.
        /// <example>
        ///  If <c>geomList</c> contains a single <c>Polygon</c>,
        /// the <c>Polygon</c> is returned.
        ///  If <c>geomList</c> contains several <c>Polygon</c>s, a
        /// <c>MultiPolygon</c> is returned.
        ///  If <c>geomList</c> contains some <c>Polygon</c>s and
        /// some <c>LineString</c>s, a <c>GeometryCollection</c> is
        /// returned.
        ///  If <c>geomList</c> is empty, an empty <c>GeometryCollection</c>
        /// is returned.
        /// Note that this method does not "flatten" Geometries in the input, and hence if
        /// any MultiGeometries are contained in the input a GeometryCollection containing
        /// them will be returned.
        /// </example>
        /// </summary>
        /// <param name="geomList">The <c>Geometry</c>s to combine.</param>
        /// <returns>A <c>Geometry</c> of the "smallest", "most type-specific" class that can contain the elements of <c>geomList</c>.</returns>
        public virtual Geometry BuildGeometry(IList geomList) 
        {
            Type geomClass = null;
            bool isHeterogeneous = false;

            for (IEnumerator i = geomList.GetEnumerator(); i.MoveNext(); ) 
            {
                Geometry geom = (Geometry) i.Current;
                Type partClass = geom.GetType();
                if (geomClass == null) 
                    geomClass = partClass;                
                if (partClass != geomClass) 
                    isHeterogeneous = true;                
            }

            // for the empty point, return an empty GeometryCollection
            if (geomClass == null) 
                return CreateGeometryCollection(null);

            if (isHeterogeneous)             
                return CreateGeometryCollection(ToGeometryArray(geomList));            

            // at this point we know the collection is hetereogenous.
            // Determine the type of the result from the first Geometry in the list
            // this should always return a point, since otherwise an empty collection would have already been returned
            IEnumerator ienum = geomList.GetEnumerator();
            ienum.MoveNext();
            Geometry geom0 = (Geometry) ienum.Current;
            bool isCollection = geomList.Count > 1;

            if (isCollection) 
            {
                if(geom0 is Polygon)
                    return CreateMultiPolygon(ToPolygonArray(geomList));                
                else if(geom0 is LineString)
                    return CreateMultiLineString(ToLineStringArray(geomList));                
                else if(geom0 is Point)
                    return CreateMultiPoint(ToPointArray(geomList));
                Assert.ShouldNeverReachHere();
            }
            return geom0;
        }

        /// <summary> 
        /// Creates a LineString using the given Coordinates; a null or empty array will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns></returns>
        public virtual LineString CreateLineString(Coordinate[] coordinates) 
        {
            return CreateLineString(coordinates != null ? 
                CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a LineString using the given CoordinateSequence; a null or empty CoordinateSequence will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        /// <returns></returns>
        public virtual LineString CreateLineString(ICoordinateSequence coordinates) 
        {
	        return new LineString(coordinates, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <returns>
        /// A clone of g based on a CoordinateSequence created by this
        /// GeometryFactory's CoordinateSequenceFactory.
        /// </returns>
        public virtual Geometry CreateGeometry(Geometry g)
        {
            // could this be cached to make this more efficient? Or maybe it isn't enough overhead to bother
            GeometryEditor editor = new GeometryEditor(this);
            return editor.Edit(g, new AnonymousCoordinateOperationImpl());            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static ICoordinateSequenceFactory GetDefaultCoordinateSequenceFactory()
        {
            return CoordinateArraySequenceFactory.Instance;
        }

        private class AnonymousCoordinateOperationImpl : GeometryEditor.CoordinateOperation
        {
            public override Coordinate[] Edit(Coordinate[] coordinates, Geometry geometry)
            {
                return coordinates;
            }
        }
    }
}
