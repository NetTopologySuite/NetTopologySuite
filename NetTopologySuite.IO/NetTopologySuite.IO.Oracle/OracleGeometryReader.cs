using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Sdo;
using NetTopologySuite.IO.UdtBase;

namespace NetTopologySuite.IO
{
    public class OracleGeometryReader
    {
        /*
             * The JTS Topology Suite is a collection of Java classes that
             * implement the fundamental operations required to validate a given
             * geo-spatial data set to a known topological specification.
             *
             * Copyright (C) 2001 Vivid Solutions
             *
             * This library is free software; you can redistribute it and/or
             * modify it under the terms of the GNU Lesser General Public
             * License as published by the Free Software Foundation; either
             * version 2.1 of the License, or (at your option) any later version.
             *
             * This library is distributed in the hope that it will be useful,
             * but WITHOUT ANY WARRANTY; without even the implied warranty of
             * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
             * Lesser General Public License for more details.
             *
             * You should have received a copy of the GNU Lesser General Public
             * License along with this library; if not, write to the Free Software
             * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
             *
             * For more information, contact:
             *
             *     Vivid Solutions
             *     Suite #1A
             *     2328 Government Street
             *     Victoria BC  V8T 5G5
             *     Canada
             *
             *     (250)385-6040
             *     www.vividsolutions.com
             */
        /*
             *    Geotools2 - OpenSource mapping toolkit
             *    http://geotools.org
             *    (C) 2003, Geotools Project Managment Committee (PMC)
             *
             *    This library is free software; you can redistribute it and/or
             *    modify it under the terms of the GNU Lesser General Public
             *    License as published by the Free Software Foundation;
             *    version 2.1 of the License.
             *
             *    This library is distributed in the hope that it will be useful,
             *    but WITHOUT ANY WARRANTY; without even the implied warranty of
             *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
             *    Lesser General Public License for more details.
             *
             */

        /**
             * Reads a {@link Geometry} from an Oracle <tt>MDSYS.GEOMETRY</tt> object.
             *
             * A {@link GeometryFactory} may be provided, otherwise
             * a default one will be used.
             * The provided GeometryFactory will be used, with the exception of the SRID field.
             * This will be extracted from the Geometry.
             * <p>
             * If a {@link PrecisionModel} is supplied it is the callers's responsibility
             * to ensure that it matches the precision of the incoming data.
             * If a lower precision for the data is required, a subsequent
             * process must be run on the data to reduce its precision.
             * <p>
             * To use this class a suitable Oracle JDBC driver JAR must be present.
             * 
             * @version 9i
             * @author David Zwiers, Vivid Solutions.
             * @author Martin Davis
             */

        private const int NullDimension = -1;
        private const int SridNull = -1;

        ///<summary>Creates a new reader</summary>
        //public OraReader()
        //{
        //}

        public OracleGeometryReader()
            :this(GeometryFactory.Default)
        {}

        public OracleGeometryReader(IGeometryFactory factory)
        {
            _factory = factory;
        }

        private readonly IGeometryFactory _factory;

        private int _dimension = -1;

        ///<summary>Gets/sets the number of coordinate dimensions which will be read.</summary>
        public int Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }


        /**
             * This method will attempt to create a JTS Geometry for the MDSYS.GEOMETRY
             * provided. The Type of gemetry returned will depend on the input datum,
             * where the Geometry type is specified within the STRUCT.
             *
             * @param struct The MDSYS.GEOMETRY Object to decode
             * @return A JTS Geometry if one could be created, null otherwise
             * @throws SQLException When a read error occured within the struct
             */

        public IGeometry Read(SdoGeometry geom)
        {

            //Note: Returning null for null Datum
            if (geom == null)
                return null;
            
            Debug.Assert(geom.Sdo_Gtype.HasValue);
            var gType = (int) geom.Sdo_Gtype;
            
            Debug.Assert(geom.Sdo_Srid.HasValue);
            var srid = (int)geom.Sdo_Srid;
            
            var point = geom.Point;
            
            var retVal = Create(gType, point, geom.ElemArray, geom.OrdinatesArray);
            retVal.SRID = srid;
            
            return retVal;
        }

        /**
             * Decode geometry from provided SDO encoded information.
             *
             * <p></p>
             *
             * @param gf Used to construct returned Geometry
             * @param gType SDO_GTEMPLATE represents dimension, LRS, and geometry type
             * @param point
             * @param elemInfo
             * @param ordinates
             *
             * @return Geometry as encoded
             */

        private IGeometry Create(int gType, SdoPoint point, Decimal[] elemInfo, Decimal[] ordinates)
        {
            int lrs = (gType%1000)/100;

            // find the dimension: represented by the smaller of the two dimensions
            int dim;
            if (_dimension != NullDimension)
            {
                dim = _dimension;
            }
            else
            {
                dim = Math.Min(gType/1000, 3);
            }

            if (dim == 0)
                return null;

            if (dim < 2)
            {
                throw new ArgumentException("Dimension D:" + dim + " is not valid for JTS. " +
                                            "Either specify a dimension or use Oracle Locator Version 9i or later");
            }

            // extract the geometry template type
            // this is represented as the rightmost two digits
            int geomTemplate = gType - (dim*1000) - (lrs*100);

            //CoordinateSequence coords = null;
            List<Coordinate> coords;

            if (lrs == 0 && geomTemplate == 1 && point != null && elemInfo == null)
            {
                // Single Coordinate Type Optimization
                Debug.Assert(point.X != null, "point.X != null");
                Debug.Assert(point.Y != null, "point.Y != null");
                if (dim == 2)
                {
                    coords = Coordinates(dim, lrs, geomTemplate, new[] { point.X.Value, point.Y.Value });
                }
                else
                {
                    Debug.Assert(point.Z != null, "point.Z != null");
                    coords = Coordinates(dim, lrs, geomTemplate,
                                         new[] {point.X.Value, point.Y.Value, point.Z.Value});
                }
                elemInfo = new Decimal[] {1, (Int32) SdoEType.Coordinate, 1};
            }
            else
            {
                coords = Coordinates(dim, lrs, geomTemplate, ordinates);
            }

            switch ((SdoGTemplate) geomTemplate)
            {
                case SdoGTemplate.Coordinate:
                    return CreatePoint(dim, lrs, elemInfo, 0, coords);

                case SdoGTemplate.Line:
                    return CreateLine(dim, lrs, elemInfo, 0, coords);

                case SdoGTemplate.Polygon:
                    return CreatePolygon(dim, lrs, elemInfo, 0, coords);

                case SdoGTemplate.MultiPoint:
                    return CreateMultiPoint(dim, lrs, elemInfo, 0, coords);

                case SdoGTemplate.MultiLine:
                    return CreateMultiLine(dim, lrs, elemInfo, 0, coords, -1);

                case SdoGTemplate.MultiPolygon:
                    return CreateMultiPolygon(dim, lrs, elemInfo, 0, coords, -1);

                case SdoGTemplate.Collection:
                    return CreateCollection(dim, lrs, elemInfo, 0, coords, -1);

                default:
                    return null;
            }
        }

        /**
             * Construct CoordinateList as described by GTYPE.
             *
             * The number of ordinates per coordinate are taken to be lrs+dim, and the
             * number of ordinates should be a multiple of this value.

             * In the Special case of GTYPE 2001 and a three ordinates are interpreted
             * as a single Coordinate rather than an error.
             *
             * @param f CoordinateSequenceFactory used to encode ordiantes for JTS
             * @param ordinates
             *
             * @return protected
             *
             * @throws IllegalArgumentException
             */

        private static List<Coordinate> Coordinates(int dim, int lrs, int gtemplate, Decimal[] ordinates)
        {
            if ((ordinates == null) || (ordinates.Length == 0))
            {
                return new List<Coordinate>();
            }

            //
            // POINT_TYPE Special Case
            //
            if ((dim == 2) && (lrs == 0) && (gtemplate == 01) && (ordinates.Length == 3))
            {
                var pt = new List<Coordinate>(1)
                             {
                                 new Coordinate((Double) ordinates[0], (Double) ordinates[1],
                                                (Double) ordinates[2])
                             };
                return pt;
            }

            int len = dim + lrs;

            if ((len == 0 && ordinates.Length != 0) || (len != 0 && ((ordinates.Length%len) != 0)))
            {
                throw new ArgumentException("Dimension D:" + dim + " and L:" +
                                         lrs + " denote Coordinates " + "of " + len +
                                         " ordinates. This cannot be resolved with" +
                                         "an ordinate array of length " + ordinates.Length);
            }

            int length = (len == 0 ? 0 : ordinates.Length/len);

            // we would have to ask for a dimension which represents all the requested
            // dimension and measures from a mask array in the future
           var pts = new List<Coordinate>(length);

            for (int i = 0; i < length; i++)
            {
                int offset = i*len;
                switch (len)
                {
                    case 2:
                        pts.Add(new Coordinate((Double) ordinates[offset], (Double) ordinates[offset + 1], Double.NaN));
                        break;
                    case 3:
                        pts.Add(new Coordinate((Double) ordinates[offset], (Double) ordinates[offset + 1],
                                               (Double) ordinates[offset + 2]));
                        break;
                }

                //// in the future change this condition to include ignored dimensions from mask array
                //for (; j < actualDim && j < dim; j++)
                //{
                //    cs.setOrdinate(i, j, ordinates[i * len + j]);
                //    // may not always want to inc. j when we have a mask array
                //}
                ////// in the future change this condition to include ignored dimensions from mask array
                ////for (int d = j; j < actualDim && (j - d) < lrs; j++)
                ////{
                ////    cs.setOrdinate(i, j, ordinates[i * len + j]);
                ////    // may not always want to inc. j when we have a mask array
                ////}
            }
            return pts;
        }

        /**
             * Create MultiGeometry as encoded by elemInfo.
             *
             * @param gf Used to construct MultiLineString
             * @param elemInfo Interpretation of coords
             * @param elemIndex Triplet in elemInfo to process as a Polygon
             * @param coords Coordinates to interpret using elemInfo
             * @param numGeom Number of triplets (or -1 for rest)
             *
             * @return GeometryCollection
             *
             * @throws IllegalArgumentException DWhen faced with an encoding error
             */

        private IGeometryCollection CreateCollection(int dim, int lrs, Decimal[] elemInfo, int elemIndex,
                                                    List<Coordinate> coords, int numGeom)
        {

            int sOffset = StartingOffset(elemInfo, elemIndex);

            int length = coords.Count*dim;

            if (!(sOffset <= length))
                throw new ArgumentException("ELEM_INFO STARTING_OFFSET " + sOffset +
                                            " inconsistent with ORDINATES length " + coords.Count);

            int endTriplet = (numGeom != -1) ? elemIndex + numGeom : elemInfo.Length/3 + 1;

            var list = new List<IGeometry>();
            SdoEType etype;
            int interpretation;
            IGeometry geom = null;

            Boolean cont = true;
            for (int i = elemIndex; cont && i < endTriplet; i++)
            {
                etype = EType(elemInfo, i);
                interpretation = Interpretation(elemInfo, i);

                switch (etype)
                {

                    case SdoEType.Unknown:
                        cont = false;
                        break;
                    case SdoEType.Coordinate:

                        if (interpretation == 1)
                        {
                            geom = CreatePoint(dim, lrs, elemInfo, i, coords);
                        }
                        else if (interpretation > 1)
                        {
                            geom = CreateMultiPoint(dim, lrs, elemInfo, i, coords);
                        }
                        else
                        {
                            throw new ArgumentException(
                                "ETYPE.POINT requires INTERPRETATION >= 1");
                        }

                        break;

                    case SdoEType.Line:
                        geom = CreateLine(dim, lrs, elemInfo, i, coords);

                        break;

                    case SdoEType.Polygon:
                    case SdoEType.PolygonExterior:
                        geom = CreatePolygon(dim, lrs, elemInfo, i, coords);
                        i += ((Polygon) geom).NumInteriorRings;

                        break;

                    case SdoEType.PolygonInterior:
                        throw new ArgumentException(
                            "ETYPE 2003 (Polygon Interior) no expected in a GeometryCollection" +
                         "(2003 is used to represent polygon holes, in a 1003 polygon exterior)");

                    default:
                        throw new ArgumentException("ETYPE " + etype +
                                                 " not representable as a JTS Geometry." +
                                                 "(Custom and Compound Straight and Curved Geometries not supported)");
                }

                list.Add(geom);
            }

            var geoms = _factory.CreateGeometryCollection(list.ToArray());

            return geoms;
        }

        /**
             * Create MultiPolygon as encoded by elemInfo.
             *
             *
             * @param gf Used to construct MultiLineString
             * @param elemInfo Interpretation of coords
             * @param elemIndex Triplet in elemInfo to process as a Polygon
             * @param coords Coordinates to interpret using elemInfo
             * @param numGeom Number of triplets (or -1 for rest)
             *
             * @return MultiPolygon
             */

        private IMultiPolygon CreateMultiPolygon(int dim, int lrs, decimal[] elemInfo, int elemIndex,
                                                List<Coordinate> coords, int numGeom)
        {

            int sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);

            int length = coords.Count*dim;

            if (!(sOffset >= 1) || !(sOffset <= length))
                throw new ArgumentException("ELEM_INFO STARTING_OFFSET " + sOffset +
                                            " inconsistent with ORDINATES length " + coords.Count);
            
            if (etype != SdoEType.Polygon && etype != SdoEType.PolygonExterior)
                throw new ArgumentException("ETYPE " + etype + " inconsistent with expected POLYGON or POLYGON_EXTERIOR");
            
            if (interpretation != 1 && interpretation != 3)
            {
                return null;
            }

            int endTriplet = (numGeom != -1) ? elemIndex + numGeom : (elemInfo.Length/3) + 1;

            var list = new List<IPolygon>();
            Boolean cont = true;

            for (int i = elemIndex; cont && i < endTriplet && (etype = EType(elemInfo, i)) != SdoEType.Unknown; i++)
            {
                if ((etype == SdoEType.Polygon) || (etype == SdoEType.PolygonExterior))
                {
                    var poly = CreatePolygon(dim, lrs, elemInfo, i, coords);
                    i += poly.NumInteriorRings; // skip interior rings
                    list.Add(poly);
                }
                else
                {
                    // not a Polygon - get out here
                    cont = false;
                }
            }

            var polys = _factory.CreateMultiPolygon(list.ToArray());

            return polys;
        }

        /**
             * Create MultiLineString as encoded by elemInfo.
             *
             *
             * @param gf Used to construct MultiLineString
             * @param elemInfo Interpretation of coords
             * @param elemIndex Triplet in elemInfo to process as a Polygon
             * @param coords Coordinates to interpret using elemInfo
             * @param numGeom Number of triplets (or -1 for rest)
             *
             * @return MultiLineString
             */

        private IMultiLineString CreateMultiLine(int dim, int lrs, Decimal[] elemInfo, int elemIndex,
                                                List<Coordinate> coords, int numGeom)
        {

            int sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);

            int length = coords.Count*dim;

            if (!(sOffset >= 1) || !(sOffset <= length))
                throw new ArgumentException("ELEM_INFO STARTING_OFFSET " + sOffset +
                                            " inconsistent with ORDINATES length " + coords.Count);
            if (!(etype == SdoEType.Line))
                throw new ArgumentException("ETYPE " + etype + " inconsistent with expected LINE");
            if (!(interpretation == 1))
            {
                // we cannot represent INTERPRETATION > 1
                return null;
            }

            int endTriplet = (numGeom != -1) ? (elemIndex + numGeom) : (elemInfo.Length/3);

            var list = new List<ILineString>();

            Boolean cont = true;
            for (int i = elemIndex; cont && i < endTriplet && (etype = EType(elemInfo, i)) != SdoEType.Unknown; i++)
            {
                if (etype == SdoEType.Line)
                {
                    list.Add(CreateLine(dim, lrs, elemInfo, i, coords));
                }
                else
                {
                    // not a LineString - get out of here
                    cont = false;
                }
            }

            var lines = _factory.CreateMultiLineString(list.ToArray());

            return lines;
        }

        /**
             * Create MultiPoint as encoded by elemInfo.
             *
             *
             * @param gf Used to construct polygon
             * @param elemInfo Interpretation of coords
             * @param elemIndex Triplet in elemInfo to process as a Polygon
             * @param coords Coordinates to interpret using elemInfo
             *
             * @return MultiPoint
             */

        private IMultiPoint CreateMultiPoint(int dim, int lrs, Decimal[] elemInfo, int elemIndex, List<Coordinate> coords)
        {
            int sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);

            if (!(sOffset >= 1) || !(sOffset <= coords.Count))
                throw new ArgumentException("ELEM_INFO STARTING_OFFSET " + sOffset +
                                            " inconsistent with ORDINATES length " + coords.Count);
            if (etype != SdoEType.Coordinate)
                throw new ArgumentException("ETYPE " + etype + " inconsistent with expected POINT");
            if (!(interpretation > 1))
            {
                return null;
            }

            int len = dim + lrs;

            int start = (sOffset - 1)/len;
            int end = start + interpretation;

            var points = _factory.CreateMultiPoint(SubArray(coords, start, end));

            return points;
        }

        /**
             * Create Polygon as encoded.
             *
             * @see #interpretation(int[], int)
             *
             * @param gf Used to construct polygon
             * @param elemInfo Interpretation of coords
             * @param elemIndex Triplet in elemInfo to process as a Polygon
             * @param coords Coordinates to interpret using elemInfo
             *
             * @return Polygon as encoded by elemInfo, or null when faced with and
             *         encoding that can not be captured by JTS
             * @throws IllegalArgumentException When faced with an invalid SDO encoding
             */

        private IPolygon CreatePolygon(int dim, int lrs, Decimal[] elemInfo, int elemIndex, List<Coordinate> coords)
        {

            int sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);

            if (!(1 <= sOffset && sOffset <= (coords.Count*dim)))
            {
                throw new ArgumentException(
                    "ELEM_INFO STARTING_OFFSET " + sOffset +
                    "inconsistent with COORDINATES length " + (coords.Count*dim));
            }

            if (etype != SdoEType.Polygon && etype != SdoEType.PolygonExterior)
            {
                throw new ArgumentException("ETYPE " + etype + " inconsistent with expected POLYGON or POLYGON_EXTERIOR");
            }
            if (interpretation != 1 && interpretation != 3)
            {
                return null;
            }

            var exteriorRing = CreateLinearRing(dim, lrs, elemInfo, elemIndex, coords);

            var rings = new List<ILinearRing>();

            Boolean cont = true;
            for (int i = elemIndex + 1; cont && (etype = EType(elemInfo, i)) != SdoEType.Unknown; i++)
            {
                if (etype == SdoEType.PolygonInterior)
                {
                    rings.Add(CreateLinearRing(dim, lrs, elemInfo, i, coords));
                }
                else if (etype == SdoEType.Polygon)
                {
                    // need to test Clockwiseness of Ring to see if it is
                    // interior or not - (use POLYGON_INTERIOR to avoid pain)

                    var ring = CreateLinearRing(dim, lrs, elemInfo, i, coords);

                    if (Algorithm.CGAlgorithms.IsCCW(ring.Coordinates))
                    {
                        // it is an Interior Hole
                        rings.Add(ring);
                    }
                    else
                    {
                        // it is the next Polygon! - get out of here
                        cont = false;
                    }
                }
                else
                {
                    // not a LinearRing - get out of here
                    cont = false;
                }
            }
        
            var poly = _factory.CreatePolygon(exteriorRing, rings.ToArray());
        
            return poly;
        }


        /**
                                  * Create Linear Ring for exterior/interior polygon ELEM_INFO triplets.
                                  *
                                  * @param gf
                                  * @param elemInfo
                                  * @param elemIndex
                                  * @param coords
                                  *
                                  * @return LinearRing
                                  *
                                  * @throws IllegalArgumentException If circle, or curve is requested
                                  */

        private ILinearRing CreateLinearRing(int dim, int lrs, Decimal[] elemInfo, int elemIndex, List<Coordinate> coords)
        {
        
                int
            sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);
            int length = coords.Count*dim;
        
            if (!(sOffset <= length))
                throw new ArgumentException("ELEM_INFO STARTING_OFFSET " + sOffset +
                                            " inconsistent with ORDINATES length " + coords.Count);
            if (etype != SdoEType.Polygon && etype != SdoEType.PolygonExterior &&
                etype != SdoEType.PolygonInterior)
            {
                throw new ArgumentException("ETYPE " + etype +
                                            " inconsistent with expected POLYGON, POLYGON_EXTERIOR or POLYGON_INTERIOR");
            }
            if (interpretation != 1 && interpretation != 3)
            {
                return null;
            }
            ILinearRing ring;
        
            int len = (dim + lrs);
            int start = (sOffset - 1)/len;
            int eOffset = StartingOffset(elemInfo, elemIndex + 1); // -1 for end
            int end = (eOffset != -1) ? ((eOffset - 1)/len) : coords.Count;
        
            if (interpretation == 1)
            {
                ring = new LinearRing(ToPointArray(SubList(coords, start, end)));
            }
            else
            {
                // interpretation == 3
                // rectangle does not maintain measures
                List<Coordinate> pts = new List<Coordinate>(5);
                List<Coordinate> ptssrc = SubList(coords, start, end);
                Coordinate min = ptssrc[0];
                Coordinate max = ptssrc[1];
                pts.AddRange(new[]
                                 {
                                     min, new Coordinate(max.X, min.Y), max, new Coordinate(min.X, max.Y) , min
                                 });
            
                ring = _factory.CreateLinearRing(pts.ToArray());
            }
        
            return ring;
        }


        /**
                                  * Create LineString as encoded.
                                  *
                                  * @param gf
                                  * @param elemInfo
                                  * @param coords
                                  *
                                  * @return LineString
                                  *
                                  * @throws IllegalArgumentException If asked to create a curve
                                  */

        private ILineString CreateLine(int dim, int lrs, Decimal[] elemInfo, int elemIndex, List<Coordinate> coords)
        {
        
                int
            sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);
        
            if (etype != SdoEType.Line)
                return null;
        
            if (interpretation != 1)
            {
                throw new ArgumentException("ELEM_INFO INTERPRETAION " +
                                         interpretation + " not supported" + 
                                         "by JTS LineString.  Straight edges" + 
                                         "( ELEM_INFO INTERPRETAION 1) is supported");
            }
        
                int
            len = (dim + lrs);
            int start = (sOffset - 1)/len;
            int eOffset = StartingOffset(elemInfo, elemIndex + 1); // -1 for end
            int end = (eOffset != -1) ? ((eOffset - 1)/len) : coords.Count;
        
            
            var line = _factory.CreateLineString(ToPointArray(SubList(coords, start, end)));
        
            return line;
        }



        private static Coordinate[] ToPointArray(ICollection<Coordinate> input)
        {
            var pts = new List<Coordinate>(input.Count);
            foreach (Coordinate point in input)
                pts.Add(new Coordinate(point.X, point.Y, point.Y));

            return pts.ToArray();
        }

        /**
                                  * Create Coordinate as encoded.
                                  *
                                  * @param gf
                                  * @param dim The number of Dimensions
                                  * @param elemInfo
                                  * @param elemIndex
                                  * @param coords
                                  *
                                  * @return Coordinate
                                  */

        private IPoint CreatePoint(int dim, int lrs, Decimal[] elemInfo, int elemIndex, List<Coordinate> coords)
        {
            int sOffset = StartingOffset(elemInfo, elemIndex);
            SdoEType etype = EType(elemInfo, elemIndex);
            int interpretation = Interpretation(elemInfo, elemIndex);
        
            if (!(sOffset >= 1) || !(sOffset <= coords.Count))
                throw new ArgumentException("ELEM_INFO STARTING_OFFSET " + sOffset +
                                            " inconsistent with ORDINATES length " + coords.Count);
            if (etype != SdoEType.Coordinate)
                throw new ArgumentException("ETYPE " + etype + " inconsistent with expected POINT");
            if (interpretation != 1)
            {
                return null;
            }
        
            int len = (dim + lrs);
            int start = (sOffset - 1)/len;
            int eOffset = StartingOffset(elemInfo, elemIndex + 1); // -1 for end
        
            Coordinate point;
            if ((sOffset == 1) && (eOffset == -1))
            {
                // Use all Coordinates
                point = coords[0];
            }
            else
            {
                int end = (eOffset != -1) ? ((eOffset - 1)/len) : coords.Count;
                point = SubList(coords, start, end)[0];
            }
        
            return _factory.CreatePoint(point);
        }



    /**
                                  * Version of List.subList() that returns a CoordinateSequence.
                                  *
                                  * <p>
                                  * Returns from start (inclusive) to end (exlusive):
                                  * </p>
                                  *
                                  * @param factory Manages CoordinateSequences for JTS
                                  * @param coords coords to sublist
                                  * @param start starting offset
                                  * @param end upper bound of sublist
                                  *
                                  * @return CoordianteSequence
                                  */


        private static List<Coordinate> SubList(List<Coordinate> coords, int start, int end)
        {
            if ((start == 0) && (end == coords.Count))
            {
                return coords;
            }
        
            return coords.GetRange(start, (end - start));
        }



        private static Coordinate[] SubArray(List<Coordinate> coords, int start, int end)
        {
            return coords.GetRange(start, end - start).ToArray();
        }


        /**
                                  * ETYPE access for the elemInfo triplet indicated.
                                  * <p>
                                  * @see Constants.SDO_ETYPE for an indication of possible values
                                  *
                                  * @param elemInfo
                                  * @param tripletIndex
                                  * @return ETYPE for indicated triplet
                                  */

        private static SdoEType EType(Decimal[] elemInfo, int tripletIndex)
        {
            if (((tripletIndex*3) + 1) >= elemInfo.Length)
            {
                return SdoEType.Unknown;
            }
        
            return (SdoEType) elemInfo[(tripletIndex*3) + 1];
        }



        /**
                                  * Accesses the interpretation value for the current geometry
                                  *
                                  * JTS valid interpretation is: 1 for strait edges, 3 for rectangle
                                  *
                                  * Other interpretations include: 2 for arcs, 4 for circles
                                  *
                                  * mostly useful for polygons
                                  *
                                  * @param elemInfo
                                  * @param tripletIndex
                                  * @return Starting Offset for the ordinates of the geometry
                                  */

        private static int Interpretation(Decimal[] elemInfo, int tripletIndex)
        {
            if (((tripletIndex*3) + 2) >= elemInfo.Length)
            {
                return -1;
            }
            
            return (Int32) elemInfo[(tripletIndex*3) + 2];
        }

    
        /**
                                  * Accesses the starting index in the ordinate array for the current geometry
                                  *
                                  * mostly useful for polygons
                                  *
                                  * @param elemInfo
                                  * @param tripletIndex
                                  * @return Starting Offset for the ordinates of the geometry
                                  */

        private static Int32 StartingOffset(Decimal[] elemInfo, int tripletIndex)
        {
            if (((tripletIndex*3) + 0) >= elemInfo.Length)
            {
                return -1;
            }
        
            return (Int32) elemInfo[(tripletIndex*3) + 0];
        }
    }
}
