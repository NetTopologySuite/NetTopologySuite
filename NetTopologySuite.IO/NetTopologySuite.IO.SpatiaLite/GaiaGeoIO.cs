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

namespace NetTopologySuite.IO
{
    internal abstract class GaiaGeoIO
    {
        public void SetGeometryType(GaiaGeoGeometry geometryTypeFlag)
        {
            //Debug.Assert(geometryTypeFlag != GaiaGeoGeometry.GAIA_UNKNOWN);
            //Debug.Assert(geometryTypeFlag > 0);

            var cflag = ((int)geometryTypeFlag) & (0x7fffff00);
            if (cflag == CoordinateFlag)
                return;

            CoordinateFlag = cflag;

            var tmp = CoordinateFlag & 0xff00;
            HasZ = (tmp == 1000) || (tmp == 3000);
            HasM = (tmp == 2000) || (tmp == 3000);

            Dimension = GaiaDimensionModels.GAIA_XY;
            if (HasZ) Dimension |= GaiaDimensionModels.GAIA_Z;
            if (HasM) Dimension |= GaiaDimensionModels.GAIA_M;

            Compressed = (CoordinateFlag > 1000000);
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

        public GaiaDimensionModels Dimension { get; private set; }
        
        public bool HasZ{ get; private set; }
        
        public bool HasM{ get; private set; }

        public bool Compressed{ get; private set; }

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
    internal delegate Single GetSingleFunction(byte[] buffer, ref int offset);
    internal delegate Int32 GetInt32Function(byte[] buffer, ref int offset);

    internal class GaiaImport : GaiaGeoIO
    {
        public readonly GetDoubleFunction GetDouble;
        public readonly GetSingleFunction GetSingle;
        public readonly GetInt32Function GetInt32;

        internal static GaiaImport Create(bool conversionNeeded)
        {
            return conversionNeeded 
                ? new GaiaImport(GetConvertedDouble, GetConvertedSingle, GetConvertedInt32) 
                : new GaiaImport(GetUnconvertedDouble, GetUnconvertedSingle, GetUnconvertedInt32);
        }

        private GaiaImport(GetDoubleFunction getDouble, GetSingleFunction getSingle, GetInt32Function getInt32)
            :this(0, getDouble, getSingle, getInt32)
        {}
        private GaiaImport(GaiaGeoGeometry geometryType, GetDoubleFunction getDouble, GetSingleFunction getSingle, GetInt32Function getInt32)
            :base(geometryType)
        {
            GetDouble = getDouble;
            GetSingle = getSingle;
            GetInt32 = getInt32;
        }

        public static readonly GaiaImport NoConversion = new GaiaImport(GetUnconvertedDouble, GetUnconvertedSingle,
                                                                        GetUnconvertedInt32);

        public static readonly GaiaImport Conversion = new GaiaImport(GetConvertedDouble, GetConvertedSingle,
                                                                      GetConvertedInt32);


        #region Double

        private static Double GetUnconvertedDouble(byte[] buffer, ref int offset)
        {
            var val = BitConverter.ToDouble(buffer, offset);
            offset += 8;
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

        #endregion

        #region Single

        private static Single GetUnconvertedSingle(byte[] buffer, ref int offset)
        {
            var val = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            return val;
        }

        private static Single GetConvertedSingle(byte[] buffer, ref int offset)
        {
            var tmp = new byte[8];
            Buffer.BlockCopy(buffer, offset, tmp, 0, 4);
            Array.Reverse(tmp);
            offset += 4;
            return BitConverter.ToSingle(tmp, 0);
        }

        #endregion

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

        #endregion
    }

#endregion

#region Export

    internal delegate void WriteDoubleFunction(Double value, BinaryWriter bw);
    internal delegate void WriteInt32Function(Int32 value, BinaryWriter bw);
    internal delegate void WriteSingleFunction(Single value, BinaryWriter bw);

    internal class GaiaExport : GaiaGeoIO
    {
        public readonly WriteDoubleFunction WriteDouble;
        public readonly WriteSingleFunction WriteSingle;
        public readonly WriteInt32Function WriteInt32;

        private GaiaExport(WriteDoubleFunction writeDouble, WriteSingleFunction writeSingle,
                           WriteInt32Function writeInt32)
            :this(0, writeDouble, writeSingle, writeInt32)
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

        private static void WriteUnconvertedDouble(Double value, BinaryWriter bw)
        {
            bw.Write(value);
        }

        private static void WriteConvertedDouble(Double value, BinaryWriter bw)
        {
            var tmp = BitConverter.GetBytes(value);
            Array.Reverse(tmp);
            bw.Write(tmp);
        }

        #endregion

        #region Single

        private static void WriteUnconvertedSingle(Single value, BinaryWriter bw)
        {
            bw.Write(value);
        }

        private static void WriteConvertedSingle(Single value, BinaryWriter bw)
        {
            var tmp = BitConverter.GetBytes(value);
            Array.Reverse(tmp);
            bw.Write(tmp);
        }

        #endregion

        #region Int32

        internal static void WriteUnconvertedInt32(Int32 value, BinaryWriter bw)
        {
            bw.Write(value);
        }

        private static void WriteConvertedInt32(Int32 value, BinaryWriter bw)
        {
            var tmp = BitConverter.GetBytes(value);
            Array.Reverse(tmp);
            bw.Write(tmp);
        }

        #endregion

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
        #endregion
}        
