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
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;

namespace NetTopologySuite.IO
{
    public class GaiaGeoWriter : IBinaryGeometryWriter
    {
        private Ordinates _handleOrdinates;

        public void Write(IGeometry geometry, Stream stream)
        {
            var g = Write(geometry);
            stream.Write(g, 0, g.Length);
        }

        public byte[] Write(IGeometry geom)
        {
            //if (geom.IsEmpty)
            //    return GaiaGeoEmptyHelper.EmptyGeometryCollectionWithSrid(geom.SRID);

            var hasZ = (HandleOrdinates & Ordinates.Z) == Ordinates.Z;
            var hasM = (HandleOrdinates & Ordinates.M) == Ordinates.M;

            var gaiaExport = SetGaiaGeoExportFunctions(GaiaGeoEndianMarker.GAIA_LITTLE_ENDIAN, hasZ, hasM, UseCompressed);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    //Header
                    bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_START);
                    bw.Write((byte)GaiaGeoEndianMarker.GAIA_LITTLE_ENDIAN);
                    //SRID
                    gaiaExport.WriteInt32(bw, geom.SRID);
                    //MBR
                    var env = geom.EnvelopeInternal; //.Coordinates;
                    if (geom.IsEmpty)
                    {
                        gaiaExport.WriteDouble(bw, 0d, 0d, 0d, 0d);
                    }
                    else
                    {
                        gaiaExport.WriteDouble(bw, env.MinX, env.MinY, env.MaxX, env.MaxY);
                    }

                    bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_MBR);
                    Debug.Assert(ms.Position == 39);

                    //Write geometry
                    WriteGeometry(geom, gaiaExport, bw);

                    bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_END);
                }
                return ms.ToArray();
            }
        }

        private static WriteCoordinates SetWriteCoordinatesFunction(GaiaExport gaiaExport)
        {
            if (gaiaExport.Uncompressed)
            {
                if (gaiaExport.HasZ && gaiaExport.HasM)
                    return WriteXYZM;
                if (gaiaExport.HasM)
                    return WriteXYM;
                if (gaiaExport.HasZ)
                    return WriteXYZ;

                return WriteXY;
            }

            if (gaiaExport.HasZ && gaiaExport.HasM)
                return WriteCompressedXYZM;
            if (gaiaExport.HasM)
                return WriteCompressedXYM;
            if (gaiaExport.HasZ)
                return WriteCompressedXYZ;

            return WriteCompressedXY;
        }

        private static void WriteGeometry(IGeometry geom, GaiaExport gaiaExport, BinaryWriter bw)
        {
            WriteCoordinates writeCoordinates = SetWriteCoordinatesFunction(gaiaExport);

            //Geometry type
            int coordinateFlag = gaiaExport.CoordinateFlag;
            int coordinateFlagNotValidForCompression = coordinateFlag > 1000000
                                                           ? coordinateFlag - 1000000
                                                           : coordinateFlag;
            switch (geom.GeometryType.ToUpper())
            {
                case "POINT":
                    gaiaExport.WriteInt32(bw, (int)(GaiaGeoGeometry.GAIA_POINT) | coordinateFlagNotValidForCompression);
                    WritePoint((IPoint)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "LINESTRING":
                    gaiaExport.WriteInt32(bw, (int)GaiaGeoGeometry.GAIA_LINESTRING | coordinateFlag);
                    WriteLineString((ILineString)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "POLYGON":
                    gaiaExport.WriteInt32(bw, (int)GaiaGeoGeometry.GAIA_POLYGON | coordinateFlag);
                    WritePolygon((IPolygon)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "MULTIPOINT":
                    gaiaExport.WriteInt32(bw, (int)GaiaGeoGeometry.GAIA_MULTIPOINT | coordinateFlagNotValidForCompression);
                    WriteMultiPoint((IMultiPoint)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "MULTILINESTRING":
                    gaiaExport.WriteInt32(bw, (int)GaiaGeoGeometry.GAIA_MULTILINESTRING | coordinateFlag);
                    WriteMultiLineString((IMultiLineString)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "MULTIPOLYGON":
                    gaiaExport.WriteInt32(bw, (int)GaiaGeoGeometry.GAIA_MULTIPOLYGON | coordinateFlag);
                    WriteMultiPolygon((IMultiPolygon)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "GEOMETRYCOLLECTION":
                    gaiaExport.WriteInt32(bw, (int)GaiaGeoGeometry.GAIA_GEOMETRYCOLLECTION | coordinateFlagNotValidForCompression);
                    WriteGeometryCollection((IGeometryCollection)geom, gaiaExport, bw);
                    break;
                default:
                    throw new ArgumentException("unknown geometry type");
            }
        }

        private static void WriteGeometryCollection(IGeometryCollection geom, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(bw, geom.NumGeometries);
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                WriteGeometry(geom[i], gaiaExport, bw);
            }
        }

        private static void WriteMultiPolygon(IGeometryCollection geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(bw, geom.NumGeometries);
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                gaiaExport.WriteInt32(bw, gaiaExport.CoordinateFlag | (int)GaiaGeoGeometry.GAIA_POLYGON);
                WritePolygon((IPolygon)geom[i], writeCoordinates, gaiaExport, bw);
            }
        }

        private static void WriteMultiLineString(IMultiLineString geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(bw, geom.NumGeometries);
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                gaiaExport.WriteInt32(bw, gaiaExport.CoordinateFlag | (int)GaiaGeoGeometry.GAIA_LINESTRING);
                WriteLineString((ILineString)geom[i], writeCoordinates, gaiaExport, bw);
            }
        }

        private static void WriteMultiPoint(IMultiPoint geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            var wi = gaiaExport.WriteInt32;

            // number of coordinates
            wi(bw, geom.NumGeometries);

            // get the coordinate flag
            var coordinateFlag = gaiaExport.CoordinateFlagUncompressed;

            for (var i = 0; i < geom.NumGeometries; i++)
            {
                //write entity begin marker
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);

                //write entity marker
                wi(bw, coordinateFlag + (int)GaiaGeoGeometryEntity.GAIA_TYPE_POINT);

                //write coordinates
                writeCoordinates(((IPoint)geom[i]).CoordinateSequence, gaiaExport, bw);
            }
        }

        private static void WritePolygon(IPolygon geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(bw, geom.NumInteriorRings + 1);
            WriteLineString(geom.Shell, writeCoordinates, gaiaExport, bw);
            for (var i = 0; i < geom.NumInteriorRings; i++)
                WriteLineString(geom.GetInteriorRingN(i), writeCoordinates, gaiaExport, bw);
        }

        private static void WriteLineString(ILineString geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            var seq = geom.CoordinateSequence;
            gaiaExport.WriteInt32(bw, seq.Count);
            writeCoordinates(geom.CoordinateSequence, gaiaExport, bw);
        }

        private static void WritePoint(IPoint geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            writeCoordinates(geom.CoordinateSequence, gaiaExport, bw);
        }

        private static GaiaExport SetGaiaGeoExportFunctions(GaiaGeoEndianMarker gaiaGeoEndianMarker, bool hasZ, bool hasM, bool useCompression)
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
                    return null;
            }

            var gaiaExport = GaiaExport.Create(conversionNeeded);
            gaiaExport.SetCoordinateType(hasZ, hasM, useCompression);
            return gaiaExport;
        }

        private delegate void WriteCoordinates(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw);

        private static void WriteXY(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;

            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(bw, c.X, c.Y);
            }
        }

        private static void WriteXYZ(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(bw, c.X, c.Y, c.Z);
            }
        }

        private static void WriteXYM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(bw, c.X, c.Y, coordinateSequence.GetOrdinate(i, Ordinate.M));
            }
        }

        private static void WriteXYZM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(bw, c.X, c.Y, c.Z, coordinateSequence.GetOrdinate(i, Ordinate.M));
            }
        }

        private static void WriteCompressedXY(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;

            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            wd(bw, cprev.X, cprev.Y);

            var ws = export.WriteSingle;
            var maxIndex = coordinateSequence.Count - 1;
            if (maxIndex <= 0) return;

            for (var i = 1; i < maxIndex; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                ws(bw, fx, fy);
                cprev = c;
            }

            // Write last coordinate
            cprev = coordinateSequence.GetCoordinate(maxIndex);
            wd(bw, cprev.X, cprev.Y);
        }

        private static void WriteCompressedXYZ(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;

            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            wd(bw, cprev.X, cprev.Y, cprev.Z);

            var maxIndex = coordinateSequence.Count - 1;
            if (maxIndex <= 0) return;

            var ws = export.WriteSingle;
            for (var i = 1; i < maxIndex; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                var fz = (float)(c.Z - cprev.Z);
                ws(bw, fx, fy, fz);
                cprev = c;
            }
            cprev = coordinateSequence.GetCoordinate(maxIndex);
            wd(bw, cprev.X, cprev.Y, cprev.Z);
        }

        private static void WriteCompressedXYM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;

            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            var mprev = coordinateSequence.GetOrdinate(0, Ordinate.M);
            wd(bw, cprev.X, cprev.Y, mprev);

            var maxIndex = coordinateSequence.Count - 1;
            if (maxIndex <= 0) return;

            var ws = export.WriteSingle;
            for (var i = 1; i < maxIndex; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                var fm = (float)(coordinateSequence.GetOrdinate(i, Ordinate.M) - mprev);
                ws(bw, fx, fy, fm);
                cprev = c;
            }
            cprev = coordinateSequence.GetCoordinate(maxIndex);
            mprev = coordinateSequence.GetOrdinate(maxIndex, Ordinate.M);
            wd(bw, cprev.X, cprev.Y, mprev);
        }

        private static void WriteCompressedXYZM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;

            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            var mprev = coordinateSequence.GetOrdinate(0, Ordinate.M);
            wd(bw, cprev.X, cprev.Y, cprev.Z, mprev);

            var maxIndex = coordinateSequence.Count - 1;
            if (maxIndex <= 0) return;

            var ws = export.WriteSingle;
            for (var i = 1; i < maxIndex; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                var fz = (float)(c.Z - cprev.Z);
                var fm = (float)(coordinateSequence.GetOrdinate(i, Ordinate.M) - mprev);
                ws(bw, fx, fy, fz, fm);
                cprev = c;
            }
            cprev = coordinateSequence.GetCoordinate(maxIndex);
            mprev = coordinateSequence.GetOrdinate(maxIndex, Ordinate.M);
            wd(bw, cprev.X, cprev.Y, cprev.Z, mprev);
        }

        public bool HandleSRID
        {
            get { return true; }
            set
            {
                if (!value)
                    throw new InvalidOperationException("Always write SRID value!");
            }
        }

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

        public ByteOrder ByteOrder
        {
            get { return ByteOrder.LittleEndian; }
            set
            {
                if (value != ByteOrder.LittleEndian)
                    throw new InvalidOperationException("Always use LittleEndian!");
            }
        }

        /// <summary>
        /// Gets or sets whether geometries should be written in compressed form
        /// </summary>
        public bool UseCompressed { get; set; }
    }
}