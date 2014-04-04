using System;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Handlers;

namespace NetTopologySuite.IO.ShapeFile.Extended.Entities
{
	[Serializable]
	internal class ShapefileFeature : IShapefileFeature
	{
	    private readonly Lazy<IGeometry> m_LazyGeometry;
		private readonly Lazy<IAttributesTable> m_LazyAttributeTable;

		private readonly ShapeReader m_ShapeReader;
		private readonly DbaseReader m_DbaseReader;
		private readonly ShapeLocationInFileInfo m_ShapeLocationInfo;

		private readonly IGeometryFactory m_GeoFactory;

	    public ShapefileFeature(ShapeReader shapeReader, DbaseReader dbfReader, ShapeLocationInFileInfo shapeLocation, IGeometryFactory geoFactory)            
		{
			m_ShapeReader = shapeReader;
			m_GeoFactory = geoFactory;
			m_ShapeLocationInfo = shapeLocation;
			m_LazyGeometry = new Lazy<IGeometry>(() => m_ShapeReader.ReadShapeAtOffset(m_ShapeLocationInfo.OffsetFromStartOfFile, m_GeoFactory), LazyThreadSafetyMode.ExecutionAndPublication);

			m_DbaseReader = dbfReader;
			m_LazyAttributeTable = new Lazy<IAttributesTable>(() => m_DbaseReader.ReadEntry(m_ShapeLocationInfo.ShapeIndex), LazyThreadSafetyMode.ExecutionAndPublication);
		}

	    public IGeometry Geometry
		{
			get
			{
				return m_LazyGeometry.Value;
			}
		}

		public IAttributesTable Attributes
		{
			get
			{
				return m_LazyAttributeTable.Value;
			}
		}

		public long FeatureId
		{
			get
			{
				return m_ShapeLocationInfo.ShapeIndex;
			}
		}
	}
}