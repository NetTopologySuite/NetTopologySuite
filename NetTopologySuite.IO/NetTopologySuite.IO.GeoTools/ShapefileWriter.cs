using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Handlers;

namespace NetTopologySuite.IO
{
	/// <summary>
	/// This class writes ESRI Shapefiles.
	/// </summary>
	public class ShapefileWriter : IDisposable
	{
	    public IGeometryFactory Factory { get; set; }

        private FileStream _shpStream;
        private BigEndianBinaryWriter _shpBinaryWriter;
        private FileStream _shxStream;
        private BigEndianBinaryWriter _shxBinaryWriter;
        private Envelope _totalEnvelope;
	    
        private readonly ShapeHandler _shapeHandler;
        private readonly ShapeGeometryType _geometryType;
        
        int _numFeaturesWritten;

        /// <summary>
        /// Initializes a buffered writer where you can write shapes individually to the file.
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <param name="geomType">The geometry type</param>
        public ShapefileWriter(string filename, ShapeGeometryType geomType)
            : this(GeometryFactory.Default, filename, geomType)
        {}

        public ShapefileWriter(IGeometryFactory geometryFactory, string filename, ShapeGeometryType geomType)
            : this(geometryFactory)
        {
            var folder = Path.GetDirectoryName(filename) ?? ".";
            var file = Path.GetFileNameWithoutExtension(filename);
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException(string.Format("Filename '{0}' is not valid", filename), "filename");
            filename = Path.Combine(folder, file);

            _shpStream = new FileStream(filename + ".shp", FileMode.Create);
            _shxStream = new FileStream(filename + ".shx", FileMode.Create);

            _geometryType = geomType;

            _shpBinaryWriter = new BigEndianBinaryWriter(_shpStream);
            _shxBinaryWriter = new BigEndianBinaryWriter(_shxStream);

            WriteShpHeader(_shpBinaryWriter, 0, new Envelope(0, 0, 0, 0), geomType);
            WriteShxHeader(_shxBinaryWriter, 0, new Envelope(0, 0, 0, 0), geomType);

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

                var shpLenWords = (int)_shpBinaryWriter.BaseStream.Length / 2;
                var shxLenWords = (int)_shxBinaryWriter.BaseStream.Length / 2;

                WriteShpHeader(_shpBinaryWriter, shpLenWords, _totalEnvelope, _geometryType);
                WriteShxHeader(_shxBinaryWriter, shxLenWords, _totalEnvelope, _geometryType);

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
        [Obsolete("use WriteGeometryCollection")]
        public void Write(string filename, IGeometryCollection geometryCollection)
        {
            WriteGeometryCollection(filename, geometryCollection);
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
		public static void WriteGeometryCollection(string filename, IGeometryCollection geometryCollection)
		{
		    var shapeFileType = Shapefile.GetShapeType(geometryCollection);

		    var numShapes = geometryCollection.NumGeometries;
            using (var writer = new ShapefileWriter(geometryCollection.Factory, filename, shapeFileType))
		    {
		        for (var i = 0; i < numShapes; i++)
		        {
		            writer.Write(geometryCollection[i]);
		        }
		    }

            WriteDummyDbf(filename + ".dbf", numShapes);

		}

        private static void WriteNullShapeRecord(BigEndianBinaryWriter shpBinaryWriter, BigEndianBinaryWriter shxBinaryWriter, int oid)
        {
            const int recordLength = 12;

            // Add shape
            shpBinaryWriter.WriteIntBE(oid);
            shpBinaryWriter.WriteIntBE(recordLength);
            shpBinaryWriter.Write((int)ShapeGeometryType.NullShape);

            // Update shapefile index (position in words, 1 word = 2 bytes)
            var posWords = shpBinaryWriter.BaseStream.Position / 2;
            shxBinaryWriter.WriteIntBE((int)posWords);
            shxBinaryWriter.WriteIntBE(recordLength);

        }


        private static /*int*/ void WriteRecordToFile(BigEndianBinaryWriter shpBinaryWriter, BigEndianBinaryWriter shxBinaryWriter, ShapeHandler handler, IGeometry body, int oid)
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
            var posWords = pos / 2;
            shxBinaryWriter.WriteIntBE((int)posWords);
            shxBinaryWriter.WriteIntBE(recordLength);

            handler.Write(body, shpBinaryWriter, body.Factory);
            /*return recordLength;*/
        }

        private static void WriteShxHeader(BigEndianBinaryWriter shxBinaryWriter, int shxLength, Envelope bounds, ShapeGeometryType shapeType)
        {
            // write the .shx header
            var shxHeader = new ShapefileHeader {FileLength = shxLength, Bounds = bounds, ShapeType = shapeType};

            // assumes Geometry type of the first item will the same for all other items in the collection.
            shxHeader.Write(shxBinaryWriter);
        }

        private static void WriteShpHeader(BigEndianBinaryWriter shpBinaryWriter, int shpLength, Envelope bounds, ShapeGeometryType shapeType)
        {
            var shpHeader = new ShapefileHeader {FileLength = shpLength, Bounds = bounds, ShapeType = shapeType};

            // assumes Geometry type of the first item will the same for all other items
            // in the collection.
            shpHeader.Write(shpBinaryWriter);
        }       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="recordCount"></param>
		public static void WriteDummyDbf(string filename, int recordCount)
        {
            filename = Path.ChangeExtension(filename, "dbf");
            var dbfHeader = new DbaseFileHeader {NumRecords = recordCount};
            dbfHeader.AddColumn("Description",'C', 20, 0);
			
			var dbfWriter = new DbaseFileWriter(filename);
			dbfWriter.Write(dbfHeader);
			for (var i = 0; i < recordCount; i++)
			{
				var columnValues = new List<double> {i};
			    dbfWriter.Write(columnValues);
			}
            // End of file flag (0x1A)
            dbfWriter.Write(0x1A);
			dbfWriter.Close();
		}

        public void Dispose()
        {
            if (_shpBinaryWriter != null)
                Close();
        }
    }
}
