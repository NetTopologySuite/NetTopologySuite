using System;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Writes a Well-Known Binary byte data representation of a <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// There are a few cases which are not specified in the standard.
    /// The implementation uses a representation which is compatible with
    /// other common spatial systems (notably, PostGIS).
    /// <list type="bullet">
    /// <item><term><see cref="LinearRing"/>s</term><description>are written as <see cref="LineString"/>s.</description></item>
    /// <item><term>Empty geometries are output as follows</term><description>
    /// <list type="bullet">
    /// <item><term><c>Point</c></term><description>A <c>WKBPoint</c> with <c>double.NaN</c> ordinate values</description></item>
    /// <item><term><c>LineString</c></term><description>A <c>WKBLineString</c> with zero points</description></item>
    /// <item><term><c>Polygon</c></term><description>currently output as a <c>WKBPolygon</c> with one <c>LinearRing</c> with zero points.
    /// <i>Note:</i> This is different to other systems. It will change to a <c>WKBPolygon</c> with zero <c>LinearRing</c>s.</description></item>
    /// <item><term>Multi geometries</term><description>A <c>WKBMulti</c> with zero elements</description></item>
    /// <item><term><c>GeometryCollection</c></term><description>A <c>WKBGeometryCollection</c> with zero elements</description></item>
    /// </list>
    /// </description></item></list>
    /// <para/>
    /// This implementation supports the <b>Extended WKB</b> standard.
    /// Extended WKB allows writing 3-dimensional coordinates
    /// and the geometry SRID value.
    /// The presence of 3D coordinates is indicated
    /// by setting the high bit of the <tt>wkbType</tt> word.
    /// The presence of a SRID is indicated
    /// by setting the third bit of the <tt>wkbType</tt> word.
    /// EWKB format is upward-compatible with the original SFS WKB format.
    /// <para/>
    /// SRID output is optimized, if specified.
    /// The top-level geometry has the SRID included. Child geometries
    /// have it included if their value differs from its parent.
    /// <para/>
    /// This class supports reuse of a single instance to read multiple
    /// geometries. This class is not thread - safe; each thread should create its own
    /// instance.
    /// </remarks>
    public class WKBWriter
    {
        /// <summary>Converts a byte array to a hexadecimal string.</summary>
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
        /// Writes the WKB Header for the geometry
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="geom">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void WriteHeader(BinaryWriter writer, Geometry geom, bool includeSRID)
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

            // Check if includeSRID is valid
            includeSRID &= HandleSRID;

            // Flag for SRID if needed
            if (includeSRID)
                intGeometryType |= 0x20000000;

            //
            writer.Write(intGeometryType);

            //Write SRID if needed
            if (includeSRID)
                writer.Write(geom.SRID);
        }

        /// <summary>
        /// Gets or sets the binary encoding type
        /// </summary>
        public ByteOrder EncodingType { get; protected set; }

        /// <summary>
        /// Standard byte size for each complex point.
        /// Each complex point (LineString, Polygon, ...) contains:
        ///     1 byte for ByteOrder and
        ///     4 bytes for WKBType.
        ///     4 bytes for SRID value
        /// </summary>
        [Obsolete("Will be removed in a future version")]
        protected int InitCount => GetHeaderSize(true);

        /// <summary>
        /// Calculates the number of bytes required to store (E)WKB Header information.
        /// </summary>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The size of the </returns>
        private int GetHeaderSize(bool includeSRID)
        {
            return 5 + (HandleSRID && includeSRID ? 4 : 0);
        }
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
        public virtual byte[] Write(Geometry geometry)
        {
            byte[] bytes = GetBuffer(geometry, true);
            Write(geometry, new MemoryStream(bytes));
            return bytes;
        }

        /// <summary>
        /// Writes a WKB representation of a given point.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public virtual void Write(Geometry geometry, Stream stream)
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
        protected void Write(Geometry geometry, BinaryWriter writer)
            => Write(geometry, writer, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(Geometry geometry, BinaryWriter writer, bool includeSRID)
        {
            if (geometry is Point point)
                Write(point, writer, includeSRID);
            else if (geometry is LineString lineString)
                Write(lineString, writer, includeSRID);
            else if (geometry is Polygon polygon)
                Write(polygon, writer, includeSRID);
            else if (geometry is MultiPoint multiPoint)
                Write(multiPoint, writer, includeSRID);
            else if (geometry is MultiLineString multiLineString)
                Write(multiLineString, writer, includeSRID);
            else if (geometry is MultiPolygon multiPolygon)
                Write(multiPolygon, writer, includeSRID);
            else if (geometry is GeometryCollection geometryCollection)
                Write(geometryCollection, writer, includeSRID);
            else
                throw new ArgumentException("Geometry not recognized: " + geometry);
        }
        /// <summary>
        /// Writes the ByteOrder defined in <see cref="EncodingType"/>.
        /// </summary>
        /// <param name="writer">The writer to use</param>
        [Obsolete("Will be made private in a future version.")]
        protected void WriteByteOrder(BinaryWriter writer)
        {
            writer.Write((byte)EncodingType);
        }

        /// <summary>
        /// Write a <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate</param>
        /// <param name="writer">The writer.</param>
        [Obsolete("Will be removed in a future version")]
        protected void Write(Coordinate coordinate, BinaryWriter writer)
        {
            writer.Write(coordinate.X);
            writer.Write(coordinate.Y);
            if ((HandleOrdinates & Ordinates.Z) == Ordinates.Z)
                writer.Write(coordinate.Z);
            if ((HandleOrdinates & Ordinates.M) == Ordinates.M)
                writer.Write(coordinate.M);
        }

        /// <summary>
        /// Write a <see cref="CoordinateSequence"/>.
        /// </summary>
        /// <param name="sequence">The coordinate sequence to write</param>
        /// <param name="emitSize">A flag indicating if the size of <paramref name="sequence"/> should be written, too.</param>
        /// <param name="writer">The writer.</param>
        [Obsolete("Will be made private in a future version.")]
        protected void Write(CoordinateSequence sequence, bool emitSize, BinaryWriter writer)
        {
            if (emitSize)
                writer.Write(sequence.Count);

            // zm-values if not provided by sequence
            double ordinateZ = Coordinate.NullOrdinate;
            double ordinateM = Coordinate.NullOrdinate;

            // test if zm-values are provided by sequence
            bool getZ = sequence.HasZ;
            bool getM = sequence.HasM;

            // test if zm-values should be emitted
            bool writeZ = (HandleOrdinates & Ordinates.Z) == Ordinates.Z;
            bool writeM = (HandleOrdinates & Ordinates.M) == Ordinates.M;

            for (int index = 0; index < sequence.Count; index++)
            {
                writer.Write(sequence.GetOrdinate(index, 0));
                writer.Write(sequence.GetOrdinate(index, 1));
                if (writeZ)
                {
                    if (getZ) ordinateZ = sequence.GetZ(index);
                    writer.Write(ordinateZ);
                }
                if (writeM)
                {
                    if (getM) ordinateM = sequence.GetM(index);
                    writer.Write(ordinateM);
                }
            }
        }

        /// <summary>
        /// Write a point in its WKB format
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(Point point, BinaryWriter writer) =>
            Write(point, writer, true);

        /// <summary>
        /// Write a point in its WKB format
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(Point point, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, point, includeSRID);
            if (point.IsEmpty)
                WriteNaNs(_coordinateSize / 8, writer);
            else
#pragma warning disable 618
                Write(point.CoordinateSequence, false, writer);
#pragma warning restore 618
        }

        /// <summary>
        /// Write a LineString in its WKB format
        /// </summary>
        /// <param name="lineString">The LineString</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(LineString lineString, BinaryWriter writer) =>
            Write(lineString, writer, true);

        /// <summary>
        /// Write a LineString in its WKB format
        /// </summary>
        /// <param name="lineString">The LineString</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(LineString lineString, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, lineString, includeSRID);
#pragma warning disable 618
            Write(lineString.CoordinateSequence, true, writer);
#pragma warning restore 618
        }

        /// <summary>
        /// Write LinearRing information
        /// </summary>
        /// <param name="ring">The linear ring</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(LinearRing ring, BinaryWriter writer)
        {
            Write(ring.CoordinateSequence, true, writer);
        }

        /// <summary>
        /// Write a Polygon in its WKB format
        /// </summary>
        /// <param name="polygon">The Polygon</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(Polygon polygon, BinaryWriter writer)
            => Write(polygon, writer, true);

        /// <summary>
        /// Write a Polygon in its WKB format
        /// </summary>
        /// <param name="polygon">The Polygon</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(Polygon polygon, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, polygon, includeSRID);

            // For an empty polygon just write 0 to indicate no rings!
            if (polygon.IsEmpty)
            {
                writer.Write(0);
                return;
            }

            writer.Write(polygon.NumInteriorRings + 1);
#pragma warning disable 618
            Write(polygon.ExteriorRing.CoordinateSequence, true, writer);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
                Write(polygon.InteriorRings[i].CoordinateSequence, true, writer);
#pragma warning restore 618
        }

        /// <summary>
        /// Write a MultiPoint in its WKB format
        /// </summary>
        /// <param name="multiPoint">The MultiPoint</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(MultiPoint multiPoint, BinaryWriter writer)
            => Write(multiPoint, writer, true);

        /// <summary>
        /// Write a MultiPoint in its WKB format
        /// </summary>
        /// <param name="multiPoint">The MultiPoint</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(MultiPoint multiPoint, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, multiPoint, includeSRID);
            writer.Write(multiPoint.NumGeometries);
            for (int i = 0; i < multiPoint.NumGeometries; i++)
            {
                var point = (Point) multiPoint.Geometries[i];
                Write(point, writer, point.SRID != multiPoint.SRID);
            }
        }

        /// <summary>
        /// Write a MultiLineString in its WKB format
        /// </summary>
        /// <param name="multiLineString">The MultiLineString</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(MultiLineString multiLineString, BinaryWriter writer)
            => Write(multiLineString, writer, true);

        /// <summary>
        /// Write a MultiLineString in its WKB format
        /// </summary>
        /// <param name="multiLineString">The MultiLineString</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(MultiLineString multiLineString, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, multiLineString, includeSRID);
            writer.Write(multiLineString.NumGeometries);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                var lineString = (LineString) multiLineString.Geometries[i];
                Write(lineString, writer, lineString.SRID != multiLineString.SRID);
            }
        }

        /// <summary>
        /// Write a MultiPolygon in its WKB format
        /// </summary>
        /// <param name="multiPolygon">The MultiPolygon</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(MultiPolygon multiPolygon, BinaryWriter writer)
            => Write(multiPolygon, writer, true);

        /// <summary>
        /// Write a MultiPolygon in its WKB format
        /// </summary>
        /// <param name="multiPolygon">The MultiPolygon</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(MultiPolygon multiPolygon, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, multiPolygon, includeSRID);
            writer.Write(multiPolygon.NumGeometries);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                var polygon = (Polygon)multiPolygon.Geometries[i];
                Write(polygon, writer, polygon.SRID != multiPolygon.SRID);
            }
        }

        /// <summary>
        /// Write a GeometryCollection in its WKB format
        /// </summary>
        /// <param name="geomCollection">The GeometryCollection</param>
        /// <param name="writer">The writer</param>
        [Obsolete("Will be removed in a future version.")]
        protected void Write(GeometryCollection geomCollection, BinaryWriter writer)
            => Write(geomCollection, writer, true);

        /// <summary>
        /// Write a GeometryCollection in its WKB format
        /// </summary>
        /// <param name="geomCollection">The GeometryCollection</param>
        /// <param name="writer">The writer</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        private void Write(GeometryCollection geomCollection, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, geomCollection, includeSRID);
            writer.Write(geomCollection.NumGeometries);
            for (int i = 0; i < geomCollection.NumGeometries; i++)
            {
                var geom = geomCollection.GetGeometryN(i);
                Write(geom, writer, geom.SRID != geomCollection.SRID);
            }
        }

        /// <summary>
        /// Gets a buffer for the <see cref="MemoryStream"/> to write <paramref name="geometry"/> to.
        /// </summary>
        /// <param name="geometry">The geometry to write</param>
        /// <returns>A buffer</returns>
        [Obsolete("Will be removed in a future version.")]
        protected byte[] GetBytes(Geometry geometry)
            => GetBuffer(geometry, true);

        /// <summary>
        /// Gets a buffer for the <see cref="MemoryStream"/> to write <paramref name="geometry"/> to.
        /// </summary>
        /// <param name="geometry">The geometry to write</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>A buffer</returns>
        private byte[] GetBuffer(Geometry geometry, bool includeSRID)
        {
            if (geometry is Point point)
                return new byte[GetRequiredBufferSize(point, includeSRID)];
            if (geometry is LineString lineString)
                return new byte[GetRequiredBufferSize(lineString, includeSRID)];
            if (geometry is Polygon polygon)
                return new byte[GetRequiredBufferSize(polygon, includeSRID)];
            if (geometry is MultiPoint multiPoint)
                return new byte[GetRequiredBufferSize(multiPoint, includeSRID)];
            if (geometry is MultiLineString multiLineString)
                return new byte[GetRequiredBufferSize(multiLineString, includeSRID)];
            if (geometry is MultiPolygon multiPolygon)
                return new byte[GetRequiredBufferSize(multiPolygon, includeSRID)];
            if (geometry is GeometryCollection geometryCollection)
                return new byte[GetRequiredBufferSize(geometryCollection, includeSRID)];

            throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Computes the length of a buffer to write <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected virtual int SetByteStream(Geometry geometry)
        { 
            if (geometry is Point point)
                return GetRequiredBufferSize(point, true);
            if (geometry is LineString lineString)
                return GetRequiredBufferSize(lineString, true);
            if (geometry is Polygon polygon)
                return GetRequiredBufferSize(polygon, true);
            if (geometry is MultiPoint multiPoint)
                return GetRequiredBufferSize(multiPoint, true);
            if (geometry is MultiLineString multiLineString)
                return GetRequiredBufferSize(multiLineString, true);
            if (geometry is MultiPolygon multiPolygon)
                return GetRequiredBufferSize(multiPolygon, true);
            if (geometry is GeometryCollection geometryCollection)
                return GetRequiredBufferSize(geometryCollection, true);

            throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Computes the length of a buffer to write <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(Geometry geometry, bool includeSRID)
        {
            if (geometry is Point point)
                return GetRequiredBufferSize(point, includeSRID);
            if (geometry is LineString lineString)
                return GetRequiredBufferSize(lineString, includeSRID);
            if (geometry is Polygon polygon)
                return GetRequiredBufferSize(polygon, includeSRID);
            if (geometry is MultiPoint multiPoint)
                return GetRequiredBufferSize(multiPoint, includeSRID);
            if (geometry is MultiLineString multiLineString)
                return GetRequiredBufferSize(multiLineString, includeSRID);
            if (geometry is MultiPolygon multiPolygon)
                return GetRequiredBufferSize(multiPolygon, includeSRID);
            if (geometry is GeometryCollection geometryCollection)
                return GetRequiredBufferSize(geometryCollection, includeSRID);

            throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="GeometryCollection"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(GeometryCollection geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="GeometryCollection"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(GeometryCollection geometry, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4;
            foreach (var geom in geometry.Geometries)
                count += GetRequiredBufferSize(geom, geom.SRID != geometry.SRID);
            return count;
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="MultiPolygon"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(MultiPolygon geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="MultiPolygon"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(MultiPolygon geometry, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4;
            foreach (Polygon geom in geometry.Geometries)
                count += GetRequiredBufferSize(geom, geom.SRID != geometry.SRID );
            return count;
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="MultiLineString"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(MultiLineString geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="MultiLineString"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(MultiLineString geometry, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4;
            foreach (LineString geom in geometry.Geometries)
                count += GetRequiredBufferSize(geom, geom.SRID != geometry.SRID);
            return count;
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="MultiPoint"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(MultiPoint geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="MultiPoint"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(MultiPoint geometry, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4;     // NumPoints
            foreach (Point geom in geometry.Geometries)
                count += GetRequiredBufferSize(geom, geom.SRID != geometry.SRID);
            return count;
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="Polygon"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(Polygon geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="Polygon"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(Polygon geometry, bool includeSRID)
        {
            int pointSize = _coordinateSize; //Double.IsNaN(geometry.Coordinate.Z) ? 16 : 24;
            int count = GetHeaderSize(includeSRID);
            count += 4 /*+ 4*/;                                 // NumRings /*+ NumPoints */
            if (!geometry.IsEmpty)
            {
                count += 4 * (geometry.NumInteriorRings + 1); // Index parts
                count += geometry.NumPoints * pointSize; // Points in exterior and interior rings
            }

            return count;
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="LineString"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(LineString geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="LineString"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(LineString geometry, bool includeSRID)
        {
            int pointSize = _coordinateSize; //Double.IsNaN(geometry.Coordinate.Z) ? 16 : 24;
            int numPoints = geometry.NumPoints;
            int count = GetHeaderSize(includeSRID);
            count += 4;                             // NumPoints
            count += pointSize * numPoints;
            return count;
        }

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="Point"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        [Obsolete("Will be removed in a future version.")]
        protected int SetByteStream(Point geometry)
            => GetRequiredBufferSize(geometry, true);

        /// <summary>
        /// Computes the length of a buffer to write the <see cref="Point"/> <paramref name="geometry"/> in its WKB format.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="includeSRID">
        /// A flag indicting if SRID value is of possible interest.
        /// The value is <c>&amp;&amp;</c>-combineed with <c>HandleSRID</c>.
        /// </param>
        /// <returns>The number of bytes required to store <paramref name="geometry"/> in its WKB format.</returns>
        private int GetRequiredBufferSize(Point geometry, bool includeSRID)
        {
            return GetHeaderSize(includeSRID) + _coordinateSize;
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
        /// <list type="bullet">
        /// <item><description><c>0x80000000</c> flag if geometry's z-ordinate values are written</description></item>
        /// <item><description><c>0x40000000</c> flag if geometry's m-ordinate values are written</description></item>
        /// <item><description><c>0x20000000</c> flag if geometry's SRID value is written</description></item></list>
        /// </summary>
        private bool _strict = true;

        /// <summary>
        /// Gets a value indicating if only original WKT elements should be handled
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

        private bool _handleSRID;

        /// <summary>
        /// Gets or sets a value indicating if an encoded SRID value should be handled or ignored.
        /// </summary>
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

        /// <summary>
        /// Gets the <see cref="Ordinates"/> that this class can write.
        /// </summary>
        public static readonly Ordinates AllowedOrdinates = Ordinates.XYZM;

        private Ordinates _handleOrdinates;

        /// <summary>
        /// Gets or sets the maximum <see cref="Ordinates"/> to write out.
        /// The default is equivalent to <see cref="AllowedOrdinates"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The purpose of this property is to <b>restrict</b> what gets written out to ensure that,
        /// e.g., Z values are never written out even if present on a geometry instance.  Ordinates
        /// that are not present on a geometry instance will be omitted regardless of this value.
        /// </para>
        /// <para>
        /// Flags not present in <see cref="AllowedOrdinates"/> are silently ignored.
        /// </para>
        /// <para>
        /// <see cref="Ordinates.X"/> and <see cref="Ordinates.Y"/> are always present.
        /// </para>
        /// </remarks>
        public Ordinates HandleOrdinates
        {
            get => _handleOrdinates;
            set
            {
                value = Ordinates.XY | AllowedOrdinates & value;
                if (value == _handleOrdinates)
                    return;

                _handleOrdinates = value;
                CalcCoordinateSize();
            }
        }

        private static void WriteNaNs(int numNaNs, BinaryWriter writer)
        {
            for (int i = 0; i<numNaNs; i++)
                writer.Write(double.NaN);
        }
    }
}
