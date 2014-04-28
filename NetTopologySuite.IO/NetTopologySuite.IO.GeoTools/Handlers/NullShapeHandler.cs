using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    public class NullShapeHandler : ShapeHandler
    {
        public NullShapeHandler()
            : base(ShapeGeometryType.NullShape)
        {
        }

        public override IGeometry Read(BigEndianBinaryReader file,
            int totalRecordLength, IGeometryFactory factory)
        {
            return null;
        }

        public override void Write(IGeometry geometry, BinaryWriter writer, IGeometryFactory factory)
        { }

        public override int ComputeRequiredLengthInWords(IGeometry geometry)
        {
            return -1;
        }

        public override IEnumerable<MBRInfo> ReadMBRs(BigEndianBinaryReader reader)
        {
            reader.Close();
            return new MBRInfo[0];
        }
    }
}
