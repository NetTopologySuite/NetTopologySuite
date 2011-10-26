using NetTopologySuite.IO.UdtBase;
using Oracle.DataAccess.Types;

namespace NetTopologySuite.IO.Sdo
{
    [OracleCustomTypeMappingAttribute("MDSYS.SDO_POINT_TYPE")]
    public class SdoPoint : OracleCustomTypeBase<SdoPoint>
    {
        private decimal? _x;
        private decimal? _y;
        private decimal? _z;
        [OracleObjectMappingAttribute("X")]
        public decimal? X
        {
            get { return _x; }
            set { _x = value; }
        }
        [OracleObjectMappingAttribute("Y")]
        public decimal? Y
        {
            get { return _y; }
            set { _y = value; }
        }
        [OracleObjectMapping("Z")]
        public decimal? Z
        {
            get { return _z; }
            set { _z = value; }
        }
        public override void MapFromCustomObject()
        {
            SetValue("X", _x);
            SetValue("Y", _y);
            SetValue("Z", _z);
        }
        public override void MapToCustomObject()
        {
            X = GetValue<decimal?>("X");
            Y = GetValue<decimal?>("Y");
            Z = GetValue<decimal?>("Z");
        }
    }
}