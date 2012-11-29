using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        protected const double NoDataValue = NoDataBorderValue - 1;

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
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public abstract IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory);

        /// <summary>
        /// Writes to the given stream the equilivent shape file record given a Geometry object.
        /// </summary>
        /// <param name="geometry">The geometry object to write.</param>
        /// <param name="file">The stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public abstract void Write(IGeometry geometry, BinaryWriter file, IGeometryFactory geometryFactory);

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

        protected static void WriteCoords(ICoordinateSequence points, BinaryWriter file, List<Double> zList, List<Double> mList)
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
        ///
        /// </summary>
        /// <returns></returns>
        protected bool HasZValue()
        {
            return HasZValue(_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        private static bool HasZValue(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.PointZ ||
                    shapeType == ShapeGeometryType.PointZM ||
                    shapeType == ShapeGeometryType.LineStringZ ||
                    shapeType == ShapeGeometryType.LineStringZM ||
                    shapeType == ShapeGeometryType.PolygonZ ||
                    shapeType == ShapeGeometryType.PolygonZM;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected bool HasMValue()
        {
            return HasMValue(_type);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool HasMValue(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.PointM ||
                    shapeType == ShapeGeometryType.PointZM ||
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
                   shapeType == ShapeGeometryType.PointZ ||
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
                   shapeType == ShapeGeometryType.MultiPointZ ||
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
                   shapeType == ShapeGeometryType.LineStringZ ||
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
                   shapeType == ShapeGeometryType.PolygonZ ||
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
        /// <param name="buffer">The coordinate buffer</param>
        /// <param name="skippedList">A list of indices which have not been added to the buffer</param>
        protected void GetZMValues(BigEndianBinaryReader file, CoordinateBuffer buffer, HashSet<int> skippedList = null)
        {
            if (skippedList == null)
                skippedList = new HashSet<int>();

            var numPoints = buffer.Capacity;
            var numSkipped = 0;

            if (HasZValue())
            {
                boundingBox[boundingBoxIndex++] = file.ReadDouble();
                boundingBox[boundingBoxIndex++] = file.ReadDouble();

                for (var i = 0; i < numPoints; i++)
                {
                    var z = ReadDouble(file);
                    if (!skippedList.Contains(i))
                        buffer.SetZ(i-numSkipped, z);
                    else
                        numSkipped++;
                }
            }

            if (HasMValue())
            {
                boundingBox[boundingBoxIndex++] = file.ReadDouble();
                boundingBox[boundingBoxIndex++] = file.ReadDouble();

                for (var i = 0; i < numPoints; i++)
                {
                    var m = ReadDouble(file);
                    if (!skippedList.Contains(i))
                        buffer.SetM(i - numSkipped, m);
                    else
                        numSkipped++;
                }
            }
        }

        protected void WriteZM(BinaryWriter file, int count, List<double> zValues, List<double> mValues)
        {
            // If we have M we also have to have Z - this is the shapefile defn
            if ((HasZValue() || HasMValue()))
            {
                if (zValues.Any())
                {
                    file.Write(zValues.Min());
                    file.Write(zValues.Max());
                    foreach (var z in zValues)
                    {
                        file.Write(z);
                    }
                }
                else
                    for (var i = 0; i < count + 2; i++)
                        file.Write(0d);
            }

            if (HasMValue() && mValues.Any())
            {
                if (mValues.Any())
                {
                    file.Write(mValues.Min());
                    file.Write(mValues.Max());
                    foreach (var m in mValues)
                    {
                        file.Write(m);
                    }
                }
                else
                    for (var i = 0; i < count + 2; i++)
                        file.Write(NoDataBorderValue-1);
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
            if (HasMValue())
                bblength += 2;
            return bblength;
        }

        public GeometryInstantiationErrorHandlingOption GeometryInstantiationErrorHandling { get; set; }
    }
}