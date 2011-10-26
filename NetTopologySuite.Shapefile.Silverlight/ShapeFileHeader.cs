// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Extension;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shapefile
{
    internal class ShapeFileHeader
    {
        private readonly IGeometryFactory _geoFactory;
        private ShapeType _shapeType;
        private IEnvelope _extents;
        private Int32 _fileLengthInWords;

        public ShapeFileHeader(BinaryReader reader, IGeometryFactory geoFactory)
        {
            _geoFactory = geoFactory;
            parseHeader(reader);
        }

        public override String ToString()
        {
            return String.Format("Shapefile header - ShapeType: {0}; " +
                                 "Envelope: {1}; FileLengthInWords: {2}",
                                 ShapeType, Extents, FileLengthInWords);
        }

        public ShapeType ShapeType
        {
            get { return _shapeType; }
            private set { _shapeType = value; }
        }

        public IEnvelope Extents
        {
            get { return _extents; }
            set { _extents = value; }
        }

        public Int32 FileLengthInWords
        {
            get { return _fileLengthInWords; }
            set { _fileLengthInWords = value; }
        }

        public void WriteHeader(BinaryWriter writer)
        {
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(ByteEncoder.GetBigEndian(ShapeFileConstants.HeaderStartCode));
            writer.Write(new Byte[20]);
            writer.Write(ByteEncoder.GetBigEndian(FileLengthInWords));
            writer.Write(ByteEncoder.GetLittleEndian(ShapeFileConstants.VersionCode));
            writer.Write(ByteEncoder.GetLittleEndian((Int32)ShapeType));
            writer.Write(ByteEncoder.GetLittleEndian(Extents.GetMin(Ordinate.X)));
            writer.Write(ByteEncoder.GetLittleEndian(Extents.GetMin(Ordinate.Y)));
            writer.Write(ByteEncoder.GetLittleEndian(Extents.GetMax(Ordinate.X)));
            writer.Write(ByteEncoder.GetLittleEndian(Extents.GetMax(Ordinate.Y)));
            writer.Write(new Byte[32]); // Z-values and M-values
        }

        #region File parsing helpers
        /// <summary>
        /// Reads and parses the header of the .shp index file
        /// </summary>
        /// <remarks>
        /// From ESRI ShapeFile Technical Description document
        /// 
        /// http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
        /// 
        /// Byte
        /// Position    Field           Value       Type    Order
        /// -----------------------------------------------------
        /// Byte 0      File Code       9994        Integer Big
        /// Byte 4      Unused          0           Integer Big
        /// Byte 8      Unused          0           Integer Big
        /// Byte 12     Unused          0           Integer Big
        /// Byte 16     Unused          0           Integer Big
        /// Byte 20     Unused          0           Integer Big
        /// Byte 24     File Length     File Length Integer Big
        /// Byte 28     Version         1000        Integer Little
        /// Byte 32     Shape Type      Shape Type  Integer Little
        /// Byte 36     Bounding Box    Xmin        Double  Little
        /// Byte 44     Bounding Box    Ymin        Double  Little
        /// Byte 52     Bounding Box    Xmax        Double  Little
        /// Byte 60     Bounding Box    Ymax        Double  Little
        /// Byte 68*    Bounding Box    Zmin        Double  Little
        /// Byte 76*    Bounding Box    Zmax        Double  Little
        /// Byte 84*    Bounding Box    Mmin        Double  Little
        /// Byte 92*    Bounding Box    Mmax        Double  Little
        /// 
        /// * Unused, with value 0.0, if not Measured or Z type
        /// 
        /// The "Integer" type corresponds to the CLS Int32 type, and "Double" to CLS Double (IEEE 754).
        /// </remarks>
        private void parseHeader(BinaryReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            // Check file header
            if (ByteEncoder.GetBigEndian(reader.ReadInt32()) != ShapeFileConstants.HeaderStartCode)
            {
                throw new ShapeFileIsInvalidException("Invalid ShapeFile (.shp)");
            }

            // Seek to File Length
            reader.BaseStream.Seek(24, 0);

            // Read filelength as big-endian. The length is number of 16-bit words in file
            FileLengthInWords = ByteEncoder.GetBigEndian(reader.ReadInt32());

            // Seek to ShapeType
            reader.BaseStream.Seek(32, 0);
            ShapeType = (ShapeType)reader.ReadInt32();

            // Seek to bounding box of shapefile
            reader.BaseStream.Seek(36, 0);

            // Read the spatial bounding box of the contents
            Double xMin = ByteEncoder.GetLittleEndian(reader.ReadDouble());
            Double yMin = ByteEncoder.GetLittleEndian(reader.ReadDouble());
            Double xMax = ByteEncoder.GetLittleEndian(reader.ReadDouble());
            Double yMax = ByteEncoder.GetLittleEndian(reader.ReadDouble());

            var min = new Coordinate(xMin, yMin);
            var max = new Coordinate(xMax, yMax);

            Extents = min.Equals(max) && min.Equals(new Coordinate(0, 0)) //jd: if the shapefile has just been created the box wil be 0,0,0,0 in this case create an empty extents
                ? new Envelope()
                : new Envelope(min, max);


            //jd:allow exmpty extents
            //if (Extents.IsEmpty)
            //{
            //    Extents = null;
            //}
        }
        #endregion

        private Int32 computeMainFileLengthInWords(ShapeFileIndex index)
        {
            Int32 length = ShapeFileConstants.HeaderSizeBytes / 2;

            foreach (KeyValuePair<UInt32, ShapeFileIndex.IndexEntry> kvp in index)
            {
                length += kvp.Value.Length + ShapeFileConstants.ShapeRecordHeaderByteLength / 2;
            }

            return length;
        }

        private Int32 computeIndexFileLengthInWords(ShapeFileIndex index)
        {
            Int32 length = ShapeFileConstants.HeaderSizeBytes / 2;

            length += index.Count * ShapeFileConstants.IndexRecordByteLength / 2;

            return length;
        }
    }
}
