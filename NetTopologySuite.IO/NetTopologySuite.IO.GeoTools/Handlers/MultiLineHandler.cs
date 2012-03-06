using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Converts a Shapefile multi-line to a OGIS LineString/MultiLineString.
    /// </summary>
    public class MultiLineHandler : ShapeHandler
    {
        /// <summary>
        /// Returns the ShapeType the handler handles.
        /// </summary>
        public override ShapeGeometryType ShapeType
        {
            get { return ShapeGeometryType.LineString; }
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
            type = (ShapeGeometryType)EnumUtility.Parse(typeof(ShapeGeometryType), shapeTypeNum.ToString());
            if (type == ShapeGeometryType.NullShape)
                return geometryFactory.CreateMultiLineString(null);

            if (!(type == ShapeGeometryType.LineString || type == ShapeGeometryType.LineStringM ||
                  type == ShapeGeometryType.LineStringZ || type == ShapeGeometryType.LineStringZM))
                throw new ShapefileException("Attempting to load a non-arc as arc.");

            // Read and for now ignore bounds.
            int bblength = GetBoundingBoxLength();
            bbox = new double[bblength];
            for (; bbindex < 4; bbindex++)
            {
                double d = file.ReadDouble();
                bbox[bbindex] = d;
            }

            int numParts = file.ReadInt32();
            int numPoints = file.ReadInt32();
            int[] partOffsets = new int[numParts];
            for (int i = 0; i < numParts; i++)
                partOffsets[i] = file.ReadInt32();

            ILineString[] lines = new ILineString[numParts];
            var part2 = 0;
            for (int part = 0; part < numParts; part++)
            {
                int start, finish, length;
                start = partOffsets[part];
                finish = part == numParts - 1
                    ? numPoints
                    : partOffsets[part + 1];
                length = finish - start;
                var points = new CoordinateList();
                points.Capacity = length;
                for (var i = 0; i < length; i++)
                {
                    var x = file.ReadDouble();
                    var y = file.ReadDouble();
                    var external = new Coordinate(x, y);
                    geometryFactory.PrecisionModel.MakePrecise(external);
                    points.Add(external);
                }

                var createLineString = true;
                if (points.Count == 1)
                {
                    switch (GeometryInstantiationErrorHandling)
                    {
                        case GeometryInstantiationErrorHandlingOption.ThrowException:
                            break;
                        case GeometryInstantiationErrorHandlingOption.Empty:
                            points.Clear();
                            break;
                        case GeometryInstantiationErrorHandlingOption.TryFix:
                            points.Add((Coordinate)points[1].Clone());
                            break;
                        case GeometryInstantiationErrorHandlingOption.Null:
                            createLineString = false;
                            break;
                    }
                }

                if (createLineString)
                {
                    var line = geometryFactory.CreateLineString(points.ToArray());
                    lines[part2++] = line;
                }
            }

            if (part2 != numParts)
            {
                var tmp = new ILineString[part2];
                Array.Copy(lines, 0, tmp, 0, part2);
                lines = tmp;
            }
            geom = geometryFactory.CreateMultiLineString(lines);
            GrabZMValues(file);
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

            file.Write(int.Parse(EnumUtility.Format(typeof(ShapeGeometryType), ShapeType, "d")));

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

            for (int part = 0; part < numParts; part++)
            {
                CoordinateList points = new CoordinateList(multi.GetGeometryN(part).Coordinates);
                for (int i = 0; i < points.Count; i++)
                {
                    Coordinate external = points[i];
                    file.Write(external.X);
                    file.Write(external.Y);
                }
            }
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