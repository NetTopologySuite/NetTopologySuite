using System;
using System.IO;
using GeoAPI;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Converts a Well-Known Binary byte data to a <c>Geometry</c>.
    /// </summary>
    public class WKBReader : IBinaryGeometryReader
    {
        ///<summary>
        /// Converts a hexadecimal string to a byte array.
        /// The hexadecimal digit symbols are case-insensitive.
        ///</summary>
        /// <param name="hex">A string containing hex digits</param>
        /// <returns>An array of bytes with the value of the hex string</returns>
        public static byte[] HexToBytes(String hex)
        {
            var byteLen = hex.Length / 2;
            var bytes = new byte[byteLen];

            for (var i = 0; i < hex.Length / 2; i++)
            {
                var i2 = 2 * i;
                if (i2 + 1 > hex.Length)
                    throw new ArgumentException("Hex string has odd length");

                var nib1 = HexToInt(hex[i2]);
                var nib0 = HexToInt(hex[i2 + 1]);
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
        private IGeometryFactory _factory;
        private readonly ICoordinateSequenceFactory _sequenceFactory;
        private readonly IPrecisionModel _precisionModel;

        private readonly IGeometryServices _geometryServices;

        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        [Obsolete]
        protected IGeometryFactory Factory
        {
            get { return _factory; }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>.
        /// </summary>
        public WKBReader() : this(GeometryServiceProvider.Instance) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        [Obsolete]
        public WKBReader(IGeometryFactory factory)
        {
            _geometryServices = GeometryServiceProvider.Instance;

            _factory = factory;
            _sequenceFactory = factory.CoordinateSequenceFactory;
            _precisionModel = factory.PrecisionModel;

            HandleSRID = true;
            HandleOrdinates = AllowedOrdinates;
        }

        public WKBReader(IGeometryServices services)
        {
            services = services ?? GeometryServiceProvider.Instance;
            _geometryServices = services;
            _precisionModel = services.DefaultPrecisionModel;
            _sequenceFactory = services.DefaultCoordinateSequenceFactory;

            HandleSRID = true;
            HandleOrdinates = AllowedOrdinates;
        }

        //IGeometryFactory IGeometryReader<byte[]>.Factory
        //{
        //    get { return _factory; }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _factory = value;
        //        }
        //    }
        //}

        /// <summary>
        /// Reads a <see cref="IGeometry"/> in binary WKB format from an array of <see cref="byte"/>s.
        /// </summary>
        /// <param name="data">The byte array to read from</param>
        /// <returns>The geometry read</returns>
        /// <exception cref="ParseException"> if the WKB data is ill-formed.</exception>
        public IGeometry Read(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
                return Read(stream);
        }

        /// <summary>
        /// Reads a <see cref="IGeometry"/> in binary WKB format from an <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The geometry read</returns>
        /// <exception cref="ParseException"> if the WKB data is ill-formed.</exception>
        public virtual IGeometry Read(Stream stream)
        {
            BinaryReader reader = null;
            var byteOrder = (ByteOrder)stream.ReadByte();
            try
            {
                reader = byteOrder == ByteOrder.BigEndian
                    ? new BEBinaryReader(stream)
                    : new BinaryReader(stream);
                return Read(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        protected enum CoordinateSystem { XY = 1, XYZ = 2, XYM = 3, XYZM = 4 };

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IGeometry Read(BinaryReader reader)
        {
            /*
            int typeInt = reader.ReadInt32();
            int srid = -1;
            var hasZ = (typeInt & 0x80000000) != 0;
            var hasM = (typeInt & 0x40000000) != 0;
            var hasSRID = (typeInt & 0x20000000) != 0;

            if (hasSRID)
                srid = reader.ReadInt32();

            if (HandleSRID)
            {
                if (_factory.SRID != srid)
                    _factory = new GeometryFactory(GeometryFactory.Default.PrecisionModel, srid);
            }
            else
            {
                _factory = GeometryFactory.Default;
            }

            typeInt = typeInt & 0xffff;
            if (hasZ && ((typeInt / 1000) & 1) != 1)
                typeInt += 1000;
            if (hasM && ((typeInt / 2000) & 1) != 1)
                typeInt += 2000;
            var geometryType = (WKBGeometryTypes) (typeInt );
            */
            CoordinateSystem cs;
            int srid;
            var geometryType = ReadGeometryType(reader, out cs, out srid);
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
        /// <returns></returns>
        protected ByteOrder ReadByteOrder(BinaryReader reader)
        {
            byte byteOrder = reader.ReadByte();
            return (ByteOrder)byteOrder;
        }

        private WKBGeometryTypes ReadGeometryType(BinaryReader reader, out CoordinateSystem coordinateSystem, out int srid)
        {
            var type = reader.ReadUInt32();
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
            var ordinate = (type & 0xffff) / 1000;
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

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="reader"></param>
        ///// <param name="cs"></param>
        ///// <returns></returns>
        //protected Coordinate ReadCoordinate(BinaryReader reader, CoordinateSystem cs)
        //{
        //    var x = reader.ReadDouble();
        //    var y = reader.ReadDouble();
        //    if (_precisionModel != null) x = _precisionModel.MakePrecise(x);
        //    if (_precisionModel != null) y = _precisionModel.MakePrecise(y);

        //    switch (cs)
        //    {
        //        case CoordinateSystem.XY:
        //            return new Coordinate(x, y);
        //        case CoordinateSystem.XYZ:
        //            var z = D
        //            if ((HandleOrdinates & Ordinates.Z ) != Ordinates.None)
        //                return new Coordinate(x, y, reader.ReadDouble());
        //            re
        //        case CoordinateSystem.XYM:
        //            reader.ReadDouble();
        //            return new Coordinate(x, y);
        //        case CoordinateSystem.XYZM:
        //            var z = reader.ReadDouble();
        //            reader.ReadDouble();
        //            return new Coordinate(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        //        default:
        //            throw new ArgumentException(String.Format("Coordinate system not supported: {0}", cs));
        //    }
        //}

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
            for (var i = 0; i < size; i++)
            {
                var x = reader.ReadDouble();
                var y = reader.ReadDouble();

                if (_precisionModel != null) x = _precisionModel.MakePrecise(x);
                if (_precisionModel != null) y = _precisionModel.MakePrecise(y);

                sequence.SetOrdinate(i, Ordinate.X, x);
                sequence.SetOrdinate(i, Ordinate.Y, y);

                switch (cs)
                {
                    case CoordinateSystem.XY:
                        continue;
                    case CoordinateSystem.XYZ:
                        var z = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.Z))
                            sequence.SetOrdinate(i, Ordinate.Z, z);
                        break;
                    case CoordinateSystem.XYM:
                        var m = reader.ReadDouble();
                        if (HandleOrdinate(Ordinate.M))
                            sequence.SetOrdinate(i, Ordinate.M, m);
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
                        throw new ArgumentException(String.Format("Coordinate system not supported: {0}", cs));
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
            if (!RepairRings) return seqence;
            if (CoordinateSequences.IsRing(seqence)) return seqence;
            return CoordinateSequences.EnsureValidRing(_sequenceFactory, seqence);
        }

        /// <summary>
        /// Function to convert from <see cref="CoordinateSystem"/> to <see cref="Ordinates"/>
        /// </summary>
        /// <param name="cs">The coordinate system</param>
        /// <returns>The corresponding <see cref="Ordinates"/></returns>
        private static Ordinates ToOrdinates(CoordinateSystem cs)
        {
            var res = Ordinates.XY;
            if (cs == CoordinateSystem.XYM) res |= Ordinates.M;
            if (cs == CoordinateSystem.XYZ) res |= Ordinates.Z;
            if (cs == CoordinateSystem.XYZM) res |= (Ordinates.M | Ordinates.Z);
            return res;
        }

        /// <summary>
        /// Reads a <see cref="ILinearRing"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="ILinearRing"/> geometry</returns>
        protected ILinearRing ReadLinearRing(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            var numPoints = reader.ReadInt32();
            var sequence = ReadCoordinateSequenceRing(reader, numPoints, cs);
            return factory.CreateLinearRing(sequence);
        }

        /// <summary>
        /// Reads a <see cref="IPoint"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="IPoint"/> geometry</returns>
        protected IGeometry ReadPoint(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            return factory.CreatePoint(ReadCoordinateSequence(reader, 1, cs));
        }

        /// <summary>
        /// Reads a <see cref="ILineString"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="ILineString"/> geometry</returns>
        protected IGeometry ReadLineString(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            var numPoints = reader.ReadInt32();
            var sequence = ReadCoordinateSequence(reader, numPoints, cs);
            return factory.CreateLineString(sequence);
        }

        /// <summary>
        /// Reads a <see cref="IPolygon"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="IPolygon"/> geometry</returns>
        protected IGeometry ReadPolygon(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            var numRings = reader.ReadInt32();
            var exteriorRing = ReadLinearRing(reader, cs, srid);
            var interiorRings = new ILinearRing[numRings - 1];
            for (var i = 0; i < numRings - 1; i++)
                interiorRings[i] = ReadLinearRing(reader, cs, srid);
            return factory.CreatePolygon(exteriorRing, interiorRings);
        }

        /// <summary>
        /// Reads a <see cref="IMultiPoint"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="IMultiPoint"/> geometry</returns>
        protected IGeometry ReadMultiPoint(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            var numGeometries = reader.ReadInt32();
            var points = new IPoint[numGeometries];
            for (var i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);//(WKBGeometryTypes)reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPoint)
                    throw new ArgumentException("IPoint feature expected");
                points[i] = ReadPoint(reader, cs2, srid2) as IPoint;
            }
            return factory.CreateMultiPoint(points);
        }

        /// <summary>
        /// Reads a <see cref="IMultiLineString"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="IMultiLineString"/> geometry</returns>
        protected IGeometry ReadMultiLineString(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            var numGeometries = reader.ReadInt32();
            var strings = new ILineString[numGeometries];
            for (int i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);//(WKBGeometryTypes) reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBLineString)
                    throw new ArgumentException("ILineString feature expected");
                strings[i] = ReadLineString(reader, cs2, srid2) as ILineString;
            }
            return factory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// Reads a <see cref="IMultiPolygon"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="IMultiPolygon"/> geometry</returns>
        protected IGeometry ReadMultiPolygon(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);
            var numGeometries = reader.ReadInt32();
            var polygons = new IPolygon[numGeometries];
            for (var i = 0; i < numGeometries; i++)
            {
                ReadByteOrder(reader);
                CoordinateSystem cs2;
                int srid2;
                var geometryType = ReadGeometryType(reader, out cs2, out srid2);//(WKBGeometryTypes) reader.ReadInt32();
                if (geometryType != WKBGeometryTypes.WKBPolygon)
                    throw new ArgumentException("IPolygon feature expected");
                polygons[i] = ReadPolygon(reader, cs2, srid2) as IPolygon;
            }
            return factory.CreateMultiPolygon(polygons);
        }

        /// <summary>
        /// Reads a <see cref="IGeometryCollection"/> geometry.
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="cs">The coordinate system</param>
        ///<param name="srid">The spatial reference id for the geometry.</param>
        ///<returns>A <see cref="IGeometryCollection"/> geometry</returns>
        protected IGeometry ReadGeometryCollection(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = _geometryServices.CreateGeometryFactory(_precisionModel, srid, _sequenceFactory);

            var numGeometries = reader.ReadInt32();
            var geometries = new IGeometry[numGeometries];

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

        public Ordinates AllowedOrdinates
        {
            get { return Ordinates.XYZM & _sequenceFactory.Ordinates; }
        }

        private Ordinates _handleOrdinates;

        public Ordinates HandleOrdinates
        {
            get { return _handleOrdinates; }
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
        public bool RepairRings { get; set; }

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