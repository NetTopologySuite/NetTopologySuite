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

namespace NetTopologySuite.IO
{
    /*
internal static class GaiaGeoEmptyHelper
{
    internal static readonly byte[] GeometryCollectionEmpty =
        new byte[]
            {
                (byte) GaiaGeoBlobMark.GAIA_MARK_START,
                (byte) GaiaGeoEndianMarker.GAIA_LITTLE_ENDIAN,
                0, 0, 0, 0, 7, 0, 0, 0,
                (byte) GaiaGeoBlobMark.GAIA_MARK_END
            };

    internal static bool IsEmptyBlob(byte[] blob)
    {
        if (GeometryCollectionEmpty.Length != blob.Length)
            return false;

        if (GeometryCollectionEmpty[0] != blob[0])
            return false;
        if (GeometryCollectionEmpty[1] != blob[1])
            return false;

        //SRID
        for (var i = 6; i < 11; i++)
            if (GeometryCollectionEmpty[i] != blob[i])
                return false;
            
        return true;
    }

    internal static byte[] EmptyGeometryCollectionWithSrid(int srid)
    {
        var ret = (byte[])GeometryCollectionEmpty.Clone();
        var sridbytes = BitConverter.GetBytes(srid);
        Buffer.BlockCopy(sridbytes, 0, ret, 2, 4);
        return ret;
    }
}
*/
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Defines byte storage order
    /// </summary>
    internal enum GaiaGeoEndianMarker : byte
    {
        GAIA_BIG_ENDIAN = 0,
        GAIA_LITTLE_ENDIAN = 1
    }

    /// <summary>
    /// Generic geometry classes
    /// </summary>
    internal enum GaiaGeoGeometryEntity
    {
        GAIA_TYPE_NONE = 0,
        GAIA_TYPE_POINT = 1,
        GAIA_TYPE_LINESTRING = 2,
        GAIA_TYPE_POLYGON = 3,
    }

    /// <summary>
    /// Special markers used for encoding of SpatiaLite internal BLOB geometries
    /// </summary>
    internal enum GaiaGeoBlobMark : byte
    {
        GAIA_MARK_START = 0x00,
        GAIA_MARK_END = 0xFE,
        GAIA_MARK_MBR = 0x7C,
        GAIA_MARK_ENTITY = 0x69
    }

    [Flags]
    internal enum GaiaDimensionModels : byte
    {
        GAIA_XY = 1,
        GAIA_Z = 2,
        GAIA_M = 4,
        GAIA_XY_Z = GAIA_XY | GAIA_Z,
        GAIA_XY_M = GAIA_XY | GAIA_M,
        GAIA_XY_Z_M = GAIA_XY | GAIA_Z | GAIA_M
    }

    /// <summary>
    /// Defines Geometry classes
    /// </summary>
    internal enum GaiaGeoGeometry
    {

        GAIA_UNKNOWN = 0,
        GAIA_POINT = 1,
        GAIA_LINESTRING = 2,
        GAIA_POLYGON = 3,
        GAIA_MULTIPOINT = 4,
        GAIA_MULTILINESTRING = 5,
        GAIA_MULTIPOLYGON = 6,
        GAIA_GEOMETRYCOLLECTION = 7,
        GAIA_POINTZ = 1001,
        GAIA_LINESTRINGZ = 1002,
        GAIA_POLYGONZ = 1003,
        GAIA_MULTIPOINTZ = 1004,
        GAIA_MULTILINESTRINGZ = 1005,
        GAIA_MULTIPOLYGONZ = 1006,
        GAIA_GEOMETRYCOLLECTIONZ = 1007,
        GAIA_POINTM = 2001,
        GAIA_LINESTRINGM = 2002,
        GAIA_POLYGONM = 2003,
        GAIA_MULTIPOINTM = 2004,
        GAIA_MULTILINESTRINGM = 2005,
        GAIA_MULTIPOLYGONM = 2006,
        GAIA_GEOMETRYCOLLECTIONM = 2007,
        GAIA_POINTZM = 3001,
        GAIA_LINESTRINGZM = 3002,
        GAIA_POLYGONZM = 3003,
        GAIA_MULTIPOINTZM = 3004,
        GAIA_MULTILINESTRINGZM = 3005,
        GAIA_MULTIPOLYGONZM = 3006,
        GAIA_GEOMETRYCOLLECTIONZM = 3007,

        /* constants that defines Compressed GEOMETRY CLASSes */
        GAIA_COMPRESSED_LINESTRING = 1000002,
        GAIA_COMPRESSED_POLYGON = 1000003,
        GAIA_COMPRESSED_LINESTRINGZ = 1001002,
        GAIA_COMPRESSED_POLYGONZ = 1001003,
        GAIA_COMPRESSED_LINESTRINGM = 1002002,
        GAIA_COMPRESSED_POLYGONM = 1002003,
        GAIA_COMPRESSED_LINESTRINGZM = 1003002,
        GAIA_COMPRESSED_POLYGONZM = 1003003,


        // /* constants that defines GEOS-WKB 3D CLASSes */
        //GAIA_GEOSWKB_POINTZ			-2147483647
        //GAIA_GEOSWKB_LINESTRINGZ		-2147483646
        //GAIA_GEOSWKB_POLYGONZ			-2147483645
        //GAIA_GEOSWKB_MULTIPOINTZ		-2147483644
        //GAIA_GEOSWKB_MULTILINESTRINGZ		-2147483643
        //GAIA_GEOSWKB_MULTIPOLYGONZ		-2147483642
        //GAIA_GEOSWKB_GEOMETRYCOLLECTIONZ	-2147483641

        // /* constants that defines multitype values */
        //GAIA_NULL_VALUE		0
        //GAIA_TEXT_VALUE		1
        //GAIA_INT_VALUE		2
        //GAIA_DOUBLE_VALUE	3

        // /* constants that defines POINT index for LINESTRING */
        //GAIA_START_POINT	1
        //GAIA_END_POINT		2
        //GAIA_POINTN		3

    }
    // ReSharper restore InconsistentNaming
}