using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.IO.Handlers
{
    internal class PointMBREnumerator : IEnumerable<MBRInfo>
    {
        private readonly BigEndianBinaryReader m_Reader;

        public PointMBREnumerator(BigEndianBinaryReader reader)
        {
            m_Reader = reader;
        }

        public IEnumerator<MBRInfo> GetEnumerator()
        {
            return new PointMBRIterator(m_Reader);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<MBRInfo>)this).GetEnumerator();
        }
    }
}
