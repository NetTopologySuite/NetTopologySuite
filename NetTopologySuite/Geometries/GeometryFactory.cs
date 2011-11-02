using System;
//using System.Collections;
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
//#if !SILVERLIGHT
    [Serializable]
//#endif
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
           
        private readonly IPrecisionModel precisionModel;

        /// <summary>
        /// Returns the PrecisionModel that Geometries created by this factory
        /// will be associated with.
        /// </summary>
        public IPrecisionModel PrecisionModel
        {
            get { return precisionModel; }
        }

        private readonly ICoordinateSequenceFactory coordinateSequenceFactory;

        /// <summary>
        /// 
        /// </summary>
        public ICoordinateSequenceFactory CoordinateSequenceFactory
        {
            get { return coordinateSequenceFactory; }
        }

        private int srid;

        /// <summary>
        /// 
        /// </summary>
        public int SRID
        {
            get { return srid; }
        }        

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
        public GeometryFactory(IPrecisionModel precisionModel, int SRID, ICoordinateSequenceFactory coordinateSequenceFactory) 
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
        /// <param name="SRID">The SRID to use.</param>
        public GeometryFactory(IPrecisionModel precisionModel, int SRID) : 
            this(precisionModel, SRID, GetDefaultCoordinateSequenceFactory()) { }

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
            IPoint[] list = new IPoint[points.Count];
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
            IGeometry[] list = new IGeometry[geometries.Count];
            int i = 0;
            foreach (IGeometry g in geometries)
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
            ILineString[] list = new ILineString[lineStrings.Count];
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
            ILinearRing[] list = new ILinearRing[linearRings.Count];
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
            IPolygon[] list = new IPolygon[polygons.Count];
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
            IMultiPoint[] list = new IMultiPoint[multiPoints.Count];
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
            IMultiLineString[] list = new IMultiLineString[multiLineStrings.Count];
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
            IMultiPolygon[] list = new IMultiPolygon[multiPolygons.Count];
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
        /// To provide this behaviour, the following cases occur:
        /// </para>
        /// <para>
        /// If the <c>Envelope</c> is:
        /// <ul>
        /// <li>null returns an empty <see cref="IPoint"/></li>
        /// <li>a point returns a non-empty <see cref="IPoint"/></li>
        /// <li>a line returns a two-point <see cref="ILineString"/></li>
        /// <li>a rectangle returns a <see cref="IPolygon"/> whose points are (minx, maxy), (maxx, maxy), (maxx, miny), (minx, miny).</li>
        /// </ul>
        /// </para>
        /// </remarks>
        /// <param name="envelope">The <c>Envelope</c></param>       
        /// <returns>
        /// An empty <c>Point</c> (for null <c>Envelope</c>s), a <c>Point</c> (when min x = max x and min y = max y) or a <c>Polygon</c> (in all other cases)
        /// </returns>
        public IGeometry ToGeometry(Envelope envelope) 
        {
            // null envelope - return empty point geometry
            if (envelope.IsNull) 
                return CreatePoint((ICoordinateSequence) null);

            // point?
            if (envelope.MinX == envelope.MaxX && envelope.MinY == envelope.MaxY) 
                return CreatePoint(new Coordinate(envelope.MinX, envelope.MinY));

            // vertical or horizontal line?
            if (envelope.MinX == envelope.MaxX
                    || envelope.MinY == envelope.MaxY)
            {
                return CreateLineString(new Coordinate[] 
                    {
                        new Coordinate(envelope.MinX, envelope.MinY),
                        new Coordinate(envelope.MaxX, envelope.MaxY)
                    });
            }


            ILinearRing ring = this.CreateLinearRing(new Coordinate[]
            {
                new Coordinate(envelope.MinX, envelope.MinY),
                new Coordinate(envelope.MinX, envelope.MaxY),
                new Coordinate(envelope.MaxX, envelope.MaxY),
                new Coordinate(envelope.MaxX, envelope.MinY),
                new Coordinate(envelope.MinX, envelope.MinY),
            });
            return CreatePolygon(ring, null);
        }

        /// <summary>
        /// Creates a Point using the given Coordinate; a null Coordinate will create
        /// an empty Geometry.
        /// </summary>
        /// <param name="coordinate"></param>
        public IPoint CreatePoint(Coordinate coordinate) 
        {
            return CreatePoint(coordinate != null ? CoordinateSequenceFactory.Create(new[] { coordinate }) : null);
        }

        /// <summary>
        /// Creates a <c>Point</c> using the given <c>CoordinateSequence</c>; a null or empty
        /// CoordinateSequence will create an empty Point.
        /// </summary>
        /// <param name="coordinates"></param>
        public IPoint CreatePoint(ICoordinateSequence coordinates) 
        {
  	        return new Point(coordinates, this);
        }

        /// <summary> 
        /// Creates a LineString using the given Coordinates; a null or empty array will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        /// <returns></returns>
        public ILineString CreateLineString(Coordinate[] coordinates)
        {
            return CreateLineString(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary>
        /// Creates a LineString using the given CoordinateSequence; a null or empty CoordinateSequence will
        /// create an empty LineString. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        /// <returns></returns>
        public ILineString CreateLineString(ICoordinateSequence coordinates)
        {
            return new LineString(coordinates, this);
        }

        /// <summary>
        /// Creates a <c>LinearRing</c> using the given <c>Coordinates</c>; a null or empty array will
        /// create an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">An array without null elements, or an empty array, or null.</param>
        public ILinearRing CreateLinearRing(Coordinate[] coordinates)
        {
            return CreateLinearRing(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary> 
        /// Creates a <c>LinearRing</c> using the given <c>CoordinateSequence</c>; a null or empty CoordinateSequence will
        /// create an empty LinearRing. The points must form a closed and simple
        /// linestring. Consecutive points must not be equal.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence possibly empty, or null.</param>
        public ILinearRing CreateLinearRing(ICoordinateSequence coordinates)
        {
            return new LinearRing(coordinates, this);
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
        public IPolygon CreatePolygon(ILinearRing shell, ILinearRing[] holes)
        {
            return new Polygon(shell, holes, this);
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
        public IMultiPoint CreateMultiPoint(Coordinate[] coordinates)
        {
            return CreateMultiPoint(coordinates != null ? CoordinateSequenceFactory.Create(coordinates) : null);
        }

        /// <summary> 
        /// Creates a <see cref="IMultiPoint"/> using the given CoordinateSequence.
        /// A null or empty CoordinateSequence will create an empty MultiPoint.
        /// </summary>
        /// <param name="coordinates">A CoordinateSequence (possibly empty), or <c>null</c>.</param>
        public IMultiPoint CreateMultiPoint(ICoordinateSequence coordinates)
        {
            if (coordinates == null)
                coordinates = CoordinateSequenceFactory.Create(new Coordinate[] { });

            List<IPoint> points = new List<IPoint>();
            for (int i = 0; i < coordinates.Count; i++)
                points.Add(CreatePoint(coordinates.GetCoordinate(i)));
            return CreateMultiPoint(points.ToArray());
        }

        /// <summary>
        /// Creates a <c>MultiLineString</c> using the given <c>LineStrings</c>; a null or empty
        /// array will create an empty MultiLineString.
        /// </summary>
        /// <param name="lineStrings">LineStrings, each of which may be empty but not null-</param>
        public IMultiLineString CreateMultiLineString(ILineString[] lineStrings) 
        {
  	        return new MultiLineString(lineStrings, this);
        }

        /// <summary>
        /// Creates a <c>MultiPolygon</c> using the given <c>Polygons</c>; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        public IMultiPolygon CreateMultiPolygon(IPolygon[] polygons)
        {
            return new MultiPolygon(polygons, this);
        }      

        /// <summary>
        /// Creates a <c>GeometryCollection</c> using the given <c>Geometries</c>; a null or empty
        /// array will create an empty GeometryCollection.
        /// </summary>
        /// <param name="geometries">Geometries, each of which may be empty but not null.</param>
        public IGeometryCollection CreateGeometryCollection(IGeometry[] geometries) 
        {
  	        return new GeometryCollection(geometries, this);
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
        /// <param name="geomList">The <c>Geometry</c> to combine.</param>
        /// <returns>
        /// A <c>Geometry</c> of the "smallest", "most type-specific" 
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

            foreach (IGeometry geom in geomList)
            {                
                Type partClass = geom.GetType();
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

            if (isHeterogeneous || hasGeometryCollection)             
                return CreateGeometryCollection(ToGeometryArray(geomList));            

            // at this point we know the collection is hetereogenous.
            // Determine the type of the result from the first Geometry in the list
            // this should always return a point, since otherwise an empty collection would have already been returned
            IEnumerator<IGeometry> ienum = geomList.GetEnumerator();
            ienum.MoveNext();
            IGeometry geom0 = ienum.Current;
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
        /// Returns a clone of g based on a CoordinateSequence created by this
        /// GeometryFactory's CoordinateSequenceFactory.        
        /// </summary>
        public IGeometry CreateGeometry(IGeometry g)
        {
            // could this be cached to make this more efficient? Or maybe it isn't enough overhead to bother
            GeometryEditor editor = new GeometryEditor(this);
            return editor.Edit(g, new AnonymousCoordinateOperationImpl());            
        }

        private static ICoordinateSequenceFactory GetDefaultCoordinateSequenceFactory()
        {
            return CoordinateArraySequenceFactory.Instance;
        }

        private class AnonymousCoordinateOperationImpl : GeometryEditor.CoordinateOperation
        {
            public override Coordinate[] Edit(Coordinate[] coordinates, IGeometry geometry)
            {
                return coordinates;
            }
        }
    }
}
