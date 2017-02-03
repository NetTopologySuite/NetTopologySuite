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
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    // ReSharper disable InconsistentNaming
    internal abstract class GaiaGeoIO
    // ReSharper restore InconsistentNaming
    {
        public void SetGeometryType(GaiaGeoGeometry geometryTypeFlag)
        {
            //Debug.Assert(geometryTypeFlag != GaiaGeoGeometry.GAIA_UNKNOWN);
            //Debug.Assert(geometryTypeFlag > 0);

            var cflag = ((int)geometryTypeFlag);
            if (cflag > 1000000)
            {
                Compressed = true;
                cflag -= 1000000;
            }

            if (cflag > 3000)
                cflag = 3000;
            else if (cflag > 2000)
                cflag = 2000;
            else if (cflag > 1000)
                cflag = 1000;
            else
                cflag = 0;

            CoordinateFlag = cflag | (Compressed ? 1000000 : 0);

            HasZ = (cflag == 1000) || (cflag == 3000);
            HasM = (cflag == 2000) || (cflag == 3000);

            Dimension = GaiaDimensionModels.GAIA_XY;
            if (HasZ) Dimension |= GaiaDimensionModels.GAIA_Z;
            if (HasM) Dimension |= GaiaDimensionModels.GAIA_M;
        }

        public void SetCoordinateType(bool hasZ, bool hasM, bool useCompression)
        {
            var cflag = 0;
            if (hasZ) cflag += 1000;
            if (hasM) cflag += 2000;
            if (useCompression)
                cflag += 1000000;

            if (cflag == CoordinateFlag)
                return;

            Dimension = GaiaDimensionModels.GAIA_XY;
            if (HasZ) Dimension |= GaiaDimensionModels.GAIA_Z;
            if (HasM) Dimension |= GaiaDimensionModels.GAIA_M;

            HasZ = hasZ;
            HasM = hasM;
            Compressed = useCompression;

            CoordinateFlag = cflag;
        }

        public int CoordinateFlag { get; private set; }

        public int CoordinateFlagUncompressed { get { return CoordinateFlag > 1000000 ? CoordinateFlag - 1000000 : CoordinateFlag; } }

        public GaiaDimensionModels Dimension { get; private set; }

        public bool HasZ { get; private set; }

        public bool HasM { get; private set; }

        public bool Compressed { get; private set; }

        public bool Uncompressed
        {
            get { return !Compressed; }
        }

        protected GaiaGeoIO()
        {
        }

        protected GaiaGeoIO(GaiaGeoGeometry geometryType)
        {
            SetGeometryType(geometryType);
        }
    }

    #region Import

    internal delegate Double GetDoubleFunction(byte[] buffer, ref int offset);
    internal delegate Double[] GetDoublesFunction(byte[] buffer, ref int offset, int size);
    internal delegate Single GetSingleFunction(byte[] buffer, ref int offset);
    internal delegate Single[] GetSinglesFunction(byte[] buffer, ref int offset, int size);
    internal delegate Int32 GetInt32Function(byte[] buffer, ref int offset);

    internal class GaiaImport : GaiaGeoIO
    {
        public readonly GetDoubleFunction GetDouble;
        public readonly GetDoublesFunction GetDoubles;
        public readonly GetSingleFunction GetSingle;
        public readonly GetSinglesFunction GetSingles;
        public readonly GetInt32Function GetInt32;

        public readonly Ordinates HandleOrdinates;

        internal static GaiaImport Create(bool conversionNeeded, Ordinates handleOrdinates)
        {
            return conversionNeeded
                ? new GaiaImport(GetConvertedDouble, GetConvertedDoubles, GetConvertedSingle, GetConvertedSingles, GetConvertedInt32, handleOrdinates)
                : new GaiaImport(GetUnconvertedDouble, GetUnconvertedDoubles, GetUnconvertedSingle, GetUnconvertedSingles, GetUnconvertedInt32, handleOrdinates);
        }

        private GaiaImport(GetDoubleFunction getDouble, GetDoublesFunction getDoubles, GetSingleFunction getSingle, GetSinglesFunction getSingles, GetInt32Function getInt32, Ordinates handleOrdinates)
            : this(0, getDouble, getDoubles, getSingle, getSingles, getInt32, handleOrdinates)
        { }

        private GaiaImport(GaiaGeoGeometry geometryType, GetDoubleFunction getDouble, GetDoublesFunction getDoubles, GetSingleFunction getSingle, GetSinglesFunction getSingles, GetInt32Function getInt32, Ordinates handleOrdinates)
            : base(geometryType)
        {
            GetDouble = getDouble;
            GetDoubles = getDoubles;
            GetSingle = getSingle;
            GetSingles = getSingles;
            GetInt32 = getInt32;
            HandleOrdinates = handleOrdinates;
        }

        //public static readonly GaiaImport NoConversion = new GaiaImport(GetUnconvertedDouble, GetConvertedDoubles,
        //                                                                GetUnconvertedSingle, GetConvertedSingles,
        //                                                                GetUnconvertedInt32, );

        //public static readonly GaiaImport Conversion = new GaiaImport(GetConvertedDouble, GetConvertedDoubles,
        //                                                              GetConvertedSingle, GetConvertedSingles,
        //                                                              GetConvertedInt32);

        #region Double

        private static Double GetUnconvertedDouble(byte[] buffer, ref int offset)
        {
            var val = BitConverter.ToDouble(buffer, offset);
            offset += 8;
            return val;
        }

        private static Double[] GetUnconvertedDoubles(byte[] buffer, ref int offset, int size)
        {
            var val = new double[size];
            for (var i = 0; i < size; i++)
            {
                val[i] = BitConverter.ToDouble(buffer, offset);
                offset += 8;
            }
            return val;
        }

        private static Double GetConvertedDouble(byte[] buffer, ref int offset)
        {
            var tmp = new byte[8];
            Buffer.BlockCopy(buffer, offset, tmp, 0, 8);
            Array.Reverse(tmp);
            offset += 8;
            return BitConverter.ToDouble(tmp, 0);
        }

        private static Double[] GetConvertedDoubles(byte[] buffer, ref int offset, int size)
        {
            var tmp = new byte[8 * size];
            Buffer.BlockCopy(buffer, offset, tmp, 0, size * 8);
            Array.Reverse(tmp);

            var val = new double[size];
            var j = 0;
            for (var i = (size - 1) * 8; i >= 0; i -= 8)
            {
                val[j++] = BitConverter.ToDouble(tmp, i);
            }
            return val;
        }

        #endregion Double

        #region Single

        private static Single GetUnconvertedSingle(byte[] buffer, ref int offset)
        {
            var val = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            return val;
        }

        private static Single[] GetUnconvertedSingles(byte[] buffer, ref int offset, int size)
        {
            var val = new float[size];
            for (var i = 0; i < size; i++)
            {
                val[i] = BitConverter.ToSingle(buffer, offset);
                offset += 4;
            }
            return val;
        }

        private static Single GetConvertedSingle(byte[] buffer, ref int offset)
        {
            var tmp = new byte[4];
            Buffer.BlockCopy(buffer, offset, tmp, 0, 4);
            Array.Reverse(tmp);
            offset += 4;
            return BitConverter.ToSingle(tmp, 0);
        }

        private static Single[] GetConvertedSingles(byte[] buffer, ref int offset, int size)
        {
            var tmp = new byte[4 * size];
            Buffer.BlockCopy(buffer, offset, tmp, 0, tmp.Length);
            Array.Reverse(tmp);
            var val = new float[size];
            var j = 0;
            for (var i = (size - 1) * 4; i >= 0; i -= 4)
            {
                val[j++] = BitConverter.ToSingle(tmp, 0);
                //offset += 4;
            }
            return val;
        }

        #endregion Single

        #region Int32

        private static Int32 GetUnconvertedInt32(byte[] buffer, ref int offset)
        {
            var val = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            return val;
        }

        private static Int32 GetConvertedInt32(byte[] buffer, ref int offset)
        {
            var tmp = new byte[4];
            Buffer.BlockCopy(buffer, offset, tmp, 0, 4);
            Array.Reverse(tmp);
            offset += 4;
            return BitConverter.ToInt32(tmp, 0);
        }

        #endregion Int32
    }

    #endregion Import

    #region Export

    internal delegate void WriteDoubleFunction(BinaryWriter bw, params Double[] value);
    internal delegate void WriteInt32Function(BinaryWriter bw, params Int32[] value);
    internal delegate void WriteSingleFunction(BinaryWriter bw, params Single[] value);

    internal class GaiaExport : GaiaGeoIO
    {
        public readonly WriteDoubleFunction WriteDouble;
        public readonly WriteSingleFunction WriteSingle;
        public readonly WriteInt32Function WriteInt32;

        private GaiaExport(WriteDoubleFunction writeDouble, WriteSingleFunction writeSingle,
                           WriteInt32Function writeInt32)
            : this(0, writeDouble, writeSingle, writeInt32)
        {
        }

        private GaiaExport(GaiaGeoGeometry geometryType, WriteDoubleFunction writeDouble, WriteSingleFunction writeSingle,
                           WriteInt32Function writeInt32)
            : base(geometryType)
        {
            WriteDouble = writeDouble;
            WriteSingle = writeSingle;
            WriteInt32 = writeInt32;
        }

        public static readonly GaiaExport NoConversion = new GaiaExport(WriteUnconvertedDouble, WriteUnconvertedSingle,
                                                                        WriteUnconvertedInt32);

        public static readonly GaiaExport Conversion = new GaiaExport(WriteConvertedDouble, WriteConvertedSingle,
                                                                      WriteConvertedInt32);

        #region Double

        private static void WriteUnconvertedDouble(BinaryWriter bw, params Double[] value)
        {
            foreach (var d in value)
                bw.Write(d);
        }

        private static void WriteConvertedDouble(BinaryWriter bw, params Double[] value)
        {
            foreach (var d in value)
            {
                var tmp = BitConverter.GetBytes(d);
                Array.Reverse(tmp);
                bw.Write(tmp);
            }
        }

        #endregion Double

        #region Single

        private static void WriteUnconvertedSingle(BinaryWriter bw, params Single[] value)
        {
            foreach (var f in value)
                bw.Write(f);
        }

        private static void WriteConvertedSingle(BinaryWriter bw, params Single[] value)
        {
            foreach (var f in value)
            {
                var tmp = BitConverter.GetBytes(f);
                Array.Reverse(tmp);
                bw.Write(tmp);
            }
        }

        #endregion Single

        #region Int32

        internal static void WriteUnconvertedInt32(BinaryWriter bw, params Int32[] value)
        {
            foreach (var i in value)
                bw.Write(i);
        }

        private static void WriteConvertedInt32(BinaryWriter bw, params Int32[] value)
        {
            foreach (var i in value)
            {
                var tmp = BitConverter.GetBytes(i);
                Array.Reverse(tmp);
                bw.Write(tmp);
            }
        }

        #endregion Int32

        public static GaiaExport Create(bool conversionNeeded)
        {
            if (conversionNeeded)
            {
                return new GaiaExport(WriteConvertedDouble, WriteConvertedSingle,
                                                            WriteConvertedInt32);
            }
            return new GaiaExport(WriteUnconvertedDouble, WriteUnconvertedSingle,
                                                                    WriteUnconvertedInt32);
        }
    }

    #endregion Export
}