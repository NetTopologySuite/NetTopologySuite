using System;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    public class GaiaGeoWriter
    {
        public static byte[] Write(IGeometry geom)
        {
            return Write(geom, false, false, false);
        }

        public static byte[] Write(IGeometry geom, bool hasZ, bool hasM, bool useCompressed)
        {
            //if (geom.IsEmpty)
            //    return GaiaGeoEmptyHelper.EmptyGeometryCollectionWithSrid(geom.SRID);

            var gaiaExport = SetGaiaGeoExportFunctions(GaiaGeoEndianMarker.GAIA_LITTLE_ENDIAN, hasZ, hasM, useCompressed);
            
            using(var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    //Header
                    bw.Write((byte) GaiaGeoBlobMark.GAIA_MARK_START);
                    bw.Write((byte) GaiaGeoEndianMarker.GAIA_LITTLE_ENDIAN);
                    //SRID
                    gaiaExport.WriteInt32(geom.SRID, bw);
                    //MBR
                    var env = geom.EnvelopeInternal; //.Coordinates;
                    if (geom.IsEmpty)
                    {
                        gaiaExport.WriteDouble(0d, bw);
                        gaiaExport.WriteDouble(0d, bw);
                        gaiaExport.WriteDouble(0d, bw);
                        gaiaExport.WriteDouble(0d, bw);
                    }
                    else
                    {
                        gaiaExport.WriteDouble(env.MinX, bw);
                        gaiaExport.WriteDouble(env.MinY, bw);
                        gaiaExport.WriteDouble(env.MaxX, bw);
                        gaiaExport.WriteDouble(env.MaxY, bw);
                    }

                    bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_MBR);
                    Debug.Assert(ms.Position == 39);

                    WriteCoordinates writeCoordinates = SetWriteCoordinatesFunction(gaiaExport);

                    //Write geometry
                    WriteGeometry(geom, writeCoordinates, gaiaExport, bw);

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
                if (gaiaExport.HasZ )
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

        private static void WriteGeometry(IGeometry geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            
            //Geometry type
            switch (geom.GeometryType.ToUpper())
            {
                case "POINT":
                    gaiaExport.WriteInt32((int)(GaiaGeoGeometry.GAIA_POINT) | gaiaExport.CoordinateFlag, bw);
                    WritePoint((IPoint)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "LINESTRING":
                    gaiaExport.WriteInt32((int)GaiaGeoGeometry.GAIA_LINESTRING | gaiaExport.CoordinateFlag, bw);
                    WriteLineString((ILineString)geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "POLYGON":
                    gaiaExport.WriteInt32((int)GaiaGeoGeometry.GAIA_POLYGON | gaiaExport.CoordinateFlag, bw);
                    WritePolygon((IPolygon) geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "MULTIPOINT":
                    gaiaExport.WriteInt32((int)GaiaGeoGeometry.GAIA_MULTIPOINT | gaiaExport.CoordinateFlag, bw);
                    WriteMultiPoint((IMultiPoint) geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "MULTILINESTRING":
                    gaiaExport.WriteInt32((int)GaiaGeoGeometry.GAIA_MULTILINESTRING | gaiaExport.CoordinateFlag, bw);
                    WriteMultiLineString((IMultiLineString) geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "MULTIPOLYGON":
                    gaiaExport.WriteInt32((int)GaiaGeoGeometry.GAIA_MULTIPOLYGON | gaiaExport.CoordinateFlag, bw);
                    WriteMultiPolygon((IMultiPolygon) geom, writeCoordinates, gaiaExport, bw);
                    break;
                case "GEOMETRYCOLLECTION":
                    gaiaExport.WriteInt32((int)GaiaGeoGeometry.GAIA_GEOMETRYCOLLECTION | gaiaExport.CoordinateFlag, bw);
                    WriteGeometryCollection((IGeometryCollection)geom, writeCoordinates, gaiaExport, bw);
                    break;
                default:
                    throw new ArgumentException("unknown geometry type");
            }
            
        }

        private static void WriteGeometryCollection(IGeometryCollection geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(geom.NumGeometries, bw);
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte) GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                WriteGeometry(geom[i], writeCoordinates, gaiaExport, bw);
            }
        }

        private static void WriteMultiPolygon(IGeometryCollection geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(geom.NumGeometries, bw);
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                gaiaExport.WriteInt32(gaiaExport.CoordinateFlag | (int)GaiaGeoGeometry.GAIA_POLYGON, bw);
                WritePolygon((IPolygon)geom[i], writeCoordinates, gaiaExport, bw);
            }
        }

        private static void WriteMultiLineString(IMultiLineString geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(geom.NumGeometries, bw);
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                gaiaExport.WriteInt32(gaiaExport.CoordinateFlag | (int)GaiaGeoGeometry.GAIA_LINESTRING, bw);
                WriteLineString((ILineString) geom[i], writeCoordinates, gaiaExport, bw);
            }
        }

        private static void WriteMultiPoint(IMultiPoint geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            var wi = gaiaExport.WriteInt32;
            var wd = gaiaExport.WriteDouble;

            wi(geom.NumGeometries, bw);
            var coords = geom.Coordinates;

            for (var i = 0; i < geom.NumGeometries; i++)
            {
                bw.Write((byte)GaiaGeoBlobMark.GAIA_MARK_ENTITY);
                var c = coords[i];
                wi(gaiaExport.CoordinateFlag | (int)GaiaGeoGeometry.GAIA_POINT, bw);
                wd(c.X, bw);
                wd(c.Y, bw);
                if ((gaiaExport.CoordinateFlag & (int)GaiaDimensionModels.GAIA_Z) == (int)GaiaDimensionModels.GAIA_Z)
                    wd(c.Z, bw);
                if ((gaiaExport.CoordinateFlag & (int)GaiaDimensionModels.GAIA_M) == (int)GaiaDimensionModels.GAIA_M)
                    wd(c.M, bw);
            }
        }

        private static void WritePolygon(IPolygon geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            gaiaExport.WriteInt32(geom.NumInteriorRings + 1, bw);
            WriteLineString(geom.Shell, writeCoordinates, gaiaExport, bw);
            for (var i = 0; i < geom.NumInteriorRings; i++ )
                WriteLineString(geom.GetInteriorRingN(i), writeCoordinates, gaiaExport, bw);
        }

        private static void WriteLineString(ILineString geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            var seq = geom.CoordinateSequence;
            gaiaExport.WriteInt32(seq.Count, bw);
            writeCoordinates(geom.CoordinateSequence, gaiaExport, bw);
        }

        private static void WritePoint(IPoint geom, WriteCoordinates writeCoordinates, GaiaExport gaiaExport, BinaryWriter bw)
        {
            writeCoordinates(geom.CoordinateSequence, gaiaExport, bw );
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
            gaiaExport .SetCoordinateType(hasZ, hasM, useCompression);
            return gaiaExport;
        }

        private delegate void WriteCoordinates(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw);


        private static void WriteXY(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;

            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(c.X, bw);
                wd(c.Y, bw);
            }
        }

        private static void WriteXYZ(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(c.X, bw);
                wd(c.Y, bw);
                wd(c.Z, bw);
            }
        }

        private static void WriteXYM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(c.X, bw);
                wd(c.Y, bw);
                wd(c.M, bw);
            }
        }

        private static void WriteXYZM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                wd(c.X, bw);
                wd(c.Y, bw);
                wd(c.Z, bw);
            }
        }

        private static void WriteCompressedXY(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            var wd = export.WriteDouble;
            
            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            wd(cprev.X, bw);
            wd(cprev.Y, bw);

            var ws = export.WriteSingle;

            for (var i = 1; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                ws(fx, bw);
                ws(fy, bw);
            }
        }

        private static void WriteCompressedXYZ(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            
            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            export.WriteDouble(cprev.X, bw);
            export.WriteDouble(cprev.Y, bw);
            export.WriteDouble(cprev.Z, bw);

            for (var i = 1; i < coordinateSequence.Count; i++)
            {
                ICoordinate c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                var fz = (float)(c.Z - cprev.Z);
                export.WriteSingle(fx, bw);
                export.WriteSingle(fy, bw);
                export.WriteSingle(fz, bw);
            }
        }

        private static void WriteCompressedXYM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            export.WriteDouble(cprev.X, bw);
            export.WriteDouble(cprev.Y, bw);
            export.WriteDouble(cprev.M, bw);

            for (var i = 1; i < coordinateSequence.Count; i++)
            {
                ICoordinate c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                var fm = (float)(c.M - cprev.M);
                export.WriteSingle(fx, bw);
                export.WriteSingle(fy, bw);
                export.WriteSingle(fm, bw);
            }
        }

        private static void WriteCompressedXYZM(ICoordinateSequence coordinateSequence, GaiaExport export, BinaryWriter bw)
        {
            // Write initial coordinate
            var cprev = coordinateSequence.GetCoordinate(0);
            export.WriteDouble(cprev.X, bw);
            export.WriteDouble(cprev.Y, bw);
            export.WriteDouble(cprev.Z, bw);
            export.WriteDouble(cprev.M, bw);

            for (var i = 1; i < coordinateSequence.Count; i++)
            {
                var c = coordinateSequence.GetCoordinate(i);
                var fx = (float)(c.X - cprev.X);
                var fy = (float)(c.Y - cprev.Y);
                var fz = (float)(c.Z - cprev.Z);
                var fm = (float)(c.M - cprev.M);
                export.WriteSingle(fx, bw);
                export.WriteSingle(fy, bw);
                export.WriteSingle(fz, bw);
                export.WriteSingle(fm, bw);
            }
        }
    }
}