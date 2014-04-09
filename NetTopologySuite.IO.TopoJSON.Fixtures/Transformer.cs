using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    public interface ITransformer
    {
        IGeometry Create(dynamic data);
    }

    public class Transformer : ITransformer
    {
        private static readonly IGeometryFactory DefaultFactory = GeometryFactory.Default;

        private readonly IGeometryFactory _factory;
        private readonly ITransform _transform;
        private readonly Coordinate[][] _arcs;

        public Transformer(int[][][] arcs) :
            this(arcs, DefaultFactory) { }

        public Transformer(int[][][] arcs, IGeometryFactory factory) :
            this(new Transform(), arcs, factory) { }

        public Transformer(ITransform transform, int[][][] arcs) :
            this(transform, arcs, DefaultFactory) { }

        public Transformer(ITransform transform, int[][][] arcs, IGeometryFactory factory)
        {
            if (transform == null)
                throw new ArgumentNullException("transform");

            _transform = transform;
            _arcs = BuildArcs(arcs);
            _factory = factory;
        }

        private Coordinate[][] BuildArcs(int[][][] arcs)
        {
            Coordinate[][] list = new Coordinate[arcs.Length][];
            for (int i = 0; i < arcs.Length; i++)
            {
                /*
                 * https://github.com/topojson/topojson-specification/blob/master/README.md#213-arcs
                 * If a topology is quantized, the positions of each arc 
                 * in the topology which are quantized must be delta-encoded. 
                 */
                int[] prev = { 0, 0 };
                int[][] arc = arcs[i];
                Coordinate[] conv = new Coordinate[arc.Length];
                for (int j = 0; j < arc.Length; j++)
                {
                    int[] pt = arc[j];
                    bool quantized = _transform.Quantized;
                    if (quantized)
                    {
                        prev[0] += pt[0];
                        prev[1] += pt[1];
                        conv[j] = ConvertPoint(prev);
                    }
                    else conv[j] = ConvertPoint(pt);
                }
                list[i] = conv;
            }
            return list;
        }

        private Coordinate ConvertPoint(double[] pt)
        {
            double[] scale = _transform.Scale;
            double[] translate = _transform.Translate;
            double x = pt[0] * scale[0] + translate[0];
            double y = pt[1] * scale[1] + translate[1];
            Coordinate c = new Coordinate(x, y);
            return c;
        }

        private Coordinate ConvertPoint(int[] pt)
        {
            int len = pt.Length;
            double[] conv = new double[len];
            for (int i = 0; i < len; i++)
                conv[i] = pt[i];
            Coordinate c = ConvertPoint(conv);
            return c;
        }

        public IGeometry Create(dynamic data)
        {            
            if (data == null)
                throw new ArgumentNullException("data");
            string type = data.type;
            if (String.IsNullOrEmpty(type))
                throw new ArgumentException("type undefined", "data");

            switch (type)
            {
                case "Point":
                case "MultiPoint":
                    return Create(type, data.coordinates);

                case "LineString":                    
                case "MultiLineString":                    
                case "Polygon":                    
                case "MultiPolygon":
                    return Create(type, data.arcs);

                case "GeometryCollection":
                    return Create(type, data.geometries);

                default:
                    string s = string.Format("type unsupported: {0}", type);
                    throw new NotSupportedException(s);
            }
        }

        private IGeometry Create(string type, object data)
        {
            if (String.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (data == null)
                throw new ArgumentNullException("data");

            switch (type)
            {
                case "Point":
                    data = FixedData<double>(data);
                    return CreatePoint((double[])data);

                case "MultiPoint":
                    if (_transform.Quantized)
                    {
                        data = FixedData<int[]>(data);
                        return CreateMultiPoint((int[][])data);
                    }
                    data = FixedData<double[]>(data);
                    return CreateMultiPoint((double[][])data);

                case "LineString":
                    data = FixedData<int>(data);
                    return CreateLineString((int[])data);

                case "MultiLineString":
                    data = FixedData<int[]>(data);
                    return CreateMultiLineString((int[][])data);

                case "Polygon":
                    data = FixedData<int[]>(data);
                    return CreatePolygon((int[][])data);

                case "MultiPolygon":
                    data = FixedData<int[][]>(data);
                    return CreateMultiPolygon((int[][][])data);

                case "GeometryCollection":
                    data = FixedData<object>(data);
                    return CreateGeometryCollection((object[])data);

                default:
                    string s = string.Format("type unsupported: {0}", type);
                    throw new NotSupportedException(s);
            }
        }

        private static object FixedData<T>(object data)
        {
            if (!(data is JArray))
                return data;

            JArray jarr = (JArray)data;
            T[] arr = new T[jarr.Count];
            int i = 0;
            foreach (JToken jitem in jarr)
            {
                if (jitem is JArray)
                {
                    // WARNING: shitty code here!
                    Type type = typeof(T).GetElementType();
                    if (type == typeof (int))
                    {
                        // default arcs
                        arr[i++] = (T) FixedData<int>(jitem);
                        continue;
                    }
                    if (type == typeof (int[]))
                    {
                        // multipolygon inner arcs
                        arr[i++] = (T) FixedData<int[]>(jitem);
                        continue;
                    }
                    if (type == typeof (double))
                    {

                        // points/multipoints without translate
                        arr[i++] = (T) FixedData<double>(jitem);
                        continue;
                    }
                    throw new ArgumentOutOfRangeException("unhandled type: " + type);                    
                }

                T value = jitem.Value<T>();
                arr[i++] = value;
            }
            return arr;
        }

        // points as integers WITH transform
        private IGeometry CreatePoint(int[] data)
        {
            Coordinate coordinate = ConvertPoint(data);
            return _factory.CreatePoint(coordinate);
        }

        // points as doubles WITHOUT transform
        private IGeometry CreatePoint(double[] data)
        {
            Coordinate coordinate = ConvertPoint(data);
            return _factory.CreatePoint(coordinate);
        }

        // multipoints as integers WITH transform
        private IGeometry CreateMultiPoint(int[][] data)
        {
            IPoint[] list = new IPoint[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = (IPoint)CreatePoint(data[i]);
            return _factory.CreateMultiPoint(list);
        }

        // multipoints as doubles WITHOUT transform
        private IGeometry CreateMultiPoint(double[][] data)
        {
            IPoint[] list = new IPoint[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = (IPoint)CreatePoint(data[i]);
            return _factory.CreateMultiPoint(list);
        }

        private IGeometry CreateLineString(int[] data)
        {
            Coordinate[] coords = InternalCreateLineString(data);
            return _factory.CreateLineString(coords);
        }

        private IGeometry CreateMultiLineString(int[][] data)
        {
            ILineString[] list = new ILineString[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = (ILineString)CreateLineString(data[i]);
            return _factory.CreateMultiLineString(list);
        }

        private IGeometry CreatePolygon(int[][] data)
        {
            Coordinate[] cshell = InternalCreateLineString(data[0]);
            ILinearRing shell = _factory.CreateLinearRing(cshell);
            ILinearRing[] holes = new ILinearRing[data.Length - 1];
            for (int i = 1; i < data.Length; i++)
            {
                Coordinate[] chole = InternalCreateLineString(data[i]);
                holes[i - 1] = _factory.CreateLinearRing(chole);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private IGeometry CreateMultiPolygon(int[][][] data)
        {
            IPolygon[] list = new IPolygon[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = (IPolygon)CreatePolygon(data[i]);
            return _factory.CreateMultiPolygon(list);
        }

        private IGeometry CreateGeometryCollection(object[] data)
        {
            IGeometry[] geometries = new IGeometry[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                dynamic component = data[i];
                string componentType = component.type;
                bool asPoint = String.Equals(componentType, "Point") || String.Equals(componentType, "MultiPoint");
                dynamic val = asPoint ? component.coordinates : component.arcs;
                geometries[i] = Create(componentType, val);
            }
            return _factory.CreateGeometryCollection(geometries);
        }

        private Coordinate[] InternalCreateLineString(int[] data)
        {
            List<Coordinate> list = new List<Coordinate>();
            for (int i = 0; i < data.Length; i++)
            {
                int el = data[i];
                Coordinate[] coords;
                if (el < 0)
                {
                    /*
                     * https://github.com/topojson/topojson-specification#214-arc-indexes
                     * A negative arc index indicates that the arc 
                     * at the ones’ complement of the index must be reversed 
                     * to reconstruct the geometry: 
                     *   -1 refers to the reversed first arc, 
                     *   -2 refers to the reversed second arc, and so on.
                     */
                    int idx = Math.Abs(el) - 1;
                    Coordinate[] temp = _arcs[idx];
                    coords = MakeCopyReversed(temp);
                }
                else coords = _arcs[el];
                list.AddRange(coords);
            }
            return list.ToArray();
        }

        private static Coordinate[] MakeCopyReversed(Coordinate[] arr)
        {
            int len = arr.Length;
            Coordinate[] clone = new Coordinate[len];
            for (int j = 0; j < len; j++)
                clone[len - j - 1] = arr[j];
            return clone;
        }
    }
}