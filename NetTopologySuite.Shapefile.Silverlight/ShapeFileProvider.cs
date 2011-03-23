// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
// Portions copyright 2008 - 2009: John Diss (www.newgrove.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Data;
using GisSharpBlog.NetTopologySuite.Extension;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Encodings;
using Trace = System.Diagnostics.Debug;
using ByteEncoder = GeoAPI.DataStructures.ByteEncoder;
//using Pair<T> = System.Tuple<T,T>;
using GisSharpBlog.NetTopologySuite.Shapefile;
using System.Linq;
using GisSharpBlog.NetTopologySuite.Utilities;
namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    public struct FilePermissions
    {
        public FileMode FileMode { get; set; }

        public FileAccess FileAccess { get; set; }

        public FileShare FileShare { get; set; }
    }

    public enum WriteAccess
    {
        ReadOnly,
        ReadWrite,
        Exclusive
    }
    public enum ForceCoordinateOptions
    {
        ForceNone = 0,
        Force2D,
        Force2DM,
        Force3D,
        Force3DM
    }

    public enum ShapeFileReadStrictness
    {
        Strict,
        Lenient
    }

    /// <summary>
    /// A data provider for the ESRI ShapeFile spatial data format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ShapeFile provider is used for accessing ESRI ShapeFiles. 
    /// The ShapeFile should at least contain the [filename].shp 
    /// and the [filename].shx index file. 
    /// If feature-data is to be used a [filename].dbf file should 
    /// also be present.
    /// </para>
    /// <para>
    /// M and Z values in a shapefile are currently ignored by SharpMap.
    /// </para>
    /// </remarks>
    /// <example>
    /// Adding a data source to a layer:
    /// <code lang="C#">
    /// using SharpMap.Layers;
    /// using SharpMap.Data.Providers.ShapeFile;
    /// // [...]
    /// FeatureLayer myLayer = new FeatureLayer("My layer");
    /// myLayer.DataSource = new ShapeFileProvider(@"C:\data\MyShapeData.shp");
    /// </code>
    /// </example>
    public class ShapeFileProvider : FeatureProviderBase //, IWritableFeatureProvider<UInt32>
    {
        public const Double NullDoubleValue = ShapeFileConstants.NullDoubleValue;
        private const String SharpMapShapeFileIndexFileExtension = ".#index-shp";
        private readonly IGeometryFactory _geometryFactory;

        public static bool IsShapefileNullValue(double d)
        {
            return d <= -10E38;
        }

        #region IdBounds

        private struct IdBounds //: IBoundable<IExtents>
        {
            private readonly UInt32 _id;
            private readonly Object _record;

            public IdBounds(UInt32 id, IRecord feature)
            {
                _id = id;
                _record = feature;
            }

            public IdBounds(UInt32 id, IEnvelope extents)
            {
                _id = id;
                _record = extents;
            }

            public UInt32 Id
            {
                get { return _id; }
            }

            public IRecord Feature
            {
                get { return _record as IRecord; }
            }

            #region IBoundable<IExtents> Members

            public IEnvelope Bounds
            {
                get { return _record as IEnvelope ?? Feature.GetValue<IGeometry>("Geom").EnvelopeInternal; }
            }

            public Boolean Intersects(IEnvelope bounds)
            {
                if (bounds == null) throw new ArgumentNullException("bounds");

                return bounds.Intersects(Bounds);
            }

            #endregion
        }

        #endregion

        #region Instance fields

        //private readonly ICoordinateFactory _coordFactory;
        private readonly String _filename;
        private readonly Boolean _hasDbf;
        private readonly Boolean _hasFileBasedSpatialIndex;
        private readonly ShapeFileHeader _header;
        private readonly ShapeFileIndex _shapeFileIndex;
        private DbaseFile _dbaseFile;
        private Predicate<IRecord> _filterDelegate;
        private ForceCoordinateOptions _forceCoordinateOptions = ForceCoordinateOptions.ForceNone;
        private Boolean _isIndexed = true;
        private ShapeFileReadStrictness _readStrictness = ShapeFileReadStrictness.Strict;
        private BinaryReader _shapeFileReader;
        private Stream _shapeFileStream;
        private BinaryWriter _shapeFileWriter;
        private ISpatialIndex<IEnvelope, IdBounds> _spatialIndex;

        #endregion

        #region Object construction and disposal

        /// <summary>
        /// Initializes a shapefile data provider.
        /// </summary>
        /// <param name="filename">Path to shapefile (.shp file).</param>
        /// <param name="geoFactory">The geometry factory to use to create geometries.</param>
        /// <remarks>
        /// This constructor creates a <see cref="ShapeFileProvider"/>
        /// with an in-memory spatial index.
        /// </remarks>
        public ShapeFileProvider(String filename, IGeometryFactory geoFactory, IStorageManager storageManager, ISchemaFactory schemaFactory)
            : this(filename, geoFactory, false, storageManager, schemaFactory)
        {
        }

        ///// <summary>
        ///// Initializes a shapefile data provider.
        ///// </summary>
        ///// <param name="filename">Path to shapefile (.shp file).</param>
        ///// <param name="geoFactory">The geometry factory to use to create geometries.</param>
        ///// <param name="coordSysFactory">
        ///// The coordinate system factory to use to create spatial reference system objects.
        ///// </param>
        ///// <remarks>
        ///// This constructor creates a <see cref="ShapeFileProvider"/>
        ///// with an in-memory spatial index.
        ///// </remarks>
        //public ShapeFileProvider(String filename,
        //                         IGeometryFactory geoFactory,
        //                          IStorageManager storageManager, ISchemaFactory schemaFactory)
        //    : this(filename, geoFactory,  false, storageManager, schemaFactory)
        //{
        //}

        /// <summary>
        /// Initializes a ShapeFile data provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <paramref name="fileBasedIndex"/> is true, the spatial index 
        /// will be read from a local copy. If it doesn't exist,
        /// it will be generated and saved to [filename] + '.sidx'.
        /// </para>
        /// </remarks>
        /// <param name="filename">Path to shapefile (.shp file).</param>
        /// <param name="geoFactory">The geometry factory to use to create geometries.</param>
        /// <param name="coordSysFactory">
        /// The coordinate system factory to use to create spatial reference system objects.
        /// </param>
        /// <param name="fileBasedIndex">True to create a file-based spatial index.</param>
        /// <param name="writeAccess">Specify the kind of access when managing files.</param>
        public ShapeFileProvider(String filename,
                                 IGeometryFactory geoFactory,
                                 Boolean fileBasedIndex, IStorageManager storageManager, ISchemaFactory schemaFactory)
        {
            _filename = filename;
            StorageManager = storageManager;
            SchemaFactory = schemaFactory;
            _geometryFactory = geoFactory;
            //IGeometryFactory geoFactoryClone = base.GeometryFactory = geoFactory.Clone();
            //OriginalSpatialReference = geoFactoryClone.SpatialReference;
            //OriginalSrid = geoFactoryClone.Srid;
            //_coordFactory = geoFactoryClone.CoordinateFactory;

            if (!StorageManager.FileExists(filename))
            {
                throw new ShapeFileException(filename);
            }

            using (BinaryReader reader = new BinaryReader(StorageManager.OpenRead(filename)))
            {
                _header = new ShapeFileHeader(reader, geoFactory);
            }

            _shapeFileIndex = new ShapeFileIndex(this);

            _hasFileBasedSpatialIndex = fileBasedIndex;

            _hasDbf = StorageManager.FileExists(DbfFilename);

            // Initialize DBF
            if (HasDbf)
            {
                _dbaseFile = new DbaseFile(DbfFilename, geoFactory, StorageManager, schemaFactory);
            }
        }

        public ISchemaFactory SchemaFactory
        {
            get;
            protected set;
        }

        public IStorageManager StorageManager
        {
            get;
            protected set;
        }

        #region Dispose pattern

        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (_dbaseFile != null)
                {
                    _dbaseFile.Close();
                    _dbaseFile = null;
                }

                if (_shapeFileReader != null)
                {
                    _shapeFileReader.Close();
                    _shapeFileReader = null;
                }

                if (_shapeFileWriter != null)
                {
                    _shapeFileWriter.Close();
                    _shapeFileWriter = null;
                }

                if (_shapeFileStream != null)
                {
                    _shapeFileStream.Close();
                    _shapeFileStream = null;
                }

                if (_spatialIndex != null)
                {
                    _spatialIndex.Dispose();
                    _spatialIndex = null;
                }

                if (StorageManager != null)
                {
                    StorageManager.Dispose();
                    StorageManager = null;
                }
            }

        }

        #endregion

        #endregion

        #region ToString

        /// <summary>
        /// Provides a String representation of the essential ShapeFile info.
        /// </summary>
        /// <returns>A String with the Name, HasDbf, FeatureCount and Extents values.</returns>
        public override String ToString()
        {
            return String.Format("Name: {0}; HasDbf: {1}; " +
                                 "Features: {2}; Extents: {3}",
                                 ConnectionId, HasDbf,
                                 GetFeatureCount(), GetExtents());
        }

        #endregion

        #region Public Methods and Properties (SharpMap ShapeFile API)

        #region Create static methods

        ///// <summary>
        ///// Creates a new <see cref="ShapeFile"/> instance and .shp and .shx file on disk.
        ///// </summary>
        ///// <param name="directory">Directory to create the shapefile in.</param>
        ///// <param name="layerName">Name of the shapefile.</param>
        ///// <param name="type">Type of shape to store in the shapefile.</param>
        ///// <returns>A ShapeFile instance.</returns>
        //public static ShapeFileProvider Create(String directory, String layerName,
        //                                       ShapeType type, IGeometryFactory geoFactory, IStorageManager storageManager)
        //{
        //    return Create(directory, layerName, type, null, geoFactory, storageManager);
        //}

        ///// <summary>
        ///// Creates a new <see cref="ShapeFile"/> instance and .shp, .shx and, optionally, .dbf file on disk.
        ///// </summary>
        ///// <remarks>If <paramref name="schema"/> is null, no .dbf file is created.</remarks>
        ///// <param name="directory">Directory to create the shapefile in.</param>
        ///// <param name="layerName">Name of the shapefile.</param>
        ///// <param name="type">Type of shape to store in the shapefile.</param>
        ///// <param name="schema">The schema for the attributes DBase file.</param>
        ///// <returns>A ShapeFile instance.</returns>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// 
        ///// Thrown if <paramref name="type"/> is <see cref="Providers.ShapeFile.ShapeType.Null"/>.
        ///// </exception>
        ///// <exception cref="ArgumentException">
        ///// Thrown if <paramref name="directory"/> is not a valid path.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="layerName"/> is null.
        ///// </exception>
        ///// <exception cref="ArgumentException">
        ///// Thrown if <paramref name="layerName"/> has invalid path characters.
        ///// </exception>
        //public static ShapeFileProvider Create(String directory, String layerName,
        //                                       ShapeType type, ISchema schema,
        //                                       IGeometryFactory geoFactory,
        //                                       ICoordinateSystemFactory coordinateSystemFactory, IStorageManager storageManager)
        //{
        //    if (type == ShapeType.Null)
        //    {
        //        throw new ShapeFileInvalidOperationException(
        //            "Cannot create a shapefile with a null geometry type");
        //    }

        //    if (String.IsNullOrEmpty(directory) || directory.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        //    {
        //        throw new ArgumentException("Parameter must be a valid path", "directory");
        //    }

        //    DirectoryInfo directoryInfo = new DirectoryInfo(directory);

        //    return Create(directoryInfo, layerName, type, schema, geoFactory, coordinateSystemFactory, storageManager);
        //}

        //public static ShapeFileProvider Create(String directory, String layerName,
        //                                       ShapeType type, ISchema schema,
        //                                       IGeometryFactory geoFactory, IStorageManager storageManager)
        //{
        //    return Create(directory, layerName, type, schema, geoFactory, null, storageManager);
        //}


        ///// <summary>
        ///// Creates a new <see cref="ShapeFile"/> instance and .shp, .shx and, optionally, 
        ///// .dbf file on disk.
        ///// </summary>
        ///// <remarks>If <paramref name="model"/> is null, no .dbf file is created.</remarks>
        ///// <param name="directory">Directory to create the shapefile in.</param>
        ///// <param name="layerName">Name of the shapefile.</param>
        ///// <param name="type">Type of shape to store in the shapefile.</param>
        ///// <param name="model">The schema for the attributes DBase file.</param>
        ///// <returns>A ShapeFile instance.</returns>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="layerName"/> is null.
        ///// </exception>
        ///// <exception cref="ArgumentException">
        ///// Thrown if <paramref name="layerName"/> has invalid path characters.
        ///// </exception>
        //public static ShapeFileProvider Create(DirectoryInfo directory, String layerName,
        //                                       ShapeType type, ISchema model,
        //                                       IGeometryFactory geoFactory,
        //                                       ICoordinateSystemFactory coordinateSystemFactory, IStorageManager storageManager)
        //{
        //    CultureInfo culture = Thread.CurrentThread.CurrentCulture;
        //    Encoding encoding = Encoding.GetEncoding(culture.TextInfo.OEMCodePage);
        //    return Create(directory, layerName, type, model, culture, encoding, geoFactory, coordinateSystemFactory, storageManager, model.SchemaFactory);
        //}


        //public static ShapeFileProvider Create(DirectoryInfo directory, String layerName,
        //                                       ShapeType type, ISchema model,
        //                                       IGeometryFactory geoFactory, IStorageManager storageManager, ISchemaFactory schemaFactory)
        //{
        //    return Create(directory, layerName, type, model, geoFactory, null, storageManager);
        //}

        /// <summary>
        /// Creates a new <see cref="GisSharpBlog.NetTopologySuite.Shapefile"/> instance and .shp, .shx and, optionally, 
        /// .dbf file on disk.
        /// </summary>
        /// <remarks>If <paramref name="model"/> is null, no .dbf file is created.</remarks>
        /// <param name="directory">Directory to create the shapefile in.</param>
        /// <param name="layerName">Name of the shapefile.</param>
        /// <param name="type">Type of shape to store in the shapefile.</param>
        /// <param name="model">The schema for the attributes DBase file.</param>
        /// <param name="culture">
        /// The culture info to use to determine default encoding and attribute formatting.
        /// </param>
        /// <param name="encoding">
        /// The encoding to use if different from the <paramref name="culture"/>'s default encoding.
        /// </param>
        /// <returns>A ShapeFile instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="layerName"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="layerName"/> has invalid path characters.
        /// </exception>
        public static ShapeFileProvider Create(DirectoryInfo directory, String layerName,
                                               ShapeType type, ISchema model,
                                               CultureInfo culture, Encoding encoding,
                                               IGeometryFactory geoFactory,
                                               ICoordinateSystemFactory coordinateSystemFactory, IStorageManager storageManager, ISchemaFactory schemaFactory)
        {
            if (String.IsNullOrEmpty(layerName))
            {
                throw new ArgumentNullException("layerName");
            }

            if (layerName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("Parameter cannot have invalid filename characters", "layerName");
            }

            if (!String.IsNullOrEmpty(Path.GetExtension(layerName)))
            {
                layerName = Path.GetFileNameWithoutExtension(layerName);
            }

            ISchema schemaTable = null;

            if (model != null)
            {
                schemaTable = DbaseSchema.DeriveSchemaTable(model);
            }

            String shapeFile = Path.Combine(directory.FullName, layerName + ".shp");

            using (MemoryStream buffer = new MemoryStream(100))
            {
                using (BinaryWriter writer = new BinaryWriter(buffer))
                {
                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write(ByteEncoder.GetBigEndian(ShapeFileConstants.HeaderStartCode));
                    writer.Write(new Byte[20]);
                    writer.Write(ByteEncoder.GetBigEndian(ShapeFileConstants.HeaderSizeBytes / 2));
                    writer.Write(ByteEncoder.GetLittleEndian(ShapeFileConstants.VersionCode));
                    writer.Write(ByteEncoder.GetLittleEndian((Int32)type));
                    writer.Write(ByteEncoder.GetLittleEndian(0.0));
                    writer.Write(ByteEncoder.GetLittleEndian(0.0));
                    writer.Write(ByteEncoder.GetLittleEndian(0.0));
                    writer.Write(ByteEncoder.GetLittleEndian(0.0));
                    writer.Write(new Byte[32]); // Z-values and M-values

                    Byte[] header = buffer.ToArray();

                    using (FileStream shape = File.Create(shapeFile))
                    {
                        shape.Write(header, 0, header.Length);
                    }

                    using (FileStream index = File.Create(Path.Combine(directory.FullName, layerName + ".shx")))
                    {
                        index.Write(header, 0, header.Length);
                    }
                }
            }

            if (schemaTable != null)
            {
                String filePath = Path.Combine(directory.FullName, layerName + ".dbf");
                DbaseFile file = DbaseFile.CreateDbaseFile(filePath, schemaTable, culture, encoding, geoFactory, storageManager);
                file.Close();
            }

            //if (geoFactory.SpatialReference != null)
            //{
            //    string filePath = Path.Combine(directory.FullName, layerName + ".prj");
            //    File.WriteAllText(filePath, geoFactory.SpatialReference.Wkt);
            //}

            return new ShapeFileProvider(shapeFile, geoFactory, storageManager, model != null ? model.SchemaFactory : schemaFactory);
        }

        #endregion

        #region ShapeFile specific properties

        /// <summary>
        /// Gets the name of the DBase attribute file.
        /// </summary>
        public String DbfFilename
        {
            get
            {
                return _filename.Replace(".shp", ".dbf");
            }
        }

        /// <summary>
        /// Gets or sets the encoding used for parsing strings from the DBase DBF file.
        /// </summary>
        /// <remarks>
        /// The DBase default encoding is <see cref="System.Text.Encoding.UTF8"/>.
        /// </remarks>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if property is read or set and the shapefile is closed. 
        /// Check <see cref="ProviderBase.IsOpen"/> before calling.
        /// </exception>
        /// <exception cref="ShapeFileIsInvalidException">
        /// Thrown if set and there is no DBase file with this shapefile.
        /// </exception>
        public Encoding Encoding
        {
            get
            {
                checkOpen();

                return HasDbf
                           ? _dbaseFile.Encoding
                           : EncodingEx.GetACSII();
            }
        }

        /// <summary>
        /// Gets the filename of the shapefile
        /// </summary>
        /// <remarks>If the filename changes, indexes will be rebuilt</remarks>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if method is executed and the shapefile is open or 
        /// if set and the specified filename already exists.
        /// Check <see cref="ProviderBase.IsOpen"/> before calling.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        /// <exception cref="ShapeFileIsInvalidException">
        /// Thrown if set and the shapefile cannot be opened after a rename.
        /// </exception>
        public String Filename
        {
            get { return _filename; }
            // set removed after r225
        }

        /// <summary>
        /// Gets or sets a delegate used for filtering records from the shapefile.
        /// </summary>
        public Predicate<IRecord> Filter
        {
            get { return _filterDelegate; }
            set { _filterDelegate = value; }
        }

        /// <summary>
        /// Gets true if the shapefile has an attributes file, false otherwise.
        /// </summary>
        public Boolean HasDbf
        {
            get { return _hasDbf; }
        }

        /// <summary>
        /// The name given to the row identifier in a ShapeFileProvider.
        /// </summary>
        public String IdColumnName
        {
            get { return ShapeFileConstants.IdColumnName; }
        }

        /// <summary>
        /// Gets the record index (.shx file) filename for the given shapefile
        /// </summary>
        public String IndexFilename
        {
            get
            {
                return _filename.Replace(".shp", ".shx");
            }
        }

        /// <summary>
        /// Gets a value indicating if the shapefile is spatially indexed.
        /// </summary>
        public Boolean IsSpatiallyIndexed
        {
            get { return _isIndexed; }
            set
            {
                //throw new NotImplementedException("Allow shapefile provider to be " +
                //                                  "created without an index. [workitem:13025]");
                if (IsOpen)
                {
                    throw new NotSupportedException("Setting 'IsSpatiallyIndexed' only supported before" +
                                                    " the shapefile is opened.");
                }

                _isIndexed = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Shapefile.ShapeType">
        /// shape geometry type</see> in this shapefile.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property isn't set until the first time the data source has been opened,
        /// and will throw an exception if this property has been called since initialization. 
        /// </para>
        /// <para>
        /// All the non-<see cref="Shapefile.ShapeType.Null"/> 
        /// shapes in a shapefile are required to be of the same shape type.
        /// </para>
        /// </remarks>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if property is read and the shapefile is closed. 
        /// Check <see cref="ProviderBase.IsOpen"/> before calling.
        /// </exception>
        public ShapeType ShapeType
        {
            get
            {
                checkOpen();
                return _header.ShapeType;
            }
        }

        public DbaseHeader DbaseHeader
        {
            get { return _dbaseFile.Header; }
        }

        public Int32 FeatureCount
        {
            get { return GetFeatureCount(); }
        }

        #endregion

        #region ShapeFile specific methods

        /// <summary>
        /// Opens the data source.
        /// </summary>
        /// <param name="writeAccess">Specify the access rights to the files.</param>
        public void Open(WriteAccess writeAccess)
        {

            if (IsOpen)
            {
                return;
            }

            IsOpen = true;

            FilePermissions @params = GetPermissions(writeAccess);

            try
            {
                //enableReading();


                _shapeFileStream = StorageManager.Open(Filename,
                                                  @params.FileMode,
                                                  @params.FileAccess,
                                                  @params.FileShare);

                _shapeFileReader = new BinaryReader(_shapeFileStream);
                if (writeAccess != WriteAccess.ReadOnly)
                    _shapeFileWriter = new BinaryWriter(_shapeFileStream);
                // TODO: NullBinaryWriter


                // Read projection file
                //parseProjection();

                // Load spatial (r-tree) index
                loadSpatialIndex(_hasFileBasedSpatialIndex);

                if (HasDbf)
                {
                    _dbaseFile = new DbaseFile(DbfFilename, GeometryFactory, StorageManager, SchemaFactory);
                    _dbaseFile.Open(writeAccess);
                }
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public bool IsOpen
        {
            get;
            protected set;
        }

        internal static FilePermissions GetPermissions(WriteAccess writeAccess)
        {
            FilePermissions @params = new FilePermissions();
            switch (writeAccess)
            {
                case WriteAccess.ReadOnly:
                    @params.FileMode = FileMode.Open;
                    @params.FileAccess = FileAccess.Read;
                    @params.FileShare = FileShare.Read;
                    break;

                case WriteAccess.ReadWrite:
                    @params.FileMode = FileMode.OpenOrCreate;
                    @params.FileAccess = FileAccess.ReadWrite;
                    @params.FileShare = FileShare.ReadWrite;
                    break;

                case WriteAccess.Exclusive:
                    @params.FileMode = FileMode.OpenOrCreate;
                    @params.FileAccess = FileAccess.ReadWrite;
                    @params.FileShare = FileShare.None;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("writeAccess");
            }
            return @params;
        }

        /// <summary>
        /// Forces a rebuild of the spatial index. 
        /// If the instance of the ShapeFile provider
        /// uses a file-based index the file is rewritten to disk,
        /// otherwise it is kept only in memory.
        /// </summary>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if method is executed and the shapefile is closed. 
        /// Check <see cref="ProviderBase.IsOpen"/> before calling.
        /// </exception>
        public void RebuildSpatialIndex()
        {
            checkOpen();
            //enableReading();

            if (_hasFileBasedSpatialIndex)
            {
                if (StorageManager.FileExists(_filename + SharpMapShapeFileIndexFileExtension))
                {
                    StorageManager.FileDelete(_filename + SharpMapShapeFileIndexFileExtension);
                }

                _spatialIndex = createSpatialIndexFromFile(_filename);
            }
            else
            {
                _spatialIndex = createSpatialIndex();
            }
        }

        //public IFeatureDataReader GetReader()
        //{
        //    FeatureQueryExpression query = FeatureQueryExpression.Intersects(GetExtents());
        //    return ExecuteFeatureQuery(query, FeatureQueryExecutionOptions.FullFeature);
        //}

        #endregion

        /// <summary>
        /// returns the EffectiveShapeType taking into account any ForcedCoordinateOptions
        /// </summary>
        public ShapeType EffectiveShapeType
        {
            get
            {
                if (ForceCoordinateOptions == ForceCoordinateOptions.ForceNone)
                    return ShapeType;

                switch (ForceCoordinateOptions)
                {
                    case ForceCoordinateOptions.Force2D:
                        {
                            switch (ShapeType)
                            {
                                case ShapeType.Point:
                                case ShapeType.PointM:
                                case ShapeType.PointZ:
                                    {
                                        return ShapeType.Point;
                                    }
                                case ShapeType.PolyLine:
                                case ShapeType.PolyLineM:
                                case ShapeType.PolyLineZ:
                                    {
                                        return ShapeType.PolyLine;
                                    }
                                case ShapeType.MultiPoint:
                                case ShapeType.MultiPointM:
                                case ShapeType.MultiPointZ:
                                    {
                                        return ShapeType.MultiPoint;
                                    }
                                case ShapeType.Polygon:
                                case ShapeType.PolygonM:
                                case ShapeType.PolygonZ:
                                    {
                                        return ShapeType.Polygon;
                                    }
                            }

                            break;
                        }
                    case ForceCoordinateOptions.Force2DM:
                        {
                            switch (ShapeType)
                            {
                                case ShapeType.Point:
                                case ShapeType.PointM:
                                case ShapeType.PointZ:
                                    {
                                        return ShapeType.PointM;
                                    }
                                case ShapeType.PolyLine:
                                case ShapeType.PolyLineM:
                                case ShapeType.PolyLineZ:
                                    {
                                        return ShapeType.PolyLineM;
                                    }
                                case ShapeType.MultiPoint:
                                case ShapeType.MultiPointM:
                                case ShapeType.MultiPointZ:
                                    {
                                        return ShapeType.MultiPointM;
                                    }
                                case ShapeType.Polygon:
                                case ShapeType.PolygonM:
                                case ShapeType.PolygonZ:
                                    {
                                        return ShapeType.PolygonM;
                                    }
                            }

                            break;
                        }

                    case ForceCoordinateOptions.Force3D:
                    case ForceCoordinateOptions.Force3DM:
                        {
                            switch (ShapeType)
                            {
                                case ShapeType.Point:
                                case ShapeType.PointM:
                                case ShapeType.PointZ:
                                    {
                                        return ShapeType.PointZ;
                                    }
                                case ShapeType.PolyLine:
                                case ShapeType.PolyLineM:
                                case ShapeType.PolyLineZ:
                                    {
                                        return ShapeType.PolyLineZ;
                                    }
                                case ShapeType.MultiPoint:
                                case ShapeType.MultiPointM:
                                case ShapeType.MultiPointZ:
                                    {
                                        return ShapeType.MultiPointZ;
                                    }
                                case ShapeType.Polygon:
                                case ShapeType.PolygonM:
                                case ShapeType.PolygonZ:
                                    {
                                        return ShapeType.PolygonZ;
                                    }
                            }

                            break;
                        }
                }
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Allows the user to override the creation of coordinates.
        /// This can be useful when reading data from bad 3D files.
        /// </summary>
        public ForceCoordinateOptions ForceCoordinateOptions
        {
            get { return _forceCoordinateOptions; }
            set { _forceCoordinateOptions = value; }
        }

        ///<summary>
        /// Enables the shapefile provider to try handling invalid polygon data. 
        ///</summary>
        public ShapeFileReadStrictness ReadStrictness
        {
            get { return _readStrictness; }
            set { _readStrictness = value; }
        }

        /// <summary>
        /// Gets the connection ID of the data source.
        /// </summary>
        /// <remarks>
        /// The connection ID of a shapefile is its filename.
        /// </remarks>
        public String ConnectionId
        {
            get { return _filename; }
        }

        /// <summary>
        /// Computes the extents of the data source.
        /// </summary>
        /// <returns>
        /// An <see cref="IExtents"/> instance describing the extents of the entire data source.
        /// </returns>
        public IEnvelope GetExtents()
        {
            IEnvelope extents = _spatialIndex != null
                                   ? _spatialIndex.Bounds
                                   : _header.Extents;

            //return CoordinateTransformation != null && CoordinateTransformation.Target != extents.SpatialReference
            //           ? CoordinateTransformation.Transform(extents, GeometryFactory)
            //           : extents;

            return extents;
        }



        public IGeometryFactory GeometryFactory
        {
            get { return _geometryFactory; }
            set
            {
                // [codekaizen 2008-10-07]
                // Setting this doesn't seem like a good idea... probably need more test cases.
                throw new NotSupportedException("Setting GeometryFactory not well tested, disallowing for now.");
                //_geoFactory = value;
            }
        }

        /// <summary>
        /// Returns the number of features in the entire data source.
        /// </summary>
        /// <returns>Count of the features in the entire data source.</returns>
        public Int32 GetFeatureCount()
        {
            return _shapeFileIndex.Count;
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> with rows describing the columns in the schema
        /// for the configured provider. Provides the same result as 
        /// <see cref="IDataReader.GetSchemaTable"/>.
        /// </summary>
        /// <seealso cref="IDataReader.GetSchemaTable"/>
        /// <returns>A DataTable that describes the column metadata.</returns>
        public ISchema GetSchemaTable()
        {
            checkOpen();

            ISchema schemaTable = _dbaseFile.GetSchemaTable();

            //DataRow oidColumn = schemaTable.NewRow();
            //oidColumn[ProviderSchemaHelper.ColumnNameColumn] = "OID";
            //oidColumn[ProviderSchemaHelper.ColumnSizeColumn] = 0;
            //oidColumn[ProviderSchemaHelper.ColumnOrdinalColumn] = 0;
            //oidColumn[ProviderSchemaHelper.NumericPrecisionColumn] = 0;
            //oidColumn[ProviderSchemaHelper.NumericScaleColumn] = 0;
            //oidColumn[ProviderSchemaHelper.DataTypeColumn] = typeof(UInt32);
            //oidColumn[ProviderSchemaHelper.AllowDBNullColumn] = true;
            //oidColumn[ProviderSchemaHelper.IsReadOnlyColumn] = false;
            //oidColumn[ProviderSchemaHelper.IsUniqueColumn] = true;
            //oidColumn[ProviderSchemaHelper.IsRowVersionColumn] = false;
            //oidColumn[ProviderSchemaHelper.IsKeyColumn] = true;
            //oidColumn[ProviderSchemaHelper.IsAutoIncrementColumn] = false;
            //oidColumn[ProviderSchemaHelper.IsLongColumn] = false;
            //schemaTable.Rows.InsertAt(oidColumn, 0);

            //for (Int32 i = 1; i < schemaTable.Rows.Count; i++)
            //{
            //    schemaTable.Rows[i][ProviderSchemaHelper.ColumnOrdinalColumn] = i;
            //}

            return schemaTable;
        }

        /// <summary>
        /// Gets the locale of the data as a CultureInfo.
        /// </summary>
        public CultureInfo Locale
        {
            get { return _dbaseFile.CultureInfo; }
        }

        ///// <summary>
        ///// Sets the schema of the given table to match the schema of the shapefile's attributes.
        ///// </summary>
        ///// <param name="target">Target table to set the schema of.</param>
        //public override void SetTableSchema(FeatureDataTable target)
        //{
        //    checkOpen();
        //    _dbaseFile.SetTableSchema(target, SchemaMergeAction.AddWithKey);
        //}

        public IEnvelope GetExtentsByOid(UInt32 oid)
        {
            checkOpen();

            IEnvelope result;

            if (Filter != null) // Apply filtering
            {
                IRecord fdr = GetFeatureByOid(oid);

                result = fdr != null
                             ? fdr.GetValue<IGeometry>("Geom").EnvelopeInternal
                             : null;
            }
            else
            {
                result = readExtents(oid);
            }

            return result;
        }

        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if method is called and the shapefile is closed. Check <see cref="ProviderBase.IsOpen"/> 
        /// before calling.
        /// </exception>
        public IRecord GetFeatureByOid(UInt32 oid)
        {
            return getFeature(oid, _dbaseFile.GetSchemaTable());
        }


        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns><see cref="IGeometry"/></returns>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if method is called and the shapefile is closed. Check <see cref="ProviderBase.IsOpen"/> 
        /// before calling.
        /// </exception>
        public IGeometry GetGeometryByOid(UInt32 oid)
        {
            checkOpen();

            IGeometry result;

            if (Filter != null) // Apply filtering
            {
                IRecord fdr = GetFeatureByOid(oid);

                result = fdr != null
                             ? fdr.GetValue<IGeometry>("Geom")
                             : null;
            }
            else
            {
                result = readGeometry(oid);
            }

            return result;
        }

        /// <summary>
        /// Returns feature oids which match <paramref name="query"/>.
        /// </summary>
        /// <param name="query">Query expression for features.</param>
        /// <returns>
        /// An enumeration of oids (Object ids) which match the given <paramref name="query"/>.
        /// </returns>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if method is called and the shapefile is closed. Check <see cref="ProviderBase.IsOpen"/> 
        /// before calling.
        /// </exception>
        public IEnumerable<UInt32> ExecuteOidQuery(IEnvelope query)
        {
            // if (query == null) throw new ArgumentNullException("query");

            //jd:modifying so that null query returns all OIDS

            if (Equals(query, null))
                return getAllOIDs();
            else
            {


                checkOpen();

                IEnumerable<IdBounds> keys = _isIndexed
                                                 ? queryIndex(query)
                                                 : queryData(query);

                return keys.Select(a => a.Id);


            }
        }

        ///// <summary>
        ///// Sets the schema of the given table to match the schema of the shapefile's attributes.
        ///// </summary>
        ///// <param name="target">Target table to set the schema of.</param>
        //public void SetTableSchema(FeatureDataTable<UInt32> target)
        //{
        //    if (String.CompareOrdinal(target.IdColumn.ColumnName, DbaseSchema.OidColumnName) != 0)
        //    {
        //        throw new InvalidOperationException(
        //            "Object ID column names for this schema and 'target' schema must be identical, " +
        //            "including case. For case-insensitive or type-only matching, use " +
        //            "SetTableSchema(FeatureDataTable, SchemaMergeAction) with the " +
        //            "SchemaMergeAction.CaseInsensitive option and/or SchemaMergeAction.KeyByType " +
        //            "option enabled.");
        //    }

        //    SetTableSchema(target, SchemaMergeAction.AddWithKey);
        //}

        ///// <summary>
        ///// Sets the schema of the given table to match the schema of the shapefile's attributes.
        ///// </summary>
        ///// <param name="target">Target table to set the schema of.</param>
        ///// <param name="mergeAction">Action or actions to take when schemas don't match.</param>
        //public void SetTableSchema(FeatureDataTable<UInt32> target, SchemaMergeAction mergeAction)
        //{
        //    checkOpen();
        //    _dbaseFile.SetTableSchema(target, mergeAction);
        //}

        ///// <summary>
        ///// Adds a feature to the end of a shapefile.
        ///// </summary>
        ///// <param name="feature">Feature to append.</param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. 
        ///// Check <see cref="ProviderBase.IsOpen"/> before calling.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="feature"/> is null.
        ///// </exception>
        ///// <exception cref="InvalidOperationException">
        ///// Thrown if <paramref name="feature.Geometry"/> is null.
        ///// </exception>
        //public void Insert(FeatureDataRow<UInt32> feature)
        //{
        //    if (feature == null)
        //    {
        //        throw new ArgumentNullException("feature");
        //    }

        //    if (feature.Geometry == null)
        //    {
        //        throw new InvalidOperationException("Cannot insert a feature with a null geometry");
        //    }

        //    if (ShapeType != EffectiveShapeType)
        //        throw new InvalidOperationException(
        //            "It is invalid to write to a shapefile whos' ForceCoordinateOptions property has been modified");

        //    checkOpen();
        //    //enableWriting();

        //    UInt32 id = _shapeFileIndex.GetNextId();
        //    feature[ShapeFileConstants.IdColumnName] = id;

        //    _shapeFileIndex.AddFeatureToIndex(feature);

        //    IExtents featureExtents = feature.Geometry.Extents;

        //    if (_spatialIndex != null)
        //    {
        //        _spatialIndex.Insert(new IdBounds(id, featureExtents));
        //    }

        //    Int32 offset = _shapeFileIndex[id].Offset;
        //    Int32 length = _shapeFileIndex[id].Length;

        //    _header.FileLengthInWords = _shapeFileIndex.ComputeShapeFileSizeInWords();
        //    _header.Extents = GeometryFactory.CreateExtents(_header.Extents, featureExtents);

        //    if (HasDbf)
        //    {
        //        _dbaseFile.AddRow(feature);
        //    }

        //    writeGeometry(feature.Geometry, id, offset, length);
        //    _header.WriteHeader(_shapeFileWriter);
        //    _shapeFileIndex.Save();
        //}

        ///// <summary>
        ///// Adds features to the end of a shapefile.
        ///// </summary>
        ///// <param name="features">Enumeration of features to append.</param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="features"/> is null.
        ///// </exception>
        //public void Insert(IEnumerable<FeatureDataRow<UInt32>> features)
        //{
        //    if (features == null)
        //    {
        //        throw new ArgumentNullException("features");
        //    }

        //    if (ShapeType != EffectiveShapeType)
        //        throw new InvalidOperationException(
        //            "It is invalid to write to a shapefile whos' ForceCoordinateOptions property has been modified");


        //    checkOpen();
        //    //enableWriting();

        //    IExtents allFeaturesExtents = null;

        //    foreach (FeatureDataRow<UInt32> feature in features)
        //    {
        //        IExtents featureExtents = feature.Geometry == null
        //                                      ? null
        //                                      : feature.Geometry.Extents;

        //        if (allFeaturesExtents == null)
        //        {
        //            allFeaturesExtents = featureExtents;
        //        }
        //        else
        //        {
        //            allFeaturesExtents.ExpandToInclude(featureExtents);
        //        }

        //        UInt32 id = _shapeFileIndex.GetNextId();
        //        feature[ShapeFileConstants.IdColumnName] = id;

        //        _shapeFileIndex.AddFeatureToIndex(feature);

        //        if (_spatialIndex != null)
        //        {
        //            _spatialIndex.Insert(new IdBounds(id, featureExtents));
        //        }

        //        //feature[ShapeFileConstants.IdColumnName] = id;


        //        ShapeFileIndex.IndexEntry entry = _shapeFileIndex[id];

        //        writeGeometry(feature.Geometry, id, entry.Offset, entry.Length);

        //        if (HasDbf)
        //        {
        //            _dbaseFile.AddRow(feature);
        //        }
        //    }

        //    _shapeFileIndex.Save();

        //    _header.Extents = GeometryFactory.CreateExtents(_header.Extents, allFeaturesExtents);
        //    _header.FileLengthInWords = _shapeFileIndex.ComputeShapeFileSizeInWords();
        //    _header.WriteHeader(_shapeFileWriter);
        //}

        ///// <summary>
        ///// Updates a feature in a shapefile by deleting the previous 
        ///// version and inserting the updated version.
        ///// </summary>
        ///// <param name="feature">Feature to update.</param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. 
        ///// Check <see cref="IsOpen"/> before calling.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="feature"/> is null.
        ///// </exception>
        //public void Update(FeatureDataRow<UInt32> feature)
        //{
        //    if (feature == null)
        //    {
        //        throw new ArgumentNullException("feature");
        //    }

        //    if (feature.RowState != DataRowState.Modified)
        //    {
        //        return;
        //    }

        //    checkOpen();
        //    //enableWriting();

        //    if (feature.IsGeometryModified)
        //    {
        //        Delete(feature);
        //        Insert(feature);
        //    }
        //    else if (HasDbf)
        //    {
        //        _dbaseFile.UpdateRow(feature.Id, feature);
        //    }

        //    feature.AcceptChanges();
        //}

        ///// <summary>
        ///// Updates a set of features in a shapefile by deleting the previous 
        ///// versions and inserting the updated versions.
        ///// </summary>
        ///// <param name="features">Enumeration of features to update.</param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. 
        ///// Check <see cref="IsOpen"/> before calling.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="features"/> is null.
        ///// </exception>
        //public void Update(IEnumerable<FeatureDataRow<UInt32>> features)
        //{
        //    if (features == null)
        //    {
        //        throw new ArgumentNullException("features");
        //    }

        //    checkOpen();
        //    //enableWriting();

        //    foreach (FeatureDataRow<UInt32> feature in features)
        //    {
        //        if (feature.RowState != DataRowState.Modified)
        //        {
        //            continue;
        //        }

        //        if (feature.IsGeometryModified)
        //        {
        //            Delete(feature);
        //            Insert(feature);
        //        }
        //        else if (HasDbf)
        //        {
        //            _dbaseFile.UpdateRow(feature.Id, feature);
        //        }

        //        feature.AcceptChanges();
        //    }
        //}

        ///// <summary>
        ///// Deletes a row from the shapefile by marking it as deleted.
        ///// </summary>
        ///// <param name="feature">Feature to delete.</param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. 
        ///// Check <see cref="IsOpen"/> before calling.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="feature"/> is null.
        ///// </exception>
        //public void Delete(FeatureDataRow<UInt32> feature)
        //{
        //    if (feature == null)
        //    {
        //        throw new ArgumentNullException("feature");
        //    }

        //    if (!_shapeFileIndex.ContainsKey(feature.Id))
        //    {
        //        return;
        //    }

        //    checkOpen();
        //    //enableWriting();

        //    feature.Geometry = null;

        //    UInt32 id = feature.Id;

        //    ShapeFileIndex.IndexEntry entry = _shapeFileIndex[id];
        //    writeGeometry(null, id, entry.Offset, entry.Length);
        //}

        ///// <summary>
        ///// Deletes a set of rows from the shapefile by marking them as deleted.
        ///// </summary>
        ///// <param name="features">Features to delete.</param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. 
        ///// Check <see cref="IsOpen"/> before calling.
        ///// </exception>
        ///// <exception cref="ArgumentNullException">
        ///// Thrown if <paramref name="features"/> is null.
        ///// </exception>
        //public void Delete(IEnumerable<FeatureDataRow<UInt32>> features)
        //{
        //    if (features == null)
        //    {
        //        throw new ArgumentNullException("features");
        //    }

        //    checkOpen();
        //    //enableWriting();

        //    foreach (FeatureDataRow<UInt32> feature in features)
        //    {
        //        if (!_shapeFileIndex.ContainsKey(feature.Id))
        //        {
        //            continue;
        //        }

        //        feature.Geometry = null;

        //        UInt32 id = feature.Id;
        //        ShapeFileIndex.IndexEntry entry = _shapeFileIndex[id];

        //        writeGeometry(null, id, entry.Offset, entry.Length);

        //    }
        //}

        //private void initSpatialReference(ICoordinateSystem coordinateSystem)
        //{
        //    //checkOpen();
        //    if (_coordsysReadFromFile)
        //    {
        //        throw new ShapeFileInvalidOperationException("Coordinate system is specified in " +
        //                                                     "projection file and is read only");
        //    }

        //    OriginalSpatialReference = coordinateSystem;
        //    OriginalSrid = coordinateSystem.AuthorityCode;
        //    GeometryFactory.SpatialReference = SpatialReference;
        //    GeometryFactory.Srid = Srid;
        //}

        //protected override IFeatureDataReader InternalExecuteFeatureQuery(FeatureQueryExpression query,
        //                                                                  FeatureQueryExecutionOptions options)
        //{
        //    if (query == null) throw new ArgumentNullException("query");

        //    checkOpen();

        //    lock (_readerSync)
        //    {
        //        //if (_currentReader != null)
        //        //{
        //        //    throw new ShapeFileInvalidOperationException("Can't open another ShapeFileDataReader " +
        //        //                                                 "on this ShapeFile, since another reader " +
        //        //                                                 "is already active.");
        //        //}

        //        ShapeFileDataReader reader = new ShapeFileDataReader(this, query, options);
        //        //reader.Disposed += readerDisposed;
        //        reader.CoordinateTransformation = CoordinateTransformation;
        //        return reader;
        //    }
        //}

        //protected override IFeatureDataReader InternalExecuteFeatureQuery(FeatureQueryExpression query)
        //{
        //    return InternalExecuteFeatureQuery(query, FeatureQueryExecutionOptions.FullFeature);
        //}

        /// <summary>
        /// Return all Features within the shapefile
        /// </summary>
        /// <returns> collection of features </returns>
        public IEnumerable<IRecord> GetAllFeatues()
        {
            IRecord result = null;
            for (UInt32 i = 1; i <= FeatureCount; i++)
            {
                result = getFeature(i, _dbaseFile.GetSchemaTable());
                if (result != null)
                    yield return result;
            }
        }

        public IEnumerable<IRecord> GetFeatures(IEnumerable<UInt32> oids)
        {
            //FeatureDataTable<UInt32> table = CreateNewTable() as FeatureDataTable<UInt32>;
            //Assert.IsNotNull(table);
            //table.IsSpatiallyIndexed = false;

            foreach (UInt32 oid in oids)
            {
                yield return getFeature(oid, null);
            }

            //return table;
        }

        #region Methods

        /// <summary>
        /// Closes the data source
        /// </summary>
        public void Close()
        {
            (this as IDisposable).Dispose();
        }

        ~ShapeFileProvider()
        {
            Close();
        }

        //public override Object ExecuteQuery(Expression query)
        //{
        //    FeatureQueryExpression featureQuery = query as FeatureQueryExpression;

        //    if (featureQuery == null)
        //    {
        //        throw new ArgumentException("The query must be a non-null FeatureQueryExpression.");
        //    }

        //    return ExecuteFeatureQuery(featureQuery);
        //}


        /// <summary>
        /// Opens the data source.
        /// </summary>
        public void Open()
        {
            // Diego Guidi: defaults to ReadOnly, to avoid any kind of lock.
            this.Open(WriteAccess.ReadOnly);
        }

        #endregion

        ///// <summary>
        ///// Saves features to the shapefile.
        ///// </summary>
        ///// <param name="table">
        ///// A FeatureDataTable containing feature data and geometry.
        ///// </param>
        ///// <exception cref="ShapeFileInvalidOperationException">
        ///// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
        ///// </exception>
        //public void Save(FeatureDataTable<UInt32> table)
        //{
        //    if (table == null)
        //    {
        //        throw new ArgumentNullException("table");
        //    }

        //    checkOpen();
        //    enableWriting();

        //    _shapeFileStream.Position = ShapeFileConstants.HeaderSizeBytes;
        //    foreach (FeatureDataRow row in table.Rows)
        //    {
        //        if (row is FeatureDataRow<UInt32>)
        //        {
        //            _tree.Insert(new RTreeIndexEntry<UInt32>((row as FeatureDataRow<UInt32>).Id, row.Geometry.GetBoundingBox()));
        //        }
        //        else
        //        {
        //            _tree.Insert(new RTreeIndexEntry<UInt32>(getNextId(), row.Geometry.GetBoundingBox()));
        //        }

        //        writeFeatureRow(row);
        //    }

        //    writeIndex();
        //    writeHeader(_shapeFileWriter);
        //}

        #endregion

        #region General helper functions

        internal static Int32 ComputeGeometryLengthInWords(IGeometry geometry, ShapeType shapeType)
        {
            if (geometry == null)
            {
                throw new NotSupportedException("Writing null shapes not supported in this version.");
            }

            Int32 byteCount;

            if (geometry is IPoint)
            {
                switch (shapeType)
                {
                    case ShapeType.Point:
                        {
                            byteCount = 20; // ShapeType integer + 2 doubles at 8 bytes each
                            break;
                        }
                    case ShapeType.PointM:
                        {
                            byteCount = 28;
                            break;
                        }
                    case ShapeType.PointZ:
                        {
                            byteCount = 36;
                            break;
                        }
                    default:
                        throw new ArgumentException("Incompatible shapeType.");

                }
            }
            else if (geometry is IMultiPoint)
            {
                byteCount = 4 /* ShapeType Integer */
                                         + ShapeFileConstants.BoundingBoxFieldByteLength + 4 /* NumPoints integer */
                                         + 16 * (geometry as IMultiPoint).Count;

                switch (shapeType)
                {
                    case ShapeType.MultiPoint:
                        {
                            break;
                        }
                    case ShapeType.MultiPointM:
                        {
                            byteCount += 16 /* Min Max M range */
                                         + 8 * (geometry as IMultiPoint).Count; /*M values array */
                            break;
                        }
                    case ShapeType.MultiPointZ:
                        {
                            byteCount += 16 /* Min Max Z range */
                                         + 8 * (geometry as IMultiPoint).Count /*Z values array */
                                         + 16 /* Min Max M range */
                                         + 8 * (geometry as IMultiPoint).Count; /*M values array */
                            break;
                        }
                    default:
                        throw new ArgumentException("Incompatible shapeType.");
                }

            }
            else if (geometry is ILineString)
            {
                byteCount = 4 /* ShapeType Integer */
                            + ShapeFileConstants.BoundingBoxFieldByteLength + 4 + 4 /* NumPoints and NumParts integers */
                            + 4 /* Parts Array 1 integer Int64 */
                            + 16 * (geometry as ILineString).Coordinates.Length;
                switch (shapeType)
                {
                    case ShapeType.PolyLine:
                        {
                            break;
                        }
                    case ShapeType.PolyLineM:
                        {
                            byteCount += 16 /* Min Max M Range */
                                         + 8 * (geometry as ILineString).Coordinates.Length; /* M values array*/
                            break;
                        }
                    case ShapeType.PolyLineZ:
                        {
                            byteCount += 16 /* Min Max Z Range */
                                         + 8 * (geometry as ILineString).Coordinates.Length /* Z values array*/
                                         + 16 /* Min Max M Range */
                                         + 8 * (geometry as ILineString).Coordinates.Length; /* M values array */

                            break;
                        }
                    default:
                        throw new ArgumentException("Incompatible shapeType.");
                }
            }
            else if (geometry is IMultiLineString)
            {
                Int32 pointCount = 0;

                foreach (ILineString line in (geometry as IEnumerable<ILineString>))
                {
                    pointCount += line.Coordinates.Length;
                }

                byteCount = 4 /* ShapeType Integer */
                            + ShapeFileConstants.BoundingBoxFieldByteLength
                            + 4
                            + 4 /* NumPoints and NumParts integers */
                            + 4 * (geometry as IMultiLineString).Count /* Parts array of integer indexes */
                            + 16 * pointCount;

                switch (shapeType)
                {
                    case ShapeType.PolyLine:
                        {
                            break;
                        }
                    case ShapeType.PolyLineM:
                        {
                            byteCount += 16 /* Min Max M Range */
                                         + 8 * (geometry as IMultiLineString).Coordinates.Length; /* M values array*/
                            break;
                        }
                    case ShapeType.PolyLineZ:
                        {
                            byteCount += 16 /* Min Max Z Range */
                                         + 8 * (geometry as IMultiLineString).Coordinates.Length /* Z values array*/
                                         + 16 /* Min Max M Range */
                                         + 8 * (geometry as IMultiLineString).Coordinates.Length; /* M values array */

                            break;
                        }
                    default:
                        throw new ArgumentException("Incompatible shapeType.");
                }
            }
            else if (geometry is IPolygon)
            /*jd: Contains Modifications from Lee Keel www.trimble.com to cope with unclosed polygons  */
            {
                Int32 pointCount = (geometry as IPolygon).ExteriorRing.Coordinates.Length;
                ILineString exring = (geometry as IPolygon).ExteriorRing;

                /* need to account for cases where the polygon is not closed. */
                if (exring.Coordinates[0] != exring.Coordinates[exring.Coordinates.Length - 1])
                    pointCount++;

                foreach (ILinearRing ring in (geometry as IPolygon).InteriorRings)
                {
                    pointCount += ring.Coordinates.Length;
                    /* need to account for cases where the polygon is not closed. */
                    if (ring.Coordinates[0] != ring.Coordinates[ring.Coordinates.Length - 1])
                        pointCount++;
                }

                byteCount = 4 /* ShapeType Integer */
                            + ShapeFileConstants.BoundingBoxFieldByteLength
                            + 4
                            + 4 /* NumPoints and NumParts integers */
                            + 4 * ((geometry as IPolygon).InteriorRings.Length + 1 /* Parts array of rings: count of interior + 1 for exterior ring */)
                            + 16 * pointCount;

                switch (shapeType)
                {
                    case ShapeType.Polygon:
                        {
                            break;
                        }
                    case ShapeType.PolygonM:
                        {
                            byteCount += 16 /* Min Max M values*/
                                         + pointCount * 8 /*M value array  */;
                            break;
                        }
                    case ShapeType.PolygonZ:
                        {
                            byteCount += 16 /* Min Max Z values*/
                                         + pointCount * 8 /*Z value array  */
                                         + 16 /* Min Max M values*/
                                         + pointCount * 8 /*M value array  */;
                            break;
                        }
                    default:
                        throw new ArgumentException("Incompatible shapeType.");
                }
            }
            else if (geometry is IMultiPolygon)
            /*jd: Contains Modifications from Lee Keel www.trimble.com to cope with unclosed polygons  */
            {
                Int32 pointCount = 0;
                Int32 ringCount = 0;
                IMultiPolygon mp = geometry as IMultiPolygon;
                foreach (IPolygon p in mp as IEnumerable<IPolygon>)
                {
                    pointCount += p.ExteriorRing.NumPoints;
                    foreach (ILinearRing ring in p.InteriorRings)
                    {
                        pointCount += ring.NumPoints;
                        /* need to account for cases where the polygon is not closed. */
                        if (ring.Coordinates[0] != ring.Coordinates[ring.Coordinates.Length - 1])
                            pointCount++;
                    }

                    ringCount += p.InteriorRings.Length + 1;
                }

                byteCount = 4
                            + ShapeFileConstants.BoundingBoxFieldByteLength + 4 + 4
                            + 4 * ringCount
                            + 16 * pointCount;

                switch (shapeType)
                {
                    case ShapeType.Polygon:
                        {
                            break;
                        }
                    case ShapeType.PolygonM:
                        {
                            byteCount += 16 /* Min Max M values*/
                                         + pointCount * 8 /*M value array  */;
                            break;
                        }
                    case ShapeType.PolygonZ:
                        {
                            byteCount += 16 /* Min Max Z values*/
                                         + pointCount * 8 /*Z value array  */
                                         + 16 /* Min Max M values*/
                                         + pointCount * 8 /*M value array  */;
                            break;
                        }
                    default:
                        throw new ArgumentException("Incompatible shapeType.");
                }
            }
            else
            {
                throw new NotSupportedException("Currently unsupported geometry type.");
            }

            return byteCount / 2; // number of 16-bit words
        }

        //private Boolean isMatch(SpatialOperation op,
        //                        Boolean isQueryLeft,
        //                        IdBounds idBounds,
        //                        SpatialExpression spatialExpression)
        //{
        //    GeometryExpression geometryExpression = spatialExpression as GeometryExpression;

        //    IGeometry candidateGeometry = geometryExpression == null
        //                                      ? null
        //                                      : idBounds.Feature == null
        //                                            ? GetGeometryByOid(idBounds.Id)
        //                                            : idBounds.Feature.Geometry;
        //    IExtents candidateExtents = geometryExpression == null
        //                                    ? idBounds.Bounds
        //                                    : candidateGeometry.Extents;

        //    if (geometryExpression != null)
        //    {
        //        return SpatialBinaryExpression.IsMatch(op, isQueryLeft, candidateGeometry, geometryExpression.Geometry);
        //    }

        //    ExtentsExpression extentsExpression = spatialExpression as ExtentsExpression;

        //    if (extentsExpression != null)
        //    {
        //        return SpatialBinaryExpression.IsMatch(op, isQueryLeft, candidateExtents, extentsExpression.Extents);
        //    }

        //    return true;
        //}

        private void checkOpen()
        {
            if (!IsOpen)
            {
                throw new ShapeFileInvalidOperationException("An attempt was made to access a closed data source.");
            }
        }

        //private static IEnumerable<UInt32> getKeysFromIndexEntries(IEnumerable<RTreeIndexEntry<UInt32>> entries)
        //{
        //    foreach (RTreeIndexEntry<UInt32> entry in entries)
        //    {
        //        yield return entry.Value;
        //    }
        //}

        private IEnumerable<IdBounds> getAllKeys()
        {
            return _isIndexed
                       ? getKeysFromSpatialIndex(_spatialIndex.Bounds)
                       : getKeysFromShapefileIndex(GetExtents());
        }

        private IEnumerable<UInt32> getAllOIDs()
        {
            foreach (IdBounds bounds in getAllKeys())
                yield return bounds.Id;
        }

        private IEnumerable<IdBounds> getKeysFromSpatialIndex(IEnvelope toIntersect)
        {
            return _spatialIndex.Query(toIntersect);
        }

        private IEnumerable<IdBounds> getKeysFromShapefileIndex(IEnvelope toIntersect)
        {
            foreach (KeyValuePair<UInt32, ShapeFileIndex.IndexEntry> entry in _shapeFileIndex)
            {
                UInt32 oid = entry.Key;

                IEnvelope featureExtents = readExtents(oid);

                if (toIntersect.Intersects(featureExtents))
                {
                    yield return new IdBounds(oid, featureExtents);
                }
            }
        }

        //private static IEnumerable<UInt32> getUint32IdsFromObjects(IEnumerable oids)
        //{
        //    foreach (Object oid in oids)
        //    {
        //        yield return (UInt32)oid;
        //    }
        //}

        //private IEnumerable<IFeatureDataRecord> getFeatureRecordsFromIds(IEnumerable<UInt32> ids,
        //                                                                 FeatureDataTable<UInt32> table)
        //{
        //    foreach (UInt32 id in ids)
        //    {
        //        yield return getFeature(id, table);
        //    }
        //}

        //private FeatureDataTable<UInt32> getNewTable()
        //{
        //    return HasDbf
        //               ? _dbaseFile.NewTable
        //               : FeatureDataTable<UInt32>.CreateEmpty(ShapeFileConstants.IdColumnName, GeometryFactory);
        //}

        /// <summary>
        /// Gets a row from the DBase attribute file which has the 
        /// specified <paramref name="oid">Object id</paramref> created from
        /// <paramref name="table"/>.
        /// </summary>
        /// <param name="oid">Object id to lookup.</param>
        /// <param name="table">DataTable with schema matching the feature to retrieve.</param>
        /// <returns>Row corresponding to the Object id.</returns>
        /// <exception cref="ShapeFileInvalidOperationException">
        /// Thrown if method is called and the shapefile is closed.
        /// Check <see cref="ProviderBase.IsOpen"/> before calling.
        /// </exception>
        private IRecord getFeature(UInt32 oid, ISchema table)
        {
            checkOpen();
            Guard.IsNotNull(table, "table");

            IRecord featureRecord = HasDbf
                                    ? _dbaseFile.GetAttributes(oid, table)
                                    : table.RecordFactory.Create(new Dictionary<IPropertyInfo, IValue>()
                                                        {
                                                            {table.Property("Geom"), null},
                                                            {table.Property("OID"), table.Property("OID").CreateValue(oid)}
                                                        });

            featureRecord["Geom"] = table.Property("Geom").CreateValue(readGeometry(oid));
            featureRecord["OID"] = table.Property("OID").CreateValue(oid);


            return Filter == null || Filter(featureRecord) ? featureRecord : null;
        }

        //private ShapeFileFeatureDataRecord getOidOnlyFeatureRecord(UInt32 oid)
        //{
        //    ShapeFileFeatureDataRecord record = new ShapeFileFeatureDataRecord(_oidFieldList);
        //    record.SetColumnValue(0, oid);
        //    return record;
        //}

        //private void readerDisposed(Object sender, EventArgs e)
        //{
        //    lock (_readerSync)
        //    {
        //        _currentReader = null;
        //    }
        //}

        //private IEnumerable<IFeatureDataRecord> matchFeatureGeometry(IEnumerable<IdBounds> keys, 
        //                                                             IGeometry query,
        //                                                             Boolean queryIsLeft,
        //                                                             SpatialOperation op)
        //{
        //    foreach (IdBounds key in keys)
        //    {
        //        IFeatureDataRecord candidate = GetFeatureByOid(key.Id);

        //        if (SpatialBinaryExpression.IsMatch(op, queryIsLeft, query, candidate.Geometry))
        //        {
        //            yield return candidate;
        //        }
        //    }
        //}

        private IEnumerable<IdBounds> queryData(IEnvelope query)
        {

            foreach (KeyValuePair<UInt32, ShapeFileIndex.IndexEntry> entry in _shapeFileIndex)
            {
                UInt32 oid = entry.Key;

                IEnvelope featureExtents = readExtents(oid);

                if (query.Intersects(featureExtents))
                {
                    yield return new IdBounds(oid, featureExtents);
                }
            }
        }

        //private static IExtents getExtentsFromSpatialQuery(SpatialExpression spatialExpression)
        //{
        //    GeometryExpression geometryExpression = spatialExpression as GeometryExpression;
        //    ExtentsExpression extentsExpression = spatialExpression as ExtentsExpression;

        //    Assert.IsTrue(geometryExpression != null || extentsExpression != null);

        //    IGeometry geometry = geometryExpression != null
        //                             ? geometryExpression.Geometry
        //                             : null;

        //    Assert.IsTrue(extentsExpression != null || geometry != null);

        //    IExtents extents = extentsExpression != null
        //                           ? extentsExpression.Extents
        //                           : geometry.Extents;

        //    return extents;
        //}

        #endregion

        //#region Spatial indexing helper functions

        private IEnumerable<IdBounds> queryIndex(IEnvelope envelope)
        {

            return _spatialIndex.Query(envelope);
        }

        /// <summary>
        /// Loads a spatial index from a file. If it doesn't exist, one is created and saved
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>QuadTree index</returns>
        private ISpatialIndex<IEnvelope, IdBounds> createSpatialIndexFromFile(String filename)
        {
            if (StorageManager.FileExists(filename + SharpMapShapeFileIndexFileExtension))
            {
                throw new NotImplementedException();

            }
            else
            {
                ISpatialIndex<IEnvelope, IdBounds> tree = createSpatialIndex();

                //using (FileStream indexStream =
                //        new FileStream(filename + ".sidx", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                //{
                //    tree.SaveIndex(indexStream);
                //}

                return tree;
            }
        }

        /// <summary>
        /// Generates a spatial index for a specified shape file.
        /// </summary>
        private ISpatialIndex<IEnvelope, IdBounds> createSpatialIndex()
        {

            ISpatialIndex<IEnvelope, IdBounds> index = new FakeSpatialIndex<IdBounds>();

            UInt32 featureCount = (UInt32)GetFeatureCount();

            for (UInt32 i = 0; i < featureCount; i++)
            {
                IGeometry geom = readGeometry(i + 1); //jd: shapefiles have 1 based index

                if (geom == null || geom.IsEmpty)
                {
                    continue;
                }

                IEnvelope extents = geom.EnvelopeInternal;
                index.Insert(extents, new IdBounds(i + 1, extents));
            }

            return index;
        }

        private void loadSpatialIndex(Boolean loadFromFile)
        {
            loadSpatialIndex(false, loadFromFile);
        }

        private void loadSpatialIndex(Boolean forceRebuild, Boolean loadFromFile)
        {
            if (!_isIndexed)
            {
                return;
            }

            //Only load the tree if we haven't already loaded it, or if we want to force a rebuild
            if (_spatialIndex == null || forceRebuild)
            {
                if (!loadFromFile)
                {
                    _spatialIndex = createSpatialIndex();
                }
                else
                {
                    _spatialIndex = createSpatialIndexFromFile(_filename);
                }
            }
        }

        //#endregion

        #region ProviderBase overrides

        //protected override void OnPropertyChanged(PropertyDescriptor property)
        //{
        //    base.OnPropertyChanged(property);

        //    if (property == CoordinateTransformationProperty)
        //    {
        //        GeometryFactory.SpatialReference = SpatialReference;
        //        _coordTransform = CoordinateTransformation.MathTransform;
        //        _header.Extents = CoordinateTransformation.Transform(_header.Extents, GeometryFactory);
        //    }
        //}

        #endregion

        #region Geometry reading helper functions

        private static readonly ShapeType[] MultiPointTypes = new[]
                                                                  {
                                                                      ShapeType.MultiPoint,
                                                                      ShapeType.MultiPointM,
                                                                      ShapeType.MultiPointZ
                                                                  };

        private static readonly ShapeType[] PointTypes = new[]
                                                             {
                                                                 ShapeType.Point,
                                                                 ShapeType.PointM,
                                                                 ShapeType.PointZ
                                                             };

        private static readonly ShapeType[] PolygonTypes = new[]
                                                               {
                                                                   ShapeType.Polygon,
                                                                   ShapeType.PolygonM,
                                                                   ShapeType.PolygonZ
                                                               };

        private static readonly ShapeType[] PolyLineTypes = new[]
                                                                {
                                                                    ShapeType.PolyLine,
                                                                    ShapeType.PolyLineM,
                                                                    ShapeType.PolyLineZ
                                                                };





        private struct StreamOffset
        {
            public readonly long? BBox;
            public readonly long X;
            public readonly long Y;
            public readonly long? Z;
            public readonly long? M;
            public readonly long Bor;
            public readonly long Eor;

            public StreamOffset(long? bbox, long? x, long? y, long? z, long? m, long? bor, long? eor)
            {
                BBox = bbox;
                X = x.Value;
                Y = y.Value;
                Z = z;
                M = m;
                Bor = bor.Value;
                Eor = eor.Value;
            }
        }

        private struct CoordinateValues
        {
            public Double this[Ordinates ordinate]
            {
                get
                {
                    switch (ordinate)
                    {
                        case Ordinates.X:
                            return X;
                        case Ordinates.Y:
                            return Y;
                        case Ordinates.M:
                            return M;
                        case Ordinates.Z:
                            return Z;
                        default:
                            throw new ArgumentException();
                    }
                }
            }
            public readonly Double X;
            public readonly Double Y;
            public readonly Double Z;
            public readonly Double M;

            public CoordinateValues(Double x, Double y, Double z, Double m)
            {
                X = ShapeFileProvider.IsShapefileNullValue(x) ? ShapeFileProvider.NullDoubleValue : x;
                Y = ShapeFileProvider.IsShapefileNullValue(y) ? ShapeFileProvider.NullDoubleValue : y;
                Z = ShapeFileProvider.IsShapefileNullValue(z) ? ShapeFileProvider.NullDoubleValue : z;
                M = ShapeFileProvider.IsShapefileNullValue(m) ? ShapeFileProvider.NullDoubleValue : m;
            }
        }


        private class StreamOffsetUtility
        {
            public bool Seek(Stream stream, long seekPosition, StreamOffset offsets)
            {
                //reduces perf considerably
                //if (offsets.Bor < 0 || offsets.Eor > stream.Length)
                //    return false;

                if (seekPosition > _indexEntry.AbsoluteByteOffset + ShapeFileConstants.ShapeRecordHeaderByteLength + _indexEntry.ByteLength)
                    return false;

                if (seekPosition < offsets.Bor || seekPosition > offsets.Eor)
                    return false;


                if (stream.Position == seekPosition)
                    return true;

                if (seekPosition == stream.Seek(seekPosition, SeekOrigin.Begin))
                    return true;

                return false;
            }



            private ShapeFileIndex.IndexEntry _indexEntry;
            private uint _oid;

            public StreamOffsetUtility(ShapeFileIndex.IndexEntry entry, uint oid)
            {
                _oid = oid;
                _indexEntry = entry;
            }

            public ShapeFileIndex.IndexEntry IndexEntry
            {
                get { return _indexEntry; }
            }

            public uint Oid
            {
                get { return _oid; }
            }

            public CoordinateValues GetValues(StreamOffset offset, BinaryReader reader)
            {
                Double x,
                       y,
                       z = ShapeFileProvider.NullDoubleValue,
                       m = ShapeFileProvider.NullDoubleValue;

                Stream s = reader.BaseStream;

                if (Seek(s, offset.X, offset))
                {
                    x = ByteEncoder.GetLittleEndian(reader.ReadDouble());
                }
                else
                {
                    throw new InvalidOperationException("Invalid stream position");
                }
                if (Seek(s, offset.Y, offset))
                {
                    y = ByteEncoder.GetLittleEndian(reader.ReadDouble());
                }
                else
                {
                    throw new InvalidOperationException("Invalid stream position");
                }
                if (offset.Z.HasValue && Seek(s, offset.Z.Value, offset))
                {
                    z = ByteEncoder.GetLittleEndian(reader.ReadDouble());
                }
                if (offset.M.HasValue && Seek(s, offset.M.Value, offset))
                {
                    m = ByteEncoder.GetLittleEndian(reader.ReadDouble());
                }
                return new CoordinateValues(x, y, z, m);
            }

            public StreamOffset CalculateOffsets(ShapeType shapeType, int numParts, int numPoints, int pointIndex)
            {
                long? bboxOffset = null, xOffset = null, yOffset = null, zOffset = null, mOffset = null, endOfRecord = null;

                long recordStart = _indexEntry.AbsoluteByteOffset + ShapeFileConstants.ShapeRecordHeaderByteLength;

                switch (shapeType)
                {
                    case ShapeType.Point:
                        {
                            xOffset = recordStart + 4L;
                            yOffset = recordStart + 12L;
                            endOfRecord = yOffset + 8L;
                            break;
                        }
                    case ShapeType.MultiPoint:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 40 + (pointIndex * 16);
                            yOffset = xOffset + 8;
                            endOfRecord = recordStart + 40 + (numPoints * 16);
                            break;
                        }
                    case ShapeType.PolyLine:
                    case ShapeType.Polygon:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 44 + (4 * numParts) + (pointIndex * 16);
                            yOffset = xOffset + 8;
                            endOfRecord = recordStart + 44 + (4 * numParts) + (16 * numPoints);
                            break;
                        }
                    case ShapeType.PointM:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 4;
                            yOffset = recordStart + 12;
                            mOffset = recordStart + 20;
                            endOfRecord = mOffset + 8;
                            break;
                        }
                    case ShapeType.MultiPointM:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 40 + (pointIndex * 16);
                            yOffset = xOffset + 8;
                            mOffset = recordStart + 40 + (numPoints * 16) + 16 + (pointIndex * 8);
                            endOfRecord = recordStart + 40 + (numPoints * 16) + (numPoints * 8);
                            break;
                        }
                    case ShapeType.PolyLineM:
                    case ShapeType.PolygonM:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 44 + (4 * numParts) + (pointIndex * 16);
                            yOffset = xOffset + 8;
                            mOffset = recordStart + 44 + (4 * numParts) + (16 * numPoints) + 16 +
                                     (pointIndex * 8);
                            endOfRecord = recordStart + 44 + (4 * numParts) + (16 * numPoints) + 16 +
                                     (numPoints * 8);

                            break;
                        }
                    case ShapeType.PointZ:
                        {
                            xOffset = recordStart + 4;
                            yOffset = recordStart + 12;
                            zOffset = recordStart + 20;
                            mOffset = recordStart + 28;
                            endOfRecord = mOffset + 8;
                            break;
                        }
                    case ShapeType.MultiPointZ:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 40 + (pointIndex * 16);
                            yOffset = xOffset + 8;
                            zOffset = recordStart + 40 + (16 * numPoints) + 16 + (pointIndex * 8);
                            mOffset = recordStart + 40 + (16 * numPoints) + 16 + (numPoints * 8) + 16 +
                                     (pointIndex * 8);
                            endOfRecord = recordStart + 40 + (16 * numPoints) + 16 + (numPoints * 8) + 16 +
                                     (numPoints * 8);
                            break;
                        }
                    case ShapeType.PolyLineZ:
                    case ShapeType.PolygonZ:
                        {
                            bboxOffset = recordStart + 4;
                            xOffset = recordStart + 44 + (4 * numParts) + (pointIndex * 16);
                            yOffset = xOffset + 8;
                            zOffset = recordStart + 44 + (4 * numParts) + (numPoints * 16) + 16 + (pointIndex * 8);
                            mOffset = recordStart + 44 + (4 * numParts) + (numPoints * 16) + 16 + (numPoints * 8) + 16 + (pointIndex * 8);
                            endOfRecord = recordStart + 44 + (4 * numParts) + (numPoints * 16) + 16 + (numPoints * 8) + 16 + (numPoints * 8);
                            break;
                        }
                }

                StreamOffset offsets = new StreamOffset(bboxOffset, xOffset, yOffset, zOffset, mOffset, recordStart, endOfRecord);

#if DEBUG
                //                Debug.WriteLine(string.Format("Point : {0} \nX : {1}\nY : {2}\nZ : {3}\nM : {4}\nEnd of Record : {5}\nIndex record start : {6}\nIndex Record End {7}", pointIndex, xOffset, yOffset, zOffset, mOffset, endOfRecord, recordStart, recordStart + _indexEntry.ByteLength));
#endif
                return offsets;
            }

            public void ValidateOffset(long offset, string action)
            {
                Debug.WriteLine(string.Format("{0} read offset : {1}", action, offset));

                Assert.IsTrue((offset >= IndexEntry.AbsoluteByteOffset &&
                               offset <= IndexEntry.AbsoluteByteOffset + IndexEntry.ByteLength));
            }

            public void ValidateOffset(long startIndex, long proposedReadLength, string action)
            {
                ValidateOffset(startIndex + proposedReadLength, action);
            }

            internal bool Seek(Stream stream, long seekPosition)
            {
                if (seekPosition < _indexEntry.AbsoluteByteOffset)
                    return false;

                if (seekPosition > _indexEntry.AbsoluteByteOffset + ShapeFileConstants.ShapeRecordHeaderByteLength + _indexEntry.ByteLength)
                    return false;

                if (stream.Position == seekPosition)
                    return true;

                if (seekPosition == stream.Seek(seekPosition, SeekOrigin.Begin))
                    return true;

                return false;
            }
        }



        private ShapeType moveToRecord(UInt32 oid, out StreamOffsetUtility offsetUtility)
        {
            ShapeFileIndex.IndexEntry entry = _shapeFileIndex[oid];

            offsetUtility = new StreamOffsetUtility(entry, oid);

            Int32 offset = entry.AbsoluteByteOffset;
            _shapeFileReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            UInt32 storedOid = ByteEncoder.GetBigEndian(_shapeFileReader.ReadUInt32());

            // Skip content length
            for (Int32 i = 0; i < ShapeFileConstants.ShapeRecordContentLengthByteLength; i++)
            {
                _shapeFileReader.ReadByte();
            }

            ShapeType recordType = (ShapeType)ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

            if (oid != storedOid)
            {
                throw new ShapeFileIsInvalidException("Record #" + oid + " is stored with #" + storedOid);
            }

            return recordType;
        }

        private ICoordinate readCoordinate()
        {
            Double x = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble());
            Double y = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble());

            ICoordinate coord = new Coordinate(x, y);

            //return CoordinateTransformation == null
            //           ? coord
            //           : CoordinateTransformation.MathTransform.Transform(coord);

            return coord;
        }

        private ICoordinate readCoordinate(StreamOffsetUtility offsetUtility, ShapeType shpType, int numShapeParts, int numShapePoints, int pointIndex)
        {
            StreamOffset offset = offsetUtility.CalculateOffsets(shpType, numShapeParts, numShapePoints, pointIndex);
            CoordinateValues values = offsetUtility.GetValues(offset, _shapeFileReader);
            ICoordinate coordinate = null;


            if (!offset.Z.HasValue && !offset.M.HasValue)
            {
                switch (ForceCoordinateOptions)
                {
                    case ForceCoordinateOptions.ForceNone:
                    case ForceCoordinateOptions.Force2D:
                        {
                            coordinate = new Coordinate(values.X, values.Y);
                            break;
                        }
                    case ForceCoordinateOptions.Force2DM:
                        {
                            throw new InvalidOperationException();
                        }
                    case ForceCoordinateOptions.Force3D:
                        {
                            coordinate = new Coordinate(values.X, values.Y, values.Z);
                            break;
                        }
                    case ForceCoordinateOptions.Force3DM:
                        {
                            throw new InvalidOperationException();
                        }
                }
            }
            else if (!offset.Z.HasValue)
            {
                switch (ForceCoordinateOptions)
                {
                    case ForceCoordinateOptions.ForceNone:
                    case ForceCoordinateOptions.Force2DM:
                        {

                            throw new InvalidOperationException();
                        }
                    case ForceCoordinateOptions.Force2D:
                        {
                            coordinate = new Coordinate(values.X, values.Y);
                            break;
                        }
                    case ForceCoordinateOptions.Force3D:
                        {
                            coordinate = new Coordinate(values.X, values.Y, values.Z);
                            break;
                        }
                    case ForceCoordinateOptions.Force3DM:
                        {
                            throw new InvalidOperationException();
                        }
                }
            }
            else
            {

                switch (ForceCoordinateOptions)
                {
                    case ForceCoordinateOptions.ForceNone:
                    case ForceCoordinateOptions.Force3DM:
                        {
                            throw new InvalidOperationException();
                        }
                    case ForceCoordinateOptions.Force3D:
                        {
                            coordinate = new Coordinate(values.X, values.Y, values.Z);
                            break;
                        }
                    case ForceCoordinateOptions.Force2DM:
                        {
                            throw new InvalidOperationException();
                        }
                    case ForceCoordinateOptions.Force2D:
                        {
                            coordinate = new Coordinate(values.X, values.Y);
                            break;
                        }
                }
            }


            if (!offsetUtility.Seek(_shapeFileReader.BaseStream, offset.Y + 8, offset))//return to the end of the x, y so any subsequent call to readCoordinate is positioned correctly

                throw new InvalidOperationException("Invalid Stream Position");


            //Assert.IsNotNull(coordinate);

            //return CoordinateTransformation == null
            //           ? coordinate
            //           : CoordinateTransformation.MathTransform.Transform(coordinate);

            return coordinate;
        }


        private IEnvelope readExtents(UInt32 oid)
        {
            StreamOffsetUtility offsetUtility;
            ShapeType recordType = moveToRecord(oid, out offsetUtility);

            // Null geometries encode deleted lines, so OIDs remain consistent
            if (recordType == ShapeType.Null)
            {
                return null;
            }

            if (recordType == ShapeType.Point ||
                recordType == ShapeType.PointM ||
                recordType == ShapeType.PointZ)
            {
                IPoint point = readPoint(offsetUtility, ShapeType.Point) as IPoint;
                return point == null ? new Envelope() : point.EnvelopeInternal;
            }

            offsetUtility.Seek(_shapeFileReader.BaseStream, offsetUtility.IndexEntry.AbsoluteByteOffset + ShapeFileConstants.ShapeRecordHeaderByteLength + 4);

            ICoordinate min = readCoordinate();
            ICoordinate max = readCoordinate();

            return new Envelope(min, max);
        }


        /// <summary>
        /// Reads and parses the geometry with ID 'oid' from the ShapeFile.
        /// </summary>
        /// <remarks>
        /// Filtering is not applied to this method.
        /// </remarks>
        /// <param name="oid">Object ID</param>
        /// <returns>
        /// <see cref="GeoAPI.Geometries.IGeometry"/> instance from the 
        /// ShapeFile corresponding to <paramref name="oid"/>.
        /// </returns>
        private IGeometry readGeometry(UInt32 oid)
        {
            StreamOffsetUtility offsetUtility;
            ShapeType recordType = moveToRecord(oid, out offsetUtility);

            // Null geometries encode deleted lines, so OIDs remain consistent
            if (recordType == ShapeType.Null)
            {
                return null;
            }

            IGeometry g;
            try
            {
                switch (ShapeType)
                {
                    case ShapeType.Point:
                    case ShapeType.PointM:
                    case ShapeType.PointZ:
                        g = readPoint(offsetUtility, ShapeType);
                        break;
                    case ShapeType.MultiPoint:
                    case ShapeType.MultiPointM:
                    case ShapeType.MultiPointZ:
                        g = readMultiPoint(offsetUtility, ShapeType);
                        break;
                    case ShapeType.PolyLine:
                    case ShapeType.PolyLineM:
                    case ShapeType.PolyLineZ:
                        g = readPolyLine(offsetUtility, ShapeType);
                        break;
                    case ShapeType.Polygon:
                    case ShapeType.PolygonM:
                    case ShapeType.PolygonZ:
                        g = readPolygon(offsetUtility, ShapeType);
                        break;
                    default:
                        throw new ShapeFileUnsupportedGeometryException("ShapeFile type " +
                                                                        ShapeType +
                                                                        " not supported");
                }
            }
            catch (Exception ex)
            {
                g = null;
                if (ReadStrictness == ShapeFileReadStrictness.Strict)
                    throw;
            }

            return g;
        }


        private IGeometry readPoint(StreamOffsetUtility offsetUtility, ShapeType shpType)
        {
            if (Array.IndexOf(PointTypes, shpType) == -1)
                throw new ArgumentException("shpType must be a point type");

            ICoordinate coord = readCoordinate(offsetUtility, shpType, 1, 1, 0);
            IPoint point = GeometryFactory.CreatePoint(coord);
            return point;
        }

        private IGeometry readMultiPoint(StreamOffsetUtility offsetUtility, ShapeType shpType)
        {
            if (Array.IndexOf(MultiPointTypes, shpType) == -1)
                throw new ArgumentException("shpType must be a multipoint type");

            // Skip min/max box
            _shapeFileReader.BaseStream.Seek(ShapeFileConstants.BoundingBoxFieldByteLength, SeekOrigin.Current);


            // Get the number of points
            Int32 pointCount = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

            if (pointCount == 0)
            {
                return null;
            }

            IList<IPoint> points = new List<IPoint>();

            for (Int32 i = 0; i < pointCount; i++)
            {
                ICoordinate coord = readCoordinate(offsetUtility, shpType, pointCount, pointCount, i);
                IPoint point = GeometryFactory.CreatePoint(coord);
                points.Add(point);
            }

            return GeometryFactory.CreateMultiPoint(points.ToArray());
        }


        private void readPolyStructure(out Int32 parts, out Int32 points, out Int32[] segments)
        {
            // Skip min/max box
            _shapeFileReader.BaseStream.Seek(ShapeFileConstants.BoundingBoxFieldByteLength, SeekOrigin.Current);

            // Get number of parts (segments)
            parts = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

            // Get number of points
            points = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

            segments = new Int32[parts + 1];

            // Read in the segment indexes
            for (Int32 b = 0; b < parts; b++)
            {
                segments[b] = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());
            }

            // Add end point
            segments[parts] = points;
        }

        private void readSegments(StreamOffsetUtility offsetUtility, ShapeType shpType, int numParts, int numPoints, int lineId, int[] segments,
                                  ref ICoordinateSequence coordinates)
        {
            List<ICoordinate> newCoordinates = new List<ICoordinate>(coordinates.ToCoordinateArray());
            for (Int32 i = segments[lineId]; i < segments[lineId + 1]; i++)
            {
                ICoordinate coord = readCoordinate(offsetUtility, shpType, numParts, numPoints, i);

                newCoordinates.Add(coord);
            }

            coordinates = GeometryFactory.CoordinateSequenceFactory.Create(newCoordinates.ToArray());

        }

        private IGeometry readPolyLine(StreamOffsetUtility offsetUtility, ShapeType shpType)
        {
            if (Array.IndexOf(PolyLineTypes, shpType) == -1)
                throw new ArgumentException("shpType must be a PolyLine Type");

            Int32 parts;
            Int32 points;
            Int32[] segments;
            readPolyStructure(out parts, out points, out segments);

            if (parts == 0)
            {
                throw new ShapeFileIsInvalidException("Polyline found with 0 parts.");
            }


            IList<ILineString> lines = new List<ILineString>();

            for (Int32 lineId = 0; lineId < parts; lineId++)
            {
                ICoordinateSequence coordinates =
                    GeometryFactory.CoordinateSequenceFactory.Create(0, shpType == ShapeType.PolyLineZ
                                                                         ? 3
                                                                         : 2);

                readSegments(offsetUtility, shpType, parts, points, lineId, segments, ref coordinates);

                ILineString line = GeometryFactory.CreateLineString(coordinates);

                lines.Add(line);
            }

            if (lines.Count == 1)
            {
                return lines[0];
            }

            return GeometryFactory.CreateMultiLineString(lines.ToArray());
        }


        private IGeometry readPolygon(StreamOffsetUtility offsetUtility, ShapeType shpType)
        {
            if (Array.IndexOf(PolygonTypes, shpType) == -1)
                throw new ArgumentException("shpType must be a polygon type");

            //shpType = ShapeType.Polygon;//temp

            Int32 parts;
            Int32 points;
            Int32[] segments;
            readPolyStructure(out parts, out points, out segments);

            if (parts == 0)
            {
                throw new ShapeFileIsInvalidException("Polygon found with 0 parts.");
            }

            // First read all the rings
            ILinearRing[] rings = new ILinearRing[parts];

            for (Int32 ringId = 0; ringId < parts; ringId++)
            {
                ICoordinateSequence coordinates =
                    GeometryFactory.CoordinateSequenceFactory.Create(0, shpType == ShapeType.PolygonZ
                                                                         ? 3
                                                                         : 2);

                readSegments(offsetUtility, shpType, parts, points, ringId, segments, ref coordinates);

                ILinearRing ring = GeometryFactory.CreateLinearRing(coordinates);
                rings[ringId] = ring;
            }

            Int32 polygonCount = 0;

            List<ILinearRing> shells = new List<ILinearRing>();
            List<ILinearRing> holes = new List<ILinearRing>();

            for (Int32 i = 0; i < parts; i++)
            {
                if (!CGAlgorithms.IsCCW(rings[i].Coordinates))
                {
                    polygonCount++;
                    shells.Add(rings[i]);
                }
                else
                {
                    holes.Add(rings[i]);
                }
            }

            if (polygonCount == 0
                && holes.Count > 0
                && ReadStrictness == ShapeFileReadStrictness.Lenient)
            {
                ///attempt to fix bad record as per GeoTools
                shells = holes;
                holes = new List<ILinearRing>();
                for (int i = 0; i < shells.Count; i++)
                    shells[i] = GeometryFactory.CreateLinearRing(shells[i].Coordinates.Reverse().ToArray());
                polygonCount = shells.Count;
            }

            Assert.IsTrue(polygonCount != 0);

            List<IPolygon> polygons = new List<IPolygon>();

            if (shells.Count == 1)
            {
                if (holes.Count == 0)
                    polygons.Add(GeometryFactory.CreatePolygon(shells[0], new ILinearRing[] { }));
                else
                    polygons.Add(GeometryFactory.CreatePolygon(shells[0], holes.ToArray()));
            }
            else
            {
                foreach (ILinearRing ring in shells)
                {
                    if (holes.Count > 0)
                    {
                        List<ILinearRing> localHoles = new List<ILinearRing>();

                        IPolygon bounds = GeometryFactory.CreatePolygon(ring, new ILinearRing[] { });
                        //unfortunately we need to build a temp shell to test contains will add processing overhead

                        for (int i = holes.Count - 1; i > -1; i--)
                        {
                            if (bounds.Contains(holes[i]))
                            {
                                localHoles.Add(holes[i]);
                                holes.RemoveAt(i);
                            }
                        }
                        polygons.Add(GeometryFactory.CreatePolygon(ring, localHoles.ToArray()));
                    }
                    else
                        polygons.Add(GeometryFactory.CreatePolygon(ring, new ILinearRing[] { }));
                }

                if (holes.Count > 0
                    && ReadStrictness == ShapeFileReadStrictness.Lenient)
                {
                    ///attempt to fix the record by turning hole into shell
                    for (int i = holes.Count - 1; i > -1; i--)
                    {
                        polygons.Add(
                            GeometryFactory.CreatePolygon(GeometryFactory.CreateLinearRing(holes[i].Coordinates.Reverse().ToArray()), new ILinearRing[] { }));
                        holes.RemoveAt(i);
                    }
                }

                Debug.Assert(holes.Count == 0);
            }

            return GeometryFactory.CreateMultiPolygon(polygons.ToArray());
        }

        #endregion

        #region File parsing helpers

        ///// <summary>
        ///// Reads and parses the projection if a projection file exists
        ///// </summary>
        //private void parseProjection()
        //{
        //    String projfile = Path.Combine(Path.GetDirectoryName(Filename),
        //                                   Path.GetFileNameWithoutExtension(Filename) + ".prj");

        //    if (StorageManager.FileExists(projfile))
        //    {
        //        if (_coordSysFactory == null)
        //        {
        //            throw new InvalidOperationException("A projection is defined for this shapefile," +
        //                                                " but no CoordinateSystemFactory was set.");
        //        }

        //        try
        //        {
        //            String wkt = StorageManager.ReadAllText(projfile);
        //            ICoordinateSystem coordinateSystem = _coordSysFactory.CreateFromWkt(wkt);
        //            initSpatialReference(coordinateSystem);
        //            _coordsysReadFromFile = true;
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            Trace.WriteLine("Coordinate system file '" + projfile +
        //                          "' found, but could not be parsed. " +
        //                          "WKT parser returned:" + ex.Message);

        //            throw new ShapeFileIsInvalidException("Invalid .prj file", ex);
        //        }
        //    }
        //}

        #endregion

        #region File writing helper functions

        //private void writeFeatureRow(FeatureDataRow feature)
        //{
        //    UInt32 recordNumber = addIndexEntry(feature);

        //    if (HasDbf)
        //    {
        //        _dbaseWriter.AddRow(feature);
        //    }

        //    writeGeometry(feature.Geometry, recordNumber, _shapeIndex[recordNumber].Length);
        //}


        //private void writeGeometry(IGeometry g, UInt32 recordNumber, Int32 recordOffsetInWords,
        //                           Int32 recordLengthInWords)
        //{
        //    Debug.Assert(recordNumber > 0);

        //    _shapeFileStream.Position = recordOffsetInWords * 2;

        //    // Record numbers are 1- based in shapefile
        //    // recordNumber += 1;

        //    _shapeFileWriter.Write(ByteEncoder.GetBigEndian(recordNumber));
        //    _shapeFileWriter.Write(ByteEncoder.GetBigEndian(recordLengthInWords));

        //    if (g == null)
        //    {
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)ShapeType.Null));
        //    }

        //    switch (ShapeType)
        //    {
        //        case ShapeType.Point:
        //        case ShapeType.PointM:
        //        case ShapeType.PointZ:
        //            writePoint(g as IPoint, ShapeType);
        //            break;
        //        case ShapeType.PolyLine:
        //        case ShapeType.PolyLineM:
        //        case ShapeType.PolyLineZ:
        //            if (g is ILineString)
        //            {
        //                writeLineString(g as ILineString, ShapeType);
        //            }
        //            else if (g is IMultiLineString)
        //            {
        //                writeMultiLineString(g as IMultiLineString, ShapeType);
        //            }
        //            break;
        //        //case ShapeType.Polygon:
        //        //    writePolygon(g as IPolygon);
        //        //    break;

        //        case ShapeType.Polygon:
        //        case ShapeType.PolygonM:
        //        case ShapeType.PolygonZ:
        //            if (g is IPolygon)
        //                writePolygon(g as IPolygon, ShapeType);
        //            else if (g is IMultiPolygon)
        //                writeMultiPolygon(g as IMultiPolygon, ShapeType);
        //            break;
        //        case ShapeType.MultiPoint:
        //        case ShapeType.MultiPointM:
        //        case ShapeType.MultiPointZ:
        //            writeMultiPoint(g as IMultiPoint, ShapeType);
        //            break;

        //        case ShapeType.MultiPatch:
        //        case ShapeType.Null:
        //        default:
        //            throw new NotSupportedException(String.Format(
        //                                                "Writing geometry type {0} " +
        //                                                "is not supported in the " +
        //                                                "current version.",
        //                                                ShapeType));
        //    }

        //    _shapeFileWriter.Flush();
        //}

        //private void writeCoordinate(Double x, Double y)
        //{
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(x));
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(y));
        //}

        //private void writePoint(IPoint point, ShapeType shpType)
        //{
        //    if (Array.IndexOf(PointTypes, shpType) == -1)
        //        throw new ArgumentException("shpType must be a Point type");

        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)shpType));
        //    writeCoordinate(point[Ordinates.X], point[Ordinates.Y]);

        //    if (shpType == ShapeType.PointZ)
        //    {
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //            point.Coordinate.ContainsOrdinate(Ordinates.Z)
        //            ? point[Ordinates.Z]
        //            : 0.0));
        //    }
        //    if (shpType == ShapeType.PointZ || shpType == ShapeType.PointM)
        //    {
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //            point.Coordinate.ContainsOrdinate(Ordinates.M) && !IsShapefileNullValue(point[Ordinates.M])
        //            ? point[Ordinates.M]
        //            : NullDoubleValue));
        //    }


        //}

        //private void writeBoundingBox(IEnvelope box)
        //{
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.GetMin(Ordinates.X)));
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.GetMin(Ordinates.Y)));
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.GetMax(Ordinates.X)));
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.GetMax(Ordinates.Y)));
        //}

        private static Tuple<double, double> GetOrdinateRange(IGeometry geometry, Ordinates ordinate)
        {
            double min = double.MaxValue;
            double max = double.MinValue;


            if (geometry is IGeometryCollection)
            {
                Tuple<double, double> rv = GetOrdinateRange(geometry as IEnumerable<IGeometry>, ordinate);
                min = Math.Min(min, rv.Item1);
                max = Math.Max(max, rv.Item2);
            }
            else
            {
                foreach (ICoordinate c in geometry.Coordinates)
                {
                    if (c.ContainsOrdinate(ordinate))
                    {
                        min = Math.Min(min, c.GetOrdinate(ordinate));
                        max = Math.Max(max, c.GetOrdinate(ordinate));
                    }
                }
            }

            return Tuple.Create(min, max);
        }

        private static Tuple<double, double> GetOrdinateRange(IEnumerable<IGeometry> geometries, Ordinates ordinate)
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (IGeometry g in geometries)
            {
                Tuple<Double, Double> rv = GetOrdinateRange(g, ordinate);
                min = Math.Min(min, rv.Item1);
                max = Math.Max(max, rv.Item2);
            }

            return Tuple.Create(min, max);
        }

        //private void writeMultiPoint(IMultiPoint multiPoint, ShapeType shpType)
        //{
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)shpType));
        //    writeBoundingBox(multiPoint.Extents);
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(multiPoint.Count));

        //    foreach (IPoint point in ((IEnumerable<IPoint>)multiPoint))
        //    {
        //        writeCoordinate(point[Ordinates.X], point[Ordinates.Y]);
        //    }

        //    if (shpType == ShapeType.MultiPointZ)
        //    {
        //        Pair<double> zrng = GetOrdinateRange((IEnumerable<IGeometry>)multiPoint, Ordinates.Z);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? 0.0
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? 0.0
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in multiPoint.Coordinates)
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.Z) && !IsShapefileNullValue(c[Ordinates.Z])
        //                ? c[Ordinates.Z]
        //                : 0.0
        //                ));
        //        }
        //    }

        //    if (shpType == ShapeType.MultiPointZ || shpType == ShapeType.MultiPointM)
        //    {
        //        Pair<double> mrng = GetOrdinateRange((IEnumerable<IGeometry>)multiPoint, Ordinates.M);

        //        double lowerBound = mrng.First == Double.MaxValue || IsShapefileNullValue(mrng.First)
        //                                ? NullDoubleValue
        //                                : mrng.First;

        //        double upperBound = mrng.Second == Double.MinValue || IsShapefileNullValue(mrng.Second)
        //                                ? NullDoubleValue
        //                                : mrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in multiPoint.Coordinates)
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.M) && !IsShapefileNullValue(c[Ordinates.M])
        //                ? c[Ordinates.M]
        //                : NullDoubleValue
        //                ));
        //        }
        //    }

        //}

        //private void writePolySegments(IExtents extents, Int32[] parts, IEnumerable points, Int32 pointCount)
        //{
        //    writeBoundingBox(extents);
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(parts.Length));
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(pointCount));

        //    foreach (Int32 partIndex in parts)
        //    {
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(partIndex));
        //    }

        //    foreach (ICoordinate point in points)
        //    {
        //        writeCoordinate(point[Ordinates.X], point[Ordinates.Y]);
        //    }
        //}

        //private void writeLineString(ILineString lineString, ShapeType shpType)
        //{
        //    if (Array.IndexOf(PolyLineTypes, shpType) == -1)
        //        throw new ArgumentException("shpType must be a Polyline type");

        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)shpType));

        //    writePolySegments(lineString.Extents,
        //                      new[] { 0 },
        //                      lineString.Coordinates,
        //                      lineString.Coordinates.Count);

        //    if (shpType == ShapeType.PolyLineZ)
        //    {
        //        Pair<double> zrng = GetOrdinateRange(lineString, Ordinates.Z);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? 0.0
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? 0.0
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in lineString.Coordinates)
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.Z) && !IsShapefileNullValue(c[Ordinates.Z])
        //                ? c[Ordinates.Z]
        //                : 0.0
        //                ));
        //        }
        //    }

        //    if (shpType == ShapeType.PolyLineZ || shpType == ShapeType.PolyLineM)
        //    {
        //        Pair<double> zrng = GetOrdinateRange(lineString, Ordinates.M);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? NullDoubleValue
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? NullDoubleValue
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in lineString.Coordinates)
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.M) && !IsShapefileNullValue(c[Ordinates.M])
        //                ? c[Ordinates.M]
        //                : NullDoubleValue
        //                ));
        //        }
        //    }
        //}

        //private void writeMultiLineString(IMultiLineString multiLineString, ShapeType shpType)
        //{
        //    if (Array.IndexOf(PolyLineTypes, shpType) == -1)
        //        throw new ArgumentException("shpType must be a Polyline type");

        //    Int32[] parts = new Int32[multiLineString.Count];
        //    ArrayList allPoints = new ArrayList();

        //    Int32 currentPartsIndex = 0;

        //    foreach (ILineString line in (IEnumerable<ILineString>)multiLineString)
        //    {
        //        parts[currentPartsIndex++] = allPoints.Count;
        //        allPoints.AddRange(line.Coordinates);
        //    }

        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)shpType));
        //    writePolySegments(multiLineString.Extents, parts, allPoints, allPoints.Count);

        //    if (shpType == ShapeType.PolyLineZ)
        //    {
        //        Pair<double> zrng = GetOrdinateRange((IEnumerable<IGeometry>)multiLineString, Ordinates.Z);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? 0.0
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? 0.0
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in multiLineString.Coordinates)
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.Z) && !IsShapefileNullValue(c[Ordinates.Z])
        //                ? c[Ordinates.Z]
        //                : 0.0
        //                ));
        //        }
        //    }

        //    if (shpType == ShapeType.PolyLineZ || shpType == ShapeType.PolyLineM)
        //    {
        //        Pair<double> zrng = GetOrdinateRange((IEnumerable<IGeometry>)multiLineString, Ordinates.M);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? NullDoubleValue
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? NullDoubleValue
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in multiLineString.Coordinates)
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.M) && !IsShapefileNullValue(c[Ordinates.M])
        //                ? c[Ordinates.M]
        //                : NullDoubleValue
        //                ));
        //        }
        //    }

        //}

        ///// <summary>
        ///// write the polygon to the shapefilewriter
        ///// </summary>
        ///// <param name="polygon">polygon to be written</param>
        //private void writePolygon(IPolygon polygon, ShapeType shpType)
        //{
        //    if (Array.IndexOf(PolygonTypes, shpType) == -1)
        //        throw new ArgumentException("shpType must be a Polygon type");

        //    Int32[] parts = new Int32[polygon.InteriorRingsCount + 1];
        //    Int32 currentPartsIndex = 0, pointCnt = 0;
        //    parts[currentPartsIndex++] = 0;

        //    /* need to account for cases where the polygon shell is not closed. */
        //    if (
        //        !polygon.ExteriorRing.Coordinates[0].Equals(
        //             polygon.ExteriorRing.Coordinates[polygon.ExteriorRing.Coordinates.Count - 1]))
        //        polygon.ExteriorRing.Coordinates.Add(polygon.ExteriorRing.Coordinates[0]);

        //    pointCnt = polygon.ExteriorRing.Coordinates.Count;

        //    foreach (ILinearRing ring in polygon.InteriorRings)
        //    {
        //        parts[currentPartsIndex++] = pointCnt;

        //        /* need to account for cases where the hole is not closed. */
        //        if (!ring.Coordinates[0].Equals(ring.Coordinates[ring.Coordinates.Count - 1]))
        //            ring.Coordinates.Add(ring.Coordinates[0]);

        //        pointCnt += ring.Coordinates.Count;
        //    }
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)shpType));
        //    //Write the extents, part count, part index information, and point count
        //    writePolygonInformation(polygon.Extents, parts, pointCnt);

        //    //Now write the actual point data
        //    writePolygonCoordinates(polygon);


        //    if (shpType == ShapeType.PolygonZ)
        //    {
        //        Pair<double> zrng = GetOrdinateRange(polygon, Ordinates.Z);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? 0.0
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? 0.0
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in getOrderedPolygonCoordinates(polygon))
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.Z) && !IsShapefileNullValue(c[Ordinates.Z])
        //                ? c[Ordinates.Z]
        //                : 0.0
        //                ));
        //        }
        //    }

        //    if (shpType == ShapeType.PolygonZ || shpType == ShapeType.PolygonM)
        //    {
        //        Pair<double> zrng = GetOrdinateRange(polygon, Ordinates.M);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? NullDoubleValue
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? NullDoubleValue
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in getOrderedPolygonCoordinates(polygon))
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.M) && !IsShapefileNullValue(c[Ordinates.M])
        //                ? c[Ordinates.M]
        //                : NullDoubleValue
        //                ));
        //        }
        //    }

        //}

        ///// <summary>
        ///// write the multipolygon to the shapefilewriter
        ///// </summary>
        ///// <param name="mpoly">multipolygon to be written</param>
        //private void writeMultiPolygon(IMultiPolygon mpoly, ShapeType shpType)
        //{
        //    if (Array.IndexOf(PolygonTypes, shpType) == -1)
        //        throw new ArgumentException("shpType must be a Polygon type");

        //    List<Int32> parts = new List<int>();
        //    Int32 pointIndex = 0;


        //    foreach (IPolygon poly in (mpoly as IEnumerable<IPolygon>))
        //    {
        //        /* need to account for cases where the polygon shell is not closed. */
        //        if (
        //            !poly.ExteriorRing.Coordinates[0].Equals(
        //                 poly.ExteriorRing.Coordinates[poly.ExteriorRing.Coordinates.Count - 1]))
        //            poly.ExteriorRing.Coordinates.Add(poly.ExteriorRing.Coordinates[0]);

        //        parts.Add(pointIndex);
        //        pointIndex += poly.ExteriorRing.Coordinates.Count;

        //        foreach (ILinearRing ring in poly.InteriorRings)
        //        {
        //            /* need to account for cases where the polygon is not closed. */
        //            if (!ring.Coordinates[0].Equals(ring.Coordinates[ring.Coordinates.Count - 1]))
        //                ring.Coordinates.Add(ring.Coordinates[0]);

        //            parts.Add(pointIndex);
        //            pointIndex += ring.Coordinates.Count;
        //        }
        //    }
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian((Int32)shpType));
        //    //Write the extents, part count, part index information, and point count
        //    writePolygonInformation(mpoly.Extents, parts.ToArray(), pointIndex);

        //    //Now write the actual point data
        //    foreach (IPolygon poly in (mpoly as IEnumerable<IPolygon>))
        //        writePolygonCoordinates(poly);


        //    if (shpType == ShapeType.PolygonZ)
        //    {
        //        Pair<double> zrng = GetOrdinateRange((IEnumerable<IGeometry>)mpoly, Ordinates.Z);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? 0.0
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? 0.0
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in getOrderedMultiPolygonCoordinates(mpoly))
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.Z) && !IsShapefileNullValue(c[Ordinates.Z])
        //                ? c[Ordinates.Z]
        //                : 0.0
        //                ));
        //        }
        //    }

        //    if (shpType == ShapeType.PolygonZ || shpType == ShapeType.PolygonM)
        //    {
        //        Pair<double> zrng = GetOrdinateRange((IEnumerable<IGeometry>)mpoly, Ordinates.M);

        //        double lowerBound = zrng.First == Double.MaxValue || IsShapefileNullValue(zrng.First)
        //                                ? NullDoubleValue
        //                                : zrng.First;

        //        double upperBound = zrng.Second == Double.MinValue || IsShapefileNullValue(zrng.Second)
        //                                ? NullDoubleValue
        //                                : zrng.Second;
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(lowerBound));
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(upperBound));

        //        foreach (ICoordinate c in getOrderedMultiPolygonCoordinates(mpoly))
        //        {
        //            _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(
        //                c.ContainsOrdinate(Ordinates.M) && !IsShapefileNullValue(c[Ordinates.M])
        //                ? c[Ordinates.M]
        //                : NullDoubleValue
        //                ));
        //        }
        //    }
        //}

        ///// <summary>
        ///// write the coordinates of a polygon to the shapefilewriter
        ///// </summary>
        ///// <param name="polygon">polygon who's coordinates are to be written out</param>
        //private void writePolygonCoordinates(IPolygon polygon)
        //{
        //    foreach (ICoordinate c in getOrderedPolygonCoordinates(polygon))
        //        writeCoordinate(c[Ordinates.X], c[Ordinates.Y]);
        //}

        private IEnumerable<ICoordinate> getOrderedPolygonCoordinates(IPolygon poly)
        {
            foreach (ICoordinate coordinate in poly.ExteriorRing.Coordinates)
                yield return coordinate;

            foreach (ILinearRing hole in poly.InteriorRings)
                foreach (ICoordinate coordinate in (CGAlgorithms.IsCCW(hole.Coordinates) ? hole.Coordinates : hole.Coordinates.Reverse().ToArray()))
                    yield return coordinate;
        }

        private IEnumerable<ICoordinate> getOrderedMultiPolygonCoordinates(IMultiPolygon polygons)
        {
            foreach (IPolygon p in (IEnumerable<IPolygon>)polygons)
                foreach (ICoordinate c in getOrderedPolygonCoordinates(p))
                    yield return c;
        }

        /// <summary>
        ///// Write the given information to the shapefilewriter
        ///// </summary>
        ///// <param name="extents">extents to be written</param>
        ///// <param name="parts">array of point indexes where the parts are to start</param>
        ///// <param name="pointCnt">total point count</param>
        //private void writePolygonInformation(IExtents extents, int[] parts, int pointCnt)
        //{

        //    writeBoundingBox(extents);
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(parts.Length));
        //    _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(pointCnt));

        //    foreach (Int32 partIndex in parts)
        //    {
        //        _shapeFileWriter.Write(ByteEncoder.GetLittleEndian(partIndex));
        //    }
        //}

        #endregion

        //#region IWritableFeatureProvider<uint> Members

        //public void Insert(FeatureDataRow feature)
        //{
        //    Insert((FeatureDataRow<UInt32>)feature);
        //}

        //public void Insert(IEnumerable<FeatureDataRow> features)
        //{
        //    Insert(Caster.Downcast<FeatureDataRow<UInt32>, FeatureDataRow>(features));
        //}

        //public void Update(FeatureDataRow feature)
        //{
        //    Update((FeatureDataRow<UInt32>)feature);
        //}

        //public void Update(IEnumerable<FeatureDataRow> features)
        //{
        //    Update(Caster.Downcast<FeatureDataRow<UInt32>, FeatureDataRow>(features));
        //}

        //public void Delete(FeatureDataRow feature)
        //{
        //    Delete((FeatureDataRow<UInt32>)feature);
        //}

        //public void Delete(IEnumerable<FeatureDataRow> features)
        //{
        //    Delete(Caster.Downcast<FeatureDataRow<UInt32>, FeatureDataRow>(features));
        //}

        //#endregion
    }
}