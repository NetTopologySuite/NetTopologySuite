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

using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using NPack;

namespace NetTopologySuite.IO.WellKnownBinary
{
    /// <summary>
    /// Converts a <see cref="IGeometry"/> instance 
    /// to a Well-Known Binary String representation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Well-Known Binary Representation for <see cref="IGeometry"/> 
    /// (WKBGeometry) provides a portable representation of a <see cref="IGeometry"/> 
    /// value as a contiguous stream of bytes. It permits <see cref="IGeometry"/> 
    /// values to be exchanged between an ODBC client and an SQL database in binary form.
    /// </para>
    /// <para>
    /// The Well-Known Binary Representation for <see cref="IGeometry"/> 
    /// is obtained by serializing a <see cref="IGeometry"/>
    /// instance as a sequence of numeric types drawn from the set {Unsigned Integer, Double} and
    /// then serializing each numeric type as a sequence of bytes using one of two well defined,
    /// standard, binary representations for numeric types (NDR, XDR). The specific binary encoding
    /// (NDR or XDR) used for a geometry Byte stream is described by a one Byte tag that precedes
    /// the serialized bytes. The only difference between the two encodings of geometry is one of
    /// Byte order, the XDR encoding is Big Endian, the NDR encoding is Little Endian.</para>
    /// </remarks> 
    internal static class GeometryToWkb
    {
        /// <summary>
        /// Encodes a <see cref="IGeometry"/> to Well-Known Binary format
        /// and writes it to a Byte array using little endian Byte encoding.
        /// </summary>
        /// <param name="g">The geometry to encode as WKB.</param>
        /// <returns>WKB representation of the geometry.</returns>
        public static Byte[] Write(IGeometry g)
        {
            return Write(g, WkbByteOrder.Ndr);
        }

        /// <summary>
        /// Encodes a <see cref="IGeometry"/> to Well-Known Binary format
        /// and writes it to a Byte array using the specified encoding.
        /// </summary>
        /// <param name="g">The geometry to encode as WKB.</param>
        /// <param name="wkbByteOrder">Byte order to encode values in.</param>
        /// <returns>WKB representation of the geometry.</returns>
        public static Byte[] Write(IGeometry g, WkbByteOrder wkbByteOrder)
        {
            if (g == null) throw new ArgumentNullException("g");

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					//Write the byteOrder format.
					bw.Write((Byte)wkbByteOrder);

					//Write the type of this geometry
					writeType(g, bw, wkbByteOrder);

					//Write the geometry
					writeGeometry(g, bw, wkbByteOrder);
				}
				return ms.ToArray();
			}
        }

        #region Private helper methods

        /// <summary>
        /// Writes the type number for this geometry.
        /// </summary>
        /// <param name="geometry">The geometry to determine the type of.</param>
        /// <param name="writer">Binary Writer</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeType(IGeometry geometry, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Points are type 1.
            if (geometry is IPoint)
            {
                writeUInt32((UInt32)WkbGeometryType.Point, writer, byteOrder);
            }
            //Linestrings are type 2.
            else if (geometry is ILineString)
            {
                writeUInt32((UInt32)WkbGeometryType.LineString, writer, byteOrder);
            }
            //Polygons are type 3.
            else if (geometry is IPolygon)
            {
                writeUInt32((UInt32)WkbGeometryType.Polygon, writer, byteOrder);
            }
            //Mulitpoints are type 4.
            else if (geometry is IMultiPoint)
            {
                writeUInt32((UInt32)WkbGeometryType.MultiPoint, writer, byteOrder);
            }
            //Multilinestrings are type 5.
            else if (geometry is IMultiLineString)
            {
                writeUInt32((UInt32)WkbGeometryType.MultiLineString, writer, byteOrder);
            }
            //Multipolygons are type 6.
            else if (geometry is IMultiPolygon)
            {
                writeUInt32((UInt32)WkbGeometryType.MultiPolygon, writer, byteOrder);
            }
            //Geometrycollections are type 7.
            else if (geometry is IGeometryCollection)
            {
                writeUInt32((UInt32)WkbGeometryType.GeometryCollection, writer, byteOrder);
            }
            //If the type is not of the above 7 throw an exception.
            else
            {
                throw new ArgumentException("Invalid Geometry Type");
            }
        }

        /// <summary>
        /// Writes the geometry to the binary writer.
        /// </summary>
        /// <param name="geometry">The geometry to be written.</param>
        /// <param name="bWriter"></param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeGeometry(IGeometry geometry, BinaryWriter bWriter, WkbByteOrder byteOrder)
        {
            //Write the point.
            if (geometry is IPoint)
            {
                writePoint((IPoint)geometry, bWriter, byteOrder);
            }
            else if (geometry is ILineString)
            {
                ILineString ls = (ILineString)geometry;
                writeLineString(ls, bWriter, byteOrder);
            }
            else if (geometry is IPolygon)
            {
                writePolygon((IPolygon)geometry, bWriter, byteOrder);
            }
            //Write the Multipoint.
            else if (geometry is IMultiPoint)
            {
                writeMultiPoint((IMultiPoint)geometry, bWriter, byteOrder);
            }
            //Write the Multilinestring.
            else if (geometry is IMultiLineString)
            {
                writeMultiLineString((IMultiLineString)geometry, bWriter, byteOrder);
            }
            //Write the Multipolygon.
            else if (geometry is IMultiPolygon)
            {
                writeMultiPolygon((IMultiPolygon)geometry, bWriter, byteOrder);
            }
            //Write the Geometrycollection.
            else if (geometry is IGeometryCollection)
            {
                writeGeometryCollection((IGeometryCollection)geometry, bWriter, byteOrder);
            }
            //If the type is not of the above 7 throw an exception.
            else
            {
                throw new ArgumentException("Invalid Geometry Type");
            }
        }

        /// <summary>
        /// Writes an <see cref="ICoordinate"/> instance.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeCoordinate(ICoordinate coordinate, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //foreach (DoubleComponent component in coordinate)
            //{
            //    writeDouble((Double)component, writer, byteOrder);
            //}

            //FObermaier: Limit WKB to 2D coordinate
            DoubleComponent x, y;
            coordinate.GetComponents(out x, out y);
            writeDouble((Double)x, writer, byteOrder);
            writeDouble((Double)y, writer, byteOrder);

        }

        /// <summary>
        /// Writes a Point.
        /// </summary>
        /// <param name="point">The point to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writePoint(IPoint point, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            writeCoordinate(point.Coordinate, writer, byteOrder);
        }

        /// <summary>
        /// Writes a LineString.
        /// </summary>
        /// <param name="ls">The linestring to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeLineString(ILineString ls, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Write the number of points in this linestring.
            writeUInt32((UInt32)ls.PointCount, writer, byteOrder);

            //Loop on each vertices.
            foreach (ICoordinate p in ls.Coordinates)
            {
                writeCoordinate(p, writer, byteOrder);
            }
        }


        /// <summary>
        /// Writes a polygon.
        /// </summary>
        /// <param name="poly">The polygon to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writePolygon(IPolygon poly, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Get the number of rings in this polygon.
            Int32 numRings = poly.InteriorRingsCount + 1;

            //Write the number of rings to the stream (add one for the shell)
            writeUInt32((UInt32)numRings, writer, byteOrder);

            //Write the exterior of this polygon.
            writeLineString(poly.ExteriorRing, writer, byteOrder);

            //Loop on the number of rings - 1 because we already wrote the shell.
            foreach (ILinearRing lr in poly.InteriorRings)
            {
                //Write the (lineString)LinearRing.
                writeLineString(lr, writer, byteOrder);
            }
        }

        /// <summary>
        /// Writes a multipoint.
        /// </summary>
        /// <param name="mp">The multipoint to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeMultiPoint(IMultiPoint mp, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Write the number of points.
            writeUInt32((UInt32)mp.Count, writer, byteOrder);

            //Loop on the number of points.
            foreach (IPoint p in (mp as IEnumerable<IPoint>))
            {
                //Write Points Header
                writer.Write((Byte)byteOrder);
                writeUInt32((UInt32)WkbGeometryType.Point, writer, byteOrder);
                //Write each point.
                writePoint(p, writer, byteOrder);
            }
        }

        /// <summary>
        /// Writes a multilinestring.
        /// </summary>
        /// <param name="mls">The multilinestring to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeMultiLineString(IMultiLineString mls, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Write the number of linestrings.
            writeUInt32((UInt32)mls.Count, writer, byteOrder);

            //Loop on the number of linestrings.
            foreach (ILineString ls in (mls as IEnumerable<ILineString>))
            {
                //Write LineString Header
                writer.Write((Byte)byteOrder);
                writeUInt32((UInt32)WkbGeometryType.LineString, writer, byteOrder);
                //Write each linestring.
                writeLineString(ls, writer, byteOrder);
            }
        }

        /// <summary>
        /// Writes a multipolygon.
        /// </summary>
        /// <param name="mp">The mulitpolygon to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeMultiPolygon(IMultiPolygon mp, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Write the number of polygons.
            writeUInt32((UInt32)mp.Count, writer, byteOrder);

            //Loop on the number of polygons.
            foreach (IPolygon poly in (mp as IEnumerable<IPolygon>))
            {
                //Write polygon header
                writer.Write((Byte)byteOrder);
                writeUInt32((UInt32)WkbGeometryType.Polygon, writer, byteOrder);
                //Write each polygon.
                writePolygon(poly, writer, byteOrder);
            }
        }


        /// <summary>
        /// Writes a GeometryCollection instance.
        /// </summary>
        /// <param name="gc">The GeometryCollection to be written.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeGeometryCollection(IGeometryCollection gc, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            //Get the number of geometries in this geometrycollection.
            Int32 count = gc.Count;

            //Write the number of geometries.
            writeUInt32((UInt32)count, writer, byteOrder);

            //Loop on the number of geometries.
            for (Int32 i = 0; i < count; i++)
            {
                //Write the Byte-order format of the following geometry.
                writer.Write((Byte)byteOrder);
                //Write the type of each geometry.
                writeType(gc[i], writer, byteOrder);
                //Write each geometry.
                writeGeometry(gc[i], writer, byteOrder);
            }
        }

        /// <summary>
        /// Writes an unsigned 32-bit integer to the BinaryWriter using the specified Byte encoding.
        /// </summary>
        /// <param name="value">Value to write.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeUInt32(UInt32 value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                writer.Write(ByteEncoder.GetBigEndian(value));
            }
            else
            {
                writer.Write(ByteEncoder.GetLittleEndian(value));
            }
        }

        /// <summary>
        /// Writes a Double floating point value to the BinaryWriter using the specified Byte encoding.
        /// </summary>
        /// <param name="value">Value to write.</param>
        /// <param name="writer">Writer to persist WKB values to.</param>
        /// <param name="byteOrder">Byte order to encode values in.</param>
        private static void writeDouble(Double value, BinaryWriter writer, WkbByteOrder byteOrder)
        {
            if (byteOrder == WkbByteOrder.Xdr)
            {
                writer.Write(ByteEncoder.GetBigEndian(value));
            }
            else
            {
                writer.Write(ByteEncoder.GetLittleEndian(value));
            }
        }
        #endregion
    }
}
