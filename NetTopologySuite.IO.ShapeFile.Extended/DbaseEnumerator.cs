using System;
using System.Collections;
using System.Collections.Generic;
using NetTopologySuite.Features;

namespace NetTopologySuite.IO.ShapeFile.Extended
{
	internal class DbaseEnumerator : IEnumerator<IAttributesTable>, IDisposable
	{
		private DbaseReader m_Reader;
		private int m_CurrentAttrTbleIndex;

		public DbaseEnumerator(DbaseReader reader)
		{
			m_Reader = reader.Clone();
			m_CurrentAttrTbleIndex = 0;
		}

		public IAttributesTable Current
		{
			get;
			private set;
		}

		public void Dispose()
		{
			m_Reader.Dispose();
		}

		object IEnumerator.Current
		{
			get { return ((IEnumerator<IAttributesTable>)this).Current; }
		}

		public bool MoveNext()
		{
			if (m_CurrentAttrTbleIndex == m_Reader.NumOfRecords)
			{
				return false;
			}

			Current = m_Reader.ReadEntry(m_CurrentAttrTbleIndex);
			m_CurrentAttrTbleIndex++;

			return true;
		}

		public void Reset()
		{
			DbaseReader newReader = m_Reader.Clone();
			m_Reader.Dispose();
			m_Reader = newReader;
			m_CurrentAttrTbleIndex = 0;
		}
	}
}
