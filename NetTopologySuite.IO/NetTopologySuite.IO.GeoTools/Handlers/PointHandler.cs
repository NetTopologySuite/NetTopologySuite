 
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Converts a Shapefile point to a OGIS Point.
    /// </summary>
    public class PointHandler : ShapeHandler
    {
        public PointHandler() : base(ShapeGeometryType.Point)
        {            
        }
        public PointHandler(ShapeGeometryType type)
            : base(type)
        {
        }
		
        /// <summary>
        /// Reads a stream and converts the shapefile record to an equilivent geometry object.
        /// </summary>
        /// <param name="file">The stream to read.</param>
        /// <param name="totalRecordLength">Total length of the record we are about to read</param>
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public override IGeometry Read(BigEndianBinaryReader file, int totalRecordLength, IGeometryFactory geometryFactory)
        {
            int totalRead = 0;
            var type = (ShapeGeometryType)ReadInt32(file, totalRecordLength, ref totalRead);
            //type = (ShapeGeometryType) EnumUtility.Parse(typeof (ShapeGeometryType), shapeTypeNum.ToString());
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreatePoint((Coordinate)null);

            if (type != ShapeType)
                throw new ShapefileException(string.Format("Encountered a '{0}' instead of a  '{1}'", type, ShapeType));

            var buffer = new CoordinateBuffer(1, NoDataBorderValue, true);
            var precisionModel = geometryFactory.PrecisionModel;

            var x = precisionModel.MakePrecise(ReadDouble(file, totalRecordLength, ref totalRead));
            var y = precisionModel.MakePrecise(ReadDouble(file, totalRecordLength, ref totalRead));

            double? z = null, m = null;
            
            // Trond Benum: Let's read optional Z and M values                                
            if (HasZValue())
                z = ReadDouble(file, totalRecordLength, ref totalRead);

            if (HasMValue() || HasZValue())
                m = ReadDouble(file, totalRecordLength, ref totalRead);

            buffer.AddCoordinate(x, y, z, m);
            return geometryFactory.CreatePoint(buffer.ToSequence(geometryFactory.CoordinateSequenceFactory));
        }

        /// <summary>
        /// Writes to the given stream the equilivent shape file record given a Geometry object.
        /// </summary>
        /// <param name="geometry">The geometry object to write.</param>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public override void Write(IGeometry geometry, BinaryWriter writer, IGeometryFactory geometryFactory)
        {
            writer.Write((int)ShapeType);

            var point = (IPoint) geometry;
            var seq = point.CoordinateSequence;

            writer.Write(seq.GetOrdinate(0, Ordinate.X));
            writer.Write(seq.GetOrdinate(0, Ordinate.Y));

            // If we have Z, write it.
            if (HasZValue())
            {
                writer.Write(seq.GetOrdinate(0, Ordinate.Z));
            }

            // If we have a Z, we also have M, this is shapefile definition
            if (HasMValue() || HasZValue())
            {
                writer.Write(HasMValue() ? seq.GetOrdinate(0, Ordinate.M) : NoDataValue);
            }
        }

        /// <summary>
        /// Gets the length in words (1 word = 2 bytes) the Geometry will need when written as a shape file record.
        /// </summary>
        /// <param name="geometry">The Geometry object to use.</param>
        /// <returns>The length in words (1 word = 2 bytes) the Geometry will use when represented as a shape file record.</returns>
        public override int ComputeRequiredLengthInWords(IGeometry geometry)
        {
            if (HasZValue())
                // 18 => shapetype(2)+ xyzm(4*4)
                return 18;
            if (HasMValue())
                // 14 => shapetype(2)+ xym(3*4)
                return 14; 
            
            // 10 => shapetype(2)+ xy(2*4)
            return 10;
        }

        public override IEnumerable<MBRInfo> ReadMBRs(BigEndianBinaryReader reader)
        {
            return new PointMBREnumerator(reader);
        }
    }
}