using System;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Converts a Well-Known Binary byte data to a <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// This class reads the format described in <see cref="WKBWriter"/>.
    /// It partially handles the <b>Extended WKB</b> format used by PostGIS,
    /// by parsing and storing optional SRID values.
    /// If a SRID is not specified in an element geometry, it is inherited
    /// from the parent's SRID.
    /// The default SRID value depends on <see cref="NtsGeometryServices.DefaultSRID"/>.
    /// <para/>
    /// Although not defined in the WKB spec, empty points
    /// are handled if they are represented as a Point with <c>NaN</c> X and Y ordinates.
    /// <para/>
    /// The reader repairs structurally-invalid input
    /// (specifically, LineStrings and LinearRings which contain
    /// too few points have vertices added,
    /// and non-closed rings are closed).
    /// <para/>
    /// The reader handles most errors caused by malformed or malicious WKB data.
    /// It checks for obviously excessive values of the fields
    /// <c>numElems</c>, <c>numRings</c>, and <c>numCoords</c>.
    /// It also checks that the reader does not read beyond the end of the data supplied.
    /// A <see cref="ParseException"/> is thrown if this situation is detected.
    /// </remarks>
    public class WKBReader
    {
        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// The hexadecimal digit symbols are case-insensitive.
        /// </summary>
        /// <param name="hex">A string containing hex digits</param>
        /// <returns>An array of bytes with the value of the hex string</returns>
        public static byte[] HexToBytes(string hex)
        {
            int byteLen = hex.Length / 2;
            byte[] bytes = new byte[byteLen];

            for (int i = 0; i < hex.Length / 2; i++)
            {
                int i2 = 2 * i;
                if (i2 + 1 > hex.Length)
                    throw new ArgumentException("Hex string has odd length");

                int nib1 = HexToInt(hex[i2]);
                int nib0 = HexToInt(hex[i2 + 1]);
                bytes[i] = (byte)((nib1 << 4) + (byte)nib0);
            }
            return bytes;
        }

        private static int HexToInt(char hex)
        {
            switch (hex)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return hex - '0';
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    return hex - 'A' + 10;
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                    return hex - 'a' + 10;
            }
            throw new ArgumentException("Invalid hex digit: " + hex);
        }


        /// <summary>
        /// A coordinate sequence factory
        /// </summary>
        protected readonly CoordinateSequenceFactory SequenceFactory;

        /// <summary>
        /// A precision model
        /// </summary>
        protected readonly PrecisionModel PrecisionModel;

        /// <summary>
        /// A GeometryServices object
        /// </summary>
        protected readonly NtsGeometryServices GeometryServices;

        /*
         * true if structurally invalid input should be reported rather than repaired.
         */
        private bool _isStrict;

        /// <summary>
        /// Initialize reader with a standard <see cref="NtsGeometryServices"/>.
        /// </summary>
        public WKBReader() : this(NtsGeometryServices.Instance) { }

        /// <summary>
        /// Creates an instance of this class using the provided <c>NtsGeometryServices</c>
        /// </summary>
        /// <param name="services"></param>
        public WKBReader(NtsGeometryServices services)
        {
            services = services ?? NtsGeometryServices.Instance;
            GeometryServices = services;
            PrecisionModel = services.DefaultPrecisionModel;
            SequenceFactory = services.DefaultCoordinateSequenceFactory;

            HandleSRID = true;
            HandleOrdinates = AllowedOrdinates;
        }

        /// <summary>
        /// Reads a <see cref="Geometry"/> in binary WKB format from an array of <see cref="byte"/>s.
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <returns>The geometry read</returns>
        /// <exception cref="ParseException"> if the WKB data is ill-formed.</exception>
        public Geometry Read(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
                return Read(stream);
        }

        /// <summary>
        /// Reads a <see cref="Geometry"/> in binary WKB format from an <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The geometry read</returns>
        /// <exception cref="ParseException"> if the WKB data is ill-formed.</exception>
        public virtual Geometry Read(Stream stream)
        {
            using (var reader = new BiEndianBinaryReader(stream))
                return Read(reader);
        }

        /// <summary>
        /// WKB Coordinate Systems
        /// </summary>
        protected enum CoordinateSystem
        {
            /// <summary>
            /// 2D coordinate system
            /// </summary>
            XY = 1,
            /// <summary>
            /// 3D coordinate system
            /// </summary>
            XYZ = 2,
            /// <summary>
            /// 2D coordinate system with additional measure value
            /// </summary>
            XYM = 3,
            /// <summary>
            /// 3D coordinate system with additional measure value
            /// </summary>
            XYZM = 4
        };

        /// <summary>
        /// Reads a <see cref="Geometry"/> in binary WKB format using the provided <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <returns>The geometry read</returns>
        /// <exception cref="ParseException"> if the WKB data is ill-formed.</exception>
        protected Geometry Read(BinaryReader reader)
        {
            return ReadGeometry(reader/*, CoordinateSystem.XY*/, GeometryServices.DefaultSRID);
        }


        protected Geometry ReadGeometry(BinaryReader reader/*, CoordinateSystem csIn*/, int sridIn)
        {
            try
            {
                ReadByteOrder(reader);
                int srid = sridIn;
                var geometryType = ReadGeometryType(reader, out var cs, ref srid);
                if (srid < 0) srid = sridIn;

                return ReadGeometry(geometryType, reader, cs, srid);
            }
            catch (Exception ex)
            {
                throw new ParseException(ex);
            }
        }

        private Geometry ReadGeometry(WKBGeometryTypes geometryType, BinaryReader reader, CoordinateSystem cs, int srid)
        {
            // Probe for other geometry
            var otherGeometry = ReadOtherGeometry((uint) geometryType, reader, cs, srid);
            if (otherGeometry != null) return otherGeometry;

            switch (geometryType)
            {
                //Point
                case WKBGeometryTypes.WKBPoint:
                case WKBGeometryTypes.WKBPointZ:
                case WKBGeometryTypes.WKBPointM:
                case WKBGeometryTypes.WKBPointZM:
                    return ReadPoint(reader, cs, srid);
                //Line String
                case WKBGeometryTypes.WKBLineString:
                case WKBGeometryTypes.WKBLineStringZ:
                case WKBGeometryTypes.WKBLineStringM:
                case WKBGeometryTypes.WKBLineStringZM:
                    return ReadLineString(reader, cs, srid);
                //Polygon
                case WKBGeometryTypes.WKBPolygon:
                case WKBGeometryTypes.WKBPolygonZ:
                case WKBGeometryTypes.WKBPolygonM:
                case WKBGeometryTypes.WKBPolygonZM:
                    return ReadPolygon(reader, cs, srid);
                //Multi Point
                case WKBGeometryTypes.WKBMultiPoint:
                case WKBGeometryTypes.WKBMultiPointZ:
                case WKBGeometryTypes.WKBMultiPointM:
                case WKBGeometryTypes.WKBMultiPointZM:
                    return ReadMultiPoint(reader, cs, srid);
                //Multi Line String
                case WKBGeometryTypes.WKBMultiLineString:
                case WKBGeometryTypes.WKBMultiLineStringZ:
                case WKBGeometryTypes.WKBMultiLineStringM:
                case WKBGeometryTypes.WKBMultiLineStringZM:
                    return ReadMultiLineString(reader, cs, srid);
                //Multi Polygon
                case WKBGeometryTypes.WKBMultiPolygon:
                case WKBGeometryTypes.WKBMultiPolygonZ:
                case WKBGeometryTypes.WKBMultiPolygonM:
                case WKBGeometryTypes.WKBMultiPolygonZM:
                    return ReadMultiPolygon(reader, cs, srid);
                //Geometry Collection
                case WKBGeometryTypes.WKBGeometryCollection:
                case WKBGeometryTypes.WKBGeometryCollectionZ:
                case WKBGeometryTypes.WKBGeometryCollectionM:
                case WKBGeometryTypes.WKBGeometryCollectionZM:
                    return ReadGeometryCollection(reader, cs, srid);
                default:
                    throw new ArgumentException("Geometry type not recognized. GeometryCode: " + geometryType);
            }
        }

        /// <summary>
        /// Extension point for reading other geometry types
        /// </summary>
        /// <param name="geometryType">The geometry type</param>
        /// <param name="reader">The binary reader</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="srid">The spatial reference id</param>
        /// <returns>The geometry read, or <c>null</c>.</returns>
        protected virtual Geometry ReadOtherGeometry(uint geometryType,
            BinaryReader reader, CoordinateSystem coordinateSystem, int srid)
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        private void ReadByteOrder(BinaryReader reader)
        {
            var byteOrder = (ByteOrder)reader.ReadByte();
            if (_isStrict && byteOrder != ByteOrder.BigEndian && byteOrder != ByteOrder.LittleEndian)
                throw new ParseException($"Unknown geometry byte order (not LittleEndian or BigEndian): {byteOrder}");

            ((BiEndianBinaryReader)reader).Endianess = byteOrder;
        }

        private WKBGeometryTypes ReadGeometryType(BinaryReader reader, out CoordinateSystem coordinateSystem, ref int srid)
        {
            uint type = reader.ReadUInt32();
            //Determine coordinate system
            if ((type & (0x80000000 | 0x40000000)) == (0x80000000 | 0x40000000))
                coordinateSystem = CoordinateSystem.XYZM;
            else if ((type & 0x80000000) == 0x80000000)
                coordinateSystem = CoordinateSystem.XYZ;
            else if ((type & 0x40000000) == 0x40000000)
                coordinateSystem = CoordinateSystem.XYM;
            else
                coordinateSystem = CoordinateSystem.XY;

            //Has SRID
            int newSrid = (type & 0x20000000) != 0 ? reader.ReadInt32() : -1;
            if (HandleSRID && newSrid >= 0) srid = newSrid;

            //Get cs from prefix
            uint ordinate = (type & 0xffff) / 1000;
            switch (ordinate)
            {
                case 1:
                    coordinateSystem = CoordinateSystem.XYZ;
                    break;
                case 2:
                    coordinateSystem = CoordinateSystem.XYM;
                    break;
                case 3:
                    coordinateSystem = CoordinateSystem.XYZM;
                    break;
            }

            return (WKBGeometryTypes)((type & 0xffff) % 1000);
        }

        /// <summary>
        /// Computes a reasonable value for the number of coordinates to read.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="cs">The coordinate system.</param>
        /// <returns>A number</returns>
        protected int ReasonableNumPoints(Stream stream, CoordinateSystem cs)
        {
            int remainingBytes = (int)(stream.Length - stream.Position) - 4;
            if (remainingBytes < 0) return int.MaxValue;

            int size = 16;

            switch (cs)
            {
                case CoordinateSystem.XYM:
                case CoordinateSystem.XYZ:
                    size = 24;
                    break;
                case CoordinateSystem.XYZM:
                    size = 32;
                    break;
            }

            return remainingBytes / size;
        }

        /// <summary>
        /// Reads an <see cref="int"/> field and checks if the value is reasonable.
        /// </summary>
        /// <remarks>A value is reasonable if it is not negative and less than or equal to <paramref name="reasonableNumField"/>.</remarks>
        /// <param name="reader">The reader</param>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="reasonableNumField">A reasonable value</param>
        /// <returns>The value read if it is reasonable, otherwise <see cref="ParseException"/> is thrown.</returns>
        protected int ReadNumField(BinaryReader reader, string fieldName, int reasonableNumField = int.MaxValue)
        {
            // num field is unsigned int, but int should do
            int num = reader.ReadInt32();
            if (num < 0 || num > reasonableNumField)
            {
                throw new ParseException(fieldName + " value is too large");
            }
            return (int)num;
        }

        /// <summary>
        /// Function to read a coordinate sequence.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="size">The number of ordinates</param>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The read coordinate sequence.</returns>
        protected CoordinateSequence ReadCoordinateSequence(BinaryReader reader, int size, CoordinateSystem cs)
        {
            var sequence = SequenceFactory.Create(size, ToOrdinates(cs));
            for (int i = 0; i < size; i++)
            {
                double x = reader.ReadDouble();
                double y = reader.ReadDouble();

                if (PrecisionModel != null) x = PrecisionModel.MakePrecise(x);
                if (PrecisionModel != null) y = PrecisionModel.MakePrecise(y);

                sequence.SetOrdinate(i, 0, x);
                sequence.SetOrdinate(i, 1, y);

                switch (cs)
                {
                    case CoordinateSystem.XY:
                        continue;
                    case CoordinateSystem.XYZ:
                        double z = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.Z))
                            sequence.SetOrdinate(i, 2, z);
                        break;
                    case CoordinateSystem.XYM:
                        double m = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.M))
                            sequence.SetOrdinate(i, 2, m);
                        break;
                    case CoordinateSystem.XYZM:
                        z = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.Z))
                            sequence.SetOrdinate(i, 2, z);
                        m = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.M))
                            sequence.SetOrdinate(i, 3, m);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Coordinate system not supported: {0}", cs));
                }
            }
            return sequence;
        }

        /// <summary>
        /// Function to read a coordinate sequence that is supposed to form a ring.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="size">The number of ordinates</param>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The read coordinate sequence.</returns>
        protected CoordinateSequence ReadCoordinateSequenceRing(BinaryReader reader, int size, CoordinateSystem cs)
        {
            var seqence = ReadCoordinateSequence(reader, size, cs);
            if (_isStrict)
                return seqence;
            if (CoordinateSequences.IsRing(seqence))
                return seqence;
            return CoordinateSequences.EnsureValidRing(SequenceFactory, seqence);
        }

        /// <summary>
        /// Function to read a coordinate sequence that is supposed to serve a line string.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="size">The number of ordinates</param>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The read coordinate sequence.</returns>
        protected CoordinateSequence ReadCoordinateSequenceLineString(BinaryReader reader, int size, CoordinateSystem cs)
        {
            var seq = ReadCoordinateSequence(reader, size, cs);
            if (_isStrict) return seq;
            if (seq.Count == 0 || seq.Count >= 2) return seq;
            return CoordinateSequences.Extend(GeometryServices.DefaultCoordinateSequenceFactory, seq, 2);
        }

        /// <summary>
        /// Function to convert from <see cref="CoordinateSystem"/> to <see cref="Ordinates"/>
        /// </summary>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The corresponding <see cref="Ordinates"/></returns>
        private static Ordinates ToOrdinates(CoordinateSystem cs)
        {
            var res = Ordinates.XY;
            if (cs == CoordinateSystem.XYM)
                res |= Ordinates.M;
            if (cs == CoordinateSystem.XYZ)
                res |= Ordinates.Z;
            if (cs == CoordinateSystem.XYZM)
                res |= (Ordinates.M | Ordinates.Z);
            return res;
        }

        /// <summary>
        /// Reads a <see cref="Point"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="Point"/> geometry</returns>
        protected Geometry ReadPoint(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            var seq = ReadCoordinateSequence(reader, 1, cs);
            if (double.IsNaN(seq.GetX(0)) || double.IsNaN(seq.GetY(0)))
                return factory.CreatePoint();
            return factory.CreatePoint(seq);
        }

        /// <summary>
        /// Reads a <see cref="LineString"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="LineString"/> geometry</returns>
        protected Geometry ReadLineString(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numPoints = ReadNumField(reader, "numPoints", ReasonableNumPoints(reader.BaseStream, cs));
            var sequence = ReadCoordinateSequenceLineString(reader, numPoints, cs);
            return factory.CreateLineString(sequence);
        }


        /// <summary>
        /// Reads a <see cref="LinearRing"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="LinearRing"/> geometry</returns>
        protected LinearRing ReadLinearRing(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numPoints = ReadNumField(reader, "numPoints", ReasonableNumPoints(reader.BaseStream, cs));
            var sequence = ReadCoordinateSequenceRing(reader, numPoints, cs);
            return factory.CreateLinearRing(sequence);
        }
        /// <summary>
        /// Reads a <see cref="Polygon"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="Polygon"/> geometry</returns>
        protected Geometry ReadPolygon(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int reasonable = ReasonableNumPoints(reader.BaseStream, cs);
            if (_isStrict) reasonable /= 4;
            int numRings = ReadNumField(reader, "numRings", reasonable);
            if (numRings == 0)
                return factory.CreatePolygon();
            
            var exteriorRing = ReadLinearRing(reader, cs, srid);
            var interiorRings = new LinearRing[numRings - 1];
            for (int i = 0; i < numRings - 1; i++)
                interiorRings[i] = ReadLinearRing(reader, cs, srid);
            
            return factory.CreatePolygon(exteriorRing, interiorRings);
        }

        /// <summary>
        /// Reads a <see cref="MultiPoint"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="MultiPoint"/> geometry</returns>
        protected Geometry ReadMultiPoint(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numGeometries = ReadNumField(reader, "numElems", ReasonableNumPoints(reader.BaseStream, cs));
            var points = new Point[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2 = srid;
                var geometryType = ReadGeometryType(reader, out cs2, ref srid2);//(WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPoint)
                    throw new ArgumentException("Point feature expected");
                points[i] = ReadPoint(reader, cs2, srid2) as Point;
            }
            return factory.CreateMultiPoint(points);
        }

        /// <summary>
        /// Reads a <see cref="MultiLineString"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="MultiLineString"/> geometry</returns>
        protected Geometry ReadMultiLineString(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numGeometries = ReadNumField(reader, "numElems", ReasonableNumPoints(reader.BaseStream, cs) / 2);
            var strings = new LineString[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2 = srid;
                var geometryType = ReadGeometryType(reader, out cs2, ref srid2);//(WKBGeometryTypes)reader.ReadInt32();
                if (srid2 < 0) srid2 = srid;
                if (geometryType != WKBGeometryTypes.WKBLineString)
                    throw new ArgumentException("LineString feature expected");
                strings[i] = ReadLineString(reader, cs2, srid2) as LineString;
            }
            return factory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// Reads a <see cref="MultiPolygon"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="MultiPolygon"/> geometry</returns>
        protected Geometry ReadMultiPolygon(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numGeometries = ReadNumField(reader, "numElems", ReasonableNumPoints(reader.BaseStream, cs));
            var polygons = new Polygon[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2 = srid;
                var geometryType = ReadGeometryType(reader, out cs2, ref srid2);//(WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPolygon)
                    throw new ArgumentException("Polygon feature expected");
                polygons[i] = ReadPolygon(reader, cs2, srid2) as Polygon;
            }
            return factory.CreateMultiPolygon(polygons);
        }

        /// <summary>
        /// Reads a <see cref="GeometryCollection"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="GeometryCollection"/> geometry</returns>
        protected Geometry ReadGeometryCollection(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);

            int numGeometries = ReadNumField(reader, "numElems", ReasonableNumPoints(reader.BaseStream, cs));
            var geometries = new Geometry[numGeometries];

            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2 = srid;
                var geometryType = ReadGeometryType(reader, out cs2, ref srid2);//(WKBGeometryTypes)reader.ReadInt32();

                switch (geometryType)
                {
                    //Point
                    case WKBGeometryTypes.WKBPoint:
                    case WKBGeometryTypes.WKBPointZ:
                    case WKBGeometryTypes.WKBPointM:
                    case WKBGeometryTypes.WKBPointZM:
                        geometries[i] = ReadPoint(reader, cs2, srid2);
                        break;

                    //Line String
                    case WKBGeometryTypes.WKBLineString:
                    case WKBGeometryTypes.WKBLineStringZ:
                    case WKBGeometryTypes.WKBLineStringM:
                    case WKBGeometryTypes.WKBLineStringZM:
                        geometries[i] = ReadLineString(reader, cs2, srid2);
                        break;

                    //Polygon
                    case WKBGeometryTypes.WKBPolygon:
                    case WKBGeometryTypes.WKBPolygonZ:
                    case WKBGeometryTypes.WKBPolygonM:
                    case WKBGeometryTypes.WKBPolygonZM:
                        geometries[i] = ReadPolygon(reader, cs2, srid2);
                        break;

                    //Multi Point
                    case WKBGeometryTypes.WKBMultiPoint:
                    case WKBGeometryTypes.WKBMultiPointZ:
                    case WKBGeometryTypes.WKBMultiPointM:
                    case WKBGeometryTypes.WKBMultiPointZM:
                        geometries[i] = ReadMultiPoint(reader, cs2, srid2);
                        break;

                    //Multi Line String
                    case WKBGeometryTypes.WKBMultiLineString:
                    case WKBGeometryTypes.WKBMultiLineStringZ:
                    case WKBGeometryTypes.WKBMultiLineStringM:
                    case WKBGeometryTypes.WKBMultiLineStringZM:
                        geometries[i] = ReadMultiLineString(reader, cs2, srid2);
                        break;

                    //Multi Polygon
                    case WKBGeometryTypes.WKBMultiPolygon:
                        geometries[i] = ReadMultiPolygon(reader, cs2, srid2);
                        break;

                    //Geometry Collection
                    case WKBGeometryTypes.WKBGeometryCollection:
                    case WKBGeometryTypes.WKBGeometryCollectionZ:
                    case WKBGeometryTypes.WKBGeometryCollectionM:
                    case WKBGeometryTypes.WKBGeometryCollectionZM:
                        geometries[i] = ReadGeometryCollection(reader, cs2, srid2);
                        break;

                    default:
                        throw new ArgumentException("Should never reach here!");
                }
            }
            return factory.CreateGeometryCollection(geometries);
        }

        /// <summary>
        /// Gets or sets a value indicating if a possibly encoded SRID value should be handled.
        /// </summary>
        public bool HandleSRID { get; set; }

        /// <summary>
        /// Gets a value indicating which ordinates can be handled.
        /// </summary>
        public Ordinates AllowedOrdinates => Ordinates.XYZM & SequenceFactory.Ordinates;

        private Ordinates _handleOrdinates;

        /// <summary>
        /// Gets a value indicating which ordinates should be handled.
        /// </summary>
        public Ordinates HandleOrdinates
        {
            get => _handleOrdinates;
            set
            {
                value = Ordinates.XY | (AllowedOrdinates & value);
                _handleOrdinates = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the reader should attempt to repair malformed input.
        /// </summary>
        /// <remarks>
        /// <i>Malformed</i> in this case means the ring has too few points (4),
        /// or is not closed.
        /// </remarks>
        public bool IsStrict
        {
            get => _isStrict;
            set => _isStrict = value;
        }

        /// <summary>
        /// Gets or sets whether invalid linear rings should be fixed
        /// </summary>
        [Obsolete("Use !IsStrict")]
        public bool RepairRings
        {
            get => !IsStrict;
            set => IsStrict = !value;
        }

        /// <summary>
        /// Function to determine whether an ordinate should be handled or not.
        /// </summary>
        /// <param name="ordinate"></param>
        /// <returns></returns>
        private bool HandleOrdinate(Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                case Ordinate.Y:
                    return true;
                case Ordinate.M:
                    return (HandleOrdinates & Ordinates.M) != Ordinates.None;
                case Ordinate.Z:
                    return (HandleOrdinates & Ordinates.Z) != Ordinates.None;
                default:
                    return false;
            }
        }
    }
}
