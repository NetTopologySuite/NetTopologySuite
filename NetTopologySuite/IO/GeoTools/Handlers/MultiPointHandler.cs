using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Converts a Shapefile point to a OGIS Polygon.
    /// </summary>
    public class MultiPointHandler : ShapeHandler
    {        
       

        /// <summary>
        /// The ShapeType this handler handles.
        /// </summary>
        public override ShapeGeometryType ShapeType
        {
            get { return ShapeGeometryType.MultiPoint; }
        }		

        /// <summary>
        /// Reads a stream and converts the shapefile record to an equilivant geometry object.
        /// </summary>
        /// <param name="file">The stream to read.</param>
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public override IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory)
        {
            int shapeTypeNum = file.ReadInt32();

            type = (ShapeGeometryType) Enum.Parse(typeof(ShapeGeometryType), shapeTypeNum.ToString());
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreateMultiPoint(new IPoint[] { });
            
            if (!(type == ShapeGeometryType.MultiPoint  || type == ShapeGeometryType.MultiPointM ||
                  type == ShapeGeometryType.MultiPointZ || type == ShapeGeometryType.MultiPointZM))	
                throw new ShapefileException("Attempting to load a non-multipoint as multipoint.");

            // Read and for now ignore bounds.
            int bblength = GetBoundingBoxLength();
            bbox = new double[bblength];
            for (; bbindex < 4; bbindex++)
            {
                double d = file.ReadDouble();
                bbox[bbindex] = d;
            }

            // Read points
            int numPoints = file.ReadInt32();
            IPoint[] points = new IPoint[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                double x = file.ReadDouble();
                double y = file.ReadDouble();
                IPoint point = geometryFactory.CreatePoint(new Coordinate(x, y));                
                points[i] = point;
            }
            geom = geometryFactory.CreateMultiPoint(points);
            GrabZMValues(file);
            return geom;
        }        

        /// <summary>
        /// Writes a Geometry to the given binary wirter.
        /// </summary>
        /// <param name="geometry">The geometry to write.</param>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public override void Write(IGeometry geometry, BinaryWriter file, IGeometryFactory geometryFactory)
        {
            if (!(geometry is IMultiPoint))
                throw new ArgumentException("Geometry Type error: MultiPoint expected, but the type retrieved is " + geometry.GetType().Name);

            // Slow and maybe not useful...
            // if (!geometry.IsValid)
            // Trace.WriteLine("Invalid multipoint being written.");

            IMultiPoint mpoint = geometry as IMultiPoint;
            
            file.Write(int.Parse(Enum.Format(typeof(ShapeGeometryType), ShapeType, "d")));

            IEnvelope box = geometry.EnvelopeInternal;
            IEnvelope bounds = GetEnvelopeExternal(geometryFactory.PrecisionModel, box);
            file.Write(bounds.MinX);
            file.Write(bounds.MinY);
            file.Write(bounds.MaxX);
            file.Write(bounds.MaxY);

            int numPoints = mpoint.NumPoints;
            file.Write(numPoints);						

            // write the points 
            for (int i = 0; i < numPoints; i++)
            {
                IPoint point = (IPoint) mpoint.Geometries[i];
                file.Write(point.X);
                file.Write(point.Y);	
            }            
        }
		
        /// <summary>
        /// Gets the length of the shapefile record using the geometry passed in.
        /// </summary>
        /// <param name="geometry">The geometry to get the length for.</param>
        /// <returns>The length in bytes this geometry is going to use when written out as a shapefile record.</returns>
        public override int GetLength(IGeometry geometry)
        {			
            return (20 + geometry.NumPoints * 8); // 20 => shapetype(2) + bbox (4*4) + numpoints
        }					
    }
}