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
using System.Diagnostics;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using NPack.Interfaces;
#if DOTNET35
using Enumerable = System.Linq.Enumerable;
#else

#endif

namespace NetTopologySuite.IO.WellKnownBinary
{
    /// <summary>
    /// Converts Well-Known Binary representations to a 
    /// <see cref="IGeometry"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Well-Known Binary Representation for <see cref="IGeometry"/> 
    /// (WkbGeometry) provides a portable 
    /// representation of a <see cref="IGeometry"/> value as a contiguous stream of bytes. 
    /// It permits <see cref="IGeometry"/> 
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
    /// Byte order, the XDR encoding is Big Endian, the NDR encoding is Little Endian.
    /// </para>
    /// </remarks> 
    internal static class GeometryFromWkb
    {
        /// <summary>
        /// Creates an <see cref="IGeometry{TCoordinate}"/> instance from the 
        /// supplied <see cref="Byte"/> array containing the Well-Known Binary representation.
        /// </summary>
        /// <typeparam name="TCoordinate">Type of coordinate to use.</typeparam>
        /// <param name="bytes">
        /// A <see cref="Byte"/> array containing the geometry encoded in 
        /// the Well-Known Binary representation.
        /// </param>
        /// <param name="factory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> to use to create the 
        /// <see cref="IGeometry{TCoordinate}"/> instance.
        /// </param>
        /// <returns>
        /// A <see cref="IGeometry"/> created from on supplied 
        /// Well-Known Binary representation.
        /// </returns>
        public static IGeometry<TCoordinate> Parse<TCoordinate>(Byte[] bytes,
                                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Create a memory stream using the suppiled Byte array.
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                // Create a new binary reader using the newly created memory stream.
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // Call the main create function.
                    return Parse(reader, factory);
                }
            }
        }

        /// <summary>
        /// Parses a stream of data encoded as Well-Known Binary to generate an 
        /// <see cref="IGeometry{TCoordinate}"/> instance.
        /// </summary>
        /// <typeparam name="TCoordinate">Type of coordinate to use.</typeparam>
        /// <param name="data">
        /// A stream containing a sequence of bytes representing a Well-Known Binary
        /// encoded geometry.
        /// </param>
        /// <param name="factory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> to use to create the 
        /// <see cref="IGeometry{TCoordinate}"/> instance.
        /// </param>
        /// <returns>
        /// A <see cref="IGeometry"/> created from on supplied 
        /// Well-Known Binary representation.
        /// </returns>
        public static IGeometry<TCoordinate> Parse<TCoordinate>(Stream data,
                                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Create a new binary reader using the newly created memory stream.
            using (BinaryReader reader = new BinaryReader(data))
            {
                // Call the main create function.
                return Parse(reader, factory);
            }
        }

        /// <summary>
        /// Creates a <see cref="IGeometry{TCoordinate}"/> encoded as 
        /// Well-Known Binary representation to be read from <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="TCoordinate">Type of coordinate to use.</typeparam>
        /// <param name="reader">
        /// A <see cref="System.IO.BinaryReader"/> used to read the 
        /// Well-Known Binary encoded geometry.
        /// </param>
        /// <param name="factory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> to use to create the 
        /// <see cref="IGeometry{TCoordinate}"/> instance.
        /// </param>
        /// <returns>
        /// A <see cref="IGeometry{TCoordinate}"/> created from the Well-Known 
        /// Binary representation.
        /// </returns>
        public static IGeometry<TCoordinate> Parse<TCoordinate>(BinaryReader reader,
                                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Get the first Byte in the array.  This specifies if the WKB is in
            // XDR (big-endian) format of NDR (little-endian) format.
            Byte byteOrder = reader.ReadByte();

            if (byteOrder != (Byte) WkbByteOrder.Xdr && byteOrder != (Byte) WkbByteOrder.Ndr)
            {
                throw new ArgumentException("Byte order not recognized.");
            }

            WkbByteOrder wkbByteOrder = (WkbByteOrder) byteOrder;

            // Get the type of this geometry.
            UInt32 type = readUInt32(reader, wkbByteOrder);

            //int geometryType = typeInt & 0xff;
            //// determine if Z values are present
            //inputDimension = hasZ ? 3 : 2;
            //// determine if SRIDs are present
            //hasSRID = (typeInt & 0x20000000) != 0;

            //if (hasSRID)
            //{
            //    SRID = dis.readInt();
            //}
            WkbGeometryType geometryType = (WkbGeometryType)(type & 0xff);

            Boolean hasZ = (type & 0x80000000) != 0;
            Int32 inputDimension = hasZ ? 3 : 2;
            Boolean hasSRID = (type & 0x20000000) != 0;
            Int32 srid = -1;
            if (hasSRID)
                srid = (Int32)readUInt32(reader, wkbByteOrder);


            switch (geometryType)
            {
                case WkbGeometryType.Point:
                    return createWkbPoint(reader, 
                                          wkbByteOrder, 
                                          factory, 
                                          Ordinates.X, 
                                          Ordinates.Y);
                case WkbGeometryType.PointM:
                    return createWkbPoint(reader,
                                          wkbByteOrder,
                                          factory,
                                          Ordinates.X,
                                          Ordinates.Y,
                                          Ordinates.M);
                case WkbGeometryType.PointZ:
                    return createWkbPoint(reader,
                                          wkbByteOrder,
                                          factory,
                                          Ordinates.X,
                                          Ordinates.Y,
                                          Ordinates.Z);
                case WkbGeometryType.PointZM:
                    return createWkbPoint(reader,
                                          wkbByteOrder,
                                          factory,
                                          Ordinates.X,
                                          Ordinates.Y,
                                          Ordinates.Z,
                                          Ordinates.M);
                case WkbGeometryType.LineString:
                    return createWkbLineString(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y);
                case WkbGeometryType.LineStringM:
                    return createWkbLineString(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y,
                                               Ordinates.M);
                case WkbGeometryType.LineStringZ:
                    return createWkbLineString(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y,
                                               Ordinates.Z);
                case WkbGeometryType.LineStringZM:
                    return createWkbLineString(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y,
                                               Ordinates.Z,
                                               Ordinates.M);
                case WkbGeometryType.Polygon:
                    return createWkbPolygon(reader, 
                                            wkbByteOrder, 
                                            factory, 
                                            Ordinates.X, 
                                            Ordinates.Y);
                case WkbGeometryType.PolygonM:
                    return createWkbPolygon(reader,
                                            wkbByteOrder,
                                            factory,
                                            Ordinates.X,
                                            Ordinates.Y,
                                            Ordinates.M);
                case WkbGeometryType.PolygonZ:
                    return createWkbPolygon(reader,
                                            wkbByteOrder,
                                            factory,
                                            Ordinates.X,
                                            Ordinates.Y,
                                            Ordinates.Z);
                case WkbGeometryType.PolygonZM:
                    return createWkbPolygon(reader,
                                            wkbByteOrder,
                                            factory,
                                            Ordinates.X,
                                            Ordinates.Y,
                                            Ordinates.Z,
                                            Ordinates.M);

                case WkbGeometryType.MultiPoint:
                    return createWkbMultiPoint(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y);
                case WkbGeometryType.MultiPointM:
                    return createWkbMultiPoint(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y,
                                               Ordinates.M);
                case WkbGeometryType.MultiPointZ:
                    return createWkbMultiPoint(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y,
                                               Ordinates.Z);
                case WkbGeometryType.MultiPointZM:
                    return createWkbMultiPoint(reader,
                                               wkbByteOrder,
                                               factory,
                                               Ordinates.X,
                                               Ordinates.Y,
                                               Ordinates.Z,
                                               Ordinates.M);

                case WkbGeometryType.MultiLineString:
                    return createWkbMultiLineString(reader,
                                                    wkbByteOrder,
                                                    factory,
                                                    Ordinates.X,
                                                    Ordinates.Y);
                case WkbGeometryType.MultiLineStringM:
                    return createWkbMultiLineString(reader,
                                                    wkbByteOrder,
                                                    factory,
                                                    Ordinates.X,
                                                    Ordinates.Y,
                                                    Ordinates.M);
                case WkbGeometryType.MultiLineStringZ:
                    return createWkbMultiLineString(reader,
                                                    wkbByteOrder,
                                                    factory,
                                                    Ordinates.X,
                                                    Ordinates.Y,
                                                    Ordinates.Z);
                case WkbGeometryType.MultiLineStringZM:
                    return createWkbMultiLineString(reader,
                                                    wkbByteOrder,
                                                    factory,
                                                    Ordinates.X,
                                                    Ordinates.Y,
                                                    Ordinates.Z,
                                                    Ordinates.M);

                case WkbGeometryType.MultiPolygon:
                    return createWkbMultiPolygon(reader,
                                                 wkbByteOrder,
                                                 factory,
                                                 Ordinates.X,
                                                 Ordinates.Y);
                case WkbGeometryType.MultiPolygonM:
                    return createWkbMultiPolygon(reader,
                                                 wkbByteOrder,
                                                 factory,
                                                 Ordinates.X,
                                                 Ordinates.Y,
                                                 Ordinates.M);
                case WkbGeometryType.MultiPolygonZ:
                    return createWkbMultiPolygon(reader,
                                                 wkbByteOrder,
                                                 factory,
                                                 Ordinates.X,
                                                 Ordinates.Y,
                                                 Ordinates.Z);
                case WkbGeometryType.MultiPolygonZM:
                    return createWkbMultiPolygon(reader,
                                                 wkbByteOrder,
                                                 factory,
                                                 Ordinates.X,
                                                 Ordinates.Y,
                                                 Ordinates.Z,
                                                 Ordinates.M);
                case WkbGeometryType.GeometryCollection:
                    return createWkbGeometryCollection(reader, wkbByteOrder, factory);

                default:
                    // even though Enum.IsDefined is a perf hit, we're about to
                    // throw an exception, which is an even bigger perf hit.
                    if (!Enum.IsDefined(typeof (WkbGeometryType), type))
                    {
                        throw new ArgumentException("Geometry type not recognized: " + type);
                    }

                    throw new NotSupportedException("Geometry type '" +
                                                    geometryType +
                                                    "' not supported");
            }
        }

        // TODO: implement this in the same way as the WKT parser implemented it.
        private static TCoordinate readCoordinate<TCoordinate>(BinaryReader reader,
                                                               WkbByteOrder byteOrder,
                                                               IGeometryFactory<TCoordinate> factory,
                                                               Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Int32 componentCount = ordinates.Length;
            Double[] components = new Double[componentCount];

            for (Int32 j = 0; j < componentCount; j++)
            {
                components[j] = readDouble(reader, byteOrder);
            }

            // Add the coordinate.
            TCoordinate coordinate;

            coordinate = Array.Exists(ordinates,
                                      delegate(Ordinates ordinate) { return ordinate == Ordinates.Z; })
                             ? factory.CoordinateFactory.Create3D(components)
                             : factory.CoordinateFactory.Create(components);

            return coordinate;
        }

        private static IEnumerable<TCoordinate> readCoordinates<TCoordinate>(BinaryReader reader,
                                                                             WkbByteOrder byteOrder,
                                                                             IGeometryFactory<TCoordinate> factory,
                                                                             Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Int32 count = (Int32) readUInt32(reader, byteOrder);

            // Create a new array of coordinates.
            TCoordinate[] coords = new TCoordinate[count];

            // Loop on the number of points in the ring.
            for (Int32 i = 0; i < count; i++)
            {
                coords[i] = readCoordinate(reader, byteOrder, factory, ordinates);
            }

            return coords;
        }

        private static IPoint<TCoordinate> createWkbPoint<TCoordinate>(BinaryReader reader,
                                                                       WkbByteOrder byteOrder,
                                                                       IGeometryFactory<TCoordinate> factory,
                                                                       params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Create and return the point.
            return factory.CreatePoint(readCoordinate(reader, byteOrder, factory, ordinates));
        }

        private static ILineString<TCoordinate> createWkbLineString<TCoordinate>(BinaryReader reader,
                                                                                 WkbByteOrder byteOrder,
                                                                                 IGeometryFactory<TCoordinate> factory,
                                                                                 params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IEnumerable<TCoordinate> coords
                = readCoordinates(reader, byteOrder, factory, ordinates);

            ILineString<TCoordinate> l = factory.CreateLineString(coords);

            return l;
        }

        private static ILinearRing<TCoordinate> createWkbLinearRing<TCoordinate>(BinaryReader reader,
                                                                                 WkbByteOrder byteOrder,
                                                                                 IGeometryFactory<TCoordinate> factory,
                                                                                 params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IEnumerable<TCoordinate> coordinates = readCoordinates(reader,
                                                                   byteOrder,
                                                                   factory,
                                                                   ordinates);

            TCoordinate first = Enumerable.First(coordinates);
            TCoordinate last = Enumerable.Last(coordinates);

            // If polygon isn't closed, add the first point to the end 
            // (this shouldn't occur for correct WKB data)
            if (!first.Equals(last))
            {
                coordinates = Slice.Append(coordinates, first.Clone());
            }

            return factory.CreateLinearRing(coordinates);
        }

        private static IPolygon<TCoordinate> createWkbPolygon<TCoordinate>(BinaryReader reader,
                                                                           WkbByteOrder byteOrder,
                                                                           IGeometryFactory<TCoordinate> factory,
                                                                           params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Get the Number of rings in this Polygon.
            Int32 ringCount = (Int32) readUInt32(reader, byteOrder);

            Debug.Assert(ringCount >= 1, "Number of rings in polygon must be 1 or more.");

            ILinearRing<TCoordinate> shell
                = createWkbLinearRing(reader, byteOrder, factory, ordinates);

            if (ringCount == 1)
            {
                return factory.CreatePolygon(shell);
            }

            // Create a new array of linearrings for the interior rings.
            List<ILinearRing<TCoordinate>> interiorRings
                = new List<ILinearRing<TCoordinate>>();

            for (Int32 i = 0; i < (ringCount - 1); i++)
            {
                interiorRings.Add(createWkbLinearRing(reader, byteOrder, factory, ordinates));
            }

            // Create and return the Poylgon.
            return factory.CreatePolygon(shell, interiorRings);
        }

        private static IMultiPoint<TCoordinate> createWkbMultiPoint<TCoordinate>(BinaryReader reader,
                                                                                 WkbByteOrder byteOrder,
                                                                                 IGeometryFactory<TCoordinate> factory,
                                                                                 params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Get the number of points in this multipoint.
            Int32 numPoints = (Int32) readUInt32(reader, byteOrder);

            // Create a new array for the points.
            IMultiPoint<TCoordinate> points = factory.CreateMultiPoint();

            // Loop on the number of points.
            for (Int32 i = 0; i < numPoints; i++)
            {
                // Read point header
                reader.ReadByte();
                readUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next point and add it to the point array.
                points.Add(createWkbPoint(reader, byteOrder, factory, ordinates));
            }

            return points;
        }

        private static IMultiLineString<TCoordinate> createWkbMultiLineString<TCoordinate>(BinaryReader reader,
                                                                                           WkbByteOrder byteOrder,
                                                                                           IGeometryFactory<TCoordinate> factory,
                                                                                           params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Get the number of linestrings in this multilinestring.
            Int32 count = (Int32) readUInt32(reader, byteOrder);

            // Create a new array for the linestrings .
            IMultiLineString<TCoordinate> mline = factory.CreateMultiLineString();

            // Loop on the number of linestrings.
            for (Int32 i = 0; i < count; i++)
            {
                // Read linestring header
                reader.ReadByte();
                readUInt32(reader, byteOrder);

                // Create the next linestring and add it to the array.
                mline.Add(createWkbLineString(reader, byteOrder, factory, ordinates));
            }

            // Create and return the MultiLineString.
            return mline;
        }

        private static IMultiPolygon<TCoordinate> createWkbMultiPolygon<TCoordinate>(BinaryReader reader,
                                                                                     WkbByteOrder byteOrder,
                                                                                     IGeometryFactory<TCoordinate> factory,
                                                                                     params Ordinates[] ordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // Get the number of Polygons.
            Int32 count = (Int32) readUInt32(reader, byteOrder);

            // Create a new array for the Polygons.
            IMultiPolygon<TCoordinate> polygons = factory.CreateMultiPolygon();

            // Loop on the number of polygons.
            for (Int32 i = 0; i < count; i++)
            {
                // read polygon header
                reader.ReadByte();
                readUInt32(reader, byteOrder);

                // TODO: Validate type

                // Create the next polygon and add it to the array.
                polygons.Add(createWkbPolygon(reader, byteOrder, factory, ordinates));
            }

            //Create and return the MultiPolygon.
            return polygons;
        }

        private static IGeometry<TCoordinate> createWkbGeometryCollection<TCoordinate>(BinaryReader reader,
                                                                                       WkbByteOrder byteOrder,
                                                                                       IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // The next Byte in the array tells the number of geometries in this collection.
            Int32 count = (Int32) readUInt32(reader, byteOrder);

            // Create a new array for the geometries.
            IGeometryCollection<TCoordinate> geometries = factory.CreateGeometryCollection();

            // Loop on the number of geometries.
            for (Int32 i = 0; i < count; i++)
            {
                // Call the main create function with the next geometry.
                geometries.Add(Parse(reader, factory));
            }

            // Create and return the next geometry.
            return geometries;
        }

        private static UInt32 readUInt32(BinaryReader reader, WkbByteOrder byteOrder)
        {
            UInt32 value = reader.ReadUInt32();

            return byteOrder == WkbByteOrder.Xdr
                       ? ByteEncoder.GetBigEndian(value)
                       : ByteEncoder.GetLittleEndian(value);
        }

        private static Double readDouble(BinaryReader reader, WkbByteOrder byteOrder)
        {
            Double value = reader.ReadDouble();

            return byteOrder == WkbByteOrder.Xdr
                       ? ByteEncoder.GetBigEndian(value)
                       : ByteEncoder.GetLittleEndian(value);
        }
    }
}