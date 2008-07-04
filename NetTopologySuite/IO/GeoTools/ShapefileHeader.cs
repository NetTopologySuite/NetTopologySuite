using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Class that represents a shape file header record.
	/// </summary>
	public class ShapefileHeader
	{
		private int _fileCode = Shapefile.ShapefileId;
		private int _fileLength = -1;
		private int _version = 1000;
        private ShapeGeometryType _shapeType = ShapeGeometryType.NullShape;
		private IEnvelope _bounds;
	
		/// <summary>
		/// Initializes a new instance of the ShapefileHeader class with values read in from the stream.
		/// </summary>
		/// <remarks>Reads the header information from the stream.</remarks>
		/// <param name="shpBinaryReader">BigEndianBinaryReader stream to the shapefile.</param>
		public ShapefileHeader(BigEndianBinaryReader shpBinaryReader)
		{
			if (shpBinaryReader == null)
				throw new ArgumentNullException("shpBinaryReader");

			_fileCode = shpBinaryReader.ReadInt32BE();	
			if (_fileCode != Shapefile.ShapefileId)
				throw new ShapefileException("The first four bytes of this file indicate this is not a shape file.");

			// skip 5 unsed bytes
			shpBinaryReader.ReadInt32BE();
			shpBinaryReader.ReadInt32BE();
			shpBinaryReader.ReadInt32BE();
			shpBinaryReader.ReadInt32BE();
			shpBinaryReader.ReadInt32BE();

			_fileLength = shpBinaryReader.ReadInt32BE();

			_version = shpBinaryReader.ReadInt32();
			Debug.Assert(_version == 1000, "Shapefile version", String.Format("Expecting only one version (1000), but got {0}",_version));
			int shapeType = shpBinaryReader.ReadInt32();
            _shapeType = (ShapeGeometryType) Enum.Parse(typeof(ShapeGeometryType), shapeType.ToString());

			//read in and store the bounding box
			double[] coords = new double[4];
			for (int i = 0; i < 4; i++)
				coords[i] = shpBinaryReader.ReadDouble();
			_bounds = new Envelope(coords[0], coords[2], coords[1], coords[3]);
			
			// skip z and m bounding boxes.
			for (int i = 0; i < 4; i++)
				shpBinaryReader.ReadDouble();	
		}

		/// <summary>
		/// Initializes a new instance of the ShapefileHeader class.
		/// </summary>
		public ShapefileHeader() { }

		/// <summary>
		/// Gets and sets the bounds of the shape file.
		/// </summary>
		public IEnvelope Bounds
		{
			get
			{
				return _bounds;
			}
			set
			{
				_bounds = value;
			}
		}

		/// <summary>
		/// Gets and sets the shape file type i.e. polygon, point etc...
		/// </summary>
        public ShapeGeometryType ShapeType
		{
			get
			{
				return _shapeType;
			}
			set
			{
				_shapeType = value;
			}
		}

		/// <summary>
		/// Gets and sets the shapefile version.
		/// </summary>
		public int Version
		{
			get
			{
				return _version;
			}
			set
			{
				_version = value;
			}
		}

		/// <summary>
		/// Gets and sets the length of the shape file in words.
		/// </summary>
		public int FileLength
		{
			get
			{
				return _fileLength;
			}
			set
			{
				_fileLength = value;
			}
		}

		/// <summary>
		/// Writes a shapefile header to the given stream;
		/// </summary>
		/// <param name="file">The binary writer to use.</param>
		public void Write(BigEndianBinaryWriter file) 
		{
			if (file == null)
				throw new ArgumentNullException("file");
			if (_fileLength==-1)
				throw new InvalidOperationException("The header properties need to be set before writing the header record.");
			int pos = 0;
			file.WriteIntBE(_fileCode);
			pos += 4;
			for (int i = 0; i < 5; i++)
			{
				file.WriteIntBE(0);//Skip unused part of header
				pos += 4;
			}
			file.WriteIntBE(_fileLength);
			pos += 4;
			file.Write(_version);
			pos += 4;

            file.Write(int.Parse(Enum.Format(typeof(ShapeGeometryType), _shapeType, "d")));
			
            pos += 4;
			// Write the bounding box
			file.Write(_bounds.MinX);
			file.Write(_bounds.MinY);
			file.Write(_bounds.MaxX);
			file.Write(_bounds.MaxY);
			pos += 8 * 4;        

			// Skip remaining unused bytes
			for (int i = 0; i < 4; i++)
			{
				file.Write(0.0); // Skip unused part of header
				pos += 8;
			}		
		}
	}
}
