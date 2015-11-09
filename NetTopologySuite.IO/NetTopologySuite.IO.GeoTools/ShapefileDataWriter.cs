using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Streams;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// A simple test class for write a complete (shp, shx and dbf) shapefile structure.
    /// </summary>
    public class ShapefileDataWriter
    {

        #region Static

        /// <summary>
        /// Gets the stub header.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static DbaseFileHeader GetHeader(IFeature feature, int count)
        {
            IAttributesTable attribs = feature.Attributes;
            string[] names = attribs.GetNames();
            DbaseFileHeader header = new DbaseFileHeader();
            header.NumRecords = count;
            foreach (string name in names)
            {
                Type type = attribs.GetType(name);
                if (type == typeof(double) || type == typeof(float))
                    header.AddColumn(name, 'N', DoubleLength, DoubleDecimals);
                else if (type == typeof(short) || type == typeof(ushort) ||
                         type == typeof(int) || type == typeof(uint))
                    header.AddColumn(name, 'N', IntLength, IntDecimals);
                else if (type == typeof(long) || type == typeof(ulong))
                    header.AddColumn(name, 'N', LongLength, IntDecimals);
                else if (type == typeof(string))
                    header.AddColumn(name, 'C', StringLength, StringDecimals);
                else if (type == typeof(bool))
                    header.AddColumn(name, 'L', BoolLength, BoolDecimals);
                else if (type == typeof(DateTime))
                    header.AddColumn(name, 'D', DateLength, DateDecimals);
                else throw new ArgumentException("Type " + type.Name + " not supported");
            }
            return header;
        }

        /// <summary>
        /// Gets the header from a dbf file.
        /// </summary>
        /// <param name="dbfFile">The DBF file.</param>
        /// <returns></returns>
        public static DbaseFileHeader GetHeader(string dbfFile)
        {
            return GetHeader(new ShapefileStreamProviderRegistry(dbfFile, false, true, false));
        }

        public static DbaseFileHeader GetHeader(IStreamProviderRegistry streamProviderRegistry)
        {

            DbaseFileHeader header = new DbaseFileHeader();

            using (var stream = streamProviderRegistry[StreamTypes.Data].OpenRead())
            using (var reader = new BinaryReader(stream))
                header.ReadHeader(reader, streamProviderRegistry[StreamTypes.Data] is FileStreamProvider ? ((FileStreamProvider)streamProviderRegistry[StreamTypes.Data]).Path : null);
            return header;
        }

        public static DbaseFileHeader GetHeader(DbaseFieldDescriptor[] dbFields, int count)
        {
            DbaseFileHeader header = new DbaseFileHeader();
            header.NumRecords = count;

            foreach (DbaseFieldDescriptor dbField in dbFields)
                header.AddColumn(dbField.Name, dbField.DbaseType, dbField.Length, dbField.DecimalCount);

            return header;
        }

        #endregion

        private const int DoubleLength = 18;
        private const int DoubleDecimals = 8;
        private const int IntLength = 10;
        private const int LongLength = 18;
        private const int IntDecimals = 0;
        private const int StringLength = 254;
        private const int StringDecimals = 0;
        private const int BoolLength = 1;
        private const int BoolDecimals = 0;
        private const int DateLength = 8;
        private const int DateDecimals = 0;

        //private readonly string _shpFile = String.Empty;
        //private readonly string _dbfFile = String.Empty;

        private IStreamProviderRegistry _streamProviderRegistry;

        private readonly DbaseFileWriter _dbaseWriter;

        private DbaseFileHeader _header;

        /// <summary>
        /// Gets or sets the header of the shapefile.
        /// </summary>
        /// <value>The header.</value>
        public DbaseFileHeader Header
        {
            get { return _header; }
            set { _header = value; }
        }

        private IGeometryFactory _geometryFactory;

        /// <summary>
        /// Gets or sets the geometry factory.
        /// </summary>
        /// <value>The geometry factory.</value>
        protected IGeometryFactory GeometryFactory
        {
            get { return _geometryFactory; }
            set { _geometryFactory = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapefileDataWriter"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file with or without any extension.</param>
        public ShapefileDataWriter(string fileName) : this(fileName, Geometries.GeometryFactory.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapefileDataWriter"/> class.
        /// </summary>
        /// <param name="fileName">File path without any extension</param>
        /// <param name="geometryFactory"></param>
        public ShapefileDataWriter(string fileName, IGeometryFactory geometryFactory)
            : this(fileName, geometryFactory, Encoding.GetEncoding(1252))
        {

        }

        public ShapefileDataWriter(string fileName, IGeometryFactory geometryFactory, Encoding encoding)
            : this(new ShapefileStreamProviderRegistry(fileName, false, false, false), geometryFactory, encoding)
        {

        }

        public ShapefileDataWriter(IStreamProviderRegistry streamProviderRegistry, IGeometryFactory geometryFactory, Encoding encoding)
        {
            _geometryFactory = geometryFactory;

            _streamProviderRegistry = streamProviderRegistry;

            // Writers
            _dbaseWriter = new DbaseFileWriter(streamProviderRegistry, encoding);
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        public void Write(IList<IFeature> featureCollection)
        {
            // Test if the Header is initialized
            if (Header == null)
                throw new ApplicationException("Header must be set first!");

#if DEBUG
            // Test if all elements of the collections are features
            foreach (object obj in featureCollection)
                if (obj.GetType().IsAssignableFrom(typeof(IFeature)))
                    throw new ArgumentException("All the elements in the given collection must be " + typeof(IFeature).Name);
#endif

            try
            {
                // Write shp and shx  
                var geometries = new IGeometry[featureCollection.Count];
                var index = 0;
                foreach (IFeature feature in featureCollection)
                    geometries[index++] = feature.Geometry;
                ShapefileWriter.WriteGeometryCollection(_streamProviderRegistry, new GeometryCollection(geometries, _geometryFactory));

                // Write dbf
                _dbaseWriter.Write(Header);
                foreach (IFeature feature in featureCollection)
                {
                    var attribs = feature.Attributes;
                    ArrayList values = new ArrayList();
                    for (int i = 0; i < Header.NumFields; i++)
                        values.Add(attribs[Header.Fields[i].Name]);
                    _dbaseWriter.Write(values);
                }
            }
            finally
            {
                // Close dbf writer
                _dbaseWriter.Close();
            }
        }
    }
}
