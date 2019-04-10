using System;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Converts a Well-Known Binary byte data to a <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// The reader repairs structurally-invalid input
    /// (specifically, LineStrings and LinearRings which contain
    /// too few points have vertices added,
    /// and non-closed rings are closed).
    /// s</remarks>
    public class WKBReader : IBinaryGeometryReader
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

        [Obsolete]
        private readonly GeometryFactory _factory;
        private readonly ICoordinateSequenceFactory _sequenceFactory;
        private readonly PrecisionModel _precisionModel;

        private readonly IGeometryServices _geometryServices;
        /**
         * true if structurally invalid input should be reported rather than repaired.
         * At some point this could be made client-controllable.
         */
        private bool _isStrict;

        /// <summary>
        /// The <see cref="Geometry"/> builder.
        /// </summary>
        [Obsolete]
        protected GeometryFactory Factory => _factory;

        /// <summary>
        /// Initialize reader with a standard <see cref="GeometryFactory"/>.
        /// </summary>
        public WKBReader() : this(NtsGeometryServices.Instance) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        [Obsolete]
        public WKBReader(GeometryFactory factory)
        {
            _geometryServices = NtsGeometryServices.Instance;

            _factory = factory;
            _sequenceFactory = factory.CoordinateSequenceFactory;
            _precisionModel = factory.PrecisionModel;

            HandleSRID = true;
            HandleOrdinates = AllowedOrdinates;
        }

        public WKBReader(IGeometryServices services)
        {
            services = services ?? NtsGeometryServices.Instance;
            _geometryServices = services;
            _precisionModel = services.DefaultPrecisionModel;
            _sequenceFactory = services.DefaultCoordinateSequenceFactory;

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

        protected enum CoordinateSystem { XY = 1, XYZ = 2, XYM = 3, XYZM = 4 };

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected Geometry Read(BinaryReader reader)
        {
            ReadByteOrder(reader);
            var geometryType = ReadGeometryType(reader, out var cs, out int srid);
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

        private WKBGeometryTypes ReadGeometryType(BinaryReader reader, out CoordinateSystem coordinateSystem, out int srid)
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
            if ((type & 0x20000000) != 0)
                srid = reader.ReadInt32();
            else
                srid = -1;

            if (!HandleSRID) srid = -1;

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
        /// Function to read a coordinate sequence.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="size">The number of ordinates</param>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The read coordinate sequence.</returns>
        protected ICoordinateSequence ReadCoordinateSequence(BinaryReader reader, int size, CoordinateSystem cs)
        {
            var sequence = _sequenceFactory.Create(size, ToOrdinates(cs));
            for (int i = 0; i < size; i++)
            {
                double x = reader.ReadDouble();
                double y = reader.ReadDouble();

                if (_precisionModel != null) x = _precisionModel.MakePrecise(x);
                if (_precisionModel != null) y = _precisionModel.MakePrecise(y);

                sequence.SetOrdinate(i, Ordinate.X, x);
                sequence.SetOrdinate(i, Ordinate.Y, y);

                switch (cs)
                {
                    case CoordinateSystem.XY:
                        continue;
                    case CoordinateSystem.XYZ:
                        double z = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.Z))
                            sequence.SetOrdinate(i, Ordinate.Z, z);
                        break;
                    case CoordinateSystem.XYM:
                        double m = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.M))
                            sequence.SetOrdinate(i, Ordinate.Ordinate2, m);
                        break;
                    case CoordinateSystem.XYZM:
                        z = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.Z))
                            sequence.SetOrdinate(i, Ordinate.Z, z);
                        m = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.M))
                            sequence.SetOrdinate(i, Ordinate.M, m);
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
        protected ICoordinateSequence ReadCoordinateSequenceRing(BinaryReader reader, int size, CoordinateSystem cs)
        {
            var seqence = ReadCoordinateSequence(reader, size, cs);
            if (_isStrict)
                return seqence;
            if (CoordinateSequences.IsRing(seqence))
                return seqence;
            return CoordinateSequences.EnsureValidRing(_sequenceFactory, seqence);
        }

        /// <summary>
        /// Function to read a coordinate sequence that is supposed to serve a line string.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="size">The number of ordinates</param>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The read coordinate sequence.</returns>
        protected ICoordinateSequence ReadCoordinateSequenceLineString(BinaryReader reader, int size, CoordinateSystem cs)
        {
            var seq = ReadCoordinateSequence(reader, size, cs);
            if (_isStrict) return seq;
            if (seq.Count == 0 || seq.Count >= 2) return seq;
            return CoordinateSequences.Extend(_geometryServices.DefaultCoordinateSequenceFactory, seq, 2);
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
        /// Reads a <see cref="LinearRing"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        /// <param name="srid">The spatial reference id for the geometry.</param>
        /// <returns>A <see cref="LinearRing"/> geometry</returns>
        protected LinearRing ReadLinearRing(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            int numPoints = reader.ReadInt32();
            var sequence = ReadCoordinateSequenceRing(reader, numPoints, cs);
            return factory.CreateLinearRing(sequence);
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            return factory.CreatePoint(ReadCoordinateSequence(reader, 1, cs));
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            int numPoints = reader.ReadInt32();
            var sequence = ReadCoordinateSequenceLineString(reader, numPoints, cs);
            return factory.CreateLineString(sequence);
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            LinearRing exteriorRing = null;
            LinearRing[] interiorRings = null;
            int numRings = reader.ReadInt32();
            if (numRings > 0)
            {
                exteriorRing = ReadLinearRing(reader, cs, srid);
                interiorRings = new LinearRing[numRings - 1];
                for (int i = 0; i < numRings - 1; i++)
                    interiorRings[i] = ReadLinearRing(reader, cs, srid);
            }
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            int numGeometries = reader.ReadInt32();
            var points = new Point[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);//(WKBGeometryTypes)reader.ReadInt32();
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            int numGeometries = reader.ReadInt32();
            var strings = new LineString[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);//(WKBGeometryTypes) reader.ReadInt32();
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            int numGeometries = reader.ReadInt32();
            var polygons = new Polygon[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);//(WKBGeometryTypes) reader.ReadInt32();
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
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);

            int numGeometries = reader.ReadInt32();
            var geometries = new Geometry[numGeometries];

            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);
                switch (geometryType)
                {
                    //Point
                    case WKBGeometryTypes.WKBPoint:
                    case WKBGeometryTypes.WKBPointZ:
                    case WKBGeometryTypes.WKBPointM:
                    case WKBGeometryTypes.WKBPointZM:
                        geometries[i] = ReadPoint(reader, cs2, srid);
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

        #region Implementation of IGeometryIOSettings

        public bool HandleSRID { get; set; }

        public Ordinates AllowedOrdinates => Ordinates.XYZM & _sequenceFactory.Ordinates;

        private Ordinates _handleOrdinates;

        public Ordinates HandleOrdinates
        {
            get => _handleOrdinates;
            set
            {
                value = Ordinates.XY | (AllowedOrdinates & value);
                _handleOrdinates = value;
            }
        }

        #endregion Implementation of IGeometryIOSettings

        /// <summary>
        /// Gets or sets whether invalid linear rings should be fixed
        /// </summary>
        public bool RepairRings { get => _isStrict;
            set => _isStrict = value;
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
