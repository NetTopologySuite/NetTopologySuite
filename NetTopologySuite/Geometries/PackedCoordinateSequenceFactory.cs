using System;
using GeoAPI.Geometries;
#region geoapi vs nts
#if useFullGeoAPI
using ICoordinate = GeoAPI.Geometries.ICoordinate;
using IGeometry = GeoAPI.Geometries.IGeometry;
using IPoint = GeoAPI.Geometries.IPoint;
using ILineString = GeoAPI.Geometries.ILineString;
using ILinearRing = GeoAPI.Geometries.ILinearRing;
using IPolygon = GeoAPI.Geometries.IPolygon;
using IGeometryCollection = GeoAPI.Geometries.IGeometryCollection;
using IMultiPoint = GeoAPI.Geometries.IMultiPoint;
using IMultiLineString = GeoAPI.Geometries.IMultiLineString;
using IMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
#endif
#endregion

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Builds packed array coordinate sequences. The array data type can be either
    /// double or float, and defaults to double.
    /// </summary>
    public class PackedCoordinateSequenceFactory : ICoordinateSequenceFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public enum PackedType
        {
            /// <summary>
            /// 
            /// </summary>
            Double = 0,

            /// <summary>
            /// 
            /// </summary>
            Float = 1,
        }

        public static readonly PackedCoordinateSequenceFactory DoubleFactory =
            new PackedCoordinateSequenceFactory(PackedType.Double);

        public static readonly PackedCoordinateSequenceFactory FloatFactory =
            new PackedCoordinateSequenceFactory(PackedType.Float);

        private PackedType type = PackedType.Double;
        private int dimension = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class, 
        /// using double values.
        /// </summary>
        public PackedCoordinateSequenceFactory() : this(PackedType.Double) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public PackedCoordinateSequenceFactory(PackedType type) : this(type, 3) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedCoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dimension"></param>
        public PackedCoordinateSequenceFactory(PackedType type, int dimension)
        {
            Type = type;
            Dimension = dimension;
        }

        /// <summary>
        /// 
        /// </summary>
        public PackedType Type
        {
            get { return type; }
            set
            {
                if (value != PackedType.Double && value != PackedType.Float)
                    throw new ArgumentException("Unknown type " + value);
                this.type = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Dimension
        {
            get { return dimension; }
            set { this.dimension = value; }
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given array; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">Coordinates array, which may not be null nor contain null elements</param>
        /// <returns></returns>
        public ICoordinateSequence Create(ICoordinate[] coordinates)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(coordinates, dimension);
            else return new PackedFloatCoordinateSequence(coordinates, dimension);
        }

        /// <summary>
        /// Returns a CoordinateSequence based on the given coordinate sequence; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(coordSeq.ToCoordinateArray(), dimension);
            else return new PackedFloatCoordinateSequence(coordSeq.ToCoordinateArray(), dimension);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packedCoordinates"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(double[] packedCoordinates, int dimension)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(packedCoordinates, dimension);
            else return new PackedFloatCoordinateSequence(packedCoordinates, dimension);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packedCoordinates"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(float[] packedCoordinates, int dimension)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(packedCoordinates, dimension);
            else return new PackedFloatCoordinateSequence(packedCoordinates, dimension);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(int size, int dimension)
        {
            if (type == PackedType.Double)
                 return new PackedDoubleCoordinateSequence(size, dimension);
            else return new PackedFloatCoordinateSequence(size, dimension);
        }
    }
}
