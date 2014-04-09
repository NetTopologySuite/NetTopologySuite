using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    public class Transformer
    {
        private static readonly IGeometryFactory DefaultFactory = GeometryFactory.Default;

        private readonly IGeometryFactory _factory;
        private readonly Transform _transform;
        private readonly Coordinate[][] _arcs;        

        public Transformer(int[][][] arcs) :
            this(arcs, DefaultFactory) { }

        public Transformer(int[][][] arcs, IGeometryFactory factory) :
            this(new Transform(), arcs, factory) { }

        public Transformer(Transform transform, int[][][] arcs) :
            this(transform, arcs, DefaultFactory) { }

        public Transformer(Transform transform, int[][][] arcs, IGeometryFactory factory)
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

        public IGeometry Create(string type, object data)
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

                case "LineString":
                    data = FixedData<int>(data);
                    return CreateLineString((int[])data);

                case "Polygon":
                    data = FixedData<int[]>(data);
                    return CreatePolygon((int[][])data);

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
                    // NOTE: nested arrays are in fact always integers!
                    arr[i++] = (T)FixedData<int>(jitem);
                    continue;
                }

                T value = jitem.Value<T>();
                arr[i++] = value;
            }
            return arr;
        }

        private IGeometry CreatePoint(double[] pt)
        {
            Coordinate coordinate = ConvertPoint(pt);
            return _factory.CreatePoint(coordinate);
        }

        private Coordinate[] InternalCreateLineString(int[] arcs)
        {
            List<Coordinate> list = new List<Coordinate>();
            for (int i = 0; i < arcs.Length; i++)
            {
                int el = arcs[i];
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

        private IGeometry CreateLineString(int[] arcs)
        {
            Coordinate[] coords = InternalCreateLineString(arcs);
            return _factory.CreateLineString(coords);
        }

        private IGeometry CreatePolygon(int[][] arcs)
        {
            Coordinate[] cshell = InternalCreateLineString(arcs[0]);
            ILinearRing shell = _factory.CreateLinearRing(cshell);
            ILinearRing[] holes = new ILinearRing[arcs.Length - 1];
            for (int i = 1; i < arcs.Length; i++)
            {
                Coordinate[] chole = InternalCreateLineString(arcs[i]);
                holes[i - 1] = _factory.CreateLinearRing(chole);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private IGeometry CreateGeometryCollection(object[] data)
        {
            IGeometry[] geometries = new IGeometry[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                dynamic component = data[i];
                string componentType = component.type;
                bool isPoint = String.Equals(componentType, "Point");
                dynamic val = isPoint ? component.coordinates : component.arcs;
                geometries[i] = Create(componentType, val);
            }
            return _factory.CreateGeometryCollection(geometries);
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