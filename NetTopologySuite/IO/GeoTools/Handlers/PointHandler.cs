using System;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Converts a Shapefile point to a OGIS Point.
	/// </summary>
	public class PointHandler : ShapeHandler
	{
		/// <summary>
		/// Initializes a new instance of the PointHandler class.
		/// </summary>
		public PointHandler() : base() { }
	
		/// <summary>
		/// The shape type this handler handles (point).
		/// </summary>
        public override ShapeGeometryTypes ShapeType
		{
			get
			{
                return ShapeGeometryTypes.Point;
			}
		}
		
		/// <summary>
		/// Reads a stream and converts the shapefile record to an equilivent geometry object.
		/// </summary>
		/// <param name="file">The stream to read.</param>
		/// <param name="geometryFactory">The geometry factory to use when making the object.</param>
		/// <returns>The Geometry object that represents the shape file record.</returns>
		public override IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory)
		{
			int shapeTypeNum = file.ReadInt32();
            ShapeGeometryTypes shapeType = (ShapeGeometryTypes) Enum.Parse(typeof(ShapeGeometryTypes), shapeTypeNum.ToString());
            if ( ! ( shapeType == ShapeGeometryTypes.Point  || shapeType == ShapeGeometryTypes.PointM   ||
                     shapeType == ShapeGeometryTypes.PointZ || shapeType == ShapeGeometryTypes.PointZM  ))	
				throw new ShapefileException("Attempting to load a point as point.");
			double x= file.ReadDouble();
			double y= file.ReadDouble();
			ICoordinate external = new Coordinate(x,y);			
			geometryFactory.PrecisionModel.MakePrecise( external);
            return geometryFactory.CreatePoint(external);
		}
		
		/// <summary>
		/// Writes to the given stream the equilivent shape file record given a Geometry object.
		/// </summary>
		/// <param name="geometry">The geometry object to write.</param>
		/// <param name="file">The stream to write to.</param>
		/// <param name="geometryFactory">The geometry factory to use.</param>
		public override void Write(IGeometry geometry, System.IO.BinaryWriter file, IGeometryFactory geometryFactory)
		{
            file.Write(int.Parse(Enum.Format(typeof(ShapeGeometryTypes), this.ShapeType, "d")));
			ICoordinate external = geometry.Coordinates[0];
			file.Write(external.X);
			file.Write(external.Y);
		}

		/// <summary>
		/// Gets the length in bytes the Geometry will need when written as a shape file record.
		/// </summary>
		/// <param name="geometry">The Geometry object to use.</param>
		/// <returns>The length in bytes the Geometry will use when represented as a shape file record.</returns>
		public override int GetLength(IGeometry geometry)
		{
			return 10; // The length of two doubles in 16bit words + the shapeType
		}		
	}
}
