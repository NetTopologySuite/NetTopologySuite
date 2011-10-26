using System;
using Oracle.DataAccess.Types;
using Oracle.DataAccess.Client;

namespace NetTopologySuite.IO.UdtBase
{
    public abstract class OracleCustomTypeBase<T> : INullable, IOracleCustomType, IOracleCustomTypeFactory
        where T : OracleCustomTypeBase<T>, new()
    {
        private static readonly string ErrorMessageHead = "Error converting Oracle User Defined Type to .Net Type " +
                                                          typeof (T) +
                                                          ", oracle column is null, failed to map to . NET valuetype, column ";

        private OracleConnection _connection;
        private IntPtr _pUdt;

        private bool _isNull;

        public virtual bool IsNull
        {
            get { return _isNull; }
        }

        public static T Null
        {
            get
            {
                T t = new T {_isNull = true};
                return t;
            }
        }

        public IOracleCustomType CreateObject()
        {
            return new T();
        }

        protected void SetConnectionAndPointer(OracleConnection connection, IntPtr pUdt)
        {
            _connection = connection;
            _pUdt = pUdt;
        }

        public abstract void MapFromCustomObject();
        public abstract void MapToCustomObject();

        public void FromCustomObject(OracleConnection con, IntPtr pUdt)
        {
            SetConnectionAndPointer(con, pUdt);
            MapFromCustomObject();
        }

        public void ToCustomObject(OracleConnection con, IntPtr pUdt)
        {
            SetConnectionAndPointer(con, pUdt);
            MapToCustomObject();
        }

        protected void SetValue(string oracleColumnName, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(_connection, _pUdt, oracleColumnName, value);
            }
        }

        protected void SetValue(int oracleColumnId, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(_connection, _pUdt, oracleColumnId, value);
            }
        }

        protected TUser GetValue<TUser>(string oracleColumnName)
        {

            if (OracleUdt.IsDBNull(_connection, _pUdt, oracleColumnName))
            {
                if (default(TUser) is ValueType)
                {
                    throw new Exception(ErrorMessageHead + oracleColumnName + " of value type " +
                                        typeof(TUser));
                }
                return default(TUser);
            }
            return (TUser)OracleUdt.GetValue(_connection, _pUdt, oracleColumnName);
        }

        protected TUser GetValue<TUser>(int oracleColumnId)
        {
            if (OracleUdt.IsDBNull(_connection, _pUdt, oracleColumnId))
            {
                if (default(TUser) is ValueType)
                {
                    throw new Exception(ErrorMessageHead + oracleColumnId.ToString() + " of value type " +
                                        typeof(TUser));
                }
                return default(TUser);
            }
            return (TUser)OracleUdt.GetValue(_connection, _pUdt, oracleColumnId);
        }
    }
}
