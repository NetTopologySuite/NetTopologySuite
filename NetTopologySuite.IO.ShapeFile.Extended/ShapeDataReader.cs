using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.Handlers;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;

namespace NetTopologySuite.IO.ShapeFile.Extended
{
	public class ShapeDataReader : IDisposable
    {
	    private const string DBF_EXT = ".dbf";

	    private readonly ISpatialIndex<ShapeLocationInFileInfo> m_SpatialIndex;
		private readonly Task m_IndexCreationTask;
		private bool m_IsIndexingComplete;
        private readonly CancellationTokenSource m_CancellationTokenSrc;
		private readonly DbaseReader m_DbfReader;
		private readonly IGeometryFactory m_GeoFactory;
		private readonly ShapeReader m_ShapeReader;

	    public ShapeDataReader(string shapeFilePath, ISpatialIndex<ShapeLocationInFileInfo> index, IGeometryFactory geoFactory, bool buildIndexAsync)
		{
			m_SpatialIndex = index;
			m_GeoFactory = geoFactory;

			ValidateParameters(shapeFilePath);

			m_ShapeReader = new ShapeReader(shapeFilePath);

			if (buildIndexAsync)
			{
                m_CancellationTokenSrc = new CancellationTokenSource();
                m_IndexCreationTask = Task.Factory.StartNew(FillSpatialIndex, m_CancellationTokenSrc.Token);
			}
			else
			{
				FillSpatialIndex();
			}

			m_DbfReader = new DbaseReader(Path.ChangeExtension(shapeFilePath, DBF_EXT));			
		}

		public ShapeDataReader(string shapeFilePath, ISpatialIndex<ShapeLocationInFileInfo> index, IGeometryFactory geoFactory)
			: this(shapeFilePath, index, geoFactory, true)
		{ }

		public ShapeDataReader(string shapeFilePath, ISpatialIndex<ShapeLocationInFileInfo> index)
			: this(shapeFilePath, index, new GeometryFactory())
		{ }

		public ShapeDataReader(string shapeFilePath)
			: this(shapeFilePath, new STRtree<ShapeLocationInFileInfo>())
		{ }

		~ShapeDataReader()
		{
			Dispose(false);
		}

	    public Envelope ShapefileBounds
		{
			get
			{
				return m_ShapeReader.ShapefileHeader.Bounds;
			}
		}

	    public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Query shapefile by MBR.
		/// MBR coordinates MUST be in the Shapefile's coordinate system.
		/// 
		/// NOTE: If you are using the default ISpatialIndex (which is an instance of the STRtree NTS class), it has some limitations.
		/// Since it works with MBRs rather than the shapes themselves, you can get some shapes that are not actually in the MBR
		/// you provided just because their MBRs are bounded by the given envelope.
		/// If you wish to avoid this behaviour, send true in the second paramter, but be weary of the consequences listed below.
		/// </summary>
		/// <param name="envelope"> The envlope to query. </param>
		/// <param name="testGeometriesActuallyInMBR"> 
		/// False by default, true to double-check the returned geometries against given Envelope, to avoid index error margin.
		/// 
		/// It is advisable that you implement your own ISpatialIndex with your required precision rather than set this to True.
		/// 
		/// **********
		/// CAUTION: If you choose to set this parameter as True, it will greatly affect performance as it
		/// will cancel any lazy mechanism implemented with reading the geometries from the file.
		/// Do not set this to True unless you either:
		/// A. Do not have any performance restrictions.
		/// Or:
		/// B. Absolutely need that precision in the geographic query.
		/// **********
		/// </param>
		/// <returns></returns>
        public IEnumerable<IShapefileFeature> ReadByMBRFilter(Envelope envelope, bool testGeometriesActuallyInMBR = false)
		{
			if (envelope == null)
			{
				throw new ArgumentNullException("envelope");
			}

			// If index creation task wasnt completed, wait for it to complete.
			if (!m_IsIndexingComplete)
			{
				m_IndexCreationTask.Wait();
			}

			IList<ShapeLocationInFileInfo> shapesInRegion = m_SpatialIndex.Query(envelope);

			if (shapesInRegion.Count == 0)
			{
                return Enumerable.Empty<IShapefileFeature>();
			}

            IEnumerable<IShapefileFeature> results = shapesInRegion.Select(ReadFeature);

			if (!testGeometriesActuallyInMBR)
			{
				return results;
			}
			else
			{
				IGeometry envelopeGeo = new GeometryFactory().ToGeometry(envelope);

				return results.Where(feature =>
					{
						return envelopeGeo.Intersects(feature.Geometry);
					});
			}
		}

		private IShapefileFeature ReadFeature(ShapeLocationInFileInfo shapeLocationInfo)
		{
			return new ShapefileFeature(m_ShapeReader, m_DbfReader, shapeLocationInfo, m_GeoFactory);
		}

		/// <summary>
		/// Check validity of parameters - null values and that all file needed to read shapes exist.
		/// </summary>
		private void ValidateParameters(string shpFilePath)
		{
			if (m_SpatialIndex == null)
			{
				throw new ArgumentNullException("index");
			}

			if (string.IsNullOrWhiteSpace(shpFilePath))
			{
				throw new ArgumentNullException("ShapeFilePath");
			}

			if (m_GeoFactory == null)
			{
				throw new ArgumentNullException("GeoFactory");
			}

			if (!File.Exists(shpFilePath))
			{
				throw new FileNotFoundException("Shape file needed for shape reader", shpFilePath);
			}

			string dbfFile = Path.ChangeExtension(shpFilePath, DBF_EXT);
			if (!File.Exists(dbfFile))
			{
				throw new FileNotFoundException("DBF file needed for shape reader", dbfFile);
			}
		}

		private void FillSpatialIndex()
		{
            bool isAsync = m_CancellationTokenSrc != null;

			foreach (MBRInfo mbrInfo in m_ShapeReader.ReadMBRs())
			{
                if (isAsync && m_CancellationTokenSrc.IsCancellationRequested)
                {
                    break;
                }

				m_SpatialIndex.Insert(mbrInfo.ShapeMBR, mbrInfo.ShapeFileDetails);
			}

			m_IsIndexingComplete = true;
		}

		private void Dispose(bool disposing)
		{
            if (m_CancellationTokenSrc != null)
            {
                m_CancellationTokenSrc.Cancel();
            }

			if (m_DbfReader != null)
			{
				m_DbfReader.Dispose();
			}

			if (m_ShapeReader != null)
			{
				m_ShapeReader.Dispose();
			}
		}
    }
}
