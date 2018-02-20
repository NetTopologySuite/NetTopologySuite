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
        private const int NullDimension = -1;
        private const int SridNull = -1;

        public OracleGeometryReader()
            :this(GeometryFactory.Default)
        {}

        public OracleGeometryReader(IGeometryFactory factory)
        {
            _factory = factory;
        }

        private readonly IGeometryFactory _factory;

        private int _dimension = -1;

        public int Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }


        public IGeometry Read(SdoGeometry geom)
        {

            //Note: Returning null for null Datum
            if (geom == null)
                return null;
            
            Debug.Assert(geom.SdoGtype.HasValue);
            var gType = (int) geom.SdoGtype;
            
            Debug.Assert(geom.Sdo_Srid.HasValue);
            var srid = (int)geom.Sdo_Srid;
            
            var point = geom.Point;
            
            var retVal = Create(gType, point, geom.ElemArray, geom.OrdinatesArray);
            retVal.SRID = srid;
            
            return retVal;
        }

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

                    if (Algorithm.OrientationFunctions.IsCCW(ring.CoordinateSequence))
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


        private static SdoEType EType(Decimal[] elemInfo, int tripletIndex)
        {
            if (((tripletIndex*3) + 1) >= elemInfo.Length)
            {
                return SdoEType.Unknown;
            }
        
            return (SdoEType) elemInfo[(tripletIndex*3) + 1];
        }


        private static int Interpretation(Decimal[] elemInfo, int tripletIndex)
        {
            if (((tripletIndex*3) + 2) >= elemInfo.Length)
            {
                return -1;
            }
            
            return (Int32) elemInfo[(tripletIndex*3) + 2];
        }

    
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
