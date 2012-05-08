using System;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;

namespace NetTopologySuite.IO
{
    public class PostGis2Writer : IBinaryGeometryWriter
    {
        private Ordinates _handleOrdinates;

        /// <summary>
        /// Gets or sets whether the geometry notation should be geodetic
        /// </summary>
        public bool IsGeodetic { get; set; }
        
        #region Implementation of IGeometryIOSettings

        /// <summary>
        /// Gets or sets whether the SpatialReference ID must be handled.
        /// </summary>
        public bool HandleSRID
        {
            get
            {
                //Postgis 2 always has SRID
                return true;
            }
            set { }
        }

        /// <summary>
        /// Gets and <see cref="Ordinates"/> flag that indicate which ordinates can be handled.
        /// </summary>
        /// <remarks>
        /// This flag must always return at least <see cref="Ordinates.XY"/>.
        /// </remarks>
        public Ordinates AllowedOrdinates
        {
            get
            {
                return IsGeodetic ? Ordinates.XYZ : Ordinates.XYZM;
            }
        }

        /// <summary>
        /// Gets and sets <see cref="Ordinates"/> flag that indicate which ordinates shall be handled.
        /// </summary>
        /// <remarks>
        /// No matter which <see cref="Ordinates"/> flag you supply, <see cref="Ordinates.XY"/> are always processed,
        /// the rest is binary and 'ed with <see cref="IGeometryIOSettings.AllowedOrdinates"/>.
        /// </remarks>
        public Ordinates HandleOrdinates
        {
            get { return _handleOrdinates; }
            set
            {
                if (IsGeodetic)
                {
                    _handleOrdinates = Ordinates.XYZ;
                    return;
                } 
                _handleOrdinates = Ordinates.XY | value;
            }
        }

        #endregion

        #region Implementation of IGeometryWriter<byte[]>

        /// <summary>
        /// Writes a binary representation of a given geometry.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns>The binary representation of <paramref name="geometry"/></returns>
        public byte[] Write(IGeometry geometry)
        {
            using (var ms = new MemoryStream())
            {
                Write(geometry, ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Writes a binary representation of a given geometry.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        public void Write(IGeometry geometry, Stream stream)
        {
            using (var writer = CreateWriter(stream))
            {
                Write(geometry, writer, 
                    AllowedOrdinates & HandleOrdinates, 
                    IsGeodetic);
            }
        }

        private static void Write(IGeometry geometry, BinaryWriter writer, Ordinates handleOrdinates, bool isGeodetic)
        {
            var pgh = new PostGis2GeometryHeader(geometry, handleOrdinates) {IsGeodetic = isGeodetic};
            pgh.Write(writer);

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    WritePoint((IPoint) geometry, pgh, writer);
                    break;
                case OgcGeometryType.LineString:
                    WriteLineString((ILineString)geometry, pgh, writer);
                    break;
                case OgcGeometryType.Polygon:
                    WritePolygon((IPolygon)geometry, pgh, writer);
                    break;
                case OgcGeometryType.MultiPoint:
                case OgcGeometryType.MultiLineString:
                case OgcGeometryType.MultiPolygon:
                case OgcGeometryType.GeometryCollection:
                    WriteCollection((IGeometryCollection) geometry, pgh, writer);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void WriteSequence(ICoordinateSequence sequence, PostGis2GeometryHeader pgh, BinaryWriter writer, int numPoints)
        {
            if (numPoints == -1)
            {
                numPoints = sequence.Count;
                writer.Write(numPoints);
            }
            
            var ordinateIndices = pgh.OrdinateIndices;
            for (var i = 0; i < numPoints; i++)
            {
                foreach (var ordinateIndex in ordinateIndices)
                    writer.Write(sequence.GetOrdinate(i, ordinateIndex));
            }
        }

        private static void WritePoint(IPoint geometry, PostGis2GeometryHeader pgh, BinaryWriter writer)
        {
            writer.Write((uint)PostGis2GeometryType.Point);
            WriteSequence(geometry.CoordinateSequence, pgh, writer, -1);
        }

        private static void WriteLineString(ILineString geometry, PostGis2GeometryHeader pgh, BinaryWriter writer)
        {
            writer.Write((uint)PostGis2GeometryType.LineString);
            WriteSequence(geometry.CoordinateSequence, pgh, writer, -1);
        }

        private static void WritePolygon(IPolygon geometry, PostGis2GeometryHeader pgh, BinaryWriter writer)
        {
            writer.Write((uint)PostGis2GeometryType.Polygon);
            if (geometry.IsEmpty)
            {
                writer.Write(0);
                return;
            }

            var numRings = 1 + geometry.NumInteriorRings;
            
            //shell
            writer.Write(numRings);
            writer.Write(geometry.Shell.NumPoints);

            //holes
            for (var i = 0; i < geometry.NumInteriorRings; i++ )
                writer.Write(geometry.GetInteriorRingN(i).NumPoints);
            
            //pad
            if (numRings % 2 != 0)
                writer.Write(0);

            WriteSequence(geometry.Shell.CoordinateSequence, pgh, writer, geometry.Shell.NumPoints);
            for (var i = 0; i < geometry.NumInteriorRings; i++ )
            {
                var sequence = geometry.GetInteriorRingN(i).CoordinateSequence;
                WriteSequence(sequence, pgh, writer, sequence.Count);
            }
        }

        private static void WriteCollection(IGeometryCollection geometry, PostGis2GeometryHeader pgh, BinaryWriter writer)
        {
            writer.Write((uint)geometry.OgcGeometryType.ToPostGis2());
            if (geometry.IsEmpty)
            {
                writer.Write(0);
                return;
            }

            for (var i = 0; i < geometry.NumGeometries; i++)
                Write(geometry.GetGeometryN(i), writer, pgh.Ordinates, pgh.IsGeodetic);
        }

        #endregion

        #region Implementation of IBinaryGeometryWriter

        /// <summary>
        /// Gets or sets the desired <see cref="IBinaryGeometryWriter.ByteOrder"/>
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        private BinaryWriter CreateWriter(Stream stream)
        {
            return (ByteOrder == ByteOrder.LittleEndian)
                ? new BinaryWriter(stream)
                : new BEBinaryWriter(stream);
        }

        #endregion
    }
}