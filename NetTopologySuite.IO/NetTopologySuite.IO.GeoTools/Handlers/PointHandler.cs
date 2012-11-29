using System;
using System.IO;
using GeoAPI.Geometries;

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
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public override IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory)
        {
            var type = (ShapeGeometryType)file.ReadInt32();
            //type = (ShapeGeometryType) EnumUtility.Parse(typeof (ShapeGeometryType), shapeTypeNum.ToString());
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreatePoint((Coordinate)null);

            if (type != ShapeType)
                throw new ShapefileException(string.Format("Encountered a '{0}' instead of a  '{1}'", type, ShapeType));

            var buffer = new CoordinateBuffer(1, NoDataBorderValue, true);
            var precisionModel = geometryFactory.PrecisionModel;

            var x = precisionModel.MakePrecise(file.ReadDouble());
            var y = precisionModel.MakePrecise(file.ReadDouble());

            double? z = null, m = null;
            
            // Trond Benum: Let's read optional Z and M values                                
            if (HasZValue())
                z = file.ReadDouble();
            
            if (HasMValue() || HasZValue())
                m = file.ReadDouble();

            buffer.AddCoordinate(x, y, z, m);
            return geometryFactory.CreatePoint(buffer.ToSequence(geometryFactory.CoordinateSequenceFactory));
        }

        /// <summary>
        /// Writes to the given stream the equilivent shape file record given a Geometry object.
        /// </summary>
        /// <param name="geometry">The geometry object to write.</param>
        /// <param name="file">The stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public override void Write(IGeometry geometry, BinaryWriter file, IGeometryFactory geometryFactory)
        {
            file.Write((int)ShapeType);

            var point = (IPoint) geometry;
            var seq = point.CoordinateSequence;

            file.Write(seq.GetOrdinate(0, Ordinate.X));
            file.Write(seq.GetOrdinate(0, Ordinate.Y));

            // If we have Z, write it.
            if (HasZValue())
            {
                file.Write(seq.GetOrdinate(0, Ordinate.Z));
            }

            // If we have a Z, we also have M, this is shapefile definition
            if (HasMValue() || HasZValue())
            {
                file.Write(HasMValue() ? seq.GetOrdinate(0, Ordinate.M) : NoDataValue);
            }
        }

        /// <summary>
        /// Gets the length in bytes the Geometry will need when written as a shape file record.
        /// </summary>
        /// <param name="geometry">The Geometry object to use.</param>
        /// <returns>The length in bytes the Geometry will use when represented as a shape file record.</returns>
        public override int GetLength(IGeometry geometry)
        {
            return 10; // 10 => shapetyppe(2)+ xy(4*2)
        }		
    }
}