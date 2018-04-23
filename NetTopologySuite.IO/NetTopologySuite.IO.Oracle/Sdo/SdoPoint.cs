using NetTopologySuite.IO.UdtBase;
using Oracle.DataAccess.Types;

namespace NetTopologySuite.IO.Sdo
{
    [OracleCustomTypeMappingAttribute("MDSYS.SDO_POINT_TYPE")]
    public class SdoPoint : OracleCustomTypeBase<SdoPoint>
    {
        [OracleObjectMappingAttribute("X")]
        public decimal? X { get; set; }

        [OracleObjectMappingAttribute("Y")]
        public decimal? Y { get; set; }

        [OracleObjectMapping("Z")]
        public decimal? Z { get; set; }

        public override void MapFromCustomObject()
        {
            SetValue("X", X);
            SetValue("Y", Y);
            SetValue("Z", Z);
        }
        public override void MapToCustomObject()
        {
            X = GetValue<decimal?>("X");
            Y = GetValue<decimal?>("Y");
            Z = GetValue<decimal?>("Z");
        }
    }
}