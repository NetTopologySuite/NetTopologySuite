using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Converts a Shapefile multi-line to a OGIS LineString/MultiLineString.
    /// </summary>
    public class MultiLineHandler : ShapeHandler
    {
        public MultiLineHandler() : base(ShapeGeometryType.LineString)
        {            
        }
        public MultiLineHandler(ShapeGeometryType type) : base(type)
        {
        }


        /// <summary>
        /// Reads a stream and converts the shapefile record to an equilivent geometry object.
        /// </summary>
        /// <param name="file">The stream to read.</param>
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public override IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory)
        {
            var type = (ShapeGeometryType)file.ReadInt32();
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreateMultiLineString(null);

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

            int numParts = file.ReadInt32();
            int numPoints = file.ReadInt32();
            int[] partOffsets = new int[numParts];
            for (int i = 0; i < numParts; i++)
                partOffsets[i] = file.ReadInt32();

            var lines = new List<ILineString>(numParts);
            var buffer = new CoordinateBuffer(numPoints, NoDataBorderValue, true);
            var pm = geometryFactory.PrecisionModel;

            for (var part = 0; part < numParts; part++)
            {
                var start = partOffsets[part];
                var finish = part == numParts - 1
                                 ? numPoints
                                 : partOffsets[part + 1];
                var length = finish - start;
                
                for (var i = 0; i < length; i++)
                {
                    var x = pm.MakePrecise(file.ReadDouble());
                    var y = pm.MakePrecise(file.ReadDouble());
                    buffer.AddCoordinate(x, y);
                }
                buffer.AddMarker();
            }

            // Trond Benum: We have now read all the parts, let's read optional Z and M values
            // and populate Z in the coordinate before we start manipulating the segments
            // We have to track corresponding optional M values and set them up in the 
            // Geometries via ICoordinateSequence further down.
            GetZMValues(file, buffer);

            var sequences = new List<ICoordinateSequence>(buffer.ToSequences(geometryFactory.CoordinateSequenceFactory));

            for (var s = 0; s < sequences.Count; s++)
            {
                var points = sequences[s];
                var createLineString = true;
                if (points.Count == 1)
                {
                    switch (GeometryInstantiationErrorHandling)
                    {
                        case GeometryInstantiationErrorHandlingOption.ThrowException:
                            break;
                        case GeometryInstantiationErrorHandlingOption.Empty:
                            sequences[s] = geometryFactory.CoordinateSequenceFactory.Create(0, points.Ordinates);
                            break;
                        case GeometryInstantiationErrorHandlingOption.TryFix:
                            sequences[s] = AddCoordinateToSequence(points, geometryFactory.CoordinateSequenceFactory,
                                points.GetOrdinate(0, Ordinate.X), points.GetOrdinate(0, Ordinate.Y),
                                points.GetOrdinate(0, Ordinate.Z), points.GetOrdinate(0, Ordinate.M));
                            break;
                        case GeometryInstantiationErrorHandlingOption.Null:
                            createLineString = false;
                            break;
                    }
                }

                if (createLineString)
                {
                    // Grabs m values if we have them
                    var line = geometryFactory.CreateLineString(points);
                    lines.Add(line);
                }
            }

            geom = (lines.Count != 1)
                ? (IGeometry)geometryFactory.CreateMultiLineString(lines.ToArray())
                : lines[0];          
            return geom;
        }

        /// <summary>
        /// Writes to the given stream the equilivent shape file record given a Geometry object.
        /// </summary>
        /// <param name="geometry">The geometry object to write.</param>
        /// <param name="file">The stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public override void Write(IGeometry geometry, BinaryWriter file, IGeometryFactory geometryFactory)
        {
            // Force to use a MultiGeometry
            IMultiLineString multi;
            if (geometry is IGeometryCollection)
                multi = (IMultiLineString)geometry;
            else
                multi = geometryFactory.CreateMultiLineString(new[] { (ILineString)geometry });

            file.Write((int)ShapeType);

            var box = multi.EnvelopeInternal;
            file.Write(box.MinX);
            file.Write(box.MinY);
            file.Write(box.MaxX);
            file.Write(box.MaxY);

            var numParts = multi.NumGeometries;
            var numPoints = multi.NumPoints;

            file.Write(numParts);
            file.Write(numPoints);

            // Write the offsets
            var offset = 0;
            for (var i = 0; i < numParts; i++)
            {
                var g = multi.GetGeometryN(i);
                file.Write(offset);
                offset = offset + g.NumPoints;
            }

            var zList = HasZValue() ? new List<double>() : null;
            var mList = HasMValue() ? new List<double>() : null;

            for (var part = 0; part < numParts; part++)
            {
                var geometryN = (ILineString)multi.GetGeometryN(part);
                var points = geometryN.CoordinateSequence;
                WriteCoords(points, file, zList, mList);
            }

            WriteZM(file, numPoints, zList, mList);
        }

        /// <summary>
        /// Gets the length in bytes the Geometry will need when written as a shape file record.
        /// </summary>
        /// <param name="geometry">The Geometry object to use.</param>
        /// <returns>The length in bytes the Geometry will use when represented as a shape file record.</returns>
        public override int GetLength(IGeometry geometry)
        {
            int numParts = GetNumParts(geometry);
            return (22 + (2 * numParts) + geometry.NumPoints * 8); // 22 => shapetype(2) + bbox(4*4) + numparts(2) + numpoints(2)
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private int GetNumParts(IGeometry geometry)
        {
            int numParts = 1;
            if (geometry is IMultiLineString)
                numParts = ((IMultiLineString)geometry).Geometries.Length;
            return numParts;
        }
    }
}