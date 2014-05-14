// Copyright 2011 - Felix Obermaier (ivv-aachen.de)
//
// This file is part of NetTopologySuite.IO.SpatiaLite
// NetTopologySuite.IO.SpatiaLite is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// NetTopologySuite.IO.SpatiaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NetTopologySuite.IO.SpatiaLite if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
using System;
using System.IO;
using GeoAPI;
using GeoAPI.Geometries;
using GeoAPI.IO;

//using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    public class GaiaGeoReader : IBinaryGeometryReader
    {
        private IGeometryFactory _factory;
        private readonly IPrecisionModel _precisionModel;
        private readonly ICoordinateSequenceFactory _coordinateSequenceFactory;
        private Ordinates _handleOrdinates;

        public GaiaGeoReader()
            : this(GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory, GeometryServiceProvider.Instance.DefaultPrecisionModel)
        { }

        public GaiaGeoReader(ICoordinateSequenceFactory coordinateSequenceFactory, IPrecisionModel precisionModel)
            : this(coordinateSequenceFactory, precisionModel, Ordinates.XYZM)
        {
        }

        public GaiaGeoReader(ICoordinateSequenceFactory coordinateSequenceFactory, IPrecisionModel precisionModel, Ordinates handleOrdinates)
        {
            _coordinateSequenceFactory = coordinateSequenceFactory;
            _precisionModel = precisionModel;
            _handleOrdinates = handleOrdinates;
        }

        public IGeometry Read(byte[] blob)
        {
            if (blob.Length < 45)
                return null;		/* cannot be an internal BLOB WKB geometry */
            if ((GaiaGeoBlobMark)blob[0] != GaiaGeoBlobMark.GAIA_MARK_START)
                return null;		/* failed to recognize START signature */
            var size = blob.Length;
            if ((GaiaGeoBlobMark)blob[size - 1] != GaiaGeoBlobMark.GAIA_MARK_END)
                return null;		/* failed to recognize END signature */
            if ((GaiaGeoBlobMark)blob[38] != GaiaGeoBlobMark.GAIA_MARK_MBR)
                return null;		/* failed to recognize MBR signature */

            var gaiaImport = SetGaiaGeoParseFunctions((GaiaGeoEndianMarker)blob[1], HandleOrdinates);
            if (gaiaImport == null)
                return null;

            //geo = gaiaAllocGeomColl();
            var offset = 2;
            var srid = gaiaImport.GetInt32(blob, ref offset);

            if (_factory == null || _factory.SRID != srid)
                _factory = GeometryServiceProvider.Instance.CreateGeometryFactory(_precisionModel, srid,
                                                                                  _coordinateSequenceFactory);
            var factory = _factory;

            //geo->endian_arch = (char)endian_arch;
            //geo->endian = (char)little_endian;
            //geo->blob = blob;
            //geo->size = size;
            //offset = 43;
            //switch ((GaiaGeoGeometry)type)
            //{
            //    /* setting up DimensionModel */
            //    case GaiaGeoGeometry.GAIA_POINTZ:
            //    case GaiaGeoGeometry.GAIA_LINESTRINGZ:
            //    case GaiaGeoGeometry.GAIA_POLYGONZ:
            //    case GaiaGeoGeometry.GAIA_MULTIPOINTZ:
            //    case GaiaGeoGeometry.GAIA_MULTILINESTRINGZ:
            //    case GaiaGeoGeometry.GAIA_MULTIPOLYGONZ:
            //    case GaiaGeoGeometry.GAIA_GEOMETRYCOLLECTIONZ:
            //    case GaiaGeoGeometry.GAIA_COMPRESSED_LINESTRINGZ:
            //    case GaiaGeoGeometry.GAIA_COMPRESSED_POLYGONZ:
            //        geo->DimensionModel = GAIA_XY_Z;
            //        break;
            //    case GaiaGeoGeometry.GAIA_POINTM:
            //    case GaiaGeoGeometry.GAIA_LINESTRINGM:
            //    case GaiaGeoGeometry.GAIA_POLYGONM:
            //    case GaiaGeoGeometry.GAIA_MULTIPOINTM:
            //    case GaiaGeoGeometry.GAIA_MULTILINESTRINGM:
            //    case GaiaGeoGeometry.GAIA_MULTIPOLYGONM:
            //    case GaiaGeoGeometry.GAIA_GEOMETRYCOLLECTIONM:
            //    case GaiaGeoGeometry.GAIA_COMPRESSED_LINESTRINGM:
            //    case GaiaGeoGeometry.GAIA_COMPRESSED_POLYGONM:
            //        geo->DimensionModel = GAIA_XY_M;
            //        break;
            //    case GaiaGeoGeometry.GAIA_POINTZM:
            //    case GaiaGeoGeometry.GAIA_LINESTRINGZM:
            //    case GaiaGeoGeometry.GAIA_POLYGONZM:
            //    case GaiaGeoGeometry.GAIA_MULTIPOINTZM:
            //    case GaiaGeoGeometry.GAIA_MULTILINESTRINGZM:
            //    case GaiaGeoGeometry.GAIA_MULTIPOLYGONZM:
            //    case GaiaGeoGeometry.GAIA_GEOMETRYCOLLECTIONZM:
            //    case GaiaGeoGeometry.GAIA_COMPRESSED_LINESTRINGZM:
            //    case GaiaGeoGeometry.GAIA_COMPRESSED_POLYGONZM:
            //        geo->DimensionModel = GAIA_XY_Z_M;
            //        break;
            //    default:
            //        geo->DimensionModel = GAIA_XY;
            //        break;
            //};
            offset = 6;
            var env = new Envelope(gaiaImport.GetDouble(blob, ref offset),
                                   gaiaImport.GetDouble(blob, ref offset),
                                   gaiaImport.GetDouble(blob, ref offset),
                                   gaiaImport.GetDouble(blob, ref offset));

            offset = 39;
            var type = (GaiaGeoGeometry)gaiaImport.GetInt32(blob, ref offset);
            var geom = ParseWkbGeometry(type, blob, ref offset, factory, gaiaImport);
            if (geom != null)
            {
                geom.SRID = srid;
                //geom.Envelope = env;
            }
            return geom;
        }

        public IGeometry Read(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return Read(buffer);
        }

        /// <summary>
        /// Gets or sets whether invalid linear rings should be fixed
        /// </summary>
        public bool RepairRings { get; set; }

        private static ReadCoordinatesFunction SetReadCoordinatesFunction(GaiaImport gaiaImport, GaiaGeoGeometry type)
        {
            gaiaImport.SetGeometryType(type);

            if (gaiaImport.Uncompressed)
            {
                if (gaiaImport.HasZ && gaiaImport.HasM)
                    return ReadXYZM;
                if (gaiaImport.HasM)
                    return ReadXYM;
                if (gaiaImport.HasZ)
                    return ReadXYZ;
                return ReadXY;
            }

            if (gaiaImport.HasZ && gaiaImport.HasM)
                return ReadCompressedXYZM;
            if (gaiaImport.HasM)
                return ReadCompressedXYM;
            if (gaiaImport.HasZ)
                return ReadCompressedXYZ;
            return ReadCompressedXY;
        }

        private static GaiaGeoGeometry ToBaseGeometryType(GaiaGeoGeometry geometry)
        {
            var geometryInt = (int)geometry;
            if (geometryInt > 1000000) geometryInt -= 1000000;
            if (geometryInt > 3000) geometryInt -= 3000;
            if (geometryInt > 2000) geometryInt -= 2000;
            if (geometryInt > 1000) geometryInt -= 1000;
            return (GaiaGeoGeometry)geometryInt;
        }

        private static IGeometry ParseWkbGeometry(GaiaGeoGeometry type, byte[] blob, ref int offset, IGeometryFactory factory, GaiaImport gaiaImport)
        {
            var readCoordinates = SetReadCoordinatesFunction(gaiaImport, type);

            switch (ToBaseGeometryType(type))
            {
                case GaiaGeoGeometry.GAIA_POINT:
                    return ParseWkbPoint(blob, ref offset, factory, readCoordinates, gaiaImport);

                case GaiaGeoGeometry.GAIA_MULTIPOINT:
                    return ParseWkbMultiPoint(blob, ref offset, factory, readCoordinates, gaiaImport);

                case GaiaGeoGeometry.GAIA_LINESTRING:
                    return ParseWkbLineString(blob, ref offset, factory, readCoordinates, gaiaImport);

                case GaiaGeoGeometry.GAIA_MULTILINESTRING:
                    return ParseWkbMultiLineString(blob, ref offset, factory, readCoordinates, gaiaImport);

                case GaiaGeoGeometry.GAIA_POLYGON:
                    return ParseWkbPolygon(blob, ref offset, factory, readCoordinates, gaiaImport);

                case GaiaGeoGeometry.GAIA_MULTIPOLYGON:
                    return ParseWkbMultiPolygon(blob, ref offset, factory, readCoordinates, gaiaImport);

                case GaiaGeoGeometry.GAIA_GEOMETRYCOLLECTION:
                    return ParseWkbGeometryCollection(blob, ref offset, factory, gaiaImport);
            }
            return null;
        }

        private static GaiaImport SetGaiaGeoParseFunctions(GaiaGeoEndianMarker gaiaGeoEndianMarker, Ordinates handleOrdinates)
        {
            var conversionNeeded = false;
            switch (gaiaGeoEndianMarker)
            {
                case GaiaGeoEndianMarker.GAIA_LITTLE_ENDIAN:
                    if (!BitConverter.IsLittleEndian)
                        conversionNeeded = true;
                    break;
                case GaiaGeoEndianMarker.GAIA_BIG_ENDIAN:
                    if (BitConverter.IsLittleEndian)
                        conversionNeeded = true;
                    break;
                default:
                    /* unknown encoding; nor litte-endian neither big-endian */
                    throw new ArgumentOutOfRangeException("gaiaGeoEndianMarker");
            }

            return GaiaImport.Create(conversionNeeded, handleOrdinates);
        }

        private static IPoint ParseWkbPoint(byte[] blob, ref int offset, IGeometryFactory factory, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            return factory.CreatePoint(readCoordinates(blob, ref offset, 1, gaiaImport, factory.CoordinateSequenceFactory, factory.PrecisionModel));
        }

        private static IMultiPoint ParseWkbMultiPoint(byte[] blob, ref int offset, IGeometryFactory factory, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            var getInt32 = gaiaImport.GetInt32;
            var getDouble = gaiaImport.GetDouble;

            var number = getInt32(blob, ref offset);
            var coords = new Coordinate[number];
            for (var i = 0; i < number; i++)
            {
                if (blob[offset++] != (byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY)
                    throw new Exception();

                var gt = getInt32(blob, ref offset);
                if (ToBaseGeometryType((GaiaGeoGeometry)gt) != GaiaGeoGeometry.GAIA_POINT)
                    throw new Exception();

                coords[i] = new Coordinate(getDouble(blob, ref offset),
                                           getDouble(blob, ref offset));
                if (gaiaImport.HasZ)
                    coords[i].Z = getDouble(blob, ref offset);
                if (gaiaImport.HasM)
                    /*coords[i].M =*/
                    getDouble(blob, ref offset);
            }
            return factory.CreateMultiPoint(coords);
        }

        private delegate ILineString CreateLineStringFunction(ICoordinateSequence coordinates);

        private static ILineString ParseWkbLineString(byte[] blob, ref int offset, IGeometryFactory factory, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            return ParseWkbLineString(blob, ref offset, factory, factory.CreateLineString, readCoordinates,
                                      gaiaImport);
        }

        private static ILineString ParseWkbLineString(byte[] blob, ref int offset, IGeometryFactory factory, CreateLineStringFunction createLineStringFunction, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            var number = gaiaImport.GetInt32(blob, ref offset);
            var sequence = readCoordinates(blob, ref offset, number, gaiaImport, factory.CoordinateSequenceFactory,
                                           factory.PrecisionModel);
            return createLineStringFunction(sequence);
        }

        private static IMultiLineString ParseWkbMultiLineString(byte[] blob, ref int offset, IGeometryFactory factory, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            int number = gaiaImport.GetInt32(blob, ref offset);
            var lineStrings = new ILineString[number];
            for (var i = 0; i < number; i++)
            {
                if (blob[offset++] != (byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY)
                    throw new Exception();

                var gt = gaiaImport.GetInt32(blob, ref offset);
                if (ToBaseGeometryType((GaiaGeoGeometry)gt) != GaiaGeoGeometry.GAIA_LINESTRING)
                    throw new Exception();

                //Since Uncompressed MultiGeom can contain compressed we need to set it here also
                readCoordinates = SetReadCoordinatesFunction(gaiaImport, (GaiaGeoGeometry)gt);

                lineStrings[i] = ParseWkbLineString(blob, ref offset, factory, factory.CreateLineString, readCoordinates, gaiaImport);
            }
            return factory.CreateMultiLineString(lineStrings);
        }

        private static IPolygon ParseWkbPolygon(byte[] blob, ref int offset, IGeometryFactory factory, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            var number = gaiaImport.GetInt32(blob, ref offset) - 1;
            var shell = (ILinearRing)ParseWkbLineString(blob, ref offset, factory, factory.CreateLinearRing, readCoordinates, gaiaImport);
            var holes = new ILinearRing[number];
            for (var i = 0; i < number; i++)
                holes[i] = (ILinearRing)ParseWkbLineString(blob, ref offset, factory, factory.CreateLinearRing, readCoordinates, gaiaImport);

            return factory.CreatePolygon(shell, holes);
        }

        private static IGeometry ParseWkbMultiPolygon(byte[] blob, ref int offset, IGeometryFactory factory, ReadCoordinatesFunction readCoordinates, GaiaImport gaiaImport)
        {
            var number = gaiaImport.GetInt32(blob, ref offset);
            var polygons = new IPolygon[number];
            for (var i = 0; i < number; i++)
            {
                if (blob[offset++] != (byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY)
                    throw new Exception();

                var gt = gaiaImport.GetInt32(blob, ref offset);
                if (ToBaseGeometryType((GaiaGeoGeometry)gt) != GaiaGeoGeometry.GAIA_POLYGON)
                    throw new Exception();

                //Since Uncompressed MultiGeom can contain compressed we need to set it here also
                readCoordinates = SetReadCoordinatesFunction(gaiaImport, (GaiaGeoGeometry)gt);


                polygons[i] = ParseWkbPolygon(blob, ref offset, factory, readCoordinates, gaiaImport);
            }
            return factory.CreateMultiPolygon(polygons);
        }

        private static IGeometryCollection ParseWkbGeometryCollection(byte[] blob, ref int offset, IGeometryFactory factory, GaiaImport gaiaImport)
        {
            var number = gaiaImport.GetInt32(blob, ref offset);
            var geometries = new IGeometry[number];
            for (var i = 0; i < number; i++)
            {
                if (blob[offset++] != (byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY)
                    throw new Exception();

                geometries[i] = ParseWkbGeometry((GaiaGeoGeometry)gaiaImport.GetInt32(blob, ref offset), blob, ref offset, factory, gaiaImport);
            }
            return factory.CreateGeometryCollection(geometries);
        }

        private delegate ICoordinateSequence ReadCoordinatesFunction(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel);

        private static ICoordinateSequence ReadXY(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var ordinateValues = import.GetDoubles(buffer, ref offset, number * 2);
            var ret = factory.Create(number, Ordinates.XY);
            var j = 0;
            for (var i = 0; i < number; i++)
            {
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(ordinateValues[j++]));
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(ordinateValues[j++]));
            }
            return ret;
        }

        private static ICoordinateSequence ReadXYZ(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var ordinateValues = import.GetDoubles(buffer, ref offset, number * 3);
            var ret = factory.Create(number, import.HandleOrdinates);
            var handleZ = (ret.Ordinates & Ordinates.Z) == Ordinates.Z;
            var j = 0;
            for (var i = 0; i < number; i++)
            {
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(ordinateValues[j++]));
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(ordinateValues[j++]));
                if (handleZ) ret.SetOrdinate(i, Ordinate.Z, precisionModel.MakePrecise(ordinateValues[j]));
                j++;
            }
            return ret;
        }

        private static ICoordinateSequence ReadXYM(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var ordinateValues = import.GetDoubles(buffer, ref offset, number * 3);
            var ret = factory.Create(number, import.HandleOrdinates);
            var handleM = (ret.Ordinates & Ordinates.M) == Ordinates.M;
            var j = 0;
            for (var i = 0; i < number; i++)
            {
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(ordinateValues[j++]));
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(ordinateValues[j++]));
                if (handleM) ret.SetOrdinate(i, Ordinate.M, precisionModel.MakePrecise(ordinateValues[j]));
                j++;
            }
            return ret;
        }

        private static ICoordinateSequence ReadXYZM(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var ordinateValues = import.GetDoubles(buffer, ref offset, number * 4);
            var ret = factory.Create(number, import.HandleOrdinates);
            var handleZ = (ret.Ordinates & Ordinates.Z) == Ordinates.Z;
            var handleM = (ret.Ordinates & Ordinates.M) == Ordinates.M;
            var j = 0;
            for (var i = 0; i < number; i++)
            {
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(ordinateValues[j++]));
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(ordinateValues[j++]));
                if (handleZ) ret.SetOrdinate(i, Ordinate.Z, precisionModel.MakePrecise(ordinateValues[j]));
                j++;
                if (handleM) ret.SetOrdinate(i, Ordinate.M, precisionModel.MakePrecise(ordinateValues[j]));
                j++;
            }
            return ret;
        }

        private static ICoordinateSequence ReadCompressedXY(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var startOrdinateValues = import.GetDoubles(buffer, ref offset, 2);
            var ret = factory.Create(number, import.HandleOrdinates);

            var x = startOrdinateValues[0];
            var y = startOrdinateValues[1];
            ret.SetOrdinate(0, Ordinate.X, precisionModel.MakePrecise(x));
            ret.SetOrdinate(0, Ordinate.Y, precisionModel.MakePrecise(y));

            if (number == 1) return ret;

            var ordinateValues = import.GetSingles(buffer, ref offset, (number - 2) * 2);

            var j = 0;
            int i;
            for (i = 1; i < number - 1; i++)
            {
                x = x + ordinateValues[j++];
                y = y + ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(x));
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(y));
            }

            startOrdinateValues = import.GetDoubles(buffer, ref offset, 2);
            ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(startOrdinateValues[0]));
            ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(startOrdinateValues[1]));

            return ret;
        }

        private static ICoordinateSequence ReadCompressedXYZ(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var startOrdinateValues = import.GetDoubles(buffer, ref offset, 3);
            var ret = factory.Create(number, Ordinates.XYZ);

            var handleZ = (ret.Ordinates & Ordinates.Z) == Ordinates.Z;

            var x = startOrdinateValues[0];
            ret.SetOrdinate(0, Ordinate.X, precisionModel.MakePrecise(x));
            var y = startOrdinateValues[1];
            ret.SetOrdinate(0, Ordinate.Y, precisionModel.MakePrecise(y));
            var z = handleZ ? startOrdinateValues[2] : Coordinate.NullOrdinate;
            ret.SetOrdinate(0, Ordinate.Z, z);

            if (number == 1) return ret;

            var ordinateValues = import.GetSingles(buffer, ref offset, (number - 2) * 3);

            var j = 0;
            int i;
            for (i = 1; i < number - 1; i++)
            {
                x += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(x));
                y += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(y));
                if (handleZ) z += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.Z, z);
            }

            startOrdinateValues = import.GetDoubles(buffer, ref offset, 3);
            ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(startOrdinateValues[0]));
            ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(startOrdinateValues[1]));
            z = handleZ ? startOrdinateValues[2] : Coordinate.NullOrdinate;
            ret.SetOrdinate(i, Ordinate.Z, z);
            return ret;
        }

        private static ICoordinateSequence ReadCompressedXYM(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var startOrdinateValues = import.GetDoubles(buffer, ref offset, 3);
            var ret = factory.Create(number, Ordinates.XYM);

            var handleM = (ret.Ordinates & Ordinates.M) == Ordinates.M;

            var x = startOrdinateValues[0];
            ret.SetOrdinate(0, Ordinate.X, precisionModel.MakePrecise(x));
            var y = startOrdinateValues[1];
            ret.SetOrdinate(0, Ordinate.Y, precisionModel.MakePrecise(y));
            var m = handleM ? startOrdinateValues[2] : Coordinate.NullOrdinate;
            ret.SetOrdinate(0, Ordinate.M, m);

            if (number == 1) return ret;

            var ordinateValues = import.GetSingles(buffer, ref offset, (number - 2) * 3);

            var j = 0;
            int i;
            for (i = 1; i < number - 1; i++)
            {
                x += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(x));
                y += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(y));
                if (handleM) m += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.M, m);
            }

            startOrdinateValues = import.GetDoubles(buffer, ref offset, 3);
            ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(startOrdinateValues[0]));
            ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(startOrdinateValues[1]));
            m = handleM ? startOrdinateValues[2] : Coordinate.NullOrdinate;
            ret.SetOrdinate(i, Ordinate.M, m);
            return ret;
        }

        private static ICoordinateSequence ReadCompressedXYZM(byte[] buffer, ref int offset, int number, GaiaImport import, ICoordinateSequenceFactory factory, IPrecisionModel precisionModel)
        {
            var startOrdinateValues = import.GetDoubles(buffer, ref offset, 4);
            var ret = factory.Create(number, Ordinates.XYM);

            var handleZ = (ret.Ordinates & Ordinates.Z) == Ordinates.Z;
            var handleM = (ret.Ordinates & Ordinates.M) == Ordinates.M;

            var x = startOrdinateValues[0];
            ret.SetOrdinate(0, Ordinate.X, precisionModel.MakePrecise(x));
            var y = startOrdinateValues[1];
            ret.SetOrdinate(0, Ordinate.Y, precisionModel.MakePrecise(y));
            var z = handleZ ? startOrdinateValues[2] : Coordinate.NullOrdinate;
            ret.SetOrdinate(0, Ordinate.Z, z);
            var m = handleM ? startOrdinateValues[3] : Coordinate.NullOrdinate;
            ret.SetOrdinate(0, Ordinate.M, m);

            if (number == 1) return ret;

            var ordinateValues = import.GetSingles(buffer, ref offset, (number - 2) * 4);

            var j = 0;
            int i;
            for (i = 1; i < number - 1; i++)
            {
                x += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(x));
                y += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(y));
                if (handleZ) z += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.Z, z);
                if (handleM) m += ordinateValues[j++];
                ret.SetOrdinate(i, Ordinate.M, m);
            }

            startOrdinateValues = import.GetDoubles(buffer, ref offset, 4);
            ret.SetOrdinate(i, Ordinate.X, precisionModel.MakePrecise(startOrdinateValues[0]));
            ret.SetOrdinate(i, Ordinate.Y, precisionModel.MakePrecise(startOrdinateValues[1]));
            z = handleZ ? startOrdinateValues[2] : Coordinate.NullOrdinate;
            ret.SetOrdinate(i, Ordinate.Z, z);
            m = handleM ? startOrdinateValues[3] : Coordinate.NullOrdinate;
            ret.SetOrdinate(i, Ordinate.M, m);
            return ret;
        }

        public bool HandleSRID { get; set; }

        public Ordinates AllowedOrdinates
        {
            get { return Ordinates.XYZM; }
        }

        public Ordinates HandleOrdinates
        {
            get { return _handleOrdinates; }
            set
            {
                value = AllowedOrdinates & value;
                _handleOrdinates = value;
            }
        }
    }
}