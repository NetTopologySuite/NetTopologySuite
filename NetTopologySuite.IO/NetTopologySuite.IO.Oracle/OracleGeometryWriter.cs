using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Sdo;
using Oracle.DataAccess.Client;

namespace NetTopologySuite.IO
{
/**
 * 
 * Translates a JTS Geometry into an Oracle STRUCT representing an MDSYS.GEOMETRY object. 
 * 
 * A connection to an oracle instance with access to the definition of the MDSYS.GEOMETRY 
 * object is required by the oracle driver.
 * 
 * @version 9i
 * @author David Zwiers, Vivid Solutions.
 */
    public class OracleGeometryWriter
    {
        private const int SridNull = -1;
	    private OracleConnection _connection;
	    private int _dimension = 2;
	    private int _srid = SridNull;

        private const String Datatype = "MDSYS.SDO_GEOMETRY";

        /**
	 * Initialize the Oracle MDSYS.GEOMETRY Encoder with a valid oracle connection. 
	 * 
	 * The connection should have sufficient priveledges to view the description of the MDSYS.GEOMETRY type.
	 * 
	 * The dimension is set to 2
	 * 
	 * @param con
	 */
	public OracleGeometryWriter(OracleConnection con)
        :this(con, 2)
    {
	}
	
	/**
	 * Initialize the Oracle MDSYS.GEOMETRY Encoder with a valid oracle connection. 
	 * 
	 * The connection should have sufficient priveledges to view the description of the MDSYS.GEOMETRY type.
	 * 
	 * @param con
	 * @param dimension 
	 */
	public OracleGeometryWriter(OracleConnection con, int dimension){
		_connection = con;
		_dimension = dimension;
	}
	
	/**
	 * Provides the oppotunity to force all geometries written using this writter to be written using the 
	 * specified srid. This is useful in two cases: 1) when you do not want the geometry's srid to be 
	 * over-written or 2) when you want to ensure an entire layer is always written using a constant srid.
	 * 
	 * @param srid
	 */
	public int SRID
    {
        get { return _srid; }
        set { _srid = value; }
    }

	/**
	 * This routine will translate the JTS Geometry into an Oracle MDSYS.GEOMETRY STRUCT.
	 * 
	 * Although invalid geometries may be encoded, and inserted into an Oracle DB, this is 
	 * not recomended. It is the responsibility of the user to ensure the geometry is valid 
	 * prior to calling this method. The user should also ensure the the geometry's SRID 
	 * field contains the correct value, if an SRID is desired. An incorrect SRID value may 
	 * cause index exceptions during an insert or update. 
	 * 
	 * When a null Geometry is passed in, a non-null, empty STRUCT is returned. Therefore, 
	 * inserting the the result of calling this method directly into a table will never result 
	 * in null insertions. 
	 * (March 2006)
	 * 
	 * To pass a NULL Geometry into an oracle geometry parameter using jdbc, use 
	 * java.sql.CallableStatement.setNull(index,java.sql.Types.STRUCT,"MDSYS.SDO_GEOMETRY")
	 * (April 2006)
	 * 
	 * @param geom JTS Geometry to encode
	 * @return Oracle MDSYS.GEOMETRY STRUCT
	 * @throws SQLException 
	 */
	public SdoGeometry Write(IGeometry geom)
    {
		
//        // this line may be problematic ... for v9i and later 
//        // need to revisit.
		
//        // was this ... does not work for 9i
////		if( geom == null) return toSTRUCT( null, DATATYPE );
		
//        //works fro 9i
//        if( geom == null) return new SdoGeometry();
		
//        // does not work for 9i
////		if( geom == null) return null;
		
//        //empty geom
//        if( geom.IsEmpty || geom.Coordinate == null) 
//            return new SdoGeometry();

//        var ret = new SdoGeometry();
//        var gtype = GType(geom);
//        var gtypeint = (int) gtype;
//        ret.Sdo_Gtype = gtypeint;
//        ret.Sdo_Srid = geom.SRID == -1 ? _srid : geom.SRID;
//        ret.Point = 
            
//            //int srid = geom.getFactory().getSRID();
//        int _srid = this._srid == Constants.SRID_NULL? geom.getSRID() : this._srid;
//        NUMBER SDO_SRID = _srid == Constants.SRID_NULL ? null : new NUMBER( _srid );
        
//        double[] point = Point( geom );
        
//        SdoPoint SDO_POINT;
        
//        ret.OrdinatesArray SDO_ELEM_INFO;
//        ARRAY SDO_ORDINATES;
        
//        if( point == null ){
//            int elemInfo[] = ElemInfo( geom , gtype);
            
//            var list = new List<double[]>();
//            coordinates(list, geom);
                        
//            int dim = gtypeint / 1000;
//            int lrs = (gtypeint - dim*1000)/100;
//            int len = dim+lrs; // size per coordinate
//            var ordinates = new double[list.Count*len];
            
//            int k=0;
//            for(int i=0;i<list.Count && k<ordinates.Length;i++){
//                int j=0;
//                double[] ords = (double[]) list[i];
//                for(;j<len && j<ords.Length;j++){
//                    ordinates[k++] = ords[j];
//                }
//                for(;j<len;j++){ // mostly safety
//                    ordinates[k++] = Double.NaN;
//                }
//            }
            
//            SDO_POINT = null;
//            SDO_ELEM_INFO = toARRAY( elemInfo, "MDSYS.SDO_ELEM_INFO_ARRAY" );
//            SDO_ORDINATES = toARRAY( ordinates, "MDSYS.SDO_ORDINATE_ARRAY" );                        
//        }
//        else { // Point Optimization
//            Datum data[] = new Datum[]{
//                toNUMBER( point[0] ),
//                toNUMBER( point[1] ),
//                toNUMBER( point[2] ),
//            };
//            SDO_POINT = toSTRUCT( data, "MDSYS.SDO_POINT_TYPE"  );
//            SDO_ELEM_INFO = null;
//            SDO_ORDINATES = null;
//        }                
//        Datum attributes[] = new Datum[]{
//            SDO_GTYPE,
//            SDO_SRID,
//            SDO_POINT,
//            SDO_ELEM_INFO,
//            SDO_ORDINATES
//        };
//        return toSTRUCT( attributes, Datatype );   
	    return null;
    }

	/**
     * Encode Geometry as described by GTYPE and ELEM_INFO
     * 
     * @param list Flat list of Double
     * @param geom Geometry 
     *
     * @throws IllegalArgumentException If geometry cannot be encoded
     */
    private void coordinates(List<double[]> list, IGeometry geom) {
        switch (Template(geom)) {

        case SdoGTemplate.Coordinate:
            AddCoordinates(list, ((IPoint)geom).CoordinateSequence);
            return;
        case SdoGTemplate.Line:
            AddCoordinates(list, ((ILineString)geom).CoordinateSequence);
            return;
        case SdoGTemplate.Polygon:
            switch (ElemInfoInterpretation(geom,SdoEType.PolygonExterior)) {
            case 3:
                var e = geom.EnvelopeInternal;
                list.Add(new[] { e.MinX, e.MinY });
                list.Add(new[] { e.MaxX, e.MaxY });
                return;
            case 1:
            	var polygon = (IPolygon) geom;
                int holes = polygon.NumInteriorRings;
                
                // check outer ring's direction
                var ring = polygon.ExteriorRing.CoordinateSequence;
                if (!Algorithm.CGAlgorithms.IsCCW(ring.ToCoordinateArray())) {
                    ring = reverse(polygon.Factory.CoordinateSequenceFactory, ring); 
                }
                AddCoordinates(list,ring);

                for (int i = 0; i < holes; i++) {
                	// check inner ring's direction
                	ring = polygon.InteriorRings[i].CoordinateSequence;
                	if (Algorithm.CGAlgorithms.IsCCW(ring.ToCoordinateArray())) {
                        ring = reverse(polygon.Factory.CoordinateSequenceFactory, ring); 
                    }
                    
                    AddCoordinates(list,ring);
                }
                return;
            }
            break; // interpretations 2,4 not supported
        case SdoGTemplate.MultiPoint:
        case SdoGTemplate.MultiLine:
        case SdoGTemplate.MultiPolygon:
        case SdoGTemplate.Collection:
            for (int i = 0; i < geom.NumGeometries; i++) {
                coordinates(list,geom.GetGeometryN(i));
            }
            return;
        }

        throw new ArgumentException("Cannot encode JTS "
            + geom.GeometryType + " as "
            + "SDO_ORDINATRES (Limitied to Point, Line, Polygon, "
            + "GeometryCollection, MultiPoint, MultiLineString and MultiPolygon)");
    }

    /**
     * Adds a double array to list.
     * 
     * <p>
     * The double array will contain all the ordinates in the Coordiante
     * sequence.
     * </p>
     *
     * @param list
     * @param sequence
     */
    private static void AddCoordinates(List<double[]> list, ICoordinateSequence sequence)
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            Coordinate coord = sequence.GetCoordinate(i);
            list.Add(Double.IsNaN(coord.Z) 
                ? new[] {coord.X, coord.Y} 
                : new[] {coord.X, coord.Y, coord.Z});
        }
    }

    /**
     * Return SDO_ELEM_INFO array for geometry
     * 
     * <pre><code><b>
     * # Name                Meaning</b>
     * 0 SDO_STARTING_OFFSET Offsets start at one
     * 1 SDO_ETYPE           Describes how ordinates are ordered
     * 2 SDO_INTERPRETATION  SDO_ETYPE: 4, 1005, or 2005
     *                       Number of triplets involved in compound geometry
     *                       
     *                       SDO_ETYPE: 1, 2, 1003, or 2003
     *                       Describes ordering of ordinates in geometry  
     * </code></pre>
     * 
     * <p>
     * For compound elements (SDO_ETYPE values 4 and 5) the last element of one
     * is the first element of the next.
     * </p>
     *
     * @param geom Geometry being represented
     *
     * @return Descriptionof Ordinates representation
     */
	private int[] ElemInfo(IGeometry geom, SdoGTemplate gtype) {
		var list = new LinkedList<int>();

        ElemInfo(list, geom, 1, gtype);
        
        int[] array = new int[list.Count];
        int offset = 0;

	    foreach (var i in list)
	    {
            array[offset++] = i;
        }

        return array;
    }
	
    /**
     * Add to SDO_ELEM_INFO list for geometry and GTYPE.
     *
     * @param elemInfoList List used to gather SDO_ELEM_INFO
     * @param geom Geometry to encode
     * @param sOffSet Starting offset in SDO_ORDINATES
     *
     * @throws IllegalArgumentException If geom cannot be encoded by ElemInfo
     */
    private void ElemInfo(LinkedList<int> elemInfoList, IGeometry geom, int sOffSet, SdoGTemplate gtype) {

        switch (gtype - ((int)gtype/100) * 100) { // removes right two digits
        case SdoGTemplate.Coordinate:
            addInt(elemInfoList, sOffSet);
            addInt(elemInfoList, (int)SdoEType.Coordinate);
            addInt(elemInfoList, 1); // INTERPRETATION single point

            return;

        case SdoGTemplate.MultiPoint:
            var points = (IMultiPoint) geom;

            addInt(elemInfoList, sOffSet);
            addInt(elemInfoList, (int)SdoEType.Coordinate);
            addInt(elemInfoList, ElemInfoInterpretation(points, SdoEType.Coordinate));

            return;

        case SdoGTemplate.Line:
            addInt(elemInfoList, sOffSet);
            addInt(elemInfoList, (int)SdoEType.Line);
            addInt(elemInfoList, 1); // INTERPRETATION straight edges    

            return;

        case SdoGTemplate.MultiLine:
        	var lines = (IMultiLineString) geom;
            ILineString line;
            int offset = sOffSet;
            int dim = (int)gtype/1000;
            int len = dim + ((int)gtype-dim*1000)/100;

            for (int i = 0; i < lines.NumGeometries; i++) {
                line = (ILineString) lines.GetGeometryN(i);
                addInt(elemInfoList, offset);
                addInt(elemInfoList, (int)SdoEType.Line);
                addInt(elemInfoList, 1); // INTERPRETATION straight edges  
                offset += (line.NumPoints * len);
            }

            return;

        case SdoGTemplate.Polygon:
        	var polygon = (IPolygon)geom;
            int holes = polygon.NumInteriorRings;

            if (holes == 0) {
                addInt(elemInfoList, sOffSet);
                addInt(elemInfoList, (int)ElemInfoEType(polygon));
                addInt(elemInfoList, ElemInfoInterpretation(polygon, SdoEType.PolygonExterior));
                return;
            }

            dim = (int)gtype/1000;
            len = dim + ((int)gtype-dim*1000)/100;
            offset = sOffSet;
            ILineString ring;

            ring = polygon.ExteriorRing;
            addInt(elemInfoList, offset);
            addInt(elemInfoList, (int)ElemInfoEType(polygon));
            addInt(elemInfoList, ElemInfoInterpretation(polygon, SdoEType.PolygonExterior));
            offset += (ring.NumPoints * len);

            for (int i = 1; i <= holes; i++) {
                ring = polygon.GetInteriorRingN(i - 1);
                addInt(elemInfoList, offset);
                addInt(elemInfoList, (int)SdoEType.PolygonInterior);
                addInt(elemInfoList, ElemInfoInterpretation(ring, SdoEType.PolygonInterior));
                offset += (ring.NumPoints * len);
            }

            return;

        case SdoGTemplate.MultiPolygon:
        	var polys = (IMultiPolygon) geom;
            IPolygon poly;
            offset = sOffSet;

            dim = (int)gtype/1000;
            len = dim + ((int)gtype-dim*1000)/100;

            for (int i = 0; i < polys.NumGeometries; i++) {
                poly = (IPolygon) polys.GetGeometryN(i);
                ElemInfo(elemInfoList, poly, offset, GType(poly));
                if( IsRectangle( poly )){
                    offset += (2 * len);                
                }
                else {
                    offset += (poly.NumPoints * len);                
                }            
            }

            return;

        case SdoGTemplate.Collection:
        	var geoms = (IGeometryCollection) geom;
            offset = sOffSet;
            dim = (int)gtype/1000;
            len = dim + ((int)gtype-dim*1000)/100;

            for (int i = 0; i < geoms.NumGeometries; i++) {
                geom = geoms.GetGeometryN(i);
                // MD  20/3/07 modified to provide gType of component geometry
                ElemInfo(elemInfoList, geom, offset, GType(geom));
                if( geom is IPolygon && IsRectangle( (IPolygon) geom )){
                    offset += (2 * len);                
                }
                else {
                    offset += (geom.NumPoints * len);                
                }                        
            }

            return;
        }

        throw new ArgumentException("Cannot encode JTS "
            + geom.GeometryType + " as SDO_ELEM_INFO "
            + "(Limitied to Point, Line, Polygon, GeometryCollection, MultiPoint,"
            + " MultiLineString and MultiPolygon)");
    }

    private void addInt(LinkedList<int> list, int i) {
        list.AddLast(i);
    }

    /**
     * We need to check if a <code>polygon</code> a rectangle so we can produce
     * the correct encoding.
     * 
     * Rectangles are only supported without a SRID!
     *
     * @param polygon
     *
     * @return <code>true</code> if polygon is SRID==0 and a rectangle
     */
    private bool IsRectangle(IPolygon polygon) {
        if (polygon.Factory.SRID != SridNull) {
            // Rectangles only valid in CAD applications
            // that do not have an SRID system
            //
            return false;
        }

        if (Lrs(polygon) != 0) {
            // cannot support LRS on a rectangle
            return false;
        }

        var coords = polygon.Coordinates;

        if (coords.Length != 5) {
            return false;
        }

        if ((coords[0] == null) || (coords[1] == null) || (coords[2] == null)
                || (coords[3] == null)) {
            return false;
        }

        if (!coords[0].Equals2D(coords[4])) {
            return false;
        }

        double x1 = coords[0].X;
        double y1 = coords[0].Y;
        double x2 = coords[1].X;
        double y2 = coords[1].Y;
        double x3 = coords[2].X;
        double y3 = coords[2].Y;
        double x4 = coords[3].X;
        double y4 = coords[3].Y;

        if ((x1 == x4) && (y1 == y2) && (x3 == x2) && (y3 == y4)) {
            // 1+-----+2
            //  |     |
            // 4+-----+3
            return true;
        }

        if ((x1 == x2) && (y1 == y4) && (x3 == x4) && (y3 == y2)) {
            // 2+-----+3
            //  |     |
            // 1+-----+4
            return true;
        }

        return false;
    }
    /**
     * Produce <code>SDO_ETYPE</code> for geometry description as stored in the
     * <code>SDO_ELEM_INFO</code>.
     * 
     * <p>
     * Describes how Ordinates are ordered:
     * </p>
     * <pre><code><b>
     * Value Elements Meaning</b>
     *    0           Custom Geometry (like spline) 
     *    1  simple   Point (or Points)
     *    2  simple   Line (or Lines)
     *    3           polygon ring of unknown order (discouraged update to 1003 or 2003)
     * 1003  simple   polygon ring (1 exterior counterclockwise order)
     * 2003  simple   polygon ring (2 interior clockwise order)
     *    4  compound series defines a linestring
     *    5  compound series defines a polygon ring of unknown order (discouraged)
     * 1005  compound series defines exterior polygon ring (counterclockwise order)
     * 2005  compound series defines interior polygon ring (clockwise order)
     * </code></pre>
     * 
     * @param geom Geometry being represented
     *
     * @return Descriptionof Ordinates representation
     *
     * @throws IllegalArgumentException
     */
    private SdoEType ElemInfoEType(IGeometry geom) {
        switch (Template(geom)) {

        case SdoGTemplate.Coordinate:
            return SdoEType.Coordinate;

        case SdoGTemplate.Line:
            return SdoEType.Line;

        case SdoGTemplate.Polygon:
        	// jts convention
            return SdoEType.PolygonExterior; // cc order

        default:

            // should never happen!
            throw new ArgumentException("Unknown encoding of SDO_GTEMPLATE");
        }
    }
    
    /**
     * Allows specification of <code>INTERPRETATION</code> used to interpret
     * <code>geom</code>.
     * 
     * @param geom Geometry to encode
     * @param etype ETYPE value requiring an INTERPREATION
     *
     * @return INTERPRETATION ELEM_INFO entry for geom given etype
     *
     * @throws IllegalArgumentException If asked to encode a curve
     */
    private int ElemInfoInterpretation(IGeometry geom, SdoEType etype) {
        switch (etype) {

        case SdoEType.Coordinate:

            if (geom is IPoint) {
                return 1;
            }

            if (geom is IMultiPoint) {
                return geom.NumGeometries;
            }

            break;

        case SdoEType.Line:
        	// always straight for jts
            return 1;

        case SdoEType.Polygon:
        case SdoEType.PolygonExterior:
        case SdoEType.PolygonInterior:

            if (geom is IPolygon) {
                var polygon = (IPolygon) geom;
            	// always straight for jts
                if (IsRectangle(polygon)) {
                    return 3;
                }
            }

            return 1;
        }

        throw new ArgumentException("Cannot encode JTS "
            + geom.GeometryType + " as "
            + "SDO_INTERPRETATION (Limitied to Point, Line, Polygon, "
            + "GeometryCollection, MultiPoint, MultiLineString and MultiPolygon)");
    }
	
    /**
     * Return SDO_POINT_TYPE for geometry
     * 
     * Will return non null for Point objects. <code>null</code> is returned
     * for all non point objects.

     * You cannot use this with LRS Coordiantes
     * Subclasses may wish to repress this method and force Points to be
     * represented using SDO_ORDINATES.
     *
     * @param geom
     *
     * @return double[]
     */
	private double[] Point(IGeometry geom) {
        if (geom is IPoint && (Lrs(geom) == 0)) {
            var point = (IPoint) geom;
            var coord = point.Coordinate;

            return new[] { coord.X, coord.Y, coord.Z };
        }

        // SDO_POINT_TYPE only used for non LRS Points
        return null;
    }

    /**
     * Produce SDO_GTEMPLATE representing provided Geometry.
     * 
     * <p>
     * Encoding of Geometry type and dimension.
     * </p>
     * 
     * <p>
     * SDO_GTEMPLATE defined as for digits <code>[d][l][tt]</code>:
     * </p>
     * 
     * @param geom
     *
     * @return SDO_GTEMPLATE
     */
	private SdoGTemplate GType(IGeometry geom) {
        int d = (int)Dimension(geom) * 1000;
        int l = Lrs(geom) * 100;
        int tt = (int)Template(geom);

        return (SdoGTemplate)(d + l + tt);
    }

    /**
     * Return dimensions as defined by SDO_GTEMPLATE (either 2,3 or 4).
     * 
     *
     * @param geom
     *
     * @return num dimensions
     */
    private int Dimension(IGeometry geom) {
    	var d = Double.IsNaN(geom.Coordinate.Z)?2:3;
		return (d<_dimension?d:_dimension);
    }

    /**
     * Return LRS as defined by SDO_GTEMPLATE (either 3,4 or 0).
     * 
     * @param geom
     *
     * @return <code>0</code>
     */
    private static int Lrs(IGeometry geom) {
        // when measures are supported this may change
    	// until then ... 
    	return 0;
    }
    
    /**
     * Return TT as defined by SDO_GTEMPLATE (represents geometry type).
     * 
     * @see Constants.SDO_GTEMPLATE
     *
     * @param geom
     *
     * @return template code
     */
    private SdoGTemplate Template(IGeometry geom) {
        if (geom == null) {
            return SdoGTemplate.Unknown; // UNKNOWN
        }
        if (geom is IPoint) {
            return SdoGTemplate.Coordinate;
        }
        if (geom is ILineString) {
            return SdoGTemplate.Line;
        }
        if (geom is IPolygon) {
            return SdoGTemplate.Polygon;
        }
        if (geom is IMultiPoint) {
            return SdoGTemplate.MultiPoint;
        }
        if (geom is IMultiLineString) {
            return SdoGTemplate.MultiLine;
        }
        if (geom is IMultiPolygon) {
            return SdoGTemplate.MultiPolygon;
        }
        if (geom is IGeometryCollection) {
            return SdoGTemplate.Collection;
        }

        throw new ArgumentException("Cannot encode JTS "
            + geom.GeometryType + " as SDO_GTEMPLATE "
            + "(Limitied to Point, Line, Polygon, GeometryCollection, MultiPoint,"
            + " MultiLineString and MultiPolygon)");
    }
	
    ///** Convience method for STRUCT construction. */
    //private STRUCT toSTRUCT( Datum attributes[], String dataType )
    //        throws SQLException
    //{
    //    if( dataType.startsWith("*.")){
    //        dataType = "DRA."+dataType.substring(2);//TODO here
    //    }
    //    StructDescriptor descriptor =
    //        StructDescriptor.createDescriptor( dataType, connection );
    
    //     return new STRUCT( descriptor, connection, attributes );
    //}
    
    ///** 
    // * Convience method for ARRAY construction.
    // * <p>
    // * Compare and contrast with toORDINATE - which treats <code>Double.NaN</code>
    // * as<code>NULL</code></p>
    // */
    //private ARRAY toARRAY( double doubles[], String dataType )
    //        throws SQLException
    //{
    //    ArrayDescriptor descriptor =
    //        ArrayDescriptor.createDescriptor( dataType, connection );
        
    //     return new ARRAY( descriptor, connection, doubles );
    //}
    
    ///** 
    // * Convience method for ARRAY construction.
    // */
    //private ARRAY toARRAY( int ints[], String dataType )
    //    throws SQLException
    //{
    //    ArrayDescriptor descriptor =
    //        ArrayDescriptor.createDescriptor( dataType, connection );
            
    //     return new ARRAY( descriptor, connection, ints );
    //}

    ///** 
    // * Convience method for NUMBER construction.
    // * <p>
    // * Double.NaN is represented as <code>NULL</code> to agree
    // * with JTS use.</p>
    // */
    //private NUMBER toNUMBER( double number ) throws SQLException{
    //    if( Double.isNaN( number )){
    //        return null;
    //    }
    //    return new NUMBER( number );
    //}

    /**
     * reverses the coordinate order
     *
     * @param factory
     * @param sequence
     *
     * @return CoordinateSequence reversed sequence
     */
    private ICoordinateSequence reverse(ICoordinateSequenceFactory factory, ICoordinateSequence sequence) 
    {
    	var list = new CoordinateList(sequence.ToCoordinateArray());
        list.Reverse();
        return factory.Create(list.ToCoordinateArray());
    }

	/**
	 * @param dimension The dimension to set.
	 */
	public void setDimension(int dimension)
    {
		_dimension = dimension;}
	}
}
        
    
