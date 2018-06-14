using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Writes a Well-Known Binary byte data representation of a <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// WKBWriter stores <see cref="Coordinate" /> X,Y,Z values if <see cref="Coordinate.Z" /> is not <see cref="double.NaN"/>,
    /// otherwise <see cref="Coordinate.Z" /> value is discarded and only X,Y are stored.
    /// </remarks>
    // Thanks to Roberto Acioli for Coordinate.Z patch
    public class WKBWriter : IBinaryGeometryWriter
    {
        ///<summary>Converts a byte array to a hexadecimal string.</summary>
        /// <param name="bytes">A byte array</param>
        [Obsolete("Use ToHex(byte[])")]
        public static string BytesToHex(byte[] bytes)
        {
            return ToHex(bytes);
        }

        ///<summary>Converts a byte array to a hexadecimal string.</summary>
        /// <param name="bytes">A byte array</param>
        public static string ToHex(byte[] bytes)
        {
            var buf = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                buf.Append(ToHexDigit((b >> 4) & 0x0F));
                buf.Append(ToHexDigit(b & 0x0F));
            }
            return buf.ToString();
        }

        private static char ToHexDigit(int n)
        {
            if (n < 0 || n > 15)
                throw new ArgumentException("Nibble value out of range: " + n);
            if (n <= 9)
                return (char)('0' + n);
            return (char)('A' + (n - 10));
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IGeometry.SRID"/> value should be emitted
        /// </summary>
        [Obsolete("Use HandleSRID instead")]
        public bool EmitSRID
        {
            get => HandleSRID;
            set => HandleSRID = value;
        }

        private bool _emitZ;

        /// <summary>
        /// Gets or sets whether the <see cref="Coordinate.Z"/> values should be emitted
        /// </summary>
        [Obsolete("Use HandleOrdinates instead")]
        public bool EmitZ
        {
            get => _emitZ;
            set
            {
                if (value == _emitZ)
                    return;
                _emitZ = value;

                if (value)
                    HandleOrdinates |= Ordinates.Z;
                else
                    HandleOrdinates &= ~Ordinates.Z;

                CalcCoordinateSize();
            }
        }

        private bool _emitM;

        /// <summary>
        /// Gets or sets whether the <see cref="ICoordinate.M"/> values should be emitted
        /// </summary>
        [Obsolete("Use HandleOrdintes instead.")]
        public bool EmitM
        {
            get => _emitM;
            set
            {
                if (value == _emitM)
                    return;

                _emitM = value;

                if (value)
                    HandleOrdinates |= Ordinates.M;
                else
                    HandleOrdinates &= ~Ordinates.M;

                CalcCoordinateSize();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="geom"></param>
        private void WriteHeader(BinaryWriter writer, IGeometry geom)
        {
            //Byte Order
            WriteByteOrder(writer);

            //TODO: use "is" check, like in "WKTWriter.AppendGeometryTaggedText"?
            WKBGeometryTypes geometryType;
            switch (geom.GeometryType)
            {
                case "Point":
                    geometryType = WKBGeometryTypes.WKBPoint;
                    break;
                case "LineString":
                case "LinearRing":
                    geometryType = WKBGeometryTypes.WKBLineString;
                    break;
                case "Polygon":
                    geometryType = WKBGeometryTypes.WKBPolygon;
                    break;
                case "MultiPoint":
                    geometryType = WKBGeometryTypes.WKBMultiPoint;
                    break;
                case "MultiPolygon":
                    geometryType = WKBGeometryTypes.WKBMultiPolygon;
                    break;
                case "MultiLineString":
                    geometryType = WKBGeometryTypes.WKBMultiLineString;
                    break;
                case "GeometryCollection":
                    geometryType = WKBGeometryTypes.WKBGeometryCollection;
                    break;
                default:
                    Assert.ShouldNeverReachHere("Unknown geometry type:" + geom.GeometryType);
                    throw new ArgumentException("geom");
            }

            //Modify WKB Geometry type
            uint intGeometryType = (uint)geometryType & 0xff;
            if ((HandleOrdinates & Ordinates.Z) == Ordinates.Z)
            {
                intGeometryType += 1000;
                if (!Strict) intGeometryType |= 0x80000000;
            }

            if ((HandleOrdinates & Ordinates.M) == Ordinates.M)
            {
                intGeometryType += 2000;
                if (!Strict) intGeometryType |= 0x40000000;
            }

            //Flag for SRID if needed
            if (HandleSRID)
                intGeometryType |= 0x20000000;

            //
            writer.Write(intGeometryType);

            //Write SRID if needed
            if (HandleSRID)
                writer.Write(geom.SRID);
        }

        protected ByteOrder EncodingType;

        /// <summary>
        /// Standard byte size for each complex point.
        /// Each complex point (LineString, Polygon, ...) contains:
        ///     1 byte for ByteOrder and
        ///     4 bytes for WKBType.
        ///     4 bytes for SRID value
        /// </summary>
        protected int InitCount => 5 + (HandleSRID ? 4 : 0);

        /// <summary>
        /// Initializes writer with LittleIndian byte order.
        /// </summary>
        public WKBWriter() :
            this(ByteOrder.LittleEndian, false)
        { }

        /// <summary>
        /// Initializes writer with the specified byte order.
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
        public WKBWriter(ByteOrder encodingType) :
            this(encodingType, false)
        {
        }

        /// <summary>
        /// Initializes writer with the specified byte order.
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
        /// <param name="handleSRID">SRID values, present or not, should be emitted.</param>
        public WKBWriter(ByteOrder encodingType, bool handleSRID) :
            this(encodingType, handleSRID, false)
        {
        }

        /// <summary>
        /// Initializes writer with the specified byte order.
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
        /// <param name="handleSRID">SRID values, present or not, should be emitted.</param>
        /// <param name="emitZ">Z values, present or not, should be emitted</param>
        public WKBWriter(ByteOrder encodingType, bool handleSRID, bool emitZ) :
            this(encodingType, handleSRID, emitZ, false)
        {
        }

        /// <summary>
        /// Initializes writer with the specified byte order.
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
        /// <param name="handleSRID">SRID values, present or not, should be emitted.</param>
        /// <param name="emitZ">Z values, present or not, should be emitted</param>
        /// <param name="emitM">M values, present or not, should be emitted</param>
        public WKBWriter(ByteOrder encodingType, bool handleSRID, bool emitZ, bool emitM)
        {
            EncodingType = encodingType;

            //Allow setting of HandleSRID
            if (handleSRID) _strict = false;
            HandleSRID = handleSRID;

            var handleOrdinates = Ordinates.XY;
            if (emitZ)
                handleOrdinates |= Ordinates.Z;
            if (emitM)
                handleOrdinates |= Ordinates.M;

            _handleOrdinates = handleOrdinates;
            CalcCoordinateSize();
        }

        /// <summary>
        /// Writes a WKB representation of a given point.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public virtual byte[] Write(IGeometry geometry)
        {
            byte[] bytes = GetBytes(geometry);
            Write(geometry, new MemoryStream(bytes));
            return bytes;
        }

        /// <summary>
        /// Writes a WKB representation of a given point.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public virtual void Write(IGeometry geometry, Stream stream)
        {
            BinaryWriter writer = null;
            try
            {
                writer = EncodingType == ByteOrder.LittleEndian
                    ? new BinaryWriter(stream)
                    : new BEBinaryWriter(stream);
                Write(geometry, writer);
            }
            finally
            {
                if (writer != null)
                    ((IDisposable)writer).Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometry geometry, BinaryWriter writer)
        {
            if (geometry is IPoint)
                Write(geometry as IPoint, writer);
            else if (geometry is ILineString)
                Write(geometry as ILineString, writer);
            else if (geometry is IPolygon)
                Write(geometry as IPolygon, writer);
            else if (geometry is IMultiPoint)
                Write(geometry as IMultiPoint, writer);
            else if (geometry is IMultiLineString)
                Write(geometry as IMultiLineString, writer);
            else if (geometry is IMultiPolygon)
                Write(geometry as IMultiPolygon, writer);
            else if (geometry is IGeometryCollection)
                Write(geometry as IGeometryCollection, writer);
            else throw new ArgumentException("Geometry not recognized: " + geometry);
        }

        /// <summary>
        /// Writes LittleIndian ByteOrder.
        /// </summary>
        /// <param name="writer"></param>
        protected void WriteByteOrder(BinaryWriter writer)
        {
            writer.Write((byte)EncodingType);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>
        protected void Write(Coordinate coordinate, BinaryWriter writer)
        {
            writer.Write(coordinate.X);
            writer.Write(coordinate.Y);
            if ((HandleOrdinates & Ordinates.Z) == Ordinates.Z)
                writer.Write(coordinate.Z);
            if ((HandleOrdinates & Ordinates.M) == Ordinates.M)
                //NOTE: Implement
                writer.Write(double.NaN);
        }

        protected void Write(ICoordinateSequence sequence, bool emitSize, BinaryWriter writer)
        {
            if (emitSize)
                writer.Write(sequence.Count);

            // zm-values if not provided by sequence
            double ordinateZ = Coordinate.NullOrdinate;
            double ordinateM = Coordinate.NullOrdinate;

            // test if zm-values are provided by sequence
            bool getZ = (sequence.Ordinates & Ordinates.Z) == Ordinates.Z;
            bool getM = (sequence.Ordinates & Ordinates.M) == Ordinates.M;

            // test if zm-values should be emitted
            bool writeZ = (HandleOrdinates & Ordinates.Z) == Ordinates.Z;
            bool writeM = (HandleOrdinates & Ordinates.M) == Ordinates.M;

            for (int index = 0; index < sequence.Count; index++)
            {
                writer.Write(sequence.GetOrdinate(index, Ordinate.X));
                writer.Write(sequence.GetOrdinate(index, Ordinate.Y));
                if (writeZ)
                {
                    if (getZ) ordinateZ = sequence.GetOrdinate(index, Ordinate.Z);
                    writer.Write(ordinateZ);
                }
                if (writeM)
                {
                    if (getM) ordinateM = sequence.GetOrdinate(index, Ordinate.M);
                    writer.Write(ordinateM);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(IPoint point, BinaryWriter writer)
        {
            ////WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, point);
            ////if (Double.IsNaN(point.Coordinate.Z))
            ////     writer.Write((int)WKBGeometryTypes.WKBPoint);
            ////else writer.Write((int)WKBGeometryTypes.WKBPointZ);
            //Write(point.Coordinate, writer);
            Write(point.CoordinateSequence, false, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(ILineString lineString, BinaryWriter writer)
        {
            ////WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, lineString);
            ////if (Double.IsNaN(lineString.Coordinate.Z))
            ////     writer.Write((int)WKBGeometryTypes.WKBLineString);
            ////else writer.Write((int)WKBGeometryTypes.WKBLineStringZ);
            //writer.Write(lineString.NumPoints);
            //for (int i = 0; i < lineString.Coordinates.Length; i++)
            //    Write(lineString.Coordinates[i], writer);
            Write(lineString.CoordinateSequence, true, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="writer"></param>
        protected void Write(ILinearRing ring, BinaryWriter writer)
        {
            //writer.Write(ring.NumPoints);
            //for (int i = 0; i < ring.Coordinates.Length; i++)
            //    Write(ring.Coordinates[i], writer);
            Write(ring.CoordinateSequence, true, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(IPolygon polygon, BinaryWriter writer)
        {
            //WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, polygon);
            //if (Double.IsNaN(polygon.Coordinate.Z))
            //     writer.Write((int)WKBGeometryTypes.WKBPolygon);
            //else writer.Write((int)WKBGeometryTypes.WKBPolygonZ);
            writer.Write(polygon.NumInteriorRings + 1);
            Write(polygon.ExteriorRing as ILinearRing, writer);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
                Write(polygon.InteriorRings[i] as ILinearRing, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPoint multiPoint, BinaryWriter writer)
        {
            //WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, multiPoint);
            //if (Double.IsNaN(multiPoint.Coordinate.Z))
            //     writer.Write((int)WKBGeometryTypes.WKBMultiPoint);
            //else writer.Write((int)WKBGeometryTypes.WKBMultiPointZ);
            writer.Write(multiPoint.NumGeometries);
            for (int i = 0; i < multiPoint.NumGeometries; i++)
                Write(multiPoint.Geometries[i] as IPoint, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiLineString multiLineString, BinaryWriter writer)
        {
            //WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, multiLineString);
            //if (Double.IsNaN(multiLineString.Coordinate.Z))
            //     writer.Write((int)WKBGeometryTypes.WKBMultiLineString);
            //else writer.Write((int)WKBGeometryTypes.WKBMultiLineStringZ);
            writer.Write(multiLineString.NumGeometries);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
                Write(multiLineString.Geometries[i] as ILineString, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPolygon multiPolygon, BinaryWriter writer)
        {
            //WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, multiPolygon);
            //if (Double.IsNaN(multiPolygon.Coordinate.Z))
            //     writer.Write((int)WKBGeometryTypes.WKBMultiPolygon);
            //else writer.Write((int)WKBGeometryTypes.WKBMultiPolygonZ);
            writer.Write(multiPolygon.NumGeometries);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
                Write(multiPolygon.Geometries[i] as IPolygon, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomCollection"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometryCollection geomCollection, BinaryWriter writer)
        {
            //WriteByteOrder(writer);     // LittleIndian
            WriteHeader(writer, geomCollection);
            //if (Double.IsNaN(geomCollection.Coordinate.Z))
            //     writer.Write((int)WKBGeometryTypes.WKBGeometryCollection);
            //else writer.Write((int)WKBGeometryTypes.WKBGeometryCollectionZ);
            writer.Write(geomCollection.NumGeometries);
            for (int i = 0; i < geomCollection.NumGeometries; i++)
                Write(geomCollection.Geometries[i], writer);
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected byte[] GetBytes(IGeometry geometry)
        {
            if (geometry is IPoint)
                return new byte[SetByteStream(geometry as IPoint)];
            if (geometry is ILineString)
                return new byte[SetByteStream(geometry as ILineString)];
            if (geometry is IPolygon)
                return new byte[SetByteStream(geometry as IPolygon)];
            if (geometry is IMultiPoint)
                return new byte[SetByteStream(geometry as IMultiPoint)];
            if (geometry is IMultiLineString)
                return new byte[SetByteStream(geometry as IMultiLineString)];
            if (geometry is IMultiPolygon)
                return new byte[SetByteStream(geometry as IMultiPolygon)];
            if (geometry is IGeometryCollection)
                return new byte[SetByteStream(geometry as IGeometryCollection)];
            throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected virtual int SetByteStream(IGeometry geometry)
        {
            if (geometry is IPoint)
                return SetByteStream(geometry as IPoint);
            if (geometry is ILineString)
                return SetByteStream(geometry as ILineString);
            if (geometry is IPolygon)
                return SetByteStream(geometry as IPolygon);
            if (geometry is IMultiPoint)
                return SetByteStream(geometry as IMultiPoint);
            if (geometry is IMultiLineString)
                return SetByteStream(geometry as IMultiLineString);
            if (geometry is IMultiPolygon)
                return SetByteStream(geometry as IMultiPolygon);
            if (geometry is IGeometryCollection)
                return SetByteStream(geometry as IGeometryCollection);
            throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IGeometryCollection geometry)
        {
            int count = InitCount;
            count += 4;
            foreach (var geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiPolygon geometry)
        {
            int count = InitCount;
            count += 4;
            foreach (IPolygon geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiLineString geometry)
        {
            int count = InitCount;
            count += 4;
            foreach (ILineString geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiPoint geometry)
        {
            int count = InitCount;
            count += 4;     // NumPoints
            foreach (IPoint geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IPolygon geometry)
        {
            int pointSize = _coordinateSize; //Double.IsNaN(geometry.Coordinate.Z) ? 16 : 24;
            int count = InitCount;
            count += 4 /*+ 4*/;                                 // NumRings /*+ NumPoints */
            count += 4 * (geometry.NumInteriorRings + 1);   // Index parts
            count += geometry.NumPoints * pointSize;        // Points in exterior and interior rings
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(ILineString geometry)
        {
            int pointSize = _coordinateSize; //Double.IsNaN(geometry.Coordinate.Z) ? 16 : 24;
            int numPoints = geometry.NumPoints;
            int count = InitCount;
            count += 4;                             // NumPoints
            count += pointSize * numPoints;
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IPoint geometry)
        {
            return InitCount + _coordinateSize;
            //return Double.IsNaN(geometry.Coordinate.Z) ? 21 : 29;
        }

        private int _coordinateSize = 16;

        private void CalcCoordinateSize()
        {
            _coordinateSize = 16;
            if ((HandleOrdinates & Ordinates.Z) == Ordinates.Z) _coordinateSize += 8;
            if ((HandleOrdinates & Ordinates.M) == Ordinates.M) _coordinateSize += 8;
        }

        /// <summary>
        /// Gets a value whether or not EWKB featues may be used.
        /// <para/>EWKB features are
        /// <list type="Bullet"><item>0x80000000 flag if geometry's z-ordinate values are written</item>
        /// <item>0x40000000 flag if geometry's m-ordinate values are written</item>
        /// <item>0x20000000 flag if geometry's SRID value is written</item></list>
        /// </summary>
        public bool Strict
        {
            get => _strict;
            set
            {
                _strict = value;
                if (_strict)
                    HandleSRID = false;
            }
        }

        #region Implementation of IGeometryIOBase

        public bool HandleSRID
        {
            get => _handleSRID;
            set
            {
                if (_strict && value)
                    throw new ArgumentException("Cannot set HandleSRID to true if Strict is set", "value");
                _handleSRID = value;
            }
        }

        public Ordinates AllowedOrdinates => Ordinates.XYZM;

        private Ordinates _handleOrdinates;
        private bool _handleSRID;
        private bool _strict = true;

        public Ordinates HandleOrdinates
        {
            get => _handleOrdinates;
            set
            {
                value = Ordinates.XY | AllowedOrdinates & value;
                if (value == _handleOrdinates)
                    return;

                _handleOrdinates = value;
                _emitZ = (value & Ordinates.Z) != 0;
                _emitM = (value & Ordinates.M) != 0;
                CalcCoordinateSize();
            }
        }

        #endregion Implementation of IGeometryIOBase

        #region Implementation of IBinaryGeometryWriter

        public ByteOrder ByteOrder
        {
            get => EncodingType;
            set { }
        }

        #endregion Implementation of IBinaryGeometryWriter
    }
}