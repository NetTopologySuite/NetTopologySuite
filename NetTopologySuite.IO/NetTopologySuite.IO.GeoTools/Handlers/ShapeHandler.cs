using System;
using System.Collections.Generic;
using System.IO;
#if !NET35
using HS = Wintellect.PowerCollections.Set<int>;
#else
using System.Linq;
using HS = System.Collections.Generic.HashSet<int>;
#endif
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Abstract class that defines the interfaces that other 'Shape' handlers must implement.
    /// </summary>
    public abstract class ShapeHandler
    {
        /*
            Floating point numbers must be numeric values. Positive infinity, negative infinity, and
            Not-a-Number (NaN) values are not allowed in shapefiles. Nevertheless, shapefiles
            support the concept of "no data" values, but they are currently used only for measures.
            Any floating point number smaller than –10E38 is considered by a shapefile reader to
            represent a "no data" value.
            http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf (page 2, bottom)
         */
        protected const double NoDataBorderValue = -10e38;
        protected const double NoDataValue = -101e37;//NoDataBorderValue - 1;

        protected int boundingBoxIndex = 0;
        protected double[] boundingBox;
        private readonly ShapeGeometryType _type;
        protected IGeometry geom;
        //protected CoordinateBuffer Buffer;

        protected ShapeHandler()
            : this(ShapeGeometryType.NullShape)
        {            
        }

        protected ShapeHandler(ShapeGeometryType type)
        {
            _type = type;
        }

        /// <summary>
        /// Returns the ShapeType the handler handles.
        /// </summary>
        public ShapeGeometryType ShapeType { get { return _type; } }

        /// <summary>
        /// Reads a stream and converts the shapefile record to an equilivent geometry object.
        /// </summary>
        /// <param name="file">The stream to read.</param>
        /// <param name="totalRecordLength">Total number of total bytes in the record to read.</param>
        /// <param name="factory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public abstract IGeometry Read(BigEndianBinaryReader file, int totalRecordLength, IGeometryFactory factory);

        /// <summary>
        /// Read an int from the stream.<br/>Tracks how many words (1 word = 2 bytes) we have read and that we do not over read.
        /// </summary>
        /// <param name="file">The reader to use</param>
        /// <param name="totalRecordLength">The total number of words (1 word = 2 bytes) this record has</param>
        /// <param name="totalRead">A word counter</param>
        /// <returns>The value read</returns>
        protected int ReadInt32(BigEndianBinaryReader file, int totalRecordLength, ref int totalRead)
        {
            var newRead = totalRead + 2;
            if (newRead > totalRecordLength)
                throw new Exception("End of data encountered while reading integer");
            
            // track how many bytes we have read to know if we have optional values at the end of the record or not
            totalRead = newRead; 
            return file.ReadInt32();
        }

        /// <summary>
        /// Read a double from the stream.<br/>Tracks how many words (1 word = 2 bytes) we have read and than we do not over read.
        /// </summary>
        /// <param name="file">The reader to use</param>
        /// <param name="totalRecordLength">The total number of words (1 word = 2 bytes) this record has</param>
        /// <param name="totalRead">A word counter</param>
        /// <returns>The value read</returns>
        protected double ReadDouble(BigEndianBinaryReader file, int totalRecordLength, ref int totalRead)
        {
            var newRead = totalRead + 4;
            if (newRead > totalRecordLength)
                throw new Exception("End of data encountered while reading double");

            // track how many bytes we have read to know if we have optional values at the end of the record or not
            totalRead = newRead; 
            return file.ReadDouble();
        }
        
        /// <summary>
        /// Writes to the given stream the equilivent shape file record given a Geometry object.
        /// </summary>
        /// <param name="geometry">The geometry object to write.</param>
        /// <param name="writer">The writer to use.</param>
        /// <param name="factory">The geometry factory to use.</param>
        public abstract void Write(IGeometry geometry, BinaryWriter writer, IGeometryFactory factory);

        /// <summary>
        /// Gets the length in words (1 word = 2 bytes) the Geometry will need when written as a shape file record.
        /// </summary>
        /// <param name="geometry">The Geometry object to use.</param>
        /// <returns>The length in 16bit words the Geometry will use when represented as a shape file record.</returns>
        public abstract int ComputeRequiredLengthInWords(IGeometry geometry);

        /// <summary>
        /// Gets the length in words (1 word = 2 bytes) of a multipart geometry needed when written as a shape file record.
        /// </summary>
        /// <param name="numParts">The number of geometry components</param>
        /// <param name="numPoints">The number of points</param>
        /// <param name="hasM">A value indicating that we have M ordinates</param>
        /// <param name="hasZ">A value indicating that we have Z (and therefore M) ordinates</param>
        /// <returns>The length in words (1  word = 2 bytes) the Geometry will use when represented as a shape file record.</returns>
        protected static int ComputeRequiredLengthInWords(int numParts, int numPoints, bool hasM, bool hasZ)
        {
            // x, y => 2 * 4;
            var pointFactor = 2 * 4;

            //  initial = shapetype(2) + bbox(4*4) + numpoints(2)
            var initial = 2 + 4*4 + 2;
            if (numParts > 0)
                initial += 2;

            if (hasZ)
            {
                initial = initial + 4 * 4;         // ZM 16 => bbox (4*4)
                pointFactor = pointFactor + 2 * 4; // ZM  8 => 2 * 4
            }
            else if (hasM)
            {
                initial = initial + 8;         // M 16 => bbox M (2 * 4)
                pointFactor = pointFactor + 4; // M  8 => 1 * 4
            }

            return (initial + (2 * numParts) + numPoints * pointFactor);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static Envelope GetEnvelopeExternal(Envelope envelope)
        {
            // Get envelope in external coordinates
            var min = new Coordinate(envelope.MinX, envelope.MinY);
            var max = new Coordinate(envelope.MaxX, envelope.MaxY);
            var bounds = new Envelope(min.X, max.X, min.Y, max.Y);
            return bounds;
        }

        /// <summary>
        /// Get Envelope in external coordinates.
        /// </summary>
        /// <param name="precisionModel">The precision model to use</param>
        /// <param name="envelope">The envelope to get</param>
        /// <returns></returns>
        public static Envelope GetEnvelopeExternal(IPrecisionModel precisionModel, Envelope envelope)
        {
            // Get envelope in external coordinates
            var min = new Coordinate(envelope.MinX, envelope.MinY);
            precisionModel.MakePrecise(min);
            var max = new Coordinate(envelope.MaxX, envelope.MaxY);
            precisionModel.MakePrecise(max);
            var bounds = new Envelope(min.X, max.X, min.Y, max.Y);

            return bounds;

            //return GetEnvelopeExternal(envelope);
        }

        /// <summary>
        /// Method to write the bounding box of x- and y- ordinates (aka envelope)
        /// </summary>
        /// <param name="writer">The writer to use</param>
        /// <param name="precisionModel">The precision model to precise</param>
        /// <param name="envelope">The envelope to write</param>
        protected static void WriteEnvelope(BinaryWriter writer, IPrecisionModel precisionModel, Envelope envelope)
        {
            //precise the envelope
            envelope = GetEnvelopeExternal(precisionModel, envelope);
            
            writer.Write(envelope.MinX);
            writer.Write(envelope.MinY);
            writer.Write(envelope.MaxX);
            writer.Write(envelope.MaxY);
        }

        protected static void WriteCoords(ICoordinateSequence points, BinaryWriter file, List<double> zList, List<double> mList)
        {
            for (var i = 0; i < points.Count; i++)
            {
                file.Write(points.GetOrdinate(i, Ordinate.X));
                file.Write(points.GetOrdinate(i, Ordinate.Y));
                if (zList != null)
                {
                    if ((points.Ordinates & Ordinates.Z) != Ordinates.Z)
                        zList.Add(0d);
                    else
                        zList.Add(points.GetOrdinate(i, Ordinate.Z));
                }

                if (mList == null) 
                    continue;
                
                if ((points.Ordinates & Ordinates.M) != Ordinates.M)
                    mList.Add(NoDataValue);
                else
                {
                    var val = points.GetOrdinate(i, Ordinate.M);
                    if (val.Equals(Coordinate.NullOrdinate)) 
                        val = NoDataValue;
                    mList.Add(val);
                }
            }
        }

        protected static ICoordinateSequence AddCoordinateToSequence(ICoordinateSequence sequence,
                                                                     ICoordinateSequenceFactory factory,
                                                                     double x, double y, double? z, double? m)
        {
            // Create a new sequence 
            var newSequence = factory.Create(sequence.Count + 1, sequence.Ordinates);
            
            // Copy old values
            var ordinates = OrdinatesUtility.ToOrdinateArray(sequence.Ordinates);
            for (var i = 0; i < sequence.Count; i++)
            {
                foreach (var ordinate in ordinates)
                    newSequence.SetOrdinate(i, ordinate, sequence.GetOrdinate(i, ordinate));
            }

            // new coordinate
            newSequence.SetOrdinate(sequence.Count, Ordinate.X, x);
            newSequence.SetOrdinate(sequence.Count, Ordinate.Y, y);
            if (z.HasValue) newSequence.SetOrdinate(sequence.Count, Ordinate.Z, z.Value);
            if (m.HasValue) newSequence.SetOrdinate(sequence.Count, Ordinate.M, m.Value);
            
            return newSequence;
        }

        /// <summary>
        /// Function to determine whether or not the shape type might supply an z-ordinate value
        /// </summary>
        /// <returns><value>true</value> if <see cref="ShapeType"/> is one of 
        /// <list type="Bullet">
        /// <item><see cref="ShapeGeometryType.PointZM"/></item>
        /// <item><see cref="ShapeGeometryType.MultiPointZM"/></item>
        /// <item><see cref="ShapeGeometryType.LineStringZM"/></item>
        /// <item><see cref="ShapeGeometryType.PolygonZM"/></item>
        /// </list>
        /// </returns>
        protected bool HasZValue()
        {
            return HasZValue(_type);
        }

        /// <summary>
        /// Function to determine whether or not the shape type might supply an z-ordinate value
        /// </summary>
        /// <param name="shapeType">The shape type</param>
        /// <returns><value>true</value> if <paramref name="shapeType"/> is one of 
        /// <list type="Bullet">
        /// <item><see cref="ShapeGeometryType.PointZM"/></item>
        /// <item><see cref="ShapeGeometryType.MultiPointZM"/></item>
        /// <item><see cref="ShapeGeometryType.LineStringZM"/></item>
        /// <item><see cref="ShapeGeometryType.PolygonZM"/></item>
        /// </list>
        /// </returns>
        private static bool HasZValue(ShapeGeometryType shapeType)
        {
            return  shapeType == ShapeGeometryType.PointZM ||
                    shapeType == ShapeGeometryType.MultiPointZM ||
                    shapeType == ShapeGeometryType.LineStringZM ||
                    shapeType == ShapeGeometryType.PolygonZM;
        }

        /// <summary>
        // Function to determine whether this handler might supply an m-ordinate value
        /// </summary>
        /// <returns><value>true</value> if <see cref="ShapeType"/> is one of 
        /// <list type="Bullet">
        /// <item><see cref="ShapeGeometryType.PointM"/>, <see cref="ShapeGeometryType.PointZM"/></item>
        /// <item><see cref="ShapeGeometryType.MultiPointM"/>,<see cref="ShapeGeometryType.MultiPointZM"/></item>
        /// <item><see cref="ShapeGeometryType.LineStringM"/>, <see cref="ShapeGeometryType.LineStringZM"/></item>
        /// <item><see cref="ShapeGeometryType.PolygonM"/>, <see cref="ShapeGeometryType.PolygonZM"/></item>
        /// </list>
        /// </returns>
        protected bool HasMValue()
        {
            return HasMValue(_type);
        }

        /// <summary>
        /// Function to determine whether or not the shape type might supply an m-ordinate value
        /// </summary>
        /// <param name="shapeType">The shape type</param>
        /// <returns><value>true</value> if <paramref name="shapeType"/> is one of 
        /// <list type="Bullet">
        /// <item><see cref="ShapeGeometryType.PointM"/>, <see cref="ShapeGeometryType.PointZM"/></item>
        /// <item><see cref="ShapeGeometryType.MultiPointM"/>,<see cref="ShapeGeometryType.MultiPointZM"/></item>
        /// <item><see cref="ShapeGeometryType.LineStringM"/>, <see cref="ShapeGeometryType.LineStringZM"/></item>
        /// <item><see cref="ShapeGeometryType.PolygonM"/>, <see cref="ShapeGeometryType.PolygonZM"/></item>
        /// </list>
        /// </returns>
        private static bool HasMValue(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.PointM ||
                   shapeType == ShapeGeometryType.PointZM ||
                   shapeType == ShapeGeometryType.MultiPointM ||
                   shapeType == ShapeGeometryType.MultiPointZM ||
                   shapeType == ShapeGeometryType.LineStringM ||
                   shapeType == ShapeGeometryType.LineStringZM ||
                   shapeType == ShapeGeometryType.PolygonM ||
                   shapeType == ShapeGeometryType.PolygonZM;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected bool IsPoint()
        {
            return IsPoint(_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsPoint(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.Point ||
                   //shapeType == ShapeGeometryType.PointZ ||
                   shapeType == ShapeGeometryType.PointM ||
                   shapeType == ShapeGeometryType.PointZM;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected bool IsMultiPoint()
        {
            return IsMultiPoint(_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsMultiPoint(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.MultiPoint ||
                   //shapeType == ShapeGeometryType.MultiPointZ ||
                   shapeType == ShapeGeometryType.MultiPointM ||
                   shapeType == ShapeGeometryType.MultiPointZM;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected bool IsLineString()
        {
            return IsLineString(_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsLineString(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.LineString ||
                   //shapeType == ShapeGeometryType.LineStringZ ||
                   shapeType == ShapeGeometryType.LineStringM ||
                   shapeType == ShapeGeometryType.LineStringZM;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected bool IsPolygon()
        {
            return IsPolygon(_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsPolygon(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.Polygon ||
                   //shapeType == ShapeGeometryType.PolygonZ ||
                   shapeType == ShapeGeometryType.PolygonM ||
                   shapeType == ShapeGeometryType.PolygonZM;
        }

        protected static double ReadDouble(BigEndianBinaryReader reader)
        {
            return reader.ReadDouble();           
        }

        /*
        protected static double[] ReadDoubles(BigEndianBinaryReader reader, int count)
        {
            var result = new double[count];
            for (var i = 0; i < count; i++)
                result[i] = reader.ReadDouble();
            return result;
        }
         */

        [Obsolete("Use ReadDouble()")]
        protected double GetZValue(BigEndianBinaryReader file)
        {
            return ReadDouble(file);
        }

        [Obsolete("Use ReadDouble()")]
        protected double GetMValue(BigEndianBinaryReader file)
        {
            return ReadDouble(file);
        }


        /// <summary>
        /// Get the z values and populate each one of them in Coordinate.Z
        /// If there are M values, return an array with those.
        /// </summary>
        /// <param name="file">The reader</param>
        /// <param name="totalRecordLength">Total number of bytes in this record</param>
        /// <param name="currentlyReadBytes">How many bytes are read from this record</param>
        /// <param name="buffer">The coordinate buffer</param>
        /// <param name="skippedList">A list of indices which have not been added to the buffer</param>     
        protected void GetZMValues(BigEndianBinaryReader file, int totalRecordLength, ref int currentlyReadBytes, ICoordinateBuffer buffer, HS skippedList = null)
        {
            if (skippedList == null)
                skippedList = new HS();

            var numPoints = buffer.Capacity;

            if (HasZValue())
            {
                boundingBox[boundingBoxIndex++] = ReadDouble(file, totalRecordLength, ref currentlyReadBytes);
                boundingBox[boundingBoxIndex++] = ReadDouble(file, totalRecordLength, ref currentlyReadBytes);

                var numSkipped = 0;
                for (var i = 0; i < numPoints; i++)
                {
                    var z = ReadDouble(file, totalRecordLength, ref currentlyReadBytes);
                    if (!skippedList.Contains(i))
                        buffer.SetZ(i-numSkipped, z);
                    else numSkipped++;
                }
            }

            // Trond: Note that M value is always optional per the shapefile spec. So we need to test total read record bytes
            // v.s. read bytes to see if we have them or not
            // Also: If we have Z we might have M. Per shapefile defn.
            if ((HasMValue() || HasZValue()) && currentlyReadBytes < totalRecordLength)
            {
                boundingBox[boundingBoxIndex++] = ReadDouble(file, totalRecordLength, ref currentlyReadBytes);
                boundingBox[boundingBoxIndex++] = ReadDouble(file, totalRecordLength, ref currentlyReadBytes);

                var numSkipped = 0;
                for (var i = 0; i < numPoints; i++)
                {
                    var m = ReadDouble(file, totalRecordLength, ref currentlyReadBytes);
                    if (!skippedList.Contains(i))
                        buffer.SetM(i - numSkipped, m);
                    else numSkipped++;
                }
            }

            if (currentlyReadBytes < totalRecordLength)
            {
                int remaining = totalRecordLength - currentlyReadBytes;
                file.ReadBytes(remaining * 2);
            }
        }

        protected void WriteZM(BinaryWriter file, int count, List<double> zValues, List<double> mValues)
        {
            // If we have Z, write it
            if ((HasZValue()))
            {
                //Do not change this as it breaks NET20 Build
                file.Write(Enumerable.Min(zValues));
                file.Write(Enumerable.Max(zValues));
                for (var i = 0; i < count; i++)
                    file.Write(zValues[i]);
            }

            // If we have Z, we might also optionally have M
            if (HasMValue() || (HasZValue() && mValues!=null && Enumerable.Any(mValues)))
            {
                if (mValues!=null && Enumerable.Any(mValues))
                {
                    file.Write(Enumerable.Min(mValues));
                    file.Write(Enumerable.Max(mValues));
                    for (var i = 0; i < count; i++)
                        file.Write(mValues[i]);
                }
                else
                {
                    for (var i = 0; i < count + 2; i++)
                        file.Write(NoDataBorderValue-1);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected int GetBoundingBoxLength()
        {
            boundingBoxIndex = 0;
            int bblength = 4;
            if (HasZValue())
                bblength += 2;
            if (HasMValue() || HasZValue()) // If we have Z we also have to have M per shapefile spec. M can contain null values though.
                bblength += 2;
            return bblength;
        }

        public GeometryInstantiationErrorHandlingOption GeometryInstantiationErrorHandling { get; set; }

        public virtual IEnumerable<MBRInfo> ReadMBRs(BigEndianBinaryReader reader)
        {
            return new ShapeMBREnumerator(reader);
        }
    }
#if !NET35
        internal static class Enumerable
        {
        internal static bool Any(IEnumerable<double> self)
        {
            foreach(var d in self)
                return true;
            return false;
        }
        internal static double Min(IEnumerable<double> self)
        {
            var res = Double.MaxValue;
            foreach(var d in self)
                res = d < res ? d : res;
            
            return (res == double.MaxValue) ? 0d : res;
        }
        internal static double Max(IEnumerable<double> self)
        {
            var res = double.MinValue;
            foreach(var d in self)
                res = d > res ? d : res;

            return (res == double.MinValue) ? 0d : res;
        }
        }
#endif
}