using System;
using Oracle.DataAccess.Types;

namespace NetTopologySuite.IO.UdtBase
{
    public abstract class OracleArrayTypeFactoryBase<T> : IOracleArrayTypeFactory
    {
        public Array CreateArray(int numElems)
        {
            return new T[numElems];
        }

        public Array CreateStatusArray(int numElems)
        {
            return null;
        }
    }
}