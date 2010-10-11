// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

namespace NetTopologySuite.IO.WellKnownBinary
{
    /// <summary>
    /// Enumeration to determine geometry type in a Well-Known Binary String.
    /// </summary>
    /// <remarks>
    /// <para>
    /// From section 8.2.3, A common list of codes for geometric types
    /// OpenGIS® Implementation Specification for Geographic information - Simple feature access - Part 1: Common architecture
    /// Reference Number: OGC 06-103r3; Version: 1.2.0
    /// </para>
    /// </remarks>
    internal enum WkbGeometryType : uint
    {
        Point = 1,
        LineString = 2,
        Polygon = 3,
        MultiPoint = 4,
        MultiLineString = 5,
        MultiPolygon = 6,
        GeometryCollection = 7,
        //CircularString = 8,
        //CompoundCurve = 9,
        //CurvePolygon = 10,
        MultiCurve = 11,
        MultiSurface = 12,
        Curve = 13,
        Surface = 14,
        PolyhedralSurface = 15,
        Tin = 16,
        GeometryZ = 1000,
        PointZ = 1001,
        LineStringZ = 1002,
        PolygonZ = 1003,
        MultiPointZ = 1004,
        MultiLineStringZ = 1005,
        MultiPolygonZ = 1006,
        GeometryCollectionZ = 1007,
        //CircularStringZ = 1008,
        //CompoundCurveZ = 1009,
        //CurvePolygonZ = 1010,
        MultiCurveZ = 1011,
        MultiSurfaceZ = 1012,
        CurveZ = 1013,
        SurfaceZ = 1014,
        PolyhedralSurfaceZ = 1015,
        TinZ = 1016,
        PointM = 2001,
        LineStringM = 2002,
        PolygonM = 2003,
        MultiPointM = 2004,
        MultiLineStringM = 2005,
        MultiPolygonM = 2006,
        GeometryCollectionM = 2007,
        //CircularStringM = 2008,
        //CompoundCurveM = 2009,
        //CurvePolygonM = 2010,
        MultiCurveM = 2011,
        MultiSurfaceM = 2012,
        CurveM = 2013,
        SurfaceM = 2014,
        PolyhedralSurfaceM = 2015,
        TinM = 2016,
        GeometryZM = 3000,
        PointZM = 3001,
        LineStringZM = 3002,
        PolygonZM = 3003,
        MultiPointZM = 3004,
        MultiLineStringZM = 3005,
        MultiPolygonZM = 3006,
        GeometryCollectionZM = 3007,
        //CircularStringZM = 3008,
        //CompoundCurveZM = 3009,
        //CurvePolygonZM = 3010,
        MultiCurveZM = 3011,
        MultiSurfaceZM = 3012,
        CurveZM = 3013,
        SurfaceZM = 3014,
        PolyhedralSurfaceZM = 3015,
        TinZM = 3016,
    }
}
