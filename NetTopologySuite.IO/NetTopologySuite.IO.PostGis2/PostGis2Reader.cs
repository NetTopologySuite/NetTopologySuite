using System;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;

namespace NetTopologySuite.IO
{
    public class PostGis2Reader : IBinaryGeometryReader
    {
        #region Implementation of IGeometryIOSettings

        /// <summary>
        /// Gets or sets whether the SpatialReference ID must be handled.
        /// </summary>
        public bool HandleSRID
        {
            get; set;
        }

        /// <summary>
        /// Gets and <see cref="Ordinates"/> flag that indicate which ordinates can be handled.
        /// </summary>
        /// <remarks>
        /// This flag must always return at least <see cref="Ordinates.XY"/>.
        /// </remarks>
        public Ordinates AllowedOrdinates
        {
            get { return Ordinates.XYZM; }
        }

        /// <summary>
        /// Gets and sets <see cref="Ordinates"/> flag that indicate which ordinates shall be handled.
        /// </summary>
        /// <remarks>
        /// No matter which <see cref="Ordinates"/> flag you supply, <see cref="Ordinates.XY"/> are always processed,
        /// the rest is binary and 'ed with <see cref="IGeometryIOSettings.AllowedOrdinates"/>.
        /// </remarks>
        public Ordinates HandleOrdinates { get; set; }

        #endregion

        #region Implementation of IGeometryReader<byte[]>

        /// <summary>
        /// Reads a geometry representation from a byte array to a <c>Geometry</c>.
        /// </summary>
        /// <param name="source">
        /// The source to read the geometry from
        /// </param>
        /// <returns>
        /// A <c>Geometry</c>
        /// </returns>
        public IGeometry Read(byte[] source)
        {
            using (var ms = new MemoryStream(source))
            {
                return Read(ms);
            }
        }

        /// <summary>
        /// Reads a geometry representation from a <see cref="System.IO.Stream"/> to a <c>Geometry</c>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// A <c>Geometry</c>
        public IGeometry Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                return Read(reader);
            }
        }

        private static IGeometry Read(BinaryReader reader)
        {
            var header = new PostGis2GeometryHeader(reader);
            return Read(header, reader);
        }

        private static IGeometry Read(PostGis2GeometryHeader header, BinaryReader reader)
        {
            var geometryType = (PostGis2GeometryType) reader.ReadUInt32();
            switch (geometryType)
            {
                case PostGis2GeometryType.Point:
                    return header.Factory.CreatePoint(ReadSequence(header, reader, -1));
                
                case PostGis2GeometryType.LineString:
                    return ReadLineString(header, reader, -1, false);
                
                case PostGis2GeometryType.Polygon:
                    return ReadPolygon(header, reader);

                case PostGis2GeometryType.MultiPoint:
                    return ReadMultiPoint(header, reader);
                    
                case PostGis2GeometryType.MultiLineString:
                    return ReadMultiLineString(header, reader);
                    
                case PostGis2GeometryType.MultiPolygon:
                    return ReadMultiPolygon(header, reader);
                    
                case PostGis2GeometryType.GeometryCollection:
                    return ReadGeometryCollection(header, reader);
                default:
                    throw new NotSupportedException();
            }
        }

        private static TGeometry[] ReadGeometryArray<TGeometry> (BinaryReader reader)
            where TGeometry : IGeometry
        {
            var numGeometries = reader.ReadInt32();
            var geoms = new TGeometry[numGeometries];
            for (var i = 0; i < numGeometries; i++)
                geoms[i] = (TGeometry)Read(reader);
            
            return geoms;
        }

        private static IGeometry ReadGeometryCollection(PostGis2GeometryHeader header, BinaryReader reader)
        {
            var geoms = ReadGeometryArray<IGeometry>(reader);
            return header.Factory.CreateGeometryCollection(geoms);
        }

        private static IGeometry ReadMultiPolygon(PostGis2GeometryHeader header, BinaryReader reader)
        {
            var geoms = ReadGeometryArray<IPolygon>(reader);
            return header.Factory.CreateMultiPolygon(geoms);
        }

        private static IGeometry ReadMultiLineString(PostGis2GeometryHeader header, BinaryReader reader)
        {
            var geoms = ReadGeometryArray<ILineString>(reader);
            return header.Factory.CreateMultiLineString(geoms);
        }

        private static IGeometry ReadMultiPoint(PostGis2GeometryHeader header, BinaryReader reader)
        {
            var geoms = ReadGeometryArray<IPoint>(reader);
            return header.Factory.CreateMultiPoint(geoms);
        }

        private static ICoordinateSequence ReadSequence(PostGis2GeometryHeader header, BinaryReader reader, int numPoints)
        {
            if (numPoints < 0) 
                numPoints = reader.ReadInt32();
            
            var sequence = header.Factory.CoordinateSequenceFactory.Create(numPoints, header.Ordinates);
            var ordinateIndices = header.OrdinateIndices;
            for (var i = 0; i < numPoints; i++)
            {
                foreach (var ord in ordinateIndices)
                    sequence.SetOrdinate(i, ord, reader.ReadDouble());
            }
            
            return sequence;
        }
        
        private static ILineString ReadLineString(PostGis2GeometryHeader header, BinaryReader reader, int numPoints, bool ring)
        {
            return ring 
                ? header.Factory.CreateLinearRing(ReadSequence(header, reader, numPoints))
                : header.Factory.CreateLineString(ReadSequence(header, reader, numPoints));
        }

        private static IPolygon ReadPolygon(PostGis2GeometryHeader header, BinaryReader reader)
        {
            var numRings = reader.ReadInt32();
            if (numRings == 0)
                return header.Factory.CreatePolygon((ILinearRing)null);

            var pointsPerRing = new int[numRings];
            int i;
            for (i = 0; i < numRings; i++)
                pointsPerRing[i] = reader.ReadInt32();
            
            //padding
            if (numRings % 2 != 0) reader.ReadInt32();

            //read shell
            var shell = (ILinearRing)ReadLineString(header, reader, pointsPerRing[0], true);
            
            //read holes
            var holes = new ILinearRing[numRings - 1];
            for (i = 1; i < numRings; i++)
                holes[i - 1] = (ILinearRing)ReadLineString(header, reader, pointsPerRing[i], true);
            
            //create polygon
            return header.Factory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Gets or sets whether invalid linear rings should be fixed
        /// </summary>
        public bool RepairRings
        {
            get
            {
                // Postgis rings are always in order!
                return false;
            }
            set { }
        }

        #endregion
    }
}