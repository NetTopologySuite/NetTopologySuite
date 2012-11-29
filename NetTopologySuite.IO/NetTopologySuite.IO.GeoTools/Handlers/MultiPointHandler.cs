using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Converts a Shapefile point to a OGIS Polygon.
    /// </summary>
    public class MultiPointHandler : ShapeHandler
    {                       
        public MultiPointHandler()
            : base(ShapeGeometryType.MultiPoint)
        {            
        }
        public MultiPointHandler(ShapeGeometryType type) : base(type)
        {                    
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

            var type = (ShapeGeometryType) EnumUtility.Parse(typeof(ShapeGeometryType), shapeTypeNum.ToString());
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreateMultiPoint(new IPoint[] { });

            if (type != ShapeType)
                throw new ShapefileException(string.Format("Encountered a '{0}' instead of a  '{1}'", type, ShapeType));

            // Read and for now ignore bounds.
            int bblength = GetBoundingBoxLength();
            boundingBox = new double[bblength];
            for (; boundingBoxIndex < 4; boundingBoxIndex++)
            {
                double d = file.ReadDouble();
                boundingBox[boundingBoxIndex] = d;
            }

            // Read points
            var numPoints = file.ReadInt32();
            var buffer = new CoordinateBuffer(numPoints, NoDataBorderValue, true);
            var points = new IPoint[numPoints];
            var pm = geometryFactory.PrecisionModel;

            for (var i = 0; i < numPoints; i++)
            {
                var x = pm.MakePrecise(file.ReadDouble());
                var y = pm.MakePrecise(file.ReadDouble());
                buffer.AddCoordinate(x, y);
                buffer.AddMarker();
            }

            // Trond Benum: We have now read all the points, let's read optional Z and M values            
            GetZMValues(file, buffer);

            var sequences = buffer.ToSequences(geometryFactory.CoordinateSequenceFactory);
            for (var i = 0; i < numPoints; i++)
                points[i] = geometryFactory.CreatePoint(sequences[i]);
         
            geom = geometryFactory.CreateMultiPoint(points);
          
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
            
            file.Write(int.Parse(EnumUtility.Format(typeof(ShapeGeometryType), ShapeType, "d")));

            Envelope box = geometry.EnvelopeInternal;
            Envelope bounds = GetEnvelopeExternal(geometryFactory.PrecisionModel, box);
            file.Write(bounds.MinX);
            file.Write(bounds.MinY);
            file.Write(bounds.MaxX);
            file.Write(bounds.MaxY);

            int numPoints = mpoint.NumPoints;
            file.Write(numPoints);

            var hasZ = HasZValue();
            var zList = hasZ ? new List<double>() : null;

            var hasM = HasMValue();
            var mList = hasM ? new List<double>() : null;

            // write the points 
            for (int i = 0; i < numPoints; i++)
            {
                var point = (IPoint) mpoint.Geometries[i];
                file.Write(point.X);
                file.Write(point.Y);

                if (hasZ) zList.Add(point.Z);
                if (hasM) mList.Add(point.M);
            }

            WriteZM(file, numPoints, zList, mList);
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