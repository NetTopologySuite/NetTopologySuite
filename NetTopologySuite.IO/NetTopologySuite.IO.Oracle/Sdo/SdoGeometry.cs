using System;
using NetTopologySuite.IO.UdtBase;
using Oracle.DataAccess.Types;

namespace NetTopologySuite.IO.Sdo
{
    [OracleCustomTypeMapping("MDSYS.SDO_GEOMETRY")]
    public class SdoGeometry : OracleCustomTypeBase<SdoGeometry>
    {

// ReSharper disable InconsistentNaming
        private enum OracleObjectColumns { SDO_GTYPE, SDO_SRID, SDO_POINT, SDO_ELEM_INFO, SDO_ORDINATES }
// ReSharper restore InconsistentNaming

        private decimal? _minX, _maxX, _minY, _maxY, _minZ, _maxZ;

        [OracleObjectMappingAttribute(0)]
        public decimal? SdoGtype { get; set; }

        [OracleObjectMappingAttribute(1)]
        public decimal? Sdo_Srid { get; set; }

        [OracleObjectMappingAttribute(2)]
        public SdoPoint Point { get; set; }

        [OracleObjectMappingAttribute(3)]
        public decimal[] ElemArray { get; set; }

        [OracleObjectMappingAttribute(4)]
        public decimal[] OrdinatesArray { get; set; }

        [OracleCustomTypeMappingAttribute("MDSYS.SDO_ELEM_INFO_ARRAY")]
        public class ElemArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

        [OracleCustomTypeMappingAttribute("MDSYS.SDO_ORDINATE_ARRAY")]
        public class OrdinatesArrayFactory : OracleArrayTypeFactoryBase<decimal> { }

        public override void MapFromCustomObject()
        {
            SetValue((int)OracleObjectColumns.SDO_GTYPE, SdoGtype);
            SetValue((int)OracleObjectColumns.SDO_SRID, Sdo_Srid);
            SetValue((int)OracleObjectColumns.SDO_POINT, Point);
            SetValue((int)OracleObjectColumns.SDO_ELEM_INFO, ElemArray);
            SetValue((int)OracleObjectColumns.SDO_ORDINATES, OrdinatesArray);
        }

        public override void MapToCustomObject()
        {
            SdoGtype = GetValue<decimal?>((int)OracleObjectColumns.SDO_GTYPE);
            Sdo_Srid = GetValue<decimal?>((int)OracleObjectColumns.SDO_SRID);
            Point = GetValue<SdoPoint>((int)OracleObjectColumns.SDO_POINT);
            ElemArray = GetValue<decimal[]>((int)OracleObjectColumns.SDO_ELEM_INFO);
            OrdinatesArray = GetValue<decimal[]>((int)OracleObjectColumns.SDO_ORDINATES);
        }

        private void GetMinMax()
        {
            _minX = _minY = _minZ = null;
            _maxX = _maxY = _maxZ = null;
            Int32 dim = Math.Min(((Int32)SdoGtype.Value)/1000, 3);
            if (Point != null)
            {
                _minX = _maxX = Point.X;
                _minY = _maxY = Point.Y;
                if (dim > 2)_minZ = _maxZ = Point.Z;
            }

            if ( OrdinatesArray != null )
            {
                for (int i = 0; i < OrdinatesArray.Length; i+=dim )
                {
                    _minX = _minX.HasValue ? Math.Min(_minX.Value, OrdinatesArray[i]) : OrdinatesArray[i]; 
                    _minY = _minY.HasValue ? Math.Min(_minY.Value, OrdinatesArray[i+1]) : OrdinatesArray[i+1];
                    if ( dim > 2 ) _minZ = _minZ.HasValue ? Math.Min(_minZ.Value, OrdinatesArray[i+2]) : OrdinatesArray[i+2];
                    _maxX = _maxX.HasValue ? Math.Max(_maxX.Value, OrdinatesArray[i]) : OrdinatesArray[i];
                    _maxY = _maxY.HasValue ? Math.Max(_maxY.Value, OrdinatesArray[i+1]) : OrdinatesArray[i+1];
                    if ( dim > 2 )_maxZ = _maxZ.HasValue ? Math.Max(_maxZ.Value, OrdinatesArray[i+2]) : OrdinatesArray[i+2];
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