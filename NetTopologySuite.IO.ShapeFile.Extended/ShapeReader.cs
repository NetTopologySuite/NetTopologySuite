using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.IO.Handlers;
using NetTopologySuite.IO.ShapeFile.Extended.Streams;

namespace NetTopologySuite.IO.ShapeFile.Extended
{
    public class ShapeReader : IDisposable
    {
        const long HEADER_LENGTH = 100;

        private BigEndianBinaryReader m_ShapeFileReader;
        private readonly ShapefileHeader m_ShapeFileHeader;
        //private readonly string m_ShapeFilePath;
        private readonly IShapeStreamProvider m_shapeStreamProvider;
        private readonly ShapeHandler m_ShapeHandler;
        private readonly Lazy<long[]> m_ShapeOffsetCache;
        private bool m_IsDisposed;

        public ShapeReader(string shapeFilePath) : this(new ShapefileStreamProvider(shapeFilePath, true))
        {

        }

        public ShapeReader(IShapeStreamProvider shapeStreamProvider)
        {
            if (shapeStreamProvider == null)
                throw new ArgumentNullException(nameof(shapeStreamProvider));

            m_shapeStreamProvider = shapeStreamProvider;

            m_ShapeFileHeader = new ShapefileHeader(ShapeReaderStream);
            m_ShapeHandler = Shapefile.GetShapeHandler(ShapefileHeader.ShapeType);

            m_ShapeOffsetCache = new Lazy<long[]>(BuildOffsetCache, LazyThreadSafetyMode.ExecutionAndPublication);

        }

        ~ShapeReader()
        {
            Dispose(false);
        }

        public ShapefileHeader ShapefileHeader
        {
            get
            {
                return m_ShapeFileHeader;
            }
        }

        private BigEndianBinaryReader ShapeReaderStream
        {
            get
            {
                if (m_ShapeFileReader == null)
                {
                    lock (m_shapeStreamProvider)
                    {
                        if (m_ShapeFileReader == null)
                        {
                            m_ShapeFileReader = new BigEndianBinaryReader(m_shapeStreamProvider.ShapeStream.OpenRead());
                        }
                    }
                }

                return m_ShapeFileReader;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<MBRInfo> ReadMBRs()
        {
            ThrowIfDisposed();

            BigEndianBinaryReader NewReader = new BigEndianBinaryReader(m_shapeStreamProvider.ShapeStream.OpenRead());
            return m_ShapeHandler.ReadMBRs(NewReader);
        }

        public IEnumerable<IGeometry> ReadAllShapes(IGeometryFactory geoFactory)
        {
            ThrowIfDisposed();

            if (geoFactory == null)
            {
                throw new ArgumentNullException("geoFactory");
            }

            return m_ShapeOffsetCache.Value.Select(offset => ReadShapeAtOffset(offset, geoFactory));
        }

        public IGeometry ReadShapeAtIndex(int index, IGeometryFactory geoFactory)
        {
            ThrowIfDisposed();

            if (geoFactory == null)
            {
                throw new ArgumentNullException("geoFactory");
            }

            if (index < 0 || index >= m_ShapeOffsetCache.Value.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return ReadShapeAtOffset(m_ShapeOffsetCache.Value[index], geoFactory);
        }

        /// <summary>
        /// Read shape at a given offset.
        /// </summary>
        /// <param name="shapeOffset"> The offset at which the requested shape metadata begins.</param>
        /// <param name="geoFactory"></param>
        /// <returns></returns>
        public IGeometry ReadShapeAtOffset(long shapeOffset, IGeometryFactory geoFactory)
        {
            IGeometry currGeomtry = null;
            ThrowIfDisposed();

            if (shapeOffset < HEADER_LENGTH || shapeOffset >= ShapeReaderStream.BaseStream.Length)
            {
                throw new IndexOutOfRangeException("Shape offset cannot be lower than header length (100) or higher than shape file size");
            }

            lock (ShapeReaderStream)
            {
                // Skip to shape size location in file.
                ShapeReaderStream.BaseStream.Seek(shapeOffset + 4, SeekOrigin.Begin);

                int currShapeLengthInWords = ShapeReaderStream.ReadInt32BE();

                currGeomtry = m_ShapeHandler.Read(ShapeReaderStream, currShapeLengthInWords, geoFactory);
            }

            return currGeomtry;
        }

        private long[] BuildOffsetCache()
        {
            using (BigEndianBinaryReader shapeFileReader = new BigEndianBinaryReader(m_shapeStreamProvider.ShapeStream.OpenRead()))
            {
                return m_ShapeHandler.ReadMBRs(shapeFileReader)
                                     .Select(mbrInfo => mbrInfo.ShapeFileDetails.OffsetFromStartOfFile)
                                     .ToArray();
            }

        }

        private void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new InvalidOperationException("Cannot use a disposed ShapeReader");
            }
        }

        private void CloseShapeFileHandle()
        {
            if (m_ShapeFileReader != null)
            {
                m_ShapeFileReader.Close();
                m_ShapeFileReader = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (m_IsDisposed)
            {
                return;
            }

            m_IsDisposed = true;
            CloseShapeFileHandle();
        }
    }
}
