using System;
using NetTopologySuite.IO.UdtBase;
using Oracle.DataAccess.Types;

namespace NetTopologySuite.IO.Sdo
{
    [OracleCustomTypeMapping("MDSYS.SDO_GEOMETRY")]
    public class SdoGeometry : OracleCustomTypeBase<SdoGeometry>
    {

        private enum OracleObjectColumns { SDO_GTYPE, SDO_SRID, SDO_POINT, SDO_ELEM_INFO, SDO_ORDINATES }

        private decimal? _minX, _maxX, _minY, _maxY, _minZ, _maxZ;

        private decimal? sdo_Gtype;

        [OracleObjectMappingAttribute(0)]
        public decimal? Sdo_Gtype
        {
            get { return sdo_Gtype; }
            set { sdo_Gtype = value; }
        }

        private decimal? sdo_Srid;

        [OracleObjectMappingAttribute(1)]
        public decimal? Sdo_Srid
        {
            get { return sdo_Srid; }
            set { sdo_Srid = value; }
        }

        private SdoPoint point;

        [OracleObjectMappingAttribute(2)]
        public SdoPoint Point
        {
            get { return point; }
            set { point = value; }
        }

        private decimal[] elemArray;

        [OracleObjectMappingAttribute(3)]
        public decimal[] ElemArray
        {
            get { return elemArray; }
            set { elemArray = value; }
        }

        private decimal[] ordinatesArray;

        [OracleObjectMappingAttribute(4)]
        public decimal[] OrdinatesArray
        {
            get { return ordinatesArray; }
            set { ordinatesArray = value; }
        }

        [OracleCustomTypeMappingAttribute("MDSYS.SDO_ELEM_INFO_ARRAY")]
        public class ElemArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

        [OracleCustomTypeMappingAttribute("MDSYS.SDO_ORDINATE_ARRAY")]
        public class OrdinatesArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

        public override void MapFromCustomObject()
        {
            SetValue((int)OracleObjectColumns.SDO_GTYPE, Sdo_Gtype);
            SetValue((int)OracleObjectColumns.SDO_SRID, Sdo_Srid);
            SetValue((int)OracleObjectColumns.SDO_POINT, Point);
            SetValue((int)OracleObjectColumns.SDO_ELEM_INFO, ElemArray);
            SetValue((int)OracleObjectColumns.SDO_ORDINATES, OrdinatesArray);
        }

        public override void MapToCustomObject()
        {
            Sdo_Gtype = GetValue<decimal?>((int)OracleObjectColumns.SDO_GTYPE);
            Sdo_Srid = GetValue<decimal?>((int)OracleObjectColumns.SDO_SRID);
            Point = GetValue<SdoPoint>((int)OracleObjectColumns.SDO_POINT);
            ElemArray = GetValue<decimal[]>((int)OracleObjectColumns.SDO_ELEM_INFO);
            OrdinatesArray = GetValue<decimal[]>((int)OracleObjectColumns.SDO_ORDINATES);
        }

        private void GetMinMax()
        {
            _minX = _minY = _minZ = null;
            _maxX = _maxY = _maxZ = null;
            Int32 dim = Math.Min((Int32)Sdo_Gtype.Value/1000, 3);
            if (point != null)
            {
                _minX = _maxX = point.X;
                _minY = _maxY = point.Y;
                if (dim > 2)_minZ = _maxZ = point.Z;
            }

            if ( ordinatesArray != null )
            {
                for (int i = 0; i < ordinatesArray.Length; i+=dim )
                {
                    _minX = _minX.HasValue ? Math.Min(_minX.Value, ordinatesArray[i]) : ordinatesArray[i]; 
                    _minY = _minY.HasValue ? Math.Min(_minY.Value, ordinatesArray[i+1]) : ordinatesArray[i+1];
                    if ( dim > 2 ) _minZ = _minZ.HasValue ? Math.Min(_minZ.Value, ordinatesArray[i+2]) : ordinatesArray[i+2];
                    _maxX = _maxX.HasValue ? Math.Max(_maxX.Value, ordinatesArray[i]) : ordinatesArray[i];
                    _maxY = _maxY.HasValue ? Math.Max(_maxY.Value, ordinatesArray[i+1]) : ordinatesArray[i+1];
                    if ( dim > 2 )_maxZ = _maxZ.HasValue ? Math.Max(_maxZ.Value, ordinatesArray[i+2]) : ordinatesArray[i+2];
                }
            }
        }

        public Decimal MinX
        {
            get
            {
                if (!_minX.HasValue)
                    GetMinMax();
                return _minX.HasValue ? _minX.Value : Decimal.MinValue;
            }
        }

        public Decimal MinY
        {
            get
            {
                if (!_minY.HasValue)
                    GetMinMax();
                return _minY.HasValue ? _minY.Value : Decimal.MinValue;
            }
        }

        public Decimal MinZ
        {
            get
            {
                if (!_minZ.HasValue)
                    GetMinMax();
                return _minZ.HasValue ? _minZ.Value : Decimal.MinValue;
            }
        }
        public Decimal MaxX
        {
            get
            {
                if (!_maxX.HasValue)
                    GetMinMax();
                return _maxX.HasValue ? _maxX.Value : Decimal.MaxValue;
            }
        }

        public Decimal MaxY
        {
            get
            {
                if (!_maxY.HasValue)
                    GetMinMax();
                return _maxY.HasValue ? _maxY.Value : Decimal.MaxValue;
            }
        }

        public Decimal MaxZ
        {
            get
            {
                if (!_maxZ.HasValue)
                    GetMinMax();
                return _maxZ.HasValue ? _maxZ.Value : Decimal.MaxValue;
            }
        }
    }
}