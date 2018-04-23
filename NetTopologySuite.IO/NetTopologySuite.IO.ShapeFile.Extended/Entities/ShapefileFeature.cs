﻿using System;
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
		    get => m_LazyGeometry.Value;
		    set => throw new NotSupportedException("Setting geometry on a shapefile reader is not supported!");
	    }

	    public Envelope BoundingBox
	    {
		    get => Geometry.EnvelopeInternal;
		    set => throw new InvalidOperationException("Setting BoundingBox not allowed for Shapefile feature");
	    }

	    public IAttributesTable Attributes
		{
		    get => m_LazyAttributeTable.Value;
		    set => throw new NotSupportedException("Setting attributes on a shapefile reader is not supported!");
	    }

		public long FeatureId => m_ShapeLocationInfo.ShapeIndex;
	}
}