using System;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Abstract class that defines the interfaces that other 'Shape' handlers must implement.
	/// </summary>
	public abstract class ShapeHandler 
	{
        /// <summary>
        /// 
        /// </summary>
		public ShapeHandler() { }

		/// <summary>
		/// Returns the ShapeType the handler handles.
		/// </summary>
        public abstract ShapeGeometryTypes ShapeType { get; }

		/// <summary>
		/// Reads a stream and converts the shapefile record to an equilivent geometry object.
		/// </summary>
		/// <param name="file">The stream to read.</param>
		/// <param name="geometryFactory">The geometry factory to use when making the object.</param>
		/// <returns>The Geometry object that represents the shape file record.</returns>
		public abstract IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory);

		/// <summary>
		/// Writes to the given stream the equilivent shape file record given a Geometry object.
		/// </summary>
		/// <param name="geometry">The geometry object to write.</param>
		/// <param name="file">The stream to write to.</param>
		/// <param name="geometryFactory">The geometry factory to use.</param>
		public abstract void Write(IGeometry geometry, System.IO.BinaryWriter file,  IGeometryFactory geometryFactory);

		/// <summary>
		/// Gets the length in bytes the Geometry will need when written as a shape file record.
		/// </summary>
		/// <param name="geometry">The Geometry object to use.</param>
        /// <returns>The length in 16bit words the Geometry will use when represented as a shape file record.</returns>
		public abstract int GetLength(IGeometry geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static IEnvelope GetEnvelopeExternal(IEnvelope envelope)
        {
            // get envelose in external coordinates
            ICoordinate min = new Coordinate(envelope.MinX, envelope.MinY);
            ICoordinate max = new Coordinate(envelope.MaxX, envelope.MaxY);
            IEnvelope bounds = new Envelope(min.X, max.X, min.Y, max.Y);
            return bounds;
        }

        /// <summary>
        /// Get Envelope in external coordinates.
        /// </summary>
        /// <param name="precisionModel"></param>
        /// <param name="envelope"></param>
        /// <returns></returns>
		public static IEnvelope GetEnvelopeExternal(IPrecisionModel precisionModel, IEnvelope envelope)
		{
            return GetEnvelopeExternal(envelope);
		}        
	}
}
