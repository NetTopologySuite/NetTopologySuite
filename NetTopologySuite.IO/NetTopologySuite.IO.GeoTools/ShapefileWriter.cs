using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Handlers;
using NetTopologySuite.IO.Streams;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// This class writes ESRI Shapefiles.
    /// </summary>
    public class ShapefileWriter : IDisposable
    {
        public IGeometryFactory Factory { get; set; }

        private Stream _shpStream;
        private BigEndianBinaryWriter _shpBinaryWriter;
        private Stream _shxStream;
        private BigEndianBinaryWriter _shxBinaryWriter;

        private Envelope _totalEnvelope;

        private readonly ShapeHandler _shapeHandler;
        private readonly ShapeGeometryType _geometryType;

        private int _numFeaturesWritten;

        /// <summary>
        /// Initializes a buffered writer where you can write shapes individually to the file.
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <param name="geomType">The geometry type</param>
        public ShapefileWriter(string filename, ShapeGeometryType geomType)
            : this(GeometryFactory.Default, filename, geomType)
        {
        }

        public ShapefileWriter(IGeometryFactory geometryFactory, string filename, ShapeGeometryType geomType)
            : this(geometryFactory, new ShapefileStreamProviderRegistry(filename, false, false, false), geomType)
        {

        }

        public ShapefileWriter(IGeometryFactory geometryFactory, IStreamProviderRegistry streamProviderRegistry,
            ShapeGeometryType geomType)
            : this(geometryFactory)
        {


            _shpStream = streamProviderRegistry[StreamTypes.Shape].OpenWrite(true);
            _shxStream = streamProviderRegistry[StreamTypes.Index].OpenWrite(true);

            _geometryType = geomType;

            _shpBinaryWriter = new BigEndianBinaryWriter(_shpStream);
            _shxBinaryWriter = new BigEndianBinaryWriter(_shxStream);

            WriteShpHeader(_shpBinaryWriter, 0, new Envelope(0, 0, 0, 0));
            WriteShxHeader(_shxBinaryWriter, 0, new Envelope(0, 0, 0, 0));

            _shapeHandler = Shapefile.GetShapeHandler(geomType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapefileWriter" /> class
        /// with the given <see cref="GeometryFactory" />.
        /// </summary>
        /// <param name="geometryFactory"></param>
        public ShapefileWriter(IGeometryFactory geometryFactory)
        {
            Factory = geometryFactory;
        }

        public void Close()
        {
            if (_shpBinaryWriter != null)
            {
                /*Update header to reflect the data written*/
                _shpStream.Seek(0, SeekOrigin.Begin);
                _shxStream.Seek(0, SeekOrigin.Begin);

                var shpLenWords = (int) _shpBinaryWriter.BaseStream.Length/2;
                var shxLenWords = (int) _shxBinaryWriter.BaseStream.Length/2;

                WriteShpHeader(_shpBinaryWriter, shpLenWords, _totalEnvelope);
                WriteShxHeader(_shxBinaryWriter, shxLenWords, _totalEnvelope);

                _shpStream.Seek(0, SeekOrigin.End);
                _shxStream.Seek(0, SeekOrigin.End);
            }

            if (_shpBinaryWriter != null)
            {
                _shpBinaryWriter.Close();
                _shpBinaryWriter = null;
            }
            if (_shxBinaryWriter != null)
            {
                _shxBinaryWriter.Close();
                _shxBinaryWriter = null;
            }
            if (_shpStream != null)
            {
                _shpStream.Close();
                _shpStream = null;
            }

            if (_shxStream != null)
            {
                _shxStream.Close();
                _shxStream = null;
            }
        }

        /// <summary>
        /// Adds a shape to the shapefile. You must have used the constrcutor with a filename to use this method!
        /// </summary>
        /// <param name="geometry"></param>
        public void Write(IGeometry geometry)
        {
            if (_shpBinaryWriter == null)
                throw new NotSupportedException("Writing not started, use the Constructor with a filename!");

            WriteRecordToFile(_shpBinaryWriter, _shxBinaryWriter, _shapeHandler, geometry, _numFeaturesWritten + 1);
            _numFeaturesWritten++;

            var env = geometry.EnvelopeInternal;
            var bounds = ShapeHandler.GetEnvelopeExternal(geometry.PrecisionModel, env);
            if (_totalEnvelope == null)
            {
                _totalEnvelope = bounds;
            }
            else
            {
                _totalEnvelope.ExpandToInclude(bounds);
            }
        }

        /// <summary>
        /// Method to write a collection of <see cref="IGeometry"/>s to a file named <paramref name="filename"/>
        /// </summary>
        /// <remarks>
        /// Assumes the type given for the first geometry is the same for all subsequent geometries.
        /// For example, is, if the first Geometry is a Multi-polygon/ Polygon, the subsequent geometies are
        /// Muli-polygon/ polygon and not lines or points.
        /// The dbase file for the corresponding shapefile contains one column called row. It contains 
        /// the row number.
        /// </remarks>
        /// <param name="filename">The name of the file</param>
        /// <param name="geometryCollection">The collection of geometries</param>
        /// <param name="writeDummyDbf">Set to true to create a dummy dbf along with the shp file</param>
        [Obsolete("use WriteGeometryCollection")]
        public void Write(string filename, IGeometryCollection geometryCollection, bool writeDummyDbf = true)
        {
            WriteGeometryCollection(filename, geometryCollection, writeDummyDbf);
        }


        /// <summary>
        /// Method to write a collection of geometries to a shapefile on disk.
        /// </summary>
        /// <remarks>
        /// Assumes the type given for the first geometry is the same for all subsequent geometries.
        /// For example, is, if the first Geometry is a Multi-polygon/ Polygon, the subsequent geometies are
        /// Muli-polygon/ polygon and not lines or points.
        /// The dbase file for the corresponding shapefile contains one column called row. It contains 
        /// the row number.
        /// </remarks>
        /// <param name="filename">The filename to write to (minus the .shp extension).</param>
        /// <param name="geometryCollection">The GeometryCollection to write.</param>
        /// <param name="writeDummyDbf">Set to true to create an empty DBF-file along with the shp-file</param>		
        public static void WriteGeometryCollection(string filename, IGeometryCollection geometryCollection,
            bool writeDummyDbf = true)
        {
            WriteGeometryCollection(new ShapefileStreamProviderRegistry(filename, false, false, false),
                geometryCollection, writeDummyDbf);

        }

        public static void WriteGeometryCollection(IStreamProviderRegistry streamProviderRegistry,
            IGeometryCollection geometryCollection, bool createDummyDbf = true)
        {
            var shapeFileType = Shapefile.GetShapeType(geometryCollection);
            using (var writer = new ShapefileWriter(geometryCollection.Factory, streamProviderRegistry, shapeFileType))
            {
                var dbfWriter = createDummyDbf ? new DbaseFileWriter(streamProviderRegistry) : null;
                WriteGeometryCollection(writer, dbfWriter, geometryCollection, createDummyDbf);
                if (dbfWriter != null)
                    dbfWriter.Dispose();
            }
        }

        public static void WriteGeometryCollection(ShapefileWriter shapefileWriter, DbaseFileWriter dbfWriter,
            IGeometryCollection geometryCollection, bool writeDummyDbf = true)
        {
            var numShapes = geometryCollection.NumGeometries;
            for (var i = 0; i < numShapes; i++)
            {
                shapefileWriter.Write(geometryCollection[i]);
            }

            if (writeDummyDbf)
            {
                WriteDummyDbf(dbfWriter, numShapes);
            }

        }

        private static void WriteNullShapeRecord(BigEndianBinaryWriter shpBinaryWriter,
            BigEndianBinaryWriter shxBinaryWriter, int oid)
        {
            const int recordLength = 12;

            // Add shape
            shpBinaryWriter.WriteIntBE(oid);
            shpBinaryWriter.WriteIntBE(recordLength);
            shpBinaryWriter.Write((int) ShapeGeometryType.NullShape);

            // Update shapefile index (position in words, 1 word = 2 bytes)
            var posWords = shpBinaryWriter.BaseStream.Position/2;
            shxBinaryWriter.WriteIntBE((int) posWords);
            shxBinaryWriter.WriteIntBE(recordLength);

        }


        private static /*int*/ void WriteRecordToFile(BigEndianBinaryWriter shpBinaryWriter,
            BigEndianBinaryWriter shxBinaryWriter, ShapeHandler handler, IGeometry body, int oid)
        {
            if (body == null || body.IsEmpty)
            {
                WriteNullShapeRecord(shpBinaryWriter, shxBinaryWriter, oid);
                return;
            }

            // Get the length of each record (in bytes)
            var recordLength = handler.ComputeRequiredLengthInWords(body);

            // Get the position in the stream
            var pos = shpBinaryWriter.BaseStream.Position;
            shpBinaryWriter.WriteIntBE(oid);
            shpBinaryWriter.WriteIntBE(recordLength);

            // update shapefile index (position in words, 1 word = 2 bytes)
            var posWords = pos/2;
            shxBinaryWriter.WriteIntBE((int) posWords);
            shxBinaryWriter.WriteIntBE(recordLength);

            handler.Write(body, shpBinaryWriter, body.Factory);
            /*return recordLength;*/
        }

        private Envelope NotNull(Envelope bounds)
        {
            return bounds ?? new Envelope();
        }

        private void WriteShxHeader(BigEndianBinaryWriter shxBinaryWriter, int shxLength, Envelope bounds)
        {
            // write the .shx header
            var shxHeader = new ShapefileHeader
            {
                FileLength = shxLength,
                Bounds = NotNull(bounds),
                ShapeType = _geometryType
            };

            // assumes Geometry type of the first item will the same for all other items in the collection.
            shxHeader.Write(shxBinaryWriter);
        }

        private void WriteShpHeader(BigEndianBinaryWriter shpBinaryWriter, int shpLength, Envelope bounds)
        {
            var shpHeader = new ShapefileHeader
            {
                FileLength = shpLength,
                Bounds = NotNull(bounds),
                ShapeType = _geometryType
            };

            // assumes Geometry type of the first item will the same for all other items
            // in the collection.
            shpHeader.Write(shpBinaryWriter);
        }

        /// <summary>
        /// Method to write a dummy dbf file
        /// </summary>
        /// <param name="filename">The dbase filename</param>
        /// <param name="recordCount">The number of records</param>
        public static void WriteDummyDbf(string filename, int recordCount)
        {
            // assert the filename is correct
            filename = Path.ChangeExtension(filename, "dbf");

            using (var dbfWriter = new DbaseFileWriter(filename))
            {
                WriteDummyDbf(dbfWriter, recordCount);
            }
        }

        /// <summary>
        /// Method to write a dummy dbase file
        /// </summary>
        /// <param name="streamProviderRegistry">The stream provider registry</param>
        /// <param name="recordCount">The number of records</param>
        public static void WriteDummyDbf(IStreamProviderRegistry streamProviderRegistry, int recordCount)
        {
            using (var dbfWriter = new DbaseFileWriter(streamProviderRegistry))
            {
                WriteDummyDbf(dbfWriter, recordCount);
            }
        }

        /// <summary>
        /// Method to write a dummy dbase file
        /// </summary>
        /// <param name="dbfWriter">The dbase file writer</param>
        /// <param name="recordCount">The number of records</param>
        public static void WriteDummyDbf(DbaseFileWriter dbfWriter, int recordCount)
        {
            // Create the dummy header
            var dbfHeader = new DbaseFileHeader {NumRecords = recordCount};
            // add some dummy column
            dbfHeader.AddColumn("Description", 'C', 20, 0);

            // Write the header
            dbfWriter.Write(dbfHeader);
            // Write the features
            for (var i = 0; i < recordCount; i++)
            {
                var columnValues = new List<double> {i};
                dbfWriter.Write(columnValues);
            }

            // End of file flag (0x1A)
            dbfWriter.WriteEndOfDbf();

            dbfWriter.Close();
        }

        public void Dispose()
        {
            //if (_shpBinaryWriter != null)
            Close();
        }

        /// <summary>
        /// Write the enumeration of features to shapefile (shp, shx and dbf)
        /// </summary>
        /// <param name="filename">Filename to create</param>
        /// <param name="features">Enumeration of features to write, features will be enumerated once</param>
        /// <param name="fields">Fields that should be written, only those attributes specified here will be mapped from the feature attributetable while writing</param>
        /// <param name="shapeGeometryType">Type of geometries shapefile</param>
        /// <param name="dbfEncoding">Optional Encoding to be used when writing the DBF-file (default Windows-1252)</param>
        public static void WriteFeatures(string filename, IEnumerable<IFeature> features, DbaseFieldDescriptor[] fields, ShapeGeometryType shapeGeometryType,
            Encoding dbfEncoding = null)
        {

            // Set default encoding if not specified
            if (dbfEncoding == null)
                dbfEncoding = Encoding.GetEncoding(1252);

            // Open shapefile and dbase stream writers
            using (var shpWriter = new ShapefileWriter(Path.ChangeExtension(filename, ".shp"), shapeGeometryType))
            {
                using (var dbfWriter = new DbaseFileWriter(Path.ChangeExtension(filename, ".dbf"), dbfEncoding))
                {
                    var dbfHeader = new DbaseFileHeader(dbfEncoding);
                    foreach (var field in fields)
                    {
                        dbfHeader.AddColumn(field.Name, field.DbaseType, field.Length, field.DecimalCount);
                    }
                    dbfWriter.Write(dbfHeader);

                    var numFeatures = 0;
                    foreach (var feature in features)
                    {
                        shpWriter.Write(feature.Geometry);
                        var values = new object[fields.Length];
                        for (var i = 0; i < fields.Length; i++)
                        {
                            values[i] = feature.Attributes[fields[i].Name];
                        }
                        dbfWriter.Write(values);
                        numFeatures++;
                    }

                    // set the number of records
                    dbfHeader.NumRecords = numFeatures;
                    // Update the header
                    dbfWriter.Write(dbfHeader);
                    // write the end of dbase file marker
                    dbfWriter.WriteEndOfDbf();
                    // close the dbase stream
                    dbfWriter.Close();
                }
            }
        }
    }
}
