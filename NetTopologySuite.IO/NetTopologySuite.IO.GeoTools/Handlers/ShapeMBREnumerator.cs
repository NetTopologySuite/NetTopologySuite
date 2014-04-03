using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.IO.Handlers
{
    internal class ShapeMBREnumerator : IEnumerable<MBRInfo>
    {
        private readonly BigEndianBinaryReader m_Reader;

        public ShapeMBREnumerator(BigEndianBinaryReader reader)
        {
            m_Reader = reader;
        }

        public IEnumerator<MBRInfo> GetEnumerator()
        {
            return new ShapeMBRIterator(m_Reader);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<MBRInfo>)this).GetEnumerator();
        }
    }
}
